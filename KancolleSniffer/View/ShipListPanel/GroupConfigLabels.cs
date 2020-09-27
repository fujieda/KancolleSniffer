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
using KancolleSniffer.Forms;

// ReSharper disable CoVariantArrayConversion

namespace KancolleSniffer.View.ShipListPanel
{
    public class GroupConfigLabels
    {
        private readonly ShipListPanel _shipListPanel;
        private readonly List<CheckBox[]> _checkBoxesList = new List<CheckBox[]>();
        private readonly List<ShipLabels> _labelList = new List<ShipLabels>();

        public const int GroupCount = 4;
        public List<List<int>> GroupSettings { get; set; }
        public bool GroupUpdated { get; set; }

        public GroupConfigLabels(ShipListPanel shipListPanel)
        {
            _shipListPanel = shipListPanel;
        }

        public void CreateComponents(int i)
        {
            var y = ShipListPanel.LineHeight * i + 1;
            var labels = new ShipLabels
            {
                Fleet = new ShipLabel.Fleet(new Point(1, 2)),
                Name = new ShipLabel.Name(new Point(10, 2), ShipNameWidth.GroupConfig),
                Level = new ShipLabel.Level(new Point(90, 2), ShipListPanel.LabelHeight),
                BackPanel = new Panel
                {
                    Location = new Point(0, y),
                    Size = new Size(ListForm.PanelWidth, ShipListPanel.LineHeight),
                    Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top
                }
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
                cb[j].CheckedChanged += checkboxGroup_CheckedChanged;
            }
            SetAnchorRight(cb.Concat(new Control[] {labels.Level}).ToArray());
            _labelList.Add(labels);
            _checkBoxesList.Add(cb);
            labels.Arrange(_shipListPanel, CustomColors.ColumnColors.BrightFirst(i));
            labels.BackPanel.Controls.AddRange(cb);
            labels.Scale();
        }

        private void SetAnchorRight(params Control[] controls)
        {
            foreach (var control in controls)
                control.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        }

        public void Resize(int i, int width)
        {
            var labels = _labelList[i];
            labels.BackPanel.Width = width;
            labels.Name.AdjustWidth(Scaler.DownWidth(width) - ListForm.PanelWidth);
        }

        private void checkboxGroup_CheckedChanged(object sender, EventArgs e)
        {
            var cb = (CheckBox)sender;
            var group = (int)cb.Tag % 10;
            var idx = (int)cb.Tag / 10;
            GroupSettings[group].RemoveAll(id => id == _shipListPanel.GetShip(idx).Id);
            if (cb.Checked)
                GroupSettings[group].Add(_shipListPanel.GetShip(idx).Id);
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
            labels.Set(s);
            var cb = _checkBoxesList[i];
            for (var j = 0; j < cb.Length; j++)
                cb[j].Checked = GroupSettings[j].Contains(s.Id);
            labels.BackPanel.Visible = true;
        }

        public void HidePanel(int i)
        {
            _labelList[i].BackPanel.Visible = false;
        }
    }
}