using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Web;
using System.Windows.Forms;

namespace KancolleSniffer
{
    [Flags]
    public enum LogType
    {
        None = 0,
        Mission = 1,
        Battle = 2,
        Material = 4,
        CreateItem = 8,
        CreateShip = 16,
        All = 31,
    }

    public class Logger
    {
        private LogType _logType;
        private readonly ShipMaster _shipMaster;
        private readonly ShipInfo _shipInfo;
        private readonly ItemInfo _itemInfo;
        private Action<string, string, string> _writer;
        private Func<DateTime> _nowFunc;
        private const string DateTimeFormat = @"yyyy\-MM\-dd HH\:mm\:ss";
        private dynamic _battle;
        private dynamic _map;
        private dynamic _basic;
        private int _kdockId;
        private DateTime _prevTime;
        private int _materialLogInterval = 10;

        public int MaterialLogInterval
        {
            set { _materialLogInterval = value; }
        }

        public string OutputDir
        {
            set { _writer = new LogWriter(value).Write; }
        }

        public Logger(ShipMaster master, ShipInfo ship, ItemInfo item)
        {
            _shipMaster = master;
            _shipInfo = ship;
            _itemInfo = item;
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
                    material[(int)Material.Burner] = count;
                else if (flag == 3)
                    material[(int)Material.Development] = count;
            }
            if ((_logType & LogType.Mission) != 0)
            {
                _writer("遠征報告書",
                    string.Join(",", _nowFunc().ToString(DateTimeFormat),
                        rstr, json.api_quest_name, string.Join(",", material)),
                    "日付,結果,遠征,燃料,弾薬,鋼材,ボーキ,開発資材,高速修復材,高速建造材");
            }
        }

        public void InspectMap(dynamic json)
        {
            _map = json;
        }

        public void InspectBattle(dynamic json)
        {
            if (!IsNightBattle(json))
                _battle = json;
        }

        private bool IsNightBattle(dynamic json)
        {
            return json.api_hougeki();
        }

        public void InspectBattleResult(dynamic result)
        {
            if ((_logType & LogType.Battle) == 0 || _map == null || _battle == null)
            {
                _map = _battle = null;
                return;
            }
            var fships = new List<string>();
            var deck = _shipInfo.GetDeck(_battle.api_dock_id() ? (int)_battle.api_dock_id - 1 : 0);
            fships.AddRange(deck.Select(id =>
            {
                if (id == -1)
                    return ",";
                var s = _shipInfo[id];
                return string.Format("{0}(Lv{1}),{2}/{3}", s.Name, s.Level, s.NowHp, s.MaxHp);
            }));
            var edeck = ((int[])_battle.api_ship_ke).Skip(1).ToArray();
            var enowhp = ((int[])_battle.api_nowhps).Skip(7).ToArray();
            var emaxhp = ((int[])_battle.api_maxhps).Skip(7).ToArray();
            var eships = new List<string>();
            for (var i = 0; i < edeck.Count(); i++)
            {
                eships.Add(edeck[i] == -1
                    ? ","
                    : string.Format("{0},{1}/{2}", _shipMaster[edeck[i]].Name, enowhp[i], emaxhp[i]));
            }
            var cell = (int)_map.api_no;
            var boss = cell == (int)_map.api_bosscell_no || (int)_map.api_event_id == 5 ? "ボス" : "";
            _writer("海戦・ドロップ報告書", string.Join(",", _nowFunc().ToString(DateTimeFormat),
                result.api_quest_name,
                cell, boss,
                result.api_win_rank,
                BattleFormationName((int)_battle.api_formation[2]),
                FormationName(_battle.api_formation[0]),
                FormationName(_battle.api_formation[1]),
                result.api_enemy_info.api_deck_name,
                result.api_get_ship() ? result.api_get_ship.api_ship_type : "",
                result.api_get_ship() ? result.api_get_ship.api_ship_name : "",
                string.Join(",", fships),
                string.Join(",", eships)),
                "日付,海域,マス,ボス,ランク,艦隊行動,味方陣形,敵陣形,敵艦隊,ドロップ艦種,ドロップ艦娘," +
                "味方艦1,味方艦1HP,味方艦2,味方艦2HP,味方艦3,味方艦3HP,味方艦4,味方艦4HP,味方艦5,味方艦5HP,味方艦6,味方艦6HP," +
                "敵艦1,敵艦1HP,敵艦2,敵艦2HP,敵艦3,敵艦3HP,敵艦4,敵艦4HP,敵艦5,敵艦5HP,敵艦6,敵艦6HP"
                );
            _map = _battle = null;
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

        private static String BattleFormationName(int f)
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

        public void InspectBasic(dynamic json)
        {
            _basic = json;
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
                _nowFunc().ToString(DateTimeFormat) + "," +
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
            var ship = _shipMaster[(int)kdock.api_created_ship_id];
            var avail = ((dynamic[])json).Count(e => (int)e.api_state == 0);
            _writer("建造報告書",
                _nowFunc().ToString(DateTimeFormat) + "," +
                string.Join(",", material.First() >= 1500 ? "大型艦建造" : "通常艦建造",
                    ship.Name, ship.ShipTypeName, string.Join(",", material), avail, Secretary(), _basic.api_level),
                "日付,種類,名前,艦種,燃料,弾薬,鋼材,ボーキ,開発資材,空きドック,秘書艦,司令部Lv");
            _kdockId = 0;
        }

        private string Secretary()
        {
            var ship = _shipInfo.GetShipStatuses(0)[0];
            return ship.Name + "(" + ship.Level + ")";
        }

        public void InspectMaterial(dynamic json)
        {
            if ((_logType & LogType.Material) == 0)
                return;
            var now = _nowFunc();
            if (now - _prevTime < TimeSpan.FromMinutes(_materialLogInterval))
                return;
            _prevTime = now;
            var material = new int[8];
            foreach (var e in json)
                material[(int)e.api_id - 1] = (int)e.api_value;
            _writer("資材ログ",
                now.ToString(DateTimeFormat) + "," +
                string.Join(",", material) + ",",
                "日付,燃料,弾薬,鋼材,ボーキ,高速修復材,高速建造材,開発資材,改修資材");
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

            public string ReadAllText(string path)
            {
                return File.ReadAllText(path, _encoding);
            }

            public void AppendAllText(string path, string text)
            {
                File.AppendAllText(path, text, _encoding);
            }

            public void Delete(string path)
            {
                File.Delete(path);
            }

            public bool Exists(string path)
            {
                return File.Exists(path);
            }
        }

        public LogWriter(string outputDir = null, IFile file = null)
        {
            _outputDir = outputDir ?? Path.GetDirectoryName(Application.ExecutablePath);
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
                catch (IOException)
                {
                }
            }
        }
    }
}