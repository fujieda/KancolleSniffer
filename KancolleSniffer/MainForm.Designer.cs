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
            this.panelShipInfo = new System.Windows.Forms.Panel();
            this.labelPresetAkashiTimer = new System.Windows.Forms.Label();
            this.linkLabelGuide = new System.Windows.Forms.LinkLabel();
            this.panelCombinedFleet = new System.Windows.Forms.Panel();
            this.panel7Ships = new System.Windows.Forms.Panel();
            this.panelBattleInfo = new System.Windows.Forms.Panel();
            this.labelEnemyFighterPower = new System.Windows.Forms.Label();
            this.labelEnemyFighterPowerCaption = new System.Windows.Forms.Label();
            this.labelFormation = new System.Windows.Forms.Label();
            this.labelResultRank = new System.Windows.Forms.Label();
            this.labelLoS = new System.Windows.Forms.Label();
            this.labelLoSCaption = new System.Windows.Forms.Label();
            this.labelFighterPower = new System.Windows.Forms.Label();
            this.labelFighterPowerCaption = new System.Windows.Forms.Label();
            this.labelCondTimerTitle = new System.Windows.Forms.Label();
            this.labelCondTimer = new System.Windows.Forms.Label();
            this.labelAkashiRepairTimer = new System.Windows.Forms.Label();
            this.panelMaterialHistory = new System.Windows.Forms.Panel();
            this.labelBouxiteHistory = new System.Windows.Forms.Label();
            this.labelSteelHistory = new System.Windows.Forms.Label();
            this.labelBulletHistory = new System.Windows.Forms.Label();
            this.label35 = new System.Windows.Forms.Label();
            this.labelFuelHistory = new System.Windows.Forms.Label();
            this.labelNDock = new System.Windows.Forms.Label();
            this.labelConstruct = new System.Windows.Forms.Label();
            this.labelQuest = new System.Windows.Forms.Label();
            this.labelMission = new System.Windows.Forms.Label();
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
            this.label36 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.imageListFuelSq = new System.Windows.Forms.ImageList(this.components);
            this.imageListBullSq = new System.Windows.Forms.ImageList(this.components);
            this.labelAkashiRepair = new System.Windows.Forms.Label();
            this.labelClearQuest = new System.Windows.Forms.Label();
            this.labelBullSq4 = new System.Windows.Forms.Label();
            this.labelFuelSq4 = new System.Windows.Forms.Label();
            this.labelBullSq3 = new System.Windows.Forms.Label();
            this.labelFuelSq3 = new System.Windows.Forms.Label();
            this.labelBullSq2 = new System.Windows.Forms.Label();
            this.labelFuelSq2 = new System.Windows.Forms.Label();
            this.labelBullSq1 = new System.Windows.Forms.Label();
            this.labelFuelSq1 = new System.Windows.Forms.Label();
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
            this.panelShipInfo.SuspendLayout();
            this.panelBattleInfo.SuspendLayout();
            this.panelMaterialHistory.SuspendLayout();
            this.contextMenuStripNotifyIcon.SuspendLayout();
            this.contextMenuStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelShipInfo
            // 
            this.panelShipInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelShipInfo.Controls.Add(this.labelPresetAkashiTimer);
            this.panelShipInfo.Controls.Add(this.linkLabelGuide);
            this.panelShipInfo.Controls.Add(this.panelCombinedFleet);
            this.panelShipInfo.Controls.Add(this.panel7Ships);
            this.panelShipInfo.Controls.Add(this.panelBattleInfo);
            this.panelShipInfo.Controls.Add(this.labelLoS);
            this.panelShipInfo.Controls.Add(this.labelLoSCaption);
            this.panelShipInfo.Controls.Add(this.labelFighterPower);
            this.panelShipInfo.Controls.Add(this.labelFighterPowerCaption);
            this.panelShipInfo.Controls.Add(this.labelCondTimerTitle);
            this.panelShipInfo.Controls.Add(this.labelCondTimer);
            this.panelShipInfo.Location = new System.Drawing.Point(6, 57);
            this.panelShipInfo.Name = "panelShipInfo";
            this.panelShipInfo.Size = new System.Drawing.Size(220, 134);
            this.panelShipInfo.TabIndex = 2;
            // 
            // labelPresetAkashiTimer
            // 
            this.labelPresetAkashiTimer.Location = new System.Drawing.Point(2, 3);
            this.labelPresetAkashiTimer.Name = "labelPresetAkashiTimer";
            this.labelPresetAkashiTimer.Size = new System.Drawing.Size(32, 12);
            this.labelPresetAkashiTimer.TabIndex = 55;
            // 
            // linkLabelGuide
            // 
            this.linkLabelGuide.AutoSize = true;
            this.linkLabelGuide.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.linkLabelGuide.LinkArea = new System.Windows.Forms.LinkArea(0, 0);
            this.linkLabelGuide.Location = new System.Drawing.Point(31, 51);
            this.linkLabelGuide.Name = "linkLabelGuide";
            this.linkLabelGuide.Size = new System.Drawing.Size(158, 13);
            this.linkLabelGuide.TabIndex = 44;
            this.linkLabelGuide.Text = "右クリックでメニューが出ます。";
            // 
            // panelCombinedFleet
            // 
            this.panelCombinedFleet.Location = new System.Drawing.Point(0, 0);
            this.panelCombinedFleet.Name = "panelCombinedFleet";
            this.panelCombinedFleet.Size = new System.Drawing.Size(220, 113);
            this.panelCombinedFleet.TabIndex = 43;
            this.panelCombinedFleet.Visible = false;
            // 
            // panel7Ships
            // 
            this.panel7Ships.Location = new System.Drawing.Point(0, 0);
            this.panel7Ships.Name = "panel7Ships";
            this.panel7Ships.Size = new System.Drawing.Size(220, 115);
            this.panel7Ships.TabIndex = 0;
            this.panel7Ships.Visible = false;
            // 
            // panelBattleInfo
            // 
            this.panelBattleInfo.Controls.Add(this.labelEnemyFighterPower);
            this.panelBattleInfo.Controls.Add(this.labelEnemyFighterPowerCaption);
            this.panelBattleInfo.Controls.Add(this.labelFormation);
            this.panelBattleInfo.Controls.Add(this.labelResultRank);
            this.panelBattleInfo.Location = new System.Drawing.Point(59, 116);
            this.panelBattleInfo.Name = "panelBattleInfo";
            this.panelBattleInfo.Size = new System.Drawing.Size(157, 14);
            this.panelBattleInfo.TabIndex = 40;
            this.panelBattleInfo.Visible = false;
            // 
            // labelEnemyFighterPower
            // 
            this.labelEnemyFighterPower.Location = new System.Drawing.Point(129, 1);
            this.labelEnemyFighterPower.Name = "labelEnemyFighterPower";
            this.labelEnemyFighterPower.Size = new System.Drawing.Size(29, 12);
            this.labelEnemyFighterPower.TabIndex = 3;
            this.labelEnemyFighterPower.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelEnemyFighterPowerCaption
            // 
            this.labelEnemyFighterPowerCaption.AutoSize = true;
            this.labelEnemyFighterPowerCaption.Location = new System.Drawing.Point(90, 1);
            this.labelEnemyFighterPowerCaption.Name = "labelEnemyFighterPowerCaption";
            this.labelEnemyFighterPowerCaption.Size = new System.Drawing.Size(41, 12);
            this.labelEnemyFighterPowerCaption.TabIndex = 2;
            this.labelEnemyFighterPowerCaption.Text = "敵制空";
            // 
            // labelFormation
            // 
            this.labelFormation.Location = new System.Drawing.Point(40, 1);
            this.labelFormation.Name = "labelFormation";
            this.labelFormation.Size = new System.Drawing.Size(48, 12);
            this.labelFormation.TabIndex = 1;
            // 
            // labelResultRank
            // 
            this.labelResultRank.Location = new System.Drawing.Point(1, 1);
            this.labelResultRank.Name = "labelResultRank";
            this.labelResultRank.Size = new System.Drawing.Size(42, 12);
            this.labelResultRank.TabIndex = 0;
            this.labelResultRank.Text = "判定";
            this.labelResultRank.Click += new System.EventHandler(this.labelResultRank_Click);
            // 
            // labelLoS
            // 
            this.labelLoS.Location = new System.Drawing.Point(85, 117);
            this.labelLoS.Name = "labelLoS";
            this.labelLoS.Size = new System.Drawing.Size(38, 12);
            this.labelLoS.TabIndex = 42;
            this.labelLoS.Text = "0.0";
            this.labelLoS.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelLoSCaption
            // 
            this.labelLoSCaption.AutoSize = true;
            this.labelLoSCaption.Location = new System.Drawing.Point(59, 117);
            this.labelLoSCaption.Name = "labelLoSCaption";
            this.labelLoSCaption.Size = new System.Drawing.Size(29, 12);
            this.labelLoSCaption.TabIndex = 41;
            this.labelLoSCaption.Text = "索敵";
            // 
            // labelFighterPower
            // 
            this.labelFighterPower.Location = new System.Drawing.Point(28, 117);
            this.labelFighterPower.Name = "labelFighterPower";
            this.labelFighterPower.Size = new System.Drawing.Size(29, 12);
            this.labelFighterPower.TabIndex = 23;
            this.labelFighterPower.Text = "0";
            this.labelFighterPower.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelFighterPowerCaption
            // 
            this.labelFighterPowerCaption.AutoSize = true;
            this.labelFighterPowerCaption.Location = new System.Drawing.Point(2, 117);
            this.labelFighterPowerCaption.Name = "labelFighterPowerCaption";
            this.labelFighterPowerCaption.Size = new System.Drawing.Size(29, 12);
            this.labelFighterPowerCaption.TabIndex = 23;
            this.labelFighterPowerCaption.Text = "制空";
            // 
            // labelCondTimerTitle
            // 
            this.labelCondTimerTitle.Location = new System.Drawing.Point(128, 117);
            this.labelCondTimerTitle.Name = "labelCondTimerTitle";
            this.labelCondTimerTitle.Size = new System.Drawing.Size(60, 12);
            this.labelCondTimerTitle.TabIndex = 39;
            // 
            // labelCondTimer
            // 
            this.labelCondTimer.AutoSize = true;
            this.labelCondTimer.Location = new System.Drawing.Point(186, 117);
            this.labelCondTimer.Name = "labelCondTimer";
            this.labelCondTimer.Size = new System.Drawing.Size(0, 12);
            this.labelCondTimer.TabIndex = 38;
            // 
            // labelAkashiRepairTimer
            // 
            this.labelAkashiRepairTimer.Location = new System.Drawing.Point(179, 276);
            this.labelAkashiRepairTimer.Name = "labelAkashiRepairTimer";
            this.labelAkashiRepairTimer.Size = new System.Drawing.Size(32, 12);
            this.labelAkashiRepairTimer.TabIndex = 43;
            // 
            // panelMaterialHistory
            // 
            this.panelMaterialHistory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMaterialHistory.Controls.Add(this.labelBouxiteHistory);
            this.panelMaterialHistory.Controls.Add(this.labelSteelHistory);
            this.panelMaterialHistory.Controls.Add(this.labelBulletHistory);
            this.panelMaterialHistory.Controls.Add(this.label35);
            this.panelMaterialHistory.Controls.Add(this.labelFuelHistory);
            this.panelMaterialHistory.Location = new System.Drawing.Point(38, 354);
            this.panelMaterialHistory.Name = "panelMaterialHistory";
            this.panelMaterialHistory.Size = new System.Drawing.Size(188, 52);
            this.panelMaterialHistory.TabIndex = 41;
            this.panelMaterialHistory.Visible = false;
            this.panelMaterialHistory.Click += new System.EventHandler(this.panelMaterialHistory_Click);
            // 
            // labelBouxiteHistory
            // 
            this.labelBouxiteHistory.Location = new System.Drawing.Point(117, 2);
            this.labelBouxiteHistory.Name = "labelBouxiteHistory";
            this.labelBouxiteHistory.Size = new System.Drawing.Size(42, 48);
            this.labelBouxiteHistory.TabIndex = 7;
            this.labelBouxiteHistory.Text = "ボーキ";
            this.labelBouxiteHistory.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.labelBouxiteHistory.Click += new System.EventHandler(this.panelMaterialHistory_Click);
            // 
            // labelSteelHistory
            // 
            this.labelSteelHistory.Location = new System.Drawing.Point(78, 2);
            this.labelSteelHistory.Name = "labelSteelHistory";
            this.labelSteelHistory.Size = new System.Drawing.Size(42, 48);
            this.labelSteelHistory.TabIndex = 6;
            this.labelSteelHistory.Text = "鋼材";
            this.labelSteelHistory.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.labelSteelHistory.Click += new System.EventHandler(this.panelMaterialHistory_Click);
            // 
            // labelBulletHistory
            // 
            this.labelBulletHistory.Location = new System.Drawing.Point(39, 2);
            this.labelBulletHistory.Name = "labelBulletHistory";
            this.labelBulletHistory.Size = new System.Drawing.Size(42, 48);
            this.labelBulletHistory.TabIndex = 5;
            this.labelBulletHistory.Text = "弾薬";
            this.labelBulletHistory.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.labelBulletHistory.Click += new System.EventHandler(this.panelMaterialHistory_Click);
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(158, 14);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(29, 36);
            this.label35.TabIndex = 4;
            this.label35.Text = "母港\r\n今日\r\n今週";
            this.label35.Click += new System.EventHandler(this.panelMaterialHistory_Click);
            // 
            // labelFuelHistory
            // 
            this.labelFuelHistory.Location = new System.Drawing.Point(0, 2);
            this.labelFuelHistory.Name = "labelFuelHistory";
            this.labelFuelHistory.Size = new System.Drawing.Size(42, 48);
            this.labelFuelHistory.TabIndex = 0;
            this.labelFuelHistory.Text = "燃料";
            this.labelFuelHistory.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.labelFuelHistory.Click += new System.EventHandler(this.panelMaterialHistory_Click);
            // 
            // labelNDock
            // 
            this.labelNDock.AutoSize = true;
            this.labelNDock.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelNDock.Location = new System.Drawing.Point(8, 195);
            this.labelNDock.Name = "labelNDock";
            this.labelNDock.Size = new System.Drawing.Size(29, 12);
            this.labelNDock.TabIndex = 3;
            this.labelNDock.Text = "入渠";
            this.labelNDock.Click += new System.EventHandler(this.labelNDock_Click);
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
            // labelMission
            // 
            this.labelMission.AutoSize = true;
            this.labelMission.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelMission.Location = new System.Drawing.Point(8, 276);
            this.labelMission.Name = "labelMission";
            this.labelMission.Size = new System.Drawing.Size(29, 12);
            this.labelMission.TabIndex = 10;
            this.labelMission.Text = "遠征";
            this.labelMission.Click += new System.EventHandler(this.labelMission_Click);
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
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(183, 342);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(29, 12);
            this.label36.TabIndex = 43;
            this.label36.Text = "資材";
            this.label36.Click += new System.EventHandler(this.labelMaterialHistoryButton_Click);
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
            // imageListFuelSq
            // 
            this.imageListFuelSq.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListFuelSq.ImageStream")));
            this.imageListFuelSq.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListFuelSq.Images.SetKeyName(0, "透明sq.png");
            this.imageListFuelSq.Images.SetKeyName(1, "燃料黄sq.png");
            this.imageListFuelSq.Images.SetKeyName(2, "燃料橙sq.png");
            this.imageListFuelSq.Images.SetKeyName(3, "燃料赤sq.png");
            this.imageListFuelSq.Images.SetKeyName(4, "燃料灰sq.png");
            this.imageListFuelSq.Images.SetKeyName(5, "透明sq.png");
            this.imageListFuelSq.Images.SetKeyName(6, "燃料薄黄sq.png");
            this.imageListFuelSq.Images.SetKeyName(7, "燃料薄橙sq.png");
            this.imageListFuelSq.Images.SetKeyName(8, "燃料薄赤sq.png");
            this.imageListFuelSq.Images.SetKeyName(9, "燃料薄灰sq.png");
            // 
            // imageListBullSq
            // 
            this.imageListBullSq.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListBullSq.ImageStream")));
            this.imageListBullSq.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListBullSq.Images.SetKeyName(0, "透明sq.png");
            this.imageListBullSq.Images.SetKeyName(1, "弾薬黄sq.png");
            this.imageListBullSq.Images.SetKeyName(2, "弾薬橙sq.png");
            this.imageListBullSq.Images.SetKeyName(3, "弾薬赤sq.png");
            this.imageListBullSq.Images.SetKeyName(4, "弾薬灰sq.png");
            this.imageListBullSq.Images.SetKeyName(5, "透明sq.png");
            this.imageListBullSq.Images.SetKeyName(6, "弾薬薄黄sq.png");
            this.imageListBullSq.Images.SetKeyName(7, "弾薬薄橙sq.png");
            this.imageListBullSq.Images.SetKeyName(8, "弾薬薄赤sq.png");
            this.imageListBullSq.Images.SetKeyName(9, "弾薬薄灰sq.png");
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
            // labelBullSq4
            // 
            this.labelBullSq4.ImageIndex = 0;
            this.labelBullSq4.ImageList = this.imageListBullSq;
            this.labelBullSq4.Location = new System.Drawing.Point(214, 42);
            this.labelBullSq4.Name = "labelBullSq4";
            this.labelBullSq4.Size = new System.Drawing.Size(8, 13);
            this.labelBullSq4.TabIndex = 53;
            // 
            // labelFuelSq4
            // 
            this.labelFuelSq4.ImageIndex = 0;
            this.labelFuelSq4.ImageList = this.imageListFuelSq;
            this.labelFuelSq4.Location = new System.Drawing.Point(205, 42);
            this.labelFuelSq4.Name = "labelFuelSq4";
            this.labelFuelSq4.Size = new System.Drawing.Size(8, 13);
            this.labelFuelSq4.TabIndex = 52;
            // 
            // labelBullSq3
            // 
            this.labelBullSq3.ImageIndex = 0;
            this.labelBullSq3.ImageList = this.imageListBullSq;
            this.labelBullSq3.Location = new System.Drawing.Point(159, 42);
            this.labelBullSq3.Name = "labelBullSq3";
            this.labelBullSq3.Size = new System.Drawing.Size(8, 13);
            this.labelBullSq3.TabIndex = 51;
            // 
            // labelFuelSq3
            // 
            this.labelFuelSq3.ImageIndex = 0;
            this.labelFuelSq3.ImageList = this.imageListFuelSq;
            this.labelFuelSq3.Location = new System.Drawing.Point(150, 42);
            this.labelFuelSq3.Name = "labelFuelSq3";
            this.labelFuelSq3.Size = new System.Drawing.Size(8, 13);
            this.labelFuelSq3.TabIndex = 50;
            // 
            // labelBullSq2
            // 
            this.labelBullSq2.ImageIndex = 0;
            this.labelBullSq2.ImageList = this.imageListBullSq;
            this.labelBullSq2.Location = new System.Drawing.Point(104, 42);
            this.labelBullSq2.Name = "labelBullSq2";
            this.labelBullSq2.Size = new System.Drawing.Size(8, 13);
            this.labelBullSq2.TabIndex = 49;
            // 
            // labelFuelSq2
            // 
            this.labelFuelSq2.ImageIndex = 0;
            this.labelFuelSq2.ImageList = this.imageListFuelSq;
            this.labelFuelSq2.Location = new System.Drawing.Point(95, 42);
            this.labelFuelSq2.Name = "labelFuelSq2";
            this.labelFuelSq2.Size = new System.Drawing.Size(8, 13);
            this.labelFuelSq2.TabIndex = 48;
            // 
            // labelBullSq1
            // 
            this.labelBullSq1.ImageIndex = 0;
            this.labelBullSq1.ImageList = this.imageListBullSq;
            this.labelBullSq1.Location = new System.Drawing.Point(49, 42);
            this.labelBullSq1.Name = "labelBullSq1";
            this.labelBullSq1.Size = new System.Drawing.Size(8, 13);
            this.labelBullSq1.TabIndex = 47;
            // 
            // labelFuelSq1
            // 
            this.labelFuelSq1.ImageIndex = 0;
            this.labelFuelSq1.ImageList = this.imageListFuelSq;
            this.labelFuelSq1.Location = new System.Drawing.Point(40, 42);
            this.labelFuelSq1.Name = "labelFuelSq1";
            this.labelFuelSq1.Size = new System.Drawing.Size(8, 13);
            this.labelFuelSq1.TabIndex = 43;
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
            this.labelMaterialHistoryButton.Click += new System.EventHandler(this.labelMaterialHistoryButton_Click);
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
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(232, 456);
            this.ContextMenuStrip = this.contextMenuStripMain;
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
            this.Controls.Add(this.panelMaterialHistory);
            this.Controls.Add(this.labelBullSq4);
            this.Controls.Add(this.labelFuelSq4);
            this.Controls.Add(this.labelBullSq3);
            this.Controls.Add(this.labelFuelSq3);
            this.Controls.Add(this.labelBullSq2);
            this.Controls.Add(this.labelFuelSq2);
            this.Controls.Add(this.labelBullSq1);
            this.Controls.Add(this.labelFuelSq1);
            this.Controls.Add(this.labelRepairListButton);
            this.Controls.Add(this.label31);
            this.Controls.Add(this.labelMaterialHistoryButton);
            this.Controls.Add(this.label36);
            this.Controls.Add(this.labelCheckFleet2);
            this.Controls.Add(this.labelFleet2);
            this.Controls.Add(this.labelCheckFleet3);
            this.Controls.Add(this.labelFleet3);
            this.Controls.Add(this.labelCheckFleet4);
            this.Controls.Add(this.labelFleet4);
            this.Controls.Add(this.labelCheckFleet1);
            this.Controls.Add(this.labelMission);
            this.Controls.Add(this.labelQuest);
            this.Controls.Add(this.labelConstruct);
            this.Controls.Add(this.labelNDock);
            this.Controls.Add(this.panelShipInfo);
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
            this.panelShipInfo.ResumeLayout(false);
            this.panelShipInfo.PerformLayout();
            this.panelBattleInfo.ResumeLayout(false);
            this.panelBattleInfo.PerformLayout();
            this.panelMaterialHistory.ResumeLayout(false);
            this.panelMaterialHistory.PerformLayout();
            this.contextMenuStripNotifyIcon.ResumeLayout(false);
            this.contextMenuStripMain.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panelShipInfo;
        private System.Windows.Forms.Label labelNDock;
        private System.Windows.Forms.Label labelConstruct;
        private System.Windows.Forms.Label labelQuest;
        private System.Windows.Forms.Label labelMission;
        private System.Windows.Forms.Timer timerMain;
        private System.Windows.Forms.Label labelCondTimerTitle;
        private System.Windows.Forms.Label labelCondTimer;
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
        private System.Windows.Forms.Label labelFighterPower;
        private System.Windows.Forms.Label labelFighterPowerCaption;
        private System.Windows.Forms.Panel panelMaterialHistory;
        private System.Windows.Forms.Label labelBouxiteHistory;
        private System.Windows.Forms.Label labelSteelHistory;
        private System.Windows.Forms.Label labelBulletHistory;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.Label labelFuelHistory;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.Label labelMaterialHistoryButton;
        private System.Windows.Forms.Label labelRepairListButton;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripNotifyIcon;
        private System.Windows.Forms.ToolStripMenuItem NotifyIconOpenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem NotifyIconExitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem listToolStripMenuItem;
        private System.Windows.Forms.Label labelLoS;
        private System.Windows.Forms.Label labelLoSCaption;
        private System.Windows.Forms.Panel panelBattleInfo;
        private System.Windows.Forms.Label labelEnemyFighterPower;
        private System.Windows.Forms.Label labelEnemyFighterPowerCaption;
        private System.Windows.Forms.Label labelFormation;
        private System.Windows.Forms.Label labelResultRank;
        private System.Windows.Forms.ImageList imageListFuelSq;
        private System.Windows.Forms.Label labelFuelSq1;
        private System.Windows.Forms.ImageList imageListBullSq;
        private System.Windows.Forms.Label labelBullSq1;
        private System.Windows.Forms.Label labelBullSq2;
        private System.Windows.Forms.Label labelFuelSq2;
        private System.Windows.Forms.Label labelBullSq3;
        private System.Windows.Forms.Label labelFuelSq3;
        private System.Windows.Forms.Label labelBullSq4;
        private System.Windows.Forms.Label labelFuelSq4;
        private System.Windows.Forms.ToolStripMenuItem LogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CaptureToolStripMenuItem;
        private System.Windows.Forms.Label labelAkashiRepairTimer;
        private System.Windows.Forms.Panel panelCombinedFleet;
        private System.Windows.Forms.Label labelAkashiRepair;
        private System.Windows.Forms.Label labelPresetAkashiTimer;
        private RepairListForMain panelRepairList;
        private System.Windows.Forms.Panel panel7Ships;
        private System.Windows.Forms.LinkLabel linkLabelGuide;
        private System.Windows.Forms.Label labelClearQuest;
        private QuestPanel questPanel;
        private System.Windows.Forms.Label labelQuestCount;
        private MissionPanel missionPanel;
        private NDockPanel ndockPanel;
        private KDockPanel kdockPanel;
        private HqPanel hqPanel;
    }
}

