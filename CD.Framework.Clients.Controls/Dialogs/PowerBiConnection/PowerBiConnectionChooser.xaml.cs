using CD.DLS.Common.Structures;
using CD.DLS.Common.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CD.DLS.Clients.Controls.Dialogs.PowerBiConnection
{

    public class PowerBiProject
    {
        public PowerBiProjectConfigType ConfigType { get; set; }
        public string ApplicationID { get; set; }
        public string RedirectUri { get; set; }
        public string WorkspaceID { get; set; }
        public string Tenant { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ReportServerURL { get; set; }
        public string ReportServerFolder { get; set; }
        public string DiskFolder { get; set; }
    }

    public partial class PowerBiConnectionChooser : System.Windows.Controls.UserControl
    {

        public PowerBiConnectionChooser()
        {
            InitializeComponent();
            radioDiskFolder.IsChecked = true;
        }

        public PowerBiConnectionChooser(PowerBiProject defaultProject)
        {
            InitializeComponent();
            SetDefaultProject(defaultProject);
        }

        public PowerBiProject SelectedProject
        {
            get
            {
                var p = new PowerBiProject()
                {
                    ApplicationID = applicationIDTextBox.Text,
                    RedirectUri = redirectUriTextBox.Text,
                    WorkspaceID = workspaceIDTextBox.Text,
                    UserName = userNameTextBox.Text,
                    Password = passwordTextBox.Password,
                    ReportServerURL = reportServerURLTextBox.Text,
                    ReportServerFolder = reportServerFolderTextBox.Text,
                    DiskFolder = diskFolderTextBox.Text
                };

                if (radioDiskFolder.IsChecked.Value)
                    p.ConfigType = PowerBiProjectConfigType.DiskFolder;
                else if (radioSelectWorkspace.IsChecked.Value)
                    p.ConfigType = PowerBiProjectConfigType.PbiAppCustomWorkspace;
                else if (radioDefaultWokspace.IsChecked.Value)
                    p.ConfigType = PowerBiProjectConfigType.PbiAppDefaultWorkspace;
                else if (reportServerWorkspace.IsChecked.Value)
                    p.ConfigType = PowerBiProjectConfigType.ReportServer;

                return p;
            }
        }

        public void SetDefaultProject(PowerBiProject defaultProject)
        {
            switch (defaultProject.ConfigType)
            {
                case PowerBiProjectConfigType.DiskFolder:
                    radioDiskFolder.IsChecked = true;
                    break;
                case PowerBiProjectConfigType.PbiAppCustomWorkspace:
                    radioSelectWorkspace.IsChecked = true;
                    break;
                case PowerBiProjectConfigType.PbiAppDefaultWorkspace:
                    radioDefaultWokspace.IsChecked = true;
                    break;
                case PowerBiProjectConfigType.ReportServer:
                    reportServerWorkspace.IsChecked = true;
                    break;
            }

            applicationIDTextBox.Text = defaultProject.ApplicationID;
            redirectUriTextBox.Text = defaultProject.RedirectUri;
            workspaceIDTextBox.Text = defaultProject.WorkspaceID; 
            userNameTextBox.Text = defaultProject.UserName;
            passwordTextBox.Password = defaultProject.Password;
            reportServerURLTextBox.Text = defaultProject.ReportServerURL;
            reportServerFolderTextBox.Text = defaultProject.ReportServerFolder;
            diskFolderTextBox.Text = defaultProject.DiskFolder;
        }

        private void radioDiskFolder_Checked(object sender, RoutedEventArgs e)
        {
            SetPowerBiAppSettingsVisibility(false);
            var nv = Visibility.Collapsed;
            reportServerFolderLabel.Visibility = nv;
            reportServerFolderTextBox.Visibility = nv;
            reportServerURLLabel.Visibility = nv;
            reportServerURLTextBox.Visibility = nv;
        }

        private void radioDefaultWokspace_Checked(object sender, RoutedEventArgs e)
        {
            SetPowerBiAppSettingsVisibility(true);
            workspaceIDLabel.Visibility = Visibility.Collapsed;
            workspaceIDTextBox.Visibility = Visibility.Collapsed;
        }

        private void radioSelectWorkspace_Checked(object sender, RoutedEventArgs e)
        {
            SetPowerBiAppSettingsVisibility(true);
        }

        private void reportServerWorkspace_Checked(object sender, RoutedEventArgs e)
        {
            SetPowerBiAppSettingsVisibility(false);
            var nv = Visibility.Collapsed;
            diskFolderLabel.Visibility = nv;
            diskFolderTextBox.Visibility = nv;
        }

        public void SetPowerBiAppSettingsVisibility(bool visible)
        {
            var v = visible ? Visibility.Visible : Visibility.Collapsed;
            var nv = visible ? Visibility.Collapsed : Visibility.Visible;

            applicationIDLabel.Visibility = v;
            applicationIDTextBox.Visibility = v;
            redirectUriLabel.Visibility = v;
            redirectUriTextBox.Visibility = v;
            workspaceIDLabel.Visibility = v;
            workspaceIDTextBox.Visibility = v;
            userNameLabel.Visibility = v;
            userNameTextBox.Visibility = v;
            passwordLabel.Visibility = v;
            passwordTextBox.Visibility = v;

            reportServerFolderLabel.Visibility = nv;
            reportServerFolderTextBox.Visibility = nv;
            reportServerURLLabel.Visibility = nv;
            reportServerURLTextBox.Visibility = nv;
            diskFolderLabel.Visibility = nv;
            diskFolderTextBox.Visibility = nv;
        }

        private void diskFolderTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            var result = folderBrowser.ShowDialog();
            var path = folderBrowser.SelectedPath;
            diskFolderTextBox.Text = path;
        }
    }

}
