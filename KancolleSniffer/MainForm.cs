// Copyright (C) 2013, 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using KancolleSniffer.Notification;
using KancolleSniffer.Util;
using KancolleSniffer.View;
using Clipboard = KancolleSniffer.Util.Clipboard;

namespace KancolleSniffer
{
    public partial class MainForm : Form, IMainForm
    {
        private readonly ResizableToolTip _toolTip = new ResizableToolTip();
        private readonly ResizableToolTip _tooltipCopy = new ResizableToolTip {ShowAlways = false, AutomaticDelay = 0};
        private readonly ListFormGroup _listFormGroup;
        private bool _started;

        private IEnumerable<IUpdateContext> _updateable;
        private IEnumerable<IUpdateTimers> _timers;
        private Main _main;

        public Sniffer Sniffer { get; private set; }
        public Config Config { get; private set; }
        public Label PlayLogSign => hqPanel.PlayLog;
        public Notifier Notifier { get; }

        public MainForm(Main main)
        {
            InitializeComponent();
            SetupMain(main);
            _listFormGroup = new ListFormGroup(this);
            Notifier = new Notifier(FlashWindow, ShowTaster, PlaySound);
            SetupView();
        }

        private void SetupMain(Main main)
        {
            _main = main;
            Config = main.Config;
            Sniffer = main.Sniffer;
        }

        private void SetupView()
        {
            SetScaleFactorOfDpiScaling();
            SetupQuestPanel();
            SetMainFormEventHandler();
            mainFleetPanel.AkashiRepairTimer = labelAkashiRepairTimer;
            mainFleetPanel.ShowShipOnList = ShowShipOnShipList;
            panelRepairList.CreateLabels(panelRepairList_Click);
            ndockPanel.SetClickHandler(labelNDockCaption);
            missionPanel.SetClickHandler(labelMissionCaption);
            materialHistoryPanel.SetClickHandler(labelMaterialCaption, dropDownButtonMaterialHistory);
            SetupUpdateable();
            PerformZoom();
        }

        private void SetMainFormEventHandler()
        {
            Load += MainForm_Load;
            FormClosing += MainForm_FormClosing;
            Resize += MainForm_Resize;
            Activated += MainForm_Activated;
        }

        private void SetupUpdateable()
        {
            _updateable = new IUpdateContext[]
            {
                hqPanel, missionPanel, kdockPanel, ndockPanel, materialHistoryPanel, mainFleetPanel, Notifier
            };
            var context = new UpdateContext(Sniffer, Config, () => _main.Step);
            foreach (var updateable in _updateable)
                updateable.Context = context;
            _timers = new IUpdateTimers[] {missionPanel, kdockPanel, ndockPanel, mainFleetPanel};
        }

        private void SetScaleFactorOfDpiScaling()
        {
            var autoScaleDimensions = new SizeF(6f, 12f); // AutoScaleDimensionの初期値
            Scaler.Factor = new SizeF(CurrentAutoScaleDimensions.Width / autoScaleDimensions.Width,
                CurrentAutoScaleDimensions.Height / autoScaleDimensions.Height);
        }

        private void SetupQuestPanel()
        {
            int prevHeight = questPanel.Height;
            questPanel.CreateLabels(Config.QuestLines, labelQuest_DoubleClick);
            Height += questPanel.Height - prevHeight;
        }

        public void UpdateInfo(Sniffer.Update update)
        {
            if (update == Sniffer.Update.Start)
            {
                hqPanel.Login.Visible = false;
                mainFleetPanel.Start();
                _started = true;
                Notifier.StopAllRepeat();
                return;
            }
            if (!_started)
                return;
            if (_main.Step.Now == DateTime.MinValue)
                _main.Step.SetNow();
            if ((update & Sniffer.Update.Item) != 0)
                UpdateItemInfo();
            if ((update & Sniffer.Update.Timer) != 0)
                UpdateTimers();
            if ((update & Sniffer.Update.NDock) != 0)
                UpdateNDocLabels();
            if ((update & Sniffer.Update.Mission) != 0)
                UpdateMissionLabels();
            if ((update & Sniffer.Update.QuestList) != 0)
                UpdateQuestList();
            if ((update & Sniffer.Update.Ship) != 0)
                UpdateShipInfo();
            if ((update & Sniffer.Update.Battle) != 0)
                UpdateBattleInfo();
            if ((update & Sniffer.Update.Cell) != 0)
                UpdateCellInfo();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SuppressActivate.Start();
            RestoreLocation();
            if (Config.HideOnMinimized && WindowState == FormWindowState.Minimized)
                ShowInTaskbar = false;
            if (Config.ShowHpInPercent)
                mainFleetPanel.ToggleHpPercent();
            if (Config.ShipList.Visible)
                _listFormGroup.Show();
            _main.CheckVersionUpMain(mainFleetPanel.Guide);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Config.ExitSilently)
            {
                using var dialog = new ConfirmDialog();
                if (dialog.ShowDialog(this) != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
            _listFormGroup.Close();
            Config.Location = (WindowState == FormWindowState.Normal ? Bounds : RestoreBounds).Location;
            Config.ShowHpInPercent = mainFleetPanel.ShowHpInPercent;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (_listFormGroup == null) // DPIが100%でないときにInitializeComponentから呼ばれるので
                return;
            SuppressActivate.Start();
            if (WindowState == FormWindowState.Minimized)
            {
                if (Config.HideOnMinimized)
                    ShowInTaskbar = false;
            }
            _listFormGroup.Main.ChangeWindowState(WindowState);
        }

        public readonly TimeOutChecker SuppressActivate = new TimeOutChecker();

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (SuppressActivate.Check())
                return;
            if (NeedRaise)
                RaiseBothWindows();
        }

        private bool NeedRaise => _listFormGroup.Visible && WindowState != FormWindowState.Minimized;

        private void RaiseBothWindows()
        {
            _listFormGroup.Main.Owner = null;
            Owner = _listFormGroup.Main;
            BringToFront();
            Owner = null;
        }

        public class TimeOutChecker
        {
            private DateTime _lastCheck;
            private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(500);

            public void Start()
            {
                _lastCheck = DateTime.Now;
            }

            public bool Check()
            {
                var now = DateTime.Now;
                var last = _lastCheck;
                _lastCheck = now;
                return now - last < _timeout;
            }
        }

        private void notifyIconMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            NotifyIconOpenToolStripMenuItem_Click(sender, e);
        }

        private void NotifyIconOpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            TopMost = _listFormGroup.TopMost = Config.TopMost; // 最前面に表示されなくなることがあるのを回避する
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _main.ShowConfigDialog();
        }

        private void PerformZoom()
        {
            if (Config.Zoom == 100)
            {
                ShipLabel.Name.BaseFont = Font;
                ShipLabel.Name.LatinFont = LatinFont();
                return;
            }
            var prev = CurrentAutoScaleDimensions;
            foreach (var control in new Control[]
            {
                this, mainFleetPanel.Guide, hqPanel.Login,
                contextMenuStripMain
            }.Concat(_main.Controls))
            {
                control.Font = ZoomFont(control.Font);
            }
            _listFormGroup.Font = ZoomFont(_listFormGroup.Font);
            foreach (var toolTip in new[] {_toolTip, _tooltipCopy})
            {
                toolTip.Font = ZoomFont(toolTip.Font);
            }
            ShipLabel.Name.BaseFont = Font;
            ShipLabel.Name.LatinFont = LatinFont();
            var cur = CurrentAutoScaleDimensions;
            Scaler.Factor = Scaler.Scale(cur.Width / prev.Width, cur.Height / prev.Height);
        }

        private Font ZoomFont(Font font)
        {
            return new Font(font.FontFamily, font.Size * Config.Zoom / 100);
        }

        private Font LatinFont()
        {
            return new Font("Tahoma", 8f * Config.Zoom / 100);
        }

        private void RestoreLocation()
        {
            if (Config.Location.X == int.MinValue)
                return;
            if (IsTitleBarOnAnyScreen(Config.Location))
                Location = Config.Location;
        }

        public void ApplyConfig()
        {
            if (TopMost != Config.TopMost)
                TopMost = _listFormGroup.TopMost = Config.TopMost;
            hqPanel.Update();
            labelAkashiRepair.Visible = labelAkashiRepairTimer.Visible = Config.UsePresetAkashi;
        }

        public static bool IsTitleBarOnAnyScreen(Point location)
        {
            var rect = new Rectangle(
                new Point(location.X + SystemInformation.IconSize.Width + SystemInformation.HorizontalFocusThickness,
                    location.Y + SystemInformation.CaptionHeight), new Size(60, 1));
            return Screen.AllScreens.Any(screen => screen.WorkingArea.Contains(rect));
        }

        private void ShowShipOnShipList(int id)
        {
            if (!_listFormGroup.Visible)
                return;
            _listFormGroup.ShowShip(id);
        }

        public void UpdateItemInfo()
        {
            hqPanel.Update();
            Notifier.NotifyShipItemCount();
            materialHistoryPanel.Update();
            if (_listFormGroup.Visible)
                _listFormGroup.UpdateList();
        }

        private void UpdateShipInfo()
        {
            mainFleetPanel.Update();
            Notifier.NotifyDamagedShip();
            UpdateChargeInfo();
            UpdateRepairList();
            UpdateMissionLabels();
            if (_listFormGroup.Visible)
                _listFormGroup.UpdateList();
        }

        private void UpdateBattleInfo()
        {
            _listFormGroup.UpdateBattleResult();
            _listFormGroup.UpdateAirBattleResult();
            mainFleetPanel.UpdateBattleInfo();
        }

        private void UpdateCellInfo()
        {
            _listFormGroup.UpdateCellInfo();
        }

        private void UpdateChargeInfo()
        {
            mainFleetPanel.UpdateChargeInfo();
        }

        private void UpdateNDocLabels()
        {
            ndockPanel.Update();
        }

        private void UpdateMissionLabels()
        {
            missionPanel.Update();
        }

        public void UpdateTimers()
        {
            foreach (var timer in _timers)
                timer.UpdateTimers();
        }

        private void UpdateRepairList()
        {
            panelRepairList.SetRepairList(Sniffer.RepairList);
            _toolTip.SetToolTip(label31, new RepairShipCount(Sniffer.RepairList).ToString());
        }

        private void UpdateQuestList()
        {
            questPanel.Update(Sniffer.Quests);
            labelQuestCount.Text = Sniffer.Quests.Length.ToString();
            Notifier.NotifyQuestComplete();
        }

        private void FlashWindow()
        {
            Win32API.FlashWindow(Handle);
        }

        private void ShowTaster(string title, string message)
        {
            notifyIconMain.ShowBalloonTip(20000, title, message, ToolTipIcon.Info);
        }

        private void PlaySound(string file, int volume)
        {
            SoundPlayer.PlaySound(Handle, file, volume);
        }

        protected override void WndProc(ref Message m)
        {
            SoundPlayer.CloseSound(m);
            base.WndProc(ref m);
        }

        private void labelRepairListButton_Click(object sender, EventArgs e)
        {
            if (panelRepairList.Visible)
            {
                panelRepairList.Visible = false;
                dropDownButtonRepairList.BackColor = DefaultBackColor;
            }
            else
            {
                panelRepairList.Visible = true;
                panelRepairList.BringToFront();
                dropDownButtonRepairList.BackColor = CustomColors.ActiveButtonColor;
            }
        }

        private void panelRepairList_Click(object sender, EventArgs e)
        {
            panelRepairList.Visible = false;
            dropDownButtonRepairList.BackColor = DefaultBackColor;
        }

        private void ShipListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _listFormGroup.ShowOrCreate();
        }

        private void LogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://localhost:" + Config.Proxy.Listen + "/");
        }

        private void labelClearQuest_Click(object sender, EventArgs e)
        {
            Sniffer.ClearQuests();
            UpdateQuestList();
        }

        private void labelClearQuest_MouseDown(object sender, MouseEventArgs e)
        {
            labelClearQuest.BackColor = CustomColors.ActiveButtonColor;
        }

        private void labelClearQuest_MouseUp(object sender, MouseEventArgs e)
        {
            labelClearQuest.BackColor = DefaultBackColor;
        }

        private void labelQuest_DoubleClick(object sender, EventArgs e)
        {
            var label = (Label)sender;
            if (string.IsNullOrEmpty(label.Text))
                return;
            Clipboard.SetText(label.Text);
            _tooltipCopy.Active = true;
            _tooltipCopy.Show("コピーしました。", label);
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                _tooltipCopy.Active = false;
            });
        }

        private void CaptureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _main.StartCapture();
        }
    }
}