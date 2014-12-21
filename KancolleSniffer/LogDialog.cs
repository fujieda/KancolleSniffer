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
using System.Windows.Forms;

namespace KancolleSniffer
{
    public partial class LogDialog : Form
    {
        private readonly LogConfig _config;
        private readonly MainForm _main;

        public LogDialog(LogConfig config, MainForm main)
        {
            InitializeComponent();
            numericUpDownMaterialLogInterval.Maximum = 1440;
            _config = config;
            _main = main;
        }

        private void LogDialog_Load(object sender, EventArgs e)
        {
            checkBoxOutput.Checked = _config.On;
            textBoxOutput.Text = _config.OutputDir;
            textBoxOutput.Select(textBoxOutput.Text.Length, 0);
            folderBrowserDialogOutputDir.SelectedPath = _config.OutputDir;
            numericUpDownMaterialLogInterval.Value = _config.MaterialLogInterval;
            (_config.ServerOn ? radioButtonServerOn : radioButtonServerOff).PerformClick();
            textBoxListen.Text = _config.Listen.ToString("D");
        }

        private void radioButtonServerOn_CheckedChanged(object sender, EventArgs e)
        {
            var on = ((RadioButton)sender).Checked;
            textBoxListen.Enabled = on;
            labelListen.Enabled = on;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            var listen = -1;
            if (radioButtonServerOn.Checked && !ValidatePortNumber(textBoxListen, out listen))
                return;
            _config.ServerOn = radioButtonServerOn.Checked;
            if (_config.ServerOn)
                _config.Listen = listen;
            _config.MaterialLogInterval = (int)numericUpDownMaterialLogInterval.Value;
            _config.OutputDir = textBoxOutput.Text;
            _main.ApplyLogSetting();
            DialogResult = DialogResult.OK;
        }

        private bool ValidatePortNumber(TextBox textBox, out int result)
        {
            var s = textBox.Text;
            if (!int.TryParse(s, out result))
            {
                ShowToolTip("数字を入力してください。", textBox);
                return false;
            }
            if (result <= 0)
            {
                ShowToolTip("0より大きい数字を入力してください。", textBox);
                return false;
            }
            return true;
        }

        private void ShowToolTip(string message, Control control)
        {
            toolTipError.Show(message, control, 0, control.Height, 3000);
        }

        private void buttonOutputDir_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialogOutputDir.ShowDialog(this) == DialogResult.OK)
                textBoxOutput.Text = folderBrowserDialogOutputDir.SelectedPath;
            textBoxOutput.Select(textBoxOutput.Text.Length, 0);
        }
    }
}
