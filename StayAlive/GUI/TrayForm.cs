using System;
using System.Windows.Forms;
using StayAliveLogic;

namespace StayAlive.GUI
{
    public partial class TrayForm : Form, IStayAliveLogicHost
    {
        private Service _service;
        
        public TrayForm()
		{
			InitializeComponent();
		}

        public void ShowBalloonTip(string notifyMessage)
        {
            notifyIcon1.ShowBalloonTip((int)TimeSpan.FromMinutes(1).TotalMilliseconds, "Stay Alive disabled", notifyMessage, ToolTipIcon.Warning);
        }

        public bool IsOnPower()
        {
            var powerStatus = SystemInformation.PowerStatus;
            return powerStatus.PowerLineStatus == PowerLineStatus.Online;
        }

        private void TrayForm_Load(object sender, EventArgs e)
        {
            _service = new Service(this,  new OsFunctions());
            _service.Start(Properties.Settings.Default.TimeOutMinutes, 1,  Properties.Settings.Default.DisableWhenNotCharging);
        }

        private void exitToolStripMenuItem_MouseUp(object sender, MouseEventArgs e)
		{
			Close();
		}

        private void TrayForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _service?.Dispose();
            notifyIcon1.Visible = false;
            notifyIcon1.Dispose();
            notifyIcon1 = null;
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var settings = new Settings())
            {
                settings.ShowDialog(this);
                if (settings.DialogResult == DialogResult.OK)
                {
                    _service.SetTimeout(Properties.Settings.Default.TimeOutMinutes);
                    _service.SetDisableWhenNotOnPower(Properties.Settings.Default.DisableWhenNotCharging);
                }

            }
        }
    }
}
