// Copyright (C) 2013, 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

namespace KancolleSniffer
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.labelAkashiRepairTimer = new System.Windows.Forms.Label();
            this.labelNDockCaption = new System.Windows.Forms.Label();
            this.labelConstruct = new System.Windows.Forms.Label();
            this.labelQuest = new System.Windows.Forms.Label();
            this.labelMissionCaption = new System.Windows.Forms.Label();
            this.timerMain = new System.Windows.Forms.Timer(this.components);
            this.notifyIconMain = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStripNotifyIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.NotifyIconOpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NotifyIconExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripMain = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.listToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CaptureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelFleet1 = new System.Windows.Forms.Label();
            this.labelFleet4 = new System.Windows.Forms.Label();
            this.labelFleet3 = new System.Windows.Forms.Label();
            this.labelFleet2 = new System.Windows.Forms.Label();
            this.labelMaterialCaption = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.labelAkashiRepair = new System.Windows.Forms.Label();
            this.labelClearQuest = new System.Windows.Forms.Label();
            this.labelRepairListButton = new System.Windows.Forms.Label();
            this.labelMaterialHistoryButton = new System.Windows.Forms.Label();
            this.labelCheckFleet2 = new System.Windows.Forms.Label();
            this.labelCheckFleet3 = new System.Windows.Forms.Label();
            this.labelCheckFleet4 = new System.Windows.Forms.Label();
            this.labelCheckFleet1 = new System.Windows.Forms.Label();
            this.labelQuestCount = new System.Windows.Forms.Label();
            this.kdockPanel = new KancolleSniffer.View.KDockPanel();
            this.panelRepairList = new KancolleSniffer.View.RepairListForMain();
            this.ndockPanel = new KancolleSniffer.View.NDockPanel();
            this.missionPanel = new KancolleSniffer.View.MissionPanel();
            this.questPanel = new KancolleSniffer.View.QuestPanel();
            this.hqPanel = new KancolleSniffer.View.HqPanel();
            this.materialHistoryPanel = new KancolleSniffer.View.MaterialHistoryPanel();
            this.shipInfoPanel = new KancolleSniffer.View.ShipInfoPanel();
            this.chargeStatus1 = new KancolleSniffer.View.ChargeStatus();
            this.chargeStatus2 = new KancolleSniffer.View.ChargeStatus();
            this.chargeStatus3 = new KancolleSniffer.View.ChargeStatus();
            this.chargeStatus4 = new KancolleSniffer.View.ChargeStatus();
            this.contextMenuStripNotifyIcon.SuspendLayout();
            this.contextMenuStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelAkashiRepairTimer
            // 
            this.labelAkashiRepairTimer.Location = new System.Drawing.Point(179, 276);
            this.labelAkashiRepairTimer.Name = "labelAkashiRepairTimer";
            this.labelAkashiRepairTimer.Size = new System.Drawing.Size(32, 12);
            this.labelAkashiRepairTimer.TabIndex = 43;
            // 
            // labelNDockCaption
            // 
            this.labelNDockCaption.AutoSize = true;
            this.labelNDockCaption.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelNDockCaption.Location = new System.Drawing.Point(8, 195);
            this.labelNDockCaption.Name = "labelNDockCaption";
            this.labelNDockCaption.Size = new System.Drawing.Size(29, 12);
            this.labelNDockCaption.TabIndex = 3;
            this.labelNDockCaption.Text = "入渠";
            // 
            // labelConstruct
            // 
            this.labelConstruct.AutoSize = true;
            this.labelConstruct.Location = new System.Drawing.Point(151, 195);
            this.labelConstruct.Name = "labelConstruct";
            this.labelConstruct.Size = new System.Drawing.Size(29, 12);
            this.labelConstruct.TabIndex = 6;
            this.labelConstruct.Text = "建造";
            // 
            // labelQuest
            // 
            this.labelQuest.AutoSize = true;
            this.labelQuest.Location = new System.Drawing.Point(8, 342);
            this.labelQuest.Name = "labelQuest";
            this.labelQuest.Size = new System.Drawing.Size(29, 12);
            this.labelQuest.TabIndex = 8;
            this.labelQuest.Text = "任務";
            // 
            // labelMissionCaption
            // 
            this.labelMissionCaption.AutoSize = true;
            this.labelMissionCaption.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelMissionCaption.Location = new System.Drawing.Point(8, 276);
            this.labelMissionCaption.Name = "labelMissionCaption";
            this.labelMissionCaption.Size = new System.Drawing.Size(29, 12);
            this.labelMissionCaption.TabIndex = 10;
            this.labelMissionCaption.Text = "遠征";
            // 
            // timerMain
            // 
            this.timerMain.Enabled = true;
            this.timerMain.Interval = 1000;
            this.timerMain.Tick += new System.EventHandler(this.timerMain_Tick);
            // 
            // notifyIconMain
            // 
            this.notifyIconMain.ContextMenuStrip = this.contextMenuStripNotifyIcon;
            this.notifyIconMain.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconMain.Icon")));
            this.notifyIconMain.Text = "KancolleSniffer";
            this.notifyIconMain.Visible = true;
            this.notifyIconMain.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIconMain_MouseDoubleClick);
            // 
            // contextMenuStripNotifyIcon
            // 
            this.contextMenuStripNotifyIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NotifyIconOpenToolStripMenuItem,
            this.NotifyIconExitToolStripMenuItem});
            this.contextMenuStripNotifyIcon.Name = "contextMenuStripNotifyIcon";
            this.contextMenuStripNotifyIcon.Size = new System.Drawing.Size(122, 48);
            // 
            // NotifyIconOpenToolStripMenuItem
            // 
            this.NotifyIconOpenToolStripMenuItem.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.NotifyIconOpenToolStripMenuItem.Name = "NotifyIconOpenToolStripMenuItem";
            this.NotifyIconOpenToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.NotifyIconOpenToolStripMenuItem.Text = "開く(&O)";
            this.NotifyIconOpenToolStripMenuItem.Click += new System.EventHandler(this.NotifyIconOpenToolStripMenuItem_Click);
            // 
            // NotifyIconExitToolStripMenuItem
            // 
            this.NotifyIconExitToolStripMenuItem.Name = "NotifyIconExitToolStripMenuItem";
            this.NotifyIconExitToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.NotifyIconExitToolStripMenuItem.Text = "終了(&X)";
            this.NotifyIconExitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // contextMenuStripMain
            // 
            this.contextMenuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.listToolStripMenuItem,
            this.LogToolStripMenuItem,
            this.CaptureToolStripMenuItem,
            this.ConfigToolStripMenuItem,
            this.ExitToolStripMenuItem});
            this.contextMenuStripMain.Name = "contextMenuStripToolTip";
            this.contextMenuStripMain.Size = new System.Drawing.Size(126, 114);
            // 
            // listToolStripMenuItem
            // 
            this.listToolStripMenuItem.Name = "listToolStripMenuItem";
            this.listToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.listToolStripMenuItem.Text = "一覧(&L)";
            this.listToolStripMenuItem.Click += new System.EventHandler(this.ShipListToolStripMenuItem_Click);
            // 
            // LogToolStripMenuItem
            // 
            this.LogToolStripMenuItem.Name = "LogToolStripMenuItem";
            this.LogToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.LogToolStripMenuItem.Text = "報告書(&R)";
            this.LogToolStripMenuItem.Click += new System.EventHandler(this.LogToolStripMenuItem_Click);
            // 
            // CaptureToolStripMenuItem
            // 
            this.CaptureToolStripMenuItem.Name = "CaptureToolStripMenuItem";
            this.CaptureToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.CaptureToolStripMenuItem.Text = "撮影(&C)";
            this.CaptureToolStripMenuItem.Click += new System.EventHandler(this.CaptureToolStripMenuItem_Click);
            // 
            // ConfigToolStripMenuItem
            // 
            this.ConfigToolStripMenuItem.Name = "ConfigToolStripMenuItem";
            this.ConfigToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.ConfigToolStripMenuItem.Text = "設定(&O)";
            this.ConfigToolStripMenuItem.Click += new System.EventHandler(this.ConfigToolStripMenuItem_Click);
            // 
            // ExitToolStripMenuItem
            // 
            this.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem";
            this.ExitToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.ExitToolStripMenuItem.Text = "終了(&X)";
            this.ExitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // labelFleet1
            // 
            this.labelFleet1.Location = new System.Drawing.Point(12, 43);
            this.labelFleet1.Name = "labelFleet1";
            this.labelFleet1.Size = new System.Drawing.Size(45, 12);
            this.labelFleet1.TabIndex = 1;
            this.labelFleet1.Text = "第一";
            this.labelFleet1.MouseLeave += new System.EventHandler(this.labelFleet1_MouseLeave);
            this.labelFleet1.MouseHover += new System.EventHandler(this.labelFleet1_MouseHover);
            // 
            // labelFleet4
            // 
            this.labelFleet4.Location = new System.Drawing.Point(177, 43);
            this.labelFleet4.Name = "labelFleet4";
            this.labelFleet4.Size = new System.Drawing.Size(45, 12);
            this.labelFleet4.TabIndex = 17;
            this.labelFleet4.Text = "第四";
            // 
            // labelFleet3
            // 
            this.labelFleet3.Location = new System.Drawing.Point(122, 43);
            this.labelFleet3.Name = "labelFleet3";
            this.labelFleet3.Size = new System.Drawing.Size(45, 12);
            this.labelFleet3.TabIndex = 19;
            this.labelFleet3.Text = "第三";
            // 
            // labelFleet2
            // 
            this.labelFleet2.Location = new System.Drawing.Point(67, 43);
            this.labelFleet2.Name = "labelFleet2";
            this.labelFleet2.Size = new System.Drawing.Size(45, 12);
            this.labelFleet2.TabIndex = 21;
            this.labelFleet2.Text = "第二";
            // 
            // labelMaterialCaption
            // 
            this.labelMaterialCaption.AutoSize = true;
            this.labelMaterialCaption.Location = new System.Drawing.Point(183, 342);
            this.labelMaterialCaption.Name = "labelMaterialCaption";
            this.labelMaterialCaption.Size = new System.Drawing.Size(29, 12);
            this.labelMaterialCaption.TabIndex = 43;
            this.labelMaterialCaption.Text = "資材";
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(81, 195);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(41, 12);
            this.label31.TabIndex = 46;
            this.label31.Text = "要修復";
            this.label31.Click += new System.EventHandler(this.labelRepairListButton_Click);
            // 
            // labelAkashiRepair
            // 
            this.labelAkashiRepair.AutoSize = true;
            this.labelAkashiRepair.Location = new System.Drawing.Point(151, 276);
            this.labelAkashiRepair.Name = "labelAkashiRepair";
            this.labelAkashiRepair.Size = new System.Drawing.Size(29, 12);
            this.labelAkashiRepair.TabIndex = 54;
            this.labelAkashiRepair.Text = "修理";
            // 
            // labelClearQuest
            // 
            this.labelClearQuest.AutoSize = true;
            this.labelClearQuest.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelClearQuest.Location = new System.Drawing.Point(49, 340);
            this.labelClearQuest.Name = "labelClearQuest";
            this.labelClearQuest.Size = new System.Drawing.Size(15, 14);
            this.labelClearQuest.TabIndex = 55;
            this.labelClearQuest.Text = "↺";
            this.labelClearQuest.Click += new System.EventHandler(this.labelClearQuest_Click);
            this.labelClearQuest.MouseDown += new System.Windows.Forms.MouseEventHandler(this.labelClearQuest_MouseDown);
            this.labelClearQuest.MouseUp += new System.Windows.Forms.MouseEventHandler(this.labelClearQuest_MouseUp);
            // 
            // labelRepairListButton
            // 
            this.labelRepairListButton.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelRepairListButton.Image = global::KancolleSniffer.Properties.Resources.arrow_virtical;
            this.labelRepairListButton.Location = new System.Drawing.Point(121, 193);
            this.labelRepairListButton.Name = "labelRepairListButton";
            this.labelRepairListButton.Size = new System.Drawing.Size(14, 14);
            this.labelRepairListButton.TabIndex = 45;
            this.labelRepairListButton.Click += new System.EventHandler(this.labelRepairListButton_Click);
            // 
            // labelMaterialHistoryButton
            // 
            this.labelMaterialHistoryButton.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelMaterialHistoryButton.Image = global::KancolleSniffer.Properties.Resources.arrow_virtical;
            this.labelMaterialHistoryButton.Location = new System.Drawing.Point(211, 340);
            this.labelMaterialHistoryButton.Name = "labelMaterialHistoryButton";
            this.labelMaterialHistoryButton.Size = new System.Drawing.Size(14, 14);
            this.labelMaterialHistoryButton.TabIndex = 10;
            // 
            // labelCheckFleet2
            // 
            this.labelCheckFleet2.Image = global::KancolleSniffer.Properties.Resources.arrow;
            this.labelCheckFleet2.Location = new System.Drawing.Point(62, 42);
            this.labelCheckFleet2.Name = "labelCheckFleet2";
            this.labelCheckFleet2.Size = new System.Drawing.Size(5, 14);
            this.labelCheckFleet2.TabIndex = 22;
            this.labelCheckFleet2.Visible = false;
            // 
            // labelCheckFleet3
            // 
            this.labelCheckFleet3.Image = global::KancolleSniffer.Properties.Resources.arrow;
            this.labelCheckFleet3.Location = new System.Drawing.Point(117, 42);
            this.labelCheckFleet3.Name = "labelCheckFleet3";
            this.labelCheckFleet3.Size = new System.Drawing.Size(5, 14);
            this.labelCheckFleet3.TabIndex = 20;
            this.labelCheckFleet3.Visible = false;
            // 
            // labelCheckFleet4
            // 
            this.labelCheckFleet4.Image = global::KancolleSniffer.Properties.Resources.arrow;
            this.labelCheckFleet4.Location = new System.Drawing.Point(172, 42);
            this.labelCheckFleet4.Name = "labelCheckFleet4";
            this.labelCheckFleet4.Size = new System.Drawing.Size(5, 14);
            this.labelCheckFleet4.TabIndex = 18;
            this.labelCheckFleet4.Visible = false;
            // 
            // labelCheckFleet1
            // 
            this.labelCheckFleet1.Image = global::KancolleSniffer.Properties.Resources.arrow;
            this.labelCheckFleet1.Location = new System.Drawing.Point(7, 42);
            this.labelCheckFleet1.Name = "labelCheckFleet1";
            this.labelCheckFleet1.Size = new System.Drawing.Size(5, 14);
            this.labelCheckFleet1.TabIndex = 16;
            // 
            // labelQuestCount
            // 
            this.labelQuestCount.AutoSize = true;
            this.labelQuestCount.Location = new System.Drawing.Point(35, 342);
            this.labelQuestCount.Name = "labelQuestCount";
            this.labelQuestCount.Size = new System.Drawing.Size(11, 12);
            this.labelQuestCount.TabIndex = 57;
            this.labelQuestCount.Text = "0";
            // 
            // kdockPanel
            // 
            this.kdockPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.kdockPanel.Location = new System.Drawing.Point(149, 209);
            this.kdockPanel.Name = "kdockPanel";
            this.kdockPanel.Size = new System.Drawing.Size(77, 64);
            this.kdockPanel.TabIndex = 60;
            // 
            // panelRepairList
            // 
            this.panelRepairList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelRepairList.Location = new System.Drawing.Point(6, 207);
            this.panelRepairList.Name = "panelRepairList";
            this.panelRepairList.Size = new System.Drawing.Size(129, 21);
            this.panelRepairList.TabIndex = 4;
            this.panelRepairList.Visible = false;
            this.panelRepairList.Click += new System.EventHandler(this.panelRepairList_Click);
            // 
            // ndockPanel
            // 
            this.ndockPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ndockPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ndockPanel.Location = new System.Drawing.Point(6, 209);
            this.ndockPanel.Name = "ndockPanel";
            this.ndockPanel.Size = new System.Drawing.Size(140, 64);
            this.ndockPanel.TabIndex = 59;
            // 
            // missionPanel
            // 
            this.missionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.missionPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.missionPanel.Location = new System.Drawing.Point(6, 290);
            this.missionPanel.Name = "missionPanel";
            this.missionPanel.Size = new System.Drawing.Size(220, 49);
            this.missionPanel.TabIndex = 58;
            // 
            // questPanel
            // 
            this.questPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.questPanel.Location = new System.Drawing.Point(6, 356);
            this.questPanel.Name = "questPanel";
            this.questPanel.Size = new System.Drawing.Size(220, 94);
            this.questPanel.TabIndex = 56;
            // 
            // hqPanel
            // 
            this.hqPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.hqPanel.Location = new System.Drawing.Point(6, 6);
            this.hqPanel.Name = "hqPanel";
            this.hqPanel.Size = new System.Drawing.Size(220, 33);
            this.hqPanel.TabIndex = 61;
            // 
            // materialHistoryPanel
            // 
            this.materialHistoryPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.materialHistoryPanel.Location = new System.Drawing.Point(38, 354);
            this.materialHistoryPanel.Name = "materialHistoryPanel";
            this.materialHistoryPanel.Size = new System.Drawing.Size(188, 52);
            this.materialHistoryPanel.TabIndex = 65;
            this.materialHistoryPanel.Visible = false;
            // 
            // shipInfoPanel
            // 
            this.shipInfoPanel.AkashiRepairTimer = null;
            this.shipInfoPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.shipInfoPanel.CombinedFleet = false;
            this.shipInfoPanel.Context = null;
            this.shipInfoPanel.CurrentFleet = 0;
            this.shipInfoPanel.Location = new System.Drawing.Point(6, 57);
            this.shipInfoPanel.Name = "shipInfoPanel";
            this.shipInfoPanel.ShowShipOnList = null;
            this.shipInfoPanel.Size = new System.Drawing.Size(220, 134);
            this.shipInfoPanel.TabIndex = 67;
            // 
            // chargeStatus1
            // 
            this.chargeStatus1.Context = null;
            this.chargeStatus1.Location = new System.Drawing.Point(40, 42);
            this.chargeStatus1.Name = "chargeStatus1";
            this.chargeStatus1.Size = new System.Drawing.Size(17, 13);
            this.chargeStatus1.TabIndex = 69;
            this.chargeStatus1.Text = "chargeStatus1";
            // 
            // chargeStatus2
            // 
            this.chargeStatus2.Context = null;
            this.chargeStatus2.Location = new System.Drawing.Point(95, 42);
            this.chargeStatus2.Name = "chargeStatus2";
            this.chargeStatus2.Size = new System.Drawing.Size(17, 13);
            this.chargeStatus2.TabIndex = 71;
            this.chargeStatus2.Text = "chargeStatus2";
            // 
            // chargeStatus3
            // 
            this.chargeStatus3.Context = null;
            this.chargeStatus3.Location = new System.Drawing.Point(150, 42);
            this.chargeStatus3.Name = "chargeStatus3";
            this.chargeStatus3.Size = new System.Drawing.Size(17, 13);
            this.chargeStatus3.TabIndex = 72;
            this.chargeStatus3.Text = "chargeStatus3";
            // 
            // chargeStatus4
            // 
            this.chargeStatus4.Context = null;
            this.chargeStatus4.Location = new System.Drawing.Point(205, 42);
            this.chargeStatus4.Name = "chargeStatus4";
            this.chargeStatus4.Size = new System.Drawing.Size(17, 13);
            this.chargeStatus4.TabIndex = 73;
            this.chargeStatus4.Text = "chargeStatus4";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(232, 456);
            this.ContextMenuStrip = this.contextMenuStripMain;
            this.Controls.Add(this.chargeStatus3);
            this.Controls.Add(this.chargeStatus4);
            this.Controls.Add(this.chargeStatus2);
            this.Controls.Add(this.chargeStatus1);
            this.Controls.Add(this.shipInfoPanel);
            this.Controls.Add(this.materialHistoryPanel);
            this.Controls.Add(this.hqPanel);
            this.Controls.Add(this.kdockPanel);
            this.Controls.Add(this.panelRepairList);
            this.Controls.Add(this.ndockPanel);
            this.Controls.Add(this.missionPanel);
            this.Controls.Add(this.labelQuestCount);
            this.Controls.Add(this.questPanel);
            this.Controls.Add(this.labelClearQuest);
            this.Controls.Add(this.labelAkashiRepair);
            this.Controls.Add(this.labelAkashiRepairTimer);
            this.Controls.Add(this.labelRepairListButton);
            this.Controls.Add(this.label31);
            this.Controls.Add(this.labelMaterialHistoryButton);
            this.Controls.Add(this.labelMaterialCaption);
            this.Controls.Add(this.labelCheckFleet2);
            this.Controls.Add(this.labelFleet2);
            this.Controls.Add(this.labelCheckFleet3);
            this.Controls.Add(this.labelFleet3);
            this.Controls.Add(this.labelCheckFleet4);
            this.Controls.Add(this.labelFleet4);
            this.Controls.Add(this.labelCheckFleet1);
            this.Controls.Add(this.labelMissionCaption);
            this.Controls.Add(this.labelQuest);
            this.Controls.Add(this.labelConstruct);
            this.Controls.Add(this.labelNDockCaption);
            this.Controls.Add(this.labelFleet1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "KancolleSniffer";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.contextMenuStripNotifyIcon.ResumeLayout(false);
            this.contextMenuStripMain.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelNDockCaption;
        private System.Windows.Forms.Label labelConstruct;
        private System.Windows.Forms.Label labelQuest;
        private System.Windows.Forms.Label labelMissionCaption;
        private System.Windows.Forms.Timer timerMain;
        private System.Windows.Forms.NotifyIcon notifyIconMain;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripMain;
        private System.Windows.Forms.ToolStripMenuItem ConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExitToolStripMenuItem;
        private System.Windows.Forms.Label labelFleet1;
        private System.Windows.Forms.Label labelCheckFleet1;
        private System.Windows.Forms.Label labelCheckFleet4;
        private System.Windows.Forms.Label labelFleet4;
        private System.Windows.Forms.Label labelCheckFleet3;
        private System.Windows.Forms.Label labelFleet3;
        private System.Windows.Forms.Label labelCheckFleet2;
        private System.Windows.Forms.Label labelFleet2;
        private System.Windows.Forms.Label labelMaterialCaption;
        private System.Windows.Forms.Label labelMaterialHistoryButton;
        private System.Windows.Forms.Label labelRepairListButton;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripNotifyIcon;
        private System.Windows.Forms.ToolStripMenuItem NotifyIconOpenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem NotifyIconExitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem listToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem LogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CaptureToolStripMenuItem;
        private System.Windows.Forms.Label labelAkashiRepairTimer;
        private System.Windows.Forms.Label labelAkashiRepair;
        private RepairListForMain panelRepairList;
        private System.Windows.Forms.Label labelClearQuest;
        private QuestPanel questPanel;
        private System.Windows.Forms.Label labelQuestCount;
        private MissionPanel missionPanel;
        private NDockPanel ndockPanel;
        private KDockPanel kdockPanel;
        private HqPanel hqPanel;
        private MaterialHistoryPanel materialHistoryPanel;
        private ShipInfoPanel shipInfoPanel;
        private ChargeStatus chargeStatus1;
        private ChargeStatus chargeStatus2;
        private ChargeStatus chargeStatus3;
        private ChargeStatus chargeStatus4;
    }
}

