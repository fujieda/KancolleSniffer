﻿// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using KancolleSniffer.Model;

namespace KancolleSniffer.View
{
    public class MainShipPanels
    {
        public Control PanelShipInfo { get; set; }
        public Control Panel7Ships { get; set; }
        public Control PanelCombinedFleet { get; set; }
    }

    public class MainShipLabels
    {
        private readonly ShipLabelLines _shipLines;
        private readonly ShipLabelLines _shipLines7;
        private readonly CombinedShipLines _combinedLines = new CombinedShipLines();
        private readonly HpDisplay _hpDisplay = new HpDisplay();

        public bool ShowHpInPercent => _hpDisplay.InPercent;

        public MainShipLabels()
        {
            _shipLines = new ShipLabelLines(ShipInfo.MemberCount, 16);
            _shipLines7 = new ShipLabelLines(7, 14);
        }

        public void CreateAllShipLabels(MainShipPanels panels, EventHandler onClick)
        {
            _shipLines.Create(panels.PanelShipInfo, _hpDisplay, onClick);
            _shipLines7.Create(panels.Panel7Ships, _hpDisplay, onClick);
            _combinedLines.Create(panels.PanelCombinedFleet, _hpDisplay, onClick);
        }

        public void ToggleHpPercent()
        {
            _hpDisplay.ToggleHpPercent();
        }

        private class HpDisplay
        {
            private readonly List<ShipLabel.Hp> _labels = new List<ShipLabel.Hp>();
            public bool InPercent { get; private set; }

            public void SetClickHandler(Control label)
            {
                label.Click += LabelClickHandler;
            }

            public void AddHpLabel(ShipLabel.Hp label)
            {
                _labels.Add(label);
                label.DoubleClick += LabelClickHandler;
            }

            private void LabelClickHandler(object sender, EventArgs ev)
            {
                ToggleHpPercent();
            }

            public void ToggleHpPercent()
            {
                InPercent = !InPercent;
                foreach (var label in _labels)
                    label.ToggleHpPercent();
            }
        }

        public void SetShipLabels(IReadOnlyList<ShipStatus> ships)
        {
            (ships.Count == 7 ? _shipLines7 : _shipLines).Set(ships);
        }

        public void SetCombinedShipLabels(IReadOnlyList<ShipStatus> first, IReadOnlyList<ShipStatus> second)
        {
            _combinedLines.Set(first, second);
        }

        public void AdjustAkashiTimers()
        {
            _shipLines.AdjustAkashiTimers();
            _shipLines7.AdjustAkashiTimers();
        }

        public void SetAkashiTimer(IReadOnlyList<ShipStatus> ships, AkashiTimer.RepairSpan[] timers)
        {
            (ships.Count == 7 ? _shipLines7 : _shipLines).SetAkashiTimer(ships, timers);
        }

        private class ShipLabelLines
        {
            private readonly int _lineHeight;
            private readonly ShipLabels[] _shipLines;
            private readonly AkashiTimerLabels _akashiTimerLabels;

            private const int Top = 1;
            private const int LabelHeight = 12;

            public ShipLabelLines(int lines, int lineHeight)
            {
                _shipLines = new ShipLabels[lines];
                _akashiTimerLabels = new AkashiTimerLabels(lineHeight, _shipLines);
                _lineHeight = lineHeight;
            }

            public void Create(Control parent, HpDisplay hpDisplay, EventHandler onClick)
            {
                parent.SuspendLayout();
                _akashiTimerLabels.Create(parent);
                CreateHeader(parent, hpDisplay);
                for (var i = 0; i < _shipLines.Length; i++)
                {

                    var y = Top + _lineHeight * (i + 1);
                    var labels = _shipLines[i] = new ShipLabels
                    {
                        Name = new ShipLabel.Name(new Point(2, y + 2), ShipNameWidth.MainPanel),
                        Hp = new ShipLabel.Hp(new Point(129, y), _lineHeight),
                        Cond = new ShipLabel.Cond(new Point(131, y), _lineHeight),
                        Level = new ShipLabel.Level(new Point(155, y + 2), LabelHeight),
                        Exp = new ShipLabel.Exp(new Point(176, y + 2), LabelHeight),
                        BackGround = new Label {Location = new Point(0, y), Size = new Size(parent.Width, _lineHeight)}
                    };
                    labels.Arrange(parent, CustomColors.ColumnColors.DarkFirst(i));
                    labels.SetClickHandler(onClick);
                    labels.SetTag(i);
                    hpDisplay.AddHpLabel(labels.Hp);
                }
                parent.ResumeLayout();
            }

            private void CreateHeader(Control parent, HpDisplay hpDisplay)
            {
                var headings = new Control[]
                {
                    new Label {Location = new Point(109, Top), Text = "HP", AutoSize = true},
                    new Label {Location = new Point(128, Top), Text = "cond", AutoSize = true},
                    new Label {Location = new Point(162, Top), Text = "Lv", AutoSize = true},
                    new Label {Location = new Point(194, Top), Text = "Exp", AutoSize = true},
                    new Label {Location = new Point(0, 1), Size = new Size(parent.Width, _lineHeight - 1)}
                };
                parent.Controls.AddRange(headings);
                foreach (var control in headings)
                {
                    Scaler.Scale(control);
                    control.BackColor = CustomColors.ColumnColors.Bright;
                }
                headings[0].Cursor = Cursors.Hand;
                hpDisplay.SetClickHandler(headings[0]);
            }

            public void Set(IReadOnlyList<ShipStatus> ships)
            {
                for (var i = 0; i < _shipLines.Length; i++)
                {
                    var labels = _shipLines[i];
                    if (i >= ships.Count)
                    {
                        labels.Reset();
                        continue;
                    }
                    labels.Set(ships[i]);
                }
            }

            public void AdjustAkashiTimers() => _akashiTimerLines.AdjustAkashiTimers();

            public void SetAkashiTimer(IReadOnlyList<ShipStatus> ships, AkashiTimer.RepairSpan[] timers) =>
                _akashiTimerLabels.SetAkashiTimer(ships, timers);
        }

        private class AkashiTimerLabels
        {
            private readonly ShipLabels[] _shipLines;
            private readonly Label[] _timerLabels = new Label[ShipInfo.MemberCount];
            private readonly int _lineHeight;

            public AkashiTimerLabels(int lineHeight, ShipLabels[] shipLabels)
            {
                _shipLines = shipLabels;
                _lineHeight = lineHeight;
            }

            public void Create(Control parent)
            {
                for (var i = 0; i < _timerLabels.Length; i++)
                {
                    const int x = 55;
                    var y = 3 + _lineHeight * (i + 1);
                    Label label;
                    parent.Controls.Add(
                        label = _timerLabels[i] =
                            new Label
                            {
                                Location = new Point(x, y),
                                Size = new Size(31, 12),
                                TextAlign = ContentAlignment.TopRight
                            });
                    label.BackColor = CustomColors.ColumnColors.DarkFirst(i);
                }
                foreach (var label in _timerLabels)
                    Scaler.Scale(label);
            }

            public void AdjustAkashiTimers()
            {
                if (Scaler.ScaleHeight(1f) < 1.2)
                    return;
                for (var i = 0; i < _timerLabels.Length; i++)
                {
                    const int x = 55;
                    var y = 3 + _lineHeight * (i + 1);
                    _timerLabels[i].Location = Scaler.Move(-3, 0, x, y);
                    _timerLabels[i].Size = new Size(Scaler.ScaleWidth(31) + 1, Scaler.ScaleWidth(12));
                }
            }

            public void SetAkashiTimer(IReadOnlyList<ShipStatus> ships, AkashiTimer.RepairSpan[] timers)
            {
                var shortest = -1;
                for (var i = 0; i < timers.Length; i++)
                {
                    if (timers[i].Span <= TimeSpan.Zero)
                        continue;
                    if (shortest == -1 || timers[i].Span < timers[shortest].Span)
                        shortest = i;
                }
                for (var i = 0; i < _timerLabels.Length; i++)
                {
                    var label = _timerLabels[i];
                    var shipLabels = _shipLines[i];
                    if (i >= timers.Length || timers[i].Span == TimeSpan.MinValue)
                    {
                        label.Visible = false;
                        shipLabels.Hp.ForeColor = Control.DefaultForeColor;
                        continue;
                    }
                    var timer = timers[i];
                    var ship = ships[i];
                    label.Visible = true;
                    label.Text = timer.Span.ToString(@"mm\:ss");
                    label.ForeColor = Control.DefaultForeColor;
                    shipLabels.Name.SetName(ship, ShipNameWidth.AkashiTimer);
                    if (timer.Diff == 0)
                    {
                        shipLabels.Hp.ForeColor = Control.DefaultForeColor;
                        continue;
                    }
                    if (i == shortest)
                        label.ForeColor = CUDColors.Red;
                    shipLabels.Hp.SetHp(ship.NowHp + timer.Diff, ship.MaxHp);
                    shipLabels.Hp.ForeColor = Color.DimGray;
                }
            }
        }

        private class CombinedShipLines
        {
            private readonly ShipLabels[] _combinedLines = new ShipLabels[ShipInfo.MemberCount * 2];

            private const int Top = 1;
            private const int LineHeight = 16;
            private const int ParentWidth = 220; // parent.Widthを使うとDPIスケーリング時に計算がくるうので

            public void Create(Control parent, HpDisplay hpDisplay, EventHandler onClick)
            {
                parent.SuspendLayout();
                CreateHeader(parent, hpDisplay);
                for (var i = 0; i < _combinedLines.Length; i++)
                {
                    var x = ParentWidth / 2 * (i / ShipInfo.MemberCount);
                    var y = Top + LineHeight * (i % ShipInfo.MemberCount + 1);
                    var labels = _combinedLines[i] = new ShipLabels
                    {
                        Name = new ShipLabel.Name(new Point(x + 2, y + 2), ShipNameWidth.Combined),
                        Hp = new ShipLabel.Hp(new Point(x + 88, y), LineHeight),
                        Cond = new ShipLabel.Cond(new Point(x + 85, y), LineHeight),
                        BackGround = new Label {Location = new Point(x, y), Size = new Size(ParentWidth / 2, LineHeight)}
                    };
                    labels.Arrange(parent, CustomColors.ColumnColors.DarkFirst(i));
                    labels.SetTag(i);
                    var hpLabel = _combinedLines[i].Hp;
                    hpDisplay.AddHpLabel(hpLabel);
                }
                parent.ResumeLayout();
            }

            private void CreateHeader(Control parent, HpDisplay hpDisplay)
            {
                var headings = new Control[]
                {
                    new Label {Location = new Point(68, Top), Text = "HP", AutoSize = true},
                    new Label {Location = new Point(86, Top), Text = "cnd", AutoSize = true},
                    new Label {Location = new Point(177, Top), Text = "HP", AutoSize = true},
                    new Label {Location = new Point(195, Top), Text = "cnd", AutoSize = true},
                    new Label {Location = new Point(0, 1), Size = new Size(ParentWidth, LineHeight - 1)}
                };
                parent.Controls.AddRange(headings);
                foreach (var control in headings)
                {
                    Scaler.Scale(control);
                    control.BackColor = CustomColors.ColumnColors.Bright;
                }
                headings[0].Cursor = headings[2].Cursor = Cursors.Hand;
                hpDisplay.SetClickHandler(headings[0]);
                hpDisplay.SetClickHandler(headings[2]);
            }

            public void Set(IReadOnlyList<ShipStatus> first, IReadOnlyList<ShipStatus> second)
            {
                for (var i = 0; i < _combinedLines.Length; i++)
                {
                    var idx = i % ShipInfo.MemberCount;
                    var ships = i < ShipInfo.MemberCount ? first : second;
                    var labels = _combinedLines[i];
                    if (idx >= ships.Count)
                    {
                        labels.Reset();
                        continue;
                    }
                    labels.Set(ships[idx]);
                }
            }
        }
    }
}