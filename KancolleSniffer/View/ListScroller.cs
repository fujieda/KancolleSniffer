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
        private const int MarkWidth = 20;

        public int Padding { get; set; }

        public int Position { get; set; }

        public int Lines { get; set; }

        public int DataCount { get; set; }

        public event Action Update;

        public event Action StartScroll;

        public event Action EndScroll;

        public ListScroller(Panel panel, Control[] topLabels, Control[] bottomLabels)
        {
            _panel = panel;
            panel.Paint += (obj, ev) => DrawMark();
            SetTopEventHandler(topLabels);
            SetBottomEventHandler(bottomLabels);
        }

        public void SetTopEventHandler(Control[] top)
        {
            foreach (var label in top)
            {
                label.MouseEnter += TopLineOnMouseEnter;
                label.MouseLeave += TopLineOnMouseLeave;
            }
            _topScrollRepeatTimer.Tick += (obj, e) => ScrollUp();
        }

        public void SetBottomEventHandler(Control[] bottom)
        {
            foreach (var label in bottom)
            {
                label.MouseEnter += BottomLineOnMouseEnter;
                label.MouseLeave += BottomLineOnMouseLeave;
            }
            _bottomScrollRepeatTimer.Tick += (obj, e) => ScrollDown();
        }

        private readonly Timer _topScrollRepeatTimer = new Timer {Interval = 100};
        private readonly Timer _bottomScrollRepeatTimer = new Timer {Interval = 100};

        private void TopLineOnMouseEnter(object sender, EventArgs e)
        {
            if (IsTop)
                return;
            StartScroll?.Invoke();
            _topScrollRepeatTimer.Start();
        }

        private void ScrollUp()
        {
            if (IsTop)
                return;
            Position--;
            Update?.Invoke();
        }

        private bool IsTop => Position == 0;

        private void TopLineOnMouseLeave(object sender, EventArgs e)
        {
            EndScroll?.Invoke();
            _topScrollRepeatTimer.Stop();
        }

        private void BottomLineOnMouseEnter(object sender, EventArgs e)
        {
            if (!NeedScroll)
                return;
            StartScroll?.Invoke();
            _bottomScrollRepeatTimer.Start();
        }

        private void ScrollDown()
        {
            if (!NeedScroll)
                return;
            Position++;
            Update?.Invoke();
        }

        private bool NeedScroll => Position + Lines < DataCount;

        private void BottomLineOnMouseLeave(object sender, EventArgs e)
        {
            EndScroll?.Invoke();
            _bottomScrollRepeatTimer.Stop();
        }

        public void DrawMark()
        {
            var halfOfWidth = _panel.Width * 0.5f;
            var halfOfMark = Scaler.ScaleWidth(MarkWidth * 0.5f);
            var left = halfOfWidth - halfOfMark;
            var right = halfOfWidth + halfOfMark;
            var center = halfOfWidth;
            using (var g = _panel.CreateGraphics())
            {
                DrawTopMark(g, left, right, center);
                DrawBottomMark(g, left, right, center);
            }
        }

        private void DrawTopMark(Graphics g, float left, float right, float center)
        {
            var brush = Position > 0 ? Brushes.Black : new SolidBrush(_panel.BackColor);
            var top = -1;
            var base_ = Padding - 1;
            g.FillPolygon(brush,
                new[]
                {
                    new PointF(left, base_), new PointF(right, base_), new PointF(center, top),
                    new PointF(left, base_)
                });
        }

        private void DrawBottomMark(Graphics g, float left, float right, float center)
        {
            var brush = Position + Lines < DataCount ? Brushes.Black : new SolidBrush(_panel.BackColor);
            var top = _panel.Height - 2;
            var base_ = _panel.Height - Padding - 1;
            g.FillPolygon(brush,
                new[]
                {
                    new PointF(left, base_), new PointF(right, base_), new PointF(center, top),
                    new PointF(left, base_)
                });
        }
    }
}