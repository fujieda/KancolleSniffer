namespace KancolleSniffer
{
    partial class NotificationConfigDialog
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
            this.listBoxNotifications = new System.Windows.Forms.ListBox();
            this.checkBoxFlashWindow = new System.Windows.Forms.CheckBox();
            this.checkBoxShowBaloonTip = new System.Windows.Forms.CheckBox();
            this.checkBoxPlaySound = new System.Windows.Forms.CheckBox();
            this.buttonClose = new System.Windows.Forms.Button();
            this.checkBoxPush = new System.Windows.Forms.CheckBox();
            this.checkBoxRepeat = new System.Windows.Forms.CheckBox();
            this.textBoxRepeat = new System.Windows.Forms.TextBox();
            this.labelRepeat = new System.Windows.Forms.Label();
            this.checkBoxCont = new System.Windows.Forms.CheckBox();
            this.textBoxPreliminary = new System.Windows.Forms.TextBox();
            this.labelPreliminary = new System.Windows.Forms.Label();
            this.checkBoxPreliminary = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // listBoxNotifications
            // 
            this.listBoxNotifications.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxNotifications.FormattingEnabled = true;
            this.listBoxNotifications.ItemHeight = 12;
            this.listBoxNotifications.Location = new System.Drawing.Point(12, 12);
            this.listBoxNotifications.Name = "listBoxNotifications";
            this.listBoxNotifications.Size = new System.Drawing.Size(264, 88);
            this.listBoxNotifications.TabIndex = 0;
            this.listBoxNotifications.SelectedIndexChanged += new System.EventHandler(this.listBoxNotifications_SelectedIndexChanged);
            // 
            // checkBoxFlashWindow
            // 
            this.checkBoxFlashWindow.AutoSize = true;
            this.checkBoxFlashWindow.Location = new System.Drawing.Point(12, 106);
            this.checkBoxFlashWindow.Name = "checkBoxFlashWindow";
            this.checkBoxFlashWindow.Size = new System.Drawing.Size(67, 16);
            this.checkBoxFlashWindow.TabIndex = 1;
            this.checkBoxFlashWindow.Text = "ウィンドウ";
            this.checkBoxFlashWindow.UseVisualStyleBackColor = true;
            this.checkBoxFlashWindow.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            // 
            // checkBoxShowBaloonTip
            // 
            this.checkBoxShowBaloonTip.AutoSize = true;
            this.checkBoxShowBaloonTip.Location = new System.Drawing.Point(81, 106);
            this.checkBoxShowBaloonTip.Name = "checkBoxShowBaloonTip";
            this.checkBoxShowBaloonTip.Size = new System.Drawing.Size(72, 16);
            this.checkBoxShowBaloonTip.TabIndex = 2;
            this.checkBoxShowBaloonTip.Text = "通知領域";
            this.checkBoxShowBaloonTip.UseVisualStyleBackColor = true;
            this.checkBoxShowBaloonTip.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            // 
            // checkBoxPlaySound
            // 
            this.checkBoxPlaySound.AutoSize = true;
            this.checkBoxPlaySound.Location = new System.Drawing.Point(155, 106);
            this.checkBoxPlaySound.Name = "checkBoxPlaySound";
            this.checkBoxPlaySound.Size = new System.Drawing.Size(61, 16);
            this.checkBoxPlaySound.TabIndex = 3;
            this.checkBoxPlaySound.Text = "サウンド";
            this.checkBoxPlaySound.UseVisualStyleBackColor = true;
            this.checkBoxPlaySound.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(201, 161);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 6;
            this.buttonClose.Text = "閉じる";
            this.buttonClose.UseVisualStyleBackColor = true;
            // 
            // checkBoxPush
            // 
            this.checkBoxPush.AutoSize = true;
            this.checkBoxPush.Location = new System.Drawing.Point(218, 106);
            this.checkBoxPush.Name = "checkBoxPush";
            this.checkBoxPush.Size = new System.Drawing.Size(58, 16);
            this.checkBoxPush.TabIndex = 7;
            this.checkBoxPush.Text = "プッシュ";
            this.checkBoxPush.UseVisualStyleBackColor = true;
            this.checkBoxPush.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            // 
            // checkBoxRepeat
            // 
            this.checkBoxRepeat.AutoSize = true;
            this.checkBoxRepeat.Location = new System.Drawing.Point(12, 133);
            this.checkBoxRepeat.Name = "checkBoxRepeat";
            this.checkBoxRepeat.Size = new System.Drawing.Size(58, 16);
            this.checkBoxRepeat.TabIndex = 8;
            this.checkBoxRepeat.Text = "リピート";
            this.checkBoxRepeat.UseVisualStyleBackColor = true;
            this.checkBoxRepeat.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            // 
            // textBoxRepeat
            // 
            this.textBoxRepeat.Location = new System.Drawing.Point(67, 131);
            this.textBoxRepeat.Name = "textBoxRepeat";
            this.textBoxRepeat.Size = new System.Drawing.Size(36, 19);
            this.textBoxRepeat.TabIndex = 9;
            this.textBoxRepeat.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxRepeat.TextChanged += new System.EventHandler(this.textBoxRepeat_TextChanged);
            // 
            // labelRepeat
            // 
            this.labelRepeat.AutoSize = true;
            this.labelRepeat.Location = new System.Drawing.Point(104, 134);
            this.labelRepeat.Name = "labelRepeat";
            this.labelRepeat.Size = new System.Drawing.Size(29, 12);
            this.labelRepeat.TabIndex = 10;
            this.labelRepeat.Text = "秒毎";
            // 
            // checkBoxCont
            // 
            this.checkBoxCont.AutoSize = true;
            this.checkBoxCont.Location = new System.Drawing.Point(66, 155);
            this.checkBoxCont.Name = "checkBoxCont";
            this.checkBoxCont.Size = new System.Drawing.Size(48, 16);
            this.checkBoxCont.TabIndex = 11;
            this.checkBoxCont.Text = "継続";
            this.checkBoxCont.UseVisualStyleBackColor = true;
            this.checkBoxCont.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            // 
            // textBoxPreliminary
            // 
            this.textBoxPreliminary.Location = new System.Drawing.Point(187, 131);
            this.textBoxPreliminary.Name = "textBoxPreliminary";
            this.textBoxPreliminary.Size = new System.Drawing.Size(36, 19);
            this.textBoxPreliminary.TabIndex = 12;
            this.textBoxPreliminary.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxPreliminary.TextChanged += new System.EventHandler(this.textBoxPreliminary_TextChanged);
            // 
            // labelPreliminary
            // 
            this.labelPreliminary.AutoSize = true;
            this.labelPreliminary.Location = new System.Drawing.Point(224, 134);
            this.labelPreliminary.Name = "labelPreliminary";
            this.labelPreliminary.Size = new System.Drawing.Size(29, 12);
            this.labelPreliminary.TabIndex = 13;
            this.labelPreliminary.Text = "秒前";
            // 
            // checkBoxPreliminary
            // 
            this.checkBoxPreliminary.AutoSize = true;
            this.checkBoxPreliminary.Location = new System.Drawing.Point(142, 133);
            this.checkBoxPreliminary.Name = "checkBoxPreliminary";
            this.checkBoxPreliminary.Size = new System.Drawing.Size(48, 16);
            this.checkBoxPreliminary.TabIndex = 14;
            this.checkBoxPreliminary.Text = "予告";
            this.checkBoxPreliminary.UseVisualStyleBackColor = true;
            this.checkBoxPreliminary.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
            // 
            // NotificationConfigDialog
            // 
            this.AcceptButton = this.buttonClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(288, 195);
            this.Controls.Add(this.textBoxPreliminary);
            this.Controls.Add(this.checkBoxPreliminary);
            this.Controls.Add(this.labelPreliminary);
            this.Controls.Add(this.checkBoxCont);
            this.Controls.Add(this.labelRepeat);
            this.Controls.Add(this.textBoxRepeat);
            this.Controls.Add(this.checkBoxRepeat);
            this.Controls.Add(this.checkBoxPush);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.checkBoxPlaySound);
            this.Controls.Add(this.checkBoxShowBaloonTip);
            this.Controls.Add(this.checkBoxFlashWindow);
            this.Controls.Add(this.listBoxNotifications);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NotificationConfigDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "通知方法の詳細設定";
            this.Load += new System.EventHandler(this.NotificationConfigDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxNotifications;
        private System.Windows.Forms.CheckBox checkBoxFlashWindow;
        private System.Windows.Forms.CheckBox checkBoxShowBaloonTip;
        private System.Windows.Forms.CheckBox checkBoxPlaySound;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.CheckBox checkBoxPush;
        private System.Windows.Forms.CheckBox checkBoxRepeat;
        private System.Windows.Forms.TextBox textBoxRepeat;
        private System.Windows.Forms.Label labelRepeat;
        private System.Windows.Forms.CheckBox checkBoxCont;
        private System.Windows.Forms.TextBox textBoxPreliminary;
        private System.Windows.Forms.Label labelPreliminary;
        private System.Windows.Forms.CheckBox checkBoxPreliminary;
    }
}