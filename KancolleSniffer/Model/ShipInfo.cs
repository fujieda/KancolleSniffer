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
using KancolleSniffer.Util;
using static System.Math;

namespace KancolleSniffer.Model
{
    public class ShipInfo
    {
        public const int FleetCount = 4;
        public const int MemberCount = 6;

        private readonly Fleet[] _fleets;
        private readonly Dictionary<int, ShipStatus> _shipInfo = new Dictionary<int, ShipStatus>();
        private readonly ShipMaster _shipMaster;
        private readonly ItemInfo _itemInfo;
        private readonly List<int> _escapedShips = new List<int>();
        private ShipStatus[] _battleResult = new ShipStatus[0];
        private readonly NumEquipsChecker _numEquipsChecker = new NumEquipsChecker();
        public int HqLevel { get; private set; }
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
                spec.NumEquips = ship.Slot.Count(item => item.Id != -1);
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

        public ShipInfo(ShipMaster shipMaster, ItemInfo itemInfo)
        {
            _shipMaster = shipMaster;
            _fleets = Enumerable.Range(0, FleetCount).Select((x, i) => new Fleet(this, i)).ToArray();
            _itemInfo = itemInfo;
            ClearShipInfo();
        }

        public void InspectMaster(dynamic json)
        {
            _shipMaster.Inspect(json);
            ClearShipInfo();
        }

        public void InspectShip(dynamic json)
        {
            if (json.api_deck_port()) // port
            {
                ClearShipInfo();
                for (var i = 0; i < FleetCount; i++)
                    _fleets[i].State = FleetState.Port;
                InspectDeck(json.api_deck_port);
                InspectShipData(json.api_ship);
                InspectBasic(json.api_basic);
                if (json.api_combined_flag())
                    _fleets[0].CombinedType = _fleets[1].CombinedType = (CombinedType)(int)json.api_combined_flag;
                _itemInfo.NowShips = ((object[])json.api_ship).Length;
                VerifyBattleResult();
            }
            else if (json.api_data()) // ship2
            {
                InspectDeck(json.api_data_deck);
                InspectShipData(json.api_data);
            }
            else if (json.api_ship_data()) // ship3とship_deck
            {
                InspectDeck(json.api_deck_data);
                InspectShipData(json.api_ship_data);
                VerifyBattleResult();
                // ship_deckでドロップ艦を反映する
                if (DropShipId != -1)
                {
                    _itemInfo.NowShips++;
                    var num = _shipMaster.GetSpec(DropShipId).NumEquips;
                    if (num > 0)
                        _itemInfo.NowEquips += num;
                }
            }
            else if (json.api_ship()) // getshipとpowerup
            {
                InspectShipData(new[] {json.api_ship});
            }
            DropShipId = -1;
        }

        public void SaveBattleResult()
        {
            _battleResult = _fleets.Where(fleet =>
                    fleet.State >= FleetState.Sortie && !GetStatus(fleet.Deck[0]).Spec.IsRepairShip)
                .SelectMany(fleet => fleet.Deck.Select(GetStatus)).ToArray();
        }

        private void VerifyBattleResult()
        {
            BattleResultDiff = (from assumed in _battleResult
                let actual = GetStatus(assumed.Id)
                where !assumed.Escaped && assumed.NowHp != actual.NowHp
                select new ShipStatusPair(assumed, actual)).ToArray();
            _battleResult = new ShipStatus[0];
        }

        public void SaveBattleStartStatus()
        {
            BattleStartStatus = _fleets.Where(fleet => fleet.State >= FleetState.Sortie)
                .SelectMany(fleet => fleet.Deck.Select(id => (ShipStatus)GetStatus(id).Clone())).ToArray();
        }

        private void ClearShipInfo()
        {
            _shipInfo.Clear();
            _shipInfo[-1] = new ShipStatus();
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
                    Locked = entry.api_locked() && entry.api_locked == 1
                };
                _shipInfo[id] = ship;
                _numEquipsChecker.Check(ship);
            }
            _numEquipsChecker.MaxId = _shipInfo.Keys.Max();
        }

        private void InspectBasic(dynamic json)
        {
            HqLevel = (int)json.api_level;
        }

        public void InspectCharge(dynamic json)
        {
            foreach (var entry in json.api_ship)
            {
                var status = _shipInfo[(int)entry.api_id];
                status.Bull = (int)entry.api_bull;
                status.Fuel = (int)entry.api_fuel;
                status.OnSlot = (from num in (dynamic[])entry.api_onslot select (int)num).ToArray();
            }
        }

        public void InspectChange(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var fleet = _fleets[int.Parse(values["api_id"]) - 1];
            var idx = int.Parse(values["api_ship_idx"]);
            var ship = int.Parse(values["api_ship_id"]);

            if (idx == -1)
            {
                var deck = fleet.Deck;
                for (var i = 1; i < deck.Length; i++)
                    deck[i] = -1;
                return;
            }
            if (ship == -1)
            {
                WithdrowShip(fleet, idx);
                return;
            }
            var of = FindFleet(ship, out var oi);
            var orig = fleet.Deck[idx];
            fleet.Deck[idx] = ship;
            if (of == null)
                return;
            // 入れ替えの場合
            if ((of.Deck[oi] = orig) == -1)
                WithdrowShip(of, oi);
        }

        private Fleet FindFleet(int ship, out int idx)
        {
            foreach (var fleet in _fleets)
            {
                idx = Array.FindIndex(fleet.Deck, id => id == ship);
                if (idx < 0)
                    continue;
                return fleet;
            }
            idx = -1;
            return null;
        }

        private void WithdrowShip(Fleet fleet, int idx)
        {
            var deck = fleet.Deck;
            var j = idx;
            for (var i = idx + 1; i < deck.Length; i++)
            {
                if (deck[i] != -1)
                    deck[j++] = deck[i];
            }
            for (; j < deck.Length; j++)
                deck[j] = -1;
        }

        public void InspectPowerup(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            var ships = values["api_id_items"].Split(',').Select(int.Parse).ToArray();
            if (!_shipInfo.ContainsKey(ships[0])) // 二重に実行された場合
                return;
            _itemInfo.NowShips -= ships.Length;
            _itemInfo.DeleteItems(ships.SelectMany(id => _shipInfo[id].Slot).ToArray());
            foreach (var id in ships)
                _shipInfo.Remove(id);
            InspectDeck(json.api_deck);
            InspectShip(json);
        }

        public void InspectSlotExchange(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            var ship = int.Parse(values["api_id"]);
            _shipInfo[ship].Slot = ((int[])json.api_slot).Select(id => new ItemStatus(id)).ToArray();
        }

        public void InspectSlotDeprive(dynamic json)
        {
            InspectShipData(new[] {json.api_ship_data.api_set_ship, json.api_ship_data.api_unset_ship});
        }

        public void InspectDestroyShip(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            var delitem = int.Parse(values["api_slot_dest_flag"] ?? "0") == 1;
            foreach (var ship in values["api_ship_id"].Split(',').Select(int.Parse))
            {
                _itemInfo.NowShips--;
                if (delitem)
                    _itemInfo.DeleteItems(_shipInfo[ship].AllSlot);
                var of = FindFleet(ship, out var oi);
                if (of != null)
                    WithdrowShip(of, oi);
                _shipInfo.Remove(ship);
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

        public void RepairShip(int id)
        {
            var s = _shipInfo[id];
            s.NowHp = s.MaxHp;
            s.Cond = Max(40, s.Cond);
        }

        public Fleet[] Fleets => _fleets;

        public ShipStatus GetStatus(int id)
        {
            if (!_shipInfo.TryGetValue(id, out var s))
                return new ShipStatus();
            s.Slot = s.Slot.Select(item => _itemInfo.GetStatus(item.Id)).ToArray();
            s.SlotEx = _itemInfo.GetStatus(s.SlotEx.Id);
            s.Escaped = _escapedShips.Contains(id);
            s.Fleet = FindFleet(s.Id, out var idx);
            s.DeckIndex = idx;
            return s;
        }

        public void SetItemHolder()
        {
            foreach (var ship in _shipInfo.Values)
            {
                foreach (var item in ship.Slot)
                    _itemInfo.GetStatus(item.Id).Holder = ship;
                _itemInfo.GetStatus(ship.SlotEx.Id).Holder = ship;
            }
        }

        public ShipSpec GetSpec(int id) => _shipMaster.GetSpec(id);

        public ShipStatus[] ShipList => _shipInfo.Keys.Where(id => id != -1).Select(GetStatus).ToArray();

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
                    .SelectMany(fleet => fleet.Deck.Where(id => id != -1).Select(GetStatus))
                where !s.Escaped && s.DamageLevel == ShipStatus.Damage.Badly &&
                      !(s.Fleet.CombinedType != 0 && s.Fleet.Number == 1 && s.DeckIndex == 0) // 第二艦隊の旗艦を除く
                select s.Name).ToArray();
        }

        public void ClearBadlyDamagedShips()
        {
            BadlyDamagedShips = new string[0];
        }

        public void SetEscapedShips(List<int> ships)
        {
            _escapedShips.AddRange(ships);
        }

        public void ClearEscapedShips()
        {
            _escapedShips.Clear();
        }

        public void InjectShips(dynamic battle, dynamic item)
        {
            var deck = (int)battle.api_deck_id - 1;
            InjectShips(deck, (int[])battle.api_f_nowhps, (int[])battle.api_f_maxhps, (int[][])item[0]);
            if (battle.api_f_nowhps_combined())
                InjectShips(1, (int[])battle.api_f_nowhps_combined, (int[])battle.api_f_maxhps_combined,
                    (int[][])item[1]);
            foreach (var enemy in (int[])battle.api_ship_ke)
                _shipMaster.InjectSpec(enemy);
            if (battle.api_ship_ke_combined())
            {
                foreach (var enemy in (int[])battle.api_ship_ke_combined)
                    _shipMaster.InjectSpec(enemy);
            }
            _itemInfo.InjectItems(((int[][])battle.api_eSlot).SelectMany(x => x));
            if (battle.api_eSlot_combined())
                _itemInfo.InjectItems(((int[][])battle.api_eSlot_combined).SelectMany(x => x));
        }

        private void InjectShips(int deck, int[] nowhps, int[] maxhps, int[][] slots)
        {
            var id = _shipInfo.Keys.Count + 1;
            var ships = nowhps.Zip(maxhps,
                (now, max) => new ShipStatus {Id = id++, NowHp = now, MaxHp = max}).ToArray();
            _fleets[deck].Deck = (from ship in ships select ship.Id).ToArray();
            foreach (var ship in ships)
                _shipInfo[ship.Id] = ship;
            foreach (var entry in ships.Zip(slots, (ship, slot) => new {ship, slot}))
            {
                entry.ship.Slot = _itemInfo.InjectItems(entry.slot.Take(5));
                if (entry.slot.Length >= 6)
                    entry.ship.SlotEx = _itemInfo.InjectItems(entry.slot.Skip(5)).First();
            }
        }
    }
}