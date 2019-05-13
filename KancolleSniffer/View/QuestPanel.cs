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
using System.Linq;
using System.Windows.Forms;
using KancolleSniffer.Model;

// ReSharper disable CoVariantArrayConversion

namespace KancolleSniffer.View
{
    public class QuestLabels
    {
        public ShipLabel Color { get; set; }
        public ShipLabel Name { get; set; }
        public ShipLabel Count { get; set; }
        public ShipLabel Progress { get; set; }

        public ShipLabel[] Labels => new[] {Color, Count, Progress, Name};
    }

    public class QuestPanel : PanelWithToolTip
    {
        private const int TopMargin = 5;
        private const int LeftMargin = 2;
        private const int LabelHeight = 12;
        public const int LineHeight = 14;
        private QuestLabels[] _labels;
        private QuestStatus[] _questList = new QuestStatus[0];
        private ListScroller _listScroller;
        private int _lines;


        public void CreateLabels(int lines, EventHandler onDoubleClick)
        {
            _lines = LimitLines(lines);
            _labels = new QuestLabels[_lines];
            SuspendLayout();
            Height = (int)Math.Round((TopMargin * 2 + LineHeight * lines) * ShipLabel.ScaleFactor.Height);
            for (var i = 0; i < _lines; i++)
            {
                var y = TopMargin + i * LineHeight;
                _labels[i] = new QuestLabels
                {
                    Color = new ShipLabel
                    {
                        Location = new Point(LeftMargin, y + 1),
                        Size = new Size(4, LabelHeight - 1)
                    },
                    Name = new ShipLabel
                    {
                        Location = new Point(LeftMargin + 4, y),
                        Size = new Size(193, LabelHeight)
                    },
                    Count = new ShipLabel
                    {
                        Location = new Point(LeftMargin + 189, y),
                        AutoSize = true,
                        AnchorRight = true
                    },
                    Progress = new ShipLabel
                    {
                        Location = new Point(LeftMargin + 186, y),
                        Size = new Size(29, LabelHeight),
                        TextAlign = ContentAlignment.MiddleRight
                    }
                };
                _labels[i].Name.DoubleClick += onDoubleClick;
                Controls.AddRange(_labels[i].Labels);
                foreach (var label in _labels[i].Labels)
                    label.Scale();
            }
            ResumeLayout();
            SetupListScroller();
        }

        private static int LimitLines(int lines)
        {
            const int min = 4;
            const int max = 7;
            return Math.Min(Math.Max(lines, min), max);
        }

        private void SetupListScroller()
        {
            _listScroller = new ListScroller(this, _labels[0].Labels, _labels[_lines - 1].Labels)
            {
                Lines = _lines,
                Padding = TopMargin
            };
            _listScroller.Update += ShowQuestList;
            _listScroller.StartScroll += () => { ToolTip.Active = false; };
            _listScroller.EndScroll += () => { ToolTip.Active = true; };
        }

        public void Update(QuestStatus[] quests)
        {
            _listScroller.DataCount = quests.Length;
            _listScroller.Position = CalcScrollPosition(quests);
            _questList = quests.Select(q => q.Clone()).ToArray();
            ShowQuestList();
        }

        private int CalcScrollPosition(QuestStatus[] newQuests)
        {
            if (newQuests.Length <= _lines)
                return 0;
            var current = _listScroller.Position;
            var bottomIndex = current + _lines - 1;
            if (newQuests.Length < _questList.Length)
                return bottomIndex >= newQuests.Length ? newQuests.Length - _lines : current;
            var changedIndex = 0;
            if (newQuests.Length > _questList.Length)
            {
                changedIndex = _questList.TakeWhile((q, i) => q.Id == newQuests[i].Id).Count();
            }
            else if (newQuests.Length == _questList.Length)
            {
                changedIndex = _questList.TakeWhile((q, i) => q.Count.Equals(newQuests[i].Count)).Count();
                if (changedIndex == _questList.Length) // unchanged
                    return current;
            }
            if (changedIndex < current)
                return changedIndex;
            if (changedIndex > bottomIndex)
                return current + changedIndex - bottomIndex;
            return current;
        }

        private void ShowQuestList()
        {
            SuspendLayout();
            for (var i = 0; i < _lines; i++)
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
            labels.Name.Text = labels.Progress.Text = "";
            ToolTip.SetToolTip(labels.Name, "");
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
    }
}