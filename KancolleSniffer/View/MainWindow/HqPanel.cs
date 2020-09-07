using System;
using System.Drawing;
using System.Windows.Forms;
using KancolleSniffer.Model;

namespace KancolleSniffer.View.MainWindow
{
    public class HqPanel : PanelWithToolTip, IUpdateContext
    {
        public Label Login { get; } = new Label
        {
            Location = new Point(6, 2),
            Size = new Size(210, 28),
            Font = new Font("MS UI Gothic", 9.75f),
            TextAlign = ContentAlignment.MiddleLeft,
            Text = "艦これにログインしてください。\r\nログイン中ならログインし直してください。"
        };

        public Label PlayLog { get; } = new Label
        {
            Location = new Point(207, 3),
            AutoSize = true,
            Text = "*",
            Visible = false
        };

        private readonly DropDownButton _bucketHistoryButton = new DropDownButton
        {
            Location = new Point(146, 15),
            Size = new Size(14, 14)
        };

        private readonly Label _achievement = new Label
        {
            Location = new Point(166, 18),
            Size = new Size(33, 12),
            TextAlign = ContentAlignment.MiddleRight
        };

        private readonly Label _numOfBuckets = new Label
        {
            Location = new Point(116, 18),
            Size = new Size(30, 12),
            Text = "0",
            TextAlign = ContentAlignment.MiddleRight
        };

        private readonly Label _numOfEquips = new Label
        {
            Location = new Point(46, 18),
            Size = new Size(60, 12),
            Text = "0/0",
            TextAlign = ContentAlignment.MiddleRight
        };

        private readonly Label _numOfShips = new Label
        {
            Location = new Point(0, 18),
            Size = new Size(48, 12),
            Text = "0/0",
            TextAlign = ContentAlignment.MiddleRight
        };

        private readonly Label _bucketHistory = new Label
        {
            BorderStyle = BorderStyle.FixedSingle,
            Name = "labelBucketHistory",
            Size = new Size(61, 28),
            Text = "今日\r\n今週",
            TextAlign = ContentAlignment.MiddleRight,
            Visible = false
        };

        private readonly Control[] _captions =
        {
            new Label
            {
                Location = new Point(170, 3),
                AutoSize = true,
                Text = "戦果"
            },
            new Label
            {
                Location = new Point(113, 3),
                AutoSize = true,
                Text = "バケツ"
            },
            new Label
            {
                Location = new Point(65, 3),
                AutoSize = true,
                Text = "装備数"
            },
            new Label
            {
                Location = new Point(7, 3),
                AutoSize = true,
                Text = "艦娘数"
            }
        };

        public UpdateContext Context { private get; set; }

        public HqPanel()
        {
            BorderStyle = BorderStyle.FixedSingle;
            Controls.AddRange(new Control[]
                {Login, PlayLog, _bucketHistoryButton, _achievement, _numOfBuckets, _numOfEquips, _numOfShips});
            Controls.AddRange(_captions);
            _bucketHistoryButton.Click += BucketHistoryButtonClick;
            _numOfBuckets.Click += BucketHistoryButtonClick;
            _bucketHistory.Click += BucketHistoryButtonClick;
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            Parent.Controls.Add(_bucketHistory);
        }

        public new void Update()
        {
            UpdateNumOfShips();
            UpdateNumOfEquips();
            UpdateNumOfBuckets();
            UpdateBucketHistory();
            UpdateAchievement();
        }

        private void UpdateNumOfBuckets()
        {
            _numOfBuckets.Text = Context.Sniffer.Material.MaterialHistory[(int)Material.Bucket].Now.ToString("D");
        }

        private void UpdateAchievement()
        {
            var ac = Context.Sniffer.Achievement.Value;
            if (ac >= 10000)
                ac = 9999;
            _achievement.Text = ac >= 1000 ? ((int)ac).ToString("D") : ac.ToString("F1");
            ToolTip.SetToolTip(_achievement,
                "今月 " + Context.Sniffer.Achievement.ValueOfMonth.ToString("F1") + "\n" +
                "EO " + Context.Sniffer.ExMap.Achievement);
        }

        private void UpdateNumOfShips()
        {
            var ship = Context.Sniffer.ShipCounter;
            _numOfShips.Text = $"{ship.Now:D}/{ship.Max:D}";
            _numOfShips.ForeColor = ship.TooMany ? CUDColors.Red : Color.Black;
        }

        private void UpdateNumOfEquips()
        {
            var item = Context.Sniffer.ItemCounter;
            _numOfEquips.Text = $"{item.Now:D}/{item.Max:D}";
            _numOfEquips.ForeColor = item.TooMany ? CUDColors.Red : Color.Black;
        }

        private void UpdateBucketHistory()
        {
            var count = Context.Sniffer.Material.MaterialHistory[(int)Material.Bucket];
            var day = CutOverflow(count.Now - count.BegOfDay, 999);
            var week = CutOverflow(count.Now - count.BegOfWeek, 999);
            _bucketHistory.Text = $"{day:+#;-#;±0} 今日\n{week:+#;-#;±0} 今週";
        }

        private void BucketHistoryButtonClick(object sender, EventArgs e)
        {
            if (_bucketHistory.Visible)
            {
                _bucketHistory.Visible = false;
                _bucketHistoryButton.BackColor = DefaultBackColor;
            }
            else
            {
                _bucketHistory.Location = new Point(Location.X + Scaler.ScaleWidth(100), Location.Y + Scaler.ScaleHeight(30));
                _bucketHistory.Visible = true;
                _bucketHistory.BringToFront();
                _bucketHistoryButton.BackColor = CustomColors.ActiveButtonColor;
            }
        }

        private static int CutOverflow(int value, int limit)
        {
            if (value > limit)
                return limit;
            if (value < -limit)
                return -limit;
            return value;
        }
    }
}