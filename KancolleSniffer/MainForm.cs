﻿// Copyright (C) 2013, 2014 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Codeplex.Data;
using Fiddler;

namespace KancolleSniffer
{
    public partial class MainForm : Form
    {
        private readonly Sniffer _sniffer = new Sniffer();
        private const int TipTimeout = 20000;

        public MainForm()
        {
            InitializeComponent();
            FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
        }

        private void FiddlerApplication_AfterSessionComplete(Session oSession)
        {
            if (!oSession.bHasResponse || !oSession.uriContains("/kcsapi/api_"))
                return;
            var response = oSession.GetResponseBodyAsString();
            if (!response.StartsWith("svdata="))
                return;
            response = response.Remove(0, "svdata=".Length);
            var json = DynamicJson.Parse(response);
            if (!json.IsDefined("api_data"))
                return;
            json = json.api_data;
            UpdateInfo update = _sniffer.Sniff(oSession.url, json);
            if ((update & UpdateInfo.Item) != 0)
                Invoke(new Action(UpdateItemInfo));
            if ((update & UpdateInfo.Mission) != 0)
                Invoke(new Action(UpdateMissionLabels));
            if ((update & UpdateInfo.NDock) != 0)
                Invoke(new Action(UpdateNDocLabels));
            if ((update & UpdateInfo.Ship) != 0)
                Invoke(new Action(UpdateShipInfo));
            if ((update & UpdateInfo.Quest) != 0)
                Invoke(new Action(UpdateQuestList));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _sniffer.LoadNames();
            FiddlerApplication.Startup(0, FiddlerCoreStartupFlags.RegisterAsSystemProxy);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            FiddlerApplication.Shutdown();
            _sniffer.SaveNames();
        }

        private void timerMain_Tick(object sender, EventArgs e)
        {
            UpdateTimers();
        }

        private void UpdateItemInfo()
        {
            var item = _sniffer.Item;
            labelNumOfShips.Text = string.Format("{0:D}/{1:D}", item.NowShips, item.MaxShips);
            labelNumOfShips.ForeColor = item.TooManyShips ? Color.Red : Color.Black;
            if (item.NeedRing)
            {
                Ring();
                notifyIconMain.ShowBalloonTip(TipTimeout, "艦娘が多すぎます",
                    string.Format("残り{0:D}隻", _sniffer.Item.MarginShips), ToolTipIcon.Info);
                item.NeedRing = false;
            }
            labelNumOfEquips.Text = string.Format("{0:D}/{1:D}", item.NowItems, item.MaxItems);
            labelNumOfBuckets.Text = item.NumBuckets.ToString("D");
        }

        private void UpdateMissionLabels()
        {
            var labels = new[] {labelMissionName1, labelMissionName2, labelMissionName3};
            for (var i = 0; i < labels.Length; i++)
                labels[i].Text = _sniffer.Missions[i].Name;
        }

        private void UpdateNDocLabels()
        {
            var ship = new[] {labelRepairShip1, labelRepairShip2, labelRepairShip3, labelRepairShip4};
            for (var i = 0; i < ship.Length; i++)
                ship[i].Text = _sniffer.NDock[i].Name;
        }

        private void UpdateShipInfo()
        {
            var name = new[] {labelShip1, labelShip2, labelShip3, labelShip4, labelShip5, labelShip6};
            var lv = new[] {labelLv1, labelLv2, labelLv3, labelLv4, labelLv5, labelLv6};
            var hp = new[] {labelHP1, labelHP2, labelHP3, labelHP4, labelHP5, labelHP6};
            var cond = new[] {labelCond1, labelCond2, labelCond3, labelCond4, labelCond5, labelCond6};
            var next = new[] {labelNextLv1, labelNextLv2, labelNextLv3, labelNextLv4, labelNextLv5, labelNextLv6};

            var stats = _sniffer.ShipStatuses;
            for (var i = 0; i < stats.Length; i++)
            {
                var stat = stats[i];
                name[i].Text = stat.Name;
                lv[i].Text = stat.Level.ToString("D");
                hp[i].Text = string.Format("{0:D}/{1:D}", stat.NowHp, stat.MaxHp);
                SetHpLavel(hp[i], stat.NowHp, stat.MaxHp);
                SetCondLabel(cond[i], stat.Cond);
                next[i].Text = stat.ExpToNext.ToString("D");
            }
        }

        private void SetHpLavel(Label label, int now, int max)
        {
            label.Text = string.Format("{0:D}/{1:D}", now, max);
            var damage = max == 0 ? 1 : (double)now / max;
            label.BackColor = damage > 0.5 ? DefaultBackColor : damage > 0.25 ? Color.Orange : Color.Red;
        }

        private void SetCondLabel(Label label, int cond)
        {
            label.Text = cond.ToString("D");
            label.BackColor = cond >= 50
                ? Color.Yellow
                : cond >= 30 || cond == 0
                    ? DefaultBackColor
                    : cond >= 20 ? Color.Orange : Color.Red;
        }

        private void UpdateTimers()
        {
            var mission = new[] {labelMission1, labelMission2, labelMission3};
            for (var i = 0; i < mission.Length; i++)
            {
                var timer = _sniffer.Missions[i].Timer;
                timer.Update();
                SetTimerLabel(timer, mission[i]);
                if (!timer.NeedRing)
                    continue;
                Ring();
                notifyIconMain.ShowBalloonTip(TipTimeout, "遠征が終わりました", _sniffer.Missions[i].Name, ToolTipIcon.Info);
                timer.NeedRing = false;
            }
            var ndock = new[] {labelRepair1, labelRepair2, labelRepair3, labelRepair4};
            for (var i = 0; i < ndock.Length; i++)
            {
                var timer = _sniffer.NDock[i].Timer;
                timer.Update();
                SetTimerLabel(timer, ndock[i]);
                if (!timer.NeedRing)
                    continue;
                Ring();
                notifyIconMain.ShowBalloonTip(TipTimeout, "入渠が終わりました", _sniffer.NDock[i].Name, ToolTipIcon.Info);
                timer.NeedRing = false;
            }
            var kdock = new[] {labelConstruct1, labelConstruct2, labelConstruct3, labelConstruct4};
            for (var i = 0; i < kdock.Length; i++)
            {
                var timer = _sniffer.KDock[i];
                timer.Update();
                SetTimerLabel(timer, kdock[i]);
                if (!timer.NeedRing)
                    continue;
                Ring();
                notifyIconMain.ShowBalloonTip(TipTimeout, "建造が終わりました", string.Format("第{0:D}ドック", i + 1),
                    ToolTipIcon.Info);
                timer.NeedRing = false;
            }
            UpdateCondTimers();
        }

        private void SetTimerLabel(RingTimer timer, Label label)
        {
            if (timer.NeedRing)
                label.ForeColor = Color.Red;
            if (!timer.IsSet)
                label.ForeColor = Color.Black;
            label.Text = timer.ToString();
        }

        private void UpdateCondTimers()
        {
            var label = new[] {labelCondTimer1, labelCondTimer2, labelCondTimer3};
            var now = DateTime.Now;
            for (var i = 0; i < label.Length; i++)
            {
                var timer = _sniffer.RecoveryTimes[i];
                label[i].Text = timer != DateTime.MinValue && timer > now ? (timer - now).ToString(@"mm\:ss") : "00:00";
            }
        }

        private void UpdateQuestList()
        {
            var name = new[] {labelQuest1, labelQuest2, labelQuest3, labelQuest4, labelQuest5};
            var progress = new[] {labelProgress1, labelProgress2, labelProgress3, labelProgress4, labelProgress5};
            var i = 0;
            foreach (var quest in _sniffer.Quests)
            {
                if (i == progress.Length)
                    break;
                name[i].Text = quest.Name;
                progress[i++].Text = string.Format("{0:D}%", quest.Progress);
            }
            for (; i < progress.Length; i++)
            {
                name[i].Text = "";
                progress[i].Text = "";
            }
        }

        private void Ring()
        {
            SystemSounds.Asterisk.Play();
            var info = new FLASHWINFO();
            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            info.hwnd = Handle;
            info.dwFlags = 3; // FLASHW_ALL
            info.uCount = 3;
            info.dwTimeout = 0;
            FlashWindowEx(ref info);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        [DllImport("user32.dll")]
        private static extern Int32 FlashWindowEx(ref FLASHWINFO pwfi);
    }
}