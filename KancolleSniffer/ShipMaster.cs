// Copyright (C) 2014 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
        private readonly Dictionary<int, ShipSpec> _shipSpecs = new Dictionary<int, ShipSpec>();

        public void Inspect(dynamic json)
        {
            foreach (var entry in json)
            {
                _shipSpecs[(int)entry.api_id] = new ShipSpec
                {
                    Name = entry.api_name,
                    FuelMax = (int)entry.api_fuel_max,
                    BullMax = (int)entry.api_bull_max,
                    ShipType = (int)entry.api_stype,
                };
            }
            _shipSpecs[-1] = new ShipSpec {Name = "不明"};
        }

        public ShipSpec this[int id]
        {
            get { return _shipSpecs[id]; }
        }
    }

    public struct ShipSpec
    {
        public string Name { get; set; }
        public int FuelMax { get; set; }
        public int BullMax { get; set; }
        public int ShipType { get; set; }

        public bool IsSubmarine
        {
            get { return ShipType == 13 || ShipType == 14; }
        }
    }
}