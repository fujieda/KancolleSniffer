using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KancolleSniffer.Model;

namespace KancolleSniffer.View
{
    public class ShipInfoPanel : PanelWithToolTip, IUpdateTimers
    {
        private readonly Panel _combinedFleet = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(220, 113),
            Visible = false
        };

        private readonly Panel _7Ships = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(220, 113),
            Visible = false
        };

        private readonly Panel _battleInfo = new Panel
        {
            Location = new Point(59, 116),
            Size = new Size(157, 14),
            Visible = false
        };

        private readonly Label _presetAkashiTimer = new Label
        {
            Location = new Point(2, 3),
            Size = new Size(32, 12),
            BackColor = CustomColors.ColumnColors.Bright
        };

        public LinkLabel Guide { get; } = new LinkLabel
        {
            AutoSize = true,
            Font = new Font("MS UI Gothic", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 128),
            LinkArea = new LinkArea(0, 0),
            Location = new Point(31, 51),
            Text = "右クリックでメニューが出ます。"
        };

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

        private readonly Label _lineOfSight = new Label
        {
            Location = new Point(85, 117),
            Size = new Size(38, 12),
            Text = "0.0",
            TextAlign = ContentAlignment.MiddleRight
        };

        private readonly Label _lineOfSightCaption = new Label
        {
            AutoSize = true,
            Location = new Point(59, 117),
            Text = "索敵"
        };

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

        private readonly Label _condTimerCaption = new Label
        {
            Location = new Point(128, 117),
            Size = new Size(60, 12)
        };

        private readonly Label _condTimer = new Label
        {
            AutoSize = true,
            Location = new Point(186, 117)
        };

        private readonly MainShipLabels _mainLabels = new MainShipLabels();

        public UpdateContext Context { get; set; }

        public int CurrentFleet { get; set; }

        public bool CombinedFleet { get; set; }

        public Action<int> ShowShipOnList { get; set; }

        public ShipInfoPanel()
        {
            Controls.AddRange(new Control[] {Guide, _presetAkashiTimer, _combinedFleet, _7Ships});
            BorderStyle = BorderStyle.FixedSingle;
            _mainLabels.CreateAllShipLabels(new MainShipPanels
            {
                PanelShipInfo = this,
                Panel7Ships = _7Ships,
                PanelCombinedFleet = _combinedFleet
            }, ShipClickHandler);
            _battleInfo.Controls.AddRange(new Control[]
            {
                _enemyFighterPower, _enemyFighterPowerCaption, _formation, _resultRank
            });
            Controls.AddRange(new Control[]
            {
                _battleInfo,
                _lineOfSight, _lineOfSightCaption, _fighterPower, _fighterPowerCaption, _condTimer, _condTimerCaption
            });
            _resultRank.Click += ResultRankClick;
            Size = new Size(220, 134);
        }

        private void ShipClickHandler(object sender, EventArgs e)
        {
            var idx = (int)((Control)sender).Tag;
            var ship = (CombinedFleet
                ? Context.Sniffer.Fleets[0].Ships.Concat(Context.Sniffer.Fleets[1].Ships)
                : Context.Sniffer.Fleets[CurrentFleet].Ships).ElementAt(idx);
            if (!ship.Empty)
                ShowShipOnList(ship.Id);
        }

        public void ToggleHpPercent()
        {
            _mainLabels.ToggleHpPercent();
        }

        public bool ShowHpInPercent => _mainLabels.ShowHpInPercent;

        private bool _inSortie;

        public void SetCurrentFleet()
        {
            var inSortie = Context.Sniffer.InSortie;
            if (_inSortie || inSortie == -1)
            {
                _inSortie = inSortie != -1;
                return;
            }
            _inSortie = true;
            if (inSortie == 10)
            {
                CombinedFleet = true;
                CurrentFleet = 0;
            }
            else
            {
                CombinedFleet = false;
                CurrentFleet = inSortie;
            }
            SetCombined();
        }

        private bool _prevCombined;

        private void SetCombined()
        {
            if (Context.Sniffer.IsCombinedFleet && !_prevCombined)
            {
                CombinedFleet = true;
                CurrentFleet = 0;
            }
            _prevCombined = Context.Sniffer.IsCombinedFleet;
        }

        public new void Update()
        {
            var ships = Context.Sniffer.Fleets[CurrentFleet].ActualShips;
            _7Ships.Visible = ships.Count == 7;
            _mainLabels.SetShipLabels(ships);
            ShowCombinedFleet();
            _presetAkashiTimer.Visible = Context.Config.UsePresetAkashi;
            UpdateAkashiTimer();
            UpdateFighterPower(IsCombinedFighterPower);
            UpdateLoS();
            UpdateCondTimers();
        }

        private void ShowCombinedFleet()
        {
            if (!Context.Sniffer.IsCombinedFleet)
                CombinedFleet = false;
            _combinedFleet.Visible = CombinedFleet;
            if (CombinedFleet)
            {
                _mainLabels.SetCombinedShipLabels(Context.Sniffer.Fleets[0].ActualShips,
                    Context.Sniffer.Fleets[1].ActualShips);
            }
        }

        private bool IsCombinedFighterPower => CombinedFleet &&
                                               (Context.Sniffer.Battle.BattleState == BattleState.None ||
                                                Context.Sniffer.Battle.EnemyIsCombined);

        private void UpdateFighterPower(bool combined)
        {
            var fleets = Context.Sniffer.Fleets;
            var fp = combined
                ? fleets[0].FighterPower + fleets[1].FighterPower
                : fleets[CurrentFleet].FighterPower;
            _fighterPower.Text = fp.Min.ToString("D");
            var cr = combined
                ? fleets[0].ContactTriggerRate + fleets[1].ContactTriggerRate
                : fleets[CurrentFleet].ContactTriggerRate;
            var text = "制空: " + (fp.Diff ? fp.RangeString : fp.Min.ToString()) +
                       $" 触接: {cr * 100:f1}";
            ToolTip.SetToolTip(_fighterPower, text);
            ToolTip.SetToolTip(_fighterPowerCaption, text);
        }

        private void UpdateLoS()
        {
            var fleet = Context.Sniffer.Fleets[CurrentFleet];
            _lineOfSight.Text = RoundDown(fleet.GetLineOfSights(1)).ToString("F1");
            var text = $"係数2: {RoundDown(fleet.GetLineOfSights(2)):F1}\r\n" +
                       $"係数3: {RoundDown(fleet.GetLineOfSights(3)):F1}\r\n" +
                       $"係数4: {RoundDown(fleet.GetLineOfSights(4)):F1}";
            ToolTip.SetToolTip(_lineOfSight, text);
            ToolTip.SetToolTip(_lineOfSightCaption, text);
        }

        private double RoundDown(double number)
        {
            return Math.Floor(number * 10) / 10.0;
        }

        public void UpdateBattleInfo()
        {
            ResetBattleInfo();
            if (Context.Sniffer.Battle.BattleState == BattleState.None)
                return;
            _battleInfo.BringToFront();
            var battle = Context.Sniffer.Battle;
            _formation.Text = new[] {"同航戦", "反航戦", "T字有利", "T字不利"}[battle.Formation[2] - 1];
            UpdateBattleFighterPower();
            if ((Context.Config.Spoilers & Spoiler.ResultRank) != 0)
                ShowResultRank();
        }

        private void ResetBattleInfo()
        {
            _formation.Text = "";
            _enemyFighterPower.Text = "";
            _fighterPower.ForeColor = DefaultForeColor;
            _fighterPowerCaption.Text = "制空";
            _resultRank.Text = "判定";
            _battleInfo.Visible = Context.Sniffer.Battle.BattleState != BattleState.None;
        }

        private void UpdateBattleFighterPower()
        {
            UpdateEnemyFighterPower();
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

        private void UpdateEnemyFighterPower()
        {
            var fp = Context.Sniffer.Battle.EnemyFighterPower;
            _enemyFighterPower.Text = fp.AirCombat + fp.UnknownMark;
            var toolTip = fp.AirCombat == fp.Interception ? "" : "防空: " + fp.Interception + fp.UnknownMark;
            ToolTip.SetToolTip(_enemyFighterPower, toolTip);
            ToolTip.SetToolTip(_enemyFighterPowerCaption, toolTip);
        }

        private void UpdateAirRaidFighterPower()
        {
            var fp = Context.Sniffer.Battle.FighterPower;
            _fighterPower.Text = fp.Min.ToString();
            var toolTop = fp.Diff ? fp.RangeString : "";
            ToolTip.SetToolTip(_fighterPower, toolTop);
            ToolTip.SetToolTip(_fighterPowerCaption, toolTop);
        }

        private static Color AirControlLevelColor(BattleInfo battle)
        {
            return new[]
                {DefaultForeColor, DefaultForeColor, CUDColors.Blue, CUDColors.Green, CUDColors.Orange, CUDColors.Red}[
                battle.BattleState == BattleState.Night ? 0 : battle.AirControlLevel + 1];
        }

        private static string AirControlLevelString(BattleInfo battle)
        {
            return new[] {"制空", "拮抗", "確保", "優勢", "劣勢", "喪失"}[
                battle.BattleState == BattleState.Night ? 0 : battle.AirControlLevel + 1];
        }

        private void ShowResultRank()
        {
            var result = new[] {"完全S", "勝利S", "勝利A", "勝利B", "敗北C", "敗北D", "敗北E"};
            _resultRank.Text = result[(int)Context.Sniffer.Battle.ResultRank];
        }

        private void ResultRankClick(object sender, EventArgs e)
        {
            ShowResultRank();
        }

        private void UpdateCondTimers()
        {
            DateTime timer;
            if (CombinedFleet)
            {
                var timer1 = Context.Sniffer.GetConditionTimer(0);
                var timer2 = Context.Sniffer.GetConditionTimer(1);
                timer = timer2 > timer1 ? timer2 : timer1;
            }
            else
            {
                timer = Context.Sniffer.GetConditionTimer(CurrentFleet);
            }
            if (timer == DateTime.MinValue)
            {
                _condTimerCaption.Text = "";
                _condTimer.Text = "";
                return;
            }
            var span = TimeSpan.FromSeconds(Math.Ceiling((timer - Context.GetNow()).TotalSeconds));
            if (span >= TimeSpan.FromMinutes(9) && Context.Config.NotifyConditions.Contains(40))
            {
                _condTimerCaption.Text = "cond40まで";
                _condTimer.Text = (span - TimeSpan.FromMinutes(9)).ToString(@"mm\:ss");
                _condTimer.ForeColor = DefaultForeColor;
            }
            else
            {
                _condTimerCaption.Text = "cond49まで";
                _condTimer.Text = (span >= TimeSpan.Zero ? span : TimeSpan.Zero).ToString(@"mm\:ss");
                _condTimer.ForeColor = span <= TimeSpan.Zero ? CUDColors.Red : DefaultForeColor;
            }
        }

        public Label AkashiRepairTimer { get; set; }

        public void UpdateTimers()
        {
            UpdateAkashiTimer();
            UpdatePresetAkashiTimer();
        }

        private void UpdateAkashiTimer()
        {
            if (Context.Config.UsePresetAkashi)
                UpdatePresetAkashiTimer();
            _mainLabels.SetAkashiTimer(Context.Sniffer.Fleets[CurrentFleet].ActualShips,
                Context.Sniffer.AkashiTimer.GetTimers(CurrentFleet, Context.GetNow()));
        }

        private void UpdatePresetAkashiTimer()
        {
            var now = Context.GetNow();
            var akashi = Context.Sniffer.AkashiTimer;
            var span = akashi.GetPresetDeckTimer(now);
            var color = span == TimeSpan.Zero && akashi.CheckPresetRepairing() ? CUDColors.Red : DefaultForeColor;
            var text = span == TimeSpan.MinValue ? "" : span.ToString(@"mm\:ss");
            AkashiRepairTimer.ForeColor = color;
            AkashiRepairTimer.Text = text;
            if (akashi.CheckPresetRepairing() && !akashi.CheckRepairing(CurrentFleet, now))
            {
                _presetAkashiTimer.ForeColor = color;
                _presetAkashiTimer.Text = text;
            }
            else
            {
                _presetAkashiTimer.ForeColor = DefaultForeColor;
                _presetAkashiTimer.Text = "";
            }
        }
    }
}