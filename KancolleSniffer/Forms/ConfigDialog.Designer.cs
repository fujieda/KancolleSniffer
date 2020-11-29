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

namespace KancolleSniffer.Forms
{
    partial class ConfigDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageWindow = new System.Windows.Forms.TabPage();
            this.label20 = new System.Windows.Forms.Label();
            this.comboBoxShape = new System.Windows.Forms.ComboBox();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.numericUpDownQuest = new System.Windows.Forms.NumericUpDown();
            this.checkBoxLocationPerMachine = new System.Windows.Forms.CheckBox();
            this.checkBoxExitSilently = new System.Windows.Forms.CheckBox();
            this.label15 = new System.Windows.Forms.Label();
            this.comboBoxZoom = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.checkBoxHideOnMinimized = new System.Windows.Forms.CheckBox();
            this.checkBoxTopMost = new System.Windows.Forms.CheckBox();
            this.tabPageNotification = new System.Windows.Forms.TabPage();
            this.checkBoxRepeat = new System.Windows.Forms.CheckBox();
            this.buttonDetailedSettings = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBoxCond49 = new System.Windows.Forms.CheckBox();
            this.checkBoxCond40 = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDownMarginEquips = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDownMarginShips = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxSound = new System.Windows.Forms.CheckBox();
            this.checkBoxBalloon = new System.Windows.Forms.CheckBox();
            this.checkBoxFlash = new System.Windows.Forms.CheckBox();
            this.tabPageAchievement = new System.Windows.Forms.TabPage();
            this.checkBoxAutoBattleResult = new System.Windows.Forms.CheckBox();
            this.checkBoxWarnBadDamageWithDamecon = new System.Windows.Forms.CheckBox();
            this.checkBoxPresetAkashi = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBoxNextCell = new System.Windows.Forms.CheckBox();
            this.checkBoxBattleResult = new System.Windows.Forms.CheckBox();
            this.checkBoxAirBattleResult = new System.Windows.Forms.CheckBox();
            this.checkBoxResultRank = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxReset02 = new System.Windows.Forms.CheckBox();
            this.buttonResetAchievement = new System.Windows.Forms.Button();
            this.checkBoxReset14 = new System.Windows.Forms.CheckBox();
            this.tabPageSound = new System.Windows.Forms.TabPage();
            this.buttonPlay = new System.Windows.Forms.Button();
            this.listBoxSoundFile = new System.Windows.Forms.ListBox();
            this.buttonOpenFile = new System.Windows.Forms.Button();
            this.textBoxSoundFile = new System.Windows.Forms.TextBox();
            this.numericUpDownSoundVolume = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPageVersion = new System.Windows.Forms.TabPage();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.linkLabelProductName = new System.Windows.Forms.LinkLabel();
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelLatest = new System.Windows.Forms.Label();
            this.tabPageProxy = new System.Windows.Forms.TabPage();
            this.groupBoxUpstream = new System.Windows.Forms.GroupBox();
            this.radioButtonUpstreamOff = new System.Windows.Forms.RadioButton();
            this.radioButtonUpstreamOn = new System.Windows.Forms.RadioButton();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.labelPort = new System.Windows.Forms.Label();
            this.groupBoxAutoConfig = new System.Windows.Forms.GroupBox();
            this.radioButtonAutoConfigOff = new System.Windows.Forms.RadioButton();
            this.radioButtonAutoConfigOn = new System.Windows.Forms.RadioButton();
            this.textBoxListen = new System.Windows.Forms.TextBox();
            this.labelListen = new System.Windows.Forms.Label();
            this.tabPageLog = new System.Windows.Forms.TabPage();
            this.label10 = new System.Windows.Forms.Label();
            this.checkBoxOutput = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.numericUpDownMaterialLogInterval = new System.Windows.Forms.NumericUpDown();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.buttonOutputDir = new System.Windows.Forms.Button();
            this.tabPagePush = new System.Windows.Forms.TabPage();
            this.groupBoxPushover = new System.Windows.Forms.GroupBox();
            this.buttonPushoverTest = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.textBoxPushoverUserKey = new System.Windows.Forms.TextBox();
            this.checkBoxPushoverOn = new System.Windows.Forms.CheckBox();
            this.textBoxPushoverApiKey = new System.Windows.Forms.TextBox();
            this.groupBoxPushbullet = new System.Windows.Forms.GroupBox();
            this.buttonPushbulletTest = new System.Windows.Forms.Button();
            this.textBoxPushbulletToken = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.checkBoxPushbulletOn = new System.Windows.Forms.CheckBox();
            this.tabPageDebug = new System.Windows.Forms.TabPage();
            this.buttonPlayDebugLog = new System.Windows.Forms.Button();
            this.buttonDebugLogOpenFile = new System.Windows.Forms.Button();
            this.textBoxDebugLog = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.checkBoxDebugLog = new System.Windows.Forms.CheckBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.openSoundFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialogOutputDir = new System.Windows.Forms.FolderBrowserDialog();
            this.openDebugLogDialog = new System.Windows.Forms.OpenFileDialog();
            this.tabControl.SuspendLayout();
            this.tabPageWindow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownQuest)).BeginInit();
            this.tabPageNotification.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginEquips)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginShips)).BeginInit();
            this.tabPageAchievement.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabPageSound.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSoundVolume)).BeginInit();
            this.tabPageVersion.SuspendLayout();
            this.tabPageProxy.SuspendLayout();
            this.groupBoxUpstream.SuspendLayout();
            this.groupBoxAutoConfig.SuspendLayout();
            this.tabPageLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaterialLogInterval)).BeginInit();
            this.tabPagePush.SuspendLayout();
            this.groupBoxPushover.SuspendLayout();
            this.groupBoxPushbullet.SuspendLayout();
            this.tabPageDebug.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tabControl.Controls.Add(this.tabPageWindow);
            this.tabControl.Controls.Add(this.tabPageNotification);
            this.tabControl.Controls.Add(this.tabPageAchievement);
            this.tabControl.Controls.Add(this.tabPageSound);
            this.tabControl.Controls.Add(this.tabPageVersion);
            this.tabControl.Controls.Add(this.tabPageProxy);
            this.tabControl.Controls.Add(this.tabPageLog);
            this.tabControl.Controls.Add(this.tabPagePush);
            this.tabControl.Controls.Add(this.tabPageDebug);
            this.tabControl.Location = new System.Drawing.Point(7, 6);
            this.tabControl.Multiline = true;
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(264, 202);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageWindow
            // 
            this.tabPageWindow.Controls.Add(this.label20);
            this.tabPageWindow.Controls.Add(this.comboBoxShape);
            this.tabPageWindow.Controls.Add(this.label19);
            this.tabPageWindow.Controls.Add(this.label18);
            this.tabPageWindow.Controls.Add(this.label17);
            this.tabPageWindow.Controls.Add(this.label8);
            this.tabPageWindow.Controls.Add(this.numericUpDownQuest);
            this.tabPageWindow.Controls.Add(this.checkBoxLocationPerMachine);
            this.tabPageWindow.Controls.Add(this.checkBoxExitSilently);
            this.tabPageWindow.Controls.Add(this.label15);
            this.tabPageWindow.Controls.Add(this.comboBoxZoom);
            this.tabPageWindow.Controls.Add(this.label14);
            this.tabPageWindow.Controls.Add(this.checkBoxHideOnMinimized);
            this.tabPageWindow.Controls.Add(this.checkBoxTopMost);
            this.tabPageWindow.Location = new System.Drawing.Point(4, 40);
            this.tabPageWindow.Name = "tabPageWindow";
            this.tabPageWindow.Padding = new System.Windows.Forms.Padding(8);
            this.tabPageWindow.Size = new System.Drawing.Size(256, 158);
            this.tabPageWindow.TabIndex = 0;
            this.tabPageWindow.Text = "ウィンドウ";
            this.tabPageWindow.UseVisualStyleBackColor = true;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(210, 98);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(11, 12);
            this.label20.TabIndex = 15;
            this.label20.Text = "*";
            // 
            // comboBoxShape
            // 
            this.comboBoxShape.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxShape.FormattingEnabled = true;
            this.comboBoxShape.Items.AddRange(new object[] {
            "縦長",
            "横長1",
            "横長2"});
            this.comboBoxShape.Location = new System.Drawing.Point(145, 97);
            this.comboBoxShape.Name = "comboBoxShape";
            this.comboBoxShape.Size = new System.Drawing.Size(64, 20);
            this.comboBoxShape.TabIndex = 14;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(125, 100);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(17, 12);
            this.label19.TabIndex = 13;
            this.label19.Text = "形";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(121, 127);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(11, 12);
            this.label18.TabIndex = 12;
            this.label18.Text = "*";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(186, 138);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(59, 12);
            this.label17.TabIndex = 11;
            this.label17.Text = "*要再起動";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 130);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(75, 12);
            this.label8.TabIndex = 10;
            this.label8.Text = "任務欄の行数";
            // 
            // numericUpDownQuest
            // 
            this.numericUpDownQuest.Location = new System.Drawing.Point(87, 127);
            this.numericUpDownQuest.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numericUpDownQuest.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.numericUpDownQuest.Name = "numericUpDownQuest";
            this.numericUpDownQuest.Size = new System.Drawing.Size(32, 19);
            this.numericUpDownQuest.TabIndex = 9;
            this.numericUpDownQuest.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDownQuest.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            // 
            // checkBoxLocationPerMachine
            // 
            this.checkBoxLocationPerMachine.AutoSize = true;
            this.checkBoxLocationPerMachine.Location = new System.Drawing.Point(11, 74);
            this.checkBoxLocationPerMachine.Name = "checkBoxLocationPerMachine";
            this.checkBoxLocationPerMachine.Size = new System.Drawing.Size(188, 16);
            this.checkBoxLocationPerMachine.TabIndex = 8;
            this.checkBoxLocationPerMachine.Text = "コンピューターごとに位置を保存する";
            this.checkBoxLocationPerMachine.UseVisualStyleBackColor = true;
            // 
            // checkBoxExitSilently
            // 
            this.checkBoxExitSilently.AutoSize = true;
            this.checkBoxExitSilently.Location = new System.Drawing.Point(11, 53);
            this.checkBoxExitSilently.Name = "checkBoxExitSilently";
            this.checkBoxExitSilently.Size = new System.Drawing.Size(189, 16);
            this.checkBoxExitSilently.TabIndex = 7;
            this.checkBoxExitSilently.Text = "閉じるボタンで終了時に確認しない";
            this.checkBoxExitSilently.UseVisualStyleBackColor = true;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(99, 98);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(11, 12);
            this.label15.TabIndex = 6;
            this.label15.Text = "*";
            // 
            // comboBoxZoom
            // 
            this.comboBoxZoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxZoom.FormattingEnabled = true;
            this.comboBoxZoom.Items.AddRange(new object[] {
            "100%",
            "125%",
            "150%"});
            this.comboBoxZoom.Location = new System.Drawing.Point(49, 97);
            this.comboBoxZoom.Name = "comboBoxZoom";
            this.comboBoxZoom.Size = new System.Drawing.Size(48, 20);
            this.comboBoxZoom.TabIndex = 5;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(11, 100);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(35, 12);
            this.label14.TabIndex = 4;
            this.label14.Text = "ズーム";
            // 
            // checkBoxHideOnMinimized
            // 
            this.checkBoxHideOnMinimized.AutoSize = true;
            this.checkBoxHideOnMinimized.Location = new System.Drawing.Point(11, 32);
            this.checkBoxHideOnMinimized.Name = "checkBoxHideOnMinimized";
            this.checkBoxHideOnMinimized.Size = new System.Drawing.Size(188, 16);
            this.checkBoxHideOnMinimized.TabIndex = 3;
            this.checkBoxHideOnMinimized.Text = "最小化時にタスクバーに表示しない";
            this.checkBoxHideOnMinimized.UseVisualStyleBackColor = true;
            // 
            // checkBoxTopMost
            // 
            this.checkBoxTopMost.AutoSize = true;
            this.checkBoxTopMost.Location = new System.Drawing.Point(11, 11);
            this.checkBoxTopMost.Name = "checkBoxTopMost";
            this.checkBoxTopMost.Size = new System.Drawing.Size(112, 16);
            this.checkBoxTopMost.TabIndex = 2;
            this.checkBoxTopMost.Text = "最前面に表示する";
            this.checkBoxTopMost.UseVisualStyleBackColor = true;
            // 
            // tabPageNotification
            // 
            this.tabPageNotification.Controls.Add(this.checkBoxRepeat);
            this.tabPageNotification.Controls.Add(this.buttonDetailedSettings);
            this.tabPageNotification.Controls.Add(this.label6);
            this.tabPageNotification.Controls.Add(this.checkBoxCond49);
            this.tabPageNotification.Controls.Add(this.checkBoxCond40);
            this.tabPageNotification.Controls.Add(this.label5);
            this.tabPageNotification.Controls.Add(this.numericUpDownMarginEquips);
            this.tabPageNotification.Controls.Add(this.label4);
            this.tabPageNotification.Controls.Add(this.numericUpDownMarginShips);
            this.tabPageNotification.Controls.Add(this.label2);
            this.tabPageNotification.Controls.Add(this.label1);
            this.tabPageNotification.Controls.Add(this.checkBoxSound);
            this.tabPageNotification.Controls.Add(this.checkBoxBalloon);
            this.tabPageNotification.Controls.Add(this.checkBoxFlash);
            this.tabPageNotification.Location = new System.Drawing.Point(4, 40);
            this.tabPageNotification.Name = "tabPageNotification";
            this.tabPageNotification.Padding = new System.Windows.Forms.Padding(8);
            this.tabPageNotification.Size = new System.Drawing.Size(256, 158);
            this.tabPageNotification.TabIndex = 1;
            this.tabPageNotification.Text = "通知";
            this.tabPageNotification.UseVisualStyleBackColor = true;
            // 
            // checkBoxRepeat
            // 
            this.checkBoxRepeat.AutoSize = true;
            this.checkBoxRepeat.Location = new System.Drawing.Point(122, 32);
            this.checkBoxRepeat.Name = "checkBoxRepeat";
            this.checkBoxRepeat.Size = new System.Drawing.Size(58, 16);
            this.checkBoxRepeat.TabIndex = 27;
            this.checkBoxRepeat.Text = "リピート";
            this.checkBoxRepeat.UseVisualStyleBackColor = true;
            // 
            // buttonDetailedSettings
            // 
            this.buttonDetailedSettings.Location = new System.Drawing.Point(11, 122);
            this.buttonDetailedSettings.Name = "buttonDetailedSettings";
            this.buttonDetailedSettings.Size = new System.Drawing.Size(75, 23);
            this.buttonDetailedSettings.TabIndex = 26;
            this.buttonDetailedSettings.Text = "詳細設定...";
            this.buttonDetailedSettings.UseVisualStyleBackColor = true;
            this.buttonDetailedSettings.Click += new System.EventHandler(this.buttonDetailedSettings_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(159, 61);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(86, 12);
            this.label6.TabIndex = 25;
            this.label6.Text = "疲労回復を通知";
            // 
            // checkBoxCond49
            // 
            this.checkBoxCond49.AutoSize = true;
            this.checkBoxCond49.Location = new System.Drawing.Point(161, 97);
            this.checkBoxCond49.Name = "checkBoxCond49";
            this.checkBoxCond49.Size = new System.Drawing.Size(60, 16);
            this.checkBoxCond49.TabIndex = 24;
            this.checkBoxCond49.Text = "cond49";
            this.checkBoxCond49.UseVisualStyleBackColor = true;
            // 
            // checkBoxCond40
            // 
            this.checkBoxCond40.AutoSize = true;
            this.checkBoxCond40.Location = new System.Drawing.Point(161, 78);
            this.checkBoxCond40.Name = "checkBoxCond40";
            this.checkBoxCond40.Size = new System.Drawing.Size(60, 16);
            this.checkBoxCond40.TabIndex = 23;
            this.checkBoxCond40.Text = "cond40";
            this.checkBoxCond40.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(95, 81);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 12);
            this.label5.TabIndex = 22;
            this.label5.Text = "個で通知";
            // 
            // numericUpDownMarginEquips
            // 
            this.numericUpDownMarginEquips.Location = new System.Drawing.Point(58, 79);
            this.numericUpDownMarginEquips.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.numericUpDownMarginEquips.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numericUpDownMarginEquips.Name = "numericUpDownMarginEquips";
            this.numericUpDownMarginEquips.Size = new System.Drawing.Size(36, 19);
            this.numericUpDownMarginEquips.TabIndex = 21;
            this.numericUpDownMarginEquips.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(29, 81);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 20;
            this.label4.Text = "装備";
            // 
            // numericUpDownMarginShips
            // 
            this.numericUpDownMarginShips.Location = new System.Drawing.Point(57, 59);
            this.numericUpDownMarginShips.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.numericUpDownMarginShips.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numericUpDownMarginShips.Name = "numericUpDownMarginShips";
            this.numericUpDownMarginShips.Size = new System.Drawing.Size(36, 19);
            this.numericUpDownMarginShips.TabIndex = 19;
            this.numericUpDownMarginShips.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(95, 61);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 18;
            this.label2.Text = "隻";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 12);
            this.label1.TabIndex = 17;
            this.label1.Text = "残り艦娘";
            // 
            // checkBoxSound
            // 
            this.checkBoxSound.AutoSize = true;
            this.checkBoxSound.Location = new System.Drawing.Point(122, 11);
            this.checkBoxSound.Name = "checkBoxSound";
            this.checkBoxSound.Size = new System.Drawing.Size(94, 16);
            this.checkBoxSound.TabIndex = 16;
            this.checkBoxSound.Text = "サウンドを再生";
            this.checkBoxSound.UseVisualStyleBackColor = true;
            // 
            // checkBoxBalloon
            // 
            this.checkBoxBalloon.AutoSize = true;
            this.checkBoxBalloon.Location = new System.Drawing.Point(11, 32);
            this.checkBoxBalloon.Name = "checkBoxBalloon";
            this.checkBoxBalloon.Size = new System.Drawing.Size(105, 16);
            this.checkBoxBalloon.TabIndex = 15;
            this.checkBoxBalloon.Text = "通知領域に表示";
            this.checkBoxBalloon.UseVisualStyleBackColor = true;
            // 
            // checkBoxFlash
            // 
            this.checkBoxFlash.AutoSize = true;
            this.checkBoxFlash.Location = new System.Drawing.Point(11, 11);
            this.checkBoxFlash.Name = "checkBoxFlash";
            this.checkBoxFlash.Size = new System.Drawing.Size(100, 16);
            this.checkBoxFlash.TabIndex = 14;
            this.checkBoxFlash.Text = "ウィンドウを点滅";
            this.checkBoxFlash.UseVisualStyleBackColor = true;
            // 
            // tabPageAchievement
            // 
            this.tabPageAchievement.Controls.Add(this.checkBoxAutoBattleResult);
            this.tabPageAchievement.Controls.Add(this.checkBoxWarnBadDamageWithDamecon);
            this.tabPageAchievement.Controls.Add(this.checkBoxPresetAkashi);
            this.tabPageAchievement.Controls.Add(this.groupBox3);
            this.tabPageAchievement.Controls.Add(this.groupBox2);
            this.tabPageAchievement.Location = new System.Drawing.Point(4, 40);
            this.tabPageAchievement.Name = "tabPageAchievement";
            this.tabPageAchievement.Padding = new System.Windows.Forms.Padding(8);
            this.tabPageAchievement.Size = new System.Drawing.Size(256, 158);
            this.tabPageAchievement.TabIndex = 2;
            this.tabPageAchievement.Text = "機能";
            this.tabPageAchievement.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoBattleResult
            // 
            this.checkBoxAutoBattleResult.AutoSize = true;
            this.checkBoxAutoBattleResult.Location = new System.Drawing.Point(143, 134);
            this.checkBoxAutoBattleResult.Name = "checkBoxAutoBattleResult";
            this.checkBoxAutoBattleResult.Size = new System.Drawing.Size(96, 16);
            this.checkBoxAutoBattleResult.TabIndex = 21;
            this.checkBoxAutoBattleResult.Text = "戦況自動切替";
            this.checkBoxAutoBattleResult.UseVisualStyleBackColor = true;
            // 
            // checkBoxWarnBadDamageWithDamecon
            // 
            this.checkBoxWarnBadDamageWithDamecon.AutoSize = true;
            this.checkBoxWarnBadDamageWithDamecon.Location = new System.Drawing.Point(20, 112);
            this.checkBoxWarnBadDamageWithDamecon.Name = "checkBoxWarnBadDamageWithDamecon";
            this.checkBoxWarnBadDamageWithDamecon.Size = new System.Drawing.Size(124, 16);
            this.checkBoxWarnBadDamageWithDamecon.TabIndex = 20;
            this.checkBoxWarnBadDamageWithDamecon.Text = "ダメコンあり大破警告";
            this.checkBoxWarnBadDamageWithDamecon.UseVisualStyleBackColor = true;
            // 
            // checkBoxPresetAkashi
            // 
            this.checkBoxPresetAkashi.AutoSize = true;
            this.checkBoxPresetAkashi.Location = new System.Drawing.Point(20, 134);
            this.checkBoxPresetAkashi.Name = "checkBoxPresetAkashi";
            this.checkBoxPresetAkashi.Size = new System.Drawing.Size(113, 16);
            this.checkBoxPresetAkashi.TabIndex = 19;
            this.checkBoxPresetAkashi.Text = "プリセット明石修理";
            this.checkBoxPresetAkashi.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBoxNextCell);
            this.groupBox3.Controls.Add(this.checkBoxBattleResult);
            this.groupBox3.Controls.Add(this.checkBoxAirBattleResult);
            this.groupBox3.Controls.Add(this.checkBoxResultRank);
            this.groupBox3.Location = new System.Drawing.Point(11, 59);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(6);
            this.groupBox3.Size = new System.Drawing.Size(234, 44);
            this.groupBox3.TabIndex = 18;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "ネタバレ";
            // 
            // checkBoxNextCell
            // 
            this.checkBoxNextCell.AutoSize = true;
            this.checkBoxNextCell.Location = new System.Drawing.Point(174, 18);
            this.checkBoxNextCell.Name = "checkBoxNextCell";
            this.checkBoxNextCell.Size = new System.Drawing.Size(48, 16);
            this.checkBoxNextCell.TabIndex = 3;
            this.checkBoxNextCell.Text = "進路";
            this.checkBoxNextCell.UseVisualStyleBackColor = true;
            // 
            // checkBoxBattleResult
            // 
            this.checkBoxBattleResult.AutoSize = true;
            this.checkBoxBattleResult.Location = new System.Drawing.Point(123, 18);
            this.checkBoxBattleResult.Name = "checkBoxBattleResult";
            this.checkBoxBattleResult.Size = new System.Drawing.Size(48, 16);
            this.checkBoxBattleResult.TabIndex = 2;
            this.checkBoxBattleResult.Text = "戦況";
            this.checkBoxBattleResult.UseVisualStyleBackColor = true;
            // 
            // checkBoxAirBattleResult
            // 
            this.checkBoxAirBattleResult.AutoSize = true;
            this.checkBoxAirBattleResult.Location = new System.Drawing.Point(60, 18);
            this.checkBoxAirBattleResult.Name = "checkBoxAirBattleResult";
            this.checkBoxAirBattleResult.Size = new System.Drawing.Size(60, 16);
            this.checkBoxAirBattleResult.TabIndex = 1;
            this.checkBoxAirBattleResult.Text = "航空戦";
            this.checkBoxAirBattleResult.UseVisualStyleBackColor = true;
            // 
            // checkBoxResultRank
            // 
            this.checkBoxResultRank.AutoSize = true;
            this.checkBoxResultRank.Location = new System.Drawing.Point(9, 18);
            this.checkBoxResultRank.Name = "checkBoxResultRank";
            this.checkBoxResultRank.Size = new System.Drawing.Size(48, 16);
            this.checkBoxResultRank.TabIndex = 0;
            this.checkBoxResultRank.Text = "勝敗";
            this.checkBoxResultRank.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxReset02);
            this.groupBox2.Controls.Add(this.buttonResetAchievement);
            this.groupBox2.Controls.Add(this.checkBoxReset14);
            this.groupBox2.Location = new System.Drawing.Point(11, 10);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(6);
            this.groupBox2.Size = new System.Drawing.Size(234, 44);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "戦果のリセット";
            // 
            // checkBoxReset02
            // 
            this.checkBoxReset02.AutoSize = true;
            this.checkBoxReset02.Location = new System.Drawing.Point(9, 18);
            this.checkBoxReset02.Name = "checkBoxReset02";
            this.checkBoxReset02.Size = new System.Drawing.Size(42, 16);
            this.checkBoxReset02.TabIndex = 13;
            this.checkBoxReset02.Text = "2時";
            this.checkBoxReset02.UseVisualStyleBackColor = true;
            // 
            // buttonResetAchievement
            // 
            this.buttonResetAchievement.Location = new System.Drawing.Point(111, 15);
            this.buttonResetAchievement.Name = "buttonResetAchievement";
            this.buttonResetAchievement.Size = new System.Drawing.Size(44, 20);
            this.buttonResetAchievement.TabIndex = 16;
            this.buttonResetAchievement.Text = "今すぐ";
            this.buttonResetAchievement.UseVisualStyleBackColor = true;
            this.buttonResetAchievement.Click += new System.EventHandler(this.buttonResetAchievement_Click);
            // 
            // checkBoxReset14
            // 
            this.checkBoxReset14.AutoSize = true;
            this.checkBoxReset14.Location = new System.Drawing.Point(57, 18);
            this.checkBoxReset14.Name = "checkBoxReset14";
            this.checkBoxReset14.Size = new System.Drawing.Size(48, 16);
            this.checkBoxReset14.TabIndex = 14;
            this.checkBoxReset14.Text = "14時";
            this.checkBoxReset14.UseVisualStyleBackColor = true;
            // 
            // tabPageSound
            // 
            this.tabPageSound.Controls.Add(this.buttonPlay);
            this.tabPageSound.Controls.Add(this.listBoxSoundFile);
            this.tabPageSound.Controls.Add(this.buttonOpenFile);
            this.tabPageSound.Controls.Add(this.textBoxSoundFile);
            this.tabPageSound.Controls.Add(this.numericUpDownSoundVolume);
            this.tabPageSound.Controls.Add(this.label3);
            this.tabPageSound.Location = new System.Drawing.Point(4, 40);
            this.tabPageSound.Name = "tabPageSound";
            this.tabPageSound.Padding = new System.Windows.Forms.Padding(8);
            this.tabPageSound.Size = new System.Drawing.Size(256, 158);
            this.tabPageSound.TabIndex = 3;
            this.tabPageSound.Text = "サウンド";
            this.tabPageSound.UseVisualStyleBackColor = true;
            // 
            // buttonPlay
            // 
            this.buttonPlay.Location = new System.Drawing.Point(93, 6);
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.Size = new System.Drawing.Size(37, 23);
            this.buttonPlay.TabIndex = 24;
            this.buttonPlay.Text = "再生";
            this.buttonPlay.UseVisualStyleBackColor = true;
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            // 
            // listBoxSoundFile
            // 
            this.listBoxSoundFile.FormattingEnabled = true;
            this.listBoxSoundFile.ItemHeight = 12;
            this.listBoxSoundFile.Location = new System.Drawing.Point(11, 34);
            this.listBoxSoundFile.Name = "listBoxSoundFile";
            this.listBoxSoundFile.Size = new System.Drawing.Size(234, 88);
            this.listBoxSoundFile.TabIndex = 23;
            this.listBoxSoundFile.SelectedIndexChanged += new System.EventHandler(this.listBoxSoundFile_SelectedIndexChanged);
            // 
            // buttonOpenFile
            // 
            this.buttonOpenFile.Location = new System.Drawing.Point(200, 126);
            this.buttonOpenFile.Name = "buttonOpenFile";
            this.buttonOpenFile.Size = new System.Drawing.Size(45, 23);
            this.buttonOpenFile.TabIndex = 22;
            this.buttonOpenFile.Text = "参照...";
            this.buttonOpenFile.UseVisualStyleBackColor = true;
            this.buttonOpenFile.Click += new System.EventHandler(this.buttonOpenFile_Click);
            // 
            // textBoxSoundFile
            // 
            this.textBoxSoundFile.Location = new System.Drawing.Point(11, 128);
            this.textBoxSoundFile.Name = "textBoxSoundFile";
            this.textBoxSoundFile.Size = new System.Drawing.Size(183, 19);
            this.textBoxSoundFile.TabIndex = 21;
            this.textBoxSoundFile.TextChanged += new System.EventHandler(this.textBoxSoundFile_TextChanged);
            // 
            // numericUpDownSoundVolume
            // 
            this.numericUpDownSoundVolume.Location = new System.Drawing.Point(42, 9);
            this.numericUpDownSoundVolume.Name = "numericUpDownSoundVolume";
            this.numericUpDownSoundVolume.Size = new System.Drawing.Size(44, 19);
            this.numericUpDownSoundVolume.TabIndex = 20;
            this.numericUpDownSoundVolume.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 11);
            this.label3.Margin = new System.Windows.Forms.Padding(3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 19;
            this.label3.Text = "音量";
            // 
            // tabPageVersion
            // 
            this.tabPageVersion.Controls.Add(this.richTextBox1);
            this.tabPageVersion.Controls.Add(this.labelCopyright);
            this.tabPageVersion.Controls.Add(this.linkLabelProductName);
            this.tabPageVersion.Controls.Add(this.labelVersion);
            this.tabPageVersion.Controls.Add(this.labelLatest);
            this.tabPageVersion.Location = new System.Drawing.Point(4, 40);
            this.tabPageVersion.Name = "tabPageVersion";
            this.tabPageVersion.Padding = new System.Windows.Forms.Padding(8);
            this.tabPageVersion.Size = new System.Drawing.Size(256, 158);
            this.tabPageVersion.TabIndex = 4;
            this.tabPageVersion.Text = "バージョン";
            this.tabPageVersion.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Location = new System.Drawing.Point(11, 62);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(234, 32);
            this.richTextBox1.TabIndex = 15;
            this.richTextBox1.Text = "このソフトウェアはApache Licenseバージョン2.0でライセンスします。";
            // 
            // labelCopyright
            // 
            this.labelCopyright.Location = new System.Drawing.Point(11, 35);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Size = new System.Drawing.Size(212, 12);
            this.labelCopyright.TabIndex = 13;
            // 
            // linkLabelProductName
            // 
            this.linkLabelProductName.AutoSize = true;
            this.linkLabelProductName.Location = new System.Drawing.Point(11, 11);
            this.linkLabelProductName.Name = "linkLabelProductName";
            this.linkLabelProductName.Size = new System.Drawing.Size(82, 12);
            this.linkLabelProductName.TabIndex = 12;
            this.linkLabelProductName.TabStop = true;
            this.linkLabelProductName.Text = "KancolleSniffer";
            this.linkLabelProductName.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelProductName_LinkClicked);
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(92, 11);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(50, 12);
            this.labelVersion.TabIndex = 10;
            this.labelVersion.Text = "バージョン";
            // 
            // labelLatest
            // 
            this.labelLatest.AutoSize = true;
            this.labelLatest.Location = new System.Drawing.Point(167, 11);
            this.labelLatest.Name = "labelLatest";
            this.labelLatest.Size = new System.Drawing.Size(49, 12);
            this.labelLatest.TabIndex = 11;
            this.labelLatest.Text = "最新です";
            // 
            // tabPageProxy
            // 
            this.tabPageProxy.Controls.Add(this.groupBoxUpstream);
            this.tabPageProxy.Controls.Add(this.groupBoxAutoConfig);
            this.tabPageProxy.Location = new System.Drawing.Point(4, 40);
            this.tabPageProxy.Name = "tabPageProxy";
            this.tabPageProxy.Padding = new System.Windows.Forms.Padding(8);
            this.tabPageProxy.Size = new System.Drawing.Size(256, 158);
            this.tabPageProxy.TabIndex = 5;
            this.tabPageProxy.Text = "プロキシ";
            this.tabPageProxy.UseVisualStyleBackColor = true;
            // 
            // groupBoxUpstream
            // 
            this.groupBoxUpstream.Controls.Add(this.radioButtonUpstreamOff);
            this.groupBoxUpstream.Controls.Add(this.radioButtonUpstreamOn);
            this.groupBoxUpstream.Controls.Add(this.textBoxPort);
            this.groupBoxUpstream.Controls.Add(this.labelPort);
            this.groupBoxUpstream.Location = new System.Drawing.Point(11, 65);
            this.groupBoxUpstream.Name = "groupBoxUpstream";
            this.groupBoxUpstream.Size = new System.Drawing.Size(234, 48);
            this.groupBoxUpstream.TabIndex = 3;
            this.groupBoxUpstream.TabStop = false;
            this.groupBoxUpstream.Text = "ツール連携";
            // 
            // radioButtonUpstreamOff
            // 
            this.radioButtonUpstreamOff.AutoSize = true;
            this.radioButtonUpstreamOff.Location = new System.Drawing.Point(59, 18);
            this.radioButtonUpstreamOff.Name = "radioButtonUpstreamOff";
            this.radioButtonUpstreamOff.Size = new System.Drawing.Size(47, 16);
            this.radioButtonUpstreamOff.TabIndex = 1;
            this.radioButtonUpstreamOff.Text = "無効";
            this.radioButtonUpstreamOff.UseVisualStyleBackColor = true;
            this.radioButtonUpstreamOff.CheckedChanged += new System.EventHandler(this.radioButtonUpstreamOff_CheckedChanged);
            // 
            // radioButtonUpstreamOn
            // 
            this.radioButtonUpstreamOn.AutoSize = true;
            this.radioButtonUpstreamOn.Location = new System.Drawing.Point(6, 18);
            this.radioButtonUpstreamOn.Name = "radioButtonUpstreamOn";
            this.radioButtonUpstreamOn.Size = new System.Drawing.Size(47, 16);
            this.radioButtonUpstreamOn.TabIndex = 0;
            this.radioButtonUpstreamOn.Text = "有効";
            this.radioButtonUpstreamOn.UseVisualStyleBackColor = true;
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(175, 17);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(36, 19);
            this.textBoxPort.TabIndex = 3;
            this.textBoxPort.Enter += new System.EventHandler(this.textBox_Enter);
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(114, 20);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(59, 12);
            this.labelPort.TabIndex = 2;
            this.labelPort.Text = "送信ポート:";
            // 
            // groupBoxAutoConfig
            // 
            this.groupBoxAutoConfig.Controls.Add(this.radioButtonAutoConfigOff);
            this.groupBoxAutoConfig.Controls.Add(this.radioButtonAutoConfigOn);
            this.groupBoxAutoConfig.Controls.Add(this.textBoxListen);
            this.groupBoxAutoConfig.Controls.Add(this.labelListen);
            this.groupBoxAutoConfig.Location = new System.Drawing.Point(11, 11);
            this.groupBoxAutoConfig.Name = "groupBoxAutoConfig";
            this.groupBoxAutoConfig.Size = new System.Drawing.Size(234, 48);
            this.groupBoxAutoConfig.TabIndex = 2;
            this.groupBoxAutoConfig.TabStop = false;
            this.groupBoxAutoConfig.Text = "自動設定";
            // 
            // radioButtonAutoConfigOff
            // 
            this.radioButtonAutoConfigOff.AutoSize = true;
            this.radioButtonAutoConfigOff.Location = new System.Drawing.Point(59, 19);
            this.radioButtonAutoConfigOff.Name = "radioButtonAutoConfigOff";
            this.radioButtonAutoConfigOff.Size = new System.Drawing.Size(47, 16);
            this.radioButtonAutoConfigOff.TabIndex = 1;
            this.radioButtonAutoConfigOff.Text = "無効";
            this.radioButtonAutoConfigOff.UseVisualStyleBackColor = true;
            // 
            // radioButtonAutoConfigOn
            // 
            this.radioButtonAutoConfigOn.AutoSize = true;
            this.radioButtonAutoConfigOn.Location = new System.Drawing.Point(6, 19);
            this.radioButtonAutoConfigOn.Name = "radioButtonAutoConfigOn";
            this.radioButtonAutoConfigOn.Size = new System.Drawing.Size(47, 16);
            this.radioButtonAutoConfigOn.TabIndex = 0;
            this.radioButtonAutoConfigOn.Text = "有効";
            this.radioButtonAutoConfigOn.UseVisualStyleBackColor = true;
            // 
            // textBoxListen
            // 
            this.textBoxListen.Location = new System.Drawing.Point(175, 18);
            this.textBoxListen.Name = "textBoxListen";
            this.textBoxListen.Size = new System.Drawing.Size(36, 19);
            this.textBoxListen.TabIndex = 3;
            this.textBoxListen.Enter += new System.EventHandler(this.textBox_Enter);
            // 
            // labelListen
            // 
            this.labelListen.AutoSize = true;
            this.labelListen.Location = new System.Drawing.Point(114, 21);
            this.labelListen.Name = "labelListen";
            this.labelListen.Size = new System.Drawing.Size(59, 12);
            this.labelListen.TabIndex = 2;
            this.labelListen.Text = "受信ポート:";
            // 
            // tabPageLog
            // 
            this.tabPageLog.Controls.Add(this.label10);
            this.tabPageLog.Controls.Add(this.checkBoxOutput);
            this.tabPageLog.Controls.Add(this.label9);
            this.tabPageLog.Controls.Add(this.numericUpDownMaterialLogInterval);
            this.tabPageLog.Controls.Add(this.textBoxOutput);
            this.tabPageLog.Controls.Add(this.label11);
            this.tabPageLog.Controls.Add(this.buttonOutputDir);
            this.tabPageLog.Location = new System.Drawing.Point(4, 40);
            this.tabPageLog.Name = "tabPageLog";
            this.tabPageLog.Padding = new System.Windows.Forms.Padding(8);
            this.tabPageLog.Size = new System.Drawing.Size(256, 158);
            this.tabPageLog.TabIndex = 6;
            this.tabPageLog.Text = "報告書";
            this.tabPageLog.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(137, 60);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(17, 12);
            this.label10.TabIndex = 6;
            this.label10.Text = "分";
            // 
            // checkBoxOutput
            // 
            this.checkBoxOutput.AutoSize = true;
            this.checkBoxOutput.Location = new System.Drawing.Point(11, 11);
            this.checkBoxOutput.Name = "checkBoxOutput";
            this.checkBoxOutput.Size = new System.Drawing.Size(67, 16);
            this.checkBoxOutput.TabIndex = 0;
            this.checkBoxOutput.Text = "出力する";
            this.checkBoxOutput.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 36);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(41, 12);
            this.label9.TabIndex = 2;
            this.label9.Text = "出力先";
            // 
            // numericUpDownMaterialLogInterval
            // 
            this.numericUpDownMaterialLogInterval.Location = new System.Drawing.Point(91, 58);
            this.numericUpDownMaterialLogInterval.Name = "numericUpDownMaterialLogInterval";
            this.numericUpDownMaterialLogInterval.Size = new System.Drawing.Size(44, 19);
            this.numericUpDownMaterialLogInterval.TabIndex = 5;
            this.numericUpDownMaterialLogInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Location = new System.Drawing.Point(50, 33);
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.Size = new System.Drawing.Size(142, 19);
            this.textBoxOutput.TabIndex = 1;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(9, 60);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(81, 12);
            this.label11.TabIndex = 4;
            this.label11.Text = "資材ログの間隔";
            // 
            // buttonOutputDir
            // 
            this.buttonOutputDir.Location = new System.Drawing.Point(196, 31);
            this.buttonOutputDir.Name = "buttonOutputDir";
            this.buttonOutputDir.Size = new System.Drawing.Size(49, 21);
            this.buttonOutputDir.TabIndex = 3;
            this.buttonOutputDir.Text = "参照...";
            this.buttonOutputDir.UseVisualStyleBackColor = true;
            this.buttonOutputDir.Click += new System.EventHandler(this.buttonOutputDir_Click);
            // 
            // tabPagePush
            // 
            this.tabPagePush.Controls.Add(this.groupBoxPushover);
            this.tabPagePush.Controls.Add(this.groupBoxPushbullet);
            this.tabPagePush.Location = new System.Drawing.Point(4, 40);
            this.tabPagePush.Name = "tabPagePush";
            this.tabPagePush.Size = new System.Drawing.Size(256, 158);
            this.tabPagePush.TabIndex = 8;
            this.tabPagePush.Text = "プッシュ通知";
            this.tabPagePush.UseVisualStyleBackColor = true;
            // 
            // groupBoxPushover
            // 
            this.groupBoxPushover.Controls.Add(this.buttonPushoverTest);
            this.groupBoxPushover.Controls.Add(this.label16);
            this.groupBoxPushover.Controls.Add(this.label12);
            this.groupBoxPushover.Controls.Add(this.textBoxPushoverUserKey);
            this.groupBoxPushover.Controls.Add(this.checkBoxPushoverOn);
            this.groupBoxPushover.Controls.Add(this.textBoxPushoverApiKey);
            this.groupBoxPushover.Location = new System.Drawing.Point(11, 60);
            this.groupBoxPushover.Name = "groupBoxPushover";
            this.groupBoxPushover.Size = new System.Drawing.Size(236, 66);
            this.groupBoxPushover.TabIndex = 2;
            this.groupBoxPushover.TabStop = false;
            this.groupBoxPushover.Text = "Pushover";
            // 
            // buttonPushoverTest
            // 
            this.buttonPushoverTest.Font = new System.Drawing.Font("ＭＳ ゴシック", 9F);
            this.buttonPushoverTest.Location = new System.Drawing.Point(6, 36);
            this.buttonPushoverTest.Name = "buttonPushoverTest";
            this.buttonPushoverTest.Size = new System.Drawing.Size(37, 23);
            this.buttonPushoverTest.TabIndex = 12;
            this.buttonPushoverTest.Text = "Test";
            this.buttonPushoverTest.UseVisualStyleBackColor = true;
            this.buttonPushoverTest.Click += new System.EventHandler(this.buttonPushoverTest_Click);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(58, 17);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(48, 12);
            this.label16.TabIndex = 11;
            this.label16.Text = "API Key:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(52, 41);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(54, 12);
            this.label12.TabIndex = 10;
            this.label12.Text = "User Key:";
            // 
            // textBoxPushoverUserKey
            // 
            this.textBoxPushoverUserKey.Location = new System.Drawing.Point(106, 38);
            this.textBoxPushoverUserKey.Name = "textBoxPushoverUserKey";
            this.textBoxPushoverUserKey.Size = new System.Drawing.Size(122, 19);
            this.textBoxPushoverUserKey.TabIndex = 9;
            // 
            // checkBoxPushoverOn
            // 
            this.checkBoxPushoverOn.AutoSize = true;
            this.checkBoxPushoverOn.Location = new System.Drawing.Point(6, 16);
            this.checkBoxPushoverOn.Name = "checkBoxPushoverOn";
            this.checkBoxPushoverOn.Size = new System.Drawing.Size(48, 16);
            this.checkBoxPushoverOn.TabIndex = 5;
            this.checkBoxPushoverOn.Text = "有効";
            this.checkBoxPushoverOn.UseVisualStyleBackColor = true;
            // 
            // textBoxPushoverApiKey
            // 
            this.textBoxPushoverApiKey.Location = new System.Drawing.Point(106, 13);
            this.textBoxPushoverApiKey.Name = "textBoxPushoverApiKey";
            this.textBoxPushoverApiKey.Size = new System.Drawing.Size(122, 19);
            this.textBoxPushoverApiKey.TabIndex = 7;
            // 
            // groupBoxPushbullet
            // 
            this.groupBoxPushbullet.Controls.Add(this.buttonPushbulletTest);
            this.groupBoxPushbullet.Controls.Add(this.textBoxPushbulletToken);
            this.groupBoxPushbullet.Controls.Add(this.label7);
            this.groupBoxPushbullet.Controls.Add(this.checkBoxPushbulletOn);
            this.groupBoxPushbullet.Location = new System.Drawing.Point(11, 11);
            this.groupBoxPushbullet.Name = "groupBoxPushbullet";
            this.groupBoxPushbullet.Size = new System.Drawing.Size(236, 43);
            this.groupBoxPushbullet.TabIndex = 1;
            this.groupBoxPushbullet.TabStop = false;
            this.groupBoxPushbullet.Text = "Pushbullet";
            // 
            // buttonPushbulletTest
            // 
            this.buttonPushbulletTest.Font = new System.Drawing.Font("ＭＳ ゴシック", 9F);
            this.buttonPushbulletTest.Location = new System.Drawing.Point(193, 13);
            this.buttonPushbulletTest.Name = "buttonPushbulletTest";
            this.buttonPushbulletTest.Size = new System.Drawing.Size(37, 23);
            this.buttonPushbulletTest.TabIndex = 3;
            this.buttonPushbulletTest.Text = "Test";
            this.buttonPushbulletTest.UseVisualStyleBackColor = true;
            this.buttonPushbulletTest.Click += new System.EventHandler(this.buttonPushbulletTest_Click);
            // 
            // textBoxPushbulletToken
            // 
            this.textBoxPushbulletToken.Location = new System.Drawing.Point(91, 15);
            this.textBoxPushbulletToken.Name = "textBoxPushbulletToken";
            this.textBoxPushbulletToken.Size = new System.Drawing.Size(98, 19);
            this.textBoxPushbulletToken.TabIndex = 2;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(52, 19);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(38, 12);
            this.label7.TabIndex = 1;
            this.label7.Text = "Token:";
            // 
            // checkBoxPushbulletOn
            // 
            this.checkBoxPushbulletOn.AutoSize = true;
            this.checkBoxPushbulletOn.Location = new System.Drawing.Point(6, 18);
            this.checkBoxPushbulletOn.Name = "checkBoxPushbulletOn";
            this.checkBoxPushbulletOn.Size = new System.Drawing.Size(48, 16);
            this.checkBoxPushbulletOn.TabIndex = 0;
            this.checkBoxPushbulletOn.Text = "有効";
            this.checkBoxPushbulletOn.UseVisualStyleBackColor = true;
            // 
            // tabPageDebug
            // 
            this.tabPageDebug.Controls.Add(this.buttonPlayDebugLog);
            this.tabPageDebug.Controls.Add(this.buttonDebugLogOpenFile);
            this.tabPageDebug.Controls.Add(this.textBoxDebugLog);
            this.tabPageDebug.Controls.Add(this.label13);
            this.tabPageDebug.Controls.Add(this.checkBoxDebugLog);
            this.tabPageDebug.Location = new System.Drawing.Point(4, 40);
            this.tabPageDebug.Name = "tabPageDebug";
            this.tabPageDebug.Padding = new System.Windows.Forms.Padding(8);
            this.tabPageDebug.Size = new System.Drawing.Size(256, 158);
            this.tabPageDebug.TabIndex = 7;
            this.tabPageDebug.Text = "デバッグ";
            this.tabPageDebug.UseVisualStyleBackColor = true;
            // 
            // buttonPlayDebugLog
            // 
            this.buttonPlayDebugLog.AutoSize = true;
            this.buttonPlayDebugLog.Location = new System.Drawing.Point(206, 46);
            this.buttonPlayDebugLog.Name = "buttonPlayDebugLog";
            this.buttonPlayDebugLog.Size = new System.Drawing.Size(39, 23);
            this.buttonPlayDebugLog.TabIndex = 12;
            this.buttonPlayDebugLog.Text = "再生";
            this.buttonPlayDebugLog.UseVisualStyleBackColor = true;
            this.buttonPlayDebugLog.Click += new System.EventHandler(this.buttonPlayDebugLog_Click);
            // 
            // buttonDebugLogOpenFile
            // 
            this.buttonDebugLogOpenFile.AutoSize = true;
            this.buttonDebugLogOpenFile.Location = new System.Drawing.Point(157, 46);
            this.buttonDebugLogOpenFile.Name = "buttonDebugLogOpenFile";
            this.buttonDebugLogOpenFile.Size = new System.Drawing.Size(45, 23);
            this.buttonDebugLogOpenFile.TabIndex = 11;
            this.buttonDebugLogOpenFile.Text = "参照...";
            this.buttonDebugLogOpenFile.UseVisualStyleBackColor = true;
            this.buttonDebugLogOpenFile.Click += new System.EventHandler(this.buttonDebugLogOpenFile_Click);
            // 
            // textBoxDebugLog
            // 
            this.textBoxDebugLog.Location = new System.Drawing.Point(11, 48);
            this.textBoxDebugLog.Name = "textBoxDebugLog";
            this.textBoxDebugLog.Size = new System.Drawing.Size(142, 19);
            this.textBoxDebugLog.TabIndex = 10;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(11, 33);
            this.label13.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(57, 12);
            this.label13.TabIndex = 9;
            this.label13.Text = "ログファイル";
            // 
            // checkBoxDebugLog
            // 
            this.checkBoxDebugLog.AutoSize = true;
            this.checkBoxDebugLog.Location = new System.Drawing.Point(11, 11);
            this.checkBoxDebugLog.Name = "checkBoxDebugLog";
            this.checkBoxDebugLog.Size = new System.Drawing.Size(174, 16);
            this.checkBoxDebugLog.TabIndex = 8;
            this.checkBoxDebugLog.Text = "開発者のために通信を記録する";
            this.checkBoxDebugLog.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(186, 214);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "キャンセル";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOk.Location = new System.Drawing.Point(105, 214);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 4;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // openSoundFileDialog
            // 
            this.openSoundFileDialog.Filter = "オーディオファイル (*.wav;*.aif;*.aifc;*.aiff;*.wma;*.mp2;*.mp3)|*.wav;*.aif;*.aifc;*.aiff" +
    ";*.wma;*.mp2;*.mp3";
            this.openSoundFileDialog.Title = "オーディオファイルを選択する";
            // 
            // folderBrowserDialogOutputDir
            // 
            this.folderBrowserDialogOutputDir.Description = "報告書の出力先を指定します。";
            // 
            // openDebugLogDialog
            // 
            this.openDebugLogDialog.CheckFileExists = false;
            this.openDebugLogDialog.Title = "ログファイルの選択";
            // 
            // ConfigDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(276, 246);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.tabControl);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "設定";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigDialog_FormClosing);
            this.Load += new System.EventHandler(this.ConfigDialog_Load);
            this.tabControl.ResumeLayout(false);
            this.tabPageWindow.ResumeLayout(false);
            this.tabPageWindow.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownQuest)).EndInit();
            this.tabPageNotification.ResumeLayout(false);
            this.tabPageNotification.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginEquips)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginShips)).EndInit();
            this.tabPageAchievement.ResumeLayout(false);
            this.tabPageAchievement.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabPageSound.ResumeLayout(false);
            this.tabPageSound.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSoundVolume)).EndInit();
            this.tabPageVersion.ResumeLayout(false);
            this.tabPageVersion.PerformLayout();
            this.tabPageProxy.ResumeLayout(false);
            this.groupBoxUpstream.ResumeLayout(false);
            this.groupBoxUpstream.PerformLayout();
            this.groupBoxAutoConfig.ResumeLayout(false);
            this.groupBoxAutoConfig.PerformLayout();
            this.tabPageLog.ResumeLayout(false);
            this.tabPageLog.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaterialLogInterval)).EndInit();
            this.tabPagePush.ResumeLayout(false);
            this.groupBoxPushover.ResumeLayout(false);
            this.groupBoxPushover.PerformLayout();
            this.groupBoxPushbullet.ResumeLayout(false);
            this.groupBoxPushbullet.PerformLayout();
            this.tabPageDebug.ResumeLayout(false);
            this.tabPageDebug.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageWindow;
        private System.Windows.Forms.TabPage tabPageNotification;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.TabPage tabPageAchievement;
        private System.Windows.Forms.TabPage tabPageSound;
        private System.Windows.Forms.TabPage tabPageVersion;
        private System.Windows.Forms.CheckBox checkBoxHideOnMinimized;
        private System.Windows.Forms.CheckBox checkBoxTopMost;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBoxCond49;
        private System.Windows.Forms.CheckBox checkBoxCond40;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDownMarginEquips;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDownMarginShips;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxSound;
        private System.Windows.Forms.CheckBox checkBoxBalloon;
        private System.Windows.Forms.CheckBox checkBoxFlash;
        private System.Windows.Forms.Button buttonResetAchievement;
        private System.Windows.Forms.CheckBox checkBoxReset14;
        private System.Windows.Forms.CheckBox checkBoxReset02;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.ListBox listBoxSoundFile;
        private System.Windows.Forms.Button buttonOpenFile;
        private System.Windows.Forms.TextBox textBoxSoundFile;
        private System.Windows.Forms.NumericUpDown numericUpDownSoundVolume;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.LinkLabel linkLabelProductName;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelLatest;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.OpenFileDialog openSoundFileDialog;
        private System.Windows.Forms.TabPage tabPageProxy;
        private System.Windows.Forms.GroupBox groupBoxUpstream;
        private System.Windows.Forms.RadioButton radioButtonUpstreamOff;
        private System.Windows.Forms.RadioButton radioButtonUpstreamOn;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.GroupBox groupBoxAutoConfig;
        private System.Windows.Forms.RadioButton radioButtonAutoConfigOff;
        private System.Windows.Forms.RadioButton radioButtonAutoConfigOn;
        private System.Windows.Forms.TextBox textBoxListen;
        private System.Windows.Forms.Label labelListen;
        private System.Windows.Forms.TabPage tabPageLog;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox checkBoxOutput;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Button buttonOutputDir;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown numericUpDownMaterialLogInterval;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogOutputDir;
        private System.Windows.Forms.TabPage tabPageDebug;
        private System.Windows.Forms.Button buttonPlayDebugLog;
        private System.Windows.Forms.Button buttonDebugLogOpenFile;
        private System.Windows.Forms.TextBox textBoxDebugLog;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox checkBoxDebugLog;
        private System.Windows.Forms.OpenFileDialog openDebugLogDialog;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBoxPresetAkashi;
        private System.Windows.Forms.TabPage tabPagePush;
        private System.Windows.Forms.ComboBox comboBoxZoom;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button buttonDetailedSettings;
        private System.Windows.Forms.GroupBox groupBoxPushbullet;
        private System.Windows.Forms.TextBox textBoxPushbulletToken;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox checkBoxPushbulletOn;
        private System.Windows.Forms.Button buttonPushbulletTest;
        private System.Windows.Forms.CheckBox checkBoxExitSilently;
        private System.Windows.Forms.CheckBox checkBoxLocationPerMachine;
        private System.Windows.Forms.TextBox textBoxPushoverApiKey;
        private System.Windows.Forms.CheckBox checkBoxPushoverOn;
        private System.Windows.Forms.GroupBox groupBoxPushover;
        private System.Windows.Forms.Button buttonPushoverTest;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox textBoxPushoverUserKey;
        private System.Windows.Forms.CheckBox checkBoxRepeat;
        private System.Windows.Forms.CheckBox checkBoxBattleResult;
        private System.Windows.Forms.CheckBox checkBoxAirBattleResult;
        private System.Windows.Forms.CheckBox checkBoxResultRank;
        private System.Windows.Forms.CheckBox checkBoxNextCell;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numericUpDownQuest;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.CheckBox checkBoxWarnBadDamageWithDamecon;
        private System.Windows.Forms.CheckBox checkBoxAutoBattleResult;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.ComboBox comboBoxShape;
        private System.Windows.Forms.Label label19;
    }
}