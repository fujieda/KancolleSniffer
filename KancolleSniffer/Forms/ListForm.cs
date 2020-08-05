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
        private Settings _settings;
        public const int PanelWidth = 215;

        public class Settings
        {
            public string Mode { get; set; }
            public ShipCategory ShipCategories { get; set; }
            public bool ShipType { get; set; }
            public bool ShowHpInPercent { get; set; }
            public SortOrder SortOrder { get; set; }

            public static Settings FromShipListConfig(ShipListConfig config)
            {
                return new Settings
                {
                    Mode = config.Mode ?? "全艦",
                    ShipCategories = config.ShipCategories,
                    ShipType = config.ShipType,
                    ShowHpInPercent = config.ShowHpInPercent,
                    SortOrder = config.SortOrder
                };
            }

            public void SetToShipListConfig(ShipListConfig config)
            {
                config.Mode = Mode;
                config.ShipCategories = ShipCategories;
                config.ShipType = ShipType;
                config.ShowHpInPercent = ShowHpInPercent;
                config.SortOrder = SortOrder;
            }
        }

        private object[] PanelNames => new object[] {"全艦", "A", "B", "C", "D", "分類", "修復", "装備", "艦隊", "対空", "戦況", "情報"}
            .Where(n => IsMaster || (string)n != "分類").ToArray();

        private char[] PanelKeys => new[] {'Z', 'A', 'B', 'C', 'D', 'G', 'R', 'W', 'X', 'Y', 'S', 'I'}
            .Where(key => IsMaster || key != 'G').ToArray();

        public bool IsMaster
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

        public ListForm(MainWindow main)
        {
            InitializeComponent();
            IsMaster = false;
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
                shipListPanel.Update(_sniffer, comboBoxGroup.Text, _settings);
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
            static void Set(Control header, bool visible)
            {
                header.Visible = visible;
                if (visible)
                    header.BringToFront();
            }

            Set(panelShipHeader, InShipStatus);
            Set(panelEmptyHeader, InItemList || InAntiAir || InBattleResult || InMiscText);
            Set(panelGroupHeader, InGroupConfig);
            Set(panelRepairHeader, InRepairList);
            Set(panelFleetHeader, InFleetInfo);
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
            switch (_settings.SortOrder)
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

        private bool InShipStatus => Array.Exists(new[] {"全艦", "A", "B", "C", "D"}, x => _settings.Mode == x);

        private bool InGroupConfig => _settings.Mode == "分類";

        private bool InRepairList => _settings.Mode == "修復";

        private bool InItemList => _settings.Mode == "装備";

        private bool InFleetInfo => _settings.Mode == "艦隊";

        private bool InAntiAir => _settings.Mode == "対空";

        private bool InBattleResult => _settings.Mode == "戦況";

        private bool InMiscText => _settings.Mode == "情報";

        private void ListForm_Load(object sender, EventArgs e)
        {
            AdjustHeader();
            SetMinimumSize();
            var config = GetConfig();
            _settings = Settings.FromShipListConfig(config);
            if (_settings.ShowHpInPercent)
            {
                shipListPanel.ToggleHpPercent();
                battleResultPanel.ToggleHpPercent();
            }
            LoadShipGroupFromConfig();
            comboBoxGroup.SelectedItem = _settings.Mode;
            SetCheckBoxSTypeState();
            if (config.Location.X == int.MinValue)
                return;
            var bounds = new Rectangle(config.Location, config.Size);
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
            MinimumSize = new Size(Width - Scaler.ScaleWidth(24) - SystemInformation.VerticalScrollBarWidth * (_config.Zoom - 100) / 100, 0);
        }

        private ShipListConfig GetConfig()
        {
            if (_isMaster || _config.ListFormGroup.Count == 0)
                return _config.ShipList;
            var config = _config.ListFormGroup[0];
            _config.ListFormGroup.RemoveAt(0);
            if (config.Mode == "分類")
                config.Mode = "全艦";
            return config;
        }

        private void LoadShipGroupFromConfig()
        {
            var group = _config.ShipList.ShipGroup;
            for (var i = 0; i < GroupConfigLabels.GroupCount; i++)
                shipListPanel.GroupSettings[i] = i < group.Count ? new HashSet<int>(group[i]) : new HashSet<int>();
        }

        private void SetCheckBoxSTypeState()
        {
            for (var type = 0; type < _shipTypeCheckBoxes.Length; type++)
                _shipTypeCheckBoxes[type].Checked = ((int)_settings.ShipCategories & (1 << type)) != 0;
            checkBoxSTypeAll.Checked = _settings.ShipCategories == ShipCategory.All;
            checkBoxSTypeDetails.Checked = _settings.ShipType;
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
            StoreShipGroupToConfig();
            var config = _config.ShipList;
            config.Visible = Visible && WindowState == FormWindowState.Normal;
            _settings.SetToShipListConfig(config);
            if (!Visible)
                return;
            SaveBounds(config); // 最小化時は以前のサイズを記録する
        }

        private void SaveSlaveState()
        {
            if (!Visible)
                return;
            if (WindowState != FormWindowState.Normal) // 最小化時は次回復旧しない
                return;
            var config = new ShipListConfig {Visible = true};
            _settings.SetToShipListConfig(config);
            _config.ListFormGroup.Add(config);
            SaveBounds(config);
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

        private void StoreShipGroupToConfig()
        {
            var all = _sniffer.ShipList.Select(s => s.Id).ToArray();
            var group = _config.ShipList.ShipGroup;
            group.Clear();
            for (var i = 0; i < GroupConfigLabels.GroupCount; i++)
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
            _settings.Mode = comboBoxGroup.Text;
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
            switch (_settings.SortOrder)
            {
                case SortOrder.CondAscend:
                    _settings.SortOrder = SortOrder.CondDescend;
                    break;
                case SortOrder.CondDescend:
                    _settings.SortOrder = SortOrder.None;
                    break;
                default:
                    _settings.SortOrder = SortOrder.CondAscend;
                    break;
            }
            UpdateList();
        }

        private void labelHeaderExp_Click(object sender, EventArgs e)
        {
            switch (_settings.SortOrder)
            {
                case SortOrder.ExpToNextAscend:
                    _settings.SortOrder = SortOrder.ExpToNextDescend;
                    break;
                case SortOrder.ExpToNextDescend:
                    _settings.SortOrder = SortOrder.None;
                    break;
                default:
                    _settings.SortOrder = SortOrder.ExpToNextAscend;
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
            fleetPanel.ShowFleet(((Label)sender).Text);
        }

        private void labelHeaderHp_Click(object sender, EventArgs e)
        {
            ToggleHpPercent();
        }

        private void ToggleHpPercent()
        {
            _settings.ShowHpInPercent = !_settings.ShowHpInPercent;
            shipListPanel.ToggleHpPercent();
            battleResultPanel.ToggleHpPercent();
        }

        private void labelSTypeButton_Click(object sender, EventArgs e)
        {
            SetPanelSTypeState(!panelSType.Visible);
        }

        private void checkBoxSType_Click(object sender, EventArgs e)
        {
            _settings.ShipCategories = SelectedShipTypes;
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
            _settings.ShipType = checkBoxSTypeDetails.Checked;
            UpdateList();
            SetActiveControl();
        }

        private void ListForm_ResizeEnd(object sender, EventArgs e)
        {
            foreach (var panel in new IPanelResize[] {shipListPanel, antiAirPanel, airBattleResultPanel, battleResultPanel, fleetPanel})
            {
                if (panel.Visible)
                    panel.ApplyResize();
            }
        }
    }
}