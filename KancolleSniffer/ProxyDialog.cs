using System;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public partial class ProxyDialog : Form
    {
        private readonly ProxyConfig _config;
        private MainForm _main;

        public ProxyDialog(ProxyConfig config, MainForm main)
        {
            InitializeComponent();
            _config = config;
            _main = main;
        }

        private void ProxyDialog_Load(object sender, EventArgs e)
        {
            (_config.Auto ? radioButtonAutoConfigOn : radioButtonAutoConfigOff).PerformClick();
            textBoxListen.Text = _config.Listen.ToString("D");
            (_config.UseUpstream ? radioButtonUpstreamOn : radioButtonUpstreamOff).PerformClick();
            textBoxPort.Text = _config.UpstreamPort.ToString("D");
        }

        private void radioButtonAutoConfigOn_CheckedChanged(object sender, EventArgs e)
        {
            var on = ((RadioButton)sender).Checked;
            textBoxListen.Enabled = !on;
            labelListen.Enabled = !on;
        }

        private void radioButtonUpstreamOff_CheckedChanged(object sender, EventArgs e)
        {
            var off = ((RadioButton)sender).Checked;
            textBoxPort.Enabled = !off;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            var listen = -1;
            var port = -1;
            if (radioButtonAutoConfigOff.Checked && !ValidatePortNumber(textBoxListen, out listen))
                return;
            if (radioButtonUpstreamOn.Checked && !ValidatePortNumber(textBoxPort, out port))
                return;
            if (radioButtonAutoConfigOff.Checked && radioButtonUpstreamOn.Checked && listen == port)
            {
                ShowToolTip("受信と送信に同じポートは使えません。", textBoxPort);
                return;
            }
            _config.Auto = radioButtonAutoConfigOn.Checked;
            if (!_config.Auto)
                _config.Listen = listen;
            _config.UseUpstream = radioButtonUpstreamOn.Checked;
            if (_config.UseUpstream)
                _config.UpstreamPort = port;
            DialogResult = DialogResult.OK;
        }

        private bool ValidatePortNumber(TextBox textBox, out int result)
        {
            var s = textBox.Text;
            if (!int.TryParse(s, out result))
            {
                ShowToolTip("数字を入力してください。", textBox);
                return false;
            }
            if (result <= 0)
            {
                ShowToolTip("0より大きい数字を入力してください。", textBox);
                return false;
            }
            return true;
        }

        private void ShowToolTip(string message, Control control)
        {
            toolTipError.Show(message, control, 0, control.Height, 3000);
        }

        private void textBox_Enter(object sender, EventArgs e)
        {
            toolTipError.Hide((Control)sender);
        }
    }
}