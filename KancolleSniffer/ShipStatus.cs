﻿// Copyright (C) 2017 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
        public int NdockTime { get; set; }
        public int[] NdockItem { get; set; }
        public int LoS { get; set; }
        public int Firepower { get; set; }
        public int Torpedo { get; set; }
        public int AntiSubmarine { get; set; }
        public int AntiAir { get; set; }
        public int Lucky { get; set; }
        public bool Locked { get; set; }
        public bool Escaped { get; set; }

        public Damage DamageLevel => CalcDamage(NowHp, MaxHp);

        public int CombinedFleetType { get; set; }

        public IEnumerable<ItemStatus> AllSlot => SlotEx.Id == 0 ? Slot : Slot.Concat(new[] {SlotEx});

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
            Badly,
            Sunk
        }

        public static Damage CalcDamage(int now, int max)
        {
            if (now == 0 && max > 0)
                return Damage.Sunk;
            var ratio = max == 0 ? 1 : (double)now / max;
            return ratio > 0.75 ? Damage.Minor : ratio > 0.5 ? Damage.Small : ratio > 0.25 ? Damage.Half : Damage.Badly;
        }

        public TimeSpan RepairTime => TimeSpan.FromSeconds((int)(RepairTimePerHp.TotalSeconds * (MaxHp - NowHp)) + 30);

        public TimeSpan RepairTimePerHp =>
            TimeSpan.FromSeconds(Spec.RepairWeight *
                                 (Level < 12
                                     ? Level * 10
                                     : Level * 5 + Floor(Sqrt(Level - 11)) * 10 + 50));

        public double EffectiveFirepower
        {
            get
            {
                if (Spec.IsSubmarine)
                    return 0;
                var isRyuseiAttack = Spec.Id == 352 && // 速吸改
                                     Slot.Any(item => item.Spec.Type == 8); // 艦攻装備
                var levelBonus = AllSlot.Sum(item => item.FirePowerLevelBonus);
                if (!Spec.IsAircraftCarrier && !isRyuseiAttack)
                    return Firepower + levelBonus + CombinedFleetFirepowerBonus + 5;
                var specs = (from item in Slot where item.Spec.IsAircraft select item.Spec).ToArray();
                var torpedo = specs.Sum(s => s.Torpedo);
                var bomber = specs.Sum(s => s.Bomber);
                if (torpedo == 0 && bomber == 0)
                    return 0;
                return (int)((Firepower + torpedo + levelBonus +
                              (int)(bomber * 1.3) + CombinedFleetFirepowerBonus) * 1.5) + 55;
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

        public double EffectiveTorpedo
        {
            get
            {
                if (Spec.IsAircraftCarrier || Torpedo == 0)
                    return 0;
                return Torpedo + AllSlot.Sum(item => item.TorpedoLevelBonus) + CombinedFleetTorpedoPenalty + 5;
            }
        }

        private int CombinedFleetTorpedoPenalty => CombinedFleetType > 0 && Fleet == 1 ? -5 : 0;

        public double EffectiveAntiSubmarine
        {
            get
            {
                if (!Spec.IsAntiSubmarine)
                    return 0;
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Spec.IsAircraftCarrier && EffectiveFirepower == 0) // 砲撃戦に参加しない
                    return 0;
                var sonar = false;
                var projector = false;
                var depthCharge = false;
                var aircraft = false;
                var all = 0.0;
                var vanilla = AntiSubmarine;
                foreach (var spec in Slot.Select(item => item.Spec))
                {
                    vanilla -= spec.AntiSubmarine;
                    if (spec.IsSonar)
                    {
                        sonar = true;
                    }
                    else if (spec.IsDepthCharge)
                    {
                        if (spec.Name.EndsWith("投射機"))
                            projector = true;
                        if (spec.Name.EndsWith("爆雷"))
                            depthCharge = true;
                    }
                    else if (spec.IsAircraft)
                    {
                        aircraft = true;
                    }
                    all += spec.EffectiveAntiSubmarine;
                }
                if (vanilla == 0 && !aircraft) // 素対潜0で航空機なしは対潜攻撃なし
                    return 0;
                var bonus = 1.0;
                if (sonar && projector)
                    bonus = 1.15;
                if (sonar && depthCharge)
                    bonus = 1.1;
                if (projector && depthCharge)
                    bonus = 1.15;
                if (sonar && projector && depthCharge)
                    bonus = 1.15 * 1.25;
                var levelBonus = Slot.Sum(item => item.AntiSubmarineLevelBonus);
                return bonus * (Sqrt(vanilla) * 2 + all * 1.5 + levelBonus + (aircraft ? 8 : 13));
            }
        }

        public bool CanOpeningAntiSubmarineAttack =>
            Spec.Id == 141 || Slot.Any(item => item.Spec.IsSonar) &&
            (AntiSubmarine >= 100 || Spec.ShipType == 1 && AntiSubmarine >= 60);

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
            DamageLevel != Damage.Badly
                ? -1
                : SlotEx.Spec.Id == 42 || SlotEx.Spec.Id == 43
                    ? SlotEx.Spec.Id
                    : Slot.FirstOrDefault(item => item.Spec.Id == 42 || item.Spec.Id == 43)?.Spec.Id ?? -1;

        public double TransportPoint
            => Spec.TransportPoint + AllSlot.Sum(item => item.Spec.TransportPoint);

        public int EffectiveAntiAirForShip
        {
            get
            {
                if (AllSlot.All(item => item.Id == -1 || item.Id == 0))
                    return AntiAir;
                var vanilla = AntiAir - AllSlot.Sum(item => item.Spec.AntiAir);
                var x = vanilla + AllSlot.Sum(item => item.EffectiveAntiAirForShip);
                return (int)(x / 2) * 2;
            }
        }

        public int EffectiveAntiAirForFleet => (int)AllSlot.Sum(item => item.EffectiveAntiAirForFleet);

        public object Clone()
        {
            var r = (ShipStatus)MemberwiseClone();
            r.Slot = r.Slot.ToArray(); // 戦闘中のダメコンの消費が見えないように複製する
            return r;
        }
    }
}