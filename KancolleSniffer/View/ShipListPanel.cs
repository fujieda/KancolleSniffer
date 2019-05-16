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
using KancolleSniffer.Model;
using static System.Math;

namespace KancolleSniffer.View
{
    public class ShipListPanel : Panel
    {
        public const int LabelHeight = 12;
        public const int LineHeight = 16;
        private ShipStatus[] _shipList;
        private readonly List<ShipLabel[]> _labelList = new List<ShipLabel[]>();
        private readonly List<Panel> _labelPanelList = new List<Panel>();
        private readonly List<ShipLabel> _hpLabels = new List<ShipLabel>();
        private readonly GroupConfigLabels _groupConfigLabels;
        private readonly RepairListLabels _repairListLabels;
        private string _mode;
        private bool _hpPercent;

        public HashSet<int>[] GroupSettings => _groupConfigLabels.GroupSettings;

        public bool GroupUpdated
        {
            get => _groupConfigLabels.GroupUpdated;
            set => _groupConfigLabels.GroupUpdated = value;
        }

        public ScrollBar ScrollBar { get; }

        public ShipStatus GetShip(int i)
        {
            return _shipList[i + ScrollBar.Value];
        }

        public ShipListPanel()
        {
            ScrollBar = new VScrollBar {Dock = DockStyle.Right, Visible = false};
            ScrollBar.ValueChanged += ScrollBarOnValueChanged;
            Controls.Add(ScrollBar);
            _groupConfigLabels = new GroupConfigLabels(this);
            _repairListLabels = new RepairListLabels(this);
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

        public void Update(Sniffer sniffer, string mode, ShipListConfig config)
        {
            _mode = mode;
            CreateShipList(sniffer, config);
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

        private void CreateShipList(Sniffer sniffer, ShipListConfig config)
        {
            var ships = FilterByShipTypes(
                _mode == "修復" ? sniffer.RepairList : _groupConfigLabels.FilterByGroup(sniffer.ShipList, _mode),
                config.ShipCategories).ToArray();
            var order = _mode == "修復" ? ListForm.SortOrder.Repair : config.SortOrder;
            if (!config.ShipType)
            {
                _shipList = ships.OrderBy(s => s, new CompareShip(false, order)).ToArray();
                return;
            }
            _shipList = ships.Select(ship => new {Id = ship.Spec.ShipType, Name = ship.Spec.ShipTypeName})
                .Distinct().Select(type => new ShipStatus
                {
                    Spec = new ShipSpec { Name = type.Name, ShipType = type.Id},
                    Level = 1000,
                }).Concat(ships).OrderBy(ship => ship, new CompareShip(true, order)).ToArray();
        }

        private static readonly int[][] ShipTypeIds =
        {
            new[] // 戦艦
            {
                8, // 巡洋戦艦
                9, // 戦艦
                10 // 航空戦艦
            },
            new[] // 空母
            {
                18, // 装甲空母
                11, // 正規空母
                7 // 軽空母
            },
            new[] // 重巡
            {
                5, // 重巡洋艦
                6 // 航空巡洋艦
            },
            new[] // 軽巡
            {
                3, // 軽巡洋艦
                4, // 重雷装巡洋艦
                21 // 練習巡洋艦
            },
            new[] // 駆逐
            {
                2 // 駆逐艦
            },
            new[] // 海防
            {
                1 // 海防艦
            },
            new[] // 潜水
            {
                13, // 潜水艦
                14 // 潜水空母
            },
            new[] // 補助
            {
                16, // 水上機母艦
                17, // 揚陸艦
                19, // 工作艦
                20, // 潜水母艦
                22 // 補給艦
            }
        };

        private static readonly int[] ShipTypeSortIds = CreateShipTypeSortIds();

        private static int[] CreateShipTypeSortIds()
        {
            var ids = ShipTypeIds.SelectMany(x => x).ToArray();
            var res = new int[ids.Max() + 1];
            for (var i = 0; i < ids.Length; i++)
                res[ids[i]] = i;
            return res;
        }

        private IEnumerable<ShipStatus> FilterByShipTypes(IEnumerable<ShipStatus> ships, ShipCategory shipTypes)
        {
            var ids = Enumerable.Range(0, ShipTypeIds.Length)
                .Where(type => ((int)shipTypes & (1 << type)) != 0)
                .SelectMany(type => ShipTypeIds[type]).ToArray();
            return ships.Where(ship => ids.Contains(ship.Spec.ShipType));
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
                        return ShipTypeSortIds[a.Spec.ShipType] - ShipTypeSortIds[b.Spec.ShipType];
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
                }
                if (a.ExpToNext != b.ExpToNext)
                {
                    if (_order == ListForm.SortOrder.ExpToNextAscend)
                        return a.ExpToNext - b.ExpToNext;
                    if (_order == ListForm.SortOrder.ExpToNextDescend)
                        return b.ExpToNext - a.ExpToNext;
                }
                if (_shipType)
                {
                    if (a.Spec.SortId != b.Spec.SortId)
                        return a.Spec.SortId - b.Spec.SortId;
                    if (a.Level != b.Level)
                        return b.Level - a.Level;
                }
                else
                {
                    if (a.Level != b.Level)
                        return b.Level - a.Level;
                    if (a.Spec.SortId != b.Spec.SortId)
                        return a.Spec.SortId - b.Spec.SortId;
                }
                return a.Id - b.Id;
            }
        }

        private void SetupLabels()
        {
            for (var i = _labelList.Count; i * LineHeight < Height; i++)
            {
                _groupConfigLabels.CreateComponents(i);
                _repairListLabels.CreateLabels(i);
                CreateShipLabels(i);
            }
            SetupScrollBar();
        }

        private void SetupScrollBar()
        {
            var needBar = Scaler.ScaleHeight((float)_shipList.Length * LineHeight) > Height;
            if (!needBar)
            {
                ScrollBar.Visible = false;
                ScrollBar.Value = 0;
                return;
            }
            ScrollBar.Visible = true;
            ScrollBar.Minimum = 0;
            var lines = Max(1, Height / Scaler.ScaleHeight(LineHeight));
            var max = _shipList.Length - lines;
            var largeChange = Min(lines, max);
            ScrollBar.LargeChange = largeChange;
            ScrollBar.Maximum = Max(0, max + largeChange - 1); // ScrollBarを最大まで動かしてもmaxには届かない
            ScrollBar.Value = Min(ScrollBar.Value, max);
        }

        private void CreateShipLabels(int i)
        {
            var y = LineHeight * i + 1;
            const int height = LabelHeight;
            var panel = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(ListForm.PanelWidth, LineHeight),
                BackColor = ShipLabel.ColumnColors[(i + 1) % 2]
            };
            Scaler.Scale(panel);
            var labels = new[]
            {
                new ShipLabel
                {
                    Location = new Point(126, 0),
                    AutoSize = true,
                    AnchorRight = true,
                    MinimumSize = new Size(0, LineHeight),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand
                },
                new ShipLabel
                {
                    Location = new Point(128, 0),
                    Size = new Size(24, LineHeight),
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
            _labelPanelList.Add(panel);
            // ReSharper disable once CoVariantArrayConversion
            panel.Controls.AddRange(labels);
            Controls.Add(panel);
            var unused = panel.Handle; // create handle
            foreach (var label in labels)
            {
                Scaler.Scale(label);
                label.PresetColor =
                    label.BackColor = ShipLabel.ColumnColors[(i + 1) % 2];
            }
            SetHpPercent(labels[0]);
        }

        public void SetHpPercent(ShipLabel label)
        {
            if (_hpPercent)
                label.ToggleHpPercent();
            _hpLabels.Add(label);
            label.DoubleClick += HpLabelClickHandler;
        }

        private void SetShipLabels()
        {
            for (var i = 0; i < (Height + LineHeight - 1) / LineHeight; i++)
            {
                HidePanels(i);
                if (i + ScrollBar.Value >= _shipList.Length)
                    continue;
                if (InShipStatus(_mode))
                    SetShipStatus(i);
                if (_mode == "分類")
                    _groupConfigLabels.SetGrouping(i);
                if (_mode == "修復")
                    _repairListLabels.SetRepairList(i);
            }
        }

        private void HidePanels(int i)
        {
            _labelPanelList[i].Visible = false;
            _repairListLabels.HidePanel(i);
            _groupConfigLabels.HidePanel(i);
        }

        private bool InShipStatus(string mode) => Array.Exists(new[] {"全艦", "A", "B", "C", "D"}, x => mode == x);

        private void SetShipStatus(int i)
        {
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
            _labelPanelList[i].Visible = true;
        }

        public void SetShipType(int i)
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

        public event Action HpLabelClick;

        private void HpLabelClickHandler(object sender, EventArgs ev)
        {
            HpLabelClick?.Invoke();
        }

        public void ToggleHpPercent()
        {
            _hpPercent = !_hpPercent;
            foreach (var label in _hpLabels)
                label.ToggleHpPercent();
        }

        public void ShowShip(int id)
        {
            if (!ScrollBar.Visible)
                return;
            var i = Array.FindIndex(_shipList, s => s.Id == id);
            if (i == -1)
                return;
            ScrollBar.Value = Min(i, ScrollBar.Maximum + 1 - ScrollBar.LargeChange);
            SetShipLabels();
        }
    }
}