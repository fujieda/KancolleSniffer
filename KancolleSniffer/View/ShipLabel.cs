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
using static System.Math;

namespace KancolleSniffer.View
{
    [DesignerCategory("Code")]
    public class ShipLabel : Label
    {
        public static Color[] ColumnColors = {SystemColors.Control, Color.White};
        public static SizeF ScaleFactor { get; set; }
        public static Font LatinFont { get; set; } = new Font("Tahoma", 8f);
        public Color PresetColor { get; set; }
        public bool AnchorRight { get; set; }
        private int _right = Int32.MinValue;
        private int _left;
        private SlotStatus _slotStatus;
        private ShipStatus _status;
        private bool _hpPercent;
        private Font _strongFont;
        private ShipLabel _hpStrongLabel;

        private Font BaseFont => Parent.Font;

        private Font StrongFont => _strongFont ?? (_strongFont = new Font("Leelawadee", BaseFont.Size));

        public override Color BackColor
        {
            get => base.BackColor;
            set => base.BackColor = value == DefaultBackColor ? PresetColor : value;
        }

        [Flags]
        private enum SlotStatus
        {
            Equipped = 0,
            SemiEquipped = 1,
            NormalEmpty = 2,
            ExtraEmpty = 4
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
                var slots = status.Slot.Take(status.Spec.SlotNum).ToArray();
                if (slots.Any(item => item.Empty))
                    empty |= slots.All(item => item.Empty) ? SlotStatus.NormalEmpty : SlotStatus.SemiEquipped;
                if (status.SlotEx.Empty)
                    empty |= SlotStatus.ExtraEmpty;
            }
            var dc = status.PreparedDamageControl;
            var dcName = dc == 42 ? "[ダ]" :
                dc == 43 ? "[メ]" : "";
            var sp = "";
            switch (status.SpecialAttack)
            {
                case ShipStatus.Attack.Fire:
                    sp = "+";
                    break;
                case ShipStatus.Attack.Fired:
                    sp = "-";
                    break;
            }
            SetName((status.Escaped ? "[避]" : dcName) + sp, status.Name, empty, width);
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
            if (lu && Font.Equals(BaseFont))
            {
                Location += new Size(0, -shift);
                Font = LatinFont;
            }
            else if (!lu && !Font.Equals(BaseFont))
            {
                Location += new Size(0, shift);
                Font = BaseFont;
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
            if (_hpStrongLabel != null)
                _hpStrongLabel.Text = "";
            Font = BaseFont;
            if (status == null)
            {
                Text = "";
                BackColor = PresetColor;
                return;
            }
            if (_hpPercent)
            {
                var percent = $"{(int)Floor(status.NowHp * 100.0 / status.MaxHp):D}";
                if (status.DamageLevel == ShipStatus.Damage.Badly)
                {
                    Text = "%";
                    if (_hpStrongLabel == null)
                        CreateHpStrongLabel();
                    _hpStrongLabel.Text = percent;
                }
                else
                {
                    Text = percent + "%";
                }
            }
            else
            {
                Text = $"{status.NowHp:D}/{status.MaxHp:D}";
                if (status.DamageLevel == ShipStatus.Damage.Badly)
                    Font = StrongFont;
            }
            BackColor = DamageColor(status);
        }

        private void CreateHpStrongLabel()
        {
            _hpStrongLabel = new ShipLabel
            {
                Font = StrongFont,
                BackColor = CUDColors.Red,
                Location = new Point(Left + (int)Round(4 * ScaleFactor.Width), Top),
                AutoSize = true,
                MinimumSize = new Size(0, Height),
                AnchorRight = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };
            _hpStrongLabel.DoubleClick += (sender, e) => { OnDoubleClick(e); };
            Parent.Controls.Add(_hpStrongLabel);
            var index = Parent.Controls.GetChildIndex(this);
            Parent.Controls.SetChildIndex(_hpStrongLabel, index + 1);
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

        public Color DamageColor(ShipStatus status)
        {
            switch (status.DamageLevel)
            {
                case ShipStatus.Damage.Sunk:
                    return Color.CornflowerBlue;
                case ShipStatus.Damage.Badly:
                    return CUDColors.Red;
                case ShipStatus.Damage.Half:
                    return CUDColors.Orange;
                case ShipStatus.Damage.Small:
                    return Color.FromArgb(240, 240, 0);
                default:
                    return PresetColor;
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
                ? CUDColors.Yellow
                : cond >= 30
                    ? PresetColor
                    : cond >= 20
                        ? CUDColors.Orange
                        : CUDColors.Red;
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

        protected override void OnSizeChanged(EventArgs args)
        {
            base.OnSizeChanged(args);
            KeepAnchorRight();
        }

        protected override void OnLayout(LayoutEventArgs args)
        {
            base.OnLayout(args);
            KeepAnchorRight();
        }

        private void KeepAnchorRight()
        {
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
                    ClientSize.Width - 3 * ScaleFactor.Width, 0,
                    2 * ScaleFactor.Width, 5 * ScaleFactor.Height);
            }
            else if ((_slotStatus & SlotStatus.SemiEquipped) != 0)
            {
                e.Graphics.DrawLine(
                    Pens.Black,
                    ClientSize.Width - 1 * ScaleFactor.Width, 0,
                    ClientSize.Width - 1 * ScaleFactor.Width, 5 * ScaleFactor.Height);
            }
            if ((_slotStatus & SlotStatus.ExtraEmpty) != 0)
            {
                e.Graphics.DrawRectangle(
                    Pens.Black,
                    ClientSize.Width - 3 * ScaleFactor.Width, 8 * ScaleFactor.Height,
                    2 * ScaleFactor.Width, 3 * ScaleFactor.Height);
            }
        }

        public void Scale()
        {
            Scale(ScaleFactor);
        }
    }
}