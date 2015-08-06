// Copyright (C) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
    public class ExMapInfo : IHaveState
    {
        public class ClearStatus
        {
            public int Map { get; set; }
            public bool Cleared { get; set; }
            public int Rate { get; set; }
        }

        private readonly Dictionary<int, ClearStatus> _clearStatus =
            new Dictionary<int, ClearStatus>
            {
                {15, new ClearStatus {Map = 15, Cleared = false, Rate = 75}},
                {16, new ClearStatus {Map = 16, Cleared = false, Rate = 75}},
                {25, new ClearStatus {Map = 25, Cleared = false, Rate = 100}},
                {35, new ClearStatus {Map = 35, Cleared = false, Rate = 150}},
                {45, new ClearStatus {Map = 45, Cleared = false, Rate = 180}},
                {55, new ClearStatus {Map = 55, Cleared = false, Rate = 200}}
            };

        private DateTime _lastReset;

        private int _currentMap;

        public bool NeedSave { get; private set; }

        public void InspectMapInfo(dynamic json)
        {
            foreach (var entry in json)
            {
                var map = (int)entry.api_id;
                if (map % 10 <= 4)
                    continue;
                ClearStatus stat;
                if (!_clearStatus.TryGetValue(map, out stat))
                    _clearStatus.Add(map, stat = new ClearStatus { Map = map});
                var prev = stat.Cleared;
                stat.Cleared = (int)entry.api_cleared == 1;
                if (prev != stat.Cleared)
                    NeedSave = true;
            }
        }

        public void InspectMapStart(dynamic json)
        {
            InspectMapNext(json);
        }

        public void InspectMapNext(dynamic json)
        {
            _currentMap = (int)json.api_maparea_id * 10 + (int)json.api_mapinfo_no;
            if (!json.api_get_eo_rate() || json.api_get_eo_rate == 0)
                return;
            ClearStatus stat;
            if (!_clearStatus.TryGetValue(_currentMap, out stat))
                _clearStatus.Add(_currentMap, stat = new ClearStatus{Map = _currentMap});
            stat.Cleared = true;
            stat.Rate = (int)json.api_get_eo_rate;
            NeedSave = true;
        }

        public void InspectBattleResult(dynamic json)
        {
            if (!json.api_get_exmap_rate())
                return;
            var rate = json.api_get_exmap_rate is string
                ? int.Parse(json.api_get_exmap_rate)
                : (int)json.api_get_exmap_rate;
            if (rate == 0)
                return;
            ClearStatus stat;
            if (!_clearStatus.TryGetValue(_currentMap, out stat))
                _clearStatus.Add(_currentMap, stat = new ClearStatus{Map = _currentMap});
            stat.Cleared = true;
            stat.Rate = rate;
            NeedSave = true;
        }

        public int Achievement => _clearStatus.Values.Where(s => s.Cleared).Sum(s => s.Rate);

        public void ResetIfNeeded()
        {
            var now = DateTime.Now;
            if (_lastReset.Month == now.Month)
                return;
            _lastReset = now;
            foreach (var e in _clearStatus.Values)
                e.Cleared = false;
        }

        // テスト用
        public void ClearClearStatus()
        {
            _clearStatus.Clear();
        }

        public void SaveState(Status status)
        {
            NeedSave = false;
            status.ExMapState = new ExMapState
            {
                ClearStatusList = _clearStatus.Values.ToArray(),
                LastReset = _lastReset
            };
        }

        public void LoadState(Status status)
        {
            if (status.ExMapState == null)
                return;
            _lastReset = status.ExMapState.LastReset;
            foreach (var s in status.ExMapState.ClearStatusList)
                _clearStatus[s.Map] = s;
        }

        public class ExMapState
        {
            public ClearStatus[] ClearStatusList { get; set; }
            public DateTime LastReset { get; set; }
        }
    }
}