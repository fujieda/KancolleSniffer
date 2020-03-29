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
    public class NDockPanel : Panel
    {
        private const int TopMargin = 3;
        private const int LeftMargin = 2;
        private const int LineHeight = 15;
        private readonly NDockLabels[] _labels = new NDockLabels[DockInfo.DockCount];

        private class NDockLabels
        {
            public Label Number { get; set; }
            public ShipLabel.Name Name { get; set; }
            public Label Timer { get; set; }
        }

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
        }

        private void SetCursor()
        {
            Cursor = Cursors.Hand;
            foreach (Control control in Controls)
                control.Cursor = Cursors.Hand;
        }

        public void SetClickHandler(EventHandler onClick)
        {
            Click += onClick;
            foreach (Control control in Controls)
                control.Click += onClick;
        }

        public void SetName(NameAndTimer[] ndock)
        {
            for (var i = 0; i < _labels.Length; i++)
                _labels[i].Name.SetName(ndock[i].Name);
        }

        public void UpdateTimers(Sniffer sniffer, DateTime now, bool showEndTime)
        {
            foreach (var entry in _labels.Zip(sniffer.NDock,
                (label, ndock) => new {label = label.Timer, timer = ndock.Timer}))
            {
                entry.label.ForeColor = entry.timer.IsFinished(now) ? CUDColors.Red : Color.Black;
                entry.label.Text = entry.timer.ToString(now, showEndTime);
            }
        }
    }
}