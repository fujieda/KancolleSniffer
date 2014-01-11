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
    public partial class ConfigDialog : Form
    {
        public ConfigDialog()
        {
            InitializeComponent();
        }

        private void ConfigDialog_Load(object sender, EventArgs e)
        {
            var config = (Config)Tag;

            checkBoxTopMost.Checked = config.TopMost;
            checkBoxFlash.Checked = config.FlashWindow;
            checkBoxBalloon.Checked = config.ShowBaloonTip;
            groupBoxSound.Enabled = checkBoxSound.Checked = config.PlaySound;
            numericUpDownMarginShips.Value = config.MarginShips;

            numericUpDownSoundVolume.Value = config.SoundVolume;
            textBoxMissionSoundFile.Text = config.MissionSoundFile;
            textBoxNDockSoundFile.Text = config.NDockSoundFile;
            textBoxKDockSoundFile.Text = config.KDockSoundFile;
            textBoxMaxShipsSoundFile.Text = config.MaxShipsSoundFile;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            var config = (Config)Tag;

            config.TopMost = checkBoxTopMost.Checked;
            config.FlashWindow = checkBoxFlash.Checked;
            config.ShowBaloonTip = checkBoxBalloon.Checked;
            config.PlaySound = checkBoxSound.Checked;
            config.MarginShips = (int)numericUpDownMarginShips.Value;

            config.SoundVolume = (int)numericUpDownSoundVolume.Value;
            config.MissionSoundFile = textBoxMissionSoundFile.Text;
            config.NDockSoundFile = textBoxNDockSoundFile.Text;
            config.KDockSoundFile = textBoxKDockSoundFile.Text;
            config.MaxShipsSoundFile = textBoxMaxShipsSoundFile.Text;
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

        private void ChooseSoundFile(TextBox textBox)
        {
            openFileDialog.FileName = textBox.Text;
            openFileDialog.InitialDirectory = Path.GetDirectoryName(textBox.Text) ?? "";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                textBox.Text = openFileDialog.FileName;
        }
    }
}