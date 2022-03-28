using System;
using System.Windows.Forms;

namespace StayAlive.GUI
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            upDownTimeoutMinutes.Value = Properties.Settings.Default.TimeOutMinutes;
            checkBox1.Checked = Properties.Settings.Default.DisableWhenNotCharging;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.TimeOutMinutes = (int)upDownTimeoutMinutes.Value;
            Properties.Settings.Default.DisableWhenNotCharging = checkBox1.Checked;
            Properties.Settings.Default.Save();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
