// Copyright (C) 2017 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KancolleSniffer.Model;
using static System.Math;
// ReSharper disable CoVariantArrayConversion

namespace KancolleSniffer.View
{
    public class RepairListForMain : Panel
    {
        private const int PanelPadding = 5;
        private const int LineHeight = 15;
        private RepairLabels[] _repairLabels;
        private ShipStatus[] _repairList = new ShipStatus[0];
        private ListScroller _listScroller;

        private class RepairLabels : ControlsArranger
        {
            public ShipLabel.Fleet Fleet { private get; set; }
            public ShipLabel.Name Name { get; set; }
            public ShipLabel.RepairTime Time { private get; set; }
            public ShipLabel.Hp Damage { get; set; }
            public Label BackGround { private get; set; }

            public override Control[] Controls => new[] {Fleet, Damage, Time, Name, BackGround};

            public void Set(ShipStatus status)
            {
                foreach (var label in new ShipLabel[] {Fleet, Time})
                    label?.Set(status);
            }
        }

        public void CreateLabels(EventHandler onClick)
        {
            _repairLabels = new RepairLabels[Lines];
            SuspendLayout();
            for (var i = 0; i < _repairLabels.Length; i++)
            {
                var y = PanelPadding + 1 + i * LineHeight;
                const int height = 12;
                _repairLabels[i] = new RepairLabels
                {
                    Fleet = new ShipLabel.Fleet(new Point(0, y)),
                    Name = new ShipLabel.Name(new Point(9, y)),
                    Damage = new ShipLabel.Hp {Location = new Point(119, y), Size = new Size(5, height - 1)},
                    Time = new ShipLabel.RepairTime(new Point(75, y)),
                    BackGround = new Label
                    {
                        Location = new Point(0, y - 1),
                        Size = new Size(Width, height + 2)
                    }
                };
                _repairLabels[i].Arrange(this, CustomColors.ColumnColors.BrightFirst(i));
                _repairLabels[i].SetClickHandler(onClick);
            }
            ResumeLayout();
            SetupListScroller();
        }

        private int Lines
        {
            get
            {
                var baseHeight = Parent.ClientRectangle.Height - Location.Y;
                return (int)Round((baseHeight - Scaler.ScaleHeight((float)PanelPadding) * 2) / Scaler.ScaleHeight((float)LineHeight));
            }
        }

        private void SetupListScroller()
        {
            _listScroller = new ListScroller(this, _repairLabels[0].Controls, _repairLabels.Last().Controls)
            {
                Lines = _repairLabels.Length,
                Padding = PanelPadding
            };
            _listScroller.Update += ShowRepairList;
        }

        public void SetRepairList(ShipStatus[] list)
        {
            _repairList = list;
            _listScroller.DataCount = list.Length;
            SetPanelHeight();
            if (list.Length == 0)
            {
                SetPanelHeight();
                ClearLabels(0);
                ClearLabels(1);
                _repairLabels[0].Name.SetName("なし");
                return;
            }
            _listScroller.Position = Min(_listScroller.Position, Max(0, _repairList.Length - _repairLabels.Length));
            ShowRepairList();
        }

        private void SetPanelHeight()
        {
            var lines = Min(Max(1, _repairList.Length), _repairLabels.Length);
            Size = new Size(Width, Scaler.ScaleHeight(lines * LineHeight + PanelPadding * 2));
        }

        private void ShowRepairList()
        {
            for (var i = 0; i < Min(_repairList.Length, _repairLabels.Length); i++)
            {
                var s = _repairList[i + _listScroller.Position];
                var labels = _repairLabels[i];
                labels.Set(s);
                labels.Name.SetName(s, ShipNameWidth.RepairList);
                labels.Damage.SetColor(s);
            }
            if (_repairList.Length < _repairLabels.Length)
                ClearLabels(_repairList.Length);
            _listScroller.DrawMark();
        }

        private void ClearLabels(int i)
        {
            var labels = _repairLabels[i];
            labels.Set(null);
            labels.Name.SetName(null);
            labels.Damage.BackColor = CustomColors.ColumnColors.BrightFirst(i);
        }
    }
}