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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CSharp.RuntimeBinder;
using static System.Math;

namespace KancolleSniffer
{
    public partial class MainForm : Form
    {
        private readonly Sniffer _sniffer = new Sniffer();
        private readonly Config _config = new Config();
        private readonly ConfigDialog _configDialog;
        private readonly ProxyManager _proxyManager;
        private readonly ToolTip _toolTipQuest = new ToolTip {ShowAlways = true};
        private readonly ToolTip _tooltipCopy = new ToolTip {AutomaticDelay = 0};
        private int _currentFleet;
        private bool _combinedFleet;
        private readonly Label[] _labelCheckFleets;
        private readonly ShipLabels _shipLabels;
        private readonly ListForm _listForm;
        private readonly NotificationManager _notificationManager;
        private bool _started;
        private bool _timerEnabled;
        private string _debugLogFile;
        private IEnumerator<string> _playLog;
        private DateTime _prev, _now;

        private readonly ErrorDialog _errorDialog = new ErrorDialog();
        private readonly ErrorLog _errorLog;

        public MainForm()
        {
            InitializeComponent();
            HttpProxy.AfterSessionComplete += HttpProxy_AfterSessionComplete;
            _configDialog = new ConfigDialog(_config, this);
            _labelCheckFleets = new[] {labelCheckFleet1, labelCheckFleet2, labelCheckFleet3, labelCheckFleet4};

            // この時点でAutoScaleDimensions == CurrentAutoScaleDimensionsなので、
            // MainForm.Designer.csのAutoScaleDimensionsの6f,12fを使う。
            ShipLabel.ScaleFactor = new SizeF(CurrentAutoScaleDimensions.Width / 6f,
                CurrentAutoScaleDimensions.Height / 12f);

            SetupFleetClick();
            _shipLabels = new ShipLabels();
            _shipLabels.CreateAkashiTimers(panelShipInfo);
            _shipLabels.CreateShipLabels(panelShipInfo, ShowShipOnShipList);
            _shipLabels.CreateAkashiTimers7(panel7Ships);
            _shipLabels.CreateShipLabels7(panel7Ships, ShowShipOnShipList);
            _shipLabels.CreateCombinedShipLabels(panelCombinedFleet, ShowShipOnShipList);
            _shipLabels.CreateNDockLabels(panelDock, labelNDock_Click);
            panelRepairList.CreateLabels(panelRepairList_Click);
            labelPresetAkashiTimer.BackColor = ShipLabels.ColumnColors[1];
            _listForm = new ListForm(_sniffer, _config) {Owner = this};
            _notificationManager = new NotificationManager(Alarm);
            try
            {
                _config.Load();
            }
            catch (Exception ex)
            {
                throw new ConfigFileException("設定ファイルが壊れています。", ex);
            }
            _proxyManager = new ProxyManager(_config, this);
            _errorLog = new ErrorLog(_sniffer);
            _proxyManager.UpdatePacFile();
            PerformZoom();
            _shipLabels.AdjustAkashiTimers();
            _sniffer.LoadState();
            _sniffer.RepeatingTimerController = new RepeatingTimerController(_notificationManager, _config);
        }

        private class RepeatingTimerController : Sniffer.IRepeatingTimerController
        {
            private readonly NotificationManager _manager;
            private readonly Config _config;

            public RepeatingTimerController(NotificationManager manager, Config config)
            {
                _manager = manager;
                _config = config;
            }

            public void Stop(string key)
            {
                _manager.StopRepeat(key,
                    (key == "入渠終了" || key == "遠征終了") &&
                    (_config.Notifications[key].Flags & NotificationType.Cont) != 0);
            }

            public void Stop(string key, int fleet) => _manager.StopRepeat(key, fleet);

            public void Suspend() => _manager.SuspendRepeat();

            public void Resume() => _manager.ResumeRepeat();
        }

        public class ConfigFileException : Exception
        {
            public ConfigFileException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        private void HttpProxy_AfterSessionComplete(HttpProxy.Session session)
        {
            Invoke(new Action<HttpProxy.Session>(ProcessRequest), session);
        }

        private void ProcessRequest(HttpProxy.Session session)
        {
            var url = session.Request.PathAndQuery;
            if (!url.Contains("kcsapi/"))
                return;
            var request = session.Request.BodyAsString;
            var response = session.Response.BodyAsString;
            if (response == null || !response.StartsWith("svdata="))
            {
                WriteDebugLog(url, request, response);
                return;
            }
            response = UnescapeString(response.Remove(0, "svdata=".Length));
            WriteDebugLog(url, request, response);
            ProcessRequestMain(url, request, response);
        }

        private void ProcessRequestMain(string url, string request, string response)
        {
            try
            {
                UpdateInfo(_sniffer.Sniff(url, request, JsonParser.Parse(response)));
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

        private string UnescapeString(string s)
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
                labelLogin.Visible = false;
                linkLabelGuide.Visible = false;
                _started = true;
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
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RestoreLocation();
            if (_config.HideOnMinimized && WindowState == FormWindowState.Minimized)
                ShowInTaskbar = false;
            if (_config.ShowHpInPercent)
                _shipLabels.ToggleHpPercent();
            if (_config.ShipList.Visible)
                _listForm.Show();
            ApplyConfig();
            ApplyDebugLogSetting();
            ApplyLogSetting();
            ApplyProxySetting();
            CheckVersionUp((current, latest) =>
            {
                if (double.Parse(latest) <= double.Parse(current))
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
            if (!_config.ExitSilently)
            {
                using (var dialog = new ConfirmDialog())
                {
                    if (dialog.ShowDialog(this) != DialogResult.Yes)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
            e.Cancel = false;
            _sniffer.FlashLog();
            _config.Location = (WindowState == FormWindowState.Normal ? Bounds : RestoreBounds).Location;
            _config.ShowHpInPercent = _shipLabels.ShowHpInPercent;
            _config.ShipList.Visible = _listForm.Visible && _listForm.WindowState == FormWindowState.Normal;
            _config.Save();
            _sniffer.SaveState();
            _proxyManager.Shutdown();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            ShowInTaskbar = !(_config.HideOnMinimized && WindowState == FormWindowState.Minimized);
        }

        private void notifyIconMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            NotifyIconOpenToolStripMenuItem_Click(sender, e);
        }

        private void NotifyIconOpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            TopMost = _config.TopMost; // 最前面に表示されなくなることがあるのを回避する
            Activate();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_configDialog.ShowDialog(this) == DialogResult.OK)
            {
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
            if (_config.Zoom == 100)
                return;
            var prev = CurrentAutoScaleDimensions;
            foreach (var control in new Control[]
            {
                this, _listForm, labelLogin, linkLabelGuide,
                _configDialog, _configDialog.NotificationConfigDialog,
                contextMenuStripMain, _errorDialog
            })
            {
                control.Font = new Font(control.Font.FontFamily, control.Font.Size * _config.Zoom / 100);
            }
            ShipLabel.LatinFont = new Font("Tahoma", 8f * _config.Zoom / 100);
            var cur = CurrentAutoScaleDimensions;
            ShipLabel.ScaleFactor = new SizeF(ShipLabel.ScaleFactor.Width * cur.Width / prev.Width,
                ShipLabel.ScaleFactor.Height * cur.Height / prev.Height);
        }

        private void RestoreLocation()
        {
            if (_config.Location.X == int.MinValue)
                return;
            if (IsTitleBarOnAnyScreen(_config.Location))
                Location = _config.Location;
        }

        private void ApplyConfig()
        {
            _listForm.TopMost = TopMost = _config.TopMost;
            _sniffer.Item.MarginShips = _config.MarginShips;
            UpdateNumOfShips();
            _sniffer.Item.MarginEquips = _config.MarginEquips;
            UpdateNumOfEquips();
            _sniffer.Achievement.ResetHours = _config.ResetHours;
            labelAkashiRepair.Visible = labelAkashiRepairTimer.Visible =
                labelPresetAkashiTimer.Visible = _config.UsePresetAkashi;
        }

        public void ApplyDebugLogSetting()
        {
            _debugLogFile = _config.DebugLogging ? _config.DebugLogFile : null;
        }

        public bool ApplyProxySetting()
        {
            return _proxyManager.ApplyConfig();
        }

        public void ApplyLogSetting()
        {
            LogServer.OutputDir = _config.Log.OutputDir;
            LogServer.MaterialHistory = _sniffer.Material.MaterialHistory;
            _sniffer.EnableLog(_config.Log.On ? LogType.All : LogType.None);
            _sniffer.MaterialLogInterval = _config.Log.MaterialLogInterval;
            _sniffer.LogOutputDir = _config.Log.OutputDir;
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
                        Application.Exit();
                }
            }
            if (_playLog == null || _configDialog.Visible)
            {
                labelPlayLog.Visible = false;
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
                        labelPlayLog.Visible = false;
                        return;
                    }
                } while (!_playLog.Current.StartsWith(s));
                lines.Add(_playLog.Current.Substring(s.Length));
            }
            labelPlayLog.Visible = !labelPlayLog.Visible;
            ProcessRequestMain(lines[0], lines[1], lines[2]);
        }

        private void ShowShipOnShipList(object sender, EventArgs ev)
        {
            if (!_listForm.Visible)
                return;
            var idx = (int)((Control)sender).Tag;
            var statuses = _sniffer.GetShipStatuses(_currentFleet);
            if (statuses.Length <= idx)
                return;
            _listForm.ShowShip(statuses[idx].Id);
        }

        private void UpdateItemInfo()
        {
            UpdateNumOfShips();
            UpdateNumOfEquips();
            _notificationManager.Flash();
            labelNumOfBuckets.Text = _sniffer.Material.MaterialHistory[(int)Material.Bucket].Now.ToString("D");
            UpdateBucketHistory();
            var ac = _sniffer.Achievement.Value;
            if (ac >= 10000)
                ac = 9999;
            labelAchievement.Text = ac >= 1000 ? ((int)ac).ToString("D") : ac.ToString("F1");
            toolTipAchievement.SetToolTip(labelAchievement,
                "今月 " + _sniffer.Achievement.ValueOfMonth.ToString("F1") + "\n" +
                "EO " + _sniffer.ExMap.Achievement);
            UpdateMaterialHistry();
            if (_listForm.Visible)
                _listForm.UpdateList();
        }

        private void UpdateNumOfShips()
        {
            var item = _sniffer.Item;
            labelNumOfShips.Text = $"{item.NowShips:D}/{item.MaxShips:D}";
            labelNumOfShips.ForeColor = item.TooManyShips ? CUDColor.Red : Color.Black;
            if (item.AlarmShips)
            {
                var message = $"残り{_sniffer.Item.MaxShips - _sniffer.Item.NowShips:D}隻";
                _notificationManager.Enqueue("艦娘数超過", message);
                item.AlarmShips = false;
            }
        }

        private void UpdateNumOfEquips()
        {
            var item = _sniffer.Item;
            labelNumOfEquips.Text = $"{item.NowEquips:D}/{item.MaxEquips:D}";
            labelNumOfEquips.ForeColor = item.TooManyEquips ? CUDColor.Red : Color.Black;
            if (item.AlarmEquips)
            {
                var message = $"残り{_sniffer.Item.MaxEquips - _sniffer.Item.NowEquips:D}個";
                _notificationManager.Enqueue("装備数超過", message);
                item.AlarmEquips = false;
            }
        }

        private void UpdateBucketHistory()
        {
            var count = _sniffer.Material.MaterialHistory[(int)Material.Bucket];
            var day = CutOverflow(count.Now - count.BegOfDay, 999);
            var week = CutOverflow(count.Now - count.BegOfWeek, 999);
            labelBucketHistory.Text = $"{day:+#;-#;±0} 今日\n{week:+#;-#;±0} 今週";
        }

        private void UpdateMaterialHistry()
        {
            var labels = new[] {labelFuelHistory, labelBulletHistory, labelSteelHistory, labelBouxiteHistory};
            var text = new[] {"燃料", "弾薬", "鋼材", "ボーキ"};
            for (var i = 0; i < labels.Length; i++)
            {
                var count = _sniffer.Material.MaterialHistory[i];
                var port = CutOverflow(count.Now - _sniffer.Material.PrevPort[i], 99999);
                var day = CutOverflow(count.Now - count.BegOfDay, 99999);
                var week = CutOverflow(count.Now - count.BegOfWeek, 99999);
                labels[i].Text = $"{text[i]}\n{port:+#;-#;±0}\n{day:+#;-#;±0}\n{week:+#;-#;±0}";
            }
        }

        private int CutOverflow(int value, int limit)
        {
            if (value > limit)
                return limit;
            if (value < -limit)
                return -limit;
            return value;
        }

        private void UpdateShipInfo()
        {
            UpdatePanelShipInfo();
            NotifyDamagedShip();
            UpdateChargeInfo();
            UpdateRepairList();
            if (_listForm.Visible)
                _listForm.UpdateList();
        }

        private void UpdatePanelShipInfo()
        {
            var statuses = _sniffer.GetShipStatuses(_currentFleet);
            panel7Ships.Visible = statuses.Length == 7;
            _shipLabels.SetShipLabels(statuses);
            if (_sniffer.CombinedFleetType == 0)
                _combinedFleet = false;
            labelFleet1.Text = _combinedFleet ? "連合" : "第一";
            panelCombinedFleet.Visible = _combinedFleet;
            if (_combinedFleet)
                _shipLabels.SetCombinedShipLabels(_sniffer.GetShipStatuses(0), _sniffer.GetShipStatuses(1));
            UpdateAkashiTimer();
            UpdateFighterPower(_combinedFleet);
            UpdateLoS();
            UpdateCondTimers();
        }

        private void NotifyDamagedShip()
        {
            if (!_sniffer.BadlyDamagedShips.Any())
                return;
            _notificationManager.Enqueue("大破警告", string.Join(" ", _sniffer.BadlyDamagedShips));
            _notificationManager.Flash();
        }

        public void UpdateFighterPower(bool combined)
        {
            var fp = combined
                ? _sniffer.GetFighterPower(0).Zip(_sniffer.GetFighterPower(1), (a, b) => a + b).ToArray()
                : _sniffer.GetFighterPower(_currentFleet);
            labelFighterPower.Text = fp[0].ToString("D");
            var cr = combined
                ? _sniffer.GetContactTriggerRate(0) + _sniffer.GetContactTriggerRate(1)
                : _sniffer.GetContactTriggerRate(_currentFleet);
            var text = "制空: " + (fp[0] == fp[1] ? $"{fp[0]}" : $"{fp[0]}～{fp[1]}") +
                       $" 触接: {cr * 100:f1}";
            toolTipFighterPower.SetToolTip(labelFighterPower, text);
            toolTipFighterPower.SetToolTip(labelFighterPowerCaption, text);
        }

        private void UpdateLoS()
        {
            labelLoS.Text = RoundDown(_sniffer.GetFleetLineOfSights(_currentFleet, 1)).ToString("F1");
            var text = $"係数3: {RoundDown(_sniffer.GetFleetLineOfSights(_currentFleet, 3)):F1}\r\n" +
                       $"係数4: {RoundDown(_sniffer.GetFleetLineOfSights(_currentFleet, 4)):F1}";
            toolTipLoS.SetToolTip(labelLoS, text);
            toolTipLoS.SetToolTip(labelLoSCaption, text);
        }

        private double RoundDown(double number)
        {
            return Floor(number * 10) / 10.0;
        }

        private void UpdateBattleInfo()
        {
            ResetBattleInfo();
            _listForm.UpdateBattleResult();
            if (_sniffer.Battle.BattleState == BattleState.None)
                return;
            panelBattleInfo.BringToFront();
            var battle = _sniffer.Battle;
            labelFormation.Text = new[] {"同航戦", "反航戦", "T字有利", "T字不利"}[battle.Formation[2] - 1];
            UpdateBattleFighterPower();
            if ((_config.Spoilers & Spoiler.ResultRank) != 0)
                ShowResultRank();
            if (_sniffer.Battle.BattleState == BattleState.Day)
                _listForm.UpdateAirBattleResult();
        }

        private void ResetBattleInfo()
        {
            labelFormation.Text = "";
            labelEnemyFighterPower.Text = "";
            labelFighterPower.ForeColor = DefaultForeColor;
            labelResultRank.Text = "判定";
            panelBattleInfo.Visible = _sniffer.Battle.BattleState != BattleState.None;
        }

        private void UpdateBattleFighterPower()
        {
            var battle = _sniffer.Battle;
            var power = battle.EnemyFighterPower;
            labelEnemyFighterPower.Text = power.AirCombat + power.UnknownMark;
            if (power.AirCombat != power.Interception)
            {
                var text = "防空: " + power.Interception + power.UnknownMark;
                toolTipFighterPower.SetToolTip(labelEnemyFighterPower, text);
                toolTipFighterPower.SetToolTip(labelEnemyFighterPowerCaption, text);
            }
            UpdateFighterPower(_sniffer.CombinedFleetType > 0 && battle.EnemyIsCombined);
            labelFighterPower.ForeColor = new[]
                {DefaultForeColor, DefaultForeColor, CUDColor.Blue, CUDColor.Green, CUDColor.Orange, CUDColor.Red}[
                battle.AirControlLevel + 1];
        }

        private void ShowResultRank()
        {
            var result = new[] {"完全S", "勝利S", "勝利A", "勝利B", "敗北C", "敗北D", "敗北E"};
            labelResultRank.Text = result[(int)_sniffer.Battle.ResultRank];
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
                var stat = _sniffer.ChargeStatuses[i];
                fuelSq[i].ImageIndex = stat.Fuel;
                bullSq[i].ImageIndex = stat.Bull;
            }
        }

        private void UpdateNDocLabels()
        {
            _shipLabels.SetNDockLabels(_sniffer.NDock);
        }


        private void labelNDock_Click(object sender, EventArgs e)
        {
            _config.ShowEndTime ^= TimerKind.NDock;
            UpdateTimers();
        }

        private void UpdateMissionLabels()
        {
            foreach (var entry in
                new[] {labelMissionName1, labelMissionName2, labelMissionName3}.Zip(_sniffer.Missions,
                    (label, mission) => new {label, mission.Name}))
                entry.label.Text = entry.Name;
        }

        private void labelMission_Click(object sender, EventArgs e)
        {
            _config.ShowEndTime ^= TimerKind.Mission;
            UpdateTimers();
        }

        private void UpdateTimers()
        {
            var mission = new[] {labelMission1, labelMission2, labelMission3};
            for (var i = 0; i < mission.Length; i++)
            {
                var entry = _sniffer.Missions[i];
                SetTimerColor(mission[i], entry.Timer, _now);
                mission[i].Text = entry.Timer.ToString(_now, (_config.ShowEndTime & TimerKind.Mission) != 0);
            }
            for (var i = 0; i < _sniffer.NDock.Length; i++)
            {
                var entry = _sniffer.NDock[i];
                _shipLabels.SetNDockTimer(i, entry.Timer, _now, (_config.ShowEndTime & TimerKind.NDock) != 0);
            }
            var kdock = new[] {labelConstruct1, labelConstruct2, labelConstruct3, labelConstruct4};
            for (var i = 0; i < kdock.Length; i++)
            {
                var timer = _sniffer.KDock[i];
                SetTimerColor(kdock[i], timer, _now);
                kdock[i].Text = timer.ToString(_now);
            }
            UpdateCondTimers();
            UpdateAkashiTimer();
            _timerEnabled = true;
        }

        private void NotifyTimers()
        {
            for (var i = 0; i < _sniffer.Missions.Length; i++)
            {
                var entry = _sniffer.Missions[i];
                CheckAlarm("遠征終了", entry.Timer, i + 1, entry.Name);
            }
            for (var i = 0; i < _sniffer.NDock.Length; i++)
            {
                var entry = _sniffer.NDock[i];
                CheckAlarm("入渠終了", entry.Timer, i, entry.Name);
            }
            for (var i = 0; i < _sniffer.KDock.Length; i++)
            {
                var timer = _sniffer.KDock[i];
                CheckAlarm("建造完了", timer, 0, $"第{i + 1:D}ドック");
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
            var pre = TimeSpan.FromSeconds(_config.Notifications[key].PreliminaryPeriod);
            if (pre == TimeSpan.Zero)
                return;
            if (timer.CheckAlarm(_prev + pre, _now + pre))
                SetPreNotification(key, fleet, subject);
        }

        private void SetTimerColor(Label label, AlarmTimer timer, DateTime now)
        {
            label.ForeColor = timer.IsFinished(now) ? CUDColor.Red : Color.Black;
        }

        private void UpdateCondTimers()
        {
            DateTime timer;
            if (_combinedFleet)
            {
                var timer1 = _sniffer.GetConditionTimer(0);
                var timer2 = _sniffer.GetConditionTimer(1);
                timer = timer2 > timer1 ? timer2 : timer1;
            }
            else
            {
                timer = _sniffer.GetConditionTimer(_currentFleet);
            }
            if (timer == DateTime.MinValue)
            {
                labelCondTimerTitle.Text = "";
                labelCondTimer.Text = "";
                return;
            }
            var span = TimeSpan.FromSeconds(Ceiling((timer - _now).TotalSeconds));
            if (span >= TimeSpan.FromMinutes(9))
            {
                labelCondTimerTitle.Text = "cond40まで";
                labelCondTimer.Text = (span - TimeSpan.FromMinutes(9)).ToString(@"mm\:ss");
                labelCondTimer.ForeColor = DefaultForeColor;
            }
            else
            {
                labelCondTimerTitle.Text = "cond49まで";
                labelCondTimer.Text = (span >= TimeSpan.Zero ? span : TimeSpan.Zero).ToString(@"mm\:ss");
                labelCondTimer.ForeColor = span <= TimeSpan.Zero ? CUDColor.Red : DefaultForeColor;
            }
        }

        private void NotifyCondTimers()
        {
            var notice = _sniffer.GetConditionNotice(_prev, _now);
            var pre = TimeSpan.FromSeconds(_config.Notifications["疲労回復"].PreliminaryPeriod);
            var preNotice = pre == TimeSpan.Zero
                ? new int[ShipInfo.FleetCount]
                : _sniffer.GetConditionNotice(_prev + pre, _now + pre);
            for (var i = 0; i < ShipInfo.FleetCount; i++)
            {
                if (_config.NotifyConditions.Contains(notice[i]))
                {
                    SetNotification("疲労回復" + notice[i], i, "cond" + notice[i]);
                }
                else if (_config.NotifyConditions.Contains(preNotice[i]))
                {
                    SetPreNotification("疲労回復" + preNotice[i], i, "cond" + notice[i]);
                }
            }
        }

        private void UpdateAkashiTimer()
        {
            if (_config.UsePresetAkashi)
                UpdatePresetAkashiTimer();
            var statuses = _sniffer.GetShipStatuses(_currentFleet);
            _shipLabels.SetAkashiTimer(statuses,
                _sniffer.AkashiTimer.GetTimers(_currentFleet));
        }

        private void UpdatePresetAkashiTimer()
        {
            var akashi = _sniffer.AkashiTimer;
            var span = akashi.PresetDeckTimer;
            var color = span == TimeSpan.Zero && akashi.CheckPresetRepairing() ? CUDColor.Red : DefaultForeColor;
            var text = span == TimeSpan.MinValue ? "" : span.ToString(@"mm\:ss");
            labelAkashiRepairTimer.ForeColor = color;
            labelAkashiRepairTimer.Text = text;
            if (akashi.CheckPresetRepairing() && !akashi.CheckRepairing(_currentFleet))
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
            var akashi = _sniffer.AkashiTimer;
            var msgs = akashi.GetNotice(_prev, _now);
            if (msgs.Length == 0)
            {
                _notificationManager.StopRepeat("泊地修理");
                return;
            }
            if (!akashi.CheckRepairing() && !(akashi.CheckPresetRepairing() && _config.UsePresetAkashi))
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
            var pre = TimeSpan.FromSeconds(_config.Notifications["泊地修理20分経過"].PreliminaryPeriod);
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
            var spec = _config.Notifications[_notificationManager.KeyToName(key)];
            _notificationManager.Enqueue(key, fleet, subject,
                (spec.Flags & _config.NotificationFlags & NotificationType.Repeat) == 0 ? 0 : spec.RepeatInterval);
        }

        private void SetPreNotification(string key, int fleet, string subject)
        {
            var spec = _config.Notifications[_notificationManager.KeyToName(key)];
            if ((spec.Flags & NotificationType.Preliminary) != 0)
                _notificationManager.Enqueue(key, fleet, subject, 0, true);
        }

        private void UpdateRepairList()
        {
            panelRepairList.SetRepairList(_sniffer.RepairList);
        }

        private void UpdateQuestList()
        {
            var category = new[]
            {
                labelQuestColor1, labelQuestColor2, labelQuestColor3, labelQuestColor4, labelQuestColor5,
                labelQuestColor6
            };
            var name = new[] {labelQuest1, labelQuest2, labelQuest3, labelQuest4, labelQuest5, labelQuest6};
            var count = new[]
            {
                labelQuestCount1, labelQuestCount2, labelQuestCount3, labelQuestCount4, labelQuestCount5,
                labelQuestCount6
            };
            var progress = new[]
                {labelProgress1, labelProgress2, labelProgress3, labelProgress4, labelProgress5, labelProgress6};
            var quests = _sniffer.Quests;
            for (var i = 0; i < name.Length; i++)
            {
                if (i < quests.Length)
                {
                    category[i].BackColor = quests[i].Color;
                    name[i].Text = quests[i].Name;
                    progress[i].Text = $"{quests[i].Progress:D}%";
                    _toolTipQuest.SetToolTip(name[i], quests[i].Detail);
                    var c = quests[i].Count;
                    if (c.Id == 0)
                    {
                        count[i].Text = "";
                        count[i].ForeColor = Color.Black;
                        _toolTipQuest.SetToolTip(count[i], "");
                        continue;
                    }
                    count[i].Text = " " + c;
                    count[i].ForeColor = c.Cleared ? CUDColor.Green : Color.Black;
                    _toolTipQuest.SetToolTip(count[i], c.ToToolTip());
                }
                else
                {
                    category[i].BackColor = DefaultBackColor;
                    name[i].Text = count[i].Text = progress[i].Text = "";
                    _toolTipQuest.SetToolTip(count[i], "");
                }
            }
        }

        private void Alarm(string balloonTitle, string balloonMessage, string name)
        {
            var flags = _config.Notifications[name].Flags;
            var effective = _config.NotificationFlags & _config.Notifications[name].Flags;
            if ((effective & NotificationType.FlashWindow) != 0)
                Win32API.FlashWindow(Handle);
            if ((effective & NotificationType.ShowBaloonTip) != 0)
                notifyIconMain.ShowBalloonTip(20000, balloonTitle, balloonMessage, ToolTipIcon.Info);
            if ((effective & NotificationType.PlaySound) != 0)
                PlaySound(_config.Sounds[name], _config.Sounds.Volume);
            if (_config.Pushbullet.On && (flags & NotificationType.Push) != 0)
            {
                Task.Run(() =>
                {
                    PushNotification.PushToPushbullet(_config.Pushbullet.Token, balloonTitle, balloonMessage);
                });
            }
            if (_config.Pushover.On && (flags & NotificationType.Push) != 0)
            {
                Task.Run(() =>
                {
                    PushNotification.PushToPushover(_config.Pushover.ApiKey, _config.Pushover.UserKey,
                        balloonTitle, balloonMessage);
                });
            }
        }

        [DllImport("winmm.dll")]
        private static extern int mciSendString(String command,
            StringBuilder buffer, int bufferSize, IntPtr hwndCallback);

// ReSharper disable InconsistentNaming
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
                for (var fleet = 0; fleet < labels[0].Length; fleet++)
                {
                    a[fleet].Tag = fleet;
                    a[fleet].Click += labelFleet_Click;
                }
            }
        }

        private void labelFleet_Click(object sender, EventArgs e)
        {
            if (!_started)
                return;
            var fleet = (int)((Label)sender).Tag;
            if (_currentFleet == fleet)
            {
                if (fleet > 0)
                    return;
                _combinedFleet = _sniffer.CombinedFleetType > 0 && !_combinedFleet;
                UpdatePanelShipInfo();
                return;
            }
            _combinedFleet = false;
            _currentFleet = fleet;
            foreach (var label in _labelCheckFleets)
                label.Visible = false;
            _labelCheckFleets[fleet].Visible = true;
            UpdatePanelShipInfo();
        }

        private void labelFleet1_MouseHover(object sender, EventArgs e)
        {
            labelFleet1.Text = _currentFleet == 0 && _sniffer.CombinedFleetType > 0 && !_combinedFleet ? "連合" : "第一";
        }

        private void labelFleet1_MouseLeave(object sender, EventArgs e)
        {
            labelFleet1.Text = _combinedFleet ? "連合" : "第一";
        }

        private readonly Color _activeButtonColor = Color.FromArgb(152, 179, 208);

        private void labelBucketHistoryButton_Click(object sender, EventArgs e)
        {
            if (labelBucketHistory.Visible)
            {
                labelBucketHistory.Visible = false;
                labelBucketHistoryButton.BackColor = DefaultBackColor;
            }
            else
            {
                labelBucketHistory.Visible = true;
                labelBucketHistory.BringToFront();
                labelBucketHistoryButton.BackColor = _activeButtonColor;
            }
        }

        private void labelBucketHistory_Click(object sender, EventArgs e)
        {
            labelBucketHistory.Visible = false;
            labelBucketHistoryButton.BackColor = DefaultBackColor;
        }

        private void labelMaterialHistoryButton_Click(object sender, EventArgs e)
        {
            if (panelMaterialHistory.Visible)
            {
                panelMaterialHistory.Visible = false;
                labelMaterialHistoryButton.BackColor = DefaultBackColor;
            }
            else
            {
                panelMaterialHistory.Visible = true;
                panelMaterialHistory.BringToFront();
                labelMaterialHistoryButton.BackColor = _activeButtonColor;
            }
        }

        private void panelMaterialHistory_Click(object sender, EventArgs e)
        {
            panelMaterialHistory.Visible = false;
            labelMaterialHistoryButton.BackColor = DefaultBackColor;
        }

        public void ResetAchievemnt()
        {
            _sniffer.Achievement.Reset();
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
                labelRepairListButton.BackColor = _activeButtonColor;
            }
        }

        private void panelRepairList_Click(object sender, EventArgs e)
        {
            panelRepairList.Visible = false;
            labelRepairListButton.BackColor = DefaultBackColor;
        }

        private void ShipListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _listForm.UpdateList();
            _listForm.Show();
            if (_listForm.WindowState == FormWindowState.Minimized)
                _listForm.WindowState = FormWindowState.Normal;
            _listForm.Activate();
        }

        private void LogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://localhost:" + _config.Proxy.Listen + "/");
        }

        private void labelClearQuest_Click(object sender, EventArgs e)
        {
            _sniffer.ClearQuests();
            UpdateQuestList();
        }

        private void labelClearQuest_MouseDown(object sender, MouseEventArgs e)
        {
            labelClearQuest.BackColor = _activeButtonColor;
        }

        private void labelClearQuest_MouseUp(object sender, MouseEventArgs e)
        {
            labelClearQuest.BackColor = DefaultBackColor;
        }

        private void labelQuest_DoubleClick(object sender, EventArgs e)
        {
            var label = (Label)sender;
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
        }
    }
}