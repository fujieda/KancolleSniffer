using System;
using System.IO;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public partial class DebugDialog : Form
    {
        private readonly Config _config;
        private readonly MainForm _main;

        public DebugDialog(Config config, MainForm main)
        {
            InitializeComponent();
            _config = config;
            _main = main;
        }

        private void DebugDialog_Load(object sender, EventArgs e)
        {
            checkBoxLogging.Checked = _config.Logging;
            textBoxLogFile.Text = _config.LogFile;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            _config.Logging = checkBoxLogging.Checked;
            _config.LogFile = textBoxLogFile.Text;
            _main.ApplyLogSetting();
        }

        private void buttonLogFileOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = textBoxLogFile.Text;
            openFileDialog.InitialDirectory = Path.GetDirectoryName(textBoxLogFile.Text);
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                textBoxLogFile.Text = openFileDialog.FileName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _main.SetPlayLog(textBoxLogFile.Text);
        }
    }

}
