using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CD.DLS.Configuration
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
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
    }
}
