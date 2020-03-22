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

using System.Collections.Generic;

namespace KancolleSniffer.Model
{
    public class ItemMaster
    {
        private readonly Dictionary<int, ItemSpec> _itemSpecs = new Dictionary<int, ItemSpec>();
        private readonly Dictionary<int, string> _useItemName = new Dictionary<int, string>();

        public const int EmergencyRepairId = 91;
        public const int EmergencyRepairSpecId = 10091;

        public AdditionalData AdditionalData { get; set; }

        public void InspectMaster(dynamic json)
        {
            var dict = new Dictionary<int, string>();
            foreach (var entry in json.api_mst_slotitem_equiptype)
                dict[(int)entry.api_id] = entry.api_name;
            AdditionalData.LoadTpSpec();
            foreach (var entry in json.api_mst_slotitem)
            {
                var type = (int)entry.api_type[2];
                var id = (int)entry.api_id;
                _itemSpecs[(int)entry.api_id] = new ItemSpec
                {
                    Id = id,
                    Name = (string)entry.api_name,
                    Type = type,
                    TypeName = dict.TryGetValue(type, out var typeName) ? typeName : "不明",
                    IconType = (int)entry.api_type[3],
                    Firepower = (int)entry.api_houg,
                    AntiAir = (int)entry.api_tyku,
                    LoS = (int)entry.api_saku,
                    AntiSubmarine = (int)entry.api_tais,
                    Torpedo = (int)entry.api_raig,
                    Bomber = (int)entry.api_baku,
                    Interception = type == 48 ? (int)entry.api_houk : 0, // 局地戦闘機は回避の値が迎撃
                    AntiBomber = type == 48 ? (int)entry.api_houm : 0, // 〃命中の値が対爆
                    Distance = entry.api_distance() ? (int)entry.api_distance : 0,
                    GetItemTp = () => AdditionalData.ItemTp(id)
                };
            }
            _itemSpecs[-1] = _itemSpecs[0] = new ItemSpec();
            foreach (var entry in json.api_mst_useitem)
            {
                var id = (int)entry.api_id;
                _useItemName[id] = entry.api_name;
            }
            if (_useItemName.ContainsKey(EmergencyRepairId))
            {
                _itemSpecs[EmergencyRepairSpecId] = new ItemSpec
                {
                    Type = 31,
                    Id = EmergencyRepairSpecId,
                    Name = _useItemName[EmergencyRepairId]
                };
            }
        }

        public ItemSpec this[int id]
        {
            get => _itemSpecs.TryGetValue(id, out var spec) ? spec : new ItemSpec();
            set => _itemSpecs[id] = value;
        }

        public string GetUseItemName(int id) => _useItemName.TryGetValue(id, out var name) ? name : "";
    }
}