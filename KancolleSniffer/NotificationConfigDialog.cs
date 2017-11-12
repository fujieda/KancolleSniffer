using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public partial class NotificationConfigDialog : Form
    {
        private readonly Dictionary<string, NotificationSpec> _notifications;
        private readonly Dictionary<NotificationType, CheckBox> _configCheckBoxs;

        public NotificationConfigDialog(Dictionary<string, NotificationSpec> notifications,
            Dictionary<NotificationType, CheckBox> checkBoxs)
        {
            InitializeComponent();
            _notifications = notifications;
            _configCheckBoxs = checkBoxs;

            checkBoxFlashWindow.Tag = NotificationType.FlashWindow;
            checkBoxShowBaloonTip.Tag = NotificationType.ShowBaloonTip;
            checkBoxPlaySound.Tag = NotificationType.PlaySound;
            checkBoxPushbullet.Tag = NotificationType.Pushbullet;
            checkBoxRepeat.Tag = NotificationType.Repeat;

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
                    checkBoxRepeat.Checked =
                        checkBoxRepeat.Enabled = textBoxRepeat.Enabled = labelRepeat.Enabled = false;
                    textBoxRepeat.Text = "";
                    break;
                default:
                    checkBoxRepeat.Enabled = textBoxRepeat.Enabled = labelRepeat.Enabled = true;
                    textBoxRepeat.Text = notification.RepeatInterval.ToString();
                    break;
            }
            checkBoxFlashWindow.Checked = (notification.Flags & NotificationType.FlashWindow) != 0;
            checkBoxShowBaloonTip.Checked = (notification.Flags & NotificationType.ShowBaloonTip) != 0;
            checkBoxPlaySound.Checked = (notification.Flags & NotificationType.PlaySound) != 0;
            checkBoxPushbullet.Checked = (notification.Flags & NotificationType.Pushbullet) != 0;
            checkBoxRepeat.Checked = textBoxRepeat.Enabled = (notification.Flags & NotificationType.Repeat) != 0;
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var type = (NotificationType)checkBox.Tag;
            var spec = _notifications[(string)listBoxNotifications.SelectedItem];
            spec.Flags = checkBox.Checked ? spec.Flags | type : spec.Flags & ~type;
            if (type == NotificationType.Repeat)
                textBoxRepeat.Enabled = checkBox.Checked;
        }

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

            if (listBoxNotifications.SelectedIndex == -1)
                listBoxNotifications.SelectedIndex = 0;
        }
    }
}