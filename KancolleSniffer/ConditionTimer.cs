// Copyright (C) 2013, 2014, 2015 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
using System.Linq;

namespace KancolleSniffer
{
    public class ConditionTimer
    {
        private readonly ShipInfo _shipInfo;
        private const int Interval = 180;
        private int _lastCond = int.MinValue;
        private DateTime _lastUpdate;
        private double _regenTime;
        private DateTime _prevNotice;

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
            if (_shipInfo.InMission(fleet) || _shipInfo.InSortie(fleet))
                return DateTime.MinValue;
            var cond = _shipInfo.GetShipStatuses(fleet).Select(s => s.Cond).DefaultIfEmpty(49).Min();
            if (cond >= 49)
                return DateTime.MinValue;
            var nextRegen = NextRegenTime(_lastUpdate);
            return cond >= 46 ? nextRegen : nextRegen.AddSeconds((46 - cond + 2) / 3 * Interval);
        }

        public int[] GetNotice()
        {
            var result = new int[ShipInfo.FleetCount];
            var now = DateTime.Now;
            var prev = _prevNotice;
            _prevNotice = now;
            if (prev == DateTime.MinValue)
                return result;
            for (var f = 0; f < result.Length; f++)
            {
                if (_shipInfo.InMission(f) || _shipInfo.InSortie(f))
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