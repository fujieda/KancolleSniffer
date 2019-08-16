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
        private readonly ItemInfo _itemInfo;
        private readonly BattleInfo _battleInfo;
        private readonly Action<string, string, string> _writer;
        private readonly Dictionary<int, string> _mapName = new Dictionary<int, string>();
        private readonly CellData _cell = new CellData();

        public BattleLogger(ItemInfo itemInfo, BattleInfo battleInfo, Action<string, string, string> writer)
        {
            _itemInfo = itemInfo;
            _battleInfo = battleInfo;
            _writer = writer;
        }

        public void InspectMapInfoMaster(dynamic json)
        {
            foreach (var entry in json)
                _mapName[(int)entry.api_id] = entry.api_name;
        }

        private class CellData
        {
            public bool Start;
            public bool Boss;
            public int Id;
            public int Area;
            public int Map;
            public int Cell;

            public void Set(dynamic json)
            {
                Area = (int)json.api_maparea_id;
                Map = (int)json.api_mapinfo_no;
                Cell = json.api_no() ? (int)json.api_no : 0;
                Boss = (int)json.api_event_id == 5;
                Id = Area * 10 + Map;
            }
        }

        public void InspectMapStart(dynamic json)
        {
            _cell.Start = true;
            _cell.Set(json);
        }

        public void InspectMapNext(dynamic json)
        {
            _cell.Set(json);
            if (!json.api_destruction_battle())
                return;
            WriteLog(null);
            _cell.Start = false;
        }

        public void InspectBattleResult(dynamic result)
        {
            WriteLog(result);
            _cell.Start = false;
        }

        private void WriteLog(dynamic result)
        {
            var log = CreateLog(result);
            _writer("海戦・ドロップ報告書", log,
                "日付,海域,マス,ボス,ランク,艦隊行動,味方陣形,敵陣形,敵艦隊,ドロップ艦種,ドロップ艦娘," +
                "味方艦1,味方艦1HP,味方艦2,味方艦2HP,味方艦3,味方艦3HP,味方艦4,味方艦4HP,味方艦5,味方艦5HP,味方艦6,味方艦6HP," +
                "敵艦1,敵艦1HP,敵艦2,敵艦2HP,敵艦3,敵艦3HP,敵艦4,敵艦4HP,敵艦5,敵艦5HP,敵艦6,敵艦6HP," +
                "味方制空値,敵制空値,制空状態,マップ"
            );
        }

        private string CreateLog(dynamic result)
        {
            var fShips = GenerateShipList(_battleInfo.Result.Friend, s => $"{s.Name}(Lv{s.Level})");
            var eShips = GenerateShipList(_battleInfo.Result.Enemy, s => $"{s.Name}");
            var boss = "";
            if (_cell.Start)
                boss = "出撃";
            if (_cell.Boss)
                boss = _cell.Start ? "出撃&ボス" : "ボス";
            var dropType = CreateDropType(result);
            var dropName = CreateDropName(result);
            var enemyName = result?.api_enemy_info.api_deck_name ?? "";
            var rank = result?.api_win_rank ?? _battleInfo.ResultRank;
            var fp = _battleInfo.FighterPower;
            var fPower = fp.Diff ? fp.RangeString : fp.Min.ToString();
            return string.Join(",",
                _mapName[_cell.Id],
                _cell.Cell, boss,
                rank,
                BattleFormationName(_battleInfo.Formation[2]),
                FormationName(_battleInfo.Formation[0]),
                FormationName(_battleInfo.Formation[1]),
                enemyName,
                dropType, dropName,
                string.Join(",", fShips),
                string.Join(",", eShips),
                fPower, _battleInfo.EnemyFighterPower.AirCombat + _battleInfo.EnemyFighterPower.UnknownMark,
                AirControlLevelName(_battleInfo.AirControlLevel),
                $"{_cell.Area}-{_cell.Map}");
        }

        private static string CreateDropType(dynamic result)
        {
            if (result == null)
                return "";
            var type = result.api_get_ship() ? (string)result.api_get_ship.api_ship_type : "";
            if (!result.api_get_useitem())
                return type;
            return type == "" ? "アイテム" : type + "+アイテム";
        }

        private string CreateDropName(dynamic result)
        {
            if (result == null)
                return "";
            var name = result.api_get_ship() ? (string)result.api_get_ship.api_ship_name : "";
            if (!result.api_get_useitem())
                return name;
            var itemName = _itemInfo.GetUseItemName((int)result.api_get_useitem.api_useitem_id);
            return name == "" ? itemName : name + "+" + itemName;
        }

        private static IEnumerable<string> GenerateShipList(BattleInfo.BattleResult.Combined fleet,
            Func<ShipStatus, string> toName)
        {
            fleet = FillEmpty(fleet);
            if (fleet.Guard.Length > 0)
            {
                return fleet.Main.Zip(fleet.Guard, (main, guard) =>
                {
                    if (main.Empty && guard.Empty)
                        return ",";
                    var name = "";
                    var hp = "";
                    if (!main.Empty)
                    {
                        name = toName(main);
                        hp = $"{main.NowHp}/{main.MaxHp}";
                    }
                    name += "・";
                    hp += "・";
                    if (!guard.Empty)
                    {
                        name += toName(guard);
                        hp += $"{guard.NowHp}/{guard.MaxHp}";
                    }
                    return name + "," + hp;
                }).ToList();
            }
            var ships = fleet.Main;
            if (fleet.Main.Length > 6)
            {
                var result = new List<string>();
                for (var i = 0; i < 12 - ships.Length; i++)
                {
                    var ship = fleet.Main[i];
                    result.Add($"{toName(ship)},{ship.NowHp}/{ship.MaxHp}");
                }
                for (var i = 0; i < ships.Length - 6; i++)
                {
                    var s1 = ships[12 - ships.Length + i];
                    var s2 = ships[6 + i];
                    result.Add(
                        $"{toName(s1)}・{toName(s2)}," +
                        $"{s1.NowHp}/{s1.MaxHp}・{s2.NowHp}/{s2.MaxHp}");
                }
                return result;
            }
            return ships.Select(ship => ship.Empty ? "," : $"{toName(ship)},{ship.NowHp}/{ship.MaxHp}");
        }

        private static BattleInfo.BattleResult.Combined FillEmpty(BattleInfo.BattleResult.Combined fleet)
        {
            return new BattleInfo.BattleResult.Combined
            {
                Main = FillEmpty(fleet.Main),
                Guard = FillEmpty(fleet.Guard)
            };
        }

        private static readonly ShipStatus[] Padding =
            Enumerable.Repeat(new ShipStatus(), ShipInfo.MemberCount).ToArray();

        private static ShipStatus[] FillEmpty(ShipStatus[] ships)
        {
            return ships.Length > ShipInfo.MemberCount || ships.Length == 0
                ? ships
                : ships.Concat(Padding).Take(ShipInfo.MemberCount).ToArray();
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

        private string AirControlLevelName(int level)
        {
            switch (level)
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