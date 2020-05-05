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
    public class FighterPower
    {
        private readonly Label _fighterPower = new Label
        {
            Location = new Point(28, 117),
            Size = new Size(29, 12),
            Text = "0",
            TextAlign = ContentAlignment.MiddleRight
        };

        private readonly Label _fighterPowerCaption = new Label
        {
            AutoSize = true,
            Location = new Point(2, 117),
            Text = "制空"
        };

        private readonly ShipInfoPanel _parent;

        public UpdateContext Context { private get; set; }

        public FighterPower(ShipInfoPanel parent)
        {
            _parent = parent;
            parent.Controls.AddRange(new Control[] {_fighterPowerCaption, _fighterPower});
        }

        public void Reset()
        {
            _fighterPower.ForeColor = Control.DefaultForeColor;
            _fighterPowerCaption.Text = "制空";
        }

        public void UpdateFighterPower()
        {
            UpdateFighterPower(IsCombinedFighterPower);
        }

        private bool IsCombinedFighterPower => _parent.CombinedFleet &&
                                               (Context.Sniffer.Battle.BattleState == BattleState.None ||
                                                Context.Sniffer.Battle.EnemyIsCombined);

        private void UpdateFighterPower(bool combined)
        {
            var fleets = Context.Sniffer.Fleets;
            var fp = combined
                ? fleets[0].FighterPower + fleets[1].FighterPower
                : fleets[_parent.CurrentFleet].FighterPower;
            _fighterPower.Text = fp.Min.ToString("D");
            var cr = combined
                ? fleets[0].ContactTriggerRate + fleets[1].ContactTriggerRate
                : fleets[_parent.CurrentFleet].ContactTriggerRate;
            var text = "制空: " + (fp.Diff ? fp.RangeString : fp.Min.ToString()) +
                       $" 触接: {cr * 100:f1}";
            _parent.ToolTip.SetToolTip(_fighterPower, text);
            _parent.ToolTip.SetToolTip(_fighterPowerCaption, text);
        }

        public void UpdateBattleFighterPower()
        {
            var battle = Context.Sniffer.Battle;
            _fighterPower.ForeColor = AirControlLevelColor(battle);
            _fighterPowerCaption.Text = AirControlLevelString(battle);
            if (battle.BattleState == BattleState.AirRaid)
            {
                UpdateAirRaidFighterPower();
            }
            else
            {
                UpdateFighterPower(Context.Sniffer.IsCombinedFleet && battle.EnemyIsCombined);
            }
        }

        private void UpdateAirRaidFighterPower()
        {
            var fp = Context.Sniffer.Battle.FighterPower;
            _fighterPower.Text = fp.Min.ToString();
            var toolTop = fp.Diff ? fp.RangeString : "";
            _parent.ToolTip.SetToolTip(_fighterPower, toolTop);
            _parent.ToolTip.SetToolTip(_fighterPowerCaption, toolTop);
        }

        private static Color AirControlLevelColor(BattleInfo battle)
        {
            return new[]
            {
                Control.DefaultForeColor, Control.DefaultForeColor, CUDColors.Blue, CUDColors.Green, CUDColors.Orange,
                CUDColors.Red
            }[
                battle.BattleState == BattleState.Night ? 0 : battle.AirControlLevel + 1];
        }

        private static string AirControlLevelString(BattleInfo battle)
        {
            return new[] {"制空", "拮抗", "確保", "優勢", "劣勢", "喪失"}[
                battle.BattleState == BattleState.Night ? 0 : battle.AirControlLevel + 1];
        }
    }
}