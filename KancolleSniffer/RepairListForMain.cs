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
using System.Collections;
using System.Collections.Generic;
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
        private readonly RepairListLabels[] _repairLabels = new RepairListLabels[14];
        private ShipStatus[] _repairList = new ShipStatus[0];
        private int _repairListPosition;

        private class RepairListLabels : IEnumerable<ShipLabel>
        {
            public ShipLabel Fleet { get; set; }
            public ShipLabel Name { get; set; }
            public ShipLabel Time { get; set; }
            public ShipLabel Damage { get; set; }
            public ShipLabel BackGround { private get; set; }

            public IEnumerator<ShipLabel> GetEnumerator()
            {
                foreach (var label in new[] {Fleet, Damage, Time, Name, BackGround})
                    yield return label;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public void CreateLabels(EventHandler onClick)
        {
            SuspendLayout();
            for (var i = 0; i < _repairLabels.Length; i++)
            {
                var y = PanelPadding + LabelPadding + i * LineHeight;
                const int height = 12;
                _repairLabels[i] = new RepairListLabels
                {
                    Fleet = new ShipLabel {Location = new Point(0, y), Size = new Size(11, height)},
                    Damage = new ShipLabel {Location = new Point(119, y), Size = new Size(5, height - 1)},
                    Time = new ShipLabel {Location = new Point(75, y), AutoSize = true},
                    Name = new ShipLabel {Location = new Point(9, y), AutoSize = true},
                    BackGround = new ShipLabel
                    {
                        Location = new Point(0, y - LabelPadding),
                        Size = new Size(Width, height + LabelPadding + 1)
                    }
                };
                Controls.AddRange(_repairLabels[i].Cast<Control>().ToArray());
                foreach (var label in _repairLabels[i])
                {
                    label.Scale();
                    label.PresetColor = label.BackColor = ShipLabels.ColumnColors[(i + 1) % 2];
                    label.Click += onClick;
                }
            }
            SetScrollEventHandler();
            ResumeLayout();
        }

        private void SetScrollEventHandler()
        {
            foreach (var label in _repairLabels.First())
            {
                label.MouseEnter += TopRepairLabelsOnMouseEnter;
                label.MouseLeave += TopRepairLabelsOnMouseLeave;
            }
            foreach (var label in _repairLabels.Last())
            {
                label.MouseEnter += BottomRepairLabelsOnMouseEnter;
                label.MouseLeave += BottomRepairLabelsOnMouseLeave;
            }
            _topScrollRepeatTimer.Tick += TopRepairLabelsOnMouseEnter;
            _bottomScrollRepeatTimer.Tick += BottomRepairLabelsOnMouseEnter;
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
            _repairList = list;
            SetPanelHeight();
            if (list.Length == 0)
            {
                SetPanelHeight();
                ClearLabels(0);
                _repairLabels[0].Name.SetName("なし");
                return;
            }
            _repairListPosition = Min(_repairListPosition, Max(0, _repairList.Length - _repairLabels.Length));
            ShowRepairList();
        }

        private void SetPanelHeight()
        {
            var lines = Min(Max(1, _repairList.Length), _repairLabels.Length);
            Size = new Size(Width, (int)Round(ShipLabel.ScaleFactor.Height * lines * LineHeight + PanelPadding * 2));
        }

        public void ShowRepairList()
        {
            for (var i = 0; i < Min(_repairList.Length, _repairLabels.Length); i++)
            {
                var s = _repairList[i + _repairListPosition];
                var labels = _repairLabels[i];
                labels.Fleet.SetFleet(s);
                labels.Name.SetName(s, ShipNameWidth.RepairList);
                labels.Time.SetRepairTime(s);
                labels.Damage.BackColor = ShipLabel.DamageColor(s, labels.Damage.PresetColor);
            }
            DrawMark();
        }

        public void ClearLabels(int i)
        {
            var labels = _repairLabels[i];
            labels.Fleet.Text = "";
            labels.Name.SetName("");
            labels.Time.Text = "";
            labels.Damage.BackColor = labels.Damage.PresetColor;
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