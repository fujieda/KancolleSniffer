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
using static System.Math;

namespace KancolleSniffer
{
    public class MiscTextInfo
    {
        private const string GuideText = "[海域ゲージ情報]\r\n 海域選択画面に進むと表示します。\r\n[演習情報]\r\n 演習相手を選ぶと表示します。";
        public string Text { get; private set; } = GuideText;
        public bool ClearFlag { private get; set; }

        public void ClearIfNeeded()
        {
            if (!ClearFlag)
                return;
            Text = GuideText;
            ClearFlag = false;
        }

        private readonly Dictionary<int, int> _required = new Dictionary<int, int>
        {
            {15, 4},
            {16, 7},
            {25, 4},
            {35, 4},
            {44, 4},
            {45, 5},
            {52, 4},
            {53, 5},
            {54, 5},
            {55, 5},
            {62, 3},
            {63, 4},
            {64, 5},
            {65, 6}
        };

        public void InspectMapInfo(dynamic json)
        {
            Text = "[海域ゲージ]\r\n";
            foreach (var entry in json.api_map_info() ? json.api_map_info : json)
            {
                var map = (int)entry.api_id;
                if (entry.api_eventmap())
                {
                    var evmap = entry.api_eventmap;
                    Text += $"{map / 10}-{map % 10} : HP {(int)evmap.api_now_maphp}/{(int)evmap.api_max_maphp}\r\n";
                    continue;
                }
                if (!entry.api_defeat_count())
                    continue;
                int req;
                var reqStr = _required.TryGetValue(map, out req) ? req.ToString() : "?";
                Text += $"{map / 10}-{map % 10} : 撃破 {(int)entry.api_defeat_count}/{reqStr}\r\n";
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
            4564000, 4777000, 4999000, 5230000, 5470000
        };

        public void InspectPracticeEnemyInfo(dynamic json)
        {
            Text = $"[演習情報]\r\n敵艦隊名 : {json.api_deckname}\r\n";
            var ships = json.api_deck.api_ships;
            var s1 = (int)ships[0].api_id != -1 ? (int)ships[0].api_level : 1;
            var s2 = (int)ships[1].api_id != -1 ? (int)ships[1].api_level : 1;
            var raw = _expTable[Min(s1, _expTable.Length) - 1] / 100.0 + _expTable[Min(s2, _expTable.Length) - 1] / 300.0;
            var exp = raw >= 500 ? 500 + (int)Sqrt(raw - 500) : (int)raw;
            Text += $"獲得経験値 : {exp}\r\nS勝利 : {(int)(exp * 1.2)}";
        }
    }
}