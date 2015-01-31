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

namespace KancolleSniffer
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
            this.components = new System.ComponentModel.Container();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageShow = new System.Windows.Forms.TabPage();
            this.checkBoxHideOnMinimized = new System.Windows.Forms.CheckBox();
            this.checkBoxTopMost = new System.Windows.Forms.CheckBox();
            this.tabPageNotification = new System.Windows.Forms.TabPage();
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
            this.buttonResetAchievement = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.checkBoxReset14 = new System.Windows.Forms.CheckBox();
            this.checkBoxReset02 = new System.Windows.Forms.CheckBox();
            this.tabPageSound = new System.Windows.Forms.TabPage();
            this.buttonPlay = new System.Windows.Forms.Button();
            this.listBoxSoundFile = new System.Windows.Forms.ListBox();
            this.buttonOpenFile = new System.Windows.Forms.Button();
            this.textBoxSoundFile = new System.Windows.Forms.TextBox();
            this.numericUpDownSoundVolume = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPageVersion = new System.Windows.Forms.TabPage();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.linkLabelProductName = new System.Windows.Forms.LinkLabel();
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelLatest = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ProxyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DebugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.tabControl.SuspendLayout();
            this.tabPageShow.SuspendLayout();
            this.tabPageNotification.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginEquips)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginShips)).BeginInit();
            this.tabPageAchievement.SuspendLayout();
            this.tabPageSound.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSoundVolume)).BeginInit();
            this.tabPageVersion.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageShow);
            this.tabControl.Controls.Add(this.tabPageNotification);
            this.tabControl.Controls.Add(this.tabPageAchievement);
            this.tabControl.Controls.Add(this.tabPageSound);
            this.tabControl.Controls.Add(this.tabPageVersion);
            this.tabControl.Location = new System.Drawing.Point(6, 6);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(262, 173);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageShow
            // 
            this.tabPageShow.Controls.Add(this.checkBoxHideOnMinimized);
            this.tabPageShow.Controls.Add(this.checkBoxTopMost);
            this.tabPageShow.Location = new System.Drawing.Point(4, 22);
            this.tabPageShow.Name = "tabPageShow";
            this.tabPageShow.Padding = new System.Windows.Forms.Padding(8);
            this.tabPageShow.Size = new System.Drawing.Size(254, 147);
            this.tabPageShow.TabIndex = 0;
            this.tabPageShow.Text = "表示";
            this.tabPageShow.UseVisualStyleBackColor = true;
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
            this.tabPageNotification.Location = new System.Drawing.Point(4, 22);
            this.tabPageNotification.Name = "tabPageNotification";
            this.tabPageNotification.Padding = new System.Windows.Forms.Padding(8);
            this.tabPageNotification.Size = new System.Drawing.Size(254, 147);
            this.tabPageNotification.TabIndex = 1;
            this.tabPageNotification.Text = "通知";
            this.tabPageNotification.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 119);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(105, 12);
            this.label6.TabIndex = 25;
            this.label6.Text = "疲労回復を通知する";
            // 
            // checkBoxCond49
            // 
            this.checkBoxCond49.AutoSize = true;
            this.checkBoxCond49.Location = new System.Drawing.Point(180, 118);
            this.checkBoxCond49.Name = "checkBoxCond49";
            this.checkBoxCond49.Size = new System.Drawing.Size(60, 16);
            this.checkBoxCond49.TabIndex = 24;
            this.checkBoxCond49.Text = "cond49";
            this.checkBoxCond49.UseVisualStyleBackColor = true;
            // 
            // checkBoxCond40
            // 
            this.checkBoxCond40.AutoSize = true;
            this.checkBoxCond40.Location = new System.Drawing.Point(118, 118);
            this.checkBoxCond40.Name = "checkBoxCond40";
            this.checkBoxCond40.Size = new System.Drawing.Size(60, 16);
            this.checkBoxCond40.TabIndex = 23;
            this.checkBoxCond40.Text = "cond40";
            this.checkBoxCond40.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(118, 96);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 12);
            this.label5.TabIndex = 22;
            this.label5.Text = "個で通知する";
            // 
            // numericUpDownMarginEquips
            // 
            this.numericUpDownMarginEquips.Location = new System.Drawing.Point(81, 94);
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
            this.label4.Location = new System.Drawing.Point(52, 96);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 20;
            this.label4.Text = "装備";
            // 
            // numericUpDownMarginShips
            // 
            this.numericUpDownMarginShips.Location = new System.Drawing.Point(80, 74);
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
            this.label2.Location = new System.Drawing.Point(118, 76);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 18;
            this.label2.Text = "隻";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 12);
            this.label1.TabIndex = 17;
            this.label1.Text = "上限まで艦娘";
            // 
            // checkBoxSound
            // 
            this.checkBoxSound.AutoSize = true;
            this.checkBoxSound.Location = new System.Drawing.Point(11, 53);
            this.checkBoxSound.Name = "checkBoxSound";
            this.checkBoxSound.Size = new System.Drawing.Size(113, 16);
            this.checkBoxSound.TabIndex = 16;
            this.checkBoxSound.Text = "サウンドを再生する";
            this.checkBoxSound.UseVisualStyleBackColor = true;
            // 
            // checkBoxBalloon
            // 
            this.checkBoxBalloon.AutoSize = true;
            this.checkBoxBalloon.Location = new System.Drawing.Point(11, 32);
            this.checkBoxBalloon.Name = "checkBoxBalloon";
            this.checkBoxBalloon.Size = new System.Drawing.Size(172, 16);
            this.checkBoxBalloon.TabIndex = 15;
            this.checkBoxBalloon.Text = "通知領域にバルーンを表示する";
            this.checkBoxBalloon.UseVisualStyleBackColor = true;
            // 
            // checkBoxFlash
            // 
            this.checkBoxFlash.AutoSize = true;
            this.checkBoxFlash.Location = new System.Drawing.Point(11, 11);
            this.checkBoxFlash.Name = "checkBoxFlash";
            this.checkBoxFlash.Size = new System.Drawing.Size(127, 16);
            this.checkBoxFlash.TabIndex = 14;
            this.checkBoxFlash.Text = "ウィンドウを点滅させる";
            this.checkBoxFlash.UseVisualStyleBackColor = true;
            // 
            // tabPageAchievement
            // 
            this.tabPageAchievement.Controls.Add(this.buttonResetAchievement);
            this.tabPageAchievement.Controls.Add(this.label8);
            this.tabPageAchievement.Controls.Add(this.checkBoxReset14);
            this.tabPageAchievement.Controls.Add(this.checkBoxReset02);
            this.tabPageAchievement.Location = new System.Drawing.Point(4, 22);
            this.tabPageAchievement.Name = "tabPageAchievement";
            this.tabPageAchievement.Padding = new System.Windows.Forms.Padding(8);
            this.tabPageAchievement.Size = new System.Drawing.Size(254, 147);
            this.tabPageAchievement.TabIndex = 2;
            this.tabPageAchievement.Text = "戦果";
            this.tabPageAchievement.UseVisualStyleBackColor = true;
            // 
            // buttonResetAchievement
            // 
            this.buttonResetAchievement.Location = new System.Drawing.Point(101, 8);
            this.buttonResetAchievement.Name = "buttonResetAchievement";
            this.buttonResetAchievement.Size = new System.Drawing.Size(44, 20);
            this.buttonResetAchievement.TabIndex = 16;
            this.buttonResetAchievement.Text = "今すぐ";
            this.buttonResetAchievement.UseVisualStyleBackColor = true;
            this.buttonResetAchievement.Click += new System.EventHandler(this.buttonResetAchievement_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(147, 12);
            this.label8.Margin = new System.Windows.Forms.Padding(3);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 12);
            this.label8.TabIndex = 15;
            this.label8.Text = "にリセットする";
            // 
            // checkBoxReset14
            // 
            this.checkBoxReset14.AutoSize = true;
            this.checkBoxReset14.Location = new System.Drawing.Point(54, 11);
            this.checkBoxReset14.Name = "checkBoxReset14";
            this.checkBoxReset14.Size = new System.Drawing.Size(48, 16);
            this.checkBoxReset14.TabIndex = 14;
            this.checkBoxReset14.Text = "14時";
            this.checkBoxReset14.UseVisualStyleBackColor = true;
            // 
            // checkBoxReset02
            // 
            this.checkBoxReset02.AutoSize = true;
            this.checkBoxReset02.Location = new System.Drawing.Point(11, 11);
            this.checkBoxReset02.Name = "checkBoxReset02";
            this.checkBoxReset02.Size = new System.Drawing.Size(42, 16);
            this.checkBoxReset02.TabIndex = 13;
            this.checkBoxReset02.Text = "2時";
            this.checkBoxReset02.UseVisualStyleBackColor = true;
            // 
            // tabPageSound
            // 
            this.tabPageSound.Controls.Add(this.buttonPlay);
            this.tabPageSound.Controls.Add(this.listBoxSoundFile);
            this.tabPageSound.Controls.Add(this.buttonOpenFile);
            this.tabPageSound.Controls.Add(this.textBoxSoundFile);
            this.tabPageSound.Controls.Add(this.numericUpDownSoundVolume);
            this.tabPageSound.Controls.Add(this.label3);
            this.tabPageSound.Location = new System.Drawing.Point(4, 22);
            this.tabPageSound.Name = "tabPageSound";
            this.tabPageSound.Padding = new System.Windows.Forms.Padding(8);
            this.tabPageSound.Size = new System.Drawing.Size(254, 147);
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
            this.listBoxSoundFile.Size = new System.Drawing.Size(228, 76);
            this.listBoxSoundFile.TabIndex = 23;
            this.listBoxSoundFile.SelectedIndexChanged += new System.EventHandler(this.listBoxSoundFile_SelectedIndexChanged);
            // 
            // buttonOpenFile
            // 
            this.buttonOpenFile.Location = new System.Drawing.Point(196, 114);
            this.buttonOpenFile.Name = "buttonOpenFile";
            this.buttonOpenFile.Size = new System.Drawing.Size(45, 23);
            this.buttonOpenFile.TabIndex = 22;
            this.buttonOpenFile.Text = "参照...";
            this.buttonOpenFile.UseVisualStyleBackColor = true;
            this.buttonOpenFile.Click += new System.EventHandler(this.buttonOpenFile_Click);
            // 
            // textBoxSoundFile
            // 
            this.textBoxSoundFile.Location = new System.Drawing.Point(11, 116);
            this.textBoxSoundFile.Name = "textBoxSoundFile";
            this.textBoxSoundFile.Size = new System.Drawing.Size(179, 19);
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
            this.tabPageVersion.Controls.Add(this.label7);
            this.tabPageVersion.Controls.Add(this.linkLabelProductName);
            this.tabPageVersion.Controls.Add(this.labelVersion);
            this.tabPageVersion.Controls.Add(this.labelLatest);
            this.tabPageVersion.Location = new System.Drawing.Point(4, 22);
            this.tabPageVersion.Name = "tabPageVersion";
            this.tabPageVersion.Padding = new System.Windows.Forms.Padding(8);
            this.tabPageVersion.Size = new System.Drawing.Size(254, 147);
            this.tabPageVersion.TabIndex = 4;
            this.tabPageVersion.Text = "バージョン情報";
            this.tabPageVersion.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Location = new System.Drawing.Point(11, 62);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(208, 50);
            this.richTextBox1.TabIndex = 15;
            this.richTextBox1.Text = "このソフトウェアは一部のファイルを除き\nGNU GPLバージョン3でライセンスされます。\n詳細はREADME.mdを参照してください。";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 35);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(212, 12);
            this.label7.TabIndex = 13;
            this.label7.Text = "Copyright © 2013-2015 Kazuhiro Fujieda";
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
            this.labelVersion.Size = new System.Drawing.Size(70, 12);
            this.labelVersion.TabIndex = 10;
            this.labelVersion.Text = "バージョン4.10";
            // 
            // labelLatest
            // 
            this.labelLatest.AutoSize = true;
            this.labelLatest.Location = new System.Drawing.Point(161, 11);
            this.labelLatest.Name = "labelLatest";
            this.labelLatest.Size = new System.Drawing.Size(49, 12);
            this.labelLatest.TabIndex = 11;
            this.labelLatest.Text = "最新です";
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(186, 185);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "キャンセル";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(105, 185);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 4;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ReportToolStripMenuItem,
            this.ProxyToolStripMenuItem,
            this.DebugToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(168, 70);
            // 
            // ReportToolStripMenuItem
            // 
            this.ReportToolStripMenuItem.Name = "ReportToolStripMenuItem";
            this.ReportToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.ReportToolStripMenuItem.Text = "報告書設定(&R)";
            this.ReportToolStripMenuItem.Click += new System.EventHandler(this.ReportToolStripMenuItem_Click);
            // 
            // ProxyToolStripMenuItem
            // 
            this.ProxyToolStripMenuItem.Name = "ProxyToolStripMenuItem";
            this.ProxyToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.ProxyToolStripMenuItem.Text = "プロキシ設定(&P)";
            this.ProxyToolStripMenuItem.Click += new System.EventHandler(this.ProxyToolStripMenuItem_Click);
            // 
            // DebugToolStripMenuItem
            // 
            this.DebugToolStripMenuItem.Name = "DebugToolStripMenuItem";
            this.DebugToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.DebugToolStripMenuItem.Text = "デバッグ設定(&D)";
            this.DebugToolStripMenuItem.Click += new System.EventHandler(this.DebugToolStripMenuItem_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "オーディオファイル (*.wav;*.aif;*.aifc;*.aiff;*.wma;*.mp2;*.mp3)|*.wav;*.aif;*.aifc;*.aiff" +
    ";*.wma;*.mp2;*.mp3";
            this.openFileDialog.Title = "オーディオファイルを選択する";
            // 
            // ConfigDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(273, 217);
            this.ContextMenuStrip = this.contextMenuStrip;
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.tabControl);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "設定";
            this.Load += new System.EventHandler(this.ConfigDialog_Load);
            this.tabControl.ResumeLayout(false);
            this.tabPageShow.ResumeLayout(false);
            this.tabPageShow.PerformLayout();
            this.tabPageNotification.ResumeLayout(false);
            this.tabPageNotification.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginEquips)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginShips)).EndInit();
            this.tabPageAchievement.ResumeLayout(false);
            this.tabPageAchievement.PerformLayout();
            this.tabPageSound.ResumeLayout(false);
            this.tabPageSound.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSoundVolume)).EndInit();
            this.tabPageVersion.ResumeLayout(false);
            this.tabPageVersion.PerformLayout();
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageShow;
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
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox checkBoxReset14;
        private System.Windows.Forms.CheckBox checkBoxReset02;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.ListBox listBoxSoundFile;
        private System.Windows.Forms.Button buttonOpenFile;
        private System.Windows.Forms.TextBox textBoxSoundFile;
        private System.Windows.Forms.NumericUpDown numericUpDownSoundVolume;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.LinkLabel linkLabelProductName;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelLatest;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem ReportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ProxyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DebugToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}