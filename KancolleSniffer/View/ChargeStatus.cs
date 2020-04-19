// Copyright (C) 2020 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

using System.Drawing;
using System.Windows.Forms;

namespace KancolleSniffer.View
{
    public class ChargeStatus : Control, IUpdateContext
    {
        private readonly Color[] _colors = {CUDColors.Yellow, CUDColors.Orange, CUDColors.Red, CUDColors.LightGray};

        private float IconWidth => Width * 8 / 17.0f;

        private float Head => Height * 8 / 13f;

        private float BulletLeft => Width * 9 / 17.0f;

        private readonly Brush _defaultBrush = new SolidBrush(DefaultBackColor);

        private Graphics _g;

        private Model.ChargeStatus _status;

        public UpdateContext Context { get; set; }

        public override string Text { get; set; } = "";

        public new void Update()
        {
            var fleet = (int)Tag;
            _status = Context.Sniffer.Fleets[fleet].ChargeStatus;
            Text = _status.Empty ? "" : $"燃{_status.FuelRatio * 100:f1}% 弾{_status.BullRatio * 100:f1}%";
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_status == null)
                return;
            _g = e.Graphics;
            DrawFuelStatus();
            DrawBulletStatus();
        }

        private void DrawFuelStatus()
        {
            var charge = _status.Fuel;
            if (charge % 5 == 0)
            {
                Clear(0);
                return;
            }
            DrawFuelHead(charge);
            DrawStatus(0, charge % 5 - 1);
        }

        private void DrawFuelHead(int charge)
        {
            _g.FillPolygon(
                new SolidBrush(charge <= 4 ? Color.FromArgb(83, 131, 52) : Color.FromArgb(178, 196, 165)),
                new[]
                {
                    new PointF(0, 0), new PointF(IconWidth, 0), new PointF(IconWidth, Height), new Point(0, Height)
                });
        }

        private void DrawBulletStatus()
        {
            var charge = _status.Bull;
            if (charge % 5 == 0)
            {
                Clear(BulletLeft);
                return;
            }
            DrawBulletHead(charge);
            DrawStatus(BulletLeft, charge % 5 - 1);
        }

        private void DrawBulletHead(int charge)
        {
            var l = BulletLeft;
            var r = l + IconWidth;
            var curve = Height * 6 / 13f;
            var brush = new SolidBrush(charge <= 4 ? Color.FromArgb(153, 101, 0) : Color.FromArgb(205, 185, 144));
            _g.FillClosedCurve(brush,
                new[] {new PointF(l, curve), new PointF(l + IconWidth / 2.0f, -1), new PointF(r, curve)});
            _g.FillPolygon(brush,
                new[]
                {
                    new PointF(l, curve), new PointF(r, curve), new PointF(r, Height), new PointF(l, Height)
                });
        }

        private void DrawStatus(float left, int color)
        {
            var right = left + IconWidth;
            _g.FillPolygon(new SolidBrush(_colors[color]),
                new[]
                {
                    new PointF(left, Head), new PointF(right, Head), new PointF(right, Height), new PointF(left, Height)
                });
        }

        private void Clear(float left)
        {
            _g.FillRectangle(_defaultBrush, left, 0, left + IconWidth, Height);
        }
    }
}