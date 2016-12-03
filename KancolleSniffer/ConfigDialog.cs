// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
            // ReSharper disable once CoVariantArrayConversion
            listBoxSoundFile.Items.AddRange(_config.Sounds.SoundNames);
            numericUpDownMaterialLogInterval.Maximum = 1440;
        }

        private void ConfigDialog_Load(object sender, EventArgs e)
        {
            var version = string.Join(".", Application.ProductVersion.Split('.').Take(2));
            labelVersion.Text = "バージョン" + version;
            SetLatestVersion(version);
            labelCopyright.Text = FileVersionInfo.GetVersionInfo(Application.ExecutablePath).LegalCopyright;

            checkBoxTopMost.Checked = _config.TopMost;
            checkBoxHideOnMinimized.Checked = _config.HideOnMinimized;
            comboBoxZoom.SelectedItem = _config.Zoom + "%";
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
            checkBoxPresetAkashi.Checked = _config.UsePresetAkashi;

            numericUpDownSoundVolume.Value = _config.Sounds.Volume;
            foreach (var name in _config.Sounds.SoundNames)
                _soundSetting[name] = _config.Sounds[name];
            listBoxSoundFile.SelectedIndex = -1;
            listBoxSoundFile.SelectedIndex = 0;

            LoadProxySettings();
            LoadLogSettings();
            LoadDebugSettings();
            checkBoxKancolleDbOn.Checked = _config.KancolleDb.On;
            textBoxKancolleDbToken.Text = _config.KancolleDb.Token;
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
                    try
                    {
                        Invoke(new Action(() => { labelLatest.Text = version == str ? "最新です" : "最新は" + str + "です"; }));
                    }
                    catch (InvalidOperationException)
                    {
                    }
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
            if (!ApplyProxySettings(listen, outbound))
                DialogResult = DialogResult.None;
            ApplyLogSettings();
            ApplyDebugSettings();
            _config.KancolleDb.On = checkBoxKancolleDbOn.Checked;
            _config.KancolleDb.Token = textBoxKancolleDbToken.Text;

            _config.TopMost = checkBoxTopMost.Checked;
            _config.HideOnMinimized = checkBoxHideOnMinimized.Checked;
            _config.Zoom = int.Parse(comboBoxZoom.SelectedItem.ToString().Substring(0, 3));
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
            _main.UpdateFighterPower();
            _config.UsePresetAkashi = checkBoxPresetAkashi.Checked;

            _config.Sounds.Volume = (int)numericUpDownSoundVolume.Value;
            foreach (var name in _config.Sounds.SoundNames)
                _config.Sounds[name] = _soundSetting[name];
        }

        private bool ValidatePorts(out int listen, out int outbound, out int server)
        {
            outbound = -1;
            server = -1;
            if (!ValidatePortNumber(textBoxListen, out listen))
                return false;
            if (radioButtonUpstreamOn.Checked && !ValidatePortNumber(textBoxPort, out outbound))
                return false;
            if (radioButtonUpstreamOn.Checked && listen == outbound)
            {
                ShowToolTip("受信と送信に同じポートは使えません。", textBoxPort);
                return false;
            }
            return true;
        }

        private bool ApplyProxySettings(int listen, int port)
        {
            _config.Proxy.Auto = radioButtonAutoConfigOn.Checked;
            _config.Proxy.Listen = listen;
            _config.Proxy.UseUpstream = radioButtonUpstreamOn.Checked;
            if (_config.Proxy.UseUpstream)
                _config.Proxy.UpstreamPort = port;
            if (!_main.ApplyProxySetting())
                return false;
            textBoxListen.Text = _config.Proxy.Listen.ToString();
            return true;
        }

        private void ApplyLogSettings()
        {
            _config.Log.On = checkBoxOutput.Checked;
            _config.Log.MaterialLogInterval = (int)numericUpDownMaterialLogInterval.Value;
            _config.Log.OutputDir = textBoxOutput.Text;
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

        private void radioButtonUpstreamOff_CheckedChanged(object sender, EventArgs e)
        {
            var off = ((RadioButton)sender).Checked;
            textBoxPort.Enabled = !off;
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