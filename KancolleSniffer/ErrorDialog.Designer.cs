// Copyright (C) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

namespace KancolleSniffer
{
    partial class ErrorDialog
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
            this.textBoxDetails = new System.Windows.Forms.TextBox();
            this.buttonContinue = new System.Windows.Forms.Button();
            this.buttonExit = new System.Windows.Forms.Button();
            this.labelSystemIcon = new System.Windows.Forms.Label();
            this.labelMessage = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxDetails
            // 
            this.textBoxDetails.Font = new System.Drawing.Font("ＭＳ ゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.textBoxDetails.Location = new System.Drawing.Point(9, 73);
            this.textBoxDetails.Multiline = true;
            this.textBoxDetails.Name = "textBoxDetails";
            this.textBoxDetails.ReadOnly = true;
            this.textBoxDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxDetails.Size = new System.Drawing.Size(305, 144);
            this.textBoxDetails.TabIndex = 5;
            this.textBoxDetails.WordWrap = false;
            // 
            // buttonContinue
            // 
            this.buttonContinue.DialogResult = System.Windows.Forms.DialogResult.Ignore;
            this.buttonContinue.Location = new System.Drawing.Point(156, 230);
            this.buttonContinue.Name = "buttonContinue";
            this.buttonContinue.Size = new System.Drawing.Size(75, 23);
            this.buttonContinue.TabIndex = 3;
            this.buttonContinue.Text = "継続";
            this.buttonContinue.UseVisualStyleBackColor = true;
            // 
            // buttonExit
            // 
            this.buttonExit.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.buttonExit.Location = new System.Drawing.Point(237, 230);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(75, 23);
            this.buttonExit.TabIndex = 4;
            this.buttonExit.Text = "終了";
            this.buttonExit.UseVisualStyleBackColor = true;
            // 
            // labelSystemIcon
            // 
            this.labelSystemIcon.Location = new System.Drawing.Point(14, 12);
            this.labelSystemIcon.Name = "labelSystemIcon";
            this.labelSystemIcon.Size = new System.Drawing.Size(32, 32);
            this.labelSystemIcon.TabIndex = 0;
            // 
            // labelMessage
            // 
            this.labelMessage.Location = new System.Drawing.Point(57, 12);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(255, 32);
            this.labelMessage.TabIndex = 1;
            this.labelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "エラーの詳細";
            // 
            // ErrorDialog
            // 
            this.AcceptButton = this.buttonContinue;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 265);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.labelSystemIcon);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.buttonContinue);
            this.Controls.Add(this.textBoxDetails);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ErrorDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "エラー";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxDetails;
        private System.Windows.Forms.Button buttonContinue;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.Label labelSystemIcon;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.Label label1;
    }
}