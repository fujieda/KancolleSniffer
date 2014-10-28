// Copyright (C) 2014 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public partial class ShipListForm : Form
    {
        private readonly Sniffer _sniffer;
        private readonly Config _config;
        private readonly List<ShipLabel[]> _labelList = new List<ShipLabel[]>();
        private const int HpLabelRight = 126;

        public ShipListForm(Sniffer sniffer, Config config)
        {
            InitializeComponent();
            _sniffer = sniffer;
            _config = config;
        }

        public void UpdateList()
        {
            if (!Visible)
                return;
            CreateListLabels();
            SetShipLabels();
        }

        private void CreateListLabels()
        {
            panelShipList.SuspendLayout();
            for (var i = _labelList.Count; i < _sniffer.Item.MaxShips; i++)
            {
                const int height = 12;
                var y = 3 + 16 * i + panelShipList.AutoScrollPosition.Y;
                var labels = new[]
                {
                    new ShipLabel {Location = new Point(1, y), Size = new Size(11, height)},
                    new ShipLabel {Location = new Point(HpLabelRight, y), AutoSize = true},
                    new ShipLabel
                    {
                        Location = new Point(132, y),
                        Size = new Size(23, height),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    new ShipLabel
                    {
                        Location = new Point(166, y),
                        Size = new Size(23, height),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    new ShipLabel
                    {
                        Location = new Point(191, y),
                        Size = new Size(41, height),
                        TextAlign = ContentAlignment.MiddleRight
                    },
                    new ShipLabel {Location = new Point(10, y), AutoSize = true}
                };
                _labelList.Add(labels);
                // ReSharper disable once CoVariantArrayConversion
                panelShipList.Controls.AddRange(labels);
                labels[1].SizeChanged += labelHP_SizeChanged;
            }
            for (var i = _labelList.Count; i > _sniffer.Item.MaxShips; i--)
            {
                foreach (var label in _labelList[i - 1])
                {
                    panelShipList.Controls.Remove(label);
                    label.Dispose();
                }
                _labelList.RemoveAt(i - 1);
            }
            panelShipList.ResumeLayout();
        }

        private class CompareShipByExp : IComparer<ShipStatus>
        {
            public int Compare(ShipStatus a, ShipStatus b)
            {
                if (a.Level != b.Level)
                    return b.Level - a.Level;
                if (a.ExpToNext != b.ExpToNext)
                    return a.ExpToNext - b.ExpToNext;
                return a.Spec.Id - b.Spec.Id;
            }
        }

        private void SetShipLabels()
        {
            var shipList = _sniffer.ShipList;
            var fn = new[] {"", "1", "2", "3", "4"};
            var i = 0;
            foreach (var s in shipList.OrderBy(s => s, new CompareShipByExp()))
            {
                var labels = _labelList[i++];
                labels[0].Text = fn[s.Fleet + 1];
                labels[1].SetHp(s);
                labels[2].SetCond(s);
                labels[3].SetLevel(s);
                labels[4].SetExpToNext(s);
                labels[5].SetName(s);
            }
        }

        private void labelHP_SizeChanged(object sender, EventArgs e)
        {
            var label = (Label)sender;
            label.Location = new Point(HpLabelRight - label.Width, label.Top);
        }

        private void ShipListForm_Load(object sender, EventArgs e)
        {
            var config = _config.ShipList;
            if (config.Location.X == int.MinValue)
                return;
            var bounds = new Rectangle(config.Location, config.Size);
            if (MainForm.IsVisibleOnAnyScreen(bounds))
                Location = bounds.Location;
            Size = bounds.Size;
        }

        private void ShipListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var config = _config.ShipList;
            var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            config.Location = bounds.Location;
            config.Size = bounds.Size;
            Hide();
            e.Cancel = true;
        }
    }
}