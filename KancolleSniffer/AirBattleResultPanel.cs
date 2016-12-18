// Copyright (C) 2016 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

using System.Drawing;
using System.Windows.Forms;

// ReSharper disable CoVariantArrayConversion

namespace KancolleSniffer
{
    public class AirBattleResult
    {
        public class StageResult
        {
            public int FriendCount { get; set; }
            public int FriendLost { get; set; }
            public int EnemyCount { get; set; }
            public int EnemyLost { get; set; }
        }

        public StageResult Stage1 { get; set; } = new StageResult();
        public StageResult Stage2 { get; set; } = new StageResult();
    }

    public class AirBattleResultPanel : Panel
    {
        private readonly Label[][][] _resultLabels = new Label[2][][];

        public AirBattleResultPanel()
        {
            var labels = new[]
            {
                new Label {Text = "航空戦", Location = new Point(4, 4), AutoSize = true},
                new Label {Text = "stage1", Location = new Point(4, 18), AutoSize = true},
                new Label {Text = "stage2", Location = new Point(4, 32), AutoSize = true},
                new Label {Text = "自軍", Location = new Point(59, 4), AutoSize = true},
                new Label {Text = "敵軍", Location = new Point(115, 4), AutoSize = true}
            };
            Controls.AddRange(labels);
            for (var stage = 0; stage < 2; stage++)
            {
                _resultLabels[stage] = new Label[2][];
                for (var fe = 0; fe < 2; fe++)
                {
                    _resultLabels[stage][fe] = new Label[2];
                        Controls.Add(_resultLabels[stage][fe][1] = new Label
                        {
                            Location = new Point(78 + 56 * fe, 18 + 14 * stage),
                            Size = new Size(23, 12),
                            TextAlign = ContentAlignment.TopLeft,
                        });
                    Controls.Add(new Label
                    {
                        Location = new Point(66 + 56 * fe, 18 + 14 * stage),
                        Text = "→",
                        AutoSize = true
                    });
                    Controls.Add(_resultLabels[stage][fe][0] = new Label
                    {
                        Location = new Point(46 + 56 * fe, 18 + 14 * stage),
                        Size = new Size(23, 12),
                        TextAlign = ContentAlignment.TopRight,
                    });
                }
            }
        }

        public void SetResult(AirBattleResult result)
        {
            var stages = new[] {result.Stage1, result.Stage2};
            for (var i = 0; i < 2; i++)
            {
                var stage = stages[i];
                var labels = _resultLabels[i];
                labels[0][0].Text = $"{stage.FriendCount}";
                labels[0][1].Text = $"{stage.FriendCount - stage.FriendLost}";
                labels[1][0].Text = $"{stage.EnemyCount}";
                labels[1][1].Text = $"{stage.EnemyCount - stage.EnemyLost}";
            }
        }
    }
}