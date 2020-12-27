﻿// Copyright (C) 2013, 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

using System.Windows.Forms;
using KancolleSniffer.Util;

namespace KancolleSniffer.Forms
{
    public partial class HorizontalMainForm : Form
    {
        public HorizontalMainForm()
        {
            InitializeComponent();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x11) // WM_QUERYENDSESSION
                MainWindow.SystemShutdown = true;
            SoundPlayer.CloseSound(m);
            base.WndProc(ref m);
        }
    }
}