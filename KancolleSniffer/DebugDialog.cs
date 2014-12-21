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
            checkBoxLogging.Checked = _config.DebugLogging;
            textBoxLogFile.Text = _config.DebugLogFile;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            _config.DebugLogging = checkBoxLogging.Checked;
            _config.DebugLogFile = textBoxLogFile.Text;
            _main.ApplyDebugLogSetting();
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
            DialogResult = DialogResult.Abort;
        }
    }

}
