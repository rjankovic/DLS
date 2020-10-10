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
    /// Interaction logic for SSISConnectionWindow.xaml
    /// </summary>
    public partial class SsasConnectionWindow : Window
    {
        public SsasConnectionWindow(SsasDatabase defaultDb = null)
        {
            InitializeComponent();
            if (defaultDb != null)
            {
                connectionChooser.SetDefaultDb(defaultDb);
            }
        }

        private void connectionChooser_Submitted(object sender, EventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        public SsasDatabase Database
        {
            get { return connectionChooser.SelectedDb; }
        }

        private void connectionChooser_Cancelled(object sender, EventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
