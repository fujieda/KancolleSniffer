// Copyright (C) 2013, 2014 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Codeplex.Data;
using Fiddler;

namespace KancolleSniffer
{
    public partial class MainForm : Form
    {
        private readonly Sniffer _sniffer = new Sniffer();
        private readonly dynamic _wmp = Activator.CreateInstance(Type.GetTypeFromProgID("WMPlayer.OCX.7"));
        private readonly Config _config = new Config();
        private readonly ConfigDialog _configDialog;
        private int _currentFleet;
        private readonly Label[] _labelCheckFleets;
        private readonly ShipLabel[][] _damagedShipList = new ShipLabel[14][];
        private readonly Label[] _akashiTimers = new Label[ShipInfo.MemberCount];
        private readonly ShipLabel[][] _ndockLabels = new ShipLabel[DockInfo.DockCount][];
        private readonly ShipInfoLabels _shipInfoLabels;
        private readonly ShipListForm _shipListForm;
        private readonly NoticeQueue _noticeQueue;
        private bool _started;

        public MainForm()
        {
            InitializeComponent();
            FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
            FiddlerApplication.BeforeResponse += FiddlerApplication_BeforeResponse;
            _wmp.PlayStateChange += new EventHandler(_wmp_PlayStateChange);
            _configDialog = new ConfigDialog(_config, this);
            _labelCheckFleets = new[] {labelCheckFleet1, labelCheckFleet2, labelCheckFleet3, labelCheckFleet4};

            var labels = new[] {labelFleet1, labelFleet2, labelFleet3, labelFleet4};
            for (var i = 0; i < labels.Length; i++)
                labels[i].Tag = i;
            _shipInfoLabels = new ShipInfoLabels(panelShipInfo);
            for (var i = 0; i < ShipInfo.MemberCount; i++)
            {
                var label = _shipInfoLabels.GetNameLabel(i);
                var tmp = i;
                label.Click += (o, e) => ShowShipOnShipList(tmp);
            }
            CreateDamagedShipList();
            CreateAkashiTimers();
            CreateNDockLabels();
            _shipListForm = new ShipListForm(_sniffer, _config);
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
            {
                oSession.Ignore();
                return;
            }
            // 戦闘開始のタイミングのずれを防ぐためにバッファする
            if (_sniffer.IsBattleAPI(path))
                oSession.bBufferResponse = true;
        }

        private void FiddlerApplication_BeforeResponse(Session oSession)
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
            var update = (Sniffer.Update)_sniffer.Sniff(session.url, request, json);
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
            ApplyConfig();
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
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _shipListForm.Close();
            _config.Location = (WindowState == FormWindowState.Normal ? Bounds : RestoreBounds).Location;
            _config.Save();
            _sniffer.SaveState();
            ShutdownProxy();
        }

        private void ShutdownProxy()
        {
            FiddlerApplication.Shutdown();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (_config.HideOnMinimized && WindowState == FormWindowState.Minimized)
                ShowInTaskbar = false;
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
            Application.Exit();
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
            TopMost = _config.TopMost;
            _sniffer.Item.MarginShips = _config.MarginShips;
            _sniffer.Item.MarginEquips = _config.MarginEquips;
            _sniffer.Achievement.ResetHours = _config.ResetHours;
        }

        public void ApplyLogSetting()
        {
            _sniffer.LogFile = _config.Logging ? _config.LogFile : null;
        }

        public void ApplyProxySetting()
        {
            ShutdownProxy();
            StartProxy();
        }

        public static bool IsVisibleOnAnyScreen(Rectangle rect)
        {
            return Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(rect));
        }

        private void timerMain_Tick(object sender, EventArgs e)
        {
            if (_started)
                UpdateTimers();
        }

        private void ShowShipOnShipList(int idx)
        {
            if (!_shipListForm.Visible)
                return;
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
            labelAchievement.Text = ac >= 1000 ? ac.ToString("D") : ac.ToString("F1");
            UpdateMaterialHistry();
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
            var statuses = _sniffer.GetShipStatuses(_currentFleet);
            _shipInfoLabels.SetShipInfo(statuses);
            if (_sniffer.Battle.HasDamagedShip)
                _noticeQueue.Enqueue("大破した艦娘がいます", string.Join(" ", _sniffer.Battle.DamagedShipNames),
                    _config.DamagedShipSoundFile);
            labelAirSuperiority.Text = _sniffer.GetAirSuperiority(_currentFleet).ToString("D");
            UpdateLoS();
            UpdateChargeInfo();
            UpdateCondTimers();
            UpdateAkashiTimer();
            UpdateDamagedShipList();
            _shipListForm.UpdateList();
        }

        private void UpdateLoS()
        {
            labelLoS.Text = _sniffer.GetFleetLineOfSights(_currentFleet).ToString("F1");
        }

        private void UpdateBattleInfo()
        {
            labelFormation.Text = "";
            labelEnemyAirSuperiority.Text = "";
            panelBattleInfo.Visible = _sniffer.Battle.InBattle;
            if (!_sniffer.Battle.InBattle)
                return;
            var t = new Timer {Interval = 2000}; // 艦隊が表示されるまで遅延させる
            t.Tick += (sender, args) =>
            {
                labelFormation.Text = _sniffer.Battle.Formation;
                labelEnemyAirSuperiority.Text = _sniffer.Battle.EnemyAirSuperiority.ToString("D");
                t.Stop();
            };
            t.Start();
        }

        private void UpdateChargeInfo()
        {
            var fuel = new[] {labelFuel1, labelFuel2, labelFuel3, labelFuel4};
            var bull = new[] {labelBull1, labelBull2, labelBull3, labelBull4};

            for (var i = 0; i < fuel.Length; i++)
            {
                var stat = _sniffer.ChargeStatuses[i];
                fuel[i].ImageIndex = stat.Fuel;
                bull[i].ImageIndex = stat.Bull;
            }
        }

        private void CreateNDockLabels()
        {
            var parent = panelDock;
            for (var i = 0; i < _ndockLabels.Length; i++)
            {
                var y = 3 + i * 15;
                parent.Controls.AddRange(
                    _ndockLabels[i] = new[]
                    {
                        new ShipLabel {Location = new Point(106, y), AutoSize = true, Text = "00:00:00"},
                        new ShipLabel {Location = new Point(30, y), AutoSize = true} // 名前のZ-orderを下に
                    });
            }
        }

        private void UpdateNDocLabels()
        {
            for (var i = 0; i < _ndockLabels.Length; i++)
                _ndockLabels[i][1].SetName(_sniffer.NDock[i].Name);
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
                SetTimerLabel(entry.label, entry.Timer);
                if (!entry.Timer.NeedRing)
                    continue;
                _noticeQueue.Enqueue("遠征が終わりました", entry.Name, _config.MissionSoundFile);
                entry.Timer.NeedRing = false;
            }
            for (var i = 0; i < _ndockLabels.Length; i++)
            {
                var entry = _sniffer.NDock[i];
                entry.Timer.Update();
                SetTimerLabel(_ndockLabels[i][0], entry.Timer);
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
                SetTimerLabel(kdock[i], timer);
                if (!timer.NeedRing)
                    continue;
                _noticeQueue.Enqueue("建造が終わりました", string.Format("第{0:D}ドック", i + 1), _config.KDockSoundFile);
                timer.NeedRing = false;
            }
            UpdateCondTimers();
            UpdateAkashiTimer();
        }

        private void SetTimerLabel(Label label, RingTimer timer)
        {
            label.ForeColor = timer.IsFinished ? Color.Red : Color.Black;
            label.Text = timer.ToString();
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

        private void CreateAkashiTimers()
        {
            var parent = panelShipInfo;
            parent.SuspendLayout();
            for (var i = 0; i < _akashiTimers.Length; i++)
            {
                const int width = 31, height = 12;
                const int x = 56;
                var y = 20 + 16 * i;
                parent.Controls.Add(
                    _akashiTimers[i] =
                        new Label {Location = new Point(x, y), Size = new Size(width, height), Visible = false});
                parent.Controls.SetChildIndex(_akashiTimers[i], 0);
            }
            parent.ResumeLayout();
        }

        private void UpdateAkashiTimer()
        {
            var timers = _sniffer.GetAkashiTimers(_currentFleet);
            var statuses = _sniffer.GetShipStatuses(_currentFleet);
            for (var i = 0; i < _akashiTimers.Length; i++)
            {
                var label = _akashiTimers[i];
                var labelHp = _shipInfoLabels.GetHpLabel(i);
                if (timers == null || i >= timers.Length || timers[i].Span == TimeSpan.MinValue)
                {
                    label.Visible = false;
                    labelHp.ForeColor = DefaultForeColor;
                    continue;
                }
                var timer = timers[i];
                var stat = statuses[i];
                label.Visible = true;
                label.Text = timer.Span.ToString(@"mm\:ss");
                if (timer.Diff == 0)
                {
                    labelHp.ForeColor = DefaultForeColor;
                    continue;
                }
                labelHp.ForeColor = Color.DimGray;
                labelHp.SetHp(stat.NowHp + timer.Diff, stat.MaxHp);
            }
            var msgs = _sniffer.GetAkashiTimerNotice();
            var fn = new[] {"第一艦隊", "第二艦隊", "第三艦隊", "第四艦隊"};
            for (var i = 0; i < fn.Length; i++)
            {
                if (msgs[i] == "")
                    continue;
                var sound = msgs[i] == "20分経過しました。" ? _config.Akashi20MinSoundFile : _config.AkashiProgressSoundFile;
                _noticeQueue.Enqueue("泊地修理 " + fn[i], msgs[i], sound);
            }
        }

        public void CreateDamagedShipList()
        {
            var parent = panelDamagedShipList;
            parent.SuspendLayout();
            for (var i = 0; i < _damagedShipList.Length; i++)
            {
                var y = 3 + i * 16;
                const int height = 12;
                parent.Controls.AddRange(_damagedShipList[i] = new[]
                {
                    new ShipLabel {Location = new Point(1, y), Size = new Size(11, height)},
                    new ShipLabel {Location = new Point(79, y), Size = new Size(45, height)},
                    new ShipLabel {Location = new Point(123, y), Size = new Size(5, height - 1)},
                    new ShipLabel {Location = new Point(10, y), AutoSize = true}
                });
            }
            parent.ResumeLayout();
        }

        private void UpdateDamagedShipList()
        {
            const int fleet = 0, name = 3, time = 1, damage = 2;
            var parent = panelDamagedShipList;
            var list = _sniffer.DamagedShipList;
            var num = Math.Min(list.Length, _damagedShipList.Length);
            const int width = 134;
            if (num == 0)
            {
                parent.Size = new Size(width, 19);
                var labels = _damagedShipList[0];
                labels[fleet].Text = "";
                labels[name].SetName("なし");
                labels[time].Text = "";
                labels[damage].BackColor = DefaultBackColor;
                return;
            }
            parent.Size = new Size(width, num * 16 + 3);
            var fn = new[] {"", "1", "2", "3", "4"};
            var colors = new[] {DefaultBackColor, Color.FromArgb(255, 225, 225, 21), Color.Orange, Color.Red};
            for (var i = 0; i < num; i++)
            {
                var e = list[i];
                var labels = _damagedShipList[i];
                labels[fleet].Text = fn[e.Fleet + 1];
                labels[name].SetName(e.Name);
                var t = e.RepairTime;
                labels[time].Text = string.Format(@"{0:d2}:{1:mm\:ss}", (int)t.TotalHours, t);
                labels[damage].BackColor = colors[(int)e.DamageLevel];
            }
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

        public void PlaySound(string file, int volume)
        {
            if (!File.Exists(file))
                return;
            _wmp.settings.volume = volume;
            _wmp.URL = file;
            _wmp.controls.play();
        }

        private void _wmp_PlayStateChange(object sender, EventArgs e)
        {
            if (_wmp.playState == 8) // MediaEnded
                _wmp.URL = ""; // 再生したファイルが差し替えできなくなるのを防ぐ。
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
            UpdateShipInfo();
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
                labelDamgedShipListButton.BackColor = SystemColors.ActiveCaption;
            }
        }

        private void ShipListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _shipListForm.Show();
            _shipListForm.UpdateList();
            _shipListForm.Activate();
        }
    }
}