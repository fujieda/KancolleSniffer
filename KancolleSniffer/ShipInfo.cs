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
using static System.Math;

namespace KancolleSniffer
{
    public struct ChargeStatus
    {
        public int Fuel { get; set; }
        public int Bull { get; set; }

        public ChargeStatus(ShipStatus status) : this()
        {
            Fuel = CalcChargeState(status.Fuel, status.Spec.FuelMax);
            Bull = CalcChargeState(status.Bull, status.Spec.BullMax);
        }

        public ChargeStatus(int fuel, int bull) : this()
        {
            Fuel = fuel;
            Bull = bull;
        }

        private int CalcChargeState(int now, int full)
        {
            if (full == 0 || now == full)
                return 0;
            var ratio = (double)now / full;
            if (ratio >= 7.0 / 9)
                return 1;
            if (ratio >= 3.0 / 9)
                return 2;
            if (ratio > 0)
                return 3;
            return 4;
        }
    }

    public class ShipInfo
    {
        public const int FleetCount = 4;
        public const int MemberCount = 6;

        private readonly int[][] _decks;
        private readonly Dictionary<int, ShipStatus> _shipInfo = new Dictionary<int, ShipStatus>();
        private readonly ShipMaster _shipMaster = new ShipMaster();
        private readonly ItemInfo _itemInfo;
        private readonly bool[] _inMission = new bool[FleetCount];
        private readonly bool[] _inSortie = new bool[FleetCount];
        private int _hqLevel;
        private readonly List<int> _escapedShips = new List<int>();
        private int _combinedFleetType;
        private ShipStatus[] _battleResult = new ShipStatus[0];
        public ShipStatusPair[] BattleResultDiff { get; private set; } = new ShipStatusPair[0];
        public bool IsBattleResultError => BattleResultDiff.Length > 0;
        public ShipStatus[] BattleStartStatus { get; private set; } = new ShipStatus[0];

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

        public ShipInfo(ItemInfo itemInfo)
        {
            _itemInfo = itemInfo;
            _decks = Enumerable.Repeat(Enumerable.Repeat(-1, MemberCount).ToArray(), FleetCount).ToArray();
            ClearShipInfo();
        }

        public void InspectMaster(dynamic json)
        {
            _shipMaster.Inspect(json);
        }

        public void InspectShip(dynamic json)
        {
            if (json.api_deck_port()) // port
            {
                ClearShipInfo();
                for (var i = 0; i < FleetCount; i++)
                    _inSortie[i] = false;
                InspectDeck(json.api_deck_port);
                InspectShipData(json.api_ship);
                InspectBasic(json.api_basic);
                if (json.api_combined_flag())
                    _combinedFleetType = (int)json.api_combined_flag;
                _itemInfo.NowShips = ((object[])json.api_ship).Length;
                VerifyBattleResult();
            }
            else if (json.api_data()) // ship2
            {
                // 一隻分のデータしか来ないことがあるので艦娘数を数えない
                InspectDeck(json.api_data_deck);
                InspectShipData(json.api_data);
            }
            else if (json.api_ship_data()) // ship3とship_deck
            {
                // 一隻分のデータしか来ないことがあるので艦娘数を数えない
                InspectDeck(json.api_deck_data);
                InspectShipData(json.api_ship_data);
                VerifyBattleResult();
            }
            else if (json.api_ship()) // getshipとpowerup
            {
                InspectShipData(new[] {json.api_ship});
            }
        }

        public void SaveBattleResult()
        {
            _battleResult = _decks.Where((deck, i) =>
                    _inSortie[i] && !GetStatus(deck[0]).Spec.IsRepairShip)
                .SelectMany(deck => deck.Select(GetStatus)).ToArray();
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
            BattleStartStatus = _decks.Where((deck, i) => _inSortie[i])
                .SelectMany(deck => deck.Select(id => (ShipStatus)GetStatus(id).Clone())).ToArray();
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
                _decks[fleet] = (int[])entry.api_ship;
                _inMission[fleet] = (int)entry.api_mission[0] != 0;
            }
        }

        private void InspectShipData(dynamic json)
        {
            foreach (var entry in json)
            {
                _shipInfo[(int)entry.api_id] = new ShipStatus
                {
                    Id = (int)entry.api_id,
                    Spec = _shipMaster[(int)entry.api_ship_id],
                    Level = (int)entry.api_lv,
                    ExpToNext = (int)entry.api_exp[1],
                    MaxHp = (int)entry.api_maxhp,
                    NowHp = (int)entry.api_nowhp,
                    Cond = (int)entry.api_cond,
                    Fuel = (int)entry.api_fuel,
                    Bull = (int)entry.api_bull,
                    OnSlot = (int[])entry.api_onslot,
                    Slot = ((int[])entry.api_slot).Select(id => new ItemStatus(id)).ToArray(),
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
            }
        }

        private void InspectBasic(dynamic json)
        {
            _hqLevel = (int)json.api_level;
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
            var fleet = int.Parse(values["api_id"]) - 1;
            var idx = int.Parse(values["api_ship_idx"]);
            var ship = int.Parse(values["api_ship_id"]);
            if (idx == -1)
            {
                var deck = _decks[fleet];
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
            var orig = _decks[fleet][idx];
            _decks[fleet][idx] = ship;
            if (of == -1)
                return;
            // 入れ替えの場合
            if ((_decks[of][oi] = orig) == -1)
                WithdrowShip(of, oi);
        }

        private int FindFleet(int ship, out int idx)
        {
            for (var f = 0; f < _decks.Length; f++)
            {
                idx = Array.FindIndex(_decks[f], id => id == ship);
                if (idx < 0)
                    continue;
                return f;
            }
            idx = -1;
            return -1;
        }

        private void WithdrowShip(int fleet, int idx)
        {
            var deck = _decks[fleet];
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
                if (of != -1)
                    WithdrowShip(of, oi);
                _shipInfo.Remove(ship);
            }
        }

        public void InspectCombined(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            _combinedFleetType = int.Parse(values["api_combined_type"]);
        }

        public void InspectMapStart(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var fleet = int.Parse(values["api_deck_id"]) - 1;
            if (_combinedFleetType == 0 || fleet > 1)
            {
                _inSortie[fleet] = true;
            }
            else
            {
                _inSortie[0] = _inSortie[1] = true;
            }
            SetBadlyDamagedShips();
        }

        public void RepairShip(int id)
        {
            var s = _shipInfo[id];
            s.NowHp = s.MaxHp;
            s.Cond = Max(40, s.Cond);
        }

        public ShipStatus[] GetShipStatuses(int fleet)
        {
            return _decks[fleet].Where(id => id != -1).Select(GetStatus).ToArray();
        }

        public int[] GetDeck(int fleet) => _decks[fleet];

        public ShipStatus GetStatus(int id)
        {
            if (!_shipInfo.TryGetValue(id, out var s))
                return new ShipStatus();
            s.Slot = s.Slot.Select(item => _itemInfo.GetStatus(item.Id)).ToArray();
            s.SlotEx = _itemInfo.GetStatus(s.SlotEx.Id);
            s.Escaped = _escapedShips.Contains(id);
            s.Fleet = FindFleet(s.Id, out var idx);
            s.DeckIndex = idx;
            s.CombinedFleetType = s.Fleet < 2 ? _combinedFleetType : 0;
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

        public ShipSpec GetSpec(int id) => _shipMaster[id];

        public bool InMission(int fleet) => _inMission[fleet];

        public bool InSortie(int fleet) => _inSortie[fleet];

        public int CombinedFleetType => _combinedFleetType;

        public ShipStatus[] ShipList => _shipInfo.Keys.Where(id => id != -1).Select(GetStatus).ToArray();

        public ChargeStatus[] ChargeStatuses
            => (from deck in _decks
                let flag = new ChargeStatus(_shipInfo[deck[0]])
                let others = (from id in deck.Skip(1)
                        select new ChargeStatus(_shipInfo[id]))
                    .Aggregate(
                        (result, next) =>
                            new ChargeStatus(Max(result.Fuel, next.Fuel), Max(result.Bull, next.Bull)))
                select new ChargeStatus(flag.Fuel != 0 ? flag.Fuel : others.Fuel + 5,
                    flag.Bull != 0 ? flag.Bull : others.Bull + 5)).ToArray();

        public int[] GetFighterPower(int fleet)
            => GetShipStatuses(fleet).Where(ship => !ship.Escaped).SelectMany(ship =>
                    ship.Slot.Zip(ship.OnSlot, (slot, onslot) => slot.CalcFighterPower(onslot)))
                .Aggregate(new[] {0, 0}, (prev, cur) => new[] {prev[0] + cur[0], prev[1] + cur[1]});

        public double GetContactTriggerRate(int fleet)
            => GetShipStatuses(fleet).Where(ship => !ship.Escaped).SelectMany(ship =>
                ship.Slot.Zip(ship.OnSlot, (slot, onslot) =>
                    slot.Spec.ContactTriggerRate * slot.Spec.LoS * Sqrt(onslot))).Sum();

        public ShipStatus[] GetRepairList(DockInfo dockInfo)
            => (from s in ShipList
                where s.NowHp < s.MaxHp && !dockInfo.InNDock(s.Id)
                select s).OrderByDescending(s => s.RepairTime).ToArray();

        public double GetLineOfSights(int fleet, int factor)
        {
            var result = 0.0;
            var emptyBonus = 6;
            foreach (var s in GetShipStatuses(fleet))
            {
                emptyBonus--;
                var itemLoS = 0;
                foreach (var item in s.Slot)
                {
                    var spec = item.Spec;
                    itemLoS += spec.LoS;
                    result += (spec.LoS + item.LoSLevelBonus) * spec.LoSScaleFactor * factor;
                }
                result += Sqrt(s.LoS - itemLoS);
            }
            return result > 0 ? result - Ceiling(_hqLevel * 0.4) + emptyBonus * 2 : 0.0;
        }

        public double GetDaihatsuBonus(int fleet)
        {
            var tokudaiBonus = new[,]
            {
                {0.00, 0.00, 0.00, 0.00, 0.00},
                {0.02, 0.02, 0.02, 0.02, 0.02},
                {0.04, 0.04, 0.04, 0.04, 0.04},
                {0.05, 0.05, 0.052, 0.054, 0.054},
                {0.054, 0.056, 0.058, 0.059, 0.06}
            };
            var daihatsu = 0;
            var tokudai = 0;
            var bonus = 0.0;
            var level = 0;
            var sum = 0;
            foreach (var ship in GetShipStatuses(fleet))
            {
                if (ship.Name == "鬼怒改二")
                    bonus += 0.05;
                foreach (var item in ship.Slot)
                {
                    switch (item.Spec.Name)
                    {
                        case "大発動艇":
                            level += item.Level;
                            sum++;
                            daihatsu++;
                            bonus += 0.05;
                            break;
                        case "特大発動艇":
                            level += item.Level;
                            sum++;
                            tokudai++;
                            bonus += 0.05;
                            break;
                        case "大発動艇(八九式中戦車&陸戦隊)":
                            level += item.Level;
                            sum++;
                            bonus += 0.02;
                            break;
                        case "特二式内火艇":
                            level += item.Level;
                            sum++;
                            bonus += 0.01;
                            break;
                    }
                }
            }
            var levelAverage = sum == 0 ? 0.0 : (double)level / sum;
            bonus = Min(bonus, 0.2);
            return bonus + 0.01 * bonus * levelAverage + tokudaiBonus[Min(tokudai, 4), Min(daihatsu, 4)];
        }

        public double GetTransportPoint(int fleet)
        {
            return GetShipStatuses(fleet).Sum(ship => ship.TransportPoint);
        }

        public string[] BadlyDamagedShips { get; private set; } = new string[0];

        public void SetBadlyDamagedShips()
        {
            BadlyDamagedShips =
                _inSortie.SelectMany((flag, i) => !flag
                        ? new ShipStatus[0]
                        : _combinedFleetType > 0 && i == 1
                            ? GetShipStatuses(1).Skip(1) // 連合艦隊第二の旗艦を飛ばす
                            : GetShipStatuses(i))
                    .Where(s => !s.Escaped && s.DamageLevel == ShipStatus.Damage.Badly)
                    .Select(s => s.Name)
                    .ToArray();
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

        public bool UseOldEnemyId
        {
            set => _shipMaster.UseOldEnemyId = value;
        }

        public void InjectShips(dynamic battle, dynamic item)
        {
            var deck = (int)battle.api_deck_id - 1;
            InjectShips(deck, (int[])battle.api_f_nowhps, (int[])battle.api_f_maxhps, (int[][])item[0]);
            if (battle.api_f_nowhps_combined())
                InjectShips(1, (int[])battle.api_f_nowhps_combined, (int[])battle.api_f_maxhps_combined, (int[][])item[1]);
            foreach (var enemy in (int[])battle.api_ship_ke)
                _shipMaster[enemy] = new ShipSpec {Id = enemy};
            if (battle.api_ship_ke_combined())
            {
                foreach (var enemy in (int[])battle.api_ship_ke_combined)
                    _shipMaster[enemy] = new ShipSpec {Id = enemy};
            }
        }

        private void InjectShips(int deck, int[] nowhps, int[] maxhps, int[][] slots)
        {
            var id = _shipInfo.Keys.Count + 1;
            var ships = nowhps.Zip(maxhps,
                (now, max) => new ShipStatus {Id = id++, NowHp = now, MaxHp = max}).ToArray();
            _decks[deck] = (from ship in ships select ship.Id).ToArray();
            foreach (var ship in ships)
                _shipInfo[ship.Id] = ship;
            foreach (var entry in ships.Zip(slots, (ship, slot) =>new {ship, slot}))
            {
                entry.ship.Slot = _itemInfo.InjectItems(entry.slot.Take(5));
                if (entry.slot.Length >= 6)
                    entry.ship.SlotEx = _itemInfo.InjectItems(entry.slot.Skip(5)).First();
            }
        }
    }
}