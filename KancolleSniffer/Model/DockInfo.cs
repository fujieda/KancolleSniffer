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
using System.Linq;
using KancolleSniffer.Util;

namespace KancolleSniffer.Model
{
    public class DockInfo
    {
        public const int DockCount = 4;
        private readonly ShipInventory _shipInventory;
        private readonly MaterialInfo _materialInfo;
        private readonly int[] _ndock = new int[DockCount];
        private readonly AlarmTimer[] _ndockTimers = new AlarmTimer[DockCount];
        private readonly AlarmTimer[] _kdockTimers = new AlarmTimer[DockCount];

        public DockInfo(ShipInventory shipInventory, MaterialInfo material)
        {
            _shipInventory = shipInventory;
            _materialInfo = material;
            for (var i = 0; i < _ndockTimers.Length; i++)
                _ndockTimers[i] = new AlarmTimer();
            for (var i = 0; i < _kdockTimers.Length; i++)
                _kdockTimers[i] = new AlarmTimer(0);
        }

        public void InspectNDock(dynamic json)
        {
            foreach (var entry in json)
            {
                var id = (int)entry.api_id - 1;
                _ndockTimers[id].SetEndTime(entry.api_complete_time);
                var prev = _ndock[id];
                _ndock[id] = (int)entry.api_ship_id;
                if (prev != 0 && _ndock[id] == 0) // 修復完了
                    _shipInventory[prev].RepairShip();
            }
        }

        public void InspectNyukyo(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var id = int.Parse(values["api_ship_id"]);
            var ship = _shipInventory[id];
            var m = ship.NdockItem;
            _materialInfo.SubMaterial(Material.Fuel, m[0]);
            _materialInfo.SubMaterial(Material.Steal, m[1]);
            if (int.Parse(values["api_highspeed"]) == 1)
            {
                ship.RepairShip();
                _materialInfo.SubMaterial(Material.Bucket, 1);
                return;
            }
            if (ship.RepairTime.CompareTo(TimeSpan.FromMinutes(1)) <= 0)
                ship.RepairShip();
        }

        public void InspectSpeedChange(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var dock = int.Parse(values["api_ndock_id"]) - 1;
            _shipInventory[_ndock[dock]].RepairShip();
            _ndock[dock] = 0;
            _ndockTimers[dock].SetEndTime(0);
            _materialInfo.SubMaterial(Material.Bucket, 1);
        }

        public NameAndTimer[] NDock
            => _ndock.Zip(_ndockTimers,
                    (id, timer) => new NameAndTimer {Name = id == 0 ? "" : _shipInventory[id].Name, Timer = timer}).ToArray();

        public bool InNDock(int id) => _ndock.Any(n => n == id); // 空のドックのidは0

        public void InspectKDock(dynamic json)
        {
            foreach (var entry in json)
            {
                var timer = _kdockTimers[(int)entry.api_id - 1];
                var complete = (double)entry.api_complete_time;
                if ((int)complete == 0 && (int)entry.api_created_ship_id != 0)
                {
                    timer.Finish();
                }
                else
                {
                    timer.SetEndTime(complete);
                }
            }
        }

        public void InspectCreateShipSpeedChange(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var dock = int.Parse(values["api_kdock_id"]) - 1;
            _kdockTimers[dock].Finish();
        }

        public AlarmTimer[] KDock => _kdockTimers;
    }
}