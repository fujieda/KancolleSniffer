// Copyright (C) 2017 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using KancolleSniffer.View;

namespace KancolleSniffer.Notification
{
    public partial class ConfigDialog : Form
    {
        private readonly Dictionary<string, NotificationSpec> _notifications;
        private readonly Dictionary<NotificationType, CheckBox> _configCheckBoxes;
        private readonly ResizableToolTip _toolTip = new ResizableToolTip();

        public ConfigDialog(Dictionary<string, NotificationSpec> notifications,
            Dictionary<NotificationType, CheckBox> checkBoxes)
        {
            InitializeComponent();
            _notifications = notifications;
            _configCheckBoxes = checkBoxes;

            checkBoxFlashWindow.Tag = NotificationType.FlashWindow;
            checkBoxShowBalloonTip.Tag = NotificationType.ShowBaloonTip;
            checkBoxPlaySound.Tag = NotificationType.PlaySound;
            checkBoxPush.Tag = NotificationType.Push;
            checkBoxRepeat.Tag = NotificationType.Repeat;
            checkBoxCont.Tag = NotificationType.Cont;
            checkBoxPreliminary.Tag = NotificationType.Preliminary;

            // ReSharper disable once CoVariantArrayConversion
            listBoxNotifications.Items.AddRange(Config.NotificationNames);
        }

        private void listBoxNotifications_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxNotifications.SelectedItem == null)
                return;
            var notification = _notifications[(string)listBoxNotifications.SelectedItem];
            switch (notification.Name)
            {
                case "艦娘数超過":
                case "装備数超過":
                    textBoxPreliminary.Visible = labelPreliminary.Visible = checkBoxPreliminary.Visible =
                        textBoxRepeat.Visible = labelRepeat.Visible = checkBoxRepeat.Visible =
                            checkBoxCont.Visible = false;
                    break;
                default:
                    textBoxRepeat.Visible = labelRepeat.Visible = checkBoxRepeat.Visible = true;
                    checkBoxRepeat.Enabled = _configCheckBoxes[NotificationType.Repeat].Checked;
                    textBoxRepeat.Text = notification.RepeatInterval.ToString();
                    checkBoxCont.Visible = IsContAvailable;
                    textBoxPreliminary.Visible =
                        labelPreliminary.Visible = checkBoxPreliminary.Visible = IsPreliminaryAvailable;
                    textBoxPreliminary.Text = notification.PreliminaryPeriod.ToString();
                    break;
            }
            checkBoxFlashWindow.Checked = (notification.Flags & NotificationType.FlashWindow) != 0;
            checkBoxShowBalloonTip.Checked = (notification.Flags & NotificationType.ShowBaloonTip) != 0;
            checkBoxPlaySound.Checked = (notification.Flags & NotificationType.PlaySound) != 0;
            checkBoxPush.Checked = (notification.Flags & NotificationType.Push) != 0;
            checkBoxRepeat.Checked = (notification.Flags & NotificationType.Repeat) != 0;
            _toolTip.SetToolTip(checkBoxCont,
                !IsContAvailable ? "" :
                notification.Name == "遠征終了" ? "再度遠征に出すまでリピートする。" : "再度入渠させるまでリピートする。");
            checkBoxCont.Checked = (notification.Flags & NotificationType.Cont) != 0;
            checkBoxPreliminary.Checked = (notification.Flags & NotificationType.Preliminary) != 0;
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var type = (NotificationType)checkBox.Tag;
            var spec = _notifications[(string)listBoxNotifications.SelectedItem];
            spec.Flags = checkBox.Checked ? spec.Flags | type : spec.Flags & ~type;
            if (type == NotificationType.Repeat)
            {
                textBoxRepeat.Enabled = labelRepeat.Enabled = checkBoxCont.Enabled =
                    _configCheckBoxes[NotificationType.Repeat].Checked && checkBox.Checked;
            }
            if (type == NotificationType.Preliminary)
                textBoxPreliminary.Enabled = labelPreliminary.Enabled = checkBox.Checked;
        }

        private bool IsContAvailable =>
            new[] {"遠征終了", "入渠終了"}.Contains((string)listBoxNotifications.SelectedItem);

        private bool IsPreliminaryAvailable =>
            new[] {"遠征終了", "入渠終了", "建造完了", "泊地修理20分経過", "疲労回復"}.Contains((string)listBoxNotifications.SelectedItem);

        private void textBoxRepeat_TextChanged(object sender, EventArgs e)
        {
            _notifications[(string)listBoxNotifications.SelectedItem].RepeatInterval =
                int.TryParse(textBoxRepeat.Text, out var interval) && interval > 0 ? interval : 0;
        }

        private void NotificationConfigDialog_Load(object sender, EventArgs e)
        {
            checkBoxFlashWindow.Enabled = _configCheckBoxes[NotificationType.FlashWindow].Checked;
            checkBoxShowBalloonTip.Enabled = _configCheckBoxes[NotificationType.ShowBaloonTip].Checked;
            checkBoxPlaySound.Enabled = _configCheckBoxes[NotificationType.PlaySound].Checked;
            checkBoxRepeat.Enabled = _configCheckBoxes[NotificationType.Repeat].Checked;
            textBoxRepeat.Enabled = labelRepeat.Enabled = checkBoxCont.Enabled =
                checkBoxRepeat.Enabled && checkBoxRepeat.Checked;
            textBoxPreliminary.Enabled = checkBoxPreliminary.Checked;

            var selected = listBoxNotifications.SelectedIndex;
            listBoxNotifications.SelectedIndex = -1;
            listBoxNotifications.SelectedIndex = selected == -1 ? 0 : selected;
        }

        private void textBoxPreliminary_TextChanged(object sender, EventArgs e)
        {
            _notifications[(string)listBoxNotifications.SelectedItem].PreliminaryPeriod =
                int.TryParse(textBoxPreliminary.Text, out var preliminary) && preliminary > 0 ? preliminary : 0;
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
            if (factor.Height > 1)
                _toolTip.Font = new Font(_toolTip.Font.FontFamily, _toolTip.Font.Size * factor.Height);
        }
    }
}