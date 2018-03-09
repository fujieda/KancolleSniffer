// Copyright (C) 2016 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Math;

namespace KancolleSniffer
{
    public class ShipListPanel : Panel
    {
        private const int LabelHeight = 12;
        public const int LineHeight = 16;
        private ShipStatus[] _shipList;
        private readonly List<ShipLabel[]> _labelList = new List<ShipLabel[]>();
        private readonly List<Panel> _labelPanelList = new List<Panel>();
        private readonly List<CheckBox[]> _checkBoxesList = new List<CheckBox[]>();
        private readonly List<ShipLabel[]> _groupingLabelList = new List<ShipLabel[]>();
        private readonly List<Panel> _groupingPanelList = new List<Panel>();
        private readonly List<ShipLabel[]> _repairLabelList = new List<ShipLabel[]>();
        private readonly List<Panel> _repairPanelList = new List<Panel>();
        private readonly List<ShipLabel> _hpLabels = new List<ShipLabel>();
        private string _mode;
        public bool ShowHpInPercent { get; private set; }

        public const int GroupCount = 4;
        public HashSet<int>[] GroupSettings { get; } = new HashSet<int>[GroupCount];
        public bool GroupUpdated { get; set; }

        public ScrollBar ScrollBar { get; }

        public ShipListPanel()
        {
            ScrollBar = new VScrollBar {Dock = DockStyle.Right, Visible = false};
            ScrollBar.ValueChanged += ScrollBarOnValueChanged;
            Controls.Add(ScrollBar);
        }

        private void ScrollBarOnValueChanged(object sender, EventArgs eventArgs)
        {
            SuspendDrawing();
            SetShipLabels();
            ResumeDrawing();
        }

        protected override void OnResize(EventArgs ev)
        {
            base.OnResize(ev);
            if (_shipList == null || _shipList.Length == 0 || !Visible)
                return;
            SuspendDrawing();
            SetupLabels();
            SetShipLabels();
            ResumeDrawing();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!ScrollBar.Visible)
                return;
            ScrollBar.Value = Max(ScrollBar.Minimum, Min(ScrollBar.Maximum - ScrollBar.LargeChange + 1,
                ScrollBar.Value - e.Delta * SystemInformation.MouseWheelScrollLines / 120));
        }

        public void Update(Sniffer sniffer, string mode, ListForm.SortOrder sortOrder, bool byShipType)
        {
            _mode = mode;
            CreateShipList(sniffer, sortOrder, byShipType);
            SuspendDrawing();
            SetupLabels();
            SetShipLabels();
            ResumeDrawing();
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, IntPtr lParam);

        private void SuspendDrawing()
        {
            SendMessage(Handle, 11, false, IntPtr.Zero); // WM_SETREDRAW = 11
            SuspendLayout();
        }

        public void ResumeDrawing()
        {
            ResumeLayout();
            SendMessage(Handle, 11, true, IntPtr.Zero);
            Refresh();
        }

        private void CreateShipList(Sniffer sniffer, ListForm.SortOrder sortOrder, bool byShipType)
        {
            var ships = _mode == "修復" ? sniffer.RepairList : FilterByGroup(sniffer.ShipList, _mode).ToArray();
            var order = _mode == "修復" ? ListForm.SortOrder.Repair : sortOrder;
            if (!byShipType)
            {
                _shipList = ships.OrderBy(s => s, new CompareShip(false, order)).ToArray();
                return;
            }
            var types = ships.Select(s => new {Id = s.Spec.ShipType, Name = s.Spec.ShipTypeName})
                .Distinct()
                .Select(stype =>
                    new ShipStatus
                    {
                        Spec = new ShipSpec {Name = stype.Name, ShipType = stype.Id},
                        Level = 1000,
                        NowHp = -1000,
                        Cond = -1000
                    });
            _shipList = ships.Concat(types).OrderBy(s => s, new CompareShip(true, order)).ToArray();
        }

        private IEnumerable<ShipStatus> FilterByGroup(IEnumerable<ShipStatus> ships, string group)
        {
            var g = Array.FindIndex(new[] {"A", "B", "C", "D"}, x => x == group);
            if (g == -1)
                return ships;
            return from s in ships where GroupSettings[g].Contains(s.Id) select s;
        }

        public IEnumerable<ShipStatus> CurrentShipList => _shipList.Where(ship => ship.Level != 1000);

        private class CompareShip : IComparer<ShipStatus>
        {
            private readonly bool _shipType;
            private readonly ListForm.SortOrder _order;

            public CompareShip(bool type, ListForm.SortOrder order)
            {
                _shipType = type;
                _order = order;
            }

            public int Compare(ShipStatus a, ShipStatus b)
            {
                if (a == null || b == null)
                    throw new ArgumentNullException();
                if (_shipType)
                {
                    if (a.Spec.ShipType != b.Spec.ShipType)
                        return a.Spec.ShipType - b.Spec.ShipType;
                    if (a.Level != b.Level)
                    {
                        if (a.Level == 1000)
                            return -1;
                        if (b.Level == 1000)
                            return 1;
                    }
                }
                if (_order == ListForm.SortOrder.Repair && a.RepairTime != b.RepairTime)
                    return (int)(b.RepairTime - a.RepairTime).TotalSeconds;
                if (a.Cond != b.Cond)
                {
                    if (_order == ListForm.SortOrder.CondAscend)
                        return a.Cond - b.Cond;
                    if (_order == ListForm.SortOrder.CondDescend)
                        return b.Cond - a.Cond;
                }
                if (a.Level != b.Level)
                {
                    if (_order == ListForm.SortOrder.ExpToNextAscend)
                        return b.Level - a.Level;
                    if (_order == ListForm.SortOrder.ExpToNextDescend)
                        return a.Level - b.Level;
                    if (!_shipType) // Condが同じかSortOrder.Noneで艦種なし
                        return b.Level - a.Level;
                }
                if (a.ExpToNext != b.ExpToNext)
                {
                    if (_order == ListForm.SortOrder.ExpToNextAscend)
                        return a.ExpToNext - b.ExpToNext;
                    if (_order == ListForm.SortOrder.ExpToNextDescend)
                        return b.ExpToNext - a.ExpToNext;
                }
                if (a.Spec.SortNo != b.Spec.SortNo)
                    return a.Spec.SortNo - b.Spec.SortNo;
                return a.Id - b.Id;
            }
        }

        private void SetupLabels()
        {
            for (var i = _labelList.Count; i * LineHeight < Height; i++)
            {
                CreateGroupingComponents(i);
                CreateRepairLabels(i);
                CreateShipLabels(i);
            }
            for (var i = 0; i * LineHeight < Height; i++)
            {
                _labelPanelList[i].Visible = InShipStatus(_mode);
                _groupingPanelList[i].Visible = _mode == "分類";
                _repairPanelList[i].Visible = _mode == "修復";
            }
            SetupScrollBar();
        }

        private void SetupScrollBar()
        {
            var needBar = _shipList.Length * LineHeight * ShipLabel.ScaleFactor.Height > Height;
            if (!needBar)
            {
                ScrollBar.Visible = false;
                ScrollBar.Value = 0;
                return;
            }
            ScrollBar.Visible = true;
            ScrollBar.Minimum = 0;
            var lines = Max(1, Height / (int)Round(LineHeight * ShipLabel.ScaleFactor.Height));
            var max = _shipList.Length - lines;
            var largeChange = Min(lines, max);
            ScrollBar.LargeChange = largeChange;
            ScrollBar.Maximum = Max(0, max + largeChange - 1); // ScrollBarを最大まで動かしてもmaxには届かない
            ScrollBar.Value = Min(ScrollBar.Value, max);
        }

        private void CreateGroupingComponents(int i)
        {
            var y = 3 + LineHeight * i;
            var cfgp = new Panel
            {
                Location = new Point(0, y - 2),
                Size = new Size(ListForm.PanelWidth, LineHeight - 1),
                BackColor = ShipLabel.ColumnColors[(i + 1) % 2],
            };
            cfgp.Scale(ShipLabel.ScaleFactor);
            cfgp.Tag = cfgp.Location.Y;
            var cfgl = new[]
            {
                new ShipLabel
                {
                    Location = new Point(90, 2),
                    Size = new Size(24, LabelHeight),
                    TextAlign = ContentAlignment.MiddleRight
                },
                new ShipLabel {Location = new Point(10, 2), AutoSize = true},
                new ShipLabel {Location = new Point(1, 2), AutoSize = true}
            };

            var cb = new CheckBox[GroupCount];
            for (var j = 0; j < cb.Length; j++)
            {
                cb[j] = new CheckBox
                {
                    Location = new Point(125 + j * 24, 2),
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(12, 11),
                    Tag = i * 10 + j
                };
                cb[j].Scale(ShipLabel.ScaleFactor);
                cb[j].CheckedChanged += checkboxGroup_CheckedChanged;
            }
            _groupingLabelList.Add(cfgl);
            _checkBoxesList.Add(cb);
            _groupingPanelList.Add(cfgp);
            // ReSharper disable CoVariantArrayConversion
            cfgp.Controls.AddRange(cfgl);
            cfgp.Controls.AddRange(cb);
            // ReSharper restore CoVariantArrayConversion
            Controls.Add(cfgp);
            foreach (var label in cfgl)
            {
                label.Scale();
                label.PresetColor =
                    label.BackColor = ShipLabel.ColumnColors[(i + 1) % 2];
            }
        }

        private void checkboxGroup_CheckedChanged(object sender, EventArgs e)
        {
            var cb = (CheckBox)sender;
            var group = (int)cb.Tag % 10;
            var idx = (int)cb.Tag / 10;
            if (cb.Checked)
            {
                GroupSettings[group].Add(_shipList[idx + ScrollBar.Value].Id);
            }
            else
            {
                GroupSettings[group].Remove(_shipList[idx + ScrollBar.Value].Id);
            }
            GroupUpdated = true;
        }

        private void CreateRepairLabels(int i)
        {
            var y = 3 + LineHeight * i;
            const int height = LabelHeight;
            var rpp = new Panel
            {
                Location = new Point(0, y - 2),
                Size = new Size(ListForm.PanelWidth, LineHeight - 1),
                BackColor = ShipLabel.ColumnColors[(i + 1) % 2],
            };
            rpp.Scale(ShipLabel.ScaleFactor);
            rpp.Tag = rpp.Location.Y;
            var rpl = new[]
            {
                new ShipLabel {Location = new Point(118, 2), AutoSize = true, AnchorRight = true},
                new ShipLabel
                {
                    Location = new Point(116, 2),
                    Size = new Size(24, height),
                    TextAlign = ContentAlignment.MiddleRight
                },
                new ShipLabel {Location = new Point(141, 2), AutoSize = true},
                new ShipLabel {Location = new Point(186, 2), AutoSize = true},
                new ShipLabel {Location = new Point(10, 2), AutoSize = true},
                new ShipLabel {Location = new Point(1, 2), AutoSize = true}
            };
            _repairLabelList.Add(rpl);
            _repairPanelList.Add(rpp);
            // ReSharper disable once CoVariantArrayConversion
            rpp.Controls.AddRange(rpl);
            Controls.Add(rpp);
            foreach (var label in rpl)
            {
                label.Scale();
                label.PresetColor =
                    label.BackColor = ShipLabel.ColumnColors[(i + 1) % 2];
            }
            if (ShowHpInPercent)
                rpl[0].ToggleHpPercent();
            _hpLabels.Add(rpl[0]);
        }

        private void CreateShipLabels(int i)
        {
            var y = 3 + LineHeight * i;
            const int height = LabelHeight;
            var lbp = new Panel
            {
                Location = new Point(0, y - 2),
                Size = new Size(ListForm.PanelWidth, LineHeight - 1),
                BackColor = ShipLabel.ColumnColors[(i + 1) % 2]
            };
            lbp.Scale(ShipLabel.ScaleFactor);
            var labels = new[]
            {
                new ShipLabel {Location = new Point(126, 2), AutoSize = true, AnchorRight = true},
                new ShipLabel
                {
                    Location = new Point(128, 2),
                    Size = new Size(24, height),
                    TextAlign = ContentAlignment.MiddleRight
                },
                new ShipLabel
                {
                    Location = new Point(154, 2),
                    Size = new Size(24, height),
                    TextAlign = ContentAlignment.MiddleRight
                },
                new ShipLabel
                {
                    Location = new Point(175, 2),
                    Size = new Size(42, height),
                    TextAlign = ContentAlignment.MiddleRight
                },
                new ShipLabel {Location = new Point(10, 2), AutoSize = true},
                new ShipLabel {Location = new Point(1, 2), AutoSize = true}
            };
            _labelList.Add(labels);
            _labelPanelList.Add(lbp);
            // ReSharper disable once CoVariantArrayConversion
            lbp.Controls.AddRange(labels);
            Controls.Add(lbp);
            foreach (var label in labels)
            {
                label.Scale();
                label.PresetColor =
                    label.BackColor = ShipLabel.ColumnColors[(i + 1) % 2];
            }
            if (ShowHpInPercent)
                labels[0].ToggleHpPercent();
            _hpLabels.Add(labels[0]);
        }

        private void SetShipLabels()
        {
            for (var i = 0; i < (Height + LineHeight - 1) / LineHeight; i++)
            {
                if (InShipStatus(_mode))
                    SetShipStatus(i);
                if (_mode == "分類")
                    SetGrouping(i);
                if (_mode == "修復")
                    SetRepairList(i);
            }
        }

        private bool InShipStatus(string mode) => Array.Exists(new[] {"全員", "A", "B", "C", "D"}, x => mode == x);

        private void SetShipStatus(int i)
        {
            var panel = _labelPanelList[i];
            if (i + ScrollBar.Value >= _shipList.Length)
            {
                panel.Visible = false;
                return;
            }
            var s = _shipList[i + ScrollBar.Value];
            var labels = _labelList[i];
            if (s.Level == 1000) // 艦種の表示
            {
                SetShipType(i);
                return;
            }
            labels[0].SetHp(s);
            labels[1].SetCond(s);
            labels[2].SetLevel(s);
            labels[3].SetExpToNext(s);
            labels[4].SetName(s, ShipNameWidth.ShipList);
            labels[5].SetFleet(s);
            panel.Visible = true;
        }

        private void SetShipType(int i)
        {
            var s = _shipList[i + ScrollBar.Value];
            var labels = _labelList[i];
            labels[0].SetHp(null);
            labels[1].SetCond(null);
            labels[2].SetLevel(null);
            labels[3].SetExpToNext(null);
            labels[4].SetName(null);
            labels[5].SetFleet(null);
            labels[5].Text = s.Name;
            _labelPanelList[i].Visible = true;
        }

        private void SetGrouping(int i)
        {
            var panel = _groupingPanelList[i];
            if (i + ScrollBar.Value >= _shipList.Length)
            {
                panel.Visible = false;
                _labelPanelList[i].Visible = false;
                return;
            }
            var s = _shipList[i + ScrollBar.Value];
            var labels = _groupingLabelList[i];
            if (s.Level == 1000)
            {
                panel.Visible = false;
                SetShipType(i);
                return;
            }
            labels[0].SetLevel(s);
            labels[1].SetName(s, ShipNameWidth.GroupConfig);
            labels[2].SetFleet(s);
            var cb = _checkBoxesList[i];
            for (var j = 0; j < cb.Length; j++)
                cb[j].Checked = GroupSettings[j].Contains(s.Id);
            panel.Visible = true;
        }

        private void SetRepairList(int i)
        {
            var panel = _repairPanelList[i];
            if (i + ScrollBar.Value >= _shipList.Length)
            {
                panel.Visible = false;
                _labelPanelList[i].Visible = false;
                return;
            }
            var s = _shipList[i + ScrollBar.Value];
            if (s.Level == 1000)
            {
                panel.Visible = false;
                SetShipType(i);
                return;
            }
            var rpl = _repairLabelList[i];
            rpl[0].SetHp(s);
            rpl[1].SetLevel(s);
            rpl[2].SetRepairTime(s);
            rpl[3].Text = s.RepairTimePerHp.ToString(@"mm\:ss");
            rpl[4].SetName(s, ShipNameWidth.RepairListFull);
            rpl[5].SetFleet(s);
            panel.Visible = true;
        }

        public void ToggleHpPercent()
        {
            ShowHpInPercent = !ShowHpInPercent;
            foreach (var label in _hpLabels)
                label.ToggleHpPercent();
        }

        public void ShowShip(int id)
        {
            var i = Array.FindIndex(_shipList, s => s.Id == id);
            if (i == -1)
                return;
            ScrollBar.Value = Min(i, ScrollBar.Maximum + 1 - ScrollBar.LargeChange);
            SetShipLabels();
        }
    }
}