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

        public LogProcessor(MaterialCount[] materialCount = null, Dictionary<string, string> mapDictionary = null)
        {
            _materialCount = materialCount ?? new MaterialCount[0];
            _battleLogProcessor = new BattleLogProcessor(mapDictionary);
        }

        public class Processor
        {
            protected virtual int Fields { get; }
            public bool Skip { get; protected set; }

            public Processor()
            {
            }

            public Processor(int fields)
            {
                Fields = fields;
            }

            public virtual string[] Process(string[] data)
            {
                Skip = data.Length != Fields;
                return Skip ? null : data;
            }
        }

        private class MissionProcessor : Processor
        {
            protected override int Fields { get; } = 11;

            public override string[] Process(string[] data)
            {
                return data.Concat(new[] {"0"}).Take(Fields).ToArray();
            }
        }

        private class MaterialProcessor : Processor
        {
            protected override int Fields { get; } = 9;

            public override string[] Process(string[] data)
            {
                if (data.Length >= Fields)
                    Array.Resize(ref data, Fields);
                return base.Process(data);
            }
        }

        public IEnumerable<string> Process(IEnumerable<string> lines, string path, DateTime from, DateTime to,
            bool number, DateTime now = default)
        {
            var logName = Path.GetFileNameWithoutExtension(path);
            var currentMaterial = logName == "資材ログ" && !number;
            var processor = DecideProcessor(logName);
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
                var entries = processor.Process(data);
                if (processor.Skip)
                    continue;
                var result =
                    number
                        ? delimiter + "[" + JavaScriptTicks(date) + "," + string.Join(",", entries.Skip(1)) + "]"
                        : delimiter + "[\"" + string.Join("\",\"", entries) + "\"]";
                delimiter = ",\n";
                yield return result;
            }
            if (currentMaterial) // 資材の現在値を出力する
                yield return delimiter + "[\"" + Logger.FormatDateTime(now) + "\",\"" +
                             string.Join("\",\"", _materialCount.Select(c => c.Now)) + "\"]";
        }

        private Processor DecideProcessor(string logName)
        {
            switch (logName)
            {
                case "遠征報告書":
                    return new MissionProcessor();
                case "改修報告書":
                    return new Processor(15);
                case "海戦・ドロップ報告書":
                    return _battleLogProcessor;
                case "開発報告書":
                    return new Processor(9);
                case "建造報告書":
                    return new Processor(12);
                case "資材ログ":
                    return new MaterialProcessor();
                case "戦果":
                    return new Processor(3);
                default:
                    return new Processor();
            }
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