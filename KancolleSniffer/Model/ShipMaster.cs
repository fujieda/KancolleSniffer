// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

namespace KancolleSniffer.Model
{
    public class ShipMaster
    {
        private readonly Dictionary<int, ShipSpec> _shipSpecs = new Dictionary<int, ShipSpec>();
        public AdditionalData AdditionalData { get; set; }

        public static bool IsEnemyId(int id) => id > 1500;

        public void Inspect(dynamic json)
        {
            var dict = new Dictionary<double, string>();
            foreach (var entry in json.api_mst_stype)
                dict[entry.api_id] = entry.api_name;
            dict[8] = "巡洋戦艦";
            AdditionalData.LoadEnemySlot();
            AdditionalData.LoadNumEquips();
            foreach (var entry in json.api_mst_ship)
            {
                var shipSpec = _shipSpecs[(int)entry.api_id] = new ShipSpec
                {
                    Id = (int)entry.api_id,
                    SortId = entry.api_sort_id() ? (int)entry.api_sort_id : 0,
                    Name = ShipName(entry),
                    FuelMax = entry.api_fuel_max() ? (int)entry.api_fuel_max : 0,
                    BullMax = entry.api_bull_max() ? (int)entry.api_bull_max : 0,
                    SlotNum = (int)entry.api_slot_num,
                    ShipType = (int)entry.api_stype,
                    ShipTypeName = dict[entry.api_stype],
                    ShipClass = entry.api_ctype() ? (int)entry.api_ctype : 0
                };
                if (entry.api_afterlv())
                {
                    shipSpec.Remodel.Level = (int)entry.api_afterlv;
                    shipSpec.Remodel.After = int.Parse(entry.api_aftershipid);
                }
                shipSpec.GetMaxEq = entry.api_maxeq()
                    ? (Func<int[]>)(() => entry.api_maxeq)
                    : () => AdditionalData.EnemySlot(shipSpec.Id);
                shipSpec.GetNumEquips = () => AdditionalData.NumEquips(shipSpec.Id);
                shipSpec.SetNumEquips = num => AdditionalData.RecordNumEquips(shipSpec.Id, shipSpec.Name,num);
            }
            _shipSpecs[-1] = new ShipSpec();
            SetRemodelBaseAndStep();
        }

        // 深海棲艦の名前にelite/flagshipを付ける
        private string ShipName(dynamic json)
        {
            var name = json.api_name;
            var flagship = json.api_yomi;
            if (!IsEnemyId((int)json.api_id) || flagship == "-" || flagship == "")
                return name;
            return name + "(" + flagship + ")";
        }

        public ShipSpec GetSpec(int id) => _shipSpecs.TryGetValue(id, out var spec) ? spec : new ShipSpec();

        private void SetRemodelBaseAndStep()
        {
            // 改造後のデータをマーク
            foreach (var spec in _shipSpecs.Values)
            {
                if (spec.Remodel.After == 0)
                    continue;
                _shipSpecs[spec.Remodel.After].Remodel.Base = 1;
            }
            foreach (var spec in _shipSpecs.Values)
            {
                if (spec.Remodel.Base != 0)
                    continue;
                var step = 0;
                var hash = new HashSet<int> {spec.Id};
                var s = spec;
                s.Remodel.Base = spec.Id;
                while (s.Remodel.After != 0)
                {
                    s.Remodel.Step = ++step;
                    if (!hash.Add(s.Remodel.After))
                        break;
                    s = _shipSpecs[s.Remodel.After];
                    s.Remodel.Base = spec.Id;
                }
            }
        }

        /// <summary>
        /// テスト用
        /// </summary>
        /// <param name="id"></param>
        public void InjectSpec(int id) => _shipSpecs[id] = new ShipSpec {Id = id};
    }
}