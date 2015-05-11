// Copyright (C) 2013, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
// 
// This program is part of KancolleSniffer.
//
// KancolleSniffer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;

namespace KancolleSniffer
{
    public struct QuestStatus
    {
        public int Category { get; set; }
        public string Name { get; set; }
        public int Progress { get; set; }
    }

    public class QuestInfo
    {
        private DateTime _lastCreared;
        private readonly SortedDictionary<int, QuestStatus> _quests = new SortedDictionary<int, QuestStatus>();

        public int QuestCount { get; set; }

        public void Inspect(dynamic json)
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
                        case 2:
                            _quests[id] = new QuestStatus {Category = cat, Name = name, Progress = progress};
                            break;
                        case 1:
                        case 3:
                            _quests.Remove(id);
                            continue;
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

        public QuestStatus[] Quests
        {
            get { return _quests.Values.ToArray(); }
        }
    }
}