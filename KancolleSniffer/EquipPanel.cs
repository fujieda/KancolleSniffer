// Copyright (C) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Drawing;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public class EquipPanel : Panel
    {
        private const int LineHeight = 14;
        private const int LabelHeight = 12;
        private EquipColumn[] _equipList;
        private readonly List<ShipLabel[]> _labelList = new List<ShipLabel[]>();
        private readonly List<Panel> _panelList = new List<Panel>();

        private class EquipColumn
        {
            public string Fleet { get; set; }
            public string Ship { get; set; }
            public int Id { get; set; }
            public string Equip { get; set; }
            public Color Color { get; set; }
            public string Spec { get; set; }

            public EquipColumn()
            {
                Fleet = Ship = Equip = "";
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
                        var slot = s.Slot[i];
                        var onslot = s.OnSlot[i];
                        var max = s.Spec.MaxEq[i];
                        if (slot == -1)
                            continue;
                        var item = sniffer.Item.ItemDict[slot];
                        if (item.Spec.Name == "ドラム缶(輸送用)")
                            drum++;
                        equips.Add(new EquipColumn
                        {
                            Equip = item.Spec.Name +
                                    (item.Alv == 0 ? "" : "+" + item.Alv) +
                                    (item.Level == 0 ? "" : "★" + item.Level) +
                                    (!item.Spec.IsAircraft ? "" : " " + onslot + "/" + max),
                            Color = item.Spec.Color
                        });
                    }
                    if (s.SlotEx > 0)
                    {
                        var item = sniffer.Item.ItemDict[s.SlotEx];
                        equips.Add(new EquipColumn {Equip = item.Spec.Name, Color = item.Spec.Color});
                    }
                    if (drum != 0)
                        drumShips++;
                    drumTotal += drum;
                    levelTotal += s.Level;
                    var rfp = s.RealFirepower;
                    var ras = s.RealAntiSubmarine;
                    ships.Add(new EquipColumn
                    {
                        Ship = (s.Escaped ? "[避]" : "") + s.Name + " Lv" + s.Level,
                        Id = s.Id,
                        Spec = (rfp == 0 ? "" : "砲" + rfp) + (ras == 0 ? "" : " 潜" + ras)
                    });
                    ships.AddRange(equips);
                }
                list.Add(new EquipColumn
                {
                    Fleet = fn[f] + (levelTotal == 0 ? "" : " 合計Lv" + levelTotal) +
                            (drumTotal == 0 ? "" : " ドラム缶" + drumTotal + "(" + drumShips + "隻)")
                });
                list.AddRange(ships);
            }
            _equipList = list.ToArray();
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
                Size = new Size(ShipListForm.PanelWidth, LineHeight),
                BackColor = ShipLabels.ColumnColors[(i + 1) % 2],
                Visible = false
            };
            lbp.Scale(ShipLabel.ScaleFactor);
            lbp.Tag = lbp.Location.Y;
            var labels = new[]
            {
                new ShipLabel {Location = new Point(1, 2), AutoSize = true},
                new ShipLabel {Location = new Point(10, 2), AutoSize = true},
                new ShipLabel {Location = new Point(40, 2), AutoSize = true},
                new ShipLabel {Location = new Point(37, 2), Size = new Size(4, LabelHeight - 2)},
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