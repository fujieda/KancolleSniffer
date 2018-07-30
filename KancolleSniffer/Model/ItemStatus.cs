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
using System.Linq;

namespace KancolleSniffer.Model
{
    public class ItemStatus
    {
        public int Id { get; set; }
        public bool Empty => Id == -1;
        public bool Unimplemented => Id == 0;
        public ItemSpec Spec { get; set; } = new ItemSpec();
        public int Level { get; set; }
        public int Alv { get; set; }
        public ShipStatus Holder { get; set; }

        public ItemStatus()
        {
            Id = -1;
        }

        public ItemStatus(int id)
        {
            Id = id;
        }

        public int[] CalcFighterPower(int slot)
        {
            if (!Spec.CanAirCombat || slot == 0)
                return new[] {0, 0};
            var unskilled = (Spec.AntiAir + FighterPowerLevelBonus) * Math.Sqrt(slot);
            return AlvBonus.Select(bonus => (int)(unskilled + bonus)).ToArray();
        }

        public int[] CalcFighterPowerInBase(int slot, bool airDefence)
        {
            if (!Spec.IsAircraft || slot == 0)
                return new[] {0, 0};
            var airDefenceBonus = airDefence ? Spec.AntiBomber * 2 + Spec.Interception : Spec.Interception * 1.5;
            var unskilled = (Spec.AntiAir + airDefenceBonus + FighterPowerLevelBonus) * Math.Sqrt(slot);
            return AlvBonusInBase.Select(bonus => (int)(unskilled + bonus)).ToArray();
        }

        private readonly double[] _alvBonusMin =
        {
            Math.Sqrt(0.0), Math.Sqrt(1.0), Math.Sqrt(2.5), Math.Sqrt(4.0), Math.Sqrt(5.5), Math.Sqrt(7.0),
            Math.Sqrt(8.5), Math.Sqrt(10.0)
        };

        private readonly double[] _alvBonusMax =
        {
            Math.Sqrt(0.9), Math.Sqrt(2.4), Math.Sqrt(3.9), Math.Sqrt(5.4), Math.Sqrt(6.9), Math.Sqrt(8.4),
            Math.Sqrt(9.9), Math.Sqrt(12.0)
        };

        private int[] AlvTypeBonusTable
        {
            get
            {
                switch (Spec.Type)
                {
                    case 6: // 艦戦
                    case 45: // 水戦
                    case 48: // 局地戦闘機
                    case 56: // 噴式戦闘機
                        return new[] {0, 0, 2, 5, 9, 14, 14, 22};
                    case 7: // 艦爆
                    case 8: // 艦攻
                    case 47: // 陸攻
                    case 57: // 噴式戦闘爆撃機
                    case 58: // 噴式攻撃機
                        return new[] {0, 0, 0, 0, 0, 0, 0, 0};
                    case 11: // 水爆
                        return new[] {0, 0, 1, 1, 1, 3, 3, 6};
                    default:
                        return null;
                }
            }
        }

        private double[] AlvBonus
        {
            get
            {
                var table = AlvTypeBonusTable;
                if (table == null)
                    return new[] {0.0, 0.0};
                return new[] {table[Alv] + _alvBonusMin[Alv], table[Alv] + _alvBonusMax[Alv]};
            }
        }

        private double[] AlvBonusInBase
        {
            get
            {
                switch (Spec.Type)
                {
                    case 9: // 艦偵
                    case 10: // 水偵
                    case 41: // 大艇
                        return new[] {_alvBonusMin[Alv], _alvBonusMax[Alv]};
                    default:
                        return AlvBonus;
                }
            }
        }

        private double FighterPowerLevelBonus
        {
            get
            {
                switch (Spec.Type)
                {
                    case 6: // 艦戦
                    case 45: // 水戦
                    case 48: // 陸戦・局戦
                        return 0.2 * Level;
                    case 7: // 改修可能なのは爆戦のみ
                        return 0.25 * Level;
                }
                return 0;
            }
        }

        public double LoSLevelBonus
        {
            get
            {
                switch (Spec.Type)
                {
                    case 10: // 水偵
                        return 1.2 * Math.Sqrt(Level);
                    case 11: // 水爆
                        return 1.15 * Math.Sqrt(Level);
                    case 12: // 小型電探
                        return 1.25 * Math.Sqrt(Level);
                    case 13: // 大型電探
                        return 1.4 * Math.Sqrt(Level);
                    case 94: // 艦上偵察機（II）
                        return 1.2 * Math.Sqrt(Level);
                    default:
                        return 0;
                }
            }
        }

        public double FirepowerLevelBonus
        {
            get
            {
                switch (Spec.Type)
                {
                    case 1: // 小口径
                    case 2: // 中口径
                    case 19: // 徹甲弾
                    case 21: // 対空機銃
                    case 24: // 上陸用舟艇
                    case 29: // 探照灯
                    case 36: // 高射装置
                    case 42: // 大型探照灯
                    case 46: // 特型内火艇
                        return Math.Sqrt(Level);
                    case 3: // 大口径
                        return 1.5 * Math.Sqrt(Level);
                    case 4: // 副砲
                        return SecondaryGunLevelBonus;
                    case 14: // ソナー
                    case 15: // 爆雷
                        return Spec.Id == 226 // 九五式爆雷
                            ? 0
                            : 0.75 * Math.Sqrt(Level);
                    default:
                        return 0;
                }
            }
        }

        public double SecondaryGunLevelBonus
        {
            get
            {
                switch (Spec.Id)
                {
                    case 10: // 12.7cm連装高角砲
                    case 66: // 8cm高角砲
                    case 220: // 8cm高角砲改+増設機銃
                    case 275: // 10cm連装高角砲改+増設機銃
                        return 0.2 * Level;
                    case 12: // 15.5cm三連装副砲
                    case 234: // 15.5cm三連装副砲改
                    case 247: // 15.2cm三連装砲
                        return 0.3 * Level;
                    default:
                        return Math.Sqrt(Level);
                }
            }
        }

        public double TorpedoLevelBonus
        {
            get
            {
                switch (Spec.Type)
                {
                    case 5: // 魚雷
                    case 21: // 機銃
                        return 1.2 * Math.Sqrt(Level);
                    default:
                        return 0;
                }
            }
        }

        public double AntiSubmarineLevelBonus
        {
            get
            {
                switch (Spec.Type)
                {
                    case 14:
                    case 15:
                        return Math.Sqrt(Level);
                    default:
                        return 0;
                }
            }
        }

        public double BomberLevelBonus => Spec.Type == 11 /* 水爆 */ ? 0.2 * Level : 0;

        public double NightBattleLevelBonus
        {
            get
            {
                switch (Spec.Type)
                {
                    case 1: // 小口径
                    case 2: // 中口径
                    case 3: // 大口径
                    case 5: // 魚雷
                    case 19: // 徹甲弾
                    case 24: // 上陸用舟艇
                    case 29: // 探照灯
                    case 36: // 高射装置
                    case 42: // 大型探照灯
                    case 46: // 特型内火艇
                        return Math.Sqrt(Level);
                    case 4: // 副砲
                        return SecondaryGunLevelBonus;
                    default:
                        return 0;
                }
            }
        }

        public double EffectiveAntiAirForShip
        {
            get
            {
                switch (Spec.IconType)
                {
                    case 15: // 機銃
                        return 6 * Spec.AntiAir + 4 * Math.Sqrt(Level);
                    case 16: // 高角砲
                        return 4 * Spec.AntiAir + (Spec.AntiAir >= 8 ? 3 : 2) * Math.Sqrt(Level);
                    case 11: // 電探
                        return 3 * Spec.AntiAir;
                    case 30: // 高射装置
                        return 4 * Spec.AntiAir + 2 * Math.Sqrt(Level);
                }
                return 0;
            }
        }

        public double EffectiveAntiAirForFleet
        {
            get
            {
                switch (Spec.IconType)
                {
                    case 1:
                    case 2:
                    case 3: // 主砲
                    case 4: // 副砲
                    case 6: // 艦戦
                    case 7: // 艦爆
                    case 15: // 機銃
                        return 0.2 * Spec.AntiAir;
                    case 11: // 電探
                        return 0.4 * Spec.AntiAir + 1.5 * Math.Sqrt(Level);
                    case 12: // 三式弾
                        return 0.6 * Spec.AntiAir;
                    case 16: // 高角砲
                        return 0.35 * Spec.AntiAir + (Spec.AntiAir >= 8 ? 3 : 2) * Math.Sqrt(Level);
                    case 30: // 高射装置
                        return 0.35 * Spec.AntiAir + 2 * Math.Sqrt(Level);
                    default:
                        if (Spec.Id == 9) // 46cm三連装砲
                            return 0.25 * Spec.AntiAir;
                        if (Spec.Type == 10) // 水偵
                            return 0.2 * Spec.AntiAir;
                        break;
                }
                return 0;
            }
        }
    }
}