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
    /// <summary>
    /// 艦娘名の横幅
    /// 艦娘名のラベルのZ-orderは最下なので名前が長すぎると右隣のラベルの下に隠れるが、
    /// 空装備マークはラベルの右端に表示するので右端が見えるように縮める必要がある。
    /// </summary>
    public enum ShipNameWidth
    {
        MainPanel = 92, // 左端2 HP右端129幅35 129-2-35=92
        AkashiTimer = 53, // 左端2 タイマー左端55 55-2=53 漢字4文字
        NDock = 65, // 左端29 終了時刻右端138幅47 138-47-29=62 空装備マークなし漢字5文字65
        RepairList = 65, // 左端9 時間左端75 75-9=66 漢字5文字65
        RepairListFull = 73, // 左端10 HP右端118幅35 118-10-35=73
        ShipList = 81, // 左端10 HP右端126幅35 126-10-35=81
        GroupConfig = 80, // 左端10 レベル左端90 90-10=80
        Combined = 53, // 左端2 HP右端88幅35 88-2-35=51 空装備マーク犠牲 漢字4文字53
        BattleResult = 65, // 左端2 HP右端101幅35 101-1-35=65
        CiShipName = 65, // 左端168幅236 236-168=68 漢字5文字65
        Max = int.MaxValue
    }

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
        private readonly List<ShipLabel> _hpLabels = new List<ShipLabel>();
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
                    Name = new ShipLabel {Location = new Point(2, y + 2), AutoSize = true},
                    Hp = new ShipLabel
                    {
                        Location = new Point(129, y),
                        GrowLeft = true,
                        MinimumSize = new Size(0, lineHeight),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Cursor = Cursors.Hand
                    },
                    Cond = new ShipLabel
                    {
                        Location = new Point(131, y),
                        Size = new Size(24, lineHeight),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    Level = new ShipLabel
                    {
                        Location = new Point(155, y + 2),
                        Size = new Size(24, height),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    Exp = new ShipLabel
                    {
                        Location = new Point(176, y + 2),
                        Size = new Size(42, height),
                        TextAlign = ContentAlignment.MiddleRight
                    },
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
                var ship = i < ships.Count ? ships[i] : null;
                labels.Name.SetName(ship, ShipNameWidth.MainPanel);
                labels.Hp.SetHp(ship);
                labels.Cond.SetCond(ship);
                labels.Level.SetLevel(ship);
                labels.Exp.SetExpToNext(ship);
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
                    Name = new ShipLabel {Location = new Point(x + 2, y + 2), AutoSize = true},
                    Hp = new ShipLabel
                    {
                        Location = new Point(x + 88, y),
                        GrowLeft = true,
                        MinimumSize = new Size(0, lh),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Cursor = Cursors.Hand
                    },
                    Cond = new ShipLabel
                    {
                        Location = new Point(x + 85, y),
                        Size = new Size(24, lh),
                        TextAlign = ContentAlignment.MiddleRight
                    },
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
                var s = idx < ships.Count ? ships[idx] : null;
                labels.Name.SetName(s, ShipNameWidth.Combined);
                labels.Hp.SetHp(s);
                labels.Cond.SetCond(s);
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
            public ShipLabel Name { get; set; }
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
                    Name = new ShipLabel {Location = new Point(29, y + 3), AutoSize = true},
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
                _ndockLabels[i].Name.SetName(ndock[i].Name, ShipNameWidth.NDock);
        }

        public void SetNDockTimer(int dock, AlarmTimer timer, DateTime now, bool finishTime)
        {
            var label = _ndockLabels[dock].Timer;
            label.ForeColor = timer.IsFinished(now) ? CUDColors.Red : Color.Black;
            label.Text = timer.ToString(now, finishTime);
        }
    }
}