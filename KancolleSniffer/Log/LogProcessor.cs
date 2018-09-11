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
using System.Globalization;
using System.IO;
using System.Linq;
using KancolleSniffer.Model;

namespace KancolleSniffer.Log
{
    public class LogProcessor
    {
        private readonly MaterialCount[] _materialCount;
        private readonly BattleLogProcessor _battleLogProcessor;

        public LogProcessor(MaterialCount[] materialCount = null)
        {
            _materialCount = materialCount ?? new MaterialCount[0];
            _battleLogProcessor = new BattleLogProcessor();
        }

        public IEnumerable<string> Process(IEnumerable<string> lines, string path, DateTime from, DateTime to,
            bool number, DateTime now = default)
        {
            var fields = 0;
            var battle = false;
            var material = false;
            switch (Path.GetFileNameWithoutExtension(path))
            {
                case "遠征報告書":
                    fields = 10;
                    break;
                case "改修報告書":
                    fields = 15;
                    break;
                case "海戦・ドロップ報告書":
                    fields = 39;
                    battle = true;
                    break;
                case "開発報告書":
                    fields = 9;
                    break;
                case "建造報告書":
                    fields = 12;
                    break;
                case "資材ログ":
                    fields = 9;
                    material = true;
                    break;
                case "戦果":
                    fields = 3;
                    break;
            }
            var delimiter = "";
            foreach (var line in lines)
            {
                var data = line.Split(',');
                var date = ParseDateTime(data[0]);
                if (date == default)
                    continue;
                if (to < date)
                    yield break;
                if (date < from)
                    continue;
                data[0] = Logger.FormatDateTime(date);
                var entries = data;
                if (material)
                    entries = data.Take(fields).ToArray();
                if (battle)
                    entries = _battleLogProcessor.Process(data);
                if (entries.Length != fields)
                    continue;
                var result =
                    number
                        ? delimiter + "[" + JavaScriptTicks(date) + "," + string.Join(",", entries.Skip(1)) + "]"
                        : delimiter + "[\"" + string.Join("\",\"", entries) + "\"]";
                delimiter = ",\n";
                yield return result;
            }
            if (material && !number) // 資材の現在値を出力する
                yield return delimiter + "[\"" + Logger.FormatDateTime(now) + "\",\"" +
                             string.Join("\",\"", _materialCount.Select(c => c.Now)) + "\"]";
        }

        private DateTime ParseDateTime(string dateTime)
        {
            if (DateTime.TryParseExact(dateTime, Logger.DateTimeFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal, out var date))
            {
                return date;
            }
            // システムが和暦に設定されていて和暦が出力されてしまったケースを救う
            if (dateTime[2] == '-')
            {
                if (!int.TryParse(dateTime.Substring(0, 2), out var year))
                    return default;
                dateTime = 1988 + year + dateTime.Substring(2);
                return DateTime.TryParseExact(dateTime, Logger.DateTimeFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out date)
                    ? date
                    : default;
            }
            return DateTime.TryParse(dateTime, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out date)
                ? date
                : default;
        }

        private long JavaScriptTicks(DateTime date) =>
            (date.ToUniversalTime().Ticks - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks) /
            TimeSpan.TicksPerMillisecond;
    }
}