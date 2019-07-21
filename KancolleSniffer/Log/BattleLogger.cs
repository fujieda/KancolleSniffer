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
using KancolleSniffer.Model;

namespace KancolleSniffer.Log
{
    public class BattleLogger
    {
        private readonly ShipInfo _shipInfo;
        private readonly ItemInfo _itemInfo;
        private readonly BattleInfo _battleInfo;
        private readonly Action<string, string, string> _writer;
        private dynamic _battle;
        private dynamic _map;
        private bool _start;

        public BattleLogger(ShipInfo shipInfo, ItemInfo itemInfo, BattleInfo battleInfo, Action<string, string, string> writer)
        {
            _shipInfo = shipInfo;
            _itemInfo = itemInfo;
            _battleInfo = battleInfo;
            _writer = writer;
        }

        public void InspectMapStart(dynamic json)
        {
            _start = true;
            _map = json;
            _battle = null;
        }

        public void InspectMapNext(dynamic json)
        {
            _map = json;
        }

        public void InspectBattle(dynamic json)
        {
            if (_battle != null) // 通常の夜戦は無視する
                return;
            _battle = json;
        }

        public void InspectBattleResult(dynamic result)
        {
            if (result.disabled() || _map == null || _battle == null)
            {
                _map = _battle = null;
                return;
            }
            var fShips = GenerateFriendShipList();
            var eShips = GenerateEnemyShipList();
            var cell = (int)_map.api_no;
            var boss = "";
            if (_start)
                boss = "出撃";
            if (cell == (int)_map.api_bosscell_no || (int)_map.api_event_id == 5)
                boss = _start ? "出撃&ボス" : "ボス";
            var dropType = result.api_get_ship() ? result.api_get_ship.api_ship_type : "";
            if (result.api_get_useitem())
            {
                if (dropType == "")
                    dropType = "アイテム";
                else
                    dropType += "+アイテム";
            }
            var dropName = result.api_get_ship() ? result.api_get_ship.api_ship_name : "";
            if (result.api_get_useitem())
            {
                var itemName = _itemInfo.GetUseItemName((int)result.api_get_useitem.api_useitem_id);
                if (dropName == "")
                    dropName = itemName;
                else
                    dropName += "+" + itemName;
            }
            var fp = _shipInfo.Fleets[(int)_battle.api_deck_id - 1].FighterPower;
            var fPower = fp.Diff ? fp.RangeString : fp.Min.ToString();
            _writer("海戦・ドロップ報告書", string.Join(",",
                    result.api_quest_name,
                    cell, boss,
                    result.api_win_rank,
                    BattleFormationName((int)_battle.api_formation[2]),
                    FormationName(_battle.api_formation[0]),
                    FormationName(_battle.api_formation[1]),
                    result.api_enemy_info.api_deck_name,
                    dropType, dropName,
                    string.Join(",", fShips),
                    string.Join(",", eShips),
                    fPower, _battleInfo.EnemyFighterPower.AirCombat + _battleInfo.EnemyFighterPower.UnknownMark,
                    AirControlLevelName(_battle),
                    $"{(int)_map.api_maparea_id}-{(int)_map.api_mapinfo_no}"),
                "日付,海域,マス,ボス,ランク,艦隊行動,味方陣形,敵陣形,敵艦隊,ドロップ艦種,ドロップ艦娘," +
                "味方艦1,味方艦1HP,味方艦2,味方艦2HP,味方艦3,味方艦3HP,味方艦4,味方艦4HP,味方艦5,味方艦5HP,味方艦6,味方艦6HP," +
                "敵艦1,敵艦1HP,敵艦2,敵艦2HP,敵艦3,敵艦3HP,敵艦4,敵艦4HP,敵艦5,敵艦5HP,敵艦6,敵艦6HP," +
                "味方制空値,敵制空値,制空状態,マップ"
            );
            _map = _battle = null;
            _start = false;
        }

        private IEnumerable<string> GenerateFriendShipList()
        {
            if (_battle.api_f_nowhps_combined())
            {
                var mainShips = _shipInfo.Fleets[0].Ships;
                var guardShips = _shipInfo.Fleets[1].Ships;
                return mainShips.Zip(guardShips, (main, guard) =>
                {
                    if (main.Empty && guard.Empty)
                        return ",";
                    var name = "";
                    var hp = "";
                    if (!main.Empty)
                    {
                        name = $"{main.Name}(Lv{main.Level})";
                        hp = $"{main.NowHp}/{main.MaxHp}";
                    }
                    name += "・";
                    hp += "・";
                    if (!guard.Empty)
                    {
                        name += $"{guard.Name}(Lv{guard.Level})";
                        hp += $"{guard.NowHp}/{guard.MaxHp}";
                    }
                    return name + "," + hp;
                }).ToList();
            }
            var ships = _shipInfo.Fleets[(int)_battle.api_deck_id - 1].Ships;
            if (ships.Count > 6)
            {
                var result = new List<string>();
                for (var i = 0; i < 12 - ships.Count; i++)
                {
                    var ship = ships[i];
                    result.Add($"{ship.Name}(Lv{ship.Level}),{ship.NowHp}/{ship.MaxHp}");
                }
                for (var i = 0; i < ships.Count - 6; i++)
                {
                    var s1 = ships[12 - ships.Count + i];
                    var s2 = ships[6 + i];
                    result.Add(
                        $"{s1.Name}(Lv{s1.Level})・{s2.Name}(Lv{s2.Level})," +
                        $"{s1.NowHp}/{s1.MaxHp}・{s2.NowHp}/{s2.MaxHp}");
                }
                return result;
            }
            return ships.Select(ship =>
            {
                if (ship.Empty)
                    return ",";
                return $"{ship.Name}(Lv{ship.Level}),{ship.NowHp}/{ship.MaxHp}";
            }).ToList();
        }

        private IEnumerable<string> GenerateEnemyShipList()
        {
            var result = _battleInfo.Result.Enemy.Main.Concat(Enumerable.Repeat(new ShipStatus(), 6)).Take(6);
            if (_battleInfo.Result.Enemy.Guard.Length == 0)
            {
                return result.Select(s => s.Empty ? "," : $"{s.Name},{s.NowHp}/{s.MaxHp}").ToList();
            }
            var mainShips = result;
            var guardShips = _battleInfo.Result.Enemy.Guard.Concat(Enumerable.Repeat(new ShipStatus(), 6)).Take(6);
            return mainShips.Zip(guardShips, (main, guard) =>
            {
                if (main.Empty && guard.Empty)
                    return ",";
                var name = "";
                var hp = "";
                if (!main.Empty)
                {
                    name = $"{main.Name}";
                    hp = $"{main.NowHp}/{main.MaxHp}";
                }
                name += "・";
                hp += "・";
                if (!guard.Empty)
                {
                    name += $"{guard.Name}";
                    hp += $"{guard.NowHp}/{guard.MaxHp}";
                }
                return name + "," + hp;
            }).ToList();
        }

        private string FormationName(dynamic f)
        {
            if (f is string) // 連合艦隊のときは文字列
                f = int.Parse(f);
            switch ((int)f)
            {
                case 1:
                    return "単縦陣";
                case 2:
                    return "複縦陣";
                case 3:
                    return "輪形陣";
                case 4:
                    return "梯形陣";
                case 5:
                    return "単横陣";
                case 6:
                    return "警戒陣";
                case 11:
                    return "第一警戒航行序列";
                case 12:
                    return "第二警戒航行序列";
                case 13:
                    return "第三警戒航行序列";
                case 14:
                    return "第四警戒航行序列";
                default:
                    return "単縦陣";
            }
        }

        private static string BattleFormationName(int f)
        {
            switch (f)
            {
                case 1:
                    return "同航戦";
                case 2:
                    return "反航戦";
                case 3:
                    return "Ｔ字戦(有利)";
                case 4:
                    return "Ｔ字戦(不利)";
                default:
                    return "同航戦";
            }
        }

        private string AirControlLevelName(dynamic json)
        {
            // BattleInfo.AirControlLevelは夜戦で消されているかもしれないので、こちらで改めて調べる。
            if (!json.api_kouku())
                return "";
            var stage1 = json.api_kouku.api_stage1;
            if (stage1 == null)
                return "";
            if (stage1.api_f_count == 0 && stage1.api_e_count == 0)
                return "";
            switch ((int)stage1.api_disp_seiku)
            {
                case 0:
                    return "航空均衡";
                case 1:
                    return "制空権確保";
                case 2:
                    return "航空優勢";
                case 3:
                    return "航空劣勢";
                case 4:
                    return "制空権喪失";
                default:
                    return "";
            }
        }
    }
}