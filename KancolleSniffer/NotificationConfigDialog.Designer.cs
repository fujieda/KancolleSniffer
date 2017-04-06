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
            this.SuspendLayout();
            // 
            // listBoxNotifications
            // 
            this.listBoxNotifications.FormattingEnabled = true;
            this.listBoxNotifications.ItemHeight = 12;
            this.listBoxNotifications.Location = new System.Drawing.Point(12, 12);
            this.listBoxNotifications.Name = "listBoxNotifications";
            this.listBoxNotifications.Size = new System.Drawing.Size(204, 88);
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
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(141, 134);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 6;
            this.buttonClose.Text = "閉じる";
            this.buttonClose.UseVisualStyleBackColor = true;
            // 
            // NotificationConfigDialog
            // 
            this.AcceptButton = this.buttonClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(228, 166);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.checkBoxPlaySound);
            this.Controls.Add(this.checkBoxShowBaloonTip);
            this.Controls.Add(this.checkBoxFlashWindow);
            this.Controls.Add(this.listBoxNotifications);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NotificationConfigDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "イベントごとの通知方法の設定";
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
    }
}