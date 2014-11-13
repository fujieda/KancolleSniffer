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
using System.Web;

namespace KancolleSniffer
{
    public struct ItemSpec
    {
        public string Name;
        public int Type;
        public int AntiAir;
        public int LoS;

        public bool CanAirCombat()
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
    }

    public class ItemInfo
    {
        private int _nowShips, _nowEquips;
        private readonly Dictionary<int, ItemSpec> _itemSpecs = new Dictionary<int, ItemSpec>();
        private readonly Dictionary<int, int> _itemIds = new Dictionary<int, int>();

        public int MaxShips { get; private set; }
        public int MarginShips { get; set; }
        public bool RingShips { get; set; }
        public int MaxEquips { get; private set; }
        public int MarginEquips { get; set; }
        public bool RingEquips { get; set; }
        public MaterialCount[] MaterialHistory { get; private set; }

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
            set
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
            foreach (var entry in json)
            {
                _itemSpecs[(int)entry.api_id] = new ItemSpec
                {
                    Name = (string)entry.api_name,
                    Type = (int)entry.api_type[2],
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
                NowEquips = ((object[])json).Length;
            foreach (var entry in json)
                _itemIds[(int)entry.api_id] = (int)entry.api_slotitem_id;
            _itemIds[-1] = -1;
        }

        public void InspectCreateItem(dynamic json)
        {
            var m = (dynamic[])json.api_material;
            for (var i = 0; i < m.Length; i++)
                MaterialHistory[i].Now = (int)m[i];
            if (!json.IsDefined("api_slot_item"))
                return;
            InspectSlotItem(json.api_slot_item);
            NowEquips++;
        }

        public void InspectGetShip(dynamic json)
        {
            NowShips += 1;
            if (json.api_slotitem == null) // まるゆにはスロットがない
                return;
            InspectSlotItem(json.api_slotitem);
            NowEquips += ((object[])json.api_slotitem).Length;
        }

        public void InspectDestroyItem(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            NowEquips -= values["api_slotitem_ids"].Split(',').Length;
            var get = (int[])json.api_get_material;
            for (var i = 0; i < get.Length; i++)
                MaterialHistory[i].Now += get[i];
        }

        public void InspectRemodelSlot(dynamic json)
        {
            if (!json.api_use_slot_id())
                return;
            NowEquips -= ((object[])json.api_use_slot_id).Length;
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
            int itemId;
            if (!_itemIds.TryGetValue(id, out itemId))
                itemId = -1;
            return _itemSpecs[itemId];
        }

        public ItemSpec GetSpecByItemId(int id)
        {
            return _itemSpecs[id];
        }

        public void SaveState(Status status)
        {
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
        Development,
        Bucket,
        Burner,
        Screw,
    }

    public class MaterialCount
    {
        private int _now;

        public int BegOfDay { get; set; }
        public int BegOfWeek { get; set; }
        public DateTime LastSet { get; set; }

        public int Now
        {
            get { return _now; }
            set
            {
                if (!Status.Restoring) // JSONから値を復旧するときは履歴に触らない
                {
                    UpdateHistory();
                    LastSet = DateTime.Now;
                }
                _now = value;
            }
        }

        public void UpdateHistory()
        {
            var morning = DateTime.Today.AddHours(5);
            var dow = (int)morning.DayOfWeek;
            var monday = morning.AddDays(dow == 0 ? -6 : -dow + 1);
            if (DateTime.Now >= morning && LastSet < morning)
                BegOfDay = _now;
            if (DateTime.Now >= monday && LastSet < monday)
                BegOfWeek = _now;
        }
    }
}