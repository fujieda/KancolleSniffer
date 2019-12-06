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
    public abstract class ShipLabel : GrowLeftLabel
    {
        protected Color InitialBackColor;

        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                if (InitialBackColor == Color.Empty)
                    InitialBackColor = value;
                base.BackColor = value;
            }
        }

        protected ShipLabel()
        {
            UseMnemonic = false;
        }

        public abstract void Set(ShipStatus status);

        public abstract void Reset();

        public new sealed class Name : ShipLabel
        {
            private SlotStatus _slotStatus, _prevSlotStatus;
            private readonly ShipNameWidth _defaultWidth;

            public static Font LatinFont { get; set; }
            public static Font BaseFont { get; set; }

            public Name(Point location, ShipNameWidth defaultWidth)
            {
                _defaultWidth = defaultWidth;
                Location = location;
                AutoSize = true;
            }

            [Flags]
            private enum SlotStatus
            {
                Equipped = 0,
                SemiEquipped = 1,
                NormalEmpty = 2,
                ExtraEmpty = 4
            }

            public override void Set(ShipStatus status)
            {
                SetName(status, _defaultWidth);
            }

            public override void Reset()
            {
                SetName("");
            }

            public void SetName(ShipStatus status, ShipNameWidth width)
            {
                var slotStatus = GetSlotStatus(status);
                var dcName = DameConName(status);
                var sp = SpecialAttack(status);
                SetName((status.Escaped ? "[避]" : dcName) + sp, status.Name, slotStatus, width);
            }

            private static SlotStatus GetSlotStatus(ShipStatus status)
            {
                if (status.Empty)
                    return SlotStatus.Equipped;
                var slots = status.Slot.Take(status.Spec.SlotNum).ToArray();
                var normal = slots.Any(item => item.Empty)
                    ? slots.All(item => item.Empty) ? SlotStatus.NormalEmpty : SlotStatus.SemiEquipped
                    : SlotStatus.Equipped;
                var extra = status.SlotEx.Empty ? SlotStatus.ExtraEmpty : SlotStatus.Equipped;
                return normal | extra;
            }

            private string DameConName(ShipStatus status)
            {
                switch (status.PreparedDamageControl)
                {
                    case 42:
                        return "[ダ]";
                    case 43:
                        return "[メ]";
                    default:
                        return "";
                }
            }

            private string SpecialAttack(ShipStatus status)
            {
                switch (status.SpecialAttack)
                {
                    case ShipStatus.Attack.Fire:
                        return "+";
                    case ShipStatus.Attack.Fired:
                        return "-";
                    default:
                        return "";
                }
            }

            public void SetName(string name)
            {
                SetName(name, _defaultWidth);
            }

            public void SetName(string name, ShipNameWidth width)
            {
                SetName("", name, SlotStatus.Equipped, width);
            }

            private void SetName(string prefix, string name, SlotStatus slotStatus, ShipNameWidth width)
            {
                _slotStatus = slotStatus;
                ChangeFont(name);
                var realWidth = width == ShipNameWidth.Max ? (int)width : Scaler.ScaleWidth((int)width);
                Text = prefix + StringTruncator.Truncate(name, "", realWidth, Font);
                if (_prevSlotStatus != _slotStatus)
                    Invalidate(); // OnPaintを実行させるため
                _prevSlotStatus = _slotStatus;
            }

            private void ChangeFont(string name)
            {
                var lu = StartWithLetter(name);
                var shift = Scaler.ScaleHeight(1);
                if (lu && !Font.Equals(LatinFont))
                {
                    Location += new Size(0, -shift);
                    Font = LatinFont;
                }
                else if (!lu && Font.Equals(LatinFont))
                {
                    Location += new Size(0, shift);
                    Font = BaseFont;
                }
            }

            public static bool StartWithLetter(string name)
            {
                return Regex.IsMatch(name, @"^\p{Lu}");
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                if ((_slotStatus & SlotStatus.NormalEmpty) != 0)
                {
                    e.Graphics.DrawRectangle(Pens.Black,
                        new Rectangle(Scaler.Move(ClientSize.Width, 0, -3, 0), Scaler.Scale(2, 5)));
                }
                else if ((_slotStatus & SlotStatus.SemiEquipped) != 0)
                {
                    e.Graphics.DrawLine(Pens.Black,
                        Scaler.Move(ClientSize.Width, 0, -1, 0),
                        Scaler.Move(ClientSize.Width, 0, -1, 5));
                }
                if ((_slotStatus & SlotStatus.ExtraEmpty) != 0)
                {
                    e.Graphics.DrawRectangle(Pens.Black,
                        new Rectangle(Scaler.Move(ClientSize.Width, 0, -3, 8), Scaler.Scale(2, 3)));
                }
            }
        }

        public sealed class Hp : ShipLabel
        {
            private bool _hpPercent;
            private ShipStatus _status;

            public Hp()
            {
            }

            public Hp(Point location, int height)
            {
                Location = location;
                MinimumSize = new Size(0, height);
                TextAlign = ContentAlignment.MiddleLeft;
                GrowLeft = true;
                Cursor = Cursors.Hand;
            }

            public override void Reset()
            {
                _status = null;
                Text = "";
                BackColor = InitialBackColor;
            }

            public override void Set(ShipStatus status)
            {
                _status = status;
                Text = _hpPercent
                    ? $"{(int)Floor(status.NowHp * 100.0 / status.MaxHp):D}%"
                    : $"{status.NowHp:D}/{status.MaxHp:D}";
                BackColor = DamageColor(status);
            }

            public void ToggleHpPercent()
            {
                _hpPercent = !_hpPercent;
                if (_status != null)
                    Set(_status);
            }

            public void SetHp(int now, int max)
            {
                Set(new ShipStatus {NowHp = now, MaxHp = max});
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
                        return InitialBackColor;
                }
            }

            public void SetColor(ShipStatus status)
            {
                BackColor = DamageColor(status);
            }
        }

        public sealed class Cond : ShipLabel
        {
            public Cond(Point location, int height)
            {
                Location = location;
                Size = new Size(24, height);
                TextAlign = ContentAlignment.MiddleRight;
            }

            public override void Reset()
            {
                Text = "";
                BackColor = InitialBackColor;
            }

            public override void Set(ShipStatus status)
            {
                var cond = status.Cond;
                Text = cond.ToString("D");
                BackColor = cond >= 50
                    ? CUDColors.Yellow
                    : cond >= 30
                        ? InitialBackColor
                        : cond >= 20
                            ? CUDColors.Orange
                            : CUDColors.Red;
            }
        }

        public sealed class Level : ShipLabel
        {
            public Level(Point location, int height)
            {
                Location = location;
                Size = new Size(24, height);
                TextAlign = ContentAlignment.MiddleRight;
            }

            public override void Reset()
            {
                Text = "";
            }

            public override void Set(ShipStatus status)
            {
                Text = status.Level.ToString("D");
            }
        }

        public sealed class Exp : ShipLabel
        {
            public Exp(Point location, int height)
            {
                Location = location;
                Size = new Size(42, height);
                TextAlign = ContentAlignment.MiddleRight;
            }

            public override void Reset()
            {
                Text = "";
            }

            public override void Set(ShipStatus status)
            {
                Text = status.ExpToNext.ToString("D");
            }
        }

        public sealed class Fleet : ShipLabel
        {
            public Fleet(Point location)
            {
                Location = location;
                AutoSize = true;
            }

            public override void Reset()
            {
                Text = "";
            }

            public override void Set(ShipStatus status)
            {
                Text = status.Fleet == null ? "" : new[] {"1", "2", "3", "4"}[status.Fleet.Number];
            }
        }

        public sealed class RepairTime : ShipLabel
        {
            public RepairTime(Point location)
            {
                Location = location;
                AutoSize = true;
            }

            public override void Reset()
            {
                Text = "";
            }

            public override void Set(ShipStatus status)
            {
                SetRepairTime(status.RepairTime);
            }

            public void SetRepairTime(TimeSpan span)
            {
                Text = $@"{(int)span.TotalHours:d2}:{span:mm\:ss}";
            }
        }
    }
}