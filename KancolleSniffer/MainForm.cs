// Copyright (C) 2013 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using Codeplex.Data;
using Fiddler;

namespace KancolleSniffer
{
    public partial class MainForm : Form
    {
        private readonly Dictionary<int, string> _missions = new Dictionary<int, string>();
        private readonly string[] _missionNames = new string[3];
        private readonly RingTimer[] _missionTimers = new RingTimer[3];
        private readonly RingTimer[] _ndocTimers = new RingTimer[4];
        private readonly int[] _ndocShips = new int[4];
        private readonly RingTimer[] _kdocTimers = new RingTimer[4];
        private int _maxShips;
        private int _nowShips;
        private int _maxItems;
        private int _nowItems;
        private readonly int[] _deckShips = new int[6];
        private readonly Dictionary<int, ShipState> _shipStatuses = new Dictionary<int, ShipState>();
        private readonly Dictionary<int, string> _shipNames = new Dictionary<int, string>();
        private readonly SortedDictionary<int, QuestState> _questList = new SortedDictionary<int, QuestState>();
        private DateTime _questLastUpdated;
        private bool _slotRinged;
        private bool _updateCond;
        private DateTime[] _condEndTime = new DateTime[3];

        private readonly string _shipNamesFile =
            Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "shipnames.json");

        private readonly string _missionsFile =
            Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "missions.json");

        private struct ShipState
        {
            public int ShipId { get; set; }
            public int Level { get; set; }
            public int ExpToNext { get; set; }
            public int MaxHp { get; set; }
            public int NowHp { get; set; }
            public int Cond { get; set; }
        }

        private struct QuestState
        {
            public string Name { get; set; }
            public int Progress { get; set; }
        }

        public MainForm()
        {
            InitializeComponent();
            FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
            for (var i = 0; i < _missionTimers.Length; i++)
                _missionTimers[i] = new RingTimer();
            for (var i = 0; i < _ndocTimers.Length; i++)
                _ndocTimers[i] = new RingTimer();
            for (var i = 0; i < _kdocTimers.Length; i++)
                _kdocTimers[i] = new RingTimer();
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
            if (oSession.url.EndsWith("api_get_member/ship"))
            {
                ParseShipData(json);
            }
            else if (oSession.uriContains("api_get_master/mission"))
            {
                ParseMission(json);
            }
            else if (oSession.uriContains("api_get_member/ndock"))
            {
                ParseNDock(json);
                Invoke(new Action(UpdateTimers));
            }
            else if (oSession.uriContains("api_get_member/kdock"))
            {
                ParseKDock(json);
                Invoke(new Action(UpdateTimers));
            }
            else if (oSession.uriContains("api_get_member/deck"))
            {
                if (!oSession.uriContains("deck_port"))
                    _updateCond = true;
                ParseDeck(json);
                Invoke(new Action(UpdateShipInfo));
                Invoke(new Action(UpdateMissionLabels));
                Invoke(new Action(UpdateTimers));
            }
            else if (oSession.uriContains("api_get_member/basic"))
            {
                _maxShips = (int)json.api_max_chara;
                _maxItems = (int)json.api_max_slotitem;
                Invoke(new Action(UpdateSlotCount));
            }
            else if (oSession.uriContains("api_get_member/record"))
            {
                _nowShips = (int)json.api_ship[0];
                _maxShips = (int)json.api_ship[1];
                _nowItems = (int)json.api_slotitem[0];
                _maxItems = (int)json.api_slotitem[1];
                Invoke(new Action(UpdateSlotCount));
            }
            else if (oSession.uriContains("api_get_member/material"))
            {
                foreach (var entry in json)
                {
                    if ((int)entry.api_id != 6)
                        continue;
                    var backet = ((int)entry.api_value).ToString("D");
                    Invoke(new Action<string>(text => labelNumOfBuckets.Text = text), backet);
                    break;
                }
            }
            else if (oSession.uriContains("api_get_member/slotitem"))
            {
                ParseSlotItem(json);
                Invoke(new Action(UpdateSlotCount));
            }
            else if (oSession.uriContains("api_get_member/ship2"))
            {
                ParseShipStatus(json);
                Invoke(new Action(UpdateShipInfo));
            }
            else if (oSession.uriContains("api_get_member/ship3"))
            {
                var ship = json.api_ship_data;
                ParseShipStatus(ship);
                Invoke(new Action(UpdateShipInfo));
            }
            else if (oSession.uriContains("api_req_sortie/battleresult"))
            {
                if (!json.IsDefined("api_get_ship"))
                    return;
                var entry = json.api_get_ship;
                _shipNames[(int)entry.api_ship_id] = (string)entry.api_ship_name;
            }
            else if (oSession.uriContains("api_get_member/questlist"))
            {
                ParseQuestList(json);
                Invoke(new Action(UpdateQuestList));
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadNames();
            FiddlerApplication.Startup(0, FiddlerCoreStartupFlags.RegisterAsSystemProxy);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            FiddlerApplication.Shutdown();
            SaveNames();
        }

        private void timerMain_Tick(object sender, EventArgs e)
        {
            UpdateTimers();
        }

        private void ParseMission(dynamic json)
        {
            foreach (var entry in json)
                _missions[(int)entry.api_id] = (string)entry.api_name;
        }

        private void ParseShipData(dynamic json)
        {
            foreach (var entry in json)
                _shipNames[(int)entry.api_ship_id] = (string)entry.api_name;
        }

        private void LoadNames()
        {
            try
            {
                ParseMission(DynamicJson.Parse(File.ReadAllText(_missionsFile)));
            }
            catch (FileNotFoundException)
            {
            }
            try
            {
                ParseShipData(DynamicJson.Parse(File.ReadAllText(_shipNamesFile)));
            }
            catch (FileNotFoundException)
            {
            }
        }

        private void SaveNames()
        {
            var ship = from data in _shipNames select new {api_ship_id = data.Key, api_name = data.Value};
            File.WriteAllText(_shipNamesFile, DynamicJson.Serialize(ship));

            var mission = from data in _missions select new {api_id = data.Key, api_name = data.Value};
            File.WriteAllText(_missionsFile, DynamicJson.Serialize(mission));
        }

        private void ParseNDock(dynamic json)
        {
            foreach (var entry in json)
            {
                var id = (int)entry.api_id;
                _ndocTimers[id - 1].EndTime = (double)entry.api_complete_time;
                _ndocShips[id - 1] = (int)entry.api_ship_id;
            }
            Invoke(new Action(UpdateNDocLabels));
        }

        private void ParseKDock(dynamic json)
        {
            foreach (var entry in json)
            {
                var id = (int)entry.api_id;
                _kdocTimers[id - 1].EndTime = (double)entry.api_complete_time;
            }
        }

        private void ParseDeck(dynamic json)
        {
            foreach (var entry in json)
            {
                var id = (int)entry.api_id;
                if (id == 1)
                {
                    Invoke((Action<string>)(text => labelFleet1.Text = text), (string)entry.api_name);
                    for (var i = 0; i < _deckShips.Count(); i++)
                    {
                        var ship = (int)entry.api_ship[i];
                        if (_deckShips[i] != ship)
                            _updateCond = true;
                        _deckShips[i] = ship;
                    }
                    continue;
                }
                id -= 2;
                var mission = entry.api_mission;
                if (mission[0] == 0)
                {
                    _missionNames[id] = "";
                    _missionTimers[id].EndTime = 0;
                    continue;
                }
                string name;
                _missionNames[id] = _missions.TryGetValue((int)mission[1], out name) ? name : "不明";
                _missionTimers[id].EndTime = mission[2];
            }
        }

        private void ParseSlotItem(dynamic json)
        {
            _nowItems = ((object[])json).Count();
        }

        private void ParseShipStatus(dynamic json)
        {
            _shipStatuses.Clear();
            foreach (var entry in json)
            {
                var data = new ShipState
                {
                    ShipId = (int)entry.api_ship_id,
                    Level = (int)entry.api_lv,
                    ExpToNext = (int)entry.api_exp[1],
                    MaxHp = (int)entry.api_maxhp,
                    NowHp = (int)entry.api_nowhp,
                    Cond = (int)entry.api_cond
                };
                _shipStatuses[(int)entry.api_id] = data;
            }
            _nowShips = _shipStatuses.Count;
            if (!_updateCond)
                return;
            _updateCond = false;
            var cond = int.MaxValue;
            foreach (var id in _deckShips)
            {
                ShipState info;
                if (id == -1 || id == 0 || !_shipStatuses.TryGetValue(id, out info))
                    continue;
                if (info.Cond < cond)
                    cond = info.Cond;
            }
            if (cond != int.MaxValue)
                SetCondTimers(cond);
        }

        private void SetCondTimers(int cond)
        {
            _condEndTime[0] = CondTimerEndTime(cond, 30);
            _condEndTime[1] = CondTimerEndTime(cond, 40);
            _condEndTime[2] = CondTimerEndTime(cond, 49);
        }

        private DateTime CondTimerEndTime(int cond, int thresh)
        {
            return (cond < thresh) ? DateTime.Now.AddMinutes((thresh - cond + 2) / 3 * 3) : DateTime.MinValue;
        }

        private void ParseQuestList(dynamic json)
        {
            var resetTime = DateTime.Today.AddHours(5);
            if (DateTime.Now >= resetTime && _questLastUpdated < resetTime)
            {
                // 前日に未消化のデイリーを消す。
                _questList.Clear();
                _questLastUpdated = DateTime.Now;
            }
            foreach (var entry in json.api_list)
            {
                if (entry is double)
                    continue;
                var id = (int)entry.api_no;
                var state = (int)entry.api_state;
                var progress = (int)entry.api_progress_flag;
                var name = (string)entry.api_title;

                switch (progress)
                {
                    case 0:
                        break;
                    case 1:
                        progress = 50;
                        break;
                    case 2:
                        progress = 80;
                        break;
                }
                switch (state)
                {
                    case 2:
                        _questList[id] = new QuestState {Name = name, Progress = progress};
                        break;
                    case 1:
                    case 3:
                        _questList.Remove(id);
                        continue;
                }
            }
        }

        private void UpdateSlotCount()
        {
            labelNumOfShips.Text = string.Format("{0:D}/{1:D}", _nowShips, _maxShips);
            if (_maxShips == 0 || // recordよりship3の方が先なので0の場合がある。
                _nowShips < _maxShips - 4)
            {
                labelNumOfShips.ForeColor = Color.Black;
                _slotRinged = false;
            }
            else
            {
                labelNumOfShips.ForeColor = Color.Red;
                if (!_slotRinged)
                {
                    Ring();
                    _slotRinged = true;
                }
            }
            labelNumOfEquips.Text = string.Format("{0:D}/{1:D}", _nowItems, _maxItems);
        }

        private void UpdateMissionLabels()
        {
            var labels = new[] {labelMissionName1, labelMissionName2, labelMissionName3};
            for (var i = 0; i < 3; i++)
                labels[i].Text = _missionNames[i];
        }

        private void UpdateNDocLabels()
        {
            var ship = new[] {labelRepairShip1, labelRepairShip2, labelRepairShip3, labelRepairShip4};
            var i = 0;
            foreach (var id in _ndocShips)
            {
                ShipState shipStatus;
                string text;
                ship[i++].Text = id == 0
                    ? ""
                    : _shipStatuses.TryGetValue(id, out shipStatus) &&
                      _shipNames.TryGetValue(shipStatus.ShipId, out text)
                        ? text
                        : "不明";
            }
        }

        private void UpdateShipInfo()
        {
            var name = new[] {labelShip1, labelShip2, labelShip3, labelShip4, labelShip5, labelShip6};
            var lv = new[] {labelLv1, labelLv2, labelLv3, labelLv4, labelLv5, labelLv6};
            var hp = new[] {labelHP1, labelHP2, labelHP3, labelHP4, labelHP5, labelHP6};
            var cond = new[] {labelCond1, labelCond2, labelCond3, labelCond4, labelCond5, labelCond6};
            var next = new[] {labelNextLv1, labelNextLv2, labelNextLv3, labelNextLv4, labelNextLv5, labelNextLv6};

            if (_shipStatuses.Count == 0)
                return;
            for (var i = 0; i < _deckShips.Count(); i++)
            {
                var id = _deckShips[i];
                ShipState info;
                if (id == -1 || id == 0 || !_shipStatuses.TryGetValue(id, out info))
                {
                    name[i].Text = "";
                    lv[i].Text = "0";
                    hp[i].Text = "0/0";
                    hp[i].BackColor = DefaultBackColor;
                    cond[i].Text = "0";
                    cond[i].BackColor = DefaultBackColor;
                    next[i].Text = "0";
                    continue;
                }
                string text;
                name[i].Text = _shipNames.TryGetValue(info.ShipId, out text) ? text : "不明";
                lv[i].Text = info.Level.ToString("D");
                hp[i].Text = string.Format("{0:D}/{1:D}", info.NowHp, info.MaxHp);
                SetHpLavel(hp[i], info.NowHp, info.MaxHp);
                SetCondLabel(cond[i], info.Cond);
                next[i].Text = info.ExpToNext.ToString("D");
            }
            UpdateSlotCount();
        }

        private void SetHpLavel(Label label, int now, int max)
        {
            label.Text = string.Format("{0:D}/{1:D}", now, max);
            var damage = (double)now / max;
            label.BackColor = damage > 0.5 ? DefaultBackColor : damage > 0.25 ? Color.Orange : Color.Red;
        }

        private void SetCondLabel(Label label, int cond)
        {
            label.Text = cond.ToString("D");
            label.BackColor = cond >= 50
                ? Color.Yellow
                : cond >= 30 ? DefaultBackColor : cond >= 20 ? Color.Orange : Color.Red;
        }

        private void UpdateTimers()
        {
            var mission = new[] {labelMission1, labelMission2, labelMission3};
            var i = 0;
            foreach (var timer in _missionTimers)
            {
                timer.Update();
                SetTimerLabel(timer, mission[i++]);
                if (!timer.NeedRing)
                    continue;
                Ring();
                timer.NeedRing = false;
            }
            var ndock = new[] {labelRepair1, labelRepair2, labelRepair3, labelRepair4};
            i = 0;
            foreach (var timer in _ndocTimers)
            {
                timer.Update();
                SetTimerLabel(timer, ndock[i++]);
                if (!timer.NeedRing)
                    continue;
                Ring();
                timer.NeedRing = false;
            }
            var kdock = new[] {labelConstruct1, labelConstruct2, labelConstruct3, labelConstruct4};
            i = 0;
            foreach (var timer in _kdocTimers)
            {
                timer.Update();
                SetTimerLabel(timer, kdock[i++]);
                if (!timer.NeedRing)
                    continue;
                Ring();
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
            for (var i = 0; i < label.Count(); i++)
            {
                var timer = _condEndTime[i];
                label[i].Text = timer != DateTime.MinValue && timer > now ? (timer - now).ToString(@"mm\:ss") : "00:00";
            }
        }

        private void UpdateQuestList()
        {
            var name = new[] {labelQuest1, labelQuest2, labelQuest3, labelQuest4, labelQuest5};
            var progress = new[] {labelProgress1, labelProgress2, labelProgress3, labelProgress4, labelProgress5};
            var i = 0;
            foreach (var quest in _questList.Values)
            {
                if (i == progress.Count())
                    break;
                name[i].Text = quest.Name;
                progress[i++].Text = string.Format("{0:D}%", quest.Progress);
            }
            for (; i < progress.Count(); i++)
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

        private class RingTimer
        {
            private bool _ringed;
            private DateTime _endTime;
            private TimeSpan _rest;

            public double EndTime
            {
                set
                {
// ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (value != 0)
                        _endTime = new DateTime(1970, 1, 1).ToLocalTime().AddSeconds(value / 1000);
                    else
                    {
                        _endTime = DateTime.MinValue;
                        _ringed = false;
                    }
                }
            }

            public void Update()
            {
                if (_endTime == DateTime.MinValue)
                {
                    _rest = TimeSpan.Zero;
                    return;
                }
                _rest = _endTime - DateTime.Now;
                if (_rest < TimeSpan.Zero)
                    _rest = TimeSpan.Zero;
                if (_rest >= TimeSpan.FromMinutes(1) || _ringed)
                    return;
                _ringed = true;
                NeedRing = true;
            }

            public bool NeedRing { get; set; }

            public bool IsSet
            {
                get { return _endTime != DateTime.MinValue; }
            }

            public override string ToString()
            {
                return _rest.Days == 0 ? _rest.ToString(@"hh\:mm\:ss") : _rest.ToString(@"d\.hh\:mm");
            }
        }
    }
}