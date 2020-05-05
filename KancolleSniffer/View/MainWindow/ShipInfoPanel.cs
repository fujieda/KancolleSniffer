using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KancolleSniffer.Model;

namespace KancolleSniffer.View.MainWindow
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

        private readonly BattleInfoPanel _battleInfo = new BattleInfoPanel
        {
            Location = new Point(59, 116),
            Size = new Size(157, 14),
            Visible = false
        };

        private readonly FighterPower _fighterPower;

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

        private UpdateContext _context;

        public UpdateContext Context
        {
            get => _context;
            set => _battleInfo.Context = _fighterPower.Context = _context = value;
        }

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
            Controls.AddRange(new Control[]
            {
                _battleInfo,
                _lineOfSight, _lineOfSightCaption, _condTimer, _condTimerCaption
            });
            _fighterPower = new FighterPower(this);
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
            _fighterPower.UpdateFighterPower();
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
            _battleInfo.Update();
            _fighterPower.UpdateBattleFighterPower();
        }

        private void ResetBattleInfo()
        {
            _battleInfo.Reset();
            _fighterPower.Reset();
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
            var span = TimeSpan.FromSeconds(Math.Ceiling((timer - Context.GetStep().Now).TotalSeconds));
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
            UpdateCondTimers();
            UpdateAkashiTimer();
            UpdatePresetAkashiTimer();
        }

        private void UpdateAkashiTimer()
        {
            if (Context.Config.UsePresetAkashi)
                UpdatePresetAkashiTimer();
            _mainLabels.SetAkashiTimer(Context.Sniffer.Fleets[CurrentFleet].ActualShips,
                Context.Sniffer.AkashiTimer.GetTimers(CurrentFleet, Context.GetStep().Now));
        }

        private void UpdatePresetAkashiTimer()
        {
            var now = Context.GetStep().Now;
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