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
        public const int NumSlots = 5;
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
                    SortNo = entry.api_sortno() ? (int)entry.api_sortno : 0,
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

    public class ShipSpec
    {
        public int Id { get; set; }
        public bool IsEnemy => ShipMaster.IsEnemyId(Id);
        public int SortNo { get; set; }
        public string Name { get; set; }
        public int FuelMax { get; set; }
        public int BullMax { get; set; }
        public int SlotNum { get; set; }
        public Func<int[]> GetMaxEq { get; set; }
        public int[] MaxEq => GetMaxEq?.Invoke();
        public Func<int> GetNumEquips { get; set; }
        public Action<int> SetNumEquips { get; set; }

        public int NumEquips
        {
            get => GetNumEquips();
            set => SetNumEquips(value);
        }

        public int ShipType { get; set; }
        public int ShipClass { get; set; }
        public string ShipTypeName { get; set; }
        public RemodelInfo Remodel { get; } = new RemodelInfo();

        public class RemodelInfo
        {
            public int Level { get; set; }
            public int After { get; set; }
            public int Base { get; set; } // 艦隊晒しページ用
            public int Step { get; set; } // 同上
        }

        public ShipSpec()
        {
            Id = -1;
            Name = "";
        }

        public double RepairWeight
        {
            get
            {
                switch (ShipType)
                {
                    case 1: // 海防艦
                    case 13: // 潜水艦
                        return 0.5;
                    case 2: // 駆逐艦
                    case 3: // 軽巡洋艦
                    case 4: // 重雷装巡洋艦
                    case 14: // 潜水空母
                    case 16: // 水上機母艦
                    case 17: // 揚陸艦
                    case 21: // 練習巡洋艦
                    case 22: // 補給艦
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

        public double TransportPoint
        {
            get
            {
                switch (ShipType)
                {
                    case 2:
                        return 5.0;
                    case 3:
                        return Id == 487 ? 10.0 : 2.0; // 鬼怒改二は大発分を加算
                    case 6:
                        return 4.0;
                    case 10:
                        return 7.0;
                    case 16:
                        return 9.0;
                    case 17:
                        return 12.0;
                    case 20:
                        return 7.0;
                    case 21:
                        return 6.0;
                    case 22:
                        return 15.0;
                    default:
                        return 0;
                }
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
                    case 1: // 海防艦
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

        public bool IsRepairShip => ShipType == 19;

        public bool IsTrainingCruiser => ShipType == 21;
    }
}