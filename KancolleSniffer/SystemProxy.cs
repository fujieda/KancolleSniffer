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
using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Local

// ReSharper disable InconsistentNaming

namespace KancolleSniffer
{
    public class SystemProxy
    {
        private InternetPerConnOptionList orgList;
        private string _currentUrl;

        private void SaveSettings()
        {
            if (orgList.dwSize != 0)
                return;
            var opts = new[]
            {
                new InternetPerConnOption {dwOption = PerConnOption.INTERNET_PER_CONN_FLAGS},
                new InternetPerConnOption {dwOption = PerConnOption.INTERNET_PER_CONN_AUTOCONFIG_URL}
            };
            var list = new InternetPerConnOptionList
            {
                pOptions = MarshalOptions(opts),
                pszConnection = IntPtr.Zero,
                dwOptionCount = opts.Length,
                dwOptionError = 0
            };
            var listSize = list.dwSize = Marshal.SizeOf(list);
            if (InternetQueryOption(IntPtr.Zero, InternetOption.INTERNET_OPTION_PER_CONNECTION_OPTION,
                ref list, ref listSize))
            {
                orgList = list;
            }
        }

        public void SetAutoProxyUrl(string url)
        {
            SaveSettings();
            var flagValue = new InternetPerConnOptionValue {dwValue = (int)PerConnFlags.PROXY_TYPE_AUTO_PROXY_URL};
            var urlValue = new InternetPerConnOptionValue {pszValue = Marshal.StringToHGlobalAuto(url)};
            _currentUrl = url;
            var opts = new[]
            {
                new InternetPerConnOption {dwOption = PerConnOption.INTERNET_PER_CONN_FLAGS, Value = flagValue},
                new InternetPerConnOption {dwOption = PerConnOption.INTERNET_PER_CONN_AUTOCONFIG_URL, Value = urlValue}
            };
            var list = new InternetPerConnOptionList
            {
                pOptions = MarshalOptions(opts),
                pszConnection = IntPtr.Zero,
                dwOptionCount = opts.Length,
                dwOptionError = 0
            };
            var listSize = list.dwSize = Marshal.SizeOf(list);
            var listBuff = Marshal.AllocCoTaskMem(listSize);
            Marshal.StructureToPtr(list, listBuff, false);
            InternetSetOption(IntPtr.Zero, InternetOption.INTERNET_OPTION_PER_CONNECTION_OPTION, listBuff, listSize);
            Refresh();

            Marshal.FreeHGlobal(urlValue.pszValue);
            Marshal.FreeCoTaskMem(list.pOptions);
            Marshal.FreeCoTaskMem(listBuff);
        }

        public void RestoreSettings()
        {
            if (orgList.dwSize == 0)
                return;
            var size = Marshal.SizeOf(typeof(InternetPerConnOption));
            var urlOpt = (InternetPerConnOption)
                Marshal.PtrToStructure((IntPtr)((long)orgList.pOptions + size), typeof(InternetPerConnOption));
            var orgUrl = Marshal.PtrToStringAuto(urlOpt.Value.pszValue);
            if (orgUrl == _currentUrl) // The restoration was sikipped or failed at last time.
            {
                // Unselect the Use automatic configration script check box.
                var flagsOpt =
                    (InternetPerConnOption)Marshal.PtrToStructure(orgList.pOptions, typeof(InternetPerConnOption));
                flagsOpt.Value.dwValue &= ~(int)PerConnFlags.PROXY_TYPE_AUTO_PROXY_URL;
                Marshal.StructureToPtr(flagsOpt, orgList.pOptions, false);
            }
            var listSize = orgList.dwSize;
            var listBuff = Marshal.AllocCoTaskMem(listSize);
            Marshal.StructureToPtr(orgList, listBuff, false);
            InternetSetOption(IntPtr.Zero, InternetOption.INTERNET_OPTION_PER_CONNECTION_OPTION, listBuff, listSize);
            Refresh();

            Marshal.FreeCoTaskMem(listBuff);
            Marshal.FreeHGlobal(urlOpt.Value.pszValue);
            Marshal.FreeCoTaskMem(orgList.pOptions);
            orgList.dwSize = 0;
        }

        private IntPtr MarshalOptions(InternetPerConnOption[] opts)
        {
            var buff = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(InternetPerConnOption)) * opts.Length);
            var size = Marshal.SizeOf(typeof(InternetPerConnOption));
            for (var i = 0; i < opts.Length; i++)
            {
                var ptr = (IntPtr)((long)buff + (i * size));
                Marshal.StructureToPtr(opts[i], ptr, false);
            }
            return buff;
        }

        public static void Refresh()
        {
            InternetSetOption(IntPtr.Zero, InternetOption.INTERNET_OPTION_PROXY_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, InternetOption.INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }

        [DllImport("WinInet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool InternetQueryOption(IntPtr hInternet, InternetOption dwOption,
            ref InternetPerConnOptionList optionList, ref int lpdwBufferLength);

        [DllImport("WinInet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool InternetSetOption(IntPtr hInternet, InternetOption dwOption,
            IntPtr lpBuffer, int dwBufferLength);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct InternetPerConnOptionList
        {
            public int dwSize;
            public IntPtr pszConnection;
            public int dwOptionCount;
            public int dwOptionError;
            public IntPtr pOptions;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct InternetPerConnOption
        {
            public PerConnOption dwOption;
            public InternetPerConnOptionValue Value;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InternetPerConnOptionValue
        {
            [FieldOffset(0)] public int dwValue;
            [FieldOffset(0)] public IntPtr pszValue;
            [FieldOffset(0)] public System.Runtime.InteropServices.ComTypes.FILETIME ftValue;
        }

        private enum InternetOption : uint
        {
            INTERNET_OPTION_REFRESH = 0x00000025,
            INTERNET_OPTION_PER_CONNECTION_OPTION = 0x0000004B,
            INTERNET_OPTION_PROXY_SETTINGS_CHANGED = 0x0000005F
        }

        private enum PerConnOption
        {
            INTERNET_PER_CONN_FLAGS = 1,
            INTERNET_PER_CONN_PROXY_SERVER = 2,
            INTERNET_PER_CONN_PROXY_BYPASS = 3,
            INTERNET_PER_CONN_AUTOCONFIG_URL = 4,
            INTERNET_PER_CONN_AUTODISCOVERY_FLAGS = 5
        }

        [Flags]
        private enum PerConnFlags
        {
            PROXY_TYPE_DIRECT = 0x00000001,
            PROXY_TYPE_PROXY = 0x00000002,
            PROXY_TYPE_AUTO_PROXY_URL = 0x00000004,
            PROXY_TYPE_AUTO_DETECT = 0x00000008
        }
    }
}