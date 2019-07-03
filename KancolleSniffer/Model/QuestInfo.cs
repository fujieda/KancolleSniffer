// Copyright (C) 2013, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using KancolleSniffer.Util;

namespace KancolleSniffer.Model
{
    public class QuestStatus
    {
        public int Id { get; set; }
        public int Category { get; set; }
        public QuestInterval Interval { get; set; }
        public string Name { get; set; }
        public string Detail { get; set; }
        public int[] Material { get; set; }
        public int Progress { get; set; }

        [XmlIgnore]
        public QuestCount Count { get; set; }

        [XmlIgnore]
        public Color Color { get; set; }

        public string ToToolTip() =>
            Detail +
            (Material == null || Material.All(x => x == 0)
                ? ""
                : "\r\n" + string.Join(" ",
                      new[] {"燃", "弾", "鋼", "ボ", "建造", "修復", "開発", "改修"}
                          .Zip(Material, (m, num) => num == 0 ? "" : m + num)
                          .Where(s => !string.IsNullOrEmpty(s))));

        public QuestStatus Clone()
        {
            var clone = (QuestStatus)MemberwiseClone();
            clone.Count = Count.Clone();
            return clone;
        }
    }

    public enum QuestInterval
    {
        // ReSharper disable once UnusedMember.Global
        Other,
        Daily,
        Weekly,
        Monthly,
        Quarterly
    }

    public class QuestSpec
    {
        public QuestInterval Interval { get; set; }
        public int Max { get; set; }
        public int[] MaxArray { get; set; }
        public bool AdjustCount { get; set; } = true;
        public int Shift { get; set; }
        public int[] Material { get; set; }
    }

    public class QuestSortie : QuestSpec
    {
        public string Rank { get; set; }
        public int[] Maps { get; set; }

        public static int CompareRank(string a, string b)
        {
            const string ranks = "SABCDE";
            return ranks.IndexOf(a, StringComparison.Ordinal) -
                   ranks.IndexOf(b, StringComparison.Ordinal);
        }

        public bool Check(string rank, int map, bool boss)
        {
            return (Rank == null || CompareRank(rank, Rank) <= 0) &&
                   (Maps == null || Maps.Contains(map) && boss);
        }
    }

    public class QuestEnemyType : QuestSpec
    {
        public int[] EnemyType { get; set; } = new int[0];

        public int CountResult(IEnumerable<ShipStatus> enemyResult) =>
            enemyResult.Count(ship => ship.NowHp == 0 && EnemyType.Contains(ship.Spec.ShipType));
    }

    public class QuestPractice : QuestSpec
    {
        public bool Win { get; set; }
        public bool Check(string rank) => !Win || QuestSortie.CompareRank(rank, "B") <= 0;
    }

    public class QuestMission : QuestSpec
    {
        public int[] Ids { get; set; }
        public bool Check(int id) => Ids == null || Ids.Contains(id);
    }

    public class QuestDestroyItem : QuestSpec
    {
        public int[] Types { get; set; }
        public int[] Ids { get; set; }

        public bool Count(QuestCount count, ItemSpec[] specs)
        {
            if (count.NowArray == null)
            {
                var num = specs.Count(spec => Types?.Contains(spec.Type) ?? (Ids?.Contains(spec.Id) ?? true));
                count.Now += num;
                return num > 0;
            }
            if (Types == null && Ids == null)
                return false;
            var result = false;
            for (var i = 0; i < count.NowArray.Length; i++)
            {
                var num = specs.Count(spec => Types != null ? Types[i] == spec.Type : Ids[i] == spec.Id);
                count.NowArray[i] += num;
                if (num > 0)
                    result = true;
            }
            return result;
        }
    }

    public class QuestPowerUp : QuestSpec
    {
    }

    public class QuestInfo : IHaveState
    {
        private readonly SortedDictionary<int, QuestStatus> _quests = new SortedDictionary<int, QuestStatus>();
        private readonly QuestCountList _countList = new QuestCountList();
        private readonly Func<DateTime> _nowFunc = () => DateTime.Now;
        private DateTime _now;
        private DateTime _lastReset;
        private IEnumerable<QuestStatus> _clearedQuest = new List<QuestStatus>();

        private readonly Color[] _color =
        {
            Color.FromArgb(60, 141, 76), Color.FromArgb(232, 57, 41), Color.FromArgb(136, 204, 120),
            Color.FromArgb(52, 147, 185), Color.FromArgb(220, 198, 126), Color.FromArgb(168, 111, 76),
            Color.FromArgb(200, 148, 231), Color.FromArgb(232, 57, 41)
        };

        public int AcceptMax { get; set; } = 5;

        public SortedDictionary<int, QuestStatus> QuestDictionary => _quests;

        public QuestStatus[] Quests => _quests.Values.ToArray();

        public QuestInfo(Func<DateTime> nowFunc = null)
        {
            if (nowFunc != null)
                _nowFunc = nowFunc;
        }

        public void GetNotifications(out string[] notify, out string[] stop)
        {
            var cleared = _quests.Values.Where(q => q.Count.Cleared).ToArray();
            notify = cleared.Except(_clearedQuest, new QuestComparer()).Select(q => q.Name).ToArray();
            stop = _clearedQuest.Except(cleared, new QuestComparer()).Select(q => q.Name).ToArray();
            _clearedQuest = cleared;
        }

        private class QuestComparer : IEqualityComparer<QuestStatus>
        {
            public bool Equals(QuestStatus x, QuestStatus y)
            {
                return x?.Id == y?.Id;
            }

            public int GetHashCode(QuestStatus obj)
            {
                return obj.Id;
            }
        }

        private readonly QuestInterval[] _intervals =
        {
            QuestInterval.Daily, QuestInterval.Weekly, QuestInterval.Monthly,
            QuestInterval.Other, QuestInterval.Quarterly
        };

        private readonly int[] _progress = {0, 50, 80};

        public void InspectQuestList(dynamic json)
        {
            ResetQuests();
            if (json.api_list == null)
                return;
            for (var i = 0; i < 2; i++)
            {
                foreach (var entry in json.api_list)
                {
                    if (entry is double) // -1の場合がある。
                        continue;
                    var quest = new QuestStatus
                    {
                        Id = (int)entry.api_no,
                        Category = (int)entry.api_category,
                        Progress = _progress[(int)entry.api_progress_flag],
                        Interval = _intervals[(int)entry.api_type - 1],
                        Name = (string)entry.api_title,
                        Detail = ((string)entry.api_detail).Replace("<br>", "\r\n"),
                        Material = (int[])entry.api_get_material
                    };
                    var state = (int)entry.api_state;
                    switch (state)
                    {
                        case 1:
                            if (_quests.Remove(quest.Id))
                                NeedSave = true;
                            break;
                        case 3:
                            quest.Progress = 100;
                            goto case 2;
                        case 2:
                            SetProcessedQuest(quest);
                            break;
                    }
                }
                if (_quests.Count <= AcceptMax)
                    break;
                /*
                 * ほかのPCで任務を達成した場合、任務が消えずに受領した任務の数がAcceptMaxを超えることがある。
                 * その場合はいったん任務をクリアして、現在のページの任務だけを登録し直す。
                 */
                _quests.Clear();
            }
        }

        private void SetProcessedQuest(QuestStatus quest)
        {
            var count = _countList.GetCount(quest.Id);
            if (count.AdjustCount(quest.Progress))
                NeedSave = true;
            quest.Material = quest.Material.Concat(count.Spec.Material).ToArray();
            if (!_quests.ContainsKey(quest.Id))
                NeedSave = true;
            SetQuest(quest);
        }

        private void SetQuest(QuestStatus quest)
        {
            quest.Count = _countList.GetCount(quest.Id);
            quest.Color = quest.Category <= _color.Length ? _color[quest.Category - 1] : Control.DefaultBackColor;
            _quests[quest.Id] = quest;
        }

        public void ClearQuests()
        {
            _quests.Clear();
        }

        private void ResetQuests()
        {
            _now = _nowFunc();
            if (!CrossBoundary(LastMorning))
                return;
            RemoveQuest(QuestInterval.Daily);
            _countList.Remove(QuestInterval.Daily);
            ResetWeekly();
            ResetMonthly();
            ResetQuarterly();
            _lastReset = _now;
            NeedSave = true;
        }

        private DateTime LastMorning => _now.Date.AddDays(_now.Hour < 5 ? -1 : 0).AddHours(5);

        private void ResetWeekly()
        {
            if (!CrossBoundary(LastMonday.AddHours(5)))
                return;
            RemoveQuest(QuestInterval.Weekly);
            _countList.Remove(QuestInterval.Weekly);
        }

        private DateTime LastMonday => _now.Date.AddDays(-((6 + (int)_now.DayOfWeek) % 7));

        private void ResetMonthly()
        {
            if (!CrossBoundary(new DateTime(_now.Year, _now.Month, 1, 5, 0, 0)))
                return;
            RemoveQuest(QuestInterval.Monthly);
            _countList.Remove(QuestInterval.Monthly);
        }

        private void ResetQuarterly()
        {
            if (!CrossBoundary(QuarterlyBoundary.AddHours(5)))
                return;
            RemoveQuest(QuestInterval.Quarterly);
            _countList.Remove(QuestInterval.Quarterly);
        }

        private DateTime QuarterlyBoundary =>
            _now.Month / 3 == 0
                ? new DateTime(_now.Year - 1, 12, 1)
                : new DateTime(_now.Year, _now.Month / 3 * 3, 1);

        private bool CrossBoundary(DateTime boundary)
        {
            return _lastReset < boundary && boundary <= _now;
        }

        private void RemoveQuest(QuestInterval interval)
        {
            foreach (var id in
                (from kv in _quests
                    where kv.Value.Count.Spec.Interval == interval || // 輸送5と空母3はカウンタを見ないとデイリーにならない
                          kv.Value.Interval == interval
                    select kv.Key).ToArray())
                _quests.Remove(id);
        }

        public void InspectStop(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            _quests.Remove(int.Parse(values["api_quest_id"]));
            NeedSave = true;
        }

        public void InspectClearItemGet(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var id = int.Parse(values["api_quest_id"]);
            _countList.Remove(id);
            _quests.Remove(id);
            NeedSave = true;
        }

        public bool NeedSave { get; set; }

        public void SaveState(Status status)
        {
            NeedSave = false;
            status.QuestLastReset = _lastReset;
            if (_quests != null)
                status.QuestList = _quests.Values.ToArray();
            if (_countList != null)
                status.QuestCountList = _countList.NonZeroCountList.ToArray();
        }

        public void LoadState(Status status)
        {
            _lastReset = status.QuestLastReset;
            if (status.QuestCountList != null)
                _countList.SetCountList(status.QuestCountList);
            if (status.QuestList != null)
            {
                _quests.Clear();
                foreach (var quest in status.QuestList)
                    SetQuest(quest);
            }
        }
    }

}