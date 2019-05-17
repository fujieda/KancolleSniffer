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
using System.Windows.Forms;

namespace KancolleSniffer.View.ShipListPanel
{
    public class ShipListLabels
    {
        private readonly ShipListPanel _shipListPanel;
        private readonly List<ShipLabel[]> _labelList = new List<ShipLabel[]>();
        private readonly List<Panel> _panelList = new List<Panel>();

        public ShipListLabels(ShipListPanel shipListPanel)
        {
            _shipListPanel = shipListPanel;
        }

        public void CreateShipLabels(int i)
        {
            var y = ShipListPanel.LineHeight * i + 1;
            const int height = ShipListPanel.LabelHeight;
            var panel = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(ListForm.PanelWidth, ShipListPanel.LineHeight),
                BackColor = ShipLabel.ColumnColors[(i + 1) % 2]
            };
            Scaler.Scale(panel);
            var labels = new[]
            {
                new ShipLabel
                {
                    Location = new Point(126, 0),
                    AutoSize = true,
                    AnchorRight = true,
                    MinimumSize = new Size(0, ShipListPanel.LineHeight),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand
                },
                new ShipLabel
                {
                    Location = new Point(128, 0),
                    Size = new Size(24, ShipListPanel.LineHeight),
                    TextAlign = ContentAlignment.MiddleRight
                },
                new ShipLabel
                {
                    Location = new Point(154, 2),
                    Size = new Size(24, height),
                    TextAlign = ContentAlignment.MiddleRight
                },
                new ShipLabel
                {
                    Location = new Point(175, 2),
                    Size = new Size(42, height),
                    TextAlign = ContentAlignment.MiddleRight
                },
                new ShipLabel {Location = new Point(10, 2), AutoSize = true},
                new ShipLabel {Location = new Point(1, 2), AutoSize = true}
            };
            _labelList.Add(labels);
            _panelList.Add(panel);
            // ReSharper disable once CoVariantArrayConversion
            panel.Controls.AddRange(labels);
            _shipListPanel.Controls.Add(panel);
            var unused = panel.Handle; // create handle
            foreach (var label in labels)
            {
                Scaler.Scale(label);
                label.PresetColor =
                    label.BackColor = ShipLabel.ColumnColors[(i + 1) % 2];
            }
            _shipListPanel.SetHpPercent(labels[0]);
        }

        public void SetShipStatus(int i)
        {
            var s = _shipListPanel.GetShip(i);
            var labels = _labelList[i];
            if (s.Level == 1000) // 艦種の表示
            {
                SetShipType(i);
                return;
            }
            labels[0].SetHp(s);
            labels[1].SetCond(s);
            labels[2].SetLevel(s);
            labels[3].SetExpToNext(s);
            labels[4].SetName(s, ShipNameWidth.ShipList);
            labels[5].SetFleet(s);
            _panelList[i].Visible = true;
        }

        public void SetShipType(int i)
        {
            var s = _shipListPanel.GetShip(i);
            var labels = _labelList[i];
            labels[0].SetHp(null);
            labels[1].SetCond(null);
            labels[2].SetLevel(null);
            labels[3].SetExpToNext(null);
            labels[4].SetName(null);
            labels[5].SetFleet(null);
            labels[5].Text = s.Name;
            _panelList[i].Visible = true;
        }

        public void HidePanel(int i)
        {
            _panelList[i].Visible = false;
        }
    }
}