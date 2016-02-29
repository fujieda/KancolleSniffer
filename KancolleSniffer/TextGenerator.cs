// Copyright (C) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
            => "ID,艦種,艦名,レベル\r\n" +
               string.Join("\r\n",
                   from ship in shipList
                   orderby ship.Spec.ShipType, -ship.Level, ship.ExpToNext
                   select $"{ship.Id},{ship.Spec.ShipTypeName},{ship.Name},{ship.Level}");

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
            var fn = new[] {"第一艦隊", "第二艦隊", "第三艦隊", "第四艦隊"};
            for (var f = 0; f < fn.Length; f++)
            {
                sb.Append(fn[f] + "\r\n");
                foreach (var s in sniffer.GetShipStatuses(f))
                {
                    sb.Append($"{s.Name} Lv{s.Level}");
                    foreach (var item in s.Slot.Where(item => item.Id != -1))
                    {
                        sb.Append(" " + dict[item.Spec.Name] +
                                  (item.Alv == 0 ? "" : "+" + item.Alv) +
                                  (item.Level == 0 ? "" : "★" + item.Level));
                    }
                    if (s.SlotEx.Id > 0)
                    {
                        var item = s.SlotEx;
                        sb.Append(" " + dict[item.Spec.Name]);
                    }
                    sb.Append("\r\n");
                }
                var fp = sniffer.GetFighterPower(f);
                sb.Append($"制空: {(fp[0] == fp[1] ? fp[0].ToString() : fp[0] + "～" + fp[1])} " +
                          $"索敵: {sniffer.GetFleetLineOfSights(f):F1}\r\n");
            }
            return sb.ToString();
        }

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

            public string this[string name]
            {
                get
                {
                    string shortName;
                    return _dict.TryGetValue(name, out shortName) ? shortName : name;
                }
            }
        }

        public static string GenerateDeckBuilderData(Sniffer sniffer)
        {
            var sb = new StringBuilder("{\"version\": 3,");
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
                    var st = ships[s];
                    sb.Append($"\"s{s + 1}\":{{\"id\":\"{st.Spec.Id}\",\"lv\":{st.Level},\"luck\":{st.Lucky},\"items\":{{");
                    var items = st.Slot;
                    for (var i = 0; i < items.Length; i++)
                    {
                        var it = items[i];
                        if (it.Id == -1)
                            continue;
                        if (i != 0)
                            sb.Append(",");
                        sb.Append($"\"i{i + 1}\":{{\"id\":{it.Spec.Id},\"rf\":{(it.Alv != 0 ? it.Alv : it.Level)}}}");
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