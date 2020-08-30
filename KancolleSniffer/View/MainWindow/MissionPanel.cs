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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KancolleSniffer.Model;

namespace KancolleSniffer.View.MainWindow
{
    public class MissionPanel : PanelWithToolTip, IUpdateTimers
    {
        private const int TopMargin = 3;
        private const int LeftMargin = 2;
        private const int LabelHeight = 12;
        private const int LineHeight = 15;
        private const int Lines = 3;
        private readonly MissionLabels[] _labels = new MissionLabels[Lines];
        private Label _caption;

        private class MissionLabels
        {
            public Label Number { get; set; }
            public Label Name { get; set; }
            public Label Params { get; set; }
            public Label Timer { get; set; }
        }

        public UpdateContext Context { private get; set; }

        public MissionPanel()
        {
            BorderStyle = BorderStyle.FixedSingle;
            for (var i = 0; i < Lines; i++)
            {
                var y = TopMargin + i * LineHeight;
                _labels[i] = new MissionLabels
                {
                    Number = new Label
                    {
                        Location = new Point(LeftMargin, y),
                        AutoSize = true,
                        Text = "第" + new[] {"二", "三", "四"}[i]
                    },
                    Params = new Label
                    {
                        Location = new Point(LeftMargin + 54, y),
                        Size = new Size(161, LabelHeight)
                    },
                    Name = new Label
                    {
                        Location = new Point(LeftMargin + 30, y),
                        Size = new Size(135, LabelHeight)
                    },
                    Timer = new GrowLeftLabel()
                    {
                        Location = new Point(LeftMargin + 216, y),
                        GrowLeft = true
                    }
                };
            }
            Controls.AddRange(_labels.SelectMany(l => new Control[] {l.Number, l.Params, l.Timer, l.Name}).ToArray());
            var timers = _labels.Select(l => l.Timer).ToArray();
            SetCursor(timers);
            SetClickHandler(timers);
        }

        public void SetClickHandler(Label caption)
        {
            caption.Click += ClickHandler;
            _caption = caption;
        }

        private void SetCursor(IEnumerable<Control> controls)
        {
            foreach (var control in controls)
                control.Cursor = Cursors.Hand;
        }

        private void SetClickHandler(IEnumerable<Control> controls)
        {
            foreach (var control in controls)
                control.Click += ClickHandler;
        }

        private void ClickHandler(Object sender, EventArgs e)
        {
            Context.Config.ShowEndTime ^= TimerKind.Mission;
            SetCaption();
            UpdateTimers();
        }

        public new void Update()
        {
            var names = Context.Sniffer.Missions.Select(mission => mission.Name).ToArray();
            for (var i = 0; i < Lines; i++)
            {
                var fleetParams = Context.Sniffer.Fleets[i + 1].MissionParameter;
                var inPort = string.IsNullOrEmpty(names[i]);
                var labels = _labels[i];
                labels.Params.Visible = inPort;
                if (inPort)
                {
                    labels.Params.BringToFront();
                }
                else
                {
                    labels.Params.SendToBack();
                }
                labels.Params.Text = fleetParams;
                labels.Name.Text = names[i];
                ToolTip.SetToolTip(labels.Name, inPort ? "" : fleetParams);
            }
            SetCaption();
        }

        private void SetCaption()
        {
            _caption.Text = (Context.Config.ShowEndTime & TimerKind.Mission) != 0 ? "遠征終了" : "遠征";
        }

        public void UpdateTimers()
        {
            var now = Context.GetStep().Now;
            var showEndTime = (Context.Config.ShowEndTime & TimerKind.Mission) != 0;
            for (var i = 0; i < Lines; i++)
            {
                var entry = Context.Sniffer.Missions[i];
                SetTimerColor(_labels[i].Timer, entry.Timer, now);
                _labels[i].Timer.Text = entry.Timer.ToString(now, showEndTime);
            }
        }

        private void SetTimerColor(Label label, AlarmTimer timer, DateTime now)
        {
            label.ForeColor = timer.IsFinished(now) ? CUDColors.Red : Color.Black;
        }
    }
}