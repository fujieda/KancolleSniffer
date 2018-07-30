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
using KancolleSniffer.Model;
using KancolleSniffer.Util;
using static System.Math;

namespace KancolleSniffer.View
{
    [DesignerCategory("Code")]
    public class ShipLabel : Label
    {
        public static Color[] ColumnColors = {SystemColors.Control, Color.FromArgb(255, 250, 250, 250)};
        public static SizeF ScaleFactor { get; set; }
        public static Font LatinFont { get; set; } = new Font("Tahoma", 8f);
        public Color PresetColor { get; set; }
        public bool AnchorRight { get; set; }
        private int _right = Int32.MinValue;
        private int _left;
        private SlotStatus _slotStatus;
        private ShipStatus _status;
        private bool _hpPercent;

        public override Color BackColor
        {
            get => base.BackColor;
            set => base.BackColor = value == DefaultBackColor ? PresetColor : value;
        }

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
            if (status == null)
            {
                SetName("");
                return;
            }
            var empty = SlotStatus.Equipped;
            if (!status.Empty)
            {
                if (status.Slot.All(item => item.Empty))
                    empty |= SlotStatus.NormalEmpty;
                if (status.SlotEx.Empty)
                    empty |= SlotStatus.ExtraEmpty;
            }
            var dc = status.PreparedDamageControl;
            var dcname = dc == 42 ? "[ダ]" :
                dc == 43 ? "[メ]" : "";
            SetName(status.Escaped ? "[避]" : dcname, status.Name, empty, width);
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
                name = "";
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
            _status = status;
            if (status == null)
            {
                Text = "";
                BackColor = PresetColor;
                return;
            }
            Text = _hpPercent
                ? $"{(int)Floor(status.NowHp * 100.0 / status.MaxHp):D}%"
                : $"{status.NowHp:D}/{status.MaxHp:D}";
            BackColor = DamageColor(status, PresetColor);
        }

        public void ToggleHpPercent()
        {
            _hpPercent = !_hpPercent;
            SetHp(_status);
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
            if (status == null)
            {
                Text = "";
                BackColor = PresetColor;
                return;
            }
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
            Text = status?.Level.ToString("D");
        }

        public void SetExpToNext(ShipStatus status)
        {
            Text = status?.ExpToNext.ToString("D");
        }

        public void SetRepairTime(ShipStatus status)
        {
            if (status == null)
            {
                Text = "";
                return;
            }
            SetRepairTime(status.RepairTime);
        }

        public void SetRepairTime(TimeSpan span)
        {
            Text = $@"{(int)span.TotalHours:d2}:{span:mm\:ss}";
        }

        public void SetFleet(ShipStatus status)
        {
            Text = status?.Fleet == null ? "" : new[] {"1", "2", "3", "4"}[status.Fleet.Number];
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (!AnchorRight)
                return;
            if (_right == Int32.MinValue || _left != Left)
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