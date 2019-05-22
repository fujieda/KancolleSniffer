// Copyright (C) 2019 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KancolleSniffer.Model;

namespace KancolleSniffer.View
{
    public class FleetSpec
    {
        private static readonly Font Font = new Control().Font;

        public static Record[] Create(Sniffer sniffer)
        {
            var list = new List<Record>();
            foreach (var fleet in sniffer.Fleets)
            {
                var shipRecords = new List<Record>();
                foreach (var ship in fleet.ActualShips)
                {
                    shipRecords.Add(Record.CreateShipRecord(ship));
                    shipRecords.AddRange(GetItemRecords(ship));
                }
                list.Add(Record.CreateFleetRecord(sniffer.Fleets, fleet.Number));
                list.AddRange(shipRecords);
            }
            if (sniffer.AirBase == null)
                return list.ToArray();
            foreach (var baseInfo in sniffer.AirBase)
            {
                list.Add(new Record {Fleet = baseInfo.AreaName + " 基地航空隊"});
                var i = 0;
                foreach (var airCorps in baseInfo.AirCorps)
                {
                    if (i >= 3)
                        break;
                    list.Add(Record.CreateAirCorpsRecord(airCorps, i++));
                    list.AddRange(airCorps.Planes.Select(Record.CreateCorpsPlaneRecord));
                }
            }
            return list.ToArray();
        }

        private static IEnumerable<Record> GetItemRecords(ShipStatus ship)
        {
            var items = ship.Slot.TakeWhile(item => !item.Empty)
                .Select((item, i) => Record.CreateItemRecord(item, ship.OnSlot[i], ship.Spec.MaxEq[i]));
            if (ship.SlotEx.Id <= 0)
                return items;
            return items.Concat(new[] {Record.CreateItemRecord(ship.SlotEx, 0, 0)});
        }

        public class Record
        {
            public string Fleet { get; set; }
            public string Fleet2 { get; set; }
            public string Ship { get; set; }
            public string Ship2 { get; set; }
            public int Id { get; set; }
            public string Equip { get; set; }
            public Color Color { get; set; }
            public string Spec { get; set; }
            public string Spec2 { get; set; }
            public string AircraftSpec { get; set; }

            public Record()
            {
                Color = Control.DefaultBackColor;
                Fleet = Ship = Equip = AircraftSpec = "";
            }

            public static Record CreateShipRecord(ShipStatus ship)
            {
                return new ShipRecord(ship);
            }

            private class ShipRecord : Record
            {
                public ShipRecord(ShipStatus ship)
                {
                    Ship = (ship.Escaped ? "[避]" : "") + ship.Name + " Lv" + ship.Level;
                    Ship2 = $"燃{ship.EffectiveFuelMax} 弾{ship.EffectiveBullMax}";
                    Id = ship.Id;
                    SetAttackPower(ship);
                }

                private void SetAttackPower(ShipStatus ship)
                {
                    Spec = HideIfZero("砲", ship.EffectiveFirepower) +
                           HideIfZero(" 潜", ship.EffectiveAntiSubmarine) +
                           (ship.CanOpeningAntiSubmarineAttack ? "*" : "");
                    Spec2 = (HideIfZero("雷", ship.EffectiveTorpedo) +
                             HideIfZero(" 夜", ship.NightBattlePower)).TrimStart(' ');
                    if (Spec != "")
                        return;
                    Spec = Spec2;
                    Spec2 = "";
                }
            }

            public static Record CreateItemRecord(ItemStatus item, int onSlot, int maxEq)
            {
                return new ItemRecord(item, onSlot, maxEq);
            }

            private class ItemRecord : Record
            {
                public ItemRecord(ItemStatus item, int onSlot, int maxEq)
                {
                    Equip = GenEquipString(item);
                    Spec = item.Spec.IsAircraft ? $"+{item.Alv} {onSlot}/{maxEq}" : "";
                    AircraftSpec = GetAircraftSpec(item, onSlot);
                    Color = item.Spec.Color;
                }

                private static string GetAircraftSpec(ItemStatus item, int onSlot)
                {
                    if (item.Spec.IsDiveBomber) // 爆撃
                    {
                        return "航空戦 " +
                               (25 + (int)((item.Spec.Bomber + item.BomberLevelBonus) * Math.Sqrt(onSlot)));
                    }
                    if (item.Spec.IsTorpedoBomber)
                    {
                        var normal = 25 + item.Spec.Torpedo * Math.Sqrt(onSlot);
                        return "航空戦 " + (int)(normal * 0.8) + "/" + (int)(normal * 1.5);
                    }
                    return "";
                }
            }

            private static string GenEquipString(ItemStatus item)
            {
                var name = item.Spec.Name;
                var attr = item.Level == 0 ? "" : "★" + item.Level;
                var proposed = new Size(int.MaxValue, int.MaxValue);
                var maxWidth = item.Spec.IsAircraft ? 132 : 180;
                var result = name + attr;
                if (TextRenderer.MeasureText(result, Font, proposed).Width <= maxWidth)
                    return result;
                var truncated = "";
                foreach (var ch in name)
                {
                    var tmp = truncated + ch;
                    if (TextRenderer.MeasureText(tmp + attr, Font, proposed).Width > maxWidth)
                        break;
                    truncated = tmp;
                }
                return truncated + attr;
            }

            public static Record CreateFleetRecord(IReadOnlyList<Fleet> fleets, int number)
            {
                return new FleetRecord(fleets, number);
            }

            private class FleetRecord : Record
            {
                public FleetRecord(IReadOnlyList<Fleet> fleets, int number)
                {
                    var fleet = fleets[number];
                    Fleet = new[] {"第一", "第二", "第三", "第四"}[number] + " " + SpeedName(fleet);
                    Fleet2 = GetSpec(fleet) + GetTp(fleets, number);
                }

                private static string SpeedName(Fleet fleet)
                {
                    var speed = fleet.ActualShips.Min(s => (int?)s.Speed);
                    return new[] {"", "低速", "高速", "高速+", "最速"}[(speed ?? 0) / 5];
                }

                private static string GetTp(IReadOnlyList<Fleet> fleets, int number)
                {
                    var tp = fleets[number].TransportPoint;
                    if (fleets[0].CombinedType != CombinedType.None)
                    {
                        if (number == 0)
                            tp += fleets[1].TransportPoint;
                        else if (number == 1)
                            return "";
                    }
                    return $"\r\nTP:S{(int)tp} A{(int)(tp * 0.7)}";
                }

                private static string GetSpec(Fleet fleet)
                {
                    var total = fleet.Ships.Aggregate(new Total(), (sum, next) => sum.Add(next));
                    return "計:" + HideIfZero(" Lv", total.Level) +
                           HideIfZero(" ド", total.Drum) + HideIfZero("(", total.DrumShips, "隻)") +
                           HideIfZero(" 大", fleet.DaihatsuBonus * 100, "%") + "\r\n" +
                           "　 火" + CutOverFlow(total.FirePower) +
                           " 空" + CutOverFlow(total.AntiAir) +
                           " 潜" + CutOverFlow(total.AntiSubmarine) +
                           " 索" + CutOverFlow(total.LoS) + "\r\n" +
                           $"戦闘:燃{total.Fuel / 5}弾{total.Bull / 5} 支援:燃{total.Fuel / 2}弾{(int)(total.Bull * 0.8)}";
                }

                private static int CutOverFlow(int value) => value > 999 ? 999 : value;

                private class Total
                {
                    public int Drum;
                    public int DrumShips;
                    public int Level;
                    public int FirePower;
                    public int AntiSubmarine;
                    public int AntiAir;
                    public int LoS;
                    public int Fuel;
                    public int Bull;

                    public Total Add(ShipStatus s)
                    {
                        var drum = s.Slot.Count(item => item.Spec.IsDrum);
                        return new Total
                        {
                            DrumShips = DrumShips + (drum != 0 ? 1 : 0),
                            Drum = Drum + drum,
                            Level = Level + s.Level,
                            FirePower = FirePower + s.Firepower,
                            AntiSubmarine = AntiSubmarine + s.MissionAntiSubmarine,
                            AntiAir = AntiAir + s.AntiAir,
                            LoS = LoS + s.LoS,
                            Fuel = Fuel + s.EffectiveFuelMax,
                            Bull = Bull + s.EffectiveBullMax
                        };
                    }
                }
            }

            private static string HideIfZero(string name, double value, string suffix = "")
            {
                return value > 0 ? name + value.ToString("f1") + suffix : "";
            }

            private static string HideIfZero(string name, int value, string suffix = "")
            {
                return value > 0 ? name + value + suffix : "";
            }

            public static Record CreateAirCorpsRecord(AirBase.AirCorpsInfo airCorps, int number)
            {
                return new AirCorpsRecord(airCorps, number);
            }

            private class AirCorpsRecord : Record
            {
                public AirCorpsRecord(AirBase.AirCorpsInfo airCorps, int number)
                {
                    var corpsFp = new AirCorpsFp(airCorps.CalcFighterPower());
                    string spec;
                    string spec2;
                    if (airCorps.Action == 2)
                    {
                        spec = "制空:" + RangeString(corpsFp.Interception);
                        spec2 = corpsFp.Difference ? "制空(出撃):" + RangeString(corpsFp.AirCombat) : "";
                    }
                    else
                    {
                        spec = "制空:" + RangeString(corpsFp.AirCombat);
                        spec2 = corpsFp.Difference ? "制空(防空):" + RangeString(corpsFp.Interception) : "";
                    }
                    var cost = airCorps.CostForSortie;
                    Ship = new[] {"第一", "第二", "第三"}[number] + " " + airCorps.ActionName;
                    Ship2 = $"出撃コスト:燃{cost[0]}弾{cost[1]}";
                    Spec = spec + $" 距離:{airCorps.Distance}";
                    Spec2 = spec2;
                }
            }

            public static Record CreateCorpsPlaneRecord(AirBase.PlaneInfo plane)
            {
                return new CorpsPlaneRecord(plane);
            }

            private class CorpsPlaneRecord : Record
            {
                public CorpsPlaneRecord(AirBase.PlaneInfo plane)
                {
                    var planeFp = new AirCorpsFp(plane.FighterPower);
                    Equip = plane.State != 1 ? plane.StateName : GenEquipString(plane.Slot);
                    Spec = plane.State != 1 ? "" : $"+{plane.Slot.Alv} {plane.Count}/{plane.MaxCount}";
                    AircraftSpec =
                        $"距離:{plane.Slot.Spec.Distance} 制空:{RangeString(planeFp.AirCombat)}" +
                        (planeFp.Difference ? $" 防空:{RangeString(planeFp.Interception)}" : "");
                    Color = plane.Slot.Spec.Color;
                }
            }

            private class AirCorpsFp
            {
                public readonly int[] AirCombat;
                public readonly int[] Interception;
                public readonly bool Difference;

                public AirCorpsFp(IReadOnlyList<AirBaseParams> fighterPower)
                {
                    AirCombat = new[] {(int)fighterPower[0].AirCombat, (int)fighterPower[1].AirCombat};
                    Interception = new[] {(int)fighterPower[0].Interception, (int)fighterPower[1].Interception};
                    Difference = Interception[0] != AirCombat[0];
                }
            }

            private static string RangeString(int[] fp) => fp[0] == fp[1] ? fp[0].ToString() : $"{fp[0]}～{fp[1]}";
        }
    }
}