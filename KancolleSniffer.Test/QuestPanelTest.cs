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

using System.Collections.Generic;
using System.Linq;
using KancolleSniffer.Model;
using KancolleSniffer.View;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class QuestPanelTest
    {
        private const int Lines = 4;
        private const int AcceptMax = 7;
        private readonly QuestPanel _panel = new QuestPanel();
        private readonly QuestCountList _countList = new QuestCountList();

        [TestInitialize]
        public void Initialize()
        {
            _panel.CreateLabels(Lines, (obj, e) => { });
        }

        /// <summary>
        /// 行数と同じ任務数
        /// </summary>
        [TestMethod]
        public void ShowAll()
        {
            _panel.Update(CreateQuests(Lines));
            Assert.IsTrue(CheckResult(CreateQuests(Lines)));
        }

        /// <summary>
        /// 最大の任務数について上から行数分
        /// </summary>
        [TestMethod]
        public void ShowTop()
        {
            _panel.Update(CreateQuests(AcceptMax));
            Assert.IsTrue(CheckResult(CreateQuests(Lines)));
        }

        /// <summary>
        /// IDの大きい任務の追加されたら、その表示のためにスクロールする
        /// </summary>
        [TestMethod]
        public void AddBottomWithScroll()
        {
            _panel.Update(CreateQuests(Lines));
            _panel.Update(CreateQuests(Lines + 1));
            Assert.IsTrue(CheckResult(CreateQuests(1, Lines)));

            SetScrollPosition(1);
            _panel.Update(CreateQuests(Lines + 2));
            Assert.IsTrue(CheckResult(CreateQuests(2, Lines)));
        }

        /// <summary>
        /// IDの小さい任務が追加されたらそれを表示する
        /// </summary>
        [TestMethod]
        public void AddTop()
        {
            _panel.Update(CreateQuests(2, Lines).ToArray());
            _panel.Update(CreateQuests(1, Lines + 1).ToArray());
            Assert.IsTrue(CheckResult(CreateQuests(1, Lines)));
        }

        /// <summary>
        /// 上が隠れているときにIDの小さい任務が追加されたら上スクロールする
        /// </summary>
        [TestMethod]
        public void AddTopWithScroll()
        {
            _panel.Update(CreateQuests(1, Lines + 1));
            SetScrollPosition(1);
            _panel.Update(CreateQuests(Lines + 2).ToArray());
            Assert.IsTrue(CheckResult(CreateQuests(Lines)));
        }

        /// <summary>
        /// 中間のIDの任務が見える位置に挿入されたらスクロールしない
        /// </summary>
        [TestMethod]
        public void AddMediumWithoutScroll()
        {
            _panel.Update(CreateQuests(new[] {0, 2, 3, 4, 5}));
            SetScrollPosition(1);
            _panel.Update(CreateQuests(new[] {0, 1, 2, 3, 4, 5}));
            Assert.IsTrue(CheckResult(CreateQuests(1, Lines)));
        }

        /// <summary>
        /// 中間のIDの任務が見えない位置に挿入されたらスクロールする
        /// </summary>
        [TestMethod]
        public void AddMediumWithScroll()
        {
            _panel.Update(CreateQuests(new[] {0, 2, 3, 4, 5, 6}));
            SetScrollPosition(2);
            _panel.Update(CreateQuests(AcceptMax));
            Assert.IsTrue(CheckResult(CreateQuests(1, Lines)));
        }

        /// <summary>
        /// 最後の任務が減る場合
        /// </summary>
        [TestMethod]
        public void RemoveBottom()
        {
            _panel.Update(CreateQuests(AcceptMax));
            _panel.Update(CreateQuests(AcceptMax - 1));
            Assert.IsTrue(CheckResult(CreateQuests(Lines)));
        }

        /// <summary>
        /// 最後の任務が減った結果上スクロールする
        /// </summary>
        [TestMethod]
        public void RemoveBottomWithScroll()
        {
            _panel.Update(CreateQuests(Lines + 2));
            SetScrollPosition(2);
            _panel.Update(CreateQuests(Lines + 1));
            Assert.IsTrue(CheckResult(CreateQuests(1, Lines)));
        }

        /// <summary>
        /// 中間の任務が減る場合
        /// </summary>
        [TestMethod]
        public void RemoveMedium()
        {
            _panel.Update(CreateQuests(AcceptMax));
            var sparse = new[] {0, 1, 3, 4, 5, 6};
            _panel.Update(CreateQuests(sparse));
            Assert.IsTrue(CheckResult(CreateQuests(sparse.Take(Lines))));
        }

        /// <summary>
        /// 中間の任務が減った結果上スクロールする
        /// </summary>
        [TestMethod]
        public void RemoveMediumWithScroll()
        {
            _panel.Update(CreateQuests(Lines + 2));
            SetScrollPosition(2);
            var sparse = new[] {0, 1, 2, 4, 5};
            _panel.Update(CreateQuests(sparse));
            Assert.IsTrue(CheckResult(CreateQuests(sparse.Skip(1))));
        }

        /// <summary>
        /// カウントが増えた任務を見える位置にスクロールする
        /// </summary>
        [TestMethod]
        public void ChangeCount()
        {
            var quests = CreateQuests(AcceptMax);
            _panel.Update(quests);

            quests[AcceptMax - 1].Count.Now = 1;
            _panel.Update(quests);
            Assert.IsTrue(CheckResult(CreateQuests(3, Lines)), "下スクロール");

            quests[2].Count.NowArray[0] = 1;
            _panel.Update(quests);
            Assert.IsTrue(CheckResult(CreateQuests(2, Lines)), "上スクロール");

            quests[3].Count.Now = 1;
            _panel.Update(quests);
            Assert.IsTrue(CheckResult(CreateQuests(2, Lines)), "そのまま");

            _panel.Update(quests);
            Assert.IsTrue(CheckResult(CreateQuests(2, Lines)), "そのまま");

            quests[0].Count.Now = 1;
            _panel.Update(quests);
            Assert.IsTrue(CheckResult(CreateQuests(Lines)), "上スクロール");
        }


        private QuestStatus[] CreateQuests(int length)
        {
            return CreateQuests(0, length);
        }

        private QuestStatus[] CreateQuests(int start, int count)
        {
            return CreateQuests(Enumerable.Range(start, count));
        }

        private QuestStatus[] CreateQuests(IEnumerable<int> indexes)
        {
            var quests = new[]
            {
                CreateStatus(210, "敵艦隊を10回邀撃せよ！"),
                CreateStatus(211, "敵空母を3隻撃沈せよ！"),
                CreateStatus(214, "あ号作戦"),
                CreateStatus(216, "敵艦隊主力を撃滅せよ！"),
                CreateStatus(218, "敵補給艦を3隻撃沈せよ！"),
                CreateStatus(403, "「遠征」を10回成功させよう！"),
                CreateStatus(503, "艦隊大整備！")
            };
            return indexes.Select(idx => quests[idx]).ToArray();
        }

        private QuestStatus CreateStatus(int id, string name)
        {
            return new QuestStatus
            {
                Id = id,
                Name = name,
                Color = _panel.BackColor,
                Count = _countList.GetCount(id)
            };
        }

        private bool CheckResult(IEnumerable<QuestStatus> expected)
        {
            var labels = (QuestLabels[])new PrivateObject(_panel).GetField("_labels");
            var result = labels.Select(ql => ql.Name.Text);
            return expected.Select(q => q.Name).Concat(Enumerable.Repeat("", Lines)).Take(Lines).SequenceEqual(result);
        }

        private void SetScrollPosition(int position)
        {
            var scroller = (ListScroller)new PrivateObject(_panel).GetField("_listScroller");
            scroller.Position = position;
        }
    }
}