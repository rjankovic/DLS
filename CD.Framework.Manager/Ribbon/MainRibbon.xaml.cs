
using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.API.Test;
using CD.DLS.Clients.Controls.Dialogs;
using CD.DLS.Clients.Controls.Dialogs.BusinessDictionaryAdmin;
using CD.DLS.Clients.Controls.Dialogs.CentricGraphBrowser;
using CD.DLS.Clients.Controls.Dialogs.Misc;
using CD.DLS.Clients.Controls.Dialogs.Overview;
using CD.DLS.Clients.Controls.Dialogs.Search;
using CD.DLS.Clients.Controls.Dialogs.Security;
using CD.DLS.Clients.Controls.Dialogs.SourceTargetSelector;
using CD.DLS.Clients.Controls.Dialogs.TreeFilterSelector;
using CD.DLS.Common.Structures;
using CD.DLS.Common.Tools;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.ExtractOperations;
using CD.DLS.DAL.Identity;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Receiver;
using CD.DLS.DAL.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock.Layout;
//using static CD.DLS.DAL.Managers.SecurityManager;

namespace CD.DLS.Manager
{
    /// <summary>
    /// Represents the main window of the application
    /// </summary>
    public partial class MainRibbon
    {
        private const string GLOBAL_ADMIN_AAD_GROUP = "GlobalAdmin";

        private ProjectConfig _projectConfig = null;
        public ProjectConfig ProjectConfig { get { return _projectConfig; } }
        private IReceiver _receiver;
        private Guid _serviceReceiverId;
        private string titleWindow;
        private ServiceHelper _serviceHelper;
        
        private string[] zips;
        private System.Windows.Threading.DispatcherTimer _uploadTimer;

        private ProjectConfigManager _projectConfigManager;
        public ProjectConfigManager ProjectConfigManager
        {
            get { return _projectConfigManager; }
        }

        private SecurityManager _securityManager;
        private RequestManager _requestManager;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainRibbon()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Uploader.UploadProgress += UploadProgress_Uploaded;
            this.InitializeComponent();

            if (IdentityProvider.GetCurrentUser() == null)
            {
                return;
            }

            _projectConfigManager = new ProjectConfigManager();
            _securityManager = new SecurityManager();
            _requestManager = new RequestManager();

            OpenProjectButton_Click(this, new RoutedEventArgs());

            ResourceDictionary myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("Resources/DefaultResourceDictionary.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries.Add(myResourceDictionary);
            var receiverIdConfigValue = ConfigManager.ServiceReceiverId;
            _serviceReceiverId = receiverIdConfigValue;

            if (ConfigManager.DeploymentMode == DeploymentModeEnum.Azure)
            {
                _receiver = new HttpReceiver(ConfigManager.CustomerCode);
            }
            else if (ConfigManager.DeploymentMode == DeploymentModeEnum.OnPremises)
            {
                _receiver = new Receiver(Guid.NewGuid(), "Desktop app - " + IdentityProvider.GetCurrentUser().Identity);
            }
            //_receiver.BroadcastMessageReceived += BroadcastMessageReceived;
            // global permissions
            SetVisibilityBasedOnPermissions();
        }

        //private void BroadcastMessageReceived(BroadcastMessage message)
        //{
        //    if (_projectConfig == null)
        //    {
        //        return;
        //    }

        //    if (_projectConfig.ProjectConfigId != message.ProjectConfigId)
        //    {
        //        return;
        //    }

        //    if (message.Type == BroadcastMessageType.ProjectUpdateFinished)
        //    {
        //        ModelUpdateFinished();
        //    }
        //    else if (message.Type == BroadcastMessageType.ProjectUpdateStarted)
        //    {
        //        ModelUpdateStarted();
        //    }
        //}

        private void ModelUpdateStarted()
        {
            MessageBox.Show("The service has started to update the project model. The model will be unavailable during this time.", "Model update", MessageBoxButton.OK, MessageBoxImage.Warning);
            Dispatcher.Invoke(MakeProjectButtonsUnavailable);
            
        }

        private void ModelUpdateFinished()
        {
            MessageBox.Show("The service has finished updating the model. The updated version is available now.", "Model update finished", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            Dispatcher.Invoke(MakeProjectButtonsAvailable);

        }

        private void SetVisibilityBasedOnPermissions()
        {
            if(ConfigManager.DeploymentMode == DeploymentModeEnum.OnPremises)
            {
                return;
            }
            var user = IdentityProvider.GetCurrentUser();
            var isGlobalAdmin = user.Groups.Any(x => x.Name == GLOBAL_ADMIN_AAD_GROUP);

            if (isGlobalAdmin)
            {
                ConfigureProjectButton.Visibility = Visibility.Visible;
                AddProjectButton.Visibility = Visibility.Visible;
                RemoveProjectButton.Visibility = Visibility.Visible;

                //debugTab.Visibility = Visibility.Visible;
                RibbonAdminTab.Visibility = Visibility.Visible;

                BrowseLineageGroupBox.Visibility = Visibility.Visible;
                ManageLineageGroupBox.Visibility = Visibility.Visible;
                UserPermissionButton.Visibility = Visibility.Visible;

                AdminDictionaryViewFieldsButton.Visibility = Visibility.Visible;
                AdminDictionaryFieldsButton.Visibility = Visibility.Visible;
                AdminBusinessLinksTypeButton.Visibility = Visibility.Visible;
                return;
            }

            var permissions = _securityManager.UserPermissions(_projectConfig?.ProjectConfigId ?? (Guid?)null);

            HashSet<PermissionEnum> permissionHash = new HashSet<PermissionEnum>(permissions.Select(x => x.Type).Distinct());

            //RibbonProjectTab.Visibility = permissionHash.Contains(PermissionEnum.ManageProject) ? Visibility.Visible : Visibility.Hidden;

            ConfigureProjectButton.Visibility = permissionHash.Contains(PermissionEnum.ManageProject) ? Visibility.Visible : Visibility.Hidden;
            AddProjectButton.Visibility = ConfigureProjectButton.Visibility;
            RemoveProjectButton.Visibility = ConfigureProjectButton.Visibility;
            
            //debugTab.Visibility = permissionHash.Contains(PermissionEnum.ManageProject) ? Visibility.Visible : Visibility.Hidden;
            RibbonAdminTab.Visibility = (permissionHash.Contains(PermissionEnum.ManageProject) || permissionHash.Contains(PermissionEnum.EditPermissions)) ? Visibility.Visible : Visibility.Hidden;

            BrowseLineageGroupBox.Visibility = permissionHash.Contains(PermissionEnum.ViewLineage) ? Visibility.Visible : Visibility.Hidden;
            ManageLineageGroupBox.Visibility = permissionHash.Contains(PermissionEnum.UpdateLineage) ? Visibility.Visible : Visibility.Hidden;
            UserPermissionButton.Visibility = permissionHash.Contains(PermissionEnum.EditPermissions) ? Visibility.Visible : Visibility.Hidden;
            AdminDictionaryViewFieldsButton.Visibility = permissionHash.Contains(PermissionEnum.EditAnnotations) ? Visibility.Visible : Visibility.Hidden;
            AdminDictionaryFieldsButton.Visibility = permissionHash.Contains(PermissionEnum.EditAnnotations) ? Visibility.Visible : Visibility.Hidden;
            AdminBusinessLinksTypeButton.Visibility = permissionHash.Contains(PermissionEnum.EditAnnotations) ? Visibility.Visible : Visibility.Hidden;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = ((Exception)e.ExceptionObject);
            var err = ex.Message + Environment.NewLine + Environment.NewLine + ((ex.InnerException == null) ? string.Empty : (ex.InnerException.Message + Environment.NewLine));
            ConfigManager.Log.Error(err);

            // not a real user action, just a RIP record
            ConfigManager.Log.LogUserAction(Common.Interfaces.UserActionEventType.Error, null, null, null);

            ConfigManager.Log.FlushMessages();
            MessageBox.Show(err, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
        
        private void OpenProjectButton_Click(object sender, RoutedEventArgs e)
        {
            List<ProjectConfig> configs = ProjectConfigManager.ListProjectConfigs();
            ListPicker lp = new ListPicker();
            lp.Init(new List<ListPicker.ListPickerItem>(configs.Select(x => new ListPicker.ListPickerItem { Name = x.Name, Value = x.ProjectConfigId.ToString() })));
            var pane = OpenPane(lp, "Select project");

            
            lp.Selected += (s, e1) =>
            {
                _projectConfig = configs.First(x => x.ProjectConfigId == Guid.Parse(lp.SelectedItem.Value));
                pane.Close();

                MakeProjectButtonsAvailable();
                titleWindow = lp.SelectedItem.Name;
                RibbonWindow.Title = titleWindow;
                
                _serviceHelper = new ServiceHelper(_receiver, _serviceReceiverId, _projectConfig);

                // global + project permissions
                SetVisibilityBasedOnPermissions();

                //TreeFilterSelector tfs = new TreeFilterSelector(_projectConfig, _receiver);
                //var selectorPane = OpenPane(tfs, "Selector");

                CheckForModelUpdate();

                //EmptyRequest emptyRequest = new EmptyRequest();
                //_serviceHelper.PostRequest(emptyRequest);

            };
            
        }

        private void MakeProjectButtonsAvailable()
        {
            ConfigureProjectButton.IsEnabled = true;
            UpdateLineageButton.IsEnabled = true;
            ExploreLineageButton.IsEnabled = true;
            ExternalSourcesButton.IsEnabled = true;
            LineageOverviewButton.IsEnabled = true;
            LineageSearchButton.IsEnabled = true;
            FindWarningsButton.IsEnabled = true;
            RibbonLineageTab.IsSelected = true;
            //EmptyRequestButton.IsEnabled = true;
            UserPermissionButton.IsEnabled = true;
            UserSyncButton.IsEnabled = true;
            AdminDictionaryViewFieldsButton.IsEnabled = true;
            AdminDictionaryFieldsButton.IsEnabled = true;
            AdminBusinessLinksTypeButton.IsEnabled = true;
            //SendSerializedMessage.IsEnabled = true;
        }

        private void MakeProjectButtonsUnavailable()
        {
            ConfigureProjectButton.IsEnabled = false;
            UpdateLineageButton.IsEnabled = false;
            ExploreLineageButton.IsEnabled = false;
            ExternalSourcesButton.IsEnabled = false;
            LineageOverviewButton.IsEnabled = false;
            LineageSearchButton.IsEnabled = false;
            FindWarningsButton.IsEnabled = false;
            //RibbonLineageTab.IsSelected = false;
            //EmptyRequestButton.IsEnabled = false;
            //SendSerializedMessage.IsEnabled = false;
        }

        private void CheckForModelUpdate()
        {
            var msgs = _requestManager.GetActiveBroadcastMessages();
            if (msgs.Any(x => x.Type == BroadcastMessageType.ProjectUpdateStarted && x.ProjectConfigId == _projectConfig.ProjectConfigId))
            {
                ModelUpdateStarted();
            }
        }

        private void ConfigureProjectButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectConfigurator configurator = new ProjectConfigurator(_projectConfig);
            var pane = OpenPane(configurator, "Configure project");
            configurator.Cancelled += (s, e2) =>
            {
                pane.Close();
            };
            configurator.Submitted += (s, e2) =>
            {
                ProjectConfigManager.SaveProjectConfig(configurator.Config, false);
                pane.Close();

                //var updateConfigRequest = CreateEmptyRequest();
                //updateConfigRequest.RequestForCoreType = Common.Interfaces.CoreTypeEnum.ManagementApi;
                //Common.Requests.ManagementMessage updateContent = new Common.Requests.UpdateConfigRequest();
                //updateConfigRequest.Content = updateContent.Serialize();
                //_receiver.PostMessageNoResponse(updateConfigRequest);
                //resHandle.Wait();
            };
        }

        private void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            List<ProjectConfig> configs = ProjectConfigManager.ListProjectConfigs();
            var usednames = configs.Select(x => x.Name).ToList();
            
            NameChooser nameChooser = new NameChooser(usednames);
            var pane = OpenPane(nameChooser, "New Project Name");


            nameChooser.Cancelled += (s, e2) =>
            {
                pane.Close();
            };

            nameChooser.Submitted += (s, e1) =>
            {
                pane.Close();
                var newConfig = new ProjectConfig() { Name = nameChooser.SelectedName, ProjectConfigId = Guid.NewGuid() };

                ProjectConfigManager.SaveProjectConfig(newConfig, true);
                ProjectConfigManager.CreateNewProjectSchemaAndTables(newConfig.ProjectConfigId);
                //ProjectConfigManager.CreateDataflowSequences();
                ProjectConfigManager.CreateProjectViews();
                //ReconfigureService();
            };
        }


        //private void ReconfigureService()
        //{
        //    var updateConfigRequest = CreateEmptyRequest();
        //    updateConfigRequest.MessageToProjectId = Guid.Empty;
        //    updateConfigRequest.RequestForCoreType = Common.Interfaces.CoreTypeEnum.ManagementApi;
        //    Common.Requests.ManagementMessage updateContent = new Common.Requests.ReconfigureRequest();
        //    updateConfigRequest.Content = updateContent.Serialize();
        //    _receiver.PostMessageNoResponse(updateConfigRequest);
        //}

        private void RemoveProjectButton_Click(object sender, RoutedEventArgs e)
        {
            
            List<ProjectConfig> configs = ProjectConfigManager.ListProjectConfigs();
            ListPicker lp = new ListPicker();
            lp.Init(new List<ListPicker.ListPickerItem>(configs.Select(x => new ListPicker.ListPickerItem { Name = x.Name, Value = x.ProjectConfigId.ToString() })));
            var pane = OpenPane(lp, "Delete project");

            lp.Selected += (s, e1) => {
                var projectConfig = configs.First(x => x.ProjectConfigId == Guid.Parse(lp.SelectedItem.Value));
                pane.Close();                             
                var msgRes = MessageBox.Show(string.Format("Are you sure you want to delete {0}?", projectConfig.Name), "Delete Project", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
                if (msgRes != MessageBoxResult.Yes)
                {    
                        return;
                }

                var origCursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = Cursors.Wait;
                
                ProjectConfigManager.DeleteProjectConfig(projectConfig.ProjectConfigId);
                //ReconfigureService();

                var credentialsFilePath = Path.Combine(System.IO.Path.GetDirectoryName(ConfigManager.ExtractorPath), "config", projectConfig.ProjectConfigId.ToString() + ".credentials");
                File.Delete(credentialsFilePath);

                Mouse.OverrideCursor = origCursor;

                if (_projectConfig != null)
                {
                    if (_projectConfig.ProjectConfigId == projectConfig.ProjectConfigId)
                    {
                        _projectConfig = null;
                        MakeProjectButtonsUnavailable();
                    }
                }
            };
        }

        private void ExploreLineageButton_Click(object sender, RoutedEventArgs e)
        {
            SourceTargetSelector sts = new SourceTargetSelector(_projectConfig, _receiver);
            sts.BusinessViewLinkClicked += BusinessViewLinkClicked;
            var pane = OpenPane(sts, "Source-Target Lineage");
        }

        private void UpdateLineageButton_Click(object sender, RoutedEventArgs e)
        {
            var msgRes = MessageBox.Show(string.Format("Update the metadata for {0} now? This may take several minutes.", _projectConfig.Name), "Lineage Update", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (msgRes != MessageBoxResult.Yes)
            {
                return;
            }

            ServiceMessagesButton_Click(this, e);
            var request = CreateEmptyRequest();

            /// TODO___

            ExtractAndUpdateModel();


            //DLSApiMessage content = new UpdateModelRequest();
            //request.Content = content.Serialize();
            //var resHandle = _receiver.PostMessage(request);

            //resHandle.ContinueWith((t) =>
            //{
            //    if (t.Result.MessageType == MessageTypeEnum.IsBusy)
            //    {
            //        MessageBox.Show("Update in progress.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    }
            //    else
            //    {
            //        var request2 = CreateEmptyRequest();
            //        DLSApiMessage content2 = new API.CreateGraphRequest() { KnowledgeBase = CreateGraphRequest.KnowledgeBaseEnum.DataFlow };
            //        request2.Content = content2.Serialize();
            //        var resHandle2 = _receiver.PostMessage(request2);
            //        //var res2 = await resHandle2;
            //    }
            //});
        }

        private async void ExtractAndUpdateModel()
        {
            var tempDirPath = Path.GetTempPath();
            var extractDirPath = Path.Combine(tempDirPath, "DLS_extract");
            if (Directory.Exists(extractDirPath))
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(extractDirPath);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            else
            {
                Directory.CreateDirectory(extractDirPath);
            }

            var configPath = Path.Combine(extractDirPath, "projectConfig.json");

            DAL.ExtractOperations.Downloader.DownloadConfig(_projectConfig.ProjectConfigId, configPath);

            var extractorPath = ConfigManager.ExtractorPath;

            ConfigManager.Log.Important(string.Format("Extracting definitions for {0}", _projectConfig.Name));

            // Use ProcessStartInfo class.
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = true;
            startInfo.FileName = extractorPath;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.WorkingDirectory = Path.GetDirectoryName(extractDirPath);
            startInfo.Arguments = string.Format("{0} {1}", extractDirPath, configPath);

            /**/
            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using-statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();

                    if (exeProcess.ExitCode != 0)
                    {
                        ConfigManager.Log.Error("Extractor returned an unexpected exit code: " + exeProcess.ExitCode.ToString());
                        throw new Exception();
                    }

                }
            }
            catch (Exception ex)
            {
                ConfigManager.Log.Error(string.Format("Extractor failed: " + ex.Message));
                throw;
            }

            ConfigManager.Log.Important(string.Format("Extraction finished"));
            zips = Directory.GetFiles(extractDirPath, "*.zip");
            if (zips.Length == 0)
            {
                throw new Exception($"No ZIP extracts found in {extractDirPath}");
            }
            if (zips.Length > 1)
            {
                throw new Exception($"Multiple ZIP extracts found in {extractDirPath}");
            }
            /**/
            //zips = new string[] { "C:\\Projects\\Descent\\Azure\\TestData\\058dd781-1457-43e3-a13e-600945897707.zip" };

            await UploadZip(zips,_receiver);
        }

        Task UploadZip(string[] zips, IReceiver receiver)
        {
            return Task.Factory.StartNew(() =>
            {
                _uploadTimer = new System.Windows.Threading.DispatcherTimer();
                _uploadTimer.Interval = new TimeSpan(0, 0, 4);
                ConfigManager.Log.Important(string.Format("Uploading extract to the DB"));
                Uploader.UploadExtract(zips[0], receiver, new DAL.Engine.NetBridge(), ConfigManager.CustomerCode);

                
                ConfigManager.Log.Important(string.Format("Upload finished"));
                MessageBox.Show($"Extract \"{zips[0]}\" successfully uploaded to the server, model update in progress. The model will not be available during this time.", "Extract complete", MessageBoxButton.OK, MessageBoxImage.Information);
                //MakeProjectButtonsUnavailable();
            });
        }

        private void UploadProgress_Uploaded(object s, UploadEventArgs e)
        {
            DispatcherOperation op = Dispatcher.BeginInvoke((Action)(() => {
                _uploadTimer.Start();
                UpdateView(e);
            }));
        }

        private void UpdateView(UploadEventArgs e)
        {
            
            UploadStatusValue.Text = "Uploading...";
            UploadStatusBar.Visibility = System.Windows.Visibility.Visible;
            UploadProgressBar.Value = e.UploadPercentage;
            UploadPercentigeValue.Text = e.UploadPercentage.ToString() + "%";
            if (e.UploadPercentage == 100)
            {
                UploadStatusValue.Text = "Uploaded";
                _uploadTimer.Stop();
                UploadStatusBar.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void ServiceMessagesButton_Click(object sender, RoutedEventArgs e)
        {
            LogListener listener = new LogListener();
            LogViewer viewer = new LogViewer();
            listener.NewLog += (s, e1) =>
            {
                Dispatcher.Invoke(() => { viewer.Write(e1.LogEntry.Message, e1.LogEntry.LogType); });
            };
            var pane = OpenPane(viewer, "Service Log");
            pane.Closing += (s, e1) =>
            {
                listener = null;
            };
        }

        private void DbMessagesButton_Click(object sender, RoutedEventArgs e)
        {

        }

        public LayoutAnchorable OpenPane(ContentControl content, string name)
        {
            LayoutAnchorable la = new LayoutAnchorable { Title = name, Content = content };
            
            la.AddToLayout(dockingManager, AnchorableShowStrategy.Right);
            la.CanClose = true;
            mainDocumentPaneGroup.Children.Add(la);
            la.IsSelected = true;
            return la;
        }

        private RequestMessage CreateEmptyRequest()
        {
            var msg = Helpers.CreateRequest(_receiver, _projectConfig == null ? Guid.Empty : _projectConfig.ProjectConfigId);
            msg.MessageToObjectId = _serviceReceiverId;
            msg.RequestForCoreType = Common.Interfaces.CoreTypeEnum.BIDoc;
            return msg;
        }

        private void ExternalSourcesButton_Click(object sender, RoutedEventArgs e)
        {
            DataSourceList dsl = new DataSourceList();
            dsl.LoadData(_projectConfig);
            var pane = OpenPane(dsl, "Data Sources");
        }

        private void LineageOverviewButton_Click(object sender, RoutedEventArgs e)
        {
            var overview = new DataFlowOverview();
            overview.LoadData(_projectConfig.ProjectConfigId);
            var pane = OpenPane(overview, "Data Flow Overview");
        }

        private void FindWarningsButton_Click(object sender, RoutedEventArgs e)
        {
            WarningGrid wg = new WarningGrid();
            wg.LoadData(_projectConfig);
            var pane = OpenPane(wg, "Warnings");
        }

        private void LineageSearchButton_Click(object sender, RoutedEventArgs e)
        {
            FulltextSearchPanel fts = new FulltextSearchPanel();
            fts.LoadData(_projectConfig.ProjectConfigId);
            var pane = OpenPane(fts, "Search");
            fts.SearchResultSelected += (o, e1) =>
            {
                ShowCentricBrowserForElement(e1.SelectedResult.ModelElementId, e1.SelectedResult.ElementName);
                //var previousCursor = Mouse.OverrideCursor;
                //Mouse.OverrideCursor = Cursors.Wait;
                //CentricBrowser centricBrowser = new CentricBrowser();
                //centricBrowser.Init(_serviceHelper, e1.SelectedResult.ModelElementId, _projectConfig.ProjectConfigId);
                //var newPane = OpenPane(centricBrowser, (string.IsNullOrWhiteSpace(e1.SelectedResult.BusinessName) ? e1.SelectedResult.ElementName : e1.SelectedResult.ElementName) + " | Data Flow");
                //Mouse.OverrideCursor = previousCursor;
                //pane.Close();
            };
        }

        private void ShowCentricBrowserForElement(int elementId, string elementName)
        {
            var previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
            CentricBrowser centricBrowser = new CentricBrowser();
            centricBrowser.Init(_serviceHelper, elementId, _projectConfig.ProjectConfigId);
            centricBrowser.BusinessViewLinkClicked += BusinessViewLinkClicked;
            var newPane = OpenPane(centricBrowser, elementName + " | Data Flow");
            Mouse.OverrideCursor = previousCursor;
        }

        private void BusinessViewLinkClicked(object sender, Clients.Controls.Dialogs.ElementView.BusinessViewLinkClickedArgs e)
        {
            ShowCentricBrowserForElement(e.LinkedElementId, e.LinkedElementName);
        }

        private void PermissionsClicked(object sender, RoleEventArgs e)
        {
            RolePermissions rp = new RolePermissions();
            rp.LoadData(_projectConfig, e.RoleId);
            var pane = OpenPane(rp, "Role Permissions | " + e.RoleName);
        }

        private void MembersClicked(object sender, RoleEventArgs e)
        {
            OpenRoleMembers(e.RoleId, e.RoleName);
        }

        private void OpenRoleMembers(int roleId, string roleName)
        {
            RoleMembers rm = new RoleMembers();
            rm.LoadData(_projectConfig, roleId);
            var pane = OpenPane(rm, "Role Members | " + roleName);
        }

        private async void EmptyRequestButton_Click(object sender, RoutedEventArgs e)
        {
            EmptyRequest emptyRequest = new EmptyRequest();
            var res = await _serviceHelper.PostRequest(emptyRequest);
            MessageBox.Show(string.Format("Empty request result: {0}", res.Message));
        }

        /*
        private void SendSerializedMessage_Click(object sender, RoutedEventArgs e)
        {
            TextInput ti = new TextInput();
            var pane = OpenPane(ti, "Message content");

            ti.Cancelled += (s, e2) =>
            {
                pane.Close();
            };

            ti.Submitted += (s, e1) =>
            {
                pane.Close();
                _receiver.SendServiceBusMessage(ti.TextContent);
            };
        }
        */

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutPane ap = new AboutPane();
            var pane = OpenPane(ap, "About");

        }

        private void UserPermissionButton_Click(object sender, RoutedEventArgs e)
        {
            SecurityGrid sg = new SecurityGrid();
            sg.LoadData(_projectConfig);
            sg.MembersClicked += MembersClicked;
            sg.PermissionsClicked += PermissionsClicked;
            var pane = OpenPane(sg, "User Role");            
        }

        private void UserSyncButton_Click(object sender, RoutedEventArgs e)
        {
            var dbUsers = _securityManager.ListUsers();
            var aadUsers = IdentityProvider.GetCustGroupUsers();

            var missingUsers = aadUsers.Where(a => dbUsers.All(x => x.Identity != a.Id)).ToList();

            if (missingUsers.Any())
            {
                var msgResult = MessageBox.Show(string.Format("Do you want to add these users from AAD? {0}{1} {2}", Environment.NewLine, Environment.NewLine,
                    string.Join(Environment.NewLine, missingUsers.Select(x => string.Format("{0} ({1})", x.DisplayName, x.UserPrincipalName)))),
                    "Add users", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (msgResult == MessageBoxResult.Yes)
                {
                    foreach (var user in missingUsers)
                    {
                        _securityManager.GetOrCreateNewUser(user.Id, user.DisplayName);
                    }
                    MessageBox.Show("Users added successfully.", "Users added", MessageBoxButton.OK);
                }
            }
            else
            {
                MessageBox.Show("There are no new users to add.", "Add users", MessageBoxButton.OK);
            }
        }

        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigManager.Log.FlushMessages();

            //if (_receiver != null)
            //{
            //    _receiver.CloseSubscriptionMessageReceiver();
            //}
        }

        private void AdminDictionaryViewFieldsButton_Click(object sender, RoutedEventArgs e)
        {
            ViewsPanel viewsPanel = new ViewsPanel();
            viewsPanel.Init(_projectConfig);
            var pane = OpenPane(viewsPanel, "Business Dictioanry Views");

            viewsPanel.Cancelled += (s, e2) =>
            {
                pane.Close();
            };
        }

        private void AdminDictionaryFieldsButton_Click(object sender, RoutedEventArgs e)
        {
            FieldsPanel fp = new FieldsPanel();
            fp.LoadData(_projectConfig);
            var pane = OpenPane(fp, "Business Fields");
        }

        private void AdminBusinessLinksTypeButton_Click(object sender, RoutedEventArgs e)
        {
            TypePanel tp = new TypePanel();
            tp.LoadData(_projectConfig);
            var pane = OpenPane(tp, "Link Types");
        }

        private void RibbonWindow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement fe = e.OriginalSource as FrameworkElement;
            if (fe == null)
            {
                return;
            }
            ConfigManager.Log.LogUserAction(Common.Interfaces.UserActionEventType.LeftClick, fe.Name, (fe.DataContext == null ? null : fe.DataContext.ToString()), null);    
        }

        private void RibbonWindow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement fe = e.OriginalSource as FrameworkElement;
            if (fe == null)
            {
                return;
            }
            ConfigManager.Log.LogUserAction(Common.Interfaces.UserActionEventType.RightClick, fe.Name, (fe.DataContext == null ? null : fe.DataContext.ToString()), null);
        }

       
    }
}