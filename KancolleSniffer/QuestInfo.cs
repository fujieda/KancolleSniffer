// Copyright (C) 2013 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
    public class QuestInfo
    {
        private DateTime _lastCreared;
        private readonly SortedDictionary<int, NameAndProgress> _quests = new SortedDictionary<int, NameAndProgress>();

        public void Inspect(dynamic json)
        {
            var resetTime = DateTime.Today.AddHours(5);
            if (DateTime.Now >= resetTime && _lastCreared < resetTime)
            {
                _quests.Clear(); // 前日に未消化のデイリーを消す。
                _lastCreared = DateTime.Now;
            }
            foreach (var entry in json.api_list)
            {
                if (entry is double) // -1の場合がある。
                    continue;

                var id = (int)entry.api_no;
                var state = (int)entry.api_state;
                var progress = (int)entry.api_progress_flag;
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
                        _quests[id] = new NameAndProgress {Name = name, Progress = progress};
                        break;
                    case 1:
                    case 3:
                        _quests.Remove(id);
                        continue;
                }
            }
        }

        public NameAndProgress[] Quests
        {
            get { return _quests.Values.ToArray(); }
        }

        public struct NameAndProgress
        {
            public string Name { get; set; }
            public int Progress { get; set; }
        }
    }
}