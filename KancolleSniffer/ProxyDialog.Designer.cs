namespace KancolleSniffer
{
    partial class ProxyDialog
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
            this.labelListen = new System.Windows.Forms.Label();
            this.textBoxListen = new System.Windows.Forms.TextBox();
            this.groupBoxAutoConfig = new System.Windows.Forms.GroupBox();
            this.radioButtonAutoConfigOff = new System.Windows.Forms.RadioButton();
            this.radioButtonAutoConfigOn = new System.Windows.Forms.RadioButton();
            this.groupBoxUpstream = new System.Windows.Forms.GroupBox();
            this.radioButtonUpstreamOff = new System.Windows.Forms.RadioButton();
            this.radioButtonUpstreamOn = new System.Windows.Forms.RadioButton();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.labelPort = new System.Windows.Forms.Label();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.toolTipError = new System.Windows.Forms.ToolTip(this.components);
            this.groupBoxAutoConfig.SuspendLayout();
            this.groupBoxUpstream.SuspendLayout();
            this.SuspendLayout();
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
            // textBoxListen
            // 
            this.textBoxListen.Location = new System.Drawing.Point(175, 18);
            this.textBoxListen.Name = "textBoxListen";
            this.textBoxListen.Size = new System.Drawing.Size(36, 19);
            this.textBoxListen.TabIndex = 3;
            this.textBoxListen.Enter += new System.EventHandler(this.textBox_Enter);
            // 
            // groupBoxAutoConfig
            // 
            this.groupBoxAutoConfig.Controls.Add(this.radioButtonAutoConfigOff);
            this.groupBoxAutoConfig.Controls.Add(this.radioButtonAutoConfigOn);
            this.groupBoxAutoConfig.Controls.Add(this.textBoxListen);
            this.groupBoxAutoConfig.Controls.Add(this.labelListen);
            this.groupBoxAutoConfig.Location = new System.Drawing.Point(6, 6);
            this.groupBoxAutoConfig.Name = "groupBoxAutoConfig";
            this.groupBoxAutoConfig.Size = new System.Drawing.Size(218, 48);
            this.groupBoxAutoConfig.TabIndex = 0;
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
            this.radioButtonAutoConfigOff.TabStop = true;
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
            this.radioButtonAutoConfigOn.TabStop = true;
            this.radioButtonAutoConfigOn.Text = "有効";
            this.radioButtonAutoConfigOn.UseVisualStyleBackColor = true;
            this.radioButtonAutoConfigOn.CheckedChanged += new System.EventHandler(this.radioButtonAutoConfigOn_CheckedChanged);
            // 
            // groupBoxUpstream
            // 
            this.groupBoxUpstream.Controls.Add(this.radioButtonUpstreamOff);
            this.groupBoxUpstream.Controls.Add(this.radioButtonUpstreamOn);
            this.groupBoxUpstream.Controls.Add(this.textBoxPort);
            this.groupBoxUpstream.Controls.Add(this.labelPort);
            this.groupBoxUpstream.Location = new System.Drawing.Point(6, 60);
            this.groupBoxUpstream.Name = "groupBoxUpstream";
            this.groupBoxUpstream.Size = new System.Drawing.Size(218, 48);
            this.groupBoxUpstream.TabIndex = 1;
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
            this.radioButtonUpstreamOn.TabStop = true;
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
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(68, 114);
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
            this.buttonCancel.Location = new System.Drawing.Point(149, 114);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "キャンセル";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // toolTipError
            // 
            this.toolTipError.AutomaticDelay = 0;
            this.toolTipError.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Error;
            this.toolTipError.ToolTipTitle = "入力エラー";
            // 
            // ProxyDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(230, 146);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.groupBoxUpstream);
            this.Controls.Add(this.groupBoxAutoConfig);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProxyDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "プロキシ設定";
            this.Load += new System.EventHandler(this.ProxyDialog_Load);
            this.groupBoxAutoConfig.ResumeLayout(false);
            this.groupBoxAutoConfig.PerformLayout();
            this.groupBoxUpstream.ResumeLayout(false);
            this.groupBoxUpstream.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelListen;
        private System.Windows.Forms.TextBox textBoxListen;
        private System.Windows.Forms.GroupBox groupBoxAutoConfig;
        private System.Windows.Forms.RadioButton radioButtonAutoConfigOff;
        private System.Windows.Forms.RadioButton radioButtonAutoConfigOn;
        private System.Windows.Forms.GroupBox groupBoxUpstream;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ToolTip toolTipError;
        private System.Windows.Forms.RadioButton radioButtonUpstreamOff;
        private System.Windows.Forms.RadioButton radioButtonUpstreamOn;


    }
}