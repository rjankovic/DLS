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

    public partial class PowerBiConnectionChooser : UserControl
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
                    //WorkspaceID = radioDefaultWokspace.IsChecked.Value ? null : workspaceIDTextBox.Text,
                    WorkspaceID = workspaceIDTextBox.Text,
                    UserName = userNameTextBox.Text,
                    Password = passwordTextBox.Password,
                    ReportServerURL = reportServerURLTextBox.Text,
                    ReportServerFolder = reportServerFolderTextBox.Text,
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
            /*
            if (defaultProject.WorkspaceID != null)
            {    
                radioSelectWorkspace.IsChecked = true;
            }
            */
            userNameTextBox.Text = defaultProject.UserName;
            passwordTextBox.Password = defaultProject.Password;
            reportServerURLTextBox.Text = defaultProject.ReportServerURL;
            reportServerFolderTextBox.Text = defaultProject.ReportServerFolder;
            diskFolderTextBox.Text = defaultProject.DiskFolder;
        }

        private void radioDiskFolder_Checked(object sender, RoutedEventArgs e)
        {
            //if(!this.IsLoaded)
            //{
            //    return;
            //}
            SetPowerBiAppSettingsVisibility(false);
            var nv = Visibility.Collapsed;
            reportServerFolderLabel.Visibility = nv;
            reportServerFolderTextBox.Visibility = nv;
            reportServerURLLabel.Visibility = nv;
            reportServerURLTextBox.Visibility = nv;
        }

        private void radioDefaultWokspace_Checked(object sender, RoutedEventArgs e)
        {
            //if (!this.IsLoaded)
            //{
            //    return;
            //}
            SetPowerBiAppSettingsVisibility(true);
            workspaceIDLabel.Visibility = Visibility.Collapsed;
            workspaceIDTextBox.Visibility = Visibility.Collapsed;
        }

        private void radioSelectWorkspace_Checked(object sender, RoutedEventArgs e)
        {
            //if (!this.IsLoaded)
            //{
            //    return;
            //}
            SetPowerBiAppSettingsVisibility(true);
        }

        private void reportServerWorkspace_Checked(object sender, RoutedEventArgs e)
        {
            //if (!this.IsLoaded)
            //{
            //    return;
            //}
            SetPowerBiAppSettingsVisibility(false);
            var nv = Visibility.Collapsed;
            diskFolderLabel.Visibility = nv;
            diskFolderTextBox.Visibility = nv;
        }

        public void SetPowerBiAppSettingsVisibility(bool visible)
        {
            //if (!this.IsLoaded)
            //{
            //    return;
            //}
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

        /*
            <Label x:Name="applicationIDLabel" Content="ApplicationID" IsEnabled="{Binding IsChecked,ElementName=radioNativeMode }" Grid.Column="0" Grid.Row="1" />
            <TextBox Style="{StaticResource GreyTextBox}" x:Name="applicationIDTextBox" IsEnabled="{Binding IsChecked,ElementName=radioNativeMode }" Margin="2" HorizontalAlignment="Stretch" Grid.Column="1" Grid.Row="1" Text="" />
            <Label x:Name="redirectUriLabel" Content="Redirect Uri" IsEnabled="{Binding IsChecked,ElementName=radioNativeMode }" Grid.Column="0" Grid.Row="2" />
            <TextBox Style="{StaticResource GreyTextBox}" x:Name="redirectUriTextBox" IsEnabled="{Binding IsChecked,ElementName=radioNativeMode }" Margin="2" HorizontalAlignment="Stretch" Grid.Column="1" Grid.Row="2" Text="" />
            <Label x:Name="workspaceIDLabel" Content="Workspace ID" IsEnabled="{Binding IsChecked,ElementName=radioSelectWorkspace }" Grid.Column="0" Grid.Row="3" />
            <TextBox x:Name="workspaceIDTextBox" IsEnabled="{Binding IsChecked,ElementName=radioSelectWorkspace }" Margin="2" HorizontalAlignment="Stretch" Grid.Column="1" Grid.Row="3" />
            <Label x:Name="userNameLabel" Content="User name" IsEnabled="True" Grid.Column="0" Grid.Row="4" />
            <TextBox x:Name="userNameTextBox" Style="{StaticResource GreyTextBox}" IsEnabled="True" Margin="2" HorizontalAlignment="Stretch" Grid.Column="1" Grid.Row="4" />
            <Label x:Name="passwordLabel" Content="Password" IsEnabled="True" Grid.Column="0" Grid.Row="5" />
            <PasswordBox x:Name="passwordTextBox" Style="{StaticResource GreyPasswordBox}" IsEnabled="True" Margin="2" HorizontalAlignment="Stretch" Grid.Column="1" Grid.Row="5" />
            <TextBox x:Name="reportServerURLTextBox" Style="{StaticResource GreyTextBox}" IsEnabled="{Binding IsChecked,ElementName=reportServerWorkspace}" Margin="2" HorizontalAlignment="Stretch" Grid.Column="1" Grid.Row="6" />
            <Label x:Name="reportServerURLLabel" Content="Report Server URL" IsEnabled="True" Grid.Column="0" Grid.Row="6" />
            <TextBox x:Name="reportServerFolderTextBox" Style="{StaticResource GreyTextBox}" IsEnabled="{Binding IsChecked,ElementName=reportServerWorkspace}" Margin="2" HorizontalAlignment="Stretch" Grid.Column="1" Grid.Row="7" />
            <Label x:Name="reportServerFolderLabel" Content="Report Server folder" IsEnabled= "True" Grid.Column="0" Grid.Row="7" />
            <Label x:Name="diskFolderLabel" Content="Disk Folder Path" IsEnabled="{Binding IsChecked,ElementName=radioDiskFolder }" Grid.Column="0" Grid.Row="8" />
            <TextBox Style="{StaticResource GreyTextBox}" x:Name="diskFolderTextBox" IsEnabled="{Binding IsChecked,ElementName=radioDiskFolder }" Margin="2" HorizontalAlignment="Stretch" Grid.Column="1" Grid.Row="8" Text="" />

         */
    }

}
