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
    public partial class ListForm : Form
    {
        private readonly Sniffer _sniffer;
        private readonly Config _config;
        public const int PanelWidth = 217;

        public enum SortOrder
        {
            None,
            Cond,
            ExpToNext,
            Repair
        }

        public ListForm(Sniffer sniffer, Config config)
        {
            InitializeComponent();
            _sniffer = sniffer;
            _config = config;
            var swipe = new SwipeScrollify();
            swipe.AddShipListPanel(shipListPanel);
            swipe.AddTreeView(itemTreeView);
            swipe.AddPanel(fleetPanel);
        }

        public void UpdateList()
        {
            panelItemHeader.Visible = InItemList || InAntiAir || InMiscText;
            panelGroupHeader.Visible = InGroupConfig;
            panelRepairHeader.Visible = InRepairList;
            panelFleetHeader.Visible = InFleetInfo;
            // SwipeScrollifyが誤作動するのでEnabledも切り替える
            shipListPanel.Visible = shipListPanel.Enabled = InShipStatus || InGroupConfig || InRepairList;
            itemTreeView.Visible = itemTreeView.Enabled = InItemList;
            fleetPanel.Visible = fleetPanel.Enabled = InFleetInfo;
            antiAirPanel.Visible = antiAirPanel.Enabled = InAntiAir;
            airBattleResultPanel.Visible = airBattleResultPanel.Enabled = InAntiAir;
            richTextBoxMiscText.Visible = InMiscText;
            if (InItemList)
            {
                itemTreeView.SetNodes(_sniffer.ItemList);
            }
            else if (InFleetInfo)
            {
                fleetPanel.Update(_sniffer);
            }
            else if (InAntiAir)
            {
                antiAirPanel.Update(_sniffer);
            }
            else if (InMiscText)
            {
                richTextBoxMiscText.Text = _sniffer.MiscText;
            }
            else
            {
                SetHeaderSortOrder();
                shipListPanel.Update(_sniffer, comboBoxGroup.Text, _config.ShipList.SortOrder, _config.ShipList.ShipType);
            }
        }

        public void UpdateAirBattleResult()
        {
            airBattleResultPanel.ShowResultAutomatic = _config.AlwaysShowResultRank;
            airBattleResultPanel.SetResult(_sniffer.Battle.AirBattleResults);
        }

        private void SetHeaderSortOrder()
        {
            switch (_config.ShipList.SortOrder)
            {
                case SortOrder.None:
                    labelHeaderCond.Text = "cond";
                    labelHeaderExp.Text = "Exp";
                    break;
                case SortOrder.Cond:
                    labelHeaderCond.Text = "cond▴";
                    labelHeaderExp.Text = "Exp";
                    break;
                case SortOrder.ExpToNext:
                    labelHeaderCond.Text = "cond";
                    labelHeaderExp.Text = "Exp▴";
                    break;
            }
        }

        private bool InShipStatus => Array.Exists(new[] {"全員", "A", "B", "C", "D"}, x => comboBoxGroup.Text == x);

        private bool InGroupConfig => comboBoxGroup.Text == "分類";

        private bool InRepairList => comboBoxGroup.Text == "修復";

        private bool InItemList => comboBoxGroup.Text == "装備";

        private bool InFleetInfo => comboBoxGroup.Text == "艦隊";

        private bool InAntiAir => comboBoxGroup.Text == "対空";

        private bool InMiscText => comboBoxGroup.Text == "情報";

        private void ShipListForm_Load(object sender, EventArgs e)
        {
            shipListPanel.Width = itemTreeView.Width = fleetPanel.Width =
                (int)Round(PanelWidth * ShipLabel.ScaleFactor.Width) + 3 + SystemInformation.VerticalScrollBarWidth;
            Width = shipListPanel.Width + 12 + (Width - ClientSize.Width);
            MinimumSize = new Size(Width, 0);
            MaximumSize = new Size(Width, int.MaxValue);
            var config = _config.ShipList;
            checkBoxShipType.Checked = config.ShipType;
            ActiveControl = shipListPanel;
            for (var i = 0; i < ShipListPanel.GroupCount; i++)
            {
                shipListPanel.GroupSettings[i] = i < config.ShipGroup.Count
                    ? new HashSet<int>(config.ShipGroup[i])
                    : new HashSet<int>();
            }
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
            for (var i = 0; i < ShipListPanel.GroupCount; i++)
            {
                if (all.Length > 0)
                    shipListPanel.GroupSettings[i].IntersectWith(all);
                config.ShipGroup.Add(shipListPanel.GroupSettings[i].ToList());
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
                shipListPanel.ShowShip(id);
            }
            else if (InFleetInfo)
            {
                fleetPanel.ShowShip(id);
            }
            else if (InAntiAir)
            {
                antiAirPanel.ShowShip(id);
            }
        }

        private void checkBoxShipType_CheckedChanged(object sender, EventArgs e)
        {
            _config.ShipList.ShipType = checkBoxShipType.Checked;
            UpdateList();
            SetActiveControl();
        }

        private void comboBoxGroup_DropDownClosed(object sender, EventArgs e)
        {
            SetActiveControl();
        }

        private void comboBoxGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateList();
            SetActiveControl();
            copyToolStripMenuItem.Enabled = InShipStatus | InItemList;
        }

        private void ShipListForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            var g = Array.FindIndex(new[] {'Z', 'A', 'B', 'C', 'D', 'G', 'R', 'W', 'X', 'Y', 'I'},
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
            {
                ActiveControl = itemTreeView;
            }
            else if (InFleetInfo)
            {
                ActiveControl = fleetPanel;
            }
            else if (InAntiAir)
            {
                ActiveControl = antiAirPanel;
            }
            else
            {
                ActiveControl = shipListPanel;
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TextGenerator.GenerateItemList(_sniffer.ItemList));
        }

        private void textToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TextGenerator.GenerateFleetData(_sniffer));
        }

        private void deckBuilderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TextGenerator.GenerateDeckBuilderData(_sniffer));
        }

        private void labelHeaderCond_Click(object sender, EventArgs e)
        {
            _config.ShipList.SortOrder = _config.ShipList.SortOrder == SortOrder.Cond ? SortOrder.None : SortOrder.Cond;
            UpdateList();
        }

        private void labelHeaderExp_Click(object sender, EventArgs e)
        {
            _config.ShipList.SortOrder = _config.ShipList.SortOrder == SortOrder.ExpToNext
                ? SortOrder.None
                : SortOrder.ExpToNext;
            UpdateList();
        }

        private void csvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TextGenerator.GenerateShipList(shipListPanel.CurrentShipList));
        }

        private void kantaiSarashiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TextGenerator.GenerateKantaiSarashiData(shipListPanel.CurrentShipList));
        }

        private void labelFleet_Click(object sender, EventArgs e)
        {
            fleetPanel.ShowFleet(((Label)sender).Text);
        }
    }
}