// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ContentAlignment = System.Drawing.ContentAlignment;

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
        private TreeNode _itemTreeNode;
        private ItemStatus[] _prevItemList;

        public ShipListForm(Sniffer sniffer, Config config)
        {
            InitializeComponent();
            _sniffer = sniffer;
            _config = config;
        }

        public void UpdateList()
        {
            panelItemHeader.Visible = InItemList();
            treeViewItem.Visible = InItemList();
            if (InItemList())
            {
                HideShipLabels();
                if (CreateItemNodes())
                    SetTreeViewItem();
            }
            else
            {
                if (InEquip())
                    CreateEquip();
                else
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

        private void CreateEquip()
        {
            var list = new List<ShipStatus>();
            var fleet = new[] {"第一艦隊", "第二艦隊", "第三艦隊", "第四艦隊"};
            for (var i = 0; i < fleet.Length; i++)
            {
                list.Add(new ShipStatus {Spec = new ShipSpec {Name = fleet[i]}, Level = 1000});
                foreach (var s in _sniffer.GetShipStatuses(i))
                {
                    s.Fleet = -1;
                    list.Add(s);
                    list.AddRange(
                        s.Slot.Where(id => id != -1).Select(
                            id => CreateDummyEntry(_sniffer.Item.ItemDict[id].Spec.Name, 500))
                            .DefaultIfEmpty(CreateDummyEntry("なし", 500)));
                }
            }
            _shipList = list.ToArray();
        }

        private ShipStatus CreateDummyEntry(string name, int level)
        {
            return new ShipStatus {Spec = new ShipSpec {Name = name}, Level = level};
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

        private bool CreateItemNodes()
        {
            var itemList = _sniffer.ItemList;
            if (_prevItemList != null && _prevItemList.SequenceEqual(itemList, new ItemStatusComparer()))
                return false;
            _prevItemList = itemList.Select(CloneItemStatus).ToArray();
            var grouped = from byId in
                (from item in itemList where item.Spec.Id != -1
                    orderby item.Spec.Type, item.Spec.Id, item.Level descending, item.Ship.Spec.Id
                    group item by new {item.Spec.Id, item.Level})
                group byId by byId.First().Spec.Type;
            _itemTreeNode = new TreeNode();
            foreach (var byType in grouped)
            {
                var typeName = byType.First().First().Spec.TypeName;
                var typeNode = new TreeNode();
                typeNode.Name = typeNode.Text = typeName;
                _itemTreeNode.Nodes.Add(typeNode);
                foreach (var byItem in byType)
                {
                    var item = byItem.First();
                    var itemNode = new TreeNode();
                    itemNode.Name = itemNode.Text = item.Spec.Name + (item.Level == 0 ? "" : "★" + item.Level);
                    typeNode.Nodes.Add(itemNode);

                    var shipGroup = (from i in byItem group i.Ship by i.Ship.Id).ToArray();
                    foreach (var name in
                        from grp in shipGroup where grp.Key != -1
                        let ship = grp.First()
                        select (ship.Fleet != -1 ? ship.Fleet + 1 + " " : "") +
                               ship.Name + "Lv" + ship.Level + "×" + grp.Count())
                    {
                        itemNode.Nodes.Add(name, name);
                    }
                    foreach (var name in (from grp in shipGroup where grp.Key == -1 select "未装備×" + grp.Count()))
                    {
                        itemNode.Nodes.Add(name, name);
                    }
                }
            }
            return true;
        }

        private ItemStatus CloneItemStatus(ItemStatus org)
        {
            return new ItemStatus
            {
                Level = org.Level,
                Spec = org.Spec,
                Ship = new ShipStatus {Id = org.Ship.Id, Fleet = org.Ship.Fleet}
            };
        }

        private class ItemStatusComparer : IEqualityComparer<ItemStatus>
        {
            public bool Equals(ItemStatus x, ItemStatus y)
            {
                return x.Level == y.Level && x.Spec == y.Spec && x.Ship.Id == y.Ship.Id && x.Ship.Fleet == y.Ship.Fleet;
            }

            public int GetHashCode(ItemStatus obj)
            {
                return obj.Level + obj.Spec.GetHashCode() + obj.Ship.GetHashCode();
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
                CreateEquipLabels(i);
            }
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
                new ShipLabel {Location = new Point(40, 2), AutoSize = true}
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
            panelItemHeader.Visible = InEquip();
            panelShipList.SuspendLayout();
            for (var i = 0; i < _shipList.Length; i++)
            {
                if (!InShipStatus())
                    _labelPanelList[i].Visible = false;
                if (!InGroupConfig())
                    _checkBoxPanelList[i].Visible = false;
                if (!InRepairList())
                    _repairPanelList[i].Visible = false;
                if (!InEquip())
                    _equipPanelList[i].Visible = false;
            }
            for (var i = 0; i < _shipList.Length; i++)
            {
                if (InShipStatus())
                    SetShipStatus(i);
                if (InGroupConfig())
                    SetGroupConfig(i);
                if (InRepairList())
                    SetRepairList(i);
                if (InEquip())
                    SetEquip(i);
            }
            for (var i = _shipList.Length; i < _labelPanelList.Count; i++)
            {
                _labelPanelList[i].Visible = _checkBoxPanelList[i].Visible =
                    _repairPanelList[i].Visible = _equipPanelList[i].Visible = false;
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

        private void SetEquip(int i)
        {
            var lbp = _equipPanelList[i];
            if (!lbp.Visible)
                lbp.Location = new Point(lbp.Left, (int)lbp.Tag + panelShipList.AutoScrollPosition.Y);
            var s = _shipList[i];
            var labels = _equipLabelList[i];
            switch (s.Level)
            {
                case 1000:
                    labels[0].Text = s.Name;
                    labels[1].SetName("");
                    labels[2].Text = "";
                    break;
                case 500:
                    labels[2].Text = s.Name;
                    labels[0].Text = "";
                    labels[1].SetName("");
                    break;
                default:
                    labels[1].SetName(s);
                    labels[0].Text = labels[2].Text = "";
                    break;
            }
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
            for (var i = 0; i < _shipList.Length; i++)
                _labelPanelList[i].Visible = _checkBoxPanelList[i].Visible =
                    _repairPanelList[i].Visible = _equipPanelList[i].Visible = false;
            panelShipList.ResumeLayout();
        }

        [DllImport("user32.dll")]
        private static extern int GetScrollPos(IntPtr hWnd, int nBar);

        [DllImport("user32.dll")]
        private static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        private void SetTreeViewItem()
        {
            var y = GetScrollPos(treeViewItem.Handle, 1);
            treeViewItem.BeginUpdate();
            var save = SaveTreeViewState(treeViewItem.Nodes);
            treeViewItem.Nodes.Clear();
            foreach (TreeNode child in _itemTreeNode.Nodes)
                treeViewItem.Nodes.Add(child);
            RestoreTreeViewState(treeViewItem.Nodes, save.Nodes);
            treeViewItem.EndUpdate();
            SetScrollPos(treeViewItem.Handle, 1, y, true);
        }

        private TreeNode SaveTreeViewState(TreeNodeCollection nodes)
        {
            var result = new TreeNode();
            foreach (TreeNode child in nodes)
            {
                var copy = SaveTreeViewState(child.Nodes);
                if (child.IsExpanded)
                    copy.Expand();
                copy.Name = child.Name;
                result.Nodes.Add(copy);
            }
            return result;
        }

        private void RestoreTreeViewState(TreeNodeCollection dst, TreeNodeCollection src)
        {
            foreach (TreeNode d in dst)
            {
                var s = src[d.Name];
                if (s == null)
                    continue;
                if (s.IsExpanded)
                    d.Expand();
                RestoreTreeViewState(d.Nodes, s.Nodes);
            }
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
            return comboBoxGroup.Text == "装着";
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
            var i = Array.FindIndex(_shipList, s => s.Id == id);
            if (i == -1)
                return;
            var y = (int)Math.Round(ShipLabel.ScaleFactor.Height * 16 * i);
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
            var g = Array.FindIndex(new[] {'Z', 'A', 'B', 'C', 'D', 'G', 'R', 'W', 'Q'},
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
            ActiveControl = InItemList() ? (Control)treeViewItem : panelShipList;
        }
    }
}