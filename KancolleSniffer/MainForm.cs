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
        private readonly ConfigDialog _configDialog = new ConfigDialog();
        private readonly int _labelRightDistance;
        private int _currentFleet;
        private readonly Label[] _labelCheckFleets;
        private bool _started;

        public MainForm()
        {
            InitializeComponent();
            FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
            FiddlerApplication.BeforeResponse += FiddlerApplication_BeforeResponse;
            _wmp.PlayStateChange += new EventHandler(_wmp_PlayStateChange);
            _configDialog.Tag = _config;
            _labelRightDistance = labelHP1.Parent.Width - labelHP1.Right;
            _labelCheckFleets = new[] {labelCheckFleet1, labelCheckFleet2, labelCheckFleet3, labelCheckFleet4};
            var i = 0;
            foreach (var label in new[] {labelFleet1, labelFleet2, labelFleet3, labelFleet4})
                label.Tag = i++;
        }

        private void FiddlerApplication_BeforeRequest(Session oSession)
        {
            var path = oSession.PathAndQuery;
            if (!path.StartsWith("/kcsapi/api_")) // 艦これのAPI以外は無視する
            {
                oSession.Ignore();
                return;
            }
            // 戦闘開始のタイミングのずれを防ぐためにバッファする
            if (path.EndsWith("api_req_sortie/battle") || path.EndsWith("api_req_practice/battle"))
                oSession.bBufferResponse = true;
        }

        private void FiddlerApplication_BeforeResponse(Session oSession)
        {
            if (!oSession.bHasResponse || !oSession.uriContains("/kcsapi/api_"))
                return;
            var response = oSession.GetResponseBodyAsString();
            if (!response.StartsWith("svdata="))
                return;
            response = response.Remove(0, "svdata=".Length);
            var json = DynamicJson.Parse(response);
            var request = oSession.GetRequestBodyAsString();
            var update = (Sniffer.Update)_sniffer.Sniff(oSession.url, request, json);
            if (update == Sniffer.Update.Start)
            {
                Invoke(new Action(() => { labelLogin.Visible = false; }));
                _started = true;
                return;
            }
            if (!_started)
                return;
            Action action = null;
            if ((update & Sniffer.Update.Item) != 0)
                action += UpdateItemInfo;
            if ((update & Sniffer.Update.Timer) != 0)
                action += UpdateTimers;
            if ((update & Sniffer.Update.NDock) != 0)
                action += UpdateNDocLabels;
            if ((update & Sniffer.Update.Mission) != 0)
                action += UpdateMissionLabels;
            if ((update & Sniffer.Update.QuestList) != 0)
                action += UpdateQuestList;
            if ((update & Sniffer.Update.Ship) != 0)
                action += UpdateShipInfo;
            if ((update & Sniffer.Update.Battle) != 0)
                action += UpdateBattleInfo;
            Invoke(action);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _config.Load();
            ApplyConfig();
            _sniffer.LoadState();
            FiddlerApplication.Startup(0, FiddlerCoreStartupFlags.RegisterAsSystemProxy);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            FiddlerApplication.Shutdown();
            _config.Save();
            _sniffer.SaveState();
        }

        private void labelHP_SizeChanged(object sender, EventArgs e)
        {
            var label = (Label)sender;
            label.Location = new Point(label.Parent.Width - _labelRightDistance - label.Width, label.Top);
        }

        private void notifyIconMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
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

        private void ApplyConfig()
        {
            TopMost = _config.TopMost;
            _wmp.settings.volume = _config.SoundVolume;
            _sniffer.Item.MarginShips = _config.MarginShips;
            _sniffer.Achievement.ResetHours = _config.ResetHours;
        }

        private void timerMain_Tick(object sender, EventArgs e)
        {
            if (_started)
                UpdateTimers();
        }

        private void UpdateItemInfo()
        {
            UpdateNumOfShips();
            var item = _sniffer.Item;
            labelNumOfEquips.Text = string.Format("{0:D}/{1:D}", item.NowItems, item.MaxItems);
            labelNumOfBuckets.Text = item.MaterialHistory[(int)Material.Bucket].Now.ToString("D");
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
            if (item.NeedRing)
            {
                var message = string.Format("残り{0:D}隻", _sniffer.Item.MaxShips - _sniffer.Item.NowShips);
                Ring("艦娘が多すぎます", message, _config.MaxShipsSoundFile);
                item.NeedRing = false;
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
            var labels = new[] { labelFuelHistory, labelBulletHistory, labelSteelHistory, labelBouxiteHistory };
            var text = new[] { "燃料", "弾薬", "鋼材", "ボーキ" };
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

        private void UpdateMissionLabels()
        {
            foreach (var entry in
                new[] {labelMissionName1, labelMissionName2, labelMissionName3}.Zip(_sniffer.Missions,
                    (label, mission) => new {label, mission.Name}))
                entry.label.Text = entry.Name;
        }

        private void UpdateNDocLabels()
        {
            foreach (var entry in
                new[] {labelRepairShip1, labelRepairShip2, labelRepairShip3, labelRepairShip4}.Zip(_sniffer.NDock,
                    (label, ndock) => new {label, ndock.Name}))
                entry.label.Text = entry.Name;
        }

        private void UpdateShipInfo()
        {
            var name = new[] {labelShip1, labelShip2, labelShip3, labelShip4, labelShip5, labelShip6};
            var lv = new[] {labelLv1, labelLv2, labelLv3, labelLv4, labelLv5, labelLv6};
            var hp = new[] {labelHP1, labelHP2, labelHP3, labelHP4, labelHP5, labelHP6};
            var cond = new[] {labelCond1, labelCond2, labelCond3, labelCond4, labelCond5, labelCond6};
            var next = new[] {labelNextLv1, labelNextLv2, labelNextLv3, labelNextLv4, labelNextLv5, labelNextLv6};

            var statuses = _sniffer.GetShipStatuses(_currentFleet);
            var empty = new ShipStatus();
            for (var i = 0; i < name.Length; i++)
            {
                var stat = i < statuses.Length ? statuses[i] : empty;
                name[i].Text = stat.Name;
                lv[i].Text = stat.Level.ToString("D");
                SetHpLavel(hp[i], stat);
                if (stat == empty)
                {
                    // SetCondLabelでは背景色が赤になってしまう
                    cond[i].Text = "0";
                    cond[i].BackColor = DefaultBackColor;
                }
                else
                    SetCondLabel(cond[i], stat.Cond);
                next[i].Text = stat.ExpToNext.ToString("D");
            }
            if (_sniffer.Battle.HasDamagedShip)
                Ring("大破した艦娘がいます", string.Join(" ", _sniffer.Battle.DamagedShipNames), _config.DamagedShipSoundFile);
            labelAirSuperiority.Text = _sniffer.GetAirSuperiority(_currentFleet).ToString("D");
            UpdateChargeInfo();
            UpdateCondTimers();
            UpdateAkashiTimer();
        }

        private void UpdateBattleInfo()
        {
            labelFormation.Text = "";
            labelEnemyAirSuperiority.Text = "";
            panelBattleInfo.Visible = _sniffer.Battle.InBattle;
            if (!_sniffer.Battle.InBattle)
                return;
            var tf = new Timer {Interval = _sniffer.Battle.DelayInFormation};
            tf.Tick += (sender, args) =>
            {
                labelFormation.Text = _sniffer.Battle.Formation;
                tf.Stop();
            };
            tf.Start();
            var ta = new Timer {Interval = _sniffer.Battle.DelayInAirSuperiority};
            ta.Tick += (sender, args) =>
            {
                labelEnemyAirSuperiority.Text = _sniffer.Battle.EnemyAirSuperiority.ToString("D");
                ta.Stop();
            };
            ta.Start();
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

        private void SetHpLavel(Label label, ShipStatus status)
        {
            var colors = new[] {DefaultBackColor, Color.FromArgb(255, 240, 240, 100), Color.Orange, Color.Red};
            label.Text = string.Format("{0:D}/{1:D}", status.NowHp, status.MaxHp);
            label.BackColor = colors[(int)status.DamageLevel];
        }

        private void SetCondLabel(Label label, int cond)
        {
            label.Text = cond.ToString("D");
            label.BackColor = cond >= 50
                ? Color.Yellow
                : cond >= 30
                    ? DefaultBackColor
                    : cond >= 20 ? Color.Orange : Color.Red;
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
                Ring("遠征が終わりました", entry.Name, _config.MissionSoundFile);
                entry.Timer.NeedRing = false;
            }
            foreach (var entry in
                new[] {labelRepair1, labelRepair2, labelRepair3, labelRepair4}.Zip(_sniffer.NDock,
                    (label, ndock) => new {label, ndock.Name, ndock.Timer}))
            {
                entry.Timer.Update();
                SetTimerLabel(entry.label, entry.Timer);
                if (!entry.Timer.NeedRing)
                    continue;
                Ring("入渠が終わりました", entry.Name, _config.NDockSoundFile);
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
                Ring("建造が終わりました", string.Format("第{0:D}ドック", i + 1), _config.KDockSoundFile);
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
            foreach (var entry in
                new[] {labelCondTimer1, labelCondTimer2, labelCondTimer3}.Zip(
                    _sniffer.GetConditionTimers(_currentFleet), (label, timer) => new {label, timer}))
                entry.label.Text = entry.timer;
        }

        private void UpdateAkashiTimer()
        {
            var stat = _sniffer.GetShipStatuses(_currentFleet);
            if (stat.Length == 0 || !stat[0].Name.StartsWith("明石"))
            {
                labelAkashiTimer.Visible = false;
                return;
            }
            labelAkashiTimer.Visible = true;
            var start = _sniffer.GetAkashiStartTime(_currentFleet);
            if (start == DateTime.MinValue)
            {
                labelAkashiTimer.Text = "00:00:00";
                return;
            }
            var span = DateTime.Now - start;
            labelAkashiTimer.Text = span.Days == 0 ? span.ToString(@"hh\:mm\:ss") : span.ToString(@"d\.hh\:mm");
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

        private void Ring(string baloonTitle, string baloonMessage, string soundFile)
        {
            if (_config.FlashWindow)
                Win32API.FlashWindow(Handle);
            if (_config.ShowBaloonTip)
                notifyIconMain.ShowBalloonTip(20000, baloonTitle, baloonMessage, ToolTipIcon.Info);
            if (_config.PlaySound && File.Exists(soundFile))
            {
                _wmp.URL = soundFile;
                _wmp.controls.play();
            }
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

        private void labelResetAchievement_Click(object sender, EventArgs e)
        {
            _sniffer.Achievement.Reset();
            UpdateItemInfo();
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
    }
}