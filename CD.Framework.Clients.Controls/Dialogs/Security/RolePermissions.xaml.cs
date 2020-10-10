using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static CD.DLS.DAL.Managers.SecurityManager;

namespace CD.DLS.Clients.Controls.Dialogs.Security
{
    /// <summary>
    /// Interaction logic for RolePermissions.xaml
    /// </summary>
    public partial class RolePermissions : UserControl
    {
        public RolePermissions()
        {
            InitializeComponent();
        }
        SecurityManager SecurityManager;
        List<SecurityRolePermission> _data;
        List<SecurityRolePermission> _takenPermissions;
        ProjectConfig _projectConfig;
        int _roleId;
        int _selectedRolePermission;

        public void LoadData(ProjectConfig projectConfig, int roleId)
        {
            waitingPanel.Visibility = Visibility.Visible;
            SecurityManager = new SecurityManager();
            Task loadingTask = Task.Factory.StartNew(() => { LoadCurrentTable(projectConfig,roleId); });
            loadingTask.ContinueWith((t) => { Dispatcher.Invoke(UpdateGrid); });
        }

        private void LoadCurrentTable(ProjectConfig projectConfig, int roleId)
        {
            var data = SecurityManager.RolePermissions(roleId);
            _data = data;
            _takenPermissions = SecurityManager.RolePermissions(roleId);
            _projectConfig = projectConfig;
            _roleId = roleId;
        }

        private void UpdateGrid()
        {
            userGrid.ItemsSource = _data;
            waitingPanel.Visibility = Visibility.Hidden;
        }

        private void AddType_Click(object sender, RoutedEventArgs e)
        {
            var permissions = SecurityManager.ListPermissions(); // .UserPermissions(_projectConfig); // .Permissions();
            NamePicker window = new NamePicker(
                _takenPermissions.Select(x => new PickerItem() { Id = x.PermissionId, Label = x.Type.ToString() }).ToList(),
                permissions.Select(x => new PickerItem() { Id = x.PermissionId, Label = x.Type.ToString() }).ToList()
                //_takenPermissions,permissions
                );
            window.Submitted += (s, e1) => { window.Close(); window.IsSubmitted = true; };
            window.Cancelled += (s, e2) => { window.Close(); };
            window.Loaded += (s, e3) => {  };
            try
            {
                window.ShowDialog();
                if (!(window.SelectedItem == null) /*|| !(window.SelectedItem == "")*/)
                {
                    if (window.IsSubmitted == true)
                    {
                        _selectedRolePermission = (int)window.SelectedItem.Id; //.ToString();
                        UploadPermission();
                        LoadData(_projectConfig, _roleId);
                    }
                }
            }
            catch
            {
                window.Close();
                MessageBox.Show("You did not choose value in the box.",
                     "Information", MessageBoxButton.OK, MessageBoxImage.Question);
            }
        }

        private void UploadPermission()
        {
            waitingPanel.Visibility = Visibility.Visible;
            SecurityManager.AddRolePermission(_roleId, _selectedRolePermission);
        }

        private void RemoveType_Click(object sender, RoutedEventArgs e)
        {
            SecurityRolePermission rowView = (SecurityRolePermission)userGrid.SelectedItem;
            if (!(rowView == null))
            {
                //string result = rowView.Type.ToString();

                MessageBoxResult MBresult = MessageBox.Show("Do you want to delete permission " + rowView.Type.ToString() + " from the role?",
                                      "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (MBresult == MessageBoxResult.Yes)
                {
                    SecurityManager.DeleteRolePermission(_roleId, rowView.PermissionId);
                    LoadData(_projectConfig, _roleId);
                }
            }
            else
            {
                MessageBox.Show("Please, select an item in the grid.",
                     "Information", MessageBoxButton.OK, MessageBoxImage.Question);
            }
        }
    }
}