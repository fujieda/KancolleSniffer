using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public partial class NotificationConfigDialog : Form
    {
        private readonly Dictionary<string, NotificationType> _notifications;
        private readonly Dictionary<NotificationType, CheckBox> _configCheckBoxs;

        public NotificationConfigDialog(Dictionary<string, NotificationType> notifications, Dictionary<NotificationType, CheckBox> checkBoxs)
        {
            InitializeComponent();
            _notifications = notifications;
            _configCheckBoxs = checkBoxs;

            checkBoxFlashWindow.Tag = NotificationType.FlashWindow;
            checkBoxShowBaloonTip.Tag = NotificationType.ShowBaloonTip;
            checkBoxPlaySound.Tag = NotificationType.PlaySound;

            // ReSharper disable once CoVariantArrayConversion
            listBoxNotifications.Items.AddRange(Config.NotificationNames);
        }

        private void listBoxNotifications_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxNotifications.SelectedItem == null)
                return;
            var notification = _notifications[(string)listBoxNotifications.SelectedItem];
            checkBoxFlashWindow.Checked = (notification & NotificationType.FlashWindow) != 0;
            checkBoxShowBaloonTip.Checked = (notification & NotificationType.ShowBaloonTip) != 0;
            checkBoxPlaySound.Checked = (notification & NotificationType.PlaySound) != 0;
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = (CheckBox)sender;
            if (checkBox.Checked)
            {
                _notifications[(string)listBoxNotifications.SelectedItem] |= (NotificationType)checkBox.Tag;
            }
            else
            {
                _notifications[(string)listBoxNotifications.SelectedItem] &= ~(NotificationType)checkBox.Tag;
            }
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
