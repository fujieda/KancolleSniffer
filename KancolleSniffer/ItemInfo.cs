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

namespace KancolleSniffer
{
    public class ItemInfo
    {
        private int _nowShips;
        private readonly Dictionary<int,int> _itemSpecs = new Dictionary<int, int>();

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

        public int MaxShips { get; set; }
        public int MarginShips { get; set; }
        public bool NeedRing { get; set; }
        public int NowItems { get; set; }
        public int MaxItems { get; set; }
        public int NumBuckets { get; set; }

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
        public void InspectRecord(dynamic json)
        {
            NowShips = (int)json.api_ship[0];
            MaxShips = (int)json.api_ship[1];
            NowItems = (int)json.api_slotitem[0];
            MaxItems = (int)json.api_slotitem[1];
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

        public void InspectSlotItem(dynamic json)
        {
            NowItems = ((object[])json).Length;
            foreach (var entry in json)
            {
                if ((int)entry.api_type[0] == 3) // 艦載機
                    _itemSpecs[(int)entry.api_id] = (int)entry.api_tyku;                
            }
        }

        public int GetTyKu(int id)
        {
            int tyku;
            return _itemSpecs.TryGetValue(id, out tyku) ? tyku : 0;
        }
    }
}