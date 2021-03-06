﻿// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using KancolleSniffer.Model;
using KancolleSniffer.Util;

namespace KancolleSniffer.Log
{
    [Flags]
    public enum LogType
    {
        None = 0,
        Mission = 1,
        Battle = 1 << 1,
        Material = 1 << 2,
        CreateItem = 1 << 3,
        CreateShip = 1 << 4,
        RemodelSlot = 1 << 5,
        Achievement = 1 << 6,
        All = (1 << 7) - 1
    }

    public class Logger
    {
        private LogType _logType;
        private readonly ShipInfo _shipInfo;
        private readonly ItemInfo _itemInfo;
        private Action<string, string, string> _writer;
        private Func<DateTime> _nowFunc;
        public const string DateTimeFormat = @"yyyy\-MM\-dd HH\:mm\:ss";
        private dynamic _basic;
        private int _kdockId;
        private DateTime _prevTime;
        private int[] _currentMaterial = new int[Enum.GetValues(typeof(Material)).Length];
        private int _materialLogInterval = 10;
        private int _lastExp = -1;
        private DateTime _lastDate;
        private DateTime _endOfMonth;
        private DateTime _nextDate;
        private readonly BattleLogger _battleLogger;

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

        public Logger(ShipInfo shipInfo, ItemInfo itemInfo, BattleInfo battleInfo)
        {
            _shipInfo = shipInfo;
            _itemInfo = itemInfo;
            _battleLogger = new BattleLogger(itemInfo, battleInfo, WriteNow);
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

        private void WriteNow(string name, string log, string header)
        {
            Write(name, _nowFunc(), log, header);
        }

        private void Write(string name, DateTime time, string log, string header)
        {
            _writer(name, FormatDateTime(time) + "," + log, header);
        }

        public void FlashLog()
        {
            FlashAchievementLog();
        }

        public void InspectMapInfoMaster(dynamic json)
        {
            _battleLogger.InspectMapInfoMaster(json);
        }

        public void InspectMissionResult(dynamic json)
        {
            var r = (int)json.api_clear_result;
            var resStr = r == 2 ? "大成功" : r == 1 ? "成功" : "失敗";
            var material = new int[8];
            if (r != 0)
                ((int[])json.api_get_material).CopyTo(material, 0);
            foreach (var i in new[] {1, 2})
            {
                var attr = "api_get_item" + i;
                if (!json.IsDefined(attr))
                    continue;
                var count = (int)json[attr].api_useitem_count;
                var flag = ((int[])json.api_useitem_flag)[i - 1];
                switch (flag)
                {
                    case 1:
                        material[(int)Material.Bucket] = count;
                        break;
                    case 2:
                        material[(int)Material.Burner + 2] = count; // 高速建造材と開発資材が反対なのでいつか直す
                        break;
                    case 3:
                        material[(int)Material.Development - 2] = count;
                        break;
                    case 4:
                        if ((int)json[attr].api_useitem_id == 4)
                            material[(int)Material.Screw] = count;
                        break;
                }
            }
            if ((_logType & LogType.Mission) != 0)
            {
                WriteNow("遠征報告書",
                    string.Join(",",
                        resStr, json.api_quest_name, string.Join(",", material)),
                    "日付,結果,遠征,燃料,弾薬,鋼材,ボーキ,開発資材,高速修復材,高速建造材,改修資材");
            }
        }

        public void InspectMapStart(dynamic json)
        {
            if ((_logType & LogType.Battle) != 0)
                _battleLogger.InspectMapStart(json);
            if ((_logType & LogType.Material) != 0)
                WriteMaterialLog(_nowFunc());
        }

        public void InspectMapNext(dynamic json)
        {
            if ((_logType & LogType.Achievement) != 0 && json.api_get_eo_rate() && (int)json.api_get_eo_rate != 0)
            {
                WriteNow("戦果", _lastExp + "," + (int)json.api_get_eo_rate, "日付,経験値,EO");
            }
            if ((_logType & LogType.Battle) != 0)
                _battleLogger.InspectMapNext(json);
        }

        public void InspectClearItemGet(dynamic json)
        {
            if ((_logType & LogType.Achievement) == 0)
                return;
            if (!json.api_bounus())
                return;
            foreach (var entry in json.api_bounus)
            {
                if (entry.api_type != 18)
                    continue;
                WriteNow("戦果", _lastExp + "," + (int)entry.api_count, "日付,経験値,EO");
                break;
            }
        }

        public void InspectBattleResult(dynamic result)
        {
            if ((_logType & LogType.Achievement) != 0 && result.api_get_exmap_rate())
            {
                var rate = result.api_get_exmap_rate is string
                    ? int.Parse(result.api_get_exmap_rate)
                    : (int)result.api_get_exmap_rate;
                if (rate != 0)
                {
                    WriteNow("戦果", _lastExp + "," + rate, "日付,経験値,EO");
                }
            }
            if ((_logType & LogType.Battle) != 0)
                _battleLogger.InspectBattleResult(result);
        }

        public void InspectBasic(dynamic json)
        {
            _basic = json;
            if ((_logType & LogType.Achievement) == 0)
                return;
            var now = _nowFunc();
            var exp = (int)json.api_experience;
            var isNewMonth = _endOfMonth == DateTime.MinValue || now.CompareTo(_endOfMonth) >= 0;
            var isNewDate = _nextDate == DateTime.MinValue || now.CompareTo(_nextDate) >= 0;
            if (isNewDate || isNewMonth)
            {
                if (_lastDate != DateTime.MinValue)
                {
                    Write("戦果", _lastDate, _lastExp + ",0", "日付,経験値,EO");
                }
                Write("戦果", now, exp + ",0", "日付,経験値,EO");
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

        private void FlashAchievementLog()
        {
            if ((_logType & LogType.Achievement) == 0)
                return;
            if (_lastDate != DateTime.MinValue)
            {
                Write("戦果", _lastDate, _lastExp + ",0", "日付,経験値,EO");
            }
        }

        public void InspectCreateItem(string request, dynamic json)
        {
            if ((_logType & LogType.CreateItem) == 0)
                return;
            var values = HttpUtility.ParseQueryString(request);
            foreach (var spec in CreateSpecList(json))
            {
                WriteNow("開発報告書",
                    string.Join(",", spec.Name, spec.TypeName,
                        values["api_item1"], values["api_item2"], values["api_item3"], values["api_item4"],
                        Secretary(), _basic.api_level),
                    "日付,開発装備,種別,燃料,弾薬,鋼材,ボーキ,秘書艦,司令部Lv");
            }
        }

        private IEnumerable<ItemSpec> CreateSpecList(dynamic json)
        {
            var fail = new ItemSpec
            {
                Name = "失敗",
                TypeName = ""
            };
            if (json.api_get_items())
            {
                return ((dynamic[])json.api_get_items).Select(entry =>
                    (int)entry.api_slotitem_id != -1 ? _itemInfo.GetSpecByItemId((int)entry.api_slotitem_id) : fail);
            }
            return new[]
            {
                json.api_slot_item()
                    ? _itemInfo.GetSpecByItemId((int)json.api_slot_item.api_slotitem_id)
                    : fail
            };
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
            WriteNow("建造報告書",
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

        private void WriteMaterialLog(DateTime now)
        {
            _prevTime = now;
            Write("資材ログ", now,
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
            if (!ships[1].Empty)
                ship2 = ships[1].Name + "(" + ships[1].Level + ")";
            WriteNow("改修報告書",
                string.Join(",", name, level, success, certain, useName, useNum,
                    diff[(int)Material.Fuel], diff[(int)Material.Bullet], diff[(int)Material.Steal],
                    diff[(int)Material.Bauxite],
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
        public LogIOException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}