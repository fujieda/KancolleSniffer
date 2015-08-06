// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
    public class Achievement : IHaveState
    {
        private int _current;

        public int Start { get; set; }
        public int StartOfMonth { get; set; }
        public DateTime LastReset { get; set; }
        public DateTime LastResetOfMonth { get; set; }

        private const double ExpPerAch = 1428.0;
        public double Value => (_current - Start) / ExpPerAch;
        public double ValueOfMonth => (_current - StartOfMonth) / ExpPerAch;
        public List<int> ResetHours { get; set; }
        public bool NeedSave { get; private set; }

        public Achievement()
        {
            ResetHours = new List<int>();
        }

        public void InspectBasic(dynamic json)
        {
            var now = DateTime.Now;
            var today = DateTime.Today;
            _current = (int)json.api_experience;
            if (Start == 0)
                Reset(_current);
            if (StartOfMonth == 0)
                ResetMonth(_current);
            foreach (var hour in ResetHours)
            {
                var time = today.AddHours(hour);
                if (now >= time && LastReset < time)
                    Reset(_current);
            }
            var limitTime = now.AddDays(1).Month != now.Month // 今日が今月末
                ? today.AddHours(22) // 今日22時
                : today.AddDays(-today.Day).AddHours(22); // 先月末22時
            if (now >= limitTime && LastResetOfMonth < limitTime)
                ResetMonth(_current);
        }

        public void Reset()
        {
            Reset(_current);
        }

        private void Reset(int current)
        {
            Start = current;
            LastReset = DateTime.Now;
            NeedSave = true;
        }

        private void ResetMonth(int current)
        {
            StartOfMonth = current;
            LastResetOfMonth = DateTime.Now;
            NeedSave = true;
        }

        public void SaveState(Status status)
        {
            NeedSave = false;
            status.Achievement = this;
        }

        public void LoadState(Status status)
        {
            var ac = status.Achievement;
            if (ac == null)
            {
                Start = status.ExperiencePoint;
                LastReset = status.LastResetTime;
            }
            else
            {
                Start = ac.Start;
                StartOfMonth = ac.StartOfMonth;
                LastReset = ac.LastReset;
                LastResetOfMonth = ac.LastResetOfMonth;
            }
        }
    }
}