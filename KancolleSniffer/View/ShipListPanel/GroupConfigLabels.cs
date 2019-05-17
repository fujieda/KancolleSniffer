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

// ReSharper disable CoVariantArrayConversion

namespace KancolleSniffer.View.ShipListPanel
{
    public class GroupConfigLabels
    {
        private readonly ShipListPanel _shipListPanel;
        private readonly List<CheckBox[]> _checkBoxesList = new List<CheckBox[]>();
        private readonly List<ShipLabel[]> _labelList = new List<ShipLabel[]>();
        private readonly List<Panel> _panelList = new List<Panel>();

        public const int GroupCount = 4;
        public HashSet<int>[] GroupSettings { get; } = new HashSet<int>[GroupCount];
        public bool GroupUpdated { get; set; }

        public GroupConfigLabels(ShipListPanel shipListPanel)
        {
            _shipListPanel = shipListPanel;
        }

        public void CreateComponents(int i)
        {
            var y = ShipListPanel.LineHeight * i + 1;
            var panel = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(ListForm.PanelWidth, ShipListPanel.LineHeight),
                BackColor = ShipLabel.ColumnColors[(i + 1) % 2]
            };
            Scaler.Scale(panel);
            panel.Tag = panel.Location.Y;
            var labels = new[]
            {
                new ShipLabel
                {
                    Location = new Point(90, 2),
                    Size = new Size(24, ShipListPanel.LabelHeight),
                    TextAlign = ContentAlignment.MiddleRight
                },
                new ShipLabel {Location = new Point(10, 2), AutoSize = true},
                new ShipLabel {Location = new Point(1, 2), AutoSize = true}
            };

            var cb = new CheckBox[GroupCount];
            for (var j = 0; j < cb.Length; j++)
            {
                cb[j] = new CheckBox
                {
                    Location = new Point(125 + j * 24, 2),
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(12, 11),
                    Tag = i * 10 + j
                };
                Scaler.Scale(cb[j]);
                cb[j].CheckedChanged += checkboxGroup_CheckedChanged;
            }
            _labelList.Add(labels);
            _checkBoxesList.Add(cb);
            _panelList.Add(panel);
            panel.Controls.AddRange(labels);
            panel.Controls.AddRange(cb);
            _shipListPanel.Controls.Add(panel);
            var unused = panel.Handle; // create handle
            foreach (var label in labels)
            {
                Scaler.Scale(label);
                label.PresetColor =
                    label.BackColor = ShipLabel.ColumnColors[(i + 1) % 2];
            }
        }

        private void checkboxGroup_CheckedChanged(object sender, EventArgs e)
        {
            var cb = (CheckBox)sender;
            var group = (int)cb.Tag % 10;
            var idx = (int)cb.Tag / 10;
            if (cb.Checked)
            {
                GroupSettings[group].Add(_shipListPanel.GetShip(idx).Id);
            }
            else
            {
                GroupSettings[group].Remove(_shipListPanel.GetShip(idx).Id);
            }
            GroupUpdated = true;
        }

        public void SetGrouping(int i)
        {
            var s = _shipListPanel.GetShip(i);
            var labels = _labelList[i];
            if (s.Level == 1000)
            {
                _shipListPanel.SetShipType(i);
                return;
            }
            labels[0].SetLevel(s);
            labels[1].SetName(s, ShipNameWidth.GroupConfig);
            labels[2].SetFleet(s);
            var cb = _checkBoxesList[i];
            for (var j = 0; j < cb.Length; j++)
                cb[j].Checked = GroupSettings[j].Contains(s.Id);
            _panelList[i].Visible = true;
        }

        public void HidePanel(int i)
        {
            _panelList[i].Visible = false;
        }

        public IEnumerable<ShipStatus> FilterByGroup(IEnumerable<ShipStatus> ships, string group)
        {
            var g = Array.FindIndex(new[] {"A", "B", "C", "D"}, x => x == group);
            if (g == -1)
                return ships;
            return from s in ships where GroupSettings[g].Contains(s.Id) select s;
        }
    }
}