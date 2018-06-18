// Copyright (C) 2013, 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Linq;

namespace KancolleSniffer
{
    public class ConditionTimer : IHaveState
    {
        private readonly ShipInfo _shipInfo;
        private const int Interval = 180;
        private int _lastCond = int.MinValue;
        private DateTime _lastUpdate;
        private double _regenTime;

        public bool NeedSave { get; private set; }

        public ConditionTimer(ShipInfo shipInfo)
        {
            _shipInfo = shipInfo;
        }

        public void CalcRegenTime()
        {
            var now = DateTime.Now;
            var prevTime = _lastUpdate;
            var prevCond = _lastCond;
            _lastUpdate = now;
            _lastCond = _shipInfo.ShipList.Min(s => s.Cond);
// ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_regenTime == double.MinValue)
            {
                ResetRegenTime(now);
                return;
            }
            if (prevCond == int.MinValue || prevCond == _lastCond)
                return;
            var next = NextRegenTime(prevTime);
            var ticks = next > now ? 0 : (int)(now - next).TotalSeconds / Interval + 1;
            var diff = (_lastCond - prevCond + 2) / 3 - ticks;
            if (_lastCond == 49 ? diff > 0 : diff != 0)
                ResetRegenTime(now);
        }

        private DateTime NextRegenTime(DateTime now)
        {
            var batch = new DateTime((long)((now.Ticks / TimeSpan.TicksPerSecond / Interval * Interval + _regenTime) *
                                            TimeSpan.TicksPerSecond));
            return batch < now ? batch.AddSeconds(Interval) : batch;
        }

        private void ResetRegenTime(DateTime now)
        {
            _regenTime = (double)now.Ticks / TimeSpan.TicksPerSecond % Interval;
            NeedSave = true;
        }

        public void InvalidateCond()
        {
            _lastCond = int.MinValue;
        }

        public void CheckCond()
        {
            if (_lastCond != _shipInfo.ShipList.Min(s => s.Cond))
                _lastCond = int.MinValue;
        }

        public DateTime GetTimer(int fleet)
        {
            var target = _shipInfo.Fleets[fleet];
            if (target.State != FleetState.Port)
                return DateTime.MinValue;
            var cond = target.Ships.Select(s => s.Cond).DefaultIfEmpty(49).Min();
            if (cond >= 49)
                return DateTime.MinValue;
            var nextRegen = NextRegenTime(_lastUpdate);
            return cond >= 46 ? nextRegen : nextRegen.AddSeconds((46 - cond + 2) / 3 * Interval);
        }

        public int[] GetNotice(DateTime prev, DateTime now)
        {
            var result = new int[ShipInfo.FleetCount];
            if (prev == DateTime.MinValue)
                return result;
            for (var f = 0; f < result.Length; f++)
            {
                if (_shipInfo.Fleets[f].State != FleetState.Port)
                    continue;
                var timer = GetTimer(f);
                if (timer == DateTime.MinValue || prev < _lastUpdate)
                    continue;
                if (prev < timer.AddMinutes(-9) && now >= timer.AddMinutes(-9))
                    result[f] = 40;
                else if (prev < timer && now >= timer)
                    result[f] = 49;
            }
            return result;
        }

        public void SaveState(Status status)
        {
            NeedSave = false;
            status.CondRegenTime = _regenTime;
        }

        public void LoadState(Status status)
        {
            _regenTime = status.CondRegenTime;
        }
    }
}