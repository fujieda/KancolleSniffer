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
using System.Linq;

namespace KancolleSniffer
{
    public class Sniffer
    {
        private bool _start;
        private readonly ItemInfo _itemInfo = new ItemInfo();
        private readonly MaterialInfo _materialInfo = new MaterialInfo();
        private readonly QuestInfo _questInfo = new QuestInfo();
        private readonly MissionInfo _missionInfo = new MissionInfo();
        private readonly ShipInfo _shipInfo;
        private readonly ConditionTimer _conditionTimer;
        private readonly DockInfo _dockInfo;
        private readonly AkashiTimer _akashiTimer;
        private readonly Achievement _achievement = new Achievement();
        private readonly BattleInfo _battleInfo;
        private readonly Logger _logger;
        private readonly ExMapInfo _exMapInfo = new ExMapInfo();
        private readonly MiscTextInfo _miscTextInfo = new MiscTextInfo();
        private readonly Status _status = new Status();
        private bool _saveState;
        private readonly List<IHaveState> _haveState;

        [Flags]
        public enum Update
        {
            None = 0,
            Error = 1 << 0,
            Start = 1 << 1,
            Item = 1 << 2,
            Ship = 1 << 3,
            Timer = 1 << 4,
            NDock = 1 << 5,
            Mission = 1 << 6,
            QuestList = 1 << 7,
            Battle = 1 << 8,
            All = (1 << 9) - 1
        }

        public Sniffer()
        {
            _shipInfo = new ShipInfo(_itemInfo);
            _conditionTimer = new ConditionTimer(_shipInfo);
            _dockInfo = new DockInfo(_shipInfo, _materialInfo);
            _akashiTimer = new AkashiTimer(_shipInfo, _dockInfo);
            _battleInfo = new BattleInfo(_shipInfo, _itemInfo);
            _logger = new Logger(_shipInfo, _itemInfo, _battleInfo);
            _haveState = new List<IHaveState> {_achievement, _materialInfo, _conditionTimer, _exMapInfo};
        }

        private void SaveState()
        {
            if (!_saveState)
                return;
            if (!_haveState.Any(x => x.NeedSave))
                return;
            foreach (var x in _haveState)
                x.SaveState(_status);
            _status.Save();
        }

        public void LoadState()
        {
            _status.Load();
            foreach (var x in _haveState)
                x.LoadState(_status);
            _saveState = true;
        }

        public Update Sniff(string url, string request, dynamic json)
        {
            if (!json.api_result())
                return Update.Error;
            if ((int)json.api_result != 1)
                return Update.None;
            var data = json.api_data() ? json.api_data : new object();

            if (url.EndsWith("api_start2"))
            {
                return ApiStart(data);
            }
            if (!_start)
                return Update.None;
            if (url.EndsWith("api_port/port"))
                return ApiPort(data);
            if (url.Contains("member"))
                return ApiMember(url, json);
            if (url.Contains("kousyou"))
                return ApiKousyou(url, request, data);
            if (url.Contains("battle"))
                return ApiBattle(url, request, data);
            return ApiOthers(url, request, data);
        }

        private Update ApiStart(dynamic data)
        {
            _shipInfo.InspectMaster(data);
            _missionInfo.InspectMaster(data.api_mst_mission);
            _itemInfo.InspectMaster(data);
            _exMapInfo.ResetIfNeeded();
            _start = true;
            return Update.Start;
        }

        private Update ApiPort(dynamic data)
        {
            _itemInfo.InspectBasic(data.api_basic);
            _materialInfo.InspectMaterial(data.api_material, true);
            _logger.InspectBasic(data.api_basic);
            _logger.InspectMaterial(data.api_material);
            _shipInfo.InspectShip(data);
            _shipInfo.ClearBadlyDamagedShips();
            _conditionTimer.CalcRegenTime();
            _missionInfo.InspectDeck(data.api_deck_port);
            _dockInfo.InspectNDock(data.api_ndock);
            _akashiTimer.Port();
            _achievement.InspectBasic(data.api_basic);
            if (data.api_parallel_quest_count()) // 昔のログにはないので
                _questInfo.QuestCount = (int)data.api_parallel_quest_count;
            _battleInfo.CleanupResult();
            _battleInfo.InBattle = false;
            _shipInfo.ClearEscapedShips();
            _miscTextInfo.ClearIfNeeded();
            SaveState();
            return Update.All;
        }

        private Update ApiMember(string url, dynamic json)
        {
            var data = json.api_data() ? json.api_data : new object();

            if (url.EndsWith("api_get_member/require_info"))
            {
                _itemInfo.InspectSlotItem(data.api_slot_item, true);
                _dockInfo.InspectKDock(data.api_kdock);
                return Update.Timer;
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
                _akashiTimer.CheckFleet();
                return Update.NDock | Update.Timer | Update.Ship;
            }
            if (url.EndsWith("api_get_member/questlist"))
            {
                _questInfo.InspectQuestList(data);
                return Update.QuestList;
            }
            if (url.EndsWith("api_get_member/deck"))
            {
                _shipInfo.InspectDeck(data);
                _missionInfo.InspectDeck(data);
                _akashiTimer.CheckFleet();
                return Update.Mission | Update.Timer;
            }
            if (url.EndsWith("api_get_member/ship2"))
            {
                // ここだけjsonなので注意
                _shipInfo.InspectShip(json);
                _akashiTimer.CheckFleet();
                _battleInfo.InBattle = false;
                return Update.Item | Update.Ship | Update.Battle;
            }
            if (url.EndsWith("api_get_member/ship_deck"))
            {
                _shipInfo.InspectShip(data);
                _akashiTimer.CheckFleet();
                _battleInfo.InBattle = false;
                return Update.Ship | Update.Battle;
            }
            if (url.EndsWith("api_get_member/ship3"))
            {
                _shipInfo.InspectShip(data);
                _akashiTimer.CheckFleet();
                _conditionTimer.CheckCond();
                return Update.Ship;
            }
            if (url.EndsWith("api_get_member/material"))
            {
                _materialInfo.InspectMaterial(data);
                return Update.Item;
            }
            if (url.EndsWith("api_get_member/mapinfo"))
            {
                _exMapInfo.InspectMapInfo(data);
                _miscTextInfo.InspectMapInfo(data);
                return Update.Item;
            }
            if (url.EndsWith("api_req_member/get_practice_enemyinfo"))
            {
                _miscTextInfo.InspectPracticeEnemyInfo(data);
                return Update.Item;
            }
            if (url.EndsWith("api_get_member/preset_deck"))
            {
                _shipInfo.InspectPresetDeck(data);
                return Update.None;
            }
            return Update.None;
        }

        private Update ApiKousyou(string url, string request, dynamic data)
        {
            if (url.EndsWith("api_req_kousyou/createitem"))
            {
                _itemInfo.InspectCreateItem(data);
                _materialInfo.InspectCreateIem(data);
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
                _materialInfo.InspectDestroyShip(data);
                _conditionTimer.CheckCond();
                _akashiTimer.CheckFleet();
                return Update.Item | Update.Ship;
            }
            if (url.EndsWith("api_req_kousyou/destroyitem2"))
            {
                _itemInfo.InspectDestroyItem(request, data);
                _materialInfo.InspectDestroyItem(data);
                return Update.Item;
            }
            if (url.EndsWith("api_req_kousyou/remodel_slot"))
            {
                _logger.SetCurrentMaterial(_materialInfo.Current);
                _logger.InspectRemodelSlot(request, data); // 資材の差が必要なので_materialInfoより前
                _itemInfo.InspectRemodelSlot(data);
                _materialInfo.InspectRemodelSlot(data);
                return Update.Item;
            }
            if (url.EndsWith("api_req_kousyou/createship"))
            {
                _logger.InspectCreateShip(request);
                return Update.None;
            }
            if (url.EndsWith("api_req_kousyou/createship_speedchange"))
            {
                _dockInfo.InspectCreateShipSpeedChange(request);
                return Update.Timer;
            }
            return Update.None;
        }

        private Update ApiBattle(string url, string request, dynamic data)
        {
            if (IsNormalBattleAPI(url))
            {
                _battleInfo.InspectBattle(data, url);
                _logger.InspectBattle(data);
                return Update.Ship | Update.Battle;
            }
            if (url.EndsWith("api_req_practice/battle") || url.EndsWith("api_req_practice/midnight_battle"))
            {
                if (url.EndsWith("/battle"))
                {
                    _shipInfo.InspectMapStart(request); // 演習を出撃中とみなす
                    _conditionTimer.InvalidateCond();
                    _miscTextInfo.ClearFlag = true;
                }
                _battleInfo.InspectBattle(data, url);
                return Update.Ship | Update.Battle | Update.Timer;
            }
            if (url.EndsWith("api_req_sortie/battleresult"))
            {
                _battleInfo.InspectBattleResult(data);
                _exMapInfo.InspectBattleResult(data);
                _logger.InspectBattleResult(data);
                return Update.Ship;
            }
            if (url.EndsWith("api_req_practice/battle_result"))
            {
                _battleInfo.InspectPracticeResult(data);
                return Update.Ship;
            }
            if (IsCombinedBattleAPI(url))
            {
                _battleInfo.InspectCombinedBattle(data, url);
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
            return Update.None;
        }

        private bool IsNormalBattleAPI(string url)
        {
            return url.EndsWith("api_req_sortie/battle") ||
                   url.EndsWith("api_req_sortie/airbattle") ||
                   url.EndsWith("api_req_sortie/ld_airbattle") ||
                   url.EndsWith("api_req_battle_midnight/battle") ||
                   url.EndsWith("api_req_battle_midnight/sp_midnight");
        }

        private bool IsCombinedBattleAPI(string url)
        {
            return url.EndsWith("api_req_combined_battle/battle") ||
                   url.EndsWith("api_req_combined_battle/airbattle") ||
                   url.EndsWith("api_req_combined_battle/ld_airbattle") ||
                   url.EndsWith("api_req_combined_battle/battle_water") ||
                   url.EndsWith("api_req_combined_battle/midnight_battle") ||
                   url.EndsWith("api_req_combined_battle/sp_midnight");
        }

        private Update ApiOthers(string url, string request, dynamic data)
        {
            if (url.EndsWith("api_req_hensei/change"))
            {
                _shipInfo.InspectChange(request);
                _akashiTimer.InspectChange(request);
                return Update.Ship;
            }
            if (url.EndsWith("api_req_hensei/preset_select"))
            {
                _shipInfo.InspectDeck(new[] {data});
                _akashiTimer.CheckFleet();
                return Update.Ship;
            }
            if (url.EndsWith("api_req_hensei/preset_register"))
            {
                _shipInfo.InspectPresetRegister(data);
                return Update.None;
            }
            if (url.EndsWith("api_req_hensei/preset_delete"))
            {
                _shipInfo.InspectPresetDelete(request);
                return Update.Timer;
            }
            if (url.EndsWith("api_req_hensei/combined"))
            {
                _shipInfo.InspectCombined(request);
                return Update.Ship;
            }
            if (url.EndsWith("api_req_hokyu/charge"))
            {
                _shipInfo.InspectCharge(data);
                _materialInfo.InspectCharge(data);
                return Update.Item | Update.Ship;
            }
            if (url.EndsWith("api_req_kaisou/powerup"))
            {
                _shipInfo.InspectPowerup(request, data);
                _conditionTimer.CheckCond();
                _akashiTimer.CheckFleet();
                return Update.Item | Update.Ship;
            }
            if (url.EndsWith("api_req_kaisou/slot_exchange_index"))
            {
                _shipInfo.InspectSlotExchange(request, data);
                return Update.Ship;
            }
            if (url.EndsWith("api_req_kaisou/slot_deprive"))
            {
                _shipInfo.InspectSlotDeprive(data);
                return Update.Ship;
            }
            if (url.EndsWith("api_req_nyukyo/start"))
            {
                _dockInfo.InspectNyukyo(request);
                _conditionTimer.CheckCond();
                _akashiTimer.CheckFleet();
                return Update.Item | Update.Ship;
            }
            if (url.EndsWith("api_req_nyukyo/speedchange"))
            {
                _dockInfo.InspectSpeedChange(request);
                _conditionTimer.CheckCond();
                return Update.NDock | Update.Timer | Update.Ship;
            }
            if (url.EndsWith("api_req_map/start"))
            {
                _shipInfo.InspectMapStart(request); // 出撃中判定が必要なので_conditionTimerより前
                _conditionTimer.InvalidateCond();
                _exMapInfo.InspectMapStart(data);
                _logger.InspectMapStart(data);
                _miscTextInfo.ClearFlag = true;
                return Update.Timer | Update.Ship;
            }
            if (url.EndsWith("api_req_map/next"))
            {
                _battleInfo.InspectMapNext(request);
                _exMapInfo.InspectMapNext(data);
                _logger.InspectMapNext(data);
                return Update.None;
            }
            if (url.EndsWith("api_req_mission/result"))
            {
                _materialInfo.InspectMissionResult(data);
                _logger.InspectMissionResult(data);
                return Update.Item;
            }
            if (url.EndsWith("api_req_quest/clearitemget"))
            {
                _questInfo.InspectClearItemGet(request);
                return Update.QuestList;
            }
            if (url.EndsWith("api_req_air_corps/supply"))
            {
                _materialInfo.InspectAirCorpsSupply(data);
                return Update.Item;
            }
            if (url.EndsWith("api_req_air_corps/set_plane"))
            {
                _materialInfo.InspectAirCorpsSetPlane(data);
                return Update.Item;
            }
            return Update.None;
        }

        public NameAndTimer[] NDock => _dockInfo.NDock;

        public RingTimer[] KDock => _dockInfo.KDock;

        public ItemInfo Item => _itemInfo;

        public MaterialInfo Material => _materialInfo;

        public QuestStatus[] Quests => _questInfo.Quests;

        public NameAndTimer[] Missions => _missionInfo.Missions;

        public DateTime GetConditionTimer(int fleet) => _conditionTimer.GetTimer(fleet);

        public int[] GetConditionNotice() => _conditionTimer.GetNotice();

        public ShipStatus[] GetShipStatuses(int fleet) => _shipInfo.GetShipStatuses(fleet);

        public int[] GetDeck(int fleet) => _shipInfo.GetDeck(fleet);

        public int CombinedFleetType => _shipInfo.CombinedFleetType;

        public ChargeStatus[] ChargeStatuses => _shipInfo.ChargeStatuses;

        public int[] GetFighterPower(int fleet) => _shipInfo.GetFighterPower(fleet);

        public double GetContactTriggerRate(int fleet) => _shipInfo.GetContactTriggerRate(fleet);

        public double GetFleetLineOfSights(int fleet) => _shipInfo.GetLineOfSights(fleet);

        public ShipStatus[] DamagedShipList => _shipInfo.GetDamagedShipList(_dockInfo);

        public ShipStatus[] ShipList => _shipInfo.ShipList;

        public string[] BadlyDamagedShips => _shipInfo.BadlyDamagedShips;

        public ItemStatus[] ItemList => _itemInfo.GetItemListWithOwner(ShipList);

        public AkashiTimer AkashiTimer => _akashiTimer;

        public Achievement Achievement => _achievement;

        public BattleInfo Battle => _battleInfo;

        public ExMapInfo ExMap => _exMapInfo;

        public string MiscText => _miscTextInfo.Text;

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
        private readonly TimeSpan _spare;

        public TimeSpan Rest { get; private set; }

        public bool IsFinished => EndTime != DateTime.MinValue && Rest <= _spare;

        public DateTime EndTime { get; private set; }

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
            EndTime = time;
        }

        public void Update()
        {
            if (EndTime == DateTime.MinValue)
            {
                Rest = TimeSpan.Zero;
                return;
            }
            var prev = Rest;
            Rest = EndTime - DateTime.Now;
            if (Rest < TimeSpan.Zero)
                Rest = TimeSpan.Zero;
            if (prev > _spare && _spare >= Rest)
                NeedRing = true;
        }

        public string ToString(bool finish = false)
            => EndTime == DateTime.MinValue
                ? ""
                : finish ? EndTime.ToString(@"dd\ HH\:mm") : $@"{(int)Rest.TotalHours:d2}:{Rest:mm\:ss}";
    }
}