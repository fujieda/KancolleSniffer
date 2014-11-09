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
using System.IO;

namespace KancolleSniffer
{
    public class Sniffer
    {
        private bool _start;
        private readonly ShipMaster _shipMaster = new ShipMaster();
        private readonly ItemInfo _itemInfo = new ItemInfo();
        private readonly QuestInfo _questInfo = new QuestInfo();
        private readonly MissionInfo _missionInfo = new MissionInfo();
        private readonly ShipInfo _shipInfo;
        private readonly DockInfo _dockInfo;
        private readonly AkashiTimer _akashiTimer;
        private readonly Achievement _achievement = new Achievement();
        private readonly BattleInfo _battleInfo;
        private readonly Status _status = new Status();

        [Flags]
        public enum Update
        {
            None = 0,
            Start = 1,
            Item = 2,
            Ship = 4,
            Timer = 8,
            NDock = 16,
            Mission = 32,
            QuestList = 64,
            Battle = 128,
            All = 255
        }

        public Sniffer()
        {
            _shipInfo = new ShipInfo(_shipMaster, _itemInfo);
            _dockInfo = new DockInfo(_shipInfo, _itemInfo);
            _akashiTimer = new AkashiTimer(_shipInfo, _itemInfo, _dockInfo, _missionInfo);
            _battleInfo = new BattleInfo(_shipMaster, _shipInfo, _itemInfo);
        }

        public void SaveState()
        {
            _achievement.SaveState(_status);
            _itemInfo.SaveState(_status);
            _status.Save();
        }

        public void LoadState()
        {
            _status.Load();
            _achievement.LoadState(_status);
            _itemInfo.LoadSate(_status);
        }

        public Update Sniff(string url, string request, dynamic json)
        {
            var data = json.IsDefined("api_data") ? json.api_data : new object();

            if (LogFile != null)
            {
                File.AppendAllText(LogFile,
                    string.Format("url: {0}\nrequest: {1}\nresponse: {2}\n", url, request, json.ToString()));                
            }

            if (url.EndsWith("api_start2"))
            {
                _start = true;
                _shipMaster.Inspect(data.api_mst_ship);
                _shipMaster.InspectStype(data.api_mst_stype);
                _missionInfo.InspectMaster(data.api_mst_mission);
                _itemInfo.InspectMaster(data.api_mst_slotitem);
                return Update.Start;
            }
            if (!_start)
                return Update.None;
            if (url.EndsWith("api_port/port"))
            {
                _itemInfo.InspectBasic(data.api_basic);
                _itemInfo.InspectMaterial(data.api_material);
                _shipInfo.InspectShip(data);
                _missionInfo.InspectDeck(data.api_deck_port);
                _dockInfo.InspectNDock(data.api_ndock);
                _akashiTimer.SetTimer(true);
                _achievement.InspectBasic(data.api_basic);
                _battleInfo.InBattle = false;
                _battleInfo.HasDamagedShip = false;
                return Update.All;
            }
            if (url.EndsWith("api_get_member/basic"))
            {
                _itemInfo.InspectBasic(data);
                return Update.None;
            }
            if (url.EndsWith("api_get_member/slot_item"))
            {
                _itemInfo.InspectSlotItem(data, true);
                return Update.None;
            }
            if (url.EndsWith("api_get_member/kdock"))
            {
                _dockInfo.InspectKDock(data);
                return Update.Timer;
            }
            if (url.EndsWith("api_get_member/ndock"))
            {
                _dockInfo.InspectNDock(data);
                _akashiTimer.SetTimer();
                return Update.NDock | Update.Timer | Update.Ship;
            }
            if (url.EndsWith("api_req_hensei/change"))
            {
                _shipInfo.InspectChange(request);
                _akashiTimer.SetTimer();
                return Update.Ship;
            }
            if (url.EndsWith("api_get_member/questlist"))
            {
                _questInfo.Inspect(data);
                return Update.QuestList;
            }
            if (url.EndsWith("api_get_member/deck"))
            {
                _missionInfo.InspectDeck(data);
                _akashiTimer.SetTimer();
                return Update.Mission | Update.Timer;
            }
            if (url.EndsWith("api_get_member/ship2"))
            {
                // ここだけjsonなので注意
                _shipInfo.InspectShip(json);
                _akashiTimer.SetTimer();
                _battleInfo.InBattle = false;
                return Update.Item | Update.Ship | Update.Battle;
            }
            if (url.EndsWith("api_get_member/ship3"))
            {
                _shipInfo.InspectShip(data);
                _akashiTimer.SetTimer();
                return Update.Ship;
            }
            if (url.EndsWith("api_get_member/material"))
            {
                _itemInfo.InspectMaterial(data);
                return Update.Item;
            }
            if (url.EndsWith("api_req_hokyu/charge"))
            {
                _shipInfo.InspectCharge(data);
                return Update.Item | Update.Ship;
            }
            if (url.EndsWith("api_req_kousyou/createitem"))
            {
                _itemInfo.InspectCreateItem(data);
                return Update.Item;
            }
            if (url.EndsWith("api_req_kousyou/getship"))
            {
                _itemInfo.InspectGetShip(data);
                _shipInfo.InspectShip(data);
                _dockInfo.InspectKDock(data.api_kdock);
                return Update.Item | Update.Timer;
            }
            if (url.EndsWith("api_req_kousyou/destroyship"))
            {
                _shipInfo.InspectDestroyShip(request, data);
                _akashiTimer.SetTimer();
                return Update.Item | Update.Ship;
            }
            if (url.EndsWith("api_req_kousyou/destroyitem2"))
            {
                _itemInfo.InspectDestroyItem(request, data);
                return Update.Item;
            }
            if (url.EndsWith("api_req_kaisou/powerup"))
            {
                _shipInfo.InspectPowerup(request, data);
                return Update.Item | Update.Ship;
            }
            if (url.EndsWith("api_req_nyukyo/start"))
            {
                _dockInfo.InspectNyukyo(request);
                _akashiTimer.SetTimer();
                return Update.Item | Update.Ship;
            }
            if (url.EndsWith("api_req_nyukyo/speedchange"))
            {
                _dockInfo.InspectSpeedChange(request);
                return Update.NDock | Update.Timer | Update.Ship;
            }
            if (IsNormalBattleAPI(url))
            {
                _battleInfo.InspectBattle(data);
                return Update.Battle;
            }
            if (url.EndsWith("api_req_sortie/battleresult") || url.EndsWith("api_req_practice/battleresult"))
            {
                _battleInfo.CauseDamage();
                return Update.Ship;
            }
            if (url.EndsWith("api_req_mission/result"))
            {
                _itemInfo.InspectMissionResult(data);
                return Update.Item;
            }
            if (IsCombinedBattleAPI(url))
            {
                _battleInfo.InspectCombinedBattle(data);
                return Update.Battle;
            }
            if (url.EndsWith("api_req_combined_battle/battleresult"))
            {
                _battleInfo.CauseDamageCombined();
                return Update.Ship;
            }
            return Update.None;
        }

        public bool IsBattleAPI(string url)
        {
            return IsNormalBattleAPI(url) || IsCombinedBattleAPI(url);
        }

        public bool IsNormalBattleAPI(string url)
        {
            return url.EndsWith("api_req_sortie/battle") || url.EndsWith("api_req_practice/battle") ||
                   url.EndsWith("api_req_battle_midnight/sp_midnight") ||
                   url.EndsWith("api_req_practice/midnight_battle");
        }

        public bool IsCombinedBattleAPI(string url)
        {
            return url.EndsWith("api_req_combined_battle/battle") || url.EndsWith("api_req_combined_battle/airbattle") ||
                   url.EndsWith("api_req_combined_battle/midnight_battle") ||
                   url.EndsWith("api_req_combined_battle/sp_midnight");
        }

        public NameAndTimer[] NDock
        {
            get { return _dockInfo.NDock; }
        }

        public RingTimer[] KDock
        {
            get { return _dockInfo.KDock; }
        }

        public ItemInfo Item
        {
            get { return _itemInfo; }
        }

        public QuestInfo.NameAndProgress[] Quests
        {
            get { return _questInfo.Quests; }
        }

        public NameAndTimer[] Missions
        {
            get { return _missionInfo.Missions; }
        }

        public DateTime GetConditionTimer(int fleet)
        {
            return _shipInfo.GetConditionTiemr(fleet);
        }

        public int[] GetConditionNotice()
        {
            return _shipInfo.GetConditionNotice();
        }

        public ShipStatus[] GetShipStatuses(int fleet)
        {
            return _shipInfo.GetShipStatuses(fleet);
        }

        public ChargeStatus[] ChargeStatuses
        {
            get { return _shipInfo.ChargeStatuses; }
        }

        public int GetAirSuperiority(int fleet)
        {
            return _shipInfo.GetAirSuperiority(fleet);
        }

        public DamageStatus[] DamagedShipList
        {
            get { return _shipInfo.GetDamagedShipList(_dockInfo); }
        }

        public ShipStatus[] ShipList
        {
            get { return _shipInfo.ShipList; }
        }

        public ShipType[] ShipTypeList
        {
            get { return _shipMaster.ShipTypeList; }
        }

        public AkashiTimer.RepairSpan[] GetAkashiTimers(int fleet)
        {
            return _akashiTimer.GetTimers(fleet);
        }

        public string[] GetAkashiTimerNotice()
        {
            return _akashiTimer.GetNotice();
        }

        public Achievement Achievement
        {
            get { return _achievement; }
        }

        public BattleInfo Battle
        {
            get { return _battleInfo; }
        }

        public string LogFile { get; set; }
    }

    public class NameAndTimer
    {
        public string Name { get; set; }
        public RingTimer Timer { get; set; }

        public NameAndTimer()
        {
            Timer = new RingTimer();
        }
    }

    public class RingTimer
    {
        private DateTime _endTime;
        private TimeSpan _rest;
        private readonly TimeSpan _spare;

        public RingTimer(int spare = 60)
        {
            _spare = TimeSpan.FromSeconds(spare);
        }

        public void SetEndTime(double time)
        {
            SetEndTime((int)time == 0
                ? DateTime.MinValue
                : new DateTime(1970, 1, 1).ToLocalTime().AddSeconds(time / 1000));
        }

        public void SetEndTime(DateTime time)
        {
            _endTime = time;
            if (_endTime == DateTime.MinValue)
                IsFinished = false;
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
            if (_rest > _spare || IsFinished)
                return;
            IsFinished = true;
            NeedRing = true;
        }

        public bool IsFinished { get; private set; }
        public bool NeedRing { get; set; }

        public override string ToString()
        {
            return _rest.Days == 0 ? _rest.ToString(@"hh\:mm\:ss") : _rest.ToString(@"d\.hh\:mm");
        }
    }
}