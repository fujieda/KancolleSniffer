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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using KancolleSniffer.Util;

namespace KancolleSniffer.Model
{
    public class BaseAirCorps
    {
        private readonly ItemInfo _itemInfo;
        private List<int> _relocatingPlanes = new List<int>();

        public BaseAirCorps(ItemInfo item)
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

        public class FighterPower
        {
            public int[] AirCombat { get; set; }
            public int[] Interception { get; set; }
            public bool IsInterceptor => AirCombat[0] != Interception[0];
        }

        public class Distance
        {
            public int Base { get; set; }
            public int Bonus { get; set; }

            public override string ToString() => Bonus > 0 ? $"{Base}+{Bonus}" : Base.ToString();
        }

        public class AirCorpsInfo
        {
            public Distance Distance { get; set; }
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

            public FighterPower FighterPower =>
                new FighterPower {AirCombat = CalcFighterPower(false), Interception = CalcFighterPower(true)};

            private int[] CalcFighterPower(bool interception)
            {
                var reconPlaneBonus = interception
                    ? Planes.Max(plane => plane.Slot.Spec.ReconPlaneInterceptionBonus)
                    : 1.0;
                return Planes.Aggregate(new[] {0, 0}, (prev, plane) =>
                {
                    if (plane.State != 1)
                        return prev;
                    var cur = plane.Slot.CalcFighterPowerInBase(plane.Count, interception);
                    return new[] {prev[0] + cur[0], prev[1] + cur[1]};
                }).Select(fp => (int)(fp * reconPlaneBonus)).ToArray();
            }

            public int[] CostForSortie => Planes.Aggregate(new[] {0, 0}, (prev, plane) =>
            {
                if (plane.State != 1)
                    return prev;
                int fuel, bull;
                if (plane.Slot.Spec.Type == 47)
                {
                    fuel = (int)Math.Ceiling(plane.Count * 1.5);
                    bull = (int)(plane.Count * 0.7);
                }
                else
                {
                    fuel = plane.Count;
                    bull = (int)Math.Ceiling(plane.Count * 0.6);
                }
                return new[] {prev[0] + fuel, prev[1] + bull};
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

            public FighterPower FighterPower
                => new FighterPower{AirCombat = CalcFighterPower(false), Interception = CalcFighterPower(true)};

            private int[] CalcFighterPower(bool interception) => Slot.CalcFighterPowerInBase(Count, interception);
        }

        public void Inspect(dynamic json)
        {
            AllAirCorps = (from entry in (dynamic[])json
                group
                new AirCorpsInfo
                {
                    Distance = CreateDistance(entry.api_distance),
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
            var airCorps = GetBaseInfo(values).AirCorps[int.Parse(values["api_base_id"]) - 1];
            if (json.api_distance()) // 2016春イベにはない
                airCorps.Distance = CreateDistance(json.api_distance);
            foreach (var planeInfo in json.api_plane_info)
            {
                var planeId = (int)planeInfo.api_squadron_id - 1;
                var prev = airCorps.Planes[planeId];
                if (!prev.Slot.Empty)
                    _relocatingPlanes.Add(prev.Slot.Id);
                airCorps.Planes[planeId] = new PlaneInfo
                {
                    Slot = _itemInfo.GetStatus((int)planeInfo.api_slotid),
                    State = (int)planeInfo.api_state,
                    Count = planeInfo.api_count() ? (int)planeInfo.api_count : 0,
                    MaxCount = planeInfo.api_max_count() ? (int)planeInfo.api_max_count : 0
                };
            }
        }

        private Distance CreateDistance(dynamic distance) => distance is double
            // ReSharper disable once PossibleInvalidCastException
            ? new Distance {Base = (int)distance}
            : new Distance {Base = (int)distance.api_base, Bonus = (int)distance.api_bonus};

        public void InspectSupply(string request, dynamic json)
        {
            InspectSetPlane(request, json);
        }

        public void InspectSetAction(string request)
        {
            if (AllAirCorps == null)
                return;
            var values = HttpUtility.ParseQueryString(request);
            var airCorps = GetBaseInfo(values).AirCorps;
            foreach (var entry in
                values["api_base_id"].Split(',')
                    .Zip(values["api_action_kind"].Split(','), (b, a) => new {baseId = b, action = a}))
            {
                airCorps[int.Parse(entry.baseId) - 1].Action = int.Parse(entry.action);
            }
        }

        public void InspectExpandBase(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            var baseInfo = GetBaseInfo(values);
            var airCorps = baseInfo.AirCorps;
            Array.Resize(ref airCorps, airCorps.Length + 1);
            baseInfo.AirCorps = airCorps;
            airCorps[airCorps.Length - 1] = new AirCorpsInfo
            {
                Planes =
                    ((dynamic[])json[0].api_plane_info).
                        Select(plane => new PlaneInfo {Slot = new ItemStatus()}).ToArray()
            };
        }

        private BaseInfo GetBaseInfo(NameValueCollection values)
        {
            var areaId = int.Parse(values["api_area_id"] ?? "0"); // 古いAPIに対応するため
            return AllAirCorps.First(b => b.AreaId == areaId);
        }

        public void InspectPlaneInfo(dynamic json)
        {
            _relocatingPlanes = json.api_base_convert_slot()
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
            foreach (var baseInfo in AllAirCorps.Select((data, i) => new {data, i}))
            {
                var areaName = baseInfo.data.AreaName;
                foreach (var airCorps in baseInfo.data.AirCorps.Select((data, i) => new {data, i}))
                {
                    var ship = new ShipStatus
                    {
                        Id = 10000 + baseInfo.i * 1000 + airCorps.i,
                        Spec = new ShipSpec {Name = areaName + " " + name[airCorps.i] + "航空隊"}
                    };
                    foreach (var plane in airCorps.data.Planes)
                    {
                        if (plane.State != 1)
                            continue;
                        _itemInfo.GetStatus(plane.Slot.Id).Holder = ship;
                    }
                }
            }
            if (_relocatingPlanes == null)
                return;
            var relocating = new ShipStatus {Id = 1500, Spec = new ShipSpec {Name = "配置転換中"}};
            foreach (var id in _relocatingPlanes)
                _itemInfo.GetStatus(id).Holder = relocating;
        }
    }
}