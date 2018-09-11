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
using ExpressionToCodeLib;
using KancolleSniffer.Log;
using KancolleSniffer.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class LogProcessorTest
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            ExpressionToCodeConfiguration.GlobalAssertionConfiguration = ExpressionToCodeConfiguration
                .GlobalAssertionConfiguration.WithPrintedListLengthLimit(200).WithMaximumValueLength(1000);
        }

        /// <summary>
        /// 遠征報告書は普通にJSONに変換する
        /// </summary>
        [TestMethod]
        public void EnseiLog()
        {
            var processor = new LogProcessor();
            var log = new[]
            {
                "2018-09-10 17:45:09,大成功,北方鼠輸送作戦,589,498,0,0,0,0,0",
                "2018-09-10 17:53:34,成功,長距離練習航海,0,117,34,0,0,1,0"
            };
            var result = processor.Process(log, "遠征報告書.csv", DateTime.MinValue, DateTime.MaxValue, false);
            PAssert.That(() => result.SequenceEqual(new[]
            {
                "[\"2018-09-10 17:45:09\",\"大成功\",\"北方鼠輸送作戦\",\"589\",\"498\",\"0\",\"0\",\"0\",\"0\",\"0\"]",
                ",\n[\"2018-09-10 17:53:34\",\"成功\",\"長距離練習航海\",\"0\",\"117\",\"34\",\"0\",\"0\",\"1\",\"0\"]"
            }));
        }

        /// <summary>
        /// 資材ログは最後に現在値を示すレコードを追加する
        /// </summary>
        [TestMethod]
        public void MaterialLogWithCurrentRecord()
        {
            var processor = new LogProcessor(new[]
            {
                new MaterialCount {Now = 2001}, new MaterialCount {Now = 2002}, new MaterialCount {Now = 2003},
                new MaterialCount {Now = 2004},
                new MaterialCount {Now = 201}, new MaterialCount {Now = 202}, new MaterialCount {Now = 203},
                new MaterialCount {Now = 100}
            });
            var now = new DateTime(2018, 1, 1);
            var log = "2018-09-10 20:36:34,294892,296784,259518,294588,2484,2975,2550,3";

            var result = processor.Process(new[] {log}, "資材ログ.csv", DateTime.MinValue, DateTime.MaxValue, false, now)
                .ToArray();
            PAssert.That(() =>
                result[0] ==
                "[\"2018-09-10 20:36:34\",\"294892\",\"296784\",\"259518\",\"294588\",\"2484\",\"2975\",\"2550\",\"3\"]");
            var date = Logger.FormatDateTime(now);
            PAssert.That(() =>
                result[1] ==
                $",\n[\"{date}\",\"2001\",\"2002\",\"2003\",\"2004\",\"201\",\"202\",\"203\",\"100\"]", "現在値");
        }

        /// <summary>
        /// 資材グラフの描画を高速化するために値をすべて数値にする
        /// </summary>
        [TestMethod]
        public void MaterialLogInNumberType()
        {
            var processor = new LogProcessor();
            var log = "2018-09-10 20:36:34,294892,296784,259518,294588,2484,2975,2550,3";

            var result = processor.Process(new[] {log}, "資材ログ.csv", DateTime.MinValue, DateTime.MaxValue, true)
                .ToArray();
            PAssert.That(() => result.Length == 1, "現在値の出力なし");
            PAssert.That(() => result[0] == "[1536579394000,294892,296784,259518,294588,2484,2975,2550,3]");
        }

        /// <summary>
        /// 指定された範囲のログを出力する
        /// </summary>
        [TestMethod]
        public void Range()
        {
            var log = new[]
            {
                "2018-09-09 23:58:35,66023314,0",
                "2018-09-10 08:08:31,66023314,0",
                "2018-09-10 11:03:01,66024154,0"
            };
            var processor = new LogProcessor();
            Func<DateTime, DateTime, IEnumerable<string>> rangeProcessor =
                (from, to) => processor.Process(log, "戦果.csv", from, to, false);

            PAssert.That(
                () => rangeProcessor(DateTime.MinValue, DateTime.MaxValue).SequenceEqual(new[]
                {
                    "[\"2018-09-09 23:58:35\",\"66023314\",\"0\"]",
                    ",\n[\"2018-09-10 08:08:31\",\"66023314\",\"0\"]",
                    ",\n[\"2018-09-10 11:03:01\",\"66024154\",\"0\"]"
                }));
            PAssert.That(
                () => rangeProcessor(DateTime.MinValue, new DateTime(2018, 9, 10)).SequenceEqual(new[]
                    {"[\"2018-09-09 23:58:35\",\"66023314\",\"0\"]"}
                ));
            PAssert.That(
                () => rangeProcessor(new DateTime(2018, 9, 10), DateTime.MaxValue).SequenceEqual(new[]
                {
                    "[\"2018-09-10 08:08:31\",\"66023314\",\"0\"]",
                    ",\n[\"2018-09-10 11:03:01\",\"66024154\",\"0\"]"
                }));
            PAssert.That(
                () => rangeProcessor(new DateTime(2018, 9, 10), new DateTime(2018, 9, 10, 11, 0, 0)).SequenceEqual(new[]
                {
                    "[\"2018-09-10 08:08:31\",\"66023314\",\"0\"]"
                }));
        }

        /// <summary>
        /// 想定と異なる日付フォーマットに対応する。
        /// </summary>
        [TestMethod]
        public void DateFormat()
        {
            var processor = new LogProcessor();

            var body = ",大型艦建造,まるゆ,潜水艦,1500,1500,2000,1000,1,0,瑞鶴改二甲(163),120";
            var expected =
                ",\"大型艦建造\",\"まるゆ\",\"潜水艦\",\"1500\",\"1500\",\"2000\",\"1000\",\"1\",\"0\",\"瑞鶴改二甲(163)\",\"120\"]";

            Func<string, string> dateProcessor =
                date => processor.Process(new[] {date + body}, "建造報告書", DateTime.MinValue, DateTime.MaxValue, false)
                    .First();
            var era = "30-09-10 20:13:39";
            PAssert.That(() => "[\"2018-09-10 20:13:39\"" + expected == dateProcessor(era), "和暦を西暦に直す");
            var excel = "2018/9/10 20:13";
            PAssert.That(() => "[\"2018-09-10 20:13:00\"" + expected == dateProcessor(excel), "Excelの形式から変換する");
        }
    }
}