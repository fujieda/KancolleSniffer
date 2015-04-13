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
        private readonly ConditionTimer _conditionTimer;
        private readonly DockInfo _dockInfo;
        private readonly AkashiTimer _akashiTimer;
        private readonly Achievement _achievement = new Achievement();
        private readonly BattleInfo _battleInfo;
        private readonly Logger _logger;
        private readonly Status _status = new Status();
        private bool _saveState;

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
            _conditionTimer = new ConditionTimer(_shipInfo);
            _dockInfo = new DockInfo(_shipInfo, _itemInfo);
            _akashiTimer = new AkashiTimer(_shipInfo, _itemInfo, _dockInfo);
            _battleInfo = new BattleInfo(_shipMaster, _shipInfo, _itemInfo);
            _logger = new Logger(_shipMaster, _shipInfo, _itemInfo);
        }

        private void SaveState()
        {
            if (!_saveState)
                return;
            if (!_achievement.NeedSave && !_itemInfo.NeedSave && !_conditionTimer.NeedSave)
                return;
            _achievement.SaveState(_status);
            _itemInfo.SaveState(_status);
            _conditionTimer.SaveState(_status);
            _status.Save();
        }

        public void LoadState()
        {
            _status.Load();
            _achievement.LoadState(_status);
            _itemInfo.LoadSate(_status);
            _conditionTimer.LoadState(_status);
            _saveState = true;
        }

        public Update Sniff(string url, string request, dynamic json)
        {
            var data = json.IsDefined("api_data") ? json.api_data : new object();

            if (url.EndsWith("api_start2"))
            {
                _start = true;
                _shipMaster.Inspect(data);
                _missionInfo.InspectMaster(data.api_mst_mission);
                _itemInfo.InspectMaster(data);
                return Update.Start;
            }
            if (!_start)
                return Update.None;
            if (url.EndsWith("api_port/port"))
            {
                _itemInfo.InspectBasic(data.api_basic);
                _itemInfo.InspectMaterial(data.api_material);
                _logger.InspectBasic(data.api_basic);
                _logger.InspectMaterial(data.api_material);
                _shipInfo.InspectShip(data);
                _conditionTimer.CalcRegenTime();
                _missionInfo.InspectDeck(data.api_deck_port);
                _dockInfo.InspectNDock(data.api_ndock);
                _akashiTimer.SetTimer(true);
                _achievement.InspectBasic(data.api_basic);
                if (data.api_parallel_quest_count()) // 昔のログにはないので
                    _questInfo.QuestCount = (int)data.api_parallel_quest_count;
                _battleInfo.InBattle = false;
                _battleInfo.HasDamagedShip = false;
                _shipInfo.ClearEscapedShips();
                SaveState();
                return Update.All;
            }
            if (url.EndsWith("api_get_member/basic"))
            {
                _itemInfo.InspectBasic(data);
                _logger.InspectBasic(data);
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
                _logger.InspectKDock(data);
                return Update.Timer;
            }
            if (url.EndsWith("api_get_member/ndock"))
            {
                _dockInfo.InspectNDock(data);
                _conditionTimer.CheckCond();
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
                _shipInfo.InspectDeck(data);
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
                _conditionTimer.CheckCond();
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
                _logger.InspectCreateItem(request, data);
                return Update.Item;
            }
            if (url.EndsWith("api_req_kousyou/getship"))
            {
                _itemInfo.InspectGetShip(data);
                _shipInfo.InspectShip(data);
                _dockInfo.InspectKDock(data.api_kdock);
                _conditionTimer.CheckCond();
                return Update.Item | Update.Timer;
            }
            if (url.EndsWith("api_req_kousyou/destroyship"))
            {
                _shipInfo.InspectDestroyShip(request, data);
                _conditionTimer.CheckCond();
                _akashiTimer.SetTimer();
                return Update.Item | Update.Ship;
            }
            if (url.EndsWith("api_req_kousyou/destroyitem2"))
            {
                _itemInfo.InspectDestroyItem(request, data);
                return Update.Item;
            }
            if (url.EndsWith("api_req_kousyou/remodel_slot"))
            {
                _itemInfo.InspectRemodelSlot(data);
                return Update.Item;
            }
            if (url.EndsWith("api_req_kousyou/createship"))
            {
                _logger.InspectCreateShip(request);
                return Update.None;
            }
            if (url.EndsWith("api_req_kaisou/powerup"))
            {
                _shipInfo.InspectPowerup(request, data);
                _conditionTimer.CheckCond();
                _akashiTimer.SetTimer();
                return Update.Item | Update.Ship;
            }
            if (url.EndsWith("api_req_nyukyo/start"))
            {
                _dockInfo.InspectNyukyo(request);
                _conditionTimer.CheckCond();
                _akashiTimer.SetTimer();
                return Update.Item | Update.Ship;
            }
            if (url.EndsWith("api_req_nyukyo/speedchange"))
            {
                _dockInfo.InspectSpeedChange(request);
                _conditionTimer.CheckCond();
                return Update.NDock | Update.Timer | Update.Ship;
            }
            if (IsNormalBattleAPI(url))
            {
                _battleInfo.InspectBattle(data);
                _logger.InspectBattle(data);
                return Update.Ship | Update.Battle;
            }
            if (url.EndsWith("api_req_practice/battle") || url.EndsWith("api_req_practice/midnight_battle"))
            {
                if (url.EndsWith("/battle"))
                {
                    _shipInfo.StartSortie(request); // 演習を出撃中とみなす
                    _conditionTimer.InvalidateCond();
                }
                _battleInfo.InspectPracticeBattle(data);
                return Update.Ship | Update.Battle | Update.Timer;
            }
            if (url.EndsWith("api_req_sortie/battleresult"))
            {
                _battleInfo.CauseDamage();
                _logger.InspectBattleResult(data);
                return Update.Ship;
            }
            if (url.EndsWith("api_req_practice/battle_result"))
            {
                _battleInfo.CausePracticeDamage();
                return Update.Ship;
            }
            if (IsCombinedBattleAPI(url))
            {
                _battleInfo.InspectCombinedBattle(data, url.EndsWith("battle_water"));
                _logger.InspectBattle(data);
                return Update.Ship | Update.Battle;
            }
            if (url.EndsWith("api_req_combined_battle/battleresult"))
            {
                _battleInfo.InspectCombinedBattleResult(data);
                _logger.InspectBattleResult(data);
                return Update.Ship;
            }
            if (url.EndsWith("api_req_combined_battle/goback_port"))
            {
                _battleInfo.CauseCombinedBattleEscape();
                return Update.Ship;
            }
            if (url.EndsWith("api_req_map/start"))
            {
                _shipInfo.StartSortie(request);
                _conditionTimer.InvalidateCond();
                _logger.InspectMap(data);
                return Update.Timer;
            }
            if (url.EndsWith("api_req_map/next"))
            {
                _logger.InspectMap(data);
                return Update.None;
            }
            if (url.EndsWith("api_req_mission/result"))
            {
                _itemInfo.InspectMissionResult(data);
                _logger.InspectMissionResult(data);
                return Update.Item;
            }
            return Update.None;
        }

        private bool IsNormalBattleAPI(string url)
        {
            return url.EndsWith("api_req_sortie/battle") ||
                   url.EndsWith("api_req_sortie/airbattle") ||
                   url.EndsWith("api_req_battle_midnight/battle") ||
                   url.EndsWith("api_req_battle_midnight/sp_midnight");
        }

        private bool IsCombinedBattleAPI(string url)
        {
            return url.EndsWith("api_req_combined_battle/battle") ||
                   url.EndsWith("api_req_combined_battle/airbattle") ||
                   url.EndsWith("api_req_combined_battle/battle_water") ||
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

        public QuestStatus[] Quests
        {
            get { return _questInfo.Quests; }
        }

        public NameAndTimer[] Missions
        {
            get { return _missionInfo.Missions; }
        }

        public DateTime GetConditionTimer(int fleet)
        {
            return _conditionTimer.GetTimer(fleet);
        }

        public int[] GetConditionNotice()
        {
            return _conditionTimer.GetNotice();
        }

        public ShipStatus[] GetShipStatuses(int fleet)
        {
            return _shipInfo.GetShipStatuses(fleet);
        }

        public int[] GetDeck(int fleet)
        {
            return _shipInfo.GetDeck(fleet);
        }

        public ChargeStatus[] ChargeStatuses
        {
            get { return _shipInfo.ChargeStatuses; }
        }

        public int GetAirSuperiority(int fleet)
        {
            return _shipInfo.GetAirSuperiority(fleet);
        }

        public double GetFleetLineOfSights(int fleet)
        {
            return _shipInfo.GetLineOfSights(fleet);
        }

        public ShipStatus[] DamagedShipList
        {
            get { return _shipInfo.GetDamagedShipList(_dockInfo); }
        }

        public ShipStatus[] ShipList
        {
            get { return _shipInfo.ShipList; }
        }

        public ItemStatus[] ItemList
        {
            get { return _itemInfo.GetItemList(_shipInfo); }
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

        public void SetLogWriter(Action<string, string, string> writer, Func<DateTime> nowFunc)
        {
            _logger.SetWriter(writer, nowFunc);
        }

        public void SkipMaster()
        {
            _start = true;
        }

        public void EnableLog(LogType type)
        {
            _logger.EnableLog(type);
        }

        public int MaterialLogInterval
        {
            set { _logger.MaterialLogInterval = value; }
        }

        public string LogOutputDir
        {
            set { _logger.OutputDir = value; }
        }
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
        private readonly TimeSpan _spare;

        public TimeSpan Rest { get; private set; }

        public bool IsFinished
        {
            get { return _endTime != DateTime.MinValue && Rest <= _spare; }
        }

        public bool NeedRing { get; set; }

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
        }

        public void Update()
        {
            if (_endTime == DateTime.MinValue)
            {
                Rest = TimeSpan.Zero;
                return;
            }
            var prev = Rest;
            Rest = _endTime - DateTime.Now;
            if (Rest < TimeSpan.Zero)
                Rest = TimeSpan.Zero;
            if (prev > _spare && _spare >= Rest)
                NeedRing = true;
        }
    }
}