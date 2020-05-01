// Copyright (C) 2013, 2014 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Windows.Forms;
using KancolleSniffer.Util;

namespace KancolleSniffer
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            if (Win32API.ProcessAlreadyExists())
                return;
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                KancolleSniffer.Main.Run();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException;
                MessageBox.Show(ex.Message +
                                (inner == null
                                    ? ""
                                    : "\r\n" + inner.Message +
                                      (inner.InnerException == null
                                          ? ""
                                          : "\r\n" + inner.InnerException.Message))
                                + "\r\n" + ex.StackTrace,
                    "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}