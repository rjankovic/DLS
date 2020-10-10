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

namespace CD.DLS.Clients.Controls.Dialogs.SsasConnection
{
    /// <summary>
    /// Interaction logic for ListPicker.xaml
    /// </summary>
    public partial class SsasConnectionChooser : UserControl
    {

        public event EventHandler Submitted;
        public event EventHandler Cancelled;

        private SsasDatabase _selectedDb;
        
        public SsasDatabase SelectedDb { get { return _selectedDb; } }

        public SsasConnectionChooser()
        {
            InitializeComponent();
        }

        public SsasConnectionChooser(SsasDatabase defaultDb)
        {
            InitializeComponent();
            SetDefaultDb(defaultDb);
        }

        public void SetDefaultDb(SsasDatabase defaultDb)
        {
            _selectedDb = defaultDb;
            serverTextBox.Text = defaultDb.Server;
            var connected = ConnectToServer(_selectedDb.Server);
            if (connected)
            {
                dbCombo.SelectedItem = ((List<SsasDatabase>)dbCombo.ItemsSource).FirstOrDefault(x => x.Database == _selectedDb.Database);
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
            var item = dbCombo.SelectedItem as SsasDatabase;
            _selectedDb = item;
            
            if (_selectedDb != null)
            {
                Submitted?.Invoke(this, new EventArgs());
            }
            else
            {
                MessageBox.Show("Please select a database", "Select database", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectToServer(serverTextBox.Text);
        }

        private bool ConnectToServer(string name)
        {
            string error;
            var databases = SsasProjectListing.ListPrjects(name, out error);
            if (databases != null)
            {
                dbCombo.ItemsSource = databases;
                dbCombo.DisplayMemberPath = "Database";
                dbCombo.Items.Refresh();
                dbCombo.IsEnabled = true;
                return true;
            }
            else
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dbCombo.IsEnabled = false;
                return false;
            }
        }
    }
}
