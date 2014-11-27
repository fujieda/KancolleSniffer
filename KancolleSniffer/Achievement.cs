// Copyright (C) 2014 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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

namespace KancolleSniffer
{
    public class Achievement
    {
        private int _start;
        private int _current;
        private DateTime _lastReset;

        public double Value { get { return (_current - _start) / 1428.0; } }
        public List<int> ResetHours { get; set; }

        public Achievement()
        {
            ResetHours = new List<int>();
        }

        public void InspectBasic(dynamic json)
        {
            _current = (int)json.api_experience;
            if (_start == 0)
                Reset(_current);
            foreach (var hour in ResetHours)
            {
                var time = DateTime.Today.AddHours(hour);
                if (DateTime.Now >= time && _lastReset < time)
                    Reset(_current);
            }
        }

        public void Reset()
        {
            Reset(_current);
        }

        private void Reset(int current)
        {
            _start = current;
            _lastReset = DateTime.Now;
        }

        public void SaveState(Status status)
        {
            status.ExperiencePoint = _start;
            status.LastResetTime = _lastReset;
        }

        public void LoadState(Status status)
        {
            _start = status.ExperiencePoint;
            _lastReset = status.LastResetTime;
            if (_lastReset == DateTime.MinValue)
                _lastReset = DateTime.Now;
        }
    }
}