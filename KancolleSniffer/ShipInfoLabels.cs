// Copyright (C) 2014 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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

namespace KancolleSniffer
{
    public class ShipInfoLabels
    {
        private readonly ShipLabel[][] _labels = new ShipLabel[ShipInfo.MemberCount][];
        private const int LabelHpRight = 130;
        private readonly Label[] _akashiTimers = new Label[ShipInfo.MemberCount];
        public static Color[] ColumnColors = {SystemColors.Control, Color.FromArgb(255, 250, 250, 250)};

        public ShipInfoLabels(Control parent, EventHandler onClick)
        {
            CreateLabels(parent, onClick);
            CreateAkashiTimers(parent);
        }

        private void CreateLabels(Control parent, EventHandler onClick)
        {
            parent.SuspendLayout();
            const int top = 3, height = 12, lh = 16;
            Control[] headings =
            {
                new Label {Location = new Point(101, top), Text = "耐久", AutoSize = true},
                new Label {Location = new Point(131, top), Text = "cond", AutoSize = true},
                new Label {Location = new Point(159, top), Text = "レベル", AutoSize = true},
                new Label {Location = new Point(195, top), Text = "経験値", AutoSize = true},
                new Label {Location = new Point(0, 1), Size = new Size(parent.Width, lh - 1)}
            };
            foreach (var label in headings)
            {
                label.Scale(ShipLabel.ScaleFactor);
                label.BackColor = ColumnColors[1];
            }
            parent.Controls.AddRange(headings);
            for (var i = 0; i < _labels.Length; i++)
            {
                var y = top + lh * (i + 1);
                parent.Controls.AddRange(_labels[i] = new[]
                {
                    new ShipLabel {Location = new Point(LabelHpRight, y), AutoSize = true},
                    new ShipLabel
                    {
                        Location = new Point(136, y),
                        Size = new Size(23, height),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    new ShipLabel
                    {
                        Location = new Point(170, y),
                        Size = new Size(23, height),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    new ShipLabel
                    {
                        Location = new Point(195, y),
                        Size = new Size(41, height),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    new ShipLabel {Location = new Point(2, y), AutoSize = true}, // 名前のZ-orderを下に
                    new ShipLabel {Location = new Point(0, y - 2), Size = new Size(parent.Width, lh - 1)}
                });
                _labels[i][0].SizeChanged += labelHP_SizeChanged;
                foreach (var label in _labels[i])
                {
                    label.Scale(ShipLabel.ScaleFactor);
                    label.PresetColor = label.BackColor = ColumnColors[i % 2];
                    label.Tag = i;
                    label.Click += onClick;
                }
            }
            parent.ResumeLayout();
        }

        private void labelHP_SizeChanged(object sender, EventArgs e)
        {
            var label = (Label)sender;
            // フォントが縮小されていなかったら移動幅を広げる
            var scale = label.Font.Equals(Control.DefaultFont) ? ShipLabel.ScaleFactor.Width : 1f;
            label.Location = new Point((int)Math.Round(LabelHpRight * scale) - label.Width, label.Top);
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

        private void CreateAkashiTimers(Control parent)
        {
            parent.SuspendLayout();
            for (var i = 0; i < _akashiTimers.Length; i++)
            {
                const int x = 56;
                var y = 20 + 16 * i;
                Label label;
                parent.Controls.Add(
                    label = _akashiTimers[i] = new Label {Location = new Point(x, y), AutoSize = true, Visible = false});
                parent.Controls.SetChildIndex(label, 0);
                label.BackColor = ColumnColors[i % 2];
            }
            foreach (var label in _akashiTimers)
                label.Scale(ShipLabel.ScaleFactor);
            parent.ResumeLayout();
        }

        public void SetAkashiTimer(ShipStatus[] statuses, AkashiTimer.RepairSpan[] timers)
        {
            for (var i = 0; i < _akashiTimers.Length; i++)
            {
                var label = _akashiTimers[i];
                var labelHp = _labels[i][0];
                if (timers == null || i >= timers.Length || timers[i].Span == TimeSpan.MinValue)
                {
                    label.Visible = false;
                    labelHp.ForeColor = Control.DefaultForeColor;
                    continue;
                }
                var timer = timers[i];
                var stat = statuses[i];
                label.Visible = true;
                label.Text = timer.Span.ToString(@"mm\:ss");
                if (timer.Diff == 0)
                {
                    labelHp.ForeColor = Control.DefaultForeColor;
                    continue;
                }
                labelHp.ForeColor = Color.DimGray;
                labelHp.SetHp(stat.NowHp + timer.Diff, stat.MaxHp);
            }
        }
    }

    [System.ComponentModel.DesignerCategory("Code")]
    public class ShipLabel : Label
    {
        public static SizeF ScaleFactor { get; set; }
        public Color PresetColor { get; set; }

        public void SetName(ShipStatus status)
        {
            SetName(status.Name);
        }

        public void SetName(string name)
        {
            var lu = name != null && new Regex(@"^\p{Lu}").IsMatch(name);
            // フォントが縮小されていなかったら移動幅を広げる
            var shift = Parent.Font.Equals(DefaultFont) ? (int)Math.Round(ScaleFactor.Height) : 1;
            if (lu && Font.Equals(Parent.Font))
            {
                Location += new Size(0, -shift);
                Font = new Font("Tahoma", 8f * Font.Height / DefaultFont.Height);
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
            Text = string.Format("{0:D}/{1:D}", now, max);
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
    }
}