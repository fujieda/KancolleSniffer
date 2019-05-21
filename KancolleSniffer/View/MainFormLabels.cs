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
using System.Linq;
using System.Windows.Forms;
using KancolleSniffer.Model;

namespace KancolleSniffer.View
{
    public class MainFormPanels
    {
        public Control PanelShipInfo { get; set; }
        public Control Panel7Ships { get; set; }
        public Control PanelCombinedFleet { get; set; }
        public Control PanelNDock { get; set; }
    }

    public class MainFormLabels
    {
        private readonly ShipLabels[] _shipLabels = new ShipLabels[ShipInfo.MemberCount];
        private readonly ShipLabels[] _shipLabels7 = new ShipLabels[7];
        private readonly ShipLabels[] _combinedLabels = new ShipLabels[ShipInfo.MemberCount * 2];
        private readonly Label[] _akashiTimers = new Label[ShipInfo.MemberCount];
        private readonly Label[] _akashiTimers7 = new Label[ShipInfo.MemberCount];
        private readonly NDockLabels[] _ndockLabels = new NDockLabels[DockInfo.DockCount];
        private readonly List<ShipLabel.Hp> _hpLabels = new List<ShipLabel.Hp>();
        private readonly MainFormPanels _panels;
        public bool ShowHpInPercent { get; private set; }

        public MainFormLabels(MainFormPanels panels)
        {
            _panels = panels;
        }

        public void CreateAllShipLabels(EventHandler onClick)
        {
            CreateAkashiTimers(_panels.PanelShipInfo);
            CreateShipLabels(_panels.PanelShipInfo, onClick);
            CreateAkashiTimers7(_panels.Panel7Ships);
            CreateShipLabels7(_panels.Panel7Ships, onClick);
            CreateCombinedShipLabels(_panels.PanelCombinedFleet, onClick);
        }

        public void CreateNDockLabels(EventHandler onClick)
        {
            CreateNDockLabels(_panels.PanelNDock, onClick);
        }

        private void CreateShipLabels(Control parent, EventHandler onClick)
        {
            CreateShipLabels(parent, onClick, _shipLabels, 16);
        }

        private void CreateShipLabels7(Control parent, EventHandler onClick)
        {
            CreateShipLabels(parent, onClick, _shipLabels7, 14);
        }

        private void CreateShipLabels(Control parent, EventHandler onClick, ShipLabels[] shipLabels, int lineHeight)
        {
            parent.SuspendLayout();
            const int top = 1, height = 12;
            var headings = new Control[]
            {
                new Label {Location = new Point(109, top), Text = "HP", AutoSize = true},
                new Label {Location = new Point(128, top), Text = "cond", AutoSize = true},
                new Label {Location = new Point(162, top), Text = "Lv", AutoSize = true},
                new Label {Location = new Point(194, top), Text = "Exp", AutoSize = true},
                new Label {Location = new Point(0, 1), Size = new Size(parent.Width, lineHeight - 1)}
            };
            parent.Controls.AddRange(headings);
            foreach (var control in headings)
            {
                Scaler.Scale(control);
                control.BackColor = CustomColors.ColumnColors.Bright;
            }
            headings[0].Cursor = Cursors.Hand;
            headings[0].Click += HpLabelClickHandler;
            for (var i = 0; i < shipLabels.Length; i++)
            {
                var y = top + lineHeight * (i + 1);
                shipLabels[i] = new ShipLabels
                {
                    Name = new ShipLabel.Name(new Point(2, y + 2), ShipNameWidth.MainPanel),
                    Hp = new ShipLabel.Hp(new Point(129, y), lineHeight),
                    Cond = new ShipLabel.Cond(new Point(131, y), lineHeight),
                    Level = new ShipLabel.Level(new Point(155, y + 2), height),
                    Exp = new ShipLabel.Exp(new Point(176, y + 2), height),
                    BackGround = new Label {Location = new Point(0, y), Size = new Size(parent.Width, lineHeight)}
                };
                shipLabels[i].Arrange(parent, CustomColors.ColumnColors.DarkFirst(i));
                shipLabels[i].SetClickHandler(onClick);
                shipLabels[i].SetTag(i);
                var hpLabel = shipLabels[i].Hp;
                _hpLabels.Add(hpLabel);
                hpLabel.DoubleClick += HpLabelClickHandler;
            }
            parent.ResumeLayout();
        }

        private void HpLabelClickHandler(object sender, EventArgs ev)
        {
            ToggleHpPercent();
        }

        public void ToggleHpPercent()
        {
            ShowHpInPercent = !ShowHpInPercent;
            foreach (var label in _hpLabels)
                label.ToggleHpPercent();
        }

        public void SetShipLabels(IReadOnlyList<ShipStatus> ships)
        {
            SetShipLabels(ships, ships.Count == 7 ? _shipLabels7 : _shipLabels);
        }

        public void SetShipLabels(IReadOnlyList<ShipStatus> ships, ShipLabels[] shipLabels)
        {
            for (var i = 0; i < shipLabels.Length; i++)
            {
                var labels = shipLabels[i];
                if (i >= ships.Count)
                {
                    labels.Reset();
                    continue;
                }
                labels.Set(ships[i]);
            }
        }

        public void CreateCombinedShipLabels(Control parent, EventHandler onClick)
        {
            parent.SuspendLayout();
            const int top = 1, lh = 16;
            const int parentWidth = 220; // parent.Widthを使うとDPIスケーリング時に計算がくるうので
            var headings = new Control[]
            {
                new Label {Location = new Point(68, top), Text = "HP", AutoSize = true},
                new Label {Location = new Point(86, top), Text = "cnd", AutoSize = true},
                new Label {Location = new Point(177, top), Text = "HP", AutoSize = true},
                new Label {Location = new Point(195, top), Text = "cnd", AutoSize = true},
                new Label {Location = new Point(0, 1), Size = new Size(parentWidth, lh - 1)}
            };
            parent.Controls.AddRange(headings);
            foreach (var control in headings)
            {
                Scaler.Scale(control);
                control.BackColor = CustomColors.ColumnColors.Bright;
            }
            for (var i = 0; i < _combinedLabels.Length; i++)
            {
                var x = parentWidth / 2 * (i / ShipInfo.MemberCount);
                var y = top + lh * (i % ShipInfo.MemberCount + 1);
                _combinedLabels[i] = new ShipLabels
                {
                    Name = new ShipLabel.Name(new Point(x + 2, y + 2), ShipNameWidth.Combined),
                    Hp = new ShipLabel.Hp(new Point(x + 88, y), lh),
                    Cond = new ShipLabel.Cond(new Point(x + 85, y), lh),
                    BackGround = new Label {Location = new Point(x, y), Size = new Size(parentWidth / 2, lh)}
                };
                _combinedLabels[i].Arrange(parent, CustomColors.ColumnColors.DarkFirst(i));
                _combinedLabels[i].SetTag(i);
                var hpLabel = _combinedLabels[i].Hp;
                _hpLabels.Add(hpLabel);
                hpLabel.DoubleClick += HpLabelClickHandler;
            }
            headings[0].Cursor = headings[2].Cursor = Cursors.Hand;
            headings[0].Click += HpLabelClickHandler;
            headings[2].Click += HpLabelClickHandler;
            parent.ResumeLayout();
        }

        public void SetCombinedShipLabels(IReadOnlyList<ShipStatus> first, IReadOnlyList<ShipStatus> second)
        {
            for (var i = 0; i < _combinedLabels.Length; i++)
            {
                var idx = i % ShipInfo.MemberCount;
                var ships = i < ShipInfo.MemberCount ? first : second;
                var labels = _combinedLabels[i];
                if (idx >= ships.Count)
                {
                    labels.Reset();
                    continue;
                }
                labels.Set(ships[idx]);
            }
        }

        public void CreateAkashiTimers(Control parent)
        {
            CreateAkashiTimers(parent, _akashiTimers, 16);
        }

        public void CreateAkashiTimers7(Control parent)
        {
            CreateAkashiTimers(parent, _akashiTimers7, 14);
        }

        public void CreateAkashiTimers(Control parent, Label[] timerLabels, int lineHeight)
        {
            parent.SuspendLayout();
            for (var i = 0; i < timerLabels.Length; i++)
            {
                const int x = 55;
                var y = 3 + lineHeight * (i + 1);
                Label label;
                parent.Controls.Add(
                    label = timerLabels[i] =
                        new Label
                        {
                            Location = new Point(x, y),
                            Size = new Size(31, 12),
                            TextAlign = ContentAlignment.TopRight
                        });
                label.BackColor = CustomColors.ColumnColors.DarkFirst(i);
            }
            foreach (var label in timerLabels)
                Scaler.Scale(label);
            parent.ResumeLayout();
        }

        public void AdjustAkashiTimers()
        {
            AdjustAkashiTimers(_akashiTimers, 16);
            AdjustAkashiTimers(_akashiTimers7, 14);
        }

        public void AdjustAkashiTimers(Label[] timers, int lineHeight)
        {
            if (Scaler.ScaleHeight(1f) < 1.2)
                return;
            for (var i = 0; i < timers.Length; i++)
            {
                const int x = 55;
                var y = 3 + lineHeight * (i + 1);
                timers[i].Location = Scaler.Move(-3, 0, x, y);
                timers[i].Size = new Size(Scaler.ScaleWidth(31) + 1, Scaler.ScaleWidth(12));
            }
        }

        public void SetAkashiTimer(IReadOnlyList<ShipStatus> ships, AkashiTimer.RepairSpan[] timers)
        {
            if (ships.Count == 7)
            {
                SetAkashiTimer(ships, timers, _akashiTimers7, _shipLabels7);
            }
            else
            {
                SetAkashiTimer(ships, timers, _akashiTimers, _shipLabels);
            }
        }

        public void SetAkashiTimer(IReadOnlyList<ShipStatus> ships, AkashiTimer.RepairSpan[] timers,
            Label[] timerLabels, ShipLabels[] shipLabels)
        {
            var shortest = -1;
            for (var i = 0; i < timers.Length; i++)
            {
                if (timers[i].Span <= TimeSpan.Zero)
                    continue;
                if (shortest == -1 || timers[i].Span < timers[shortest].Span)
                    shortest = i;
            }
            for (var i = 0; i < timerLabels.Length; i++)
            {
                var label = timerLabels[i];
                var labelHp = shipLabels[i].Hp;
                var labelName = shipLabels[i].Name;
                if (i >= timers.Length || timers[i].Span == TimeSpan.MinValue)
                {
                    label.Visible = false;
                    labelHp.ForeColor = Control.DefaultForeColor;
                    continue;
                }
                var timer = timers[i];
                var stat = ships[i];
                label.Visible = true;
                label.Text = timer.Span.ToString(@"mm\:ss");
                label.ForeColor = Control.DefaultForeColor;
                labelName.SetName(stat, ShipNameWidth.AkashiTimer);
                if (timer.Diff == 0)
                {
                    labelHp.ForeColor = Control.DefaultForeColor;
                    continue;
                }
                if (i == shortest)
                    label.ForeColor = CUDColors.Red;
                labelHp.SetHp(stat.NowHp + timer.Diff, stat.MaxHp);
                labelHp.ForeColor = Color.DimGray;
            }
        }

        private class NDockLabels : ControlsArranger
        {
            public ShipLabel.Name Name { get; set; }
            public Label Timer { get; set; }

            public override Control[] Controls => new Control[] {Timer, Name};
        }

        public void CreateNDockLabels(Control parent, EventHandler onClick)
        {
            const int lh = 15;
            for (var i = 0; i < _ndockLabels.Length; i++)
            {
                var y = i * lh;
                _ndockLabels[i] = new NDockLabels
                {
                    Name = new ShipLabel.Name(new Point(29, y + 3), ShipNameWidth.NDock),
                    Timer = new GrowLeftLabel
                    {
                        Location = new Point(138, y + 2),
                        GrowLeft = true,
                        MinimumSize = new Size(0, lh),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Cursor = Cursors.Hand
                    }
                };
                _ndockLabels[i].Arrange(parent);
                _ndockLabels[i].SetClickHandler(onClick);
            }
        }

        public void SetNDockLabels(NameAndTimer[] ndock)
        {
            for (var i = 0; i < _ndockLabels.Length; i++)
                _ndockLabels[i].Name.SetName(ndock[i].Name);
        }

        public void SetNDockTimer(int dock, AlarmTimer timer, DateTime now, bool finishTime)
        {
            var label = _ndockLabels[dock].Timer;
            label.ForeColor = timer.IsFinished(now) ? CUDColors.Red : Color.Black;
            label.Text = timer.ToString(now, finishTime);
        }
    }
}