// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

namespace KancolleSniffer.View.MainWindow
{
    public class ShipListPanels
    {
        private const int PanelWidth = 220;

        private readonly Panel _combined = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(PanelWidth, 113),
            Visible = false
        };

        private readonly Panel _7Ships = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(PanelWidth, 113),
            Visible = false
        };

        private readonly ShipLabelLines _shipLines;
        private readonly ShipLabelLines _shipLines7;
        private readonly CombinedShipLines _combinedLines = new CombinedShipLines();
        private readonly HpToggle _hpToggle = new HpToggle();
        private readonly ToolTip _toolTip;
        private readonly EventHandler _onClick;

        public bool ShowHpInPercent => _hpToggle.InPercent;

        public ShipListPanels(ShipInfoPanel parent, EventHandler onClick)
        {
            _onClick = onClick;
            _toolTip = parent.ToolTip;
            _shipLines = new ShipLabelLines(ShipInfo.MemberCount, 16);
            _shipLines7 = new ShipLabelLines(7, 14);
            parent.Controls.AddRange(new Control[] {_combined, _7Ships});
            _shipLines.Create(parent, this);
            _shipLines7.Create(_7Ships, this);
            _combinedLines.Create(_combined, this);
        }

        public void ToggleHpPercent()
        {
            _hpToggle.ToggleHpPercent();
        }

        private class HpToggle
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
            _combined.Visible = false;
            if (ships.Count == 7)
            {
                _7Ships.Visible = true;
                _shipLines7.Set(ships);
            }
            else
            {
                _7Ships.Visible = false;
                _shipLines.Set(ships);
            }
        }

        public void SetCombinedShipLabels(IReadOnlyList<ShipStatus> first, IReadOnlyList<ShipStatus> second)
        {
            _combined.Visible = true;
            _combinedLines.Set(first, second);
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
            private ToolTip _toolTip;

            private const int Top = 1;
            private const int LabelHeight = 12;

            public ShipLabelLines(int lines, int lineHeight)
            {
                _shipLines = new ShipLabels[lines];
                _akashiTimerLabels = new AkashiTimerLabels(lineHeight, _shipLines);
                _lineHeight = lineHeight;
            }

            public void Create(Control parent, ShipListPanels panels)
            {
                _toolTip = panels._toolTip;
                parent.SuspendLayout();
                _akashiTimerLabels.Create(parent);
                CreateHeader(parent, panels._hpToggle);
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
                        BackGround = new Label {Location = new Point(0, y), Size = new Size(PanelWidth, _lineHeight)}
                    };
                    labels.Arrange(parent, CustomColors.ColumnColors.DarkFirst(i));
                    labels.SetClickHandler(panels._onClick);
                    labels.SetTag(i);
                    panels._hpToggle.AddHpLabel(labels.Hp);
                }
                parent.ResumeLayout();
            }

            private void CreateHeader(Control parent, HpToggle hpToggle)
            {
                var headings = new Control[]
                {
                    new Label {Location = new Point(109, Top), Text = "HP", AutoSize = true},
                    new Label {Location = new Point(128, Top), Text = "cond", AutoSize = true},
                    new Label {Location = new Point(162, Top), Text = "Lv", AutoSize = true},
                    new Label {Location = new Point(194, Top), Text = "Exp", AutoSize = true},
                    new Label {Location = new Point(0, 1), Size = new Size(PanelWidth, _lineHeight - 1)}
                };
                parent.Controls.AddRange(headings);
                foreach (var control in headings)
                    control.BackColor = CustomColors.ColumnColors.Bright;
                headings[0].Cursor = Cursors.Hand;
                hpToggle.SetClickHandler(headings[0]);
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
                    labels.Set(ships[i], _toolTip);
                }
            }

            public void SetAkashiTimer(IReadOnlyList<ShipStatus> ships, AkashiTimer.RepairSpan[] timers) =>
                _akashiTimerLabels.SetAkashiTimer(ships, timers);
        }

        private class AkashiTimerLabels
        {
            private readonly ShipLabels[] _shipLines;
            private readonly Label[] _timerLabels = new Label[ShipInfo.MemberCount];
            private readonly int _lineHeight;
            private int _originalLeft;

            public AkashiTimerLabels(int lineHeight, ShipLabels[] shipLabels)
            {
                _shipLines = shipLabels;
                _lineHeight = lineHeight;
            }

            public void Create(Control parent)
            {
                const int x = 55;
                for (var i = 0; i < _timerLabels.Length; i++)
                {
                    var y = 3 + _lineHeight * (i + 1);
                    Label label;
                    parent.Controls.Add(
                        label = _timerLabels[i] =
                            new Label
                            {
                                Location = new Point(x, y),
                                AutoSize = true,
                                TextAlign = ContentAlignment.TopRight
                            });
                    label.BackColor = CustomColors.ColumnColors.DarkFirst(i);
                }
            }

            public void SetAkashiTimer(IReadOnlyList<ShipStatus> ships, AkashiTimer.RepairSpan[] timers)
            {
                var shortest = ShortestSpanIndex(timers);
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
                    }
                    else
                    {
                        if (i == shortest)
                            label.ForeColor = CUDColors.Red;
                        shipLabels.Hp.SetHp(ship.NowHp + timer.Diff, ship.MaxHp);
                        shipLabels.Hp.ForeColor = Color.DimGray;
                    }
                    AdjustAkashiTimer(label, shipLabels.Hp);
                }
            }

            private void AdjustAkashiTimer(Control timer, Control hp)
            {
                if (_originalLeft == 0)
                    _originalLeft = timer.Left;
                const int labelMargin = 3;
                var overlap = timer.Right - hp.Left - labelMargin;
                timer.Left = overlap < 0 ? _originalLeft : timer.Left - overlap;
            }

            private static int ShortestSpanIndex(AkashiTimer.RepairSpan[] timers)
            {
                var index = -1; // Spanが全部MinValueかZeroなら-1
                for (var i = 0; i < timers.Length; i++)
                {
                    if (timers[i].Span <= TimeSpan.Zero)
                        continue;
                    if (index == -1 || timers[i].Span < timers[index].Span)
                        index = i;
                }
                return index;
            }
        }

        private class CombinedShipLines
        {
            private readonly ShipLabels[] _combinedLines = new ShipLabels[ShipInfo.MemberCount * 2];

            private const int Top = 1;
            private const int LineHeight = 16;
            private ToolTip _toolTip;

            public void Create(Control parent, ShipListPanels panels)
            {
                _toolTip = panels._toolTip;
                parent.SuspendLayout();
                CreateHeader(parent, panels._hpToggle);
                for (var i = 0; i < _combinedLines.Length; i++)
                {
                    var x = PanelWidth / 2 * (i / ShipInfo.MemberCount);
                    var y = Top + LineHeight * (i % ShipInfo.MemberCount + 1);
                    var labels = _combinedLines[i] = new ShipLabels
                    {
                        Name = new ShipLabel.Name(new Point(x + 2, y + 2), ShipNameWidth.Combined),
                        Hp = new ShipLabel.Hp(new Point(x + 88, y), LineHeight),
                        Cond = new ShipLabel.Cond(new Point(x + 85, y), LineHeight),
                        BackGround = new Label {Location = new Point(x, y), Size = new Size(PanelWidth / 2, LineHeight)}
                    };
                    labels.Arrange(parent, CustomColors.ColumnColors.DarkFirst(i));
                    labels.SetClickHandler(panels._onClick);
                    labels.SetTag(i);
                    var hpLabel = _combinedLines[i].Hp;
                    panels._hpToggle.AddHpLabel(hpLabel);
                }
                parent.ResumeLayout();
            }

            private void CreateHeader(Control parent, HpToggle hpToggle)
            {
                var headings = new Control[]
                {
                    new Label {Location = new Point(68, Top), Text = "HP", AutoSize = true},
                    new Label {Location = new Point(86, Top), Text = "cnd", AutoSize = true},
                    new Label {Location = new Point(177, Top), Text = "HP", AutoSize = true},
                    new Label {Location = new Point(195, Top), Text = "cnd", AutoSize = true},
                    new Label {Location = new Point(0, 1), Size = new Size(PanelWidth, LineHeight - 1)}
                };
                parent.Controls.AddRange(headings);
                foreach (var control in headings)
                    control.BackColor = CustomColors.ColumnColors.Bright;
                headings[0].Cursor = headings[2].Cursor = Cursors.Hand;
                hpToggle.SetClickHandler(headings[0]);
                hpToggle.SetClickHandler(headings[2]);
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
                    labels.Set(ships[idx], _toolTip);
                }
            }
        }
    }
}