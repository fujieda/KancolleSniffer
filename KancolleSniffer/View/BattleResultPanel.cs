// Copyright (C) 2017 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using KancolleSniffer.Util;
using static System.Math;

namespace KancolleSniffer.View
{
    public class BattleResultPanel : PanelWithToolTip
    {
        private readonly List<ShipLabels> _friendLabels = new List<ShipLabels>();
        private readonly List<ShipLabels> _enemyLabels = new List<ShipLabels>();
        private readonly List<ShipLabel.Hp> _hpLabels = new List<ShipLabel.Hp>();
        private Label _phaseLabel, _rankLabel, _supportLabel, _cellLabel;
        private readonly InformationPanel _information;
        private CellInfo _cellInfo;
        private readonly BattleData _data = new BattleData();

        public Spoiler Spoilers { get; set; }

        private class BattleData
        {
            private Result _day;
            private Result _night;
            public int[] Formation;
            public Range FighterPower;
            public EnemyFighterPower EnemyFighterPower;
            public int AirControlLevel;
            public int SupportType;
            public bool HaveDay => _day != null;
            public bool HaveNight => _night != null;

            public class Result
            {
                public BattleInfo.BattleResult Damage;
                public BattleResultRank Rank;
            }

            public Result GetResult(BattleState state)
            {
                switch (state)
                {
                    case BattleState.Day:
                        return _day;
                    case BattleState.SpNight:
                    case BattleState.Night:
                        return _night;
                    default:
                        return _day;
                }
            }

            public void SetData(BattleInfo battle)
            {
                switch (battle.BattleState)
                {
                    case BattleState.Day:
                        _day = new Result {Damage = battle.Result, Rank = battle.ResultRank};
                        _night = null;
                        break;
                    case BattleState.SpNight:
                        _day = null;
                        goto case BattleState.Night;
                    case BattleState.Night:
                        _night = new Result {Damage = battle.Result, Rank = battle.ResultRank};
                        break;
                }
                if (battle.BattleState != BattleState.Night)
                    SetNonStateData(battle);
            }

            private void SetNonStateData(BattleInfo battle)
            {
                Formation = battle.Formation;
                FighterPower = battle.FighterPower;
                EnemyFighterPower = battle.EnemyFighterPower;
                AirControlLevel = battle.AirControlLevel;
                SupportType = battle.SupportType;
            }
        }

        public BattleResultPanel()
        {
            SuspendLayout();
            CreateLabels();
            _information = new InformationPanel();
            Controls.Add(_information);
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
                    if (!_data.HaveDay && !_data.HaveNight)
                        return;
                    ClearResult();
                    SetPhase("結果");
                    UpdateCellInfo(_cellInfo);
                    return;
                case BattleState.Result:
                case BattleState.Unknown:
                    return;
            }
            _data.SetData(sniffer.Battle);
            if ((Spoilers & Spoiler.BattleResult) != 0)
            {
                ShowData(sniffer.Battle.BattleState);
                switch (state)
                {
                    case BattleState.Day:
                        SetPhase("昼戦");
                        break;
                    case BattleState.Night:
                    case BattleState.SpNight:
                        SetPhase("夜戦");
                        break;
                }
            }
            else
            {
                ClearResult();
                SetPhase("結果");
            }
        }

        private void PhaseLabelClick(object sender, EventArgs ev)
        {
            switch (_phaseLabel.Text)
            {
                case "結果":
                    if (_data.HaveDay)
                    {
                        ShowData(BattleState.Day);
                        SetPhase("昼戦");
                    }
                    else if (_data.HaveNight)
                    {
                        ShowData(BattleState.Night);
                        SetPhase("夜戦");
                    }
                    break;
                case "昼戦":
                    if (_data.HaveNight)
                    {
                        ShowData(BattleState.Night);
                        SetPhase("夜戦");
                    }
                    break;
                case "夜戦":
                    if (_data.HaveDay)
                    {
                        ShowData(BattleState.Day);
                        SetPhase("昼戦");
                    }
                    break;
            }
        }

        private void SetPhase(string phase)
        {
            _phaseLabel.Text = phase;
            if (phase == "結果" || _data.HaveDay && _data.HaveNight)
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
            SetPanelVisible(0);
            _information.Visible = false;
            _rankLabel.Text = "";
            _supportLabel.Text = "";
        }

        private void ShowData(BattleState state)
        {
            var result = _data.GetResult(state);
            ShowDamage(result.Damage);
            ShowResultRank(result.Rank);
            _information.Show(_data);
            ShowSupportType(_data.SupportType);
            UpdateCellInfo(_cellInfo);
            AutoScrollPosition = new Point(-_scrollPosition.X, -_scrollPosition.Y);
        }

        private void ShowDamage(BattleInfo.BattleResult result)
        {
            SuspendLayout();
            SetEachResult(_friendLabels, result.Friend);
            SetEachResult(_enemyLabels, result.Enemy);
            var lines = Max(Ships(result.Friend).Length, Ships(result.Enemy).Length);
            SetPanelVisible(lines);
            ResumeLayout(); // スクロールバーの有無を決定する
            AdjustPanelWidth(lines);
        }

        private void SetEachResult(IReadOnlyList<ShipLabels> labelsList, BattleInfo.BattleResult.Combined fleet)
        {
            var ships = Ships(fleet);
            for (var i = 0; i < ships.Length; i++)
            {
                var labels = labelsList[i];
                if (i == fleet.Main.Length)
                {
                    labels.Name.Text = "護衛";
                    labels.Hp.Reset();
                    continue;
                }
                var ship = ships[i];
                if (ShipMaster.IsEnemyId(ship.Spec.Id))
                {
                    labels.Hp.Set(ship);
                    labels.Name.SetName(ShortenName(ship.Name));
                }
                else
                {
                    labels.Set(ship);
                }
                ToolTip.SetToolTip(labels.Name, GetEquipString(ship));
            }
            for (var i = ships.Length; i < labelsList.Count; i++)
                labelsList[i].Reset();
        }

        private static ShipStatus[] Ships(BattleInfo.BattleResult.Combined fleet)
        {
            if (fleet.Guard.Length == 0)
                return fleet.Main;
            return fleet.Main.Concat(new[] {new ShipStatus()}.Concat(fleet.Guard)).ToArray();
        }

        private static string GetEquipString(ShipStatus ship)
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

        private static string ShortenName(string name)
        {
            return new Regex(@"\(elite\)|\(flagship\)").Replace(name,
                match => match.Value == "(elite)" ? "(e)" : "(f)");
        }

        private void ShowResultRank(BattleResultRank rank)
        {
            var result = new[] {"完全S", "勝利S", "勝利A", "勝利B", "敗北C", "敗北D", "敗北E"};
            _rankLabel.Text = result[(int)rank];
        }

        private void ShowSupportType(int support)
        {
            _supportLabel.Text = new[] {"", "空支援", "砲支援", "雷支援", "潜支援"}[support];
        }

        public void UpdateCellInfo(CellInfo cellInfo)
        {
            _cellLabel.Text = (Spoilers & Spoiler.NextCell) == 0 ? cellInfo.Current : cellInfo.Next;
            _cellLabel.Location = new Point(ClientSize.Width - _cellLabel.Width - 2, _cellLabel.Location.Y);
        }

        private const int LineHeight = 16;
        private const int LabelHeight = 12;
        private const int LabelMargin = (LineHeight - LabelHeight) / 2;
        private const int TopMargin = 4;

        private void CreateLabels()
        {
            _phaseLabel = new Label
            {
                Location = new Point(4, TopMargin),
                Size = new Size(31, LabelHeight + 2)
            };
            _phaseLabel.Click += PhaseLabelClick;
            Controls.Add(_phaseLabel);
            _rankLabel = new Label
            {
                Location = new Point(37, TopMargin),
                Size = new Size(42, LabelHeight)
            };
            Controls.Add(_rankLabel);
            _supportLabel = new Label
            {
                Location = new Point(77, TopMargin),
                AutoSize = true
            };
            Controls.Add(_supportLabel);
            _cellLabel = new Label
            {
                Location = new Point(0, TopMargin),
                AutoSize = true
            };
            Controls.Add(_cellLabel);
            for (var i = 0; i < 13; i++)
            {
                var friend = new ShipLabels
                {
                    Name = new ShipLabel.Name(new Point(1, LabelMargin), ShipNameWidth.BattleResult),
                    Hp = new ShipLabel.Hp(new Point(101, 0), LineHeight),
                    BackPanel = new Panel
                    {
                        Location = new Point(0, LineHeight * i + 38),
                        Size = new Size(0, LineHeight),
                        BackColor = CustomColors.ColumnColors.DarkFirst(i),
                        Visible = false
                    }
                };
                _friendLabels.Add(friend);
                friend.Arrange(this, CustomColors.ColumnColors.DarkFirst(i));
                _hpLabels.Add(friend.Hp);
                friend.Hp.Click += HpLabelClickHandler;
                var enemy = new ShipLabels
                {
                    Name = new ShipLabel.Name(new Point(164, LabelMargin), ShipNameWidth.Max),
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

        private void SetPanelVisible(int showPanels)
        {
            for (var i = 0; i < _friendLabels.Count; i++)
                _friendLabels[i].BackPanel.Visible = i < showPanels;
        }

        private void AdjustPanelWidth(int lines)
        {
            var panelWidth = Max(ClientSize.Width, // スクロールバーの有無を反映した横幅
                _enemyLabels[0].Name.Location.X + _enemyLabels.Max(labels => labels.Name.Size.Width) - 1); // 敵の名前の右端
            for (var i = 0; i < lines; i++)
                _friendLabels[i].BackPanel.Width = panelWidth;
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

        private sealed class InformationPanel : PanelWithToolTip
        {
            private readonly Formation _formation;
            private readonly FighterPower _fighterPower;

            public InformationPanel()
            {
                _formation = new Formation
                {
                    Friend = new Label
                    {
                        Location = new Point(47, LabelMargin),
                        Size = new Size(29, LabelHeight)
                    },
                    Enemy = new Label
                    {
                        Location = new Point(75, LabelMargin),
                        Size = new Size(29, LabelHeight)
                    },
                    State = new Label
                    {
                        Location = new Point(1, LabelMargin),
                        Size = new Size(48, LabelHeight),
                        TextAlign = ContentAlignment.MiddleCenter
                    }
                };
                Controls.AddRange(_formation.Controls);
                _fighterPower = new FighterPower
                {
                    ToolTip = ToolTip,
                    Friend = new Label
                    {
                        Location = new Point(162, LabelMargin),
                        Size = new Size(23, LabelHeight),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    Enemy = new Label
                    {
                        Location = new Point(183, LabelMargin),
                        Size = new Size(23, LabelHeight),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    State = new Label
                    {
                        Location = new Point(110, LabelMargin),
                        Size = new Size(53, LabelHeight)
                    }
                };
                Controls.AddRange(_fighterPower.Controls);
                Location = new Point(0, 20);
                Size = new Size(206, 16);
                BackColor = CustomColors.ColumnColors.Bright;
                Visible = false;
            }

            public void Show(BattleData data)
            {
                SetData(data);
                Visible = true;
            }

            private void SetData(BattleData data)
            {
                _formation.SetFormation(data.Formation);
                _fighterPower.SetFighterPower(data);
            }

            private class StateLabels
            {
                public Label Friend;
                public Label Enemy;
                public Label State;

                public Control[] Controls => new Control[] {Friend, Enemy, State};
            }

            private class Formation : StateLabels
            {
                public void SetFormation(int[] formation)
                {
                    Friend.Text = FormationName(formation[0]);
                    Enemy.Text = FormationName(formation[1]);
                    State.Text = new[] {"同航戦", "反航戦", "T字有利", "T字不利"}[formation[2] - 1];
                }

                private static string FormationName(int formation)
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

            private class FighterPower : StateLabels
            {
                public ToolTip ToolTip { private get; set; }

                public void SetFighterPower(BattleData data)
                {
                    if (data.AirControlLevel == -1)
                    {
                        foreach (var control in Controls)
                            control.Text = "";
                        return;
                    }
                    SetFriend(data.FighterPower);
                    SetEnemy(data.EnemyFighterPower);
                    SetAirControlLevel(data.AirControlLevel);
                }

                private void SetFriend(Range fighterPower)
                {
                    Friend.Text = fighterPower.Min.ToString();
                    ToolTip.SetToolTip(Friend, fighterPower.Diff ? fighterPower.RangeString : "");
                }

                private void SetEnemy(EnemyFighterPower enemy)
                {
                    Enemy.Text = enemy.AirCombat + enemy.UnknownMark;
                    ToolTip.SetToolTip(Enemy,
                        enemy.AirCombat == enemy.Interception ? "" : "防空:" + enemy.Interception + enemy.UnknownMark);
                }

                private void SetAirControlLevel(int level)
                {
                    State.Text =
                        new[] {"", "制空均衡", "制空確保", "航空優勢", "航空劣勢", "制空喪失"}[level + 1];
                }
            }
        }
    }
}