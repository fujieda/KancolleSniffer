// Copyright (C) 2014 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
// 
// This program is part of KancolleSniffer.
//
// KancolleSniffer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Codeplex.Data;
using ExpressionToCodeLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class SnifferTest
    {
        public static StreamReader OpenLogFile(string name)
        {
            var dir = Path.GetDirectoryName(Path.GetDirectoryName(Environment.CurrentDirectory));
// ReSharper disable once AssignNullToNotNullAttribute
            var path = Path.Combine(dir, Path.Combine("logs", name + ".log.gz"));
            return new StreamReader(new GZipStream(File.Open(path, FileMode.Open), CompressionMode.Decompress));
        }

        public static void SniffLogFile(Sniffer sniffer, string name)
        {
            var ln = 0;
            using (var stream = OpenLogFile(name))
            {
                while (!stream.EndOfStream)
                {
                    var triple = new List<string>();
                    foreach (var s in new[] {"url: ", "request: ", "response: "})
                    {
                        var line = stream.ReadLine();
                        ln++;
                        if (line == null)
                            throw new Exception(string.Format("ログのurl, request, responseがそろっていません: {0:d}行目", ln));
                        if (!line.StartsWith(s))
                            throw new Exception(string.Format("ログに不正な行が含まれています: {0:d}行目", ln));
                        triple.Add(line.Substring(s.Count()));
                    }
                    var json = DynamicJson.Parse(triple[2]);
                    sniffer.Sniff(triple[0], triple[1], json);
                }
            }
        }

        /// <summary>
        ///  演習で受けたダメージが次の戦闘の結果に影響しない
        /// </summary>
        [TestMethod]
        public void DamageInPracticeNotSpillIntoSortie()
        {
            var expected = new[] {31, 15, 15};
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "battle_001");
            var result = sniffer.GetShipStatuses(0).Select(s => s.NowHp);
            PAssert.That(() => (expected.SequenceEqual(result)));
        }

        /// <summary>
        /// 演習では大破警告を出さない
        /// </summary>
        [TestMethod]
        public void IgnoreDamagedShipsInPractice()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "practice_001");
            PAssert.That(() => !sniffer.Battle.HasDamagedShip);
        }

        /// <summary>
        /// 夜戦のダメージを戦闘結果に反映する
        /// </summary>
        [TestMethod]
        public void CaptureDamageInNightCombat()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "battle_002");
            AssertEqualBattleResult(sniffer, new[] {28, 1, 13});
            PAssert.That(()=> sniffer.Battle.HasDamagedShip);
        }

        private void AssertEqualBattleResult(Sniffer sniffer, IEnumerable<int> expected)
        {
            var result = sniffer.GetShipStatuses(0).Select(s => s.NowHp);
            PAssert.That(() => (expected.SequenceEqual(result)));
        }

        /// <summary>
        /// 連合艦隊(水上打撃部隊)による戦闘のダメージを結果に反映する
        /// </summary>
        [TestMethod]
        public void CombinedFleetSurface()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "combined_surface_001");
            AssertEauqlCombinedResult(sniffer, new[] {40, 77, 77, 33, 51, 47}, new[] {39, 35, 11, 39, 37, 40});
            PAssert.That(() => !sniffer.Battle.HasDamagedShip);

            SniffLogFile(sniffer, "combined_surface_002");
            AssertEauqlCombinedResult(sniffer, new[] {40, 77, 77, 33, 15, 6}, new[] {39, 35, 4, 3, 14, 40});
            PAssert.That(() => sniffer.Battle.HasDamagedShip);
        }

        private void AssertEauqlCombinedResult(Sniffer sniffer, IEnumerable<int> expected0, IEnumerable<int> expected1)
        {
            var result0 = sniffer.GetShipStatuses(0).Select(s => s.NowHp);
            var result1 = sniffer.GetShipStatuses(1).Select(s => s.NowHp);
            PAssert.That(() => (expected0.SequenceEqual(result0) && expected1.SequenceEqual(result1)));
        }

        /// <summary>
        /// 開幕夜戦のダメージを戦闘結果に反映する
        /// </summary>
        [TestMethod]
        public void SpMidnight()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "sp_midnight_001");
            AssertEqualBattleResult(sniffer, new[] {1});
            PAssert.That(() => sniffer.Battle.HasDamagedShip);
        }

        /// <summary>
        /// 連合艦隊(空母機動部隊)による戦闘のダメージを結果に反映する
        /// </summary>
        [TestMethod]
        public void CombinedFleetAir()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "combined_air_001");
            AssertEauqlCombinedResult(sniffer, new[] {40, 98, 90, 66, 78, 86}, new[] {47, 41, 5, 42, 43, 29});
            PAssert.That(() => sniffer.Battle.HasDamagedShip);

            SniffLogFile(sniffer, "combined_air_002");
            AssertEauqlCombinedResult(sniffer, new[] {13, 87, 90, 59, 69, 86}, new[] {47, 41, 5, 20, 43, 29});
            PAssert.That(() => !sniffer.Battle.HasDamagedShip, "退避しているので大破警告を出さない");
        }

        /// <summary>
        /// 夜戦の開始時に昼戦の結果を反映する
        /// </summary>
        [TestMethod]
        public void ResultOfDayBattleShowInNightBattle()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "battle_003");
            AssertEqualBattleResult(sniffer, new[] {28, 2, 13});
            PAssert.That(() => !sniffer.Battle.HasDamagedShip, "夜戦の開始時は大破警告を出さない");
        }

        /// <summary>
        /// 編成で空き番号を使ったローテートを正しく反映する
        /// </summary>
        [TestMethod]
        public void RotateFleetMember()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "deck_001");
            var result = sniffer.GetDeck(0);
            PAssert.That(() => new[] {756, 17204, 6156, 28806, 1069, -1}.SequenceEqual(result));
        }

        /// <summary>
        /// 編成で艦隊に配置ずみの艦娘を交換する
        /// </summary>
        [TestMethod]
        public void ExchangeFleetMember()
        {
            var sniffer = new Sniffer();

            SniffLogFile(sniffer, "deck_002");
            var result0 = sniffer.GetDeck(0);
            PAssert.That(() => new[] {1069, 6156, 756, 3223, -1, -1}.SequenceEqual(result0), "編成で艦隊内で艦娘と交換する");

            SniffLogFile(sniffer, "deck_003");
            var result10 = sniffer.GetDeck(0);
            var result11 = sniffer.GetDeck(1);
            PAssert.That(() => new[] {1069, 6156, 14258, 3223, -1, -1}.SequenceEqual(result10) &&
                               new[] {101, 4487, 756, 14613, 28806, -1}.SequenceEqual(result11), "002に続いて艦隊をまたがって交換する");

            SniffLogFile(sniffer, "deck_004");
            var result20 = sniffer.GetDeck(0);
            var result21 = sniffer.GetDeck(1);
            PAssert.That(() => new[] {1069, 6156, 14258, 3223, 756, -1}.SequenceEqual(result20) &&
                               new[] {101, 4487, 14613, 28806, -1, -1}.SequenceEqual(result21),
                "003に続いて空き番号にほかの艦隊の艦娘を配置する");
        }
    }
}