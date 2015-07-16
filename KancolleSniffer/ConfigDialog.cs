// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
        private readonly Dictionary<string, string> _soundSetting = new Dictionary<string, string>();
        private const string Home = "http://kancollesniffer.osdn.jp/";

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
            numericUpDownMaterialLogInterval.Maximum = 1440;
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
            radioButtonResultRankAlways.Checked = _config.AlwaysShowResultRank;
            radioButtonResultRankWhenClick.Checked = !_config.AlwaysShowResultRank;

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

            LoadProxySettings();
            LoadLogSettings();
            LoadDebugSettings();
        }

        private void LoadProxySettings()
        {
            // 見えていないTabPage上でPerformClickは使えない。
            radioButtonAutoConfigOn.Checked = _config.Proxy.Auto;
            radioButtonAutoConfigOff.Checked = !_config.Proxy.Auto;
            textBoxListen.Text = _config.Proxy.Listen.ToString("D");
            radioButtonUpstreamOn.Checked = _config.Proxy.UseUpstream;
            radioButtonUpstreamOff.Checked = !_config.Proxy.UseUpstream;
            textBoxPort.Text = _config.Proxy.UpstreamPort.ToString("D");
        }

        private void LoadLogSettings()
        {
            checkBoxOutput.Checked = _config.Log.On;
            textBoxOutput.Text = _config.Log.OutputDir;
            textBoxOutput.Select(textBoxOutput.Text.Length, 0);
            folderBrowserDialogOutputDir.SelectedPath = _config.Log.OutputDir;
            numericUpDownMaterialLogInterval.Value = _config.Log.MaterialLogInterval;
            radioButtonServerOn.Checked = _config.Log.ServerOn;
            radioButtonServerOff.Checked = !_config.Log.ServerOn;
            textBoxServer.Text = _config.Log.Listen.ToString("D");
        }

        private void LoadDebugSettings()
        {
            checkBoxDebugLog.Checked = _config.DebugLogging;
            textBoxDebugLog.Text = _config.DebugLogFile;
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
            int listen, outbound, server;
            if (!ValidatePorts(out listen, out outbound, out server))
                return;
            DialogResult = DialogResult.OK;

            ApplyProxySettings(listen, outbound);
            ApplyLogSettings(server);
            ApplyDebugSettings();

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

            _config.AlwaysShowResultRank = radioButtonResultRankAlways.Checked;

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

        private bool ValidatePorts(out int listen, out int outbound, out int server)
        {
            outbound = -1;
            server = -1;
            if (!ValidatePortNumber(textBoxListen, out listen))
                return false;
            if (radioButtonUpstreamOn.Checked && !ValidatePortNumber(textBoxPort, out outbound))
                return false;
            if (radioButtonServerOn.Checked && !ValidatePortNumber(textBoxServer, out server))
                return false;
            if (radioButtonUpstreamOn.Checked && listen == outbound)
            {
                ShowToolTip("受信と送信に同じポートは使えません。", textBoxPort);
                return false;
            }
            if (radioButtonServerOn.Checked && server == listen)
            {
                ShowToolTip("プロキシの受信ポートと同じポートは使えません。", textBoxServer);
                return false;
            }
            return true;
        }

        private void ApplyProxySettings(int listen, int port)
        {
            _config.Proxy.Auto = radioButtonAutoConfigOn.Checked;
            _config.Proxy.Listen = listen;
            _config.Proxy.UseUpstream = radioButtonUpstreamOn.Checked;
            if (_config.Proxy.UseUpstream)
                _config.Proxy.UpstreamPort = port;
            _main.ApplyProxySetting();
        }

        private void ApplyLogSettings(int server)
        {
            _config.Log.On = checkBoxOutput.Checked;
            _config.Log.MaterialLogInterval = (int)numericUpDownMaterialLogInterval.Value;
            _config.Log.OutputDir = textBoxOutput.Text;
            _config.Log.ServerOn = radioButtonServerOn.Checked;
            if (_config.Log.ServerOn)
                _config.Log.Listen = server;
            _main.ApplyLogSetting();
        }

        private void ApplyDebugSettings()
        {
            _config.DebugLogging = checkBoxDebugLog.Checked;
            _config.DebugLogFile = textBoxDebugLog.Text;
            _main.ApplyDebugLogSetting();
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
            openSoundFileDialog.FileName = textBoxSoundFile.Text;
            openSoundFileDialog.InitialDirectory = Path.GetDirectoryName(textBoxSoundFile.Text) ?? "";
            if (openSoundFileDialog.ShowDialog() != DialogResult.OK)
                return;
            textBoxSoundFile.Text = openSoundFileDialog.FileName;
            textBoxSoundFile.Select(textBoxSoundFile.Text.Length, 0);
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            _main.PlaySound(_soundSetting[(string)listBoxSoundFile.SelectedItem], (int)numericUpDownSoundVolume.Value);
        }

        private void buttonResetAchievement_Click(object sender, EventArgs e)
        {
            _main.ResetAchievemnt();
        }

        private void linkLabelProductName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabelProductName.LinkVisited = true;
            Process.Start(Home);
        }

        private void radioButtonAutoConfigOn_CheckedChanged(object sender, EventArgs e)
        {
            var on = ((RadioButton)sender).Checked;
            textBoxListen.Enabled = !on;
            if (on)
                textBoxListen.Text = ProxyConfig.DefaultListenPort.ToString();
        }

        private void radioButtonUpstreamOff_CheckedChanged(object sender, EventArgs e)
        {
            var off = ((RadioButton)sender).Checked;
            textBoxPort.Enabled = !off;
        }

        private void radioButtonServerOff_CheckedChanged(object sender, EventArgs e)
        {
            var off = ((RadioButton)sender).Checked;
            textBoxServer.Enabled = !off;
        }

        private void buttonOutputDir_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialogOutputDir.ShowDialog(this) == DialogResult.OK)
                textBoxOutput.Text = folderBrowserDialogOutputDir.SelectedPath;
            textBoxOutput.Select(textBoxOutput.Text.Length, 0);
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
            tabControl.SelectedTab = (TabPage)control.Parent.Parent;
            toolTipError.Show(message, control, 0, control.Height, 3000);
        }

        private void textBox_Enter(object sender, EventArgs e)
        {
            toolTipError.Hide((Control)sender);
        }

        private void buttonDebugLogOpenFile_Click(object sender, EventArgs e)
        {
            openDebugLogDialog.FileName = textBoxDebugLog.Text;
            openDebugLogDialog.InitialDirectory = Path.GetDirectoryName(textBoxDebugLog.Text);
            if (openDebugLogDialog.ShowDialog(this) == DialogResult.OK)
                textBoxDebugLog.Text = openDebugLogDialog.FileName;
            textBoxDebugLog.Select(textBoxDebugLog.Text.Length, 0);
        }

        private void buttonPlayDebugLog_Click(object sender, EventArgs e)
        {
            _main.SetPlayLog(textBoxDebugLog.Text);
            DialogResult = DialogResult.Cancel;
        }
    }
}