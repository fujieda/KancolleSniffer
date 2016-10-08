// Copyright (C) 2016 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace KancolleSniffer
{
    public class BaseAirCoprs
    {
        private readonly ItemInfo _itemInfo;
        private List<int> _relocationgPlanes = new List<int>();

        public BaseAirCoprs(ItemInfo item)
        {
            _itemInfo = item;
        }

        public BaseInfo[] AllAirCorps { get; set; }

        public class BaseInfo
        {
            public int AreaId { get; set; }
            public string AreaName => AreaId == 6 ? "中部海域" : "限定海域";
            public AirCorpsInfo[] AirCorps { get; set; }
        }

        public class AirCorpsInfo
        {
            public int Distance { get; set; }
            public int Action { get; set; }
            public PlaneInfo[] Planes { get; set; }

            public string ActionName
            {
                get
                {
                    switch (Action)
                    {
                        case 0:
                            return "待機";
                        case 1:
                            return "出撃";
                        case 2:
                            return "防空";
                        case 3:
                            return "退避";
                        case 4:
                            return "休息";
                        default:
                            return "";
                    }
                }
            }

            public int[] FighterPower => Planes.Aggregate(new[] {0, 0}, (prev, plane) =>
            {
                if (plane.State != 1)
                    return prev;
                var slot = plane.Slot;
                var intercepterBonus = Action == 2
                    ? slot.Spec.AntiBomber * 2 + slot.Spec.Interception
                    : slot.Spec.Interception * 1.5;
                var unskilled = (slot.Spec.AntiAir + intercepterBonus + slot.FighterPowerLevelBonus) * Sqrt(plane.Count);
                return new[]
                {
                    prev[0] + (int)(unskilled + slot.AlvBonus[0]),
                    prev[1] + (int)(unskilled + slot.AlvBonus[1])
                };
            });
        }

        public class PlaneInfo
        {
            public int State { get; set; }
            public ItemStatus Slot { get; set; }
            public int Count { get; set; }
            public int MaxCount { get; set; }

            public string StateName
            {
                get
                {
                    switch (State)
                    {
                        case 0:
                            return "未配備";
                        case 1:
                            return "配備中";
                        case 2:
                            return "配置転換中";
                        default:
                            return "";
                    }
                }
            }
        }

        public void Inspect(dynamic json)
        {
            AllAirCorps = (from entry in (dynamic[])json
                group
                new AirCorpsInfo
                {
                    Distance = (int)entry.api_distance,
                    Action = (int)entry.api_action_kind,
                    Planes = (from plane in (dynamic[])entry.api_plane_info
                        select new PlaneInfo
                        {
                            Slot = _itemInfo.GetStatus((int)plane.api_slotid),
                            State = (int)plane.api_state,
                            Count = plane.api_count() ? (int)plane.api_count : 0,
                            MaxCount = plane.api_max_count() ? (int)plane.api_max_count : 0,
                        }).ToArray()
                } by entry.api_area_id() ? (int)entry.api_area_id : 0
                into grp
                select new BaseInfo {AreaId = grp.Key, AirCorps = grp.ToArray()}).ToArray();
        }

        public void InspectSetPlane(string request, dynamic json)
        {
            if (AllAirCorps == null)
                return;
            var values = HttpUtility.ParseQueryString(request);
            var areaId = int.Parse(values["api_area_id"] ?? "0");
            var airCorps =
                AllAirCorps.First(b => b.AreaId == areaId).AirCorps[int.Parse(values["api_base_id"]) - 1];
            if (json.api_distance()) // 2016春イベにはない
                airCorps.Distance = (int)json.api_distance;
            foreach (var planeInfo in json.api_plane_info)
            {
                var planeId = (int)planeInfo.api_squadron_id - 1;
                var prev = airCorps.Planes[planeId];
                if (prev.Slot.Id != -1)
                    _relocationgPlanes.Add(prev.Slot.Id);
                airCorps.Planes[planeId] = new PlaneInfo
                {
                    Slot = _itemInfo.GetStatus((int)planeInfo.api_slotid),
                    State = (int)planeInfo.api_state,
                    Count = planeInfo.api_count() ? (int)planeInfo.api_count : 0,
                    MaxCount = planeInfo.api_max_count() ? (int)planeInfo.api_max_count : 0,
                };
            }
        }

        public void InspectSupply(string request, dynamic json)
        {
            InspectSetPlane(request, json);
        }

        public void InspectSetAction(string request)
        {
            if (AllAirCorps == null)
                return;
            var values = HttpUtility.ParseQueryString(request);
            var areaId = int.Parse(values["api_area_id"] ?? "0");
            var airCorps = AllAirCorps.First(b => b.AreaId == areaId).AirCorps;
            foreach (var entry in
                values["api_base_id"].Split(',')
                    .Zip(values["api_action_kind"].Split(','), (b, a) => new {baseId = b, action = a}))
            {
                airCorps[int.Parse(entry.baseId) - 1].Action = int.Parse(entry.action);
            }
        }

        public void InspectPlaneInfo(dynamic json)
        {
            _relocationgPlanes = json.api_base_convert_slot()
                ? new List<int>((int[])json.api_base_convert_slot)
                : new List<int>();
        }

        public void InspectEventObject(dynamic json)
        {
            InspectPlaneInfo(json);
        }

        public void SetItemHolder()
        {
            if (AllAirCorps == null)
                return;
            var name = new[] {"第一", "第二", "第三"};
            var i = 0;
            foreach (var baseInfo in AllAirCorps)
            {
                var areaAame = baseInfo.AreaName;
                foreach (var airCorps in baseInfo.AirCorps)
                {
                    if (i >= name.Length)
                        break;
                    var ship = new ShipStatus
                    {
                        Id = 1000 + i,
                        Spec = new ShipSpec {Name = areaAame + " " + name[i++] + "航空隊"}
                    };
                    foreach (var plane in airCorps.Planes)
                    {
                        if (plane.State != 1)
                            continue;
                        _itemInfo.GetStatus(plane.Slot.Id).Holder = ship;
                    }
                }
            }
            if (_relocationgPlanes == null)
                return;
            var relocating = new ShipStatus {Id = 1500, Spec = new ShipSpec {Name = "配置転換中"}};
            foreach (var id in _relocationgPlanes)
                _itemInfo.GetStatus(id).Holder = relocating;
        }
    }
}