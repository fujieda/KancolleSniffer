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
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Win32;
using static System.Math;
using Timer = System.Windows.Forms.Timer;

namespace KancolleSniffer
{
    public partial class MainForm : Form
    {
        private readonly Sniffer _sniffer = new Sniffer();
        private readonly Config _config = new Config();
        private readonly ConfigDialog _configDialog;
        private int _currentFleet;
        private bool _combinedFleet;
        private readonly Label[] _labelCheckFleets;
        private readonly ShipLabels _shipLabels;
        private readonly ShipListForm _shipListForm;
        private readonly NoticeQueue _noticeQueue;
        private bool _started;
        private string _debugLogFile;
        private IEnumerator<string> _playLog;
        private LogServer _logServer;
        private int _prevProxyPort;
        private readonly SystemProxy _systemProxy = new SystemProxy();
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
            _shipLabels.CreateLabels(panelShipInfo, ShowShipOnShipList);
            _shipLabels.CreateCombinedShipLabels(panelCombinedFleet, ShowShipOnShipList);
            _shipLabels.CreateDamagedShipList(panelDamagedShipList, panelDamagedShipList_Click);
            _shipLabels.CreateNDockLabels(panelDock, (obj, args) =>
            {
                _ndockFinishTimeMode = !_ndockFinishTimeMode;
                UpdateTimers();
            });
            labelPresetAkashiTimer.BackColor = ShipLabels.ColumnColors[1];
            _shipListForm = new ShipListForm(_sniffer, _config) {Owner = this};
            _noticeQueue = new NoticeQueue(Ring);
            _config.Load();
            _sniffer.LoadState();
        }

        private void HttpProxy_AfterSessionComplete(HttpProxy.Session session)
        {
            Invoke(new Action<HttpProxy.Session>(ProcessRequest), session);
        }

        private void ProcessRequest(HttpProxy.Session session)
        {
            var url = session.Request.PathAndQuery;
            var request = session.Request.BodyAsString;
            var response = session.Response.BodyAsString;
            if (!url.Contains("kcsapi/"))
                return;
            if (response == null || !response.StartsWith("svdata="))
            {
                WriteDebugLog(url, request, response);
                ShowServerError(url, request, response);
                return;
            }
            if (_config.KancolleDb.On)
                _kancolleDb.Send(url, request, response);
            response = UnescapeString(response.Remove(0, "svdata=".Length));
            WriteDebugLog(url, request, response);
            try
            {
                var update = _sniffer.Sniff(url, request, JsonParser.Parse(response));
                if (update == Sniffer.Update.Error)
                {
                    ShowServerError(url, request, response);
                    return;
                }
                UpdateInfo(update);
            }
            catch (FormatException e)
            {
                ShowServerError(url, request, response, e);
            }
            catch (RuntimeBinderException e)
            {
                if (_errorDialog.ShowDialog(this,
                    "このバージョンは現在の艦これに対応していません。\r\n新しいバージョンを利用してください。", e.ToString()) == DialogResult.Abort)
                    Application.Exit();
            }
            catch (LogIOException e)
            {
                if (_errorDialog.ShowDialog(this, e.Message, e.InnerException.ToString()) == DialogResult.Abort)
                    Application.Exit();
            }
            catch (Exception e)
            {
                if (_errorDialog.ShowDialog(this, "エラーが発生しました。", e.ToString()) == DialogResult.Abort)
                    Application.Exit();
            }
        }

        private void WriteDebugLog(string url, string request, string response)
        {
            if (_debugLogFile != null)
            {
                File.AppendAllText(_debugLogFile,
                    $"url: {url}\nrequest: {request}\nresponse: {response ?? "(null)"}\n");
            }
        }

        private void ShowServerError(string url, string request, string response, Exception e = null)
        {
            if (_errorDialog.ShowDialog(this, "サーバーからの応答が異常です。",
                $"{(e == null ? "" : e + "\r\n")}url: {url}\r\nrequest: {request}\r\nresponse: {response ?? "(null)"}\r\n") ==
                DialogResult.Abort)
                Application.Exit();
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
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            if (_config.KancolleDb.On)
                _kancolleDb.Start(_config.KancolleDb.Token);
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode != PowerModes.Resume || !_config.Proxy.Auto)
                return;
            Task.Run(() =>
            {
                for (var i = 0; i < 5; Thread.Sleep(10000), i++)
                {
                    WebResponse res = null;
                    SystemProxy.Refresh();
                    var system = WebRequest.GetSystemWebProxy();
                    if (!system.GetProxy(new Uri("http://125.6.184.16/")).IsLoopback)
                    {
                        File.AppendAllText("wakeup.log",
                            $"[{DateTime.Now.ToString("g")}] proxy settings doesn't work.\r\n");
                        continue;
                    }
                    try
                    {
                        var req = WebRequest.Create("http://kancollesniffer.osdn.jp/version");
                        res = req.GetResponse();
                        break;
                    }
                    catch
                    {
                        File.AppendAllText("wakeup.log",
                            $"[{DateTime.Now.ToString("g")}] failed to connect internet.\r\n");
                    }
                    finally
                    {
                        res?.Close();
                    }
                }
            });
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;
            _config.Location = (WindowState == FormWindowState.Normal ? Bounds : RestoreBounds).Location;
            _config.Save();
            Task.Run(() => ShutdownProxy());
            _logServer?.Stop();
            if (_config.Proxy.Auto)
                _systemProxy.RestoreSettings();
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            _kancolleDb.Stop();
        }

        private void ShutdownProxy()
        {
            HttpProxy.Shutdown();
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

        private void RestoreLocation()
        {
            if (_config.Location.X == int.MinValue)
                return;
            var newBounds = Bounds;
            newBounds.Location = _config.Location;
            if (IsVisibleOnAnyScreen(newBounds))
                Location = _config.Location;
        }

        private void ApplyConfig()
        {
            _shipListForm.TopMost = TopMost = _config.TopMost;
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
            if (!_config.Proxy.Auto)
                _systemProxy.RestoreSettings();
            if (_config.Proxy.UseUpstream)
            {
                HttpProxy.UpstreamProxyHost = "127.0.0.1";
                HttpProxy.UpstreamProxyPort = _config.Proxy.UpstreamPort;
            }
            HttpProxy.IsEnableUpstreamProxy = _config.Proxy.UseUpstream;
            var result = true;
            if (!HttpProxy.IsInListening || _config.Proxy.Listen != _prevProxyPort)
            {
                ShutdownProxy();
                result = StartProxy();
            }
            if (_config.Proxy.Auto && result)
            {
                _systemProxy.SetAutoProxyUrl(HttpProxy.LocalPort == 8080
                    ? ProxyConfig.AutoConfigUrl
                    : ProxyConfig.AutoConfigUrlWithPort + HttpProxy.LocalPort);
            }
            _prevProxyPort = _config.Proxy.Listen;
            return result;
        }

        private bool StartProxy()
        {
            try
            {
                HttpProxy.Startup(_config.Proxy.Listen, false, false);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.AddressAlreadyInUse)
                    throw;
                if (WarnConflictPortNumber("プロキシサーバー", _config.Proxy.Listen, _config.Proxy.Auto) == DialogResult.No ||
                    !_config.Proxy.Auto)
                {
                    _systemProxy.RestoreSettings();
                    return false;
                }
                HttpProxy.Startup(0, false, false);
                _config.Proxy.Listen = HttpProxy.LocalPort;
            }
            return true;
        }

        private DialogResult WarnConflictPortNumber(string name, int port, bool auto)
        {
            var msg = $"{name}のポート番号{port}は他のアプリケーションが使用中です。";
            var cap = "ポート番号の衝突";
            return auto
                ? MessageBox.Show(this, msg + "自動的に別の番号を割り当てますか？", cap,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                : MessageBox.Show(this, msg + "設定ダイアログでポート番号を変更してください。", cap,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public bool ApplyLogSetting()
        {
            var result = true;
            if (_config.Log.ServerOn)
            {
                result = StartLogServer();
            }
            else
            {
                _logServer?.Stop();
                _logServer = null;
            }
            _sniffer.EnableLog(_config.Log.On ? LogType.All : LogType.None);
            _sniffer.MaterialLogInterval = _config.Log.MaterialLogInterval;
            _sniffer.LogOutputDir = _config.Log.OutputDir;
            return result;
        }

        private bool StartLogServer()
        {
            var port = _config.Log.Listen;
            if (_logServer?.Port == port)
                return true;
            _logServer?.Stop();
            _logServer = null;
            try
            {
                _logServer = new LogServer(port);
                _logServer.Start();
            }
            catch (SocketException e) when (e.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
                if (WarnConflictPortNumber("閲覧サーバー", port, true) == DialogResult.No)
                    return false;
                _logServer = new LogServer(0);
                _logServer.Start();
                _config.Log.Listen = _logServer.Port;
            }
            _logServer.OutputDir = _config.Log.OutputDir;
            return true;
        }

        public static bool IsVisibleOnAnyScreen(Rectangle rect)
        {
            return Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(rect));
        }

        private void timerMain_Tick(object sender, EventArgs e)
        {
            if (_started)
                UpdateTimers();
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
                if (!_playLog.MoveNext() || !_playLog.Current.StartsWith(s))
                {
                    labelPlayLog.Visible = false;
                    return;
                }
                lines.Add(_playLog.Current.Substring(s.Length));
            }
            labelPlayLog.Visible = !labelPlayLog.Visible;
            var json = JsonParser.Parse(lines[2]);
            UpdateInfo(_sniffer.Sniff(lines[0], lines[1], json));
        }

        private void ShowShipOnShipList(object sender, EventArgs ev)
        {
            if (!_shipListForm.Visible)
                return;
            var idx = (int)((Control)sender).Tag;
            var statuses = _sniffer.GetShipStatuses(_currentFleet);
            if (statuses.Length <= idx)
                return;
            _shipListForm.ShowShip(statuses[idx].Id);
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
            if (_shipListForm.Visible)
                _shipListForm.UpdateList();
        }

        private void UpdateNumOfShips()
        {
            var item = _sniffer.Item;
            labelNumOfShips.Text = $"{item.NowShips:D}/{item.MaxShips:D}";
            labelNumOfShips.ForeColor = item.TooManyShips ? Color.Red : Color.Black;
            if (item.RingShips)
            {
                var message = $"残り{_sniffer.Item.MaxShips - _sniffer.Item.NowShips:D}隻";
                _noticeQueue.Enqueue("艦娘が多すぎます", message, _config.Sounds["艦娘数超過"]);
                item.RingShips = false;
            }
        }

        private void UpdateNumOfEquips()
        {
            var item = _sniffer.Item;
            labelNumOfEquips.Text = $"{item.NowEquips:D}/{item.MaxEquips:D}";
            labelNumOfEquips.ForeColor = item.TooManyEquips ? Color.Red : Color.Black;
            if (item.RingEquips)
            {
                var message = $"残り{_sniffer.Item.MaxEquips - _sniffer.Item.NowEquips:D}個";
                _noticeQueue.Enqueue("装備が多すぎます", message, _config.Sounds["装備数超過"]);
                item.RingEquips = false;
            }
        }

        private void UpdateBucketHistory()
        {
            var count = _sniffer.Material.MaterialHistory[(int)Material.Bucket];
            var day = count.Now - count.BegOfDay;
            var week = count.Now - count.BegOfWeek;
            if (day >= 1000)
                day = 999;
            if (week >= 1000)
                week = 999;
            labelBucketHistory.Text = $"{day:+#;-#;±0} 今日\n{week:+#;-#;±0} 今週";
        }

        private void UpdateMaterialHistry()
        {
            var labels = new[] {labelFuelHistory, labelBulletHistory, labelSteelHistory, labelBouxiteHistory};
            var text = new[] {"燃料", "弾薬", "鋼材", "ボーキ"};
            for (var i = 0; i < labels.Length; i++)
            {
                var count = _sniffer.Material.MaterialHistory[i];
                var port = count.Now - _sniffer.Material.PrevPort[i];
                if (port >= 100000)
                    port = 99999;
                var day = count.Now - count.BegOfDay;
                if (day >= 100000)
                    day = 99999;
                var week = count.Now - count.BegOfWeek;
                if (week >= 100000)
                    week = 99999;
                labels[i].Text = $"{text[i]}\n{port:+#;-#;±0}\n{day:+#;-#;±0}\n{week:+#;-#;±0}";
            }
        }

        private void UpdateShipInfo()
        {
            UpdatePanelShipInfo();
            NotifyDamagedShip();
            UpdateChargeInfo();
            UpdateDamagedShipList();
            if (_shipListForm.Visible)
                _shipListForm.UpdateList();
        }

        private void UpdatePanelShipInfo()
        {
            var statuses = _sniffer.GetShipStatuses(_currentFleet);
            _shipLabels.SetShipInfo(statuses);
            if (_sniffer.CombinedFleetType == 0)
                _combinedFleet = false;
            labelFleet1.Text = _combinedFleet ? "連合" : "第一";
            panelCombinedFleet.Visible = _combinedFleet;
            if (_combinedFleet)
                _shipLabels.SetCombinedShipInfo(_sniffer.GetShipStatuses(0), _sniffer.GetShipStatuses(1));
            UpdateAkashiTimer();
            UpdateFighterPower();
            UpdateLoS();
            UpdateCondTimers();
        }

        private void NotifyDamagedShip()
        {
            if (_sniffer.BadlyDamagedShips.Any())
                _noticeQueue.Enqueue("大破した艦娘がいます", string.Join(" ", _sniffer.BadlyDamagedShips),
                    _config.Sounds["大破警告"]);
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
                _noticeQueue.Enqueue("泊地修理", msgs[0].Proceeded, _config.Sounds["泊地修理20分経過"]);
                msgs[0].Proceeded = "";
                // 修理完了がいるかもしれないので続ける
            }
            var fn = new[] {"第一艦隊", "第二艦隊", "第三艦隊", "第四艦隊"};
            for (var i = 0; i < fn.Length; i++)
            {
                if (msgs[i].Proceeded != "")
                    _noticeQueue.Enqueue("泊地修理 " + fn[i], "修理進行：" + msgs[i].Proceeded, _config.Sounds["泊地修理進行"]);
                if (msgs[i].Completed != "")
                    _noticeQueue.Enqueue("泊地修理 " + fn[i], "修理完了：" + msgs[i].Completed, _config.Sounds["泊地修理完了"]);
            }
        }

        public void UpdateFighterPower()
        {
            var fp = _sniffer.GetFighterPower(_currentFleet);
            labelFighterPower.Text = fp[0].ToString("D");
            toolTipFighterPower.SetToolTip(labelFighterPower, fp[0] == fp[1] ? $"{fp[0]}" : $"{fp[0]}～{fp[1]}");
        }

        private void UpdateLoS()
        {
            labelLoS.Text = _sniffer.GetFleetLineOfSights(_currentFleet).ToString("F1");
        }

        private void UpdateBattleInfo()
        {
            labelFormation.Text = "";
            labelEnemyFighterPower.Text = "";
            labelFighterPower.ForeColor = DefaultForeColor;
            labelResultRank.Text = "判定";
            panelBattleInfo.Visible = _sniffer.Battle.InBattle;
            if (!_sniffer.Battle.InBattle)
                return;
            panelBattleInfo.BringToFront();
            var battle = _sniffer.Battle;
            var color = new[] {DefaultForeColor, DefaultForeColor, Color.Blue, Color.Green, Color.Orange, Color.Red};
            labelFormation.Text = battle.Formation;
            var enemyFp = battle.EnemyFighterPower;
            labelEnemyFighterPower.Text = (enemyFp & BattleInfo.IncollectFighterPowerFlag) == 0
                ? enemyFp.ToString()
                : (enemyFp & ~BattleInfo.IncollectFighterPowerFlag) + "+";
            labelFighterPower.ForeColor = color[battle.AirControlLevel + 1];
            if (_config.AlwaysShowResultRank)
                ShowResultRank();
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
                var rest = entry.Timer.Rest;
                entry.label.Text =
                    entry.Timer.EndTime == DateTime.MinValue
                        ? ""
                        : _missionFinishTimeMode
                            ? entry.Timer.EndTime.ToString(@"dd\ HH\:mm")
                            : rest.ToString(@"hh\:mm\:ss");
                if (!entry.Timer.NeedRing)
                    continue;
                _noticeQueue.Enqueue("遠征が終わりました", entry.Name, _config.Sounds["遠征終了"]);
                entry.Timer.NeedRing = false;
            }
            for (var i = 0; i < _sniffer.NDock.Length; i++)
            {
                var entry = _sniffer.NDock[i];
                entry.Timer.Update();
                _shipLabels.SetNDockTimer(i, entry.Timer, _ndockFinishTimeMode);
                if (!entry.Timer.NeedRing)
                    continue;
                _noticeQueue.Enqueue("入渠が終わりました", entry.Name, _config.Sounds["入渠終了"]);
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
                _noticeQueue.Enqueue("建造が終わりました", $"第{i + 1:D}ドック", _config.Sounds["建造完了"]);
                timer.NeedRing = false;
            }
            UpdateCondTimers();
            UpdateAkashiTimer();
        }

        private void SetTimerColor(Label label, RingTimer timer)
        {
            label.ForeColor = timer.IsFinished ? Color.Red : Color.Black;
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
            }
            else
            {
                labelCondTimerTitle.Text = "cond49まで";
                labelCondTimer.Text = (span >= TimeSpan.Zero ? span : TimeSpan.Zero).ToString(@"mm\:ss");
            }
            var notice = _sniffer.GetConditionNotice();
            if (notice == null)
                return;
            var fn = new[] {"第一艦隊", "第二艦隊", "第三艦隊", "第四艦隊"};
            for (var i = 0; i < fn.Length; i++)
            {
                if (!_config.NotifyConditions.Contains(notice[i]))
                    return;
                _noticeQueue.Enqueue("疲労が回復しました", fn[i] + " cond" + notice[i].ToString("D"), _config.Sounds["疲労回復"]);
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
            var color = span == TimeSpan.Zero && akashi.CheckPresetReparing() ? Color.Red : DefaultForeColor;
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

        private void UpdateDamagedShipList()
        {
            _shipLabels.SetDamagedShipList(_sniffer.DamagedShipList);
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
            var color = new[]
            {
                Color.FromArgb(60, 141, 76), Color.FromArgb(232, 57, 41), Color.FromArgb(136, 204, 120),
                Color.FromArgb(52, 147, 185), Color.FromArgb(220, 198, 126), Color.FromArgb(168, 111, 76),
                Color.FromArgb(200, 148, 231)
            };

            var quests = _sniffer.Quests;
            for (var i = 0; i < name.Length; i++)
            {
                if (i < quests.Length)
                {
                    category[i].BackColor = color[quests[i].Category - 1];
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

        private class NoticeQueue
        {
            private readonly Action<string, string, string> _ring;
            private readonly Queue<Tuple<string, string, string>> _queue = new Queue<Tuple<string, string, string>>();
            private readonly Timer _timer = new Timer {Interval = 2000};

            public NoticeQueue(Action<string, string, string> ring)
            {
                _ring = ring;
                _timer.Tick += TimerOnTick;
            }

            private void TimerOnTick(object obj, EventArgs e)
            {
                if (_queue.Count == 0)
                {
                    _timer.Stop();
                    return;
                }
                var notice = _queue.Dequeue();
                _ring(notice.Item1, notice.Item2, notice.Item3);
            }

            public void Enqueue(string title, string message, string soundFile)
            {
                if (_timer.Enabled)
                {
                    _queue.Enqueue(new Tuple<string, string, string>(title, message, soundFile));
                }
                else
                {
                    _ring(title, message, soundFile);
                    _timer.Start();
                }
            }
        }

        private void Ring(string baloonTitle, string baloonMessage, string soundFile)
        {
            if (_config.FlashWindow)
                Win32API.FlashWindow(Handle);
            if (_config.ShowBaloonTip)
                notifyIconMain.ShowBalloonTip(20000, baloonTitle, baloonMessage, ToolTipIcon.Info);
            if (_config.PlaySound)
                PlaySound(soundFile, _config.Sounds.Volume);
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

        private void labelDamgedShipListButton_Click(object sender, EventArgs e)
        {
            if (panelDamagedShipList.Visible)
            {
                panelDamagedShipList.Visible = false;
                labelDamgedShipListButton.BackColor = DefaultBackColor;
            }
            else
            {
                panelDamagedShipList.Visible = true;
                panelDamagedShipList.BringToFront();
                labelDamgedShipListButton.BackColor = SystemColors.ActiveCaption;
            }
        }

        private void panelDamagedShipList_Click(object sender, EventArgs e)
        {
            panelDamagedShipList.Visible = false;
            labelDamgedShipListButton.BackColor = DefaultBackColor;
        }

        private void ShipListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _shipListForm.UpdateList();
            _shipListForm.Show();
            if (_shipListForm.WindowState == FormWindowState.Minimized)
                _shipListForm.WindowState = FormWindowState.Normal;
            _shipListForm.Activate();
        }

        private void LogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://localhost:" + _config.Log.Listen + "/");
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