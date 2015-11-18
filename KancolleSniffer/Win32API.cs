// Copyright (C) 2014 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KancolleSniffer
{
    public class Win32API
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        public static bool ProcessAlreadyExists()
        {
            try
            {
                var cur = Process.GetCurrentProcess();
                var all = Process.GetProcessesByName(cur.ProcessName);
                foreach (var p in all)
                {
                    if (cur.Id == p.Id)
                        continue;
                    if (p.MainModule.FileName != cur.MainModule.FileName)
                        continue;
                    if (IsIconic(p.MainWindowHandle))
                        ShowWindowAsync(p.MainWindowHandle, 9); // SW_RESTORE
                    else
                        SetForegroundWindow(p.MainWindowHandle);
                    return true;
                }
            }
            /*
             * マルウェア対策ソフトが原因でMainModule.FileNameが失敗することがあり、
             * その場合はWin32Exceptionが発生する。
            */
            catch (Win32Exception)
            {
            }
            return false;
        }

        [DllImport("user32.dll")]
        private static extern Int32 FlashWindowEx(ref FLASHWINFO pwfi);

        public static void FlashWindow(IntPtr handle)
        {
            var info = new FLASHWINFO();
            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            info.hwnd = handle;
            info.dwFlags = 3; // FLASHW_ALL
            info.uCount = 3;
            info.dwTimeout = 0;
            FlashWindowEx(ref info);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }
    }
}