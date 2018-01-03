// Copyright (C) 2014 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.IO;
using System.IO.Compression;
using System.Linq;
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

        public static void SniffLogFile(Sniffer sniffer, string name, Action<Sniffer> action = null)
        {
            var ln = 0;
            using (var stream = OpenLogFile(name))
            {
                while (!stream.EndOfStream)
                {
                    var triple = new List<string>();
                    foreach (var s in new[] {"url: ", "request: ", "response: "})
                    {
                        string line;
                        do
                        {
                            line = stream.ReadLine();
                            ln++;
                            if (line == null)
                                throw new Exception($"ログの内容がそろっていません: {ln:d}行目");
                        } while (!line.StartsWith(s));
                        triple.Add(line.Substring(s.Length));
                    }
                    var json = JsonParser.Parse(triple[2]);
                    sniffer.Sniff(triple[0], triple[1], json);
                    action?.Invoke(sniffer);
                }
            }
        }

        /// <summary>
        /// 4-2-1で開幕対潜雷撃を含む戦闘を行う
        /// </summary>
        [TestMethod]
        public void NormalBattleWithVriousTypesOfAttack()
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
            var result = sniffer.GetShipStatuses(0).Select(s => s.NowHp);
            PAssert.That(() => expected.SequenceEqual(result), msg);
            var enemyResult = sniffer.Battle.EnemyResultStatus.Select(s => s.NowHp);
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
            PAssert.That(() => !sniffer.IsBattleResultStatusError);
        }

        /// <summary>
        /// 連合艦隊が開幕雷撃で被弾する
        /// </summary>
        [TestMethod]
        public void OpeningTorpedoInCombinedBattle()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "combined_battle_001");
            PAssert.That(() => !sniffer.IsBattleResultStatusError);
        }

        /// <summary>
        /// 連合艦隊が閉幕雷撃で被弾する
        /// </summary>
        [TestMethod]
        public void ClosingTorpedoInCombinedBattle()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "combined_battle_002");
            PAssert.That(() => !sniffer.IsBattleResultStatusError);
        }

        /// <summary>
        /// 第一が6隻未満の連合艦隊で戦闘する
        /// </summary>
        [TestMethod]
        public void SmallCombinedFleetBattle()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "combined_battle_003");
            PAssert.That(() => !sniffer.IsBattleResultStatusError);
        }

        /// <summary>
        /// 護衛退避する
        /// </summary>
        [TestMethod]
        public void EscapeWithEscort()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "escape_001");
            PAssert.That(() => sniffer.GetShipStatuses(0)[5].Escaped &&
                               sniffer.GetShipStatuses(1)[2].Escaped);
        }

        /// <summary>
        /// 開幕夜戦に支援が来る
        /// </summary>
        [TestMethod]
        public void SpMidnightSupportAttack()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "sp_midnight_002");
            PAssert.That(() => !sniffer.Battle.DisplayedResultRank.IsError);
        }

        /// <summary>
        /// 払暁戦を行う
        /// </summary>
        [TestMethod]
        public void NightToDay()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "nighttoday_001");
            PAssert.That(() => !sniffer.Battle.DisplayedResultRank.IsError && !sniffer.IsBattleResultStatusError);
        }

        /// <summary>
        /// 単艦退避する
        /// </summary>
        [TestMethod]
        public void EscapeWithoutEscort()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "escape_002");
            PAssert.That(() => sniffer.GetShipStatuses(2)[1].Escaped);
            PAssert.That(() => !sniffer.IsBattleResultStatusError);
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
        /// 熟練度込みの制空値を正しく計算する
        /// </summary>
        [TestMethod]
        public void FighterPowerWithBonus()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "fighterpower_001");
            PAssert.That(() => sniffer.GetFighterPower(0).SequenceEqual(new[] {156, 159}));
            SniffLogFile(sniffer, "fighterpower_002");
            PAssert.That(() => sniffer.GetFighterPower(0).SequenceEqual(new[] {140, 143}), "全滅したスロットがある");
        }

        /// <summary>
        /// 改修効果込みの制空値を正しく計算する
        /// </summary>
        [TestMethod]
        public void FighterPowerWithImprovement()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "fighterpower_003");
            PAssert.That(() => sniffer.GetFighterPower(0).SequenceEqual(new[] {135, 135}));
        }

        /// <summary>
        /// 基地航空隊の制空値を正しく計算する
        /// </summary>
        [TestMethod]
        public void FighterPowerOfBaseAirCorps()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "baseaircorps_001");
            PAssert.That(() => sniffer.BaseAirCorps[0].AirCorps[0].FighterPower.AirCombat[1] == 301);
            sniffer.BaseAirCorps[0].AirCorps[0].Action = 2; // 防空
            PAssert.That(() => sniffer.BaseAirCorps[0].AirCorps[0].FighterPower.Interception[1] == 320);
        }

        /// <summary>
        /// 基地航空隊の防空時の偵察機補正を含む制空値を計算する
        /// </summary>
        [TestMethod]
        public void FighterPowerWithReconBonus()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "baseaircorps_002");
            PAssert.That(() => sniffer.BaseAirCorps[0].AirCorps[2].FighterPower.Interception[0] == 353);
        }

        /// <summary>
        /// 陸上攻撃機の熟練度を制空値に反映させる
        /// </summary>
        [TestMethod]
        public void FighterPowerOfLandBasedAttackAircraft()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "baseaircorps_003");
            PAssert.That(() => sniffer.BaseAirCorps[1].AirCorps[0].FighterPower.AirCombat[0] == 121);
        }

        /// <summary>
        /// マップ索敵の判定式(33)を正しく計算する
        /// </summary>
        [TestMethod]
        public void LineOfSight()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "lineofsight_001");
            PAssert.That(() => Math.Abs(sniffer.GetFleetLineOfSights(0, 1) - 39.45) < 0.01);
            PAssert.That(() => Math.Abs(sniffer.GetFleetLineOfSights(0, 3) - 115.19) < 0.01);
            PAssert.That(() => Math.Abs(sniffer.GetFleetLineOfSights(0, 4) - 153.06) < 0.01);
            SniffLogFile(sniffer, "lineofsight_002");
            PAssert.That(() => Math.Abs(sniffer.GetFleetLineOfSights(0, 1) - -25.10) < 0.01, "艦隊に空きがある");
        }

        /// <summary>
        /// 触接開始率を正しく計算する
        /// </summary>
        [TestMethod]
        public void ContactTriggerRage()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "contact_001");
            PAssert.That(() => Math.Abs(sniffer.GetContactTriggerRate(0) - 1.8182) < 0.0001);
        }

        /// <summary>
        /// TPを正しく計算する
        /// </summary>
        [TestMethod]
        public void TransportPoint()
        {
            var sniffer1 = new Sniffer();
            SniffLogFile(sniffer1, "transportpoint_001");
            ItemSpec.IncreaceLandPowerTp = false;
            PAssert.That(() => (int)sniffer1.GetShipStatuses(0).Sum(s => s.TransportPoint) == 27);
            ItemSpec.IncreaceLandPowerTp = true;
            PAssert.That(() => (int)sniffer1.GetShipStatuses(0).Sum(s => s.TransportPoint) == 37, "陸上戦力揚陸時");

            var sniffer2 = new Sniffer();
            SniffLogFile(sniffer2, "transportpoint_002");
            PAssert.That(() => (int)sniffer2.GetShipStatuses(0).Sum(s => s.TransportPoint) == 19, "鬼怒改二+特大発+おにぎり");

            var sniffer3 = new Sniffer();
            SniffLogFile(sniffer3, "transportpoint_003");
            PAssert.That(() => (int)sniffer3.GetShipStatuses(0).Sum(s => s.TransportPoint) == 13, "駆逐艦+士魂部隊");
        }

        /// <summary>
        /// 対空砲火のパラメータを計算する
        /// </summary>
        [TestMethod]
        public void AntiAirFire()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "antiairfire_001");
            var ships = sniffer.GetShipStatuses(0);
            PAssert.That(() => ships.Sum(ship => ship.EffectiveAntiAirForFleet) == 88);
            PAssert.That(
                () =>
                    ships.Select(ship => ship.EffectiveAntiAirForShip)
                        .SequenceEqual(new[] {92, 90, 88, 228, 146, 226}));
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
        /// ドラッグ＆ドロップで離れた空き番号を使って編成をローテートする
        /// </summary>
        [TestMethod]
        public void RotateFleetMemberWithDragAndDrop()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "deck_005");
            var result = sniffer.GetDeck(0);
            PAssert.That(() => new[] {57391, 50, 24475, 113, -1, -1}.SequenceEqual(result));
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
        /// 編成展開を正しく反映する
        /// </summary>
        [TestMethod]
        public void PresetSelect()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "preset_001");
            var result = sniffer.GetDeck(0);
            PAssert.That(() => new[] {50510, 632, 39843, 113, 478, 47422}.SequenceEqual(result));
        }

        /// <summary>
        /// 拡張した編成記録枠にすぐに記録してもエラーにならない
        /// </summary>
        [TestMethod]
        public void PresetExpand()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "preset_002");
        }

        /// <summary>
        /// 装備の交換を正しく反映する
        /// </summary>
        [TestMethod]
        public void SlotExchange()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "slot_exchange_001");
            var result = sniffer.GetShipStatuses(0)[0].Slot.Select(item => item.Id);
            PAssert.That(() => new[] {26096, 30571, 77694, 61383, -1}.SequenceEqual(result));
        }

        /// <summary>
        /// 近代化改修の結果をすぐに反映する
        /// </summary>
        [TestMethod]
        public void PowerupResult()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "powerup_001");
            PAssert.That(() => Math.Abs(sniffer.GetShipStatuses(0)[0].EffectiveFirepower - 30) < 0.0001);
        }

        /// <summary>
        /// 近代化改修が二重に行われた場合に対応する
        /// </summary>
        [TestMethod]
        public void DuplicatedPowerup()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "powerup_002");
            PAssert.That(() => sniffer.Item.NowShips == 218);
        }

        /// <summary>
        /// 改修による資材の減少をすぐに反映する
        /// </summary>
        [TestMethod]
        public void ConsumptionByRemodelSlot()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "remodel_slot_001");
            PAssert.That(() => sniffer.Material.Current
                .SequenceEqual(new[] {25292, 25570, 25244, 41113, 1405, 1525, 2137, 8}));
        }

        /// <summary>
        /// 装備の数を正しく数える
        /// </summary>
        [TestMethod]
        public void CountItem()
        {
            var sniffer1 = new Sniffer();
            SniffLogFile(sniffer1, "createitem_001");
            PAssert.That(() => sniffer1.Item.NowEquips == 900);
            var sniffer2 = new Sniffer();
            SniffLogFile(sniffer2, "createship_001");
            PAssert.That(() => sniffer2.Item.NowEquips == 904);
        }

        /// <summary>
        /// 装備数の超過を警告する
        /// </summary>
        [TestMethod]
        public void WarnItemCount()
        {
            var sniffer1 = new Sniffer();
            SniffLogFile(sniffer1, "item_count_001");
            PAssert.That(() => sniffer1.Item.AlarmEquips, "出撃から母港に戻ったとき");
            var sniffer2 = new Sniffer();
            SniffLogFile(sniffer2, "item_count_002");
            PAssert.That(() => sniffer2.Item.AlarmEquips, "ログインしたとき");
        }

        /// <summary>
        /// 装備の所持者を設定する
        /// </summary>
        [TestMethod]
        public void SetItemHolder()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "itemholder_001");
            var itemIds = new HashSet<int> {75298, 76572, 82725, 90213, 5910};
            var items = sniffer.ItemList.Where(status => itemIds.Contains(status.Id));
            PAssert.That(() => items.All(x => x.Holder.Id == 861));
        }

        /// <summary>
        /// 資材の変動を正しく反映する
        /// </summary>
        [TestMethod]
        public void MaterialChanges()
        {
            var sniffer1 = new Sniffer();
            var result1 = new List<int[]>();
            SniffLogFile(sniffer1, "material_001", sn =>
            {
                var cur = sn.Material.Current;
                if (result1.Count == 0)
                {
                    result1.Add(cur);
                }
                else
                {
                    if (!result1.Last().SequenceEqual(cur))
                        result1.Add(cur);
                }
            });
            var expected1 = new List<int[]>
            {
                new[] {0, 0, 0, 0, 0, 0, 0, 0},
                new[] {26178, 26742, 21196, 33750, 1426, 1574, 2185, 10},
                new[] {26178, 26842, 21226, 33750, 1426, 1574, 2185, 10},
                new[] {28951, 29493, 24945, 35580, 1426, 1574, 2185, 10},
                new[] {26074, 26616, 21068, 33700, 1426, 1572, 2183, 10},
                new[] {26171, 26721, 21175, 33750, 1426, 1574, 2185, 10},
                new[] {27023, 27829, 28136, 42404, 1404, 1521, 2142, 15},
                new[] {31208, 29819, 29714, 42345, 1407, 1530, 2155, 13},
                new[] {24595, 25353, 18900, 32025, 1427, 1576, 2187, 10},
                new[] {24515, 25353, 18749, 32025, 1427, 1575, 2187, 10},
                new[] {23463, 24964, 17284, 31765, 1427, 1572, 2187, 10},
                new[] {23463, 25064, 17314, 31765, 1427, 1572, 2187, 10}
            };
            PAssert.That(() => SequenceOfSequenceEqual(expected1, result1));

            var sniffer2 = new Sniffer();
            var result2 = new List<int[]>();
            SniffLogFile(sniffer2, "material_002", sn =>
            {
                var cur = sn.Material.Current;
                if (result2.Count == 0)
                {
                    result2.Add(cur);
                }
                else
                {
                    if (!result2.Last().SequenceEqual(cur))
                        result2.Add(cur);
                }
            });
            var expected2 = new List<int[]>
            {
                new[] {0, 0, 0, 0, 0, 0, 0, 0},
                new[] {201649, 189713, 261490, 123227, 2743, 2828, 3000, 44},
                new[] {201649, 189714, 261491, 123227, 2743, 2828, 3000, 44},
                new[] {201650, 189718, 261500, 123227, 2743, 2828, 3000, 44}
            };
            PAssert.That(() => SequenceOfSequenceEqual(expected2, result2));
        }

        /// <summary>
        /// 基地航空隊における資材の変動を反映する
        /// </summary>
        [TestMethod]
        public void MaterialChangesInAirCorps()
        {
            var sniffer3 = new Sniffer();
            var result3 = new List<int[]>();
            SniffLogFile(sniffer3, "material_003", sn =>
            {
                var cur = sn.Material.Current;
                if (result3.Count == 0)
                {
                    result3.Add(cur);
                }
                else
                {
                    if (!result3.Last().SequenceEqual(cur))
                        result3.Add(cur);
                }
            });
            var expected3 = new List<int[]>
            {
                new[] {0, 0, 0, 0, 0, 0, 0, 0},
                new[] {288194, 282623, 299496, 295958, 3000, 2968, 2997, 7},
                new[] {288185, 282623, 299496, 295943, 3000, 2968, 2997, 7},
                new[] {288161, 282623, 299496, 295903, 3000, 2968, 2997, 7}
            };
            PAssert.That(() => SequenceOfSequenceEqual(expected3, result3), "航空機の補充");

            var sniffer4 = new Sniffer();
            var result4 = new List<int[]>();
            SniffLogFile(sniffer4, "material_004", sn =>
            {
                var cur = sn.Material.Current;
                if (result4.Count == 0)
                {
                    result4.Add(cur);
                }
                else
                {
                    if (!result4.Last().SequenceEqual(cur))
                        result4.Add(cur);
                }
            });
            var expected4 = new List<int[]>
            {
                new[] {0, 0, 0, 0, 0, 0, 0, 0},
                new[] {261012, 252252, 298492, 279622, 3000, 2842, 3000, 22},
                new[] {261012, 252252, 298492, 279538, 3000, 2842, 3000, 22},
                new[] {261012, 252252, 298492, 279454, 3000, 2842, 3000, 22}
            };
            PAssert.That(() => SequenceOfSequenceEqual(expected4, result4), "航空機の配備");
        }

        private bool SequenceOfSequenceEqual<T>(IEnumerable<IEnumerable<T>> a, IEnumerable<IEnumerable<T>> b)
        {
            var aa = a.ToArray();
            var bb = b.ToArray();
            if (aa.Length != bb.Length)
                return false;
            return aa.Zip(bb, (x, y) => x.SequenceEqual(y)).All(x => x);
        }

        /// <summary>
        /// 修復時間が1分以内の艦娘が入渠する
        /// </summary>
        [TestMethod]
        public void NyukyoLessThanOrEqualTo1Min()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "nyukyo_001");
            PAssert.That(() => sniffer.RepairList.Length == 1);
        }

        /// <summary>
        /// 一括解体する(装備保管なしとあり)
        /// </summary>
        [TestMethod]
        public void DestroyShip()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "destroyship_001");
            PAssert.That(() => sniffer.Item.NowShips == 250);
            PAssert.That(() => sniffer.Item.NowEquips == 1118);
            PAssert.That(() => sniffer.Material.Current.Take(4).SequenceEqual(new[] {285615, 286250, 291010, 284744}));
        }

        /// <summary>
        /// 第2艦隊までしか解放していなくてもエラーにならないようにする
        /// </summary>
        [TestMethod]
        public void TwoFleets()
        {
            var sniffer = new Sniffer(true);
            SniffLogFile(sniffer, "twofleets_001");
            var expected = Enumerable.Repeat(new ChargeStatus(5, 5), ShipInfo.FleetCount);
            PAssert.That(() => expected.SequenceEqual(sniffer.ChargeStatuses));
        }

        /// <summary>
        /// ship2がリクエストで指定した艦娘のデータしか返さない
        /// </summary>
        [TestMethod]
        public void Ship2ReturnShipSpecifiedByRequest()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "ship2_001");
            PAssert.That(() => sniffer.Item.NowShips == 243);
        }
    }
}