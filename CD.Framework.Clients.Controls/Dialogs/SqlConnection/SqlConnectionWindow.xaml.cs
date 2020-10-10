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

namespace CD.DLS.Clients.Controls.Dialogs.SqlConnection
{
    /// <summary>
    /// Interaction logic for SqlConnectionWindow.xaml
    /// </summary>
    public partial class SqlConnectionWindow : Window
    {
        public SqlConnectionWindow()
        {
            InitializeComponent();
        }

        public SqlConnectionWindow(string server, string database)
        {
            InitializeComponent();
            connectionBuilder.ConnectionString = new SqlConnectionString() { Database = database, Server = server, IntegratedSecurity = true };
        }

        public SqlConnectionString Connection { get { return connectionBuilder.ConnectionString; } }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (connectionBuilder.TestConnection())
            {
                this.DialogResult = true;
                Close();
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}
