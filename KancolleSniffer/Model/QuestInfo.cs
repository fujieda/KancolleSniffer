﻿// Copyright (C) 2013, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
        Quarterly,
        Yearly1,
        Yearly2,
        Yearly3,
        Yearly5,
        Yearly8,
        Yearly9,
        Yearly10,
        Yearly11
    }

    public class QuestInfo : IHaveState
    {
        private readonly QuestCountList _countList;
        private readonly Func<DateTime> _nowFunc = () => DateTime.Now;
        private DateTime _now;
        private DateTime _lastReset;
        private IEnumerable<QuestStatus> _clearedQuest = new List<QuestStatus>();

        private readonly Color[] _color =
        {
            Color.FromArgb(60, 141, 76), Color.FromArgb(232, 57, 41), Color.FromArgb(136, 204, 120),
            Color.FromArgb(52, 147, 185), Color.FromArgb(220, 198, 126), Color.FromArgb(168, 111, 76),
            Color.FromArgb(200, 148, 231), Color.FromArgb(232, 57, 41), Color.FromArgb(232, 57, 41)
        };

        public SortedDictionary<int, QuestStatus> QuestDictionary { get; } = new SortedDictionary<int, QuestStatus>();

        public QuestStatus[] Quests => QuestDictionary.Values.ToArray();

        public QuestInfo(QuestCountList countList, Func<DateTime> nowFunc = null)
        {
            _countList = countList;
            if (nowFunc != null)
                _nowFunc = nowFunc;
        }

        public void GetNotifications(out string[] notify, out string[] stop)
        {
            var cleared = QuestDictionary.Values.Where(q => q.Count.Cleared).ToArray();
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

        private readonly int[] _progress = {0, 50, 80};

        public void InspectQuestList(string request, dynamic json)
        {
            ResetCounts();
            var values = HttpUtility.ParseQueryString(request);
            if (values["api_tab_id"] == "0")
                QuestDictionary.Clear();
            if (json.api_list == null)
                return;
            foreach (var entry in json.api_list)
            {
                if (entry is double) // -1の場合がある。
                    continue;
                var quest = new QuestStatus
                {
                    Id = (int)entry.api_no,
                    Category = (int)entry.api_category,
                    Progress = _progress[(int)entry.api_progress_flag],
                    Name = (string)entry.api_title,
                    Detail = ((string)entry.api_detail).Replace("<br>", "\r\n"),
                    Material = (int[])entry.api_get_material
                };
                var state = (int)entry.api_state;
                switch (state)
                {
                    case 3:
                        quest.Progress = 100;
                        goto case 2;
                    case 2:
                        AdjustQuest(quest);
                        SetQuest(quest);
                        break;
                }
            }
        }

        private void AdjustQuest(QuestStatus quest)
        {
            quest.Count = _countList.GetCount(quest.Id);
            if (quest.Count.AdjustCount(quest.Progress))
                NeedSave = true;
            quest.Material = quest.Material.Concat(quest.Count.Spec.Material).ToArray();
            if (!QuestDictionary.ContainsKey(quest.Id))
                NeedSave = true;
        }

        private void SetQuest(QuestStatus quest)
        {
            quest.Count = _countList.GetCount(quest.Id);
            quest.Color = quest.Category <= _color.Length ? _color[quest.Category - 1] : Control.DefaultBackColor;
            QuestDictionary[quest.Id] = quest;
        }

        private void ResetCounts()
        {
            _now = _nowFunc();
            if (!CrossBoundary(QuestInterval.Daily))
                return;
            foreach (var interval in (QuestInterval[])typeof(QuestInterval).GetEnumValues())
            {
                if (!CrossBoundary(interval))
                    continue;
                _countList.Remove(interval);
            }
            _lastReset = _now;
            NeedSave = true;
        }

        private DateTime LastMorning => _now.Date.AddDays(_now.Hour < 5 ? -1 : 0).AddHours(5);

        private bool CrossBoundary(QuestInterval interval)
        {
            return interval switch
            {
                QuestInterval.Other => false,
                QuestInterval.Daily => CrossBoundary(LastMorning),
                QuestInterval.Weekly => CrossBoundary(LastMonday.AddHours(5)),
                QuestInterval.Monthly => CrossBoundary(new DateTime(_now.Year, _now.Month, 1, 5, 0, 0)),
                QuestInterval.Quarterly => CrossBoundary(QuarterlyBoundary.AddHours(5)),
                QuestInterval.Yearly1 => CrossBoundary(new DateTime(_now.Year, 1, 1, 5, 0, 0)),
                QuestInterval.Yearly2 => CrossBoundary(new DateTime(_now.Year, 2, 1, 5, 0, 0)),
                QuestInterval.Yearly3 => CrossBoundary(new DateTime(_now.Year, 3, 1, 5, 0, 0)),
                QuestInterval.Yearly5 => CrossBoundary(new DateTime(_now.Year, 5, 1, 5, 0, 0)),
                QuestInterval.Yearly8 => CrossBoundary(new DateTime(_now.Year, 8, 1, 5, 0, 0)),
                QuestInterval.Yearly9 => CrossBoundary(new DateTime(_now.Year, 9, 1, 5, 0, 0)),
                QuestInterval.Yearly10 => CrossBoundary(new DateTime(_now.Year, 10, 1, 5, 0, 0)),
                QuestInterval.Yearly11 => CrossBoundary(new DateTime(_now.Year, 11, 1, 5, 0, 0)),
                _ => false
            };
        }

        private DateTime LastMonday => _now.Date.AddDays(-((6 + (int)_now.DayOfWeek) % 7));

        private DateTime QuarterlyBoundary =>
            _now.Month / 3 == 0
                ? new DateTime(_now.Year - 1, 12, 1)
                : new DateTime(_now.Year, _now.Month / 3 * 3, 1);

        private bool CrossBoundary(DateTime boundary)
        {
            return _lastReset < boundary && boundary <= _now;
        }

        public void InspectStop(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            QuestDictionary.Remove(int.Parse(values["api_quest_id"]));
            NeedSave = true;
        }

        public void InspectClearItemGet(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var id = int.Parse(values["api_quest_id"]);
            _countList.Remove(id);
            QuestDictionary.Remove(id);
            NeedSave = true;
        }

        public bool NeedSave { get; set; }

        public void SaveState(Status status)
        {
            NeedSave = false;
            status.QuestLastReset = _lastReset;
            if (QuestDictionary != null)
                status.QuestList = QuestDictionary.Values.ToArray();
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
                QuestDictionary.Clear();
                foreach (var quest in status.QuestList)
                    SetQuest(quest);
            }
        }
    }
}