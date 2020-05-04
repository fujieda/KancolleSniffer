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

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KancolleSniffer.Model;

namespace KancolleSniffer.View.MainWindow
{
    public class KDockPanel : Panel, IUpdateTimers
    {
        private const int TopMargin = 3;
        private const int LeftMargin = 2;
        private const int LabelHeight = 12;
        private const int LineHeight = 15;
        private readonly KDockLabels[] _labels = new KDockLabels[DockInfo.DockCount];

        private class KDockLabels
        {
            public Label Number { get; set; }
            public Label Timer { get; set; }
        }

        public UpdateContext Context { private get; set; }

        public KDockPanel()
        {
            BorderStyle = BorderStyle.FixedSingle;
            for (var i = 0; i < _labels.Length; i++)
            {
                var y = TopMargin + i * LineHeight;
                _labels[i] = new KDockLabels
                {
                    Number = new Label
                    {
                        Location = new Point(LeftMargin, y),
                        AutoSize = true,
                        Text = "第" + new[] {"一", "二", "三", "四"}[i]
                    },
                    Timer = new Label
                    {
                        Location = new Point(LeftMargin + 26, y),
                        Size = new Size(47, LabelHeight),
                        TextAlign = ContentAlignment.MiddleRight
                    }
                };
            }
            Controls.AddRange(_labels.SelectMany(l => new Control[] {l.Number, l.Timer}).ToArray());
        }

        public void UpdateTimers()
        {
            var now = Context.GetStep().Now;
            foreach (var entry in _labels.Zip(Context.Sniffer.KDock,
                (label, kdock) => new {label = label.Timer, timer = kdock}))
            {
                SetTimerColor(entry.label, entry.timer, now);
                entry.label.Text = entry.timer.ToString(now);
            }
        }

        private void SetTimerColor(Label label, AlarmTimer timer, DateTime now)
        {
            label.ForeColor = timer.IsFinished(now) ? CUDColors.Red : Color.Black;
        }
    }
}