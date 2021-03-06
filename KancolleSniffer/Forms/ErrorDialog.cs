﻿// Copyright (C) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Windows.Forms;
using Clipboard = KancolleSniffer.Util.Clipboard;

namespace KancolleSniffer.Forms
{
    public partial class ErrorDialog : Form
    {
        public ErrorDialog()
        {
            InitializeComponent();

            var icon = new Icon(SystemIcons.Error, 32, 32);
            labelSystemIcon.Image = icon.ToBitmap();
        }

        public DialogResult ShowDialog(IWin32Window owner, string message, string details)
        {
            if (Visible || checkBoxDisable.Checked)
                return DialogResult.Ignore;
            labelMessage.Text = message;
            textBoxDetails.Text = details;
            return ShowDialog(owner);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            textBoxDetails.Font = new Font(new FontFamily("MS Gothic"), Font.Size);
        }

        private void buttonCopyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBoxDetails.Text);
        }
    }
}