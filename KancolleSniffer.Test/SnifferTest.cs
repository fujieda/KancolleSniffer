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
using KancolleSniffer.Model;
using KancolleSniffer.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static System.Math;

namespace KancolleSniffer.Test
{
    using Sniffer = SnifferTest.TestingSniffer;

    [TestClass]
    public class SnifferTest
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            ExpressionToCodeConfiguration.GlobalAssertionConfiguration = ExpressionToCodeConfiguration
                .GlobalAssertionConfiguration.WithPrintedListLengthLimit(200).WithMaximumValueLength(1000);
        }

        public class TestingSniffer : KancolleSniffer.Sniffer
        {
            public TestingSniffer(bool start = false) : base(start)
            {
                AdditionalData.UseNumEquipsFile = false;
            }
        }

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
        /// 熟練度込みの制空値を正しく計算する
        /// </summary>
        [TestMethod]
        public void FighterPowerWithBonus()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "fighterpower_001");
            var fleet = sniffer.Fleets[0];
            PAssert.That(() => fleet.FighterPower.SequenceEqual(new[] {156, 159}));
            SniffLogFile(sniffer, "fighterpower_002");
            PAssert.That(() => fleet.FighterPower.SequenceEqual(new[] {140, 143}), "全滅したスロットがある");
        }

        /// <summary>
        /// 改修効果込みの制空値を正しく計算する
        /// </summary>
        [TestMethod]
        public void FighterPowerWithImprovement()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "fighterpower_003");
            PAssert.That(() => sniffer.Fleets[0].FighterPower.SequenceEqual(new[] {135, 135}));
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
        /// 基地航空隊の出撃コストを計算する
        /// </summary>
        [TestMethod]
        public void CostForSortieOfBaseAirCorps()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "baseaircorps_003");
            PAssert.That(() => sniffer.BaseAirCorps[1].AirCorps[0].CostForSortie.SequenceEqual(new[] {99, 47}));
        }

        /// <summary>
        /// 陸上戦闘機の改修レベルを制空値に反映させる。
        /// </summary>
        [TestMethod]
        public void FighterPowerOfLandBAseFighter()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "baseaircorps_004");
            PAssert.That(() => sniffer.BaseAirCorps[1].AirCorps[0].FighterPower.AirCombat[0] == 328);
        }

        /// <summary>
        /// マップ索敵の判定式(33)を正しく計算する
        /// </summary>
        [TestMethod]
        public void LineOfSight()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "lineofsight_001");
            var fleet = sniffer.Fleets[0];
            PAssert.That(() => Abs(fleet.GetLineOfSights(1) - 39.45) < 0.01);
            PAssert.That(() => Abs(fleet.GetLineOfSights(3) - 115.19) < 0.01);
            PAssert.That(() => Abs(fleet.GetLineOfSights(4) - 153.06) < 0.01);
            SniffLogFile(sniffer, "lineofsight_002");
            PAssert.That(() => Abs(fleet.GetLineOfSights(1) - -25.10) < 0.01, "艦隊に空きがある");
        }

        /// <summary>
        /// 触接開始率を正しく計算する
        /// </summary>
        [TestMethod]
        public void ContactTriggerRage()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "contact_001");
            PAssert.That(() => Abs(sniffer.Fleets[0].ContactTriggerRate - 1.8182) < 0.0001);
        }

        /// <summary>
        /// TPを計算する
        /// </summary>
        [TestMethod]
        public void TransportPoint()
        {
            var sniffer = new Sniffer();
            var msgs = new[] {"", "鬼怒改二+特大発+おにぎり", "駆逐艦+士魂部隊", "補給艦"};
            var results = new[] {47, 19, 13, 15};
            for (var i = 0; i < msgs.Length; i++)
            {
                SniffLogFile(sniffer, "transportpoint_00" + (i + 1));
                var j = i;
                PAssert.That(() => (int)sniffer.Fleets[0].TransportPoint == results[j], msgs[j]);
            }
        }

        /// <summary>
        /// 対空砲火のパラメータを計算する
        /// </summary>
        [TestMethod]
        public void AntiAirFire()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "antiairfire_001");
            var ships = sniffer.Fleets[0].Ships;
            PAssert.That(() => ships.Sum(ship => ship.EffectiveAntiAirForFleet) == 88);
            PAssert.That(
                () =>
                    ships.Select(ship => ship.EffectiveAntiAirForShip)
                        .SequenceEqual(new[] {92, 90, 88, 228, 146, 226}));
        }

        /// <summary>
        /// 空母の夜戦火力を計算する
        /// </summary>
        [TestMethod]
        public void CvNightBattlePower()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "nightbattlepower_001");
            var ships = sniffer.Fleets[0].ActualShips;
            PAssert.That(() =>
                ships.Select(ship => (int)(ship.NightBattlePower * 100))
                    .SequenceEqual(new[] {11202, 14985, 20092, 17354}));
            // 夜間作戦航空要員を外す
            ships[0].FreeSlot(3);
            ships[1].FreeSlot(2);
            ships[3].FreeSlot(2);
            PAssert.That(() =>
                ships.Select(ship => (int)(ship.NightBattlePower * 100))
                    .SequenceEqual(new[] {6900, 7500, 20092, 0}));
            // Ark RoyalからSwordfishを外す
            ships[0].FreeSlot(0);
            PAssert.That(() => (int)ships[0].NightBattlePower == 0);
        }

        /// <summary>
        /// 副砲の改修レベルの効果を計算する
        /// </summary>
        [TestMethod]
        public void SecondaryGunFirepowerLevelBonus()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "firepower_001");
            var ships = sniffer.Fleets[0].Ships;
            // ReSharper disable CompareOfFloatsByEqualityOperator
            PAssert.That(() => ships[0].EffectiveFirepower == 93.5);
            PAssert.That(() => ships[1].EffectiveFirepower == 82.5);
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        /// 連合艦隊補正の載った火力を計算する
        /// </summary>
        [TestMethod]
        public void CombinedFleetFirepower()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "combined_status_001");
            // ReSharper disable CompareOfFloatsByEqualityOperator
            PAssert.That(() => sniffer.Fleets[0].Ships[0].EffectiveFirepower == 117.0);
            PAssert.That(() => sniffer.Fleets[1].Ships[0].EffectiveFirepower == 72.0);
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        /// 対潜攻撃力を計算する
        /// </summary>
        [TestMethod]
        public void AntiSubmarine()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "antisubmarine_001");
            PAssert.That(() => Abs(sniffer.Fleets[0].Ships[0].EffectiveAntiSubmarine - 92.16) < 0.01);
            PAssert.That(() => Abs(sniffer.Fleets[0].Ships[1].EffectiveAntiSubmarine - 84.49) < 0.01);
            PAssert.That(() => Abs(sniffer.Fleets[0].Ships[2].EffectiveAntiSubmarine - 57.84) < 0.01);
            PAssert.That(() => Abs(sniffer.Fleets[0].Ships[3].EffectiveAntiSubmarine - 61.37) < 0.01);
        }

        /// <summary>
        /// 編成で空き番号を使ったローテートを正しく反映する
        /// </summary>
        [TestMethod]
        public void RotateFleetMember()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "deck_001");
            var result = sniffer.Fleets[0].Deck;
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
            var result = sniffer.Fleets[0].Deck;
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
            var result0 = sniffer.Fleets[0].Deck;
            PAssert.That(() => new[] {1069, 6156, 756, 3223, -1, -1}.SequenceEqual(result0), "編成で艦隊内で艦娘と交換する");

            SniffLogFile(sniffer, "deck_003");
            var result10 = sniffer.Fleets[0].Deck;
            var result11 = sniffer.Fleets[1].Deck;
            PAssert.That(() => new[] {1069, 6156, 14258, 3223, -1, -1}.SequenceEqual(result10) &&
                               new[] {101, 4487, 756, 14613, 28806, -1}.SequenceEqual(result11), "002に続いて艦隊をまたがって交換する");

            SniffLogFile(sniffer, "deck_004");
            var result20 = sniffer.Fleets[0].Deck;
            var result21 = sniffer.Fleets[1].Deck;
            PAssert.That(() => new[] {1069, 6156, 14258, 3223, 756, -1}.SequenceEqual(result20) &&
                               new[] {101, 4487, 14613, 28806, -1, -1}.SequenceEqual(result21),
                "003に続いて空き番号にほかの艦隊の艦娘を配置する");
        }

        /// <summary>
        /// 随伴艦一括解除を実行する
        /// </summary>
        [TestMethod]
        public void WithdrawAccompanyingShipsAtOnce()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "deck_006");
            var result = sniffer.Fleets[0].Deck;
            PAssert.That(() => new[] {135, -1, -1, -1, -1, -1}.SequenceEqual(result));
        }

        /// <summary>
        /// 編成展開を正しく反映する
        /// </summary>
        [TestMethod]
        public void PresetSelect()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "preset_001");
            var result = sniffer.Fleets[0].Deck;
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
            var result = sniffer.Fleets[0].Ships[0].Slot.Select(item => item.Id);
            PAssert.That(() => new[] {26096, 30571, 77694, 61383, -1}.SequenceEqual(result));
        }

        /// <summary>
        /// 近代化改修の結果をすぐに反映する
        /// </summary>
        [TestMethod]
        public void PowerUpResult()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "powerup_001");
            PAssert.That(() => Abs(sniffer.Fleets[0].Ships[0].EffectiveFirepower - 30) < 0.0001);
        }

        /// <summary>
        /// 近代化改修が二重に行われた場合に対応する
        /// </summary>
        [TestMethod]
        public void DuplicatedPowerUp()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "powerup_002");
            PAssert.That(() => sniffer.ShipCounter.Now == 218);
        }

        /// <summary>
        /// ship2を待たずにケッコンの結果を反映する
        /// </summary>
        [TestMethod]
        public void MarriageResult()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "marriage_001");
            PAssert.That(() => sniffer.Fleets[0].Ships[2].Level == 100);
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
            PAssert.That(() => sniffer1.ItemCounter.Now == 900);
            var sniffer2 = new Sniffer();
            SniffLogFile(sniffer2, "createship_001");
            PAssert.That(() => sniffer2.ItemCounter.Now == 904);
        }

        /// <summary>
        /// 装備数の超過を警告する
        /// </summary>
        [TestMethod]
        public void WarnItemCount()
        {
            Action<int> func = i => { };
            var sniffer1 = new Sniffer();
            SniffLogFile(sniffer1, "item_count_001");
            func.Invoke(sniffer1.ItemCounter.Now); // Nowを読まないとAlarmが立たない
            PAssert.That(() => sniffer1.ItemCounter.Alarm, "出撃から母港に戻ったとき");
            var sniffer2 = new Sniffer();
            SniffLogFile(sniffer2, "item_count_002");
            func.Invoke(sniffer2.ItemCounter.Now);
            PAssert.That(() => sniffer2.ItemCounter.Alarm, "ログインしたとき");
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
            PAssert.That(() => sniffer.ShipCounter.Now == 250);
            PAssert.That(() => sniffer.ItemCounter.Now == 1118);
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
            PAssert.That(() => expected.SequenceEqual(sniffer.Fleets.Select(f => f.ChargeStatus)));
        }

        /// <summary>
        /// ship2がリクエストで指定した艦娘のデータしか返さない
        /// </summary>
        [TestMethod]
        public void Ship2ReturnShipSpecifiedByRequest()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "ship2_001");
            PAssert.That(() => sniffer.ShipCounter.Now == 243);
        }

        /// <summary>
        /// 出撃中にアイテムを取得する
        /// </summary>
        [TestMethod]
        public void ItemGetInSortie()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "itemget_001");
            PAssert.That(() => sniffer.MiscText ==
                               "[獲得アイテム]\r\n" +
                               "燃料: 1115\r\n" +
                               "弾薬: 25\r\n" +
                               "鋼材: 70\r\n" +
                               "家具箱（大）: 1\r\n" +
                               "給糧艦「間宮」: 1\r\n" +
                               "勲章: 1\r\n" +
                               "給糧艦「伊良湖」: 3\r\n" +
                               "プレゼント箱: 1\r\n" +
                               "補強増設: 2\r\n" +
                               "戦闘詳報: 1\r\n" +
                               "瑞雲(六三一空): 1\r\n" +
                               "夜間作戦航空要員: 1\r\n" +
                               "130mm B-13連装砲: 1\r\n" +
                               "潜水空母な桐箪笥: 1\r\n" +
                               "Gambier Bay: 1");
        }

        /// <summary>
        /// 出撃直後に資源を獲得する
        /// </summary>
        [TestMethod]
        public void ItemGetAtStart()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "itemget_002");
            PAssert.That(() => sniffer.MiscText ==
                               "[獲得アイテム]\r\n" +
                               "燃料: 65");
        }

        /// <summary>
        /// 航空偵察でアイテムを取得する
        /// </summary>
        [TestMethod]
        public void ItemGetInAirRecon()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "airrecon_001");
            PAssert.That(() =>
                sniffer.MiscText ==
                "[海域ゲージ情報]\r\n 海域選択画面に進むと表示します。\r\n" +
                "[演習情報]\r\n 演習相手を選ぶと表示します。\r\n" +
                "[獲得アイテム]\r\n 帰投したときに表示します。", "失敗の場合");

            SniffLogFile(sniffer, "airrecon_002");
            PAssert.That(() =>
                sniffer.MiscText == "[獲得アイテム]\r\n弾薬: 150\r\n開発資材: 1", "成功");

            SniffLogFile(sniffer, "airrecon_003");
            PAssert.That(() =>
                sniffer.MiscText == "[獲得アイテム]\r\n弾薬: 150\r\n開発資材: 1", "途中でリロードして再出撃");
        }

        /// <summary>
        /// 海域ゲージの情報を生成する
        /// </summary>
        [TestMethod]
        public void AreaGauge()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "mapgauge_001");
            PAssert.That(() =>
                sniffer.MiscText ==
                               "[海域ゲージ]\r\n" +
                               "1-6 : 残り 5/7\r\n" +
                               "2-5 : 残り 4/4\r\n" +
                               "3-5 : 残り 4/4\r\n" +
                               "4-4 : 残り 4/4\r\n");
        }

        /// <summary>
        /// 演習の獲得経験値を計算する
        /// </summary>
        [TestMethod]
        public void PracticeExpPoint()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "practice_004");
            PAssert.That(() => sniffer.MiscText == "[演習情報]\r\n敵艦隊名 : 第一艦隊\r\n獲得経験値 : 878\r\nS勝利 : 1053");
        }

        /// <summary>
        /// 新規のドロップ艦の初期装備数を登録する
        /// </summary>
        [TestMethod]
        public void RecordNumEquipsOfNewDropShip()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "dropship_001");
            PAssert.That(() => sniffer.AdditionalData.NumEquips(565) == 2);
            PAssert.That(() => sniffer.ShipList.First(s => s.Spec.Id == 565).Spec.NumEquips == 2);
        }

        /// <summary>
        /// 既知のドロップ艦とその装備をカウントする
        /// </summary>
        [TestMethod]
        public void CountDropShip()
        {
            var sniffer = new Sniffer();
            sniffer.AdditionalData.RecordNumEquips(11, "", 1);
            SniffLogFile(sniffer, "dropship_002");
            PAssert.That(() => sniffer.ShipCounter.Now == 250);
            PAssert.That(() => sniffer.ItemCounter.Now == 1159);
        }

        /// <summary>
        /// 艦娘数を数える
        /// </summary>
        [TestMethod]
        public void CountShips()
        {
            var sniffer = new Sniffer();
            SniffLogFile(sniffer, "ship_count_001");
            PAssert.That(() => sniffer.ShipCounter.Now == 267 && sniffer.ShipCounter.Alarm, "ログイン");
            SniffLogFile(sniffer, "ship_count_002");
            PAssert.That(() => sniffer.ShipCounter.Now == 266 && sniffer.ShipCounter.Alarm, "建造");
            SniffLogFile(sniffer, "ship_count_003");
            PAssert.That(() => sniffer.ShipCounter.Now == 266 && sniffer.ShipCounter.Alarm, "ドロップ");
        }
    }
}