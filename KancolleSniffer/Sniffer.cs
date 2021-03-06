﻿// Copyright (C) 2013, 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Collections.Generic;
using System.Linq;
using KancolleSniffer.Log;
using KancolleSniffer.Model;

namespace KancolleSniffer
{
    public class Sniffer
    {
        private readonly ItemMaster _itemMaster = new ItemMaster();
        private readonly ItemInventory _itemInventory = new ItemInventory();
        private readonly ItemInfo _itemInfo;
        private readonly ShipMaster _shipMaster = new ShipMaster();
        private readonly ShipInventory _shipInventory = new ShipInventory();
        private readonly ShipInfo _shipInfo;
        private readonly MaterialInfo _materialInfo = new MaterialInfo();
        private readonly QuestInfo _questInfo;
        private readonly QuestCounter _questCounter;
        private readonly QuestCountList _questCountList = new QuestCountList();
        private readonly MissionInfo _missionInfo = new MissionInfo();
        private readonly ConditionTimer _conditionTimer;
        private readonly DockInfo _dockInfo;
        private readonly AkashiTimer _akashiTimer;
        private readonly Achievement _achievement = new Achievement();
        private readonly BattleInfo _battleInfo;
        private readonly Logger _logger;
        private readonly ExMapInfo _exMapInfo = new ExMapInfo();
        private readonly MiscTextInfo _miscTextInfo;
        private readonly AirBase _airBase;
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

        public Dictionary<string, string> MapDictionary { get; } = new Dictionary<string, string>
        {
            {"南西作戦海域方面 バリ島沖", "42-1"},
            {"西方作戦海域方面 マラッカ海峡北方", "42-2"},
            {"西方作戦海域方面 セイロン島南西沖", "42-3"},
            {"欧州作戦海域方面 地中海マルタ島沖", "42-4"},
            {"欧州作戦海域方面 北海/北大西洋海域", "42-5"}
        };

        [Flags]
        public enum Update
        {
            None = 0,
            Error = 1,
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

        public bool Started { get; private set; }

        public Sniffer(bool started = false)
        {
            Started = started;
            _itemInfo = new ItemInfo(_itemMaster, _itemInventory);
            _shipInfo = new ShipInfo(_shipMaster, _shipInventory, _itemInventory);
            _conditionTimer = new ConditionTimer(_shipInfo);
            _dockInfo = new DockInfo(_shipInventory, _materialInfo);
            _akashiTimer = new AkashiTimer(_shipInfo, _dockInfo, _presetDeck);
            _airBase = new AirBase(_itemInfo);
            _battleInfo = new BattleInfo(_shipInfo, _itemInfo, _airBase);
            _logger = new Logger(_shipInfo, _itemInfo, _battleInfo);
            _questInfo = new QuestInfo(_questCountList);
            _questCounter = new QuestCounter(_questInfo, _itemInventory, _shipInventory, _battleInfo);
            _miscTextInfo = new MiscTextInfo(_shipInfo, _itemInfo);
            _haveState = new List<IHaveState> {_achievement, _materialInfo, _conditionTimer, _exMapInfo, _questInfo};
            AdditionalData = new AdditionalData();
        }

        public AdditionalData AdditionalData
        {
            get => _additionalData;
            private set
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

            if (url.Contains("api_start2"))
            {
                return ApiStart(data);
            }
            if (!Started)
                return Update.None;

            if (url.EndsWith("api_port/port"))
                return ApiPort(data);
            if (url.Contains("member"))
                return ApiMember(url, request,json);
            if (url.Contains("kousyou"))
                return ApiKousyou(url, request, data);
            if (url.Contains("practice"))
                return ApiPractice(url, request, data);
            if (IsBattleAPI(url))
                return ApiBattle(url, request, data);
            if (url.Contains("hensei"))
                return ApiHensei(url, request, data);
            if (url.Contains("kaisou"))
                return ApiKaisou(url, request, data);
            if (url.Contains("air_corps"))
                return ApiAirCorps(url, request, data);
            if (url.Contains("map"))
                return ApiMap(url, request, data);
            return ApiOthers(url, request, data);
        }

        private static bool IsBattleAPI(string url)
        {
            return url.Contains("api_req_sortie/") ||
                   url.Contains("api_req_battle_midnight/") ||
                   url.Contains("api_req_combined_battle/");
        }

        private Update ApiStart(dynamic data)
        {
            _shipInfo.InspectMaster(data);
            _shipInfo.ClearBattleResult();
            _missionInfo.InspectMaster(data.api_mst_mission);
            _itemInfo.InspectMaster(data);
            _exMapInfo.ResetIfNeeded();
            _miscTextInfo.InspectMaster(data);
            _logger.InspectMapInfoMaster(data.api_mst_mapinfo);
            SetMapDictionary(data.api_mst_mapinfo);
            _questCountList.SetMissionNames(data.api_mst_mission);
            Started = true;
            return Update.Start;
        }

        private void SetMapDictionary(dynamic json)
        {
            foreach (var map in json)
                MapDictionary[map.api_name] = $"{map.api_maparea_id}-{map.api_no}";
        }

        public interface IPort
        {
            void Port();
        }

        private Update ApiPort(dynamic data)
        {
            _itemInfo.InspectBasic(data.api_basic);
            _materialInfo.InspectMaterialPort(data.api_material);
            _logger.InspectBasic(data.api_basic);
            _logger.InspectMaterial(data.api_material);
            _shipInfo.Port(data);
            _missionInfo.InspectDeck(data.api_deck_port);
            _questCounter.InspectDeck(data.api_deck_port);
            _dockInfo.InspectNDock(data.api_ndock);
            _achievement.InspectBasic(data.api_basic);
            if (data.api_event_object())
                _airBase.InspectEventObject(data.api_event_object);
            if (data.api_plane_info())
                _airBase.InspectPlaneInfo(data.api_plane_info);
            foreach (var receiver in new IPort[]{_conditionTimer, _akashiTimer, _battleInfo, _miscTextInfo, _cellInfo})
                receiver.Port();
            SaveState();
            RepeatingTimerController?.Resume();
            foreach (var s in new[] {"遠征終了", "入渠終了", "疲労回復", "泊地修理", "大破警告"})
                RepeatingTimerController?.Stop(s);
            return Update.All;
        }

        private Update ApiMember(string url, string request, dynamic json)
        {
            var data = json.api_data() ? json.api_data : new object();

            if (url.EndsWith("api_get_member/require_info"))
            {
                _itemInfo.InspectSlotItem(data.api_slot_item, true);
                if (data.api_useitem())
                    _itemInfo.InspectUseItem(data.api_useitem);
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
            if (url.EndsWith("api_get_member/useitem"))
            {
                if (data == null)
                    return Update.None;
                _itemInfo.InspectUseItem(data);
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
                _questInfo.InspectQuestList(request, data);
                return Update.QuestList;
            }
            if (url.EndsWith("api_get_member/deck"))
            {
                _shipInfo.InspectDeck(data);
                _missionInfo.InspectDeck(data);
                _akashiTimer.CheckFleet();
                _questCounter.InspectDeck(data);
                return Update.Mission | Update.Timer;
            }
            if (url.EndsWith("api_get_member/ship2"))
            {
                // ここだけjsonなので注意
                _shipInfo.InspectShip(url, json);
                _akashiTimer.CheckFleet();
                _battleInfo.BattleState = BattleState.None;
                return Update.Item | Update.Ship;
            }
            if (url.EndsWith("api_get_member/ship_deck"))
            {
                _shipInfo.InspectShip(url, data);
                _akashiTimer.CheckFleet();
                _battleInfo.BattleState = BattleState.None;
                return Update.Ship | Update.Item;
            }
            if (url.EndsWith("api_get_member/ship3"))
            {
                _shipInfo.InspectShip(url, data);
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
                    _airBase.Inspect(data.api_air_base);
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
                _airBase.Inspect(data);
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
                _questCounter.InspectCreateItem(request);
                return Update.Item | Update.QuestList;
            }
            if (url.EndsWith("api_req_kousyou/getship"))
            {
                _itemInfo.InspectGetShip(data);
                _shipInfo.InspectShip(url, data);
                _dockInfo.InspectKDock(data.api_kdock);
                _conditionTimer.CheckCond();
                RepeatingTimerController?.Stop("建造完了");
                return Update.Item | Update.Timer;
            }
            if (url.EndsWith("api_req_kousyou/destroyship"))
            {
                _shipInfo.InspectDestroyShip(request);
                _materialInfo.InspectDestroyShip(data);
                _conditionTimer.CheckCond();
                _akashiTimer.CheckFleet();
                _questCounter.InspectDestroyShip(request);
                return Update.Item | Update.Ship | Update.QuestList;
            }
            if (url.EndsWith("api_req_kousyou/destroyitem2"))
            {
                _questCounter.InspectDestroyItem(request); // 本当に削除される前
                _itemInfo.InspectDestroyItem(request);
                _materialInfo.InspectDestroyItem(data);
                return Update.Item | Update.QuestList;
            }
            if (url.EndsWith("api_req_kousyou/remodel_slot"))
            {
                _logger.SetCurrentMaterial(_materialInfo.Current);
                _logger.InspectRemodelSlot(request, data); // 資材の差が必要なので_materialInfoより前
                _itemInfo.InspectRemodelSlot(data);
                _materialInfo.InspectRemodelSlot(data);
                _questCounter.CountRemodelSlot();
                return Update.Item | Update.QuestList;
            }
            if (url.EndsWith("api_req_kousyou/createship"))
            {
                _logger.InspectCreateShip(request);
                _questCounter.CountCreateShip();
                return Update.QuestList;
            }
            if (url.EndsWith("api_req_kousyou/createship_speedchange"))
            {
                _dockInfo.InspectCreateShipSpeedChange(request);
                return Update.Timer;
            }
            return Update.None;
        }

        private Update ApiPractice(string url, string request, dynamic data)
        {
            if (url.EndsWith("api_req_practice/battle_result"))
            {
                _battleInfo.InspectPracticeResult(data);
                _questCounter.InspectPracticeResult(data);
                return Update.Ship | Update.QuestList;
            }
            if (url.EndsWith("api_req_practice/battle"))
            {
                _shipInfo.StartPractice(request);
                _questCounter.StartPractice(request);
                _cellInfo.StartPractice();
                _conditionTimer.InvalidateCond();
                RepeatingTimerController?.Suspend();
            }
            if (url.EndsWith("api_req_practice/battle") || url.EndsWith("api_req_practice/midnight_battle"))
            {
                _battleInfo.InspectBattle(url, request, data);
                return Update.Ship | Update.Battle | Update.Timer;
            }
            return Update.None;
        }

        private Update ApiBattle(string url, string request, dynamic data)
        {
            if (url.EndsWith("/battleresult"))
            {
                _battleInfo.InspectBattleResult(data);
                _exMapInfo.InspectBattleResult(data);
                _logger.InspectBattleResult(data);
                _questCounter.InspectBattleResult(data);
                _miscTextInfo.InspectBattleResult(data);
                return Update.Ship | Update.QuestList;
            }
            if (url.EndsWith("/goback_port"))
            {
                _battleInfo.CauseEscape();
                return Update.Ship;
            }
            _shipInfo.ClearBadlyDamagedShips();
            RepeatingTimerController?.Stop("大破警告");
            _battleInfo.InspectBattle(url, request, data);
            _cellInfo.StartBattle();
            return Update.Ship | Update.Battle;
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

        private Update ApiKaisou(string url, string request, dynamic data)
        {
            if (url.EndsWith("api_req_kaisou/powerup"))
            {
                _questCounter.InspectPowerUp(request, data); // 艦種が必要なので艦が消える前
                _shipInfo.InspectPowerUp(request, data);
                _conditionTimer.CheckCond();
                _akashiTimer.CheckFleet();
                return Update.Item | Update.Ship | Update.QuestList;
            }
            if (url.EndsWith("api_req_kaisou/slot_exchange_index"))
            {
                _shipInfo.InspectSlotExchange(data);
                return Update.Ship;
            }
            if (url.EndsWith("api_req_kaisou/slot_deprive"))
            {
                _shipInfo.InspectSlotDeprive(data);
                return Update.Ship;
            }
            if (url.EndsWith("api_req_kaisou/marriage"))
            {
                _shipInfo.InspectMarriage(data);
                return Update.Ship;
            }
            return Update.None;
        }

        private Update ApiAirCorps(string url, string request, dynamic data)
        {
            if (url.EndsWith("api_req_air_corps/supply"))
            {
                _materialInfo.InspectAirCorpsSupply(data);
                _airBase.InspectSupply(request, data);
                return Update.Item;
            }
            if (url.EndsWith("api_req_air_corps/set_plane"))
            {
                _materialInfo.InspectAirCorpsSetPlane(data);
                _airBase.InspectSetPlane(request, data);
                return Update.Item | Update.Ship;
            }
            if (url.EndsWith("api_req_air_corps/set_action"))
            {
                _airBase.InspectSetAction(request);
                return Update.Ship;
            }
            if (url.EndsWith("api_req_air_corps/expand_base"))
            {
                _airBase.InspectExpandBase(request, data);
                return Update.Ship;
            }
            return Update.None;
        }

        private Update ApiMap(string url, string request, dynamic data)
        {
            if (url.EndsWith("api_req_map/start"))
            {
                _shipInfo.InspectMapStart(request); // 出撃中判定が必要なので_conditionTimerより前
                _conditionTimer.InvalidateCond();
                _exMapInfo.InspectMapStart(data);
                _battleInfo.InspectMapStart(data);
                _logger.InspectMapStart(data);
                _miscTextInfo.InspectMapStart(data);
                _questCounter.InspectMapStart(data);
                _cellInfo.InspectMapStart(data);
                RepeatingTimerController?.Suspend("大破警告");
                return Update.Timer | Update.Ship | Update.Cell;
            }
            if (url.EndsWith("api_req_map/next"))
            {
                _exMapInfo.InspectMapNext(data);
                _battleInfo.InspectMapNext(data);
                _logger.InspectMapNext(data);
                _questCounter.InspectMapNext(data);
                _miscTextInfo.InspectMapNext(data);
                _cellInfo.InspectMapNext(data);
                return Update.Battle;
            }
            if (url.EndsWith("api_req_map/anchorage_repair"))
            {
                _shipInfo.InspectAnchorageRepair(data);
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
                _questCounter.CountCharge();
                return Update.Item | Update.Ship | Update.QuestList;
            }
            if (url.EndsWith("api_req_nyukyo/start"))
            {
                _dockInfo.InspectNyukyo(request);
                _conditionTimer.CheckCond();
                _akashiTimer.CheckFleet();
                _questCounter.CountNyukyo();
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
                _questCounter.InspectMissionResult(request, data);
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

        public AlarmCounter ItemCounter => _itemInfo.Counter;

        public MaterialInfo Material => _materialInfo;

        public QuestStatus[] Quests => _questInfo.Quests;

        public void GetQuestNotifications(out string[] notify, out string[] stop) =>
            _questInfo.GetNotifications(out notify, out stop);

        public NameAndTimer[] Missions => _missionInfo.Missions;

        public DateTime GetConditionTimer(int fleet) => _conditionTimer.GetTimer(fleet);

        public int[] GetConditionNotice(TimeStep step) => _conditionTimer.GetNotice(step);

        public AlarmCounter ShipCounter => _shipInfo.Counter;

        public IReadOnlyList<Fleet> Fleets => _shipInfo.Fleets;

        public int InSortie => _shipInfo.InSortie;

        public ShipInfo.ShipStatusPair[] BattleResultStatusDiff => _shipInfo.BattleResultDiff;

        public bool IsBattleResultError => _shipInfo.IsBattleResultError || _battleInfo.DisplayedResultRank.IsError;

        public ShipStatus[] BattleStartStatus => _shipInfo.BattleStartStatus;

        public bool IsCombinedFleet => _shipInfo.Fleets[0].CombinedType != 0;

        public ShipStatus[] RepairList => _shipInfo.GetRepairList(_dockInfo);

        public ShipStatus[] ShipList => _shipInfo.ShipList;

        public string[] BadlyDamagedShips => _shipInfo.BadlyDamagedShips;

        public bool WarnBadDamageWithDameCon
        {
            set => _shipInfo.WarnBadDamageWithDameCon = value;
        }

        public ItemStatus[] ItemList
        {
            get
            {
                _itemInfo.ClearHolder();
                _shipInfo.SetItemHolder();
                _airBase.SetItemHolder();
                return _itemInfo.ItemList;
            }
        }

        public AkashiTimer AkashiTimer => _akashiTimer;

        public Achievement Achievement => _achievement;

        public BattleInfo Battle => _battleInfo;

        public ExMapInfo ExMap => _exMapInfo;

        public string MiscText => _miscTextInfo.Text;

        public AirBase.BaseInfo[] AirBase => _airBase.AllBase;

        public CellInfo CellInfo => _cellInfo;

        public void SetLogWriter(Action<string, string, string> writer, Func<DateTime> nowFunc)
        {
            _logger.SetWriter(writer, nowFunc);
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