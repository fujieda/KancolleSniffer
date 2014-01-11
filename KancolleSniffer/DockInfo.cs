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

using System.Collections.Generic;
using System.Linq;

namespace KancolleSniffer
{
    public class DockInfo
    {
        private readonly ShipInfo _shipInfo;
        private readonly NameAndTimer[] _ndocInfo = new NameAndTimer[4];
        private readonly RingTimer[] _kdocTimers = new RingTimer[4];

        public DockInfo(ShipInfo shipInfo)
        {
            _shipInfo = shipInfo;
            for (var i = 0; i < _ndocInfo.Length; i++)
                _ndocInfo[i] = new NameAndTimer();
            for (var i = 0; i < _kdocTimers.Length; i++)
                _kdocTimers[i] = new RingTimer(0);
        }

        public void InspectNDock(dynamic json)
        {
            foreach (var entry in json)
            {
                var id = (int)entry.api_id;
                _ndocInfo[id - 1].Timer.EndTime = (double)entry.api_complete_time;
                var ship = (int)entry.api_ship_id;
                _ndocInfo[id - 1].Name = ship == 0 ? "" : _shipInfo.GetNameById(ship);
            }
        }

        public NameAndTimer[] NDock
        {
            get { return _ndocInfo; }
        }

        public void InspectKDock(dynamic json)
        {
            foreach (var entry in json)
                _kdocTimers[(int)entry.api_id - 1].EndTime = (double)entry.api_complete_time;
        }

        public RingTimer[] KDock
        {
            get { return _kdocTimers; }
        }
    }
}