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
using static System.Math;

namespace KancolleSniffer
{
    public enum ShipNameWidth
    {
        MainPanel = 93,
        AkashiTimer = 53,
        NDock = 69,
        RepairList = NDock,
        RepairListFull = 75,
        ShipList = 82,
        GroupConfig = 82,
        Combined = 54,
        BattleResult = 65,
        CiShipName = 65,
        Max = int.MaxValue
    }

    public class MainFormLabels
    {
        private readonly ShipLabel[][] _shiplabels = new ShipLabel[ShipInfo.MemberCount][];
        private readonly ShipLabel[][] _shipLabels7 = new ShipLabel[7][];
        private readonly ShipLabel[][] _combinedLabels = new ShipLabel[ShipInfo.MemberCount * 2][];
        private readonly ShipLabel[] _akashiTimers = new ShipLabel[ShipInfo.MemberCount];
        private readonly ShipLabel[] _akashiTimers7 = new ShipLabel[ShipInfo.MemberCount];
        private readonly ShipLabel[][] _ndockLabels = new ShipLabel[DockInfo.DockCount][];
        private readonly List<ShipLabel> _hpLables = new List<ShipLabel>();
        public bool ShowHpInPercent { get; private set; }

        public void CreateShipLabels(Control parent, EventHandler onClick)
        {
            CreateShipLabels(parent, onClick, _shiplabels, 16);
        }

        public void CreateShipLabels7(Control parent, EventHandler onClick)
        {
            CreateShipLabels(parent, onClick, _shipLabels7, 14);
        }

        public void CreateShipLabels(Control parent, EventHandler onClick, ShipLabel[][] shipLabels, int lineHeight)
        {
            parent.SuspendLayout();
            const int top = 1, height = 12;
            ShipLabel[] headings;
            parent.Controls.AddRange(headings = new[]
            {
                new ShipLabel {Location = new Point(109, top), Text = "HP", AutoSize = true},
                new ShipLabel {Location = new Point(128, top), Text = "cond", AutoSize = true},
                new ShipLabel {Location = new Point(162, top), Text = "Lv", AutoSize = true},
                new ShipLabel {Location = new Point(194, top), Text = "Exp", AutoSize = true},
                new ShipLabel {Location = new Point(0, 1), Size = new Size(parent.Width, lineHeight - 1)}
            });
            foreach (var label in headings)
            {
                label.Scale();
                label.BackColor = ShipLabel.ColumnColors[1];
            }
            for (var i = 0; i < shipLabels.Length; i++)
            {
                var y = top + lineHeight * (i + 1);
                parent.Controls.AddRange(shipLabels[i] = new[]
                {
                    new ShipLabel
                    {
                        Location = new Point(129, y),
                        AutoSize = true,
                        AnchorRight = true,
                        MinimumSize = new Size(0, lineHeight),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Cursor = Cursors.Hand
                    },
                    new ShipLabel
                    {
                        Location = new Point(131, y),
                        Size = new Size(24, lineHeight),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    new ShipLabel
                    {
                        Location = new Point(155, y + 2),
                        Size = new Size(24, height),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    new ShipLabel
                    {
                        Location = new Point(176, y + 2),
                        Size = new Size(42, height),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    new ShipLabel {Location = new Point(2, y + 2), AutoSize = true}, // 名前のZ-orderを下に
                    new ShipLabel {Location = new Point(0, y), Size = new Size(parent.Width, lineHeight)}
                });
                foreach (var label in shipLabels[i])
                {
                    label.Scale();
                    label.PresetColor = label.BackColor = ShipLabel.ColumnColors[i % 2];
                    label.Tag = i;
                    label.Click += onClick;
                }
                var hpLabel = shipLabels[i][0];
                _hpLables.Add(hpLabel);
                hpLabel.Click += HpLabelClickHander;
            }
            headings[0].Cursor = Cursors.Hand;
            headings[0].Click += HpLabelClickHander;
            parent.ResumeLayout();
        }

        private void HpLabelClickHander(object sender, EventArgs ev)
        {
            ToggleHpPercent();
        }

        public void ToggleHpPercent()
        {
            ShowHpInPercent = !ShowHpInPercent;
            foreach (var label in _hpLables)
                label.ToggleHpPercent();
        }

        public void SetShipLabels(ShipStatus[] statuses)
        {
            SetShipLabels(statuses, statuses.Length == 7 ? _shipLabels7 : _shiplabels);
        }

        public void SetShipLabels(ShipStatus[] statuses, ShipLabel[][] shipLabels)
        {
            for (var i = 0; i < shipLabels.Length; i++)
            {
                var labels = shipLabels[i];
                var s = i < statuses.Length ? statuses[i] : null;
                labels[0].SetHp(s);
                labels[1].SetCond(s);
                labels[2].SetLevel(s);
                labels[3].SetExpToNext(s);
                labels[4].SetName(s, ShipNameWidth.MainPanel);
            }
        }

        public void CreateCombinedShipLabels(Control parent, EventHandler onClick)
        {
            parent.SuspendLayout();
            const int top = 1, lh = 16;
            const int parentWidth = 220; // parent.Widthを使うとDPIスケーリング時に計算がくるうので
            ShipLabel[] headings;
            parent.Controls.AddRange(headings = new[]
            {
                new ShipLabel {Location = new Point(68, top), Text = "HP", AutoSize = true},
                new ShipLabel {Location = new Point(86, top), Text = "cnd", AutoSize = true},
                new ShipLabel {Location = new Point(177, top), Text = "HP", AutoSize = true},
                new ShipLabel {Location = new Point(195, top), Text = "cnd", AutoSize = true},
                new ShipLabel {Location = new Point(0, 1), Size = new Size(parentWidth, lh - 1)}
            });
            foreach (var label in headings)
            {
                label.Scale();
                label.BackColor = ShipLabel.ColumnColors[1];
            }
            for (var i = 0; i < _combinedLabels.Length; i++)
            {
                var x = parentWidth / 2 * (i / ShipInfo.MemberCount);
                var y = top + lh * (i % ShipInfo.MemberCount + 1);
                parent.Controls.AddRange(_combinedLabels[i] = new[]
                {
                    new ShipLabel
                    {
                        Location = new Point(x + 88, y),
                        AutoSize = true,
                        AnchorRight = true,
                        MinimumSize = new Size(0, lh),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Cursor = Cursors.Hand
                    },
                    new ShipLabel
                    {
                        Location = new Point(x + 85, y),
                        Size = new Size(24, lh),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    new ShipLabel {Location = new Point(x + 2, y + 2), AutoSize = true}, // 名前のZ-orderを下に
                    new ShipLabel {Location = new Point(x, y), Size = new Size(parentWidth / 2, lh)}
                });
                foreach (var label in _combinedLabels[i])
                {
                    label.Scale();
                    label.PresetColor = label.BackColor = ShipLabel.ColumnColors[i % 2];
                    label.Tag = i;
                    label.Click += onClick;
                }
                var hpLavel = _combinedLabels[i][0];
                _hpLables.Add(hpLavel);
                hpLavel.Click += HpLabelClickHander;
            }
            headings[0].Cursor = headings[2].Cursor = Cursors.Hand;
            headings[0].Click += HpLabelClickHander;
            headings[2].Click += HpLabelClickHander;
            parent.ResumeLayout();
        }

        public void SetCombinedShipLabels(ShipStatus[] first, ShipStatus[] second)
        {
            for (var i = 0; i < _combinedLabels.Length; i++)
            {
                var idx = i % ShipInfo.MemberCount;
                var statuses = i < ShipInfo.MemberCount ? first : second;
                var labels = _combinedLabels[i];
                var s = idx < statuses.Length ? statuses[idx] : null;
                labels[0].SetHp(s);
                labels[1].SetCond(s);
                labels[2].SetName(s, ShipNameWidth.Combined);
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

        public void CreateAkashiTimers(Control parent, ShipLabel[] timerLabels, int lineHeight)
        {
            parent.SuspendLayout();
            for (var i = 0; i < timerLabels.Length; i++)
            {
                const int x = 55;
                var y = 3 + lineHeight * (i + 1);
                ShipLabel label;
                parent.Controls.Add(
                    label = timerLabels[i] =
                        new ShipLabel
                        {
                            Location = new Point(x, y),
                            Size = new Size(31, 12),
                            TextAlign = ContentAlignment.TopRight
                        });
                label.BackColor = ShipLabel.ColumnColors[i % 2];
            }
            foreach (var label in timerLabels)
                label.Scale();
            parent.ResumeLayout();
        }

        public void AdjustAkashiTimers()
        {
            AdjustAkashiTimers(_akashiTimers, 16);
            AdjustAkashiTimers(_akashiTimers7, 14);
        }

        public void AdjustAkashiTimers(ShipLabel[] timers, int lineHeight)
        {
            var scale = ShipLabel.ScaleFactor;
            if (scale.Height < 1.2)
                return;
            for (var i = 0; i < timers.Length; i++)
            {
                const int x = 55;
                var y = 3 + lineHeight * (i + 1);
                timers[i].Location = new Point((int)Round(x * scale.Width) - 3, (int)Round(y * scale.Height));
                timers[i].Size = new Size((int)Round(31 * scale.Width) + 1, (int)Round(12 * scale.Height));
            }
        }

        public void SetAkashiTimer(ShipStatus[] statuses, AkashiTimer.RepairSpan[] timers)
        {
            if (statuses.Length == 7)
            {
                SetAkashiTimer(statuses, timers, _akashiTimers7, _shipLabels7);
            }
            else
            {
                SetAkashiTimer(statuses, timers, _akashiTimers, _shiplabels);
            }
        }

        public void SetAkashiTimer(ShipStatus[] statuses, AkashiTimer.RepairSpan[] timers, ShipLabel[] timerLabels,
            ShipLabel[][] shipLabels)
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
                var labelHp = shipLabels[i][0];
                var labelName = shipLabels[i][4];
                if (i >= timers.Length || timers[i].Span == TimeSpan.MinValue)
                {
                    label.Visible = false;
                    labelHp.ForeColor = Control.DefaultForeColor;
                    continue;
                }
                var timer = timers[i];
                var stat = statuses[i];
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
                    label.ForeColor = CUDColor.Red;
                labelHp.ForeColor = Color.DimGray;
                labelHp.SetHp(stat.NowHp + timer.Diff, stat.MaxHp);
            }
        }

        public void CreateNDockLabels(Control parent, EventHandler onClick)
        {
            for (var i = 0; i < _ndockLabels.Length; i++)
            {
                var y = 3 + i * 15;
                parent.Controls.AddRange(
                    _ndockLabels[i] = new[]
                    {
                        new ShipLabel
                        {
                            Location = new Point(138, y),
                            AutoSize = true,
                            AnchorRight = true
                        },
                        new ShipLabel {Location = new Point(29, y), AutoSize = true} // 名前のZ-orderを下に
                    });
                foreach (var label in _ndockLabels[i])
                {
                    label.Scale();
                    label.Click += onClick;
                }
            }
        }

        public void SetNDockLabels(NameAndTimer[] ndock)
        {
            for (var i = 0; i < _ndockLabels.Length; i++)
                _ndockLabels[i][1].SetName(ndock[i].Name, ShipNameWidth.NDock);
        }

        public void SetNDockTimer(int dock, AlarmTimer timer, DateTime now, bool finishTime)
        {
            var label = _ndockLabels[dock][0];
            label.ForeColor = timer.IsFinished(now) ? CUDColor.Red : Color.Black;
            label.Text = timer.ToString(now, finishTime);
        }
    }
}