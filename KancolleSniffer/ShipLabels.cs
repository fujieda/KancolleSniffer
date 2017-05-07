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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
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
        Max = int.MaxValue
    }

    public class ShipLabels
    {
        private readonly ShipLabel[][] _labels = new ShipLabel[ShipInfo.MemberCount][];
        private readonly ShipLabel[][] _combinedLabels = new ShipLabel[ShipInfo.MemberCount * 2][];
        private readonly ShipLabel[] _akashiTimers = new ShipLabel[ShipInfo.MemberCount];
        private readonly ShipLabel[][] _ndockLabels = new ShipLabel[DockInfo.DockCount][];
        public static Color[] ColumnColors = {SystemColors.Control, Color.FromArgb(255, 250, 250, 250)};

        public void CreateShipLabels(Control parent, EventHandler onClick)
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

        public void SetShipLabels(ShipStatus[] statuses)
        {
            for (var i = 0; i < _labels.Length; i++)
            {
                var labels = _labels[i];
                if (i < statuses.Length)
                {
                    var s = statuses[i];
                    labels[0].SetHp(s);
                    labels[1].SetCond(s);
                    labels[2].SetLevel(s);
                    labels[3].SetExpToNext(s);
                    labels[4].SetName(s, ShipNameWidth.MainPanel);
                }
                else
                {
                    labels[0].Text = labels[1].Text = labels[2].Text = labels[3].Text = "";
                    labels[4].SetName("");
                    labels[0].BackColor = labels[1].BackColor = labels[0].PresetColor;
                }
            }
        }

        public void CreateCombinedShipLabels(Control parent, EventHandler onClick)
        {
            parent.SuspendLayout();
            const int top = 3, height = 12, lh = 16;
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
                label.BackColor = ColumnColors[1];
            }
            for (var i = 0; i < _combinedLabels.Length; i++)
            {
                var x = parentWidth / 2 * (i / ShipInfo.MemberCount);
                var y = top + lh * (i % ShipInfo.MemberCount + 1);
                parent.Controls.AddRange(_combinedLabels[i] = new[]
                {
                    new ShipLabel {Location = new Point(x + 88, y), AutoSize = true, AnchorRight = true},
                    new ShipLabel
                    {
                        Location = new Point(x + 86, y),
                        Size = new Size(23, height),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    new ShipLabel {Location = new Point(x + 2, y), AutoSize = true}, // 名前のZ-orderを下に
                    new ShipLabel {Location = new Point(x, y - 2), Size = new Size(parentWidth / 2, lh - 1)}
                });
                foreach (var label in _combinedLabels[i])
                {
                    label.Scale();
                    label.PresetColor = label.BackColor = ColumnColors[i % 2];
                    label.Tag = i;
                    label.Click += onClick;
                }
            }
            parent.ResumeLayout();
        }

        public void SetCombinedShipLabels(ShipStatus[] first, ShipStatus[] second)
        {
            for (var i = 0; i < _combinedLabels.Length; i++)
            {
                var idx = i % ShipInfo.MemberCount;
                var statuses = i < ShipInfo.MemberCount ? first : second;
                var labels = _combinedLabels[i];
                if (idx < statuses.Length)
                {
                    var s = statuses[idx];
                    labels[0].SetHp(s);
                    labels[1].SetCond(s);
                    labels[2].SetName(s, ShipNameWidth.Combined);
                }
                else
                {
                    labels[0].Text = labels[1].Text = "";
                    labels[2].SetName("");
                    labels[0].BackColor = labels[1].BackColor = labels[0].PresetColor;
                }
            }
        }

        public void CreateAkashiTimers(Control parent)
        {
            parent.SuspendLayout();
            for (var i = 0; i < _akashiTimers.Length; i++)
            {
                const int x = 55;
                var y = 3 + 16 * (i + 1);
                ShipLabel label;
                parent.Controls.Add(
                    label = _akashiTimers[i] =
                        new ShipLabel
                        {
                            Location = new Point(x, y),
                            Size = new Size(31, 12),
                            TextAlign = ContentAlignment.TopRight
                        });
                label.BackColor = ColumnColors[i % 2];
            }
            foreach (var label in _akashiTimers)
                label.Scale();
            parent.ResumeLayout();
        }

        public void AdjustAkashiTimers()
        {
            var scale = ShipLabel.ScaleFactor;
            if (scale.Height < 1.2 || scale.Height > 1.3)
                return;
            for (var i = 0; i < _akashiTimers.Length; i++)
            {
                const int x = 55;
                var y = 3 + 16 * (i + 1);
                _akashiTimers[i].Location = new Point((int)Round(x * scale.Width) - 3, (int)Round(y * scale.Height));
                _akashiTimers[i].Size = new Size((int)Round(31 * scale.Width) + 1, (int)Round(12 * scale.Height));
            }
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
                var labelName = _labels[i][4];
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

        public void SetNDockTimer(int dock, RingTimer timer, bool finishTime)
        {
            var label = _ndockLabels[dock][0];
            label.ForeColor = timer.IsFinished ? CUDColor.Red : Color.Black;
            label.Text = timer.ToString(finishTime);
        }
    }

    [DesignerCategory("Code")]
    public class ShipLabel : Label
    {
        public static SizeF ScaleFactor { get; set; }
        public static Font LatinFont { get; set; } = new Font("Tahoma", 8f);
        public Color PresetColor { get; set; }
        public bool AnchorRight { get; set; }
        private int _right = int.MinValue;
        private int _left;
        private SlotStatus _slotStatus;

        [Flags]
        private enum SlotStatus
        {
            Equipped = 0,
            NormalEmpty = 1,
            ExtraEmpty = 2
        }

        public ShipLabel()
        {
            UseMnemonic = false;
        }

        public void SetName(ShipStatus status, ShipNameWidth width = ShipNameWidth.Max)
        {
            SlotStatus empty = SlotStatus.Equipped;
            if (status.Id != -1)
            {
                if (status.Slot.All(item => item.Id == -1))
                    empty |= SlotStatus.NormalEmpty;
                if (status.SlotEx.Id == -1)
                    empty |= SlotStatus.ExtraEmpty;
            }
            var dc = status.PreparedDamageControl;
            var dcname = dc == 42 ? "[ダ]" : dc == 43 ? "[メ]" : "";
            SetName((status.Escaped ? "[避]" : dcname), status.Name, empty, width);
        }

        public void SetName(string name)
        {
            SetName("", name, SlotStatus.Equipped);
        }

        public void SetName(string name, ShipNameWidth width)
        {
            SetName("", name, SlotStatus.Equipped, width);
        }

        private void SetName(string prefix, string name, SlotStatus slotStatus, ShipNameWidth width = ShipNameWidth.Max)
        {
            if (name == null)
                return;
            _slotStatus = slotStatus;
            var lu = new Regex(@"^\p{Lu}").IsMatch(name);
            var shift = (int)Round(ScaleFactor.Height);
            if (lu && Font.Equals(Parent.Font))
            {
                Location += new Size(0, -shift);
                Font = LatinFont;
            }
            else if (!lu && !Font.Equals(Parent.Font))
            {
                Location += new Size(0, shift);
                Font = Parent.Font;
            }
            var result = prefix + name;
            var measured = TextRenderer.MeasureText(result, Font).Width;
            if (measured <= (int)width)
            {
                Text = result;
                Invalidate(); // 必ずOnPaintを実行させるため
                return;
            }
            var truncated = "";
            foreach (var ch in name)
            {
                var tmp = truncated + ch;
                if (TextRenderer.MeasureText(tmp, Font).Width > (int)width * ScaleFactor.Width)
                    break;
                truncated = tmp;
            }
            Text = prefix + truncated.TrimEnd(' ');
            Invalidate();
        }

        public void SetHp(ShipStatus status)
        {
            Text = $"{status.NowHp:D}/{status.MaxHp:D}";
            BackColor = DamageColor(status, PresetColor);
        }

        public void SetHp(int now, int max)
        {
            SetHp(new ShipStatus {NowHp = now, MaxHp = max});
        }

        public static Color DamageColor(ShipStatus status, Color backcolor)
        {
            switch (status.DamageLevel)
            {
                case ShipStatus.Damage.Sunk:
                    return Color.CornflowerBlue;
                case ShipStatus.Damage.Badly:
                    return CUDColor.Red;
                case ShipStatus.Damage.Half:
                    return CUDColor.Orange;
                case ShipStatus.Damage.Small:
                    return Color.FromArgb(240, 240, 0);
                default:
                    return backcolor;
            }
        }

        public void SetCond(ShipStatus status)
        {
            var cond = status.Cond;
            Text = cond.ToString("D");
            BackColor = cond >= 50
                ? CUDColor.Yellow
                : cond >= 30
                    ? PresetColor
                    : cond >= 20
                        ? CUDColor.Orange
                        : CUDColor.Red;
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if ((_slotStatus & SlotStatus.NormalEmpty) != 0)
            {
                e.Graphics.DrawRectangle(
                    Pens.Black,
                    ClientSize.Width - 3 * ScaleFactor.Width, 1 * ScaleFactor.Height,
                    2 * ScaleFactor.Width, 4 * ScaleFactor.Height);
            }
            if ((_slotStatus & SlotStatus.ExtraEmpty) != 0)
            {
                e.Graphics.DrawRectangle(
                    Pens.Black,
                    ClientSize.Width - 3 * ScaleFactor.Width, 7 * ScaleFactor.Height,
                    2 * ScaleFactor.Width, 3 * ScaleFactor.Height);
            }
        }

        public void Scale()
        {
            Scale(ScaleFactor);
        }
    }
}