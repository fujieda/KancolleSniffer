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
using KancolleSniffer.Model;
using KancolleSniffer.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class QuestInfoTest
    {
        [TestMethod]
        public void ResetQuestCount()
        {
            var queue = new Queue<DateTime>(new[]
            {
                new DateTime(2017, 11, 1, 5, 0, 0), new DateTime(2017, 11, 6, 5, 0, 0),
                new DateTime(2017, 12, 1, 5, 0, 0), new DateTime(2018, 2, 1, 5, 0, 0)
            });
            var questInfo = new QuestInfo(() => queue.Dequeue());
            var status = new Status
            {
                QuestCountList = new[]
                {
                    new QuestCount {Id = 201, Now = 1}, new QuestCount {Id = 213, Now = 1},
                    new QuestCount {Id = 265, Now = 1}, new QuestCount {Id = 822, Now = 1},
                    new QuestCount {Id = 904, NowArray = new[] {1, 1, 1, 1}}
                },
                QuestLastReset = new DateTime(2017, 10, 31, 5, 0, 0)
            };
            questInfo.LoadState(status);
            questInfo.InspectQuestList(CreateQuestList(new[] {201}));
            questInfo.SaveState(status);
            PAssert.That(() =>
                status.QuestCountList.Select(qc => qc.Id).SequenceEqual(new[] {213, 822, 904})); // デイリーとマンスリーが消える
            questInfo.InspectQuestList(CreateQuestList(new[] {201}));
            questInfo.SaveState(status);
            PAssert.That(() => status.QuestCountList.Select(qc => qc.Id).SequenceEqual(new[] {822, 904})); // ウィークリーが消える
            questInfo.InspectQuestList(CreateQuestList(new[] {201}));
            questInfo.SaveState(status);
            PAssert.That(() => status.QuestCountList.Select(qc => qc.Id).SequenceEqual(new[] {904})); // クォータリーが消える
            questInfo.InspectQuestList(CreateQuestList(new[] {201}));
            questInfo.SaveState(status);
            PAssert.That(() => status.QuestCountList.Length == 0); // イヤーリーが消える
        }

        [TestMethod]
        public void ResetQuestList()
        {
            var queue = new Queue<DateTime>(new[]
            {
                new DateTime(2017, 11, 1, 5, 0, 0), new DateTime(2017, 11, 6, 5, 0, 0),
                new DateTime(2017, 12, 1, 5, 0, 0)
            });
            var questInfo = new QuestInfo(() => queue.Dequeue());
            var status = new Status
            {
                QuestList = new[]
                {
                    new QuestStatus {Id = 201, Category = 2}, new QuestStatus {Id = 213, Category = 2},
                    new QuestStatus {Id = 265, Category = 2}, new QuestStatus {Id = 822, Category = 8}
                },
                QuestLastReset = new DateTime(2017, 10, 31, 5, 0, 0)
            };
            questInfo.LoadState(status);
            questInfo.InspectQuestList(CreateQuestList(new int[0]));
            questInfo.SaveState(status);
            PAssert.That(() => status.QuestList.Select(q => q.Id).SequenceEqual(new[] {213, 822})); // デイリーとマンスリーが消える
            questInfo.InspectQuestList(CreateQuestList(new int[0]));
            questInfo.SaveState(status);
            PAssert.That(() => status.QuestList.Select(q => q.Id).SequenceEqual(new[] {822})); // ウィークリーが消える
            questInfo.InspectQuestList(CreateQuestList(new int[0]));
            questInfo.SaveState(status);
            PAssert.That(() => status.QuestList.Length == 0); // クォータリーが消える
        }

        [TestMethod]
        public void ResetFrom0To5OClock()
        {
            var queue = new Queue<DateTime>(new[]
            {
                new DateTime(2019, 1, 22, 4, 0, 0)
            });
            var questInfo = new QuestInfo(() => queue.Dequeue());
            var status = new Status
            {
                QuestCountList = new[] {new QuestCount {Id = 213, Now = 1}},
                QuestLastReset = new DateTime(2019, 1, 20, 5, 16, 22)
            };
            questInfo.LoadState(status);
            questInfo.InspectQuestList(CreateQuestList(new[] {201}));
            questInfo.SaveState(status);
            PAssert.That(() => status.QuestCountList.Length == 0);
        }

        [TestMethod]
        public void ResetWeeklyWithoutCount()
        {
            var queue = new Queue<DateTime>(new[]
            {
                new DateTime(2019, 1, 27, 10, 0, 0),
                new DateTime(2019, 1, 28, 5, 0, 0)
            });
            var questInfo = new QuestInfo(() => queue.Dequeue());
            var status = new Status
            {
                QuestLastReset = new DateTime(2019, 1, 27, 5, 0, 0)
            };
            questInfo.LoadState(status);
            questInfo.InspectQuestList( // 2019-1-27 10:00
                CreateQuestList(new[] {237})); // 【節分拡張任務】南方海域 艦隊決戦
            PAssert.That(() => questInfo.Quests[0].Id == 237);
            questInfo.InspectQuestList(CreateQuestList(new[] {201})); // 2019-1-28 05:00
            PAssert.That(() => questInfo.Quests[0].Id == 201);
        }

        private JsonObject Js(object obj) => JsonObject.CreateJsonObject(obj);

        private object CreateQuestList(int[] ids) => Js(new
        {
            api_list = ids.Select(id => CreateQuest(id, 2))
        });

        private object CreateQuest(int id, int state)
        {
            return new
            {
                api_no = id,
                api_category = id / 100,
                api_type = 1,
                api_state = state,
                api_title = "",
                api_detail = "",
                api_get_material = new int[0],
                api_progress_flag = 0
            };
        }

        [TestMethod]
        public void NotImplemented()
        {
            var questInfo = new QuestInfo(() => new DateTime(2015, 1, 1));
            questInfo.InspectQuestList(CreateQuestList(new[] {679}));
            PAssert.That(() => questInfo.Quests[0].Count.Spec.Material.Length == 0);
        }

        /// <summary>
        /// 状態をロードするときに獲得資材に特殊資材のリストを追加しない
        /// </summary>
        [TestMethod]
        public void LoadStateNotAppendMaterialList()
        {
            var questInfo = new QuestInfo(() => new DateTime(2015, 1, 1));
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
                    new QuestCount {Id = 854, NowArray = new[] {1, 0, 1, 0}}
                }
            };
            questInfo.LoadState(status);
            PAssert.That(() => questInfo.Quests[0].Material.Length == 8);
        }

        /// <summary>
        /// 任務を受領したときにNeedSaveを設定する
        /// </summary>
        [TestMethod]
        public void SetNeedSaveOnStartQuest()
        {
            var questInfo = new QuestInfo(() => new DateTime(2019, 1, 1));
            // _lastResetが未設定だと必ずResetQuestsが動いてNeedSaveがtrueになる
            questInfo.LoadState(new Status {QuestLastReset = new DateTime(2019, 1, 1)});
            questInfo.InspectQuestList(Js(
                new
                {
                    api_list = new[]
                    {
                        CreateQuest(213, 1),
                        CreateQuest(214, 1)
                    }
                }));
            Assert.IsFalse(questInfo.NeedSave);
            questInfo.InspectQuestList(Js(new
            {
                api_list = new[]
                {
                    CreateQuest(213, 1),
                    CreateQuest(214, 2)
                }
            }));
            Assert.IsTrue(questInfo.NeedSave);
        }
    }
}