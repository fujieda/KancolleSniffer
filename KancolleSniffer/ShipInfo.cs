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
using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace KancolleSniffer
{
    public class ShipStatus : ICloneable
    {
        public int Id { get; set; }
        public int Fleet { get; set; }
        public ShipSpec Spec { get; set; }

        public string Name => Spec.Name;

        public int Level { get; set; }
        public int ExpToNext { get; set; }
        public int MaxHp { get; set; }
        public int NowHp { get; set; }
        public int Cond { get; set; }
        public int Fuel { get; set; }
        public int Bull { get; set; }
        public int[] OnSlot { get; set; }
        public ItemStatus[] Slot { get; set; }
        public ItemStatus SlotEx { get; set; }
        public int LoS { get; set; }
        public int Firepower { get; set; }
        public int Torpedo { get; set; }
        public int AntiSubmarine { get; set; }
        public int Lucky { get; set; }
        public bool Locked { get; set; }
        public bool Escaped { get; set; }

        public Damage DamageLevel => CalcDamage(NowHp, MaxHp);

        public int CombinedFleetType { get; set; }

        public ShipStatus()
        {
            Id = -1;
            Fleet = -1;
            Spec = new ShipSpec();
            OnSlot = new int[0];
            Slot = new ItemStatus[0];
            SlotEx = new ItemStatus();
        }

        public enum Damage
        {
            Minor,
            Small,
            Half,
            Badly
        }

        public static Damage CalcDamage(int now, int max)
        {
            var ratio = max == 0 ? 1 : (double)now / max;
            return ratio > 0.75 ? Damage.Minor : ratio > 0.5 ? Damage.Small : ratio > 0.25 ? Damage.Half : Damage.Badly;
        }

        public TimeSpan RepairTime => TimeSpan.FromSeconds(CalcRepairSec(MaxHp - NowHp) + 30);

        public int CalcRepairSec(int damage) => (int)(RepairSecPerHp * damage);

        public double RepairSecPerHp
        {
            get
            {
                var weight = Spec.RepairWeight;
                var level = Level < 12 ? Level * 10 : Level * 5 + Floor(Sqrt(Level - 11)) * 10 + 50;
                return level * weight;
            }
        }

        public void CalcMaterialsToRepair(out int fuel, out int steal)
        {
            var damage = MaxHp - NowHp;
            fuel = (int)(Spec.FuelMax * 0.2 * 0.16 * damage);
            steal = (int)(Spec.FuelMax * 0.2 * 0.3 * damage);
        }

        public double RealFirepower
        {
            get
            {
                if (Spec.IsSubmarine)
                    return 0;
                var isRyuseiAttack = Spec.Id == 352 && // 速吸改
                                     Slot.Any(item => item.Spec.Type == 8); // 艦攻装備
                var levelBonus = Slot.Sum(item => item.FirePowerLevelBonus);
                if (!Spec.IsAircraftCarrier && !isRyuseiAttack)
                    return Firepower + levelBonus + CombinedFleetFirepowerBonus + 5;
                var specs = (from item in Slot where item.Spec.IsAircraft select item.Spec).ToArray();
                var torpedo = specs.Sum(s => s.Torpedo);
                var bomber = specs.Sum(s => s.Bomber);
                if (torpedo == 0 && bomber == 0)
                    return 0;
                return (int)((Firepower + torpedo + levelBonus + (int)(bomber * 1.3) + CombinedFleetFirepowerBonus) * 1.5) + 55;
            }
        }

        private int CombinedFleetFirepowerBonus
        {
            get
            {
                switch (CombinedFleetType)
                {
                    case 0:
                        return 0;
                    case 1: // 機動
                        return Fleet == 0 ? 2 : 10;
                    case 2: // 水上
                        return Fleet == 0 ? 10 : -5;
                    case 3: // 輸送
                        return Fleet == 0 ? -5 : 10;
                    default:
                        return 0;
                }
            }
        }

        public double RealTorpedo
        {
            get
            {
                if (Spec.IsAircraftCarrier || Torpedo == 0)
                    return 0;
                return Torpedo + Slot.Sum(item => item.TorpedoLevelBonus) + CombinedFleetTorpedoPenalty + 5;
            }
        }

        private int CombinedFleetTorpedoPenalty => CombinedFleetType > 0 && Fleet == 1 ? -5 : 0;

        public double RealAntiSubmarine
        {
            get
            {
                if (!Spec.IsAntiSubmarine)
                    return 0;
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Spec.IsAircraftCarrier && RealFirepower == 0) // 砲撃戦に参加しない
                    return 0;
                var sonar = false;
                var dc = false;
                var aircraft = false;
                var all = 0.0;
                var vanilla = AntiSubmarine;
                foreach (var spec in Slot.Select(item => item.Spec))
                {
                    vanilla -= spec.AntiSubmarine;
                    if (spec.IsSonar)
                        sonar = true;
                    else if (spec.IsDepthCharge)
                        dc = true;
                    else if (spec.IsAircraft)
                        aircraft = true;
                    all += spec.RealAntiSubmarine;
                }
                if (vanilla == 0 && !aircraft) // 素対潜0で航空機なしは対潜攻撃なし
                    return 0;
                var bonus = sonar && dc ? 1.15 : 1.0;
                var levelBonus = Slot.Sum(item => item.AntiSubmarineLevelBonus);
                return bonus * (Sqrt(vanilla) * 2 + all * 1.5 + levelBonus + (aircraft ? 8 : 13));
            }
        }

        public double NightBattlePower
        {
            get
            {
                if (Spec.IsAircraftCarrier && Spec.Id != 353 && Spec.Id != 432) // Graf Zeppelin以外の空母
                    return 0;
                return Firepower + Torpedo + Slot.Sum(item => item.NightBattleLevelBonus);
            }
        }

        public int PreparedDamageControl =>
            (DamageLevel < Damage.Badly)
                ? -1
                : SlotEx.Spec.Id == 42 || SlotEx.Spec.Id == 43
                    ? SlotEx.Spec.Id
                    : Slot.FirstOrDefault(item => item.Spec.Id == 42 || item.Spec.Id == 43)?.Spec.Id ?? -1;

        public double TransportPoint
            => Spec.TransportPoint + Slot.Sum(item => item.Spec.TransportPoint) + SlotEx.Spec.TransportPoint;

        public object Clone()
        {
            var r = (ShipStatus)MemberwiseClone();
            r.Slot = r.Slot.ToArray(); // 戦闘中のダメコンの消費が見えないように複製する
            return r;
        }
    }

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

        private readonly int[][] _decks = new int[FleetCount][];
        private readonly Dictionary<int, ShipStatus> _shipInfo = new Dictionary<int, ShipStatus>();
        private readonly ShipMaster _shipMaster = new ShipMaster();
        private readonly ItemInfo _itemInfo;
        private readonly bool[] _inMission = new bool[FleetCount];
        private readonly bool[] _inSortie = new bool[FleetCount];
        private int _hqLevel;
        private readonly List<int> _escapedShips = new List<int>();
        private int _combinedFleetType;
        private int[][] _presetDeck = new int[0][];

        public ShipInfo(ItemInfo itemInfo)
        {
            _itemInfo = itemInfo;

            for (var fleet = 0; fleet < FleetCount; fleet++)
            {
                var deck = new int[MemberCount];
                for (var i = 0; i < deck.Length; i++)
                    deck[i] = -1;
                _decks[fleet] = deck;
            }
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
            }
            else if (json.api_data()) // ship2
            {
                ClearShipInfo();
                InspectDeck(json.api_data_deck);
                InspectShipData(json.api_data);
                _itemInfo.NowShips = ((object[])json.api_data).Length;
            }
            else if (json.api_ship_data()) // ship3とship_deck
            {
                // 一隻分のデータしか来ないことがあるので艦娘数を数えない
                InspectDeck(json.api_deck_data);
                InspectShipData(json.api_ship_data);
            }
            else if (json.api_ship()) // getshipとpowerup
            {
                InspectShipData(new[] {json.api_ship});
            }
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
                var deck = _decks[fleet];
                for (var i = 0; i < deck.Length; i++)
                    deck[i] = (int)entry.api_ship[i];
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
                    SlotEx = entry.api_slot_ex() ? new ItemStatus((int)entry.api_slot_ex) : new ItemStatus(),
                    LoS = (int)entry.api_sakuteki[0],
                    Firepower = (int)entry.api_karyoku[0],
                    Torpedo = (int)entry.api_raisou[0],
                    AntiSubmarine = (int)entry.api_taisen[0],
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
            int oi;
            var of = FindFleet(ship, out oi);
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
            var ships = values["api_id_items"].Split(',');
            _itemInfo.NowShips -= ships.Length;
            _itemInfo.DeleteItems(ships.SelectMany(s => _shipInfo[int.Parse(s)].Slot).ToArray());
            foreach (var ship in ships)
                _shipInfo.Remove(int.Parse(ship));
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
            var ship = int.Parse(values["api_ship_id"]);
            _itemInfo.NowShips--;
            _itemInfo.DeleteItems(_shipInfo[ship].Slot);
            int oi;
            var of = FindFleet(ship, out oi);
            if (of != -1)
                WithdrowShip(of, oi);
            _shipInfo.Remove(ship);
        }

        public void InspectPresetDeck(dynamic json)
        {
            _presetDeck = new int[(int)json.api_max_num][];
            foreach (KeyValuePair<string, dynamic> entry in json.api_deck)
                InspectPresetRegister(entry.Value);
        }

        public void InspectPresetRegister(dynamic json)
        {
            var no = (int)json.api_preset_no - 1;
            _presetDeck[no] = json.api_ship;
        }

        public void InspectPresetDelete(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            _presetDeck[int.Parse(values["api_preset_no"]) - 1] = null;
        }

        public void InspectCombined(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            _combinedFleetType = int.Parse(values["api_combined_type"]);
        }

        public int[][] PresetDeck => _presetDeck;

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
            ShipStatus s;
            if (!_shipInfo.TryGetValue(id, out s))
                return new ShipStatus();
            s.Slot = s.Slot.Select(item => _itemInfo.GetStatus(item.Id)).ToArray();
            s.SlotEx = _itemInfo.GetStatus(s.SlotEx.Id);
            s.Escaped = _escapedShips.Contains(id);
            int idx;
            s.Fleet = FindFleet(s.Id, out idx);
            s.CombinedFleetType = s.Fleet < 2 ? _combinedFleetType : 0;
            return s;
        }

        public void SetItemHolder()
        {
            foreach (var ship in _shipInfo.Values)
                foreach (var item in ship.Slot)
                    _itemInfo.GetStatus(item.Id).Holder = ship;
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
                ship.Slot.Zip(ship.OnSlot, (slot, onslot) =>
                {
                    if (!slot.Spec.CanAirCombat || onslot == 0)
                        return new[] {0, 0};
                    var unskilled = (slot.Spec.AntiAir + slot.FighterPowerLevelBonus) * Sqrt(onslot);
                    return new[] {(int)(unskilled + slot.AlvBonus[0]), (int)(unskilled + slot.AlvBonus[1])};
                }))
                .Aggregate(new[] {0, 0}, (prev, fp) => new[] {prev[0] + fp[0], prev[1] + fp[1]});

        public double GetContactTriggerRate(int fleet)
            => GetShipStatuses(fleet).Where(ship => !ship.Escaped).SelectMany(ship =>
                ship.Slot.Zip(ship.OnSlot, (slot, onslot) =>
                    slot.Spec.ContactTriggerRate * slot.Spec.LoS * Sqrt(onslot))).Sum();

        public ShipStatus[] GetRepairList(DockInfo dockInfo)
            => (from s in ShipList
                where s.NowHp < s.MaxHp && !dockInfo.InNDock(s.Id)
                select s).OrderByDescending(s => s.RepairTime).ToArray();


        public double GetLineOfSights(int fleet)
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
                    result += (spec.LoS + item.LoSLevelBonus) * spec.LoSScaleFactor;
                }
                result += Sqrt(s.LoS - itemLoS);
            }
            return result > 0 ? result - Ceiling(_hqLevel * 0.4) + emptyBonus * 2 : 0.0;
        }

        public string[] BadlyDamagedShips { get; private set; } = new string[0];

        public void SetBadlyDamagedShips()
        {
            BadlyDamagedShips =
                _inSortie.SelectMany((sortie, i) => sortie ? GetShipStatuses(i) : new ShipStatus[0])
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
    }
}