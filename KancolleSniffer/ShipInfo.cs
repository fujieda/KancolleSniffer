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
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KancolleSniffer
{
    public class ShipStatus
    {
        public int Id { get; set; }
        public int Fleet { get; set; } // ShipListだけで使う
        public ShipSpec Spec { get; set; }

        public string Name
        {
            get { return Spec.Name; }
        }

        public int Level { get; set; }
        public int ExpToNext { get; set; }
        public int MaxHp { get; set; }
        public int NowHp { get; set; }
        public int Cond { get; set; }
        public int Fuel { get; set; }
        public int Bull { get; set; }
        public int[] OnSlot { get; set; }
        public int[] Slot { get; set; }
        public int LoS { get; set; }

        public Damage DamageLevel
        {
            get { return CalcDamage(NowHp, MaxHp); }
        }

        public ShipStatus()
        {
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

        public TimeSpan RepairTime
        {
            get { return CalcRepairTime(MaxHp - NowHp); }
        }

        public TimeSpan CalcRepairTime(int damage)
        {
            return TimeSpan.FromSeconds(RepairSecPerHp * damage + 30);
        }

        public double RepairSecPerHp
        {
            get
            {
                var weight = Spec.RepairWeight;
                var level = Level < 12 ? Level * 10 : Level * 5 + Math.Floor(Math.Sqrt(Level - 11)) * 10 + 50;
                return level * weight;
            }
        }

        public void CalcMaterialsToRepair(out int fuel, out int steal)
        {
            var damage = MaxHp - NowHp;
            fuel = (int)(Spec.FuelMax * 0.2 * 0.16 * damage);
            steal = (int)(Spec.FuelMax * 0.2 * 0.3 * damage);
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
        private readonly ConditionTimer _conditionTimer;
        private readonly ShipMaster _shipMaster;
        private readonly ItemInfo _itemInfo;
        private readonly bool[] _inMission = new bool[FleetCount];
        private readonly bool[] _inSortie = new bool[FleetCount];
        private int _hqLevel;
        private readonly List<int> _escapedShips = new List<int>();
        private int _combinedFleetType;

        public ShipInfo(ShipMaster shipMaster, ItemInfo itemInfo)
        {
            _shipMaster = shipMaster;
            _itemInfo = itemInfo;
            _conditionTimer = new ConditionTimer(this);

            for (var fleet = 0; fleet < FleetCount; fleet++)
            {
                var deck = new int[MemberCount];
                for (var i = 0; i < deck.Length; i++)
                    deck[i] = -1;
                _decks[fleet] = deck;
            }
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
                _conditionTimer.SetTimer();
            }
            else if (json.api_data()) // ship2
            {
                ClearShipInfo();
                InspectDeck(json.api_data_deck);
                InspectShipData(json.api_data);
                _itemInfo.NowShips = ((object[])json.api_data).Length;
                _conditionTimer.SetTimer();
            }
            else if (json.api_ship_data()) // ship3
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
                if (_inMission[fleet])
                    _conditionTimer.Disable(fleet);
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
                    Slot = (int[])entry.api_slot,
                    LoS = (int)entry.api_sakuteki[0]
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
            var material = (int[])json.api_material;
            for (var i = 0; i < material.Length; i++)
                _itemInfo.MaterialHistory[i].Now = material[i];
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
                _conditionTimer.Invalidate(fleet);
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
            _conditionTimer.Invalidate(fleet);
            if (of == -1)
                return;
            // 入れ替えの場合
            if ((_decks[of][oi] = orig) == -1)
                WithdrowShip(of, oi);
            if (of != fleet)
                _conditionTimer.Invalidate(of);
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
            _conditionTimer.Invalidate(fleet);
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

            var material = (int[])json.api_material;
            for (var i = 0; i < material.Length; i++)
                _itemInfo.MaterialHistory[i].Now = material[i];
        }

        public void StartSortie(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var fleet = int.Parse(values["api_deck_id"]) - 1;
            if (_combinedFleetType == 0)
            {
                _conditionTimer.Disable(fleet);
                _inSortie[fleet] = true;
            }
            else
            {
                _conditionTimer.Disable(0);
                _conditionTimer.Disable(1);
                _inSortie[0] = _inSortie[1] = true;
            }
        }

        public void RepairShip(int id)
        {
            var s = _shipInfo[id];
            s.NowHp = s.MaxHp;
            s.Cond = Math.Max(40, s.Cond);
            _conditionTimer.SetTimer();
        }

        public ShipStatus[] GetShipStatuses(int fleet)
        {
            return
                (from id in _decks[fleet] where id != -1
                    select _escapedShips.Contains(id) ? new ShipStatus() : _shipInfo[id]).ToArray();
        }

        public int[] GetDeck(int fleet)
        {
            return _decks[fleet];
        }

        public ShipStatus this[int idx]
        {
            get { return _shipInfo[idx]; }
        }

        public bool InMission(int fleet)
        {
            return _inMission[fleet];
        }

        public bool InSortie(int fleet)
        {
            return _inSortie[fleet];
        }

        public ShipStatus[] ShipList
        {
            get
            {
                return _shipInfo.Values.Where(s => s.Level != 0).Select(s =>
                {
                    int oi;
                    var f = FindFleet(s.Id, out oi);
                    s.Fleet = f;
                    return s;
                }).ToArray();
            }
        }

        public DateTime GetConditionTiemr(int fleet)
        {
            return _conditionTimer.GetTimer(fleet);
        }

        public int[] GetConditionNotice()
        {
            return _conditionTimer.GetNotice();
        }

        public ChargeStatus[] ChargeStatuses
        {
            get
            {
                return (from deck in _decks
                    let flag = new ChargeStatus(_shipInfo[deck[0]])
                    let others = (from id in deck.Skip(1)
                        select new ChargeStatus(_shipInfo[id]))
                        .Aggregate(
                            (result, next) =>
                                new ChargeStatus(Math.Max(result.Fuel, next.Fuel), Math.Max(result.Bull, next.Bull)))
                    select new ChargeStatus(flag.Fuel != 0 ? flag.Fuel : others.Fuel + 5,
                        flag.Bull != 0 ? flag.Bull : others.Bull + 5)).ToArray();
            }
        }

        public int GetAirSuperiority(int fleet)
        {
            return (from id in _decks[fleet]
                let ship = _shipInfo[id]
                from slot in ship.Slot.Zip(ship.OnSlot, (s, o) => new {slot = s, onslot = o})
                let item = _itemInfo[slot.slot]
                where item.CanAirCombat()
                select (int)Math.Floor(item.AntiAir * Math.Sqrt(slot.onslot))).DefaultIfEmpty().Sum();
        }

        public ShipStatus[] GetDamagedShipList(DockInfo dockInfo)
        {
            return (from s in ShipList
                where s.NowHp < s.MaxHp && !dockInfo.InNDock(s.Id)
                select s).OrderByDescending(s => s.RepairTime).ToArray();
        }

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
                result += Math.Sqrt(s.LoS - items) * 1.6841056;
            }
            return result > 0 ? result + (_hqLevel + 4) / 5 * 5 * -0.6142467 : 0.0;
        }

        public void SetEscapedShips(List<int> ships)
        {
            _escapedShips.Clear();
            _escapedShips.AddRange(ships);
        }

        public void ClearEscapedShips()
        {
            _escapedShips.Clear();
        }
    }
}