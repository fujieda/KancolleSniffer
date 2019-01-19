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
using static System.Math;

namespace KancolleSniffer.Model
{
    public class QuestStatus
    {
        public int Id { get; set; }
        public int Category { get; set; }
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

    public class QuestCount
    {
        public int Id { get; set; }
        public int Now { get; set; }
        public int[] NowArray { get; set; }

        [XmlIgnore]
        public QuestSpec Spec { get; set; }

        public bool AdjustCount(int progress)
        {
            if (!Spec.AdjustCount)
                return false;
            if (NowArray != null)
            {
                if (progress != 100)
                    return false;
                NowArray = NowArray.Zip(Spec.MaxArray, Max).ToArray();
                return true;
            }
            var next = 0;
            switch (progress)
            {
                case 0:
                    next = 50;
                    break;
                case 50:
                    next = 80;
                    break;
                case 80:
                    next = 100;
                    break;
                case 100:
                    next = 100000;
                    break;
            }
            var now = Now + Spec.Shift;
            var max = Spec.Max + Spec.Shift;
            var low = (int)Ceiling(max * progress / 100.0);
            if (low >= max && progress != 100)
                low = max - 1;
            var high = (int)Ceiling(max * next / 100.0);
            if (now < low)
            {
                Now = low - Spec.Shift;
                return true;
            }
            if (now >= high)
            {
                Now = high - 1 - Spec.Shift;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            if (Id == 426 || Id == 854 || Id == 873 || Id == 888 || Id == 894)
                return $"{NowArray.Count(n => n >= 1)}/{Spec.MaxArray.Length}";
            return NowArray != null
                ? string.Join(" ", NowArray.Zip(Spec.MaxArray, (n, m) => $"{n}/{m}"))
                : $"{Now}/{Spec.Max}";
        }

        public string ToToolTip()
        {
            switch (Id)
            {
                case 426:
                    return string.Join(" ",
                        new[] {"警備任務", "対潜警戒任務", "海上護衛任務", "強硬偵察任務"}
                            .Zip(NowArray, (mission, n) => n >= 1 ? mission : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
                case 428:
                    return string.Join(" ",
                        new[] {"対潜警戒任務", "海峡警備行動", "長時間対潜警戒"}.Zip(NowArray, (mission, n) => n >= 1 ? mission + n : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
                case 854:
                    return string.Join(" ",
                        new[] {"2-4", "6-1", "6-3", "6-4"}.Zip(NowArray, (map, n) => n >= 1 ? map : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
                case 873:
                    return string.Join(" ",
                        new[] {"3-1", "3-2", "3-3"}.Zip(NowArray, (map, n) => n >= 1 ? map : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
                case 888:
                    return string.Join(" ",
                        new[] {"5-1", "5-3", "5-4"}.Zip(NowArray, (map, n) => n >= 1 ? map : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
                case 688:
                    return string.Join(" ",
                        new[] {"艦戦", "艦爆", "艦攻", "水偵"}.Zip(NowArray, (type, n) => n >= 1 ? type + n : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
                case 893:
                    return string.Join(" ",
                        new[] {"1-5", "7-1", "7-2G", "7-2M"}.Zip(NowArray, (map, n) => n >= 1 ? $"{map}:{n}" : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
                case 894:
                    return string.Join(" ",
                        new[] {"1-3", "1-4", "2-1", "2-2", "2-3"}.Zip(NowArray, (map, n) => n >= 1 ? map : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
            }
            return "";
        }

        public bool Cleared => NowArray?.Zip(Spec.MaxArray, (n, m) => n >= m).All(x => x) ?? Spec.Max != 0 && Now >= Spec.Max;
    }

    public class QuestInfo : IHaveState
    {
        private readonly SortedDictionary<int, QuestStatus> _quests = new SortedDictionary<int, QuestStatus>();
        private readonly QuestCountList _countList = new QuestCountList();
        private readonly ItemInfo _itemInfo;
        private readonly BattleInfo _battleInfo;
        private readonly Func<DateTime> _nowFunc = () => DateTime.Now;
        private DateTime _lastReset;
        private IEnumerable<QuestStatus> _clearedQuest = new List<QuestStatus>();

        private readonly Color[] _color =
        {
            Color.FromArgb(60, 141, 76), Color.FromArgb(232, 57, 41), Color.FromArgb(136, 204, 120),
            Color.FromArgb(52, 147, 185), Color.FromArgb(220, 198, 126), Color.FromArgb(168, 111, 76),
            Color.FromArgb(200, 148, 231), Color.FromArgb(232, 57, 41)
        };

        public int AcceptMax { get; set; } = 5;

        public QuestStatus[] Quests => _quests.Values.ToArray();

        public QuestInfo(ItemInfo itemInfo, BattleInfo battleInfo, Func<DateTime> nowFunc = null)
        {
            _itemInfo = itemInfo;
            _battleInfo = battleInfo;
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

                    var id = (int)entry.api_no;
                    var state = (int)entry.api_state;
                    var progress = (int)entry.api_progress_flag;
                    var cat = (int)entry.api_category;
                    var name = (string)entry.api_title;
                    var detail = ((string)entry.api_detail).Replace("<br>", "\r\n");
                    var material = (int[])entry.api_get_material;

                    switch (progress)
                    {
                        case 0:
                            break;
                        case 1:
                            progress = 50;
                            break;
                        case 2:
                            progress = 80;
                            break;
                    }
                    switch (state)
                    {
                        case 1:
                            if (_quests.Remove(id))
                                NeedSave = true;
                            break;
                        case 3:
                            progress = 100;
                            goto case 2;
                        case 2:
                            AddQuest(id, cat, name, detail, material, progress, true);
                            break;
                    }
                }
                if (_quests.Count <= AcceptMax)
                    break;
                /*
                 * ほかのPCで任務を達成した場合、任務が消えずに受領した任務の数が_questCountを超えることがある。
                 * その場合はいったん任務をクリアして、現在のページの任務だけを登録し直す。
                 */
                _quests.Clear();
            }
        }

        private void AddQuest(int id, int category, string name, string detail, int[] material, int progress,
            bool adjustCount)
        {
            var count = _countList.GetCount(id);
            if (adjustCount)
            {
                if (count.AdjustCount(progress))
                    NeedSave = true;
            }
            _quests[id] = new QuestStatus
            {
                Id = id,
                Category = category,
                Name = name,
                Detail = detail,
                Material = adjustCount ? material?.Concat(count.Spec.Material).ToArray() : material,
                Count = count,
                Progress = progress,
                Color = category <= _color.Length ? _color[category - 1] : Control.DefaultBackColor
            };
        }

        public void ClearQuests()
        {
            _quests.Clear();
        }

        private void ResetQuests()
        {
            var now = _nowFunc();
            var daily = now.Date.AddHours(5);
            if (!(_lastReset < daily && daily <= now))
                return;
            RemoveQuest(QuestInterval.Daily);
            _countList.Remove(QuestInterval.Daily);
            var weekly = now.Date.AddDays(-((6 + (int)now.DayOfWeek) % 7)).AddHours(5);
            if (_lastReset < weekly && weekly <= now)
            {
                RemoveQuest(QuestInterval.Weekly);
                _countList.Remove(QuestInterval.Weekly);
            }
            var monthly = new DateTime(now.Year, now.Month, 1, 5, 0, 0);
            if (_lastReset < monthly && monthly <= now)
            {
                RemoveQuest(QuestInterval.Monthly);
                _countList.Remove(QuestInterval.Monthly);
            }
            var season = now.Month / 3;
            var quarterly = new DateTime(now.Year - (season == 0 ? 1 : 0), (season == 0 ? 12 : season * 3), 1, 5, 0, 0);
            if (_lastReset < quarterly && quarterly <= now)
            {
                RemoveQuest(QuestInterval.Quarterly);
                _countList.Remove(QuestInterval.Quarterly);
            }
            _lastReset = now;
            NeedSave = true;
        }

        private void RemoveQuest(QuestInterval interval)
        {
            foreach (var id in
                (from kv in _quests where kv.Value.Count.Spec.Interval == interval select kv.Key).ToArray())
                _quests.Remove(id);
        }

        private int _map;
        private int _cell;
        private bool _boss;

        public void InspectMapStart(dynamic json)
        {
            if (_quests.TryGetValue(214, out var ago)) // あ号
                ago.Count.NowArray[0]++;
            InspectMapNext(json);
        }

        public void InspectMapNext(dynamic json)
        {
            _map = (int)json.api_maparea_id * 10 + (int)json.api_mapinfo_no;
            _cell = json.api_no() ? (int)json.api_no : 0;
            _boss = (int)json.api_event_id == 5;

            if (_quests.TryGetValue(861, out var q861))
            {
                if (_map == 16 && (int)json.api_event_id == 8)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.ShipType)
                        .ToArray();
                    if (fleet.Count(s => s == 10 || s == 22) == 2)
                        IncrementCount(q861.Count);
                }
            }
        }

        public void InspectBattleResult(dynamic json)
        {
            var rank = json.api_win_rank;
            foreach (var quest in _quests.Values)
            {
                var count = quest.Count;
                switch (count.Spec)
                {
                    case QuestSortie sortie:
                        if (count.Id == 216 && !_boss || sortie.Check(rank, _map, _boss))
                            IncrementCount(count);
                        break;
                    case QuestEnemyType enemyType:
                        var num = enemyType.CountResult(
                            _battleInfo.Result.Enemy.Main.Concat(_battleInfo.Result.Enemy.Guard));
                        if (num > 0)
                            AddCount(count, num);
                        break;
                }
            }
            if (_quests.TryGetValue(214, out var ago))
            {
                var count = ago.Count;
                if (_boss)
                {
                    IncrementNowArray(count, 2);
                    if (QuestSortie.CompareRank(rank, "B") <= 0)
                        IncrementNowArray(count, 3);
                }
                if (rank == "S")
                    IncrementNowArray(count, 1);
            }
            if (_quests.TryGetValue(249, out var q249))
            {
                if (_map == 25 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.Id)
                        .ToArray();
                    if (fleet.Intersect(new[] {62, 63, 64, 265, 266, 268, 319, 192, 194}).Count() == 3)
                        IncrementCount(q249.Count);
                }
            }
            if (_quests.TryGetValue(257, out var q257))
            {
                if (_map == 14 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.ShipType)
                        .ToArray();
                    if (fleet[0] == 3 && fleet.Count(s => s == 3) <= 3 && fleet.All(s => s == 2 || s == 3))
                        IncrementCount(q257.Count);
                }
            }
            if (_quests.TryGetValue(259, out var q259))
            {
                if (_map == 51 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec).ToArray();
                    // ReSharper disable once IdentifierTypo
                    var ctype = new[]
                    {
                        2, // 伊勢型
                        19, // 長門型
                        26, // 扶桑型
                        37 // 大和型
                    };
                    if (fleet.Select(s => s.ShipClass).Count(c => ctype.Contains(c)) == 3 &&
                        fleet.Count(s => s.ShipType == 3) > 0)
                    {
                        IncrementCount(q259.Count);
                    }
                }
            }
            if (_quests.TryGetValue(264, out var q264))
            {
                if (_map == 42 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec)
                        .ToArray();
                    if (fleet.Count(spec => spec.ShipType == 2) >= 2 &&
                        fleet.Count(spec => spec.IsAircraftCarrier) >= 2)
                        IncrementCount(q264.Count);
                }
            }
            if (_quests.TryGetValue(266, out var q266))
            {
                if (_map == 25 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.ShipType)
                        .ToArray();
                    if (fleet[0] == 2 && fleet.OrderBy(x => x).SequenceEqual(new[] {2, 2, 2, 2, 3, 5}))
                        IncrementCount(q266.Count);
                }
            }
            if (_quests.TryGetValue(854, out var opz) && _boss)
            {
                var count = opz.Count;
                switch (_map)
                {
                    case 24 when QuestSortie.CompareRank(rank, "A") <= 0:
                        IncrementNowArray(count, 0);
                        break;
                    case 61 when QuestSortie.CompareRank(rank, "A") <= 0:
                        IncrementNowArray(count, 1);
                        break;
                    case 63 when QuestSortie.CompareRank(rank, "A") <= 0:
                        IncrementNowArray(count, 2);
                        NeedSave = true;
                        break;
                    case 64 when QuestSortie.CompareRank(rank, "S") <= 0:
                        IncrementNowArray(count, 3);
                        break;
                }
            }
            if (_quests.TryGetValue(862, out var q862))
            {
                if (_map == 63 && _boss && QuestSortie.CompareRank(rank, "A") <= 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.ShipType)
                        .ToArray();
                    if (fleet.Count(s => s == 3) >= 2 && fleet.Count(s => s == 16) >= 1)
                        IncrementCount(q862.Count);
                }
            }
            if (_quests.TryGetValue(873, out var q873))
            {
                if (_battleInfo.Result.Friend.Main.Count(s => s.NowHp > 0 && s.Spec.ShipType == 3) >= 1 &&
                    _boss && QuestSortie.CompareRank(rank, "A") <= 0)
                {
                    var count = q873.Count;
                    switch (_map)
                    {
                        case 31:
                            IncrementNowArray(count, 0);
                            break;
                        case 32:
                            IncrementNowArray(count, 1);
                            break;
                        case 33:
                            IncrementNowArray(count, 2);
                            break;
                    }
                }
            }
            if (_quests.TryGetValue(875, out var q875))
            {
                if (_map == 54 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.Id).ToArray();
                    if (fleet.Contains(543) && fleet.Intersect(new[] {344, 345, 359}).Any())
                        IncrementCount(q875.Count);
                }
            }
            if (_quests.TryGetValue(888, out var q888))
            {
                if (!_boss || QuestSortie.CompareRank(rank, "S") != 0)
                    return;
                var fleet = from ship in _battleInfo.Result.Friend.Main where ship.NowHp > 0 select ship.Spec.Id;
                var member = new[]
                {
                    69, 272, 427, // 鳥海
                    61, 264, // 青葉
                    123, 295, 142, // 衣笠
                    59, 262, 416, // 古鷹
                    60, 263, 417, // 加古
                    51, 213, 477, // 天龍
                    115, 293 // 夕張
                };
                if (fleet.Intersect(member).Count() < 4)
                    return;
                var count = q888.Count;
                switch (_map)
                {
                    case 51:
                        IncrementNowArray(count, 0);
                        break;
                    case 53:
                        IncrementNowArray(count, 1);
                        break;
                    case 54:
                        IncrementNowArray(count, 2);
                        break;
                }
            }
            if (_quests.TryGetValue(893, out var q893))
            {
                if (QuestSortie.CompareRank(rank, "S") != 0)
                    return;
                var count = q893.Count;
                switch (_map)
                {
                    case 15:
                        IncrementNowArray(count, 0);
                        break;
                    case 71:
                        IncrementNowArray(count, 1);
                        break;
                    case 72:
                        if (_cell == 7)
                        {
                            IncrementNowArray(count, 2);
                            break;
                        }
                        IncrementNowArray(count, 3);
                        break;
                }
            }
            if (_quests.TryGetValue(894, out var q894))
            {
                if (!_boss ||
                    QuestSortie.CompareRank(rank, "S") != 0 ||
                    !_battleInfo.Result.Friend.Main.Any(s => s.Spec.IsAircraftCarrier && s.NowHp > 0))
                    return;
                var count = q894.Count;
                switch (_map)
                {
                    case 13:
                        IncrementNowArray(count, 0);
                        break;
                    case 14:
                        IncrementNowArray(count, 1);
                        break;
                    case 21:
                        IncrementNowArray(count, 2);
                        break;
                    case 22:
                        IncrementNowArray(count, 3);
                        break;
                    case 23:
                        IncrementNowArray(count, 4);
                        break;
                }
            }
        }

        private int _questFleet;

        public void StartPractice(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            _questFleet = int.Parse(values["api_deck_id"]) - 1;
        }

        public void InspectPracticeResult(dynamic json)
        {
            foreach (var quest in _quests.Values)
            {
                var count = quest.Count;
                if (!(count.Spec is QuestPractice practice))
                    continue;
                if (practice.Check(json.api_win_rank))
                    IncrementCount(count);
            }
            if (_quests.TryGetValue(318, out var q318))
            {
                if (_questFleet == 0 && QuestSortie.CompareRank(json.api_win_rank, "B") <= 0 &&
                    _battleInfo.Result.Friend.Main.Count(s => s.Spec.ShipType == 3) >= 2)
                {
                    IncrementCount(q318.Count);
                }
            }
        }

        private readonly int[] _missionId = new int[ShipInfo.FleetCount];

        public void InspectDeck(dynamic json)
        {
            foreach (var entry in json)
                _missionId[(int)entry.api_id - 1] = (int)entry.api_mission[1];
        }

        public void InspectMissionResult(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            var deck = int.Parse(values["api_deck_id"]);
            if ((int)json.api_clear_result == 0)
                return;
            var mid = _missionId[deck - 1];
            foreach (var quest in _quests.Values)
            {
                var count = quest.Count;
                if (!(count.Spec is QuestMission mission))
                    continue;
                if (mission.Check(mid))
                    IncrementCount(count);
            }
            if (_quests.TryGetValue(426, out var q426))
            {
                var count = q426.Count;
                switch (mid)
                {
                    case 3:
                        IncrementNowArray(count, 0);
                        break;
                    case 4:
                        IncrementNowArray(count, 1);
                        break;
                    case 5:
                        IncrementNowArray(count, 2);
                        break;
                    case 10:
                        IncrementNowArray(count, 3);
                        break;
                }
            }
            if (_quests.TryGetValue(428, out var q428))
            {
                var count = q428.Count;
                switch (mid)
                {
                    case 4:
                        IncrementNowArray(count, 0);
                        break;
                    case 101:
                        IncrementNowArray(count, 1);
                        break;
                    case 102:
                        IncrementNowArray(count, 2);
                        break;
                }
            }
        }

        private void IncrementCount(QuestCount count)
        {
            count.Now++;
            NeedSave = true;
        }

        private void AddCount(QuestCount count, int value)
        {
            count.Now += value;
            NeedSave = true;
        }

        private void IncrementCount(int id)
        {
            AddCount(id, 1);
        }

        private void AddCount(int id, int value)
        {
            if (_quests.TryGetValue(id, out var quest))
            {
                quest.Count.Now += value;
                NeedSave = true;
            }
        }

        private void IncrementNowArray(QuestCount count, int n)
        {
            count.NowArray[n]++;
            NeedSave = true;
        }

        public void CountNyukyo() => IncrementCount(503);

        public void CountCharge() => IncrementCount(504);

        public void CountCreateItem()
        {
            IncrementCount(605);
            IncrementCount(607);
        }

        public void CountCreateShip()
        {
            IncrementCount(606);
            IncrementCount(608);
        }

        public void InspectDestroyShip(string request)
        {
            AddCount(609, HttpUtility.ParseQueryString(request)["api_ship_id"].Split(',').Length);
        }

        public void CountRemodelSlot() => IncrementCount(619);

        public void InspectDestroyItem(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            var items = values["api_slotitem_ids"].Split(',')
                .Select(id => _itemInfo.GetStatus(int.Parse(id)).Spec).ToArray();
            IncrementCount(613); // 613: 資源の再利用
            foreach (var quest in _quests.Values)
            {
                var count = quest.Count;
                if (!(count.Spec is QuestDestroyItem destroy))
                    continue;
                if (destroy.Count(count, items))
                    NeedSave = true;
            }
            if (_quests.TryGetValue(680, out var q680))
            {
                q680.Count.NowArray[0] += items.Count(spec => spec.Type == 21);
                q680.Count.NowArray[1] += items.Count(spec => spec.Type == 12 || spec.Type == 13);
                NeedSave = true;
            }
        }

        public void InspectPowerUp(dynamic json)
        {
            if ((int)json.api_powerup_flag == 0)
                return;
            foreach (var quest in _quests.Values)
            {
                var count = quest.Count;
                if (!(count.Spec is QuestPowerUp))
                    continue;
                IncrementCount(count);
            }
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

        public bool NeedSave { get; private set; }

        public void SaveState(Status status)
        {
            NeedSave = false;
            status.QuestLastReset = _lastReset;
            if (_quests != null)
                status.QuestList = _quests.Values.ToArray();
            if (_countList != null)
                status.QuestCountList = _countList.CountList.ToArray();
        }

        public void LoadState(Status status)
        {
            _lastReset = status.QuestLastReset;
            if (status.QuestCountList != null)
                _countList.CountList = status.QuestCountList;
            if (status.QuestList != null)
            {
                _quests.Clear();
                foreach (var q in status.QuestList)
                    AddQuest(q.Id, q.Category, q.Name, q.Detail, q.Material, q.Progress, false);
            }
        }
    }
}