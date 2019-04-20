﻿// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using KancolleSniffer.View;
using static System.Math;

namespace KancolleSniffer
{
    public partial class ListForm : Form
    {
        private readonly Sniffer _sniffer;
        private readonly Config _config;
        private readonly Form _main;
        private readonly CheckBox[] _shipTypeCheckBoxes;
        public const int PanelWidth = 217;

        public enum SortOrder
        {
            None,
            Cond,
            CondAscend = Cond,
            CondDescend,
            ExpToNext,
            ExpToNextAscend = ExpToNext,
            ExpToNextDescend,
            Repair
        }

        public ListForm(Sniffer sniffer, Config config, Form main)
        {
            InitializeComponent();
            _sniffer = sniffer;
            _config = config;
            _main = main;
            _shipTypeCheckBoxes = new[]
            {
                checkBoxSTypeBattleShip,
                checkBoxSTypeAircraftCarrier,
                checkBoxSTypeHeavyCruiser,
                checkBoxSTypeLightCruiser,
                checkBoxSTypeDestroyer,
                checkBoxSTypeEscort,
                checkBoxSTypeSubmarine,
                checkBoxSTypeAuxiliary
            };
            battleResultPanel.HpLabelClick += ToggleHpPercent;
            shipListPanel.HpLabelClick += ToggleHpPercent;
            var swipe = new SwipeScrollify();
            swipe.AddShipListPanel(shipListPanel);
            swipe.AddTreeView(itemTreeView);
            swipe.AddPanel(fleetPanel);
        }

        /// <summary>
        /// パネルのz-orderがくるうのを避ける
        /// https://stackoverflow.com/a/5777090/1429506
        /// </summary>
        private void ListForm_Shown(object sender, EventArgs e)
        {
            // ReSharper disable once NotAccessedVariable
            IntPtr handle;
            foreach (Control panel in Controls)
                // ReSharper disable once RedundantAssignment
                handle = panel.Handle;
        }

        public void UpdateList()
        {
            SetHeaderVisibility();
            SetPanelVisibility();
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
            else if (InShipStatus || InGroupConfig || InRepairList)
            {
                SetHeaderSortOrder();
                shipListPanel.Update(_sniffer, comboBoxGroup.Text, _config.ShipList);
            }
            if (shipListPanel.GroupUpdated)
            {
                StoreShipGroupToConfig();
                _config.Save();
                shipListPanel.GroupUpdated = false;
            }
        }

        private void SetHeaderVisibility()
        {
            panelItemHeader.Visible = InItemList || InAntiAir || InBattleResult || InMiscText;
            panelGroupHeader.Visible = InGroupConfig;
            panelRepairHeader.Visible = InRepairList;
            panelFleetHeader.Visible = InFleetInfo;
        }

        private void SetPanelVisibility()
        {
            SetVisible(shipListPanel, InShipStatus || InGroupConfig || InRepairList);
            SetVisible(itemTreeView, InItemList);
            SetVisible(fleetPanel, InFleetInfo);
            SetVisible(antiAirPanel, InAntiAir);
            SetVisible(airBattleResultPanel, InBattleResult);
            SetVisible(battleResultPanel, InBattleResult);
            SetVisible(richTextBoxMiscText, InMiscText);
        }

        private void SetVisible(Control control, bool visible)
        {
            // SwipeScrollifyが誤作動するのでEnabledも切り替える
            control.Visible = control.Enabled = visible;
        }

        public void UpdateAirBattleResult()
        {
            airBattleResultPanel.ShowResultAutomatic = (_config.Spoilers & Spoiler.AirBattleResult) != 0;
            airBattleResultPanel.SetResult(_sniffer);
        }

        public void UpdateBattleResult()
        {
            battleResultPanel.Spoilers = _config.Spoilers;
            battleResultPanel.Update(_sniffer);
        }

        public void UpdateCellInfo()
        {
            battleResultPanel.Spoilers = _config.Spoilers;
            battleResultPanel.UpdateCellInfo(_sniffer.CellInfo);
        }

        private void SetHeaderSortOrder()
        {
            switch (_config.ShipList.SortOrder)
            {
                case SortOrder.None:
                    labelHeaderCond.Text = "cond";
                    labelHeaderExp.Text = "Exp";
                    break;
                case SortOrder.CondAscend:
                    labelHeaderCond.Text = "cond▴";
                    labelHeaderExp.Text = "Exp";
                    break;
                case SortOrder.CondDescend:
                    labelHeaderCond.Text = "cond▾";
                    labelHeaderExp.Text = "Exp";
                    break;
                case SortOrder.ExpToNextAscend:
                    labelHeaderCond.Text = "cond";
                    labelHeaderExp.Text = "Exp▴";
                    break;
                case SortOrder.ExpToNextDescend:
                    labelHeaderCond.Text = "cond";
                    labelHeaderExp.Text = "Exp▾";
                    break;
            }
        }

        private bool InShipStatus => Array.Exists(new[] {"全艦", "A", "B", "C", "D"}, x => comboBoxGroup.Text == x);

        private bool InGroupConfig => comboBoxGroup.Text == "分類";

        private bool InRepairList => comboBoxGroup.Text == "修復";

        private bool InItemList => comboBoxGroup.Text == "装備";

        private bool InFleetInfo => comboBoxGroup.Text == "艦隊";

        private bool InAntiAir => comboBoxGroup.Text == "対空";

        private bool InBattleResult => comboBoxGroup.Text == "戦況";

        private bool InMiscText => comboBoxGroup.Text == "情報";

        private void ShipListForm_Load(object sender, EventArgs e)
        {
            /* DPIではなくズームしたときにパネルは大きくなるがScrollBarはそのままなので隙間ができる。
               そこでScrollBarの幅に合わせて全体の横幅を設定し直す。*/
            Width = (int)Round((PanelWidth + 12 /* PanelとFrameの内側 */) * ShipLabel.ScaleFactor.Width) +
                    SystemInformation.VerticalScrollBarWidth + 2 /* 縁の幅 */ + Width - ClientSize.Width;
            MinimumSize = new Size(Width, 0);
            MaximumSize = new Size(Width, int.MaxValue);
            var config = _config.ShipList;
            if (config.ShowHpInPercent)
            {
                shipListPanel.ToggleHpPercent();
                battleResultPanel.ToggleHpPercent();
            }
            LoadShipGroupFromConfig();
            comboBoxGroup.SelectedItem = config.Mode ?? "全艦";
            SetCheckBoxSTypeSate();
            if (config.Location.X == int.MinValue)
                return;
            var bounds = new Rectangle(config.Location, config.Size);
            if (MainForm.IsTitleBarOnAnyScreen(bounds.Location))
                Location = bounds.Location;
            Height = bounds.Height;
        }

        private void LoadShipGroupFromConfig()
        {
            var group = _config.ShipList.ShipGroup;
            for (var i = 0; i < ShipListPanel.GroupCount; i++)
                shipListPanel.GroupSettings[i] = i < group.Count ? new HashSet<int>(group[i]) : new HashSet<int>();
        }

        private void SetCheckBoxSTypeSate()
        {
            for (var type = 0; type < _shipTypeCheckBoxes.Length; type++)
                _shipTypeCheckBoxes[type].Checked = ((int)_config.ShipList.ShipCategories & (1 << type)) != 0;
            checkBoxSTypeAll.Checked = _config.ShipList.ShipCategories == ShipCategory.All;
            checkBoxSTypeDetails.Checked = _config.ShipList.ShipType;
        }

        private void ShipListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            if (e.CloseReason == CloseReason.FormOwnerClosing)
                return;
            var config = _config.ShipList;
            StoreShipGroupToConfig();
            var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            config.Location = bounds.Location;
            config.Size = bounds.Size;
            config.Mode = (string)comboBoxGroup.SelectedItem;
            config.Visible = Visible && WindowState == FormWindowState.Normal;
            Hide();
        }

        private bool _suppressActivate = true;

        private void ListForm_Activated(object sender, EventArgs e)
        {
            if (_suppressActivate)
            {
                _suppressActivate = false;
                return;
            }
            if (Owner == _main)
                return;
            _main.Owner = null;
            Owner = _main;
            BringToFront();
        }

        private void StoreShipGroupToConfig()
        {
            var all = _sniffer.ShipList.Select(s => s.Id).ToArray();
            var group = _config.ShipList.ShipGroup;
            group.Clear();
            for (var i = 0; i < ShipListPanel.GroupCount; i++)
            {
                if (all.Length > 0)
                    shipListPanel.GroupSettings[i].IntersectWith(all);
                group.Add(shipListPanel.GroupSettings[i].ToList());
            }
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

        private void comboBoxGroup_DropDownClosed(object sender, EventArgs e)
        {
            SetActiveControl();
        }

        private void comboBoxGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateList();
            SetActiveControl();
            copyToolStripMenuItem.Enabled = InShipStatus | InItemList;
            if (!(InShipStatus || InGroupConfig || InRepairList))
                SetPanelSTypeState(false);
        }

        private void ShipListForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            var g = Array.FindIndex(new[] {'Z', 'A', 'B', 'C', 'D', 'G', 'R', 'W', 'X', 'Y', 'S', 'I'},
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
            var sl = _config.ShipList;
            switch (sl.SortOrder)
            {
                case SortOrder.CondAscend:
                    sl.SortOrder = SortOrder.CondDescend;
                    break;
                case SortOrder.CondDescend:
                    sl.SortOrder = SortOrder.None;
                    break;
                default:
                    sl.SortOrder = SortOrder.CondAscend;
                    break;
            }
            UpdateList();
        }

        private void labelHeaderExp_Click(object sender, EventArgs e)
        {
            var sl = _config.ShipList;
            switch (sl.SortOrder)
            {
                case SortOrder.ExpToNextAscend:
                    sl.SortOrder = SortOrder.ExpToNextDescend;
                    break;
                case SortOrder.ExpToNextDescend:
                    sl.SortOrder = SortOrder.None;
                    break;
                default:
                    sl.SortOrder = SortOrder.ExpToNextAscend;
                    break;
            }
            UpdateList();
        }

        private void csvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TextGenerator.GenerateShipList(shipListPanel.CurrentShipList));
        }

        // ReSharper disable IdentifierTypo
        private void kantaiSarashiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TextGenerator.GenerateKantaiSarashiData(shipListPanel.CurrentShipList));
        }
        // ReSharper enable IdentifierTypo

        private void labelFleet_Click(object sender, EventArgs e)
        {
            fleetPanel.ShowFleet(((Label)sender).Text);
        }

        private void labelHeaderHp_Click(object sender, EventArgs e)
        {
            ToggleHpPercent();
        }

        private void ToggleHpPercent()
        {
            _config.ShipList.ShowHpInPercent = !_config.ShipList.ShowHpInPercent;
            shipListPanel.ToggleHpPercent();
            battleResultPanel.ToggleHpPercent();
        }

        private void labelSTypeButton_Click(object sender, EventArgs e)
        {
            SetPanelSTypeState(!panelSType.Visible);
        }

        private void checkBoxSType_Click(object sender, EventArgs e)
        {
            _config.ShipList.ShipCategories = SelectedShipTypes;
            UpdateList();
            SetActiveControl();
        }

        private ShipCategory SelectedShipTypes =>
            (ShipCategory)_shipTypeCheckBoxes.Select((cb, type) => cb.Checked ? 1 << type : 0).Sum();

        private void checkBoxSTypeAll_Click(object sender, EventArgs e)
        {
            foreach (var checkBox in _shipTypeCheckBoxes)
                checkBox.Checked = checkBoxSTypeAll.Checked;
            checkBoxSType_Click(sender, e);
        }

        private void panelSType_Click(object sender, EventArgs e)
        {
            SetPanelSTypeState(false);
        }

        private void SetPanelSTypeState(bool visible)
        {
            panelSType.Visible = visible;
            labelSTypeButton.BackColor = visible ? CustomColors.ActiveButtonColor : DefaultBackColor;
        }

        private void checkBoxSTypeDetails_Click(object sender, EventArgs e)
        {
            _config.ShipList.ShipType = checkBoxSTypeDetails.Checked;
            UpdateList();
            SetActiveControl();
        }
    }
}