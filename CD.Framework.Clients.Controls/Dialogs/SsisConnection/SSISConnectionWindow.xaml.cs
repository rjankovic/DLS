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
    /// Interaction logic for SSISConnectionWindow.xaml
    /// </summary>
    public partial class SSISConnectionWindow : Window
    {
        public SSISConnectionWindow(SsisProject defaultProject = null)
        {
            InitializeComponent();
            if (defaultProject != null)
            {
                connectionChooser.SetDefaultProject(defaultProject);
            }
        }

        private void connectionChooser_Submitted(object sender, EventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        public SsisProject Project
        {
            get { return connectionChooser.SelectedProject; }
        }

        private void connectionChooser_Cancelled(object sender, EventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
