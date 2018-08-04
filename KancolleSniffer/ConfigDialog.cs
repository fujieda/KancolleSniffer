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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using KancolleSniffer.Net;
using KancolleSniffer.View;

namespace KancolleSniffer
{
    public partial class ConfigDialog : Form
    {
        private readonly Config _config;
        private readonly MainForm _main;

        private readonly Dictionary<string, NotificationSpec> _notificationSettings =
            new Dictionary<string, NotificationSpec>();

        private readonly Dictionary<string, string> _soundSettings = new Dictionary<string, string>();
        private const string Home = "http://kancollesniffer.osdn.jp/";
        private Point _prevPosition = new Point(int.MinValue, int.MinValue);

        public List<string> RepeatSettingsChanged { get; } = new List<string>();
        public NotificationConfigDialog NotificationConfigDialog { get; }

        public ConfigDialog(Config config, MainForm main)
        {
            InitializeComponent();
            _config = config;
            _main = main;
            // ReSharper disable once CoVariantArrayConversion
            listBoxSoundFile.Items.AddRange(Config.NotificationNames);
            numericUpDownMaterialLogInterval.Maximum = 1440;

            NotificationConfigDialog = new NotificationConfigDialog(_notificationSettings,
                new Dictionary<NotificationType, CheckBox>
                {
                    {NotificationType.FlashWindow, checkBoxFlash},
                    {NotificationType.ShowBaloonTip, checkBoxBalloon},
                    {NotificationType.PlaySound, checkBoxSound},
                    {NotificationType.Repeat, checkBoxRepeat}
                });
        }

        private void ConfigDialog_Load(object sender, EventArgs e)
        {
            if (_prevPosition.X != int.MinValue)
                Location = _prevPosition;
            _main.CheckVersionUp((current, latest) =>
            {
                labelVersion.Text = "バージョン" + current;
                labelLatest.Text = current == latest ? "最新です" : "最新は" + latest + "です";
            });
            labelCopyright.Text = FileVersionInfo.GetVersionInfo(Application.ExecutablePath).LegalCopyright;

            checkBoxTopMost.Checked = _config.TopMost;
            checkBoxHideOnMinimized.Checked = _config.HideOnMinimized;
            checkBoxExitSilently.Checked = _config.ExitSilently;
            checkBoxLocationPerMachine.Checked = _config.SaveLocationPerMachine;
            comboBoxZoom.SelectedItem = _config.Zoom + "%";

            checkBoxFlash.Checked = (_config.NotificationFlags & NotificationType.FlashWindow) != 0;
            checkBoxBalloon.Checked = (_config.NotificationFlags & NotificationType.ShowBaloonTip) != 0;
            checkBoxSound.Checked = (_config.NotificationFlags & NotificationType.PlaySound) != 0;
            checkBoxRepeat.Checked = (_config.NotificationFlags & NotificationType.Repeat) != 0;
            foreach (var name in Config.NotificationNames)
                _notificationSettings[name] = _config.Notifications[name];
            numericUpDownMarginShips.Value = _config.MarginShips;
            numericUpDownMarginEquips.Value = _config.MarginEquips;
            checkBoxCond40.Checked = _config.NotifyConditions.Contains(40);
            checkBoxCond49.Checked = _config.NotifyConditions.Contains(49);

            checkBoxReset02.Checked = _config.ResetHours.Contains(2);
            checkBoxReset14.Checked = _config.ResetHours.Contains(14);
            checkBoxResultRank.Checked = (_config.Spoilers & Spoiler.ResultRank) != 0;
            checkBoxAirBattleResult.Checked = (_config.Spoilers & Spoiler.AirBattleResult) != 0;
            checkBoxBattleResult.Checked = (_config.Spoilers & Spoiler.BattleResult) != 0;
            checkBoxNextCell.Checked = (_config.Spoilers & Spoiler.NextCell) != 0;
            checkBoxPresetAkashi.Checked = _config.UsePresetAkashi;

            numericUpDownSoundVolume.Value = _config.Sounds.Volume;
            foreach (var name in Config.NotificationNames)
                _soundSettings[name] = _config.Sounds[name];
            listBoxSoundFile.SelectedIndex = -1;
            listBoxSoundFile.SelectedIndex = 0;

            LoadProxySettings();
            LoadLogSettings();
            LoadDebugSettings();

            checkBoxPushbulletOn.Checked = _config.Pushbullet.On;
            textBoxPushbulletToken.Text = _config.Pushbullet.Token;
            checkBoxPushoverOn.Checked = _config.Pushover.On;
            textBoxPushoverApiKey.Text = _config.Pushover.ApiKey;
            textBoxPushoverUserKey.Text = _config.Pushover.UserKey;
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

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (!ValidatePorts(out var listen, out var outbound, out _))
                return;
            DialogResult = DialogResult.OK;
            if (!ApplyProxySettings(listen, outbound))
                DialogResult = DialogResult.None;
            ApplyLogSettings();
            ApplyDebugSettings();

            _config.Pushbullet.On = checkBoxPushbulletOn.Checked;
            _config.Pushbullet.Token = textBoxPushbulletToken.Text;
            _config.Pushover.On = checkBoxPushoverOn.Checked;
            _config.Pushover.ApiKey = textBoxPushoverApiKey.Text;
            _config.Pushover.UserKey = textBoxPushoverUserKey.Text;

            _config.TopMost = checkBoxTopMost.Checked;
            _config.HideOnMinimized = checkBoxHideOnMinimized.Checked;
            _config.ExitSilently = checkBoxExitSilently.Checked;
            _config.SaveLocationPerMachine = checkBoxLocationPerMachine.Checked;
            _config.Zoom = int.Parse(comboBoxZoom.SelectedItem.ToString().Substring(0, 3));
            _config.NotificationFlags = (checkBoxFlash.Checked ? NotificationType.FlashWindow : 0) |
                                        (checkBoxBalloon.Checked ? NotificationType.ShowBaloonTip : 0) |
                                        (checkBoxSound.Checked ? NotificationType.PlaySound : 0) |
                                        (checkBoxRepeat.Checked ? NotificationType.Repeat : 0);
            _config.MarginShips = (int)numericUpDownMarginShips.Value;
            _config.MarginEquips = (int)numericUpDownMarginEquips.Value;

            RepeatSettingsChanged.Clear();
            var repeatOff = (_config.NotificationFlags & NotificationType.Repeat) == 0;
            foreach (var name in Config.NotificationNames)
            {
                var old = _config.Notifications[name];
                var cur = _notificationSettings[name];
                if (repeatOff || old.RepeatInterval != cur.RepeatInterval ||
                    (cur.Flags & NotificationType.Repeat) == 0 ||
                    (cur.Flags & NotificationType.Cont) == 0)
                {
                    RepeatSettingsChanged.Add(name);
                }
                _config.Notifications[name] = cur;
            }

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

            _config.Spoilers = (checkBoxResultRank.Checked ? Spoiler.ResultRank : 0) |
                               (checkBoxAirBattleResult.Checked ? Spoiler.AirBattleResult : 0) |
                               (checkBoxBattleResult.Checked ? Spoiler.BattleResult : 0) |
                               (checkBoxNextCell.Checked ? Spoiler.NextCell : 0);
            _config.UsePresetAkashi = checkBoxPresetAkashi.Checked;

            _config.Sounds.Volume = (int)numericUpDownSoundVolume.Value;
            foreach (var name in Config.NotificationNames)
                _config.Sounds[name] = MakePathRooted(_soundSettings[name]);
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
            _config.Log.OutputDir = MakePathRooted(textBoxOutput.Text);
            _main.ApplyLogSetting();
        }

        private void ApplyDebugSettings()
        {
            _config.DebugLogging = checkBoxDebugLog.Checked;
            _config.DebugLogFile = MakePathRooted(textBoxDebugLog.Text);
            _main.ApplyDebugLogSetting();
        }

        private string MakePathRooted(string path)
        {
            try
            {
                return string.IsNullOrWhiteSpace(path)
                    ? ""
                    : Path.IsPathRooted(path)
                        ? path
                        : Path.Combine(Config.BaseDir, path);
            }
            catch (ArgumentException)
            {
                return "";
            }
        }

        private void textBoxSoundFile_TextChanged(object sender, EventArgs e)
        {
            _soundSettings[(string)listBoxSoundFile.SelectedItem] = textBoxSoundFile.Text;
        }

        private void listBoxSoundFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxSoundFile.SelectedItem == null)
                return;
            textBoxSoundFile.Text = _soundSettings[(string)listBoxSoundFile.SelectedItem];
            textBoxSoundFile.Select(textBoxSoundFile.Text.Length, 0);
        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            SetInitialPath(openSoundFileDialog, textBoxSoundFile.Text);
            if (openSoundFileDialog.ShowDialog() != DialogResult.OK)
                return;
            textBoxSoundFile.Text = openSoundFileDialog.FileName;
            textBoxSoundFile.Select(textBoxSoundFile.Text.Length, 0);
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            _main.PlaySound(_soundSettings[(string)listBoxSoundFile.SelectedItem], (int)numericUpDownSoundVolume.Value);
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
            if (result > 65535)
            {
                ShowToolTip("65535以下の数字を入力してください。", textBox);
                return false;
            }
            return true;
        }

        private readonly ResizableToolTip _toolTip =
            new ResizableToolTip {AutomaticDelay = 0, ToolTipIcon = ToolTipIcon.Error};

        private void ShowToolTip(string message, Control control)
        {
            tabControl.SelectedTab = (TabPage)control.Parent.Parent;
            _toolTip.Show(message, control, 0, control.Height, 3000);
        }

        private void textBox_Enter(object sender, EventArgs e)
        {
            _toolTip.Hide((Control)sender);
        }

        private void buttonDebugLogOpenFile_Click(object sender, EventArgs e)
        {
            SetInitialPath(openDebugLogDialog, textBoxDebugLog.Text);
            if (openDebugLogDialog.ShowDialog(this) == DialogResult.OK)
                textBoxDebugLog.Text = openDebugLogDialog.FileName;
            textBoxDebugLog.Select(textBoxDebugLog.Text.Length, 0);
        }

        private void SetInitialPath(OpenFileDialog dialog, string path)
        {
            var dir = Config.BaseDir;
            var file = "";
            if (!string.IsNullOrWhiteSpace(path))
            {
                var res = Path.GetDirectoryName(path);
                if (res == null) // root
                {
                    dir = path;
                }
                else if (res != "") // contain directory
                {
                    dir = res;
                    file = Path.GetFileName(path);
                }
                else
                {
                    file = path;
                }
            }
            dialog.InitialDirectory = dir;
            dialog.FileName = file;
        }

        private void buttonPlayDebugLog_Click(object sender, EventArgs e)
        {
            _main.SetPlayLog(textBoxDebugLog.Text);
            DialogResult = DialogResult.Cancel;
        }

        private void buttonDetailedSettings_Click(object sender, EventArgs e)
        {
            NotificationConfigDialog.ShowDialog(this);
        }

        private void ConfigDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            _prevPosition = Location;
        }

        private void buttonPushbulletTest_Click(object sender, EventArgs e)
        {
            try
            {
                PushNotification.PushToPushbullet(textBoxPushbulletToken.Text, "KancolleSniffer", "うまくいったかな？");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Pushbulletエラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonPushoverTest_Click(object sender, EventArgs e)
        {
            try
            {
                PushNotification.PushToPushover(textBoxPushoverApiKey.Text, textBoxPushoverUserKey.Text,
                    "KancolleSniffer", "うまくいったかな？");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Pushoverエラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
            if (factor.Height > 1)
                _toolTip.Font = new Font(_toolTip.Font.FontFamily, _toolTip.Font.Size * factor.Height);
        }
    }
}