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
using KancolleSniffer.Util;
using KancolleSniffer.View;
using System.Collections.Generic;
using System.Linq;
using KancolleSniffer.Model;

namespace KancolleSniffer
{
    public class Sniffer
    {
        private bool _start;
        private readonly ItemMaster _itemMaster = new ItemMaster();
        private readonly ItemInventry _itemInventry = new ItemInventry();
        private readonly ItemInfo _itemInfo;
        private readonly ShipMaster _shipMaster = new ShipMaster();
        private readonly ShipInventry _shipInventry = new ShipInventry();
        private readonly ShipInfo _shipInfo;
        private readonly MaterialInfo _materialInfo = new MaterialInfo();
        private readonly QuestInfo _questInfo;
        private readonly MissionInfo _missionInfo = new MissionInfo();
        private readonly ConditionTimer _conditionTimer;
        private readonly DockInfo _dockInfo;
        private readonly AkashiTimer _akashiTimer;
        private readonly Achievement _achievement = new Achievement();
        private readonly BattleInfo _battleInfo;
        private readonly Logger _logger;
        private readonly ExMapInfo _exMapInfo = new ExMapInfo();
        private readonly MiscTextInfo _miscTextInfo;
        private readonly BaseAirCoprs _baseAirCoprs;
        private readonly PresetDeck _presetDeck = new PresetDeck();
        private readonly CellInfo _cellInfo = new CellInfo();
        private readonly Status _status = new Status();
        private bool _saveState;
        private readonly List<IHaveState> _haveState;
        private AdditionalData _additionalData;

        public interface IRepeatingTimerController
        {
            void Stop(string key);
            void Stop(string key, int fleet);
            void Suspend(string exception = null);
            void Resume();
        }

        public IRepeatingTimerController RepeatingTimerController { get; set; }

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
            Cell = 1 << 9,
            All = (1 << 10) - 1
        }

        public Sniffer(bool start = false)
        {
            _start = start;
            _itemInfo = new ItemInfo(_itemMaster, _itemInventry);
            _shipInfo = new ShipInfo(_shipMaster, _shipInventry, _itemInfo);
            _conditionTimer = new ConditionTimer(_shipInfo);
            _dockInfo = new DockInfo(_shipInfo, _materialInfo);
            _akashiTimer = new AkashiTimer(_shipInfo, _dockInfo, _presetDeck);
            _battleInfo = new BattleInfo(_shipInfo, _itemInfo);
            _logger = new Logger(_shipInfo, _itemInfo, _battleInfo);
            _questInfo = new QuestInfo(_itemInfo, _battleInfo);
            _baseAirCoprs = new BaseAirCoprs(_itemInfo);
            _miscTextInfo = new MiscTextInfo(_shipInfo, _itemInfo);
            _haveState = new List<IHaveState> {_achievement, _materialInfo, _conditionTimer, _exMapInfo, _questInfo};
            AdditionalData = new AdditionalData();
        }

        public AdditionalData AdditionalData
        {
            get => _additionalData;
            set
            {
                _additionalData = value;
                _itemMaster.AdditionalData = value;
                _shipMaster.AdditionalData = value;
            }
        }

        public void SaveState()
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
            if (url.Contains("battle") || url.Contains("sortie"))
                return ApiBattle(url, request, data);
            if (url.Contains("hensei"))
                return ApiHensei(url, request, data);
            if (url.Contains("air_corps"))
                return ApiAirCorps(url, request, data);
            return ApiOthers(url, request, data);
        }

        private Update ApiStart(dynamic data)
        {
            _shipInfo.InspectMaster(data);
            _missionInfo.InspectMaster(data.api_mst_mission);
            _itemInfo.InspectMaster(data);
            _exMapInfo.ResetIfNeeded();
            _miscTextInfo.InspectMaster(data);
            _start = true;
            return Update.Start;
        }

        private Update ApiPort(dynamic data)
        {
            _itemInfo.InspectBasic(data.api_basic);
            _materialInfo.InspectMaterialPort(data.api_material);
            _logger.InspectBasic(data.api_basic);
            _logger.InspectMaterial(data.api_material);
            _shipInfo.InspectShip(data);
            _shipInfo.ClearBadlyDamagedShips();
            _conditionTimer.CalcRegenTime();
            _missionInfo.InspectDeck(data.api_deck_port);
            _questInfo.InspectDeck(data.api_deck_port);
            _dockInfo.InspectNDock(data.api_ndock);
            _akashiTimer.Port();
            _achievement.InspectBasic(data.api_basic);
            if (data.api_parallel_quest_count()) // 昔のログにはないので
                _questInfo.AcceptMax = (int)data.api_parallel_quest_count;
            if (data.api_event_object())
                _baseAirCoprs.InspectEventObject(data.api_event_object);
            if (data.api_plane_info())
                _baseAirCoprs.InspectPlaneInfo(data.api_plane_info);
            _battleInfo.CleanupResult();
            _battleInfo.BattleState = BattleState.None;
            _shipInfo.ClearEscapedShips();
            _miscTextInfo.Port();
            _cellInfo.Port();
            SaveState();
            RepeatingTimerController?.Resume();
            foreach (var s in new[] {"遠征終了", "入渠終了", "疲労回復", "泊地修理", "大破警告"})
                RepeatingTimerController?.Stop(s);
            return Update.All;
        }

        private Update ApiMember(string url, dynamic json)
        {
            var data = json.api_data() ? json.api_data : new object();

            if (url.EndsWith("api_get_member/require_info"))
            {
                _itemInfo.InspectSlotItem(data.api_slot_item, true);
                _dockInfo.InspectKDock(data.api_kdock);
                return Update.None;
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
                return Update.Item;
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
                RepeatingTimerController?.Stop("入渠終了");
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
                _questInfo.InspectDeck(data);
                return Update.Mission | Update.Timer;
            }
            if (url.EndsWith("api_get_member/ship2"))
            {
                // ここだけjsonなので注意
                _shipInfo.InspectShip(json);
                _akashiTimer.CheckFleet();
                _battleInfo.BattleState = BattleState.None;
                return Update.Item | Update.Ship | Update.Battle;
            }
            if (url.EndsWith("api_get_member/ship_deck"))
            {
                _shipInfo.InspectShip(data);
                _akashiTimer.CheckFleet();
                _battleInfo.BattleState = BattleState.None;
                return Update.Ship | Update.Battle | Update.Item;
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
                if (data.api_air_base())
                    _baseAirCoprs.Inspect(data.api_air_base);
                return Update.Item;
            }
            if (url.EndsWith("api_req_member/get_practice_enemyinfo"))
            {
                _miscTextInfo.InspectPracticeEnemyInfo(data);
                return Update.Item;
            }
            if (url.EndsWith("api_get_member/preset_deck"))
            {
                _presetDeck.Inspect(data);
                return Update.None;
            }
            if (url.EndsWith("api_get_member/base_air_corps"))
            {
                _baseAirCoprs.Inspect(data);
                return Update.Ship;
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
                _questInfo.CountCreateItem();
                return Update.Item | Update.QuestList;
            }
            if (url.EndsWith("api_req_kousyou/getship"))
            {
                _itemInfo.InspectGetShip(data);
                _shipInfo.InspectShip(data);
                _dockInfo.InspectKDock(data.api_kdock);
                _conditionTimer.CheckCond();
                RepeatingTimerController?.Stop("建造完了");
                return Update.Item | Update.Timer;
            }
            if (url.EndsWith("api_req_kousyou/destroyship"))
            {
                _shipInfo.InspectDestroyShip(request, data);
                _materialInfo.InspectDestroyShip(data);
                _conditionTimer.CheckCond();
                _akashiTimer.CheckFleet();
                _questInfo.InspectDestroyShip(request);
                return Update.Item | Update.Ship | Update.QuestList;
            }
            if (url.EndsWith("api_req_kousyou/destroyitem2"))
            {
                _questInfo.InspectDestroyItem(request, data); // 本当に削除される前
                _itemInfo.InspectDestroyItem(request, data);
                _materialInfo.InspectDestroyItem(data);
                return Update.Item | Update.QuestList;
            }
            if (url.EndsWith("api_req_kousyou/remodel_slot"))
            {
                _logger.SetCurrentMaterial(_materialInfo.Current);
                _logger.InspectRemodelSlot(request, data); // 資材の差が必要なので_materialInfoより前
                _itemInfo.InspectRemodelSlot(data);
                _materialInfo.InspectRemodelSlot(data);
                _questInfo.CountRemodelSlot();
                return Update.Item | Update.QuestList;
            }
            if (url.EndsWith("api_req_kousyou/createship"))
            {
                _logger.InspectCreateShip(request);
                _questInfo.CountCreateShip();
                return Update.QuestList;
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
            if (IsNormalBattleAPI(url) || IsCombinedBattleAPI(url))
            {
                _battleInfo.InspectBattle(url, request, data);
                _logger.InspectBattle(data);
                _cellInfo.StartBattle();
                return Update.Ship | Update.Battle;
            }
            if (url.EndsWith("api_req_practice/battle") || url.EndsWith("api_req_practice/midnight_battle"))
            {
                if (url.EndsWith("/battle"))
                {
                    _shipInfo.StartPractice(request);
                    _questInfo.StartPractice(request);
                    _cellInfo.StartPractice();
                    _conditionTimer.InvalidateCond();
                    RepeatingTimerController?.Suspend();
                }
                _battleInfo.InspectBattle(url, request, data);
                return Update.Ship | Update.Battle | Update.Timer;
            }
            if (url.EndsWith("api_req_sortie/battleresult") || url.EndsWith("api_req_combined_battle/battleresult"))
            {
                _battleInfo.InspectBattleResult(data);
                _exMapInfo.InspectBattleResult(data);
                _logger.InspectBattleResult(data);
                _questInfo.InspectBattleResult(data);
                _miscTextInfo.InspectBattleResult(data);
                return Update.Ship | Update.QuestList;
            }
            if (url.EndsWith("api_req_practice/battle_result"))
            {
                _battleInfo.InspectPracticeResult(data);
                _questInfo.InspectPracticeResult(data);
                return Update.Ship | Update.QuestList;
            }
            if (url.EndsWith("/goback_port"))
            {
                _battleInfo.CauseEscape();
                return Update.Ship;
            }
            _battleInfo.BattleState = BattleState.Unknown;
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
                   url.EndsWith("api_req_combined_battle/sp_midnight") ||
                   url.EndsWith("api_req_combined_battle/ec_battle") ||
                   url.EndsWith("api_req_combined_battle/ec_midnight_battle") ||
                   url.EndsWith("api_req_combined_battle/ec_night_to_day") ||
                   url.EndsWith("api_req_combined_battle/each_battle") ||
                   url.EndsWith("api_req_combined_battle/each_battle_water");
        }

        private Update ApiHensei(string url, string request, dynamic data)
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
                _presetDeck.InspectRegister(data);
                return Update.None;
            }
            if (url.EndsWith("api_req_hensei/preset_delete"))
            {
                _presetDeck.InspectDelete(request);
                return Update.Timer;
            }
            if (url.EndsWith("api_req_hensei/combined"))
            {
                _shipInfo.InspectCombined(request);
                return Update.Ship;
            }
            return Update.None;
        }

        private Update ApiAirCorps(string url, string request, dynamic data)
        {
            if (url.EndsWith("api_req_air_corps/supply"))
            {
                _materialInfo.InspectAirCorpsSupply(data);
                _baseAirCoprs.InspectSupply(request, data);
                return Update.Item;
            }
            if (url.EndsWith("api_req_air_corps/set_plane"))
            {
                _materialInfo.InspectAirCorpsSetPlane(data);
                _baseAirCoprs.InspectSetPlane(request, data);
                return Update.Item | Update.Ship;
            }
            if (url.EndsWith("api_req_air_corps/set_action"))
            {
                _baseAirCoprs.InspectSetAction(request);
                return Update.Ship;
            }
            if (url.EndsWith("api_req_air_corps/expand_base"))
            {
                _baseAirCoprs.InspectExpandBase(request, data);
                return Update.Ship;
            }
            return Update.None;
        }

        private Update ApiOthers(string url, string request, dynamic data)
        {
            if (url.EndsWith("api_req_hokyu/charge"))
            {
                _shipInfo.InspectCharge(data);
                _materialInfo.InspectCharge(data);
                _questInfo.CountCharge();
                return Update.Item | Update.Ship | Update.QuestList;
            }
            if (url.EndsWith("api_req_kaisou/powerup"))
            {
                _shipInfo.InspectPowerup(request, data);
                _conditionTimer.CheckCond();
                _akashiTimer.CheckFleet();
                _questInfo.InspectPowerup(data);
                return Update.Item | Update.Ship | Update.QuestList;
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
                _questInfo.CountNyukyo();
                var ndock = HttpUtility.ParseQueryString(request)["api_ndock_id"];
                if (ndock != null && int.TryParse(ndock, out int id))
                    RepeatingTimerController?.Stop("入渠終了", id - 1);
                return Update.Item | Update.Ship | Update.QuestList;
            }
            if (url.EndsWith("api_req_nyukyo/speedchange"))
            {
                _dockInfo.InspectSpeedChange(request);
                _conditionTimer.CheckCond();
                return Update.NDock | Update.Timer | Update.Item | Update.Ship;
            }
            if (url.EndsWith("api_req_map/start"))
            {
                _shipInfo.InspectMapStart(request); // 出撃中判定が必要なので_conditionTimerより前
                _conditionTimer.InvalidateCond();
                _exMapInfo.InspectMapStart(data);
                _battleInfo.InspectMapStart(data);
                _logger.InspectMapStart(data);
                _miscTextInfo.InspectMapStart(data);
                _questInfo.InspectMapStart(data);
                _cellInfo.InspectMapStart(data);
                RepeatingTimerController?.Suspend("大破警告");
                return Update.Timer | Update.Ship | Update.Cell;
            }
            if (url.EndsWith("api_req_map/next"))
            {
                _exMapInfo.InspectMapNext(data);
                _battleInfo.InspectMapNext(data);
                _logger.InspectMapNext(data);
                _questInfo.InspectMapNext(data);
                _miscTextInfo.InspectMapNext(data);
                _cellInfo.InspectMapNext(data);
                return Update.Cell;
            }
            if (url.EndsWith("api_req_mission/start"))
            {
                var deck = HttpUtility.ParseQueryString(request)["api_deck_id"];
                if (deck != null && int.TryParse(deck, out int id))
                    RepeatingTimerController?.Stop("遠征終了", id - 1);
                return Update.None;
            }
            if (url.EndsWith("api_req_mission/result"))
            {
                _materialInfo.InspectMissionResult(data);
                _logger.InspectMissionResult(data);
                _questInfo.InspectMissionResult(request, data);
                return Update.Item;
            }
            if (url.EndsWith("api_req_quest/stop"))
            {
                _questInfo.InspectStop(request);
                return Update.QuestList;
            }
            if (url.EndsWith("api_req_quest/clearitemget"))
            {
                _questInfo.InspectClearItemGet(request);
                _logger.InspectClearItemGet(data);
                return Update.QuestList;
            }
            return Update.None;
        }

        public NameAndTimer[] NDock => _dockInfo.NDock;

        public AlarmTimer[] KDock => _dockInfo.KDock;

        public ItemInfo Item => _itemInfo;

        public MaterialInfo Material => _materialInfo;

        public QuestStatus[] Quests => _questInfo.Quests;

        public void ClearQuests() => _questInfo.ClearQuests();

        public NameAndTimer[] Missions => _missionInfo.Missions;

        public DateTime GetConditionTimer(int fleet) => _conditionTimer.GetTimer(fleet);

        public int[] GetConditionNotice(DateTime prev, DateTime now) => _conditionTimer.GetNotice(prev, now);

        public Fleet[] Fleets => _shipInfo.Fleets;

        public ShipInfo.ShipStatusPair[] BattleResultStatusDiff => _shipInfo.BattleResultDiff;

        public bool IsBattleResultStatusError => _shipInfo.IsBattleResultError;

        public ShipStatus[] BattleStartStatus => _shipInfo.BattleStartStatus;

        public bool IsCombinedFleet => _shipInfo.Fleets[0].CombinedType != 0;

        public ShipStatus[] RepairList => _shipInfo.GetRepairList(_dockInfo);

        public ShipStatus[] ShipList => _shipInfo.ShipList;

        public string[] BadlyDamagedShips => _shipInfo.BadlyDamagedShips;

        public ItemStatus[] ItemList
        {
            get
            {
                _itemInfo.ClearHolder();
                _shipInfo.SetItemHolder();
                _baseAirCoprs.SetItemHolder();
                return _itemInfo.ItemList;
            }
        }

        public AkashiTimer AkashiTimer => _akashiTimer;

        public Achievement Achievement => _achievement;

        public BattleInfo Battle => _battleInfo;

        public ExMapInfo ExMap => _exMapInfo;

        public string MiscText => _miscTextInfo.Text;

        public BaseAirCoprs.BaseInfo[] BaseAirCorps => _baseAirCoprs.AllAirCorps;

        public CellInfo CellInfo => _cellInfo;

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
            set => _logger.MaterialLogInterval = value;
        }

        public string LogOutputDir
        {
            set => _logger.OutputDir = value;
        }

        public void FlashLog()
        {
            _logger.FlashLog();
        }
    }
}