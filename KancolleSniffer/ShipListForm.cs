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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public partial class ShipListForm : Form
    {
        private readonly Sniffer _sniffer;
        private readonly Config _config;
        private const int LabelHeight = 12;
        private const int LineHeight = 16;
        private const int HpLabelRight = 126;
        private const int PanelWidth = 232;
        private ShipStatus[] _currentList;
        private readonly List<ShipLabel[]> _labelList = new List<ShipLabel[]>();
        private readonly List<Panel> _labelPanelList = new List<Panel>();
        private readonly List<CheckBox[]> _checkBoxesList = new List<CheckBox[]>();
        private readonly List<Panel> _checkBoxPanelList = new List<Panel>();
        public const int GroupCount = 4;
        private readonly HashSet<int>[] _groupSettings = new HashSet<int>[GroupCount];

        public ShipListForm(Sniffer sniffer, Config config)
        {
            InitializeComponent();
            _sniffer = sniffer;
            _config = config;
        }

        public void UpdateList()
        {
            CreateList();
            CreateListLabels();
            SetShipLabels();
        }

        private void CreateList()
        {
            var ships = FilterByGroup(_sniffer.ShipList).ToArray();
            if (!_config.ShipList.ShipType)
            {
                _currentList = ships.OrderBy(s => s, new CompareShipByExp()).ToArray();
                return;
            }
            var types = from id in (from s in ships select s.Spec.ShipType).Distinct()
                join stype in _sniffer.ShipTypeList on id equals stype.Id
                select new ShipStatus {Spec = new ShipSpec {Name = stype.Name, ShipType = stype.Id}, Level = 1000};
            _currentList = ships.Concat(types).OrderBy(s => s, new CompareShipByType()).ToArray();
        }

        private IEnumerable<ShipStatus> FilterByGroup(IEnumerable<ShipStatus> ships)
        {
            var g = Array.FindIndex(new[] {"A", "B", "C", "D"}, x => x == comboBoxGroup.Text);
            if (g == -1)
                return ships;
            return from s in ships where _groupSettings[g].Contains(s.Id) select s;
        }

        private void CreateListLabels()
        {
            panelShipList.SuspendLayout();
            for (var i = _labelList.Count; i < _currentList.Length; i++)
            {
                CreateCheckBoxes(i);
                CreateShipLabels(i);
            }
            panelShipList.ResumeLayout();
        }

        private void CreateCheckBoxes(int i)
        {
            var y = 3 + LineHeight * i;
            var cb = new CheckBox[GroupCount];
            var cbp = new Panel
            {
                Location = new Point(79, y),
                Size = new Size(153, LabelHeight),
                BackColor = ShipInfoLabels.ColumnColors[(i + 1) % 2],
                Visible = false
            };
            cbp.Scale(ShipLabel.ScaleFactor);
            cbp.Tag = cbp.Location.Y;
            for (var j = 0; j < cb.Length; j++)
            {
                cb[j] = new CheckBox
                {
                    Location = new Point(31 + j * 30, 0),
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(15, 14),
                    Tag = i * 10 + j
                };
                cb[j].Scale(ShipLabel.ScaleFactor);
                cb[j].CheckedChanged += checkboxGroup_CheckedChanged;
            }
            _checkBoxesList.Add(cb);
            _checkBoxPanelList.Add(cbp);
// ReSharper disable once CoVariantArrayConversion
            cbp.Controls.AddRange(cb);
            panelShipList.Controls.Add(cbp);
        }

        private void CreateShipLabels(int i)
        {
            var y = 3 + LineHeight * i;
            const int height = LabelHeight;
            const int lh = LineHeight;
            var lbp = new Panel
            {
                Location = new Point(0, y - 1),
                Size = new Size(PanelWidth, lh),
                BackColor = ShipInfoLabels.ColumnColors[(i + 1) % 2],
                Visible = false
            };
            lbp.Scale(ShipLabel.ScaleFactor);
            lbp.Tag = lbp.Location.Y;
            var labels = new[]
            {
                new ShipLabel {Location = new Point(HpLabelRight, 1), AutoSize = true},
                new ShipLabel
                {
                    Location = new Point(132, 1),
                    Size = new Size(23, height),
                    TextAlign = ContentAlignment.MiddleRight
                },
                new ShipLabel
                {
                    Location = new Point(166, 1),
                    Size = new Size(23, height),
                    TextAlign = ContentAlignment.MiddleRight
                },
                new ShipLabel
                {
                    Location = new Point(191, 1),
                    Size = new Size(41, height),
                    TextAlign = ContentAlignment.MiddleRight
                },
                new ShipLabel {Location = new Point(10, 1), AutoSize = true},
                new ShipLabel {Location = new Point(1, 1), AutoSize = true},
                new ShipLabel {Location = new Point(0, 0), Size = new Size(PanelWidth, lh - 1)}
            };
            foreach (var label in labels)
            {
                label.Scale(ShipLabel.ScaleFactor);
                label.PresetColor =
                    label.BackColor = ShipInfoLabels.ColumnColors[(i + 1) % 2];
            }
            _labelList.Add(labels);
            _labelPanelList.Add(lbp);
// ReSharper disable once CoVariantArrayConversion
            lbp.Controls.AddRange(labels);
            panelShipList.Controls.Add(lbp);
            labels[0].SizeChanged += labelHP_SizeChanged;
        }

        private class CompareShipByExp : IComparer<ShipStatus>
        {
            public int Compare(ShipStatus a, ShipStatus b)
            {
                if (a.Level != b.Level)
                    return b.Level - a.Level;
                if (a.ExpToNext != b.ExpToNext)
                    return a.ExpToNext - b.ExpToNext;
                return a.Spec.Id - b.Spec.Id;
            }
        }

        private class CompareShipByType : IComparer<ShipStatus>
        {
            public int Compare(ShipStatus a, ShipStatus b)
            {
                if (a.Spec.ShipType != b.Spec.ShipType)
                    return a.Spec.ShipType - b.Spec.ShipType;
                if (a.Level != b.Level)
                    return b.Level - a.Level;
                if (a.ExpToNext != b.ExpToNext)
                    return a.ExpToNext - b.ExpToNext;
                return a.Spec.Id - b.Spec.Id;
            }
        }

        private void SetShipLabels()
        {
            panelGroupHeader.Visible = InGroupConfig();
            panelShipList.SuspendLayout();
            var fn = new[] {"", "1", "2", "3", "4"};
            for (var i = 0; i < _currentList.Length; i++)
            {
                var lbp = _labelPanelList[i];
                if (lbp.Visible == false)
                {
                    lbp.Location = new Point(lbp.Left, (int)lbp.Tag + panelShipList.AutoScrollPosition.Y);
                    lbp.Visible = true;
                }
                var cbp = _checkBoxPanelList[i];
                if (cbp.Visible == false && InGroupConfig())
                {
                    cbp.Location = new Point(cbp.Left, (int)cbp.Tag + panelShipList.AutoScrollPosition.Y);
                }
                var s = _currentList[i];
                var labels = _labelList[i];
                if (s.Level == 1000)
                {
                    cbp.Visible = false;
                    for (var c = 0; c < 6; c++)
                    {
                        labels[c].Text = "";
                        labels[c].BackColor = labels[c].PresetColor;
                    }
                    labels[4].SetName("");
                    labels[5].Text = s.Name;
                    continue;
                }
                cbp.Visible = InGroupConfig();
                if (InGroupConfig())
                {
                    var cb = _checkBoxesList[i];
                    for (var j = 0; j < cb.Length; j++)
                        cb[j].Checked = _groupSettings[j].Contains(s.Id);
                }
                else
                {
                    labels[0].SetHp(s);
                    labels[1].SetCond(s);
                    labels[2].SetLevel(s);
                    labels[3].SetExpToNext(s);
                }
                labels[4].SetName(s);
                labels[5].Text = fn[s.Fleet + 1];
            }
            for (var i = _currentList.Length; i < _labelPanelList.Count; i++)
            {
                _labelPanelList[i].Visible = false;
                _checkBoxPanelList[i].Visible = false;
            }
            panelShipList.ResumeLayout();
        }

        private bool InGroupConfig()
        {
            return comboBoxGroup.Text == "設定";
        }

        private void labelHP_SizeChanged(object sender, EventArgs e)
        {
            var label = (Label)sender;
            label.Location = new Point(
                (int)Math.Round(HpLabelRight * ShipLabel.ScaleFactor.Width - label.Width), label.Top);
        }

        private void ShipListForm_Load(object sender, EventArgs e)
        {
            panelShipList.Width = (int)Math.Round(PanelWidth * ShipLabel.ScaleFactor.Width) + 3 +
                                  SystemInformation.VerticalScrollBarWidth;
            Width = panelShipList.Width + 12 + (Width - ClientSize.Width);
            MinimumSize = new Size(Width, 0);
            MaximumSize = new Size(Width, int.MaxValue);
            var config = _config.ShipList;
            checkBoxShipType.Checked = config.ShipType;
            ActiveControl = panelShipList;
            for (var i = 0; i < GroupCount; i++)
                _groupSettings[i] = new HashSet<int>(config.ShipGroup[i]);
            comboBoxGroup.SelectedIndex = 0;
            if (config.Location.X == int.MinValue)
                return;
            var bounds = new Rectangle(config.Location, config.Size);
            if (MainForm.IsVisibleOnAnyScreen(bounds))
                Location = bounds.Location;
            Height = bounds.Height;
        }

        private void ShipListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var config = _config.ShipList;
            var all = _sniffer.ShipList.Select(s => s.Id).ToArray();
            for (var i = 0; i < GroupCount; i++)
            {
                if (_groupSettings[i] == null)
                    break;
                if (all.Count() > 0)
                    _groupSettings[i].IntersectWith(all);
                config.ShipGroup[i] = _groupSettings[i].ToList();                
            }
            e.Cancel = true;
            if (!Visible)
                return;
            var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            config.Location = bounds.Location;
            config.Size = bounds.Size;
            Hide();
        }

        public void ShowShip(int id)
        {
            var i = Array.FindIndex(_currentList, s => s.Id == id);
            if (i == -1)
                return;
            var y = (int)Math.Round(ShipLabel.ScaleFactor.Height * 16 * i);
            panelShipList.AutoScrollPosition = new Point(0, y);
        }

        private void checkBoxShipType_CheckedChanged(object sender, EventArgs e)
        {
            _config.ShipList.ShipType = checkBoxShipType.Checked;
            UpdateList();
            ActiveControl = panelShipList;
        }

        private void checkboxGroup_CheckedChanged(object sender, EventArgs e)
        {
            var cb = (CheckBox)sender;
            var group = (int)cb.Tag % 10;
            var idx = (int)cb.Tag / 10;
            if (cb.Checked)
                _groupSettings[group].Add(_currentList[idx].Id);
            else
                _groupSettings[group].Remove(_currentList[idx].Id);
        }

        private void comboBoxGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void comboBoxGroup_DropDownClosed(object sender, EventArgs e)
        {
            ActiveControl = panelShipList;
        }
    }
}