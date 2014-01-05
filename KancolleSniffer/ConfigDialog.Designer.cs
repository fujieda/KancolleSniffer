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
            this.groupBoxNotification = new System.Windows.Forms.GroupBox();
            this.numericUpDownMarginShips = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxSound = new System.Windows.Forms.CheckBox();
            this.checkBoxBalloon = new System.Windows.Forms.CheckBox();
            this.checkBoxFlash = new System.Windows.Forms.CheckBox();
            this.groupBoxSound = new System.Windows.Forms.GroupBox();
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
            this.groupBoxNotification.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginShips)).BeginInit();
            this.groupBoxSound.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSoundVolume)).BeginInit();
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
            this.groupBoxNotification.Location = new System.Drawing.Point(12, 12);
            this.groupBoxNotification.Name = "groupBoxNotification";
            this.groupBoxNotification.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxNotification.Size = new System.Drawing.Size(248, 119);
            this.groupBoxNotification.TabIndex = 0;
            this.groupBoxNotification.TabStop = false;
            this.groupBoxNotification.Text = "通知";
            // 
            // numericUpDownMarginShips
            // 
            this.numericUpDownMarginShips.Location = new System.Drawing.Point(125, 89);
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
            this.label2.Location = new System.Drawing.Point(161, 91);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "隻で通知する";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 91);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "最大保有艦数まで残り";
            // 
            // checkBoxSound
            // 
            this.checkBoxSound.AutoSize = true;
            this.checkBoxSound.Location = new System.Drawing.Point(11, 67);
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
            this.checkBoxBalloon.Location = new System.Drawing.Point(11, 45);
            this.checkBoxBalloon.Name = "checkBoxBalloon";
            this.checkBoxBalloon.Size = new System.Drawing.Size(172, 16);
            this.checkBoxBalloon.TabIndex = 1;
            this.checkBoxBalloon.Text = "通知領域にバルーンを表示する";
            this.checkBoxBalloon.UseVisualStyleBackColor = true;
            // 
            // checkBoxFlash
            // 
            this.checkBoxFlash.AutoSize = true;
            this.checkBoxFlash.Location = new System.Drawing.Point(11, 23);
            this.checkBoxFlash.Name = "checkBoxFlash";
            this.checkBoxFlash.Size = new System.Drawing.Size(127, 16);
            this.checkBoxFlash.TabIndex = 0;
            this.checkBoxFlash.Text = "ウィンドウを点滅させる";
            this.checkBoxFlash.UseVisualStyleBackColor = true;
            // 
            // groupBoxSound
            // 
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
            this.groupBoxSound.Location = new System.Drawing.Point(12, 146);
            this.groupBoxSound.Margin = new System.Windows.Forms.Padding(3, 12, 3, 12);
            this.groupBoxSound.Name = "groupBoxSound";
            this.groupBoxSound.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxSound.Size = new System.Drawing.Size(248, 155);
            this.groupBoxSound.TabIndex = 1;
            this.groupBoxSound.TabStop = false;
            this.groupBoxSound.Text = "サウンド";
            // 
            // numericUpDownSoundVolume
            // 
            this.numericUpDownSoundVolume.Location = new System.Drawing.Point(46, 23);
            this.numericUpDownSoundVolume.Name = "numericUpDownSoundVolume";
            this.numericUpDownSoundVolume.Size = new System.Drawing.Size(44, 19);
            this.numericUpDownSoundVolume.TabIndex = 1;
            this.numericUpDownSoundVolume.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "音量";
            // 
            // buttonMaxShipsOpenFile
            // 
            this.buttonMaxShipsOpenFile.Location = new System.Drawing.Point(196, 121);
            this.buttonMaxShipsOpenFile.Name = "buttonMaxShipsOpenFile";
            this.buttonMaxShipsOpenFile.Size = new System.Drawing.Size(41, 23);
            this.buttonMaxShipsOpenFile.TabIndex = 13;
            this.buttonMaxShipsOpenFile.Text = "参照";
            this.buttonMaxShipsOpenFile.UseVisualStyleBackColor = true;
            this.buttonMaxShipsOpenFile.Click += new System.EventHandler(this.buttonMaxShipsOpenFile_Click);
            // 
            // textBoxMaxShipsSoundFile
            // 
            this.textBoxMaxShipsSoundFile.Location = new System.Drawing.Point(46, 123);
            this.textBoxMaxShipsSoundFile.Name = "textBoxMaxShipsSoundFile";
            this.textBoxMaxShipsSoundFile.Size = new System.Drawing.Size(144, 19);
            this.textBoxMaxShipsSoundFile.TabIndex = 12;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 126);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 12);
            this.label7.TabIndex = 11;
            this.label7.Text = "艦数";
            // 
            // buttonKDockOpenFile
            // 
            this.buttonKDockOpenFile.Location = new System.Drawing.Point(196, 96);
            this.buttonKDockOpenFile.Name = "buttonKDockOpenFile";
            this.buttonKDockOpenFile.Size = new System.Drawing.Size(41, 23);
            this.buttonKDockOpenFile.TabIndex = 10;
            this.buttonKDockOpenFile.Text = "参照";
            this.buttonKDockOpenFile.UseVisualStyleBackColor = true;
            this.buttonKDockOpenFile.Click += new System.EventHandler(this.buttonKDockOpenFile_Click);
            // 
            // textBoxKDockSoundFile
            // 
            this.textBoxKDockSoundFile.Location = new System.Drawing.Point(46, 98);
            this.textBoxKDockSoundFile.Name = "textBoxKDockSoundFile";
            this.textBoxKDockSoundFile.Size = new System.Drawing.Size(144, 19);
            this.textBoxKDockSoundFile.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 101);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 12);
            this.label6.TabIndex = 8;
            this.label6.Text = "建造";
            // 
            // buttonNDockOpenFile
            // 
            this.buttonNDockOpenFile.Location = new System.Drawing.Point(196, 71);
            this.buttonNDockOpenFile.Name = "buttonNDockOpenFile";
            this.buttonNDockOpenFile.Size = new System.Drawing.Size(41, 23);
            this.buttonNDockOpenFile.TabIndex = 7;
            this.buttonNDockOpenFile.Text = "参照";
            this.buttonNDockOpenFile.UseVisualStyleBackColor = true;
            this.buttonNDockOpenFile.Click += new System.EventHandler(this.buttonNDockOpenFile_Click);
            // 
            // textBoxNDockSoundFile
            // 
            this.textBoxNDockSoundFile.Location = new System.Drawing.Point(46, 73);
            this.textBoxNDockSoundFile.Name = "textBoxNDockSoundFile";
            this.textBoxNDockSoundFile.Size = new System.Drawing.Size(144, 19);
            this.textBoxNDockSoundFile.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 76);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 5;
            this.label5.Text = "入渠";
            // 
            // buttonMissionOpenFile
            // 
            this.buttonMissionOpenFile.Location = new System.Drawing.Point(196, 46);
            this.buttonMissionOpenFile.Name = "buttonMissionOpenFile";
            this.buttonMissionOpenFile.Size = new System.Drawing.Size(41, 23);
            this.buttonMissionOpenFile.TabIndex = 4;
            this.buttonMissionOpenFile.Text = "参照";
            this.buttonMissionOpenFile.UseVisualStyleBackColor = true;
            this.buttonMissionOpenFile.Click += new System.EventHandler(this.buttonMissionOpenFile_Click);
            // 
            // textBoxMissionSoundFile
            // 
            this.textBoxMissionSoundFile.Location = new System.Drawing.Point(46, 48);
            this.textBoxMissionSoundFile.Name = "textBoxMissionSoundFile";
            this.textBoxMissionSoundFile.Size = new System.Drawing.Size(144, 19);
            this.textBoxMissionSoundFile.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 2;
            this.label4.Text = "遠征";
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(104, 316);
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
            this.buttonCancel.Location = new System.Drawing.Point(185, 316);
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
            // ConfigDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(272, 351);
            this.ControlBox = false;
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.groupBoxSound);
            this.Controls.Add(this.groupBoxNotification);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ConfigDialog";
            this.Text = "設定";
            this.Load += new System.EventHandler(this.ConfigDialog_Load);
            this.groupBoxNotification.ResumeLayout(false);
            this.groupBoxNotification.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMarginShips)).EndInit();
            this.groupBoxSound.ResumeLayout(false);
            this.groupBoxSound.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSoundVolume)).EndInit();
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
    }
}