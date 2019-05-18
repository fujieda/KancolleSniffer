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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KancolleSniffer.View
{
    public abstract class ControlsArranger
    {
        public abstract Control[] Controls { get; }

        public void Arrange(Control parent)
        {
            SetParent(parent);
            Scale();
        }

        public void Arrange(Control parent, Color color)
        {
            Arrange(parent);
            SetBackColor(color);
        }

        private void SetParent(Control parent)
        {
            parent.Controls.AddRange(Controls);
        }

        private void Scale()
        {
            foreach (var control in Controls)
                Scaler.Scale(control);
        }

        private void SetBackColor(Color color)
        {
            foreach (var control in Controls)
                control.BackColor = color;
        }

        public void SetClickHandler(EventHandler onClick)
        {
            foreach (var control in Controls)
                control.Click += onClick;
        }

        public void SetTag(int index)
        {
            foreach (var control in Controls)
                control.Tag = index;
        }
    }
}