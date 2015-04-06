// Copyright (C) 2013, 2014, 2015 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
// 
// This program is part of KancolleSniffer.
//
// KancolleSniffer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Codeplex.Data;
using Fiddler;

namespace KancolleSniffer
{
    public partial class MainForm : Form
    {
        private readonly Sniffer _sniffer = new Sniffer();
        private readonly Config _config = new Config();
        private readonly ConfigDialog _configDialog;
        private int _currentFleet;
        private readonly Label[] _labelCheckFleets;
        private readonly ShipLabels _shipLabels;
        private readonly ShipListForm _shipListForm;
        private readonly NoticeQueue _noticeQueue;
        private bool _started;
        private string _debugLogFile;
        private IEnumerator<string> _playLog;
        private LogServer _logServer;
        private readonly ProxyConfig _prevProxy = new ProxyConfig();

        public MainForm()
        {
            InitializeComponent();
            FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
            FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
            _configDialog = new ConfigDialog(_config, this);
            _labelCheckFleets = new[] {labelCheckFleet1, labelCheckFleet2, labelCheckFleet3, labelCheckFleet4};

            // この時点でAutoScaleDimensions == CurrentAutoScaleDimensionsなので、
            // MainForm.Designer.csのAutoScaleDimensionsの6f,12fを使う。
            ShipLabel.ScaleFactor = new SizeF(CurrentAutoScaleDimensions.Width / 6f, CurrentAutoScaleDimensions.Height / 12f);

            SetupFleetClick();
            _shipLabels = new ShipLabels();
            _shipLabels.CreateAkashiTimers(panelShipInfo);
            _shipLabels.CreateLabels(panelShipInfo, ShowShipOnShipList);
            _shipLabels.CreateDamagedShipList(panelDamagedShipList);
            _shipLabels.CreateNDockLabels(panelDock);
            _shipListForm = new ShipListForm(_sniffer, _config) {Owner = this};
            _noticeQueue = new NoticeQueue(Ring);
        }

        private void FiddlerApplication_BeforeRequest(Session oSession)
        {
            var path = oSession.PathAndQuery;
            var proxy = _config.Proxy;
            if (proxy.UseUpstream && (path.StartsWith("/kcsapi/api_") ||
                                      // この二つはMyFleetGirlsに必要
                                      path.StartsWith("/kcs/resources/") || path.StartsWith("/kcs/sound/")))
                oSession["x-overrideGateway"] = string.Format("localhost:{0:D}", proxy.UpstreamPort); // 上流プロキシを設定する
            if (!path.StartsWith("/kcsapi/api_")) // 艦これのAPI以外は無視する
                oSession.Ignore();
        }

        private void FiddlerApplication_AfterSessionComplete(Session oSession)
        {
            if (!oSession.bHasResponse || !oSession.uriContains("/kcsapi/api_"))
                return;
            Invoke(new Action<Session>(ProcessRequest), oSession);
        }

        private void ProcessRequest(Session session)
        {
            var response = session.GetResponseBodyAsString();
            if (!response.StartsWith("svdata="))
                return;
            response = response.Remove(0, "svdata=".Length);
            var json = DynamicJson.Parse(response);
            var request = session.GetRequestBodyAsString();
            if (_debugLogFile != null)
            {
                File.AppendAllText(_debugLogFile,
                    string.Format("url: {0}\nrequest: {1}\nresponse: {2}\n", session.url, request, json.ToString()));
            }
            UpdateInfo(_sniffer.Sniff(session.url, request, json));
        }

        private void UpdateInfo(Sniffer.Update update)
        {
            if (update == Sniffer.Update.Start)
            {
                labelLogin.Visible = false;
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
            _config.Load();
            RestoreLocation();
            if (_config.HideOnMinimized && WindowState == FormWindowState.Minimized)
                ShowInTaskbar = false;
            ApplyConfig();
            ApplyDebugLogSetting();
            ApplyLogSetting();
            _sniffer.LoadState();
            StartProxy();
        }

        private void StartProxy()
        {
            if (_config.Proxy.Auto)
                FiddlerApplication.Startup(0, FiddlerCoreStartupFlags.RegisterAsSystemProxy);
            else
                FiddlerApplication.Startup(_config.Proxy.Listen, FiddlerCoreStartupFlags.None);
            _prevProxy.Auto = _config.Proxy.Auto;
            _prevProxy.Listen = _config.Proxy.Listen;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;
            _config.Location = (WindowState == FormWindowState.Normal ? Bounds : RestoreBounds).Location;
            _config.Save();
            ShutdownProxy();
            if (_logServer != null)
                _logServer.Stop();
        }

        private void ShutdownProxy()
        {
            FiddlerApplication.Shutdown();
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
        }

        public void ApplyDebugLogSetting()
        {
            _debugLogFile = _config.DebugLogging ? _config.DebugLogFile : null;
        }

        public void ApplyProxySetting()
        {
            if (_config.Proxy.Auto == _prevProxy.Auto && _config.Proxy.Listen == _prevProxy.Listen)
                return;
            ShutdownProxy();
            StartProxy();
        }

        public void ApplyLogSetting()
        {
            if (_logServer != null && (!_config.Log.ServerOn || _config.Log.Listen != _logServer.Port))
            {
                _logServer.Stop();
                _logServer = null;
            }
            if (_logServer == null && _config.Log.ServerOn)
            {
                _logServer = new LogServer(_config.Log.Listen);
                _logServer.Start();
            }
            if (_logServer != null)
                _logServer.OutputDir = _config.Log.OutputDir;
            _sniffer.EnableLog(_config.Log.On ? LogType.All : LogType.None);
            _sniffer.MaterialLogInterval = _config.Log.MaterialLogInterval;
            _sniffer.LogOutputDir = _config.Log.OutputDir;
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
                lines.Add(_playLog.Current.Substring(s.Count()));
            }
            labelPlayLog.Visible = !labelPlayLog.Visible;
            var json = DynamicJson.Parse(lines[2]);
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
            labelNumOfBuckets.Text = _sniffer.Item.MaterialHistory[(int)Material.Bucket].Now.ToString("D");
            UpdateBucketHistory();
            var ac = _sniffer.Achievement.Value;
            if (ac >= 10000)
                ac = 9999;
            labelAchievement.Text = ac >= 1000 ? ((int)ac).ToString("D") : ac.ToString("F1");
            UpdateMaterialHistry();
            if (_shipListForm.Visible)
                _shipListForm.UpdateList();
        }

        private void UpdateNumOfShips()
        {
            var item = _sniffer.Item;
            labelNumOfShips.Text = string.Format("{0:D}/{1:D}", item.NowShips, item.MaxShips);
            labelNumOfShips.ForeColor = item.TooManyShips ? Color.Red : Color.Black;
            if (item.RingShips)
            {
                var message = string.Format("残り{0:D}隻", _sniffer.Item.MaxShips - _sniffer.Item.NowShips);
                _noticeQueue.Enqueue("艦娘が多すぎます", message, _config.MaxShipsSoundFile);
                item.RingShips = false;
            }
        }

        private void UpdateNumOfEquips()
        {
            var item = _sniffer.Item;
            labelNumOfEquips.Text = string.Format("{0:D}/{1:D}", item.NowEquips, item.MaxEquips);
            labelNumOfEquips.ForeColor = item.TooManyEquips ? Color.Red : Color.Black;
            if (item.RingEquips)
            {
                var message = string.Format("残り{0:D}個", _sniffer.Item.MaxEquips - _sniffer.Item.NowEquips);
                _noticeQueue.Enqueue("装備が多すぎます", message, _config.MaxEquipsSoundFile);
                item.RingEquips = false;
            }
        }

        private void UpdateBucketHistory()
        {
            var count = _sniffer.Item.MaterialHistory[(int)Material.Bucket];
            var day = count.Now - count.BegOfDay;
            var week = count.Now - count.BegOfWeek;
            if (day >= 1000)
                day = 999;
            if (week >= 1000)
                week = 999;
            labelBucketHistory.Text = string.Format("{0:+#;-#;±0} 今日\n{1:+#;-#;±0} 今週", day, week);
        }

        private void UpdateMaterialHistry()
        {
            var labels = new[] {labelFuelHistory, labelBulletHistory, labelSteelHistory, labelBouxiteHistory};
            var text = new[] {"燃料", "弾薬", "鋼材", "ボーキ"};
            for (var i = 0; i < labels.Length; i++)
            {
                var count = _sniffer.Item.MaterialHistory[i];
                var day = count.Now - count.BegOfDay;
                if (day >= 100000)
                    day = 99999;
                var week = count.Now - count.BegOfWeek;
                if (week >= 100000)
                    week = 99999;
                labels[i].Text = string.Format("{0}\n{1:+#;-#;±0}\n{2:+#;-#;±0}", text[i], day, week);
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
            labelAirSuperiority.Text = _sniffer.GetAirSuperiority(_currentFleet).ToString("D");
            UpdateAkashiTimer();
            UpdateLoS();
            UpdateCondTimers();
        }

        private void NotifyDamagedShip()
        {
            if (_sniffer.Battle.HasDamagedShip)
                _noticeQueue.Enqueue("大破した艦娘がいます", string.Join(" ", _sniffer.Battle.DamagedShipNames),
                    _config.DamagedShipSoundFile);
        }

        private void NotifyAkashiTimer()
        {
            var msgs = _sniffer.GetAkashiTimerNotice();
            if (msgs.Length == 0)
                return;
            if (msgs[0] == "20分経過しました。")
            {
                _noticeQueue.Enqueue("泊地修理", msgs[0], _config.Akashi20MinSoundFile);
                return;
            }
            var fn = new[] {"第一艦隊", "第二艦隊", "第三艦隊", "第四艦隊"};
            for (var i = 0; i < fn.Length; i++)
            {
                if (msgs[i] == "")
                    continue;
                _noticeQueue.Enqueue("泊地修理 " + fn[i], msgs[i], _config.AkashiProgressSoundFile);
            }
        }

        private void UpdateLoS()
        {
            labelLoS.Text = _sniffer.GetFleetLineOfSights(_currentFleet).ToString("F1");
        }

        private void UpdateBattleInfo()
        {
            labelFormation.Text = "";
            labelEnemyAirSuperiority.Text = "";
            labelAirSuperiority.ForeColor = DefaultForeColor;
            panelBattleInfo.Visible = _sniffer.Battle.InBattle;
            if (!_sniffer.Battle.InBattle)
                return;
            panelBattleInfo.BringToFront();
            var color = new[] {DefaultForeColor, Color.Yellow, Color.Blue, Color.Green, Color.Orange, Color.Red};
            var t = new Timer {Interval = 2000}; // 艦隊が表示されるまで遅延させる
            t.Tick += (sender, args) =>
            {
                labelFormation.Text = _sniffer.Battle.Formation;
                labelEnemyAirSuperiority.Text = _sniffer.Battle.EnemyAirSuperiority.ToString("D");
                labelAirSuperiority.ForeColor = color[_sniffer.Battle.AirBattleResult + 1];
                t.Stop();
            };
            t.Start();
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

        private void UpdateTimers()
        {
            foreach (var entry in
                new[] {labelMission1, labelMission2, labelMission3}.Zip(_sniffer.Missions,
                    (label, mission) => new {label, mission.Name, mission.Timer}))
            {
                entry.Timer.Update();
                SetTimerColor(entry.label, entry.Timer);
                var rest = entry.Timer.Rest;
                entry.label.Text = rest.Days == 0 ? rest.ToString(@"hh\:mm\:ss") : rest.ToString(@"d\.hh\:mm");
                if (!entry.Timer.NeedRing)
                    continue;
                _noticeQueue.Enqueue("遠征が終わりました", entry.Name, _config.MissionSoundFile);
                entry.Timer.NeedRing = false;
            }
            for (var i = 0; i < _sniffer.NDock.Length; i++)
            {
                var entry = _sniffer.NDock[i];
                entry.Timer.Update();
                _shipLabels.SetNDockTimer(i, entry.Timer);
                if (!entry.Timer.NeedRing)
                    continue;
                _noticeQueue.Enqueue("入渠が終わりました", entry.Name, _config.NDockSoundFile);
                entry.Timer.NeedRing = false;
            }
            var kdock = new[] {labelConstruct1, labelConstruct2, labelConstruct3, labelConstruct4};
            for (var i = 0; i < kdock.Length; i++)
            {
                var timer = _sniffer.KDock[i];
                timer.Update();
                SetTimerColor(kdock[i], timer);
                kdock[i].Text = timer.Rest.ToString(@"hh\:mm\:ss");
                if (!timer.NeedRing)
                    continue;
                _noticeQueue.Enqueue("建造が終わりました", string.Format("第{0:D}ドック", i + 1), _config.KDockSoundFile);
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
            var timer = _sniffer.GetConditionTimer(_currentFleet);
            var now = DateTime.Now;
            if (timer == DateTime.MinValue)
            {
                labelCondTimerTitle.Text = "";
                labelCondTimer.Text = "";
                return;
            }
            var span = TimeSpan.FromSeconds(Math.Ceiling((timer - now).TotalSeconds));
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
                _noticeQueue.Enqueue("疲労が回復しました", fn[i] + " cond" + notice[i].ToString("D"), _config.ConditionSoundFile);
            }
        }

        private void UpdateAkashiTimer()
        {
            _shipLabels.SetAkashiTimer(_sniffer.GetShipStatuses(_currentFleet),
                _sniffer.GetAkashiTimers(_currentFleet));
            NotifyAkashiTimer();
        }

        private void UpdateDamagedShipList()
        {
            _shipLabels.SetDamagedShipList(_sniffer.DamagedShipList);
        }

        private void UpdateQuestList()
        {
            var name = new[] {labelQuest1, labelQuest2, labelQuest3, labelQuest4, labelQuest5};
            var progress = new[] {labelProgress1, labelProgress2, labelProgress3, labelProgress4, labelProgress5};

            for (var i = 0; i < name.Length; i++)
            {
                if (i < _sniffer.Quests.Length)
                {
                    name[i].Text = _sniffer.Quests[i].Name;
                    progress[i].Text = string.Format("{0:D}%", _sniffer.Quests[i].Progress);
                }
                else
                {
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
                PlaySound(soundFile, _config.SoundVolume);
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
            var fleet = (int)((Label)sender).Tag;
            if (_currentFleet == fleet)
                return;
            _currentFleet = fleet;
            foreach (var label in _labelCheckFleets)
                label.Visible = false;
            _labelCheckFleets[fleet].Visible = true;
            if (!_started)
                return;
            UpdatePanelShipInfo();
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
    }
}