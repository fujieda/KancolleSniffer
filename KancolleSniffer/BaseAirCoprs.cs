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

using System.Linq;
using static System.Math;

namespace KancolleSniffer
{
    public class BaseAirCoprs
    {
        private readonly ItemInfo _itemInfo;

        public BaseAirCoprs(ItemInfo item)
        {
            _itemInfo = item;
        }

        public AirCorpsInfo[] AirCorps { get; set; }

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
                var unskilled = (slot.Spec.AntiAir + slot.Spec.Interception * 1.5 + slot.FighterPowerLevelBonus) *
                                Sqrt(plane.Count);
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
            AirCorps = (from entry in (dynamic[])json
                select
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
                    }).ToArray();
        }

        public void InspectSetPlane(string request, dynamic json)
        {
            if (AirCorps == null)
                return;
            var values = HttpUtility.ParseQueryString(request);
            var planeInfo = json.api_plane_info[0];
            var airCorps = AirCorps[int.Parse(values["api_base_id"]) - 1];
            airCorps.Distance = (int)json.api_distance;
            airCorps.Planes[(int)planeInfo.api_squadron_id - 1] = new PlaneInfo
            {
                Slot = _itemInfo.GetStatus((int)planeInfo.api_slotid),
                State = (int)planeInfo.api_state,
                Count = planeInfo.api_count() ? (int)planeInfo.api_count : 0,
                MaxCount = planeInfo.api_max_count() ? (int)planeInfo.api_max_count : 0,
            };
        }

        public void InspectSetAction(string request)
        {
            if (AirCorps == null)
                return;
            var values = HttpUtility.ParseQueryString(request);
            foreach (var entry in
                values["api_base_id"].Split(',')
                    .Zip(values["api_action_kind"].Split(','), (b, a) => new {baseId = b, action = a}))
            {
                AirCorps[int.Parse(entry.baseId) - 1].Action = int.Parse(entry.action);
            }
        }
    }
}