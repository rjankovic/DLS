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
