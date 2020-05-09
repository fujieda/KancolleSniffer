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
using KancolleSniffer.Forms;
using KancolleSniffer.Model;

namespace KancolleSniffer.View.ShipListPanel
{
    public class RepairListLabels
    {
        private readonly ShipListPanel _shipListPanel;
        private readonly List<RepairLabels> _labelList = new List<RepairLabels>();

        private class RepairLabels : ShipLabels
        {
            public ShipLabel.RepairTime Time { get; set; }
            public Label PerHp { get; set; }

            protected override Control[] AddedControls => new Control[] {Time, PerHp};

            public override void Set(ShipStatus status)
            {
                base.Set(status);
                Time.Set(status);
            }
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
                Fleet = new ShipLabel.Fleet(new Point(1, 2)),
                Name = new ShipLabel.Name(new Point(10, 2), ShipNameWidth.RepairListFull),
                Hp = new ShipLabel.Hp(new Point(118, 0), ShipListPanel.LineHeight),
                Level = new ShipLabel.Level(new Point(116, 2), height),
                Time = new ShipLabel.RepairTime(new Point(141, 2)),
                PerHp = new Label {Location = new Point(185, 2), AutoSize = true},
                BackPanel = new Panel
                {
                    Location = new Point(0, y),
                    Size = new Size(ListForm.PanelWidth, ShipListPanel.LineHeight),
                    Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top
                }
            };
            SetAnchorRight(labels.Hp, labels.Level, labels.Time, labels.PerHp);
            _labelList.Add(labels);
            labels.Arrange(_shipListPanel, CustomColors.ColumnColors.BrightFirst(i));
            _shipListPanel.SetHpPercent(labels.Hp);
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
            labels.Hp.AdjustLocation();
            labels.Name.AdjustWidth(Scaler.DownWidth(width) - ListForm.PanelWidth);
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
            labels.Set(s);
            labels.PerHp.Text = s.RepairTimePerHp.ToString(@"mm\:ss");
            labels.BackPanel.Visible = true;
        }

        public void HidePanel(int i)
        {
            _labelList[i].BackPanel.Visible = false;
        }
    }
}