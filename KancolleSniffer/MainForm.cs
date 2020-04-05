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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DynaJson;
using KancolleSniffer.Log;
using KancolleSniffer.Model;
using KancolleSniffer.Net;
using KancolleSniffer.Util;
using KancolleSniffer.View;
using Microsoft.CSharp.RuntimeBinder;
using static System.Math;
using Clipboard = KancolleSniffer.Util.Clipboard;
using Timer = System.Windows.Forms.Timer;

namespace KancolleSniffer
{
    public partial class MainForm : Form
    {
        private readonly ConfigDialog _configDialog;
        private readonly ProxyManager _proxyManager;
        private readonly ResizableToolTip _toolTip = new ResizableToolTip();
        private readonly ResizableToolTip _tooltipCopy = new ResizableToolTip {ShowAlways = false, AutomaticDelay = 0};
        private int _currentFleet;
        private bool _combinedFleet;
        private readonly MainShipLabels _mainLabels = new MainShipLabels();
        private readonly ListFormGroup _listFormGroup;

        private readonly NotificationManager _notificationManager;
        private bool _started;
        private bool _timerEnabled;
        private string _debugLogFile;
        private IEnumerator<string> _playLog;
        private DateTime _prev, _now;
        private IEnumerable<IUpdateContext> _updateable;
        private IEnumerable<IUpdateTimers> _timers;

        private readonly ErrorDialog _errorDialog = new ErrorDialog();
        private readonly ErrorLog _errorLog;

        public Sniffer Sniffer { get; } = new Sniffer();
        public Config Config { get; } = new Config();

        public interface INotifySubmitter
        {
            void Flash();
            void Enqueue(string key, string subject);
        }

        public MainForm()
        {
            InitializeComponent();
            HttpProxy.AfterSessionComplete += HttpProxy_AfterSessionComplete;
            Config.Load();
            _configDialog = new ConfigDialog(this);
            _listFormGroup = new ListFormGroup(this);
            _notificationManager = new NotificationManager(Alarm);
            SetupView();
            _proxyManager = new ProxyManager(this);
            _proxyManager.UpdatePacFile();
            _errorLog = new ErrorLog(Sniffer);
            LoadData();
            Sniffer.RepeatingTimerController = new RepeatingTimerController(this);
        }

        private void SetupView()
        {
            SetScaleFactorOfDpiScaling();
            SetupFleetClick();
            CreateMainLabels();
            labelPresetAkashiTimer.BackColor = CustomColors.ColumnColors.Bright;
            SetupQuestPanel();
            panelRepairList.CreateLabels(panelRepairList_Click);
            ndockPanel.SetClickHandler(labelNDock_Click);
            missionPanel.SetClickHandler(labelMission_Click);
            materialHistoryPanel.SetClickHandler(labelMaterialCaption, labelMaterialHistoryButton);
            SetupUpdateable();
            PerformZoom();
        }

        private void SetupUpdateable()
        {
            _updateable = new IUpdateContext[] {hqPanel, missionPanel, kdockPanel, ndockPanel, materialHistoryPanel};
            var context = new UpdateContext(Sniffer, Config, new NotifySubmitter(_notificationManager), () => _now);
            foreach (var updateable in _updateable)
                updateable.Context = context;
            _timers = new IUpdateTimers[] {missionPanel, kdockPanel, ndockPanel};
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

        private void CreateMainLabels()
        {
            _mainLabels.CreateAllShipLabels(new MainShipPanels
            {
                PanelShipInfo = panelShipInfo,
                Panel7Ships = panel7Ships,
                PanelCombinedFleet = panelCombinedFleet
            }, ShowShipOnShipList);
        }

        private class NotifySubmitter : INotifySubmitter
        {
            private readonly NotificationManager _manager;

            public NotifySubmitter(NotificationManager manager)
            {
                _manager = manager;
            }

            public void Flash()
            {
                _manager.Flash();
            }

            public void Enqueue(string key, string subject)
            {
                _manager.Enqueue(key, subject);
            }
        }

        /// <summary>
        /// パネルのz-orderがくるうのを避ける
        /// https://stackoverflow.com/a/5777090/1429506
        /// </summary>
        private void MainForm_Shown(object sender, EventArgs e)
        {
            // ReSharper disable once NotAccessedVariable
            IntPtr handle;
            foreach (var panel in new[] {panelShipInfo, panel7Ships, panelCombinedFleet})
                // ReSharper disable once RedundantAssignment
                handle = panel.Handle;
        }

        private readonly FileSystemWatcher _watcher = new FileSystemWatcher
        {
            Path = AppDomain.CurrentDomain.BaseDirectory,
            NotifyFilter = NotifyFilters.LastWrite
        };

        private readonly Timer _watcherTimer = new Timer {Interval = 1000};

        private void LoadData()
        {
            var target = "";
            Sniffer.LoadState();
            _watcher.SynchronizingObject = this;
            _watcherTimer.Tick += (sender, ev) =>
            {
                _watcherTimer.Stop();
                switch (target)
                {
                    case "status.xml":
                        Sniffer.LoadState();
                        break;
                    case "TP.csv":
                        Sniffer.AdditionalData.LoadTpSpec();
                        break;
                }
            };
            _watcher.Changed += (sender, ev) =>
            {
                target = ev.Name;
                _watcherTimer.Stop();
                _watcherTimer.Start();
            };
            _watcher.EnableRaisingEvents = true;
        }

        private class RepeatingTimerController : Sniffer.IRepeatingTimerController
        {
            private readonly NotificationManager _manager;
            private readonly Config _config;

            public RepeatingTimerController(MainForm main)
            {
                _manager = main._notificationManager;
                _config = main.Config;
            }

            public void Stop(string key)
            {
                _manager.StopRepeat(key,
                    (key == "入渠終了" || key == "遠征終了") &&
                    (_config.Notifications[key].Flags & NotificationType.Cont) != 0);
            }

            public void Stop(string key, int fleet) => _manager.StopRepeat(key, fleet);

            public void Suspend(string exception = null) => _manager.SuspendRepeat(exception);

            public void Resume() => _manager.ResumeRepeat();
        }

        private void HttpProxy_AfterSessionComplete(HttpProxy.Session session)
        {
            BeginInvoke(new Action<HttpProxy.Session>(ProcessRequest), session);
        }

        private void ProcessRequest(HttpProxy.Session session)
        {
            var url = session.Request.PathAndQuery;
            if (!url.Contains("kcsapi/"))
                return;
            var request = session.Request.BodyAsString;
            var response = session.Response.BodyAsString;
            Privacy.Remove(ref url, ref request, ref response);
            if (response == null || !response.StartsWith("svdata="))
            {
                WriteDebugLog(url, request, response);
                return;
            }
            response = UnEscapeString(response.Remove(0, "svdata=".Length));
            WriteDebugLog(url, request, response);
            ProcessRequestMain(url, request, response);
        }

        private void ProcessRequestMain(string url, string request, string response)
        {
            try
            {
                UpdateInfo(Sniffer.Sniff(url, request, JsonObject.Parse(response)));
                _errorLog.CheckBattleApi(url, request, response);
            }

            catch (RuntimeBinderException e)
            {
                if (_errorDialog.ShowDialog(this,
                        "艦これに仕様変更があったか、受信内容が壊れています。",
                        _errorLog.GenerateErrorLog(url, request, response, e.ToString())) == DialogResult.Abort)
                    Exit();
            }
            catch (LogIOException e)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (_errorDialog.ShowDialog(this, e.Message, e.InnerException.ToString()) == DialogResult.Abort)
                    Exit();
            }
            catch (BattleResultError)
            {
                if (_errorDialog.ShowDialog(this, "戦闘結果の計算に誤りがあります。",
                        _errorLog.GenerateBattleErrorLog()) == DialogResult.Abort)
                    Exit();
            }
            catch (Exception e)
            {
                if (_errorDialog.ShowDialog(this, "エラーが発生しました。",
                        _errorLog.GenerateErrorLog(url, request, response, e.ToString())) == DialogResult.Abort)
                    Exit();
            }
        }

        private void Exit()
        {
            _proxyManager.Shutdown();
            Environment.Exit(1);
        }

        private void WriteDebugLog(string url, string request, string response)
        {
            if (_debugLogFile != null)
            {
                File.AppendAllText(_debugLogFile,
                    $"date: {DateTime.Now:g}\nurl: {url}\nrequest: {request}\nresponse: {response ?? "(null)"}\n");
            }
        }

        private string UnEscapeString(string s)
        {
            try
            {
                var rx = new Regex(@"\\[uU]([0-9A-Fa-f]{4})");
                return rx.Replace(s,
                    match => ((char)int.Parse(match.Value.Substring(2), NumberStyles.HexNumber)).ToString());
            }
            catch (ArgumentException)
            {
                return s;
            }
        }

        private void UpdateInfo(Sniffer.Update update)
        {
            if (update == Sniffer.Update.Start)
            {
                hqPanel.Login.Visible = false;
                linkLabelGuide.Visible = false;
                _started = true;
                _notificationManager.StopAllRepeat();
                return;
            }
            if (!_started)
                return;
            if (_now == DateTime.MinValue)
                _now = DateTime.Now;
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
                _mainLabels.ToggleHpPercent();
            if (Config.ShipList.Visible)
                _listFormGroup.Show();
            ApplyConfig();
            ApplyDebugLogSetting();
            ApplyLogSetting();
            ApplyProxySetting();
            CheckVersionUp((current, latest) =>
            {
                if (latest == current)
                    return;
                linkLabelGuide.Text = $"バージョン{latest}があります。";
                linkLabelGuide.LinkArea = new LinkArea(0, linkLabelGuide.Text.Length);
                linkLabelGuide.Click += (obj, ev) =>
                {
                    Process.Start("https://ja.osdn.net/rel/kancollesniffer/" + latest);
                };
            });
        }

        public async void CheckVersionUp(Action<string, string> action)
        {
            var current = string.Join(".", Application.ProductVersion.Split('.').Take(2));
            try
            {
                var latest = (await new WebClient().DownloadStringTaskAsync("http://kancollesniffer.osdn.jp/version"))
                    .TrimEnd();
                try
                {
                    action(current, latest);
                }
                catch (InvalidOperationException)
                {
                }
            }
            catch (WebException)
            {
            }
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
            Sniffer.FlashLog();
            Config.Location = (WindowState == FormWindowState.Normal ? Bounds : RestoreBounds).Location;
            Config.ShowHpInPercent = _mainLabels.ShowHpInPercent;
            Config.Save();
            Sniffer.SaveState();
            _proxyManager.Shutdown();
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

        public TimeOutChecker SuppressActivate = new TimeOutChecker();

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
            if (_configDialog.ShowDialog(this) == DialogResult.OK)
            {
                Config.Save();
                ApplyConfig();
                StopRepeatingTimer(_configDialog.RepeatSettingsChanged);
            }
        }

        private void StopRepeatingTimer(IEnumerable<string> names)
        {
            foreach (var name in names)
                _notificationManager.StopRepeat(name);
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
                this, linkLabelGuide, hqPanel.Login,
                _configDialog, _configDialog.NotificationConfigDialog,
                contextMenuStripMain, _errorDialog
            })
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

        private void ApplyConfig()
        {
            if (TopMost != Config.TopMost)
                TopMost = _listFormGroup.TopMost = Config.TopMost;
            Sniffer.ShipCounter.Margin = Config.MarginShips;
            Sniffer.ItemCounter.Margin = Config.MarginEquips;
            hqPanel.Update();
            Sniffer.Achievement.ResetHours = Config.ResetHours;
            labelAkashiRepair.Visible = labelAkashiRepairTimer.Visible =
                labelPresetAkashiTimer.Visible = Config.UsePresetAkashi;
            Sniffer.WarnBadDamageWithDameCon = Config.WarnBadDamageWithDameCon;
        }

        public void ApplyDebugLogSetting()
        {
            _debugLogFile = Config.DebugLogging ? Config.DebugLogFile : null;
        }

        public bool ApplyProxySetting()
        {
            return _proxyManager.ApplyConfig();
        }

        public void ApplyLogSetting()
        {
            LogServer.OutputDir = Config.Log.OutputDir;
            LogServer.LogProcessor = new LogProcessor(Sniffer.Material.MaterialHistory, Sniffer.MapDictionary);
            Sniffer.EnableLog(Config.Log.On ? LogType.All : LogType.None);
            Sniffer.MaterialLogInterval = Config.Log.MaterialLogInterval;
            Sniffer.LogOutputDir = Config.Log.OutputDir;
        }

        public static bool IsTitleBarOnAnyScreen(Point location)
        {
            var rect = new Rectangle(
                new Point(location.X + SystemInformation.IconSize.Width + SystemInformation.HorizontalFocusThickness,
                    location.Y + SystemInformation.CaptionHeight), new Size(60, 1));
            return Screen.AllScreens.Any(screen => screen.WorkingArea.Contains(rect));
        }

        private void timerMain_Tick(object sender, EventArgs e)
        {
            if (_timerEnabled)
            {
                try
                {
                    _now = DateTime.Now;
                    UpdateTimers();
                    NotifyTimers();
                    _prev = _now;
                }
                catch (Exception ex)
                {
                    if (_errorDialog.ShowDialog(this, "エラーが発生しました。", ex.ToString()) == DialogResult.Abort)
                        Exit();
                }
            }
            if (_playLog == null || _configDialog.Visible)
            {
                hqPanel.PlayLog.Visible = false;
                return;
            }
            PlayLog();
        }

        public void SetPlayLog(string file)
        {
            _playLog = File.ReadLines(file).GetEnumerator();
        }

        private void PlayLog()
        {
            var lines = new List<string>();
            foreach (var s in new[] {"url: ", "request: ", "response: "})
            {
                do
                {
                    if (!_playLog.MoveNext() || _playLog.Current == null)
                    {
                        hqPanel.PlayLog.Visible = false;
                        return;
                    }
                } while (!_playLog.Current.StartsWith(s));
                lines.Add(_playLog.Current.Substring(s.Length));
            }
            hqPanel.PlayLog.Visible = !hqPanel.PlayLog.Visible;
            ProcessRequestMain(lines[0], lines[1], lines[2]);
        }

        private void ShowShipOnShipList(object sender, EventArgs ev)
        {
            if (!_listFormGroup.Visible)
                return;
            var idx = (int)((Control)sender).Tag;
            var ship = (_combinedFleet
                ? Sniffer.Fleets[0].Ships.Concat(Sniffer.Fleets[1].Ships).ToArray()
                : Sniffer.Fleets[_currentFleet].Ships)[idx];
            if (!ship.Empty)
                _listFormGroup.ShowShip(ship.Id);
        }


        private void UpdateItemInfo()
        {
            hqPanel.Update();
            materialHistoryPanel.Update();
            if (_listFormGroup.Visible)
                _listFormGroup.UpdateList();
        }

        private void UpdateShipInfo()
        {
            SetCurrentFleet();
            SetCombined();
            UpdatePanelShipInfo();
            NotifyDamagedShip();
            UpdateChargeInfo();
            UpdateRepairList();
            UpdateMissionLabels();
            if (_listFormGroup.Visible)
                _listFormGroup.UpdateList();
        }

        private bool _inSortie;

        private void SetCurrentFleet()
        {
            var inSortie = Sniffer.InSortie;
            if (_inSortie || inSortie == -1)
            {
                _inSortie = inSortie != -1;
                return;
            }
            _inSortie = true;
            if (inSortie == 10)
            {
                _combinedFleet = true;
                _currentFleet = 0;
            }
            else
            {
                _combinedFleet = false;
                _currentFleet = inSortie;
            }
        }

        private bool _prevCombined;

        private void SetCombined()
        {
            if (Sniffer.IsCombinedFleet && !_prevCombined)
            {
                _combinedFleet = true;
                _currentFleet = 0;
            }
            _prevCombined = Sniffer.IsCombinedFleet;
        }

        private void UpdatePanelShipInfo()
        {
            var ships = Sniffer.Fleets[_currentFleet].ActualShips;
            panel7Ships.Visible = ships.Count == 7;
            _mainLabels.SetShipLabels(ships);
            ShowCombinedFleet();
            ShowCurrentFleetNumber();
            UpdateAkashiTimer();
            UpdateFighterPower(IsCombinedFighterPower);
            UpdateLoS();
            UpdateCondTimers();
        }

        private void ShowCombinedFleet()
        {
            if (!Sniffer.IsCombinedFleet)
                _combinedFleet = false;
            labelFleet1.Text = _combinedFleet ? CombinedName : "第一";
            panelCombinedFleet.Visible = _combinedFleet;
            if (_combinedFleet)
                _mainLabels.SetCombinedShipLabels(Sniffer.Fleets[0].ActualShips, Sniffer.Fleets[1].ActualShips);
        }

        private void ShowCurrentFleetNumber()
        {
            var labels = new[] {labelCheckFleet1, labelCheckFleet2, labelCheckFleet3, labelCheckFleet4};
            for (var i = 0; i < labels.Length; i++)
                labels[i].Visible = _currentFleet == i;
        }

        private bool IsCombinedFighterPower => _combinedFleet &&
                                               (Sniffer.Battle.BattleState == BattleState.None ||
                                                Sniffer.Battle.EnemyIsCombined);

        private string CombinedName
        {
            get
            {
                switch (Sniffer.Fleets[0].CombinedType)
                {
                    case CombinedType.Carrier:
                        return "機動";
                    case CombinedType.Surface:
                        return "水上";
                    case CombinedType.Transport:
                        return "輸送";
                    default:
                        return "連合";
                }
            }
        }

        private void NotifyDamagedShip()
        {
            _notificationManager.StopRepeat("大破警告");
            if (!Sniffer.BadlyDamagedShips.Any())
                return;
            SetNotification("大破警告", string.Join(" ", Sniffer.BadlyDamagedShips));
            _notificationManager.Flash();
        }

        private void UpdateFighterPower(bool combined)
        {
            var fleets = Sniffer.Fleets;
            var fp = combined
                ? fleets[0].FighterPower + fleets[1].FighterPower
                : fleets[_currentFleet].FighterPower;
            labelFighterPower.Text = fp.Min.ToString("D");
            var cr = combined
                ? fleets[0].ContactTriggerRate + fleets[1].ContactTriggerRate
                : fleets[_currentFleet].ContactTriggerRate;
            var text = "制空: " + (fp.Diff ? fp.RangeString : fp.Min.ToString()) +
                       $" 触接: {cr * 100:f1}";
            _toolTip.SetToolTip(labelFighterPower, text);
            _toolTip.SetToolTip(labelFighterPowerCaption, text);
        }

        private void UpdateLoS()
        {
            var fleet = Sniffer.Fleets[_currentFleet];
            labelLoS.Text = RoundDown(fleet.GetLineOfSights(1)).ToString("F1");
            var text = $"係数2: {RoundDown(fleet.GetLineOfSights(2)):F1}\r\n" +
                       $"係数3: {RoundDown(fleet.GetLineOfSights(3)):F1}\r\n" +
                       $"係数4: {RoundDown(fleet.GetLineOfSights(4)):F1}";
            _toolTip.SetToolTip(labelLoS, text);
            _toolTip.SetToolTip(labelLoSCaption, text);
        }

        private double RoundDown(double number)
        {
            return Floor(number * 10) / 10.0;
        }

        private void UpdateBattleInfo()
        {
            ResetBattleInfo();
            _listFormGroup.UpdateBattleResult();
            _listFormGroup.UpdateAirBattleResult();
            if (Sniffer.Battle.BattleState == BattleState.None)
                return;
            panelBattleInfo.BringToFront();
            var battle = Sniffer.Battle;
            labelFormation.Text = new[] {"同航戦", "反航戦", "T字有利", "T字不利"}[battle.Formation[2] - 1];
            UpdateBattleFighterPower();
            if ((Config.Spoilers & Spoiler.ResultRank) != 0)
                ShowResultRank();
        }

        private void UpdateCellInfo()
        {
            _listFormGroup.UpdateCellInfo();
        }

        private void ResetBattleInfo()
        {
            labelFormation.Text = "";
            labelEnemyFighterPower.Text = "";
            labelFighterPower.ForeColor = DefaultForeColor;
            labelFighterPowerCaption.Text = "制空";
            labelResultRank.Text = "判定";
            panelBattleInfo.Visible = Sniffer.Battle.BattleState != BattleState.None;
        }

        private void UpdateBattleFighterPower()
        {
            UpdateEnemyFighterPower();
            var battle = Sniffer.Battle;
            labelFighterPower.ForeColor = AirControlLevelColor(battle);
            labelFighterPowerCaption.Text = AirControlLevelString(battle);
            if (battle.BattleState == BattleState.AirRaid)
            {
                UpdateAirRaidFighterPower();
            }
            else
            {
                UpdateFighterPower(Sniffer.IsCombinedFleet && battle.EnemyIsCombined);
            }
        }

        private void UpdateEnemyFighterPower()
        {
            var fp = Sniffer.Battle.EnemyFighterPower;
            labelEnemyFighterPower.Text = fp.AirCombat + fp.UnknownMark;
            var toolTip = fp.AirCombat == fp.Interception ? "" : "防空: " + fp.Interception + fp.UnknownMark;
            _toolTip.SetToolTip(labelEnemyFighterPower, toolTip);
            _toolTip.SetToolTip(labelEnemyFighterPowerCaption, toolTip);
        }

        private void UpdateAirRaidFighterPower()
        {
            var fp = Sniffer.Battle.FighterPower;
            labelFighterPower.Text = fp.Min.ToString();
            var toolTop = fp.Diff ? fp.RangeString : "";
            _toolTip.SetToolTip(labelFighterPower, toolTop);
            _toolTip.SetToolTip(labelFighterPowerCaption, toolTop);
        }

        private static Color AirControlLevelColor(BattleInfo battle)
        {
            return new[]
                {DefaultForeColor, DefaultForeColor, CUDColors.Blue, CUDColors.Green, CUDColors.Orange, CUDColors.Red}[
                battle.BattleState == BattleState.Night ? 0 : battle.AirControlLevel + 1];
        }

        private static string AirControlLevelString(BattleInfo battle)
        {
            return new[] {"制空", "拮抗", "確保", "優勢", "劣勢", "喪失"}[
                battle.BattleState == BattleState.Night ? 0 : battle.AirControlLevel + 1];
        }

        private void ShowResultRank()
        {
            var result = new[] {"完全S", "勝利S", "勝利A", "勝利B", "敗北C", "敗北D", "敗北E"};
            labelResultRank.Text = result[(int)Sniffer.Battle.ResultRank];
        }

        private void labelResultRank_Click(object sender, EventArgs e)
        {
            ShowResultRank();
        }

        private void UpdateChargeInfo()
        {
            var fuelSq = new[] {labelFuelSq1, labelFuelSq2, labelFuelSq3, labelFuelSq4};
            var bullSq = new[] {labelBullSq1, labelBullSq2, labelBullSq3, labelBullSq4};

            for (var i = 0; i < fuelSq.Length; i++)
            {
                var stat = Sniffer.Fleets[i].ChargeStatus;
                fuelSq[i].ImageIndex = stat.Fuel;
                bullSq[i].ImageIndex = stat.Bull;
                var text = stat.Empty ? "" : $"燃{stat.FuelRatio * 100:f1}% 弾{stat.BullRatio * 100:f1}%";
                _toolTip.SetToolTip(fuelSq[i], text);
                _toolTip.SetToolTip(bullSq[i], text);
            }
        }

        private void UpdateNDocLabels()
        {
            ndockPanel.Update();
            SetNDockLabel();
        }

        private void SetNDockLabel()
        {
            labelNDock.Text = (Config.ShowEndTime & TimerKind.NDock) != 0 ? "入渠終了" : "入渠";
        }

        private void labelNDock_Click(object sender, EventArgs e)
        {
            Config.ShowEndTime ^= TimerKind.NDock;
            SetNDockLabel();
            UpdateTimers();
        }

        private void UpdateMissionLabels()
        {
            missionPanel.Update();
            SetMissionLabel();
        }

        private void SetMissionLabel()
        {
            labelMission.Text = (Config.ShowEndTime & TimerKind.Mission) != 0 ? "遠征終了" : "遠征";
        }

        private void labelMission_Click(object sender, EventArgs e)
        {
            Config.ShowEndTime ^= TimerKind.Mission;
            SetMissionLabel();
            UpdateTimers();
        }

        private void UpdateTimers()
        {
            foreach (var timer in _timers)
                timer.UpdateTimers();
            UpdateCondTimers();
            UpdateAkashiTimer();
            _timerEnabled = true;
        }

        private void NotifyTimers()
        {
            for (var i = 0; i < Sniffer.Missions.Length; i++)
            {
                var entry = Sniffer.Missions[i];
                if (entry.Name == "前衛支援任務" || entry.Name == "艦隊決戦支援任務")
                    continue;
                CheckAlarm("遠征終了", entry.Timer, i + 1, entry.Name);
            }
            for (var i = 0; i < Sniffer.NDock.Length; i++)
            {
                var entry = Sniffer.NDock[i];
                CheckAlarm("入渠終了", entry.Timer, i, entry.Name);
            }
            for (var i = 0; i < Sniffer.KDock.Length; i++)
            {
                var timer = Sniffer.KDock[i];
                CheckAlarm("建造完了", timer, i, "");
            }
            NotifyCondTimers();
            NotifyAkashiTimer();
            _notificationManager.Flash();
        }

        private void CheckAlarm(string key, AlarmTimer timer, int fleet, string subject)
        {
            if (timer.CheckAlarm(_prev, _now))
            {
                SetNotification(key, fleet, subject);
                return;
            }
            var pre = TimeSpan.FromSeconds(Config.Notifications[key].PreliminaryPeriod);
            if (pre == TimeSpan.Zero)
                return;
            if (timer.CheckAlarm(_prev + pre, _now + pre))
                SetPreNotification(key, fleet, subject);
        }

        private void UpdateCondTimers()
        {
            DateTime timer;
            if (_combinedFleet)
            {
                var timer1 = Sniffer.GetConditionTimer(0);
                var timer2 = Sniffer.GetConditionTimer(1);
                timer = timer2 > timer1 ? timer2 : timer1;
            }
            else
            {
                timer = Sniffer.GetConditionTimer(_currentFleet);
            }
            if (timer == DateTime.MinValue)
            {
                labelCondTimerTitle.Text = "";
                labelCondTimer.Text = "";
                return;
            }
            var span = TimeSpan.FromSeconds(Ceiling((timer - _now).TotalSeconds));
            if (span >= TimeSpan.FromMinutes(9) && Config.NotifyConditions.Contains(40))
            {
                labelCondTimerTitle.Text = "cond40まで";
                labelCondTimer.Text = (span - TimeSpan.FromMinutes(9)).ToString(@"mm\:ss");
                labelCondTimer.ForeColor = DefaultForeColor;
            }
            else
            {
                labelCondTimerTitle.Text = "cond49まで";
                labelCondTimer.Text = (span >= TimeSpan.Zero ? span : TimeSpan.Zero).ToString(@"mm\:ss");
                labelCondTimer.ForeColor = span <= TimeSpan.Zero ? CUDColors.Red : DefaultForeColor;
            }
        }

        private void NotifyCondTimers()
        {
            var notice = Sniffer.GetConditionNotice(_prev, _now);
            var pre = TimeSpan.FromSeconds(Config.Notifications["疲労回復"].PreliminaryPeriod);
            var preNotice = pre == TimeSpan.Zero
                ? new int[ShipInfo.FleetCount]
                : Sniffer.GetConditionNotice(_prev + pre, _now + pre);
            for (var i = 0; i < ShipInfo.FleetCount; i++)
            {
                if (Config.NotifyConditions.Contains(notice[i]))
                {
                    SetNotification("疲労回復" + notice[i], i, "cond" + notice[i]);
                }
                else if (Config.NotifyConditions.Contains(preNotice[i]))
                {
                    SetPreNotification("疲労回復" + preNotice[i], i, "cond" + notice[i]);
                }
            }
        }

        private void UpdateAkashiTimer()
        {
            if (Config.UsePresetAkashi)
                UpdatePresetAkashiTimer();
            _mainLabels.SetAkashiTimer(Sniffer.Fleets[_currentFleet].ActualShips,
                Sniffer.AkashiTimer.GetTimers(_currentFleet, _now));
        }

        private void UpdatePresetAkashiTimer()
        {
            var akashi = Sniffer.AkashiTimer;
            var span = akashi.GetPresetDeckTimer(_now);
            var color = span == TimeSpan.Zero && akashi.CheckPresetRepairing() ? CUDColors.Red : DefaultForeColor;
            var text = span == TimeSpan.MinValue ? "" : span.ToString(@"mm\:ss");
            labelAkashiRepairTimer.ForeColor = color;
            labelAkashiRepairTimer.Text = text;
            if (akashi.CheckPresetRepairing() && !akashi.CheckRepairing(_currentFleet, _now))
            {
                labelPresetAkashiTimer.ForeColor = color;
                labelPresetAkashiTimer.Text = text;
            }
            else
            {
                labelPresetAkashiTimer.ForeColor = DefaultForeColor;
                labelPresetAkashiTimer.Text = "";
            }
        }

        private void NotifyAkashiTimer()
        {
            var akashi = Sniffer.AkashiTimer;
            var msgs = akashi.GetNotice(_prev, _now);
            if (msgs.Length == 0)
            {
                _notificationManager.StopRepeat("泊地修理");
                return;
            }
            if (!akashi.CheckRepairing(_now) && !(akashi.CheckPresetRepairing() && Config.UsePresetAkashi))
            {
                _notificationManager.StopRepeat("泊地修理");
                return;
            }
            var skipPreliminary = false;
            if (msgs[0].Proceeded == "20分経過しました。")
            {
                SetNotification("泊地修理20分経過", msgs[0].Proceeded);
                msgs[0].Proceeded = "";
                skipPreliminary = true;
                // 修理完了がいるかもしれないので続ける
            }
            for (var i = 0; i < ShipInfo.FleetCount; i++)
            {
                if (msgs[i].Proceeded != "")
                    SetNotification("泊地修理進行", i, msgs[i].Proceeded);
                if (msgs[i].Completed != "")
                    SetNotification("泊地修理完了", i, msgs[i].Completed);
            }
            var pre = TimeSpan.FromSeconds(Config.Notifications["泊地修理20分経過"].PreliminaryPeriod);
            if (skipPreliminary || pre == TimeSpan.Zero)
                return;
            if ((msgs = akashi.GetNotice(_prev + pre, _now + pre))[0].Proceeded == "20分経過しました。")
                SetPreNotification("泊地修理20分経過", 0, msgs[0].Proceeded);
        }

        private void SetNotification(string key, string subject)
        {
            SetNotification(key, 0, subject);
        }

        private void SetNotification(string key, int fleet, string subject)
        {
            var spec = Config.Notifications[_notificationManager.KeyToName(key)];
            _notificationManager.Enqueue(key, fleet, subject,
                (spec.Flags & Config.NotificationFlags & NotificationType.Repeat) == 0 ? 0 : spec.RepeatInterval);
        }

        private void SetPreNotification(string key, int fleet, string subject)
        {
            var spec = Config.Notifications[_notificationManager.KeyToName(key)];
            if ((spec.Flags & NotificationType.Preliminary) != 0)
                _notificationManager.Enqueue(key, fleet, subject, 0, true);
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
            SetQuestNotification();
        }

        private void SetQuestNotification()
        {
            Sniffer.GetQuestNotifications(out var notify, out var stop);
            foreach (var questName in notify)
                SetNotification("任務達成", 0, questName);
            foreach (var questName in stop)
                _notificationManager.StopRepeat("任務達成", questName);
            _notificationManager.Flash();
        }

        private void Alarm(string balloonTitle, string balloonMessage, string name)
        {
            var flags = Config.Notifications[name].Flags;
            var effective = Config.NotificationFlags & Config.Notifications[name].Flags;
            if ((effective & NotificationType.FlashWindow) != 0)
                Win32API.FlashWindow(Handle);
            if ((effective & NotificationType.ShowBaloonTip) != 0)
                notifyIconMain.ShowBalloonTip(20000, balloonTitle, balloonMessage, ToolTipIcon.Info);
            if ((effective & NotificationType.PlaySound) != 0)
                PlaySound(Config.Sounds[name], Config.Sounds.Volume);
            if (Config.Pushbullet.On && (flags & NotificationType.Push) != 0)
            {
                Task.Run(() =>
                {
                    PushNotification.PushToPushbullet(Config.Pushbullet.Token, balloonTitle, balloonMessage);
                });
            }
            if (Config.Pushover.On && (flags & NotificationType.Push) != 0)
            {
                Task.Run(() =>
                {
                    PushNotification.PushToPushover(Config.Pushover.ApiKey, Config.Pushover.UserKey,
                        balloonTitle, balloonMessage);
                });
            }
        }

        [DllImport("winmm.dll")]
        private static extern int mciSendString(String command,
            StringBuilder buffer, int bufferSize, IntPtr hWndCallback);

// ReSharper disable InconsistentNaming
        // ReSharper disable once IdentifierTypo
        private const int MM_MCINOTIFY = 0x3B9;

        private const int MCI_NOTIFY_SUCCESSFUL = 1;
// ReSharper restore InconsistentNaming

        public void PlaySound(string file, int volume)
        {
            if (!File.Exists(file))
                return;
            mciSendString("close sound", null, 0, IntPtr.Zero);
            if (mciSendString("open \"" + file + "\" type mpegvideo alias sound", null, 0, IntPtr.Zero) != 0)
                return;
            mciSendString("setaudio sound volume to " + volume * 10, null, 0, IntPtr.Zero);
            mciSendString("play sound notify", null, 0, Handle);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == MM_MCINOTIFY && (int)m.WParam == MCI_NOTIFY_SUCCESSFUL)
                mciSendString("close sound", null, 0, IntPtr.Zero);
            base.WndProc(ref m);
        }

        private void SetupFleetClick()
        {
            var labels = new[]
            {
                new[] {labelFleet1, labelFleet2, labelFleet3, labelFleet4},
                new[] {labelFuelSq1, labelFuelSq2, labelFuelSq3, labelFuelSq4},
                new[] {labelBullSq1, labelBullSq2, labelBullSq3, labelBullSq4}
            };
            foreach (var a in labels)
            {
                a[0].Tag = 0;
                a[0].Click += labelFleet1_Click;
                a[0].DoubleClick += labelFleet1_DoubleClick;
                for (var fleet = 1; fleet < labels[0].Length; fleet++)
                {
                    a[fleet].Tag = fleet;
                    a[fleet].Click += labelFleet_Click;
                    a[fleet].DoubleClick += labelFleet_DoubleClick;
                }
            }
        }

        private void labelFleet_Click(object sender, EventArgs e)
        {
            if (!_started)
                return;
            var fleet = (int)((Label)sender).Tag;
            if (_currentFleet == fleet)
                return;
            _combinedFleet = false;
            _currentFleet = fleet;
            UpdatePanelShipInfo();
        }

        private readonly SemaphoreSlim _clickSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _doubleClickSemaphore = new SemaphoreSlim(0);

        private async void labelFleet1_Click(object sender, EventArgs e)
        {
            if (!_started)
                return;
            if (_currentFleet != 0)
            {
                labelFleet_Click(sender, e);
                return;
            }
            if (!_clickSemaphore.Wait(0))
                return;
            try
            {
                if (await _doubleClickSemaphore.WaitAsync(SystemInformation.DoubleClickTime))
                    return;
            }
            finally
            {
                _clickSemaphore.Release();
            }
            _combinedFleet = Sniffer.IsCombinedFleet && !_combinedFleet;
            UpdatePanelShipInfo();
        }

        private void labelFleet1_MouseHover(object sender, EventArgs e)
        {
            labelFleet1.Text = _currentFleet == 0 && Sniffer.IsCombinedFleet && !_combinedFleet ? "連合" : "第一";
        }

        private void labelFleet1_MouseLeave(object sender, EventArgs e)
        {
            labelFleet1.Text = _combinedFleet ? CombinedName : "第一";
        }

        private void labelFleet_DoubleClick(object sender, EventArgs e)
        {
            if (!_started)
                return;
            var fleet = (int)((Label)sender).Tag;
            var text = TextGenerator.GenerateFleetData(Sniffer, fleet);
            CopyFleetText(text, (Label)sender);
        }

        private void labelFleet1_DoubleClick(object sender, EventArgs e)
        {
            if (!_started)
                return;
            _doubleClickSemaphore.Release();
            var text = TextGenerator.GenerateFleetData(Sniffer, 0);
            if (_combinedFleet)
                text += TextGenerator.GenerateFleetData(Sniffer, 1);
            CopyFleetText(text, (Label)sender);
        }

        private void CopyFleetText(string text, Label fleetButton)
        {
            if (string.IsNullOrEmpty(text))
                return;
            Clipboard.SetText(text);
            _tooltipCopy.Active = true;
            _tooltipCopy.Show("コピーしました。", fleetButton);
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                _tooltipCopy.Active = false;
            });
        }

        public void ResetAchievement()
        {
            Sniffer.Achievement.Reset();
            UpdateItemInfo();
        }

        private void labelRepairListButton_Click(object sender, EventArgs e)
        {
            if (panelRepairList.Visible)
            {
                panelRepairList.Visible = false;
                labelRepairListButton.BackColor = DefaultBackColor;
            }
            else
            {
                panelRepairList.Visible = true;
                panelRepairList.BringToFront();
                labelRepairListButton.BackColor = CustomColors.ActiveButtonColor;
            }
        }

        private void panelRepairList_Click(object sender, EventArgs e)
        {
            panelRepairList.Visible = false;
            labelRepairListButton.BackColor = DefaultBackColor;
        }

        private void ShipListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _listFormGroup.ShowOrCreate();
            /*
            _listForm.UpdateList();
            _listForm.Show();
            if (_listForm.WindowState == FormWindowState.Minimized)
                _listForm.WindowState = FormWindowState.Normal;
            _listForm.Activate();
            */
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
            try
            {
                var proc = new ProcessStartInfo("BurageSnap.exe") {WorkingDirectory = "Capture"};
                Process.Start(proc);
            }
            catch (FileNotFoundException)
            {
            }
            catch (Win32Exception)
            {
            }
        }
    }
}