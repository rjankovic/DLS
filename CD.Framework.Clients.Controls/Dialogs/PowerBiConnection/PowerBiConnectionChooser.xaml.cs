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
   
        public string ApplicationID { get; set; }
        public string RedirectUri { get; set; }
        public string WorkspaceID { get; set; }
        public string Tenant { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ReportServerURL { get; set; }
        public string ReportServerFolder { get; set; }
    }

    public partial class PowerBiConnectionChooser : UserControl
    {

        public PowerBiConnectionChooser()
        {
            InitializeComponent();
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
                return new PowerBiProject()
                {
                    ApplicationID = applicationIDTextBox.Text,
                    RedirectUri = redirectUriTextBox.Text,
                    WorkspaceID = radioDefaultWokspace.IsChecked.Value ? null : workspaceIDTextBox.Text,
                    UserName = userNameTextBox.Text,
                    Password = passwordTextBox.Password,
                    ReportServerURL = reportServerURLTextBox.Text,
                    ReportServerFolder = reportServerFolderTextBox.Text
                };
            }
        }

        public void SetDefaultProject(PowerBiProject defaultProject)
        {
            applicationIDTextBox.Text = defaultProject.ApplicationID;
            redirectUriTextBox.Text = defaultProject.RedirectUri;
            if (defaultProject.WorkspaceID != null)
            {
                workspaceIDTextBox.Text = defaultProject.WorkspaceID;
                radioSelectWorkspace.IsChecked = true;
            }
            userNameTextBox.Text = defaultProject.UserName;
            passwordTextBox.Password = defaultProject.Password;
            reportServerURLTextBox.Text = defaultProject.ReportServerURL;
            reportServerFolderTextBox.Text = defaultProject.ReportServerFolder;

        }
    }

}
