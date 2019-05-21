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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace KancolleSniffer.View
{
    [DesignerCategory("Code")]
    public class GrowLeftLabel : Label
    {
        private int _right = int.MinValue;
        private int _left;
        private bool _growLeft;

        public bool GrowLeft
        {
            get => _growLeft;
            set
            {
                _growLeft = value;
                AutoSize = true;
                Size = Size.Empty;
            }
        }

        public GrowLeftLabel()
        {
            UseMnemonic = false;
        }

        protected override void OnSizeChanged(EventArgs args)
        {
            base.OnSizeChanged(args);
            AdjustLocation();
        }

        protected override void OnLayout(LayoutEventArgs args)
        {
            base.OnLayout(args);
            AdjustLocation();
        }

        private void AdjustLocation()
        {
            if (!GrowLeft)
                return;
            if (_right == int.MinValue || _left != Left)
            {
                _right = Right;
                _left = Left;
                return;
            }
            if (_right == Right)
                return;
            _left -= Right - _right;
            Location = new Point(_left, Top);
        }
    }
}