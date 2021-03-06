﻿// Copyright (C) 2014 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

using KancolleSniffer.View;
using KancolleSniffer.View.ListWindow;
using KancolleSniffer.View.ShipListPanel;

namespace KancolleSniffer.Forms
{
    partial class ListForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ListForm));
            this.contextMenuStripShipList = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.shipCsvToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kantaiSarashiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripItemList = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.itemCsvToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kantaiBunsekiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelHeaderHp = new System.Windows.Forms.Label();
            this.labelHeaderCond = new System.Windows.Forms.Label();
            this.labelHeaderExp = new System.Windows.Forms.Label();
            this.labelHeaderLv = new System.Windows.Forms.Label();
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
            this.label1RepairHp = new System.Windows.Forms.Label();
            this.richTextBoxMiscText = new System.Windows.Forms.RichTextBox();
            this.contextMenuStripFleetData = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.fleetTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deckBuilderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelFleetHeader = new System.Windows.Forms.Panel();
            this.labelFleetBase = new System.Windows.Forms.Label();
            this.labelFleet4 = new System.Windows.Forms.Label();
            this.label1Fleet3 = new System.Windows.Forms.Label();
            this.labelFleet2 = new System.Windows.Forms.Label();
            this.labelFleet1 = new System.Windows.Forms.Label();
            this.panelSType = new System.Windows.Forms.Panel();
            this.checkBoxSTypeDetails = new System.Windows.Forms.CheckBox();
            this.checkBoxSTypeAll = new System.Windows.Forms.CheckBox();
            this.checkBoxSTypeAuxiliary = new System.Windows.Forms.CheckBox();
            this.checkBoxSTypeSubmarine = new System.Windows.Forms.CheckBox();
            this.checkBoxSTypeEscort = new System.Windows.Forms.CheckBox();
            this.checkBoxSTypeDestroyer = new System.Windows.Forms.CheckBox();
            this.checkBoxSTypeLightCruiser = new System.Windows.Forms.CheckBox();
            this.checkBoxSTypeHeavyCruiser = new System.Windows.Forms.CheckBox();
            this.checkBoxSTypeAircraftCarrier = new System.Windows.Forms.CheckBox();
            this.checkBoxSTypeBattleShip = new System.Windows.Forms.CheckBox();
            this.labelSType = new System.Windows.Forms.Label();
            this.panelShipHeader = new System.Windows.Forms.Panel();
            this.airBattleResultPanel = new KancolleSniffer.View.ListWindow.AirBattleResultPanel();
            this.battleResultPanel = new KancolleSniffer.View.ListWindow.BattleResultPanel();
            this.antiAirPanel = new KancolleSniffer.View.ListWindow.AntiAirPanel();
            this.dropDownButtonSType = new KancolleSniffer.View.DropDownButton();
            this.fleetPanel = new KancolleSniffer.View.ListWindow.FleetDataPanel();
            this.itemTreeView = new KancolleSniffer.View.ListWindow.ItemTreeView();
            this.shipListPanel = new KancolleSniffer.View.ShipListPanel.ShipListPanel();
            this.contextMenuStripShipList.SuspendLayout();
            this.contextMenuStripItemList.SuspendLayout();
            this.panelGroupHeader.SuspendLayout();
            this.panelRepairHeader.SuspendLayout();
            this.contextMenuStripFleetData.SuspendLayout();
            this.panelFleetHeader.SuspendLayout();
            this.panelSType.SuspendLayout();
            this.panelShipHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStripShipList
            // 
            this.contextMenuStripShipList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.shipCsvToolStripMenuItem,
            this.kantaiSarashiToolStripMenuItem});
            this.contextMenuStripShipList.Name = "contextMenuStripShipList";
            this.contextMenuStripShipList.Size = new System.Drawing.Size(193, 48);
            // 
            // shipCsvToolStripMenuItem
            // 
            this.shipCsvToolStripMenuItem.Name = "shipCsvToolStripMenuItem";
            this.shipCsvToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.shipCsvToolStripMenuItem.Text = "CSV形式でコピー(&C)";
            this.shipCsvToolStripMenuItem.Click += new System.EventHandler(this.shipCsvToolStripMenuItem_Click);
            // 
            // kantaiSarashiToolStripMenuItem
            // 
            this.kantaiSarashiToolStripMenuItem.Name = "kantaiSarashiToolStripMenuItem";
            this.kantaiSarashiToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.kantaiSarashiToolStripMenuItem.Text = "艦隊晒し形式でコピー(&K)";
            this.kantaiSarashiToolStripMenuItem.Click += new System.EventHandler(this.kantaiSarashiToolStripMenuItem_Click);
            // 
            // contextMenuStripItemList
            // 
            this.contextMenuStripItemList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemCsvToolStripMenuItem,
            this.kantaiBunsekiToolStripMenuItem});
            this.contextMenuStripItemList.Name = "contextMenuStrip";
            this.contextMenuStripItemList.Size = new System.Drawing.Size(197, 48);
            // 
            // itemCsvToolStripMenuItem
            // 
            this.itemCsvToolStripMenuItem.Name = "itemCsvToolStripMenuItem";
            this.itemCsvToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.itemCsvToolStripMenuItem.ShowShortcutKeys = false;
            this.itemCsvToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.itemCsvToolStripMenuItem.Text = "CSV形式でコピー(&C)";
            this.itemCsvToolStripMenuItem.Click += new System.EventHandler(this.itemCsvToolStripMenuItem_Click);
            // 
            // kantaiBunsekiToolStripMenuItem
            // 
            this.kantaiBunsekiToolStripMenuItem.Name = "kantaiBunsekiToolStripMenuItem";
            this.kantaiBunsekiToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.kantaiBunsekiToolStripMenuItem.Text = "艦隊分析形式でコピー(&K)";
            this.kantaiBunsekiToolStripMenuItem.Click += new System.EventHandler(this.kantaiBunsekiToolStripMenuItem_Click);
            // 
            // labelHeaderHp
            // 
            this.labelHeaderHp.AutoSize = true;
            this.labelHeaderHp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelHeaderHp.Location = new System.Drawing.Point(0, 5);
            this.labelHeaderHp.Name = "labelHeaderHp";
            this.labelHeaderHp.Size = new System.Drawing.Size(20, 12);
            this.labelHeaderHp.TabIndex = 13;
            this.labelHeaderHp.Text = "HP";
            this.labelHeaderHp.Click += new System.EventHandler(this.labelHeaderHp_Click);
            // 
            // labelHeaderCond
            // 
            this.labelHeaderCond.AutoSize = true;
            this.labelHeaderCond.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelHeaderCond.Location = new System.Drawing.Point(20, 5);
            this.labelHeaderCond.Name = "labelHeaderCond";
            this.labelHeaderCond.Size = new System.Drawing.Size(29, 12);
            this.labelHeaderCond.TabIndex = 13;
            this.labelHeaderCond.Text = "cond";
            this.labelHeaderCond.Click += new System.EventHandler(this.labelHeaderCond_Click);
            // 
            // labelHeaderExp
            // 
            this.labelHeaderExp.AutoSize = true;
            this.labelHeaderExp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelHeaderExp.Location = new System.Drawing.Point(86, 5);
            this.labelHeaderExp.Name = "labelHeaderExp";
            this.labelHeaderExp.Size = new System.Drawing.Size(24, 12);
            this.labelHeaderExp.TabIndex = 14;
            this.labelHeaderExp.Text = "Exp";
            this.labelHeaderExp.Click += new System.EventHandler(this.labelHeaderExp_Click);
            // 
            // labelHeaderLv
            // 
            this.labelHeaderLv.AutoSize = true;
            this.labelHeaderLv.Location = new System.Drawing.Point(55, 5);
            this.labelHeaderLv.Name = "labelHeaderLv";
            this.labelHeaderLv.Size = new System.Drawing.Size(17, 12);
            this.labelHeaderLv.TabIndex = 13;
            this.labelHeaderLv.Text = "Lv";
            // 
            // panelGroupHeader
            // 
            this.panelGroupHeader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelGroupHeader.Controls.Add(this.label5);
            this.panelGroupHeader.Controls.Add(this.label9);
            this.panelGroupHeader.Controls.Add(this.label8);
            this.panelGroupHeader.Controls.Add(this.label7);
            this.panelGroupHeader.Controls.Add(this.label6);
            this.panelGroupHeader.Location = new System.Drawing.Point(104, 3);
            this.panelGroupHeader.Name = "panelGroupHeader";
            this.panelGroupHeader.Size = new System.Drawing.Size(110, 19);
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
            this.comboBoxGroup.Location = new System.Drawing.Point(6, 4);
            this.comboBoxGroup.Name = "comboBoxGroup";
            this.comboBoxGroup.Size = new System.Drawing.Size(48, 20);
            this.comboBoxGroup.TabIndex = 1;
            this.comboBoxGroup.SelectedIndexChanged += new System.EventHandler(this.comboBoxGroup_SelectedIndexChanged);
            this.comboBoxGroup.DropDownClosed += new System.EventHandler(this.comboBoxGroup_DropDownClosed);
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
            this.panelRepairHeader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelRepairHeader.Controls.Add(this.label10);
            this.panelRepairHeader.Controls.Add(this.label12);
            this.panelRepairHeader.Controls.Add(this.label13);
            this.panelRepairHeader.Controls.Add(this.label1RepairHp);
            this.panelRepairHeader.Location = new System.Drawing.Point(104, 3);
            this.panelRepairHeader.Name = "panelRepairHeader";
            this.panelRepairHeader.Size = new System.Drawing.Size(118, 19);
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
            // label1RepairHp
            // 
            this.label1RepairHp.AutoSize = true;
            this.label1RepairHp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label1RepairHp.Location = new System.Drawing.Point(1, 5);
            this.label1RepairHp.Name = "label1RepairHp";
            this.label1RepairHp.Size = new System.Drawing.Size(20, 12);
            this.label1RepairHp.TabIndex = 17;
            this.label1RepairHp.Text = "HP";
            this.label1RepairHp.Click += new System.EventHandler(this.labelHeaderHp_Click);
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
            this.fleetTextToolStripMenuItem,
            this.deckBuilderToolStripMenuItem});
            this.contextMenuStripFleetData.Name = "contextMenuStripFleetData";
            this.contextMenuStripFleetData.Size = new System.Drawing.Size(211, 48);
            // 
            // fleetTextToolStripMenuItem
            // 
            this.fleetTextToolStripMenuItem.Name = "fleetTextToolStripMenuItem";
            this.fleetTextToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.fleetTextToolStripMenuItem.Text = "テキスト形式でコピー(&C)";
            this.fleetTextToolStripMenuItem.Click += new System.EventHandler(this.fleetTextToolStripMenuItem_Click);
            // 
            // deckBuilderToolStripMenuItem
            // 
            this.deckBuilderToolStripMenuItem.Name = "deckBuilderToolStripMenuItem";
            this.deckBuilderToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.deckBuilderToolStripMenuItem.Text = "デッキビルダー形式でコピー(&D)";
            this.deckBuilderToolStripMenuItem.Click += new System.EventHandler(this.deckBuilderToolStripMenuItem_Click);
            // 
            // panelFleetHeader
            // 
            this.panelFleetHeader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelFleetHeader.Controls.Add(this.labelFleetBase);
            this.panelFleetHeader.Controls.Add(this.labelFleet4);
            this.panelFleetHeader.Controls.Add(this.label1Fleet3);
            this.panelFleetHeader.Controls.Add(this.labelFleet2);
            this.panelFleetHeader.Controls.Add(this.labelFleet1);
            this.panelFleetHeader.Location = new System.Drawing.Point(56, 3);
            this.panelFleetHeader.Name = "panelFleetHeader";
            this.panelFleetHeader.Size = new System.Drawing.Size(169, 19);
            this.panelFleetHeader.TabIndex = 0;
            // 
            // labelFleetBase
            // 
            this.labelFleetBase.Location = new System.Drawing.Point(142, 1);
            this.labelFleetBase.Name = "labelFleetBase";
            this.labelFleetBase.Size = new System.Drawing.Size(29, 18);
            this.labelFleetBase.TabIndex = 4;
            this.labelFleetBase.Text = "基地";
            this.labelFleetBase.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelFleetBase.Click += new System.EventHandler(this.labelFleet_Click);
            // 
            // labelFleet4
            // 
            this.labelFleet4.Location = new System.Drawing.Point(111, 1);
            this.labelFleet4.Name = "labelFleet4";
            this.labelFleet4.Size = new System.Drawing.Size(29, 18);
            this.labelFleet4.TabIndex = 3;
            this.labelFleet4.Text = "第四";
            this.labelFleet4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelFleet4.Click += new System.EventHandler(this.labelFleet_Click);
            // 
            // label1Fleet3
            // 
            this.label1Fleet3.Location = new System.Drawing.Point(80, 1);
            this.label1Fleet3.Name = "label1Fleet3";
            this.label1Fleet3.Size = new System.Drawing.Size(29, 18);
            this.label1Fleet3.TabIndex = 2;
            this.label1Fleet3.Text = "第三";
            this.label1Fleet3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1Fleet3.Click += new System.EventHandler(this.labelFleet_Click);
            // 
            // labelFleet2
            // 
            this.labelFleet2.Location = new System.Drawing.Point(49, 1);
            this.labelFleet2.Name = "labelFleet2";
            this.labelFleet2.Size = new System.Drawing.Size(29, 18);
            this.labelFleet2.TabIndex = 1;
            this.labelFleet2.Text = "第二";
            this.labelFleet2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelFleet2.Click += new System.EventHandler(this.labelFleet_Click);
            // 
            // labelFleet1
            // 
            this.labelFleet1.Location = new System.Drawing.Point(19, 1);
            this.labelFleet1.Name = "labelFleet1";
            this.labelFleet1.Size = new System.Drawing.Size(29, 18);
            this.labelFleet1.TabIndex = 0;
            this.labelFleet1.Text = "第一";
            this.labelFleet1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelFleet1.Click += new System.EventHandler(this.labelFleet_Click);
            // 
            // panelSType
            // 
            this.panelSType.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSType.Controls.Add(this.checkBoxSTypeDetails);
            this.panelSType.Controls.Add(this.checkBoxSTypeAll);
            this.panelSType.Controls.Add(this.checkBoxSTypeAuxiliary);
            this.panelSType.Controls.Add(this.checkBoxSTypeSubmarine);
            this.panelSType.Controls.Add(this.checkBoxSTypeEscort);
            this.panelSType.Controls.Add(this.checkBoxSTypeDestroyer);
            this.panelSType.Controls.Add(this.checkBoxSTypeLightCruiser);
            this.panelSType.Controls.Add(this.checkBoxSTypeHeavyCruiser);
            this.panelSType.Controls.Add(this.checkBoxSTypeAircraftCarrier);
            this.panelSType.Controls.Add(this.checkBoxSTypeBattleShip);
            this.panelSType.Location = new System.Drawing.Point(56, 21);
            this.panelSType.Name = "panelSType";
            this.panelSType.Size = new System.Drawing.Size(188, 64);
            this.panelSType.TabIndex = 17;
            this.panelSType.Visible = false;
            this.panelSType.Click += new System.EventHandler(this.panelSType_Click);
            // 
            // checkBoxSTypeDetails
            // 
            this.checkBoxSTypeDetails.AutoSize = true;
            this.checkBoxSTypeDetails.Location = new System.Drawing.Point(49, 45);
            this.checkBoxSTypeDetails.Name = "checkBoxSTypeDetails";
            this.checkBoxSTypeDetails.Size = new System.Drawing.Size(48, 16);
            this.checkBoxSTypeDetails.TabIndex = 9;
            this.checkBoxSTypeDetails.Text = "種別";
            this.checkBoxSTypeDetails.UseVisualStyleBackColor = true;
            this.checkBoxSTypeDetails.Click += new System.EventHandler(this.checkBoxSTypeDetails_Click);
            // 
            // checkBoxSTypeAll
            // 
            this.checkBoxSTypeAll.AutoSize = true;
            this.checkBoxSTypeAll.Location = new System.Drawing.Point(3, 45);
            this.checkBoxSTypeAll.Name = "checkBoxSTypeAll";
            this.checkBoxSTypeAll.Size = new System.Drawing.Size(48, 16);
            this.checkBoxSTypeAll.TabIndex = 8;
            this.checkBoxSTypeAll.Text = "全部";
            this.checkBoxSTypeAll.UseVisualStyleBackColor = true;
            this.checkBoxSTypeAll.Click += new System.EventHandler(this.checkBoxSTypeAll_Click);
            // 
            // checkBoxSTypeAuxiliary
            // 
            this.checkBoxSTypeAuxiliary.AutoSize = true;
            this.checkBoxSTypeAuxiliary.Location = new System.Drawing.Point(140, 24);
            this.checkBoxSTypeAuxiliary.Name = "checkBoxSTypeAuxiliary";
            this.checkBoxSTypeAuxiliary.Size = new System.Drawing.Size(48, 16);
            this.checkBoxSTypeAuxiliary.TabIndex = 7;
            this.checkBoxSTypeAuxiliary.Text = "補助";
            this.checkBoxSTypeAuxiliary.UseVisualStyleBackColor = true;
            this.checkBoxSTypeAuxiliary.Click += new System.EventHandler(this.checkBoxSType_Click);
            // 
            // checkBoxSTypeSubmarine
            // 
            this.checkBoxSTypeSubmarine.AutoSize = true;
            this.checkBoxSTypeSubmarine.Location = new System.Drawing.Point(95, 24);
            this.checkBoxSTypeSubmarine.Name = "checkBoxSTypeSubmarine";
            this.checkBoxSTypeSubmarine.Size = new System.Drawing.Size(48, 16);
            this.checkBoxSTypeSubmarine.TabIndex = 6;
            this.checkBoxSTypeSubmarine.Text = "潜水";
            this.checkBoxSTypeSubmarine.UseVisualStyleBackColor = true;
            this.checkBoxSTypeSubmarine.Click += new System.EventHandler(this.checkBoxSType_Click);
            // 
            // checkBoxSTypeEscort
            // 
            this.checkBoxSTypeEscort.AutoSize = true;
            this.checkBoxSTypeEscort.Location = new System.Drawing.Point(49, 24);
            this.checkBoxSTypeEscort.Name = "checkBoxSTypeEscort";
            this.checkBoxSTypeEscort.Size = new System.Drawing.Size(48, 16);
            this.checkBoxSTypeEscort.TabIndex = 5;
            this.checkBoxSTypeEscort.Text = "海防";
            this.checkBoxSTypeEscort.UseVisualStyleBackColor = true;
            this.checkBoxSTypeEscort.Click += new System.EventHandler(this.checkBoxSType_Click);
            // 
            // checkBoxSTypeDestroyer
            // 
            this.checkBoxSTypeDestroyer.AutoSize = true;
            this.checkBoxSTypeDestroyer.Location = new System.Drawing.Point(3, 24);
            this.checkBoxSTypeDestroyer.Name = "checkBoxSTypeDestroyer";
            this.checkBoxSTypeDestroyer.Size = new System.Drawing.Size(48, 16);
            this.checkBoxSTypeDestroyer.TabIndex = 4;
            this.checkBoxSTypeDestroyer.Text = "駆逐";
            this.checkBoxSTypeDestroyer.UseVisualStyleBackColor = true;
            this.checkBoxSTypeDestroyer.Click += new System.EventHandler(this.checkBoxSType_Click);
            // 
            // checkBoxSTypeLightCruiser
            // 
            this.checkBoxSTypeLightCruiser.AutoSize = true;
            this.checkBoxSTypeLightCruiser.Location = new System.Drawing.Point(141, 3);
            this.checkBoxSTypeLightCruiser.Name = "checkBoxSTypeLightCruiser";
            this.checkBoxSTypeLightCruiser.Size = new System.Drawing.Size(48, 16);
            this.checkBoxSTypeLightCruiser.TabIndex = 3;
            this.checkBoxSTypeLightCruiser.Text = "軽巡";
            this.checkBoxSTypeLightCruiser.UseVisualStyleBackColor = true;
            this.checkBoxSTypeLightCruiser.Click += new System.EventHandler(this.checkBoxSType_Click);
            // 
            // checkBoxSTypeHeavyCruiser
            // 
            this.checkBoxSTypeHeavyCruiser.AutoSize = true;
            this.checkBoxSTypeHeavyCruiser.Location = new System.Drawing.Point(95, 3);
            this.checkBoxSTypeHeavyCruiser.Name = "checkBoxSTypeHeavyCruiser";
            this.checkBoxSTypeHeavyCruiser.Size = new System.Drawing.Size(48, 16);
            this.checkBoxSTypeHeavyCruiser.TabIndex = 2;
            this.checkBoxSTypeHeavyCruiser.Text = "重巡";
            this.checkBoxSTypeHeavyCruiser.UseVisualStyleBackColor = true;
            this.checkBoxSTypeHeavyCruiser.Click += new System.EventHandler(this.checkBoxSType_Click);
            // 
            // checkBoxSTypeAircraftCarrier
            // 
            this.checkBoxSTypeAircraftCarrier.AutoSize = true;
            this.checkBoxSTypeAircraftCarrier.Location = new System.Drawing.Point(49, 3);
            this.checkBoxSTypeAircraftCarrier.Name = "checkBoxSTypeAircraftCarrier";
            this.checkBoxSTypeAircraftCarrier.Size = new System.Drawing.Size(48, 16);
            this.checkBoxSTypeAircraftCarrier.TabIndex = 1;
            this.checkBoxSTypeAircraftCarrier.Text = "空母";
            this.checkBoxSTypeAircraftCarrier.UseVisualStyleBackColor = true;
            this.checkBoxSTypeAircraftCarrier.Click += new System.EventHandler(this.checkBoxSType_Click);
            // 
            // checkBoxSTypeBattleShip
            // 
            this.checkBoxSTypeBattleShip.AutoSize = true;
            this.checkBoxSTypeBattleShip.Location = new System.Drawing.Point(3, 3);
            this.checkBoxSTypeBattleShip.Name = "checkBoxSTypeBattleShip";
            this.checkBoxSTypeBattleShip.Size = new System.Drawing.Size(48, 16);
            this.checkBoxSTypeBattleShip.TabIndex = 0;
            this.checkBoxSTypeBattleShip.Text = "戦艦";
            this.checkBoxSTypeBattleShip.UseVisualStyleBackColor = true;
            this.checkBoxSTypeBattleShip.Click += new System.EventHandler(this.checkBoxSType_Click);
            // 
            // labelSType
            // 
            this.labelSType.AutoSize = true;
            this.labelSType.Location = new System.Drawing.Point(73, 8);
            this.labelSType.Name = "labelSType";
            this.labelSType.Size = new System.Drawing.Size(29, 12);
            this.labelSType.TabIndex = 20;
            this.labelSType.Text = "艦種";
            this.labelSType.Click += new System.EventHandler(this.labelSTypeButton_Click);
            // 
            // panelShipHeader
            // 
            this.panelShipHeader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelShipHeader.Controls.Add(this.labelHeaderHp);
            this.panelShipHeader.Controls.Add(this.labelHeaderCond);
            this.panelShipHeader.Controls.Add(this.labelHeaderLv);
            this.panelShipHeader.Controls.Add(this.labelHeaderExp);
            this.panelShipHeader.Location = new System.Drawing.Point(113, 3);
            this.panelShipHeader.Name = "panelShipHeader";
            this.panelShipHeader.Size = new System.Drawing.Size(113, 19);
            this.panelShipHeader.TabIndex = 21;
            // 
            // airBattleResultPanel
            // 
            this.airBattleResultPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.airBattleResultPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.airBattleResultPanel.Location = new System.Drawing.Point(6, 23);
            this.airBattleResultPanel.Name = "airBattleResultPanel";
            this.airBattleResultPanel.ShowResultAutomatic = false;
            this.airBattleResultPanel.Size = new System.Drawing.Size(236, 51);
            this.airBattleResultPanel.TabIndex = 18;
            // 
            // battleResultPanel
            // 
            this.battleResultPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.battleResultPanel.AutoScroll = true;
            this.battleResultPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.battleResultPanel.Location = new System.Drawing.Point(6, 73);
            this.battleResultPanel.Name = "battleResultPanel";
            this.battleResultPanel.Size = new System.Drawing.Size(236, 213);
            this.battleResultPanel.TabIndex = 0;
            // 
            // antiAirPanel
            // 
            this.antiAirPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.antiAirPanel.AutoScroll = true;
            this.antiAirPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.antiAirPanel.Location = new System.Drawing.Point(6, 23);
            this.antiAirPanel.Name = "antiAirPanel";
            this.antiAirPanel.Size = new System.Drawing.Size(236, 263);
            this.antiAirPanel.TabIndex = 17;
            // 
            // dropDownButtonSType
            // 
            this.dropDownButtonSType.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dropDownButtonSType.Location = new System.Drawing.Point(58, 7);
            this.dropDownButtonSType.Name = "dropDownButtonSType";
            this.dropDownButtonSType.Size = new System.Drawing.Size(14, 14);
            this.dropDownButtonSType.TabIndex = 19;
            this.dropDownButtonSType.Click += new System.EventHandler(this.labelSTypeButton_Click);
            // 
            // fleetPanel
            // 
            this.fleetPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fleetPanel.AutoScroll = true;
            this.fleetPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.fleetPanel.ContextMenuStrip = this.contextMenuStripFleetData;
            this.fleetPanel.Location = new System.Drawing.Point(6, 23);
            this.fleetPanel.Name = "fleetPanel";
            this.fleetPanel.Size = new System.Drawing.Size(236, 263);
            this.fleetPanel.TabIndex = 1;
            // 
            // itemTreeView
            // 
            this.itemTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.itemTreeView.ContextMenuStrip = this.contextMenuStripItemList;
            this.itemTreeView.Location = new System.Drawing.Point(6, 23);
            this.itemTreeView.Name = "itemTreeView";
            this.itemTreeView.Size = new System.Drawing.Size(236, 263);
            this.itemTreeView.TabIndex = 0;
            // 
            // shipListPanel
            // 
            this.shipListPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.shipListPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.shipListPanel.ContextMenuStrip = this.contextMenuStripShipList;
            this.shipListPanel.GroupSettings = null;
            this.shipListPanel.GroupUpdated = false;
            this.shipListPanel.Location = new System.Drawing.Point(6, 23);
            this.shipListPanel.Name = "shipListPanel";
            this.shipListPanel.Size = new System.Drawing.Size(236, 263);
            this.shipListPanel.TabIndex = 0;
            // 
            // ListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(248, 292);
            this.Controls.Add(this.panelSType);
            this.Controls.Add(this.panelFleetHeader);
            this.Controls.Add(this.airBattleResultPanel);
            this.Controls.Add(this.battleResultPanel);
            this.Controls.Add(this.antiAirPanel);
            this.Controls.Add(this.panelRepairHeader);
            this.Controls.Add(this.panelGroupHeader);
            this.Controls.Add(this.labelSType);
            this.Controls.Add(this.dropDownButtonSType);
            this.Controls.Add(this.panelShipHeader);
            this.Controls.Add(this.richTextBoxMiscText);
            this.Controls.Add(this.fleetPanel);
            this.Controls.Add(this.itemTreeView);
            this.Controls.Add(this.shipListPanel);
            this.Controls.Add(this.comboBoxGroup);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "ListForm";
            this.Text = "一覧";
            this.Activated += new System.EventHandler(this.ListForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ListForm_FormClosing);
            this.Load += new System.EventHandler(this.ListForm_Load);
            this.ResizeEnd += new System.EventHandler(this.ListForm_ResizeEnd);
            this.VisibleChanged += new System.EventHandler(this.ListForm_VisibleChanged);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ListForm_KeyPress);
            this.Resize += new System.EventHandler(this.ListForm_Resize);
            this.contextMenuStripShipList.ResumeLayout(false);
            this.contextMenuStripItemList.ResumeLayout(false);
            this.panelGroupHeader.ResumeLayout(false);
            this.panelGroupHeader.PerformLayout();
            this.panelRepairHeader.ResumeLayout(false);
            this.panelRepairHeader.PerformLayout();
            this.contextMenuStripFleetData.ResumeLayout(false);
            this.panelFleetHeader.ResumeLayout(false);
            this.panelSType.ResumeLayout(false);
            this.panelSType.PerformLayout();
            this.panelShipHeader.ResumeLayout(false);
            this.panelShipHeader.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ShipListPanel shipListPanel;
        private System.Windows.Forms.Label labelHeaderHp;
        private System.Windows.Forms.Label labelHeaderCond;
        private System.Windows.Forms.Label labelHeaderExp;
        private System.Windows.Forms.Label labelHeaderLv;
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
        private System.Windows.Forms.Label label1RepairHp;
        private FleetDataPanel fleetPanel;
        private ItemTreeView itemTreeView;
        private System.Windows.Forms.RichTextBox richTextBoxMiscText;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripItemList;
        private System.Windows.Forms.ToolStripMenuItem itemCsvToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFleetData;
        private System.Windows.Forms.ToolStripMenuItem fleetTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deckBuilderToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripShipList;
        private System.Windows.Forms.ToolStripMenuItem shipCsvToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem kantaiSarashiToolStripMenuItem;
        private System.Windows.Forms.Panel panelFleetHeader;
        private System.Windows.Forms.Label labelFleet4;
        private System.Windows.Forms.Label label1Fleet3;
        private System.Windows.Forms.Label labelFleet2;
        private System.Windows.Forms.Label labelFleet1;
        private AntiAirPanel antiAirPanel;
        private AirBattleResultPanel airBattleResultPanel;
        private BattleResultPanel battleResultPanel;
        private System.Windows.Forms.Panel panelSType;
        private System.Windows.Forms.CheckBox checkBoxSTypeAll;
        private System.Windows.Forms.CheckBox checkBoxSTypeAuxiliary;
        private System.Windows.Forms.CheckBox checkBoxSTypeSubmarine;
        private System.Windows.Forms.CheckBox checkBoxSTypeEscort;
        private System.Windows.Forms.CheckBox checkBoxSTypeDestroyer;
        private System.Windows.Forms.CheckBox checkBoxSTypeLightCruiser;
        private System.Windows.Forms.CheckBox checkBoxSTypeHeavyCruiser;
        private System.Windows.Forms.CheckBox checkBoxSTypeAircraftCarrier;
        private System.Windows.Forms.CheckBox checkBoxSTypeBattleShip;
        private DropDownButton dropDownButtonSType;
        private System.Windows.Forms.Label labelSType;
        private System.Windows.Forms.CheckBox checkBoxSTypeDetails;
        private System.Windows.Forms.ToolStripMenuItem kantaiBunsekiToolStripMenuItem;
        private System.Windows.Forms.Panel panelShipHeader;
        private System.Windows.Forms.Label labelFleetBase;
    }
}