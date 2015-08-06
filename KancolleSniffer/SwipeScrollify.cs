// Copyright (C) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
//
// This program is part of KancolleSniffer.
//
// KancolleSniffer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, see <http://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Math;

namespace KancolleSniffer
{
    public class SwipeScrollify
    {
        private readonly MouseFilter _filter;

        public SwipeScrollify()
        {
            _filter = new MouseFilter();
            Application.AddMessageFilter(_filter);
        }

        public void AddPanel(Panel panel)
        {
            var handler = new PanelHandler(panel);
            _filter.MouseDown += handler.MouseDown;
            _filter.MouseMove += handler.MouseMove;
            _filter.MouseUp += handler.MouseUp;
        }

        public void AddTreeView(TreeView treeView)
        {
            var handler = new TreeViewHandler(treeView);
            _filter.MouseDown += handler.MouseDown;
            _filter.MouseMove += handler.MouseMove;
            _filter.MouseUp += handler.MouseUp;
        }

        private class MouseFilter : IMessageFilter
        {
            public delegate void MouseHandler(ref bool handled);

            public event MouseHandler MouseMove , MouseDown , MouseUp;

            // ReSharper disable InconsistentNaming
            private const int WM_MOUSEMOVE = 0x0200;
            private const int WM_LBUTTONDOWN = 0x0201;
            private const int WM_LBUTTONUP = 0x0202;
            // ReSharper restore InconsistentNaming

            public bool PreFilterMessage(ref Message m)
            {
                var handled = false;
                switch (m.Msg)
                {
                    case WM_LBUTTONDOWN:
                        MouseDown?.Invoke(ref handled);
                        break;
                    case WM_MOUSEMOVE:
                        MouseMove?.Invoke(ref handled);
                        break;
                    case WM_LBUTTONUP:
                        MouseUp?.Invoke(ref handled);
                        break;
                }
                return handled;
            }
        }

        private class PanelHandler
        {
            private readonly Panel _panel;
            private bool _touch;
            private Point _mouseStart;
            private Point _panelStart;

            public PanelHandler(Panel panel)
            {
                _panel = panel;
            }

            public void MouseDown(ref bool handled)
            {
                if (!_mouseStart.IsEmpty)
                    return;
                if (!_panel.RectangleToScreen(_panel.ClientRectangle).Contains(Control.MousePosition))
                    return;
                _mouseStart = Control.MousePosition;
                _panelStart = _panel.AutoScrollPosition;
            }

            public void MouseMove(ref bool handled)
            {
                if (_mouseStart.IsEmpty)
                    return;
                var cur = Control.MousePosition;
                var dx = cur.X - _mouseStart.X;
                var dy = cur.Y - _mouseStart.Y;
                if (_touch)
                    _panel.AutoScrollPosition = new Point(-_panelStart.X - dx, -_panelStart.Y - dy);
                else if (Abs(dx) > 5 || Abs(dy) > 5)
                    _touch = true;
            }

            public void MouseUp(ref bool handled)
            {
                if (_touch && !_panelStart.IsEmpty && _panelStart != _panel.AutoScrollPosition)
                    handled = true;
                _touch = false;
                _mouseStart = _panelStart = Point.Empty;
            }
        }

        private class TreeViewHandler
        {
            private readonly TreeView _treeView;
            private bool _touch;
            private Point _mouseStart;
            private Point _panelStart;

            [DllImport("user32.dll")]
            private static extern int GetScrollPos(IntPtr hWnd, int nBar);

            [DllImport("user32.dll")]
            private static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

            // ReSharper disable InconsistentNaming
            private const int GWL_STYLE = -16;
            private const int WS_HSCROLL = 0x00100000;
            private const int WS_VSCROLL = 0x00200000;
            // ReSharper restore InconsistentNaming

            [DllImport("user32.dll")]
            private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            public TreeViewHandler(TreeView treeView)
            {
                _treeView = treeView;
            }

            public void MouseDown(ref bool handled)
            {
                if (!_mouseStart.IsEmpty)
                    return;
                if (!_treeView.RectangleToScreen(_treeView.ClientRectangle).Contains(Control.MousePosition))
                    return;
                var loc = _treeView.HitTest(_treeView.PointToClient(Control.MousePosition)).Location;
                // アイテムをクリックするとWM_LBUTTONUPが来ないので避ける
                if (loc == TreeViewHitTestLocations.Label || loc == TreeViewHitTestLocations.Image)
                    return;
                _mouseStart = Control.MousePosition;
                _panelStart = ScrollPosition();
            }

            public void MouseMove(ref bool handled)
            {
                if (_mouseStart.IsEmpty)
                    return;
                if (!_treeView.RectangleToScreen(_treeView.ClientRectangle).Contains(Control.MousePosition))
                {
                    // TreeViewではうまく動かないので外に出たら止める
                    _touch = false;
                    _mouseStart = _panelStart = Point.Empty;
                    return;
                }
                var cur = Control.MousePosition;
                var dx = cur.X - _mouseStart.X;
                var dy = cur.Y - _mouseStart.Y;
                var style = GetWindowLong(_treeView.Handle, GWL_STYLE);
                if (_touch)
                {
                    _treeView.BeginUpdate();
                    if ((style & WS_HSCROLL) != 0)
                        SetScrollPos(_treeView.Handle, 0, _panelStart.X - dx, true);
                    if ((style & WS_VSCROLL) != 0)
                        SetScrollPos(_treeView.Handle, 1, _panelStart.Y - dy / _treeView.ItemHeight, true);
                    _treeView.EndUpdate();
                    handled = true;
                }
                else if (Abs(dx) > 5 || Abs(dy) > 5)
                    _touch = true;
            }

            public void MouseUp(ref bool handled)
            {
                if (_touch && !_panelStart.IsEmpty && _panelStart != ScrollPosition())
                    handled = true;
                _touch = false;
                _mouseStart = _panelStart = Point.Empty;
            }

            private Point ScrollPosition()
            {
                return new Point(GetScrollPos(_treeView.Handle, 0), GetScrollPos(_treeView.Handle, 1));
            }
        }
    }
}