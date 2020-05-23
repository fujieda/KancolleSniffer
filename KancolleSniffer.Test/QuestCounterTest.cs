// Copyright (C) 2019 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Linq;
using ExpressionToCodeLib;
using KancolleSniffer.Model;
using KancolleSniffer.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class QuestCountTest
    {
        [TestMethod]
        public void AdjustCount()
        {
            var count = new QuestCount
            {
                Spec = new QuestSpec {Max = 7},
                Now = 3
            };
            count.AdjustCount(0);
            Assert.AreEqual(3, count.Now);
            count.AdjustCount(50);
            Assert.AreEqual(4, count.Now);
            count.AdjustCount(80);
            Assert.AreEqual(6, count.Now);
            count.AdjustCount(100);
            Assert.AreEqual(7, count.Now);
            count.Now = 14;
            count.AdjustCount(100);
            Assert.AreEqual(14, count.Now);
            count.AdjustCount(80);
            Assert.AreEqual(6, count.Now);
            count.AdjustCount(50);
            Assert.AreEqual(5, count.Now);
            count.AdjustCount(0);
            Assert.AreEqual(3, count.Now);
        }

        [TestMethod]
        public void AdjustCountWithShift()
        {
            var count = new QuestCount
            {
                Spec = new QuestSpec {Max = 7, Shift = 1},
                Now = 3
            };
            count.AdjustCount(0);
            Assert.AreEqual(2, count.Now);
            count.AdjustCount(50);
            Assert.AreEqual(3, count.Now);
            count.AdjustCount(80);
            Assert.AreEqual(6, count.Now);
            count.AdjustCount(100);
            Assert.AreEqual(7, count.Now);
            count.Now = 14;
            count.AdjustCount(100);
            Assert.AreEqual(14, count.Now);
            count.AdjustCount(80);
            Assert.AreEqual(6, count.Now);
            count.AdjustCount(50);
            Assert.AreEqual(5, count.Now);
            count.AdjustCount(0);
            Assert.AreEqual(2, count.Now);
        }

        [TestMethod]
        public void AdjustCountMax3WithShift2()
        {
            var count = new QuestCount
            {
                Spec = new QuestSpec {Max = 3, Shift = 2},
                Now = 0
            };
            count.AdjustCount(0);
            Assert.AreEqual(0, count.Now);
            count.AdjustCount(50);
            Assert.AreEqual(1, count.Now);
            count.AdjustCount(80);
            Assert.AreEqual(2, count.Now);
            count.AdjustCount(100);
            Assert.AreEqual(3, count.Now);
            count.Now = 4;
            count.AdjustCount(100);
            Assert.AreEqual(4, count.Now);
            count.AdjustCount(80);
            Assert.AreEqual(2, count.Now);
            count.AdjustCount(50);
            Assert.AreEqual(1, count.Now);
            count.AdjustCount(0);
            Assert.AreEqual(0, count.Now);
        }

        [TestMethod]
        public void AdjustCount80Percent()
        {
            var count = new QuestCount
            {
                Spec = new QuestSpec()
            };
            for (var shift = 0; shift <= 1; shift++)
            {
                for (var max = 2; max <= 6; max++)
                {
                    count.Spec.Max = max;
                    count.Spec.Shift = shift;
                    count.Now = 1;
                    count.AdjustCount(80);
                    Assert.AreEqual(count.Spec.Max - 1, count.Now);
                }
            }
        }

        [TestMethod]
        public void AdjustCountNowArray()
        {
            var count = new QuestCount
            {
                Spec = new QuestSpec {MaxArray = new[] {36, 6, 24, 12}},
                NowArray = new[] {1, 2, 3, 4}
            };
            count.AdjustCount(50);
            Assert.IsTrue(count.NowArray.SequenceEqual(new[] {1, 2, 3, 4}));
            count.AdjustCount(100);
            Assert.IsTrue(count.NowArray.SequenceEqual(new[] {36, 6, 24, 12}));
            count.NowArray = new[] {38, 12, 19, 12};
            count.AdjustCount(100);
            Assert.IsTrue(count.NowArray.SequenceEqual(new[] {38, 12, 24, 12}));
        }

        /// <summary>
        /// カウンターを文字列表記にする
        /// </summary>
        [TestMethod]
        public void ToStringTest()
        {
            var status = new Status
            {
                QuestCountList = new[]
                {
                    new QuestCount {Id = 211, Now = 2},
                    new QuestCount {Id = 214, NowArray = new[] {20, 7, 10, 8}},
                    new QuestCount {Id = 854, NowArray = new[] {2, 1, 1, 1}},
                    new QuestCount {Id = 426, NowArray = new[] {1, 1, 1, 1}},
                    new QuestCount {Id = 428, NowArray = new[] {1, 1, 1}},
                    new QuestCount {Id = 873, NowArray = new[] {1, 1, 1}},
                    new QuestCount {Id = 888, NowArray = new[] {1, 1, 1}},
                    new QuestCount {Id = 688, NowArray = new[] {2, 1, 2, 1}},
                    new QuestCount {Id = 893, NowArray = new[] {1, 1, 1, 1}},
                    new QuestCount {Id = 894, NowArray = new[] {1, 1, 1, 1, 1}},
                    new QuestCount {Id = 280, NowArray = new[] {1, 1, 1, 1}},
                    new QuestCount {Id = 872, NowArray = new[] {1, 1, 1, 1}},
                    new QuestCount {Id = 284, NowArray = new[] {1, 1, 1, 1}},
                    new QuestCount {Id = 226, Now = 2},
                    new QuestCount {Id = 436, NowArray = new[] {1, 0, 1, 1, 1}},
                }
            };
            new QuestInfo().LoadState(status);
            Assert.AreEqual("2/3", status.QuestCountList[0].ToString());
            Assert.AreEqual("20/36 7/6 10/24 8/12", status.QuestCountList[1].ToString());
            var z = status.QuestCountList[2];
            Assert.AreEqual("2\u200a1\u200a1\u200a1", z.ToString());
            Assert.AreEqual("2-4:2 6-1:1 6-3:1 6-4:1", z.ToToolTip());
            z.NowArray = new[] {0, 0, 0, 0};
            Assert.AreEqual("2-4:0 6-1:0 6-3:0 6-4:0", z.ToToolTip());
            var q426 = status.QuestCountList[3];
            Assert.AreEqual("1\u200a1\u200a1\u200a1", q426.ToString());
            Assert.AreEqual("警備任務1 対潜警戒任務1 海上護衛任務1 強硬偵察任務1", q426.ToToolTip());
            var q428 = status.QuestCountList[4];
            Assert.AreEqual("対潜警戒任務1 海峡警備行動1 長時間対潜警戒1", q428.ToToolTip());
            q428.NowArray = new[] {0, 1, 0};
            Assert.AreEqual("対潜警戒任務0 海峡警備行動1 長時間対潜警戒0", q428.ToToolTip());
            var q873 = status.QuestCountList[5];
            Assert.AreEqual("1\u200a1\u200a1", q873.ToString());
            Assert.AreEqual("3-1:1 3-2:1 3-3:1", q873.ToToolTip());
            var q888 = status.QuestCountList[6];
            Assert.AreEqual("1\u200a1\u200a1", q888.ToString());
            Assert.AreEqual("5-1:1 5-3:1 5-4:1", q888.ToToolTip());
            var q688 = status.QuestCountList[7];
            Assert.AreEqual("艦戦2 艦爆1 艦攻2 水偵1", q688.ToToolTip());
            var q893 = status.QuestCountList[8];
            Assert.AreEqual("1-5:1 7-1:1 7-2G:1 7-2M:1", q893.ToToolTip());
            var q894 = status.QuestCountList[9];
            Assert.AreEqual("1\u200a1\u200a1\u200a1\u200a1", q894.ToString());
            Assert.AreEqual("1-3:1 1-4:1 2-1:1 2-2:1 2-3:1", q894.ToToolTip());
            var q280 = status.QuestCountList[10];
            Assert.AreEqual("1\u200a1\u200a1\u200a1", q280.ToString());
            Assert.AreEqual("1-2:1 1-3:1 1-4:1 2-1:1", q280.ToToolTip());
            var q872 = status.QuestCountList.First(q => q.Id == 872);
            Assert.AreEqual("1\u200a1\u200a1\u200a1", q872.ToString());
            Assert.AreEqual("7-2M:1 5-5:1 6-2:1 6-5:1", q872.ToToolTip());
            var q284 = status.QuestCountList.First(q => q.Id == 284);
            Assert.AreEqual("1\u200a1\u200a1\u200a1", q284.ToString());
            Assert.AreEqual("1-4:1 2-1:1 2-2:1 2-3:1", q284.ToToolTip());
            var q226 = status.QuestCountList.First(q => q.Id == 226);
            Assert.AreEqual("2/5", q226.ToString());
            Assert.AreEqual("", q226.ToToolTip());
            var q436 = status.QuestCountList.First(q => q.Id == 436);
            Assert.AreEqual("1\u200a0\u200a1\u200a1\u200a1", q436.ToString());
            Assert.AreEqual("練習航海1 長距離練習航海0 警備任務1 対潜警戒任務1 強行偵察任務1", q436.ToToolTip());
        }
    }

    [TestClass]
    public class QuestCounterTest
    {
        private JsonObject Js(object obj) => JsonObject.CreateJsonObject(obj);

        private object CreateQuestList(int[] ids) => Js(new
        {
            api_list =
                ids.Select(id => new
                {
                    api_no = id,
                    api_category = id / 100,
                    api_type = 1,
                    api_state = 2,
                    api_title = "",
                    api_detail = "",
                    api_get_material = new int[0],
                    api_progress_flag = 0
                })
        });

        private QuestCount InjectQuest(int id)
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {id}));
            return _questInfo.Quests[0].Count;
        }

        private void InjectMapStart(int map, int eventId)
        {
            _questCounter.InspectMapStart(CreateMap(map, eventId));
        }

        private void InjectMapNext(int map, int eventId)
        {
            _questCounter.InspectMapNext(CreateMap(map, eventId));
        }

        private object CreateMap(int map, int eventId)
        {
            return Js(new
            {
                api_maparea_id = map / 10,
                api_mapinfo_no = map % 10,
                api_event_id = eventId
            });
        }

        private void InjectBattleResult(string result)
        {
            _questCounter.InspectBattleResult(Js(new {api_win_rank = result}));
        }

        private void InjectPracticeResult(string result)
        {
            _questCounter.InspectPracticeResult(Js(new {api_win_rank = result}));
        }

        private ShipStatus[] ShipStatusList(params int[] shipTypes)
        {
            return shipTypes.Select(sType => ShipStatus(sType)).ToArray();
        }

        private ShipStatus ShipStatus(int shipType, int specId = 0)
        {
            return new ShipStatus {NowHp = 1, Spec = new ShipSpec {Id = specId, ShipType = shipType}};
        }

        private ShipStatus ShipStatus(int shipType, int shipClass, int specId)
        {
            return new ShipStatus
                {NowHp = 1, Spec = new ShipSpec {Id = specId, ShipType = shipType, ShipClass = shipClass}};
        }

        private ShipStatus ShipStatus(string name)
        {
            return new ShipStatus {NowHp = 1, Spec = new ShipSpec {Name = name}};
        }


        private BattleInfo _battleInfo;
        private ItemInfo _itemInfo;
        private QuestInfo _questInfo;
        private QuestCounter _questCounter;

        [TestInitialize]
        public void Initialize()
        {
            _battleInfo = new BattleInfo(null, null, null);
            _itemInfo = new ItemInfo(new ItemMaster(), new ItemInventory());
            _questInfo = new QuestInfo(() => new DateTime(2015, 1, 1)) {AcceptMax = 10};
            _questCounter = new QuestCounter(_questInfo, _itemInfo, _battleInfo);
        }

        /// <summary>
        /// 201: 敵艦隊を撃滅せよ！
        /// 210: 敵艦隊を10回邀撃せよ！
        /// 214: あ号
        /// 216: 敵艦隊主力を撃滅せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_201_216_210_214()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {201, 216, 210, 214}));

            InjectMapStart(11, 4);
            var counts = _questInfo.Quests.Select(q => q.Count).ToArray();
            // 出撃カウント
            Assert.AreEqual(214, counts[2].Id);
            Assert.AreEqual(1, counts[2].NowArray[0]);
            InjectBattleResult("S");
            // 道中S勝利
            PAssert.That(() => counts.Select(c => new {c.Id, c.Now}).SequenceEqual(new[]
            {
                new {Id = 201, Now = 1}, new {Id = 210, Now = 1},
                new {Id = 214, Now = 0}, new {Id = 216, Now = 1}
            }));
            PAssert.That(() => counts[2].NowArray.SequenceEqual(new[] {1, 1, 0, 0}));

            InjectMapNext(11, 5);
            // ボスB勝利
            InjectBattleResult("B");
            PAssert.That(() => counts.Select(c => new {c.Id, c.Now}).SequenceEqual(new[]
            {
                new {Id = 201, Now = 2}, new {Id = 210, Now = 2},
                new {Id = 214, Now = 0}, new {Id = 216, Now = 2}
            }));
            // ボス敗北
            PAssert.That(() => counts[2].NowArray.SequenceEqual(new[] {1, 1, 1, 1}));
            InjectBattleResult("C");
            PAssert.That(() => counts.Select(c => new {c.Id, c.Now}).SequenceEqual(new[]
            {
                new {Id = 201, Now = 2}, new {Id = 210, Now = 3},
                new {Id = 214, Now = 0}, new {Id = 216, Now = 2}
            }));
            PAssert.That(() => counts[2].NowArray.SequenceEqual(new[] {1, 1, 2, 1}));
        }

        /// <summary>
        /// 211: 敵空母を3隻撃沈せよ！
        /// 212: 敵輸送船団を叩け！
        /// 213: 海上通商破壊作戦
        /// 218: 敵補給艦を3隻撃沈せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_211_212_213_218_220_221()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {211, 212, 213, 218, 220, 221}));
            // 補給艦1隻と空母2隻
            _battleInfo.InjectResultStatus(new ShipStatus[0], new ShipStatus[0], new[]
            {
                new ShipStatus {NowHp = 0, MaxHp = 130, Spec = new ShipSpec {Id = 1558, ShipType = 15}},
                new ShipStatus {NowHp = 0, MaxHp = 90, Spec = new ShipSpec {Id = 1543, ShipType = 8}},
                new ShipStatus {NowHp = 0, MaxHp = 90, Spec = new ShipSpec {Id = 1543, ShipType = 8}},
                new ShipStatus {NowHp = 0, MaxHp = 96, Spec = new ShipSpec {Id = 1528, ShipType = 11}},
                new ShipStatus {NowHp = 0, MaxHp = 70, Spec = new ShipSpec {Id = 1523, ShipType = 7}},
                new ShipStatus {NowHp = 1, MaxHp = 70, Spec = new ShipSpec {Id = 1523, ShipType = 7}}
            }, new ShipStatus[0]);
            InjectBattleResult("A");
            PAssert.That(() =>
                _questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[]
                    {
                        new {Id = 211, Now = 2}, new {Id = 212, Now = 1}, new {Id = 213, Now = 1},
                        new {Id = 218, Now = 1}, new {Id = 220, Now = 2}, new {Id = 221, Now = 1}
                    }));
        }

        /// <summary>
        /// 228: 海上護衛戦
        /// 230: 敵潜水艦を制圧せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_228_230()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {228, 230}));
            // 潜水艦3
            _battleInfo.InjectResultStatus(new ShipStatus[0], new ShipStatus[0], new[]
            {
                new ShipStatus {NowHp = 0, MaxHp = 27, Spec = new ShipSpec {Id = 1532, ShipType = 13}},
                new ShipStatus {NowHp = 0, MaxHp = 19, Spec = new ShipSpec {Id = 1530, ShipType = 13}},
                new ShipStatus {NowHp = 0, MaxHp = 19, Spec = new ShipSpec {Id = 1530, ShipType = 13}}
            }, new ShipStatus[0]);
            InjectBattleResult("S");
            PAssert.That(() =>
                _questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[]
                    {
                        new {Id = 228, Now = 3}, new {Id = 230, Now = 3}
                    }));
        }

        /// <summary>
        /// 226: 南西諸島海域の制海権を握れ！
        /// </summary>
        [TestMethod]
        public void BattleResult_226()
        {
            var count = InjectQuest(226);

            InjectMapStart(21, 4);
            InjectBattleResult("S");
            Assert.AreEqual(0, count.Now);

            InjectMapNext(21, 5);
            InjectBattleResult("S");
            Assert.AreEqual(1, count.Now);
            InjectBattleResult("B");
            Assert.AreEqual(2, count.Now);
        }

        /// <summary>
        /// // 243: 南方海域珊瑚諸島沖の制空権を握れ！
        /// </summary>
        [TestMethod]
        public void BattleResult_243()
        {
            var count = InjectQuest(243);

            InjectMapNext(52, 4);
            InjectBattleResult("S");
            Assert.AreEqual(0, count.Now);

            InjectMapNext(52, 5);
            InjectBattleResult("A");
            Assert.AreEqual(0, count.Now);
            InjectBattleResult("S");
            Assert.AreEqual(1, count.Now);
        }

        /// <summary>
        /// 249: 「第五戦隊」出撃せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_249()
        {
            var count = InjectQuest(249);

            _battleInfo.InjectResultStatus(new[]
            {
                ShipStatus("妙高改二"), ShipStatus("那智改二"), ShipStatus("羽黒改二"),
                ShipStatus("足柄改二"), ShipStatus("筑摩改二"), ShipStatus("利根改二")
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(25, 4);
            InjectBattleResult("S");
            Assert.AreEqual(0, count.Now);

            InjectMapNext(25, 5);
            InjectBattleResult("A");
            Assert.AreEqual(0, count.Now);
            InjectBattleResult("S");
            Assert.AreEqual(1, count.Now);

            _battleInfo.Result.Friend.Main[3].NowHp = 0;
            InjectBattleResult("S");
            Assert.AreEqual(2, count.Now, "足柄改二轟沈");

            _battleInfo.Result.Friend.Main[1].NowHp = 0;
            InjectBattleResult("S");
            Assert.AreEqual(2, count.Now, "那智改二轟沈");
        }

        /// <summary>
        /// 257: 「水雷戦隊」南西へ！
        /// </summary>
        [TestMethod]
        public void BattleResult_257()
        {
            var count = InjectQuest(257);

            _battleInfo.InjectResultStatus(
                ShipStatusList(3, 2, 2, 2, 2, 2), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(14, 4);
            InjectBattleResult("S");
            Assert.AreEqual(0, count.Now);

            InjectMapNext(14, 5);
            InjectBattleResult("A");
            Assert.AreEqual(0, count.Now);
            InjectBattleResult("S");
            Assert.AreEqual(1, count.Now);
            _questInfo.Quests[0].Count.Now = 0;

            _battleInfo.Result.Friend.Main[0].NowHp = 0;
            InjectBattleResult("S");
            Assert.AreEqual(0, count.Now, "軽巡轟沈");
            _battleInfo.Result.Friend.Main[0].NowHp = 1;

            _battleInfo.Result.Friend.Main[0].Spec.ShipType = 2;
            _battleInfo.Result.Friend.Main[1].Spec.ShipType = 3;
            InjectBattleResult("S");
            Assert.AreEqual(0, count.Now, "旗艦が駆逐");
            _battleInfo.Result.Friend.Main[0].Spec.ShipType = 3;

            _battleInfo.Result.Friend.Main[2].Spec.ShipType = 3;
            _battleInfo.Result.Friend.Main[3].Spec.ShipType = 3;
            InjectBattleResult("S");
            Assert.AreEqual(0, count.Now, "軽巡が4隻");

            _battleInfo.Result.Friend.Main[0].Spec.ShipType = 3;
            _battleInfo.Result.Friend.Main[3].Spec.ShipType = 4;
            InjectBattleResult("S");
            Assert.AreEqual(0, count.Now, "駆逐軽巡以外");
        }

        /// <summary>
        /// 257: 「水上打撃部隊」南方へ！
        /// </summary>
        [TestMethod]
        public void BattleResult_259()
        {
            var count = InjectQuest(259);

            var org = new[]
            {
                ShipStatus(3, 52, 321), ShipStatus(9, 19, 276), ShipStatus(10, 26, 411),
                ShipStatus(10, 26, 412), ShipStatus(5, 29, 193), ShipStatus(5, 29, 194)
            };
            _battleInfo.InjectResultStatus(
                org.ToArray(), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(51, 4);
            InjectBattleResult("S");
            Assert.AreEqual(0, count.Now);

            InjectMapNext(51, 5);
            InjectBattleResult("A");
            Assert.AreEqual(0, count.Now);
            InjectBattleResult("S");
            Assert.AreEqual(1, count.Now);
            _questInfo.Quests[0].Count.Now = 0;

            _battleInfo.Result.Friend.Main[0].NowHp = 0;
            InjectBattleResult("S");
            Assert.AreEqual(0, count.Now, "軽巡轟沈");
            _battleInfo.Result.Friend.Main[0].NowHp = 1;

            _battleInfo.Result.Friend.Main[4] = ShipStatus(9, 37, 136);
            InjectBattleResult("S");
            Assert.AreEqual(0, count.Now, "戦艦4隻");
            _battleInfo.Result.Friend.Main[4] = org[4];

            _battleInfo.Result.Friend.Main[0] = ShipStatus(4, 4, 58);
            InjectBattleResult("S");
            Assert.AreEqual(0, count.Now, "軽巡なし");
            _battleInfo.Result.Friend.Main[0] = org[0];

            _battleInfo.Result.Friend.Main[2] = ShipStatus(10, 2, 553);
            InjectBattleResult("S");
            Assert.AreEqual(1, count.Now, "伊勢改二");
        }

        /// <summary>
        /// 264: 「空母機動部隊」西へ！
        /// </summary>
        [TestMethod]
        public void BattleResult_264()
        {
            var count = InjectQuest(264);

            _battleInfo.InjectResultStatus(
                ShipStatusList(7, 11, 3, 3, 2, 2), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(42, 4);
            InjectBattleResult("S");
            Assert.AreEqual(0, count.Now);

            InjectMapNext(42, 5);
            InjectBattleResult("A");
            Assert.AreEqual(0, count.Now);
            InjectBattleResult("S");
            Assert.AreEqual(1, count.Now);

            _battleInfo.Result.Friend.Main[0].NowHp = 0;
            InjectBattleResult("S");
            Assert.AreEqual(1, count.Now, "轟沈あり");
        }

        /// <summary>
        /// 266: 「水上反撃部隊」突入せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_266()
        {
            var count = InjectQuest(266);

            _battleInfo.InjectResultStatus(
                ShipStatusList(2, 5, 3, 2, 2, 2), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(25, 4);
            InjectBattleResult("S");
            Assert.AreEqual(0, count.Now);

            InjectMapNext(25, 5);
            InjectBattleResult("A");
            Assert.AreEqual(0, count.Now);
            InjectBattleResult("S");
            Assert.AreEqual(1, count.Now);

            _battleInfo.Result.Friend.Main[1].NowHp = 0;
            InjectBattleResult("S");
            Assert.AreEqual(1, count.Now, "轟沈あり");
            _battleInfo.Result.Friend.Main[1].NowHp = 1;

            _battleInfo.Result.Friend.Main[0].Spec.ShipType = 3;
            _battleInfo.Result.Friend.Main[2].Spec.ShipType = 2;
            InjectBattleResult("S");
            Assert.AreEqual(1, count.Now, "旗艦が軽巡");
            _battleInfo.Result.Friend.Main[0].Spec.ShipType = 2;
            _battleInfo.Result.Friend.Main[2].Spec.ShipType = 3;

            _battleInfo.Result.Friend.Main[3].Spec.ShipType = 3;
            InjectBattleResult("S");
            Assert.AreEqual(1, count.Now, "軽巡が2隻");
        }

        /// <summary>
        /// 280: 兵站線確保！海上警備を強化実施せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_280()
        {
            var count = InjectQuest(280);

            _battleInfo.InjectResultStatus(
                ShipStatusList(7, 2, 1, 1, 8, 8), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(12, 4);
            InjectBattleResult("S");
            InjectMapNext(12, 5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0}));

            InjectBattleResult("S");
            InjectMapNext(13, 5);
            InjectBattleResult("S");
            InjectMapNext(14, 5);
            InjectBattleResult("S");
            InjectMapNext(21, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}));

            _battleInfo.Result.Friend.Main = ShipStatusList(7, 1, 1, 8, 8, 8);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}));

            _battleInfo.Result.Friend.Main = ShipStatusList(8, 1, 1, 1, 8, 8);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}));

            _battleInfo.Result.Friend.Main = ShipStatusList(3, 2, 1, 1, 8, 8);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 2}));

            _battleInfo.Result.Friend.Main = ShipStatusList(2, 4, 2, 1, 8, 8);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 3}));

            _battleInfo.Result.Friend.Main = ShipStatusList(2, 2, 21, 2, 8, 8);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 4}));
        }

        /// <summary>
        /// 284: 南西諸島方面「海上警備行動」発令！
        /// </summary>
        [TestMethod]
        public void BattleResult_284()
        {
            var count = InjectQuest(284);

            _battleInfo.InjectResultStatus(
                ShipStatusList(7, 2, 1, 1, 8, 8), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(14, 4);
            InjectBattleResult("S");
            InjectMapNext(14, 5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0}));

            InjectBattleResult("S");
            InjectMapNext(21, 5);
            InjectBattleResult("S");
            InjectMapNext(22, 5);
            InjectBattleResult("S");
            InjectMapNext(23, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}));

            // 艦種チェックは280と共通
        }

        /// <summary>
        /// 822: 沖ノ島海域迎撃戦
        /// 854: 戦果拡張任務！「Z作戦」前段作戦
        /// </summary>
        [TestMethod]
        public void BattleResult_822_854()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {822, 854}));
            var c822 = _questInfo.Quests[0].Count;
            var c854 = _questInfo.Quests[1].Count;

            InjectMapNext(24, 4);
            InjectBattleResult("S");

            PAssert.That(() => c854.NowArray.SequenceEqual(new[] {0, 0, 0, 0}));
            Assert.AreEqual(0, c822.Now);

            InjectMapNext(24, 5);
            InjectBattleResult("A");
            InjectMapNext(61, 5);
            InjectBattleResult("A");
            InjectMapNext(63, 5);
            InjectBattleResult("A");
            InjectMapNext(64, 5);
            InjectBattleResult("S");
            PAssert.That(() => c854.NowArray.SequenceEqual(new[] {1, 1, 1, 1}));
            Assert.AreEqual(0, c822.Now);
            InjectMapNext(24, 5);
            InjectBattleResult("S");
            PAssert.That(() => c854.NowArray.SequenceEqual(new[] {2, 1, 1, 1}));
            Assert.AreEqual(1, c822.Now);
        }

        /// <summary>
        /// 845: 発令！「西方海域作戦」
        /// </summary>
        [TestMethod]
        public void BattleResult_845()
        {
            var count = InjectQuest(845);

            InjectMapNext(41, 4);
            InjectBattleResult("S");
            InjectMapNext(41, 5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0, 0}));

            InjectMapNext(41, 5);
            InjectBattleResult("S");
            InjectMapNext(42, 5);
            InjectBattleResult("S");
            InjectMapNext(43, 5);
            InjectBattleResult("S");
            InjectMapNext(44, 5);
            InjectBattleResult("S");
            InjectMapNext(45, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1, 1}));
        }

        /// <summary>
        /// 861: 強行輸送艦隊、抜錨！
        /// </summary>
        [TestMethod]
        public void MapNext_861()
        {
            var count = InjectQuest(861);

            _battleInfo.InjectResultStatus(
                ShipStatusList(10, 22, 2, 2, 2, 2), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(16, 4);
            Assert.AreEqual(0, count.Now);

            InjectMapNext(16, 8);
            Assert.AreEqual(1, count.Now);

            _battleInfo.Result.Friend.Main[1].NowHp = 0;
            InjectMapNext(16, 8);
            Assert.AreEqual(1, count.Now, "轟沈あり");
            _battleInfo.Result.Friend.Main[1].NowHp = 1;

            _battleInfo.Result.Friend.Main[2].Spec.ShipType = 10;
            InjectMapNext(16, 8);
            Assert.AreEqual(1, count.Now, "補給・航戦が3隻");
        }

        /// <summary>
        /// 862: 前線の航空偵察を実施せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_862()
        {
            var count = InjectQuest(862);

            _battleInfo.InjectResultStatus(
                ShipStatusList(2, 3, 3, 2, 2, 16), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(63, 4);
            InjectBattleResult("A");
            Assert.AreEqual(0, count.Now);

            InjectMapNext(63, 5);
            InjectBattleResult("B");
            Assert.AreEqual(0, count.Now);
            InjectBattleResult("A");
            Assert.AreEqual(1, count.Now);

            _battleInfo.Result.Friend.Main[1].NowHp = 0;
            InjectBattleResult("A");
            Assert.AreEqual(1, count.Now, "轟沈あり");
            _battleInfo.Result.Friend.Main[1].NowHp = 1;

            _battleInfo.Result.Friend.Main[3].Spec.ShipType = 3;
            _battleInfo.Result.Friend.Main[4].Spec.ShipType = 16;
            InjectBattleResult("A");
            Assert.AreEqual(2, count.Now, "軽巡3隻水母2隻");
        }

        /// <summary>
        /// 872: 戦果拡張任務！「Z作戦」後段作戦
        /// </summary>
        [TestMethod]
        public void BattleResult_872()
        {
            var count = InjectQuest(872);

            InjectMapNext(55, 4);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0}));
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0}));
            InjectMapNext(55, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 1, 0, 0}));

            InjectMapNext(62, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 1, 1, 0}));
            InjectMapNext(65, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 1, 1, 1}));
            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 7,
                api_mapinfo_no = 2,
                api_no = 15,
                api_event_id = 5
            }));
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}), "7-2M");
        }

        /// <summary>
        /// 873: 北方海域警備を実施せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_873()
        {
            var count = InjectQuest(873);

            _battleInfo.InjectResultStatus(
                ShipStatusList(3, 2, 2, 2, 2, 2), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(31, 4);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0}));

            InjectMapNext(31, 5);
            InjectBattleResult("B");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0}));
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0}));

            _battleInfo.Result.Friend.Main[0].Spec.ShipType = 2;
            InjectBattleResult("A");
            Assert.AreEqual(1, _questInfo.Quests[0].Count.NowArray[0], "軽巡なし");
            _battleInfo.Result.Friend.Main[0].Spec.ShipType = 3;

            InjectMapNext(32, 5);
            InjectBattleResult("A");
            InjectMapNext(33, 5);
            InjectBattleResult("A");
            Assert.IsTrue(_questInfo.Quests[0].Count.NowArray.SequenceEqual(new[] {1, 1, 1}));
        }

        /// <summary>
        /// 875: 精鋭「三一駆」、鉄底海域に突入せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_875()
        {
            var count = InjectQuest(875);

            _battleInfo.InjectResultStatus(new[]
            {
                ShipStatus("長波改二"), ShipStatus("Iowa改"), ShipStatus("Saratoga Mk.II"),
                ShipStatus("瑞鶴改二甲"), ShipStatus("望月改"), ShipStatus("朝霜改")
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(54, 4);
            InjectBattleResult("S");
            Assert.AreEqual(0, count.Now);

            InjectMapNext(54, 5);
            InjectBattleResult("A");
            Assert.AreEqual(0, count.Now);
            InjectBattleResult("S");
            Assert.AreEqual(1, count.Now);

            _battleInfo.Result.Friend.Main[5].NowHp = 0;
            InjectBattleResult("S");
            Assert.AreEqual(1, count.Now, "朝霜改轟沈");
            _battleInfo.Result.Friend.Main[5].NowHp = 1;

            _battleInfo.Result.Friend.Main[0] = ShipStatus("高波改");
            InjectBattleResult("S");
            Assert.AreEqual(1, count.Now, "長波改二なし");
            _battleInfo.Result.Friend.Main[0] = ShipStatus("長波改二");

            _battleInfo.Result.Friend.Main[5] = ShipStatus("高波改");
            InjectBattleResult("S");
            Assert.AreEqual(2, count.Now, "高波改");
            _battleInfo.Result.Friend.Main[5] = ShipStatus("沖波改");
            InjectBattleResult("S");
            Assert.AreEqual(3, count.Now, "沖波改");
            _battleInfo.Result.Friend.Main[5] = ShipStatus("朝霜改二");
            InjectBattleResult("S");
            Assert.AreEqual(4, count.Now, "朝霜改二");
        }

        /// <summary>
        /// 888: 新編成「三川艦隊」、鉄底海峡に突入せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_888()
        {
            var count = InjectQuest(888);

            _battleInfo.InjectResultStatus(new[]
            {
                ShipStatus("鳥海改二"), ShipStatus("青葉改"), ShipStatus("衣笠改二"),
                ShipStatus("加古改二"), ShipStatus("夕立改二"), ShipStatus("綾波改二")
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(51, 4);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0}));

            InjectMapNext(51, 5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0}));
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0}));
            _battleInfo.Result.Friend.Main[0].NowHp = 0;
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0}), "轟沈あり");
            _battleInfo.Result.Friend.Main[0].NowHp = 1;

            _battleInfo.Result.Friend.Main[0] = ShipStatus("妙高改二");
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0}), "三川艦隊3隻");

            _battleInfo.Result.Friend.Main[0] = ShipStatus("夕張改二特");
            InjectMapNext(53, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 0}));

            _battleInfo.Result.Friend.Main[0] = ShipStatus("天龍改二");
            _battleInfo.Result.Friend.Main[1] = ShipStatus("古鷹改二");
            InjectMapNext(54, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1}));
        }

        /// <summary>
        /// 893: 泊地周辺海域の安全確保を徹底せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_893()
        {
            var count = InjectQuest(893);

            InjectMapNext(15, 4);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0}));

            InjectMapNext(15, 5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0}), "A勝利はカウントしない");
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0}), "1-5");

            InjectMapNext(71, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 0, 0}), "7-1");

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 7,
                api_mapinfo_no = 2,
                api_no = 7,
                api_event_id = 5
            }));
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 0}), "7-2G");

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 7,
                api_mapinfo_no = 2,
                api_no = 15,
                api_event_id = 5
            }));
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}), "7-2M");
        }

        /// <summary>
        /// 894: 空母戦力の投入による兵站線戦闘哨戒
        /// </summary>
        [TestMethod]
        public void BattleResult_894()
        {
            var count = InjectQuest(894);
            _battleInfo.InjectResultStatus(
                ShipStatusList(2, 2, 2, 2, 2, 2),
                new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(13, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0, 0}), "空母なしはカウントしない");

            _battleInfo.Result.Friend.Main[0].Spec.ShipType = 7;
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0, 0}), "A勝利はカウントしない");

            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0, 0}), "1-3");

            InjectMapNext(14, 4);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0, 0}), "1-4");

            InjectMapNext(14, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 0, 0, 0}), "1-4");

            InjectMapNext(21, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 0, 0}), "2-1");

            InjectMapNext(22, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1, 0}), "2-2");

            InjectMapNext(23, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1, 1}), "2-3");
        }

        /// <summary>
        /// 拡張「六水戦」、最前線へ！
        /// </summary>
        [TestMethod]
        public void BattleResult_903()
        {
            var count = InjectQuest(903);
            _battleInfo.InjectResultStatus(new[] {ShipStatus("夕張改二"), ShipStatus("睦月"), ShipStatus("綾波")},
                new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(51, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0}), "六水戦駆逐が1隻");

            _battleInfo.Result.Friend.Main[2] = ShipStatus("如月");
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0}), "A勝利はカウントしない");

            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0}), "5-1");

            InjectMapNext(54, 4);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0}), "ボス以外はカウントしない");

            _battleInfo.Result.Friend.Main[0] = ShipStatus("夕張改");
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0}), "旗艦が夕張改");

            _battleInfo.Result.Friend.Main = new[] {ShipStatus("睦月"), ShipStatus("如月"), ShipStatus("夕張改二")};
            InjectMapNext(54, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0}), "旗艦が夕張改二ではない");

            _battleInfo.Result.Friend.Main = new[] {ShipStatus("夕張改二"), ShipStatus("弥生"), ShipStatus("卯月")};
            InjectMapNext(54, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 0, 0}), "5-4");

            _battleInfo.Result.Friend.Main = new[] {ShipStatus("夕張改二"), ShipStatus("菊月"), ShipStatus("望月")};
            InjectMapNext(64, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 0}), "6-4");

            InjectMapNext(65, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}), "6-5");

            _battleInfo.Result.Friend.Main = new[] {ShipStatus("夕張改二"), ShipStatus("由良改")};
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}), "由良改");

            _battleInfo.Result.Friend.Main = new[] {ShipStatus("夕張改二"), ShipStatus("由良改二")};
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 2}), "由良改二");
        }

        /// <summary>
        /// 904: 精鋭「十九駆」、躍り出る！
        /// </summary>
        [TestMethod]
        public void BattleResult_904()
        {
            var count = InjectQuest(904);
            _battleInfo.InjectResultStatus(
                new []{ShipStatus("綾波改二"), ShipStatus("敷波")},
                new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(25, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0}), "敷波はカウントしない");

            _battleInfo.Result.Friend.Main[1] = ShipStatus("敷波改二");
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0}), "A勝利はカウントしない");

            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0}), "2-5");

            InjectMapNext(34, 4);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0}), "ボス以外はカウントしない");

            InjectMapNext(34, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 0, 0}), "3-4");

            InjectMapNext(45, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 0}), "4-5");

            InjectMapNext(53, 5);
            InjectBattleResult("S");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}), "5-3");
        }

        /// <summary>
        /// 905: 「海防艦」、海を護る！
        /// </summary>
        [TestMethod]
        public void BattleResult_905()
        {
            var count = InjectQuest(905);
            _battleInfo.InjectResultStatus(
                ShipStatusList(1, 1, 1, 2, 2, 2),
                new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(11, 5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0, 0}), "6隻はカウントしない");

            _battleInfo.Result.Friend.Main[5] = new ShipStatus();
            InjectBattleResult("B");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0, 0}), "B勝利はカウントしない");

            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0, 0}), "1-1");

            InjectMapNext(12, 4);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0, 0}), "ボス以外はカウントしない");

            InjectMapNext(12, 5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 0, 0, 0}), "1-2");

            _battleInfo.Result.Friend.Main[0] = ShipStatus(2);
            InjectMapNext(13, 5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 0, 0, 0}), "海防艦2隻はカウントしない");

            _battleInfo.Result.Friend.Main[0] = ShipStatus(1);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 0, 0}), "1-3");

            InjectMapNext(15, 5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1, 0}), "1-5");

            InjectMapNext(16, 8);
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1, 1}), "1-6");
        }

        /// <summary>
        /// 912: 工作艦「明石」護衛任務
        /// </summary>
        [TestMethod]
        public void BattleResult_912()
        {
            var count = InjectQuest(912);
            _battleInfo.InjectResultStatus(
                new []{ShipStatus("明石"), ShipStatus(2), ShipStatus(2), ShipStatus(1)},
                new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(13, 5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0, 0}), "駆逐艦2隻はカウントしない");

            _battleInfo.Result.Friend.Main[3] = ShipStatus(2);
            InjectBattleResult("B");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0, 0}), "B勝利はカウントしない");

            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0, 0}), "1-3");

            InjectMapNext(21, 4);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0, 0}), "ボス以外はカウントしない");

            _battleInfo.Result.Friend.Main[0] = ShipStatus(2);
            _battleInfo.Result.Friend.Main[1] = ShipStatus("明石");
            InjectMapNext(21, 5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0, 0}), "旗艦明石以外はカウントしない");

            _battleInfo.Result.Friend.Main[0] = ShipStatus("明石");
            _battleInfo.Result.Friend.Main[1] = ShipStatus(2);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 0, 0, 0}), "2-1");

            InjectMapNext(22, 5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 0, 0}), "2-2");

            InjectMapNext(23, 5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1, 0}), "2-3");

            InjectMapNext(16, 8);
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1, 1}), "1-6");
        }

        /// <summary>
        /// 912: 重巡戦隊、西へ！
        /// </summary>
        [TestMethod]
        public void BattleResult_914()
        {
            var count = InjectQuest(914);
            _battleInfo.InjectResultStatus(
                new []{ShipStatus(5), ShipStatus(5), ShipStatus(5), ShipStatus(1)},
                new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(41, 5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0}), "駆逐艦なしはカウントしない");

            _battleInfo.Result.Friend.Main[3] = ShipStatus(2);
            InjectBattleResult("B");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {0, 0, 0, 0}), "B勝利はカウントしない");

            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0}), "4-1");

            InjectMapNext(42, 4);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0}), "ボス以外はカウントしない");

            InjectMapNext(42, 5);
            _battleInfo.Result.Friend.Main[0] = ShipStatus(6);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 0, 0, 0}), "重巡2隻はカウントしない");

            _battleInfo.Result.Friend.Main[0] = ShipStatus(5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 0, 0}), "4-2");

            InjectMapNext(43, 5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 0}), "4-3");

            InjectMapNext(44, 5);
            InjectBattleResult("A");
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}), "4-3");
        }

        /// <summary>
        /// 280と854以降を同時に遂行していると854以降がカウントされないことがある
        /// </summary>
        [TestMethod]
        public void BattleResult_280_854()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {280, 854}));

            _battleInfo.InjectResultStatus(
                ShipStatusList(1, 1, 1, 1, 1, 1), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(24, 5);
            InjectBattleResult("S");
            Assert.AreEqual(1, _questInfo.Quests[1].Count.NowArray[0]);
        }

        /// <summary>
        /// 888と893以降を同時に遂行していると893以降がカウントされないことがある
        /// </summary>
        [TestMethod]
        public void BattleResult_888_893()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {888, 893}));

            _battleInfo.InjectResultStatus(
                ShipStatusList(1, 1, 1, 1, 1, 1), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            InjectMapNext(71, 5);
            InjectBattleResult("S");
            Assert.AreEqual(1, _questInfo.Quests[1].Count.NowArray[1]);
        }

        /// <summary>
        /// 302: 大規模演習
        /// 303: 「演習」で練度向上！
        /// 304: 「演習」で他提督を圧倒せよ！
        /// 311: 精鋭艦隊演習
        /// 315: 春季大演習
        /// </summary>
        [TestMethod]
        public void PracticeResult_303_304_302_311_315()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {302, 303, 304, 311, 315}));

            _battleInfo.InjectResultStatus(new[]
            {
                ShipStatus(2, 543), ShipStatus(3, 488)
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);
            InjectPracticeResult("C");
            InjectPracticeResult("A");
            PAssert.That(() =>
                _questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[]
                    {
                        new {Id = 302, Now = 1}, new {Id = 303, Now = 2}, new {Id = 304, Now = 1},
                        new {Id = 311, Now = 1}, new {Id = 315, Now = 1}
                    }));
        }

        /// <summary>
        /// 318: 給糧艦「伊良湖」の支援
        /// </summary>
        [TestMethod]
        public void PracticeResult_318()
        {
            var count = InjectQuest(318);

            _battleInfo.InjectResultStatus(new[]
            {
                ShipStatus(2, 543), ShipStatus(3, 488)
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);

            InjectPracticeResult("B");
            Assert.AreEqual(0, count.Now, "軽巡1隻");
            _battleInfo.Result.Friend.Main[0] = ShipStatus(3, 200);
            _questCounter.StartPractice("api%5Fdeck%5Fid=2");
            InjectPracticeResult("B");
            Assert.AreEqual(0, count.Now, "第2艦隊");
            _questCounter.StartPractice("api%5Fdeck%5Fid=1"); // 第一艦隊
            InjectPracticeResult("C");
            Assert.AreEqual(0, count.Now, "敗北");
            InjectPracticeResult("B");
            Assert.AreEqual(1, count.Now);

            count.Now = 2;
            _questInfo.InspectQuestList(CreateQuestList(new[] {318}));
            Assert.AreEqual(2, count.Now, "進捗調節しない");
        }

        /// <summary>
        /// 330: 空母機動部隊、演習始め！
        /// </summary>
        [TestMethod]
        public void PracticeResult_330()
        {
            var count = InjectQuest(330);

            _battleInfo.InjectResultStatus(
                ShipStatusList(18, 7, 2, 2),
                new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);
            InjectPracticeResult("B");
            Assert.AreEqual(0, count.Now, "B勝利でカウントしない");

            InjectPracticeResult("A");
            Assert.AreEqual(1, count.Now, "装甲空母、軽空母");

            _battleInfo.Result.Friend.Main = ShipStatusList(11, 7, 2, 2);
            InjectPracticeResult("A");
            Assert.AreEqual(2, count.Now, "正規空母、軽空母");

            count.Now = 0;
            InjectPracticeResult("C");
            Assert.AreEqual(0, count.Now, "敗北");

            _battleInfo.Result.Friend.Main = ShipStatusList(2, 7, 11, 2);
            InjectPracticeResult("A");
            Assert.AreEqual(0, count.Now, "旗艦空母以外");

            _battleInfo.Result.Friend.Main = ShipStatusList(11, 2, 2, 2);
            InjectPracticeResult("A");
            Assert.AreEqual(0, count.Now, "空母一隻");

            _battleInfo.Result.Friend.Main = ShipStatusList(11, 7, 3, 2);
            InjectPracticeResult("A");
            Assert.AreEqual(0, count.Now, "駆逐一隻");
        }

        /// <summary>
        /// 337: 「十八駆」演習！
        /// </summary>
        [TestMethod]
        public void PracticeResult_337()
        {
            var count = InjectQuest(337);

            _battleInfo.InjectResultStatus(new []
            {
                ShipStatus("霰"), ShipStatus("霰"),
                ShipStatus("陽炎"), ShipStatus("不知火"),
                ShipStatus("黒潮")
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);
            InjectPracticeResult("A");
            Assert.AreEqual(0, count.Now, "A");
            InjectPracticeResult("S");
            Assert.AreEqual(1, count.Now);
            _battleInfo.Result.Friend.Main[0] = ShipStatus("涼風");
            InjectPracticeResult("S");
            Assert.AreEqual(1, count.Now, "霰→涼風");
            _battleInfo.Result.Friend.Main[4] = ShipStatus("霞改二");
            InjectPracticeResult("S");
            Assert.AreEqual(2, count.Now, "黒潮→霞改二");
        }

        /// <summary>
        /// 339: 「十九駆」演習！
        /// </summary>
        [TestMethod]
        public void PracticeResult_339()
        {
            var count = InjectQuest(339);

            _battleInfo.InjectResultStatus(new []
            {
                ShipStatus("磯波"), ShipStatus("浦波"),
                ShipStatus("綾波"), ShipStatus("敷波"),
                ShipStatus("初雪")
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);
            InjectPracticeResult("A");
            Assert.AreEqual(0, count.Now, "A");
            InjectPracticeResult("S");
            Assert.AreEqual(1, count.Now);
            _battleInfo.Result.Friend.Main[0] = ShipStatus("深雪");
            InjectPracticeResult("S");
            Assert.AreEqual(1, count.Now, "磯波→深雪");
        }

        /// <summary>
        /// 402: 「遠征」を3回成功させよう！
        /// 403: 「遠征」を10回成功させよう！
        /// 404: 大規模遠征作戦、発令！
        /// 410: 南方への輸送作戦を成功させよ！
        /// 411: 南方への鼠輸送を継続実施せよ！
        /// </summary>
        [TestMethod]
        public void MissionResult_402_403_404_410_411()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {402, 403, 404, 410, 411}));

            _questCounter.InspectDeck(Js(
                new[]
                {
                    new {api_id = 2, api_mission = new[] {2, 6}},
                    new {api_id = 3, api_mission = new[] {2, 37}},
                    new {api_id = 4, api_mission = new[] {2, 2}}
                }));
            _questCounter.InspectMissionResult("api%5Fdeck%5Fid=2", Js(new {api_clear_result = 1}));
            _questCounter.InspectMissionResult("api%5Fdeck%5Fid=3", Js(new {api_clear_result = 2}));
            _questCounter.InspectMissionResult("api%5Fdeck%5Fid=4", Js(new {api_clear_result = 0}));
            PAssert.That(() =>
                _questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[]
                    {
                        new {Id = 402, Now = 2}, new {Id = 403, Now = 2}, new {Id = 404, Now = 2},
                        new {Id = 410, Now = 1}, new {Id = 411, Now = 1}
                    }));
        }

        /// <summary>
        /// 426: 海上通商航路の警戒を厳とせよ！
        /// </summary>
        [TestMethod]
        public void MissionResult_426()
        {
            var count = InjectQuest(426);

            _questCounter.InspectDeck(Js(
                new[]
                {
                    new {api_id = 2, api_mission = new[] {2, 3}},
                    new {api_id = 3, api_mission = new[] {2, 4}},
                    new {api_id = 4, api_mission = new[] {2, 5}}
                }));
            _questCounter.InspectMissionResult("api%5Fdeck%5Fid=2", Js(new {api_clear_result = 1}));
            _questCounter.InspectMissionResult("api%5Fdeck%5Fid=3", Js(new {api_clear_result = 1}));
            _questCounter.InspectMissionResult("api%5Fdeck%5Fid=4", Js(new {api_clear_result = 1}));
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 0}));
            _questCounter.InspectDeck(Js(
                new[]
                {
                    new {api_id = 2, api_mission = new[] {2, 10}}
                }));
            _questCounter.InspectMissionResult("api%5Fdeck%5Fid=2", Js(new {api_clear_result = 1}));
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}));
        }

        /// <summary>
        /// 428: 近海に侵入する敵潜を制圧せよ！
        /// </summary>
        [TestMethod]
        public void MissionResult_428()
        {
            var count = InjectQuest(428);

            _questCounter.InspectDeck(Js(
                new[]
                {
                    new {api_id = 2, api_mission = new[] {2, 4}},
                    new {api_id = 3, api_mission = new[] {2, 101}},
                    new {api_id = 4, api_mission = new[] {2, 102}}
                }));
            _questCounter.InspectMissionResult("api%5Fdeck%5Fid=2", Js(new {api_clear_result = 1}));
            _questCounter.InspectMissionResult("api%5Fdeck%5Fid=3", Js(new {api_clear_result = 1}));
            _questCounter.InspectMissionResult("api%5Fdeck%5Fid=4", Js(new {api_clear_result = 1}));
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1}));
        }

        /// <summary>
        /// 436: 練習航海及び警備任務を実施せよ！
        /// </summary>
        [TestMethod]
        public void MissionResult_436()
        {
            var count = InjectQuest(436);

            _questCounter.InspectDeck(Js(
                new[]
                {
                    new {api_id = 2, api_mission = new[] {2, 1}},
                    new {api_id = 3, api_mission = new[] {2, 2}},
                    new {api_id = 4, api_mission = new[] {2, 3}}
                }));
            _questCounter.InspectMissionResult("api%5Fdeck%5Fid=2", Js(new {api_clear_result = 1}));
            _questCounter.InspectMissionResult("api%5Fdeck%5Fid=3", Js(new {api_clear_result = 1}));
            _questCounter.InspectMissionResult("api%5Fdeck%5Fid=4", Js(new {api_clear_result = 1}));
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 0, 0}));

            _questCounter.InspectDeck(Js(
                new[]
                {
                    new {api_id = 2, api_mission = new[] {2, 4}},
                    new {api_id = 3, api_mission = new[] {2, 10}}
                }));
            _questCounter.InspectMissionResult("api%5Fdeck%5Fid=2", Js(new {api_clear_result = 1}));
            _questCounter.InspectMissionResult("api%5Fdeck%5Fid=3", Js(new {api_clear_result = 1}));
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1, 1, 1}));
        }

        /// <summary>
        /// 503: 艦隊大整備！
        /// 504: 艦隊酒保祭り！
        /// </summary>
        [TestMethod]
        public void PowerUp_503_504()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {503, 504}));

            _questCounter.CountNyukyo();
            _questCounter.CountCharge();
            PAssert.That(() =>
                _questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[] {new {Id = 503, Now = 1}, new {Id = 504, Now = 1}}));
        }

        /// <summary>
        /// 605: 新装備「開発」指令
        /// 606: 新造艦「建造」指令
        /// 607: 装備「開発」集中強化！
        /// 608: 艦娘「建造」艦隊強化！
        /// 609: 軍縮条約対応！
        /// 619: 装備の改修強化
        /// </summary>
        [TestMethod]
        public void Kousyou_605_606_607_608_609_619()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {605, 606, 607, 608, 609, 619}));

            _questCounter.InspectCreateItem(
                "api_verno=1&api_item1=10&api_item2=10&api_item3=30&api_item4=10&api_multiple_flag=0");
            _questCounter.InspectCreateItem(
                "api_verno=1&api_item1=10&api_item2=10&api_item3=30&api_item4=10&api_multiple_flag=1");
            _questCounter.CountCreateShip();
            _questCounter.InspectDestroyShip("api%5Fship%5Fid=98159%2C98166%2C98168&api%5Fverno=1");
            _questCounter.CountRemodelSlot();
            PAssert.That(() =>
                _questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[]
                    {
                        new {Id = 605, Now = 4}, new {Id = 606, Now = 1}, new {Id = 607, Now = 4},
                        new {Id = 608, Now = 1}, new {Id = 609, Now = 3}, new {Id = 619, Now = 1}
                    }));
        }

        /// <summary>
        /// 613: 資源の再利用
        /// 638: 対空機銃量産
        /// 643: 主力「陸攻」の調達
        /// 645: 「洋上補給」物資の調達
        /// 653: 工廠稼働！次期作戦準備！
        /// 663: 新型艤装の継続研究
        /// 673: 装備開発力の整備
        /// 674: 工廠環境の整備
        /// 675: 運用装備の統合整備
        /// 676: 装備開発力の集中整備
        /// 677: 継戦支援能力の整備
        /// 678: 主力艦上戦闘機の更新
        /// 680: 対空兵装の整備拡充
        /// 688: 航空戦力の強化
        /// </summary>
        [TestMethod]
        public void DestroyItem_613_638_643_645_653_663_673_674_675_676_677_678_680_686_688()
        {
            _itemInfo.InjectItemSpec(new[]
            {
                new ItemSpec {Id = 1, Name = "12cm単装砲", Type = 1},
                new ItemSpec {Id = 37, Name = "7.7mm機銃", Type = 21},
                new ItemSpec {Id = 19, Name = "九六式艦戦", Type = 6},
                new ItemSpec {Id = 4, Name = "14cm単装砲", Type = 2},
                new ItemSpec {Id = 11, Name = "15.2cm単装砲", Type = 4},
                new ItemSpec {Id = 75, Name = "ドラム缶(輸送用)", Type = 30},
                new ItemSpec {Id = 7, Name = "35.6cm連装砲", Type = 3},
                new ItemSpec {Id = 25, Name = "零式水上偵察機", Type = 10},
                new ItemSpec {Id = 13, Name = "61cm三連装魚雷", Type = 5},
                new ItemSpec {Id = 20, Name = "零式艦戦21型", Type = 6},
                new ItemSpec {Id = 28, Name = "22号水上電探", Type = 12},
                new ItemSpec {Id = 31, Name = "32号水上電探", Type = 13},
                new ItemSpec {Id = 35, Name = "三式弾", Type = 18},
                new ItemSpec {Id = 23, Name = "九九式艦爆", Type = 7},
                new ItemSpec {Id = 16, Name = "九七式艦攻", Type = 8},
                new ItemSpec {Id = 3, Name = "10cm連装高角砲", Type = 1},
                new ItemSpec {Id = 121, Name = "94式高射装置", Type = 36}
            });
            var items = new[] {1, 37, 19, 4, 11, 75, 7, 25, 13, 20, 28, 31, 35, 23, 16, 3, 121};
            _itemInfo.InjectItems(items);
            var questList = new[] {613, 638, 643, 645, 653, 663, 673, 674, 675, 676, 677, 678, 680, 686, 688};
            _questInfo.AcceptMax = questList.Length;
            _questInfo.InspectQuestList(CreateQuestList(questList));
            _questCounter.InspectDestroyItem(
                $"api%5Fslotitem%5Fids={string.Join("%2C", Enumerable.Range(1, items.Length))}&api%5Fverno=1", null);
            var scalar = new[]
            {
                new {Id = 613, Now = 1}, new {Id = 638, Now = 1}, new {Id = 643, Now = 1}, new {Id = 645, Now = 1},
                new {Id = 653, Now = 1}, new {Id = 663, Now = 1}, new {Id = 673, Now = 2}, new {Id = 674, Now = 1}
            };
            foreach (var e in scalar)
            {
                var c = Array.Find(_questInfo.Quests, q => q.Id == e.Id).Count;
                Assert.AreEqual(e.Id, c.Id);
                Assert.AreEqual(e.Now, c.Now, $"{c.Id}");
            }
            var array = new[]
            {
                new {Id = 675, NowArray = new[] {2, 1}}, new {Id = 676, NowArray = new[] {1, 1, 1}},
                new {Id = 677, NowArray = new[] {1, 1, 1}}, new {Id = 678, NowArray = new[] {1, 1}},
                new {Id = 680, NowArray = new[] {1, 2}}, new {Id = 686, NowArray = new[] {1, 1}},
                new {Id = 688, NowArray = new[] {2, 1, 1, 1}}
            };
            foreach (var e in array)
            {
                var c = Array.Find(_questInfo.Quests, q => q.Id == e.Id).Count;
                Assert.AreEqual(e.Id, c.Id);
                PAssert.That(() => c.NowArray.SequenceEqual(e.NowArray), $"{c.Id}");
            }
        }

        /// <summary>
        /// 702: 艦の「近代化改修」を実施せよ！
        /// 703: 「近代化改修」を進め、戦備を整えよ！
        /// </summary>
        [TestMethod]
        public void PowerUp_702_703()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {702, 703}));
            _questCounter.InspectPowerUp(Js(new {api_powerup_flag = 1}));
            PAssert.That(() =>
                _questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[] {new {Id = 702, Now = 1}, new {Id = 703, Now = 1}}));
        }
    }
}