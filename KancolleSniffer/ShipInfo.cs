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

namespace KancolleSniffer
{
    public struct ShipStatus
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
    }

    public struct ChargeStatus
    {
        public int Fuel { get; set; }
        public int Bull { get; set; }
    }

    public class ShipInfo
    {
        private readonly int[][] _decks = new int[4][];
        private readonly Dictionary<int, ShipStatus> _shipInfo = new Dictionary<int, ShipStatus>();
        private readonly DateTime[] _recoveryTimes = new DateTime[3];
        private readonly ShipMaster _shipMaster;

        public string FleetName { get; set; }

        public ShipInfo(ShipMaster shipMaster)
        {
            _shipMaster = shipMaster;

            for (var i = 0; i < _decks.Length; i++)
                _decks[i] = new[] {-1, -1, -1, -1, -1, -1};
        }

        public DateTime[] RecoveryTimes
        {
            get { return _recoveryTimes; }
        }

        public string GetNameById(int id)
        {
            ShipStatus ship;
            return _shipInfo.TryGetValue(id, out ship) ? _shipMaster.GetSpec(ship.ShipId).Name : "";
        }

        public void InspectDeck(dynamic json)
        {
            foreach (var entry in json)
            {
                var fleet = (int)entry.api_id;
                if (fleet == 1)
                    FleetName = (string)entry.api_name;
                var deck = _decks[fleet - 1];
                for (var i = 0; i < deck.Length; i++)
                    deck[i] = (int)entry.api_ship[i];
            }
        }

        public int NumShips
        {
            get { return _shipInfo.Count; }
        }

        public void InspectShipInfo(dynamic json)
        {
            _shipInfo.Clear();
            foreach (var entry in json)
            {
                _shipInfo[(int)entry.api_id] = new ShipStatus
                {
                    ShipId = (int)entry.api_ship_id,
                    Level = (int)entry.api_lv,
                    ExpToNext = (int)entry.api_exp[1],
                    MaxHp = (int)entry.api_maxhp,
                    NowHp = (int)entry.api_nowhp,
                    Cond = (int)entry.api_cond,
                    Fuel = (int)entry.api_fuel,
                    Bull = (int)entry.api_bull
                };
            }
            SetRecoveryTime();
        }

        private void SetRecoveryTime()
        {
            var cond =
                (from id in _decks[0] where _shipInfo.ContainsKey(id) select _shipInfo[id].Cond).DefaultIfEmpty(49)
                    .Min();
            if (cond < 49 && _recoveryTimes[2] != DateTime.MinValue) // 計時中
            {
                // コンディション値から推定される残り時刻と経過時間の差
                var diff = TimeSpan.FromMinutes((49 - cond + 2) / 3 * 3) - (_recoveryTimes[2] - DateTime.Now);
                if (diff >= TimeSpan.Zero && diff <= TimeSpan.FromMinutes(3)) // 差が0以上3分以内ならタイマーを更新しない。
                    return;
            }
            var thresh = new[] {30, 40, 49};
            for (var i = 0; i < thresh.Length; i++)
                _recoveryTimes[i] = cond < thresh[i]
                    ? DateTime.Now.AddMinutes((thresh[i] - cond + 2) / 3 * 3)
                    : DateTime.MinValue;
        }

        public ShipStatus[] ShipStatuses
        {
            get
            {
                return _decks[0].Select(id =>
                {
                    ShipStatus status;
                    if (_shipInfo.TryGetValue(id, out status))
                        status.Name = _shipMaster.GetSpec(status.ShipId).Name;
                    return status;
                }).ToArray();
            }
        }

        public ChargeStatus[] ChargeStatuses
        {
            get
            {
                return _decks.Select(deck =>
                {
                    var result = new ChargeStatus();
                    foreach (var id in deck)
                    {
                        ShipStatus status;
                        if (!_shipInfo.TryGetValue(id, out status))
                            continue;
                        var spec = _shipMaster.GetSpec(status.ShipId);
                        result.Fuel = Math.Max(CalcChargeState(status.Fuel, spec.FuelMax), result.Fuel);
                        result.Bull = Math.Max(CalcChargeState(status.Bull, spec.BullMax), result.Bull);
                    }
                    return result;
                }).ToArray();
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
    }
}