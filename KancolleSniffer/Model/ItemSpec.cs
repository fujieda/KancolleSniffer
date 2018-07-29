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
using System.Drawing;

namespace KancolleSniffer.Model
{
    public class ItemSpec
    {
        public static bool IncreaceLandPowerTp = false;
        public int Id;
        public string Name;
        public int Type;
        public string TypeName;
        public int Firepower;
        public int IconType;
        public int AntiAir;
        public int LoS;
        public int AntiSubmarine;
        public int Torpedo;
        public int Bomber;
        public int Interception;
        public int AntiBomber;
        public int Distance;

        public ItemSpec()
        {
            Id = -1;
            Name = "";
        }

        public bool CanAirCombat
        {
            get
            {
                switch (Type)
                {
                    case 6: // 艦戦
                    case 7: // 艦爆
                    case 8: // 艦攻
                    case 11: // 水爆
                    case 45: // 水戦
                    case 56: // 噴式戦闘機
                    case 57: // 噴式戦闘爆撃機
                    case 58: // 噴式攻撃機
                        return true;
                }
                return false;
            }
        }

        // http://ja.kancolle.wikia.com/wiki/%E3%83%9E%E3%83%83%E3%83%97%E7%B4%A2%E6%95%B5
        public double LoSScaleFactor
        {
            get
            {
                switch (Type)
                {
                    case 8: // 艦攻
                        return 0.8;
                    case 9: // 艦偵
                        return 1;
                    case 10: // 水偵
                        return 1.2;
                    case 11: // 水爆
                        return 1.1;
                    default:
                        return 0.6;
                }
            }
        }

        public bool IsAircraft
        {
            get
            {
                switch (Type)
                {
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 25: // オートジャイロ
                    case 26: // 対潜哨戒機
                    case 41: // 大艇
                    case 45:
                    case 47: // 陸上攻撃機
                    case 48: // 局地戦闘機
                    case 56:
                    case 57:
                    case 58:
                    case 59: // 噴式偵察機
                        return true;
                }
                return false;
            }
        }

        public bool IsDiveBomber => Type == 7 || Type == 11 || Type == 57;

        public bool IsTorpedoBomber => Type == 8 || Type == 58;

        public int EffectiveAntiSubmarine
        {
            get
            {
                switch (Type)
                {
                    case 1: // 小口径(12.7cm連装高角砲(後期型))
                    case 10: // 水偵
                    case 12: // 小型電探(22号対水上電探改四)
                    case 45: // 水戦
                        return 0;
                    default:
                        return AntiSubmarine;
                }
            }
        }

        public bool IsSonar => Type == 14 || // ソナー
                               Type == 40; // 大型ソナー

        public bool IsDepthCharge => Type == 15;

        public bool IsRepairFacility => Type == 31;

        public bool IsAntiAirGun => Type == 21;

        public double ContactTriggerRate
        {
            get
            {
                switch (Type)
                {
                    case 9: // 艦偵
                    case 10: // 水偵
                    case 41: // 大艇
                        return 0.04;
                    default:
                        return 0;
                }
            }
        }

        public Func<double> GetItemTp { get; set; }

        public double TransportPoint
        {
            get
            {
                var tp = GetItemTp?.Invoke();
                if (tp >= 0)
                    return (double)tp;
                switch (Id)
                {
                    case 75: // ドラム缶(輸送用)
                        return 5.0;
                    case 68: // 大発動艇
                        return 8.0;
                    case 193: // 特大発動艇
                        return 8.0;
                    case 166: // 大発動艇(八九式中戦車&陸戦隊)
                        return 8.0;
                    case 167: // 特二式内火艇
                        return 2.0;
                    case 230: // 特大発動艇＋戦車第11連隊
                        return 8.0;
                    case 145: // 戦闘糧食
                        return 1.0;
                    case 150: // 秋刀魚の缶詰
                        return 1.0;
                    case 241: // 戦闘糧食(特別なおにぎり)
                        return 1.0;
                    default:
                        return 0;
                }
            }
        }

        public double ReconPlaneInterceptionBonus
        {
            get
            {
                switch (Type)
                {
                    case 9:
                        return LoS <= 7 ? 1.2 : 1.3;
                    case 10:
                    case 41:
                        return LoS <= 7 ? 1.1 :
                            LoS <= 8 ? 1.13 : 1.16;
                }
                return 1;
            }
        }

        public Color Color
        {
            get
            {
                switch (IconType)
                {
                    case 1:
                    case 2:
                    case 3: // 主砲
                    case 13: // 徹甲弾
                        return Color.FromArgb(209, 89, 89);
                    case 4: // 副砲
                        return Color.FromArgb(253, 233, 0);
                    case 5: // 魚雷
                        return Color.FromArgb(88, 134, 170);
                    case 6: // 艦戦
                        return Color.FromArgb(93, 179, 108);
                    case 7: // 艦爆
                        return Color.FromArgb(223, 102, 102);
                    case 8: // 艦攻
                        return Color.FromArgb(95, 173, 234);
                    case 9: // 艦偵
                        return Color.FromArgb(254, 191, 0);
                    case 10: // 水上機
                    case 43: // 水上戦闘機
                        return Color.FromArgb(142, 203, 152);
                    case 11: // 電探
                        return Color.FromArgb(231, 153, 53);
                    case 12: // 三式弾
                        return Color.FromArgb(69, 175, 88);
                    case 14: // 応急修理要員
                        return Color.FromArgb(254, 254, 254);
                    case 15: // 機銃
                    case 16: // 高角砲
                        return Color.FromArgb(102, 204, 118);
                    case 17: // 爆雷
                    case 18: // ソナー
                        return Color.FromArgb(126, 203, 215);
                    case 19: // 缶
                        return Color.FromArgb(254, 195, 77);
                    case 20: // 大発
                    case 36: // 特型内火艇
                        return Color.FromArgb(154, 163, 90);
                    case 21: // オートジャイロ
                        return Color.FromArgb(99, 203, 115);
                    case 22: // 対潜哨戒機
                        return Color.FromArgb(125, 205, 217);
                    case 23: // 追加装甲
                        return Color.FromArgb(152, 124, 172);
                    case 24: // 探照灯
                    case 27: // 照明弾
                        return Color.FromArgb(254, 155, 0);
                    case 25: // ドラム缶
                        return Color.FromArgb(161, 161, 160);
                    case 26: // 艦艇修理施設
                        return Color.FromArgb(175, 156, 126);
                    case 28: // 司令部施設
                        return Color.FromArgb(204, 172, 252);
                    case 29: // 航空要員
                        return Color.FromArgb(206, 166, 108);
                    case 30: // 高射装置
                        return Color.FromArgb(137, 153, 77);
                    case 31: // 対地装備
                        return Color.FromArgb(253, 49, 49);
                    case 32: // 水上艦要員
                        return Color.FromArgb(188, 238, 155);
                    case 33: // 大型飛行艇
                        return Color.FromArgb(142, 203, 152);
                    case 34: // 戦闘糧食
                        return Color.FromArgb(254, 254, 254);
                    case 35: // 補給物資
                        return Color.FromArgb(90, 200, 155);
                    case 37: // 陸上攻撃機
                    case 38: // 局地戦闘機
                    case 44: // 陸軍戦闘機
                        return Color.FromArgb(57, 182, 78);
                    case 39: // 噴式景雲改
                    case 40: // 橘花改
                        return Color.FromArgb(72, 178, 141);
                    case 42: // 潜水艦機材
                        return Color.FromArgb(158, 187, 226);
                    case 45: // 夜間戦闘機
                    case 46: // 夜間攻撃機
                        return Color.FromArgb(128, 121, 161);
                    case 47: // 陸上対潜哨戒機
                        return Color.FromArgb(91, 113, 209);
                    default:
                        return SystemColors.Control;
                }
            }
        }
    }
}