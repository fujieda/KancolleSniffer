﻿// Copyright (C) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
    public class FleetPanel : PanelWithToolTip
    {
        private const int LineHeight = 14;
        private const int LabelHeight = 12;
        private Record[] _table = new Record[0];
        private readonly List<FleetLabels> _labelList = new List<FleetLabels>();
        private readonly List<Panel> _panelList = new List<Panel>();

        public FleetPanel()
        {
            ToolTip.AutoPopDelay = 10000;
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
                Fleet = Ship = Equip = AircraftSpec = "";
                Color = DefaultBackColor;
            }
        }

        public void Update(Sniffer sniffer)
        {
            _table = CreateTable(sniffer);
            SuspendLayout();
            CreateLabels();
            SetRecords();
            ResumeLayout();
        }

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

            public void Add(ShipStatus s)
            {
                var drum = s.Slot.Count(item => item.Spec.IsDrum);
                DrumShips += drum != 0 ? 1 : 0;
                Drum += drum;
                Level += s.Level;
                FirePower += s.Firepower;
                AntiSubmarine += s.MissionAntiSubmarine;
                AntiAir += s.AntiAir;
                LoS += s.LoS;
                Fuel += s.EffectiveFuelMax;
                Bull += s.EffectiveBullMax;
            }
        }

        public Record[] CreateTable(Sniffer sniffer)
        {
            var list = new List<Record>();
            var fn = new[] {"第一", "第二", "第三", "第四"};
            foreach (var fleet in sniffer.Fleets)
            {
                var total = new Total();
                var shipRecords = new List<Record>();
                var speed = int.MaxValue;
                foreach (var ship in fleet.ActualShips)
                {
                    var equips = new List<Record>();
                    for (var i = 0; i < ship.Slot.Count; i++)
                    {
                        var item = ship.Slot[i];
                        var onSlot = ship.OnSlot[i];
                        var max = ship.Spec.MaxEq[i];
                        if (item.Empty)
                            continue;
                        var airSpec = "";
                        if (item.Spec.IsDiveBomber) // 爆撃
                        {
                            airSpec = "航空戦 " +
                                      (25 + (int)((item.Spec.Bomber + item.BomberLevelBonus) * Math.Sqrt(onSlot)));
                        }
                        else if (item.Spec.IsTorpedoBomber)
                        {
                            var normal = 25 + item.Spec.Torpedo * Math.Sqrt(onSlot);
                            airSpec = "航空戦 " + (int)(normal * 0.8) + "/" + (int)(normal * 1.5);
                        }
                        equips.Add(new Record
                        {
                            Equip = GenEquipString(item),
                            Spec = item.Spec.IsAircraft ? $"+{item.Alv} {onSlot}/{max}" : "",
                            AircraftSpec = airSpec,
                            Color = item.Spec.Color
                        });
                    }
                    if (ship.SlotEx.Id > 0)
                    {
                        var item = ship.SlotEx;
                        equips.Add(new Record {Equip = GenEquipString(item), Color = item.Spec.Color});
                    }
                    total.Add(ship);
                    speed = Math.Min(speed, ship.Speed);
                    var fire = ship.EffectiveFirepower;
                    // ReSharper disable IdentifierTypo
                    var subm = ship.EffectiveAntiSubmarine;
                    var torp = ship.EffectiveTorpedo;
                    var night = ship.NightBattlePower;
                    var oasa = ship.CanOpeningAntiSubmarineAttack ? "*" : "";
                    // ReSharper restore IdentifierTypo
                    var record = new Record
                    {
                        Ship = (ship.Escaped ? "[避]" : "") + ship.Name + " Lv" + ship.Level,
                        Ship2 = $"燃{ship.EffectiveFuelMax} 弾{ship.EffectiveBullMax}",
                        Id = ship.Id,
                        Spec = HideIfZero("砲", fire) + HideIfZero(" 潜", subm) + oasa,
                        Spec2 = (HideIfZero("雷", torp) + HideIfZero(" 夜", night)).TrimStart(' ')
                    };
                    if (record.Spec == "")
                    {
                        record.Spec = record.Spec2;
                        record.Spec2 = "";
                    }
                    shipRecords.Add(record);
                    shipRecords.AddRange(equips);
                }
                var daihatsu = fleet.DaihatsuBonus;
                var tp = fleet.TransportPoint;
                if (sniffer.IsCombinedFleet && fleet.Number == 0)
                    tp += sniffer.Fleets[1].TransportPoint;
                var speedName = speed == int.MaxValue ? "" : new[] {"", "低速", "高速", "高速+", "最速"}[speed / 5];
                list.Add(new Record
                {
                    Fleet = fn[fleet.Number] + " " + speedName,
                    Fleet2 =
                        "計:" + HideIfZero(" Lv", total.Level) +
                        HideIfZero(" ド", total.Drum) + HideIfZero("(", total.DrumShips, "隻)") +
                        HideIfZero(" 大", daihatsu * 100, "%") + "\r\n" +
                        "　 火" + CutOverFlow(total.FirePower) +
                        " 空" + CutOverFlow(total.AntiAir) +
                        " 潜" + CutOverFlow(total.AntiSubmarine) +
                        " 索" + CutOverFlow(total.LoS) + "\r\n" +
                        $"戦闘:燃{total.Fuel / 5}弾{total.Bull / 5} 支援:燃{total.Fuel / 2}弾{(int)(total.Bull * 0.8)}" +
                        (sniffer.IsCombinedFleet && fleet.Number == 1
                            ? ""
                            : $"\r\nTP:S{(int)tp} A{(int)(tp * 0.7)}")
                });
                list.AddRange(shipRecords);
            }
            if (sniffer.AirBase != null)
            {
                var name = new[] {"第一", "第二", "第三"};
                foreach (var baseInfo in sniffer.AirBase)
                {
                    list.Add(new Record {Fleet = baseInfo.AreaName + " 基地航空隊"});
                    var i = 0;
                    foreach (var airCorps in baseInfo.AirCorps)
                    {
                        if (i >= name.Length)
                            break;
                        var corpsFp = airCorps.CalcFighterPower();
                        var airCombat = new[] { (int)corpsFp[0].AirCombat, (int)corpsFp[1].AirCombat };
                        var interception = new[] { (int)corpsFp[0].Interception, (int)corpsFp[1].Interception };
                        var different = interception[0] != airCombat[0];
                        string spec;
                        string spec2;
                        if (airCorps.Action == 2)
                        {
                            spec = "制空:" + RangeString(interception);
                            spec2 = different ? "制空(出撃):" + RangeString(airCombat) : "";
                        }
                        else
                        {
                            spec = "制空:" + RangeString(airCombat);
                            spec2 = different ? "制空(防空):" + RangeString(interception) : "";
                        }
                        var cost = airCorps.CostForSortie;
                        list.Add(new Record
                        {
                            Ship = name[i++] + " " + airCorps.ActionName,
                            Ship2 = $"出撃コスト:燃{cost[0]}弾{cost[1]}",
                            Spec = spec + $" 距離:{airCorps.Distance}",
                            Spec2 = spec2
                        });
                        list.AddRange(airCorps.Planes.Select(plane =>
                        {
                            var planeFp = plane.FighterPower;
                            airCombat = new[] {(int)planeFp[0].AirCombat, (int)planeFp[1].AirCombat};
                            interception = new[] {(int) planeFp[0].Interception, (int)planeFp[1].Interception };
                            different = interception[0] != airCombat[0];
                            return new Record
                            {
                                Equip = plane.State != 1 ? plane.StateName : GenEquipString(plane.Slot),
                                Spec = plane.State != 1 ? "" : $"+{plane.Slot.Alv} {plane.Count}/{plane.MaxCount}",
                                AircraftSpec =
                                    $"距離:{plane.Slot.Spec.Distance} 制空:{RangeString(airCombat)}" +
                                    (different ? $" 防空:{RangeString(interception)}" : ""),
                                Color = plane.Slot.Spec.Color
                            };
                        }));
                    }
                }
            }
            return list.ToArray();
        }

        private string RangeString(int[] fp) => fp[0] == fp[1] ? fp[0].ToString() : $"{fp[0]}～{fp[1]}";

        private int CutOverFlow(int value) => value > 999 ? 999 : value;

        private string HideIfZero(string name, double value, string suffix = "")
        {
            return value > 0 ? name + value.ToString("f1") + suffix : "";
        }

        private string HideIfZero(string name, int value, string suffix = "")
        {
            return value > 0 ? name + value + suffix : "";
        }

        private string GenEquipString(ItemStatus item)
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

        private void CreateLabels()
        {
            for (var i = _labelList.Count; i < _table.Length; i++)
                CreateLabels(i);
        }

        private class FleetLabels : ControlsArranger
        {
            public ShipLabel Fleet { get; set; }
            public ShipLabel Name { get; set; }
            public ShipLabel Equip { get; set; }
            public ShipLabel EquipColor { get; set; }
            public ShipLabel Spec { get; set; }

            public override Control[] Controls => new Control[] {Fleet, Name, Equip, EquipColor, Spec};
        }

        private void CreateLabels(int i)
        {
            var y = 1 + LineHeight * i;
            var lbp = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(ListForm.PanelWidth, LineHeight),
                BackColor = CustomColors.ColumnColors.BrightFirst(i),
                Visible = false
            };
            Scaler.Scale(lbp);
            lbp.Tag = lbp.Location.Y;
            var labels = new FleetLabels
            {
                Fleet = new ShipLabel {Location = new Point(1, 2), AutoSize = true},
                Name = new ShipLabel {Location = new Point(10, 2), AutoSize = true},
                Equip = new ShipLabel {Location = new Point(38, 2), AutoSize = true},
                EquipColor = new ShipLabel {Location = new Point(35, 2), Size = new Size(4, LabelHeight - 2)},
                Spec = new ShipLabel {Location = new Point(217, 2), GrowLeft = true}
            };
            _labelList.Add(labels);
            _panelList.Add(lbp);
            labels.Arrange(lbp, CustomColors.ColumnColors.BrightFirst(i));
            Controls.Add(lbp);
        }

        private void SetRecords()
        {
            for (var i = 0; i < _table.Length; i++)
                SetRecord(i);
            for (var i = _table.Length; i < _labelList.Count; i++)
                _panelList[i].Visible = false;
        }

        private void SetRecord(int i)
        {
            var lbp = _panelList[i];
            if (!lbp.Visible)
                lbp.Location = new Point(lbp.Left, (int)lbp.Tag + AutoScrollPosition.Y);
            var e = _table[i];
            var labels = _labelList[i];
            labels.Fleet.Text = e.Fleet;
            labels.Name.SetName(e.Ship);
            if (e.Ship2 != "")
                ToolTip.SetToolTip(labels.Name, e.Ship2);
            labels.Equip.Text = e.Equip;
            labels.EquipColor.Visible = e.Equip != "";
            labels.EquipColor.BackColor = e.Color;
            labels.Spec.Text = e.Spec;
            if (e.Fleet != "" && e.Fleet2 != "")
                ToolTip.SetToolTip(labels.Fleet, e.Fleet2);
            ToolTip.SetToolTip(labels.Equip, e.AircraftSpec != "" ? e.AircraftSpec : "");
            ToolTip.SetToolTip(labels.Spec, e.Spec2 != "" ? e.Spec2 : "");
            lbp.Visible = true;
        }

        public void ShowShip(int id)
        {
            var i = Array.FindIndex(_table, e => e.Id == id);
            if (i == -1)
                return;
            var y = Scaler.ScaleHeight(LineHeight * i);
            AutoScrollPosition = new Point(0, y);
        }

        public void ShowFleet(string fn)
        {
            var i = Array.FindIndex(_table, e => e.Fleet.StartsWith(fn));
            if (i == -1)
                return;
            var y = Scaler.ScaleHeight(LineHeight * i);
            AutoScrollPosition = new Point(0, y);
        }
    }
}