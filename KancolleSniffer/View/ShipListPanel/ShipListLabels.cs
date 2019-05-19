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
        private readonly List<ShipLabels> _labelList = new List<ShipLabels>();

        public ShipListLabels(ShipListPanel shipListPanel)
        {
            _shipListPanel = shipListPanel;
        }

        public void CreateShipLabels(int i)
        {
            var y = ShipListPanel.LineHeight * i + 1;
            const int height = ShipListPanel.LabelHeight;
            var labels = new ShipLabels
            {
                Fleet = new ShipLabel.Fleet(new Point(1, 2)),
                Name = new ShipLabel.Name(new Point(10, 2)),
                Hp = new ShipLabel.Hp(new Point(126, 0), ShipListPanel.LineHeight),
                Cond = new ShipLabel.Cond(new Point(128, 0), ShipListPanel.LineHeight),
                Level = new ShipLabel.Level(new Point(154, 2), height),
                Exp = new ShipLabel.Exp(new Point(175, 2), height),
                BackPanel =  new Panel
                {
                    Location = new Point(0, y),
                    Size = new Size(ListForm.PanelWidth, ShipListPanel.LineHeight),
                }
            };
            _labelList.Add(labels);
            labels.Arrange(_shipListPanel, CustomColors.ColumnColors.BrightFirst(i));
            _shipListPanel.SetHpPercent(labels.Hp);
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
            labels.Fleet.SetFleet(s);
            labels.Name.SetName(s, ShipNameWidth.ShipList);
            labels.Hp.SetHp(s);
            labels.Cond.SetCond(s);
            labels.Level.SetLevel(s);
            labels.Exp.SetExpToNext(s);
            labels.BackPanel.Visible = true;
        }

        public void SetShipType(int i)
        {
            var s = _shipListPanel.GetShip(i);
            var labels = _labelList[i];
            labels.Fleet.SetFleet(null);
            labels.Fleet.Text = s.Name;
            labels.Name.SetName(null);
            labels.Hp.SetHp(null);
            labels.Cond.SetCond(null);
            labels.Level.SetLevel(null);
            labels.Exp.SetExpToNext(null);
            labels.BackPanel.Visible = true;
        }

        public void HidePanel(int i)
        {
            _labelList[i].BackPanel.Visible = false;
        }
    }
}