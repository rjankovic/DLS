using CD.DLS.Clients.Controls.Dialogs.CentricGraphBrowser;
using CD.DLS.Clients.Controls.Dialogs.Search;
using CD.DLS.Common.Structures;
using System;
using System.Windows;
using System.Windows.Controls;
using CD.DLS.DAL.Managers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static CD.DLS.DAL.Managers.SecurityManager;
using CD.DLS.Clients.Controls.Windows;

namespace CD.DLS.Clients.Controls.Dialogs.Security
{
    /// <summary>
    /// Interaction logic for Security.xaml
    /// </summary>
    public partial class SecurityGrid : UserControl
    {
        private SecurityManager SecurityManager;
        public event RoleEventHandler MembersClicked;
        public event RoleEventHandler PermissionsClicked;
        private List<SecurityManager.SecurityRole> _data;
        private ProjectConfig _projectConfig;
        private string _roleName;

        public SecurityGrid()
        {
            InitializeComponent();       
        }

        public void LoadData(ProjectConfig projectConfig)
        {
            waitingPanel.Visibility = Visibility.Visible;
            SecurityManager = new SecurityManager();
            Task loadingTask = Task.Factory.StartNew(() => { LoadCurrentTable(projectConfig); });
            loadingTask.ContinueWith((t) => { Dispatcher.Invoke(UpdateGrid); });            
        }

        private void  LoadCurrentTable(ProjectConfig projectConfig)
        {
            var data = SecurityManager.ListRoles(projectConfig);
            _data = data;
            _projectConfig = projectConfig;
        }

        private void UpdateGrid()
        {
            userGrid.ItemsSource = _data;
            waitingPanel.Visibility = Visibility.Hidden;
        }

        private void AddRole_Click(object sender, RoutedEventArgs e)
        {
            var roles = SecurityManager.ListRoles(_projectConfig); // Roles();
            NameChooserWindow window = new NameChooserWindow(roles.Select(x => x.Name).ToList());
            var res = window.ShowDialog();
            if (!(window.SelectedName == null) || !(window.SelectedName == ""))
            {
                if (window.Submittted == true)
                {
                    _roleName = window.SelectedName;
                    UploadRole();
                    LoadData(_projectConfig);
                }
                     
            }
            else
            {
                return;
            }
        }

        private void UploadRole()
        {
            waitingPanel.Visibility = Visibility;
            SecurityManager.AddRole(_projectConfig, _roleName);
        }
               
        private void RemoveRole_Click(object sender, RoutedEventArgs e)
        {
            SecurityRole rowView = (SecurityRole)userGrid.SelectedItem;
            if (!(rowView == null))
            {
                //string result = rowView.Name.ToString();

                var roleName = rowView.Name;
                if (roleName == "CustomerAdmin" || roleName == "CustomerUser")
                {
                    MessageBox.Show("System roles CustomerUser and CustomerAdmin cannot be removed.", "Cannot remove system role", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                MessageBoxResult MBresult = MessageBox.Show("Do you want to delete role " + rowView.Name + " ?",
                                      "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (MBresult == MessageBoxResult.Yes)
                {
                    SecurityManager.DeleteRole(rowView.RoleId);
                    LoadData(_projectConfig);
                }
            }
            else
            {
                MessageBox.Show("Please, select an item in the grid.",
                     "Information", MessageBoxButton.OK, MessageBoxImage.Question);
            }
        }

        private void Members_Click(object sender, RoutedEventArgs e)
        {
            SecurityRole rowView = (SecurityRole)userGrid.SelectedItem;
            //String result = rowView.Name.ToString();
            MembersClicked(this, new RoleEventArgs() { RoleId = rowView.RoleId, RoleName = rowView.Name });
        }

        private void Permissions_Click(object sender, RoutedEventArgs e)
        {
            SecurityRole rowView = (SecurityRole)userGrid.SelectedItem;
            //String result = rowView.Name.ToString();
            PermissionsClicked(this, new RoleEventArgs() { RoleId = rowView.RoleId, RoleName = rowView.Name });
        }
    }
}
