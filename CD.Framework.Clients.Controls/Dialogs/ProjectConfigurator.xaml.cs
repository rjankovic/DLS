using CD.DLS.Clients.Controls.Dialogs.SqlConnection;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.AnalysisServices;
using CD.DLS.Clients.Controls.Dialogs.PowerBiConnection;

namespace CD.DLS.Clients.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for ListPicker.xaml
    /// </summary>
    public partial class ProjectConfigurator : UserControl
    {
        public event EventHandler Submitted;
        public event EventHandler Cancelled;
        private ProjectConfig _config;

        public ProjectConfig Config { get { return _config; } }

        public ProjectConfigurator(ProjectConfig config)
        {
            InitializeComponent();

            _config = config;

            gridSqlDbs.DataContext = _config.DatabaseComponents;
            gridSqlDbs.ItemsSource = _config.DatabaseComponents;
            gridSsasDbs.DataContext = _config.SsasComponents;
            gridSsasDbs.ItemsSource = _config.SsasComponents;
            gridSsisProjects.DataContext = _config.SsisComponents;
            gridSsisProjects.ItemsSource = _config.SsisComponents;
            gridSsrsPrejects.DataContext = _config.SsrsComponents;
            gridSsrsPrejects.ItemsSource = _config.SsrsComponents;
            gridPowerBi.DataContext = _config.PowerBiComponents;
            gridPowerBi.ItemsSource = _config.PowerBiComponents;

            //gridAgentJobs.ItemsSource = _config.MssqlAgentComponents;
            //gridAgentJobs.DataContext = _config.MssqlAgentComponents;
        }

        public bool IsTabular(SsasDbProjectComponent currentProject)
        {
            var serverName = currentProject.ServerName;
            Microsoft.AnalysisServices.Server _server = new Microsoft.AnalysisServices.Server();
            _server.Connect(string.Format("Provider=MSOLAP.8;Integrated Security=SSPI;DataSource={0}", currentProject.ServerName));

            if (_server.ServerMode == Microsoft.AnalysisServices.ServerMode.Tabular)
            {
                return true;
            }

            else { return false; }

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void addSqlDbButton_Click(object sender, RoutedEventArgs e)
        {
            SqlConnectionWindow window = new SqlConnectionWindow();
            var res = window.ShowDialog();
            if (res.HasValue && res.Value)
            {
                var connection = window.Connection;
                _config.DatabaseComponents.Add(new MssqlDbProjectComponent()
                {
                    DbName = connection.Database, ServerName = connection.Server, ProjectConfig = _config, Username = connection.UserName, Password = connection.Password
                });
                gridSqlDbs.Items.Refresh();
            }
        }

        private void editSqlDbButton_Click(object sender, RoutedEventArgs e)
        {
            var item = gridSqlDbs.SelectedItem as MssqlDbProjectComponent;
            if (item == null)
            {
                return;
            }

            SqlConnectionWindow window = new SqlConnectionWindow(item.ServerName, item.DbName);
            var res = window.ShowDialog();
            if (res.HasValue && res.Value)
            {
                var connection = window.Connection;
                item.DbName = connection.Database;
                item.ServerName = connection.Server;
                item.Username = connection.UserName;
                item.Password = connection.Password;
                gridSqlDbs.Items.Refresh();
            }
        }

        private void removeSqlDbButton_Click(object sender, RoutedEventArgs e)
        {
            var item = gridSqlDbs.SelectedItem as MssqlDbProjectComponent;
            if (item == null)
            {
                return;
            }
            _config.DatabaseComponents.Remove(item);
            gridSqlDbs.Items.Refresh();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Submitted?.Invoke(this, new EventArgs());
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancelled?.Invoke(this, new EventArgs());
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
        
        private void addSsisButton_Click(object sender, RoutedEventArgs e)
        {
            SsisConnection.SSISConnectionWindow window = new SsisConnection.SSISConnectionWindow();
            var res = window.ShowDialog();
            if (res.HasValue && res.Value)
            {
                var connection = window.Project;
                _config.SsisComponents.Add(new SsisProjectComponent() { ProjectConfig = _config, FolderName = connection.Folder, ProjectName = connection.Project, ServerName = connection.Server });
                gridSsisProjects.Items.Refresh();
            }
        }

        private void editSsisButton_Click(object sender, RoutedEventArgs e)
        {
            var item = gridSsisProjects.SelectedItem as SsisProjectComponent;
            if (item == null)
            {
                return;
            }

            var defaultProject = new SsisConnection.SsisProject() { Folder = item.FolderName, Project = item.ProjectName, Server = item.ServerName };
            SsisConnection.SSISConnectionWindow window = new SsisConnection.SSISConnectionWindow(defaultProject);
            var res = window.ShowDialog();
            if (res.HasValue && res.Value)
            {
                var connection = window.Project;
                item.ServerName = connection.Server;
                item.FolderName = connection.Folder;
                item.ProjectName = connection.Project;
                gridSsisProjects.Items.Refresh();
            }
        }

        private void removeSsisButton_Click(object sender, RoutedEventArgs e)
        {
            var item = gridSsisProjects.SelectedItem as SsisProjectComponent;
            if (item == null)
            {
                return;
            }
            _config.SsisComponents.Remove(item);
            gridSsisProjects.Items.Refresh();
        }

        private void removeSsasButton_Click(object sender, RoutedEventArgs e)
        {
            var item = gridSsasDbs.SelectedItem as SsasDbProjectComponent;
            if (item == null)
            {
                return;
            }
            _config.SsasComponents.Remove(item);
            gridSsasDbs.Items.Refresh();
        }

        private void editSsasButton_Click(object sender, RoutedEventArgs e)
        {
            var item = gridSsasDbs.SelectedItem as SsasDbProjectComponent;
            if (item == null)
            {
                return;
            }
            SsasConnection.SsasConnectionWindow window = new SsasConnection.SsasConnectionWindow(new SsasConnection.SsasDatabase() { Server = item.ServerName, Database = item.DbName });
            var res = window.ShowDialog();
            if (res.HasValue && res.Value)
            {
                var connection = window.Database;
                item.ServerName = connection.Server;
                item.DbName = connection.Database;
                gridSsasDbs.Items.Refresh();
            }
        }

        private void addSsasButton_Click(object sender, RoutedEventArgs e)
        {
            SsasConnection.SsasConnectionWindow window = new SsasConnection.SsasConnectionWindow();
            var res = window.ShowDialog();
            if (res.HasValue && res.Value)
            {
                var connection = window.Database;
                SsasDbProjectComponent newSSAS = new SsasDbProjectComponent() { ProjectConfig = _config, ServerName = connection.Server, DbName = connection.Database };

                if (IsTabular(newSSAS)){
                    newSSAS.Type = SsasTypeEnum.Tabular;
                }
                else
                {
                    newSSAS.Type = SsasTypeEnum.Multidimensional;
                }
                _config.SsasComponents.Add(newSSAS);
                gridSsasDbs.Items.Refresh();
            }
        }

        private void addSsrsButton_Click(object sender, RoutedEventArgs e)
        {
            SsrsConnection.SsrsConnectionWindow window = new SsrsConnection.SsrsConnectionWindow();
            var res = window.ShowDialog();
            if (res.HasValue && res.Value)
            {
                var connection = window.Project;
                var newItem = new SsrsProjectComponent()
                {
                    ProjectConfig = _config,
                    ServerName = connection.ServerName,
                    SsrsExecutionServiceUrl = connection.ExecutionServiceUrl,
                    SsrsServiceUrl = connection.ReportServiceUrl,
                    FolderPath = connection.RootFolder,
                    SsrsMode = connection.SsrsMode,
                    SharePointBaseUrl = connection.SharePointBaseUrl,
                    SharePointFolder = connection.SharePointFolder
                };
                if (newItem.SsrsMode == SsrsModeEnum.Native)
                {
                    newItem.FolderPath = "/" + newItem.FolderPath.TrimStart('/');
                }

                _config.SsrsComponents.Add(newItem);
                gridSsrsPrejects.Items.Refresh();
            }
        }

        private void editSsrsButton_Click(object sender, RoutedEventArgs e)
        {
            var item = gridSsrsPrejects.SelectedItem as SsrsProjectComponent;
            if (item == null)
            {
                return;
            }

            SsrsConnection.SsrsProject project = null;
            if (item.SsrsMode == SsrsModeEnum.Native)
            {
                project = SsrsConnection.SsrsProject.CreateNativeMode(item.SsrsServiceUrl, item.FolderPath);
            }
            else if (item.SsrsMode == SsrsModeEnum.SpIntegrated)
            {
                project = SsrsConnection.SsrsProject.CreateIntegratedMode(item.SharePointBaseUrl, item.SharePointFolder);
            }
            else
            {
                throw new Exception();
            }
            SsrsConnection.SsrsConnectionWindow window = new SsrsConnection.SsrsConnectionWindow(project);
            var res = window.ShowDialog();
            if (res.HasValue && res.Value)
            {
                var connection = window.Project;
                item.SsrsMode = connection.SsrsMode;
                item.FolderPath = connection.RootFolder;
                item.ServerName = connection.ServerName;
                item.SsrsServiceUrl = connection.ReportServiceUrl;
                item.SsrsExecutionServiceUrl = connection.ExecutionServiceUrl;
                item.SharePointBaseUrl = connection.SharePointBaseUrl;
                item.SharePointFolder = connection.SharePointFolder;
                //if (item.SsrsMode == SsrsModeEnum.Native)
                //{
                    item.FolderPath = "/" + item.FolderPath.TrimStart('/');
                //}

                gridSsrsPrejects.Items.Refresh();
            }
        }

        private void removeSsrsButton_Click(object sender, RoutedEventArgs e)
        {
            var item = gridSsrsPrejects.SelectedItem as SsrsProjectComponent;
            if (item == null)
            {
                return;
            }
            _config.SsrsComponents.Remove(item);
            gridSsrsPrejects.Items.Refresh();
        }

        private void addPowerBiButton_Click(object sender, RoutedEventArgs e)
        {                   
            PowerBiConnectionWindow window = new PowerBiConnectionWindow();
            var res = window.ShowDialog();
            if (res.HasValue && res.Value)
            {
                var connection = window.Project;
                PowerBiProjectComponent newPBI = new PowerBiProjectComponent()
                {
                    ApplicationID = connection.ApplicationID,
                    RedirectUri = connection.RedirectUri,
                    WorkspaceID = connection.WorkspaceID,
                    UserName = connection.UserName,
                    Password = connection.Password,
                    Tenant = connection.ApplicationID,
                    ReportServerURL = connection.ReportServerURL,
                    ReportServerFolder = connection.ReportServerFolder
                    

                };
                                              
                _config.PowerBiComponents.Add(newPBI);
                
                gridPowerBi.Items.Refresh();                
            }          
        }
        private void editPowerBiButton_Click(object sender, RoutedEventArgs e)
        {
            var item = gridPowerBi.SelectedItem as PowerBiProjectComponent;
            if (item == null)
            {
                return;
            }
            PowerBiProject project = new PowerBiProject();
            project.ApplicationID = item.ApplicationID;
            project.RedirectUri = item.RedirectUri;
            project.WorkspaceID = item.WorkspaceID;
            project.UserName = item.UserName;
            project.Password = item.Password;
            project.Tenant = item.ApplicationID;
            project.ReportServerURL = item.ReportServerURL;
            project.ReportServerFolder = item.ReportServerFolder;
            PowerBiConnectionWindow window = new PowerBiConnectionWindow(project);
            var res = window.ShowDialog();
            if(res.HasValue && res.Value)
            {
                var connection = window.Project;
                item.ApplicationID = connection.ApplicationID;
                item.RedirectUri = connection.RedirectUri;
                item.WorkspaceID = connection.WorkspaceID;
                item.UserName = connection.UserName;
                item.Password = connection.Password;
                item.Tenant = connection.ApplicationID;
                item.ReportServerURL = connection.ReportServerURL;
                item.ReportServerFolder = connection.ReportServerFolder;
                             
                gridPowerBi.Items.Refresh();
            }           
        }
        private void removePowerBiButton_Click(object sender, RoutedEventArgs e)
        {
            var item = gridPowerBi.SelectedItem as PowerBiProjectComponent;
            if (item == null)
            {
                return;
            }
           
            _config.PowerBiComponents.Remove(item);
            gridPowerBi.Items.Refresh();
        }
    }
}
