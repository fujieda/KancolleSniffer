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

namespace KancolleSniffer
{
    public class FleetPanel : Panel
    {
        private const int LineHeight = 14;
        private const int LabelHeight = 12;
        private EquipColumn[] _equipList;
        private readonly List<ShipLabel[]> _labelList = new List<ShipLabel[]>();
        private readonly List<Panel> _panelList = new List<Panel>();
        private readonly ToolTip _toolTip = new ToolTip {ShowAlways = true};

        private class EquipColumn
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

            public EquipColumn()
            {
                Fleet = Ship = Equip = AircraftSpec = "";
                Color = DefaultBackColor;
            }
        }

        public void UpdateEquip(Sniffer sniffer)
        {
            CreateEquipList(sniffer);
            SuspendLayout();
            CreateEquipLabels();
            SetEquipLabels();
            ResumeLayout();
        }

        private void CreateEquipList(Sniffer sniffer)
        {
            var list = new List<EquipColumn>();
            var fn = new[] {"第一艦隊", "第二艦隊", "第三艦隊", "第四艦隊"};
            var tp = 0.0;
            for (var f = 0; f < fn.Length; f++)
            {
                var drumTotal = 0;
                var drumShips = 0;
                var levelTotal = 0;
                var ships = new List<EquipColumn>();
                foreach (var s in sniffer.GetShipStatuses(f))
                {
                    var drum = 0;
                    var equips = new List<EquipColumn>();
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
                        if (item.Spec.CanAirCombat)
                        {
                            if (item.Spec.Type == 7 || item.Spec.Type == 11) // 爆撃
                            {
                                airspec = "航空戦 " + (25 + (int)(item.Spec.Bomber * Math.Sqrt(onslot)));
                            }
                            else if (item.Spec.Type == 8)
                            {
                                var normal = 25 + item.Spec.Torpedo * Math.Sqrt(onslot);
                                airspec = "航空戦 " + (int)(normal * 0.8) + "/" + (int)(normal * 1.5);
                            }
                        }

                        equips.Add(new EquipColumn
                        {
                            Equip = GenEquipString(item, onslot, max),
                            AircraftSpec = airspec,
                            Color = item.Spec.Color
                        });
                    }
                    if (s.SlotEx.Id > 0)
                    {
                        var item = s.SlotEx;
                        equips.Add(new EquipColumn {Equip = item.Spec.Name, Color = item.Spec.Color});
                    }
                    if (drum != 0)
                        drumShips++;
                    drumTotal += drum;
                    levelTotal += s.Level;
                    if (f < 2)
                        tp += s.TransportPoint;
                    var fire = s.RealFirepower;
                    var subm = s.RealAntiSubmarine;
                    var torp = s.RealTorpedo;
                    var night = s.NightBattlePower;
                    var ship = new EquipColumn
                    {
                        Ship = (s.Escaped ? "[避]" : "") + s.Name + " Lv" + s.Level,
                        Id = s.Id,
                        // ReSharper disable CompareOfFloatsByEqualityOperator
                        Spec = (fire == 0 ? "" : $"砲{fire:f1}") + (subm == 0 ? "" : $" 潜{subm:f1}"),
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
                list.Add(new EquipColumn
                {
                    Fleet = fn[f] + (levelTotal == 0 ? "" : " 合計Lv" + levelTotal) +
                            (drumTotal == 0 ? "" : " ドラム缶" + drumTotal + "(" + drumShips + "隻)")
                });
                list.AddRange(ships);
            }
            list[0].Fleet2 = $"TP: S{(int)tp} A{(int)(tp * 0.7)}";
            if (sniffer.BaseAirCorps != null)
            {
                var name = new[] {"第一", "第二", "第三"};
                list.Add(new EquipColumn {Fleet = "基地航空隊"});
                var i = 0;
                foreach (var corps in sniffer.BaseAirCorps)
                {
                    if (i >= name.Length)
                        break;
                    var fp = corps.FighterPower;
                    list.Add(new EquipColumn
                    {
                        Ship = name[i++] + " " + corps.ActionName,
                        Spec = "制空" + (fp[0] == fp[1] ? fp[0].ToString() : fp[0] + "～" + fp[1]) + " 距離" + corps.Distance
                    });
                    list.AddRange(corps.Planes.Select(plane => new EquipColumn
                    {
                        Equip =
                            plane.State != 1 ? plane.StateName : GenEquipString(plane.Slot, plane.Count, plane.MaxCount),
                        Color = plane.Slot.Spec.Color
                    }));
                }
            }
            _equipList = list.ToArray();
        }

        private string GenEquipString(ItemStatus item, int onslot, int max)
        {
            var name = item.Spec.Name;
            var attr = (item.Alv == 0 ? "" : "+" + item.Alv) +
                       (item.Level == 0 ? "" : "★" + item.Level) +
                       (!item.Spec.IsAircraft ? "" : " " + onslot + "/" + max);
            var proposed = new Size(int.MaxValue, int.MaxValue);
            const int maxWidth = 180;
            var result = name + attr;
            if (TextRenderer.MeasureText(result, Font, proposed).Width <= maxWidth)
                return result;
            attr = " " + attr;
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

        private void CreateEquipLabels()
        {
            for (var i = _labelList.Count; i < _equipList.Length; i++)
                CreateEquipLabels(i);
        }

        private void CreateEquipLabels(int i)
        {
            var y = 1 + LineHeight * i;
            var lbp = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(ListForm.PanelWidth, LineHeight),
                BackColor = ShipLabels.ColumnColors[(i + 1) % 2],
                Visible = false
            };
            lbp.Scale(ShipLabel.ScaleFactor);
            lbp.Tag = lbp.Location.Y;
            var labels = new[]
            {
                new ShipLabel {Location = new Point(1, 2), AutoSize = true},
                new ShipLabel {Location = new Point(10, 2), AutoSize = true},
                new ShipLabel {Location = new Point(38, 2), AutoSize = true},
                new ShipLabel {Location = new Point(35, 2), Size = new Size(4, LabelHeight - 2)},
                new ShipLabel {Location = new Point(217, 2), AutoSize = true, AnchorRight = true}
            };
            _labelList.Add(labels);
            _panelList.Add(lbp);
            // ReSharper disable once CoVariantArrayConversion
            lbp.Controls.AddRange(labels);
            Controls.Add(lbp);
            foreach (var label in labels)
            {
                label.Scale();
                label.PresetColor =
                    label.BackColor = ShipLabels.ColumnColors[(i + 1) % 2];
            }
        }

        private void SetEquipLabels()
        {
            for (var i = 0; i < _equipList.Length; i++)
                SetEquip(i);
            for (var i = _equipList.Length; i < _labelList.Count; i++)
                _panelList[i].Visible = false;
        }

        private void SetEquip(int i)
        {
            var lbp = _panelList[i];
            if (!lbp.Visible)
                lbp.Location = new Point(lbp.Left, (int)lbp.Tag + AutoScrollPosition.Y);
            var e = _equipList[i];
            var labels = _labelList[i];
            labels[0].Text = e.Fleet;
            labels[1].SetName(e.Ship);
            labels[2].Text = e.Equip;
            labels[3].Visible = e.Equip != "";
            labels[3].BackColor = e.Color;
            labels[4].Text = e.Spec;
            if (e.Fleet != "" && e.Fleet2 != "")
                _toolTip.SetToolTip(labels[0], e.Fleet2);
            _toolTip.SetToolTip(labels[2], e.AircraftSpec != "" ? e.AircraftSpec : "");
            _toolTip.SetToolTip(labels[4], e.Spec2 != "" ? e.Spec2 : "");
            lbp.Visible = true;
        }

        public void ShowShip(int id)
        {
            var i = Array.FindIndex(_equipList, e => e.Id == id);
            if (i == -1)
                return;
            var y = (int)Math.Round(ShipLabel.ScaleFactor.Height * LineHeight * i);
            AutoScrollPosition = new Point(0, y);
        }
    }
}