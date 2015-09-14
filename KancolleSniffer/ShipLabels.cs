﻿// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
// 
// This program is part of KancolleSniffer.
//
// KancolleSniffer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, see <http://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Math;

namespace KancolleSniffer
{
    public class ShipLabels
    {
        private readonly ShipLabel[][] _labels = new ShipLabel[ShipInfo.MemberCount][];
        private readonly ShipLabel[] _akashiTimers = new ShipLabel[ShipInfo.MemberCount];
        private readonly ShipLabel[][] _damagedShipList = new ShipLabel[16][];
        private Control _panelDamagedShipList;
        private readonly ShipLabel[][] _ndockLabels = new ShipLabel[DockInfo.DockCount][];
        public static Color[] ColumnColors = {SystemColors.Control, Color.FromArgb(255, 250, 250, 250)};

        public void CreateLabels(Control parent, EventHandler onClick)
        {
            parent.SuspendLayout();
            const int top = 3, height = 12, lh = 16;
            ShipLabel[] headings;
            parent.Controls.AddRange(headings = new[]
            {
                new ShipLabel {Location = new Point(109, top), Text = "HP", AutoSize = true},
                new ShipLabel {Location = new Point(128, top), Text = "cond", AutoSize = true},
                new ShipLabel {Location = new Point(163, top), Text = "Lv", AutoSize = true},
                new ShipLabel {Location = new Point(194, top), Text = "Exp", AutoSize = true},
                new ShipLabel {Location = new Point(0, 1), Size = new Size(parent.Width, lh - 1)}
            });
            foreach (var label in headings)
            {
                label.Scale();
                label.BackColor = ColumnColors[1];
            }
            for (var i = 0; i < _labels.Length; i++)
            {
                var y = top + lh * (i + 1);
                parent.Controls.AddRange(_labels[i] = new[]
                {
                    new ShipLabel {Location = new Point(129, y), AutoSize = true, AnchorRight = true},
                    new ShipLabel
                    {
                        Location = new Point(132, y),
                        Size = new Size(23, height),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    new ShipLabel
                    {
                        Location = new Point(157, y),
                        Size = new Size(23, height),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    new ShipLabel
                    {
                        Location = new Point(177, y),
                        Size = new Size(41, height),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    new ShipLabel {Location = new Point(2, y), AutoSize = true}, // 名前のZ-orderを下に
                    new ShipLabel {Location = new Point(0, y - 2), Size = new Size(parent.Width, lh - 1)}
                });
                foreach (var label in _labels[i])
                {
                    label.Scale();
                    label.PresetColor = label.BackColor = ColumnColors[i % 2];
                    label.Tag = i;
                    label.Click += onClick;
                }
            }
            parent.ResumeLayout();
        }

        public void SetShipInfo(ShipStatus[] statuses)
        {
            var empty = new ShipStatus();
            for (var i = 0; i < _labels.Length; i++)
            {
                var labels = _labels[i];
                var s = i < statuses.Length ? statuses[i] : empty;
                labels[0].SetHp(s);
                labels[1].SetCond(s);
                labels[2].SetLevel(s);
                labels[3].SetExpToNext(s);
                labels[4].SetName(s);
            }
        }

        public void CreateAkashiTimers(Control parent)
        {
            parent.SuspendLayout();
            for (var i = 0; i < _akashiTimers.Length; i++)
            {
                const int x = 54;
                var y = 20 + 16 * i;
                ShipLabel label;
                parent.Controls.Add(
                    label = _akashiTimers[i] = new ShipLabel {Location = new Point(x, y), AutoSize = true});
                label.BackColor = ColumnColors[i % 2];
            }
            foreach (var label in _akashiTimers)
                label.Scale();
            parent.ResumeLayout();
        }

        public void SetAkashiTimer(ShipStatus[] statuses, AkashiTimer.RepairSpan[] timers)
        {
            var shortest = -1;
            for (var i = 0; i < timers.Length; i++)
            {
                if (timers[i].Span <= TimeSpan.Zero)
                    continue;
                if (shortest == -1 || timers[i].Span < timers[shortest].Span)
                    shortest = i;
            }
            for (var i = 0; i < _akashiTimers.Length; i++)
            {
                var label = _akashiTimers[i];
                var labelHp = _labels[i][0];
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
                if (timer.Diff == 0)
                {
                    labelHp.ForeColor = Control.DefaultForeColor;
                    continue;
                }
                if (i == shortest)
                    label.ForeColor = Color.Red;
                labelHp.ForeColor = Color.DimGray;
                labelHp.SetHp(stat.NowHp + timer.Diff, stat.MaxHp);
            }
        }

        public void CreateDamagedShipList(Control parent)
        {
            parent.SuspendLayout();
            for (var i = 0; i < _damagedShipList.Length; i++)
            {
                var y = 3 + i * 16;
                const int height = 12;
                parent.Controls.AddRange(_damagedShipList[i] = new[]
                {
                    new ShipLabel {Location = new Point(0, y), Size = new Size(11, height)},
                    new ShipLabel {Location = new Point(119, y), Size = new Size(4, height - 1)},
                    new ShipLabel {Location = new Point(75, y), AutoSize = true},
                    new ShipLabel {Location = new Point(9, y), AutoSize = true},
                    new ShipLabel {Location = new Point(0, y - 2), Size = new Size(parent.Width, height + 3)}
                });
                foreach (var label in _damagedShipList[i])
                {
                    label.Scale();
                    label.PresetColor = label.BackColor = ColumnColors[(i + 1) % 2];
                }
            }
            _panelDamagedShipList = parent;
            parent.ResumeLayout();
        }

        public void SetDamagedShipList(ShipStatus[] list)
        {
            const int fleet = 0, name = 3, time = 2, damage = 1;
            var parent = _panelDamagedShipList;
            var num = Min(list.Length, _damagedShipList.Length);
            if (num == 0)
            {
                parent.Size = new Size(parent.Width, (int)Round(ShipLabel.ScaleFactor.Height * 19));
                var labels = _damagedShipList[0];
                labels[fleet].Text = "";
                labels[name].SetName("なし");
                labels[time].Text = "";
                labels[damage].BackColor = labels[damage].PresetColor;
                return;
            }
            parent.Size = new Size(parent.Width, (int)Round(ShipLabel.ScaleFactor.Height * (num * 16 + 3)));
            var colors = new[] { Color.FromArgb(255, 225, 225, 21), Color.Orange, Color.Red };
            for (var i = 0; i < num; i++)
            {
                var s = list[i];
                var labels = _damagedShipList[i];
                labels[fleet].SetFleet(s);
                labels[name].SetName(s);
                labels[time].SetRepairTime(s);
                labels[damage].BackColor = (int)s.DamageLevel == 0
                    ? labels[damage].PresetColor
                    : colors[(int)s.DamageLevel - 1];
            }
        }

        public void CreateNDockLabels(Control parent)
        {
            for (var i = 0; i < _ndockLabels.Length; i++)
            {
                var y = 3 + i * 15;
                parent.Controls.AddRange(
                    _ndockLabels[i] = new[]
                    {
                        new ShipLabel {Location = new Point(93, y), AutoSize = true, Text = "00:00:00"},
                        new ShipLabel {Location = new Point(29, y), AutoSize = true} // 名前のZ-orderを下に
                    });
                foreach (var label in _ndockLabels[i])
                    label.Scale();
            }
        }

        public void SetNDockLabels(NameAndTimer[] ndock)
        {
            for (var i = 0; i < _ndockLabels.Length; i++)
                _ndockLabels[i][1].SetName(ndock[i].Name);
        }

        public void SetNDockTimer(int dock, RingTimer timer)
        {
            var label = _ndockLabels[dock][0];
            label.ForeColor = timer.IsFinished ? Color.Red : Color.Black;
            label.SetRepairTime(timer.Rest);
        }
    }

    [System.ComponentModel.DesignerCategory("Code")]
    public class ShipLabel : Label
    {
        public static SizeF ScaleFactor { get; set; }
        public Color PresetColor { get; set; }
        public bool AnchorRight { get; set; }
        private int _right = int.MinValue;
        private int _left;

        public void SetName(ShipStatus status)
        {
            SetName((status.Escaped ? "[避]" : "") + status.Name);
        }

        public void SetName(string name)
        {
            var lu = name != null && new Regex(@"^\p{Lu}").IsMatch(name);
            var shift = (int)Round(ScaleFactor.Height);
            if (lu && Font.Equals(Parent.Font))
            {
                Location += new Size(0, -shift);
                Font = new Font("Tahoma", 8f);
            }
            else if (!lu && !Font.Equals(Parent.Font))
            {
                Location += new Size(0, shift);
                Font = Parent.Font;
            }
            Text = name;
        }

        public void SetHp(ShipStatus status)
        {
            SetHp(status.NowHp, status.MaxHp);
        }

        public void SetHp(int now, int max)
        {
            var colors = new[] {PresetColor, Color.FromArgb(255, 240, 240, 100), Color.Orange, Color.Red};
            Text = $"{now:D}/{max:D}";
            BackColor = colors[(int)ShipStatus.CalcDamage(now, max)];
        }

        public void SetCond(ShipStatus status)
        {
            if (status.Level == 0)
            {
                Text = "0";
                BackColor = PresetColor;
                return;
            }
            var cond = status.Cond;
            Text = cond.ToString("D");
            BackColor = cond >= 50
                ? Color.Yellow
                : cond >= 30
                    ? PresetColor
                    : cond >= 20 ? Color.Orange : Color.Red;
        }

        public void SetLevel(ShipStatus status)
        {
            Text = status.Level.ToString("D");
        }

        public void SetExpToNext(ShipStatus status)
        {
            Text = status.ExpToNext.ToString("D");
        }

        public void SetRepairTime(ShipStatus status)
        {
            SetRepairTime(status.RepairTime);
        }

        public void SetRepairTime(TimeSpan span)
        {
            Text = $@"{(int)span.TotalHours:d2}:{span:mm\:ss}";
        }

        public void SetFleet(ShipStatus status)
        {
            Text = new[] {"", "1", "2", "3", "4"}[status.Fleet + 1];
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (!AnchorRight)
                return;
            if (_right == int.MinValue || _left != Left)
            {
                _right = Right;
                _left = Left;
                return;
            }
            if (_right == Right)
                return;
            _left -= Right - _right;
            Location = new Point(_left, Top);
        }

        public void Scale()
        {
            Scale(ScaleFactor);
        }
    }
}