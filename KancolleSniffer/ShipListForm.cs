﻿// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
        private const int PanelWidth = 217;
        private ShipStatus[] _shipList;
        private readonly List<ShipLabel[]> _labelList = new List<ShipLabel[]>();
        private readonly List<Panel> _labelPanelList = new List<Panel>();
        private readonly List<CheckBox[]> _checkBoxesList = new List<CheckBox[]>();
        private readonly List<ShipLabel[]> _configLabelList = new List<ShipLabel[]>();
        private readonly List<Panel> _checkBoxPanelList = new List<Panel>();
        private readonly List<ShipLabel[]> _repairLabelList = new List<ShipLabel[]>();
        private readonly List<Panel> _repairPanelList = new List<Panel>();
        private readonly List<ShipLabel[]> _equipLabelList = new List<ShipLabel[]>();
        private readonly List<Panel> _equipPanelList = new List<Panel>();
        public const int GroupCount = 4;
        private readonly HashSet<int>[] _groupSettings = new HashSet<int>[GroupCount];
        private EquipColumn[] _equipList;

        public ShipListForm(Sniffer sniffer, Config config)
        {
            InitializeComponent();
            _sniffer = sniffer;
            _config = config;
        }

        public void UpdateList()
        {
            panelItemHeader.Visible = InItemList();
            itemTreeView.Visible = InItemList();
            if (InItemList())
            {
                HideShipLabels();
                HideEquipLabels();
                itemTreeView.SetNodes(_sniffer.ItemList);
            }
            else if (InEquip())
            {
                HideShipLabels();
                CreateEquip();
                CreateEquipLabels();
                SetEquipLabels();
            }
            else
            {
                HideEquipLabels();
                CreateShipList();
                CreateListLabels();
                SetShipLabels();
            }
        }

        private void CreateShipList()
        {
            var ships = InRepairList() ? _sniffer.DamagedShipList : FilterByGroup(_sniffer.ShipList).ToArray();
            if (!_config.ShipList.ShipType)
            {
                _shipList = ships.OrderBy(s => s, new CompareShip(false, InRepairList())).ToArray();
                return;
            }
            var types = ships.Select(s => new {Id = s.Spec.ShipType, Name = s.Spec.ShipTypeName}).Distinct().
                Select(stype =>
                    new ShipStatus
                    {
                        Spec = new ShipSpec {Name = stype.Name, ShipType = stype.Id},
                        Level = 1000,
                        NowHp = -1000
                    });
            _shipList = ships.Concat(types).OrderBy(s => s, new CompareShip(true, InRepairList())).ToArray();
        }

        private class EquipColumn
        {
            public string Fleet { get; set; }
            public string Ship { get; set; }
            public int Id { get; set; }
            public string Equip { get; set; }
            public Color Color { get; set; }

            public EquipColumn()
            {
                Fleet = Ship = Equip = "";
                Color = DefaultBackColor;
            }
        }

        private void CreateEquip()
        {
            var list = new List<EquipColumn>();
            var fleet = new[] {"第一艦隊", "第二艦隊", "第三艦隊", "第四艦隊"};
            for (var i = 0; i < fleet.Length; i++)
            {
                list.Add(new EquipColumn {Fleet = fleet[i]});
                foreach (var s in _sniffer.GetShipStatuses(i))
                {
                    s.Fleet = -1;
                    list.Add(new EquipColumn {Ship = s.Name, Id = s.Id});
                    list.AddRange(
                        (from e in Enumerable.Range(0, s.Slot.Length)
                            let slot = s.Slot[e]
                            let onslot = s.OnSlot[e]
                            let max = s.Spec.MaxEq[e]
                            where slot != -1
                            let item = _sniffer.Item.ItemDict[slot]
                            select
                                new EquipColumn
                                {
                                    Equip = item.Spec.Name + (item.Level == 0 ? "" : "★" + item.Level) +
                                            (!item.Spec.IsAircraft ? "" : " " + onslot + "/" + max),
                                    Color = item.Spec.Color
                                })
                            .DefaultIfEmpty(new EquipColumn {Equip = "なし"}));
                }
            }
            _equipList = list.ToArray();
        }

        private IEnumerable<ShipStatus> FilterByGroup(IEnumerable<ShipStatus> ships)
        {
            var g = Array.FindIndex(new[] {"A", "B", "C", "D"}, x => x == comboBoxGroup.Text);
            if (g == -1)
                return ships;
            return from s in ships where _groupSettings[g].Contains(s.Id) select s;
        }

        private class CompareShip : IComparer<ShipStatus>
        {
            private readonly bool _type;
            private readonly bool _repair;

            public CompareShip(bool type, bool repair)
            {
                _type = type;
                _repair = repair;
            }

            public int Compare(ShipStatus a, ShipStatus b)
            {
                if (_type && a.Spec.ShipType != b.Spec.ShipType)
                    return a.Spec.ShipType - b.Spec.ShipType;
                if (_repair && a.RepairTime != b.RepairTime)
                    return (int)(b.RepairTime - a.RepairTime).TotalSeconds;
                if (a.Level != b.Level)
                    return b.Level - a.Level;
                if (a.ExpToNext != b.ExpToNext)
                    return a.ExpToNext - b.ExpToNext;
                return a.Spec.Id - b.Spec.Id;
            }
        }

        private void CreateListLabels()
        {
            panelShipList.SuspendLayout();
            for (var i = _labelList.Count; i < _shipList.Length; i++)
            {
                CreateConfigComponents(i);
                CreateRepairLabels(i);
                CreateShipLabels(i);
            }
            panelShipList.ResumeLayout();
        }

        private void CreateEquipLabels()
        {
            panelShipList.SuspendLayout();
            for (var i = _equipLabelList.Count; i < _equipList.Length; i++)
                CreateEquipLabels(i);
            panelShipList.ResumeLayout();
        }

        private void CreateConfigComponents(int i)
        {
            var y = 3 + LineHeight * i;
            var cfgp = new Panel
            {
                Location = new Point(0, y - 2),
                Size = new Size(PanelWidth, LineHeight - 1),
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
            panelShipList.Controls.Add(cfgp);
            foreach (var label in cfgl)
            {
                label.Scale();
                label.PresetColor =
                    label.BackColor = ShipLabels.ColumnColors[(i + 1) % 2];
            }
        }

        private void CreateRepairLabels(int i)
        {
            var y = 3 + LineHeight * i;
            const int height = LabelHeight;
            var rpp = new Panel
            {
                Location = new Point(0, y - 2),
                Size = new Size(PanelWidth, LineHeight - 1),
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
            panelShipList.Controls.Add(rpp);
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
                Size = new Size(PanelWidth, LineHeight - 1),
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
            panelShipList.Controls.Add(lbp);
            foreach (var label in labels)
            {
                label.Scale();
                label.PresetColor =
                    label.BackColor = ShipLabels.ColumnColors[(i + 1) % 2];
            }
        }

        private void CreateEquipLabels(int i)
        {
            var y = 3 + (LineHeight - 2) * i;
            var lbp = new Panel
            {
                Location = new Point(0, y - 2),
                Size = new Size(PanelWidth, LineHeight - 2),
                BackColor = ShipLabels.ColumnColors[(i + 1) % 2],
                Visible = false
            };
            lbp.Scale(ShipLabel.ScaleFactor);
            lbp.Tag = lbp.Location.Y;
            var labels = new[]
            {
                new ShipLabel {Location = new Point(1, 2), AutoSize = true},
                new ShipLabel {Location = new Point(10, 2), AutoSize = true},
                new ShipLabel {Location = new Point(40, 2), AutoSize = true},
                new ShipLabel {Location = new Point(37, 2), Size = new Size(4, LabelHeight - 2)}
            };
            _equipLabelList.Add(labels);
            _equipPanelList.Add(lbp);
            // ReSharper disable once CoVariantArrayConversion
            lbp.Controls.AddRange(labels);
            panelShipList.Controls.Add(lbp);
            foreach (var label in labels)
            {
                label.Scale();
                label.PresetColor =
                    label.BackColor = ShipLabels.ColumnColors[(i + 1) % 2];
            }
        }

        private void SetShipLabels()
        {
            panelGroupHeader.Visible = InGroupConfig();
            panelRepairHeader.Visible = InRepairList();
            panelShipList.SuspendLayout();
            for (var i = 0; i < _shipList.Length; i++)
            {
                if (!InShipStatus())
                    _labelPanelList[i].Visible = false;
                if (!InGroupConfig())
                    _checkBoxPanelList[i].Visible = false;
                if (!InRepairList())
                    _repairPanelList[i].Visible = false;
            }
            for (var i = 0; i < _shipList.Length; i++)
            {
                if (InShipStatus())
                    SetShipStatus(i);
                if (InGroupConfig())
                    SetGroupConfig(i);
                if (InRepairList())
                    SetRepairList(i);
            }
            for (var i = _shipList.Length; i < _labelPanelList.Count; i++)
            {
                _labelPanelList[i].Visible = _checkBoxPanelList[i].Visible = _repairPanelList[i].Visible;
            }
            panelShipList.ResumeLayout();
        }

        private void SetShipStatus(int i)
        {
            var lbp = _labelPanelList[i];
            if (!lbp.Visible)
                lbp.Location = new Point(lbp.Left, (int)lbp.Tag + panelShipList.AutoScrollPosition.Y);
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
            labels[4].SetName(s);
            labels[5].SetFleet(s);
            lbp.Visible = true;
        }

        private void SetShipType(int i)
        {
            var lbp = _labelPanelList[i];
            if (!lbp.Visible)
                lbp.Location = new Point(lbp.Left, (int)lbp.Tag + panelShipList.AutoScrollPosition.Y);
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

        private void SetEquipLabels()
        {
            panelItemHeader.Visible = true;
            panelShipList.SuspendLayout();
            for (var i = 0; i < _equipList.Length; i++)
                SetEquip(i);
            for (var i = _equipList.Length; i < _equipLabelList.Count; i++)
                _equipPanelList[i].Visible = false;
            panelShipList.ResumeLayout();
        }

        private void SetEquip(int i)
        {
            var lbp = _equipPanelList[i];
            if (!lbp.Visible)
                lbp.Location = new Point(lbp.Left, (int)lbp.Tag + panelShipList.AutoScrollPosition.Y);
            var e = _equipList[i];
            var labels = _equipLabelList[i];
            labels[0].Text = e.Fleet;
            labels[1].SetName(e.Ship);
            labels[2].Text = e.Equip;
            labels[3].Visible = e.Equip != "";
            labels[3].BackColor = e.Color;
            lbp.Visible = true;
        }

        private void SetGroupConfig(int i)
        {
            var cbp = _checkBoxPanelList[i];
            var s = _shipList[i];
            if (s.Level == 1000)
            {
                SetShipType(i);
                return;
            }
            if (!cbp.Visible)
                cbp.Location = new Point(cbp.Left, (int)cbp.Tag + panelShipList.AutoScrollPosition.Y);
            var cfgl = _configLabelList[i];
            cfgl[0].SetLevel(s);
            cfgl[1].SetName(s);
            cfgl[2].SetFleet(s);
            var cb = _checkBoxesList[i];
            for (var j = 0; j < cb.Length; j++)
                cb[j].Checked = _groupSettings[j].Contains(s.Id);
            cbp.Visible = true;
        }

        private void SetRepairList(int i)
        {
            var rpp = _repairPanelList[i];
            var s = _shipList[i];
            if (s.Level == 1000)
            {
                SetShipType(i);
                return;
            }
            if (!rpp.Visible)
                rpp.Location = new Point(rpp.Left, (int)rpp.Tag + panelShipList.AutoScrollPosition.Y);
            var rpl = _repairLabelList[i];
            rpl[0].SetHp(s);
            rpl[1].SetLevel(s);
            rpl[2].SetRepairTime(s);
            rpl[3].Text = TimeSpan.FromSeconds(s.RepairSecPerHp).ToString(@"mm\:ss");
            rpl[4].SetName(s);
            rpl[5].SetFleet(s);
            rpp.Visible = true;
        }

        private void HideShipLabels()
        {
            panelShipList.SuspendLayout();
            for (var i = 0; i < _labelPanelList.Count; i++)
                _labelPanelList[i].Visible = _checkBoxPanelList[i].Visible = _repairPanelList[i].Visible = false;
            panelShipList.ResumeLayout();
        }

        private void HideEquipLabels()
        {
            panelShipList.SuspendLayout();
            foreach (var e in _equipPanelList)
                e.Visible = false;
            panelShipList.ResumeLayout();
        }

        private bool InShipStatus()
        {
            return Array.Exists(new[] {"全員", "A", "B", "C", "D"}, x => comboBoxGroup.Text == x);
        }

        private bool InGroupConfig()
        {
            return comboBoxGroup.Text == "分類";
        }

        private bool InRepairList()
        {
            return comboBoxGroup.Text == "修復";
        }

        private bool InItemList()
        {
            return comboBoxGroup.Text == "装備";
        }

        private bool InEquip()
        {
            return comboBoxGroup.Text == "艦隊";
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
            var y = 0;
            if (InShipStatus())
            {
                var i = Array.FindIndex(_shipList, s => s.Id == id);
                if (i == -1)
                    return;
                y = (int)Math.Round(ShipLabel.ScaleFactor.Height * LineHeight * i);
            }
            else if (InEquip())
            {
                var i = Array.FindIndex(_equipList, e => e.Id == id);
                if (i == -1)
                    return;
                y = (int)Math.Round(ShipLabel.ScaleFactor.Height * (LineHeight - 2) * i);
            }
            panelShipList.AutoScrollPosition = new Point(0, y);
        }

        private void checkBoxShipType_CheckedChanged(object sender, EventArgs e)
        {
            _config.ShipList.ShipType = checkBoxShipType.Checked;
            UpdateList();
            SetActiveControl();
        }

        private void checkboxGroup_CheckedChanged(object sender, EventArgs e)
        {
            var cb = (CheckBox)sender;
            var group = (int)cb.Tag % 10;
            var idx = (int)cb.Tag / 10;
            if (cb.Checked)
                _groupSettings[group].Add(_shipList[idx].Id);
            else
                _groupSettings[group].Remove(_shipList[idx].Id);
        }

        private void comboBoxGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void comboBoxGroup_DropDownClosed(object sender, EventArgs e)
        {
            SetActiveControl();
        }

        private void ShipListForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            var g = Array.FindIndex(new[] {'Z', 'A', 'B', 'C', 'D', 'G', 'R', 'W', 'X'},
                x => x == char.ToUpper(e.KeyChar));
            if (g == -1)
                return;
            comboBoxGroup.SelectedIndex = g;
            SetActiveControl();
            e.Handled = true;
        }

        // マウスホイールでスクロールするためにコントロールにフォーカスを合わせる。
        private void SetActiveControl()
        {
            ActiveControl = InItemList() ? (Control)itemTreeView : panelShipList;
        }
    }
}