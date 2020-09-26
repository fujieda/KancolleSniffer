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
using DynaJson;
using ExpressionToCodeLib;
using KancolleSniffer.Model;
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
                new DateTime(2017, 12, 1, 5, 0, 0), new DateTime(2018, 2, 1, 5, 0, 0),
                new DateTime(2018, 3, 1, 5, 0, 0), new DateTime(2018, 5, 1, 5, 0, 0),
                new DateTime(2018, 8, 1, 5, 0, 0), new DateTime(2018, 9, 1, 5, 0, 0)
            });
            var questInfo = MakeQuestInfo(() => queue.Dequeue());
            var status = new Status
            {
                QuestCountList = new[]
                {
                    new QuestCount {Id = 201, Now = 1},
                    new QuestCount {Id = 265, Now = 1},
                    new QuestCount {Id = 213, Now = 1},
                    new QuestCount {Id = 822, Now = 1},
                    new QuestCount {Id = 904, NowArray = new[] {1, 1, 1, 1}},
                    new QuestCount {Id = 436, NowArray = new[] {1, 1, 1, 1, 0}},
                    new QuestCount {Id = 437, NowArray = new[] {1, 1, 1, 1}},
                    new QuestCount {Id = 438, NowArray = new[] {1, 1, 1, 1}},
                },
                QuestLastReset = new DateTime(2017, 10, 31, 5, 0, 0)
            };
            var ids = status.QuestCountList.Select(qc => qc.Id).ToArray();
            questInfo.LoadState(status);
            CheckQuestCountList(questInfo, status, ids.Skip(2)); // デイリーとマンスリーが消える
            CheckQuestCountList(questInfo, status, ids.Skip(3)); // ウィークリーが消える
            CheckQuestCountList(questInfo, status, ids.Skip(4)); // クォータリーが消える
            CheckQuestCountList(questInfo, status, ids.Skip(5)); // イヤーリー2月が消える
            CheckQuestCountList(questInfo, status, ids.Skip(6)); // イヤーリー3月が消える
            CheckQuestCountList(questInfo, status, ids.Skip(7)); // イヤーリー5月が消える
            CheckQuestCountList(questInfo, status, ids.Skip(8)); // イヤーリー8月が消える
        }

        private void CheckQuestCountList(QuestInfo questInfo, Status status, IEnumerable<int> quests)
        {
            InspectQuestList(questInfo, new[] {201});
            questInfo.SaveState(status);
            PAssert.That(() =>  status.QuestCountList.Select(qc => qc.Id).SequenceEqual(quests));
        }

        [TestMethod]
        public void ResetFrom0To5OClock()
        {
            var queue = new Queue<DateTime>(new[]
            {
                new DateTime(2019, 1, 22, 4, 0, 0)
            });
            var questInfo = MakeQuestInfo(() => queue.Dequeue());
            var status = new Status
            {
                QuestCountList = new[] {new QuestCount {Id = 213, Now = 1}},
                QuestLastReset = new DateTime(2019, 1, 20, 5, 16, 22)
            };
            questInfo.LoadState(status);
            InspectQuestList(questInfo, new[] {201});
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
            var questInfo = MakeQuestInfo(() => queue.Dequeue());
            var status = new Status
            {
                QuestLastReset = new DateTime(2019, 1, 27, 5, 0, 0)
            };
            questInfo.LoadState(status);
            InspectQuestList(questInfo, // 2019-1-27 10:00
                new[] {237}); // 【節分拡張任務】南方海域 艦隊決戦
            PAssert.That(() => questInfo.Quests[0].Id == 237);
            InspectQuestList(questInfo, new[] {201}); // 2019-1-28 05:00
            PAssert.That(() => questInfo.Quests[0].Id == 201);
        }

        [TestMethod]
        public void NotImplemented()
        {
            var questInfo = MakeQuestInfo(() => new DateTime(2015, 1, 1));
            InspectQuestList(questInfo, new[] {679});
            PAssert.That(() => questInfo.Quests[0].Count.Spec.Material.Length == 0);
        }

        private JsonObject Js(object obj) => new JsonObject(obj);

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

        private void InspectQuestList(QuestInfo questInfo, int[] ids)
        {
            questInfo.InspectQuestList("api_tab_id=0", CreateQuestList(ids));
        }


        /// <summary>
        /// 状態をロードするときに獲得資材に特殊資材のリストを追加しない
        /// </summary>
        [TestMethod]
        public void LoadStateNotAppendMaterialList()
        {
            var questInfo = MakeQuestInfo(() => new DateTime(2015, 1, 1));
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

        private QuestInfo MakeQuestInfo(Func<DateTime> nowFunc)
        {
            return new QuestInfo(new QuestCountList(), nowFunc);
        }
    }
}