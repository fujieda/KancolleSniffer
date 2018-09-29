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
using static System.Math;

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
                var count = _gaugeCount.TryGetValue(map, out var max)
                    ? $"{max - (int)entry.api_defeat_count}/{max}"
                    : "?/?";
                Text += $"{map / 10}-{map % 10} : 残り {count}\r\n";
            }
        }

        private readonly int[] _expTable =
        {
            0, 100, 300, 600, 1000, 1500, 2100, 2800, 3600, 4500,
            5500, 6600, 7800, 9100, 10500, 12000, 13600, 15300, 17100, 19000,
            21000, 23100, 25300, 27600, 30000, 32500, 35100, 37800, 40600, 43500,
            46500, 49600, 52800, 56100, 59500, 63000, 66600, 70300, 74100, 78000,
            82000, 86100, 90300, 94600, 99000, 103500, 108100, 112800, 117600, 122500,
            127500, 132700, 138100, 143700, 149500, 155500, 161700, 168100, 174700, 181500,
            188500, 195800, 203400, 211300, 219500, 228000, 236800, 245900, 255300, 265000,
            275000, 285400, 296200, 307400, 319000, 331000, 343400, 356200, 369400, 383000,
            397000, 411500, 426500, 442000, 458000, 474500, 491500, 509000, 527000, 545500,
            564500, 584500, 606500, 631500, 661500, 701500, 761500, 851500, 1000000, 1000000,
            1010000, 1011000, 1013000, 1016000, 1020000, 1025000, 1031000, 1038000, 1046000, 1055000,
            1065000, 1077000, 1091000, 1107000, 1125000, 1145000, 1168000, 1194000, 1223000, 1255000,
            1290000, 1329000, 1372000, 1419000, 1470000, 1525000, 1584000, 1647000, 1714000, 1785000,
            1860000, 1940000, 2025000, 2115000, 2210000, 2310000, 2415000, 2525000, 2640000, 2760000,
            2887000, 3021000, 3162000, 3310000, 3465000, 3628000, 3799000, 3978000, 4165000, 4360000,
            4564000, 4777000, 4999000, 5230000, 5470000, 5720000, 5780000, 5860000, 5970000, 6120000,
            6320000, 6580000, 6910000, 7320000, 7820000
        };

        public void InspectPracticeEnemyInfo(dynamic json)
        {
            Text = $"[演習情報]\r\n敵艦隊名 : {json.api_deckname}\r\n";
            var ships = json.api_deck.api_ships;
            var s1 = (int)ships[0].api_id != -1 ? (int)ships[0].api_level : 1;
            var s2 = (int)ships[1].api_id != -1 ? (int)ships[1].api_level : 1;
            var raw = _expTable[Min(s1, _expTable.Length) - 1] / 100.0 +
                      _expTable[Min(s2, _expTable.Length) - 1] / 300.0;
            var exp = raw >= 500 ? 500 + (int)Sqrt(raw - 500) : (int)raw;
            var bonus = 1 + TrainingCruiserBonus(_shipInfo.Fleets[0].ActualShips);
            Text += $"獲得経験値 : {(int)(exp * bonus)}\r\nS勝利 : {(int)((int)(exp * 1.2) * bonus)}";
        }

        private double TrainingCruiserBonus(IReadOnlyList<ShipStatus> fleet)
        {
            if (fleet[0].Spec.IsTrainingCruiser)
            {
                var fsLevel = fleet[0].Level;
                if (fleet.Skip(1).Any(s => s.Spec.IsTrainingCruiser))
                {
                    if (fsLevel < 10)
                        return 0.10;
                    if (fsLevel < 30)
                        return 0.13;
                    if (fsLevel < 60)
                        return 0.16;
                    if (fsLevel < 100)
                        return 0.20;
                    return 0.25;
                }
                if (fsLevel < 10)
                    return 0.05;
                if (fsLevel < 30)
                    return 0.08;
                if (fsLevel < 60)
                    return 0.12;
                if (fsLevel < 100)
                    return 0.15;
                return 0.20;
            }
            var tc = fleet.Count(s => s.Spec.IsTrainingCruiser);
            if (tc == 0)
                return 0;
            var level = fleet.Where(s => s.Spec.IsTrainingCruiser).Max(s => s.Level);
            if (tc == 1)
            {
                if (level < 10)
                    return 0.03;
                if (level < 30)
                    return 0.05;
                if (level < 60)
                    return 0.07;
                if (level < 100)
                    return 0.10;
                return 0.15;
            }
            if (level < 10)
                return 0.04;
            if (level < 30)
                return 0.06;
            if (level < 60)
                return 0.08;
            if (level < 100)
                return 0.12;
            return 0.175;
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