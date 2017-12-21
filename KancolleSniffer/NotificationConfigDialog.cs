﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public partial class NotificationConfigDialog : Form
    {
        private readonly Dictionary<string, NotificationSpec> _notifications;
        private readonly Dictionary<NotificationType, CheckBox> _configCheckBoxs;
        private readonly ToolTip _tooltip = new ToolTip();

        public NotificationConfigDialog(Dictionary<string, NotificationSpec> notifications,
            Dictionary<NotificationType, CheckBox> checkBoxs)
        {
            InitializeComponent();
            _notifications = notifications;
            _configCheckBoxs = checkBoxs;

            checkBoxFlashWindow.Tag = NotificationType.FlashWindow;
            checkBoxShowBaloonTip.Tag = NotificationType.ShowBaloonTip;
            checkBoxPlaySound.Tag = NotificationType.PlaySound;
            checkBoxPush.Tag = NotificationType.Push;
            checkBoxRepeat.Tag = NotificationType.Repeat;
            checkBoxCont.Tag = NotificationType.Cont;

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
                case "大破警告":
                    textBoxPreliminary.Visible = labelPreliminary.Visible = textBoxRepeat.Visible =
                        labelRepeat.Visible = checkBoxRepeat.Visible = checkBoxCont.Visible = false;
                    break;
                default:
                    textBoxRepeat.Visible = labelRepeat.Visible = checkBoxRepeat.Visible = true;
                    checkBoxRepeat.Enabled = _configCheckBoxs[NotificationType.Repeat].Checked;
                    textBoxRepeat.Text = notification.RepeatInterval.ToString();
                    checkBoxCont.Visible = IsContAvailable;
                    textBoxPreliminary.Visible = labelPreliminary.Visible = IspreliminaryAvailable;
                    textBoxPreliminary.Text = notification.PreliminaryPeriod.ToString();
                    break;
            }
            checkBoxFlashWindow.Checked = (notification.Flags & NotificationType.FlashWindow) != 0;
            checkBoxShowBaloonTip.Checked = (notification.Flags & NotificationType.ShowBaloonTip) != 0;
            checkBoxPlaySound.Checked = (notification.Flags & NotificationType.PlaySound) != 0;
            checkBoxPush.Checked = (notification.Flags & NotificationType.Push) != 0;
            checkBoxRepeat.Checked = (notification.Flags & NotificationType.Repeat) != 0;
            _tooltip.SetToolTip(checkBoxCont,
                !IsContAvailable ? "" : notification.Name == "遠征終了" ? "再度遠征に出すまでリピートする。" : "再度入渠させるまでリピートする。");
            checkBoxCont.Checked = (notification.Flags & NotificationType.Cont) != 0;
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
                    _configCheckBoxs[NotificationType.Repeat].Checked && checkBox.Checked;
            }
        }

        private bool IsContAvailable =>
            new[] {"遠征終了", "入渠終了"}.Contains((string)listBoxNotifications.SelectedItem);

        private bool IspreliminaryAvailable =>
            new[] {"遠征終了", "入渠終了", "建造完了", "泊地修理20分経過", "疲労回復"}.Contains((string)listBoxNotifications.SelectedItem);

        private void textBoxRepeat_TextChanged(object sender, EventArgs e)
        {
            _notifications[(string)listBoxNotifications.SelectedItem].RepeatInterval =
                int.TryParse(textBoxRepeat.Text, out int interval) ? interval : 0;
        }

        private void NotificationConfigDialog_Load(object sender, EventArgs e)
        {
            checkBoxFlashWindow.Enabled = _configCheckBoxs[NotificationType.FlashWindow].Checked;
            checkBoxShowBaloonTip.Enabled = _configCheckBoxs[NotificationType.ShowBaloonTip].Checked;
            checkBoxPlaySound.Enabled = _configCheckBoxs[NotificationType.PlaySound].Checked;
            checkBoxRepeat.Enabled = _configCheckBoxs[NotificationType.Repeat].Checked;
            textBoxRepeat.Enabled = labelRepeat.Enabled = checkBoxCont.Enabled =
                checkBoxRepeat.Enabled && checkBoxRepeat.Checked;

            if (listBoxNotifications.SelectedIndex == -1)
                listBoxNotifications.SelectedIndex = 0;
        }

        private void textBoxpreliminary_TextChanged(object sender, EventArgs e)
        {
            _notifications[(string)listBoxNotifications.SelectedItem].PreliminaryPeriod =
                int.TryParse(textBoxPreliminary.Text, out int preliminary) ? preliminary : 0;
        }
    }
}