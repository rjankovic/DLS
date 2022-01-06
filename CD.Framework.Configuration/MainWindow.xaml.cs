using CD.DLS.Clients.Controls.Dialogs.SqlConnection;
using CD.DLS.Common.Tools;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Identity;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Misc;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Configuration.Install;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows;

namespace CD.DLS.Configuration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _preconfiguredConnectionString;
        private SqlConnectionString _setConnectionString;
        public MainWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //throw new Exception("AAAA");
            InitializeComponent();

            _preconfiguredConnectionString = Registry.GetConfigValue(StandardConfigManager.KV_DLS_CUSTOMER_CONNECTION_STRING);

            if (_preconfiguredConnectionString != null)
            {
                connectionStringBuilder.ConnectionString = new Clients.Controls.Dialogs.SqlConnection.SqlConnectionString(_preconfiguredConnectionString);
            }

            //connectionStringBuilder.PropertyChanged += ConnectionStringBuilder_PropertyChanged;
            connectionStringBuilder.ConnectText = "Connect";
            connectionStringBuilder.ShowConnectionSuccessfulMessage = false;
            connectionStringBuilder.OnConnectionSuccessful += ConnectionStringBuilder_OnConnectionSuccessful;
        }

        private void ConnectionStringBuilder_OnConnectionSuccessful(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = ((Exception)e.ExceptionObject);
            MessageBox.Show(ex.Message + Environment.NewLine + Environment.NewLine + ((ex.InnerException == null) ? string.Empty : (ex.InnerException.Message + Environment.NewLine)) /* + ex.StackTrace */, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }

        //private async void ConfigureButton_Click(object sender, RoutedEventArgs e)
        //{
        //    var css = connectionStringBuilder.ConnectionString.ToString();
        //    configureButton.IsEnabled = false;
        //    string errorMessage = string.Empty;
        //    bool success = false;

        //    if (onPremisesDeploymentRadio.IsChecked.Value)
        //    {
        //        success = await Task.Run(() => ConfigureOnPremises(css, out errorMessage));
        //    }
        //    else
        //    {
        //        success = await Task.Run(() => ConfigureAzure(out errorMessage));
        //    }

        //    configureButton.IsEnabled = true;
        //    if (success)
        //    {
        //        MessageBox.Show("Configuration finished successfully", "Configuration Successful", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        //        this.Close();
        //    }
        //    else
        //    {
        //        if (errorMessage != null)
        //        {
        //            MessageBox.Show("Configuration failed:  " + Environment.NewLine + errorMessage, "Configuration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    }
        //}

        private bool ConfigureOnPremises(string connectionString, out string errorMessage)
        {
            try
            {
                var connstringWithoutDb = new SqlConnectionString(connectionString.ToString());
                connstringWithoutDb.Database = string.Empty;
                var setConnstring = new SqlConnectionString(connectionString.ToString());

                // test connection
                using (SqlConnection conn = new SqlConnection(connstringWithoutDb))
                {
                    try
                    {
                        conn.Open();
                    }
                    catch (Exception ex)
                    {
                        errorMessage = ex.Message + (ex.InnerException == null ? string.Empty : (Environment.NewLine + ex.InnerException.Message));
                        //MessageBox.Show(ex.Message + (ex.InnerException == null ? string.Empty : (Environment.NewLine + ex.InnerException.Message)), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

                Registry.SetConfigValue6432(StandardConfigManager.KV_DLS_CUSTOMER_CONNECTION_STRING, connectionString);

                NetBridge nb = new NetBridge(true);
                nb.SetConnectionString(connectionString);
                ProjectConfigManager pcm = new ProjectConfigManager(nb);
                var svcReceiverId = pcm.GetServiceReceiverId();

                Registry.SetConfigValue6432(StandardConfigManager.DLS_SERVICERECEIVERID, svcReceiverId.ToString());
                Registry.SetConfigValue6432(StandardConfigManager.DLS_DEPLOYMENT_MODE, DeploymentModeEnum.OnPremises.ToString());

                ConfigureExtractorPath();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message + (ex.InnerException == null ? string.Empty : (Environment.NewLine + ex.InnerException.Message)) + Environment.NewLine + ex.StackTrace;
                return false;
            }

            errorMessage = null;
            return true;
        }

        private void ConfigureExtractorPath()
        {
            var installDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var extractorFileName = "CD.DLS.Extract.exe";
            var expectedExtractorPath = Path.Combine(installDir, "extractor", extractorFileName);
            if (File.Exists(expectedExtractorPath))
            {
                Registry.SetConfigValue6432(StandardConfigManager.DLS_EXTRACTOR_PATH, expectedExtractorPath);
                return;
            }

            MessageBox.Show("The extractor executable (CD.DLS.Extract.exe) could not be found in " + expectedExtractorPath + ". Please locate it manually", "Extractor not found", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "Executable files (*.exe)|*.exe";
            ofd.FilterIndex = 1;

            var result = ofd.ShowDialog();
            if (result.HasValue)
            {
                if (result.Value)
                {
                    Registry.SetConfigValue6432(StandardConfigManager.DLS_EXTRACTOR_PATH, ofd.FileName);
                }
            }
        }


    }
}
