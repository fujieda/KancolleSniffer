// Copyright (C) 2018 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KancolleSniffer.View
{
    public class ResizableToolTip : ToolTip
    {
        private const TextFormatFlags TfFlags =
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix;

        private readonly Brush _backBrush;
        private Size _padding;
        private int _iconSize;
        private int _iconPadding;

        public Font Font { get; set; }

        public ResizableToolTip()
        {
            OwnerDraw = true;
            Font = SystemFonts.StatusFont;
            Popup += OnPopup;
            Draw += OnDraw;
            DwmIsCompositionEnabled(out var aero);
            _backBrush = new SolidBrush(aero ? Color.White : BackColor);
            ShowAlways = true;
            AutoPopDelay = 30000;
        }

        private void OnPopup(object sender, PopupEventArgs e)
        {
            if (ToolTipIcon == ToolTipIcon.Error)
            {
                _iconSize = 16;
                _iconPadding = (int)Math.Round((Font.Height - _iconSize) / 2.0);
            }
            _padding = new Size((int)Math.Round(Font.Height * 0.2), (int)Math.Round(Font.Height * 0.15));
            using var g = Graphics.FromHwnd(e.AssociatedWindow.Handle);
            var size = TextRenderer.MeasureText(g, GetToolTip(e.AssociatedControl), Font,
                new Size(int.MaxValue, int.MaxValue), TfFlags);
            e.ToolTipSize = new Size(size.Width + _padding.Width * 2 + _iconSize,
                size.Height + _padding.Height * 2);
        }

        private void OnDraw(object sender, DrawToolTipEventArgs e)
        {
            e.Graphics.FillRectangle(_backBrush, e.Bounds);
            e.Graphics.DrawRectangle(SystemPens.ControlDarkDark,
                new Rectangle(e.Bounds.Location, new Size(e.Bounds.Width - 1, e.Bounds.Height - 1)));
            TextRenderer.DrawText(e.Graphics, e.ToolTipText, Font,
                new Rectangle(e.Bounds.X + _padding.Width + _iconSize,
                    e.Bounds.Y + _padding.Height, e.Bounds.Width - _padding.Width * 2,
                    e.Bounds.Height - _padding.Height * 2),
                Color.Black, TfFlags);
            if (ToolTipIcon != ToolTipIcon.Error)
                return;
            e.Graphics.DrawIcon(SystemIcons.Error,
                new Rectangle(e.Bounds.X + _padding.Width + _iconPadding,
                    e.Bounds.Y + _padding.Height + _iconPadding, _iconSize, _iconSize));
        }

        [DllImport("dwmapi.dll")]
        private static extern IntPtr DwmIsCompositionEnabled(out bool pfEnabled);
    }
}