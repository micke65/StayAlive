using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using StayAlive.GUI;
using Timer = System.Windows.Forms.Timer;

namespace StayAlive
{
	static class Program
	{
		static SplashForm _splashScreen;
        // ReSharper disable once NotAccessedField.Local
        private static Mutex _mutex;

		[STAThread]
		static void Main()
		{
            const string appName = "StayAlive";
            _mutex = new Mutex(true, appName, out var createdNew);
            if (!createdNew)
            {
                return;
            }

			var processName = Process.GetCurrentProcess().ProcessName;
			if(Process.GetProcessesByName(processName).Length > 1)
			{
				Application.ExitThread();
				return;
			}

            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			_splashScreen = new SplashForm();
			_splashScreen.Show();

			var main = new TrayForm();
			main.Shown += (sender, eventArgs) =>
			{
				var timer = new Timer { Interval = 5000 };
				timer.Tick += (o, args) =>
                {
                    _splashScreen.Close();
					timer.Stop();
                };
				timer.Start();
			};
			Application.Run(main);
		}

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
	}
}
