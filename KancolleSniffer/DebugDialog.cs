// Copyright (C) 2014 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
using System.IO;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public partial class DebugDialog : Form
    {
        private readonly Config _config;
        private readonly MainForm _main;

        public DebugDialog(Config config, MainForm main)
        {
            InitializeComponent();
            _config = config;
            _main = main;
        }

        private void DebugDialog_Load(object sender, EventArgs e)
        {
            checkBoxLogging.Checked = _config.DebugLogging;
            textBoxLogFile.Text = _config.DebugLogFile;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            _config.DebugLogging = checkBoxLogging.Checked;
            _config.DebugLogFile = textBoxLogFile.Text;
            _main.ApplyDebugLogSetting();
        }

        private void buttonLogFileOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = textBoxLogFile.Text;
            openFileDialog.InitialDirectory = Path.GetDirectoryName(textBoxLogFile.Text);
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                textBoxLogFile.Text = openFileDialog.FileName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _main.SetPlayLog(textBoxLogFile.Text);
            DialogResult = DialogResult.Abort;
        }
    }

}
