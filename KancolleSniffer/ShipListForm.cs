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
        private ShipStatus[] _currentList;

        public ShipListForm(Sniffer sniffer, Config config)
        {
            InitializeComponent();
            _sniffer = sniffer;
            _config = config;
            checkBoxShipType.Checked = config.ShipList.ShipType;
        }

        public void UpdateList()
        {
            if (!Visible)
                return;
            CreateList();
            CreateListLabels();
            SetShipLabels();
        }

        private void CreateList()
        {
            var ships = (IEnumerable<ShipStatus>)_sniffer.ShipList;
            if (!_config.ShipList.ShipType)
            {
                _currentList = ships.OrderBy(s => s, new CompareShipByExp()).ToArray();
                return;
            }
            var types = from id in (from s in ships select s.Spec.ShipType).Distinct()
                join stype in _sniffer.ShipTypeList on id equals stype.Id
                select new ShipStatus {Spec = new ShipSpec {Name = stype.Name, ShipType = stype.Id}, Level = 1000};
            _currentList = ships.Concat(types).OrderBy(s => s, new CompareShipByType()).ToArray();
        }

        private void CreateListLabels()
        {
            panelShipList.SuspendLayout();
            for (var i = _labelList.Count; i < _currentList.Length; i++)
            {
                const int height = 12;
                var y = 3 + 16 * i +
                    (int)Math.Round(panelShipList.AutoScrollPosition.Y / (_config.AutoScale ? ShipLabel.ScaleFactor.Height : 1f));
                var labels = new[]
                {
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
                    new ShipLabel {Location = new Point(10, y), AutoSize = true},
                    new ShipLabel {Location = new Point(1, y), AutoSize = true}
                };
                _labelList.Add(labels);
                if (_config.AutoScale)
                {
                    foreach (var label in labels)
                        label.Scale(ShipLabel.ScaleFactor);
                }
                // ReSharper disable once CoVariantArrayConversion
                panelShipList.Controls.AddRange(labels);
                labels[0].SizeChanged += labelHP_SizeChanged;
            }
            for (var i = _labelList.Count; i > _currentList.Length; i--)
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

        private class CompareShipByType : IComparer<ShipStatus>
        {
            public int Compare(ShipStatus a, ShipStatus b)
            {
                if (a.Spec.ShipType != b.Spec.ShipType)
                    return a.Spec.ShipType - b.Spec.ShipType;
                if (a.Level != b.Level)
                    return b.Level - a.Level;
                if (a.ExpToNext != b.ExpToNext)
                    return a.ExpToNext - b.ExpToNext;
                return a.Spec.Id - b.Spec.Id;
            }
        }

        private void SetShipLabels()
        {
            var fn = new[] {"", "1", "2", "3", "4"};
            var i = 0;
            foreach (var s in _currentList)
            {
                var labels = _labelList[i++];
                if (s.Level == 1000)
                {
                    for (var c = 0; c <= 3; c++)
                    {
                        labels[c].Text = "";
                        labels[c].BackColor = DefaultBackColor;
                    }
                    labels[4].SetName("");
                    labels[5].Text = s.Name;
                    labels[5].BackColor = Color.FromArgb(255, 220, 220, 220);
                    continue;
                }
                labels[0].SetHp(s);
                labels[1].SetCond(s);
                labels[2].SetLevel(s);
                labels[3].SetExpToNext(s);
                labels[4].SetName(s);
                labels[5].Text = fn[s.Fleet + 1];
                labels[5].BackColor = DefaultBackColor;
            }
        }

        private void labelHP_SizeChanged(object sender, EventArgs e)
        {
            var label = (Label)sender;
            label.Location = new Point(
                (int)Math.Round(HpLabelRight * (_config.AutoScale ? ShipLabel.ScaleFactor.Width : 1f)) -
                label.Width, label.Top);
        }

        private void ShipListForm_Load(object sender, EventArgs e)
        {
            var panelWidth = 235 + SystemInformation.VerticalScrollBarWidth;
            if (!_config.AutoScale)
            {
                // DPIに応じて拡大したくないときはフォントを小さくする
                Font = new Font(DefaultFont.Name, DefaultFont.Size / ShipLabel.ScaleFactor.Height);
            }
            else
            {
                // フォントサイズに合わせて拡大する
                panelWidth = (int)Math.Round(panelWidth * ShipLabel.ScaleFactor.Width);
            }
            panelShipList.Width = panelWidth;
            Width = panelWidth + 12 + (Width - ClientSize.Width);
            MinimumSize = new Size(Width, 0);
            MaximumSize = new Size(Width, int.MaxValue);
            var config = _config.ShipList;
            if (config.Location.X == int.MinValue)
                return;
            var bounds = new Rectangle(config.Location, config.Size);
            if (MainForm.IsVisibleOnAnyScreen(bounds))
                Location = bounds.Location;
            Height = bounds.Height;
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

        public void ShowShip(int id)
        {
            var i = Array.FindIndex(_currentList, s => s.Id == id);
            if (i == -1)
                return;
            var y = (int)Math.Round((_config.AutoScale ? ShipLabel.ScaleFactor.Height : 1f) * 16 * i);
            panelShipList.AutoScrollPosition = new Point(0, y);
        }

        private void panelShipList_MouseEnter(object sender, EventArgs e)
        {
            panelShipList.Focus();
        }

        private void checkBoxShipType_CheckedChanged(object sender, EventArgs e)
        {
            _config.ShipList.ShipType = checkBoxShipType.Checked;
            UpdateList();
        }
    }
}