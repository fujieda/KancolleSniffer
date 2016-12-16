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
using System.Windows.Forms;

namespace KancolleSniffer
{
    public class ShipListPanel : Panel
    {
        private const int LabelHeight = 12;
        private const int LineHeight = 16;
        private ShipStatus[] _shipList;
        private readonly List<ShipLabel[]> _labelList = new List<ShipLabel[]>();
        private readonly List<Panel> _labelPanelList = new List<Panel>();
        private readonly List<CheckBox[]> _checkBoxesList = new List<CheckBox[]>();
        private readonly List<ShipLabel[]> _configLabelList = new List<ShipLabel[]>();
        private readonly List<Panel> _checkBoxPanelList = new List<Panel>();
        private readonly List<ShipLabel[]> _repairLabelList = new List<ShipLabel[]>();
        private readonly List<Panel> _repairPanelList = new List<Panel>();

        public const int GroupCount = 4;
        public HashSet<int>[] GroupSettings { get; } = new HashSet<int>[GroupCount];

        public void Update(Sniffer sniffer, string mode, ListForm.SortOrder sortOrder, bool byShipType)
        {
            CreateShipList(sniffer, mode, sortOrder, byShipType);
            CreateListLabels();
            SetShipLabels(mode);
        }

        void CreateShipList(Sniffer sniffer, string mode, ListForm.SortOrder sortOrder, bool byShipType)
        {
            var ships = mode == "修復" ? sniffer.RepairList : FilterByGroup(sniffer.ShipList, mode).ToArray();
            var order = mode == "修復" ? ListForm.SortOrder.Repair : sortOrder;
            if (!byShipType)
            {
                _shipList = ships.OrderBy(s => s, new CompareShip(false, order)).ToArray();
                return;
            }
            var types = ships.Select(s => new {Id = s.Spec.ShipType, Name = s.Spec.ShipTypeName}).Distinct().
                Select(stype =>
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
            private readonly bool _type;
            private readonly ListForm.SortOrder _order;

            public CompareShip(bool type, ListForm.SortOrder order)
            {
                _type = type;
                _order = order;
            }

            public int Compare(ShipStatus a, ShipStatus b)
            {
                if (_type && a.Spec.ShipType != b.Spec.ShipType)
                    return a.Spec.ShipType - b.Spec.ShipType;
                switch (_order)
                {
                    case ListForm.SortOrder.None:
                    case ListForm.SortOrder.ExpToNext:
                        break;
                    case ListForm.SortOrder.Cond:
                        if (a.Cond != b.Cond)
                            return a.Cond - b.Cond;
                        break;
                    case ListForm.SortOrder.Repair:
                        if (a.RepairTime != b.RepairTime)
                            return (int)(b.RepairTime - a.RepairTime).TotalSeconds;
                        break;
                }
                if ((!_type || _order == ListForm.SortOrder.ExpToNext) && a.Level != b.Level)
                    return b.Level - a.Level;
                if (_order == ListForm.SortOrder.ExpToNext && a.ExpToNext != b.ExpToNext)
                    return a.ExpToNext - b.ExpToNext;
                if (a.Spec.SortNo != b.Spec.SortNo)
                    return a.Spec.SortNo - b.Spec.SortNo;
                return a.Id - b.Id;
            }
        }

        private void CreateListLabels()
        {
            SuspendLayout();
            for (var i = _labelList.Count; i < _shipList.Length; i++)
            {
                CreateConfigComponents(i);
                CreateRepairLabels(i);
                CreateShipLabels(i);
            }
            ResumeLayout();
        }

        private void CreateConfigComponents(int i)
        {
            var y = 3 + LineHeight * i;
            var cfgp = new Panel
            {
                Location = new Point(0, y - 2),
                Size = new Size(ListForm.PanelWidth, LineHeight - 1),
                BackColor = ShipLabels.ColumnColors[(i + 1) % 2],
                Visible = false
            };
            cfgp.Scale(ShipLabel.ScaleFactor);
            cfgp.Tag = cfgp.Location.Y;
            var cfgl = new[]
            {
                new ShipLabel
                {
                    Location = new Point(91, 2),
                    Size = new Size(23, LabelHeight),
                    TextAlign = ContentAlignment.MiddleRight,
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
            _configLabelList.Add(cfgl);
            _checkBoxesList.Add(cb);
            _checkBoxPanelList.Add(cfgp);
            // ReSharper disable CoVariantArrayConversion
            cfgp.Controls.AddRange(cfgl);
            cfgp.Controls.AddRange(cb);
            // ReSharper restore CoVariantArrayConversion
            Controls.Add(cfgp);
            foreach (var label in cfgl)
            {
                label.Scale();
                label.PresetColor =
                    label.BackColor = ShipLabels.ColumnColors[(i + 1) % 2];
            }
        }

        private void checkboxGroup_CheckedChanged(object sender, EventArgs e)
        {
            var cb = (CheckBox)sender;
            var group = (int)cb.Tag % 10;
            var idx = (int)cb.Tag / 10;
            if (cb.Checked)
            {
                GroupSettings[group].Add(_shipList[idx].Id);
            }
            else
            {
                GroupSettings[group].Remove(_shipList[idx].Id);
            }
        }

        private void CreateRepairLabels(int i)
        {
            var y = 3 + LineHeight * i;
            const int height = LabelHeight;
            var rpp = new Panel
            {
                Location = new Point(0, y - 2),
                Size = new Size(ListForm.PanelWidth, LineHeight - 1),
                BackColor = ShipLabels.ColumnColors[(i + 1) % 2],
                Visible = false
            };
            rpp.Scale(ShipLabel.ScaleFactor);
            rpp.Tag = rpp.Location.Y;
            var rpl = new[]
            {
                new ShipLabel {Location = new Point(118, 2), AutoSize = true, AnchorRight = true},
                new ShipLabel
                {
                    Location = new Point(117, 2),
                    Size = new Size(23, height),
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
                    label.BackColor = ShipLabels.ColumnColors[(i + 1) % 2];
            }
        }

        private void CreateShipLabels(int i)
        {
            var y = 3 + LineHeight * i;
            const int height = LabelHeight;
            var lbp = new Panel
            {
                Location = new Point(0, y - 2),
                Size = new Size(ListForm.PanelWidth, LineHeight - 1),
                BackColor = ShipLabels.ColumnColors[(i + 1) % 2],
                Visible = false
            };
            lbp.Scale(ShipLabel.ScaleFactor);
            lbp.Tag = lbp.Location.Y;
            var labels = new[]
            {
                new ShipLabel {Location = new Point(126, 2), AutoSize = true, AnchorRight = true},
                new ShipLabel
                {
                    Location = new Point(129, 2),
                    Size = new Size(23, height),
                    TextAlign = ContentAlignment.MiddleRight
                },
                new ShipLabel
                {
                    Location = new Point(155, 2),
                    Size = new Size(23, height),
                    TextAlign = ContentAlignment.MiddleRight
                },
                new ShipLabel
                {
                    Location = new Point(176, 2),
                    Size = new Size(41, height),
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
                    label.BackColor = ShipLabels.ColumnColors[(i + 1) % 2];
            }
        }

        private void SetShipLabels(string mode)
        {
            SuspendLayout();
            for (var i = 0; i < _shipList.Length; i++)
            {
                if (!InShipStatus(mode))
                    _labelPanelList[i].Visible = false;
                if (mode != "分類")
                    _checkBoxPanelList[i].Visible = false;
                if (mode != "修復")
                    _repairPanelList[i].Visible = false;
            }
            for (var i = 0; i < _shipList.Length; i++)
            {
                if (InShipStatus(mode))
                    SetShipStatus(i);
                if (mode == "分類")
                    SetGroupConfig(i);
                if (mode == "修復")
                    SetRepairList(i);
            }
            for (var i = _shipList.Length; i < _labelPanelList.Count; i++)
            {
                _labelPanelList[i].Visible = _checkBoxPanelList[i].Visible = _repairPanelList[i].Visible = false;
            }
            ResumeLayout();
        }

        private bool InShipStatus(string mode) => Array.Exists(new[] {"全員", "A", "B", "C", "D"}, x => mode == x);

        private void SetShipStatus(int i)
        {
            var lbp = _labelPanelList[i];
            if (!lbp.Visible)
                lbp.Location = new Point(lbp.Left, (int)lbp.Tag + AutoScrollPosition.Y);
            var s = _shipList[i];
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
            lbp.Visible = true;
        }

        private void SetShipType(int i)
        {
            var lbp = _labelPanelList[i];
            if (!lbp.Visible)
                lbp.Location = new Point(lbp.Left, (int)lbp.Tag + AutoScrollPosition.Y);
            var s = _shipList[i];
            var labels = _labelList[i];
            for (var c = 0; c < 4; c++)
            {
                labels[c].Text = "";
                labels[c].BackColor = labels[c].PresetColor;
            }
            labels[4].SetName("");
            labels[5].Text = s.Name;
            lbp.Visible = true;
        }

        private void SetGroupConfig(int i)
        {
            var cbp = _checkBoxPanelList[i];
            var s = _shipList[i];
            if (s.Level == 1000)
            {
                SetShipType(i);
                cbp.Visible = false;
                return;
            }
            if (!cbp.Visible)
                cbp.Location = new Point(cbp.Left, (int)cbp.Tag + AutoScrollPosition.Y);
            var cfgl = _configLabelList[i];
            cfgl[0].SetLevel(s);
            cfgl[1].SetName(s, ShipNameWidth.GroupConfig);
            cfgl[2].SetFleet(s);
            var cb = _checkBoxesList[i];
            for (var j = 0; j < cb.Length; j++)
                cb[j].Checked = GroupSettings[j].Contains(s.Id);
            cbp.Visible = true;
        }

        private void SetRepairList(int i)
        {
            var rpp = _repairPanelList[i];
            var s = _shipList[i];
            if (s.Level == 1000)
            {
                SetShipType(i);
                rpp.Visible = false;
                return;
            }
            if (!rpp.Visible)
                rpp.Location = new Point(rpp.Left, (int)rpp.Tag + AutoScrollPosition.Y);
            var rpl = _repairLabelList[i];
            rpl[0].SetHp(s);
            rpl[1].SetLevel(s);
            rpl[2].SetRepairTime(s);
            rpl[3].Text = TimeSpan.FromSeconds(s.RepairSecPerHp).ToString(@"mm\:ss");
            rpl[4].SetName(s, ShipNameWidth.RepairListFull);
            rpl[5].SetFleet(s);
            rpp.Visible = true;
        }

        public void ShowShip(int id)
        {
            var i = Array.FindIndex(_shipList, s => s.Id == id);
            if (i == -1)
                return;
            var y = (int)Math.Round(ShipLabel.ScaleFactor.Height * LineHeight * i);
            AutoScrollPosition = new Point(0, y);
        }
    }
}