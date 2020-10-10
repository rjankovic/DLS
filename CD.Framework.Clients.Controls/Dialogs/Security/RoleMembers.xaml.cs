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
    /// Interaction logic for RoleMembers.xaml
    /// </summary>
    public partial class RoleMembers : UserControl
    {
        public RoleMembers()
        {
            InitializeComponent();
        }
        private SecurityManager SecurityManager;
        private List<SecurityManager.SecurityUser> _data;
        private int _roleId;
        private int _selectedUserId;
        private ProjectConfig _projectConfig;
        private List<SecurityUser> _takenMembers;

        public void LoadData(ProjectConfig projectConfig, int roleId)
        {
            waitingPanel.Visibility = Visibility.Visible;
            SecurityManager = new SecurityManager();
            Task loadingTask = Task.Factory.StartNew(() => { LoadCurrentTable(projectConfig, roleId); });
            loadingTask.ContinueWith((t) => { Dispatcher.Invoke(UpdateGrid); });
            _projectConfig = projectConfig;
        }

        private void LoadCurrentTable(ProjectConfig projectConfig, int roleId)
        {
            var data = SecurityManager.RoleMembers(roleId);
            _data = data;
            _takenMembers = SecurityManager.RoleMembers(roleId); //.ListRoles(); RoleMemberString(roleId);
            _roleId = roleId;
        }

        private void UpdateGrid()
        {
            userGrid.ItemsSource = _data;
            waitingPanel.Visibility = Visibility.Hidden;
        }

        private void AddMember_Click(object sender, RoutedEventArgs e)
        {
            var displayNames = SecurityManager.ListUsers();
            NamePicker window = new NamePicker(_takenMembers.Select(x => new PickerItem() { Id = x.UserId, Label = x.DisplayName }).ToList()
                , displayNames.Select(x => new PickerItem() { Id = x.UserId, Label = x.DisplayName }).ToList());
            window.Submitted += (s, e1) => { window.Close(); window.IsSubmitted = true; };
            window.Cancelled += (s, e2) => { window.Close(); };
            window.Loaded += (s, e3) => { };
            try
            {
                window.ShowDialog();
                if (!(window.SelectedItem == null) /*|| !(window.SelectedItem == "")*/)
                {
                    if (window.IsSubmitted == true)
                    {
                        _selectedUserId = (int)window.SelectedItem.Id; // ToString();
                        UploadDisplayName();
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

        private void RemoveMember_Click(object sender, RoutedEventArgs e)
        {
            SecurityUser rowView = (SecurityUser)userGrid.SelectedItem;
            if (!(rowView == null))
            {
                //string result = rowView.DisplayName.ToString();

                MessageBoxResult MBresult = MessageBox.Show("Do you want to delete member " + rowView.DisplayName + " from the role?",
                                      "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (MBresult == MessageBoxResult.Yes)
                {
                    SecurityManager.DeleteRoleMember(_roleId, rowView.UserId);
                    LoadData(_projectConfig,_roleId);
                }
            }
            else
            {
                MessageBox.Show("Please, select an item in the grid.",
                     "Information", MessageBoxButton.OK, MessageBoxImage.Question);
            }
        }

        private void UploadDisplayName()
        {
            waitingPanel.Visibility = Visibility.Visible;
            SecurityManager.AddRoleMemeber(_selectedUserId, _roleId);
        }
    }
}
