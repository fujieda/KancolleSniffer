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
using static System.Math;

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

    public class MainFormLabels
    {
        private readonly ShipLabel[][] _shipLabels = new ShipLabel[ShipInfo.MemberCount][];
        private readonly ShipLabel[][] _shipLabels7 = new ShipLabel[7][];
        private readonly ShipLabel[][] _combinedLabels = new ShipLabel[ShipInfo.MemberCount * 2][];
        private readonly ShipLabel[] _akashiTimers = new ShipLabel[ShipInfo.MemberCount];
        private readonly ShipLabel[] _akashiTimers7 = new ShipLabel[ShipInfo.MemberCount];
        private readonly ShipLabel[][] _ndockLabels = new ShipLabel[DockInfo.DockCount][];
        private readonly List<ShipLabel> _hpLabels = new List<ShipLabel>();
        public bool ShowHpInPercent { get; private set; }

        public void CreateShipLabels(Control parent, EventHandler onClick)
        {
            CreateShipLabels(parent, onClick, _shipLabels, 16);
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
                _hpLabels.Add(hpLabel);
                hpLabel.DoubleClick += HpLabelClickHandler;
            }
            headings[0].Cursor = Cursors.Hand;
            headings[0].Click += HpLabelClickHandler;
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

        public void SetShipLabels(IReadOnlyList<ShipStatus> ships, ShipLabel[][] shipLabels)
        {
            for (var i = 0; i < shipLabels.Length; i++)
            {
                var labels = shipLabels[i];
                var ship = i < ships.Count ? ships[i] : null;
                labels[0].SetHp(ship);
                labels[1].SetCond(ship);
                labels[2].SetLevel(ship);
                labels[3].SetExpToNext(ship);
                labels[4].SetName(ship, ShipNameWidth.MainPanel);
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
                var hpLabel = _combinedLabels[i][0];
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

        public void SetAkashiTimer(IReadOnlyList<ShipStatus> ships, AkashiTimer.RepairSpan[] timers, ShipLabel[] timerLabels,
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
                labelHp.ForeColor = Color.DimGray;
                labelHp.SetHp(stat.NowHp + timer.Diff, stat.MaxHp);
            }
        }

        public void CreateNDockLabels(Control parent, EventHandler onClick)
        {
            const int lh = 15;
            for (var i = 0; i < _ndockLabels.Length; i++)
            {
                var y = i * lh;
                parent.Controls.AddRange(
                    _ndockLabels[i] = new[]
                    {
                        new ShipLabel
                        {
                            Location = new Point(138, y + 2),
                            AutoSize = true,
                            AnchorRight = true,
                            MinimumSize = new Size(0, lh),
                            TextAlign = ContentAlignment.MiddleLeft,
                            Cursor = Cursors.Hand
                        },
                        new ShipLabel {Location = new Point(29, y + 3), AutoSize = true} // 名前のZ-orderを下に
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
            label.ForeColor = timer.IsFinished(now) ? CUDColors.Red : Color.Black;
            label.Text = timer.ToString(now, finishTime);
        }
    }
}