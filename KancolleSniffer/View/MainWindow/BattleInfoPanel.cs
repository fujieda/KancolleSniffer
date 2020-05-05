// Copyright (C) 2020 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

using System.Drawing;
using System.Windows.Forms;
using KancolleSniffer.Model;

namespace KancolleSniffer.View.MainWindow
{
    public class BattleInfoPanel : Panel
    {
        private readonly Label _enemyFighterPower = new Label
        {
            Location = new Point(129, 1),
            Size = new Size(29, 12),
            TextAlign = ContentAlignment.MiddleRight
        };

        private readonly Label _enemyFighterPowerCaption = new Label
        {
            AutoSize = true,
            Location = new Point(90, 1),
            Text = "敵制空"
        };

        private readonly Label _formation = new Label
        {
            Location = new Point(40, 1),
            Size = new Size(48, 12)
        };

        private readonly Label _resultRank = new Label
        {
            Location = new Point(1, 1),
            Size = new Size(42, 12),
            TabIndex = 0,
            Text = "判定"
        };

        public UpdateContext Context { private get; set; }

        private ToolTip ToolTip => ((PanelWithToolTip)Parent).ToolTip;

        public BattleInfoPanel()
        {
            Controls.AddRange(new Control[] {_enemyFighterPower, _enemyFighterPowerCaption, _formation, _resultRank});
            _resultRank.Click += (sender, e) => ShowResultRank();
        }

        public new void Update()
        {
            BringToFront();
            var battle = Context.Sniffer.Battle;
            _formation.Text = new[] {"同航戦", "反航戦", "T字有利", "T字不利"}[battle.Formation[2] - 1];
            UpdateBattleFighterPower();
            if ((Context.Config.Spoilers & Spoiler.ResultRank) != 0)
                ShowResultRank();
        }

        public void Reset()
        {
            _formation.Text = "";
            _enemyFighterPower.Text = "";
            _resultRank.Text = "判定";
            Visible = Context.Sniffer.Battle.BattleState != BattleState.None;
        }

        private void UpdateBattleFighterPower()
        {
            UpdateEnemyFighterPower();
        }

        private void UpdateEnemyFighterPower()
        {
            var fp = Context.Sniffer.Battle.EnemyFighterPower;
            _enemyFighterPower.Text = fp.AirCombat + fp.UnknownMark;
            var toolTip = fp.AirCombat == fp.Interception ? "" : "防空: " + fp.Interception + fp.UnknownMark;
            ToolTip.SetToolTip(_enemyFighterPower, toolTip);
            ToolTip.SetToolTip(_enemyFighterPowerCaption, toolTip);
        }

        private void ShowResultRank()
        {
            var result = new[] {"完全S", "勝利S", "勝利A", "勝利B", "敗北C", "敗北D", "敗北E"};
            _resultRank.Text = result[(int)Context.Sniffer.Battle.ResultRank];
        }
    }
}