using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using KancolleSniffer.Model;
using Clipboard = KancolleSniffer.Util.Clipboard;

namespace KancolleSniffer.View.MainWindow
{
    public class FleetPanel : PanelWithToolTip, IUpdateTimers
    {
        private readonly ShipInfoPanel _shipInfoPanel = new ShipInfoPanel
        {
            Location = new Point(0, 15),
            Size = new Size(220, 133)
        };

        private readonly ChargeStatus[] _chargeStatus =
        {
            new ChargeStatus
            {
                Location = new Point(34, 0),
                Size = new Size(17, 13)
            },
            new ChargeStatus
            {
                Location = new Point(89, 0),
                Size = new Size(17, 13)
            },
            new ChargeStatus
            {
                Location = new Point(144, 0),
                Size = new Size(17, 13)
            },
            new ChargeStatus
            {
                Location = new Point(199, 0),
                Size = new Size(17, 13)
            }
        };

        private readonly TriangleMark[] _triangleMarks =
        {
            new TriangleMark
            {
                Location = new Point(1, 0),
                Name = "triangleMark1",
                Size = new Size(5, 13)
            },
            new TriangleMark
            {
                Location = new Point(56, 0),
                Name = "triangleMark2",
                Size = new Size(5, 13)
            },
            new TriangleMark
            {
                Location = new Point(111, 0),
                Name = "triangleMark3",
                Size = new Size(5, 13)
            },
            new TriangleMark
            {
                Location = new Point(166, 0),
                Name = "triangleMark4",
                Size = new Size(5, 13)
            }
        };

        private readonly Control[] _fleets =
        {
            new Label
            {
                Location = new Point(6, 1),
                Size = new Size(45, 12),
                Text = "第一"
            },
            new Label
            {
                Location = new Point(61, 1),
                Size = new Size(45, 12),
                Text = "第二"
            },
            new Label
            {
                Location = new Point(116, 1),
                Size = new Size(45, 12),
                Text = "第三"
            },
            new Label
            {
                Location = new Point(171, 1),
                Size = new Size(45, 12),
                Text = "第四"
            }
        };

        private UpdateContext _context;

        public UpdateContext Context
        {
            get => _context;
            set
            {
                _context = value;
                _shipInfoPanel.Context = value;
                foreach (var c in _chargeStatus)
                    c.Context = value;
            }
        }

        public FleetPanel()
        {
            SetupFleetClick();
            _fleets[0].MouseHover += labelFleet1_MouseHover;
            _fleets[0].MouseLeave += labelFleet1_MouseLeave;
            for (var i = 1; i < _triangleMarks.Length; i++)
                _triangleMarks[i].Visible = false;
            Controls.AddRange(_chargeStatus.Concat(_fleets).Concat(_triangleMarks).Concat(new[] {_shipInfoPanel})
                .ToArray());
        }

        private bool _started;

        public void Start()
        {
            _started = true;
            _shipInfoPanel.Guide.Visible = false;
        }

        public LinkLabel Guide => _shipInfoPanel.Guide;

        public Label AkashiRepairTimer
        {
            set => _shipInfoPanel.AkashiRepairTimer = value;
        }

        public Action<int> ShowShipOnList
        {
            set => _shipInfoPanel.ShowShipOnList = value;
        }

        public bool ShowHpInPercent => _shipInfoPanel.ShowHpInPercent;

        public void ToggleHpPercent() => _shipInfoPanel.ToggleHpPercent();

        public new void Update()
        {
            _shipInfoPanel.SetCurrentFleet();
            _shipInfoPanel.Update();
            UpdatePanelShipInfo();
        }

        public void UpdateBattleInfo()
        {
            _shipInfoPanel.UpdateBattleInfo();
        }

        private void UpdatePanelShipInfo()
        {
            _shipInfoPanel.Update();
            ShowCurrentFleetNumber();
            _fleets[0].Text = _shipInfoPanel.CombinedFleet ? CombinedName : "第一";
        }

        public void UpdateChargeInfo()
        {
            foreach (var status in _chargeStatus)
            {
                status.Update();
                ToolTip.SetToolTip(status, status.Text);
            }
        }

        private void ShowCurrentFleetNumber()
        {
            for (var i = 0; i < _triangleMarks.Length; i++)
                _triangleMarks[i].Visible = _shipInfoPanel.CurrentFleet == i;
        }

        private void SetupFleetClick()
        {
            SetupFleetClick(_fleets);
            // ReSharper disable once CoVariantArrayConversion
            SetupFleetClick(_chargeStatus);
        }

        private void SetupFleetClick(Control[] a)
        {
            a[0].Tag = 0;
            a[0].Click += labelFleet1_Click;
            a[0].DoubleClick += labelFleet1_DoubleClick;
            for (var fleet = 1; fleet < a.Length; fleet++)
            {
                a[fleet].Tag = fleet;
                a[fleet].Click += labelFleet_Click;
                a[fleet].DoubleClick += labelFleet_DoubleClick;
            }
        }

        private void labelFleet_Click(object sender, EventArgs e)
        {
            if (!_started)
                return;
            var fleet = (int)((Control)sender).Tag;
            if (_shipInfoPanel.CurrentFleet == fleet)
                return;
            _shipInfoPanel.CombinedFleet = false;
            _shipInfoPanel.CurrentFleet = fleet;
            UpdatePanelShipInfo();
        }

        private readonly SemaphoreSlim _clickSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _doubleClickSemaphore = new SemaphoreSlim(0);

        private async void labelFleet1_Click(object sender, EventArgs e)
        {
            if (!_started)
                return;
            if (_shipInfoPanel.CurrentFleet != 0)
            {
                labelFleet_Click(sender, e);
                return;
            }
            if (!_clickSemaphore.Wait(0))
                return;
            try
            {
                if (await _doubleClickSemaphore.WaitAsync(SystemInformation.DoubleClickTime))
                    return;
            }
            finally
            {
                _clickSemaphore.Release();
            }
            _shipInfoPanel.CombinedFleet = Context.Sniffer.IsCombinedFleet && !_shipInfoPanel.CombinedFleet;
            UpdatePanelShipInfo();
        }

        private void labelFleet1_MouseHover(object sender, EventArgs e)
        {
            _fleets[0].Text =
                _shipInfoPanel.CurrentFleet == 0 && Context.Sniffer.IsCombinedFleet && !_shipInfoPanel.CombinedFleet
                    ? "連合"
                    : "第一";
        }

        private void labelFleet1_MouseLeave(object sender, EventArgs e)
        {
            _fleets[0].Text = _shipInfoPanel.CombinedFleet ? CombinedName : "第一";
        }

        private string CombinedName
        {
            get
            {
                switch (Context.Sniffer.Fleets[0].CombinedType)
                {
                    case CombinedType.Carrier:
                        return "機動";
                    case CombinedType.Surface:
                        return "水上";
                    case CombinedType.Transport:
                        return "輸送";
                    default:
                        return "連合";
                }
            }
        }

        private void labelFleet_DoubleClick(object sender, EventArgs e)
        {
            if (!_started)
                return;
            var fleet = (int)((Control)sender).Tag;
            var text = TextGenerator.GenerateFleetData(Context.Sniffer, fleet);
            CopyFleetText(text, (Control)sender);
        }

        private void labelFleet1_DoubleClick(object sender, EventArgs e)
        {
            if (!_started)
                return;
            _doubleClickSemaphore.Release();
            var text = TextGenerator.GenerateFleetData(Context.Sniffer, 0);
            if (_shipInfoPanel.CombinedFleet)
                text += TextGenerator.GenerateFleetData(Context.Sniffer, 1);
            CopyFleetText(text, (Control)sender);
        }

        private void CopyFleetText(string text, Control fleetButton)
        {
            if (string.IsNullOrEmpty(text))
                return;
            Clipboard.SetText(text);
            ToolTip.Show("コピーしました。", fleetButton, 1000);
        }

        public void UpdateTimers()
        {
            _shipInfoPanel.UpdateTimers();
        }
    }
}