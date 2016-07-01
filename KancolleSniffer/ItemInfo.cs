// Copyright (C) 2013, 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Drawing;
using System.Linq;
using static System.Math;

namespace KancolleSniffer
{
    public class ItemSpec
    {
        public int Id;
        public string Name;
        public int Type;
        public string TypeName;
        public int IconType;
        public int AntiAir;
        public int LoS;
        public int AntiSubmarine;
        public int Torpedo;
        public int Bomber;

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
                        return true;
                }
                return false;
            }
        }

        public int RealAntiSubmarine
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

        public bool IsReconSeaplane => Type == 10;

        public bool IsSonar => Type == 14 || // ソナー
                               Type == 40; // 大型ソナー

        public bool IsDepthCharge => Type == 15;

        public bool IsRepairFacility => Type == 31;

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

        public double TransportPoint
        {
            get
            {
                switch (Id)
                {
                    case 75: // ドラム缶(輸送用)
                        return 5.0;
                    case 68: // 大発動艇
                        return 8.0;
                    case 166: // 大発動艇(八九式中戦車&陸戦隊)
                        return 8.0;
                    case 167: // 特二式内火艇
                        return 2.0;
                    case 145: // 戦闘糧食
                        return 1.0;
                    case 150: // 秋刀魚の缶詰
                        return 1.0;
                    default:
                        return 0;
                }
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
                        return Color.FromArgb(33, 194, 89);
                    default:
                        return SystemColors.Control;
                }
            }
        }
    }

    public class ItemStatus
    {
        public int Id { get; set; }
        public ItemSpec Spec { get; set; } = new ItemSpec();
        public int Level { get; set; }
        public int Alv { get; set; }
        public ShipStatus Ship { get; set; }

        public ItemStatus()
        {
            Id = -1;
        }

        public ItemStatus(int id)
        {
            Id = id == 0 ? -1 : id;
        }

        private readonly double[] _alvBonusMin =
        {
            Sqrt(0.0), Sqrt(1.0), Sqrt(2.5), Sqrt(4.0), Sqrt(5.5), Sqrt(7.0),
            Sqrt(8.5), Sqrt(10.0)
        };

        private readonly double[] _alvBonusMax =
        {
            Sqrt(0.9), Sqrt(2.4), Sqrt(3.9), Sqrt(5.4), Sqrt(6.9), Sqrt(8.4),
            Sqrt(9.9), Sqrt(12.0)
        };

        private readonly Dictionary<int, int[]> _alvTypeBonus = new Dictionary<int, int[]>
        {
            {06, new[] {0, 0, 2, 5, 9, 14, 14, 22}}, // 艦戦
            {07, new[] {0, 0, 0, 0, 0, 0, 0, 0}}, // 艦爆
            {08, new[] {0, 0, 0, 0, 0, 0, 0, 0}}, // 艦攻
            {11, new[] {0, 0, 1, 1, 1, 3, 3, 6}}, // 水爆
            {45, new[] {0, 0, 2, 5, 9, 14, 14, 22}} // 水戦
        };

        public double[] AlvBonus
        {
            get
            {
                int[] table;
                if (!_alvTypeBonus.TryGetValue(Spec.Type, out table))
                    return new[] {0.0, 0.0};
                return new[] {table[Alv] + _alvBonusMin[Alv], table[Alv] + _alvBonusMax[Alv]};
            }
        }

        public double LoSLevelBonus
        {
            get
            {
                switch (Spec.Type)
                {
                    case 10: // 水偵
                        return 1.2 * Sqrt(Level);
                    case 12: // 小型電探
                    case 13: // 大型電探
                        return 1.25 * Sqrt(Level);
                    default:
                        return 0;
                }
            }
        }

        public double FirePowerLevelBonus
        {
            get
            {
                switch (Spec.Type)
                {
                    case 1: // 小口径
                    case 2: // 中口径
                        return Sqrt(Level);
                    case 3: // 大口径
                        return 1.5 * Sqrt(Level);
                    case 4: // 副砲
                        return Sqrt(Level);
                    case 14: // ソナー
                    case 15: // 爆雷
                        return 0.75 * Sqrt(Level);
                    case 19: // 徹甲弾
                        return Sqrt(Level);
                    default:
                        return 0;
                }
            }
        }

        public double TorpedoLevelBonus
        {
            get
            {
                if (Spec.Type == 5) // 魚雷
                    return 1.2 * Sqrt(Level);
                if (Spec.Type == 21) // 機銃
                    return 1.2 * Sqrt(Level);
                return 0;
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
                        return Sqrt(Level);
                    default:
                        return 0;
                }
            }
        }

        public double NightBattleLevelBonus
        {
            get
            {
                switch (Spec.Type)
                {
                    case 1: // 小口径
                    case 2: // 中口径
                    case 3: // 大口径
                        return Sqrt(Level);
                    case 4: // 副砲
                        return Sqrt(Level);
                    case 5: // 魚雷
                    case 19: // 徹甲弾
                    case 29: // 探照灯
                    case 36: // 高射装置
                    case 42: // 大型探照灯
                        return Sqrt(Level);
                    default:
                        return 0;
                }
            }
        }
    }

    public class ItemInfo
    {
        private int _nowShips, _nowEquips;
        private readonly Dictionary<int, ItemSpec> _itemSpecs = new Dictionary<int, ItemSpec>();
        private readonly Dictionary<int, ItemStatus> _itemInfo = new Dictionary<int, ItemStatus>();
        private readonly Dictionary<int, string> _useItemName = new Dictionary<int, string>();

        public int MaxShips { get; private set; }
        public int MarginShips { get; set; }
        public bool RingShips { get; set; }
        public int MaxEquips { get; private set; }
        public int MarginEquips { get; set; }
        public bool RingEquips { get; set; }

        public int NowShips
        {
            get { return _nowShips; }
            set
            {
                if (MaxShips != 0)
                {
                    var limit = MaxShips - MarginShips;
                    RingShips = _nowShips < limit && value >= limit;
                }
                _nowShips = value;
            }
        }

        public bool TooManyShips => MaxShips != 0 && NowShips >= MaxShips - MarginShips;

        public int NowEquips
        {
            get { return _nowEquips; }
            private set
            {
                if (MaxEquips != 0)
                {
                    var limit = MaxEquips - MarginEquips;
                    RingEquips = _nowEquips < limit && value >= limit;
                }
                _nowEquips = value;
            }
        }

        public bool TooManyEquips => MaxEquips != 0 && NowEquips >= MaxEquips - MarginEquips;

        public ItemInfo()
        {
            MarginShips = 4;
            MarginEquips = 10;
        }

        public void InspectBasic(dynamic json)
        {
            MaxShips = (int)json.api_max_chara;
            MaxEquips = (int)json.api_max_slotitem;
        }

        public void InspectMaster(dynamic json)
        {
            var dict = new Dictionary<int, string>();
            foreach (var entry in json.api_mst_slotitem_equiptype)
                dict[(int)entry.api_id] = entry.api_name;
            foreach (var entry in json.api_mst_slotitem)
            {
                _itemSpecs[(int)entry.api_id] = new ItemSpec
                {
                    Id = (int)entry.api_id,
                    Name = (string)entry.api_name,
                    Type = (int)entry.api_type[2],
                    TypeName = dict[(int)entry.api_type[2]],
                    IconType = (int)entry.api_type[3],
                    AntiAir = (int)entry.api_tyku,
                    LoS = (int)entry.api_saku,
                    AntiSubmarine = (int)entry.api_tais,
                    Torpedo = (int)entry.api_raig,
                    Bomber = (int)entry.api_baku
                };
            }
            _itemSpecs[-1] = new ItemSpec();
            foreach (var entry in json.api_mst_useitem)
                _useItemName[(int)entry.api_id] = entry.api_name;
        }

        public void InspectSlotItem(dynamic json, bool full = false)
        {
            if (!json.IsArray)
                json = new[] {json};
            if (full)
            {
                _itemInfo.Clear();
                _itemInfo[-1] = new ItemStatus();
            }
            foreach (var entry in json)
            {
                var id = (int)entry.api_id;
                _itemInfo[id] = new ItemStatus(id)
                {
                    Spec = _itemSpecs[(int)entry.api_slotitem_id],
                    Level = entry.api_level() ? (int)entry.api_level : 0,
                    Alv = entry.api_alv() ? (int)entry.api_alv : 0
                };
            }
            NowEquips = _itemInfo.Count - 1;
        }

        public void InspectCreateItem(dynamic json)
        {
            if (!json.IsDefined("api_slot_item"))
                return;
            InspectSlotItem(json.api_slot_item);
        }

        public void InspectGetShip(dynamic json)
        {
            NowShips += 1;
            if (json.api_slotitem == null) // まるゆにはスロットがない
                return;
            InspectSlotItem(json.api_slotitem);
        }

        public void InspectDestroyItem(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            DeleteItems(values["api_slotitem_ids"].Split(',').Select(int.Parse).ToArray());
        }

        public void InspectRemodelSlot(dynamic json)
        {
            if (json.api_after_slot())
                InspectSlotItem(json.api_after_slot);
            if (!json.api_use_slot_id())
                return;
            DeleteItems(((int[])json.api_use_slot_id));
        }

        public void DeleteItems(ItemStatus[] items)
        {
            DeleteItems(items.Select(item => item.Id));
        }

        private void DeleteItems(IEnumerable<int> ids)
        {
            foreach (var id in ids.Where(id => id != -1))
            {
                _itemInfo.Remove(id);
                NowEquips--;
            }
        }

        public void CountNewItems(int[] ids)
        {
            foreach (var id in ids.Where(id => id != -1 && !_itemInfo.ContainsKey(id)))
            {
                _itemInfo[id] = new ItemStatus(id);
                NowEquips++;
            }
        }

        public string GetName(int id) => _itemInfo[id].Spec.Name;

        public int GetItemId(int id) => _itemInfo[id].Spec.Id;

        public ItemSpec GetSpecByItemId(int id) => _itemSpecs[id];

        public ItemStatus GetStatus(int id) => _itemInfo[id];

        public ItemStatus[] GetItemListWithOwner(ShipStatus[] shipList)
        {
            foreach (var e in _itemInfo)
                e.Value.Ship = new ShipStatus();
            foreach (var s in shipList)
            {
                foreach (var id in s.Slot)
                    _itemInfo[id.Id].Ship = s;
                _itemInfo[s.SlotEx.Id].Ship = s;
            }
            return (from e in _itemInfo where e.Key != -1 select e.Value).ToArray();
        }

        public string GetUseItemName(int id) => _useItemName[id];
    }
}