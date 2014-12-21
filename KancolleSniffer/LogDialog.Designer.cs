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
    partial class LogDialog
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
            this.checkBoxOutput = new System.Windows.Forms.CheckBox();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonOutputDir = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownMaterialLogInterval = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.labelListen = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonServerOff = new System.Windows.Forms.RadioButton();
            this.radioButtonServerOn = new System.Windows.Forms.RadioButton();
            this.textBoxListen = new System.Windows.Forms.TextBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.folderBrowserDialogOutputDir = new System.Windows.Forms.FolderBrowserDialog();
            this.toolTipError = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaterialLogInterval)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBoxOutput
            // 
            this.checkBoxOutput.AutoSize = true;
            this.checkBoxOutput.Location = new System.Drawing.Point(6, 18);
            this.checkBoxOutput.Name = "checkBoxOutput";
            this.checkBoxOutput.Size = new System.Drawing.Size(67, 16);
            this.checkBoxOutput.TabIndex = 0;
            this.checkBoxOutput.Text = "出力する";
            this.checkBoxOutput.UseVisualStyleBackColor = true;
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Location = new System.Drawing.Point(45, 40);
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.Size = new System.Drawing.Size(127, 19);
            this.textBoxOutput.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "出力先";
            // 
            // buttonOutputDir
            // 
            this.buttonOutputDir.Location = new System.Drawing.Point(178, 39);
            this.buttonOutputDir.Name = "buttonOutputDir";
            this.buttonOutputDir.Size = new System.Drawing.Size(49, 21);
            this.buttonOutputDir.TabIndex = 3;
            this.buttonOutputDir.Text = "参照...";
            this.buttonOutputDir.UseVisualStyleBackColor = true;
            this.buttonOutputDir.Click += new System.EventHandler(this.buttonOutputDir_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "資材ログの間隔を";
            // 
            // numericUpDownMaterialLogInterval
            // 
            this.numericUpDownMaterialLogInterval.Location = new System.Drawing.Point(94, 65);
            this.numericUpDownMaterialLogInterval.Name = "numericUpDownMaterialLogInterval";
            this.numericUpDownMaterialLogInterval.Size = new System.Drawing.Size(44, 19);
            this.numericUpDownMaterialLogInterval.TabIndex = 5;
            this.numericUpDownMaterialLogInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(139, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "分開ける";
            // 
            // labelListen
            // 
            this.labelListen.AutoSize = true;
            this.labelListen.Location = new System.Drawing.Point(114, 20);
            this.labelListen.Name = "labelListen";
            this.labelListen.Size = new System.Drawing.Size(59, 12);
            this.labelListen.TabIndex = 8;
            this.labelListen.Text = "受信ポート:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButtonServerOff);
            this.groupBox1.Controls.Add(this.radioButtonServerOn);
            this.groupBox1.Controls.Add(this.textBoxListen);
            this.groupBox1.Controls.Add(this.labelListen);
            this.groupBox1.Location = new System.Drawing.Point(6, 104);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(234, 48);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "閲覧サーバー";
            // 
            // radioButtonServerOff
            // 
            this.radioButtonServerOff.AutoSize = true;
            this.radioButtonServerOff.Location = new System.Drawing.Point(59, 18);
            this.radioButtonServerOff.Name = "radioButtonServerOff";
            this.radioButtonServerOff.Size = new System.Drawing.Size(47, 16);
            this.radioButtonServerOff.TabIndex = 10;
            this.radioButtonServerOff.TabStop = true;
            this.radioButtonServerOff.Text = "無効";
            this.radioButtonServerOff.UseVisualStyleBackColor = true;
            // 
            // radioButtonServerOn
            // 
            this.radioButtonServerOn.AutoSize = true;
            this.radioButtonServerOn.Location = new System.Drawing.Point(6, 18);
            this.radioButtonServerOn.Name = "radioButtonServerOn";
            this.radioButtonServerOn.Size = new System.Drawing.Size(47, 16);
            this.radioButtonServerOn.TabIndex = 9;
            this.radioButtonServerOn.TabStop = true;
            this.radioButtonServerOn.Text = "有効";
            this.radioButtonServerOn.UseVisualStyleBackColor = true;
            this.radioButtonServerOn.CheckedChanged += new System.EventHandler(this.radioButtonServerOn_CheckedChanged);
            // 
            // textBoxListen
            // 
            this.textBoxListen.Location = new System.Drawing.Point(175, 17);
            this.textBoxListen.Name = "textBoxListen";
            this.textBoxListen.Size = new System.Drawing.Size(36, 19);
            this.textBoxListen.TabIndex = 11;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(165, 158);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 11;
            this.buttonCancel.Text = "キャンセル";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(84, 158);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 10;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxOutput);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.textBoxOutput);
            this.groupBox2.Controls.Add(this.buttonOutputDir);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.numericUpDownMaterialLogInterval);
            this.groupBox2.Location = new System.Drawing.Point(6, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(234, 92);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "報告書";
            // 
            // folderBrowserDialogOutputDir
            // 
            this.folderBrowserDialogOutputDir.Description = "報告書の出力先を指定します。";
            // 
            // toolTipError
            // 
            this.toolTipError.AutomaticDelay = 0;
            this.toolTipError.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Error;
            this.toolTipError.ToolTipTitle = "入力エラー";
            // 
            // LogDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(246, 190);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LogDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "報告書設定";
            this.Load += new System.EventHandler(this.LogDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaterialLogInterval)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxOutput;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonOutputDir;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownMaterialLogInterval;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelListen;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonServerOff;
        private System.Windows.Forms.RadioButton radioButtonServerOn;
        private System.Windows.Forms.TextBox textBoxListen;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogOutputDir;
        private System.Windows.Forms.ToolTip toolTipError;
    }
}