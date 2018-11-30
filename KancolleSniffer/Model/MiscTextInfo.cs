// Copyright (C) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

namespace KancolleSniffer.Model
{
    public class MiscTextInfo
    {
        private readonly ShipInfo _shipInfo;
        private readonly ItemInfo _itemInfo;
        private bool _inSortie;
        private readonly Dictionary<int, int> _gaugeCount = new Dictionary<int, int>();
        private readonly Dictionary<int, string> _furniture = new Dictionary<int, string>();

        private const string GuideText =
            "[海域ゲージ情報]\r\n 海域選択画面に進むと表示します。\r\n[演習情報]\r\n 演習相手を選ぶと表示します。\r\n[獲得アイテム]\r\n 帰投したときに表示します。";

        public string Text { get; private set; } = GuideText;

        public MiscTextInfo(ShipInfo shipInfo, ItemInfo itemInfo)
        {
            _shipInfo = shipInfo;
            _itemInfo = itemInfo;
        }

        public void Port()
        {
            if (_inSortie)
            {
                _inSortie = false;
                var text = GenerateItemGetText();
                Text = text == "" ? GuideText : "[獲得アイテム]\r\n" + text;
            }
            _items.Clear();
        }

        public void InspectMaster(dynamic json)
        {
            if (json.api_mst_mapinfo())
            {
                foreach (var entry in json.api_mst_mapinfo)
                {
                    if (entry.api_required_defeat_count != null)
                        _gaugeCount[(int)entry.api_id] = (int)entry.api_required_defeat_count;
                }
            }
            if (json.api_mst_furniture())
            {
                foreach (var entry in json.api_mst_furniture)
                    _furniture[(int)entry.api_id] = (string)entry.api_title;
            }
        }

        public void InspectMapInfo(dynamic json)
        {
            Text = "[海域ゲージ]\r\n";
            foreach (var entry in json.api_map_info() ? json.api_map_info : json)
            {
                var map = (int)entry.api_id;
                if (entry.api_eventmap())
                {
                    var eventMap = entry.api_eventmap;
                    Text +=
                        $"{map / 10}-{map % 10} : HP {(int)eventMap.api_now_maphp}/{(int)eventMap.api_max_maphp}\r\n";
                    continue;
                }
                if (!entry.api_defeat_count())
                    continue;
                var max = entry.api_required_defeat_count()
                    ? (int)entry.api_required_defeat_count
                    : _gaugeCount[map];
                var count = $"{max - (int)entry.api_defeat_count}/{max}";
                Text += $"{map / 10}-{map % 10} : 残り {count}\r\n";
            }
        }

        public void InspectPracticeEnemyInfo(dynamic json)
        {
            Text = $"[演習情報]\r\n敵艦隊名 : {json.api_deckname}\r\n";
            var ships = json.api_deck.api_ships;
            var s1 = (int)ships[0].api_id != -1 ? (int)ships[0].api_level : 1;
            var s2 = (int)ships[1].api_id != -1 ? (int)ships[1].api_level : 1;
            var exp = PracticeExp.GetExp(s1, s2);
            var bonus = PracticeExp.TrainingCruiserBonus(_shipInfo.Fleets[0].Ships);
            Text += $"獲得経験値 : {(int)(exp * bonus)}\r\nS勝利 : {(int)((int)(exp * 1.2) * bonus)}";
        }

        public void InspectMapNext(dynamic json)
        {
            if (json.api_airsearch() && (int)json.api_airsearch.api_result != 0)
            {
                var item = json.api_itemget;
                AddItemCount((int)item.api_usemst + 100, (int)item.api_id, (int)item.api_getcount);
                return;
            }
            if (json.api_itemget())
            {
                foreach (var item in json.api_itemget)
                    AddItemCount((int)item.api_usemst, (int)item.api_id, (int)item.api_getcount);
            }
            if (json.api_itemget_eo_result())
            {
                var eo = json.api_itemget_eo_result;
                AddItemCount((int)eo.api_usemst, (int)eo.api_id, (int)eo.api_getcount);
            }
            if (json.api_itemget_eo_comment())
            {
                var eo = json.api_itemget_eo_comment;
                AddItemCount((int)eo.api_usemst, (int)eo.api_id, (int)eo.api_getcount);
            }
            if (json.api_eventmap() && json.api_eventmap.api_itemget())
            {
                foreach (var item in json.api_eventmap.api_itemget)
                {
                    var type = (int)item.api_type;
                    type = type == 1 ? 5 :
                        type == 5 ? 6 : type;
                    AddItemCount(type, (int)item.api_id, (int)item.api_value);
                }
            }
        }

        public void InspectMapStart(dynamic json)
        {
            _inSortie = true;
            InspectMapNext(json);
        }

        public void InspectBattleResult(dynamic json)
        {
            if (json.api_get_eventitem())
            {
                foreach (var item in json.api_get_eventitem)
                {
                    var type = (int)item.api_type;
                    type = type == 1 ? 5 :
                        type == 5 ? 6 : type;
                    var id = (int)item.api_id;
                    AddItemCount(type, id, (int)item.api_value);
                }
            }
            if (json.api_mapcell_incentive() && (int)json.api_mapcell_incentive == 1)
            {
                foreach (var type in _items.Keys.Where(type => type > 100).ToArray())
                {
                    foreach (var id in _items[type])
                        AddItemCount(type - 100, id.Key, id.Value);
                }
            }
        }

        private readonly Dictionary<int, SortedDictionary<int, int>> _items =
            new Dictionary<int, SortedDictionary<int, int>>();

        private void AddItemCount(int type, int id, int count)
        {
            if (!_items.ContainsKey(type))
                _items[type] = new SortedDictionary<int, int>();
            var dict = _items[type];
            if (!dict.ContainsKey(id))
                dict[id] = 0;
            dict[id] += count;
        }

        private string GetName(int type, int id)
        {
            switch (type)
            {
                case 2:
                    return _shipInfo.GetSpec(id).Name;
                case 3:
                    return _itemInfo.GetSpecByItemId(id).Name;
                case 4:
                    return new[] {"燃料", "弾薬", "鋼材", "ボーキサイト", "高速建造材", "高速修復材", "開発資材", "改修資材"}[id - 1];
                case 5:
                    return _itemInfo.GetUseItemName(id);
                case 6:
                    return _furniture[id];
                default:
                    return "";
            }
        }

        private string GenerateItemGetText()
        {
            return string.Join("\r\n",
                new[] {4, 5, 3, 6, 2}.Where(_items.ContainsKey).SelectMany(type =>
                    _items[type].Select(pair => GetName(type, pair.Key) + ": " + pair.Value)));
        }
    }
}