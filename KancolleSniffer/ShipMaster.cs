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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Codeplex.Data;

namespace KancolleSniffer
{
    public class ShipMaster
    {
        private readonly Dictionary<int, ShipSpec> _shipSpecs = new Dictionary<int, ShipSpec>();

        private readonly string _shipMasterFile =
            Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "shipmaster.json");

        public void InspectShip(dynamic json)
        {
            foreach (var entry in json)
                _shipSpecs[(int)entry.api_id] = new ShipSpec
                {
                    Name = entry.api_name,
                    FuelMax = (int)entry.api_fuel_max,
                    BullMax = (int)entry.api_bull_max
                };
        }

        public ShipSpec GetSpec(int id)
        {
            ShipSpec spec;
            return _shipSpecs.TryGetValue(id, out spec) ? spec : new ShipSpec {Name = "不明"};
        }

        public void Load()
        {
            try
            {
                InspectShip(DynamicJson.Parse(File.ReadAllText(_shipMasterFile)));
            }
            catch (FileNotFoundException)
            {
            }
        }

        public void Save()
        {
            var ship = from data in _shipSpecs
                let val = data.Value
                select
                    new
                    {
                        api_id = data.Key,
                        api_name = val.Name,
                        api_fuel_max = val.FuelMax,
                        api_bull_max = val.BullMax
                    };
            File.WriteAllText(_shipMasterFile, DynamicJson.Serialize(ship));
        }
    }

    public struct ShipSpec
    {
        public string Name { get; set; }
        public int FuelMax { get; set; }
        public int BullMax { get; set; }
    }
}