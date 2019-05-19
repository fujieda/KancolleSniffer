﻿// Copyright (C) 2017 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using KancolleSniffer.Model;
using static System.Math;

namespace KancolleSniffer.View
{
    public class BattleResultPanel : PanelWithToolTip
    {
        private const int LineHeight = 16;
        private readonly List<ShipLabels> _friendLabels = new List<ShipLabels>();
        private readonly List<ShipLabels> _enemyLabels = new List<ShipLabels>();
        private readonly List<ShipLabel.Hp> _hpLabels = new List<ShipLabel.Hp>();
        private readonly BattleInfo.BattleResult[] _result = new BattleInfo.BattleResult[2];
        private Label _phaseLabel, _rankLabel, _supportLabel, _cellLabel;
        private readonly BattleResultRank[] _rank = new BattleResultRank[2];
        private readonly InformationPanel _informationPanel;
        private CellInfo _cellInfo;
        private string _supportType;

        public Spoiler Spoilers { get; set; }

        public BattleResultPanel()
        {
            SuspendLayout();
            CreateLabels();
            _informationPanel = new InformationPanel();
            Controls.Add(_informationPanel);
            ResumeLayout();
        }

        public event Action HpLabelClick;

        private void HpLabelClickHandler(object sender, EventArgs ev)
        {
            HpLabelClick?.Invoke();
        }

        public void ToggleHpPercent()
        {
            foreach (var label in _hpLabels)
                label.ToggleHpPercent();
        }

        public void Update(Sniffer sniffer)
        {
            var state = sniffer.Battle.BattleState;
            _cellInfo = sniffer.CellInfo;
            switch (sniffer.Battle.BattleState)
            {
                case BattleState.None:
                    if (_result[0] == null && _result[1] == null)
                        return;
                    ClearResult();
                    SetPhase("結果");
                    UpdateCellInfo(_cellInfo);
                    return;
                case BattleState.Day:
                case BattleState.SpNight:
                    _result[0] = _result[1] = null;
                    break;
                case BattleState.Result:
                case BattleState.Unknown:
                    return;
            }
            _supportType = sniffer.Battle.SupportType;
            if ((Spoilers & Spoiler.BattleResult) != 0)
            {
                ShowResult(sniffer.Battle.Result);
                ShowResultRank(sniffer.Battle.ResultRank);
                switch (state)
                {
                    case BattleState.Day:
                        _result[0] = sniffer.Battle.Result;
                        _rank[0] = sniffer.Battle.ResultRank;
                        SetPhase("昼戦");
                        break;
                    case BattleState.Night:
                    case BattleState.SpNight:
                        _result[1] = sniffer.Battle.Result;
                        _rank[1] = sniffer.Battle.ResultRank;
                        SetPhase("夜戦");
                        break;
                }
                _informationPanel.Visible = true;
            }
            else
            {
                ClearResult();
                SetPhase("結果");
                switch (state)
                {
                    case BattleState.Day:
                        _result[0] = sniffer.Battle.Result;
                        _rank[0] = sniffer.Battle.ResultRank;
                        break;
                    case BattleState.Night:
                    case BattleState.SpNight:
                        _result[1] = sniffer.Battle.Result;
                        _rank[1] = sniffer.Battle.ResultRank;
                        break;
                }
            }
            _informationPanel.SetInformation(sniffer.Battle);
        }

        private void PhaseLabelClick(object sender, EventArgs ev)
        {
            switch (_phaseLabel.Text)
            {
                case "結果":
                    if (_result[0] != null)
                    {
                        ShowResult(_result[0]);
                        ShowResultRank(_rank[0]);
                        SetPhase("昼戦");
                    }
                    else if (_result[1] != null)
                    {
                        ShowResult(_result[1]);
                        ShowResultRank(_rank[1]);
                        SetPhase("夜戦");
                    }
                    break;
                case "昼戦":
                    if (_result[1] != null)
                    {
                        ShowResult(_result[1]);
                        ShowResultRank(_rank[1]);
                        SetPhase("夜戦");
                    }
                    break;
                case "夜戦":
                    if (_result[0] != null)
                    {
                        ShowResult(_result[0]);
                        ShowResultRank(_rank[0]);
                        SetPhase("昼戦");
                    }
                    break;
            }
        }

        private void SetPhase(string phase)
        {
            _phaseLabel.Text = phase;
            if (phase == "結果" || _result[0] != null && _result[1] != null)
            {
                _phaseLabel.BorderStyle = BorderStyle.FixedSingle;
                _phaseLabel.Cursor = Cursors.Hand;
            }
            else
            {
                _phaseLabel.BorderStyle = BorderStyle.None;
                _phaseLabel.Cursor = Cursors.Default;
            }
        }

        private Point _scrollPosition;

        private void ClearResult()
        {
            _scrollPosition = AutoScrollPosition;
            foreach (var labels in _friendLabels)
                labels.BackPanel.Visible = false;
            _informationPanel.Visible = false;
            _rankLabel.Text = "";
            _supportLabel.Text = "";
        }

        private void ShowResult(BattleInfo.BattleResult result)
        {
            SuspendLayout();
            AutoScrollPosition = _scrollPosition;
            var friend = result.Friend;
            var enemy = result.Enemy;
            for (var i = 0; i < friend.Main.Length; i++)
            {
                var labels = _friendLabels[i];
                var ship = friend.Main[i];
                labels.Name.SetName(ship, ShipNameWidth.BattleResult);
                labels.Hp.SetHp(ship);
                ToolTip.SetToolTip(labels.Name, GetEquipString(ship));
            }
            if (friend.Guard.Length > 0)
            {
                _friendLabels[friend.Main.Length].Name.Text = "護衛";
                _friendLabels[friend.Main.Length].Hp.SetHp(null);
                for (var i = 0; i < friend.Guard.Length; i++)
                {
                    var labels = _friendLabels[friend.Main.Length + 1 + i];
                    var ship = friend.Guard[i];
                    labels.Name.SetName(ship, ShipNameWidth.BattleResult);
                    labels.Hp.SetHp(ship);
                    ToolTip.SetToolTip(labels.Name, GetEquipString(ship));
                }
            }
            var friendLines = friend.Main.Length + (friend.Guard.Length > 0 ? friend.Guard.Length + 1 : 0);
            for (var i = friendLines; i < _friendLabels.Count; i++)
            {
                _friendLabels[i].Name.SetName("");
                _friendLabels[i].Hp.SetHp(null);
            }
            for (var i = 0; i < enemy.Main.Length; i++)
            {
                var labels = _enemyLabels[i];
                var ship = enemy.Main[i];
                labels.Name.SetName(ShortenName(ship.Name));
                labels.Hp.SetHp(ship);
                ToolTip.SetToolTip(labels.Name, GetEquipString(ship));
            }
            if (enemy.Guard.Length > 0)
            {
                _enemyLabels[enemy.Main.Length].Name.Text = "護衛";
                _enemyLabels[enemy.Main.Length].Hp.SetHp(null);
                for (var i = 0; i < enemy.Guard.Length; i++)
                {
                    var labels = _enemyLabels[enemy.Main.Length + 1 + i];
                    var ship = enemy.Guard[i];
                    labels.Name.SetName(ShortenName(ship.Name));
                    labels.Hp.SetHp(ship);
                    ToolTip.SetToolTip(labels.Name, GetEquipString(ship));
                }
            }
            var enemyLines = enemy.Main.Length + (enemy.Guard.Length > 0 ? enemy.Guard.Length + 1 : 0);
            for (var i = enemyLines; i < _enemyLabels.Count; i++)
            {
                _enemyLabels[i].Name.SetName("");
                _enemyLabels[i].Hp.SetHp(null);
            }
            var lines = Max(friendLines, enemyLines);
            for (var i = 0; i < lines; i++)
            {
                var panel = _friendLabels[i].BackPanel;
                panel.Visible = true;
            }
            for (var i = lines; i < _friendLabels.Count; i++)
                _friendLabels[i].BackPanel.Visible = false;
            ResumeLayout(); // スクロールバーの有無を決定する
            var panelWidth = Max(ClientSize.Width, // スクロールバーの有無を反映した横幅
                _enemyLabels[0].Name.Location.X + _enemyLabels.Max(labels => labels.Name.Size.Width) - 1); // 敵の名前の右端
            for (var i = 0; i < lines; i++)
                _friendLabels[i].BackPanel.Width = panelWidth;
            _informationPanel.Location = Scaler.Move(AutoScrollPosition.X, AutoScrollPosition.Y, 0, 20);
            _informationPanel.Visible = true;
            UpdateCellInfo(_cellInfo);
        }

        private string GetEquipString(ShipStatus ship)
        {
            var result =
                (from i in Enumerable.Range(0, ship.Slot.Count)
                    let item = ship.Slot[i]
                    where !item.Empty
                    select item.Spec.Name +
                           (item.Spec.IsAircraft && ship.OnSlot.Length > 0 && ship.Spec.MaxEq.Length > 0
                               ? $"{ship.OnSlot[i]}/{ship.Spec.MaxEq[i]}"
                               : ""));
            if (ship.SlotEx.Id > 0)
                result = result.Concat(new[] {ship.SlotEx.Spec.Name});
            return string.Join("\r\n", result);
        }

        private string ShortenName(string name)
        {
            return new Regex(@"\(elite\)|\(flagship\)").Replace(name,
                match => match.Value == "(elite)" ? "(e)" : "(f)");
        }

        private void ShowResultRank(BattleResultRank rank)
        {
            var result = new[] {"完全S", "勝利S", "勝利A", "勝利B", "敗北C", "敗北D", "敗北E"};
            _rankLabel.Text = result[(int)rank];
            _supportLabel.Text = _supportType;
        }

        public void UpdateCellInfo(CellInfo cellInfo)
        {
            _cellLabel.Text = (Spoilers & Spoiler.NextCell) == 0 ? cellInfo.Current : cellInfo.Next;
            _cellLabel.Location = new Point(ClientSize.Width - _cellLabel.Width - 2, 4);
        }

        private void CreateLabels()
        {
            _phaseLabel = new Label
            {
                Location = new Point(4, 4),
                Size = new Size(31, 14)
            };
            _phaseLabel.Click += PhaseLabelClick;
            Controls.Add(_phaseLabel);
            _rankLabel = new Label
            {
                Location = new Point(37, 4),
                Size = new Size(42, 12)
            };
            Controls.Add(_rankLabel);
            _supportLabel = new Label
            {
                Location = new Point(77, 4),
                AutoSize = true
            };
            Controls.Add(_supportLabel);
            _cellLabel = new Label
            {
                Location = new Point(0, 4),
                AutoSize = true
            };
            Controls.Add(_cellLabel);
            for (var i = 0; i < 13; i++)
            {
                var y = LineHeight * i + 38;
                var friend = new ShipLabels
                {
                    Name = new ShipLabel {Location = new Point(1, 2), AutoSize = true},
                    Hp = new ShipLabel.Hp(new Point(101, 0), LineHeight),
                    BackPanel = new Panel
                    {
                        Location = new Point(0, y),
                        Size = new Size(0, LineHeight),
                        BackColor = CustomColors.ColumnColors.DarkFirst(i),
                    }
                };
                _friendLabels.Add(friend);
                friend.Arrange(this, CustomColors.ColumnColors.DarkFirst(i));
                _hpLabels.Add(friend.Hp);
                friend.Hp.Click += HpLabelClickHandler;
                var enemy = new ShipLabels
                {
                    Name = new ShipLabel {Location = new Point(164, 2), AutoSize = true},
                    Hp = new ShipLabel.Hp
                    {
                        Location = new Point(119, 0),
                        AutoSize = true,
                        MinimumSize = new Size(0, LineHeight),
                        TextAlign = ContentAlignment.MiddleLeft
                    }
                };
                _enemyLabels.Add(enemy);
                enemy.Arrange(friend.BackPanel, CustomColors.ColumnColors.DarkFirst(i));
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (VScroll && (ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                VScroll = false;
                OnMouseWheel(e);
                VScroll = true;
                return;
            }
            base.OnMouseWheel(e);
        }

        private class InformationPanel : PanelWithToolTip
        {
            private readonly Label[] _formation;
            private readonly Label[] _fighterPower;

            public InformationPanel()
            {
                Visible = false;
                Size = new Size(206, 16);
                Controls.AddRange(_formation = new[]
                {
                    new Label
                    {
                        Location = new Point(47, 2),
                        Size = new Size(29, 12)
                    },
                    new Label
                    {
                        Location = new Point(75, 2),
                        Size = new Size(29, 12)
                    },
                    new Label
                    {
                        Location = new Point(1, 2),
                        Size = new Size(48, 12),
                        TextAlign = ContentAlignment.MiddleCenter
                    }
                });
                Controls.AddRange(_fighterPower = new[]
                {
                    new Label
                    {
                        Location = new Point(162, 2),
                        Size = new Size(23, 12),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    new Label
                    {
                        Location = new Point(183, 2),
                        Size = new Size(23, 12),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    new Label
                    {
                        Location = new Point(110, 2),
                        Size = new Size(53, 12)
                    }
                });
                // ReSharper disable once VirtualMemberCallInConstructor
                BackColor = CustomColors.ColumnColors.Bright;
            }

            public void SetInformation(BattleInfo battleInfo)
            {
                _formation[0].Text = FormationName(battleInfo.Formation[0]);
                _formation[1].Text = FormationName(battleInfo.Formation[1]);
                _formation[2].Text = new[] {"同航戦", "反航戦", "T字有利", "T字不利"}[battleInfo.Formation[2] - 1];

                if (battleInfo.AirControlLevel == -1)
                {
                    if (battleInfo.BattleState == BattleState.Night)
                        return;
                    foreach (var label in _fighterPower)
                        label.Visible = false;
                    return;
                }
                var fp = battleInfo.FighterPower;
                _fighterPower[0].Text = fp[0].ToString("D");
                ToolTip.SetToolTip(_fighterPower[0], fp[0] == fp[1] ? "" : $"{fp[0]}～{fp[1]}");
                var efp = battleInfo.EnemyFighterPower;
                _fighterPower[1].Text = efp.AirCombat + efp.UnknownMark;
                ToolTip.SetToolTip(_fighterPower[1],
                    efp.AirCombat == efp.Interception ? "" : "防空:" + efp.Interception + efp.UnknownMark);
                _fighterPower[2].Text =
                    new[] {"", "制空均衡", "制空確保", "航空優勢", "航空劣勢", "制空喪失"}[battleInfo.AirControlLevel + 1];
                foreach (var label in _fighterPower)
                    label.Visible = true;
            }

            private string FormationName(int formation)
            {
                switch (formation)
                {
                    case 1:
                        return "単縦";
                    case 2:
                        return "複縦";
                    case 3:
                        return "輪形";
                    case 4:
                        return "梯形";
                    case 5:
                        return "単横";
                    case 6:
                        return "警戒";
                    case 11:
                        return "第一";
                    case 12:
                        return "第二";
                    case 13:
                        return "第三";
                    case 14:
                        return "第四";
                    default:
                        return "";
                }
            }
        }
    }
}