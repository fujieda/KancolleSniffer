// Copyright (C) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
//
// This program is part of KancolleSniffer.
//
// KancolleSniffer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, see <http://www.gnu.org/licenses/>.

using System.Drawing;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public partial class ErrorDialog: Form
    {
        public ErrorDialog()
        {
            InitializeComponent();

            var icon = new Icon(SystemIcons.Error, 32, 32);
            labelSystemIcon.Image = icon.ToBitmap();
        }

        public DialogResult ShowDialog(IWin32Window owner, string message, string details)
        {
            labelMessage.Text = message;
            textBoxDetails.Text = details;
            return ShowDialog(owner);
        }
    }
}
