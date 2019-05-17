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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Math;

namespace KancolleSniffer.View
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

        public void AddShipListPanel(ShipListPanel.ShipListPanel panel)
        {
            var handler = new ShipListPanelHandler(panel);
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
            public delegate void MouseHandler(IntPtr handle, ref bool handled);

            public event MouseHandler MouseMove, MouseDown, MouseUp;

            // ReSharper disable InconsistentNaming
            // ReSharper disable IdentifierTypo
            private const int WM_MOUSEMOVE = 0x0200;
            private const int WM_LBUTTONDOWN = 0x0201;

            private const int WM_LBUTTONUP = 0x0202;
            // ReSharper restore IdentifierTypo
            // ReSharper restore InconsistentNaming

            public bool PreFilterMessage(ref Message m)
            {
                var handled = false;
                switch (m.Msg)
                {
                    case WM_LBUTTONDOWN:
                        MouseDown?.Invoke(m.HWnd, ref handled);
                        break;
                    case WM_MOUSEMOVE:
                        MouseMove?.Invoke(m.HWnd, ref handled);
                        break;
                    case WM_LBUTTONUP:
                        MouseUp?.Invoke(m.HWnd, ref handled);
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
            private Point _scrollStart;
            private const int ScrollCount = 6;

            public PanelHandler(Panel panel)
            {
                _panel = panel;
            }

            public void MouseDown(IntPtr handle, ref bool handled)
            {
                if (!_mouseStart.IsEmpty)
                    return;
                if (!_panel.RectangleToScreen(_panel.ClientRectangle).Contains(Control.MousePosition))
                    return;
                var found = false;
                for (var control = Control.FromHandle(handle); control != null; control = control.Parent)
                {
                    if (control != _panel)
                        continue;
                    found = true;
                    break;
                }
                if (!found)
                    return;
                _mouseStart = _scrollStart = Control.MousePosition;
                _panelStart = _panel.AutoScrollPosition;
            }

            public void MouseMove(IntPtr handle, ref bool handled)
            {
                if (_mouseStart.IsEmpty)
                    return;
                var cur = Control.MousePosition;
                var dx = cur.X - _mouseStart.X;
                var dy = cur.Y - _mouseStart.Y;
                if (!_touch)
                {
                    if (!(Abs(dx) > ScrollCount || Abs(dy) > ScrollCount))
                        return;
                    _touch = true;
                }
                if (Abs(_scrollStart.X - cur.X) > ScrollCount || Abs(_scrollStart.Y - cur.Y) > ScrollCount)
                {
                    _panel.AutoScrollPosition = new Point(-_panelStart.X - dx, -_panelStart.Y - dy);
                    _scrollStart = cur;
                }
            }

            public void MouseUp(IntPtr handle, ref bool handled)
            {
                _touch = false;
                _mouseStart = _panelStart = Point.Empty;
            }
        }

        private class ShipListPanelHandler
        {
            private readonly ShipListPanel.ShipListPanel _panel;
            private bool _touch;
            private Point _mouseStart;
            private int _barStart = -1;
            private Point _scrollStart;
            private const int ScrollCount = ShipListPanel.ShipListPanel.LineHeight;

            public ShipListPanelHandler(ShipListPanel.ShipListPanel panel)
            {
                _panel = panel;
            }

            public void MouseDown(IntPtr handle, ref bool handled)
            {
                if (!_panel.ScrollBar.Visible)
                    return;
                if (!_mouseStart.IsEmpty)
                    return;
                if (!_panel.RectangleToScreen(_panel.ClientRectangle).Contains(Control.MousePosition) ||
                    _panel.ScrollBar.RectangleToScreen(_panel.ScrollBar.ClientRectangle)
                        .Contains(Control.MousePosition))
                    return;
                var found = false;
                for (var control = Control.FromHandle(handle); control != null; control = control.Parent)
                {
                    if (control != _panel)
                        continue;
                    found = true;
                    break;
                }
                if (!found)
                    return;
                _mouseStart = _scrollStart = Control.MousePosition;
                _barStart = _panel.ScrollBar.Value;
            }

            public void MouseMove(IntPtr handle, ref bool handled)
            {
                if (_mouseStart.IsEmpty)
                    return;
                var cur = Control.MousePosition;
                var dy = cur.Y - _mouseStart.Y;
                if (!_touch)
                {
                    if (Abs(dy) <= ScrollCount)
                        return;
                    _touch = true;
                }
                if (Abs(_scrollStart.Y - cur.Y) > ScrollCount)
                {
                    var bar = _panel.ScrollBar;
                    bar.Value = Max(0, Min(bar.Maximum - bar.LargeChange + 1, _barStart - dy / ScrollCount));
                    _scrollStart = cur;
                }
            }

            public void MouseUp(IntPtr handle, ref bool handled)
            {
                _touch = false;
                _barStart = -1;
                _mouseStart = Point.Empty;
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
            // ReSharper disable IdentifierTypo
            private const int GWL_STYLE = -16;
            private const int WS_HSCROLL = 0x00100000;

            private const int WS_VSCROLL = 0x00200000;
            // ReSharper restore IdentifierTypo
            // ReSharper restore InconsistentNaming

            [DllImport("user32.dll")]
            private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            public TreeViewHandler(TreeView treeView)
            {
                _treeView = treeView;
            }

            public void MouseDown(IntPtr handle, ref bool handled)
            {
                if (!_mouseStart.IsEmpty)
                    return;
                var control = Control.FromHandle(handle);
                if (control == null || control != _treeView)
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

            public void MouseMove(IntPtr handle, ref bool handled)
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

            public void MouseUp(IntPtr handle, ref bool handled)
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