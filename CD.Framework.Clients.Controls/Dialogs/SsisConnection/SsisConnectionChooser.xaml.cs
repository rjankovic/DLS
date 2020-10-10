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

namespace CD.DLS.Clients.Controls.Dialogs.SsisConnection
{
    /// <summary>
    /// Interaction logic for ListPicker.xaml
    /// </summary>
    public partial class SsisConnectionChooser : UserControl
    {

        public event EventHandler Submitted;
        public event EventHandler Cancelled;

        private SsisProject _selectedProject;
        
        public SsisProject SelectedProject { get { return _selectedProject; } }

        public SsisConnectionChooser()
        {
            InitializeComponent();
        }

        public SsisConnectionChooser(SsisProject defaultProject)
        {
            InitializeComponent();
            SetDefaultProject(defaultProject);
        }

        public void SetDefaultProject(SsisProject defaultProject)
        {
            _selectedProject = defaultProject;
            serverTextBox.Text = defaultProject.Server;
            var connected = ConnectToServer(_selectedProject.Server);
            if (connected)
            {
                projectCombo.SelectedItem = ((List<SsisProject>)projectCombo.ItemsSource).FirstOrDefault(x => x.FullPath == _selectedProject.FullPath);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancelled?.Invoke(this, new EventArgs());
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            var item = projectCombo.SelectedItem as SsisProject;
            _selectedProject = item;
            
            if (_selectedProject != null)
            {
                Submitted?.Invoke(this, new EventArgs());
            }
            else
            {
                MessageBox.Show("Please select a project", "Select project", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectToServer(serverTextBox.Text);
        }

        private bool ConnectToServer(string name)
        {
            string error;
            var projects = SsisProjectListing.ListPrjects(name, out error);
            if (projects != null)
            {
                projectCombo.ItemsSource = projects;
                projectCombo.DisplayMemberPath = "FullPath";
                projectCombo.Items.Refresh();
                projectCombo.IsEnabled = true;
                return true;
            }
            else
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                projectCombo.IsEnabled = false;
                return false;
            }
        }
    }
}
