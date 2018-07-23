// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Xml.Serialization;

namespace KancolleSniffer.Model
{
    public class Achievement : IHaveState
    {
        private int _current;

        public int Start { get; set; }
        public int StartOfMonth { get; set; }
        public DateTime LastReset { get; set; }
        public DateTime LastResetOfMonth { get; set; }

        private const double ExpPerAch = 10000 / 7.0;
        public double Value => (_current - Start) / ExpPerAch;
        public double ValueOfMonth => (_current - StartOfMonth) / ExpPerAch;

        [XmlIgnore]
        public List<int> ResetHours { private get; set; }

        [XmlIgnore]
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
                return;
            Start = ac.Start;
            StartOfMonth = ac.StartOfMonth;
            LastReset = ac.LastReset;
            LastResetOfMonth = ac.LastResetOfMonth;
        }
    }
}