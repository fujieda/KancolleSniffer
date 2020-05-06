// Copyright (C) 2018 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Globalization;
using KancolleSniffer.Util;

namespace KancolleSniffer.Model
{
    public class NameAndTimer
    {
        public string Name { get; set; }
        public AlarmTimer Timer { get; set; }

        public NameAndTimer()
        {
            Timer = new AlarmTimer();
        }
    }

    public class AlarmTimer
    {
        private readonly TimeSpan _spare;
        private bool _finished;
        private DateTime _endTime;

        public bool IsFinished(DateTime now) => _endTime != DateTime.MinValue && _endTime - now < _spare || _finished;

        public AlarmTimer(int spare = 60)
        {
            _spare = TimeSpan.FromSeconds(spare);
        }

        public void SetEndTime(double time)
        {
            SetEndTime((int)time == 0
                ? DateTime.MinValue
                : new DateTime(1970, 1, 1).ToLocalTime().AddSeconds(time / 1000));
        }

        public void SetEndTime(DateTime time)
        {
            _endTime = time;
            _finished = false;
        }

        public void Finish()
        {
            _finished = true;
        }

        public bool CheckAlarm(TimeStep step)
        {
            return _endTime != DateTime.MinValue && step.Prev < _endTime - _spare && _endTime - _spare <= step.Now;
        }

        public string ToString(DateTime now, bool endTime = false)
        {
            if (_endTime == DateTime.MinValue && !_finished)
                return "";
            if (endTime)
                return _endTime.ToString(@"dd\ HH\:mm", CultureInfo.InvariantCulture);
            var rest = _finished || _endTime - now < TimeSpan.Zero ? TimeSpan.Zero : _endTime - now;
            return $"{(int)rest.TotalHours:d2}:" + rest.ToString(@"mm\:ss", CultureInfo.InvariantCulture);
        }
    }
}