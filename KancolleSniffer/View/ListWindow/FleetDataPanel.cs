// Copyright (C) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Windows.Forms;
using Clipboard = KancolleSniffer.Util.Clipboard;
using KancolleSniffer.Forms;

namespace KancolleSniffer.View.ListWindow
{
    public class FleetDataPanel : PanelWithToolTip
    {
        private const int LineHeight = 14;
        private const int LabelHeight = 12;
        private FleetData.Record[] _data = new FleetData.Record[0];
        private readonly List<FleetLabels> _labelList = new List<FleetLabels>();

        public FleetDataPanel()
        {
            ToolTip.AutoPopDelay = 10000;
        }

        public void Update(Sniffer sniffer)
        {
            _data = FleetData.Create(sniffer);
            SuspendLayout();
            CreateLabels();
            SetRecords();
            ResumeLayout();
        }

        private void CreateLabels()
        {
            for (var i = _labelList.Count; i < _data.Length; i++)
                CreateLabels(i);
        }

        private class FleetLabels : ControlsArranger
        {
            public Label Fleet { get; set; }
            public ShipLabel.Name Name { get; set; }
            public Label Equip { get; set; }
            public Label EquipColor { get; set; }
            public Label Spec { get; set; }

            public override Control[] Controls => new Control[] {Fleet, Name, Equip, EquipColor, Spec};
        }

        private void CreateLabels(int i)
        {
            var y = 1 + LineHeight * i;
            var labels = new FleetLabels
            {
                Fleet = new Label {Location = new Point(1, 2), AutoSize = true},
                Name = new ShipLabel.Name(new Point(10, 2), ShipNameWidth.Max),
                Equip = new Label {Location = new Point(38, 2), AutoSize = true},
                EquipColor = new Label {Location = new Point(35, 2), Size = new Size(4, LabelHeight - 2)},
                Spec = new GrowLeftLabel {Location = new Point(217, 2), GrowLeft = true},
                BackPanel = new Panel
                {
                    Location = new Point(0, y),
                    Size = new Size(ListForm.PanelWidth, LineHeight),
                    BackColor = CustomColors.ColumnColors.BrightFirst(i)
                }
            };
            _labelList.Add(labels);
            labels.Fleet.DoubleClick += (obj, ev) => { Clipboard.SetText((string)labels.Fleet.Tag); };
            labels.Arrange(this, CustomColors.ColumnColors.BrightFirst(i));
            labels.Move(AutoScrollPosition);
        }

        private void SetRecords()
        {
            for (var i = 0; i < _data.Length; i++)
                SetRecord(i);
            for (var i = _data.Length; i < _labelList.Count; i++)
                _labelList[i].BackPanel.Visible = false;
        }

        private void SetRecord(int i)
        {
            var e = _data[i];
            var labels = _labelList[i];
            labels.Fleet.Text = e.Fleet;
            labels.Fleet.Tag = "";
            labels.Name.SetName(e.Ship);
            if (e.Ship2 != "")
                ToolTip.SetToolTip(labels.Name, e.Ship2);
            labels.Equip.Text = e.Equip;
            labels.EquipColor.Visible = e.Equip != "";
            labels.EquipColor.BackColor = e.Color;
            labels.Spec.Text = e.Spec;
            if (e.Fleet != "" && e.Fleet2 != "")
            {
                ToolTip.SetToolTip(labels.Fleet, e.Fleet2);
                labels.Fleet.Tag = e.Fleet2;
            }
            ToolTip.SetToolTip(labels.Equip, e.AircraftSpec != "" ? e.AircraftSpec : "");
            ToolTip.SetToolTip(labels.Spec, e.Spec2 != "" ? e.Spec2 : "");
            labels.BackPanel.Visible = true;
        }

        public void ShowShip(int id)
        {
            var i = Array.FindIndex(_data, e => e.Id == id);
            if (i == -1)
                return;
            var y = Scaler.ScaleHeight(LineHeight * i);
            AutoScrollPosition = new Point(0, y);
        }

        public void ShowFleet(string fn)
        {
            var i = Array.FindIndex(_data, e => e.Fleet.StartsWith(fn));
            if (i == -1)
                return;
            var y = Scaler.ScaleHeight(LineHeight * i);
            AutoScrollPosition = new Point(0, y);
        }
    }
}