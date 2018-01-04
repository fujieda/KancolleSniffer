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
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Math;

namespace KancolleSniffer
{
    public class BattleResultPanel : Panel
    {
        private const int LineHeight = 16;
        private readonly List<ShipLabel[]> _friendLabels = new List<ShipLabel[]>();
        private readonly List<ShipLabel[]> _enemyLabels = new List<ShipLabel[]>();
        private readonly List<Panel> _panelList = new List<Panel>();
        private bool _hpPercent;
        private readonly List<ShipLabel> _hpLabels = new List<ShipLabel>();
        private readonly ToolTip _toolTip = new ToolTip {ShowAlways = true};
        private readonly BattleInfo.BattleResult[] _result = new BattleInfo.BattleResult[2];
        private Label _phaseLabel;
        private BattleState _prevBattleState;

        public bool Spoiler { get; set; }

        public BattleResultPanel()
        {
            CreateLabels();
        }

        public void SetShowHpPercent(bool hpPercent)
        {
            if (hpPercent == _hpPercent)
                return;
            foreach (var label in _hpLabels)
                label.ToggleHpPercent();
            _hpPercent = hpPercent;
        }

        public void Update(Sniffer sniffer)
        {

            if (_prevBattleState == BattleState.None)
                _result[0] = _result[1] = null;
            var state = _prevBattleState = sniffer.Battle.BattleState;
            if (state != BattleState.Day && state != BattleState.Night)
                return;
            if (Spoiler)
            {
                ShowResult(sniffer.Battle.Result);
                if (state == BattleState.Day)
                {
                    _result[0] = sniffer.Battle.Result;
                    SetPhase("昼戦");
                }
                else if (state == BattleState.Night)
                {
                    _result[1] = sniffer.Battle.Result;
                    SetPhase("夜戦");
                }
            }
            else
            {
                ClearResult();
                _phaseLabel.Text = "結果";
                _phaseLabel.BorderStyle = BorderStyle.FixedSingle;
                _phaseLabel.Cursor = Cursors.Hand;
                if (state == BattleState.Day)
                {
                    _result[0] = sniffer.Battle.Result;
                }
                else if (state == BattleState.Night)
                {
                    _result[1] = sniffer.Battle.Result;
                }
            }
        }

        private void PhaseLabelClick(object sender, EventArgs ev)
        {
            switch (_phaseLabel.Text)
            {
                case "結果":
                    if (_result[0] != null)
                    {
                        ShowResult(_result[0]);
                        SetPhase("昼戦");
                    }
                    else if (_result[1] != null)
                    {
                        ShowResult(_result[1]);
                        SetPhase("夜戦");
                    }
                    break;
                case "昼戦":
                    if (_result[1] != null)
                    {
                        ShowResult(_result[1]);
                        SetPhase("夜戦");
                    }
                    break;
                case "夜戦":
                    if (_result[0] != null)
                    {
                        ShowResult(_result[0]);
                        SetPhase("昼戦");
                    }
                    break;
            }
        }

        private void SetPhase(string phase)
        {
            _phaseLabel.Text = phase;
            if (_result[0] != null && _result[1] != null)
            {
                _phaseLabel.BorderStyle = BorderStyle.FixedSingle;
                _phaseLabel.Cursor = Cursors.Hand;
            }
            else
            {
                _phaseLabel.BorderStyle = BorderStyle.None;
                _phaseLabel.Cursor = Cursors.Default;
            }
        }

        private void ClearResult()
        {
            foreach (var panel in _panelList)
                panel.Visible = false;
        }

        private void ShowResult(BattleInfo.BattleResult result)
        {
            SuspendLayout();
            var friend = result.Friend;
            var enemy = result.Enemy;

            var fleet = new[] {"第一", "第二", "第三", "第四"};
            _friendLabels[0][1].Text = fleet[friend.Main[0].Fleet];
            for (var i = 0; i < friend.Main.Length; i++)
            {
                _friendLabels[i + 1][0].SetHp(friend.Main[i]);
                _friendLabels[i + 1][1].SetName(friend.Main[i], ShipNameWidth.BattleResult);
            }
            if (friend.Guard.Length > 0)
            {
                _friendLabels[friend.Main.Length + 1][1].Text = fleet[friend.Guard[0].Fleet];
                _friendLabels[friend.Main.Length + 1][0].SetHp(null);
                for (var i = 0; i < friend.Guard.Length; i++)
                {
                    var labels = _friendLabels[friend.Main.Length + 2 + i];
                    labels[0].SetHp(friend.Guard[i]);
                    labels[1].SetName(friend.Guard[i], ShipNameWidth.BattleResult);
                }
            }
            var friendLines = 1 + friend.Main.Length + (friend.Guard.Length > 0 ? friend.Guard.Length + 1 : 0);
            for (var i = friendLines; i < _friendLabels.Count; i++)
            {
                _friendLabels[i][0].SetHp(null);
                _friendLabels[i][1].SetName("");
            }
            _enemyLabels[0][1].Text = "本隊";
            for (var i = 0; i < enemy.Main.Length; i++)
            {
                var labels = _enemyLabels[i + 1];
                labels[0].SetHp(enemy.Main[i]);
                labels[1].SetName(ShortenName(enemy.Main[i].Name));
                _toolTip.SetToolTip(labels[1], string.Join("\r\n", enemy.Main[i].Slot.Select(item => item.Spec.Name)));
            }
            if (enemy.Guard.Length > 0)
            {
                _enemyLabels[enemy.Main.Length + 1][0].SetHp(null);
                _enemyLabels[enemy.Main.Length + 1][1].Text = "護衛";
                for (var i = 0; i < enemy.Guard.Length; i++)
                {
                    var labels = _enemyLabels[enemy.Main.Length + 2 + i];
                    labels[0].SetHp(enemy.Guard[i]);
                    labels[1].SetName(ShortenName(enemy.Guard[i].Name));
                    _toolTip.SetToolTip(labels[1],
                        string.Join("\r\n", enemy.Guard[i].Slot.Select(item => item.Spec.Name)));
                }
            }
            var enemyLines = 1 + enemy.Main.Length + (enemy.Guard.Length > 0 ? enemy.Guard.Length + 1 : 0);
            for (var i = enemyLines; i < _enemyLabels.Count; i++)
            {
                _enemyLabels[i][0].SetHp(null);
                _enemyLabels[i][1].SetName("");
            }
            var lines = Max(friendLines, enemyLines);
            var labelWidth = _enemyLabels.Max(labels => labels[1].Size.Width);
            for (var i = 0; i < lines; i++)
            {
                var panel = _panelList[i];
                _panelList[i].Width = Max(ClientSize.Width,
                    (int)Round(164 * ShipLabel.ScaleFactor.Width) + labelWidth - 1);
                if (panel.Visible)
                    continue;
                panel.Location = new Point(AutoScrollPosition.X, (int)panel.Tag + AutoScrollPosition.Y);
                panel.Visible = true;
            }
            for (var i = lines; i < _panelList.Count; i++)
                _panelList[i].Visible = false;
            ResumeLayout();
        }

        private string ShortenName(string name)
        {
            return new Regex(@"\(elite\)|\(flagship\)").Replace(name,
                match => match.Value == "(elite)" ? "(e)" : "(f)");
        }

        private void CreateLabels()
        {
            SuspendLayout();
            _phaseLabel = new Label
            {
                Location = new Point(93, 2),
                Size = new Size(31, 14)
            };
            _phaseLabel.Click += PhaseLabelClick;
            Controls.Add(_phaseLabel);
            for (var i = 0; i < 14; i++)
            {
                var y = 1 + LineHeight * i;
                var panel = new Panel
                {
                    Location = new Point(0, y),
                    Size = new Size(0, LineHeight),
                    BackColor = ShipLabels.ColumnColors[i % 2],
                    Visible = false,
                    Tag = y
                };
                _panelList.Add(panel);

                var friend = new[]
                {
                    new ShipLabel {Location = new Point(100, 2), AutoSize = true, AnchorRight = true},
                    new ShipLabel {Location = new Point(1, 2), AutoSize = true}
                };
                _friendLabels.Add(friend);
                _hpLabels.Add(friend[0]);
                var enemy = new[]
                {
                    new ShipLabel {Location = new Point(119, 2), AutoSize = true},
                    new ShipLabel {Location = new Point(164, 2), AutoSize = true}
                };
                _enemyLabels.Add(enemy);
                foreach (var label in friend.Concat(enemy))
                {
                    panel.Controls.Add(label);
                    label.BackColor = label.PresetColor = ShipLabels.ColumnColors[i % 2];
                }
                Controls.Add(panel);
            }
            ResumeLayout();
        }
    }
}