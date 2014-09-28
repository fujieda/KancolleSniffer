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
using System.Linq;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public partial class ConfigDialog : Form
    {
        private readonly Config _config;
        private readonly MainForm _main;
        private DebugDialog _debugDialog;
        private ProxyDialog _proxyDialog;

        public ConfigDialog(Config config, MainForm main)
        {
            InitializeComponent();
            _config = config;
            _main = main;
        }

        private void ConfigDialog_Load(object sender, EventArgs e)
        {
            checkBoxTopMost.Checked = _config.TopMost;
            checkBoxFlash.Checked = _config.FlashWindow;
            checkBoxBalloon.Checked = _config.ShowBaloonTip;
            groupBoxSound.Enabled = checkBoxSound.Checked = _config.PlaySound;
            numericUpDownMarginShips.Value = _config.MarginShips;

            checkBoxReset02.Checked = _config.ResetHours.Any(x => x == 2);
            checkBoxReset14.Checked = _config.ResetHours.Any(x => x == 14);

            numericUpDownSoundVolume.Value = _config.SoundVolume;
            textBoxMissionSoundFile.Text = _config.MissionSoundFile;
            textBoxNDockSoundFile.Text = _config.NDockSoundFile;
            textBoxKDockSoundFile.Text = _config.KDockSoundFile;
            textBoxMaxShipsSoundFile.Text = _config.MaxShipsSoundFile;
            textBoxDamagedShipSoundFile.Text = _config.DamagedShipSoundFile;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            _config.TopMost = checkBoxTopMost.Checked;
            _config.FlashWindow = checkBoxFlash.Checked;
            _config.ShowBaloonTip = checkBoxBalloon.Checked;
            _config.PlaySound = checkBoxSound.Checked;
            _config.MarginShips = (int)numericUpDownMarginShips.Value;

            _config.ResetHours.Clear();
            if (checkBoxReset02.Checked)
                _config.ResetHours.Add(2);
            if (checkBoxReset14.Checked)
                _config.ResetHours.Add(14);

            _config.SoundVolume = (int)numericUpDownSoundVolume.Value;
            _config.MissionSoundFile = textBoxMissionSoundFile.Text;
            _config.NDockSoundFile = textBoxNDockSoundFile.Text;
            _config.KDockSoundFile = textBoxKDockSoundFile.Text;
            _config.MaxShipsSoundFile = textBoxMaxShipsSoundFile.Text;
            _config.DamagedShipSoundFile = textBoxDamagedShipSoundFile.Text;
        }

        private void checkBoxSound_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxSound.Enabled = checkBoxSound.Checked;
        }

        private void buttonMissionOpenFile_Click(object sender, EventArgs e)
        {
            ChooseSoundFile(textBoxMissionSoundFile);
        }

        private void buttonNDockOpenFile_Click(object sender, EventArgs e)
        {
            ChooseSoundFile(textBoxNDockSoundFile);
        }

        private void buttonKDockOpenFile_Click(object sender, EventArgs e)
        {
            ChooseSoundFile(textBoxKDockSoundFile);
        }

        private void buttonMaxShipsOpenFile_Click(object sender, EventArgs e)
        {
            ChooseSoundFile(textBoxMaxShipsSoundFile);
        }

        private void buttonDamagedShipOpenFile_Click(object sender, EventArgs e)
        {
            ChooseSoundFile(textBoxDamagedShipSoundFile);
        }

        private void ChooseSoundFile(TextBox textBox)
        {
            openFileDialog.FileName = textBox.Text;
            openFileDialog.InitialDirectory = Path.GetDirectoryName(textBox.Text) ?? "";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                textBox.Text = openFileDialog.FileName;
        }

        private void DebugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_debugDialog == null)
                _debugDialog = new DebugDialog(_config, _main);
            _debugDialog.ShowDialog(this);
        }

        private void buttonResetAchievement_Click(object sender, EventArgs e)
        {
            _main.ResetAchievemnt();
        }

        private void ProxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_proxyDialog == null)
                _proxyDialog = new ProxyDialog(_config.Proxy, _main);
            _proxyDialog.ShowDialog(this);
        }
    }
}