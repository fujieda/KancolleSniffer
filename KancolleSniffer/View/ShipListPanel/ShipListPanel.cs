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
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KancolleSniffer.Forms;
using KancolleSniffer.Model;
using KancolleSniffer.View.ListWindow;
using static System.Math;

namespace KancolleSniffer.View.ShipListPanel
{
    public class ShipListPanel : Panel, IPanelResize
    {
        public const int LabelHeight = 12;
        public const int LineHeight = 16;
        private ShipStatus[] _shipList;
        private readonly List<ShipLabel.Hp> _hpLabels = new List<ShipLabel.Hp>();
        private readonly ShipListLabels _shipListLabels;
        private readonly GroupConfigLabels _groupConfigLabels;
        private readonly RepairListLabels _repairListLabels;
        private int _labelCount;
        private string _mode;
        private bool _hpPercent;

        public List<List<int>> GroupSettings
        {
            get => _groupConfigLabels.GroupSettings;
            set => _groupConfigLabels.GroupSettings = value;
        }


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
            _shipListLabels = new ShipListLabels(this);
            _groupConfigLabels = new GroupConfigLabels(this);
            _repairListLabels = new RepairListLabels(this);
        }

        private void ScrollBarOnValueChanged(object sender, EventArgs eventArgs)
        {
            SuspendDrawing();
            SetShipLabels();
            ResumeDrawing();
        }

        public void ApplyResize()
        {
            if (_shipList == null || _shipList.Length == 0 || !Visible)
                return;
            SuspendDrawing();
            SetupLabels();
            ResizeLabels();
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

        public void Update(Sniffer sniffer, string mode, ShipListConfig settings)
        {
            _mode = mode;
            CreateShipList(sniffer, settings);
            SuspendDrawing();
            SetupLabels();
            ResizeLabels();
            SetShipLabels();
            ResumeDrawing();
        }

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, IntPtr lParam);

        private void SuspendDrawing()
        {
            SendMessage(Handle, 11, false, IntPtr.Zero); // WM_SETREDRAW = 11
            SuspendLayout();
        }

        private void ResumeDrawing()
        {
            ResumeLayout();
            SendMessage(Handle, 11, true, IntPtr.Zero);
            Refresh();
        }

        private void CreateShipList(Sniffer sniffer, ShipListConfig settings)
        {
            var ships = FilterByShipTypes(
                _mode == "修復" ? sniffer.RepairList : FilterByGroup(sniffer.ShipList, settings.ShipGroup, _mode),
                settings.ShipCategories).ToArray();
            var order = _mode == "修復" ? ListForm.SortOrder.Repair : settings.SortOrder;
            if (!settings.ShipType)
            {
                _shipList = ships.OrderBy(s => s, new CompareShip(false, order)).ToArray();
                return;
            }
            _shipList = ships.Select(ship => new {Id = ship.Spec.ShipType, Name = ship.Spec.ShipTypeName})
                .Distinct().Select(type => new ShipStatus
                {
                    Spec = new ShipSpec {Name = type.Name, ShipType = type.Id},
                    Level = 1000
                }).Concat(ships).OrderBy(ship => ship, new CompareShip(true, order)).ToArray();
        }

        private static IEnumerable<ShipStatus> FilterByGroup(IEnumerable<ShipStatus> ships,
            IReadOnlyList<List<int>> groups, string groupName)
        {
            var g = Array.FindIndex(new[] {"A", "B", "C", "D"}, x => x == groupName);
            if (g == -1)
                return ships;
            if (groups.Count == 0)
                return new ShipStatus[0];
            return from s in ships where groups[g].Contains(s.Id) select s;
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

        private static IEnumerable<ShipStatus> FilterByShipTypes(IEnumerable<ShipStatus> ships, ShipCategory shipTypes)
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
            for (; _labelCount * Scaler.ScaleHeight(LineHeight) < Height; _labelCount++)
            {
                _groupConfigLabels.CreateComponents(_labelCount);
                _repairListLabels.CreateLabels(_labelCount);
                _shipListLabels.CreateShipLabels(_labelCount);
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

        private void ResizeLabels()
        {
            var width = Width - SystemInformation.VerticalScrollBarWidth - 2;
            for (var i = 0; i < _labelCount; i++)
            {
                _shipListLabels.Resize(i, width);
                _groupConfigLabels.Resize(i, width);
                _repairListLabels.Resize(i, width);
            }
        }

        public void SetHpPercent(ShipLabel.Hp label)
        {
            if (_hpPercent)
                label.ToggleHpPercent();
            _hpLabels.Add(label);
            label.DoubleClick += HpLabelClickHandler;
        }

        private void SetShipLabels()
        {
            for (var i = 0; i * Scaler.ScaleHeight(LineHeight) < Height; i++)
            {
                HidePanels(i);
                if (i + ScrollBar.Value >= _shipList.Length)
                    continue;
                if (InShipStatus(_mode))
                    _shipListLabels.SetShipStatus(i);
                if (_mode == "分類")
                    _groupConfigLabels.SetGrouping(i);
                if (_mode == "修復")
                    _repairListLabels.SetRepairList(i);
            }
        }

        public void SetShipType(int i)
        {
            _shipListLabels.SetShipType(i);
        }

        private void HidePanels(int i)
        {
            _shipListLabels.HidePanel(i);
            _repairListLabels.HidePanel(i);
            _groupConfigLabels.HidePanel(i);
        }

        private bool InShipStatus(string mode) => Array.Exists(new[] {"全艦", "A", "B", "C", "D"}, x => mode == x);

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