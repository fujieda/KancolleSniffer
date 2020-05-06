using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using KancolleSniffer.Forms;
using KancolleSniffer.Notification;
using KancolleSniffer.Util;
using KancolleSniffer.View;
using KancolleSniffer.View.MainWindow;

namespace KancolleSniffer
{
    public class MainWindow
    {
        private readonly ResizableToolTip _toolTip = new ResizableToolTip();
        private readonly ResizableToolTip _tooltipCopy = new ResizableToolTip {ShowAlways = false, AutomaticDelay = 0};
        private readonly ListFormGroup _listFormGroup;
        private readonly ContextMenuMain _contextMenuMain = new ContextMenuMain();
        private readonly ContextMenuNotifyIcon _contextMenuNotifyIcon = new ContextMenuNotifyIcon();
        private readonly Components _c;

        private IEnumerable<IUpdateContext> _updateable;
        private IEnumerable<IUpdateTimers> _timers;
        private Main _main;

        public Sniffer Sniffer { get; private set; }
        public Config Config { get; private set; }
        public Label PlayLogSign => _c.hqPanel.PlayLog;
        public Notifier Notifier { get; }
        public Form Form { get; }

        private class Components
        {
            // ReSharper disable InconsistentNaming
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public NotifyIcon notifyIconMain { get; set; }
            public HqPanel hqPanel { get; set; }
            public MainFleetPanel mainFleetPanel { get; set; }
            public Label labelNDockCaption { get; set; }
            public NDockPanel ndockPanel { get; set; }
            public KDockPanel kdockPanel { get; set; }
            public Label labelMissionCaption { get; set; }
            public MissionPanel missionPanel { get; set; }
            public Label labelMaterialCaption { get; set; }
            public DropDownButton dropDownButtonMaterialHistory { get; set; }
            public MaterialHistoryPanel materialHistoryPanel { get; set; }
            public Label labelQuestCount { get; set; }
            public QuestPanel questPanel { get; set; }
            public Label labelAkashiRepair { get; set; }
            public Label labelAkashiRepairTimer { get; set; }
            public Label labelRepairListCaption { get; set; }
            public Label dropDownButtonRepairList { get; set; }
            public RepairListPanel panelRepairList { get; set; }
            // ReSharper restore InconsistentNaming
            // ReSharper restore UnusedAutoPropertyAccessor.Local
        }

        public MainWindow(Main main, Form form)
        {
            _c = GetComponents(form);
            Form = form;
            _c.notifyIconMain.ContextMenuStrip = _contextMenuNotifyIcon;
            Form.ContextMenuStrip = _contextMenuMain;
            Form.ContextMenuStrip = _contextMenuMain;
            SetupMain(main);
            _listFormGroup = new ListFormGroup(this);
            Notifier = new Notifier(FlashWindow, ShowTaster, PlaySound);
            SetupView();
        }

        private Components GetComponents(Form form)
        {
            var r = new Components();
            foreach (var prop in typeof(Components).GetProperties())
            {
                // ReSharper disable once PossibleNullReferenceException
                prop.SetValue(r,
                    form.GetType().GetField(prop.Name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form));
            }
            return r;
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
            SetEventHandlers();
            _c.mainFleetPanel.AkashiRepairTimer = _c.labelAkashiRepairTimer;
            _c.mainFleetPanel.ShowShipOnList = ShowShipOnShipList;
            _c.panelRepairList.CreateLabels(panelRepairList_Click);
            _c.ndockPanel.SetClickHandler(_c.labelNDockCaption);
            _c.missionPanel.SetClickHandler(_c.labelMissionCaption);
            _c.materialHistoryPanel.SetClickHandler(_c.labelMaterialCaption, _c.dropDownButtonMaterialHistory);
            SetupUpdateable();
            PerformZoom();
        }

        private void SetEventHandlers()
        {
            SetMainFormEventHandler();
            SetContextMenuMainEventHandler();
            SetContextMenuNotifyIconEventHandler();
            SetNotifyIconEventHandler();
            SetRepairListEventHandler();
        }

        private void SetMainFormEventHandler()
        {
            Form.Load += MainForm_Load;
            Form.FormClosing += MainForm_FormClosing;
            Form.Resize += MainForm_Resize;
            Form.Activated += MainForm_Activated;
        }

        private void SetContextMenuMainEventHandler()
        {
            _contextMenuMain.SetClickHandlers(
                _listFormGroup.ShowOrCreate,
                _main.ShowReport,
                _main.StartCapture,
                _main.ShowConfigDialog,
                Form.Close);
        }

        private void SetContextMenuNotifyIconEventHandler()
        {
            _contextMenuNotifyIcon.SetEventHandlers(RevertFromIcon, Form.Close);
        }

        private void SetNotifyIconEventHandler()
        {
            _c.notifyIconMain.MouseDoubleClick += notifyIconMain_MouseDoubleClick;
        }

        private void SetRepairListEventHandler()
        {
            _c.labelRepairListCaption.Click += labelRepairListButton_Click;
            _c.dropDownButtonRepairList.Click += labelRepairListButton_Click;
        }

        private void SetupUpdateable()
        {
            _updateable = new IUpdateContext[]
            {
                _c.hqPanel, _c.missionPanel, _c.kdockPanel, _c.ndockPanel, _c.materialHistoryPanel, _c.mainFleetPanel,
                Notifier
            };
            var context = new UpdateContext(Sniffer, Config, () => _main.Step);
            foreach (var updateable in _updateable)
                updateable.Context = context;
            _timers = new IUpdateTimers[] {_c.missionPanel, _c.kdockPanel, _c.ndockPanel, _c.mainFleetPanel};
        }

        private void SetScaleFactorOfDpiScaling()
        {
            var autoScaleDimensions = new SizeF(6f, 12f); // AutoScaleDimensionの初期値
            Scaler.Factor = new SizeF(Form.CurrentAutoScaleDimensions.Width / autoScaleDimensions.Width,
                Form.CurrentAutoScaleDimensions.Height / autoScaleDimensions.Height);
        }

        private void SetupQuestPanel()
        {
            var prevHeight = _c.questPanel.Height;
            _c.questPanel.CreateLabels(Config.QuestLines);
            Form.Height += _c.questPanel.Height - prevHeight;
        }

        public void UpdateInfo(Sniffer.Update update)
        {
            if (update == Sniffer.Update.Start)
            {
                _c.hqPanel.Login.Visible = false;
                _c.mainFleetPanel.Start();
                Notifier.StopAllRepeat();
                return;
            }
            if (!Sniffer.Started)
                return;
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
            if (Config.HideOnMinimized && Form.WindowState == FormWindowState.Minimized)
                Form.ShowInTaskbar = false;
            if (Config.ShowHpInPercent)
                _c.mainFleetPanel.ToggleHpPercent();
            if (Config.ShipList.Visible)
                _listFormGroup.Show();
            _main.CheckVersionUpMain(_c.mainFleetPanel.Guide);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!Config.ExitSilently)
            {
                using var dialog = new ConfirmDialog();
                if (dialog.ShowDialog(Form) != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
            _listFormGroup.Close();
            Config.Location = (Form.WindowState == FormWindowState.Normal ? Form.Bounds : Form.RestoreBounds).Location;
            Config.ShowHpInPercent = _c.mainFleetPanel.ShowHpInPercent;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (_listFormGroup == null) // DPIが100%でないときにInitializeComponentから呼ばれるので
                return;
            SuppressActivate.Start();
            if (Form.WindowState == FormWindowState.Minimized)
            {
                if (Config.HideOnMinimized)
                    Form.ShowInTaskbar = false;
            }
            _listFormGroup.Main.ChangeWindowState(Form.WindowState);
        }

        public readonly TimeOutChecker SuppressActivate = new TimeOutChecker();

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (SuppressActivate.Check())
                return;
            if (NeedRaise)
                RaiseBothWindows();
        }

        private bool NeedRaise => _listFormGroup.Visible && Form.WindowState != FormWindowState.Minimized;

        private void RaiseBothWindows()
        {
            _listFormGroup.Main.Owner = null;
            Form.Owner = _listFormGroup.Main;
            Form.BringToFront();
            Form.Owner = null;
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

        private void RevertFromIcon()
        {
            Form.ShowInTaskbar = true;
            Form.WindowState = FormWindowState.Normal;
            Form.TopMost = _listFormGroup.TopMost = Config.TopMost; // 最前面に表示されなくなることがあるのを回避する
        }

        private void notifyIconMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            RevertFromIcon();
        }

        private void PerformZoom()
        {
            if (Config.Zoom == 100)
            {
                ShipLabel.Name.BaseFont = Form.Font;
                ShipLabel.Name.LatinFont = LatinFont();
                return;
            }
            var prev = Form.CurrentAutoScaleDimensions;
            foreach (var control in new Control[]
            {
                Form, _c.mainFleetPanel.Guide, _c.hqPanel.Login,
                _contextMenuMain
            }.Concat(_main.Controls))
            {
                control.Font = ZoomFont(control.Font);
            }
            _listFormGroup.Font = ZoomFont(_listFormGroup.Font);
            foreach (var toolTip in new[] {_toolTip, _tooltipCopy})
            {
                toolTip.Font = ZoomFont(toolTip.Font);
            }
            ShipLabel.Name.BaseFont = Form.Font;
            ShipLabel.Name.LatinFont = LatinFont();
            var cur = Form.CurrentAutoScaleDimensions;
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
                Form.Location = Config.Location;
        }

        public void ApplyConfig()
        {
            if (Form.TopMost != Config.TopMost)
                Form.TopMost = _listFormGroup.TopMost = Config.TopMost;
            _c.hqPanel.Update();
            _c.labelAkashiRepair.Visible = _c.labelAkashiRepairTimer.Visible = Config.UsePresetAkashi;
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
            _c.hqPanel.Update();
            Notifier.NotifyShipItemCount();
            _c.materialHistoryPanel.Update();
            if (_listFormGroup.Visible)
                _listFormGroup.UpdateList();
        }

        private void UpdateShipInfo()
        {
            _c.mainFleetPanel.Update();
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
            _c.mainFleetPanel.UpdateBattleInfo();
        }

        private void UpdateCellInfo()
        {
            _listFormGroup.UpdateCellInfo();
        }

        private void UpdateChargeInfo()
        {
            _c.mainFleetPanel.UpdateChargeInfo();
        }

        private void UpdateNDocLabels()
        {
            _c.ndockPanel.Update();
        }

        private void UpdateMissionLabels()
        {
            _c.missionPanel.Update();
        }

        public void UpdateTimers()
        {
            foreach (var timer in _timers)
                timer.UpdateTimers();
        }

        private void UpdateRepairList()
        {
            _c.panelRepairList.SetRepairList(Sniffer.RepairList);
            _toolTip.SetToolTip(_c.labelRepairListCaption, new RepairShipCount(Sniffer.RepairList).ToString());
        }

        private void UpdateQuestList()
        {
            _c.questPanel.Update(Sniffer.Quests);
            _c.labelQuestCount.Text = Sniffer.Quests.Length.ToString();
            Notifier.NotifyQuestComplete();
        }

        private void FlashWindow()
        {
            Win32API.FlashWindow(Form.Handle);
        }

        private void ShowTaster(string title, string message)
        {
            _c.notifyIconMain.ShowBalloonTip(20000, title, message, ToolTipIcon.Info);
        }

        private void PlaySound(string file, int volume)
        {
            SoundPlayer.PlaySound(Form.Handle, file, volume);
        }

        private void labelRepairListButton_Click(object sender, EventArgs e)
        {
            if (_c.panelRepairList.Visible)
            {
                _c.panelRepairList.Visible = false;
                _c.dropDownButtonRepairList.BackColor = Control.DefaultBackColor;
            }
            else
            {
                _c.panelRepairList.Visible = true;
                _c.panelRepairList.BringToFront();
                _c.dropDownButtonRepairList.BackColor = CustomColors.ActiveButtonColor;
            }
        }

        private void panelRepairList_Click(object sender, EventArgs e)
        {
            _c.panelRepairList.Visible = false;
            _c.dropDownButtonRepairList.BackColor = Control.DefaultBackColor;
        }
    }
}