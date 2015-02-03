// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public partial class ConfigDialog : Form
    {
        private readonly Config _config;
        private readonly MainForm _main;
        private DebugDialog _debugDialog;
        private ProxyDialog _proxyDialog;
        private LogDialog _logDialog;
        private readonly Dictionary<string, string> _soundSetting = new Dictionary<string, string>();
        private const string Home = "http://kancollesniffer.sourceforge.jp/";

        public ConfigDialog(Config config, MainForm main)
        {
            InitializeComponent();
            _config = config;
            _main = main;
            listBoxSoundFile.Items.AddRange(new object[]
            {
                "遠征終了", "入渠終了", "建造完了", "艦娘数超過", "装備数超過",
                "大破警告", "泊地修理20分経過", "泊地修理進行", "疲労回復"
            });
        }

        private void ConfigDialog_Load(object sender, EventArgs e)
        {
            var version = string.Join(".", Application.ProductVersion.Split('.').Take(2));
            labelVersion.Text = "バージョン" + version;
            SetLatestVersion(version);

            checkBoxTopMost.Checked = _config.TopMost;
            checkBoxHideOnMinimized.Checked = _config.HideOnMinimized;
            checkBoxFlash.Checked = _config.FlashWindow;
            checkBoxBalloon.Checked = _config.ShowBaloonTip;
            checkBoxSound.Checked = _config.PlaySound;
            numericUpDownMarginShips.Value = _config.MarginShips;
            numericUpDownMarginEquips.Value = _config.MarginEquips;
            checkBoxCond40.Checked = _config.NotifyConditions.Contains(40);
            checkBoxCond49.Checked = _config.NotifyConditions.Contains(49);
            checkBoxReset02.Checked = _config.ResetHours.Contains(2);
            checkBoxReset14.Checked = _config.ResetHours.Contains(14);

            numericUpDownSoundVolume.Value = _config.SoundVolume;

            _soundSetting["遠征終了"] = _config.MissionSoundFile;
            _soundSetting["入渠終了"] = _config.NDockSoundFile;
            _soundSetting["建造完了"] = _config.KDockSoundFile;
            _soundSetting["艦娘数超過"] = _config.MaxShipsSoundFile;
            _soundSetting["装備数超過"] = _config.MaxEquipsSoundFile;
            _soundSetting["大破警告"] = _config.DamagedShipSoundFile;
            _soundSetting["泊地修理20分経過"] = _config.Akashi20MinSoundFile;
            _soundSetting["泊地修理進行"] = _config.AkashiProgressSoundFile;
            _soundSetting["疲労回復"] = _config.ConditionSoundFile;

            listBoxSoundFile.SelectedIndex = -1;
            listBoxSoundFile.SelectedIndex = 0;
        }

        private async void SetLatestVersion(string version)
        {
            try
            {
                var req = WebRequest.Create(Home + "version");
                var response = await req.GetResponseAsync();
                var stream = response.GetResponseStream();
                if (stream == null)
                    return;
                using (var reader = new StreamReader(stream))
                {
                    var str = await reader.ReadLineAsync();
                    Invoke(new Action(() => { labelLatest.Text = version == str ? "最新です" : "最新は" + str + "です"; }));
                }
            }
            catch (WebException)
            {
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            _config.TopMost = checkBoxTopMost.Checked;
            _config.HideOnMinimized = checkBoxHideOnMinimized.Checked;
            _config.FlashWindow = checkBoxFlash.Checked;
            _config.ShowBaloonTip = checkBoxBalloon.Checked;
            _config.PlaySound = checkBoxSound.Checked;
            _config.MarginShips = (int)numericUpDownMarginShips.Value;
            _config.MarginEquips = (int)numericUpDownMarginEquips.Value;

            _config.NotifyConditions.Clear();
            if (checkBoxCond40.Checked)
                _config.NotifyConditions.Add(40);
            if (checkBoxCond49.Checked)
                _config.NotifyConditions.Add(49);

            _config.ResetHours.Clear();
            if (checkBoxReset02.Checked)
                _config.ResetHours.Add(2);
            if (checkBoxReset14.Checked)
                _config.ResetHours.Add(14);

            _config.SoundVolume = (int)numericUpDownSoundVolume.Value;

            _config.MissionSoundFile = _soundSetting["遠征終了"];
            _config.NDockSoundFile = _soundSetting["入渠終了"];
            _config.KDockSoundFile = _soundSetting["建造完了"];
            _config.MaxShipsSoundFile = _soundSetting["艦娘数超過"];
            _config.MaxEquipsSoundFile = _soundSetting["装備数超過"];
            _config.DamagedShipSoundFile = _soundSetting["大破警告"];
            _config.Akashi20MinSoundFile = _soundSetting["泊地修理20分経過"];
            _config.AkashiProgressSoundFile = _soundSetting["泊地修理進行"];
            _config.ConditionSoundFile = _soundSetting["疲労回復"];
        }

        private void textBoxSoundFile_TextChanged(object sender, EventArgs e)
        {
            _soundSetting[(string)listBoxSoundFile.SelectedItem] = textBoxSoundFile.Text;
        }

        private void listBoxSoundFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxSoundFile.SelectedItem == null)
                return;
            textBoxSoundFile.Text = _soundSetting[(string)listBoxSoundFile.SelectedItem];
            textBoxSoundFile.Select(textBoxSoundFile.Text.Length, 0);
        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = textBoxSoundFile.Text;
            openFileDialog.InitialDirectory = Path.GetDirectoryName(textBoxSoundFile.Text) ?? "";
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            textBoxSoundFile.Text = openFileDialog.FileName;
            textBoxSoundFile.Select(textBoxSoundFile.Text.Length, 0);
        }


        private void buttonPlay_Click(object sender, EventArgs e)
        {
            _main.PlaySound(_soundSetting[(string)listBoxSoundFile.SelectedItem], (int)numericUpDownSoundVolume.Value);
        }

        private void DebugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_debugDialog == null)
                _debugDialog = new DebugDialog(_config, _main);
            if (_debugDialog.ShowDialog(this) == DialogResult.Abort)
                DialogResult = DialogResult.Cancel;
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

        private void ReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_logDialog == null)
                _logDialog = new LogDialog(_config.Log, _main);
            _logDialog.ShowDialog(this);
        }

        private void linkLabelProductName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabelProductName.LinkVisited = true;
            Process.Start(Home);
        }
    }
}