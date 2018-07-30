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
using System.Collections.Generic;
using System.Linq;

namespace KancolleSniffer.Model
{
    public class ShipInventry : Inventry<ShipStatus>
    {
        public ShipInventry() : base(new ShipStatus())
        {
        }

        protected override ShipStatus CreateDummy(int id) => new ShipStatus();

        protected override int GetId(ShipStatus ship) => ship.Id;

        public IEnumerable<ShipStatus> AllShips => AllItems;
    }

    public class ItemInventry : Inventry<ItemStatus>
    {
        public ItemInventry() : base(new ItemStatus())
        {
        }

        protected override ItemStatus CreateDummy(int id) => new ItemStatus(id);

        protected override int GetId(ItemStatus item) => item.Id;
    }

    public abstract class Inventry<T>
    {
        private readonly Dictionary<int, T> _dict = new Dictionary<int, T>();
        private int _inflated;

        protected abstract T CreateDummy(int id);

        protected Inventry(T dummy)
        {
            _dict[-1] = dummy;
        }

        public void Clear()
        {
            _dict.Clear();
            _dict[-1] = CreateDummy(-1);
            _inflated = 0;
        }

        public virtual T this[int id]
        {
            get => _dict.TryGetValue(id, out var item) ? item : CreateDummy(id);
            set => _dict[id] = value;
        }

        protected abstract int GetId(T item);

        public void Add(T item)
        {
            _dict[GetId(item)] = item;
        }

        public void Add(IEnumerable<T> items)
        {
            foreach (var item in items)
                Add(item);
        }

        public void Remove(int id)
        {
            if (id != -1)
                _dict.Remove(id);
        }

        public void Remove(IEnumerable<int> ids)
        {
            foreach (var id in ids)
                Remove(id);
        }

        public void Remove(T item)
        {
            Remove(GetId(item));
        }

        public void Remove(IEnumerable<T> items)
        {
            foreach (var item in items)
                Remove(item);
        }

        public bool Contains(int id) => _dict.ContainsKey(id);

        public bool Contains(T item) => Contains(GetId(item));

        public IEnumerable<T> AllItems =>
            from kv in _dict where kv.Key != -1 select kv.Value;

        public void InflateCount(int count) => _inflated += count;

        public int Count => _dict.Count + _inflated - 1;

        public int MaxId => Math.Max(_dict.Keys.Max(), 0);
    }
}