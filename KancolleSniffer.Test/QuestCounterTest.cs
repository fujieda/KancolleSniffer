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
            PAssert.That(() => count.Now == 3);
            count.AdjustCount(50);
            PAssert.That(() => count.Now == 4);
            count.AdjustCount(80);
            PAssert.That(() => count.Now == 6);
            count.AdjustCount(100);
            PAssert.That(() => count.Now == 7);
            count.Now = 14;
            count.AdjustCount(100);
            PAssert.That(() => count.Now == 14);
            count.AdjustCount(80);
            PAssert.That(() => count.Now == 6);
            count.AdjustCount(50);
            PAssert.That(() => count.Now == 5);
            count.AdjustCount(0);
            PAssert.That(() => count.Now == 3);
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
            PAssert.That(() => count.Now == 2);
            count.AdjustCount(50);
            PAssert.That(() => count.Now == 3);
            count.AdjustCount(80);
            PAssert.That(() => count.Now == 6);
            count.AdjustCount(100);
            PAssert.That(() => count.Now == 7);
            count.Now = 14;
            count.AdjustCount(100);
            PAssert.That(() => count.Now == 14);
            count.AdjustCount(80);
            PAssert.That(() => count.Now == 6);
            count.AdjustCount(50);
            PAssert.That(() => count.Now == 5);
            count.AdjustCount(0);
            PAssert.That(() => count.Now == 2);
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
                    PAssert.That(() => count.Now == count.Spec.Max - 1);
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
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 2, 3, 4}));
            count.AdjustCount(100);
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {36, 6, 24, 12}));
            count.NowArray = new[] {38, 12, 19, 12};
            count.AdjustCount(100);
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {38, 12, 24, 12}));
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
                    new QuestCount {Id = 280, NowArray = new[] {1, 1, 1, 1}}
                }
            };
            new QuestInfo().LoadState(status);
            PAssert.That(() => status.QuestCountList[0].ToString() == "2/3");
            PAssert.That(() => status.QuestCountList[1].ToString() == "20/36 7/6 10/24 8/12");
            var z = status.QuestCountList[2];
            PAssert.That(() => z.ToString() == "4/4");
            PAssert.That(() => z.ToToolTip() == "2-4 6-1 6-3 6-4");
            z.NowArray = new[] {0, 0, 0, 0};
            PAssert.That(() => z.ToToolTip() == "");
            var q426 = status.QuestCountList[3];
            PAssert.That(() => q426.ToString() == "4/4");
            PAssert.That(() => q426.ToToolTip() == "警備任務 対潜警戒任務 海上護衛任務 強硬偵察任務");
            var q428 = status.QuestCountList[4];
            PAssert.That(() => q428.ToToolTip() == "対潜警戒任務1 海峡警備行動1 長時間対潜警戒1");
            q428.NowArray = new[] {0, 1, 0};
            PAssert.That(() => q428.ToToolTip() == "海峡警備行動1");
            var q873 = status.QuestCountList[5];
            PAssert.That(() => q873.ToString() == "3/3");
            PAssert.That(() => q873.ToToolTip() == "3-1 3-2 3-3");
            var q888 = status.QuestCountList[6];
            PAssert.That(() => q888.ToString() == "3/3");
            PAssert.That(() => q888.ToToolTip() == "5-1 5-3 5-4");
            var q688 = status.QuestCountList[7];
            PAssert.That(() => q688.ToToolTip() == "艦戦2 艦爆1 艦攻2 水偵1");
            var q893 = status.QuestCountList[8];
            PAssert.That(() => q893.ToToolTip() == "1-5:1 7-1:1 7-2G:1 7-2M:1");
            var q894 = status.QuestCountList[9];
            PAssert.That(() => q894.ToString() == "5/5");
            PAssert.That(() => q894.ToToolTip() == "1-3 1-4 2-1 2-2 2-3");
            var q280 = status.QuestCountList[10];
            PAssert.That(() => q280.ToString() == "4/4");
            PAssert.That(() => q280.ToToolTip() == "1-2 1-3 1-4 2-1");
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

        private ShipStatus[] ShipStatusList(params int[] shipTypes)
        {
            return shipTypes.Select(sType => ShipStatus(sType)).ToArray();
        }

        private ShipStatus ShipStatus(int shipType, int specId = 0)
        {
            return new ShipStatus {NowHp = 1, Spec = new ShipSpec {Id = specId, ShipType = shipType}};
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

            _questCounter.InspectMapStart(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 1,
                api_event_id = 4
            }));
            var quests = _questInfo.Quests;
            // 出撃カウント
            PAssert.That(() => quests[2].Id == 214 && quests[2].Count.NowArray[0] == 1);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            // 道中S勝利
            PAssert.That(() => quests.Select(q => new {q.Id, q.Count.Now}).SequenceEqual(new[]
            {
                new {Id = 201, Now = 1}, new {Id = 210, Now = 1},
                new {Id = 214, Now = 0}, new {Id = 216, Now = 1}
            }));
            PAssert.That(() => quests[2].Id == 214 &&
                               quests[2].Count.NowArray.SequenceEqual(new[] {1, 1, 0, 0}));

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 1,
                api_event_id = 5
            }));
            // ボスB勝利
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "B"}));
            PAssert.That(() => quests.Select(q => new {q.Id, q.Count.Now}).SequenceEqual(new[]
            {
                new {Id = 201, Now = 2}, new {Id = 210, Now = 2},
                new {Id = 214, Now = 0}, new {Id = 216, Now = 2}
            }));
            // ボス敗北
            PAssert.That(() => quests[2].Id == 214 && quests[2].Count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "C"}));
            PAssert.That(() => quests.Select(q => new {q.Id, q.Count.Now}).SequenceEqual(new[]
            {
                new {Id = 201, Now = 2}, new {Id = 210, Now = 3},
                new {Id = 214, Now = 0}, new {Id = 216, Now = 2}
            }));
            PAssert.That(() => quests[2].Id == 214 && quests[2].Count.NowArray.SequenceEqual(new[] {1, 1, 2, 1}));
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
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
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
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
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
            _questInfo.InspectQuestList(CreateQuestList(new[] {226}));

            _questCounter.InspectMapStart(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 1,
                api_event_id = 4
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 1,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "B"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 2);
        }

        /// <summary>
        /// // 243: 南方海域珊瑚諸島沖の制空権を握れ！
        /// </summary>
        [TestMethod]
        public void BattleResult_243()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {243}));

            _questCounter.InspectMapStart(Js(new
            {
                api_maparea_id = 5,
                api_mapinfo_no = 2,
                api_event_id = 4
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);

            _questCounter.InspectMapStart(Js(new
            {
                api_maparea_id = 5,
                api_mapinfo_no = 2,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1);
        }

        /// <summary>
        /// 249: 「第五戦隊」出撃せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_249()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {249}));

            _battleInfo.InjectResultStatus(new[]
            {
                ShipStatus(5, 319), ShipStatus(5, 192), ShipStatus(5, 194),
                ShipStatus(5, 193), ShipStatus(6, 189), ShipStatus(6, 188)
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 5,
                api_event_id = 4
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 5,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1);
            _questInfo.Quests[0].Count.Now = 0;

            _battleInfo.Result.Friend.Main[1].NowHp = 0;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0, "那智改二轟沈");
        }

        /// <summary>
        /// 257: 「水雷戦隊」南西へ！
        /// </summary>
        [TestMethod]
        public void BattleResult_257()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {257}));

            _battleInfo.InjectResultStatus(
                ShipStatusList(3, 2, 2, 2, 2, 2), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 4,
                api_event_id = 4
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 4,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1);
            _questInfo.Quests[0].Count.Now = 0;

            _battleInfo.Result.Friend.Main[0].NowHp = 0;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0, "軽巡轟沈");
            _battleInfo.Result.Friend.Main[0].NowHp = 1;

            _battleInfo.Result.Friend.Main[0].Spec.ShipType = 2;
            _battleInfo.Result.Friend.Main[1].Spec.ShipType = 3;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0, "旗艦が駆逐");
            _battleInfo.Result.Friend.Main[0].Spec.ShipType = 3;

            _battleInfo.Result.Friend.Main[2].Spec.ShipType = 3;
            _battleInfo.Result.Friend.Main[3].Spec.ShipType = 3;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0, "軽巡が4隻");

            _battleInfo.Result.Friend.Main[0].Spec.ShipType = 3;
            _battleInfo.Result.Friend.Main[3].Spec.ShipType = 4;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0, "駆逐軽巡以外");
        }

        private ShipStatus ShipStatus(int shipType, int shipClass, int specId) =>
            new ShipStatus {NowHp = 1, Spec = new ShipSpec {Id = specId, ShipType = shipType, ShipClass = shipClass}};

        /// <summary>
        /// 257: 「水上打撃部隊」南方へ！
        /// </summary>
        [TestMethod]
        public void BattleResult_259()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {259}));

            var org = new[]
            {
                ShipStatus(3, 52, 321), ShipStatus(9, 19, 276), ShipStatus(10, 26, 411),
                ShipStatus(10, 26, 412), ShipStatus(5, 29, 193), ShipStatus(5, 29, 194)
            };
            _battleInfo.InjectResultStatus(
                org.ToArray(), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 5,
                api_mapinfo_no = 1,
                api_event_id = 4
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 5,
                api_mapinfo_no = 1,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1);
            _questInfo.Quests[0].Count.Now = 0;

            _battleInfo.Result.Friend.Main[0].NowHp = 0;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0, "軽巡轟沈");
            _battleInfo.Result.Friend.Main[0].NowHp = 1;

            _battleInfo.Result.Friend.Main[4] = ShipStatus(9, 37, 136);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0, "戦艦4隻");
            _battleInfo.Result.Friend.Main[4] = org[4];

            _battleInfo.Result.Friend.Main[0] = ShipStatus(4, 4, 58);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0, "軽巡なし");
            _battleInfo.Result.Friend.Main[0] = org[0];

            _battleInfo.Result.Friend.Main[2] = ShipStatus(10, 2, 553);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1, "伊勢改二");
        }

        /// <summary>
        /// 264: 「空母機動部隊」西へ！
        /// </summary>
        [TestMethod]
        public void BattleResult_264()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {264}));

            _battleInfo.InjectResultStatus(
                ShipStatusList(7, 11, 3, 3, 2, 2), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 4,
                api_mapinfo_no = 2,
                api_event_id = 4
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 4,
                api_mapinfo_no = 2,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1);

            _battleInfo.Result.Friend.Main[0].NowHp = 0;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1, "轟沈あり");
        }

        /// <summary>
        /// 266: 「水上反撃部隊」突入せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_266()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {266}));

            _battleInfo.InjectResultStatus(
                ShipStatusList(2, 5, 3, 2, 2, 2), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 5,
                api_event_id = 4
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 5,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1);

            _battleInfo.Result.Friend.Main[1].NowHp = 0;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1, "轟沈あり");
            _battleInfo.Result.Friend.Main[1].NowHp = 1;

            _battleInfo.Result.Friend.Main[0].Spec.ShipType = 3;
            _battleInfo.Result.Friend.Main[2].Spec.ShipType = 2;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1, "旗艦が軽巡");
            _battleInfo.Result.Friend.Main[0].Spec.ShipType = 2;
            _battleInfo.Result.Friend.Main[2].Spec.ShipType = 3;

            _battleInfo.Result.Friend.Main[3].Spec.ShipType = 3;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1, "軽巡が2隻");
        }

        /// <summary>
        /// 280: 兵站線確保！海上警備を強化実施せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_280()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {280}));

            _battleInfo.InjectResultStatus(
                ShipStatusList(7, 1, 1, 1, 8, 8), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 2,
                api_event_id = 4
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 2,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => _questInfo.Quests[0].Count.NowArray.SequenceEqual(new[] {0, 0, 0, 0}));

            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 3,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 4,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 1,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}));

            _battleInfo.Result.Friend.Main = ShipStatusList(7, 1, 1, 8, 8, 8);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}));

            _battleInfo.Result.Friend.Main = ShipStatusList(8, 1, 1, 1, 8, 8);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}));

            _battleInfo.Result.Friend.Main = ShipStatusList(3, 2, 1, 1, 8, 8);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.NowArray.SequenceEqual(new[] {1, 1, 1, 2}));

            _battleInfo.Result.Friend.Main = ShipStatusList(2, 4, 2, 1, 8, 8);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.NowArray.SequenceEqual(new[] {1, 1, 1, 3}));

            _battleInfo.Result.Friend.Main = ShipStatusList(2, 2, 21, 2, 8, 8);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.NowArray.SequenceEqual(new[] {1, 1, 1, 4}));
        }

        /// <summary>
        /// 822: 沖ノ島海域迎撃戦
        /// 854: 戦果拡張任務！「Z作戦」前段作戦
        /// </summary>
        [TestMethod]
        public void BattleResult_822_854()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {822, 854}));

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 4,
                api_event_id = 4
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[1].Count.NowArray.SequenceEqual(new[] {0, 0, 0, 0}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 4,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 6,
                api_mapinfo_no = 1,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 6,
                api_mapinfo_no = 3,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 6,
                api_mapinfo_no = 4,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[1].Count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);
            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 4,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[1].Count.NowArray.SequenceEqual(new[] {2, 1, 1, 1}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1);
        }

        /// <summary>
        /// 861: 強行輸送艦隊、抜錨！
        /// </summary>
        [TestMethod]
        public void MapNext_861()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {861}));

            _battleInfo.InjectResultStatus(
                ShipStatusList(10, 22, 2, 2, 2, 2), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 6,
                api_event_id = 4
            }));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 6,
                api_event_id = 8
            }));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1);

            _battleInfo.Result.Friend.Main[1].NowHp = 0;
            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 6,
                api_event_id = 8
            }));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1, "轟沈あり");
            _battleInfo.Result.Friend.Main[1].NowHp = 1;

            _battleInfo.Result.Friend.Main[2].Spec.ShipType = 10;
            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 6,
                api_event_id = 8
            }));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1, "補給・航戦が3隻");
        }

        /// <summary>
        /// 862: 前線の航空偵察を実施せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_862()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {862}));

            _battleInfo.InjectResultStatus(
                ShipStatusList(2, 3, 3, 2, 2, 16), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 6,
                api_mapinfo_no = 3,
                api_event_id = 4
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 6,
                api_mapinfo_no = 3,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "B"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1);

            _battleInfo.Result.Friend.Main[1].NowHp = 0;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1, "轟沈あり");
            _battleInfo.Result.Friend.Main[1].NowHp = 1;

            _battleInfo.Result.Friend.Main[3].Spec.ShipType = 3;
            _battleInfo.Result.Friend.Main[4].Spec.ShipType = 16;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 2, "軽巡3隻水母2隻");
        }

        /// <summary>
        /// 873: 北方海域警備を実施せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_873()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {873}));

            _battleInfo.InjectResultStatus(
                ShipStatusList(3, 2, 2, 2, 2, 2), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 3,
                api_mapinfo_no = 1,
                api_event_id = 4
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => _questInfo.Quests[0].Count.NowArray[0] == 0);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 3,
                api_mapinfo_no = 1,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "B"}));
            PAssert.That(() => _questInfo.Quests[0].Count.NowArray[0] == 0);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => _questInfo.Quests[0].Count.NowArray[0] == 1);

            _battleInfo.Result.Friend.Main[0].Spec.ShipType = 2;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => _questInfo.Quests[0].Count.NowArray[0] == 1, "軽巡なし");
            _battleInfo.Result.Friend.Main[0].Spec.ShipType = 3;

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 3,
                api_mapinfo_no = 2,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 3,
                api_mapinfo_no = 3,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => _questInfo.Quests[0].Count.NowArray.SequenceEqual(new[] {1, 1, 1}));
        }

        /// <summary>
        /// 875: 精鋭「三一駆」、鉄底海域に突入せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_875()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {875}));

            _battleInfo.InjectResultStatus(new[]
            {
                ShipStatus(2, 543), ShipStatus(8, 360), ShipStatus(11, 545),
                ShipStatus(18, 467), ShipStatus(11, 261), ShipStatus(2, 344)
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 5,
                api_mapinfo_no = 4,
                api_event_id = 4
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 5,
                api_mapinfo_no = 4,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 0);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1);

            _battleInfo.Result.Friend.Main[5].NowHp = 0;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1, "朝霜改轟沈");
            _battleInfo.Result.Friend.Main[5].NowHp = 1;

            _battleInfo.Result.Friend.Main[0].Spec.Id = 345;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 1, "長波改二なし");
            _battleInfo.Result.Friend.Main[0].Spec.Id = 543;

            _battleInfo.Result.Friend.Main[5].Spec.Id = 345;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 2, "高波改");
            _battleInfo.Result.Friend.Main[5].Spec.Id = 359;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[0].Count.Now == 3, "沖波改");
        }

        /// <summary>
        /// 888: 新編成「三川艦隊」、鉄底海峡に突入せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_888()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {888}));
            var count = _questInfo.Quests[0].Count;

            _battleInfo.InjectResultStatus(new[]
            {
                ShipStatus(5, 427), ShipStatus(5, 264), ShipStatus(5, 142),
                ShipStatus(5, 417), ShipStatus(2, 144), ShipStatus(2, 195)
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 5,
                api_mapinfo_no = 1,
                api_event_id = 4
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray[0] == 0);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 5,
                api_mapinfo_no = 1,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => count.NowArray[0] == 0);
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray[0] == 1);

            _battleInfo.Result.Friend.Main[0].NowHp = 0;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray[0] == 1, "轟沈あり");
            _battleInfo.Result.Friend.Main[0].NowHp = 1;

            _battleInfo.Result.Friend.Main[0].Spec.Id = 319;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray[0] == 1, "三川艦隊3隻");
            _battleInfo.Result.Friend.Main[0].Spec.Id = 427;

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 5,
                api_mapinfo_no = 3,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 0}));

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 5,
                api_mapinfo_no = 4,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray.SequenceEqual(new[] {1, 1, 1}));
        }

        /// <summary>
        /// 893: 泊地周辺海域の安全確保を徹底せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_893()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {893}));
            var count = _questInfo.Quests[0].Count;

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 5,
                api_event_id = 4
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray[0] == 0, "1-5");

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 5,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => count.NowArray[0] == 0, "A勝利はカウントしない");
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray[0] == 1, "1-5");

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 7,
                api_mapinfo_no = 1,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray[1] == 1, "7-1");

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 7,
                api_mapinfo_no = 2,
                api_no = 7,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray[2] == 1, "7-2G");

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 7,
                api_mapinfo_no = 2,
                api_no = 15,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray[3] == 1, "7-2M");
        }

        /// <summary>
        /// 894: 空母戦力の投入による兵站線戦闘哨戒
        /// </summary>
        [TestMethod]
        public void BattleResult_894()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {894}));
            var count = _questInfo.Quests[0].Count;
            _battleInfo.InjectResultStatus(
                ShipStatusList(2, 2, 2, 2, 2, 2),
                new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 3,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray[0] == 0, "空母なしはカウントしない");

            _battleInfo.Result.Friend.Main[0].Spec.ShipType = 7;
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => count.NowArray[0] == 0, "A勝利はカウントしない");

            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray[0] == 1, "1-3");

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 4,
                api_event_id = 4
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray[1] == 0, "1-4");

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 4,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray[1] == 1, "1-4");

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 1,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray[2] == 1, "2-1");

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 2,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray[3] == 1, "2-2");

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 3,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => count.NowArray[4] == 1, "2-3");
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

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 4,
                api_event_id = 5
            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[1].Count.NowArray[0] == 1);
        }

        /// <summary>
        /// 888と893以降を同時に遂行していると893以降がカウントされないことがある
        /// </summary>
        [TestMethod]
        public void BattleResult_888_893()
        {
            _questInfo.InspectQuestList(CreateQuestList(new []{888, 893}));

            _battleInfo.InjectResultStatus(
                ShipStatusList(1, 1, 1, 1, 1, 1), new ShipStatus[0],
                new ShipStatus[0], new ShipStatus[0]);

            _questCounter.InspectMapNext(Js(new
            {
                api_maparea_id = 7,
                api_mapinfo_no = 1,
                api_event_id = 5

            }));
            _questCounter.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => _questInfo.Quests[1].Count.NowArray[1] == 1);
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
            _questCounter.InspectPracticeResult(Js(new {api_win_rank = "C"}));
            _questCounter.InspectPracticeResult(Js(new {api_win_rank = "A"}));
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
            _questInfo.InspectQuestList(CreateQuestList(new[] {318}));
            var q318 = _questInfo.Quests[0];

            _battleInfo.InjectResultStatus(new[]
            {
                ShipStatus(2, 543), ShipStatus(3, 488)
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);

            _questCounter.InspectPracticeResult(Js(new {api_win_rank = "B"}));
            PAssert.That(() => q318.Count.Now == 0, "軽巡1隻");
            _battleInfo.Result.Friend.Main[0] = ShipStatus(3, 200);
            _questCounter.StartPractice("api%5Fdeck%5Fid=2");
            _questCounter.InspectPracticeResult(Js(new {api_win_rank = "B"}));
            PAssert.That(() => q318.Count.Now == 0, "第2艦隊");
            _questCounter.StartPractice("api%5Fdeck%5Fid=1"); // 第一艦隊
            _questCounter.InspectPracticeResult(Js(new {api_win_rank = "C"}));
            PAssert.That(() => q318.Count.Now == 0, "敗北");
            _questCounter.InspectPracticeResult(Js(new {api_win_rank = "B"}));
            PAssert.That(() => q318.Count.Now == 1);

            q318.Count.Now = 2;
            _questInfo.InspectQuestList(CreateQuestList(new[] {318}));
            PAssert.That(() => q318.Count.Now == 2, "進捗調節しない");
        }

        /// <summary>
        /// 330: 空母機動部隊、演習始め！
        /// </summary>
        [TestMethod]
        public void PracticeResult_330()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {330}));
            var q330 = _questInfo.Quests[0];

            _battleInfo.InjectResultStatus(
                ShipStatusList(18, 7, 2, 2),
                new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);
            _questCounter.InspectPracticeResult(Js(new {api_win_rank = "B"}));
            Assert.AreEqual(1, q330.Count.Now, "装甲空母、軽空母");

            _battleInfo.Result.Friend.Main = ShipStatusList(11, 7, 2, 2);
            _questCounter.InspectPracticeResult(Js(new {api_win_rank = "B"}));
            Assert.AreEqual(2, q330.Count.Now, "正規空母、軽空母");

            q330.Count.Now = 0;
            _questCounter.InspectPracticeResult(Js(new {api_win_rank = "C"}));
            Assert.AreEqual(0, q330.Count.Now, "敗北");

            _battleInfo.Result.Friend.Main = ShipStatusList(2, 7, 11, 2);
            _questCounter.InspectPracticeResult(Js(new {api_win_rank = "B"}));
            Assert.AreEqual(0, q330.Count.Now, "旗艦空母以外");

            _battleInfo.Result.Friend.Main = ShipStatusList(11, 2, 2, 2);
            _questCounter.InspectPracticeResult(Js(new {api_win_rank = "B"}));
            Assert.AreEqual(0, q330.Count.Now, "空母一隻");

            _battleInfo.Result.Friend.Main = ShipStatusList(11, 7, 3, 2);
            _questCounter.InspectPracticeResult(Js(new {api_win_rank = "B"}));
            Assert.AreEqual(0, q330.Count.Now, "駆逐一隻");
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
            _questInfo.InspectQuestList(CreateQuestList(new[] {426}));

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
            PAssert.That(() =>
                _questInfo.Quests[0].Count.NowArray.SequenceEqual(new[] {1, 1, 1, 0}));
            _questCounter.InspectDeck(Js(
                new[]
                {
                    new {api_id = 2, api_mission = new[] {2, 10}}
                }));
            _questCounter.InspectMissionResult("api%5Fdeck%5Fid=2", Js(new {api_clear_result = 1}));
            PAssert.That(() =>
                _questInfo.Quests[0].Count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}));
        }

        /// <summary>
        /// 428: 近海に侵入する敵潜を制圧せよ！
        /// </summary>
        [TestMethod]
        public void MissionResult_428()
        {
            _questInfo.InspectQuestList(CreateQuestList(new[] {428}));

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
            PAssert.That(() =>
                _questInfo.Quests[0].Count.NowArray.SequenceEqual(new[] {1, 1, 1}));
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

            _questCounter.CountCreateItem();
            _questCounter.CountCreateShip();
            _questCounter.InspectDestroyShip("api%5Fship%5Fid=98159%2C98166%2C98168&api%5Fverno=1");
            _questCounter.CountRemodelSlot();
            PAssert.That(() =>
                _questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[]
                    {
                        new {Id = 605, Now = 1}, new {Id = 606, Now = 1}, new {Id = 607, Now = 1},
                        new {Id = 608, Now = 1}, new {Id = 609, Now = 3}, new {Id = 619, Now = 1}
                    }));
        }

        /// <summary>
        /// 613: 資源の再利用
        /// 638: 対空機銃量産
        /// 643: 主力「陸攻」の調達
        /// 645: 「洋上補給」物資の調達
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
        public void DestroyItem_613_638_643_645_663_673_674_675_676_677_678_680_688()
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
                new ItemSpec {Id = 16, Name = "九七式艦攻", Type = 8}
            });
            var items = new[] {1, 37, 19, 4, 11, 75, 7, 25, 13, 20, 28, 31, 35, 23, 16};
            _itemInfo.InjectItems(items);
            var questList = new[] {613, 638, 643, 645, 663, 673, 674, 675, 676, 677, 678, 680, 688};
            _questInfo.AcceptMax = questList.Length;
            _questInfo.InspectQuestList(CreateQuestList(questList));
            _questCounter.InspectDestroyItem(
                $"api%5Fslotitem%5Fids={string.Join("%2C", Enumerable.Range(1, items.Length))}&api%5Fverno=1", null);
            var scalar = new[]
            {
                new {Id = 613, Now = 1}, new {Id = 638, Now = 1}, new {Id = 643, Now = 1}, new {Id = 645, Now = 1},
                new {Id = 663, Now = 1}, new {Id = 673, Now = 1}, new {Id = 674, Now = 1}
            };
            foreach (var e in scalar)
            {
                var c = Array.Find(_questInfo.Quests, q => q.Id == e.Id).Count;
                PAssert.That(() => c.Id == e.Id && c.Now == e.Now, $"{c.Id}");
            }
            var array = new[]
            {
                new {Id = 675, NowArray = new[] {2, 1}}, new {Id = 676, NowArray = new[] {1, 1, 1}},
                new {Id = 677, NowArray = new[] {1, 1, 1}}, new {Id = 678, NowArray = new[] {1, 1}},
                new {Id = 680, NowArray = new[] {1, 2}}, new {Id = 688, NowArray = new[] {2, 1, 1, 1}}
            };
            foreach (var e in array)
            {
                var c = Array.Find(_questInfo.Quests, q => q.Id == e.Id).Count;
                PAssert.That(() => c.Id == e.Id && c.NowArray.SequenceEqual(e.NowArray), $"{c.Id}");
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