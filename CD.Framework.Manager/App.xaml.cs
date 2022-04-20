using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Identity;
using CD.DLS.DAL.Managers;
using Fluent;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;


namespace CD.DLS.Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private WindowsIdentity _windowsIdentity;
        private Process _serviceProcess = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            //// add custom accent and theme resource dictionaries to the ThemeManager
            //// you should replace FluentRibbonThemesSample with your application name
            //// and correct place where your custom accent lives
            //ThemeManager.AddAccent("RibbonTheme",
            //    //new Uri("pack://application:,,,/CD.DLS.Manager;component/Resources/RibbonTheme.xaml")
            //    new Uri("Resources/RibbonTheme.xaml", UriKind.Relative)
            //    );

            //// get the current app style (theme and accent) from the application
            //Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(Application.Current);

            //var accent = ThemeManager.GetAccent("RibbonTheme");

            //// now change app style to the custom accent and current theme
            //ThemeManager.ChangeAppStyle(Application.Current,
            //                            accent,
            //                            theme.Item1);

            //var x = MessageBox.Show("A");

            this.DispatcherUnhandledException += Application_DispatcherUnhandledException;

            base.OnStartup(e);
            ConfigManager.ApplicationClass = ApplicationClassEnum.Client;

            
     //       AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
     //ShowUnhandledException(args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException");


            IdentityProvider.Login();
            if (IdentityProvider.GetCurrentUser() == null)
            {
                this.Shutdown();
            }

            StartServiceIfNeeded();


            
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            var messageBoxTitle = $"Error";
            var msg = e.Exception.Message + Environment.NewLine;
            if (e.Exception.InnerException != null)
            {
                msg += e.Exception.InnerException.Message + Environment.NewLine;
            }
            msg += e.Exception.StackTrace;
            //var messageBoxButtons = MessageBoxButton.OK;
            var res = MessageBox.Show(msg, messageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
            this.Shutdown();

        }


        //void ShowUnhandledException(Exception e, string unhandledExceptionType)
        //{
        //    var messageBoxTitle = $"Unexpected Error Occurred: {unhandledExceptionType}";
        //    var msg = e.Message + Environment.NewLine;
        //    if (e.InnerException != null)
        //    {
        //        msg += e.InnerException.Message + Environment.NewLine;
        //    }
        //    msg += e.StackTrace;
        //    var messageBoxButtons = MessageBoxButton.OK;


        //    // Let the user decide if the app should die or not (if applicable).
        //    MessageBox.Show(msg, messageBoxTitle, messageBoxButtons);
        //}

        //private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        //{
        //    e.Handled = true;
        //    var msg = e.Exception.Message + Environment.NewLine;
        //    if (e.Exception.InnerException != null)
        //    {
        //        msg += e.Exception.InnerException.Message + Environment.NewLine;
        //    }
        //    msg += e.Exception.StackTrace;

        //    //var res = MessageBox.Show(msg, "Error", MessageBoxButton.OK);
        //    Dispatcher.Invoke(() => MessageBox.Show(msg, "Error", MessageBoxButton.OK), System.Windows.Threading.DispatcherPriority.Send);
        //}

        private void StartServiceIfNeeded()
        {
            var serviceInConsole = ConfigManager.ServiceRunsInConsole;
            if (!serviceInConsole)
            {
                return;
            }

            var runningProcessByName = Process.GetProcessesByName("DLS.Service");
            if (runningProcessByName.Any())
            {
                return;
            }


            var clientFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var svcFile = Path.Combine(Path.GetDirectoryName(clientFolder), "service", "DLS.Service.exe");

            if (!File.Exists(svcFile))
            {
                var err = $"Service EXE not found: {svcFile}!";
                MessageBox.Show(err, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new FileNotFoundException(err);
            }
            ProcessStartInfo si = new ProcessStartInfo(svcFile);
            //si.RedirectStandardInput = true;
            //si.RedirectStandardOutput = true;
            //si.UseShellExecute = false;
            si.WindowStyle = ProcessWindowStyle.Hidden;
            _serviceProcess = Process.Start(si);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_serviceProcess == null)
            {
                return;
            }


            //_serviceProcess.StandardInput.Write('\u001b');


            //_serviceProcess.WaitForExit();

            if (_serviceProcess.HasExited == false)
            {
                _serviceProcess.Kill();
            }
            //var output = _serviceProcess.StandardOutput.ReadToEnd();


        }
    }
}
