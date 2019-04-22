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
using System.Drawing;
using System.Windows.Forms;

namespace KancolleSniffer.View
{
    public class ListScroller
    {
        private readonly Panel _panel;

        public int Padding { get; set; }

        public int Position { get; set; }

        public int Lines { get; set; }

        public int DataCount { get; set; }

        public event Action Update;

        public ListScroller(Panel panel, Label[] topLabels, Label[] bottomLabels)
        {
            _panel = panel;
            panel.Paint += (obj, ev) => DrawMark();
            SetTopEventHandler(topLabels);
            SetBottomEventHandler(bottomLabels);
        }

        public void SetTopEventHandler(Label[] top)
        {
            foreach (var label in top)
            {
                label.MouseEnter += TopLineOnMouseEnter;
                label.MouseLeave += TopLineOnMouseLeave;
            }
            _topScrollRepeatTimer.Tick += TopLineOnMouseEnter;
        }

        public void SetBottomEventHandler(Label[] bottom)
        {
            foreach (var label in bottom)
            {
                label.MouseEnter += BottomLineOnMouseEnter;
                label.MouseLeave += BottomLineOnMouseLeave;
            }
            _bottomScrollRepeatTimer.Tick += BottomLineOnMouseEnter;
        }

        private readonly Timer _topScrollRepeatTimer = new Timer {Interval = 100};
        private readonly Timer _bottomScrollRepeatTimer = new Timer {Interval = 100};

        private void TopLineOnMouseEnter(object sender, EventArgs e)
        {
            if (Position == 0)
                return;
            Position--;
            Update?.Invoke();
            _topScrollRepeatTimer.Start();
        }

        private void TopLineOnMouseLeave(object sender, EventArgs e)
        {
            _topScrollRepeatTimer.Stop();
        }

        private void BottomLineOnMouseEnter(object sender, EventArgs e)
        {
            if (Position + Lines >= DataCount)
                return;
            Position++;
            Update?.Invoke();
            _bottomScrollRepeatTimer.Start();
        }

        private void BottomLineOnMouseLeave(object sender, EventArgs e)
        {
            _bottomScrollRepeatTimer.Stop();
        }

        public void DrawMark()
        {
            using (var g = _panel.CreateGraphics())
            {
                var topBrush = Position > 0 ? Brushes.Black : new SolidBrush(_panel.BackColor);
                g.FillPolygon(topBrush,
                    new[]
                    {
                        new PointF(_panel.Width * 0.45f, Padding), new PointF(_panel.Width * 0.55f, Padding),
                        new PointF(_panel.Width * 0.5f, 0), new PointF(_panel.Width * 0.45f, Padding)
                    });
                var bottomBrush = Position + Lines < DataCount
                    ? Brushes.Black
                    : new SolidBrush(_panel.BackColor);
                g.FillPolygon(bottomBrush,
                    new[]
                    {
                        new PointF(_panel.Width * 0.45f, _panel.Height - Padding - 2),
                        new PointF(_panel.Width * 0.55f, _panel.Height - Padding - 2),
                        new PointF(_panel.Width * 0.5f, _panel.Height - 2),
                        new PointF(_panel.Width * 0.45f, _panel.Height - Padding - 2)
                    });
            }
        }
    }
}