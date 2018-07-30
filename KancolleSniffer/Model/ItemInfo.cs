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
using System.Linq;
using KancolleSniffer.Util;

namespace KancolleSniffer.Model
{
    public class ItemInfo
    {
        private int _nowShips, _nowEquips;
        private readonly ItemMaster _itemMaster;
        private readonly ItemInventry _itemInventry;
        public int MaxShips { get; private set; }
        public int MarginShips { get; set; }
        public bool AlarmShips { get; set; }
        public int MaxEquips { get; private set; }
        public int MarginEquips { get; set; }
        public bool AlarmEquips { get; set; }

        public int NowShips
        {
            get => _nowShips;
            set
            {
                if (MaxShips != 0)
                {
                    var limit = MaxShips - MarginShips;
                    AlarmShips = AlarmShips || _nowShips < limit && value >= limit;
                }
                _nowShips = value;
            }
        }

        public bool TooManyShips => MaxShips != 0 && NowShips >= MaxShips - MarginShips;

        public int NowEquips
        {
            get => _nowEquips;
            set
            {
                if (MaxEquips != 0)
                {
                    var limit = MaxEquips - MarginEquips;
                    AlarmEquips = AlarmEquips || _nowEquips < limit && value >= limit;
                }
                _nowEquips = value;
            }
        }

        public bool TooManyEquips => MaxEquips != 0 && NowEquips >= MaxEquips - MarginEquips;

        public ItemInfo(ItemMaster itemMaster, ItemInventry itemInventry)
        {
            _itemMaster = itemMaster;
            _itemInventry = itemInventry;
            MarginShips = 4;
            MarginEquips = 10;
        }

        public void InspectBasic(dynamic json)
        {
            MaxShips = (int)json.api_max_chara;
            var check = MaxEquips == 0;
            MaxEquips = (int)json.api_max_slotitem;
            if (check)
                AlarmEquips = NowEquips >= MaxEquips - MarginEquips;
        }

        public void InspectMaster(dynamic json)
        {
            _itemMaster.InspectMaster(json);
        }

        public void InspectSlotItem(dynamic json, bool full = false)
        {
            if (!json.IsArray)
                json = new[] {json};
            if (full)
                _itemInventry.Clear();
            foreach (var entry in json)
            {
                var id = (int)entry.api_id;
                _itemInventry[id] = new ItemStatus(id)
                {
                    Spec = _itemMaster[(int)entry.api_slotitem_id],
                    Level = entry.api_level() ? (int)entry.api_level : 0,
                    Alv = entry.api_alv() ? (int)entry.api_alv : 0
                };
            }
            NowEquips = _itemInventry.Count;
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
            DeleteItems((int[])json.api_use_slot_id);
        }

        public void DeleteItems(IEnumerable<ItemStatus> items)
        {
            DeleteItems(items.Select(item => item.Id));
        }

        private void DeleteItems(IEnumerable<int> ids)
        {
            _itemInventry.Remove(ids);
            NowEquips = _itemInventry.Count;
        }

        public ItemSpec GetSpecByItemId(int id) => _itemMaster[id];

        public string GetName(int id) => GetStatus(id).Spec.Name;

        public ItemStatus GetStatus(int id)
        {
            return _itemInventry[id];
        }

        public void ClearHolder()
        {
            foreach (var item in _itemInventry.AllItems)
                item.Holder = new ShipStatus();
        }

        public ItemStatus[] ItemList => _itemInventry.AllItems.ToArray();

        public string GetUseItemName(int id) => _itemMaster.GetUseItemName(id);

        public void InjectItemSpec(IEnumerable<ItemSpec> specs)
        {
            foreach (var spec in specs)
                _itemMaster[spec.Id] = spec;
        }

        public ItemStatus[] InjectItems(IEnumerable<int> itemIds)
        {
            var id = _itemInventry.MaxId + 1;
            return itemIds.Select(itemId =>
            {
                var spec = _itemMaster[itemId];
                if (spec.Empty)
                {
                    spec = new ItemSpec {Id = itemId};
                    _itemMaster[itemId] = spec;
                }
                var item = new ItemStatus {Id = id++, Spec = spec};
                _itemInventry.Add(item);
                return item;
            }).ToArray();
        }
    }
}