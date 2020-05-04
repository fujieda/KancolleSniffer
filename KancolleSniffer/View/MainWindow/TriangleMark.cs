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

namespace KancolleSniffer.View.MainWindow
{
    public class TriangleMark : Control
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillPolygon(Brushes.Black,
                new[] {new PointF(0, 0), new PointF(Width, Height / 2.0f), new PointF(0, Height)});
        }
    }
}