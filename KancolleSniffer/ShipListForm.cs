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
using System.Windows.Forms;
using static System.Math;

namespace KancolleSniffer
{
    public partial class ShipListForm : Form
    {
        private readonly Sniffer _sniffer;
        private readonly Config _config;
        private const int LabelHeight = 12;
        private const int LineHeight = 16;
        public const int PanelWidth = 217;
        private ShipStatus[] _shipList;
        private readonly List<ShipLabel[]> _labelList = new List<ShipLabel[]>();
        private readonly List<Panel> _labelPanelList = new List<Panel>();
        private readonly List<CheckBox[]> _checkBoxesList = new List<CheckBox[]>();
        private readonly List<ShipLabel[]> _configLabelList = new List<ShipLabel[]>();
        private readonly List<Panel> _checkBoxPanelList = new List<Panel>();
        private readonly List<ShipLabel[]> _repairLabelList = new List<ShipLabel[]>();
        private readonly List<Panel> _repairPanelList = new List<Panel>();
        public const int GroupCount = 4;
        private readonly HashSet<int>[] _groupSettings = new HashSet<int>[GroupCount];

        public ShipListForm(Sniffer sniffer, Config config)
        {
            InitializeComponent();
            _sniffer = sniffer;
            _config = config;
            var swipe = new SwipeScrollify();
            swipe.AddPanel(panelShipList);
            swipe.AddTreeView(itemTreeView);
            swipe.AddPanel(equipPanel);
        }

        public void UpdateList()
        {
            panelItemHeader.Visible = InItemList || InEquip || InMiscText;
            panelGroupHeader.Visible = InGroupConfig;
            panelRepairHeader.Visible = InRepairList;
            // SwipeScrollifyが誤作動するのでEnabledも切り替える
            panelShipList.Visible = panelShipList.Enabled = InShipStatus || InGroupConfig || InRepairList;
            itemTreeView.Visible = itemTreeView.Enabled = InItemList;
            equipPanel.Visible = equipPanel.Enabled = InEquip;
            richTextBoxMiscText.Visible = InMiscText;
            if (InItemList)
            {
                itemTreeView.SetNodes(_sniffer.ItemList);
            }
            else if (InEquip)
            {
                equipPanel.UpdateEquip(_sniffer);
            }
            else if (InMiscText)
            {
                richTextBoxMiscText.Text = _sniffer.MiscText;
            }
            else
            {
                CreateShipList();
                CreateListLabels();
                SetShipLabels();
            }
        }

        private void CreateShipList()
        {
            var ships = InRepairList ? _sniffer.DamagedShipList : FilterByGroup(_sniffer.ShipList).ToArray();
            if (!_config.ShipList.ShipType)
            {
                _shipList = ships.OrderBy(s => s, new CompareShip(false, InRepairList)).ToArray();
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
            _shipList = ships.Concat(types).OrderBy(s => s, new CompareShip(true, InRepairList)).ToArray();
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

        private void SetShipLabels()
        {
            panelShipList.SuspendLayout();
            for (var i = 0; i < _shipList.Length; i++)
            {
                if (!InShipStatus)
                    _labelPanelList[i].Visible = false;
                if (!InGroupConfig)
                    _checkBoxPanelList[i].Visible = false;
                if (!InRepairList)
                    _repairPanelList[i].Visible = false;
            }
            for (var i = 0; i < _shipList.Length; i++)
            {
                if (InShipStatus)
                    SetShipStatus(i);
                if (InGroupConfig)
                    SetGroupConfig(i);
                if (InRepairList)
                    SetRepairList(i);
            }
            for (var i = _shipList.Length; i < _labelPanelList.Count; i++)
            {
                _labelPanelList[i].Visible = _checkBoxPanelList[i].Visible = _repairPanelList[i].Visible = false;
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
                rpp.Visible = false;
                return;
            }
            if (!rpp.Visible)
                rpp.Location = new Point(rpp.Left, (int)rpp.Tag + panelShipList.AutoScrollPosition.Y);
            var rpl = _repairLabelList[i];
            rpl[0].SetHp(s);
            rpl[1].SetLevel(s);
            rpl[2].SetRepairTime(s);
            rpl[3].Text = TimeSpan.FromSeconds(s.RepairSecPerHp).ToString(@"mm\:ss");
            rpl[4].SetName(s, new Dictionary<string, string> {{"Graf Zeppelin改", "Graf Zeppelin"}});
            rpl[5].SetFleet(s);
            rpp.Visible = true;
        }

        private bool InShipStatus => Array.Exists(new[] {"全員", "A", "B", "C", "D"}, x => comboBoxGroup.Text == x);

        private bool InGroupConfig => comboBoxGroup.Text == "分類";

        private bool InRepairList => comboBoxGroup.Text == "修復";

        private bool InItemList => comboBoxGroup.Text == "装備";

        private bool InEquip => comboBoxGroup.Text == "艦隊";

        private bool InMiscText => comboBoxGroup.Text == "情報";

        private void ShipListForm_Load(object sender, EventArgs e)
        {
            panelShipList.Width = (int)Round(PanelWidth * ShipLabel.ScaleFactor.Width) + 3 +
                                  SystemInformation.VerticalScrollBarWidth;
            Width = panelShipList.Width + 12 + (Width - ClientSize.Width);
            MinimumSize = new Size(Width, 0);
            MaximumSize = new Size(Width, int.MaxValue);
            var config = _config.ShipList;
            checkBoxShipType.Checked = config.ShipType;
            ActiveControl = panelShipList;
            for (var i = 0; i < GroupCount; i++)
                _groupSettings[i] = config.ShipGroup.Count == 0
                    ? new HashSet<int>()
                    : new HashSet<int>(config.ShipGroup[i]);
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
            e.Cancel = true;
            if (!Visible)
                return;
            var config = _config.ShipList;
            var all = _sniffer.ShipList.Select(s => s.Id).ToArray();
            config.ShipGroup.Clear();
            for (var i = 0; i < GroupCount; i++)
            {
                if (all.Length > 0)
                    _groupSettings[i].IntersectWith(all);
                config.ShipGroup.Add(_groupSettings[i].ToList());
            }
            var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            config.Location = bounds.Location;
            config.Size = bounds.Size;
            Hide();
        }

        public void ShowShip(int id)
        {
            if (InShipStatus)
            {
                var i = Array.FindIndex(_shipList, s => s.Id == id);
                if (i == -1)
                    return;
                var y = (int)Round(ShipLabel.ScaleFactor.Height * LineHeight * i);
                panelShipList.AutoScrollPosition = new Point(0, y);
            }
            else if (InEquip)
            {
                equipPanel.ShowShip(id);
            }
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
            SetActiveControl();
            copyToolStripMenuItem.Enabled = InShipStatus;
        }

        private void ShipListForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            var g = Array.FindIndex(new[] {'Z', 'A', 'B', 'C', 'D', 'G', 'R', 'W', 'X', 'I'},
                x => x == char.ToUpper(e.KeyChar));
            if (g == -1)
                return;
            comboBoxGroup.SelectedIndex = g;
            e.Handled = true;
        }

        // マウスホイールでスクロールするためにコントロールにフォーカスを合わせる。
        private void SetActiveControl()
        {
            if (InItemList)
                ActiveControl = itemTreeView;
            else if (InEquip)
                ActiveControl = equipPanel;
            else
                ActiveControl = panelShipList;
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (InItemList)
                Clipboard.SetText(TextGenerator.GenerateItemList(_sniffer.ItemList));
            if (InShipStatus)
                Clipboard.SetText(TextGenerator.GenerateShipList(FilterByGroup(_sniffer.ShipList)));
        }

        private void textToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TextGenerator.GenerateFleetData(_sniffer));
        }

        private void deckBuilderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TextGenerator.GenerateDeckBuilderData(_sniffer));
        }
    }
}