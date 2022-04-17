using CD.DLS.Common.Tools;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Managers;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Diagnostics;
using System.IO;
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
        private string _sqlConnection;
        private string _databaseName;
        private bool _serviceInConsole = true;
        

        public bool ServiceInConsole { get => _serviceInConsole; set => _serviceInConsole = value; }
        
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

            ServiceInConsole = ConfigManager.ServiceRunsInConsole;
            if (ServiceInConsole)
            {
                RadioServiceRunsInConsole.IsChecked = true;
                RadioServiceRunsInWin.IsChecked = false;
            }
            else
            {
                RadioServiceRunsInConsole.IsChecked = false;
                RadioServiceRunsInWin.IsChecked = true;
            }
        }

        private void ConnectionStringBuilder_OnConnectionSuccessful(object sender, EventArgs e)
        {
            var connstring = connectionStringBuilder.ConnectionString.ToString();
            _databaseName = connectionStringBuilder.ConnectionString.Database;
            logViewer.Info("Connection successful");
            logViewer.Info(connstring);
            ConfigButton.IsEnabled = true;
            _sqlConnection = connstring;
            //ConfigButton.Foreground = Brushes.White;
            //var color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F18719"));
            //ConfigButton.Background = color;
            
            //string errors;
            
            
            //if (!ConfigureOnPremises(connstring, out errors))
            //{
            //    MessageBox.Show(errors, "Configuration failed", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            //else
            //{
            //    MessageBox.Show("Configuration finished successfully", "Configuration successful", MessageBoxButton.OK, MessageBoxImage.Information);
            //    Close();
            //}
            //throw new NotImplementedException();
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

        private bool ConfigureRegistry()
        {
            Log("Configuring registry...");
            string errorMessage;
            try
            {
                //var connstringWithoutDb = new SqlConnectionString(connectionString.ToString());
                //connstringWithoutDb.Database = string.Empty;
                //var setConnstring = new SqlConnectionString(connectionString.ToString());

                //// test connection
                //using (SqlConnection conn = new SqlConnection(connstringWithoutDb))
                //{
                //    try
                //    {
                //        conn.Open();
                //    }
                //    catch (Exception ex)
                //    {
                //        errorMessage = ex.Message + (ex.InnerException == null ? string.Empty : (Environment.NewLine + ex.InnerException.Message));
                //        //MessageBox.Show(ex.Message + (ex.InnerException == null ? string.Empty : (Environment.NewLine + ex.InnerException.Message)), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //        return false;
                //    }
                //}

                Registry.SetConfigValue6432(StandardConfigManager.KV_DLS_CUSTOMER_CONNECTION_STRING, _sqlConnection);

                NetBridge nb = new NetBridge(true);
                nb.SetConnectionString(_sqlConnection);
                ProjectConfigManager pcm = new ProjectConfigManager(nb);
                var svcReceiverId = pcm.GetServiceReceiverId();

                Registry.SetConfigValue6432(StandardConfigManager.DLS_SERVICERECEIVERID, svcReceiverId.ToString());
                Registry.SetConfigValue6432(StandardConfigManager.DLS_DEPLOYMENT_MODE, DeploymentModeEnum.OnPremises.ToString());
                Registry.SetConfigValue6432(StandardConfigManager.DLS_SERVICE_IN_CONSOLE, ServiceInConsole.ToString());

                ConfigureExtractorPath();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message + (ex.InnerException == null ? string.Empty : (Environment.NewLine + ex.InnerException.Message)); //+ Environment.NewLine + ex.StackTrace;
                Log(errorMessage);
                return false;
            }

            errorMessage = null;
            return true;
        }

        private void ConfigureExtractorPath()
        {
            var installDir = Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
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

        private bool ConfigureService()
        {
            Log("Configuring service...");
            try
            {
                if (ServiceInConsole)
                {
                    return true;
                }

                var installDir = Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                var serviceFileName = "DLS.Service.exe";
                var servicePath = Path.Combine(installDir, "service", serviceFileName);
                if (!File.Exists(servicePath))
                {
                    Log($"Service not found: {servicePath}!");
                    return false;
                }

                ProcessStartInfo si = new ProcessStartInfo(servicePath);
                si.Arguments = "-install";
                var process = Process.Start(si);
                process.WaitForExit();

                ServiceController service = new ServiceController("DLS");
                if ((service.Status.Equals(ServiceControllerStatus.Stopped)) ||
                    (service.Status.Equals(ServiceControllerStatus.StopPending)))
                {
                    Log("Starting service...");
                    service.Start();
                }
                else
                {
                    Log("Service already up and running");
                }
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message + (ex.InnerException == null ? string.Empty : (Environment.NewLine + ex.InnerException.Message)); //+ Environment.NewLine + ex.StackTrace;
                Log(errorMessage);
                return false;
            }

            return true;
        }

        private async void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            //DbDeploymentManager deploymentManager = new DbDeploymentManager(logViewer);
            //deploymentManager.ConnectionInfoMessage += DeploymentManager_ConnectionInfoMessage1;
            //try
            //{
            //    deploymentManager.CheckAppliedDbVersion(0);
            //    InitDb();
            //}
            //catch
            //{
            //    logViewer.Info("DB is already initialized");
            //}
            Task<bool> dbTask = new Task<bool>(ConfigureDb);
            Task<bool> registryTask = new Task<bool>(ConfigureRegistry);
            Task<bool> serviceTask = new Task<bool>(ConfigureService);


            dbTask.Start();
            await dbTask;
            var dbSuccess = dbTask.Result;
            if (!dbSuccess)
            {
                Log("DB initialization failed");
                return;
            }
            
            registryTask.Start();
            await registryTask;
            var registrySuccess = registryTask.Result;
            if (!registrySuccess)
            {
                Log("Registry setup failed");
                return;
            }

            serviceTask.Start();
            await serviceTask;
            var serviceSuccess = registryTask.Result;
            if (!serviceSuccess)
            {
                Log("Service setup failed");
                return;
            }

            Log("Configuration finished successfully.");

            ConfigButton.Content = "Close";
            ConfigButton.Click -= ConfigButton_Click;
            ConfigButton.Click += CloseButton_Click1;
        }

        private void CloseButton_Click1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool ConfigureDb()
        {

            DbDeploymentManager deploymentManager = new DbDeploymentManager(logViewer);
            deploymentManager.ConnectionInfoMessage += DeploymentManager_ConnectionInfoMessage1;
            try
            {
                deploymentManager.CheckAppliedDbVersion(0);
                InitDb();
            }
            catch
            {
                Log("DB is already initialized");
            }
            return true;
        }


        private void InitDb()
        {
            var binLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var sqlPath = Path.Combine(binLocation, "DeployScripts", "DLS.publish_1.sql");
            var sqlTemplate = File.ReadAllText(sqlPath);
            var sql = sqlTemplate.Replace("$(DatabaseName)", _databaseName);
            Log($"Running deploy script on {_databaseName}");
            using (Microsoft.Data.SqlClient.SqlConnection conn = new Microsoft.Data.SqlClient.SqlConnection(_sqlConnection))
            {
                Microsoft.SqlServer.Management.Common.ServerConnection svrConnection = new Microsoft.SqlServer.Management.Common.ServerConnection(conn);
                conn.Open();
                //svrConnection.ServerMessage += SvrConnection_ServerMessage;
                //svrConnection.InfoMessage += DeploymentManager_ConnectionInfoMessage;
                conn.InfoMessage += DeploymentManager_ConnectionInfoMessage;
                Server server = new Server(svrConnection);
                server.ConnectionContext.ExecuteNonQuery(sql);
                conn.Close();
            }

            //ServerConnection svrConnection = new ServerConnection(connection);
            //Server server = new Server(svrConnection);
            //server.ConnectionContext.ExecuteNonQuery(script);
        }

        //private void SvrConnection_ServerMessage(object sender, Microsoft.SqlServer.Management.Common.ServerMessageEventArgs e)
        //{
        //    logViewer.Info(e.Error.Message);
        //}

        private void DeploymentManager_ConnectionInfoMessage(object sender, Microsoft.Data.SqlClient.SqlInfoMessageEventArgs e)
        {
            Dispatcher.Invoke(() => logViewer.Info(e.Message), System.Windows.Threading.DispatcherPriority.Background);
        }
        private void DeploymentManager_ConnectionInfoMessage1(object sender, System.Data.SqlClient.SqlInfoMessageEventArgs e)
        {
            Dispatcher.Invoke(() => logViewer.Info(e.Message), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void Log(string log)
        {
            Dispatcher.Invoke(() => logViewer.Info(log), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void RadioServiceRunsInConsole_Checked(object sender, RoutedEventArgs e)
        {
            ServiceInConsole = true;
        }

        private void RadioServiceRunsInWin_Checked(object sender, RoutedEventArgs e)
        {
            ServiceInConsole = false;
        }
    }
}
