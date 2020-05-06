﻿// Copyright (C) 2013, 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using KancolleSniffer.View.MainWindow;

namespace KancolleSniffer.Forms
{
    partial class HorizontalMainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HorizontalMainForm));
            this.labelAkashiRepairTimer = new System.Windows.Forms.Label();
            this.labelNDockCaption = new System.Windows.Forms.Label();
            this.labelConstruct = new System.Windows.Forms.Label();
            this.labelQuest = new System.Windows.Forms.Label();
            this.labelMissionCaption = new System.Windows.Forms.Label();
            this.notifyIconMain = new System.Windows.Forms.NotifyIcon(this.components);
            this.labelMaterialCaption = new System.Windows.Forms.Label();
            this.labelRepairListCaption = new System.Windows.Forms.Label();
            this.labelAkashiRepair = new System.Windows.Forms.Label();
            this.dropDownButtonRepairList = new KancolleSniffer.View.DropDownButton();
            this.labelQuestCount = new System.Windows.Forms.Label();
            this.kdockPanel = new KancolleSniffer.View.MainWindow.KDockPanel();
            this.panelRepairList = new KancolleSniffer.View.MainWindow.RepairListPanel();
            this.ndockPanel = new KancolleSniffer.View.MainWindow.NDockPanel();
            this.missionPanel = new KancolleSniffer.View.MainWindow.MissionPanel();
            this.questPanel = new KancolleSniffer.View.MainWindow.QuestPanel();
            this.hqPanel = new KancolleSniffer.View.MainWindow.HqPanel();
            this.materialHistoryPanel = new KancolleSniffer.View.MainWindow.MaterialHistoryPanel();
            this.dropDownButtonMaterialHistory = new KancolleSniffer.View.DropDownButton();
            this.mainFleetPanel = new KancolleSniffer.View.MainWindow.MainFleetPanel();
            this.SuspendLayout();
            // 
            // labelAkashiRepairTimer
            // 
            this.labelAkashiRepairTimer.Location = new System.Drawing.Point(630, 6);
            this.labelAkashiRepairTimer.Name = "labelAkashiRepairTimer";
            this.labelAkashiRepairTimer.Size = new System.Drawing.Size(32, 12);
            this.labelAkashiRepairTimer.TabIndex = 43;
            // 
            // labelNDockCaption
            // 
            this.labelNDockCaption.AutoSize = true;
            this.labelNDockCaption.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelNDockCaption.Location = new System.Drawing.Point(460, 73);
            this.labelNDockCaption.Name = "labelNDockCaption";
            this.labelNDockCaption.Size = new System.Drawing.Size(29, 12);
            this.labelNDockCaption.TabIndex = 3;
            this.labelNDockCaption.Text = "入渠";
            // 
            // labelConstruct
            // 
            this.labelConstruct.AutoSize = true;
            this.labelConstruct.Location = new System.Drawing.Point(603, 73);
            this.labelConstruct.Name = "labelConstruct";
            this.labelConstruct.Size = new System.Drawing.Size(29, 12);
            this.labelConstruct.TabIndex = 6;
            this.labelConstruct.Text = "建造";
            // 
            // labelQuest
            // 
            this.labelQuest.AutoSize = true;
            this.labelQuest.Location = new System.Drawing.Point(8, 43);
            this.labelQuest.Name = "labelQuest";
            this.labelQuest.Size = new System.Drawing.Size(29, 12);
            this.labelQuest.TabIndex = 8;
            this.labelQuest.Text = "任務";
            // 
            // labelMissionCaption
            // 
            this.labelMissionCaption.AutoSize = true;
            this.labelMissionCaption.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelMissionCaption.Location = new System.Drawing.Point(458, 6);
            this.labelMissionCaption.Name = "labelMissionCaption";
            this.labelMissionCaption.Size = new System.Drawing.Size(29, 12);
            this.labelMissionCaption.TabIndex = 10;
            this.labelMissionCaption.Text = "遠征";
            // 
            // notifyIconMain
            // 
            this.notifyIconMain.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconMain.Icon")));
            this.notifyIconMain.Text = "KancolleSniffer";
            this.notifyIconMain.Visible = true;
            // 
            // labelMaterialCaption
            // 
            this.labelMaterialCaption.AutoSize = true;
            this.labelMaterialCaption.Location = new System.Drawing.Point(183, 43);
            this.labelMaterialCaption.Name = "labelMaterialCaption";
            this.labelMaterialCaption.Size = new System.Drawing.Size(29, 12);
            this.labelMaterialCaption.TabIndex = 43;
            this.labelMaterialCaption.Text = "資材";
            // 
            // labelRepairListCaption
            // 
            this.labelRepairListCaption.AutoSize = true;
            this.labelRepairListCaption.Location = new System.Drawing.Point(533, 6);
            this.labelRepairListCaption.Name = "labelRepairListCaption";
            this.labelRepairListCaption.Size = new System.Drawing.Size(41, 12);
            this.labelRepairListCaption.TabIndex = 46;
            this.labelRepairListCaption.Text = "要修復";
            // 
            // labelAkashiRepair
            // 
            this.labelAkashiRepair.AutoSize = true;
            this.labelAkashiRepair.Location = new System.Drawing.Point(603, 6);
            this.labelAkashiRepair.Name = "labelAkashiRepair";
            this.labelAkashiRepair.Size = new System.Drawing.Size(29, 12);
            this.labelAkashiRepair.TabIndex = 54;
            this.labelAkashiRepair.Text = "修理";
            // 
            // dropDownButtonRepairList
            // 
            this.dropDownButtonRepairList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dropDownButtonRepairList.Location = new System.Drawing.Point(573, 4);
            this.dropDownButtonRepairList.Name = "dropDownButtonRepairList";
            this.dropDownButtonRepairList.Size = new System.Drawing.Size(14, 14);
            this.dropDownButtonRepairList.TabIndex = 45;
            // 
            // labelQuestCount
            // 
            this.labelQuestCount.AutoSize = true;
            this.labelQuestCount.Location = new System.Drawing.Point(35, 43);
            this.labelQuestCount.Name = "labelQuestCount";
            this.labelQuestCount.Size = new System.Drawing.Size(11, 12);
            this.labelQuestCount.TabIndex = 57;
            this.labelQuestCount.Text = "0";
            // 
            // kdockPanel
            // 
            this.kdockPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.kdockPanel.Location = new System.Drawing.Point(601, 87);
            this.kdockPanel.Name = "kdockPanel";
            this.kdockPanel.Size = new System.Drawing.Size(77, 64);
            this.kdockPanel.TabIndex = 60;
            // 
            // panelRepairList
            // 
            this.panelRepairList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelRepairList.Location = new System.Drawing.Point(458, 18);
            this.panelRepairList.Name = "panelRepairList";
            this.panelRepairList.Size = new System.Drawing.Size(129, 21);
            this.panelRepairList.TabIndex = 4;
            this.panelRepairList.Visible = false;
            // 
            // ndockPanel
            // 
            this.ndockPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ndockPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ndockPanel.Location = new System.Drawing.Point(458, 87);
            this.ndockPanel.Name = "ndockPanel";
            this.ndockPanel.Size = new System.Drawing.Size(140, 64);
            this.ndockPanel.TabIndex = 59;
            // 
            // missionPanel
            // 
            this.missionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.missionPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.missionPanel.Location = new System.Drawing.Point(458, 20);
            this.missionPanel.Name = "missionPanel";
            this.missionPanel.Size = new System.Drawing.Size(220, 49);
            this.missionPanel.TabIndex = 58;
            // 
            // questPanel
            // 
            this.questPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.questPanel.Location = new System.Drawing.Point(6, 57);
            this.questPanel.MinLines = 6;
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
            this.materialHistoryPanel.Location = new System.Drawing.Point(38, 55);
            this.materialHistoryPanel.Name = "materialHistoryPanel";
            this.materialHistoryPanel.Size = new System.Drawing.Size(188, 52);
            this.materialHistoryPanel.TabIndex = 65;
            this.materialHistoryPanel.Visible = false;
            // 
            // dropDownButtonMaterialHistory
            // 
            this.dropDownButtonMaterialHistory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dropDownButtonMaterialHistory.Location = new System.Drawing.Point(211, 41);
            this.dropDownButtonMaterialHistory.Name = "dropDownButtonMaterialHistory";
            this.dropDownButtonMaterialHistory.Size = new System.Drawing.Size(14, 14);
            this.dropDownButtonMaterialHistory.TabIndex = 80;
            this.dropDownButtonMaterialHistory.Text = "dropDownButton1";
            // 
            // mainFleetPanel
            // 
            this.mainFleetPanel.Context = null;
            this.mainFleetPanel.Location = new System.Drawing.Point(232, 3);
            this.mainFleetPanel.Name = "mainFleetPanel";
            this.mainFleetPanel.Size = new System.Drawing.Size(220, 148);
            this.mainFleetPanel.TabIndex = 82;
            // 
            // HorizontalMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 157);
            this.Controls.Add(this.labelMissionCaption);
            this.Controls.Add(this.panelRepairList);
            this.Controls.Add(this.labelAkashiRepair);
            this.Controls.Add(this.labelAkashiRepairTimer);
            this.Controls.Add(this.missionPanel);
            this.Controls.Add(this.mainFleetPanel);
            this.Controls.Add(this.dropDownButtonMaterialHistory);
            this.Controls.Add(this.materialHistoryPanel);
            this.Controls.Add(this.hqPanel);
            this.Controls.Add(this.kdockPanel);
            this.Controls.Add(this.ndockPanel);
            this.Controls.Add(this.labelQuestCount);
            this.Controls.Add(this.questPanel);
            this.Controls.Add(this.dropDownButtonRepairList);
            this.Controls.Add(this.labelRepairListCaption);
            this.Controls.Add(this.labelMaterialCaption);
            this.Controls.Add(this.labelQuest);
            this.Controls.Add(this.labelConstruct);
            this.Controls.Add(this.labelNDockCaption);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "HorizontalMainForm";
            this.Text = "KancolleSniffer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelNDockCaption;
        private System.Windows.Forms.Label labelConstruct;
        private System.Windows.Forms.Label labelQuest;
        private System.Windows.Forms.Label labelMissionCaption;
        private System.Windows.Forms.NotifyIcon notifyIconMain;
        private System.Windows.Forms.Label labelMaterialCaption;
        private DropDownButton dropDownButtonRepairList;
        private System.Windows.Forms.Label labelRepairListCaption;
        private System.Windows.Forms.Label labelAkashiRepairTimer;
        private System.Windows.Forms.Label labelAkashiRepair;
        private RepairListPanel panelRepairList;
        private QuestPanel questPanel;
        private System.Windows.Forms.Label labelQuestCount;
        private MissionPanel missionPanel;
        private NDockPanel ndockPanel;
        private KDockPanel kdockPanel;
        private HqPanel hqPanel;
        private MaterialHistoryPanel materialHistoryPanel;
        private DropDownButton dropDownButtonMaterialHistory;
        private MainFleetPanel mainFleetPanel;
    }
}

