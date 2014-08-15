using System;
using System.IO;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public partial class DebugDialog : Form
    {
        public bool Logging { get; set; }
        public string LogFile { get; set; }

        public DebugDialog()
        {
            InitializeComponent();
        }

        private void DebugDialog_Load(object sender, EventArgs e)
        {
            checkBoxLogging.Checked = Logging;
            textBoxLogFile.Text = LogFile;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            Logging = checkBoxLogging.Checked;
            LogFile = textBoxLogFile.Text;
        }

        private void buttonLogFileOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = textBoxLogFile.Text;
            openFileDialog.InitialDirectory = Path.GetDirectoryName(textBoxLogFile.Text);
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                textBoxLogFile.Text = openFileDialog.FileName;
        }
    }

}
