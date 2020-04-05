// Copyright (C) 2019 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KancolleSniffer.Model;

namespace KancolleSniffer.View
{
    public class NDockPanel : Panel, IUpdateTimers
    {
        private const int TopMargin = 3;
        private const int LeftMargin = 2;
        private const int LineHeight = 15;
        private readonly NDockLabels[] _labels = new NDockLabels[DockInfo.DockCount];
        private Label _caption;

        private class NDockLabels
        {
            public Label Number { get; set; }
            public ShipLabel.Name Name { get; set; }
            public Label Timer { get; set; }
        }

        public UpdateContext Context { private get; set; }

        public NDockPanel()
        {
            BorderStyle = BorderStyle.FixedSingle;
            for (var i = 0; i < _labels.Length; i++)
            {
                var y = TopMargin + i * LineHeight;
                _labels[i] = new NDockLabels
                {
                    Number = new Label
                    {
                        Location = new Point(LeftMargin, y),
                        AutoSize = true,
                        Text = "第" + new[] {"一", "二", "三", "四"}[i]
                    },
                    Name = new ShipLabel.Name(new Point(LeftMargin + 27, y), ShipNameWidth.NDock),
                    Timer = new GrowLeftLabel
                    {
                        Location = new Point(LeftMargin + 136, y - 1),
                        GrowLeft = true,
                        MinimumSize = new Size(0, LineHeight),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Cursor = Cursors.Hand
                    }
                };
            }
            Controls.AddRange(_labels.SelectMany(l => new Control[] {l.Number, l.Name, l.Timer}).ToArray());
            SetCursor();
            SetClickHandler();
        }

        public void SetClickHandler(Label caption)
        {
            caption.Click += ClickHandler;
            _caption = caption;
        }

        private void SetCursor()
        {
            Cursor = Cursors.Hand;
            foreach (Control control in Controls)
                control.Cursor = Cursors.Hand;
        }

        private void SetClickHandler()
        {
            Click += ClickHandler;
            foreach (Control control in Controls)
                control.Click += ClickHandler;
        }

        private void ClickHandler(object sender, EventArgs e)
        {
            Context.Config.ShowEndTime ^= TimerKind.NDock;
            SetCaption();
            UpdateTimers();
        }

        public new void Update()
        {
            for (var i = 0; i < _labels.Length; i++)
                _labels[i].Name.SetName(Context.Sniffer.NDock[i].Name);
            SetCaption();
        }

        private void SetCaption()
        {
            _caption.Text = (Context.Config.ShowEndTime & TimerKind.NDock) != 0 ? "入渠終了" : "入渠";
        }

        public void UpdateTimers()
        {
            var now = Context.GetNow();
            var showEndTime = (Context.Config.ShowEndTime & TimerKind.NDock) != 0;
            foreach (var entry in _labels.Zip(Context.Sniffer.NDock,
                (label, ndock) => new {label = label.Timer, timer = ndock.Timer}))
            {
                entry.label.ForeColor = entry.timer.IsFinished(now) ? CUDColors.Red : Color.Black;
                entry.label.Text = entry.timer.ToString(now, showEndTime);
            }
        }
    }
}