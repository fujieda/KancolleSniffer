﻿// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
    public class DockInfo
    {
        public const int DockCount = 4;
        private readonly ShipInfo _shipInfo;
        private readonly MaterialInfo _materialInfo;
        private readonly int[] _ndoc = new int[DockCount];
        private readonly RingTimer[] _ndocTimers = new RingTimer[DockCount];
        private readonly RingTimer[] _kdocTimers = new RingTimer[DockCount];

        public DockInfo(ShipInfo ship, MaterialInfo material)
        {
            _shipInfo = ship;
            _materialInfo = material;
            for (var i = 0; i < _ndocTimers.Length; i++)
                _ndocTimers[i] = new RingTimer();
            for (var i = 0; i < _kdocTimers.Length; i++)
                _kdocTimers[i] = new RingTimer(0);
        }

        public void InspectNDock(dynamic json)
        {
            foreach (var entry in json)
            {
                var id = (int)entry.api_id - 1;
                _ndocTimers[id].SetEndTime(entry.api_complete_time);
                var prev = _ndoc[id];
                _ndoc[id] = (int)entry.api_ship_id;
                if (prev != 0 && _ndoc[id] == 0) // 修復完了
                    _shipInfo.RepairShip(prev);
            }
        }

        public void InspectNyukyo(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var id = int.Parse(values["api_ship_id"]);
            int fuel, steal;
            _shipInfo.GetStatus(id).CalcMaterialsToRepair(out fuel, out steal);
            _materialInfo.SubMaterial(Material.Fuel, fuel);
            _materialInfo.SubMaterial(Material.Steal, steal);
            if (int.Parse(values["api_highspeed"]) == 0)
                return;
            _shipInfo.RepairShip(id);
            _materialInfo.SubMaterial(Material.Bucket, 1);
        }

        public void InspectSpeedChange(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var dock = int.Parse(values["api_ndock_id"]) - 1;
            _shipInfo.RepairShip(_ndoc[dock]);
            _ndoc[dock] = 0;
            _ndocTimers[dock].SetEndTime(0);
            _materialInfo.SubMaterial(Material.Bucket, 1);
        }

        public NameAndTimer[] NDock
            => _ndoc.Zip(_ndocTimers,
                    (id, timer) => new NameAndTimer {Name = id == 0 ? "" : _shipInfo.GetStatus(id).Name, Timer = timer}).ToArray();

        public bool InNDock(int id) => _ndoc.Any(n => n == id); // 空のドックのidは0

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

        public void InspectCreateShipSpeedChange(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var dock = int.Parse(values["api_kdock_id"]) - 1;
            _kdocTimers[dock].SetEndTime(DateTime.Now.AddHours(-1));
        }

        public RingTimer[] KDock => _kdocTimers;
    }
}