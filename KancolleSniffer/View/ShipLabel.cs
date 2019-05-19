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
        protected ShipStatus Status;

        protected Font BaseFont => Parent.Font;

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

        public new sealed class Name : ShipLabel
        {
            private SlotStatus _slotStatus;

            public static Font LatinFont { get; set; } = new Font("Tahoma", 8f);

            public Name(Point location)
            {
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
                SetName(status);
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

            private void SetName(string prefix, string name, SlotStatus slotStatus,
                ShipNameWidth width = ShipNameWidth.Max)
            {
                if (name == null)
                    name = "";
                _slotStatus = slotStatus;
                var lu = new Regex(@"^\p{Lu}").IsMatch(name);
                var shift = Scaler.ScaleHeight(1);
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
                    if (TextRenderer.MeasureText(tmp, Font).Width > Scaler.ScaleWidth((float)width))
                        break;
                    truncated = tmp;
                }
                Text = prefix + truncated.TrimEnd(' ');
                Invalidate();
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
            private Font _strongFont;
            private ShipLabel _hpStrongLabel;
            private Font StrongFont => _strongFont ?? (_strongFont = new Font("Leelawadee", BaseFont.Size));

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

            public override void Set(ShipStatus status)
            {
                Status = status;
                if (_hpStrongLabel != null)
                    _hpStrongLabel.Text = "";
                Font = BaseFont;
                if (status == null)
                {
                    Text = "";
                    BackColor = InitialBackColor;
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
                _hpStrongLabel = new Hp(Scaler.Move(Left, Top, 4, 0), Height)
                {
                    Font = StrongFont,
                    BackColor = CUDColors.Red
                };
                _hpStrongLabel.DoubleClick += (sender, e) => { OnDoubleClick(e); };
                Parent.Controls.Add(_hpStrongLabel);
                var index = Parent.Controls.GetChildIndex(this);
                Parent.Controls.SetChildIndex(_hpStrongLabel, index + 1);
            }

            public void ToggleHpPercent()
            {
                _hpPercent = !_hpPercent;
                Set(Status);
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

            public override void Set(ShipStatus status)
            {
                if (status == null)
                {
                    Text = "";
                    BackColor = InitialBackColor;
                    return;
                }
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

            public override void Set(ShipStatus status)
            {
                Text = status?.Level.ToString("D");
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

            public override void Set(ShipStatus status)
            {
                Text = status?.ExpToNext.ToString("D");
            }
        }

        public sealed class Fleet : ShipLabel
        {
            public Fleet(Point location)
            {
                Location = location;
                AutoSize = true;
            }

            public override void Set(ShipStatus status)
            {
                Text = status?.Fleet == null ? "" : new[] {"1", "2", "3", "4"}[status.Fleet.Number];
            }
        }

        public sealed class RepairTime : ShipLabel
        {
            public RepairTime(Point location)
            {
                Location = location;
                AutoSize = true;
            }

            public override void Set(ShipStatus status)
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
        }
    }
}