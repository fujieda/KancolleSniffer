// Copyright (C) 2018 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

namespace KancolleSniffer.Model
{
    public class ShipSpec
    {
        public int Id { get; set; }
        public bool IsEnemy => ShipMaster.IsEnemyId(Id);
        public int SortId { get; set; }
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