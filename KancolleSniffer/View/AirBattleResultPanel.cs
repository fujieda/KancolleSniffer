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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KancolleSniffer.Model;

// ReSharper disable CoVariantArrayConversion

namespace KancolleSniffer.View
{
    [DesignerCategory("Code")]
    public class AirBattleResultPanel : PanelWithToolTip
    {
        private readonly Label _phaseName;
        private readonly Label _stage1;
        private readonly Label[][][] _resultLabels = new Label[2][][];
        private AirBattleResult.AirBattleRecord[] _resultList;
        private int _resultIndex;
        private readonly ShipLabel.Name _ciShipName;
        private readonly Label _ciKind;

        public bool ShowResultAutomatic { get; set; }

        private bool ResultRemained
        {
            set => _phaseName.BorderStyle = value ? BorderStyle.FixedSingle : BorderStyle.None;
        }

        public AirBattleResultPanel()
        {
            const int top = 20;
            const int ci = 168;
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
                new Label {Text = "敵軍", Location = new Point(122, 6), AutoSize = true},
                new Label {Text = "CI", Location = new Point(ci, 4), AutoSize = true}
            };
            Controls.AddRange(labels);
            const int left = 53;
            const int space = 55;
            for (var stage = 0; stage < 2; stage++)
            {
                _resultLabels[stage] = new Label[2][];
                for (var fe = 0; fe < 2; fe++)
                {
                    _resultLabels[stage][fe] = new Label[2];
                    Controls.Add(_resultLabels[stage][fe][1] = new Label
                    {
                        Location = new Point(left + 34 + space * fe, top + 14 * stage),
                        Size = new Size(24, 12),
                        TextAlign = ContentAlignment.TopLeft
                    });
                    Controls.Add(new Label
                    {
                        Location = new Point(left + 21 + space * fe, top + 14 * stage),
                        Text = "→",
                        AutoSize = true
                    });
                    Controls.Add(_resultLabels[stage][fe][0] = new Label
                    {
                        Location = new Point(left + space * fe, top + 14 * stage),
                        Size = new Size(24, 12),
                        TextAlign = ContentAlignment.TopRight
                    });
                }
            }
            Controls.Add(_ciShipName = new ShipLabel.Name(new Point(ci, top), ShipNameWidth.CiShipName));
            Controls.Add(_ciKind = new Label
            {
                Location = new Point(ci, top + 14),
                Size = new Size(24, 12)
            });
            _phaseName.Click += PhaseNameOnClick;
        }

        public void SetResult(Sniffer sniffer)
        {
            var state = sniffer.Battle.BattleState;
            if (state != BattleState.Day && state != BattleState.SpNight)
                return;
            _resultList = sniffer.Battle.AirBattleResult.Result.ToArray();
            if (_resultList.Length == 0)
            {
                ResultRemained = false;
                ClearResult();
                return;
            }
            _resultIndex = _resultList.Length - 1;
            if (!ShowResultAutomatic)
            {
                ResultRemained = true;
                ClearResult();
                return;
            }
            ShowResult();
            ResultRemained = _resultList.Length > 1;
            _resultIndex = 0;
        }

        private void PhaseNameOnClick(object sender, EventArgs eventArgs)
        {
            if (_resultList == null || _resultList.Length == 0)
                return;
            ShowResult();
            if (_resultList.Length == 1)
                ResultRemained = false;
            _resultIndex = (_resultIndex + 1) % _resultList.Length;
        }

        private void ShowResult()
        {
            if (_resultIndex >= _resultList.Length)
                return;
            var result = _resultList[_resultIndex];
            _phaseName.Text = result.PhaseName;
            var color = new[] {DefaultForeColor, CUDColors.Blue, CUDColors.Green, CUDColors.Orange, CUDColors.Red};
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
            ShowAirFireResult();
        }

        private void ShowAirFireResult()
        {
            var result = _resultList[_resultIndex];
            if (result.AirFire == null)
            {
                _ciShipName.Reset();
                _ciKind.Text = "";
                ToolTip.SetToolTip(_ciKind, "");
            }
            else
            {
                _ciShipName.SetName(result.AirFire.ShipName);
                _ciKind.Text = result.AirFire.Kind.ToString();
                ToolTip.SetToolTip(_ciKind, string.Join("\r\n", result.AirFire.Items));
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
            _ciShipName.Reset();
            _ciKind.Text = "";
            ToolTip.SetToolTip(_ciKind, "");
        }
    }
}