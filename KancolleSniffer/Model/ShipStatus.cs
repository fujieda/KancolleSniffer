// Copyright (C) 2017 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
    public class ShipStatus : ICloneable
    {
        public int Id { get; set; }
        public bool Empty => Id == -1;
        public Fleet Fleet { get; set; }
        public int DeckIndex { get; set; }
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
        public bool SpecialAttackTriggered { get; set; }

        public Damage DamageLevel => CalcDamage(NowHp, MaxHp);

        private IList<ItemStatus> _slot;
        private ItemStatus _slotEx;
        public Func<ItemStatus, ItemStatus> GetItem { get; set; } = item => item;

        public IReadOnlyList<ItemStatus> Slot
        {
            get => _slot.Select(item => GetItem(item)).ToArray();
            set => _slot = value.ToArray();
        }

        public ItemStatus SlotEx
        {
            get => GetItem(_slotEx);
            set => _slotEx = value;
        }

        public void FreeSlot(int idx) => _slot[idx] = new ItemStatus();

        public IEnumerable<ItemStatus> AllSlot => SlotEx.Id == 0 ? Slot : Slot.Concat(new[] {SlotEx});

        public ShipStatus()
        {
            Id = -1;
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

        public void RepairShip()
        {
            NowHp = MaxHp;
            Cond = Max(40, Cond);
        }

        public static Damage CalcDamage(int now, int max)
        {
            if (now == 0 && max > 0)
                return Damage.Sunk;
            var ratio = max == 0 ? 1 : (double)now / max;
            return ratio > 0.75 ? Damage.Minor :
                ratio > 0.5 ? Damage.Small :
                ratio > 0.25 ? Damage.Half : Damage.Badly;
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
                var levelBonus = AllSlot.Sum(item => item.FirepowerLevelBonus);
                if (!Spec.IsAircraftCarrier && !isRyuseiAttack)
                    return Firepower + levelBonus + Fleet.CombinedFirepowerBonus + 5;
                var specs = (from item in Slot where item.Spec.IsAircraft select item.Spec).ToArray();
                var torpedo = specs.Sum(s => s.Torpedo);
                var bomber = specs.Sum(s => s.Bomber);
                if (torpedo == 0 && bomber == 0)
                    return 0;
                return (int)((Firepower + torpedo + levelBonus +
                              (int)(bomber * 1.3) + Fleet.CombinedFirepowerBonus) * 1.5) + 55;
            }
        }

        public double EffectiveTorpedo
        {
            get
            {
                if (Spec.IsAircraftCarrier || Torpedo == 0)
                    return 0;
                return Torpedo + AllSlot.Sum(item => item.TorpedoLevelBonus) + Fleet.CombinedTorpedoPenalty + 5;
            }
        }

        public double EffectiveAntiSubmarine
        {
            get
            {
                if (!Spec.IsAntiSubmarine)
                    return 0;
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Spec.IsAircraftCarrier && EffectiveFirepower == 0 && !CanOpeningAntiSubmarineAttack)
                    return 0;
                var sonar = false;
                var dct = false;
                var dc = false;
                var special = false;
                var aircraft = false;
                var all = 0.0;
                foreach (var spec in Slot.Select(item => item.Spec))
                {
                    if (spec.IsSonar)
                    {
                        sonar = true;
                    }
                    else if (spec.IsDCT)
                    {
                        dct = true;
                    }
                    else if (spec.IsDC)
                    {
                        dc = true;
                    }
                    else if (spec.IsSpecialDCT)
                    {
                        special = true;
                    }
                    else if (spec.IsAircraft)
                    {
                        aircraft = true;
                    }
                    all += spec.EffectiveAntiSubmarine;
                }
                var vanilla = ShipAntiSubmarine;
                if (vanilla == 0 && !aircraft) // 素対潜0で航空機なしは対潜攻撃なし
                    return 0;
                var bonus = 1.0;
                if (dct && dc)
                    bonus = 1.1;
                if (sonar && (dct || dc || special))
                    bonus = 1.15;
                if (sonar && dct && dc)
                    bonus = 1.15 * 1.25;
                var levelBonus = Slot.Sum(item => item.AntiSubmarineLevelBonus);
                return bonus * (Sqrt(vanilla) * 2 + all * 1.5 + levelBonus + (aircraft ? 8 : 13));
            }
        }

        public int ShipAntiSubmarine => AntiSubmarine - Slot.Sum(item => item.Spec.AntiSubmarine);

        public bool CanOpeningAntiSubmarineAttack
        {
            get
            {
                switch (Name)
                {
                    case "五十鈴改二":
                    case "龍田改二":
                    case "Jervis改":
                    case "Samuel B.Roberts改":
                        return true;
                    case "大鷹改":
                    case "大鷹改二":
                        return Slot.Any(item => item.Spec.IsAircraft && item.Spec.EffectiveAntiSubmarine > 0);
                    case "大鷹":
                    case "Gambier Bay":
                    case "Gambier Bay改":
                    case "瑞鳳改二乙":
                        return Slot.Any(item => item.Spec.IsAircraft && item.Spec.EffectiveAntiSubmarine >= 7) &&
                               AntiSubmarine >= (HaveSonar ? 50 : 65);
                    default:
                        return Spec.ShipType == 1
                            ? Slot.Sum(item => item.Spec.AntiSubmarine) >= 4 && AntiSubmarine >= 75 ||
                              HaveSonar && AntiSubmarine >= 60
                            : HaveSonar && AntiSubmarine >= 100;
                }
            }
        }

        public int MissionAntiSubmarine => AntiSubmarine - AllSlot.Sum(item =>
        {
            switch (item.Spec.Type)
            {
                case 10: // 水偵
                case 11: // 水爆
                case 41: // 大艇
                    return item.Spec.AntiSubmarine;
                default:
                    return 0;
            }
        });

        private bool HaveSonar => Slot.Any(item => item.Spec.IsSonar);

        public double NightBattlePower
        {
            get
            {
                if (!Spec.IsAircraftCarrier)
                    return Firepower + Torpedo + Slot.Sum(item => item.NightBattleLevelBonus);

                if (Slot.Any(item => item.Spec.IconType == 45 || item.Spec.IconType == 46) && // 夜戦か夜攻
                    (Spec.Id == 545 || // Saratoga Mk.II
                     Slot.Any(item => item.Spec.Id == 258 || item.Spec.Id == 259))) // 夜間作戦航空要員
                {
                    return Firepower + Slot.Zip(OnSlot, (item, onSlot) =>
                    {
                        double a, b;
                        var spec = item.Spec;
                        switch (spec.Id)
                        {
                            case 154: // 零戦62型(爆戦/岩井隊)
                            case 242: // Swordfish
                            case 243: // Swordfish Mk.II(熟練)
                            case 244: // Swordfish Mk.III(熟練)
                                a = 0.0;
                                b = 0.3;
                                break;
                            case 254: // F6F-3N
                            case 255: // F6F-5N
                            case 257: // TBD-3D
                                a = 3.0;
                                b = 0.45;
                                break;
                            default:
                                return -spec.Firepower;
                        }
                        return spec.Torpedo + a * onSlot +
                               b * (spec.Firepower + spec.Torpedo + spec.Bomber + spec.AntiSubmarine) *
                               Sqrt(onSlot) + Sqrt(item.Level);
                    }).Sum();
                }
                switch (Spec.Id)
                {
                    case 353: // Graf Zeppelin改
                    case 432: // Graf Zeppelin
                    case 433: // Saratoga
                        break;
                    case 393: // Ark Royal改
                    case 515: // Ark Royal
                        if (Slot.Any(item => new[] {242, 243, 244}.Contains(item.Spec.Id)))
                            break;
                        return 0;
                    default:
                        return 0;
                }
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
                if (AllSlot.All(item => item.Empty || item.Unimplemented))
                    return AntiAir;
                var vanilla = AntiAir - AllSlot.Sum(item => item.Spec.AntiAir);
                var x = vanilla + AllSlot.Sum(item => item.EffectiveAntiAirForShip);
                return (int)(x / 2) * 2;
            }
        }

        public int EffectiveAntiAirForFleet => (int)AllSlot.Sum(item => item.EffectiveAntiAirForFleet);

        public int EffectiveFuelMax => Max((int)(Spec.FuelMax * (Level >= 100 ? 0.85 : 1.0)), 1);

        public int EffectiveBullMax => Max((int)(Spec.BullMax * (Level >= 100 ? 0.85 : 1.0)), 1);

        public object Clone()
        {
            var r = (ShipStatus)MemberwiseClone();
            r.Slot = r.Slot.ToArray(); // 戦闘中のダメコンの消費が見えないように複製する
            return r;
        }
    }
}