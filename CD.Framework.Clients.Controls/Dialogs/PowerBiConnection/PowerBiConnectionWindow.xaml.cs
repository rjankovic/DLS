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
    /// <summary>
    /// Interaction logic for PowerBiConnectionWindow.xaml
    /// </summary>
    public partial class PowerBiConnectionWindow : Window
    {
        public PowerBiConnectionWindow(PowerBiProject defaultProject = null)
        {
            InitializeComponent();
            if (defaultProject != null)
            {
                connectionChooser.SetDefaultProject(defaultProject);
            }
        }

        public void PowerBiConnectionChooser()
        {
            InitializeComponent();
        }

        public PowerBiProject Project
        {
            get { return connectionChooser.SelectedProject; }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {         
            /*
            if (String.IsNullOrEmpty(Project.ApplicationID))
            {
                MessageBox.Show("Please specify the applicationID", "Specify applicationID", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (String.IsNullOrEmpty(Project.RedirectUri))
            {
                MessageBox.Show("Please specify the redirectURI", "Specify redirectURI", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (String.IsNullOrEmpty(Project.UserName))
            {
                MessageBox.Show("Please specify the user name", "Specify user name", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (String.IsNullOrEmpty(Project.Password))
            {
                MessageBox.Show("Please specify password", "Specify password", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Project.WorkspaceID != null)
            {
                if (Project.WorkspaceID.Trim() == "")
                {
                    MessageBox.Show("Please specify the workspace ID or select the Default Wokspace Mode. (The workspace ID is the code after \"https://app.powerbi.com/groups/\" when viewing the workspace.)", "Specify Workspace ID", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            */
            this.DialogResult = true;
            this.Close();
                                                         
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
