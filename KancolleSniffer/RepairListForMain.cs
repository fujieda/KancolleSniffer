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
using static System.Math;

namespace KancolleSniffer
{
    public class RepairListForMain : Panel
    {
        private const int PanelPadding = 5;
        private const int LabelPadding = 2;
        private const int LineHeight = 16;
        private readonly ShipLabel[][] _repairLabels = new ShipLabel[14][];
        private ShipStatus[] _repairList;
        private int _repairListPosition;

        public void CreateLabels(EventHandler onClick)
        {
            SuspendLayout();
            for (var i = 0; i < _repairLabels.Length; i++)
            {
                var y = PanelPadding + LabelPadding + i * LineHeight;
                const int height = 12;
                Controls.AddRange(_repairLabels[i] = new[]
                {
                    new ShipLabel {Location = new Point(0, y), Size = new Size(11, height)},
                    new ShipLabel {Location = new Point(119, y), Size = new Size(5, height - 1)},
                    new ShipLabel {Location = new Point(75, y), AutoSize = true},
                    new ShipLabel {Location = new Point(9, y), AutoSize = true},
                    new ShipLabel {Location = new Point(0, y - LabelPadding), Size = new Size(Width, height + LabelPadding + 1)}
                });
                foreach (var label in _repairLabels[i])
                {
                    label.Scale();
                    label.PresetColor = label.BackColor = ShipLabels.ColumnColors[(i + 1) % 2];
                    label.Click += onClick;
                }
            }
            foreach (var label in _repairLabels[0])
            {
                label.MouseEnter += TopRepairLabelsOnMouseEnter;
                label.MouseLeave += TopRepairLabelsOnMouseLeave;
            }
            _topScrollRepeatTimer.Tick += TopRepairLabelsOnMouseEnter;
            foreach (var label in _repairLabels.Last())
            {
                label.MouseEnter += BottomRepairLabelsOnMouseEnter;
                label.MouseLeave += BottomRepairLabelsOnMouseLeave;
            }
            _bottomScrollRepeatTimer.Tick += BottomRepairLabelsOnMouseEnter;
            ResumeLayout();
        }

        private readonly Timer _topScrollRepeatTimer = new Timer {Interval = 100};
        private readonly Timer _bottomScrollRepeatTimer = new Timer {Interval = 100};

        private void TopRepairLabelsOnMouseEnter(object sender, EventArgs e)
        {
            if (_repairListPosition == 0)
                return;
            _repairListPosition--;
            ShowRepairList();
            _topScrollRepeatTimer.Start();
        }

        private void TopRepairLabelsOnMouseLeave(object sender, EventArgs e)
        {
            _topScrollRepeatTimer.Stop();
        }

        private void BottomRepairLabelsOnMouseEnter(object sender, EventArgs e)
        {
            if (_repairListPosition + _repairLabels.Length >= _repairList.Length)
                return;
            _repairListPosition++;
            ShowRepairList();
            _bottomScrollRepeatTimer.Start();
        }

        private void BottomRepairLabelsOnMouseLeave(object sender, EventArgs e)
        {
            _bottomScrollRepeatTimer.Stop();
        }

        public void SetRepairList(ShipStatus[] list)
        {
            const int fleet = 0, name = 3, time = 2, damage = 1;
            _repairList = list;
            if (list.Length == 0)
            {
                Size = new Size(Width, (int)Round(ShipLabel.ScaleFactor.Height * (LineHeight + PanelPadding * 2)));
                var labels = _repairLabels[0];
                labels[fleet].Text = "";
                labels[name].SetName("なし");
                labels[time].Text = "";
                labels[damage].BackColor = labels[damage].PresetColor;
                return;
            }
            Size = new Size(Width,
                (int)Round(ShipLabel.ScaleFactor.Height *
                           (Min(_repairList.Length, _repairLabels.Length) * LineHeight + PanelPadding * 2)));
            _repairListPosition = Min(_repairListPosition, Max(0, _repairList.Length - _repairLabels.Length));
            ShowRepairList();
        }

        public void ShowRepairList()
        {
            const int fleet = 0, name = 3, time = 2, damage = 1;
            for (var i = 0; i < Min(_repairList.Length, _repairLabels.Length); i++)
            {
                var s = _repairList[i + _repairListPosition];
                var labels = _repairLabels[i];
                labels[fleet].SetFleet(s);
                labels[name].SetName(s, ShipNameWidth.RepairList);
                labels[time].SetRepairTime(s);
                labels[damage].BackColor = ShipLabel.DamageColor(s, labels[damage].PresetColor);
            }
            DrawMark();
        }

        private void DrawMark()
        {
            using (var g = CreateGraphics())
            {
                var topBrush = _repairListPosition > 0 ? Brushes.Black : new SolidBrush(BackColor);
                g.FillPolygon(topBrush,
                    new[]
                    {
                        new PointF(Width * 0.45f, PanelPadding), new PointF(Width * 0.55f, PanelPadding),
                        new PointF(Width * 0.5f, 0), new PointF(Width * 0.45f, PanelPadding)
                    });
                var bottomBrush = _repairLabels.Length + _repairListPosition < _repairList.Length
                    ? Brushes.Black
                    : new SolidBrush(BackColor);
                g.FillPolygon(bottomBrush,
                    new[]
                    {
                        new PointF(Width * 0.45f, Height - PanelPadding - 2), new PointF(Width * 0.55f, Height - PanelPadding - 2),
                        new PointF(Width * 0.5f, Height - 2), new PointF(Width * 0.45f, Height - PanelPadding - 2)
                    });

            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawMark();
        }
    }
}