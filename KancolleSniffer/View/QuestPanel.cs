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
        private const int TopMargin = 5;
        private const int LeftMargin = 2;
        private const int LabelHeight = 12;
        private const int LineHeight = 14;
        private const int Lines = 6;
        private readonly QuestLabels[] _labels = new QuestLabels[Lines];
        private QuestStatus[] _questList = new QuestStatus[0];
        private ListScroller _listScroller;

        public QuestPanel()
        {
            CreateLabels();
            SetupListScroller();
        }

        private void CreateLabels()
        {
            SuspendLayout();
            for (var i = 0; i < Lines; i++)
            {
                var y = TopMargin + i * LineHeight;
                _labels[i] = new QuestLabels
                {
                    Color = new Label
                    {
                        Location = new Point(LeftMargin, y + 1),
                        Size = new Size(4, LabelHeight - 1)
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
                        Size = new Size(29, LabelHeight),
                        TextAlign = ContentAlignment.MiddleRight
                    }
                };
                _labels[i].Name.DoubleClick += NameLabelDoubleClickHandler;
                // ReSharper disable once CoVariantArrayConversion
                Controls.AddRange(_labels[i].Labels);
            }
            ResumeLayout();
        }

        private void SetupListScroller()
        {
            _listScroller = new ListScroller(this, _labels[0].Labels, _labels[Lines - 1].Labels)
            {
                Lines = Lines,
                Padding = TopMargin
            };
            _listScroller.Update += ShowQuestList;
        }

        public void Update(QuestStatus[] quests)
        {
            _questList = quests;
            _listScroller.DataCount = quests.Length;
            if (quests.Length <= Lines)
                _listScroller.Position = 0;
            ShowQuestList();
        }

        private void ShowQuestList()
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
                var quest = _questList[i + _listScroller.Position];
                SetQuest(labels, quest);
                if (quest.Count.Id == 0)
                {
                    ClearCount(labels.Count);
                    continue;
                }
                SetCount(labels.Count, quest.Count);
            }
            ResumeLayout(true);
            _listScroller.DrawMark();
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
    }
}