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
using System.Windows.Forms;
using KancolleSniffer.Model;

namespace KancolleSniffer.View
{
    public class MainNDockLabels
    {
        private readonly NDockLabels[] _ndockLabels = new NDockLabels[DockInfo.DockCount];

        private class NDockLabels : ControlsArranger
        {
            public ShipLabel.Name Name { get; set; }
            public Label Timer { get; set; }

            public override Control[] Controls => new Control[] {Timer, Name};
        }

        public void Create(Control parent, EventHandler onClick)
        {
            const int lh = 15;
            for (var i = 0; i < _ndockLabels.Length; i++)
            {
                var y = i * lh;
                _ndockLabels[i] = new NDockLabels
                {
                    Name = new ShipLabel.Name(new Point(29, y + 3), ShipNameWidth.NDock),
                    Timer = new GrowLeftLabel
                    {
                        Location = new Point(138, y + 2),
                        GrowLeft = true,
                        MinimumSize = new Size(0, lh),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Cursor = Cursors.Hand
                    }
                };
                _ndockLabels[i].Arrange(parent);
                _ndockLabels[i].SetClickHandler(onClick);
            }
        }

        public void SetName(NameAndTimer[] ndock)
        {
            for (var i = 0; i < _ndockLabels.Length; i++)
                _ndockLabels[i].Name.SetName(ndock[i].Name);
        }

        public void SetTimer(int dock, AlarmTimer timer, DateTime now, bool finishTime)
        {
            var label = _ndockLabels[dock].Timer;
            label.ForeColor = timer.IsFinished(now) ? CUDColors.Red : Color.Black;
            label.Text = timer.ToString(now, finishTime);
        }
    }
}