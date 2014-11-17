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

        public ShipInfoLabels(Control parent)
        {
            CreateLabels(parent);
        }

        private void CreateLabels(Control parent)
        {
            parent.SuspendLayout();
            for (var i = 0; i < _labels.Length; i++)
            {
                var y = 20 + 16 * i;
                const int height = 12;
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
                    new ShipLabel {Location = new Point(2, y), AutoSize = true} // 名前のZ-orderを下に
                });
                _labels[i][0].SizeChanged += labelHP_SizeChanged;
            }
            parent.ResumeLayout();
        }

        public ShipLabel GetHpLabel(int idx)
        {
            return _labels[idx][0];
        }

        public ShipLabel GetNameLabel(int idx)
        {
            return _labels[idx][4];
        }

        private void labelHP_SizeChanged(object sender, EventArgs e)
        {
            var label = (Label)sender;
            label.Location =
                new Point(
                    (int)Math.Round(LabelHpRight * (ShipLabel.AutoScale ? ShipLabel.AutoScaleFactor.Width : 1f)) -
                    label.Width, label.Top);
        }

        public void SetShipInfo(ShipStatus[] statuses)
        {
            var empty = new ShipStatus();
            for (var i = 0; i < _labels.Length; i++)
            {
                var labels = _labels[i];
                var s = i < statuses.Length ? statuses[i] : empty;
                labels[4].SetName(s);
                labels[0].SetHp(s);
                labels[1].SetCond(s);
                labels[2].SetLevel(s);
                labels[3].SetExpToNext(s);
            }
        }
    }

    [System.ComponentModel.DesignerCategory("Code")]
    public class ShipLabel : Label
    {
        public static bool AutoScale { get; set; }
        public static SizeF AutoScaleFactor { get; set; }

        public void SetName(ShipStatus status)
        {
            SetName(status.Name);
        }

        public void SetName(string name)
        {
            var lu = name != null && new Regex(@"^\p{Lu}").IsMatch(name);
            if (lu && Font.Equals(Parent.Font))
            {
                Location += new Size(0, (int)Math.Round(-1 * AutoScaleFactor.Height));
                Font = new Font("Tahoma", AutoScale ? 8 : 8 / AutoScaleFactor.Height);
            }
            else if (!lu && !Font.Equals(Parent.Font))
            {
                Location += new Size(0, (int)Math.Round(1 * AutoScaleFactor.Height));
                Font = Parent.Font;
            }
            Text = name;
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            if (!AutoScale)
                return;
            Size = new Size((int)Math.Round(Size.Width * AutoScaleFactor.Width),
                (int)Math.Round(Size.Height * AutoScaleFactor.Height));
            Location = new Point((int)Math.Round(Location.X * AutoScaleFactor.Width),
                (int)Math.Round(Location.Y * AutoScaleFactor.Height));
        }

        public void SetHp(ShipStatus status)
        {
            SetHp(status.NowHp, status.MaxHp);
        }

        public void SetHp(int now, int max)
        {
            var colors = new[] {DefaultBackColor, Color.FromArgb(255, 240, 240, 100), Color.Orange, Color.Red};
            Text = string.Format("{0:D}/{1:D}", now, max);
            BackColor = colors[(int)ShipStatus.CalcDamage(now, max)];
        }

        public void SetCond(ShipStatus status)
        {
            if (status.Level == 0)
            {
                Text = "0";
                BackColor = DefaultBackColor;
                return;
            }
            var cond = status.Cond;
            Text = cond.ToString("D");
            BackColor = cond >= 50
                ? Color.Yellow
                : cond >= 30
                    ? DefaultBackColor
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