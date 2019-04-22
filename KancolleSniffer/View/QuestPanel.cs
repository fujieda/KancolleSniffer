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
    public class QuestLabels
    {
        public Label Color { get; set; }
        public Label Name { get; set; }
        public ShipLabel Count { get; set; }
        public Label Progress { get; set; }

        public Label[] Labels => new[] {Color, Count, Progress, Name};
    }

    public class QuestPanel : PanelWithToolTip
    {
        const int TopMargin = 5;
        const int LeftMargin = 2;
        const int LineHeight = 14;
        private const int Lines = 6;
        private readonly QuestLabels[] _labels = new QuestLabels[Lines];
        private QuestStatus[] _questList = new QuestStatus[0];
        private int _listPosition;

        public QuestPanel()
        {
            const int height = 12;

            SuspendLayout();
            for (var i = 0; i < Lines; i++)
            {
                var y = TopMargin + i * LineHeight;
                _labels[i] = new QuestLabels
                {
                    Color = new Label
                    {
                        Location = new Point(LeftMargin, y + 1),
                        Size = new Size(4, height - 1)
                    },
                    Name = new Label
                    {
                        Location = new Point(LeftMargin + 4, y),
                        AutoSize = true
                    },
                    Count = new ShipLabel
                    {
                        Location = new Point(LeftMargin + 189, y),
                        AutoSize = true,
                        AnchorRight = true
                    },
                    Progress = new Label
                    {
                        Location = new Point(LeftMargin + 186, y),
                        Size = new Size(29, height),
                        TextAlign = ContentAlignment.MiddleRight
                    }
                };
                _labels[i].Name.DoubleClick += NameLabelDoubleClickHandler;
                // ReSharper disable once CoVariantArrayConversion
                Controls.AddRange(_labels[i].Labels);
            }
            ResumeLayout();
            SetScrollEventHandlers();
        }

        private void SetScrollEventHandlers()
        {
            foreach (var label in _labels[0].Labels)
            {
                label.MouseEnter += TopLineOnMouseEnter;
                label.MouseLeave += TopLineOnMouseLeave;
            }
            foreach (var label in _labels[Lines - 1].Labels)
            {
                label.MouseEnter += BottomLineOnMouseEnter;
                label.MouseLeave += BottomLineOnMouseLeave;
            }
            _topScrollRepeatTimer.Tick += TopLineOnMouseEnter;
            _bottomScrollRepeatTimer.Tick += BottomLineOnMouseEnter;
        }

        private readonly Timer _topScrollRepeatTimer = new Timer {Interval = 100};
        private readonly Timer _bottomScrollRepeatTimer = new Timer {Interval = 100};

        private void TopLineOnMouseEnter(object sender, EventArgs e)
        {
            if (_listPosition == 0)
                return;
            _listPosition--;
            ShowQuestList();
            _topScrollRepeatTimer.Start();
        }

        private void TopLineOnMouseLeave(object sender, EventArgs e)
        {
            _topScrollRepeatTimer.Stop();
        }

        private void BottomLineOnMouseEnter(object sender, EventArgs e)
        {
            if (_listPosition + Lines >= _questList.Length)
                return;
            _listPosition++;
            ShowQuestList();
            _bottomScrollRepeatTimer.Start();
        }

        private void BottomLineOnMouseLeave(object sender, EventArgs e)
        {
            _bottomScrollRepeatTimer.Stop();
        }

        public void Update(QuestStatus[] quests)
        {
            _questList = quests;
            if (quests.Length <= Lines)
                _listPosition = 0;
            ShowQuestList();
        }

        public void ShowQuestList()
        {
            SuspendLayout();
            for (var i = 0; i < Lines; i++)
            {
                var labels = _labels[i];
                if (i >= _questList.Length)
                {
                    ClearQuest(labels);
                    ClearCount(labels.Count);
                    continue;
                }
                var quest = _questList[i + _listPosition];
                SetQuest(labels, quest);
                if (quest.Count.Id == 0)
                {
                    ClearCount(labels.Count);
                    continue;
                }
                SetCount(labels.Count, quest.Count);
            }
            ResumeLayout(true);
            DrawMark();
        }

        private void ClearQuest(QuestLabels labels)
        {
            labels.Color.BackColor = DefaultBackColor;
            labels.Name.Text = labels.Count.Text = labels.Progress.Text = "";
        }

        private void ClearCount(Label label)
        {
            label.Text = "";
            label.ForeColor = Color.Black;
            ToolTip.SetToolTip(label, "");
        }

        private void SetQuest(QuestLabels labels, QuestStatus quest)
        {
            labels.Color.BackColor = quest.Color;
            labels.Name.Text = quest.Name;
            labels.Progress.Text = $"{quest.Progress:D}%";
            ToolTip.SetToolTip(labels.Name, quest.ToToolTip());
        }

        private void SetCount(Label label, QuestCount count)
        {
            label.Text = " " + count;
            label.ForeColor = count.Cleared ? CUDColors.Red : Color.Black;
            ToolTip.SetToolTip(label, count.ToToolTip());
        }

        private void NameLabelDoubleClickHandler(object sender, EventArgs e)
        {
            NameLabelDoubleClick?.Invoke(sender, e);
        }

        public event EventHandler NameLabelDoubleClick;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawMark();
        }

        private void DrawMark()
        {
            using (var g = CreateGraphics())
            {
                var topBrush = _listPosition > 0 ? Brushes.Black : new SolidBrush(BackColor);
                g.FillPolygon(topBrush,
                    new[]
                    {
                        new PointF(Width * 0.45f, TopMargin), new PointF(Width * 0.55f, TopMargin),
                        new PointF(Width * 0.5f, 0), new PointF(Width * 0.45f, TopMargin)
                    });
                var bottomBrush = _listPosition + Lines < _questList.Length
                    ? Brushes.Black
                    : new SolidBrush(BackColor);
                g.FillPolygon(bottomBrush,
                    new[]
                    {
                        new PointF(Width * 0.45f, Height - TopMargin - 2),
                        new PointF(Width * 0.55f, Height - TopMargin - 2),
                        new PointF(Width * 0.5f, Height - 2), new PointF(Width * 0.45f, Height - TopMargin - 2)
                    });
            }
        }
    }
}