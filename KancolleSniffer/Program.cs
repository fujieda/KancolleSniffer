// Copyright (C) 2013 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KancolleSniffer
{
    internal static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (Win32API.ProcessAlreadyExists())
                return;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    /// <summary>
    /// Win32APIを実行するクラス
    /// </summary>
    public class Win32API
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        /// <summary>
        /// 同じアプリケーションがすでに起動しているか調べる。起動していたら最前面に表示する。
        /// </summary>
        /// <returns>起動していたらtrue</returns>
        public static bool ProcessAlreadyExists()
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
            return false;
        }
    }
}