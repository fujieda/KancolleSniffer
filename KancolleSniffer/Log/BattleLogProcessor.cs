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
using System.Text.RegularExpressions;
using KancolleSniffer.Model;

namespace KancolleSniffer.Log
{
    public class BattleLogProcessor : LogProcessor.Processor
    {
        private readonly Dictionary<string, string> _mapDictionary;

        public BattleLogProcessor(Dictionary<string, string> mapDictionary = null)
        {
            _mapDictionary = mapDictionary ?? new Dictionary<string, string>();
        }

        public override string[] Process(string[] data)
        {
            string map;
            switch (data.Length)
            {
                case 35:
                    data = data.Concat(Enumerable.Repeat("", 3)).ToArray();
                    goto case 38;
                case 38:
                    map = _mapDictionary.TryGetValue(data[1], out var num) ? num : "";
                    break;
                case 39:
                    map = data[38];
                    break;
                case 40: // 七隻分のログが出力されている
                    data[21] = data[21] + "・" + data[23];
                    data[22] = data[22] + "・" + data[24];
                    Array.Copy(data, 24, data, 23, 15);
                    goto case 38;
                default:
                    Skip = true;
                    return null;
            }
            Skip = false;
            if (data[5] == "Ｔ字戦(有利)")
                data[5] = "Ｔ字有利";
            if (data[5] == "Ｔ字戦(不利)")
                data[5] = "Ｔ字不利";
            if (data[6].EndsWith("航行序列"))
                data[6] = data[6].Substring(0, 4);
            if (data[7].EndsWith("航行序列"))
                data[7] = data[7].Substring(0, 4);
            data[37] = ShortenAirBattleResult(data[37]);
            var result = new string[41];
            var damage = GenerateDamagedShip(data);
            result[38] = damage[0];
            result[39] =  damage[1];
            result[40] = map;
            Array.Copy(data, result, 38);
            return result;
        }

        private static string ShortenAirBattleResult(string result)
        {
            switch (result)
            {
                case "制空均衡":
                    return "均衡";
                case "制空権確保":
                    return "確保";
                case "航空優勢":
                    return "優勢";
                case "航空劣勢":
                    return "劣勢";
                case "制空権喪失":
                    return "喪失";
                default:
                    return "";
            }
        }

        private static string[] GenerateDamagedShip(string[] data)
        {
            var badly = new List<string>();
            var half = new List<string>();
            for (var i = 11; i < 11 + 12; i += 2)
            {
                if (data[i] == "")
                    continue;
                var ship = data[i] = StripKana(data[i]);
                var hp = data[i + 1];
                try
                {
                    foreach (var entry in from entry in ship.Split('・').Zip(hp.Split('・'), (s, h) => new {s, h})
                        where entry.h.Contains("/")
                        let nm = entry.h.Split('/').Select(int.Parse).ToArray()
                        let level = ShipStatus.CalcDamage(nm[0], nm[1])
                        select new {level, name = entry.s})
                    {
                        if (entry.level == ShipStatus.Damage.Half)
                            half.Add(entry.name);
                        else if (entry.level == ShipStatus.Damage.Badly)
                            badly.Add(entry.name);
                    }
                }
                catch (FormatException)
                {
                    return new[] {"", ""};
                }
            }
            return new []{string.Join("・", badly), string.Join("・", half)};
        }

        private static readonly Regex Kana = new Regex(@"\([^)]+\)\(", RegexOptions.Compiled);

        private static string StripKana(string name)
        {
            return Kana.Replace(name, "(");
        }
    }
}