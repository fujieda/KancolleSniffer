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
        private StreamReader OpenLogFile(string path)
        {
            return new StreamReader(new GZipStream(File.Open(path, FileMode.Open), CompressionMode.Decompress));
        }

        private void SniffLogFile(Sniffer sniffer, string name)
        {
            var dir = Path.GetDirectoryName(Path.GetDirectoryName(Environment.CurrentDirectory));
            if (dir == null)
                return;
            var ln = 0;
            var stream = OpenLogFile(Path.Combine(dir, Path.Combine("logs", name + ".log.gz")));
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
            SniffLogFile(sniffer, "combined_surface_002");
            AssertEauqlCombinedResult(sniffer, new[] {40, 77, 77, 33, 15, 6}, new[] {39, 35, 4, 3, 14, 40});
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
            SniffLogFile(sniffer, "combined_air_002");
            AssertEauqlCombinedResult(sniffer, new[] {13, 87, 90, 59, 69, 86}, new[] {47, 41, 5, 20, 43, 29});
        }
    }
}