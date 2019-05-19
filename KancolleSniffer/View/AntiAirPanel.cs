﻿// Copyright (C) 2016 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

namespace KancolleSniffer.View
{
    public class AntiAirPanel : Panel
    {
        private const int LineHeight = 16;
        private readonly List<AntiAirLabels> _labelList = new List<AntiAirLabels>();
        private readonly List<Record> _table = new List<Record>();

        private class AntiAirLabels : ShipLabels
        {
            public Label Rate { get; set; }
            public Label Diff { get; set; }

            public override Control[] Controls => base.Controls.Concat(new Control[] {Rate, Diff}).ToArray();
        }

        public void Update(Sniffer sniffer)
        {
            CreateTable(sniffer);
            SuspendLayout();
            CreateLabels();
            SetRecords();
            ResumeLayout();
        }

        private class Record
        {
            public string Fleet { get; set; }
            public string Ship { get; set; }
            public int Id { get; set; }
            public string Rate { get; set; }
            public string Diff { get; set; }

            public Record()
            {
                Fleet = Ship = Rate = Diff = "";
            }
        }

        private void CreateTable(Sniffer sniffer)
        {
            _table.Clear();
            var fn = new[] {"第一艦隊", "第二艦隊", "第三艦隊", "第四艦隊"};
            foreach (var fleet in sniffer.Fleets)
            {
                var ships = fleet.ActualShips;
                var rawForFleet = ships.Sum(ship => ship.EffectiveAntiAirForFleet);
                var forFleet = new[] {1.0, 1.2, 1.6}.Select(r => (int)(rawForFleet * r) * 2 / 1.3).ToArray();
                _table.Add(new Record
                {
                    Fleet = fn[fleet.Number] + " : " + string.Join("/", forFleet.Select(x => x.ToString("f1")))
                });
                foreach (var ship in ships)
                {
                    var rate = ship.EffectiveAntiAirForShip / 4.0;
                    var diff = forFleet.Select(x => (x + ship.EffectiveAntiAirForShip) / 10.0);
                    _table.Add(new Record
                    {
                        Ship = ship.Name + " Lv" + ship.Level +
                               " : " + ship.EffectiveAntiAirForShip.ToString("d"),
                        Id = ship.Id
                    });
                    _table.Add(new Record
                    {
                        Rate = "割合: " + rate.ToString("f1") + "% ",
                        Diff = "固定: " + string.Join("/", diff.Select(d => d.ToString("f1")))
                    });
                }
            }
        }

        private void CreateLabels()
        {
            for (var i = _labelList.Count; i < _table.Count; i++)
                CreateLabels(i);
        }

        private void CreateLabels(int i)
        {
            var y = 1 + LineHeight * i;
            var labels = new AntiAirLabels
            {
                Fleet = new ShipLabel {Location = new Point(1, 3), AutoSize = true},
                Name = new ShipLabel {Location = new Point(10, 3), AutoSize = true},
                Rate = new Label {Location = new Point(35, 3), AutoSize = true},
                Diff = new Label {Location = new Point(100, 3), AutoSize = true},
                BackPanel = new Panel
                {
                    Location = new Point(0, y),
                    Size = new Size(ListForm.PanelWidth, LineHeight),
                }
            };
            _labelList.Add(labels);
            labels.Arrange(this, CustomColors.ColumnColors.BrightFirst(i));
        }

        private void SetRecords()
        {
            for (var i = 0; i < _table.Count; i++)
                SetRecord(i);
            for (var i = _table.Count; i < _labelList.Count; i++)
                _labelList[i].BackPanel.Visible = false;
        }

        private void SetRecord(int i)
        {
            var lbp = _labelList[i].BackPanel;
            var column = _table[i];
            var labels = _labelList[i];
            labels.Fleet.Text = column.Fleet;
            labels.Name.SetName(column.Ship);
            labels.Rate.Text = column.Rate;
            labels.Diff.Text = column.Diff;
            lbp.Visible = true;
        }

        public void ShowShip(int id)
        {
            var i = _table.FindIndex(e => e.Id == id);
            if (i == -1)
                return;
            var y = Scaler.ScaleHeight(LineHeight * i);
            AutoScrollPosition = new Point(0, y);
        }
    }
}