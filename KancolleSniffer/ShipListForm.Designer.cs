// Copyright (C) 2014 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
    partial class ShipListForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShipListForm));
            this.panelShipList = new System.Windows.Forms.Panel();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBoxShipType = new System.Windows.Forms.CheckBox();
            this.panelGroupHeader = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxGroup = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.panelRepairHeader = new System.Windows.Forms.Panel();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.panelItemHeader = new System.Windows.Forms.Panel();
            this.richTextBoxMiscText = new System.Windows.Forms.RichTextBox();
            this.contextMenuStripFleetData = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.textToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deckBuilderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.equipPanel = new KancolleSniffer.EquipPanel();
            this.itemTreeView = new KancolleSniffer.ItemTreeView();
            this.contextMenuStrip.SuspendLayout();
            this.panelGroupHeader.SuspendLayout();
            this.panelRepairHeader.SuspendLayout();
            this.contextMenuStripFleetData.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelShipList
            // 
            this.panelShipList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panelShipList.AutoScroll = true;
            this.panelShipList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelShipList.ContextMenuStrip = this.contextMenuStrip;
            this.panelShipList.Location = new System.Drawing.Point(6, 23);
            this.panelShipList.Name = "panelShipList";
            this.panelShipList.Size = new System.Drawing.Size(238, 263);
            this.panelShipList.TabIndex = 0;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(153, 48);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.ShowShortcutKeys = false;
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.copyToolStripMenuItem.Text = "コピー(&C)";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(113, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 12);
            this.label1.TabIndex = 13;
            this.label1.Text = "HP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(132, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 13;
            this.label2.Text = "cond";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(200, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(24, 12);
            this.label4.TabIndex = 14;
            this.label4.Text = "Exp";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(168, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 13;
            this.label3.Text = "Lv";
            // 
            // checkBoxShipType
            // 
            this.checkBoxShipType.AutoSize = true;
            this.checkBoxShipType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxShipType.Location = new System.Drawing.Point(58, 6);
            this.checkBoxShipType.Name = "checkBoxShipType";
            this.checkBoxShipType.Size = new System.Drawing.Size(45, 16);
            this.checkBoxShipType.TabIndex = 15;
            this.checkBoxShipType.Text = "艦種";
            this.checkBoxShipType.CheckedChanged += new System.EventHandler(this.checkBoxShipType_CheckedChanged);
            // 
            // panelGroupHeader
            // 
            this.panelGroupHeader.Controls.Add(this.label5);
            this.panelGroupHeader.Controls.Add(this.label9);
            this.panelGroupHeader.Controls.Add(this.label8);
            this.panelGroupHeader.Controls.Add(this.label7);
            this.panelGroupHeader.Controls.Add(this.label6);
            this.panelGroupHeader.Location = new System.Drawing.Point(103, 3);
            this.panelGroupHeader.Name = "panelGroupHeader";
            this.panelGroupHeader.Size = new System.Drawing.Size(119, 19);
            this.panelGroupHeader.TabIndex = 16;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(1, 5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 12);
            this.label5.TabIndex = 5;
            this.label5.Text = "Lv";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(101, 5);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(13, 12);
            this.label9.TabIndex = 4;
            this.label9.Text = "D";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(77, 5);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(13, 12);
            this.label8.TabIndex = 3;
            this.label8.Text = "C";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(53, 5);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(13, 12);
            this.label7.TabIndex = 2;
            this.label7.Text = "B";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(29, 5);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(13, 12);
            this.label6.TabIndex = 1;
            this.label6.Text = "A";
            // 
            // comboBoxGroup
            // 
            this.comboBoxGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxGroup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxGroup.FormattingEnabled = true;
            this.comboBoxGroup.Items.AddRange(new object[] {
            "全員",
            "A",
            "B",
            "C",
            "D",
            "分類",
            "修復",
            "装備",
            "艦隊",
            "情報"});
            this.comboBoxGroup.Location = new System.Drawing.Point(6, 4);
            this.comboBoxGroup.Name = "comboBoxGroup";
            this.comboBoxGroup.Size = new System.Drawing.Size(48, 20);
            this.comboBoxGroup.TabIndex = 1;
            this.comboBoxGroup.SelectedIndexChanged += new System.EventHandler(this.comboBoxGroup_SelectedIndexChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(27, 5);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(17, 12);
            this.label12.TabIndex = 17;
            this.label12.Text = "Lv";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(60, 5);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(29, 12);
            this.label13.TabIndex = 18;
            this.label13.Text = "入渠";
            // 
            // panelRepairHeader
            // 
            this.panelRepairHeader.Controls.Add(this.label10);
            this.panelRepairHeader.Controls.Add(this.label12);
            this.panelRepairHeader.Controls.Add(this.label13);
            this.panelRepairHeader.Controls.Add(this.label11);
            this.panelRepairHeader.Location = new System.Drawing.Point(104, 3);
            this.panelRepairHeader.Name = "panelRepairHeader";
            this.panelRepairHeader.Size = new System.Drawing.Size(120, 19);
            this.panelRepairHeader.TabIndex = 2;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(94, 5);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(26, 12);
            this.label10.TabIndex = 19;
            this.label10.Text = "/HP";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(1, 5);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(20, 12);
            this.label11.TabIndex = 17;
            this.label11.Text = "HP";
            // 
            // panelItemHeader
            // 
            this.panelItemHeader.Location = new System.Drawing.Point(58, 3);
            this.panelItemHeader.Name = "panelItemHeader";
            this.panelItemHeader.Size = new System.Drawing.Size(166, 19);
            this.panelItemHeader.TabIndex = 0;
            // 
            // richTextBoxMiscText
            // 
            this.richTextBoxMiscText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxMiscText.Location = new System.Drawing.Point(6, 23);
            this.richTextBoxMiscText.Name = "richTextBoxMiscText";
            this.richTextBoxMiscText.ReadOnly = true;
            this.richTextBoxMiscText.Size = new System.Drawing.Size(236, 263);
            this.richTextBoxMiscText.TabIndex = 0;
            this.richTextBoxMiscText.Text = "";
            // 
            // contextMenuStripFleetData
            // 
            this.contextMenuStripFleetData.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.textToolStripMenuItem,
            this.deckBuilderToolStripMenuItem});
            this.contextMenuStripFleetData.Name = "contextMenuStripFleetData";
            this.contextMenuStripFleetData.Size = new System.Drawing.Size(252, 48);
            // 
            // textToolStripMenuItem
            // 
            this.textToolStripMenuItem.Name = "textToolStripMenuItem";
            this.textToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.textToolStripMenuItem.Text = "テキスト形式でコピー(&C)";
            this.textToolStripMenuItem.Click += new System.EventHandler(this.textToolStripMenuItem_Click);
            // 
            // deckBuilderToolStripMenuItem
            // 
            this.deckBuilderToolStripMenuItem.Name = "deckBuilderToolStripMenuItem";
            this.deckBuilderToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.deckBuilderToolStripMenuItem.Text = "デッキビルダー形式でコピー(&D)";
            this.deckBuilderToolStripMenuItem.Click += new System.EventHandler(this.deckBuilderToolStripMenuItem_Click);
            // 
            // equipPanel
            // 
            this.equipPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.equipPanel.AutoScroll = true;
            this.equipPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.equipPanel.ContextMenuStrip = this.contextMenuStripFleetData;
            this.equipPanel.Location = new System.Drawing.Point(6, 23);
            this.equipPanel.Name = "equipPanel";
            this.equipPanel.Size = new System.Drawing.Size(238, 263);
            this.equipPanel.TabIndex = 1;
            // 
            // itemTreeView
            // 
            this.itemTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.itemTreeView.ContextMenuStrip = this.contextMenuStrip;
            this.itemTreeView.Location = new System.Drawing.Point(6, 23);
            this.itemTreeView.Name = "itemTreeView";
            this.itemTreeView.Size = new System.Drawing.Size(238, 263);
            this.itemTreeView.TabIndex = 0;
            // 
            // ShipListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(250, 292);
            this.Controls.Add(this.panelItemHeader);
            this.Controls.Add(this.panelRepairHeader);
            this.Controls.Add(this.panelGroupHeader);
            this.Controls.Add(this.checkBoxShipType);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.richTextBoxMiscText);
            this.Controls.Add(this.equipPanel);
            this.Controls.Add(this.itemTreeView);
            this.Controls.Add(this.panelShipList);
            this.Controls.Add(this.comboBoxGroup);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "ShipListForm";
            this.Text = "一覧";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ShipListForm_FormClosing);
            this.Load += new System.EventHandler(this.ShipListForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ShipListForm_KeyPress);
            this.contextMenuStrip.ResumeLayout(false);
            this.panelGroupHeader.ResumeLayout(false);
            this.panelGroupHeader.PerformLayout();
            this.panelRepairHeader.ResumeLayout(false);
            this.panelRepairHeader.PerformLayout();
            this.contextMenuStripFleetData.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelShipList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxShipType;
        private System.Windows.Forms.Panel panelGroupHeader;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBoxGroup;
        private System.Windows.Forms.Panel panelRepairHeader;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Panel panelItemHeader;
        private EquipPanel equipPanel;
        private ItemTreeView itemTreeView;
        private System.Windows.Forms.RichTextBox richTextBoxMiscText;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFleetData;
        private System.Windows.Forms.ToolStripMenuItem textToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deckBuilderToolStripMenuItem;
    }
}