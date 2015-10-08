// Copyright (C) 2013, 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Linq;
using System.Web;
using static System.Math;

namespace KancolleSniffer
{
    public class ShipStatus
    {
        private readonly ItemInfo _itemInfo;
        public int Id { get; set; }
        public int Fleet { get; set; } // ShipListだけで使う
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
        public int[] Slot { get; set; }
        public int SlotEx { get; set; }
        public int LoS { get; set; }
        public int Firepower { get; set; }
        public int AntiSubmarine { get; set; }
        public bool Escaped { get; set; }

        public Damage DamageLevel => CalcDamage(NowHp, MaxHp);

        public ShipStatus(ItemInfo itemInfo = null)
        {
            _itemInfo = itemInfo;
            Id = -1;
            Spec = new ShipSpec();
            OnSlot = new int[0];
            Slot = new int[0];
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

        public int RealFirepower
        {
            get
            {
                if (Spec.IsSubmarine)
                    return 0;
                if (!Spec.IsAircraftCarrier)
                    return Firepower + 5;
                var specs = (from id in Slot
                    let spec = _itemInfo.ItemDict[id].Spec
                    where spec.IsAircraft
                    select new {torpedo = spec.Torpedo, bomber = spec.Bomber}).ToArray();
                var torpedo = specs.Sum(s => s.torpedo);
                var bomber = specs.Sum(s => s.bomber);
                if (torpedo == 0 && bomber == 0)
                    return 0;
                return (int)((Firepower + torpedo) * 1.5 + bomber * 2 + 55);
            }
        }

        public int RealAntiSubmarine
        {
            get
            {
                if (!Spec.IsAntiSubmarine)
                    return 0;
                if (Spec.IsAircraftCarrier && RealFirepower == 0) // 砲撃戦に参加しない
                    return 0;
                var sonar = 0;
                var dc = 0;
                var aircraft = 0;
                var all = 0;
                var vanilla = AntiSubmarine;
                foreach (var spec in Slot.Select(id => _itemInfo.ItemDict[id].Spec))
                {
                    vanilla -= spec.AntiSubmarine;
                    if (spec.IsReconSeaplane) // 水偵は除外
                        continue;
                    if (spec.IsSonar)
                        sonar += spec.AntiSubmarine;
                    else if (spec.IsDepthCharge)
                        dc += spec.AntiSubmarine;
                    else if (spec.IsAircraft)
                        aircraft += spec.AntiSubmarine;
                    all += spec.AntiSubmarine;
                }
                if (vanilla == 0 && aircraft == 0) // 素対潜0で航空機なしは対潜攻撃なし
                    return 0;
                var bonus = sonar > 0 && dc > 0 ? 1.15 : 1.0;
                return (int)(bonus * (vanilla / 5 + all * 2 + (aircraft > 0 ? 10 : 25)));
            }
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
                _combinedFleetType = json.api_combined_flag() ? (int)json.api_combined_flag : 0;
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
            else if (json.api_ship()) // getship
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
                _shipInfo[(int)entry.api_id] = new ShipStatus(_itemInfo)
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
                    Slot = (int[])entry.api_slot,
                    SlotEx = entry.api_slot_ex() ? (int)entry.api_slot_ex : 0,
                    LoS = (int)entry.api_sakuteki[0],
                    Firepower = (int)entry.api_karyoku[0],
                    AntiSubmarine = (int)entry.api_taisen[0]
                };
                _itemInfo.CountNewItems((int[])entry.api_slot);
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
            for (var i = idx; i < deck.Length - 1; i++)
                deck[i] = deck[i + 1];
            deck[deck.Length - 1] = -1;
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
            InspectShip(json.api_ship);
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

        public void StartSortie(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var fleet = int.Parse(values["api_deck_id"]) - 1;
            if (_combinedFleetType == 0)
            {
                _inSortie[fleet] = true;
            }
            else
            {
                _inSortie[0] = _inSortie[1] = true;
            }
        }

        public void RepairShip(int id)
        {
            var s = _shipInfo[id];
            s.NowHp = s.MaxHp;
            s.Cond = Max(40, s.Cond);
        }

        public ShipStatus[] GetShipStatuses(int fleet)
        {
            return _decks[fleet].Where(id => id != -1).Select(id =>
            {
                var s = _shipInfo[id];
                s.Escaped = _escapedShips.Contains(id);
                return s;
            }).ToArray();
        }

        public int[] GetDeck(int fleet) => _decks[fleet];

        public ShipStatus this[int idx] => _shipInfo[idx];

        public ShipSpec GetSpec(int id) => _shipMaster[id];

        public bool InMission(int fleet) => _inMission[fleet];

        public bool InSortie(int fleet) => _inSortie[fleet];

        public ShipStatus[] ShipList
            => _shipInfo.Values.Where(s => s.Level != 0).Select(s =>
            {
                int oi;
                var f = FindFleet(s.Id, out oi);
                s.Fleet = f;
                s.Escaped = _escapedShips.Contains(s.Id);
                return s;
            }).ToArray();

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

        private readonly Dictionary<int, int> _alvBonus = new Dictionary<int, int>
        {
            {6, 25}, // 艦戦
            {7, 3}, // 艦爆
            {8, 3}, // 艦攻
            {11, 9}  // 水爆
        };

        public int GetFighterPower(int fleet, bool withBonus)
            => GetShipStatuses(fleet).Where(s => !s.Escaped).SelectMany(ship =>
                ship.Slot.Zip(ship.OnSlot, (slot, onslot) =>
                {
                    var spec = _itemInfo[slot];
                    if (!spec.CanAirCombat)
                        return 0;
                    var item = _itemInfo.ItemDict[slot];
                    var bonus = 0;
                    if (onslot != 0 && item.Alv == 7 && withBonus)
                        _alvBonus.TryGetValue(spec.Type, out bonus);
                    return (int)Floor(spec.AntiAir * Sqrt(onslot)) + bonus;
                })).Sum();

        public ShipStatus[] GetDamagedShipList(DockInfo dockInfo)
            => (from s in ShipList
                where s.NowHp < s.MaxHp && !dockInfo.InNDock(s.Id)
                select s).OrderByDescending(s => s.RepairTime).ToArray();


        public double GetLineOfSights(int fleet)
        {
            var result = 0.0;
            foreach (var s in _decks[fleet].Select(id => _shipInfo[id]))
            {
                var items = 0;
                foreach (var spec in s.Slot.Select(t => _itemInfo[t]))
                {
                    items += spec.LoS;
                    result += spec.LoS * spec.LoSScaleFactor();
                }
                result += Sqrt(s.LoS - items) * 1.6841056;
            }
            return result > 0 ? result + (_hqLevel + 4) / 5 * 5 * -0.6142467 : 0.0;
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