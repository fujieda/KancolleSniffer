﻿// Copyright (C) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.IO;
using System.Linq;
using System.Text;

namespace KancolleSniffer
{
    public static class TextGenerator
    {
        public static string GenerateShipList(IEnumerable<ShipStatus> shipList)
            => "ID,艦種,艦名,レベル,cond,素対潜\r\n" +
               string.Join("\r\n",
                   from ship in shipList
                   orderby ship.Spec.ShipType, -ship.Level, ship.ExpToNext
                   select $"{ship.Id},{ship.Spec.ShipTypeName},{ship.Name},{ship.Level},{ship.Cond},{ship.ShipAntiSubmarine}");

        public static string GenerateKantaiSarashiData(IEnumerable<ShipStatus> shipList)
        {
            return ".2|" +
                   string.Join("|", from ship in shipList
                       where ship.Locked
                       group ship by ship.Spec.Remodel.Base
                       into grp
                       orderby grp.Key
                       select grp.Key + ":" + string.Join(",", from ship in grp
                           orderby -ship.Level
                           select ship.Level +
                                  (ship.Level >= ship.Spec.Remodel.Level && ship.Spec.Remodel.Step != 0
                                      ? "." + ship.Spec.Remodel.Step
                                      : "")));
        }

        public static string GenerateItemList(IEnumerable<ItemStatus> itemList)
            => "区分,装備名,熟練度,改修,個数\r\n" +
               string.Join("\r\n",
                   (from item in itemList
                       where item.Spec.Id != -1
                       orderby item.Spec.Type, item.Spec.Id, item.Alv, item.Level
                       group item by
                           $"{item.Spec.TypeName},{item.Spec.Name},{item.Alv},{item.Level}"
                       into grp
                       select grp.Key + $",{grp.Count()}"));

        public static string GenerateFleetData(Sniffer sniffer)
        {
            var dict = new ItemName();
            var sb = new StringBuilder();
            for (var f = 0; f < ShipInfo.FleetCount; f++)
                sb.Append(GenerateFleetData(sniffer, f, dict));
            sb.Append(GenerateBaseAirCorps(sniffer, dict));
            return sb.ToString();
        }

        public static string GenerateFleetData(Sniffer sniffer, int fleet)
        {
            return GenerateFleetData(sniffer, fleet, new ItemName()).ToString();
        }

        private static StringBuilder GenerateFleetData(Sniffer sniffer, int fleet, ItemName dict)
        {
            var sb = new StringBuilder();
            var fn = new[] {"第一艦隊", "第二艦隊", "第三艦隊", "第四艦隊"};
            sb.Append(fn[fleet] + "\r\n");
            sb.Append(string.Concat(from s in sniffer.GetShipStatuses(fleet)
                select ($"{s.Name} Lv{s.Level} " +
                        string.Join(",",
                            from item in s.AllSlot
                            where item.Id != -1
                            select dict[item.Spec.Name] + ItemStatusString(item))).TrimEnd(' ') + "\r\n"));
            var fp = sniffer.GetFighterPower(fleet);
            sb.Append($"制空: {(fp[0] == fp[1] ? fp[0].ToString() : fp[0] + "～" + fp[1])} " +
                      $"索敵: {sniffer.GetFleetLineOfSights(fleet, 1):F1}\r\n");
            return sb;
        }

        private static StringBuilder GenerateBaseAirCorps(Sniffer sniffer, ItemName dict)
        {
            var sb = new StringBuilder();
            if (sniffer.BaseAirCorps == null)
                return sb;
            foreach (var baseInfo in sniffer.BaseAirCorps)
            {
                sb.Append(baseInfo.AreaName + " 基地航空隊\r\n");
                var i = 0;
                var name = new[] {"第一 ", "第二 ", "第三 "};
                foreach (var airCorps in baseInfo.AirCorps)
                {
                    sb.Append(name[i++]);
                    sb.Append(
                        string.Join(",",
                            from plane in airCorps.Planes
                            select plane.State == 1
                                ? dict[plane.Slot.Spec.Name] + ItemStatusString(plane.Slot)
                                : plane.StateName) + "\r\n");
                }
            }
            return sb;
        }

        private static string ItemStatusString(ItemStatus item)
            => (item.Alv == 0 ? "" : "+" + item.Alv) + (item.Level == 0 ? "" : "★" + item.Level);

        private class ItemName
        {
            private readonly Dictionary<string, string> _dict = new Dictionary<string, string>();

            public ItemName()
            {
                try
                {
                    foreach (var line in File.ReadLines("ItemName.csv"))
                    {
                        var cols = line.Split(',');
                        _dict[cols[0]] = cols[1];
                    }
                }
                catch (IOException)
                {
                }
            }

            public string this[string name] => _dict.TryGetValue(name, out var shortName) ? shortName : name;
        }

        public static string GenerateDeckBuilderData(Sniffer sniffer)
        {
            var sb = new StringBuilder("{\"version\": 4,");
            for (var f = 0; f < ShipInfo.FleetCount; f++)
            {
                if (f != 0)
                    sb.Append(",");
                sb.Append($"\"f{f + 1}\":{{");
                var ships = sniffer.GetShipStatuses(f);
                for (var s = 0; s < ships.Length; s++)
                {
                    if (s != 0)
                        sb.Append(",");
                    var ship = ships[s];
                    sb.Append(
                        $"\"s{s + 1}\":{{\"id\":\"{ship.Spec.Id}\",\"lv\":{ship.Level},\"luck\":{ship.Lucky},\"items\":{{");
                    var items = ship.Slot;
                    for (var i = 0; i < items.Length; i++)
                    {
                        var item = items[i];
                        if (item.Id == -1)
                            continue;
                        if (i != 0)
                            sb.Append(",");
                        sb.Append($"\"i{i + 1}\":{{\"id\":{item.Spec.Id},\"rf\":{item.Level},\"mas\":{item.Alv}}}");
                    }
                    if (ship.SlotEx.Id != 0 && ship.SlotEx.Id != -1)
                    {
                        if (ship.Slot.Any(item => item.Id != -1))
                            sb.Append(",");
                        var name = ship.Spec.SlotNum == 5 ? "ix" : $"i{ship.Spec.SlotNum + 1}";
                        sb.Append($"\"{name}\":{{\"id\":{ship.SlotEx.Spec.Id},\"rf\":{ship.SlotEx.Level}}}");
                    }
                    sb.Append("}}");
                }
                sb.Append("}");
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}