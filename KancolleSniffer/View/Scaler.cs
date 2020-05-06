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
    public static class Scaler
    {
        public static SizeF Factor { private get; set; }

        public static void Scale(Control control)
        {
            control.Scale(Factor);
        }

        public static float ScaleWidth(float width)
        {
            return width * Factor.Width;
        }

        public static float ScaleHeight(float height)
        {
            return height * Factor.Height;
        }

        public static int ScaleWidth(int width)
        {
            return (int)Math.Round(width * Factor.Width);
        }

        public static int ScaleHeight(int height)
        {
            return (int)Math.Round(height * Factor.Height);
        }

        public static Size Scale(int width, int height)
        {
            return new Size(ScaleWidth(width), ScaleHeight(height));
        }

        public static SizeF Scale(float width, float height)
        {
            return new SizeF(ScaleWidth(width), ScaleHeight(height));
        }

        public static Point Move(int x, int y, int width, int height)
        {
            return new Point(x + ScaleWidth(width), y + ScaleHeight(height));
        }
    }
}