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
using System.Runtime.InteropServices;
using Microsoft.Win32;

// ReSharper disable InconsistentNaming

namespace KancolleSniffer
{
    public static class SystemProxy
    {
        private const string _regPath = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
        private const string _regName = "AutoConfigURL";

        public static string AutoConfigUrl
        {
            get
            {
                using (var regkey = Registry.CurrentUser.OpenSubKey(_regPath))
                {
                    if (regkey == null)
                        return "";
                    return regkey.GetValue(_regName) as string ?? "";
                }
            }
            set
            {
                using (var regkey = Registry.CurrentUser.OpenSubKey(_regPath, true))
                {
                    if (regkey == null)
                        return;
                    if (value == "")
                    {
                        regkey.DeleteValue(_regName, false);
                    }
                    else
                    {
                        regkey.SetValue(_regName, value);
                    }
                }
                Refresh();
            }
        }

        /// <summary>
        /// PACファイルでDIRECTを指定すると、すべてのサイトがローカルイントラネットになり、
        /// IEが互換表示になるなどの不具合があるので、イントラネットにならないようにする
        /// </summary>
        public static void AdjustLocalIntranetZoneFlags()
        {
            var zones = Registry.CurrentUser.OpenSubKey(_regPath + @"\Zones\1", true);
            if (zones == null)
                return;
            if (!(zones.GetValue("Flags") is int flags))
                return;
            zones.SetValue("Flags", flags & (-1 ^ 0x108));
        }

        public static void Refresh()
        {
            InternetSetOption(IntPtr.Zero, InternetOption.INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, InternetOption.INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }

        [DllImport("WinInet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool InternetSetOption(IntPtr hInternet, InternetOption dwOption,
            IntPtr lpBuffer, int dwBufferLength);

        private enum InternetOption : uint
        {
            INTERNET_OPTION_REFRESH = 37,
            INTERNET_OPTION_SETTINGS_CHANGED = 39
        }
    }
}