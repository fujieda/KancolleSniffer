// Copyright (C) 2013 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
using System.Web;

namespace KancolleSniffer
{
    public class ItemInfo
    {
        private int _nowShips;
        private readonly Dictionary<int,int> _itemSpecs = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _itemIds = new Dictionary<int, int>();

        public int MaxShips { get; private set; }
        public int MarginShips { get; set; }
        public bool NeedRing { get; set; }
        public int NowItems { get; set; }
        public int MaxItems { get; private set; }
        public int NumBuckets { get; set; }

        public int NowShips
        {
            get { return _nowShips; }
            set
            {
                if (MaxShips != 0)
                {
                    var limit = MaxShips - MarginShips;
                    NeedRing = _nowShips < limit && value >= limit;
                }
                _nowShips = value;
            }
        }

        public bool TooManyShips
        {
            get { return MaxShips != 0 && NowShips >= MaxShips - MarginShips; }
        }

        public ItemInfo()
        {
            MarginShips = 4;
        }

        public void InspectBasic(dynamic json)
        {
            MaxShips = (int)json.api_max_chara;
            MaxItems = (int)json.api_max_slotitem;
        }

        public void InspectMaterial(dynamic json)
        {
            foreach (var entry in json)
            {
                if ((int)entry.api_id != 6)
                    continue;
                NumBuckets = (int)entry.api_value;
            }
        }

        public void InspectMaster(dynamic json)
        {
            foreach (var entry in json)
            {
                if ((int)entry.api_type[0] == 3) // 艦載機
                    _itemSpecs[(int)entry.api_id] = (int)entry.api_tyku;                
            }
        }

        public void InspectSlotItem(dynamic json, bool full = false)
        {
            if (!json.IsArray)
                json = new[] {json};
            if (full)
                NowItems = ((object[])json).Length;
            foreach (var entry in json)
                _itemIds[(int)entry.api_id] = (int)entry.api_slotitem_id;
        }

        public void InspectCreateItem(dynamic json)
        {
            if (!json.IsDefined("api_slot_item"))
                return;
            InspectSlotItem(json.api_slot_item);
            NowItems++;
        }

        public void InspectGetShip(dynamic json)
        {
            InspectSlotItem(json.api_slotitem);
            NowItems += ((object[])json.api_slotitem).Length;
            NowShips += 1;
        }

        public void InspectDestroyItem(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            NowItems -= values["api_slotitem_ids"].Split(',').Length;
        }

        public int GetTyKu(int id)
        {
            int item;
            int tyku;
            return _itemIds.TryGetValue(id, out item) ? _itemSpecs.TryGetValue(item, out tyku) ? tyku : 0 : 0;
        }

    }
}