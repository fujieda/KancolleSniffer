// Copyright (C) 2018 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

namespace KancolleSniffer.Model
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

    public enum FleetState
    {
        Port,
        Mission,
        Sortie,
        Practice
    }

    public enum CombinedType
    {
        None,
        Carrier, // 機動
        Surface, // 水上
        Transport // 輸送
    }

    public class Fleet
    {
        private readonly ShipInventry _shipInventry;
        private readonly Func<int> _getHqLevel;
        private int[] _deck = Enumerable.Repeat(-1, ShipInfo.MemberCount).ToArray();
        public int Number { get; }
        public FleetState State { get; set; }
        public CombinedType CombinedType { get; set; }
        public IReadOnlyList<ShipStatus> Ships { get; private set; }
        public IReadOnlyList<ShipStatus> ActualShips { get; private set; }

        public Fleet(ShipInventry shipInventry, int number, Func<int> getHqLevel)
        {
            _shipInventry = shipInventry;
            Number = number;
            _getHqLevel = getHqLevel;
            Ships = _deck.Select(id => new ShipStatus()).ToArray();
            ActualShips = new ShipStatus[0];
        }

        public IReadOnlyList<int> Deck
        {
            get => _deck.ToArray();
            set
            {
                _deck = value.ToArray();
                SetDeck();
            }
        }

        public void SetDeck()
        {
            foreach (var ship in Ships)
            {
                if (ship.Fleet != this) // 入れ替え操作で他の艦隊に移動しているときには触らない。
                    continue;
                ship.Fleet = null;
                ship.DeckIndex = -1;
            }
            Ships = _deck.Select((id, num) =>
            {
                var ship = _shipInventry[id];
                if (ship.Empty)
                    return ship;
                ship.DeckIndex = num;
                ship.Fleet = this;
                return ship;
            }).ToArray();
            ActualShips = Ships.Where(ship => !ship.Empty).ToArray();
        }

        public int SetShip(int index, int shipId)
        {
            var prev = _deck[index];
            _deck[index] = shipId;
            SetDeck();
            return prev;
        }

        public void WithdrowShip(int index)
        {
            if (index == -1) // 旗艦以外解除
            {
                for (var i = 1; i < _deck.Length; i++)
                    _deck[i] = -1;
                SetDeck();
                return;
            }
            var dst = index;
            for (var src = index + 1; src < _deck.Length; src++)
            {
                if (_deck[src] != -1)
                    _deck[dst++] = _deck[src];
            }
            for (; dst < _deck.Length; dst++)
                _deck[dst] = -1;
            SetDeck();
        }

        public ChargeStatus ChargeStatus
        {
            get
            {
                var fs = new ChargeStatus(Ships[0]);
                var others = (from ship in Ships.Skip(1) select new ChargeStatus(ship)).Aggregate(
                    (result, next) => new ChargeStatus(Max(result.Fuel, next.Fuel), Max(result.Bull, next.Bull)));
                return new ChargeStatus(fs.Fuel != 0 ? fs.Fuel : others.Fuel + 5,
                    fs.Bull != 0 ? fs.Bull : others.Bull + 5);
            }
        }

        public int[] FighterPower
            => ActualShips.Where(ship => !ship.Escaped).SelectMany(ship =>
                    ship.Slot.Zip(ship.OnSlot, (slot, onslot) => slot.CalcFighterPower(onslot)))
                .Aggregate(new[] {0, 0}, (prev, cur) => new[] {prev[0] + cur[0], prev[1] + cur[1]});

        public double ContactTriggerRate
            => ActualShips.Where(ship => !ship.Escaped).SelectMany(ship =>
                ship.Slot.Zip(ship.OnSlot, (slot, onslot) =>
                    slot.Spec.ContactTriggerRate * slot.Spec.LoS * Sqrt(onslot))).Sum();

        public double GetLineOfSights(int factor)
        {
            var result = 0.0;
            var emptyBonus = 6;
            foreach (var s in ActualShips.Where(s => !s.Escaped))
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
            return result > 0 ? result - Ceiling(_getHqLevel() * 0.4) + emptyBonus * 2 : 0.0;
        }

        public double DaihatsuBonus
        {
            get
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
                foreach (var ship in Ships)
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
        }

        public double TransportPoint => Ships.Where(ship => !ship.Escaped).Sum(ship => ship.TransportPoint);

        public int CombinedFirepowerBonus
        {
            get
            {
                switch (CombinedType)
                {
                    case CombinedType.None:
                        return 0;
                    case CombinedType.Carrier:
                        return Number == 0 ? 2 : 10;
                    case CombinedType.Surface:
                        return Number == 0 ? 10 : -5;
                    case CombinedType.Transport:
                        return Number == 0 ? -5 : 10;
                    default:
                        return 0;
                }
            }
        }

        public int CombinedTorpedoPenalty => CombinedType != 0 && Number == 1 ? -5 : 0;
    }
}