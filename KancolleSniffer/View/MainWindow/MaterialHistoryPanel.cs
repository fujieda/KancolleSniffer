using System;
using System.Drawing;
using System.Windows.Forms;

namespace KancolleSniffer.View.MainWindow
{
    public class MaterialHistoryPanel : Panel, IUpdateContext
    {
        private readonly Label _bauxite = new Label
        {
            Location = new Point(117, 2),
            Size = new Size(42, 48),
            Text = "ボーキ",
            TextAlign = ContentAlignment.TopRight
        };

        private readonly Label _steel = new Label
        {
            Location = new Point(78, 2),
            Size = new Size(42, 48),
            Text = "鋼材",
            TextAlign = ContentAlignment.TopRight
        };

        private readonly Label _bullet = new Label
        {
            Location = new Point(39, 2),
            Size = new Size(42, 48),
            Text = "弾薬",
            TextAlign = ContentAlignment.TopRight
        };

        private readonly Label _fuel = new Label
        {
            Location = new Point(0, 2),
            Size = new Size(42, 48),
            Text = "燃料",
            TextAlign = ContentAlignment.TopRight
        };

        private readonly Label _caption = new Label
        {
            AutoSize = true,
            Location = new Point(158, 14),
            Size = new Size(29, 36),
            Text = "母港\r\n今日\r\n今週"
        };

        private Label _button;

        public UpdateContext Context { private get; set; }

        public void SetClickHandler(Label caption, Label button)
        {
            caption.Click += ClickHandler;
            button.Click += ClickHandler;
            _button = button;
        }

        public MaterialHistoryPanel()
        {
            BorderStyle = BorderStyle.FixedSingle;
            Visible = false;
            Size = new Size(188, 52);
            Controls.AddRange(new Control[]{_bullet, _fuel, _bauxite, _steel, _caption});
            SetClickHandler();
        }

        private void SetClickHandler()
        {
            Click += ClickHandler;
            foreach (Control control in Controls)
                control.Click += ClickHandler;
        }

        private void ClickHandler(object sender, EventArgs e)
        {
            if (Visible)
            {
                Visible = false;
                _button.BackColor = DefaultBackColor;
            }
            else
            {
                Visible = true;
                BringToFront();
                _button.BackColor = CustomColors.ActiveButtonColor;
            }
        }

        public new void Update()
        {
            var labels = new[] {_fuel, _bullet, _steel, _bauxite };
            var text = new[] { "燃料", "弾薬", "鋼材", "ボーキ" };
            for (var i = 0; i < labels.Length; i++)
            {
                var count = Context.Sniffer.Material.MaterialHistory[i];
                var port = CutOverflow(count.Now - Context.Sniffer.Material.PrevPort[i], 99999);
                var day = CutOverflow(count.Now - count.BegOfDay, 99999);
                var week = CutOverflow(count.Now - count.BegOfWeek, 99999);
                labels[i].Text = $"{text[i]}\n{port:+#;-#;±0}\n{day:+#;-#;±0}\n{week:+#;-#;±0}";
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

        public void UpdateTimers()
        {
        }
    }
}