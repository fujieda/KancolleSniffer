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

        public Damage DamageLevel
        {
            get { return CalcDamage(NowHp, MaxHp); }
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

        public TimeSpan RepairTime()
        {
            return RepairTime(MaxHp - NowHp);
        }

        public TimeSpan RepairTime(int damage)
        {
            var weight = Spec.RepairWeight;
            var level = Level < 12 ? Level * 10 : Level * 5 + Math.Floor(Math.Sqrt(Level - 11)) * 10 + 50;
            return TimeSpan.FromSeconds(Math.Floor(level * weight * damage) + 30);
        }
    }

    public struct ChargeStatus
    {
        public int Fuel { get; set; }
        public int Bull { get; set; }
    }

    public class DamageStatus
    {
        public int Fleet { private set; get; }
        public string Name { private set; get; }
        public ShipStatus.Damage DamageLevel { private set; get; }
        public TimeSpan RepairTime { private set; get; }

        public DamageStatus(int fleet, string name, ShipStatus.Damage damage, TimeSpan time)
        {
            Fleet = fleet;
            Name = name;
            DamageLevel = damage;
            RepairTime = time;
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
                _shipInfo.Clear();
                InspectDeck(json.api_deck_port);
                InspectShipData(json.api_ship);
                _itemInfo.NowShips = ((object[])json.api_ship).Length;
            }
            else if (json.api_data()) // ship2
            {
                _shipInfo.Clear();
                InspectDeck(json.api_data_deck);
                InspectShipData(json.api_data);
                _itemInfo.NowShips = ((object[])json.api_data).Length;
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

        private void InspectDeck(dynamic json)
        {
            foreach (var entry in json)
            {
                var fleet = (int)entry.api_id - 1;
                var deck = _decks[fleet];
                for (var i = 0; i < deck.Length; i++)
                    deck[i] = (int)entry.api_ship[i];
            }
        }

        private void InspectShipData(dynamic json)
        {
            foreach (var entry in json)
            {
                _shipInfo[(int)entry.api_id] = new ShipStatus
                {
                    Spec = _shipMaster[(int)entry.api_ship_id],
                    Level = (int)entry.api_lv,
                    ExpToNext = (int)entry.api_exp[1],
                    MaxHp = (int)entry.api_maxhp,
                    NowHp = (int)entry.api_nowhp,
                    Cond = (int)entry.api_cond,
                    Fuel = (int)entry.api_fuel,
                    Bull = (int)entry.api_bull,
                    OnSlot = (from num in (dynamic[])entry.api_onslot select (int)num).ToArray(),
                    Slot = (from num in (dynamic[])entry.api_slot select (int)num).ToArray()
                };
                _shipInfo[-1] = new ShipStatus {Spec = _shipMaster[-1]};
            }
            _conditionTimer.SetTimer();
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
            if (of != -1)
            {
                // 入れ替えの場合
                if ((_decks[of][oi] = _decks[fleet][idx]) == -1)
                    WithdrowShip(of, oi);
                if (of != fleet)
                    _conditionTimer.Invalidate(of);
            }
            _decks[fleet][idx] = ship;
            _conditionTimer.Invalidate(fleet);
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
            _itemInfo.NowItems -= (from s in ships select SlotItemCount(int.Parse(s))).Sum();
            foreach (var ship in ships)
                _shipInfo.Remove(int.Parse(ship));
            InspectDeck(json.api_deck);
            InspectShip(json.api_ship);
        }

        public void InspectDestroyShip(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            var ship = int.Parse(values["api_ship_id"]);
            _itemInfo.NowShips -= 1;
            _itemInfo.NowItems -= SlotItemCount(ship);
            int oi;
            var of = FindFleet(ship, out oi);
            if (of != -1)
                WithdrowShip(of, oi);
            _shipInfo.Remove(ship);

            var material = (int[])json.api_material;
            for (var i = 0; i < material.Length; i++)
                _itemInfo.MaterialHistory[i].Now = material[i];
        }

        private int SlotItemCount(int id)
        {
            return _shipInfo[id].Slot.Count(item => item != -1);
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
            return (from id in _decks[fleet] where id != -1 select _shipInfo[id]).ToArray();
        }

        public int[] GetDeck(int fleet)
        {
            return _decks[fleet];
        }

        public ShipStatus this[int idx]
        {
            get { return _shipInfo[idx]; }
        }

        public string[] GetConditionTimers(int fleet)
        {
            return _conditionTimer.GetTimerStrings(fleet);
        }

        public ChargeStatus[] ChargeStatuses
        {
            get
            {
                return (from deck in _decks
                    select (from id in deck
                        where id != -1
                        let status = _shipInfo[id]
                        select new {status.Bull, status.Fuel, status.Spec.BullMax, status.Spec.FuelMax})
                        .Aggregate(
                            new ChargeStatus(), (result, next) => new ChargeStatus
                            {
                                Bull = Math.Max(result.Bull, CalcChargeState(next.Bull, next.BullMax)),
                                Fuel = Math.Max(result.Fuel, CalcChargeState(next.Fuel, next.FuelMax))
                            })).ToArray();
            }
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

        public int GetAirSuperiority(int fleet)
        {
            return (from id in _decks[fleet]
                where id != -1
                let ship = _shipInfo[id]
                from slot in ship.Slot.Zip(ship.OnSlot, (s, o) => new {slot = s, onslot = o})
                select (int)Math.Floor(_itemInfo[slot.slot].TyKu * Math.Sqrt(slot.onslot))).Sum();
        }

        public DamageStatus[] GetDamagedShipList(DockInfo dockInfo)
        {
            int oi;
            return (from entry in _shipInfo
                let s = entry.Value
                where s.NowHp < s.MaxHp && !dockInfo.InNDock(entry.Key)
                select new DamageStatus(FindFleet(entry.Key, out oi), s.Name, s.DamageLevel, s.RepairTime())).
                OrderByDescending(entry => entry.RepairTime).ToArray();
        }
    }
}