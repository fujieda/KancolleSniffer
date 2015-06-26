// Copyright (C) 2014 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

        private void AssertEqualBattleResult(Sniffer sniffer, IEnumerable<int> expected, string msg = null)
        {
            var result = sniffer.GetShipStatuses(0).Select(s => s.NowHp);
            PAssert.That(() => (expected.SequenceEqual(result)), msg);
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

        private void AssertEauqlCombinedResult(Sniffer sniffer, IEnumerable<int> expected0, IEnumerable<int> expected1, string msg = null)
        {
            var result0 = sniffer.GetShipStatuses(0).Select(s => s.NowHp);
            var result1 = sniffer.GetShipStatuses(1).Select(s => s.NowHp);
            PAssert.That(() => (expected0.SequenceEqual(result0) && expected1.SequenceEqual(result1)), msg);
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
        }

        /// <summary>
        /// 護衛退避を正しく処理する
        /// </summary>
        [TestMethod]
        public void EscapeShip()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "combined_escape_001");
            AssertEauqlCombinedResult(sniffer, new[]{37, 105, 106, 90, 66, 10}, new[]{41, 41, 37, 44, 43, 43}, "連合艦隊で2戦して大破が出るまで");
            PAssert.That(() => sniffer.Battle.HasDamagedShip);
            SniffLogFile(sniffer, "combined_escape_002");
            PAssert.That(() => sniffer.GetShipStatuses(0)[5].Escaped && sniffer.GetShipStatuses(1)[1].Escaped, "続けて護衛退避を実行");
            PAssert.That(() => !sniffer.Battle.HasDamagedShip);
            SniffLogFile(sniffer, "combined_escape_003");
            AssertEauqlCombinedResult(sniffer, new[] {37, 105, 106, 90, 1, 10}, new[] {41, 41, 32, 44, 43, 43}, "もう一戦して大破が出るまで");
            PAssert.That(() => sniffer.Battle.HasDamagedShip);
            SniffLogFile(sniffer, "combined_escape_004");
            PAssert.That(() => sniffer.GetShipStatuses(0)[5].Escaped && sniffer.GetShipStatuses(1)[1].Escaped &&
                sniffer.GetShipStatuses(0)[4].Escaped && sniffer.GetShipStatuses(1)[2].Escaped, "続けて護衛退避を実行");
            PAssert.That(() => !sniffer.Battle.HasDamagedShip);
        }

        /// <summary>
        /// 護衛退避した空母を除いた制空値を計算する
        /// </summary>
        [TestMethod]
        public void FighterPowerWithoutEscapedShip()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "combined_escape_001");
            PAssert.That(() => sniffer.GetFighterPower(0) == 187);
            SniffLogFile(sniffer, "combined_escape_002");
            PAssert.That(() => sniffer.GetFighterPower(0) == 65);
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
        /// 通常艦隊の航空戦のダメージを結果に反映する
        /// </summary>
        [TestMethod]
        public void AirBattle()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "airbattle_001");
            AssertEqualBattleResult(sniffer, new[]{37, 36, 31, 37, 17, 63}, "夜戦あり");

            sniffer = new Sniffer();
            SniffLogFile(sniffer, "airbattle_002");
            AssertEqualBattleResult(sniffer, new[] { 66, 36, 16, 27, 35, 38 }, "昼戦のみ");
        }

        /// <summary>
        /// 支援射撃による敵の損傷を勝利判定に反映させる
        /// </summary>
        [TestMethod]
        public void SupportShellingChangeResultRank()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "support_001");
            PAssert.That(()=> sniffer.Battle.ResultRank == BattleResultRank.A);
        }

        /// <summary>
        /// ダメコンの使用を戦闘結果に反映させる
        /// </summary>
        [TestMethod]
        public void DamageControl()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "damecon_001");
            AssertEqualBattleResult(sniffer, new[] {30, 1, 3}, "戦闘前");
            PAssert.That(() => sniffer.GetShipStatuses(0)[1].Slot.SequenceEqual(new[] {2, 4593, -1, -1, -1}), "ダメコンを二つ装備");
            PAssert.That(() => sniffer.Battle.ResultRank == BattleResultRank.S, "判定はS勝利");
            SniffLogFile(sniffer, "damecon_002");
            AssertEqualBattleResult(sniffer, new[] {30, 1, 3}, "戦闘後");
            PAssert.That(() => sniffer.GetShipStatuses(0)[1].Slot.SequenceEqual(new[] {-1, 4593, -1, -1, -1}), "ダメコンを一つ消費");
        }

        /// <summary>
        /// 連合艦隊(水上打撃部隊)で二回目の砲撃戦がない場合を正しく処理する
        /// </summary>
        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void CombinedFleetSurfaceWithout2ndShelling()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "combined_surface_003");
            PAssert.That(() => sniffer.Battle.ResultRank == BattleResultRank.P);
        }

        /// <summary>
        /// ship2に代わるship_deckを処理する
        /// </summary>
        [TestMethod]
        public void ShipDeck()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "ship_deck_001");
            PAssert.That(() => sniffer.GetShipStatuses(0)[0].Fuel == 36);
        }

        /// <summary>
        /// 夜戦かどうかを選択する画面でリロードしても結果を次の戦闘に持ち越さない
        /// </summary>
        [TestMethod]
        public void ReloadBeforeBattleResult()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "reload_001");
            PAssert.That(() => sniffer.GetShipStatuses(0)[0].NowHp == 41);
        }

        /// <summary>
        /// 航空戦のない演習を正しく処理する
        /// </summary>
        [TestMethod]
        public void PracticeWithoutAirBattle()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "practice_002");
            PAssert.That(() => sniffer.Battle.AirControlLevel == -1);
        }

        /// <summary>
        /// 2-5をクリアしたときの特別戦果を反映する
        /// </summary>
        [TestMethod]
        public void ExMapBattleResult()
        {
            var sniffer0 = new Sniffer();
            sniffer0.ExMap.ClearClearStatus();
            SniffLogFile(sniffer0, "eo_001");
            PAssert.That(() => sniffer0.ExMap.Achievement == 100, "ほかのマップの情報なし");
            var sniffer1 = new Sniffer();
            SniffLogFile(sniffer1, "eo_001");
            // すでに3-5をクリアしているので合計で250
            PAssert.That(() => sniffer1.ExMap.Achievement == 250, "ほかのマップの情報あり");
        }

        /// <summary>
        /// 1-6をクリアしたときの特別戦果を反映する
        /// </summary>
        [TestMethod]
        public void ExMapMapNext()
        {
            var sniffer0 = new Sniffer();
            sniffer0.ExMap.ClearClearStatus();
            SniffLogFile(sniffer0, "eo_002");
            PAssert.That(() => sniffer0.ExMap.Achievement == 75, "ほかのマップの情報なし");
            var sniffer1 = new Sniffer();
            SniffLogFile(sniffer1, "eo_002");
            // 5-5以外クリアしているので合計で400
            PAssert.That(()=> sniffer1.ExMap.Achievement == 400, "ほかのマップの情報あり");
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

        /// <summary>
        /// 改修による資材の減少をすぐに反映する
        /// </summary>
        [TestMethod]
        public void ConsumptionByRemodelSlot()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "remodel_slot_001");
            PAssert.That(() => sniffer.Item.MaterialHistory.Select(m => m.Now)
                .SequenceEqual(new[] {25292, 25570, 25244, 41113, 1405, 1525, 2137, 8}));
        }
    }
}