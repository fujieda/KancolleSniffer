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
        public int ShipId { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int ExpToNext { get; set; }
        public int MaxHp { get; set; }
        public int NowHp { get; set; }
        public int Cond { get; set; }
        public int Fuel { get; set; }
        public int Bull { get; set; }
        public int[] OnSlot { get; set; }
        public int[] Slot { get; set; }

        public int DamageLevel
        {
            get
            {
                var ratio = MaxHp == 0 ? 1 : (double)NowHp / MaxHp;
                return ratio > 0.75 ? 0 : ratio > 0.5 ? 1 : ratio > 0.25 ? 2 : 3;
            }
        }
    }

    public struct ChargeStatus
    {
        public int Fuel { get; set; }
        public int Bull { get; set; }
    }

    public class ShipInfo
    {
        public const int FleetCount = 4;
        public const int MemberCount = 6;

        private readonly int[][] _decks = new int[FleetCount][];
        private readonly Dictionary<int, ShipStatus> _shipInfo = new Dictionary<int, ShipStatus>();

        private readonly DateTime[][] _recoveryTimes =
        {
            new DateTime[3], new DateTime[3], new DateTime[3],
            new DateTime[3]
        };

        private readonly ShipMaster _shipMaster;
        private readonly ItemInfo _itemInfo;

        public ShipInfo(ShipMaster shipMaster, ItemInfo itemInfo)
        {
            _shipMaster = shipMaster;
            _itemInfo = itemInfo;

            for (var fleet = 0; fleet < FleetCount; fleet++)
            {
                var deck = new int[MemberCount];
                for (var i = 0; i < deck.Length; i++)
                    deck[i] = -1;
                _decks[fleet] = deck;
            }
        }

        public DateTime[] GetRecoveryTimes(int fleet)
        {
            return _recoveryTimes[fleet];
        }

        public void InspectShip(dynamic json)
        {
            if (json.api_deck_port()) // port
            {
                InspectShipData(json.api_ship);
                InspectDeck(json.api_deck_port);
                _itemInfo.NowShips = ((object[])json.api_ship).Length;
            }
            else if (json.api_data()) // ship2
            {
                InspectShipData(json.api_data);
                InspectDeck(json.api_data_deck);
                _itemInfo.NowShips = ((object[])json.api_data).Length;
            }
            else if (json.api_ship_data()) // ship3
            {
                // 一隻分のデータしか来ないことがあるので艦娘数を数えない
                InspectShipData(json.api_ship_data);
                InspectDeck(json.api_deck_data);
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
                    ShipId = (int)entry.api_ship_id,
                    Name = _shipMaster[(int)entry.api_ship_id].Name,
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
            }
            SetRecoveryTime();
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
                RemoveShip(fleet, idx);
                return;
            }
            for (var f = 0; f < _decks.Length; f++)
                for (var i = 0; i < _decks[f].Length; i++)
                    if (_decks[f][i] == ship) // 入れ替えの場合
                    {
                        if ((_decks[f][i] = _decks[fleet][idx]) == -1)
                            RemoveShip(f, i);
                        goto last;
                    }
            last:
            _decks[fleet][idx] = ship;
        }

        private void RemoveShip(int fleet, int idx)
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
            _itemInfo.NowItems -= (from s in ships select SlotItemCount(int.Parse(s))).Sum();
            InspectDeck(json.api_deck);
            InspectShip(json.api_ship);
        }

        public void InspectDestroyShip(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var id = int.Parse(values["api_ship_id"]);
            _itemInfo.NowShips -= 1;
            _itemInfo.NowItems -= SlotItemCount(id);
            foreach (var deck in _decks)
            {
                for (var i = 0; i < deck.Length; i++)
                    if (deck[i] == id)
                    {
                        for (var j = i; j < deck.Length - 1; j++)
                            deck[j] = deck[j + 1];
                        deck[deck.Length - 1] = -1;
                    }
            }
        }

        private int SlotItemCount(int id)
        {
            return _shipInfo[id].Slot.Count(item => item != -1);
        }

        public void InspectNyukyo(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var id = int.Parse(values["api_ship_id"]);
            if (int.Parse(values["api_highspeed"]) == 0)
                return;
            var ship = _shipInfo[id];
            ship.NowHp = ship.MaxHp;
            if (ship.Cond < 40)
                ship.Cond = 40;
            _itemInfo.NumBuckets--;
        }

        private void SetRecoveryTime()
        {
            for (var fleet = 0; fleet < 4; fleet++)
            {
                var cond =
                    (from id in _decks[fleet] where _shipInfo.ContainsKey(id) select _shipInfo[id].Cond)
                        .DefaultIfEmpty(49).Min();
                if (cond < 49 && _recoveryTimes[fleet][2] != DateTime.MinValue) // 計時中
                {
                    // コンディション値から推定される残り時刻と経過時間の差
                    var diff = TimeSpan.FromMinutes((49 - cond + 2) / 3 * 3) - (_recoveryTimes[fleet][2] - DateTime.Now);
                    if (diff >= TimeSpan.Zero && diff <= TimeSpan.FromMinutes(3)) // 差が0以上3分以内ならタイマーを更新しない。
                        return;
                }
                var thresh = new[] {30, 40, 49};
                for (var i = 0; i < thresh.Length; i++)
                    _recoveryTimes[fleet][i] = cond < thresh[i]
                        ? DateTime.Now.AddMinutes((thresh[i] - cond + 2) / 3 * 3)
                        : DateTime.MinValue;
            }
        }

        public ShipStatus[] GetShipStatuses(int fleet)
        {
            return _decks[fleet].Select(id => (id == -1) ? new ShipStatus {Name = ""} : _shipInfo[id]).ToArray();
        }

        public int[] GetDeck(int fleet)
        {
            return _decks[fleet];
        }

        public ShipStatus this[int idx]
        {
            get { return _shipInfo[idx]; }
        }

        public ChargeStatus[] ChargeStatuses
        {
            get
            {
                return (from deck in _decks
                    select (from id in deck
                        where id != -1
                        let status = _shipInfo[id]
                        let spec = _shipMaster[status.ShipId]
                        select new {status.Bull, status.Fuel, spec.BullMax, spec.FuelMax})
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
                where _shipInfo.ContainsKey(id)
                let ship = _shipInfo[id]
                from slot in ship.Slot.Zip(ship.OnSlot, (s, o) => new {slot = s, onslot = o})
                select (int)Math.Floor(_itemInfo[slot.slot].TyKu * Math.Sqrt(slot.onslot))).Sum();
        }
    }
}