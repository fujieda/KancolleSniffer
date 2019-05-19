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

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KancolleSniffer.View.ShipListPanel
{
    public class RepairListLabels
    {
        private readonly ShipListPanel _shipListPanel;
        private readonly List<RepairLabels> _labelList = new List<RepairLabels>();

        private class RepairLabels : ShipLabels
        {
            public ShipLabel Time { get; set; }
            public Label PerHp { get; set; }

            public override Control[] Controls => base.Controls.Concat(new[] {Time, PerHp}).ToArray();
        }

        public RepairListLabels(ShipListPanel shipListPanel)
        {
            _shipListPanel = shipListPanel;
        }

        public void CreateLabels(int i)
        {
            var y = ShipListPanel.LineHeight * i + 1;
            const int height = ShipListPanel.LabelHeight;
            var labels = new RepairLabels
            {
                Fleet = new ShipLabel {Location = new Point(1, 2), AutoSize = true},
                Name = new ShipLabel {Location = new Point(10, 2), AutoSize = true},
                Hp = new ShipLabel.Hp(new Point(118, 0), ShipListPanel.LineHeight),
                Level = new ShipLabel
                {
                    Location = new Point(116, 2),
                    Size = new Size(24, height),
                    TextAlign = ContentAlignment.MiddleRight
                },
                Time = new ShipLabel {Location = new Point(141, 2), AutoSize = true},
                PerHp = new Label {Location = new Point(186, 2), AutoSize = true},
                BackPanel = new Panel
                {
                    Location = new Point(0, y),
                    Size = new Size(ListForm.PanelWidth, ShipListPanel.LineHeight)
                }
            };
            _labelList.Add(labels);
            labels.Arrange(_shipListPanel, CustomColors.ColumnColors.BrightFirst(i));
            _shipListPanel.SetHpPercent(labels.Hp);
        }

        public void SetRepairList(int i)
        {
            var s = _shipListPanel.GetShip(i);
            if (s.Level == 1000)
            {
                _shipListPanel.SetShipType(i);
                return;
            }
            var labels = _labelList[i];
            labels.Fleet.SetFleet(s);
            labels.Name.SetName(s, ShipNameWidth.RepairListFull);
            labels.Hp.SetHp(s);
            labels.Level.SetLevel(s);
            labels.Time.SetRepairTime(s);
            labels.PerHp.Text = s.RepairTimePerHp.ToString(@"mm\:ss");
            labels.BackPanel.Visible = true;
        }

        public void HidePanel(int i)
        {
            _labelList[i].BackPanel.Visible = false;
        }
    }
}