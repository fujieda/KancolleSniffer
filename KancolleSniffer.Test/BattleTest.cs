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

using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using KancolleSniffer.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    using Sniffer = SnifferTest.TestingSniffer;
    using static SnifferTest;

    [TestClass]
    public class BattleTest
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            ExpressionToCodeConfiguration.GlobalAssertionConfiguration = ExpressionToCodeConfiguration
                .GlobalAssertionConfiguration.WithPrintedListLengthLimit(200).WithMaximumValueLength(1000);
        }

        /// <summary>
        /// 4-2-1で開幕対潜雷撃を含む戦闘を行う
        /// </summary>
        [TestMethod]
        public void NormalBattleWithVariousTypesOfAttack()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "battle_001");
            PAssert.That(() => sniffer.Battle.ResultRank == BattleResultRank.A);
            AssertEqualBattleResult(sniffer,
                new[] {57, 66, 50, 65, 40, 42}, new[] {34, 5, 0, 0, 0, 0});
        }

        private void AssertEqualBattleResult(Sniffer sniffer, IEnumerable<int> expected, IEnumerable<int> enemy,
            string msg = null)
        {
            var result = sniffer.Fleets[0].Ships.Select(s => s.NowHp);
            PAssert.That(() => expected.SequenceEqual(result), msg);
            var enemyResult = sniffer.Battle.Result.Enemy.Main.Select(s => s.NowHp);
            PAssert.That(() => enemy.SequenceEqual(enemyResult), msg);
        }

        /// <summary>
        /// 開幕夜戦で潜水艦同士がお見合いする
        /// </summary>
        [TestMethod]
        public void SpMidnightWithoutBattle()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "sp_midnight_001");
            PAssert.That(() => sniffer.Battle.ResultRank == BattleResultRank.D);
        }

        /// <summary>
        /// 夜戦で戦艦が攻撃すると一回で三発分のデータが来る
        /// そのうち存在しない攻撃はターゲット、ダメージともに-1になる
        /// </summary>
        [TestMethod]
        public void BattleShipAttackInMidnight()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "midnight_001");
            PAssert.That(() => sniffer.Battle.ResultRank == BattleResultRank.S);
        }

        /// <summary>
        /// 7隻編成の戦闘で7隻目が攻撃される
        /// </summary>
        [TestMethod]
        public void Ship7Battle()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "ship7battle_001");
            PAssert.That(() => sniffer.Battle.ResultRank == BattleResultRank.P);
        }

        /// <summary>
        /// 演習のあとのportで戦闘結果の検証を行わない
        /// </summary>
        [TestMethod]
        public void NotVerifyBattleResultAfterPractice()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "practice_001");
            PAssert.That(() => !sniffer.IsBattleResultError);
        }

        /// <summary>
        /// 演習でダメコンを発動させない
        /// </summary>
        [TestMethod]
        public void NotTriggerDameConInPractice()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "practice_002");
            PAssert.That(() => !sniffer.IsBattleResultError);
        }

        /// <summary>
        /// 演習中の艦を要修復リストに載せない
        /// </summary>
        [TestMethod]
        public void DamagedShipListNotShowShipInPractice()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "practice_003");
            PAssert.That(() => sniffer.RepairList.Select(s => s.Name).SequenceEqual(new[] {"飛龍改二", "翔鶴改二"}));
        }

        /// <summary>
        /// 連合艦隊が開幕雷撃で被弾する
        /// </summary>
        [TestMethod]
        public void OpeningTorpedoInCombinedBattle()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "combined_battle_001");
            PAssert.That(() => !sniffer.IsBattleResultError);
        }

        /// <summary>
        /// 連合艦隊が閉幕雷撃で被弾する
        /// </summary>
        [TestMethod]
        public void ClosingTorpedoInCombinedBattle()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "combined_battle_002");
            PAssert.That(() => !sniffer.IsBattleResultError);
        }

        /// <summary>
        /// 第一が6隻未満の連合艦隊で戦闘する
        /// </summary>
        [TestMethod]
        public void SmallCombinedFleetBattle()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "combined_battle_003");
            PAssert.That(() => !sniffer.IsBattleResultError);
        }

        /// <summary>
        /// 護衛退避する
        /// </summary>
        [TestMethod]
        public void EscapeWithEscort()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "escape_001");
            var fleets = sniffer.Fleets;
            PAssert.That(() => fleets[0].Ships[5].Escaped &&
                               fleets[1].Ships[2].Escaped);
        }

        /// <summary>
        /// 開幕夜戦に支援が来る
        /// </summary>
        [TestMethod]
        public void SpMidnightSupportAttack()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "sp_midnight_002");
            PAssert.That(() => !sniffer.IsBattleResultError);
            PAssert.That(() => sniffer.Battle.SupportType == 2);
        }

        /// <summary>
        /// 払暁戦を行う
        /// </summary>
        [TestMethod]
        public void NightToDay()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "nighttoday_001");
            PAssert.That(() => !sniffer.IsBattleResultError);
        }

        /// <summary>
        /// 第二期の開幕夜戦のセル情報を表示する
        /// </summary>
        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void SpMidnightIn2ndSequence()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "sp_midnight_003");
            PAssert.That(() => sniffer.CellInfo.Current == "１戦目(夜戦)");
        }

        /// <summary>
        /// 単艦退避する
        /// </summary>
        [TestMethod]
        public void EscapeWithoutEscort()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "escape_002");
            PAssert.That(() => sniffer.Fleets[2].Ships[1].Escaped);
            PAssert.That(() => !sniffer.IsBattleResultError);
        }

        /// <summary>
        /// 出撃時に大破している艦娘がいたら警告する
        /// </summary>
        [TestMethod]
        public void DamagedShipWarningOnMapStart()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "mapstart_001");
            PAssert.That(() => sniffer.BadlyDamagedShips.SequenceEqual(new[] {"大潮"}));
        }

        /// <summary>
        /// 連合艦隊に大破艦がいる状態で第3艦隊が出撃したときに警告しない
        /// </summary>
        [TestMethod]
        public void NotWarnDamagedShipInCombinedFleetOnMapStart()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "mapstart_002");
            PAssert.That(() => !sniffer.BadlyDamagedShips.Any());
        }

        /// <summary>
        /// 連合艦隊の第二旗艦の大破を警告しない
        /// </summary>
        [TestMethod]
        public void NotWarnDamaged1StShipInGuardFleet()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "combined_battle_004");
            PAssert.That(() => !sniffer.BadlyDamagedShips.Any());
        }

        /// <summary>
        /// Nelson Touchに対応する
        /// </summary>
        [TestMethod]
        public void NelsonTouch()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "nelsontouch_001");
            PAssert.That(() => !sniffer.IsBattleResultError);
            PAssert.That(() => sniffer.Battle.Result.Friend.Main[0].SpecialAttack == ShipStatus.Attack.Fire);
            PAssert.That(() => sniffer.Fleets[0].Ships[0].SpecialAttack == ShipStatus.Attack.Fired);
            // ship_deckでフラグを引き継ぐ
            SniffLogFile(sniffer, "nelsontouch_002");
            PAssert.That(() => sniffer.Fleets[0].Ships[0].SpecialAttack == ShipStatus.Attack.Fired);
            // 夜戦
            var night = new Sniffer();
            SniffLogFile(night, "nelsontouch_003");
            PAssert.That(() => night.Battle.Result.Friend.Main[0].SpecialAttack == ShipStatus.Attack.Fire);
        }

        /// <summary>
        /// 長門改二の一斉射に対応する
        /// </summary>
        [TestMethod]
        // ReSharper disable once IdentifierTypo
        public void NagatoSpecial()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "nagatospecial_001");
            PAssert.That(() => !sniffer.IsBattleResultError);
            PAssert.That(() => sniffer.Battle.Result.Friend.Main[0].SpecialAttack == ShipStatus.Attack.Fire);
            PAssert.That(() => sniffer.Fleets[0].Ships[0].SpecialAttack == ShipStatus.Attack.Fired);
        }

        /// <summary>
        /// 陸奥改二の一斉射に対応する
        /// </summary>
        [TestMethod]
        // ReSharper disable once IdentifierTypo
        public void MutsuSpecial()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "mutsuspecial_001");
            PAssert.That(() => !sniffer.IsBattleResultError);
            PAssert.That(() => sniffer.Battle.Result.Friend.Main[0].SpecialAttack == ShipStatus.Attack.Fire);
            PAssert.That(() => sniffer.Fleets[0].Ships[0].SpecialAttack == ShipStatus.Attack.Fired);
        }

        /// <summary>
        /// レーダー射撃戦に対応する
        /// </summary>
        [TestMethod]
        public void LdShooting()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "ld_shooting_001");
            PAssert.That(() => !sniffer.IsBattleResultError);
        }

        /// <summary>
        /// 敵連合艦隊の護衛の装備を正しく読み取る
        /// </summary>
        [TestMethod]
        public void EnemyGuardSlot()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "enemy_combined_001");
            PAssert.That(() => sniffer.Battle.Result.Enemy.Guard[0].Slot[0].Spec.Id == 506);
        }

        /// <summary>
        /// 敵艦隊の制空値を計算する
        /// </summary>
        [TestMethod]
        public void EnemyFighterPower()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "enemy_combined_001");
            var fp = sniffer.Battle.EnemyFighterPower;
            PAssert.That(() => fp.AirCombat == 209 && fp.Interception == 212);
        }

        /// <summary>
        /// ダメコン進撃する
        /// </summary>
        [TestMethod]
        public void NotWarnDamagedShipWithDamageControl()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "damecon_advance_001");
            PAssert.That(() => sniffer.BadlyDamagedShips.Length == 0);
        }

        /// <summary>
        /// 迎撃機ありの基地空襲戦
        /// </summary>
        [TestMethod]
        public void AirRaidBattleWithInterceptor()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "airraid_battle_001");
            var battle = sniffer.Battle;
            Assert.AreEqual(BattleState.AirRaid, battle.BattleState);
            Assert.AreEqual(2, battle.AirControlLevel);
            Assert.AreEqual(425, battle.FighterPower.Min);
            Assert.AreEqual(231, battle.EnemyFighterPower.AirCombat);
            Assert.AreEqual(BattleResultRank.S, battle.ResultRank);
            var ships = battle.Result.Friend.Main;
            Assert.IsTrue(new[] {200, 200, 200}.SequenceEqual(ships.Select(ship => ship.NowHp)));
            Assert.IsTrue(new[] {"基地航空隊1", "基地航空隊2", "基地航空隊3"}.SequenceEqual(ships.Select(ship => ship.Name)));
            Assert.IsTrue(
                new[] {"烈風改(三五二空/熟練)", "雷電", "雷電", "烈風改"}.SequenceEqual(ships[2].Slot.Select(item => item.Spec.Name)));
            Assert.IsTrue(new[]{18, 18, 18, 18}.SequenceEqual(ships[2].OnSlot));
        }

        /// <summary>
        /// 迎撃機なしの基地航空戦
        /// </summary>
        [TestMethod]
        public void AirRaidBattleWithoutInterceptor()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "airraid_battle_002");
            var battle = sniffer.Battle;
            Assert.AreEqual(BattleState.AirRaid, battle.BattleState);
            Assert.AreEqual(4, battle.AirControlLevel);
            Assert.AreEqual(0, battle.FighterPower.Min);
            Assert.AreEqual(231, battle.EnemyFighterPower.AirCombat);
            Assert.AreEqual(BattleResultRank.B, battle.ResultRank);
            var ships = battle.Result.Friend.Main;
            Assert.IsTrue(new[] {82, 174, 147}.SequenceEqual(ships.Select(ship => ship.NowHp)));
        }
    }
}