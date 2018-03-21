// Copyright (C) 2017 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class QuestInfoTest
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
        public void AdjestCountNowArray()
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

        [TestMethod]
        public void ResetQuest()
        {
            var queue = new Queue<DateTime>(new[]
            {
                new DateTime(2017, 11, 1, 5, 0, 0), new DateTime(2017, 11, 6, 5, 0, 0),
                new DateTime(2017, 12, 1, 5, 0, 0)
            });
            var questInfo = new QuestInfo(null, null, () => queue.Dequeue());
            var status = new Status
            {
                QuestCountList = new[]
                {
                    new QuestCount {Id = 201, Now = 1}, new QuestCount {Id = 213, Now = 1},
                    new QuestCount {Id = 265, Now = 1}, new QuestCount {Id = 822, Now = 1}
                },
                QuestLastReset = new DateTime(2017, 10, 31, 5, 0, 0)
            };
            questInfo.LoadState(status);
            questInfo.InspectQuestList(CreateQuestList(new[] {201}));
            questInfo.SaveState(status);
            PAssert.That(() =>
                status.QuestCountList.Select(qc => new {qc.Id, qc.Now}).SequenceEqual(new[]
                    {new {Id = 213, Now = 1}, new {Id = 822, Now = 1}})); // デイリーとマンスリーが消える
            questInfo.InspectQuestList(CreateQuestList(new[] {201}));
            questInfo.SaveState(status);
            PAssert.That(() =>
                status.QuestCountList.Select(qc => new {qc.Id, qc.Now}).SequenceEqual(new[]
                    {new {Id = 822, Now = 1}})); // ウィークリーが消える
            questInfo.InspectQuestList(CreateQuestList(new[] {201}));
            questInfo.SaveState(status);
            PAssert.That(() => status.QuestCountList.Length == 0); // クォータリーが消える
        }

        private JsonObject Js(object obj) => JsonObject.CreateJsonObject(obj);

        private object CreateQuestList(int[] ids) => Js(new
        {
            api_list =
            ids.Select(id => new
            {
                api_no = id,
                api_category = id / 100,
                api_state = 2,
                api_title = "",
                api_detail = "",
                api_get_material = new int[0],
                api_progress_flag = 0
            })
        });

        /// <summary>
        /// 201: 敵艦隊を撃滅せよ！
        /// 210: 敵艦隊を10回邀撃せよ！
        /// 214: あ号
        /// 216: 敵艦隊主力を撃滅せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_201_216_210_214()
        {
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {201, 216, 210, 214}));

            questInfo.InspectMapStart(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 1,
                api_event_id = 4
            }));
            var quests = questInfo.Quests;
            // 出撃カウント
            PAssert.That(() => quests[2].Id == 214 && quests[2].Count.NowArray[0] == 1);
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            // 道中S勝利
            PAssert.That(() => quests.Select(q => new {q.Id, q.Count.Now}).SequenceEqual(new[]
            {
                new {Id = 201, Now = 1}, new {Id = 210, Now = 1},
                new {Id = 214, Now = 0}, new {Id = 216, Now = 1}
            }));
            PAssert.That(() => quests[2].Id == 214 &&
                               quests[2].Count.NowArray.SequenceEqual(new[] {1, 1, 0, 0}));

            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 1,
                api_event_id = 5
            }));
            // ボスB勝利
            questInfo.InspectBattleResult(Js(new {api_win_rank = "B"}));
            PAssert.That(() => quests.Select(q => new {q.Id, q.Count.Now}).SequenceEqual(new[]
            {
                new {Id = 201, Now = 2}, new {Id = 210, Now = 2},
                new {Id = 214, Now = 0}, new {Id = 216, Now = 2}
            }));
            // ボス敗北
            PAssert.That(() => quests[2].Id == 214 && quests[2].Count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "C"}));
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
            var battleInfo = new BattleInfo(null, null);
            var questInfo = new QuestInfo(null, battleInfo, () => new DateTime(2015, 1, 1)) {AcceptMax = 6};
            questInfo.InspectQuestList(CreateQuestList(new[] {211, 212, 213, 218, 220, 221}));
            // 補給艦1隻と空母2隻
            battleInfo.InjectResultStatus(new ShipStatus[0], new ShipStatus[0], new[]
            {
                new ShipStatus {NowHp = 0, MaxHp = 130, Spec = new ShipSpec {Id = 1558, ShipType = 15}},
                new ShipStatus {NowHp = 0, MaxHp = 90, Spec = new ShipSpec {Id = 1543, ShipType = 8}},
                new ShipStatus {NowHp = 0, MaxHp = 90, Spec = new ShipSpec {Id = 1543, ShipType = 8}},
                new ShipStatus {NowHp = 0, MaxHp = 96, Spec = new ShipSpec {Id = 1528, ShipType = 11}},
                new ShipStatus {NowHp = 0, MaxHp = 70, Spec = new ShipSpec {Id = 1523, ShipType = 7}},
                new ShipStatus {NowHp = 1, MaxHp = 70, Spec = new ShipSpec {Id = 1523, ShipType = 7}}
            }, new ShipStatus[0]);
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() =>
                questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
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
            var battleInfo = new BattleInfo(null, null);
            var questInfo = new QuestInfo(null, battleInfo, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {228, 230}));
            // 潜水艦3
            battleInfo.InjectResultStatus(new ShipStatus[0], new ShipStatus[0], new[]
            {
                new ShipStatus {NowHp = 0, MaxHp = 27, Spec = new ShipSpec {Id = 1532, ShipType = 13}},
                new ShipStatus {NowHp = 0, MaxHp = 19, Spec = new ShipSpec {Id = 1530, ShipType = 13}},
                new ShipStatus {NowHp = 0, MaxHp = 19, Spec = new ShipSpec {Id = 1530, ShipType = 13}}
            }, new ShipStatus[0]);
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() =>
                questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
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
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {226}));

            questInfo.InspectMapStart(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 1,
                api_event_id = 4
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() =>
                questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[] {new {Id = 226, Now = 0}}));
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 1,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() =>
                questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[] {new {Id = 226, Now = 1}}));
            questInfo.InspectMapStart(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 1,
                api_event_id = 4
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() =>
                questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[] {new {Id = 226, Now = 1}}));
        }

        /// <summary>
        /// // 243: 南方海域珊瑚諸島沖の制空権を握れ！
        /// </summary>
        [TestMethod]
        public void BattleResult_243()
        {
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {243}));

            questInfo.InspectMapStart(Js(new
            {
                api_maparea_id = 5,
                api_mapinfo_no = 2,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() =>
                questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[] {new {Id = 243, Now = 0}}));

            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 5,
                api_mapinfo_no = 2,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() =>
                questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[] {new {Id = 243, Now = 1}}));
        }

        private ShipStatus ShipStatus(int shipType, int specId = 0) =>
            new ShipStatus {NowHp = 1, Spec = new ShipSpec {Id = specId, ShipType = shipType}};


        /// <summary>
        /// 249: 「第五戦隊」出撃せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_249()
        {
            var battleInfo = new BattleInfo(null, null);
            var questInfo = new QuestInfo(null, battleInfo, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {249}));

            battleInfo.InjectResultStatus(new[]
            {
                ShipStatus(5, 319), ShipStatus(5, 192), ShipStatus(5, 194),
                ShipStatus(5, 193), ShipStatus(6, 189), ShipStatus(6, 188)
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 5,
                api_event_id = 4
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 5,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 0);
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1);
            questInfo.Quests[0].Count.Now = 0;

            battleInfo.Result.Friend.Main[1].NowHp = 0;
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 0, "那智改二轟沈");
        }

        /// <summary>
        /// 257: 「水雷戦隊」南西へ！
        /// </summary>
        [TestMethod]
        public void BattleResult_257()
        {
            var battleInfo = new BattleInfo(null, null);
            var questInfo = new QuestInfo(null, battleInfo, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {257}));

            battleInfo.InjectResultStatus(new[]
            {
                ShipStatus(3), ShipStatus(2), ShipStatus(2),
                ShipStatus(2), ShipStatus(2), ShipStatus(2)
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 4,
                api_event_id = 4
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 4,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 0);
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1);
            questInfo.Quests[0].Count.Now = 0;

            battleInfo.Result.Friend.Main[0].NowHp = 0;
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 0, "軽巡轟沈");
            battleInfo.Result.Friend.Main[0].NowHp = 1;

            battleInfo.Result.Friend.Main[0].Spec.ShipType = 2;
            battleInfo.Result.Friend.Main[1].Spec.ShipType = 3;
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 0, "旗艦が駆逐");
            battleInfo.Result.Friend.Main[0].Spec.ShipType = 3;

            battleInfo.Result.Friend.Main[2].Spec.ShipType = 3;
            battleInfo.Result.Friend.Main[3].Spec.ShipType = 3;
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 0, "軽巡が4隻");

            battleInfo.Result.Friend.Main[0].Spec.ShipType = 3;
            battleInfo.Result.Friend.Main[3].Spec.ShipType = 4;
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 0, "駆逐軽巡以外");
        }

        /// <summary>
        /// 257: 「水上打撃部隊」南方へ！
        /// </summary>
        [TestMethod]
        public void BattleResult_259()
        {
            var battleInfo = new BattleInfo(null, null);
            var questInfo = new QuestInfo(null, battleInfo, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {259}));

            battleInfo.InjectResultStatus(new[]
            {
                ShipStatus(3, 183), ShipStatus(9, 276), ShipStatus(10, 411),
                ShipStatus(10, 412), ShipStatus(5, 193), ShipStatus(5, 194)
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 5,
                api_mapinfo_no = 1,
                api_event_id = 4
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 5,
                api_mapinfo_no = 1,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 0);
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1);
            questInfo.Quests[0].Count.Now = 0;

            battleInfo.Result.Friend.Main[0].NowHp = 0;
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 0, "軽巡轟沈");
            battleInfo.Result.Friend.Main[0].NowHp = 1;

            battleInfo.Result.Friend.Main[4].Spec = new ShipSpec {Id = 136, ShipType = 9};
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 0, "戦艦4隻");
            battleInfo.Result.Friend.Main[4].Spec = new ShipSpec {Id = 193, ShipType = 5};

            battleInfo.Result.Friend.Main[0].Spec = new ShipSpec {Id = 58, ShipType = 4};
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 0, "軽巡なし");
        }

        /// <summary>
        /// 266: 「水上反撃部隊」突入せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_266()
        {
            var battleInfo = new BattleInfo(null, null);
            var questInfo = new QuestInfo(null, battleInfo, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {266}));

            battleInfo.InjectResultStatus(new[]
            {
                ShipStatus(2), ShipStatus(5), ShipStatus(3),
                ShipStatus(2), ShipStatus(2), ShipStatus(2)
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 5,
                api_event_id = 4
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 5,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 0);
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1);

            battleInfo.Result.Friend.Main[1].NowHp = 0;
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1, "轟沈あり");
            battleInfo.Result.Friend.Main[1].NowHp = 1;

            battleInfo.Result.Friend.Main[0].Spec.ShipType = 3;
            battleInfo.Result.Friend.Main[2].Spec.ShipType = 2;
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1, "旗艦が軽巡");
            battleInfo.Result.Friend.Main[0].Spec.ShipType = 2;
            battleInfo.Result.Friend.Main[2].Spec.ShipType = 3;

            battleInfo.Result.Friend.Main[3].Spec.ShipType = 3;
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1, "軽巡が2隻");
        }

        /// <summary>
        /// 822: 沖ノ島海域迎撃戦
        /// 854: 戦果拡張任務！「Z作戦」前段作戦
        /// </summary>
        [TestMethod]
        public void BattleResult_822_854()
        {
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {822, 854}));

            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 4,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 6,
                api_mapinfo_no = 1,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 6,
                api_mapinfo_no = 3,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 6,
                api_mapinfo_no = 4,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[1].Count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 0);
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 2,
                api_mapinfo_no = 4,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[1].Count.NowArray.SequenceEqual(new[] {2, 1, 1, 1}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1);
        }

        /// <summary>
        /// 861: 強行輸送艦隊、抜錨！
        /// </summary>
        [TestMethod]
        public void MapNext_861()
        {
            var battleInfo = new BattleInfo(null, null);
            var questInfo = new QuestInfo(null, battleInfo, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {861}));

            battleInfo.InjectResultStatus(new[]
            {
                ShipStatus(10), ShipStatus(22), ShipStatus(2),
                ShipStatus(2), ShipStatus(2), ShipStatus(2)
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);

            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 6,
                api_event_id = 4
            }));
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 6,
                api_event_id = 8
            }));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1);

            battleInfo.Result.Friend.Main[1].NowHp = 0;
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 6,
                api_event_id = 8
            }));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1, "轟沈あり");
            battleInfo.Result.Friend.Main[1].NowHp = 1;

            battleInfo.Result.Friend.Main[2].Spec.ShipType = 10;
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 1,
                api_mapinfo_no = 6,
                api_event_id = 8
            }));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1, "補給・航戦が3隻");
        }

        /// <summary>
        /// 862: 前線の航空偵察を実施せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_862()
        {
            var battleInfo = new BattleInfo(null, null);
            var questInfo = new QuestInfo(null, battleInfo, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {862}));

            battleInfo.InjectResultStatus(new[]
            {
                ShipStatus(2), ShipStatus(3), ShipStatus(3),
                ShipStatus(2), ShipStatus(2), ShipStatus(16)
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 6,
                api_mapinfo_no = 3,
                api_event_id = 4
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 6,
                api_mapinfo_no = 3,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "B"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 0);
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1);

            battleInfo.Result.Friend.Main[1].NowHp = 0;
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1, "轟沈あり");
            battleInfo.Result.Friend.Main[1].NowHp = 1;

            battleInfo.Result.Friend.Main[4].Spec.ShipType = 16;
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1, "水母2隻");
        }

        /// <summary>
        /// 873: 北方海域警備を実施せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_873()
        {
            var battleInfo = new BattleInfo(null, null);
            var questInfo = new QuestInfo(null, battleInfo, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {873}));

            battleInfo.InjectResultStatus(new[]
            {
                ShipStatus(3), ShipStatus(2), ShipStatus(2),
                ShipStatus(2), ShipStatus(2), ShipStatus(2)
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 3,
                api_mapinfo_no = 1,
                api_event_id = 4
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 3,
                api_mapinfo_no = 1,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "B"}));
            PAssert.That(() => questInfo.Quests[0].Count.NowArray[0] == 0);
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => questInfo.Quests[0].Count.NowArray[0] == 1);

            battleInfo.Result.Friend.Main[0].Spec.ShipType = 2;
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => questInfo.Quests[0].Count.NowArray[0] == 1, "軽巡なし");
            battleInfo.Result.Friend.Main[0].Spec.ShipType = 3;

            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 3,
                api_mapinfo_no = 2,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 3,
                api_mapinfo_no = 3,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => questInfo.Quests[0].Count.NowArray.SequenceEqual(new[] {1, 1, 1}));
        }

        /// <summary>
        /// 875: 精鋭「三一駆」、鉄底海域に突入せよ！
        /// </summary>
        [TestMethod]
        public void BattleResult_875()
        {
            var battleInfo = new BattleInfo(null, null);
            var questInfo = new QuestInfo(null, battleInfo, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {875}));

            battleInfo.InjectResultStatus(new[]
            {
                ShipStatus(2, 543), ShipStatus(8, 360), ShipStatus(11, 545),
                ShipStatus(18, 467), ShipStatus(11, 261), ShipStatus(2, 344)
            }, new ShipStatus[0], new ShipStatus[0], new ShipStatus[0]);
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 5,
                api_mapinfo_no = 4,
                api_event_id = 4
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            questInfo.InspectMapNext(Js(new
            {
                api_maparea_id = 5,
                api_mapinfo_no = 4,
                api_event_id = 5
            }));
            questInfo.InspectBattleResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 0);
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1);

            battleInfo.Result.Friend.Main[5].NowHp = 0;
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1, "朝霜改轟沈");
            battleInfo.Result.Friend.Main[5].NowHp = 1;

            battleInfo.Result.Friend.Main[0].Spec.Id = 345;
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 1, "長波改二なし");
            battleInfo.Result.Friend.Main[0].Spec.Id = 543;

            battleInfo.Result.Friend.Main[5].Spec.Id = 345;
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 2, "高波改");
            battleInfo.Result.Friend.Main[5].Spec.Id = 359;
            questInfo.InspectBattleResult(Js(new {api_win_rank = "S"}));
            PAssert.That(() => questInfo.Quests[0].Count.Now == 3, "沖波改");
        }

        /// <summary>
        /// 302: 大規模演習
        /// 303: 「演習」で練度向上！
        /// 304: 「演習」で他提督を圧倒せよ！
        /// 311: 精鋭艦隊演習
        /// </summary>
        [TestMethod]
        public void PracticeResult_303_304_302_311()
        {
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {302, 303, 304, 311}));

            questInfo.InspectPracticeResult(Js(new {api_win_rank = "C"}));
            questInfo.InspectPracticeResult(Js(new {api_win_rank = "A"}));
            PAssert.That(() =>
                questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[]
                    {
                        new {Id = 302, Now = 1}, new {Id = 303, Now = 2}, new {Id = 304, Now = 1},
                        new {Id = 311, Now = 1}
                    }));
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
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {402, 403, 404, 410, 411}));

            questInfo.InspectDeck(Js(
                new[]
                {
                    new {api_id = 2, api_mission = new[] {2, 6}},
                    new {api_id = 3, api_mission = new[] {2, 37}},
                    new {api_id = 4, api_mission = new[] {2, 2}}
                }));
            questInfo.InspectMissionResult("api%5Fdeck%5Fid=2", Js(new {api_clear_result = 1}));
            questInfo.InspectMissionResult("api%5Fdeck%5Fid=3", Js(new {api_clear_result = 2}));
            questInfo.InspectMissionResult("api%5Fdeck%5Fid=4", Js(new {api_clear_result = 0}));
            PAssert.That(() =>
                questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
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
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {426}));

            questInfo.InspectDeck(Js(
                new[]
                {
                    new {api_id = 2, api_mission = new[] {2, 3}},
                    new {api_id = 3, api_mission = new[] {2, 4}},
                    new {api_id = 4, api_mission = new[] {2, 5}}
                }));
            questInfo.InspectMissionResult("api%5Fdeck%5Fid=2", Js(new {api_clear_result = 1}));
            questInfo.InspectMissionResult("api%5Fdeck%5Fid=3", Js(new {api_clear_result = 1}));
            questInfo.InspectMissionResult("api%5Fdeck%5Fid=4", Js(new {api_clear_result = 1}));
            PAssert.That(() =>
                questInfo.Quests[0].Count.NowArray.SequenceEqual(new[] {1, 1, 1, 0}));
            questInfo.InspectDeck(Js(
                new[]
                {
                    new {api_id = 2, api_mission = new[] {2, 10}}
                }));
            questInfo.InspectMissionResult("api%5Fdeck%5Fid=2", Js(new {api_clear_result = 1}));
            PAssert.That(() =>
                questInfo.Quests[0].Count.NowArray.SequenceEqual(new[] {1, 1, 1, 1}));
        }

        /// <summary>
        /// 428: 近海に侵入する敵潜を制圧せよ！
        /// </summary>
        [TestMethod]
        public void MissionResult_428()
        {
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {428}));

            questInfo.InspectDeck(Js(
                new[]
                {
                    new {api_id = 2, api_mission = new[] {2, 4}},
                    new {api_id = 3, api_mission = new[] {2, 101}},
                    new {api_id = 4, api_mission = new[] {2, 102}}
                }));
            questInfo.InspectMissionResult("api%5Fdeck%5Fid=2", Js(new {api_clear_result = 1}));
            questInfo.InspectMissionResult("api%5Fdeck%5Fid=3", Js(new {api_clear_result = 1}));
            questInfo.InspectMissionResult("api%5Fdeck%5Fid=4", Js(new {api_clear_result = 1}));
            PAssert.That(() =>
                questInfo.Quests[0].Count.NowArray.SequenceEqual(new[] {1, 1, 1}));
        }

        /// <summary>
        /// 503: 艦隊大整備！
        /// 504: 艦隊酒保祭り！
        /// </summary>
        [TestMethod]
        public void Powerup_503_504()
        {
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {503, 504}));

            questInfo.CountNyukyo();
            questInfo.CountCharge();
            PAssert.That(() =>
                questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
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
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1)) {AcceptMax = 6};
            questInfo.InspectQuestList(CreateQuestList(new[] {605, 606, 607, 608, 609, 619}));

            questInfo.CountCreateItem();
            questInfo.CountCreateShip();
            questInfo.InspectDestroyShip("api%5Fship%5Fid=98159%2C98166%2C98168&api%5Fverno=1");
            questInfo.CountRemodelSlot();
            PAssert.That(() =>
                questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[]
                    {
                        new {Id = 605, Now = 1}, new {Id = 606, Now = 1}, new {Id = 607, Now = 1},
                        new {Id = 608, Now = 1}, new {Id = 609, Now = 3}, new {Id = 619, Now = 1}
                    }));
        }

        /// <summary>
        /// 613: 資源の再利用
        /// 638: 対空機銃量産
        /// 663: 新型艤装の継続研究
        /// 673: 装備開発力の整備
        /// 674: 工廠環境の整備
        /// 675: 運用装備の統合整備
        /// 676: 装備開発力の集中整備
        /// 677: 継戦支援能力の整備
        /// </summary>
        [TestMethod]
        public void DestroyItem_613_638_663_673_674_675_676_677()
        {
            var itemInfo = new ItemInfo();
            var questInfo = new QuestInfo(itemInfo, null, () => new DateTime(2015, 1, 1)) {AcceptMax = 8};

            itemInfo.InjectItemSpec(new[]
            {
                new ItemSpec {Id = 1, Name = "12cm単装砲", Type = 1},
                new ItemSpec {Id = 37, Name = "7.7mm機銃", Type = 21},
                new ItemSpec {Id = 19, Name = "九六式艦戦", Type = 6},
                new ItemSpec {Id = 4, Name = "14cm単装砲", Type = 2},
                new ItemSpec {Id = 11, Name = "15.2cm単装砲", Type = 4},
                new ItemSpec {Id = 75, Name = "ドラム缶(輸送用)", Type = 30},
                new ItemSpec {Id = 7, Name = "35.6cm連装砲", Type = 3},
                new ItemSpec {Id = 25, Name = "零式水上偵察機", Type = 10},
                new ItemSpec {Id = 13, Name = "61cm三連装魚雷", Type = 5}
            });
            itemInfo.InjectItems(new[] {1, 37, 19, 4, 11, 75, 7, 25, 13});
            questInfo.InspectQuestList(CreateQuestList(new[] {613, 638, 663, 673, 674, 675, 676, 677}));
            questInfo.InspectDestroyItem("api%5Fslotitem%5Fids=1%2C2%2C3%2C4%2C5%2C6%2C7%2C8%2C9&api%5Fverno=1", null);
            PAssert.That(() =>
                questInfo.Quests.Select(q => new {q.Id, q.Count.Now}).Take(5).SequenceEqual(new[]
                {
                    new {Id = 613, Now = 1}, new {Id = 638, Now = 1}, new {Id = 663, Now = 1},
                    new {Id = 673, Now = 1}, new {Id = 674, Now = 1}
                }));
            var q675 = questInfo.Quests[5];
            PAssert.That(() => q675.Id == 675 && q675.Count.NowArray.SequenceEqual(new[] {1, 1}));
            var q676 = questInfo.Quests[6];
            PAssert.That(() => q676.Id == 676 && q676.Count.NowArray.SequenceEqual(new[] {1, 1, 1}));
            var q677 = questInfo.Quests[7];
            PAssert.That(() => q677.Id == 677 && q677.Count.NowArray.SequenceEqual(new[] {1, 1, 1}));
        }

        /// <summary>
        /// 702: 艦の「近代化改修」を実施せよ！
        /// 703: 「近代化改修」を進め、戦備を整えよ！
        /// </summary>
        [TestMethod]
        public void Powerup_702_703()
        {
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {702, 703}));
            questInfo.InspectPowerup(Js(new {api_powerup_flag = 1}));
            PAssert.That(() =>
                questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[] {new {Id = 702, Now = 1}, new {Id = 703, Now = 1}}));
        }

        [TestMethod]
        public void NotImplemented()
        {
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {318}));
            PAssert.That(() => questInfo.Quests[0].Count.Spec.Material.Length == 0);
        }

        /// <summary>
        /// 文字列表記にする
        /// </summary>
        [TestMethod]
        public void ToStringTest()
        {
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));
            var status = new Status
            {
                QuestCountList = new[]
                {
                    new QuestCount {Id = 211, Now = 2},
                    new QuestCount {Id = 214, NowArray = new[] {20, 7, 10, 8}},
                    new QuestCount {Id = 854, NowArray = new[] {2, 1, 1, 1}},
                    new QuestCount {Id = 426, NowArray = new[] {1, 1, 1, 1}},
                    new QuestCount {Id = 428, NowArray = new[] {1, 1, 1}},
                    new QuestCount {Id = 873, NowArray = new[] {1, 1, 1}}
                }
            };
            questInfo.LoadState(status);
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
        }

        /// <summary>
        /// 状態をロードするときに獲得資材に特殊資材のリストを追加しない
        /// </summary>
        [TestMethod]
        public void LoadStateNotAppendMaterialList()
        {
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));
            var status = new Status
            {
                QuestList = new[]
                {
                    new QuestStatus
                    {
                        Id = 854,
                        Category = 8,
                        Name = "",
                        Detail = "",
                        Material = new[] {0, 2000, 0, 0, 0, 0, 0, 4}
                    }
                },
                QuestCountList = new[]
                {
                    new QuestCount{Id = 854,NowArray = new []{1,0,1,0}}
                }
            };
            questInfo.LoadState(status);
            PAssert.That(() => questInfo.Quests[0].Material.Length == 8);
        }
    }
}