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

namespace KancolleSniffer
{
    public struct QuestStatus
    {
        public int Category { get; set; }
        public string Name { get; set; }
        public int Progress { get; set; }
        public Color Color { get; set; }
    }

    public class QuestInfo
    {
        private DateTime _lastCreared;
        private readonly SortedDictionary<int, QuestStatus> _quests = new SortedDictionary<int, QuestStatus>();

        private readonly Color[] _color =
        {
            Color.FromArgb(60, 141, 76), Color.FromArgb(232, 57, 41), Color.FromArgb(136, 204, 120),
            Color.FromArgb(52, 147, 185), Color.FromArgb(220, 198, 126), Color.FromArgb(168, 111, 76),
            Color.FromArgb(200, 148, 231), Color.FromArgb(232, 57, 41)
        };

        public int QuestCount { get; set; }

        public void InspectQuestList(dynamic json)
        {
            var resetTime = DateTime.Today.AddHours(5);
            if (DateTime.Now >= resetTime && _lastCreared < resetTime)
            {
                _quests.Clear(); // 前日に未消化のデイリーを消す。
                _lastCreared = DateTime.Now;
            }
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
                            _quests.Remove(id);
                            break;
                        case 3:
                            progress = 100;
                            goto case 2;
                        case 2:
                            _quests[id] = new QuestStatus
                            {
                                Category = cat,
                                Name = name,
                                Progress = progress,
                                Color = cat <= _color.Length ? _color[cat - 1] : Control.DefaultBackColor
                            };
                            break;
                    }
                }
                if (_quests.Count <= QuestCount)
                    break;
                /*
                 * ほかのPCで任務を達成した場合、任務が消えずに受領した任務の数が_questCountを超えることがある。
                 * その場合はいったん任務をクリアして、現在のページの任務だけを登録し直す。
                 */
                _quests.Clear();
            }
        }

        public void InspectClearItemGet(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            _quests.Remove(int.Parse(values["api_quest_id"]));
        }

        public QuestStatus[] Quests => _quests.Values.ToArray();
    }
}