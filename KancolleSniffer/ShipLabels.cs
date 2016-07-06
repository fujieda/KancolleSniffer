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
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Math;

namespace KancolleSniffer
{
    public class ShipLabels
    {
        private readonly ShipLabel[][] _labels = new ShipLabel[ShipInfo.MemberCount][];
        private readonly ShipLabel[][] _combinedLabels = new ShipLabel[ShipInfo.MemberCount * 2][];
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
                    labels[4].SetName(s);
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
                var y = top + lh * ((i % ShipInfo.MemberCount) + 1);
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

        public void SetCombinedShipInfo(ShipStatus[] first, ShipStatus[] second)
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
                    labels[2].SetName(s);
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
                            Size = new Size(34, 12),
                            TextAlign = ContentAlignment.TopRight
                        });
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
                labelName.SetName(stat, _shortNameDict);
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

        private readonly Dictionary<string, string> _shortNameDict = new Dictionary<string, string>
        {
            {"千代田航改", "千代田航"},
            {"千代田航改二", "千代田航"},
            {"千歳航改二", "千歳航改"},
            {"五十鈴改二", "五十鈴改"},
            {"あきつ丸改", "あきつ丸"},
            {"Bismarck改", "Bismarck"},
            {"Bismarck twei", "Bismarck"},
            {"Bismarck drei", "Bismarck"},
            {"Prinz Eugen", "Prinz Eug"},
            {"Prinz Eugen改", "Prinz Eug"},
            {"Graf Zeppelin", "Graf Zep"},
            {"Graf Zeppelin改", "Graf Zep"},
            {"Libeccio改", "Libeccio"},
            {"阿武隈改二", "阿武隈改"},
            {"瑞鶴改二甲", "瑞鶴改二"},
            {"翔鶴改二甲", "翔鶴改二"},
            {"朝潮改二丁", "朝潮改二"}
        };

        public void CreateDamagedShipList(Control parent, EventHandler onClick)
        {
            parent.SuspendLayout();
            for (var i = 0; i < _damagedShipList.Length; i++)
            {
                var y = 3 + i * 16;
                const int height = 12;
                parent.Controls.AddRange(_damagedShipList[i] = new[]
                {
                    new ShipLabel {Location = new Point(0, y), Size = new Size(11, height)},
                    new ShipLabel {Location = new Point(119, y), Size = new Size(5, height - 1)},
                    new ShipLabel {Location = new Point(75, y), AutoSize = true},
                    new ShipLabel {Location = new Point(9, y), AutoSize = true},
                    new ShipLabel {Location = new Point(0, y - 2), Size = new Size(parent.Width, height + 3)}
                });
                foreach (var label in _damagedShipList[i])
                {
                    label.Scale();
                    label.PresetColor = label.BackColor = ColumnColors[(i + 1) % 2];
                    label.Click += onClick;
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
            for (var i = 0; i < num; i++)
            {
                var s = list[i];
                var labels = _damagedShipList[i];
                labels[fleet].SetFleet(s);
                labels[name].SetName(s, new Dictionary<string, string>
                {
                    {"Graf Zeppelin", "Graf Zeppeli"},
                    {"Graf Zeppelin改", "Graf Zeppeli"},
                    {"千代田航改二", "千代田航改"}
                });
                labels[time].SetRepairTime(s);
                labels[damage].BackColor = ShipLabel.DamageColor(s, labels[damage].PresetColor);
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
            var dict = new Dictionary<string, string>
            {
                {"Graf Zeppelin", "Graf Zeppeli"},
                {"Graf Zeppelin改", "Graf Zeppeli"},
                {"千代田航改二", "千代田航改"}
            };
            for (var i = 0; i < _ndockLabels.Length; i++)
            {
                string name;
                _ndockLabels[i][1].SetName(dict.TryGetValue(ndock[i].Name, out name) ? name : ndock[i].Name);
            }
        }

        public void SetNDockTimer(int dock, RingTimer timer, bool finishTime)
        {
            var label = _ndockLabels[dock][0];
            label.ForeColor = timer.IsFinished ? Color.Red : Color.Black;
            label.Text = timer.ToString(finishTime);
        }
    }

    [System.ComponentModel.DesignerCategory("Code")]
    public class ShipLabel : Label
    {
        public static SizeF ScaleFactor { get; set; }
        public static Font LatinFont { get; set; } = new Font("Tahoma", 8f);
        public Color PresetColor { get; set; }
        public bool AnchorRight { get; set; }
        private int _right = int.MinValue;
        private int _left;

        public void SetName(ShipStatus status, Dictionary<string, string> convDict = null)
        {
            string name;
            if (convDict == null || !convDict.TryGetValue(status.Name, out name))
                name = status.Name;
            var empty = status.Id != -1 && status.Slot.All(e => e.Id == -1) ? "▫" : "";
            var dc = status.PreparedDamageControl;
            var dcname = dc == 42 ? "[ダ]" : dc == 43 ? "[メ]" : "";
            SetName((status.Escaped ? "[避]" : dcname) + name + empty);
        }

        public void SetName(string name)
        {
            var lu = name != null && new Regex(@"^(?:\[.\])?\p{Lu}").IsMatch(name);
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
            Text = name;
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
                case ShipStatus.Damage.Badly:
                    return Color.Red;
                case ShipStatus.Damage.Half:
                    return Color.Orange;
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