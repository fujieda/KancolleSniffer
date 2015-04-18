// Copyright (C) 2013, 2014 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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

using System;
using System.Collections.Generic;
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
        public int AntiAir;
        public int LoS;

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
    }

    public class ItemStatus
    {
        public ItemSpec Spec { get; set; }
        public int Level { get; set; }
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

        public int MaxShips { get; private set; }
        public int MarginShips { get; set; }
        public bool RingShips { get; set; }
        public int MaxEquips { get; private set; }
        public int MarginEquips { get; set; }
        public bool RingEquips { get; set; }
        public MaterialCount[] MaterialHistory { get; private set; }

        public bool NeedSave
        {
            get { return MaterialHistory.Any(m => m.NeedSave); }
            private set
            {
                foreach (var m in MaterialHistory)
                    m.NeedSave = value;
            }
        }

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

        public bool TooManyShips
        {
            get { return MaxShips != 0 && NowShips >= MaxShips - MarginShips; }
        }

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

        public bool TooManyEquips
        {
            get { return MaxEquips != 0 && NowEquips >= MaxEquips - MarginEquips; }
        }

        public ItemInfo()
        {
            MaterialHistory = new MaterialCount[Enum.GetValues(typeof (Material)).Length];
            foreach (Material m in Enum.GetValues(typeof (Material)))
                MaterialHistory[(int)m] = new MaterialCount();
            MarginShips = 4;
            MarginEquips = 10;
        }

        public void InspectBasic(dynamic json)
        {
            MaxShips = (int)json.api_max_chara;
            MaxEquips = (int)json.api_max_slotitem;
        }

        public void InspectMaterial(dynamic json)
        {
            foreach (var entry in json)
                MaterialHistory[(int)entry.api_id - 1].Now = (int)entry.api_value;
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
                    AntiAir = (int)entry.api_tyku,
                    LoS = (int)entry.api_saku
                };
            }
            _itemSpecs[-1] = new ItemSpec();
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
                    Level = entry.api_level() ? (int)entry.api_level : 0
                };
            }
            NowEquips = _itemInfo.Count - 1;
        }

        public void InspectCreateItem(dynamic json)
        {
            var m = (dynamic[])json.api_material;
            for (var i = 0; i < m.Length; i++)
                MaterialHistory[i].Now = (int)m[i];
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
            var get = (int[])json.api_get_material;
            for (var i = 0; i < get.Length; i++)
                MaterialHistory[i].Now += get[i];
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

        public void InspectMissionResult(dynamic json)
        {
            if ((int)json.api_clear_result == 0) // 失敗
                return;
            var get = (int[])json.api_get_material;
            for (var i = 0; i < get.Length; i++)
                MaterialHistory[i].Now += get[i];
        }

        public ItemSpec this[int id]
        {
            get { return GetSpecById(id); }
        }

        public ItemSpec GetSpecById(int id)
        {
            return _itemInfo[id].Spec;
        }

        public ItemSpec GetSpecByItemId(int id)
        {
            return _itemSpecs[id];
        }

        public Dictionary<int, ItemStatus> ItemDict
        {
            get { return _itemInfo; }
        }

        public void SetItemOwner(ShipStatus[] shipList)
        {
            foreach (var e in _itemInfo)
                e.Value.Ship = new ShipStatus();
            foreach (var s in shipList)
            {
                foreach (var id in s.Slot)
                    _itemInfo[id].Ship = s;
            }
        }

        public void SaveState(Status status)
        {
            NeedSave = false;
            status.MatreialHistory = MaterialHistory;
        }

        public void LoadSate(Status status)
        {
            if (status.MatreialHistory != null)
                status.MatreialHistory.CopyTo(MaterialHistory, 0);
        }
    }

    public enum Material
    {
        Fuel,
        Bullet,
        Steal,
        Bouxite,
        Burner,
        Bucket,
        Development,
        Screw,
    }

    public class MaterialCount
    {
        private int _now;

        public int BegOfDay { get; set; }
        public int BegOfWeek { get; set; }
        public DateTime LastSet { get; set; }
        public bool NeedSave { get; set; }

        public int Now
        {
            get { return _now; }
            set
            {
                var prev = _now;
                _now = value;
                if (Status.Restoring) // JSONから値を復旧するときは履歴に触らない
                    return;
                if (_now != prev)
                    NeedSave = true;
                if (LastSet == DateTime.MinValue)
                {
                    BegOfDay = BegOfWeek = value;
                    LastSet = DateTime.Now;
                    return;
                }
                UpdateHistory(prev);
                LastSet = DateTime.Now;
            }
        }

        private void UpdateHistory(int prev)
        {
            var morning = DateTime.Today.AddHours(5);
            var dow = (int)morning.DayOfWeek;
            var monday = morning.AddDays(dow == 0 ? -6 : -dow + 1);
            if (DateTime.Now >= morning && LastSet < morning)
                BegOfDay = prev;
            if (DateTime.Now >= monday && LastSet < monday)
                BegOfWeek = prev;
        }
    }
}