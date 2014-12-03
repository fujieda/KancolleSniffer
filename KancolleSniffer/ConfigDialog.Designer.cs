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
            this.groupBoxNotification = new System.Windows.Forms.GroupBox();
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
            this.groupBoxSound = new System.Windows.Forms.GroupBox();
            this.buttonPlay = new System.Windows.Forms.Button();
            this.listBoxSoundFile = new System.Windows.Forms.ListBox();
            this.buttonOpenFile = new System.Windows.Forms.Button();
            this.textBoxSoundFile = new System.Windows.Forms.TextBox();
            this.numericUpDownSoundVolume = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.groupBoxShow = new System.Windows.Forms.GroupBox();
            this.checkBoxHideOnMinimized = new System.Windows.Forms.CheckBox();
            this.checkBoxTopMost = new System.Windows.Forms.CheckBox();
            this.groupBoxAchievement = new System.Windows.Forms.GroupBox();
            this.buttonResetAchievement = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.checkBoxReset14 = new System.Windows.Forms.CheckBox();
            this.checkBoxReset02 = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ProxyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DebugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxNotification.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginEquips)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginShips)).BeginInit();
            this.groupBoxSound.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSoundVolume)).BeginInit();
            this.groupBoxShow.SuspendLayout();
            this.groupBoxAchievement.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxNotification
            // 
            this.groupBoxNotification.Controls.Add(this.label6);
            this.groupBoxNotification.Controls.Add(this.checkBoxCond49);
            this.groupBoxNotification.Controls.Add(this.checkBoxCond40);
            this.groupBoxNotification.Controls.Add(this.label5);
            this.groupBoxNotification.Controls.Add(this.numericUpDownMarginEquips);
            this.groupBoxNotification.Controls.Add(this.label4);
            this.groupBoxNotification.Controls.Add(this.numericUpDownMarginShips);
            this.groupBoxNotification.Controls.Add(this.label2);
            this.groupBoxNotification.Controls.Add(this.label1);
            this.groupBoxNotification.Controls.Add(this.checkBoxSound);
            this.groupBoxNotification.Controls.Add(this.checkBoxBalloon);
            this.groupBoxNotification.Controls.Add(this.checkBoxFlash);
            this.groupBoxNotification.Location = new System.Drawing.Point(6, 71);
            this.groupBoxNotification.Name = "groupBoxNotification";
            this.groupBoxNotification.Size = new System.Drawing.Size(240, 147);
            this.groupBoxNotification.TabIndex = 0;
            this.groupBoxNotification.TabStop = false;
            this.groupBoxNotification.Text = "通知";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 126);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(105, 12);
            this.label6.TabIndex = 13;
            this.label6.Text = "疲労回復を通知する";
            // 
            // checkBoxCond49
            // 
            this.checkBoxCond49.AutoSize = true;
            this.checkBoxCond49.Location = new System.Drawing.Point(175, 125);
            this.checkBoxCond49.Name = "checkBoxCond49";
            this.checkBoxCond49.Size = new System.Drawing.Size(60, 16);
            this.checkBoxCond49.TabIndex = 12;
            this.checkBoxCond49.Text = "cond49";
            this.checkBoxCond49.UseVisualStyleBackColor = true;
            // 
            // checkBoxCond40
            // 
            this.checkBoxCond40.AutoSize = true;
            this.checkBoxCond40.Location = new System.Drawing.Point(113, 125);
            this.checkBoxCond40.Name = "checkBoxCond40";
            this.checkBoxCond40.Size = new System.Drawing.Size(60, 16);
            this.checkBoxCond40.TabIndex = 11;
            this.checkBoxCond40.Text = "cond40";
            this.checkBoxCond40.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(113, 103);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "個で通知する";
            // 
            // numericUpDownMarginEquips
            // 
            this.numericUpDownMarginEquips.Location = new System.Drawing.Point(76, 101);
            this.numericUpDownMarginEquips.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.numericUpDownMarginEquips.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numericUpDownMarginEquips.Name = "numericUpDownMarginEquips";
            this.numericUpDownMarginEquips.Size = new System.Drawing.Size(36, 19);
            this.numericUpDownMarginEquips.TabIndex = 8;
            this.numericUpDownMarginEquips.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(47, 103);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "装備";
            // 
            // numericUpDownMarginShips
            // 
            this.numericUpDownMarginShips.Location = new System.Drawing.Point(76, 81);
            this.numericUpDownMarginShips.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.numericUpDownMarginShips.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numericUpDownMarginShips.Name = "numericUpDownMarginShips";
            this.numericUpDownMarginShips.Size = new System.Drawing.Size(36, 19);
            this.numericUpDownMarginShips.TabIndex = 6;
            this.numericUpDownMarginShips.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(113, 83);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "隻";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 83);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "上限まで艦娘";
            // 
            // checkBoxSound
            // 
            this.checkBoxSound.AutoSize = true;
            this.checkBoxSound.Location = new System.Drawing.Point(6, 60);
            this.checkBoxSound.Name = "checkBoxSound";
            this.checkBoxSound.Size = new System.Drawing.Size(113, 16);
            this.checkBoxSound.TabIndex = 2;
            this.checkBoxSound.Text = "サウンドを再生する";
            this.checkBoxSound.UseVisualStyleBackColor = true;
            this.checkBoxSound.CheckedChanged += new System.EventHandler(this.checkBoxSound_CheckedChanged);
            // 
            // checkBoxBalloon
            // 
            this.checkBoxBalloon.AutoSize = true;
            this.checkBoxBalloon.Location = new System.Drawing.Point(6, 39);
            this.checkBoxBalloon.Name = "checkBoxBalloon";
            this.checkBoxBalloon.Size = new System.Drawing.Size(172, 16);
            this.checkBoxBalloon.TabIndex = 1;
            this.checkBoxBalloon.Text = "通知領域にバルーンを表示する";
            this.checkBoxBalloon.UseVisualStyleBackColor = true;
            // 
            // checkBoxFlash
            // 
            this.checkBoxFlash.AutoSize = true;
            this.checkBoxFlash.Location = new System.Drawing.Point(6, 18);
            this.checkBoxFlash.Name = "checkBoxFlash";
            this.checkBoxFlash.Size = new System.Drawing.Size(127, 16);
            this.checkBoxFlash.TabIndex = 0;
            this.checkBoxFlash.Text = "ウィンドウを点滅させる";
            this.checkBoxFlash.UseVisualStyleBackColor = true;
            // 
            // groupBoxSound
            // 
            this.groupBoxSound.Controls.Add(this.buttonPlay);
            this.groupBoxSound.Controls.Add(this.listBoxSoundFile);
            this.groupBoxSound.Controls.Add(this.buttonOpenFile);
            this.groupBoxSound.Controls.Add(this.textBoxSoundFile);
            this.groupBoxSound.Controls.Add(this.numericUpDownSoundVolume);
            this.groupBoxSound.Controls.Add(this.label3);
            this.groupBoxSound.Location = new System.Drawing.Point(6, 266);
            this.groupBoxSound.Name = "groupBoxSound";
            this.groupBoxSound.Size = new System.Drawing.Size(240, 138);
            this.groupBoxSound.TabIndex = 1;
            this.groupBoxSound.TabStop = false;
            this.groupBoxSound.Text = "サウンド";
            // 
            // buttonPlay
            // 
            this.buttonPlay.Location = new System.Drawing.Point(91, 13);
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.Size = new System.Drawing.Size(37, 23);
            this.buttonPlay.TabIndex = 18;
            this.buttonPlay.Text = "再生";
            this.buttonPlay.UseVisualStyleBackColor = true;
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            // 
            // listBoxSoundFile
            // 
            this.listBoxSoundFile.FormattingEnabled = true;
            this.listBoxSoundFile.ItemHeight = 12;
            this.listBoxSoundFile.Location = new System.Drawing.Point(6, 41);
            this.listBoxSoundFile.Name = "listBoxSoundFile";
            this.listBoxSoundFile.Size = new System.Drawing.Size(228, 64);
            this.listBoxSoundFile.TabIndex = 17;
            this.listBoxSoundFile.SelectedIndexChanged += new System.EventHandler(this.listBoxSoundFile_SelectedIndexChanged);
            // 
            // buttonOpenFile
            // 
            this.buttonOpenFile.Location = new System.Drawing.Point(189, 109);
            this.buttonOpenFile.Name = "buttonOpenFile";
            this.buttonOpenFile.Size = new System.Drawing.Size(45, 23);
            this.buttonOpenFile.TabIndex = 16;
            this.buttonOpenFile.Text = "参照...";
            this.buttonOpenFile.UseVisualStyleBackColor = true;
            this.buttonOpenFile.Click += new System.EventHandler(this.buttonOpenFile_Click);
            // 
            // textBoxSoundFile
            // 
            this.textBoxSoundFile.Location = new System.Drawing.Point(6, 111);
            this.textBoxSoundFile.Name = "textBoxSoundFile";
            this.textBoxSoundFile.Size = new System.Drawing.Size(179, 19);
            this.textBoxSoundFile.TabIndex = 15;
            this.textBoxSoundFile.TextChanged += new System.EventHandler(this.textBoxSoundFile_TextChanged);
            // 
            // numericUpDownSoundVolume
            // 
            this.numericUpDownSoundVolume.Location = new System.Drawing.Point(41, 16);
            this.numericUpDownSoundVolume.Name = "numericUpDownSoundVolume";
            this.numericUpDownSoundVolume.Size = new System.Drawing.Size(44, 19);
            this.numericUpDownSoundVolume.TabIndex = 1;
            this.numericUpDownSoundVolume.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 18);
            this.label3.Margin = new System.Windows.Forms.Padding(3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "音量";
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(87, 413);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 2;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(165, 413);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "キャンセル";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "オーディオファイル (*.wav;*.aif;*.aifc;*.aiff;*.wma;*.mp2;*.mp3)|*.wav;*.aif;*.aifc;*.aiff" +
    ";*.wma;*.mp2;*.mp3";
            this.openFileDialog.Title = "オーディオファイルを選択する";
            // 
            // groupBoxShow
            // 
            this.groupBoxShow.Controls.Add(this.checkBoxHideOnMinimized);
            this.groupBoxShow.Controls.Add(this.checkBoxTopMost);
            this.groupBoxShow.Location = new System.Drawing.Point(6, 6);
            this.groupBoxShow.Name = "groupBoxShow";
            this.groupBoxShow.Size = new System.Drawing.Size(240, 61);
            this.groupBoxShow.TabIndex = 4;
            this.groupBoxShow.TabStop = false;
            this.groupBoxShow.Text = "表示";
            // 
            // checkBoxHideOnMinimized
            // 
            this.checkBoxHideOnMinimized.AutoSize = true;
            this.checkBoxHideOnMinimized.Location = new System.Drawing.Point(6, 39);
            this.checkBoxHideOnMinimized.Name = "checkBoxHideOnMinimized";
            this.checkBoxHideOnMinimized.Size = new System.Drawing.Size(188, 16);
            this.checkBoxHideOnMinimized.TabIndex = 1;
            this.checkBoxHideOnMinimized.Text = "最小化時にタスクバーに表示しない";
            this.checkBoxHideOnMinimized.UseVisualStyleBackColor = true;
            // 
            // checkBoxTopMost
            // 
            this.checkBoxTopMost.AutoSize = true;
            this.checkBoxTopMost.Location = new System.Drawing.Point(6, 18);
            this.checkBoxTopMost.Name = "checkBoxTopMost";
            this.checkBoxTopMost.Size = new System.Drawing.Size(112, 16);
            this.checkBoxTopMost.TabIndex = 0;
            this.checkBoxTopMost.Text = "最前面に表示する";
            this.checkBoxTopMost.UseVisualStyleBackColor = true;
            // 
            // groupBoxAchievement
            // 
            this.groupBoxAchievement.Controls.Add(this.buttonResetAchievement);
            this.groupBoxAchievement.Controls.Add(this.label8);
            this.groupBoxAchievement.Controls.Add(this.checkBoxReset14);
            this.groupBoxAchievement.Controls.Add(this.checkBoxReset02);
            this.groupBoxAchievement.Location = new System.Drawing.Point(6, 222);
            this.groupBoxAchievement.Name = "groupBoxAchievement";
            this.groupBoxAchievement.Size = new System.Drawing.Size(240, 40);
            this.groupBoxAchievement.TabIndex = 5;
            this.groupBoxAchievement.TabStop = false;
            this.groupBoxAchievement.Text = "戦果";
            // 
            // buttonResetAchievement
            // 
            this.buttonResetAchievement.Location = new System.Drawing.Point(179, 14);
            this.buttonResetAchievement.Name = "buttonResetAchievement";
            this.buttonResetAchievement.Size = new System.Drawing.Size(44, 20);
            this.buttonResetAchievement.TabIndex = 4;
            this.buttonResetAchievement.Text = "今すぐ";
            this.buttonResetAchievement.UseVisualStyleBackColor = true;
            this.buttonResetAchievement.Click += new System.EventHandler(this.buttonResetAchievement_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 18);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(56, 12);
            this.label8.TabIndex = 3;
            this.label8.Text = "リセットする";
            // 
            // checkBoxReset14
            // 
            this.checkBoxReset14.AutoSize = true;
            this.checkBoxReset14.Location = new System.Drawing.Point(125, 17);
            this.checkBoxReset14.Name = "checkBoxReset14";
            this.checkBoxReset14.Size = new System.Drawing.Size(48, 16);
            this.checkBoxReset14.TabIndex = 2;
            this.checkBoxReset14.Text = "14時";
            this.checkBoxReset14.UseVisualStyleBackColor = true;
            // 
            // checkBoxReset02
            // 
            this.checkBoxReset02.AutoSize = true;
            this.checkBoxReset02.Location = new System.Drawing.Point(77, 17);
            this.checkBoxReset02.Name = "checkBoxReset02";
            this.checkBoxReset02.Size = new System.Drawing.Size(42, 16);
            this.checkBoxReset02.TabIndex = 1;
            this.checkBoxReset02.Text = "2時";
            this.checkBoxReset02.UseVisualStyleBackColor = true;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ProxyToolStripMenuItem,
            this.DebugToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(168, 48);
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
            // ConfigDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(252, 445);
            this.ContextMenuStrip = this.contextMenuStrip;
            this.Controls.Add(this.groupBoxAchievement);
            this.Controls.Add(this.groupBoxShow);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.groupBoxSound);
            this.Controls.Add(this.groupBoxNotification);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "設定";
            this.Load += new System.EventHandler(this.ConfigDialog_Load);
            this.groupBoxNotification.ResumeLayout(false);
            this.groupBoxNotification.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginEquips)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginShips)).EndInit();
            this.groupBoxSound.ResumeLayout(false);
            this.groupBoxSound.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSoundVolume)).EndInit();
            this.groupBoxShow.ResumeLayout(false);
            this.groupBoxShow.PerformLayout();
            this.groupBoxAchievement.ResumeLayout(false);
            this.groupBoxAchievement.PerformLayout();
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxNotification;
        private System.Windows.Forms.CheckBox checkBoxFlash;
        private System.Windows.Forms.CheckBox checkBoxBalloon;
        private System.Windows.Forms.CheckBox checkBoxSound;
        private System.Windows.Forms.GroupBox groupBoxSound;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownSoundVolume;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.NumericUpDown numericUpDownMarginShips;
        private System.Windows.Forms.GroupBox groupBoxShow;
        private System.Windows.Forms.CheckBox checkBoxTopMost;
        private System.Windows.Forms.GroupBox groupBoxAchievement;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox checkBoxReset14;
        private System.Windows.Forms.CheckBox checkBoxReset02;
        private System.Windows.Forms.Button buttonOpenFile;
        private System.Windows.Forms.TextBox textBoxSoundFile;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem DebugToolStripMenuItem;
        private System.Windows.Forms.Button buttonResetAchievement;
        private System.Windows.Forms.ToolStripMenuItem ProxyToolStripMenuItem;
        private System.Windows.Forms.ListBox listBoxSoundFile;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.CheckBox checkBoxHideOnMinimized;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDownMarginEquips;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBoxCond49;
        private System.Windows.Forms.CheckBox checkBoxCond40;
    }
}