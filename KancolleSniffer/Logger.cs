// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace KancolleSniffer
{
    [Flags]
    public enum LogType
    {
        None = 0,
        Mission = 1 << 0,
        Battle = 1 << 1,
        Material = 1 << 2,
        CreateItem = 1 << 3,
        CreateShip = 1 << 4,
        RemodelSlot = 1 << 5,
        Achivement = 1 << 6,
        All = (1 << 7) - 1
    }

    public class Logger
    {
        private LogType _logType;
        private readonly ShipInfo _shipInfo;
        private readonly ItemInfo _itemInfo;
        private readonly BattleInfo _battleInfo;
        private Action<string, string, string> _writer;
        private Func<DateTime> _nowFunc;
        public const string DateTimeFormat = @"yyyy\-MM\-dd HH\:mm\:ss";
        private dynamic _battle;
        private dynamic _map;
        private dynamic _basic;
        private int _kdockId;
        private DateTime _prevTime;
        private int[] _currentMaterial = new int[Enum.GetValues(typeof(Material)).Length];
        private int _materialLogInterval = 10;
        private bool _start;
        private int _lastExp = -1;
        private DateTime _lastDate;
        private DateTime _endOfMonth;
        private DateTime _nextDate;

        public int MaterialLogInterval
        {
            set => _materialLogInterval = value;
        }

        public string OutputDir
        {
            set => _writer = new LogWriter(value).Write;
        }

        public static string FormatDateTime(DateTime date)
        {
            return date.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
        }

        public Logger(ShipInfo ship, ItemInfo item, BattleInfo battle)
        {
            _shipInfo = ship;
            _itemInfo = item;
            _battleInfo = battle;
            _writer = new LogWriter().Write;
            _nowFunc = () => DateTime.Now;
        }

        public void EnableLog(LogType type)
        {
            _logType = type;
        }

        public void SetWriter(Action<string, string, string> writer, Func<DateTime> nowFunc)
        {
            _writer = writer;
            _nowFunc = nowFunc;
        }

        public void FlashLog()
        {
            FlashAchivementLog();
        }

        public void InspectMissionResult(dynamic json)
        {
            var r = (int)json.api_clear_result;
            var rstr = r == 2 ? "大成功" : r == 1 ? "成功" : "失敗";
            var material = new int[7];
            if (r != 0)
                ((int[])json.api_get_material).CopyTo(material, 0);
            foreach (var i in new[] {1, 2})
            {
                var attr = "api_get_item" + i;
                if (!json.IsDefined(attr) || json[attr].api_useitem_id != -1)
                    continue;
                var count = (int)json[attr].api_useitem_count;
                var flag = ((int[])json.api_useitem_flag)[i - 1];
                if (flag == 1)
                    material[(int)Material.Bucket] = count;
                else if (flag == 2)
                    material[(int)Material.Burner + 2] = count; // 高速建造材と開発資材が反対なのでいつか直す
                else if (flag == 3)
                    material[(int)Material.Development - 2] = count;
            }
            if ((_logType & LogType.Mission) != 0)
            {
                _writer("遠征報告書",
                    string.Join(",", FormatDateTime(_nowFunc()),
                        rstr, json.api_quest_name, string.Join(",", material)),
                    "日付,結果,遠征,燃料,弾薬,鋼材,ボーキ,開発資材,高速修復材,高速建造材");
            }
        }

        public void InspectMapStart(dynamic json)
        {
            _start = true;
            _map = json;
            _battle = null;
            if ((_logType & LogType.Material) != 0)
                WriteMaterialLog(_nowFunc());
        }

        public void InspectMapNext(dynamic json)
        {
            if ((_logType & LogType.Achivement) != 0 && json.api_get_eo_rate() && (int)json.api_get_eo_rate != 0)
            {
                _writer("戦果",
                    FormatDateTime(_nowFunc()) + "," + _lastExp + "," + (int)json.api_get_eo_rate,
                    "日付,経験値,EO");
            }
            _map = json;
        }

        public void InspectClearItemGet(dynamic json)
        {
            if ((_logType & LogType.Achivement) == 0)
                return;
            if (!json.api_bounus())
                return;
            foreach (var entry in json.api_bounus)
            {
                if (entry.api_type != 18)
                    continue;
                _writer("戦果",
                    FormatDateTime(_nowFunc()) + "," + _lastExp + "," + (int)entry.api_count,
                    "日付,経験値,EO");
                break;
            }
        }

        public void InspectBattle(dynamic json)
        {
            if (_battle != null) // 通常の夜戦は無視する
                return;
            _battle = json;
        }

        public void InspectBattleResult(dynamic result)
        {
            if ((_logType & LogType.Achivement) != 0 && result.api_get_exmap_rate())
            {
                var rate = result.api_get_exmap_rate is string
                    ? int.Parse(result.api_get_exmap_rate)
                    : (int)result.api_get_exmap_rate;
                if (rate != 0)
                {
                    _writer("戦果", FormatDateTime(_nowFunc()) + "," + _lastExp + "," + rate,
                        "日付,経験値,EO");
                }
            }
            if ((_logType & LogType.Battle) == 0 || _map == null || _battle == null)
            {
                _map = _battle = null;
                return;
            }
            var fships = GenerateFirendShipList();
            var eships = GenerateEnemyShipList();
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
            var fp = _shipInfo.Fleets[BattleInfo.DeckId(_battle)].FighterPower;
            var fpower = fp[0] == fp[1] ? fp[0].ToString() : fp[0] + "～" + fp[1];
            _writer("海戦・ドロップ報告書", string.Join(",", FormatDateTime(_nowFunc()),
                    result.api_quest_name,
                    cell, boss,
                    result.api_win_rank,
                    BattleFormationName((int)_battle.api_formation[2]),
                    FormationName(_battle.api_formation[0]),
                    FormationName(_battle.api_formation[1]),
                    result.api_enemy_info.api_deck_name,
                    dropType, dropName,
                    string.Join(",", fships),
                    string.Join(",", eships),
                    fpower, _battleInfo.EnemyFighterPower.AirCombat + _battleInfo.EnemyFighterPower.UnknownMark,
                    AirControlLevelName(_battle)),
                "日付,海域,マス,ボス,ランク,艦隊行動,味方陣形,敵陣形,敵艦隊,ドロップ艦種,ドロップ艦娘," +
                "味方艦1,味方艦1HP,味方艦2,味方艦2HP,味方艦3,味方艦3HP,味方艦4,味方艦4HP,味方艦5,味方艦5HP,味方艦6,味方艦6HP," +
                "敵艦1,敵艦1HP,敵艦2,敵艦2HP,敵艦3,敵艦3HP,敵艦4,敵艦4HP,敵艦5,敵艦5HP,敵艦6,敵艦6HP," +
                "味方制空値,敵制空値,制空状態"
            );
            _map = _battle = null;
            _start = false;
        }

        private IEnumerable<string> GenerateFirendShipList()
        {
            int deckId = BattleInfo.DeckId(_battle);
            if (_battle.api_f_nowhps_combined())
            {
                var main = _shipInfo.Fleets[0].Deck;
                var guard = _shipInfo.Fleets[1].Deck;
                return main.Zip(guard, (m, g) =>
                {
                    if (m == -1 && g == -1)
                        return ",";
                    var name = "";
                    var hp = "";
                    if (m != -1)
                    {
                        var sm = _shipInfo.GetStatus(m);
                        name = $"{sm.Name}(Lv{sm.Level})";
                        hp = $"{sm.NowHp}/{sm.MaxHp}";
                    }
                    name += "・";
                    hp += "・";
                    if (g != -1)
                    {
                        var sg = _shipInfo.GetStatus(g);
                        name += $"{sg.Name}(Lv{sg.Level})";
                        hp += $"{sg.NowHp}/{sg.MaxHp}";
                    }
                    return name + "," + hp;
                }).ToList();
            }
            var deck = _shipInfo.Fleets[deckId].Deck;
            if (deck.Length > 6)
            {
                var result = new List<string>();
                for (var i = 0; i < 12 - deck.Length; i++)
                {
                    var s = _shipInfo.GetStatus(deck[i]);
                    result.Add($"{s.Name}(Lv{s.Level}),{s.NowHp}/{s.MaxHp}");
                }
                for (var i = 0; i < deck.Length - 6; i++)
                {
                    var s1 = _shipInfo.GetStatus(deck[12 - deck.Length + i]);
                    var s2 = _shipInfo.GetStatus(deck[6 + i]);
                    result.Add(
                        $"{s1.Name}(Lv{s1.Level})・{s2.Name}(Lv{s2.Level})," +
                        $"{s1.NowHp}/{s1.MaxHp}・{s2.NowHp}/{s2.MaxHp}");
                }
                return result;
            }
            return deck.Select(id =>
            {
                if (id == -1)
                    return ",";
                var s = _shipInfo.GetStatus(id);
                return $"{s.Name}(Lv{s.Level}),{s.NowHp}/{s.MaxHp}";
            }).ToList();
        }

        private IEnumerable<string> GenerateEnemyShipList()
        {
            var result = _battleInfo.Result.Enemy.Main.Concat(Enumerable.Repeat(new ShipStatus(), 6)).Take(6);
            if (_battleInfo.Result.Enemy.Guard.Length == 0)
            {
                return result.Select(s => s.Id == -1 ? "," : $"{s.Name},{s.NowHp}/{s.MaxHp}").ToList();
            }
            var main = result;
            var guard = _battleInfo.Result.Enemy.Guard.Concat(Enumerable.Repeat(new ShipStatus(), 6)).Take(6);
            return main.Zip(guard, (m, g) =>
            {
                if (m.Id == -1 && g.Id == -1)
                    return ",";
                var name = "";
                var hp = "";
                if (m.Id != -1)
                {
                    name = $"{m.Name}";
                    hp = $"{m.NowHp}/{m.MaxHp}";
                }
                name += "・";
                hp += "・";
                if (g.Id != -1)
                {
                    name += $"{g.Name}";
                    hp += $"{g.NowHp}/{g.MaxHp}";
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

        public void InspectBasic(dynamic json)
        {
            _basic = json;
            if ((_logType & LogType.Achivement) == 0)
                return;
            var now = _nowFunc();
            var exp = (int)json.api_experience;
            var isNewMonth = _endOfMonth == DateTime.MinValue || now.CompareTo(_endOfMonth) >= 0;
            var isNewDate = _nextDate == DateTime.MinValue || now.CompareTo(_nextDate) >= 0;
            if (isNewDate || isNewMonth)
            {
                if (_lastDate != DateTime.MinValue)
                {
                    _writer("戦果", FormatDateTime(_lastDate) + "," + _lastExp + ",0", "日付,経験値,EO");
                }
                _writer("戦果", FormatDateTime(now) + "," + exp + ",0", "日付,経験値,EO");
                if (isNewMonth)
                {
                    _endOfMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month),
                        22, 0, 0);
                    if (_endOfMonth.CompareTo(now) <= 0)
                    {
                        var days = _endOfMonth.Month == 12
                            ? DateTime.DaysInMonth(_endOfMonth.Year + 1, 1)
                            : DateTime.DaysInMonth(_endOfMonth.Year, _endOfMonth.Month);
                        _endOfMonth = _endOfMonth.AddDays(days);
                    }
                }
                _nextDate = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0);
                if (now.Hour >= 2)
                    _nextDate = _nextDate.AddDays(1);
                if (_nextDate.Day == 1)
                    _nextDate = _nextDate.AddDays(1);
            }
            _lastDate = now;
            _lastExp = exp;
        }

        private void FlashAchivementLog()
        {
            if ((_logType & LogType.Achivement) == 0)
                return;
            if (_lastDate != DateTime.MinValue)
            {
                _writer("戦果", FormatDateTime(_lastDate) + "," + _lastExp + ",0", "日付,経験値,EO");
            }
        }

        public void InspectCreateItem(string request, dynamic json)
        {
            if ((_logType & LogType.CreateItem) == 0)
                return;
            var values = HttpUtility.ParseQueryString(request);
            var name = "失敗";
            var type = "";
            if (json.api_slot_item())
            {
                var spec = _itemInfo.GetSpecByItemId((int)json.api_slot_item.api_slotitem_id);
                name = spec.Name;
                type = spec.TypeName;
            }
            _writer("開発報告書",
                FormatDateTime(_nowFunc()) + "," +
                string.Join(",", name, type,
                    values["api_item1"], values["api_item2"], values["api_item3"], values["api_item4"],
                    Secretary(), _basic.api_level),
                "日付,開発装備,種別,燃料,弾薬,鋼材,ボーキ,秘書艦,司令部Lv");
        }

        public void InspectCreateShip(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            _kdockId = int.Parse(values["api_kdock_id"]);
        }

        public void InspectKDock(dynamic json)
        {
            if ((_logType & LogType.CreateShip) == 0 || _basic == null || _kdockId == 0)
                return;
            var kdock = ((dynamic[])json).First(e => e.api_id == _kdockId);
            var material = Enumerable.Range(1, 5).Select(i => (int)kdock["api_item" + i]).ToArray();
            var ship = _shipInfo.GetSpec((int)kdock.api_created_ship_id);
            var avail = ((dynamic[])json).Count(e => (int)e.api_state == 0);
            _writer("建造報告書",
                FormatDateTime(_nowFunc()) + "," +
                string.Join(",", material.First() >= 1500 ? "大型艦建造" : "通常艦建造",
                    ship.Name, ship.ShipTypeName, string.Join(",", material), avail, Secretary(), _basic.api_level),
                "日付,種類,名前,艦種,燃料,弾薬,鋼材,ボーキ,開発資材,空きドック,秘書艦,司令部Lv");
            _kdockId = 0;
        }

        private string Secretary()
        {
            var ship = _shipInfo.Fleets[0].Ships[0];
            return ship.Name + "(" + ship.Level + ")";
        }

        public void InspectMaterial(dynamic json)
        {
            if ((_logType & LogType.Material) == 0)
                return;
            foreach (var e in json)
                _currentMaterial[(int)e.api_id - 1] = (int)e.api_value;
            var now = _nowFunc();
            if (now - _prevTime < TimeSpan.FromMinutes(_materialLogInterval))
                return;
            WriteMaterialLog(now);
        }

        public void WriteMaterialLog(DateTime now)
        {
            _prevTime = now;
            _writer("資材ログ",
                FormatDateTime(now) + "," +
                string.Join(",", _currentMaterial),
                "日付,燃料,弾薬,鋼材,ボーキ,高速建造材,高速修復材,開発資材,改修資材");
        }

        public void SetCurrentMaterial(int[] material)
        {
            _currentMaterial = material;
        }

        public void InspectRemodelSlot(string request, dynamic json)
        {
            if ((_logType & LogType.RemodelSlot) == 0)
                return;
            var now = _nowFunc();
            var values = HttpUtility.ParseQueryString(request);
            var id = int.Parse(values["api_slot_id"]);
            var name = _itemInfo.GetName(id);
            var level = _itemInfo.GetStatus(id).Level;
            var success = (int)json.api_remodel_flag == 1 ? "○" : "×";
            var certain = int.Parse(values["api_certain_flag"]) == 1 ? "○" : "";
            var useName = "";
            var useNum = "";
            if (json.api_use_slot_id())
            {
                var use = (int[])json.api_use_slot_id;
                useName = _itemInfo.GetName(use[0]);
                useNum = use.Length.ToString();
            }
            var after = (int[])json.api_after_material;
            var diff = new int[after.Length];
            for (var i = 0; i < after.Length; i++)
                diff[i] = _currentMaterial[i] - after[i];
            var ship1 = Secretary();
            var ship2 = "";
            var ships = _shipInfo.Fleets[0].Ships;
            if (ships.Length >= 2)
                ship2 = ships[1].Name + "(" + ships[1].Level + ")";
            _writer("改修報告書",
                FormatDateTime(now) + "," +
                string.Join(",", name, level, success, certain, useName, useNum,
                    diff[(int)Material.Fuel], diff[(int)Material.Bullet], diff[(int)Material.Steal],
                    diff[(int)Material.Bouxite],
                    diff[(int)Material.Development], diff[(int)Material.Screw],
                    ship1, ship2),
                "日付,改修装備,レベル,成功,確実化,消費装備,消費数,燃料,弾薬,鋼材,ボーキ,開発資材,改修資材,秘書艦,二番艦");
        }
    }

    public class LogWriter
    {
        private readonly IFile _file;
        private readonly string _outputDir;

        public interface IFile
        {
            string ReadAllText(string path);
            void AppendAllText(string path, string text);
            void Delete(string path);
            bool Exists(string path);
        }

        private class FileWrapper : IFile
        {
            // Shift_JISでないとExcelで文字化けする
            private readonly Encoding _encoding = Encoding.GetEncoding("Shift_JIS");

            public string ReadAllText(string path) => File.ReadAllText(path, _encoding);

            public void AppendAllText(string path, string text)
            {
                File.AppendAllText(path, text, _encoding);
            }

            public void Delete(string path)
            {
                File.Delete(path);
            }

            public bool Exists(string path) => File.Exists(path);
        }

        public LogWriter(string outputDir = null, IFile file = null)
        {
            _outputDir = outputDir ?? AppDomain.CurrentDomain.BaseDirectory;
            _file = file ?? new FileWrapper();
        }

        public void Write(string file, string s, string header)
        {
            var path = Path.Combine(_outputDir, file);
            var csv = path + ".csv";
            var tmp = path + ".tmp";
            if (_file.Exists(tmp))
            {
                try
                {
                    _file.AppendAllText(csv, _file.ReadAllText(tmp));
                    _file.Delete(tmp);
                }
                catch (IOException)
                {
                }
            }
            if (!_file.Exists(csv))
                s = header + "\r\n" + s;
            foreach (var f in new[] {csv, tmp})
            {
                try
                {
                    _file.AppendAllText(f, s + "\r\n");
                    break;
                }
                catch (IOException e)
                {
                    if (f == tmp)
                        throw new LogIOException("報告書の出力中にエラーが発生しました。", e);
                }
            }
        }
    }

    public class LogIOException : Exception
    {
        public LogIOException()
        {
        }

        public LogIOException(string message) : base(message)
        {
        }

        public LogIOException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}