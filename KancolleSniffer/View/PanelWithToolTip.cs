// Copyright (C) 2018 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

using System.Drawing;
using System.Windows.Forms;

namespace KancolleSniffer.View
{
    public class PanelWithToolTip : Panel
    {
        public ResizableToolTip ToolTip { get; } = new ResizableToolTip();

        protected sealed override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
            if (Font.Height != DefaultFont.Height)
                ToolTip.Font = new Font(ToolTip.Font.FontFamily, ToolTip.Font.Size * factor.Height);
        }
    }
}