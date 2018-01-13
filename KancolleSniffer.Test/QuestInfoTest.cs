﻿// Copyright (C) 2017 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
            questInfo.InspectQuestList(Js(new
            {
                api_list = new[]
                    {new {api_no = 201, api_category = 2, api_state = 2, api_title = "", api_progress_flag = 0}}
            }));
            questInfo.SaveState(status);
            PAssert.That(() =>
                status.QuestCountList.Select(qc => new {qc.Id, qc.Now}).SequenceEqual(new[]
                    {new {Id = 213, Now = 1}, new {Id = 822, Now = 1}})); // デイリーとマンスリーが消える
            questInfo.InspectQuestList(Js(new
            {
                api_list = new[]
                    {new {api_no = 201, api_category = 2, api_state = 2, api_title = "", api_progress_flag = 0}}
            }));
            questInfo.SaveState(status);
            PAssert.That(() =>
                status.QuestCountList.Select(qc => new {qc.Id, qc.Now}).SequenceEqual(new[]
                    {new {Id = 822, Now = 1}})); // ウィークリーが消える
            questInfo.InspectQuestList(Js(new
            {
                api_list = new[]
                    {new {api_no = 201, api_category = 2, api_state = 2, api_title = "", api_progress_flag = 0}}
            }));
            questInfo.SaveState(status);
            PAssert.That(() => status.QuestCountList.Length == 0); // クォータリーが消える
        }

        private JsonObject Js(object obj) => JsonObject.CreateJsonObject(obj);

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
            questInfo.InspectQuestList(Js(new
            {
                api_list = new[]
                {
                    new {api_no = 201, api_category = 2, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 210, api_category = 2, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 214, api_category = 2, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 216, api_category = 2, api_state = 2, api_title = "", api_progress_flag = 0}
                }
            }));

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
        public void BattleResult_211_212_218_213_220_221()
        {
            var battleInfo = new BattleInfo(null, null);
            var questInfo = new QuestInfo(null, battleInfo, () => new DateTime(2015, 1, 1)) {AcceptMax = 6};
            questInfo.InspectQuestList(Js(new
            {
                api_list = new[]
                {
                    new {api_no = 211, api_category = 2, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 212, api_category = 2, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 213, api_category = 2, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 218, api_category = 2, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 220, api_category = 2, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 221, api_category = 2, api_state = 2, api_title = "", api_progress_flag = 0}
                }
            }));
            // 補給艦1隻と空母2隻
            battleInfo.InjectEnemyResultStatus(new[]
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
            questInfo.InspectQuestList(Js(new
            {
                api_list = new[]
                {
                    new {api_no = 228, api_category = 2, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 230, api_category = 2, api_state = 2, api_title = "", api_progress_flag = 0}
                }
            }));
            // 潜水艦3
            battleInfo.InjectEnemyResultStatus(new[]
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
            questInfo.InspectQuestList(Js(new
            {
                api_list = new[]
                {
                    new {api_no = 226, api_category = 2, api_state = 2, api_title = "", api_progress_flag = 0}
                }
            }));

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
        /// 822: 沖ノ島海域迎撃戦
        /// 854: 戦果拡張任務！「Z作戦」前段作戦
        /// </summary>
        [TestMethod]
        public void BattleResult_822_854()
        {
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(Js(new
            {
                api_list = new[]
                {
                    new {api_no = 822, api_category = 8, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 854, api_category = 8, api_state = 2, api_title = "", api_progress_flag = 0}
                }
            }));

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
        /// 302: 大規模演習
        /// 303: 「演習」で練度向上！
        /// 304: 「演習」で他提督を圧倒せよ！
        /// 311: 精鋭艦隊演習
        /// </summary>
        [TestMethod]
        public void PracticeResult_303_304_302_311()
        {
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(Js(new
            {
                api_list = new[]
                {
                    new {api_no = 302, api_category = 3, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 303, api_category = 3, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 304, api_category = 3, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 311, api_category = 3, api_state = 2, api_title = "", api_progress_flag = 0}
                }
            }));

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
            questInfo.InspectQuestList(Js(new
            {
                api_list = new[]
                {
                    new {api_no = 402, api_category = 4, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 403, api_category = 4, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 404, api_category = 4, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 410, api_category = 4, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 411, api_category = 4, api_state = 2, api_title = "", api_progress_flag = 0}
                }
            }));

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
        /// 503: 艦隊大整備！
        /// 504: 艦隊酒保祭り！
        /// </summary>
        [TestMethod]
        public void Powerup_503_504()
        {
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(Js(new
            {
                api_list = new[]
                {
                    new {api_no = 503, api_category = 5, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 504, api_category = 5, api_state = 2, api_title = "", api_progress_flag = 0}
                }
            }));

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
            questInfo.InspectQuestList(Js(new
            {
                api_list = new[]
                {
                    new {api_no = 605, api_category = 6, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 606, api_category = 6, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 607, api_category = 6, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 608, api_category = 6, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 609, api_category = 6, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 619, api_category = 6, api_state = 2, api_title = "", api_progress_flag = 0}
                }
            }));

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
        /// 673: 装備開発力の整備
        /// 674: 工廠環境の整備
        /// 675: 運用装備の統合整備
        /// 676: 装備開発力の集中整備
        /// </summary>
        [TestMethod]
        public void DestroyItem_613_638_673_674_675_676()
        {
            var itemInfo = new ItemInfo();
            var questInfo = new QuestInfo(itemInfo, null, () => new DateTime(2015, 1, 1)) {AcceptMax = 6};

            itemInfo.InjectItemSpec(new[]
            {
                new ItemSpec {Id = 1, Name = "12cm単装砲", Type = 1},
                new ItemSpec {Id = 37, Name = "7.7mm機銃", Type = 21},
                new ItemSpec {Id = 19, Name = "九六式艦戦", Type = 6},
                new ItemSpec {Id = 4, Name = "14cm単装砲", Type = 2},
                new ItemSpec {Id = 11, Name = "15.2cm単装砲", Type = 4},
                new ItemSpec {Id = 75, Name = "ドラム缶(輸送用)", Type = 30}
            });
            itemInfo.InjectItems(new[] {1, 37, 19, 4, 11, 75});
            questInfo.InspectQuestList(Js(new
            {
                api_list = new[]
                {
                    new {api_no = 613, api_category = 6, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 638, api_category = 6, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 673, api_category = 6, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 674, api_category = 6, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 675, api_category = 6, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 676, api_category = 6, api_state = 2, api_title = "", api_progress_flag = 0}
                }
            }));
            questInfo.InspectDestroyItem("api%5Fslotitem%5Fids=1%2C2%2C3%2C4%2C5%2C6&api%5Fverno=1", null);
            PAssert.That(() =>
                questInfo.Quests.Select(q => new {q.Id, q.Count.Now}).Take(4).SequenceEqual(new[]
                {
                    new {Id = 613, Now = 1}, new {Id = 638, Now = 1},
                    new {Id = 673, Now = 1}, new {Id = 674, Now = 1}
                }));
            var q675 = questInfo.Quests[4];
            PAssert.That(() => q675.Id == 675 && q675.Count.NowArray.SequenceEqual(new[] {1, 1}));
            var q676 = questInfo.Quests[5];
            PAssert.That(() => q676.Id == 676 && q676.Count.NowArray.SequenceEqual(new[] {1, 1, 1}));
        }

        /// <summary>
        /// 702: 艦の「近代化改修」を実施せよ！
        /// 703: 「近代化改修」を進め、戦備を整えよ！
        /// </summary>
        [TestMethod]
        public void Powerup_702_703()
        {
            var questInfo = new QuestInfo(null, null, () => new DateTime(2015, 1, 1));

            questInfo.InspectQuestList(Js(new
            {
                api_list = new[]
                {
                    new {api_no = 702, api_category = 7, api_state = 2, api_title = "", api_progress_flag = 0},
                    new {api_no = 703, api_category = 7, api_state = 2, api_title = "", api_progress_flag = 0}
                }
            }));
            questInfo.InspectPowerup(Js(new {api_powerup_flag = 1}));
            PAssert.That(() =>
                questInfo.Quests.Select(q => new {q.Id, q.Count.Now})
                    .SequenceEqual(new[] {new {Id = 702, Now = 1}, new {Id = 703, Now = 1}}));
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
                    new QuestCount {Id = 854, NowArray = new[] {1, 1, 1, 1}}
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
        }
    }
}