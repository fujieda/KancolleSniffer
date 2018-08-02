// Copyright (C) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KancolleSniffer.Model;

namespace KancolleSniffer.View
{
    public class FleetPanel : Panel
    {
        private const int LineHeight = 14;
        private const int LabelHeight = 12;
        private Record[] _table;
        private readonly List<FleetLabels> _labelList = new List<FleetLabels>();
        private readonly List<Panel> _panelList = new List<Panel>();
        private readonly ResizableToolTip _toolTip = new ResizableToolTip {ShowAlways = true, AutoPopDelay = 10000};

        private class Record
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
            CreateTable(sniffer);
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
                var drum = s.Slot.Count(item => item.Spec.Name == "ドラム缶(輸送用)");
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

        private void CreateTable(Sniffer sniffer)
        {
            var list = new List<Record>();
            var fn = new[] {"第一", "第二", "第三", "第四"};
            foreach (var fleet in sniffer.Fleets)
            {
                var total = new Total();
                var shipRecords = new List<Record>();
                foreach (var ship in fleet.ActualShips)
                {
                    var equips = new List<Record>();
                    for (var i = 0; i < ship.Slot.Count; i++)
                    {
                        var item = ship.Slot[i];
                        var onslot = ship.OnSlot[i];
                        var max = ship.Spec.MaxEq[i];
                        if (item.Empty)
                            continue;
                        var airspec = "";
                        if (item.Spec.IsDiveBomber) // 爆撃
                        {
                            airspec = "航空戦 " +
                                      (25 + (int)((item.Spec.Bomber + item.BomberLevelBonus) * Math.Sqrt(onslot)));
                        }
                        else if (item.Spec.IsTorpedoBomber)
                        {
                            var normal = 25 + item.Spec.Torpedo * Math.Sqrt(onslot);
                            airspec = "航空戦 " + (int)(normal * 0.8) + "/" + (int)(normal * 1.5);
                        }
                        equips.Add(new Record
                        {
                            Equip = GenEquipString(item),
                            Spec = item.Spec.IsAircraft ? $"+{item.Alv} {onslot}/{max}" : "",
                            AircraftSpec = airspec,
                            Color = item.Spec.Color
                        });
                    }
                    if (ship.SlotEx.Id > 0)
                    {
                        var item = ship.SlotEx;
                        equips.Add(new Record {Equip = GenEquipString(item), Color = item.Spec.Color});
                    }
                    total.Add(ship);
                    var fire = ship.EffectiveFirepower;
                    var subm = ship.EffectiveAntiSubmarine;
                    var torp = ship.EffectiveTorpedo;
                    var night = ship.NightBattlePower;
                    var oasa = ship.CanOpeningAntiSubmarineAttack ? "*" : "";
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
                list.Add(new Record
                {
                    Fleet = fn[fleet.Number] + HideIfZero(" Lv", total.Level) +
                            HideIfZero(" ドラム缶", total.Drum) + HideIfZero("(", total.DrumShips, "隻)") +
                            HideIfZero(" 大発", daihatsu * 100, "%"),
                    Fleet2 = "計:" +
                             "火" + CutOverFlow(total.FirePower) +
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
            if (sniffer.BaseAirCorps != null)
            {
                var name = new[] {"第一", "第二", "第三"};
                foreach (var baseInfo in sniffer.BaseAirCorps)
                {
                    list.Add(new Record {Fleet = baseInfo.AreaName + " 基地航空隊"});
                    var i = 0;
                    foreach (var airCorps in baseInfo.AirCorps)
                    {
                        if (i >= name.Length)
                            break;
                        var corpsFp = airCorps.FighterPower;
                        string spec;
                        string spec2;
                        if (airCorps.Action == 2)
                        {
                            spec = "制空:" + RangeString(corpsFp.Interception);
                            spec2 = corpsFp.IsInterceptor ? "制空(出撃):" + RangeString(corpsFp.AirCombat) : "";
                        }
                        else
                        {
                            spec = "制空:" + RangeString(corpsFp.AirCombat);
                            spec2 = corpsFp.IsInterceptor ? "制空(防空):" + RangeString(corpsFp.Interception) : "";
                        }
                        var cost = airCorps.CostForSortie;
                        list.Add(new Record
                        {
                            Ship = name[i++] + " " + airCorps.ActionName,
                            Ship2 = $"出撃コスト:燃{cost[0]}弾{cost[1]}",
                            Spec = spec + " 距離:" + airCorps.Distance,
                            Spec2 = spec2
                        });
                        list.AddRange(airCorps.Planes.Select(plane =>
                        {
                            var planeFp = plane.FighterPower;
                            return new Record
                            {
                                Equip = plane.State != 1 ? plane.StateName : GenEquipString(plane.Slot),
                                Spec = plane.State != 1 ? "" : $"+{plane.Slot.Alv} {plane.Count}/{plane.MaxCount}",
                                AircraftSpec =
                                    $"距離:{plane.Slot.Spec.Distance} 制空:{RangeString(planeFp.AirCombat)}" +
                                    (planeFp.IsInterceptor ? $" 防空:{RangeString(planeFp.Interception)}" : ""),
                                Color = plane.Slot.Spec.Color
                            };
                        }));
                    }
                }
            }
            _table = list.ToArray();
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

        private class FleetLabels : IEnumerable<ShipLabel>
        {
            public ShipLabel Fleet { get; set; }
            public ShipLabel Name { get; set; }
            public ShipLabel Equip { get; set; }
            public ShipLabel EquipColor { get; set; }
            public ShipLabel Spec { get; set; }

            public IEnumerator<ShipLabel> GetEnumerator() =>
                ((IEnumerable<ShipLabel>)new[] {Fleet, Name, Equip, EquipColor, Spec}).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private void CreateLabels(int i)
        {
            var y = 1 + LineHeight * i;
            var lbp = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(ListForm.PanelWidth, LineHeight),
                BackColor = ShipLabel.ColumnColors[(i + 1) % 2],
                Visible = false
            };
            lbp.Scale(ShipLabel.ScaleFactor);
            lbp.Tag = lbp.Location.Y;
            var labels = new FleetLabels
            {
                Fleet = new ShipLabel {Location = new Point(1, 2), AutoSize = true},
                Name = new ShipLabel {Location = new Point(10, 2), AutoSize = true},
                Equip = new ShipLabel {Location = new Point(38, 2), AutoSize = true},
                EquipColor = new ShipLabel {Location = new Point(35, 2), Size = new Size(4, LabelHeight - 2)},
                Spec = new ShipLabel {Location = new Point(217, 2), AutoSize = true, AnchorRight = true}
            };
            _labelList.Add(labels);
            _panelList.Add(lbp);
            lbp.Controls.AddRange(labels.Cast<Control>().ToArray());
            Controls.Add(lbp);
            foreach (var label in labels)
            {
                label.Scale();
                label.PresetColor =
                    label.BackColor = ShipLabel.ColumnColors[(i + 1) % 2];
            }
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
                _toolTip.SetToolTip(labels.Name, e.Ship2);
            labels.Equip.Text = e.Equip;
            labels.EquipColor.Visible = e.Equip != "";
            labels.EquipColor.BackColor = e.Color;
            labels.Spec.Text = e.Spec;
            if (e.Fleet != "" && e.Fleet2 != "")
                _toolTip.SetToolTip(labels.Fleet, e.Fleet2);
            _toolTip.SetToolTip(labels.Equip, e.AircraftSpec != "" ? e.AircraftSpec : "");
            _toolTip.SetToolTip(labels.Spec, e.Spec2 != "" ? e.Spec2 : "");
            lbp.Visible = true;
        }

        public void ShowShip(int id)
        {
            var i = Array.FindIndex(_table, e => e.Id == id);
            if (i == -1)
                return;
            var y = (int)Math.Round(ShipLabel.ScaleFactor.Height * LineHeight * i);
            AutoScrollPosition = new Point(0, y);
        }

        public void ShowFleet(string fn)
        {
            var i = Array.FindIndex(_table, e => e.Fleet.StartsWith(fn));
            if (i == -1)
                return;
            var y = (int)Math.Round(ShipLabel.ScaleFactor.Height * LineHeight * i);
            AutoScrollPosition = new Point(0, y);
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
            if (factor.Height > 1)
                _toolTip.Font = new Font(_toolTip.Font.FontFamily, _toolTip.Font.Size * factor.Height);
        }
    }
}