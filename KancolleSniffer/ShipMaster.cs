// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

namespace KancolleSniffer
{
    public class ShipMaster
    {
        public const int NumSlots = 5;
        private readonly Dictionary<int, ShipSpec> _shipSpecs = new Dictionary<int, ShipSpec>();

        public void Inspect(dynamic json)
        {
            var dict = new Dictionary<double, string>();
            foreach (var entry in json.api_mst_stype)
                dict[entry.api_id] = entry.api_name;
            dict[8] = "高速戦艦" ;
            foreach (var entry in json.api_mst_ship)
            {
                var shipSpec = _shipSpecs[(int)entry.api_id] = new ShipSpec
                {
                    Id = (int)entry.api_id,
                    Name = ShipName(entry),
                    FuelMax = entry.api_fuel_max() ? (int)entry.api_fuel_max : 0,
                    BullMax = entry.api_bull_max() ? (int)entry.api_bull_max : 0,
                    ShipType = (int)entry.api_stype,
                    ShipTypeName = dict[entry.api_stype]
                };
                int[] maxEq;
                shipSpec.MaxEq = entry.api_maxeq()
                    ? entry.api_maxeq
                    : MissingData.MaxEq.TryGetValue(shipSpec.Id, out maxEq) ? maxEq : null;
            }
            _shipSpecs[-1] = new ShipSpec();
        }

        // 深海棲艦の名前にelite/flagshipを付ける
        private string ShipName(dynamic json)
        {
            var name = json.api_name;
            var flagship = json.api_yomi;
            if ((int)json.api_id <= 500 || flagship == "-" || flagship == "")
                return name;
            return name + "(" + flagship + ")";
        }

        public ShipSpec this[int id] => _shipSpecs[id];
    }

    public class ShipSpec
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int FuelMax { get; set; }
        public int BullMax { get; set; }
        public int[] MaxEq { get; set; }
        public int ShipType { get; set; }
        public string ShipTypeName { get; set; }

        public ShipSpec()
        {
            Id = -1;
            Name = "";
            MaxEq = new int[0];
        }

        public double RepairWeight
        {
            get
            {
                switch (ShipType)
                {
                    case 13: // 潜水艦
                        return 0.5;
                    case 2: // 駆逐艦
                    case 3: // 軽巡洋艦
                    case 4: // 重雷装巡洋艦
                    case 14: // 潜水空母
                    case 16: // 水上機母艦
                    case 17: // 揚陸艦
                    case 21: // 練習巡洋艦
                        return 1.0;
                    case 5: // 重巡洋艦
                    case 6: // 航空巡洋艦
                    case 7: // 軽空母
                    case 8: // 高速戦艦
                    case 20: // 潜水母艦
                        return 1.5;
                    case 9: // 低速戦艦
                    case 10: // 航空戦艦
                    case 11: // 正規空母
                    case 18: // 装甲空母
                    case 19: // 工作艦
                        return 2.0;
                }
                return 1.0;
            }
        }

        public bool IsSubmarine => ShipType == 13 || ShipType == 14;

        public bool IsAircraftCarrier => ShipType == 7 || ShipType == 11 || ShipType == 18;

        public bool IsAntiSubmarine
        {
            get
            {
                switch (ShipType)
                {
                    case 2: // 駆逐
                    case 3: // 軽巡
                    case 4: // 雷巡
                    case 6: // 航巡
                    case 7: // 軽空
                    case 10: // 航戦
                    case 16: // 水母
                    case 17: // 揚陸艦
                    case 21: // 練巡
                    case 22: // 補給艦
                        return true;
                }
                return false;
            }
        }
    }
}