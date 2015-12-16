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

using System.Collections.Generic;
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
            => "区分,装備名,改修・熟練度,個数\r\n" +
               string.Join("\r\n",
                   (from item in itemList
                       where item.Spec.Id != -1
                       orderby item.Spec.Type, item.Spec.Id, item.Alv, item.Level
                       group item by
                           $"{item.Spec.TypeName},{item.Spec.Name},{(item.Level == 0 ? item.Alv == 0 ? 0 : item.Alv : item.Level)}"
                       into grp
                       select grp.Key + $",{grp.Count()}"));

        public static string GenerateFleetInfo(Sniffer sniffer)
        {
            var sb = new StringBuilder();
            var fn = new[] { "第一艦隊", "第二艦隊", "第三艦隊", "第四艦隊" };
            for (var f = 0; f < fn.Length; f++)
            {
                sb.Append(fn[f] + "\r\n");
                foreach (var s in sniffer.GetShipStatuses(f))
                {
                    sb.Append($"{s.Name} Lv{s.Level}");
                    foreach (var item in s.Slot.Where(item => item.Id != -1))
                    {
                        sb.Append(" " + item.Spec.Name +
                                  (item.Alv == 0 ? "" : "+" + item.Alv) +
                                  (item.Level == 0 ? "" : "★" + item.Level));
                    }
                    if (s.SlotEx.Id > 0)
                    {
                        var item = s.SlotEx;
                        sb.Append(" " + item.Spec.Name);
                    }
                    sb.Append("\r\n");
                }
                sb.Append($"制空: {sniffer.GetFighterPower(f)} 索敵: {sniffer.GetFleetLineOfSights(f):F1}\r\n");
            }
            return sb.ToString();
        }
    }
}