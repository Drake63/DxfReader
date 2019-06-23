using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using System;
using System.Globalization;
using System.Threading;

namespace DxfOpener
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int showWindowCommand);
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //CultureInfo ci = new CultureInfo("en-US");
            //Thread.CurrentThread.CurrentCulture = ci;
            //Thread.CurrentThread.CurrentUICulture = ci;

            if (e.Args.Length != 0)
                Current.Properties["args"] = e.Args[0];
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            bool isOneWindow = false;
            var userConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);

            if (File.Exists(userConfig.FilePath))
            {
                XDocument doc = XDocument.Load(userConfig.FilePath);

                var isOne = (from setting in doc.Descendants("setting")
                             where (string)setting.Attribute("name") == "onlyOneWindow"
                             select setting.Value).ToArray();

                if (isOne.Length > 0)
                    bool.TryParse(isOne[0], out isOneWindow);
            }

            if (isOneWindow)
            {
                Process currentProc = Process.GetCurrentProcess();

                var processes = (from p in Process.GetProcesses()
                                 where p.ProcessName == currentProc.ProcessName && p.Id != currentProc.Id
                                 orderby p.StartTime descending
                                 select p).ToArray();

                if (processes.Length > 0 && e.Args.Length > 0)
                {
                    WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
                    WindowPlacement.GetPlacement(processes[0].MainWindowHandle, out wp);
                    ShowWindow(processes[0].MainWindowHandle, wp.showCmd);
                    SetForegroundWindow(processes[0].MainWindowHandle);

                    SendMessageHelper.SendWindowStringMessage(processes[0].MainWindowHandle, e.Args[0]);                    

                    Shutdown();
                }
            }
            base.OnStartup(e);
        }
    }
}
