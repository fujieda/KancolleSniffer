// Copyright (C) 2013, 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Drawing;
using System.Linq;
using System.Web;

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
                        return true;
                }
                return false;
            }
        }

        // http://ch.nicovideo.jp/biikame/blomaga/ar663428
        public double LoSScaleFactor()
        {
            switch (Type)
            {
                case 7: // 艦爆
                    return 1.0376255;
                case 8: // 艦攻
                    return 1.3677954;
                case 9: // 艦偵
                    return 1.6592780;
                case 10: // 水偵
                    return 2.0000000;
                case 11: // 水爆
                    return 1.7787282;
                case 12: // 小型電探
                    return 1.0045358;
                case 13: // 大型電探
                    return 0.9906638;
            }
            if (Name == "探照灯")
                return 0.9067950;
            return 0;
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
                        return true;
                }
                return false;
            }
        }

        public bool IsSonar => Type == 14 || // ソナー
                               Type == 40; // 大型ソナー

        public bool IsDepthCharge => Type == 15;

        public bool IsReconSeaplane => Type == 10;

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
                    default:
                        return SystemColors.Control;
                }
            }
        }
    }

    public class ItemStatus
    {
        public ItemSpec Spec { get; set; }
        public int Level { get; set; }
        public int Alv { get; set; }
        public ShipStatus Ship { get; set; }

        public ItemStatus()
        {
            Spec = new ItemSpec();
            Ship = new ShipStatus();
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
                _itemInfo[(int)entry.api_id] = new ItemStatus
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

        public void DeleteItems(int[] ids)
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
                _itemInfo[id] = new ItemStatus();
                NowEquips++;
            }
        }

        public ItemSpec this[int id] => GetSpecById(id);

        public ItemSpec GetSpecById(int id) => _itemInfo[id].Spec;

        public ItemSpec GetSpecByItemId(int id) => _itemSpecs[id];

        public Dictionary<int, ItemStatus> ItemDict => _itemInfo;

        public void SetItemOwner(ShipStatus[] shipList)
        {
            foreach (var e in _itemInfo)
                e.Value.Ship = new ShipStatus();
            foreach (var s in shipList)
            {
                foreach (var id in s.Slot)
                    _itemInfo[id].Ship = s;
                if (s.SlotEx != 0)
                    _itemInfo[s.SlotEx].Ship = s;
            }
        }

        public string GetUseItemName(int id) => _useItemName[id];
    }
}