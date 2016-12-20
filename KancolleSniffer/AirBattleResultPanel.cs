﻿// Copyright (C) 2016 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

        public string PhaseName { get; set; }
        public int AirControlLevel { get; set; }
        public StageResult Stage1 { get; set; } = new StageResult();
        public StageResult Stage2 { get; set; } = new StageResult();
    }

    public class AirBattleResultPanel : Panel
    {
        private readonly Label _phaseName;
        private readonly Label _stage1;
        private readonly Label[][][] _resultLabels = new Label[2][][];
        private List<AirBattleResult> _resultList;
        private int _resultIndex;

        public bool ShowResultAutomatic { get; set; }

        private bool ResultRemained
        {
            set { _phaseName.BorderStyle = value ? BorderStyle.FixedSingle : BorderStyle.None; }
        }

        public AirBattleResultPanel()
        {
            const int top = 20;
            var labels = new[]
            {
                _phaseName =
                    new Label
                    {
                        Text = "航空戦",
                        Location = new Point(4, 4),
                        Size = new Size(49, 12),
                        TextAlign = ContentAlignment.TopCenter
                    },
                _stage1 = new Label {Text = "stage1", Location = new Point(8, top), AutoSize = true},
                new Label {Text = "stage2", Location = new Point(8, top + 14), AutoSize = true},
                new Label {Text = "自軍", Location = new Point(67, 6), AutoSize = true},
                new Label {Text = "敵軍", Location = new Point(124, 6), AutoSize = true}
            };
            Controls.AddRange(labels);
            const int left = 54;
            for (var stage = 0; stage < 2; stage++)
            {
                _resultLabels[stage] = new Label[2][];
                for (var fe = 0; fe < 2; fe++)
                {
                    _resultLabels[stage][fe] = new Label[2];
                    Controls.Add(_resultLabels[stage][fe][1] = new Label
                    {
                        Location = new Point(left + 32 + 57 * fe, top + 14 * stage),
                        Size = new Size(23, 12),
                        TextAlign = ContentAlignment.TopLeft
                    });
                    Controls.Add(new Label
                    {
                        Location = new Point(left + 20 + 57 * fe, top + 14 * stage),
                        Text = "→",
                        AutoSize = true
                    });
                    Controls.Add(_resultLabels[stage][fe][0] = new Label
                    {
                        Location = new Point(left + 57 * fe, top + 14 * stage),
                        Size = new Size(23, 12),
                        TextAlign = ContentAlignment.TopRight
                    });
                }
            }
            _phaseName.Click += PhaseNameOnClick;
        }

        public void SetResult(List<AirBattleResult> resultList)
        {
            _resultList = resultList;
            if (_resultList.Count == 0)
            {
                ResultRemained = false;
                ClearResult();
                return;
            }
            _resultIndex = _resultList.Count - 1;
            if (!ShowResultAutomatic)
            {
                ResultRemained = true;
                ClearResult();
                return;
            }
            ShowResult();
            ResultRemained = _resultList.Count > 1;
            _resultIndex = 0;
        }

        private void PhaseNameOnClick(object sender, EventArgs eventArgs)
        {
            if (_resultList == null || _resultList.Count == 0)
                return;
            ShowResult();
            if (_resultList.Count == 1)
                ResultRemained = false;
            _resultIndex = (_resultIndex + 1) % _resultList.Count;
        }

        private void ShowResult()
        {
            if (_resultIndex >= _resultList.Count)
                return;
            var result = _resultList[_resultIndex];
            _phaseName.Text = result.PhaseName;
            var color = new[] {DefaultForeColor, Color.Blue, Color.Green, Color.Orange, Color.Red};
            _stage1.ForeColor = color[result.AirControlLevel];
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

        private void ClearResult()
        {
            _phaseName.Text = "航空戦";
            _stage1.ForeColor = DefaultForeColor;
            for (var st = 0; st < 2; st++)
            {
                for (var fe = 0; fe < 2; fe++)
                {
                    for (var ba = 0; ba < 2; ba++)
                    {
                        _resultLabels[st][fe][ba].Text = "";
                    }
                }
            }
        }
    }
}