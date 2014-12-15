﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
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
        All = 7,
    }

    public class Logger
    {
        private LogType _logType;
        private readonly ShipMaster _shipMaster;
        private readonly ShipInfo _shipInfo;
        private Action<string, string, string> _writer;
        private Func<DateTime> _nowFunc;
        private const string DateTimeFormat = @"yyyy\-MM\-dd HH\:mm\:ss";
        private dynamic _battle;
        private dynamic _map;

        public Logger(ShipMaster master, ShipInfo ship)
        {
            _shipMaster = master;
            _shipInfo = ship;
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
            if ((_logType & LogType.Mission) == 0)
                return;
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
            _writer("遠征報告書",
                string.Join(",", _nowFunc().ToString(DateTimeFormat),
                    rstr, json.api_quest_name, string.Join(",", material)),
                "日付,結果,遠征,燃料,弾薬,鋼材,ボーキ,開発資材,高速修復材,高速建造材");
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

        public void InspectMaterial(dynamic json)
        {
            if ((_logType & LogType.Material) == 0)
                return;
            _writer("資材ログ",
                _nowFunc().ToString(DateTimeFormat) + "," +
                string.Join(",", ((dynamic[])json).Select(e => (int)e.api_value)),
                "日付,燃料,弾薬,鋼材,ボーキ,高速修復材,高速建造材,開発資材,改修資材");
        }
    }

    public class LogWriter
    {
        private readonly IFile _file;

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

        public LogWriter(IFile file = null)
        {
            _file = file ?? new FileWrapper();
        }

        public void Write(string file, string s, string header)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), file);
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