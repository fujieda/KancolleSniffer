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
using KancolleSniffer.Model;
using KancolleSniffer.View;
using KancolleSniffer.View.ListWindow;
using KancolleSniffer.View.ShipListPanel;
using Clipboard = KancolleSniffer.Util.Clipboard;

namespace KancolleSniffer.Forms
{
    public partial class ListForm : Form
    {
        private readonly Sniffer _sniffer;
        private readonly Config _config;
        private readonly Form _form;
        private readonly MainWindow.TimeOutChecker _suppressActivate;
        private readonly CheckBox[] _shipTypeCheckBoxes;
        private bool _isMaster;
        private ShipListConfig _listConfig;
        private FormWindowState _windowState = FormWindowState.Normal;
        public const int PanelWidth = 215;

        private object[] PanelNames => new object[] {"全艦", "A", "B", "C", "D", "分類", "修復", "装備", "艦隊", "対空", "戦況", "情報"}
            .Where(n => IsMaster || (string)n != "分類").ToArray();

        private char[] PanelKeys => new[] {'Z', 'A', 'B', 'C', 'D', 'G', 'R', 'W', 'X', 'Y', 'S', 'I'}
            .Where(key => IsMaster || key != 'G').ToArray();

        private bool IsMaster
        {
            get => _isMaster;
            set
            {
                _isMaster = value;
                Text = _isMaster ? "一覧 プライマリ" : "一覧";
                comboBoxGroup.Items.Clear();
                comboBoxGroup.Items.AddRange(PanelNames);
            }
        }

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

        public ListForm(MainWindow main, bool isMaster = false)
        {
            InitializeComponent();
            IsMaster = isMaster;
            _form = main.Form;
            _sniffer = main.Sniffer;
            _config = main.Config;
            _suppressActivate = main.SuppressActivate;
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
            SetupSettings();
        }

        private void SetupSettings()
        {
            _listConfig = GetConfig();
            if (_listConfig.ShowHpInPercent)
            {
                shipListPanel.ToggleHpPercent();
                battleResultPanel.ToggleHpPercent();
            }
        }

        private ShipListConfig GetConfig()
        {
            if (_isMaster)
            {
                SetGroup();
                return _config.ShipList;
            }
            if (_config.ListFormGroup.Count == 0)
                return CreateSecondaryConfig();
            var config = _config.ListFormGroup[0];
            _config.ListFormGroup.RemoveAt(0);
            config.ShipGroup = _config.ShipList.ShipGroup;
            return config;
        }

        private ShipListConfig CreateSecondaryConfig()
        {
            var src = _config.ShipList;
            var config = new ShipListConfig
            {
                Mode = src.Mode,
                ShipCategories = src.ShipCategories,
                ShipType = src.ShipType,
                ShowHpInPercent = src.ShowHpInPercent,
                SortOrder = src.SortOrder,
                Location = src.Location,
                Size = src.Size,
                ShipGroup = src.ShipGroup
            };
            if (config.Mode == "分類" || string.IsNullOrEmpty(config.Mode))
                config.Mode = "全艦";
            return config;
        }

        private void SetGroup()
        {
            var groups = _config.ShipList.ShipGroup;
            for (var i = groups.Count; i < GroupConfigLabels.GroupCount; i++)
                groups.Add(new List<int>());
            shipListPanel.GroupSettings = groups;
        }

        public void UpdateList()
        {
            if (!Visible)
                return;
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
                shipListPanel.Update(_sniffer, comboBoxGroup.Text, _listConfig);
            }
            if (shipListPanel.GroupUpdated)
            {
                PurifyShipGroup();
                _config.Save();
                shipListPanel.GroupUpdated = false;
            }
        }

        private void PurifyShipGroup()
        {
            var all = _sniffer.ShipList.Select(s => s.Id).ToArray();
            if (all.Length == 0)
                return;
            foreach (var g in _config.ShipList.ShipGroup)
            {
                var filtered = g.Intersect(all).ToArray();
                g.Clear();
                g.AddRange(filtered);
            }
        }

        private void SetHeaderVisibility()
        {
            static void Set(Control header, bool visible)
            {
                header.Visible = visible;
                if (visible)
                    header.BringToFront();
            }

            Set(panelShipHeader, InShipStatus);
            Set(panelGroupHeader, InGroupConfig);
            Set(panelRepairHeader, InRepairList);
            Set(panelFleetHeader, InFleetInfo);
            SetSTypeDropDownVisible(InShipStatus || InGroupConfig || InRepairList);
        }

        private void SetPanelVisibility()
        {
            static void Set(Control panel, bool visible)
            {
                // SwipeScrollifyが誤作動するのでEnabledも切り替える
                panel.Visible = panel.Enabled = visible;
            }

            Set(shipListPanel, InShipStatus || InGroupConfig || InRepairList);
            Set(itemTreeView, InItemList);
            Set(fleetPanel, InFleetInfo);
            Set(antiAirPanel, InAntiAir);
            Set(airBattleResultPanel, InBattleResult);
            Set(battleResultPanel, InBattleResult);
            Set(richTextBoxMiscText, InMiscText);
        }

        public void UpdateAirBattleResult()
        {
            airBattleResultPanel.ShowResultAutomatic = (_config.Spoilers & Spoiler.AirBattleResult) != 0;
            airBattleResultPanel.SetResult(_sniffer);
        }

        public void UpdateBattleResult()
        {
            MoveToBattleResult();
            battleResultPanel.Spoilers = _config.Spoilers;
            battleResultPanel.Update(_sniffer);
            BackFromBattleResult();
        }

        private int _prevSelectedIndex = -1;
        private const int BattleResultIndex = 10;

        private void MoveToBattleResult()
        {
            if (!_isMaster || !_config.ShipList.AutoBattleResult || comboBoxGroup.SelectedIndex == BattleResultIndex ||
                _sniffer.InSortie == -1)
                return;
            _prevSelectedIndex = comboBoxGroup.SelectedIndex;
            comboBoxGroup.SelectedIndex = BattleResultIndex;
        }

        private void BackFromBattleResult()
        {
            if (_sniffer.InSortie != -1 || _prevSelectedIndex == -1)
                return;
            comboBoxGroup.SelectedIndex = _prevSelectedIndex;
            _prevSelectedIndex = -1;
        }

        public void UpdateCellInfo()
        {
            MoveToBattleResult();
            battleResultPanel.Spoilers = _config.Spoilers;
            battleResultPanel.UpdateCellInfo(_sniffer.CellInfo);
        }

        private void SetHeaderSortOrder()
        {
            switch (_listConfig.SortOrder)
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

        private bool InShipStatus => Array.Exists(new[] {"全艦", "A", "B", "C", "D"}, x => _listConfig.Mode == x);

        private bool InGroupConfig => _listConfig.Mode == "分類";

        private bool InRepairList => _listConfig.Mode == "修復";

        private bool InItemList => _listConfig.Mode == "装備";

        private bool InFleetInfo => _listConfig.Mode == "艦隊";

        private bool InAntiAir => _listConfig.Mode == "対空";

        private bool InBattleResult => _listConfig.Mode == "戦況";

        private bool InMiscText => _listConfig.Mode == "情報";

        private void ListForm_Load(object sender, EventArgs e)
        {
            AdjustHeader();
            SetMinimumSize();
            comboBoxGroup.SelectedItem = _listConfig.Mode;
            SetCheckBoxSTypeState();
            if (_listConfig.Location.X == int.MinValue)
                return;
            var bounds = new Rectangle(_listConfig.Location, _listConfig.Size);
            if (MainWindow.IsTitleBarOnAnyScreen(bounds.Location))
                Location = bounds.Location;
            Size = bounds.Size;
        }

        private void AdjustHeader()
        {
            if (_config.Zoom == 100)
                return;
            foreach (var header in new[]
            {
                panelShipHeader, panelGroupHeader, panelRepairHeader
            })
            {
                header.Left += SystemInformation.VerticalScrollBarWidth * (_config.Zoom - 100) / 100;
            }
        }

        private void SetMinimumSize()
        {
            MinimumSize = new Size(Width - Scaler.ScaleWidth(24) -
                                   SystemInformation.VerticalScrollBarWidth * (_config.Zoom - 100) / 100, 0);
        }

        private void SetCheckBoxSTypeState()
        {
            for (var type = 0; type < _shipTypeCheckBoxes.Length; type++)
                _shipTypeCheckBoxes[type].Checked = ((int)_listConfig.ShipCategories & (1 << type)) != 0;
            checkBoxSTypeAll.Checked = _listConfig.ShipCategories == ShipCategory.All;
            checkBoxSTypeDetails.Checked = _listConfig.ShipType;
        }

        private void ListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        public void SaveConfig()
        {
            if (_isMaster)
            {
                SaveMasterState();
            }
            else
            {
                SaveSlaveState();
            }
        }

        private void SaveMasterState()
        {
            PurifyShipGroup();
            _listConfig.Visible = Visible && WindowState == FormWindowState.Normal;
            SaveBounds(_listConfig); // 最小化時は以前のサイズを記録する
        }

        private void SaveSlaveState()
        {
            if (!Visible)
                return;
            if (WindowState != FormWindowState.Normal) // 最小化時は次回復旧しない
                return;
            _listConfig.Visible = true;
            _listConfig.ShipGroup = null;
            _config.ListFormGroup.Add(_listConfig);
            SaveBounds(_listConfig);
        }

        private void SaveBounds(ShipListConfig config)
        {
            var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            config.Location = bounds.Location;
            config.Size = bounds.Size;
        }

        public void ChangeWindowState(FormWindowState newState)
        {
            if (!Visible)
                return;
            if (newState == FormWindowState.Minimized)
            {
                if (WindowState == FormWindowState.Normal)
                    WindowState = FormWindowState.Minimized;
                if (_config.HideOnMinimized)
                    ShowInTaskbar = false;
            }
            else
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    Application.DoEvents();
                    if (_config.HideOnMinimized)
                        ShowInTaskbar = true;
                    WindowState = FormWindowState.Normal;
                }
            }
        }

        private void ListForm_Activated(object sender, EventArgs e)
        {
            if (!_isMaster)
                return;
            if (_suppressActivate.Check())
                return;
            if (WindowState == FormWindowState.Minimized)
                return;
            RaiseBothWindows();
        }

        private void RaiseBothWindows()
        {
            _form.Owner = null;
            Owner = _form;
            BringToFront();
            Owner = null;
        }

        public void ShowShip(int id)
        {
            if (!Visible)
                return;
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
            _listConfig.Mode = comboBoxGroup.Text;
            if (!Visible)
                return;
            UpdateList();
            SetActiveControl();
            if (!(InShipStatus || InGroupConfig || InRepairList))
                SetPanelSTypeState(false);
        }

        private void ListForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            var g = Array.FindIndex(PanelKeys, x => x == char.ToUpper(e.KeyChar));
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

        private void itemCsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TextGenerator.GenerateItemList(_sniffer.ItemList));
        }

        private void kantaiBunsekiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TextGenerator.GenerateKantaiBunsekiItemList(_sniffer.ItemList));
        }

        private void fleetTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TextGenerator.GenerateFleetData(_sniffer));
        }

        private void deckBuilderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TextGenerator.GenerateDeckBuilderData(_sniffer));
        }

        private void labelHeaderCond_Click(object sender, EventArgs e)
        {
            switch (_listConfig.SortOrder)
            {
                case SortOrder.CondAscend:
                    _listConfig.SortOrder = SortOrder.CondDescend;
                    break;
                case SortOrder.CondDescend:
                    _listConfig.SortOrder = SortOrder.None;
                    break;
                default:
                    _listConfig.SortOrder = SortOrder.CondAscend;
                    break;
            }
            UpdateList();
        }

        private void labelHeaderExp_Click(object sender, EventArgs e)
        {
            switch (_listConfig.SortOrder)
            {
                case SortOrder.ExpToNextAscend:
                    _listConfig.SortOrder = SortOrder.ExpToNextDescend;
                    break;
                case SortOrder.ExpToNextDescend:
                    _listConfig.SortOrder = SortOrder.None;
                    break;
                default:
                    _listConfig.SortOrder = SortOrder.ExpToNextAscend;
                    break;
            }
            UpdateList();
        }

        private void shipCsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TextGenerator.GenerateShipList(shipListPanel.CurrentShipList));
        }

        private void kantaiSarashiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TextGenerator.GenerateKantaiSarashiData(shipListPanel.CurrentShipList));
        }

        private void labelFleet_Click(object sender, EventArgs e)
        {
            fleetPanel.ShowFleet(((Control)sender).Text);
        }

        private void labelHeaderHp_Click(object sender, EventArgs e)
        {
            ToggleHpPercent();
        }

        private void ToggleHpPercent()
        {
            _listConfig.ShowHpInPercent = !_listConfig.ShowHpInPercent;
            shipListPanel.ToggleHpPercent();
            battleResultPanel.ToggleHpPercent();
        }

        private void SetSTypeDropDownVisible(bool visible)
        {
            if (!visible)
                SetPanelSTypeState(false);
            dropDownButtonSType.Visible = visible;
            labelSType.Visible = visible;
        }

        private void labelSTypeButton_Click(object sender, EventArgs e)
        {
            SetPanelSTypeState(!panelSType.Visible);
        }

        private void checkBoxSType_Click(object sender, EventArgs e)
        {
            _listConfig.ShipCategories = SelectedShipTypes;
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
            if (visible)
                panelSType.BringToFront();
            dropDownButtonSType.BackColor = visible ? CustomColors.ActiveButtonColor : DefaultBackColor;
        }

        private void checkBoxSTypeDetails_Click(object sender, EventArgs e)
        {
            _listConfig.ShipType = checkBoxSTypeDetails.Checked;
            UpdateList();
            SetActiveControl();
        }

        private void ListForm_ResizeEnd(object sender, EventArgs e)
        {
            foreach (var panel in new IPanelResize[]
                {shipListPanel, antiAirPanel, airBattleResultPanel, battleResultPanel, fleetPanel})
            {
                if (panel.Visible)
                    panel.ApplyResize();
            }
        }

        private void ListForm_Resize(object sender, EventArgs e)
        {
            if (_windowState != WindowState && WindowState == FormWindowState.Normal)
                UpdateList();
            _windowState = WindowState;
        }

        private void ListForm_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
                UpdateList();
        }
    }
}