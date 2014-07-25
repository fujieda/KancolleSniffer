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
using System.Linq;

namespace KancolleSniffer
{
    public class DockInfo
    {
        private readonly ShipInfo _shipInfo;
        private readonly int[] _ndoc = new int[4];
        private readonly RingTimer[] _ndocTimers = new RingTimer[4];
        private readonly RingTimer[] _kdocTimers = new RingTimer[4];

        public DockInfo(ShipInfo shipInfo)
        {
            _shipInfo = shipInfo;
            for (var i = 0; i < _ndocTimers.Length; i++)
                _ndocTimers[i] = new RingTimer();
            for (var i = 0; i < _kdocTimers.Length; i++)
                _kdocTimers[i] = new RingTimer(0);
        }

        public void InspectNDock(dynamic json)
        {
            foreach (var entry in json)
            {
                var id = (int)entry.api_id;
                _ndocTimers[id - 1].SetEndTime(entry.api_complete_time);
                _ndoc[id - 1] = (int)entry.api_ship_id;
            }
        }

        public NameAndTimer[] NDock
        {
            get
            {
                return _ndoc.Zip(_ndocTimers,
                    (id, timer) => new NameAndTimer {Name = id == 0 ? "" : _shipInfo[id].Name, Timer = timer}).ToArray();
            }
        }

        public bool InNDock(int id)
        {
            return _ndoc.Any(n => n == id); // 空のドックのidは0
        }

        public void InspectKDock(dynamic json)
        {
            foreach (var entry in json)
            {
                var timer = _kdocTimers[(int)entry.api_id - 1];
                var complete = (double)entry.api_complete_time;
                if ((int)complete == 0 && (int)entry.api_created_ship_id != 0)
                    timer.SetEndTime(DateTime.Now.AddHours(-1)); // 過去の時刻を設定する
                else
                    timer.SetEndTime(complete);
            }
        }

        public RingTimer[] KDock
        {
            get { return _kdocTimers; }
        }
    }
}