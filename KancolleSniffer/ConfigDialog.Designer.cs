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
            this.numericUpDownMarginShips = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxSound = new System.Windows.Forms.CheckBox();
            this.checkBoxBalloon = new System.Windows.Forms.CheckBox();
            this.checkBoxFlash = new System.Windows.Forms.CheckBox();
            this.groupBoxSound = new System.Windows.Forms.GroupBox();
            this.buttonDamagedShipOpenFile = new System.Windows.Forms.Button();
            this.textBoxDamagedShipSoundFile = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.numericUpDownSoundVolume = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonMaxShipsOpenFile = new System.Windows.Forms.Button();
            this.textBoxMaxShipsSoundFile = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.buttonKDockOpenFile = new System.Windows.Forms.Button();
            this.textBoxKDockSoundFile = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonNDockOpenFile = new System.Windows.Forms.Button();
            this.textBoxNDockSoundFile = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonMissionOpenFile = new System.Windows.Forms.Button();
            this.textBoxMissionSoundFile = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.groupBoxShow = new System.Windows.Forms.GroupBox();
            this.checkBoxTopMost = new System.Windows.Forms.CheckBox();
            this.groupBoxAchievement = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.checkBoxReset14 = new System.Windows.Forms.CheckBox();
            this.checkBoxReset02 = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.DebugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxNotification.SuspendLayout();
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
            this.groupBoxNotification.Controls.Add(this.numericUpDownMarginShips);
            this.groupBoxNotification.Controls.Add(this.label2);
            this.groupBoxNotification.Controls.Add(this.label1);
            this.groupBoxNotification.Controls.Add(this.checkBoxSound);
            this.groupBoxNotification.Controls.Add(this.checkBoxBalloon);
            this.groupBoxNotification.Controls.Add(this.checkBoxFlash);
            this.groupBoxNotification.Location = new System.Drawing.Point(6, 50);
            this.groupBoxNotification.Name = "groupBoxNotification";
            this.groupBoxNotification.Size = new System.Drawing.Size(240, 108);
            this.groupBoxNotification.TabIndex = 0;
            this.groupBoxNotification.TabStop = false;
            this.groupBoxNotification.Text = "通知";
            // 
            // numericUpDownMarginShips
            // 
            this.numericUpDownMarginShips.Location = new System.Drawing.Point(120, 82);
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
            this.label2.Location = new System.Drawing.Point(156, 84);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "隻で通知する";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 84);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "最大保有艦数まで残り";
            // 
            // checkBoxSound
            // 
            this.checkBoxSound.AutoSize = true;
            this.checkBoxSound.Location = new System.Drawing.Point(6, 62);
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
            this.checkBoxBalloon.Location = new System.Drawing.Point(6, 40);
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
            this.groupBoxSound.Controls.Add(this.buttonDamagedShipOpenFile);
            this.groupBoxSound.Controls.Add(this.textBoxDamagedShipSoundFile);
            this.groupBoxSound.Controls.Add(this.label9);
            this.groupBoxSound.Controls.Add(this.numericUpDownSoundVolume);
            this.groupBoxSound.Controls.Add(this.label3);
            this.groupBoxSound.Controls.Add(this.buttonMaxShipsOpenFile);
            this.groupBoxSound.Controls.Add(this.textBoxMaxShipsSoundFile);
            this.groupBoxSound.Controls.Add(this.label7);
            this.groupBoxSound.Controls.Add(this.buttonKDockOpenFile);
            this.groupBoxSound.Controls.Add(this.textBoxKDockSoundFile);
            this.groupBoxSound.Controls.Add(this.label6);
            this.groupBoxSound.Controls.Add(this.buttonNDockOpenFile);
            this.groupBoxSound.Controls.Add(this.textBoxNDockSoundFile);
            this.groupBoxSound.Controls.Add(this.label5);
            this.groupBoxSound.Controls.Add(this.buttonMissionOpenFile);
            this.groupBoxSound.Controls.Add(this.textBoxMissionSoundFile);
            this.groupBoxSound.Controls.Add(this.label4);
            this.groupBoxSound.Location = new System.Drawing.Point(6, 208);
            this.groupBoxSound.Name = "groupBoxSound";
            this.groupBoxSound.Size = new System.Drawing.Size(240, 165);
            this.groupBoxSound.TabIndex = 1;
            this.groupBoxSound.TabStop = false;
            this.groupBoxSound.Text = "サウンド";
            // 
            // buttonDamagedShipOpenFile
            // 
            this.buttonDamagedShipOpenFile.Location = new System.Drawing.Point(191, 135);
            this.buttonDamagedShipOpenFile.Name = "buttonDamagedShipOpenFile";
            this.buttonDamagedShipOpenFile.Size = new System.Drawing.Size(41, 23);
            this.buttonDamagedShipOpenFile.TabIndex = 16;
            this.buttonDamagedShipOpenFile.Text = "参照";
            this.buttonDamagedShipOpenFile.UseVisualStyleBackColor = true;
            this.buttonDamagedShipOpenFile.Click += new System.EventHandler(this.buttonDamagedShipOpenFile_Click);
            // 
            // textBoxDamagedShipSoundFile
            // 
            this.textBoxDamagedShipSoundFile.Location = new System.Drawing.Point(41, 137);
            this.textBoxDamagedShipSoundFile.Name = "textBoxDamagedShipSoundFile";
            this.textBoxDamagedShipSoundFile.Size = new System.Drawing.Size(144, 19);
            this.textBoxDamagedShipSoundFile.TabIndex = 15;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 140);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 12);
            this.label9.TabIndex = 14;
            this.label9.Text = "大破";
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
            // buttonMaxShipsOpenFile
            // 
            this.buttonMaxShipsOpenFile.Location = new System.Drawing.Point(191, 111);
            this.buttonMaxShipsOpenFile.Name = "buttonMaxShipsOpenFile";
            this.buttonMaxShipsOpenFile.Size = new System.Drawing.Size(41, 23);
            this.buttonMaxShipsOpenFile.TabIndex = 13;
            this.buttonMaxShipsOpenFile.Text = "参照";
            this.buttonMaxShipsOpenFile.UseVisualStyleBackColor = true;
            this.buttonMaxShipsOpenFile.Click += new System.EventHandler(this.buttonMaxShipsOpenFile_Click);
            // 
            // textBoxMaxShipsSoundFile
            // 
            this.textBoxMaxShipsSoundFile.Location = new System.Drawing.Point(41, 113);
            this.textBoxMaxShipsSoundFile.Name = "textBoxMaxShipsSoundFile";
            this.textBoxMaxShipsSoundFile.Size = new System.Drawing.Size(144, 19);
            this.textBoxMaxShipsSoundFile.TabIndex = 12;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 116);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 12);
            this.label7.TabIndex = 11;
            this.label7.Text = "艦数";
            // 
            // buttonKDockOpenFile
            // 
            this.buttonKDockOpenFile.Location = new System.Drawing.Point(191, 87);
            this.buttonKDockOpenFile.Name = "buttonKDockOpenFile";
            this.buttonKDockOpenFile.Size = new System.Drawing.Size(41, 23);
            this.buttonKDockOpenFile.TabIndex = 10;
            this.buttonKDockOpenFile.Text = "参照";
            this.buttonKDockOpenFile.UseVisualStyleBackColor = true;
            this.buttonKDockOpenFile.Click += new System.EventHandler(this.buttonKDockOpenFile_Click);
            // 
            // textBoxKDockSoundFile
            // 
            this.textBoxKDockSoundFile.Location = new System.Drawing.Point(41, 89);
            this.textBoxKDockSoundFile.Name = "textBoxKDockSoundFile";
            this.textBoxKDockSoundFile.Size = new System.Drawing.Size(144, 19);
            this.textBoxKDockSoundFile.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 92);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 12);
            this.label6.TabIndex = 8;
            this.label6.Text = "建造";
            // 
            // buttonNDockOpenFile
            // 
            this.buttonNDockOpenFile.Location = new System.Drawing.Point(191, 63);
            this.buttonNDockOpenFile.Name = "buttonNDockOpenFile";
            this.buttonNDockOpenFile.Size = new System.Drawing.Size(41, 23);
            this.buttonNDockOpenFile.TabIndex = 7;
            this.buttonNDockOpenFile.Text = "参照";
            this.buttonNDockOpenFile.UseVisualStyleBackColor = true;
            this.buttonNDockOpenFile.Click += new System.EventHandler(this.buttonNDockOpenFile_Click);
            // 
            // textBoxNDockSoundFile
            // 
            this.textBoxNDockSoundFile.Location = new System.Drawing.Point(41, 65);
            this.textBoxNDockSoundFile.Name = "textBoxNDockSoundFile";
            this.textBoxNDockSoundFile.Size = new System.Drawing.Size(144, 19);
            this.textBoxNDockSoundFile.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 68);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 5;
            this.label5.Text = "入渠";
            // 
            // buttonMissionOpenFile
            // 
            this.buttonMissionOpenFile.Location = new System.Drawing.Point(191, 39);
            this.buttonMissionOpenFile.Name = "buttonMissionOpenFile";
            this.buttonMissionOpenFile.Size = new System.Drawing.Size(41, 23);
            this.buttonMissionOpenFile.TabIndex = 4;
            this.buttonMissionOpenFile.Text = "参照";
            this.buttonMissionOpenFile.UseVisualStyleBackColor = true;
            this.buttonMissionOpenFile.Click += new System.EventHandler(this.buttonMissionOpenFile_Click);
            // 
            // textBoxMissionSoundFile
            // 
            this.textBoxMissionSoundFile.Location = new System.Drawing.Point(41, 41);
            this.textBoxMissionSoundFile.Name = "textBoxMissionSoundFile";
            this.textBoxMissionSoundFile.Size = new System.Drawing.Size(144, 19);
            this.textBoxMissionSoundFile.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 44);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 2;
            this.label4.Text = "遠征";
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(90, 380);
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
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(171, 380);
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
            this.groupBoxShow.Controls.Add(this.checkBoxTopMost);
            this.groupBoxShow.Location = new System.Drawing.Point(6, 6);
            this.groupBoxShow.Name = "groupBoxShow";
            this.groupBoxShow.Size = new System.Drawing.Size(240, 40);
            this.groupBoxShow.TabIndex = 4;
            this.groupBoxShow.TabStop = false;
            this.groupBoxShow.Text = "表示";
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
            this.groupBoxAchievement.Controls.Add(this.label8);
            this.groupBoxAchievement.Controls.Add(this.checkBoxReset14);
            this.groupBoxAchievement.Controls.Add(this.checkBoxReset02);
            this.groupBoxAchievement.Location = new System.Drawing.Point(6, 162);
            this.groupBoxAchievement.Name = "groupBoxAchievement";
            this.groupBoxAchievement.Size = new System.Drawing.Size(240, 42);
            this.groupBoxAchievement.TabIndex = 5;
            this.groupBoxAchievement.TabStop = false;
            this.groupBoxAchievement.Text = "戦果";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 19);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(56, 12);
            this.label8.TabIndex = 3;
            this.label8.Text = "リセットする";
            // 
            // checkBoxReset14
            // 
            this.checkBoxReset14.AutoSize = true;
            this.checkBoxReset14.Location = new System.Drawing.Point(125, 18);
            this.checkBoxReset14.Name = "checkBoxReset14";
            this.checkBoxReset14.Size = new System.Drawing.Size(48, 16);
            this.checkBoxReset14.TabIndex = 2;
            this.checkBoxReset14.Text = "14時";
            this.checkBoxReset14.UseVisualStyleBackColor = true;
            // 
            // checkBoxReset02
            // 
            this.checkBoxReset02.AutoSize = true;
            this.checkBoxReset02.Location = new System.Drawing.Point(77, 18);
            this.checkBoxReset02.Name = "checkBoxReset02";
            this.checkBoxReset02.Size = new System.Drawing.Size(42, 16);
            this.checkBoxReset02.TabIndex = 1;
            this.checkBoxReset02.Text = "2時";
            this.checkBoxReset02.UseVisualStyleBackColor = true;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DebugToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(149, 26);
            // 
            // DebugToolStripMenuItem
            // 
            this.DebugToolStripMenuItem.Name = "DebugToolStripMenuItem";
            this.DebugToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.DebugToolStripMenuItem.Text = "デバッグ設定";
            this.DebugToolStripMenuItem.Click += new System.EventHandler(this.DebugToolStripMenuItem_Click);
            // 
            // ConfigDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(252, 412);
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
        private System.Windows.Forms.Button buttonKDockOpenFile;
        private System.Windows.Forms.TextBox textBoxKDockSoundFile;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonNDockOpenFile;
        private System.Windows.Forms.TextBox textBoxNDockSoundFile;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonMissionOpenFile;
        private System.Windows.Forms.TextBox textBoxMissionSoundFile;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonMaxShipsOpenFile;
        private System.Windows.Forms.TextBox textBoxMaxShipsSoundFile;
        private System.Windows.Forms.Label label7;
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
        private System.Windows.Forms.Button buttonDamagedShipOpenFile;
        private System.Windows.Forms.TextBox textBoxDamagedShipSoundFile;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem DebugToolStripMenuItem;
    }
}