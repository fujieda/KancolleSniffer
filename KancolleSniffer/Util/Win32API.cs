// Copyright (C) 2014 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KancolleSniffer.Util
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