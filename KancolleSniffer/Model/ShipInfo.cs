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

using System.Collections.Generic;
using System.Linq;
using KancolleSniffer.Util;

namespace KancolleSniffer.Model
{
    public class ShipInfo
    {
        public const int FleetCount = 4;
        public const int MemberCount = 6;

        private readonly IReadOnlyList<Fleet> _fleets;
        private readonly ShipMaster _shipMaster;
        private readonly ShipInventry _shipInventry;
        private readonly ItemInventry _itemInventry;
        private readonly List<int> _escapedShips = new List<int>();
        private ShipStatus[] _battleResult = new ShipStatus[0];
        private readonly NumEquipsChecker _numEquipsChecker = new NumEquipsChecker();
        private int _hqLevel;
        public AlarmCounter Counter { get; }
        public ShipStatusPair[] BattleResultDiff { get; private set; } = new ShipStatusPair[0];
        public bool IsBattleResultError => BattleResultDiff.Length > 0;
        public ShipStatus[] BattleStartStatus { get; private set; } = new ShipStatus[0];
        public int DropShipId { private get; set; } = -1;

        private class NumEquipsChecker
        {
            public int MaxId { private get; set; } = int.MaxValue;

            public void Check(ShipStatus ship)
            {
                var spec = ship.Spec;
                if (spec.NumEquips != -1 || ship.Id <= MaxId)
                    return;
                spec.NumEquips = ship.Slot.Count(item => !item.Empty);
            }
        }

        public class ShipStatusPair
        {
            public ShipStatus Assumed { get; set; }
            public ShipStatus Actual { get; set; }

            public ShipStatusPair(ShipStatus assumed, ShipStatus actual)
            {
                Assumed = assumed;
                Actual = actual;
            }
        }

        public ShipInfo(ShipMaster shipMaster, ShipInventry shipInventry, ItemInventry itemInventry)
        {
            _shipMaster = shipMaster;
            _shipInventry = shipInventry;
            _fleets = Enumerable.Range(0, FleetCount).Select((x, i) => new Fleet(_shipInventry, i, () => _hqLevel)).ToArray();
            _itemInventry = itemInventry;
            Counter = new AlarmCounter(() => _shipInventry.Count){Margin = 4};
        }

        public void InspectMaster(dynamic json)
        {
            _shipMaster.Inspect(json);
            _shipInventry.Clear();
        }

        public void InspectShip(dynamic json)
        {
            if (json.api_deck_port()) // port
            {
                _shipInventry.Clear();
                _escapedShips.Clear();
                for (var i = 0; i < FleetCount; i++)
                    _fleets[i].State = FleetState.Port;
                InspectShipDataAndDeck(json.api_ship, json.api_deck_port);
                InspectBasic(json.api_basic);
                if (json.api_combined_flag())
                    _fleets[0].CombinedType = _fleets[1].CombinedType = (CombinedType)(int)json.api_combined_flag;
                VerifyBattleResult();
            }
            else if (json.api_data()) // ship2
            {
                InspectShipDataAndDeck(json.api_data, json.api_data_deck);
            }
            else if (json.api_ship_data()) // ship3とship_deck
            {
                InspectShipDataAndDeck(json.api_ship_data, json.api_deck_data);
                VerifyBattleResult();
                // ship_deckでドロップ艦を反映する
                if (DropShipId != -1)
                {
                    _shipInventry.InflateCount(1);
                    var num = _shipMaster.GetSpec(DropShipId).NumEquips;
                    if (num > 0)
                        _itemInventry.InflateCount(num);
                }
            }
            else if (json.api_ship()) // getship
            {
                InspectShipData(new[] {json.api_ship});
            }
            DropShipId = -1;
        }

        public void SaveBattleResult()
        {
            _battleResult = _fleets.Where(fleet =>
                    fleet.State >= FleetState.Sortie && !fleet.Ships[0].Spec.IsRepairShip)
                .SelectMany(fleet => fleet.Ships).ToArray();
        }

        private void VerifyBattleResult()
        {
            BattleResultDiff = (from assumed in _battleResult
                let actual = GetShip(assumed.Id)
                where !assumed.Escaped && assumed.NowHp != actual.NowHp
                select new ShipStatusPair(assumed, actual)).ToArray();
            _battleResult = new ShipStatus[0];
        }

        public void SaveBattleStartStatus()
        {
            BattleStartStatus = _fleets.Where(fleet => fleet.State >= FleetState.Sortie)
                .SelectMany(fleet => fleet.Ships.Select(ship => (ShipStatus)ship.Clone())).ToArray();
        }

        private void InspectShipDataAndDeck(dynamic ship, dynamic deck)
        {
            InspectShipData(ship);
            InspectDeck(deck); // FleetのDeckを設定した時点でShipStatusを取得するので必ずdeckが後
        }

        public void InspectDeck(dynamic json)
        {
            foreach (var entry in json)
            {
                var fleet = (int)entry.api_id - 1;
                _fleets[fleet].Deck = (int[])entry.api_ship;
                if ((int)entry.api_mission[0] != 0)
                    _fleets[fleet].State = FleetState.Mission;
            }
        }

        private void InspectShipData(dynamic json)
        {
            foreach (var entry in json)
            {
                var id = (int)entry.api_id;
                var ship = new ShipStatus
                {
                    Id = id,
                    Spec = _shipMaster.GetSpec((int)entry.api_ship_id),
                    Level = (int)entry.api_lv,
                    ExpToNext = (int)entry.api_exp[1],
                    MaxHp = (int)entry.api_maxhp,
                    NowHp = (int)entry.api_nowhp,
                    Cond = (int)entry.api_cond,
                    Fuel = (int)entry.api_fuel,
                    Bull = (int)entry.api_bull,
                    OnSlot = (int[])entry.api_onslot,
                    GetItem = item => _itemInventry[item.Id],
                    Slot = ((int[])entry.api_slot).Select(item => new ItemStatus(item)).ToArray(),
                    SlotEx = entry.api_slot_ex() ? new ItemStatus((int)entry.api_slot_ex) : new ItemStatus(0),
                    NdockTime = (int)entry.api_ndock_time,
                    NdockItem = (int[])entry.api_ndock_item,
                    LoS = (int)entry.api_sakuteki[0],
                    Firepower = (int)entry.api_karyoku[0],
                    Torpedo = (int)entry.api_raisou[0],
                    AntiSubmarine = (int)entry.api_taisen[0],
                    AntiAir = (int)entry.api_taiku[0],
                    Lucky = (int)entry.api_lucky[0],
                    Locked = entry.api_locked() && entry.api_locked == 1,
                    Escaped = _escapedShips.Contains(id)
                };
                _shipInventry.Add(ship);
                _numEquipsChecker.Check(ship);
            }
            _numEquipsChecker.MaxId = _shipInventry.MaxId;
        }

        private void InspectBasic(dynamic json)
        {
            _hqLevel = (int)json.api_level;
            Counter.Max = (int)json.api_max_chara;
        }

        public void InspectCharge(dynamic json)
        {
            foreach (var entry in json.api_ship)
            {
                var status = _shipInventry[(int)entry.api_id];
                status.Bull = (int)entry.api_bull;
                status.Fuel = (int)entry.api_fuel;
                status.OnSlot = (from num in (dynamic[])entry.api_onslot select (int)num).ToArray();
            }
        }

        public void InspectChange(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var dstFleet = _fleets[int.Parse(values["api_id"]) - 1];
            var dstIdx = int.Parse(values["api_ship_idx"]);
            var shipId = int.Parse(values["api_ship_id"]);

            if (dstIdx == -1 || shipId == -1) // 解除
            {
                dstFleet.WithdrowShip(dstIdx);
                return;
            }
            var srcFleet = FindFleet(shipId, out var srcIdx);
            var prevShipId = dstFleet.SetShip(dstIdx, shipId);
            if (srcFleet == null)
                return;
            // 入れ替えの場合
            srcFleet.SetShip(srcIdx, prevShipId);
            if (prevShipId == -1)
                srcFleet.WithdrowShip(srcIdx);
        }

        private Fleet FindFleet(int ship, out int idx)
        {
            foreach (var fleet in _fleets)
            {
                idx = fleet.Deck.ToList().IndexOf(ship);
                if (idx < 0)
                    continue;
                return fleet;
            }
            idx = -1;
            return null;
        }

        public void InspectPowerup(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            var ships = values["api_id_items"].Split(',').Select(int.Parse).ToArray();
            if (!_shipInventry.Contains(ships[0])) // 二重に実行された場合
                return;
            _itemInventry.Remove(ships.SelectMany(id => _shipInventry[id].Slot));
            _shipInventry.Remove(ships);
            InspectShipDataAndDeck(new[]{json.api_ship}, json.api_deck);
        }

        public void InspectSlotExchange(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            var ship = int.Parse(values["api_id"]);
            _shipInventry[ship].Slot = ((int[])json.api_slot).Select(id => new ItemStatus(id)).ToArray();
        }

        public void InspectSlotDeprive(dynamic json)
        {
            InspectShipData(new[] {json.api_ship_data.api_set_ship, json.api_ship_data.api_unset_ship});
            foreach (var fleet in _fleets)
                fleet.SetDeck(); // ShipStatusの差し替え
        }

        public void InspectDestroyShip(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            var delitem = int.Parse(values["api_slot_dest_flag"] ?? "0") == 1;
            foreach (var shipId in values["api_ship_id"].Split(',').Select(int.Parse))
            {
                if (delitem)
                    _itemInventry.Remove(_shipInventry[shipId].AllSlot);
                var srcFleet = FindFleet(shipId, out var srcIdx);
                srcFleet?.WithdrowShip(srcIdx);
                _shipInventry.Remove(shipId);
            }
        }

        public void InspectCombined(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            _fleets[0].CombinedType = _fleets[1].CombinedType = (CombinedType)int.Parse(values["api_combined_type"]);
        }

        public void InspectMapStart(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var fleet = int.Parse(values["api_deck_id"]) - 1;
            if (_fleets[0].CombinedType == 0 || fleet > 1)
            {
                _fleets[fleet].State = FleetState.Sortie;
            }
            else
            {
                _fleets[0].State = _fleets[1].State = FleetState.Sortie;
            }
            SetBadlyDamagedShips();
        }

        public void StartPractice(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var fleet = int.Parse(values["api_deck_id"]) - 1;
            _fleets[fleet].State = FleetState.Practice;
        }

        public IReadOnlyList<Fleet> Fleets => _fleets;

        public ShipStatus GetShip(int id) => _shipInventry[id];

        public void SetItemHolder()
        {
            foreach (var ship in _shipInventry.AllShips)
            {
                foreach (var item in ship.Slot)
                    _itemInventry[item.Id].Holder = ship;
                _itemInventry[ship.SlotEx.Id].Holder = ship;
            }
        }

        public ShipSpec GetSpec(int id) => _shipMaster.GetSpec(id);

        public ShipStatus[] ShipList => _shipInventry.AllShips.ToArray();

        public ShipStatus[] GetRepairList(DockInfo dockInfo)
            => (from s in ShipList
                where s.NowHp < s.MaxHp && !dockInfo.InNDock(s.Id) &&
                      (s.Fleet == null || s.Fleet.State != FleetState.Practice)
                select s).OrderByDescending(s => s.RepairTime).ToArray();

        public string[] BadlyDamagedShips { get; private set; } = new string[0];

        public void SetBadlyDamagedShips()
        {
            BadlyDamagedShips =
            (from s in _fleets.Where(fleet => fleet.State == FleetState.Sortie)
                    .SelectMany(fleet => fleet.CombinedType != 0 && fleet.Number == 1
                        ? fleet.ActualShips.Skip(1) // 第二艦隊の旗艦を除く
                        : fleet.ActualShips)
                where !s.Escaped && s.DamageLevel == ShipStatus.Damage.Badly
                select s.Name).ToArray();
        }

        public void ClearBadlyDamagedShips()
        {
            BadlyDamagedShips = new string[0];
        }

        public void SetEscapedShips(List<int> ids)
        {
            _escapedShips.AddRange(ids);
            foreach (var id in ids)
                _shipInventry[id].Escaped = true;
        }
    }
}