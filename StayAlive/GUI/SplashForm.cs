using System.Windows.Forms;

namespace StayAlive.GUI
{
	public partial class SplashForm : Form
	{
		public SplashForm()
		{
			InitializeComponent();
		}

        private void SplashForm_Load(object sender, System.EventArgs e)
        {
            labelTimeOutMinutes.Text = @"Prevents computer from locking down within a specified amount of time";
		}
    }
}
