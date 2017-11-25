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
        private int _currentFleet;
        private bool _combinedFleet;
        private readonly Label[] _labelCheckFleets;
        private readonly ShipLabels _shipLabels;
        private readonly ListForm _listForm;
        private readonly NotificationManager _notificationManager;
        private bool _started;
        private string _debugLogFile;
        private IEnumerator<string> _playLog;

        private readonly ErrorDialog _errorDialog = new ErrorDialog();
        private bool _missionFinishTimeMode;
        private bool _ndockFinishTimeMode;
        private readonly KancolleDb _kancolleDb = new KancolleDb();

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
            _shipLabels.CreateCombinedShipLabels(panelCombinedFleet, ShowShipOnShipList);
            _shipLabels.CreateNDockLabels(panelDock, labelNDock_Click);
            panelRepairList.CreateLabels(panelRepairList_Click);
            labelPresetAkashiTimer.BackColor = ShipLabels.ColumnColors[1];
            _listForm = new ListForm(_sniffer, _config) {Owner = this};
            _notificationManager = new NotificationManager(Ring);
            _config.Load();
            _proxyManager = new ProxyManager(_config, this);
            PerformZoom();
            _shipLabels.AdjustAkashiTimers();
            _sniffer.LoadState();
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
            if (_config.KancolleDb.On)
                _kancolleDb.Send(url, request, response);
            response = UnescapeString(response.Remove(0, "svdata=".Length));
            WriteDebugLog(url, request, response);
            ProcessRequestMain(url, request, response);
        }

        private readonly List<string[]> _battleApiLog = new List<string[]>();

        private void ProcessRequestMain(string url, string request, string response)
        {
            try
            {
                UpdateInfo(_sniffer.Sniff(url, request, JsonParser.Parse(response)));
                CheckBattleResult();
                SaveBattleApi(url, request, response);
            }
            catch (RuntimeBinderException e)
            {
                if (_errorDialog.ShowDialog(this,
                        "艦これに仕様変更があったか、受信内容が壊れています。",
                        GenerateErrorLog(url, request, response, e.ToString())) == DialogResult.Abort)
                    Application.Exit();
            }
            catch (LogIOException e)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (_errorDialog.ShowDialog(this, e.Message, e.InnerException.ToString()) == DialogResult.Abort)
                    Application.Exit();
            }
            catch (Exception e)
            {
                if (_errorDialog.ShowDialog(this, "エラーが発生しました。",
                        GenerateErrorLog(url, request, response, e.ToString())) == DialogResult.Abort)
                    Application.Exit();
            }
        }

        private void CheckBattleResult()
        {
            if (_sniffer.WrongBattleResult.Count > 0)
            {
                if (_errorDialog.ShowDialog(this,
                        "戦闘結果の計算に誤りがあります。",
                        GenerateBattleErrorLog()) == DialogResult.Abort)
                    Application.Exit();
                _sniffer.WrongBattleResult.Clear();
            }
        }

        private void SaveBattleApi(string url, string request, string response)
        {
            if (_sniffer.Battle.BattleState != BattleState.None)
            {
                _battleApiLog.Add(new[] {url, request, response});
                return;
            }
            _battleApiLog.Clear();
        }

        private string GenerateBattleErrorLog()
        {
            foreach (var logs in _battleApiLog)
                RemoveSensitiveInformation(ref logs[1], ref logs[2]);
            var version = string.Join(".", Application.ProductVersion.Split('.').Take(2));
            var api = string.Join("\r\n", _battleApiLog.Select(logs => string.Join("\r\n", logs)));
            var status = string.Join(" ",
                from pair in _sniffer.WrongBattleResult
                let assumed = pair.Assumed
                let actual = pair.Actual
                select $"[{assumed.Fleet}-{assumed.DeckIndex}] {assumed.Id}: {assumed.NowHp}->{actual.NowHp}");
            return $"{DateTime.Now:g} {version}\r\n{api}\r\n{status}";
        }

        private string GenerateErrorLog(string url, string request, string response, string exception)
        {
            RemoveSensitiveInformation(ref request, ref response);
            var version = string.Join(".", Application.ProductVersion.Split('.').Take(2));
            return $"{DateTime.Now:g} {version}\r\n{exception}\r\n{url}\r\n{request}\r\n{response}\r\n";
        }

        private void RemoveSensitiveInformation(ref string request, ref string response)
        {
            var token = new Regex("&api%5Ftoken=[^&]*|api%5Ftoken=[^&]*&?");
            request = token.Replace(request, "");
            var id = new Regex(@"""api_member_id"":\d+,?|""api_nickname"":[^,]+,""api_nickname_id"":""d+"",?");
            response = id.Replace(response, "");
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
                labelGuide.Visible = false;
                _started = true;
                return;
            }
            if (!_started)
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
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RestoreLocation();
            if (_config.HideOnMinimized && WindowState == FormWindowState.Minimized)
                ShowInTaskbar = false;
            ApplyConfig();
            ApplyDebugLogSetting();
            ApplyLogSetting();
            ApplyProxySetting();
            if (_config.KancolleDb.On)
                _kancolleDb.Start(_config.KancolleDb.Token);
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
            _config.Save();
            _proxyManager.Shutdown();
            _kancolleDb.Stop();
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
                ApplyConfig();
        }

        private void PerformZoom()
        {
            if (_config.Zoom == 100)
                return;
            var prev = CurrentAutoScaleDimensions;
            foreach (var control in new Control[] {this, _listForm, labelLogin, labelGuide})
                control.Font = new Font(control.Font.FontFamily, control.Font.Size * _config.Zoom / 100);
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
            _sniffer.Item.MarginEquips = _config.MarginEquips;
            _sniffer.Achievement.ResetHours = _config.ResetHours;
            labelAkashiRepair.Visible = labelAkashiRepairTimer.Visible =
                labelPresetAkashiTimer.Visible = _config.UsePresetAkashi;
            if (_config.KancolleDb.On)
                _kancolleDb.Start(_config.KancolleDb.Token);
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
                    location.Y + SystemInformation.CaptionHeight) , new Size(60, 1));
            return Screen.AllScreens.Any(screen => screen.WorkingArea.Contains(rect));
        }

        private void timerMain_Tick(object sender, EventArgs e)
        {
            if (_started)
            {
                try
                {
                    UpdateTimers();
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
            if (item.RingShips)
            {
                var message = $"残り{_sniffer.Item.MaxShips - _sniffer.Item.NowShips:D}隻";
                _notificationManager.Enqueue("艦娘数超過", message);
                item.RingShips = false;
            }
        }

        private void UpdateNumOfEquips()
        {
            var item = _sniffer.Item;
            labelNumOfEquips.Text = $"{item.NowEquips:D}/{item.MaxEquips:D}";
            labelNumOfEquips.ForeColor = item.TooManyEquips ? CUDColor.Red : Color.Black;
            if (item.RingEquips)
            {
                var message = $"残り{_sniffer.Item.MaxEquips - _sniffer.Item.NowEquips:D}個";
                _notificationManager.Enqueue("装備数超過", message);
                item.RingEquips = false;
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
            if (_sniffer.BadlyDamagedShips.Any())
                _notificationManager.Enqueue("大破警告", string.Join(" ", _sniffer.BadlyDamagedShips));
        }

        private void NotifyAkashiTimer()
        {
            var akashi = _sniffer.AkashiTimer;
            var msgs = akashi.GetNotice();
            if (msgs.Length == 0)
                return;
            if (!akashi.CheckReparing() && !(akashi.CheckPresetReparing() && _config.UsePresetAkashi))
                return;
            if (msgs[0].Proceeded == "20分経過しました。")
            {
                _notificationManager.Enqueue("泊地修理20分経過", msgs[0].Proceeded);
                msgs[0].Proceeded = "";
                // 修理完了がいるかもしれないので続ける
            }
            for (var i = 0; i < ShipInfo.FleetCount; i++)
            {
                if (msgs[i].Proceeded != "")
                    _notificationManager.Enqueue("泊地修理進行", i, msgs[i].Proceeded);
                if (msgs[i].Completed != "")
                    _notificationManager.Enqueue("泊地修理完了", i, msgs[i].Completed);
            }
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
            if (_sniffer.Battle.BattleState == BattleState.None)
                return;
            panelBattleInfo.BringToFront();
            var battle = _sniffer.Battle;
            labelFormation.Text = battle.Formation;
            UpdateBattleFighterPower();
            if (_config.AlwaysShowResultRank)
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
            _ndockFinishTimeMode = !_ndockFinishTimeMode;
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
            _missionFinishTimeMode = !_missionFinishTimeMode;
            UpdateTimers();
        }

        private void UpdateTimers()
        {
            foreach (var entry in
                new[] {labelMission1, labelMission2, labelMission3}.Zip(_sniffer.Missions,
                    (label, mission) => new {label, mission.Name, mission.Timer}))
            {
                entry.Timer.Update();
                SetTimerColor(entry.label, entry.Timer);
                entry.label.Text = entry.Timer.ToString(_missionFinishTimeMode);
                if (!entry.Timer.NeedRing)
                    continue;
                _notificationManager.Enqueue("遠征終了", entry.Name);
                entry.Timer.NeedRing = false;
            }
            for (var i = 0; i < _sniffer.NDock.Length; i++)
            {
                var entry = _sniffer.NDock[i];
                entry.Timer.Update();
                _shipLabels.SetNDockTimer(i, entry.Timer, _ndockFinishTimeMode);
                if (!entry.Timer.NeedRing)
                    continue;
                _notificationManager.Enqueue("入渠終了", entry.Name);
                entry.Timer.NeedRing = false;
            }
            var kdock = new[] {labelConstruct1, labelConstruct2, labelConstruct3, labelConstruct4};
            for (var i = 0; i < kdock.Length; i++)
            {
                var timer = _sniffer.KDock[i];
                timer.Update();
                SetTimerColor(kdock[i], timer);

                kdock[i].Text = timer.EndTime == DateTime.MinValue ? "" : timer.Rest.ToString(@"hh\:mm\:ss");
                if (!timer.NeedRing)
                    continue;
                _notificationManager.Enqueue("建造完了", $"第{i + 1:D}ドック");
                timer.NeedRing = false;
            }
            UpdateCondTimers();
            UpdateAkashiTimer();
        }

        private void SetTimerColor(Label label, RingTimer timer)
        {
            label.ForeColor = timer.IsFinished ? CUDColor.Red : Color.Black;
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
            var now = DateTime.Now;
            if (timer == DateTime.MinValue)
            {
                labelCondTimerTitle.Text = "";
                labelCondTimer.Text = "";
                return;
            }
            var span = TimeSpan.FromSeconds(Ceiling((timer - now).TotalSeconds));
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
            var notice = _sniffer.GetConditionNotice();
            if (notice == null)
                return;
            for (var i = 0; i < ShipInfo.FleetCount; i++)
            {
                if (!_config.NotifyConditions.Contains(notice[i]))
                    return;
                _notificationManager.Enqueue("疲労回復" + notice[i], i, "cond" + notice[i]);
            }
        }

        private void UpdateAkashiTimer()
        {
            if (_config.UsePresetAkashi)
                UpdatePresetAkashiTimer();
            _shipLabels.SetAkashiTimer(_sniffer.GetShipStatuses(_currentFleet),
                _sniffer.AkashiTimer.GetTimers(_currentFleet));
            NotifyAkashiTimer();
        }

        private void UpdatePresetAkashiTimer()
        {
            var akashi = _sniffer.AkashiTimer;
            var span = akashi.PresetDeckTimer;
            var color = span == TimeSpan.Zero && akashi.CheckPresetReparing() ? CUDColor.Red : DefaultForeColor;
            var text = span == TimeSpan.MinValue ? "" : span.ToString(@"mm\:ss");
            labelAkashiRepairTimer.ForeColor = color;
            labelAkashiRepairTimer.Text = text;
            if (akashi.CheckPresetReparing() && !akashi.CheckReparing(_currentFleet))
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
                }
                else
                {
                    category[i].BackColor = DefaultBackColor;
                    name[i].Text = progress[i].Text = "";
                }
            }
        }

        private void Ring(string balloonTitle, string balloonMessage, string name)
        {
            if (_config.FlashWindow && (_config.Notifications[name] & NotificationType.FlashWindow) != 0)
                Win32API.FlashWindow(Handle);
            if (_config.ShowBaloonTip && (_config.Notifications[name] & NotificationType.ShowBaloonTip) != 0)
                notifyIconMain.ShowBalloonTip(20000, balloonTitle, balloonMessage, ToolTipIcon.Info);
            if (_config.PlaySound && (_config.Notifications[name] & NotificationType.PlaySound) != 0)
                PlaySound(_config.Sounds[name], _config.Sounds.Volume);
            if (_config.Pushbullet.On && (_config.Notifications[name] & NotificationType.Pushbullet) != 0)
            {
                Task.Run(() =>
                {
                    PushBullet.PushNote(_config.Pushbullet.Token, balloonTitle, balloonMessage);
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
                labelBucketHistoryButton.BackColor = SystemColors.ActiveCaption;
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
                labelMaterialHistoryButton.BackColor = SystemColors.ActiveCaption;
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
                labelRepairListButton.BackColor = SystemColors.ActiveCaption;
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