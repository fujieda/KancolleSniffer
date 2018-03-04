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

namespace KancolleSniffer
{
    public class FleetPanel : Panel
    {
        private const int LineHeight = 14;
        private const int LabelHeight = 12;
        private Record[] _table;
        private readonly List<FleetLabels> _labelList = new List<FleetLabels>();
        private readonly List<Panel> _panelList = new List<Panel>();
        private readonly ToolTip _toolTip = new ToolTip {ShowAlways = true};

        private class Record
        {
            public string Fleet { get; set; }
            public string Fleet2 { get; set; }
            public string Ship { get; set; }
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

        private void CreateTable(Sniffer sniffer)
        {
            var list = new List<Record>();
            var fn = new[] {"第一", "第二", "第三", "第四"};
            for (var f = 0; f < fn.Length; f++)
            {
                var drumTotal = 0;
                var drumShips = 0;
                var levelTotal = 0;
                var aswTotal = 0;
                var antiAirTotal = 0;
                var losTotal = 0;
                var ships = new List<Record>();
                foreach (var s in sniffer.GetShipStatuses(f))
                {
                    var drum = 0;
                    var equips = new List<Record>();
                    for (var i = 0; i < s.Slot.Length; i++)
                    {
                        var item = s.Slot[i];
                        var onslot = s.OnSlot[i];
                        var max = s.Spec.MaxEq[i];
                        if (item.Id == -1)
                            continue;
                        if (item.Spec.Name == "ドラム缶(輸送用)")
                            drum++;
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
                    if (s.SlotEx.Id > 0)
                    {
                        var item = s.SlotEx;
                        equips.Add(new Record {Equip = GenEquipString(item), Color = item.Spec.Color});
                    }
                    if (drum != 0)
                        drumShips++;
                    drumTotal += drum;
                    levelTotal += s.Level;
                    aswTotal += s.AntiSubmarine;
                    antiAirTotal += s.AntiAir;
                    losTotal += s.LoS;
                    var fire = s.EffectiveFirepower;
                    var subm = s.EffectiveAntiSubmarine;
                    var torp = s.EffectiveTorpedo;
                    var night = s.NightBattlePower;
                    var oasa = s.CanOpeningAntiSubmarineAttack ? "*" : "";
                    var ship = new Record
                    {
                        Ship = (s.Escaped ? "[避]" : "") + s.Name + " Lv" + s.Level,
                        Id = s.Id,
                        // ReSharper disable CompareOfFloatsByEqualityOperator
                        Spec = (fire == 0 ? "" : $"砲{fire:f1}") + (subm == 0 ? "" : $" 潜{subm:f1}{oasa}"),
                        Spec2 = (torp == 0 ? "" : $"雷{torp:f1}") + (night == 0 ? "" : $" 夜{night:f1}")
                        // ReSharper restore CompareOfFloatsByEqualityOperator
                    };
                    if (ship.Spec == "")
                    {
                        ship.Spec = ship.Spec2;
                        ship.Spec2 = "";
                    }
                    ships.Add(ship);
                    ships.AddRange(equips);
                }
                var daihatsu = sniffer.GetDaihatsuBonus(f);
                var tp = sniffer.GetTransportPoint(f);
                if (sniffer.CombinedFleetType != 0 && f == 0)
                    tp += sniffer.GetTransportPoint(1);
                list.Add(new Record
                {
                    Fleet = fn[f] + (levelTotal == 0 ? "" : " Lv" + levelTotal) +
                            (drumTotal == 0 ? "" : " 缶" + drumTotal + "(" + drumShips + "隻)") +
                            (aswTotal > 0 ? $" 潜{CutOverFlow(aswTotal)}" : "") +
                            (antiAirTotal > 0 ? $" 空{CutOverFlow(antiAirTotal)}" : "") +
                            (losTotal > 0 ? $" 索{CutOverFlow(losTotal)}" : ""),
                    Fleet2 = (sniffer.CombinedFleetType != 0 && f == 1 ? "" : $"TP:S{(int)tp}A{(int)(tp * 0.7)}") +
                             (daihatsu > 0 ? $" 発{daihatsu * 100:f1}%" : "")
                });
                list.AddRange(ships);
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
                        list.Add(new Record
                        {
                            Ship = name[i++] + " " + airCorps.ActionName,
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
    }
}