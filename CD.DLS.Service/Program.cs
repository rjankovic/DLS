using CD.DLS.DAL.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Service
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ConfigManager.ApplicationClass = ApplicationClassEnum.Service;
            ConfigManager.DeploymentMode = DeploymentModeEnum.OnPremises;

            bool runInWindow = ConfigManager.ServiceRunsInConsole;
            if (runInWindow)
            {
                DLS service = new DLS();
                ConfigManager.Log.Important("Starting in window");
                try
                {
                    service.StartConsole();
                }
                catch (AggregateException agge)
                {
                    foreach (var ex in agge.InnerExceptions)
                    {
                        ConfigManager.Log.Important($"Error:\n{ex.Message}\n{ex.StackTrace}");
                        Console.ReadKey();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ConfigManager.Log.Important($"Error:\n{ex.Message}\n{ex.StackTrace}");
                    Console.ReadKey();
                    return;
                }
            }
            else
            {
                try
                {
                    var handle = GetConsoleWindow();
                    ShowWindow(handle, SW_HIDE);

                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                new DLS()
                    };
                    ServiceBase.Run(ServicesToRun);
                }
                catch (Exception ex)
                {
                    ConfigManager.Log.Error(ex.Message + (ex.InnerException == null ? string.Empty : (Environment.NewLine + ex.InnerException.Message)));
                }
            }

        }
    }
}
