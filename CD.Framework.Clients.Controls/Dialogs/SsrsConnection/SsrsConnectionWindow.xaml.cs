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

namespace CD.DLS.Clients.Controls.Dialogs.SsrsConnection
{
    /// <summary>
    /// Interaction logic for SSISConnectionWindow.xaml
    /// </summary>
    public partial class SsrsConnectionWindow : Window
    {
        public SsrsConnectionWindow(SsrsProject defaultProject = null)
        {
            InitializeComponent();
            if (defaultProject != null)
            {
                connectionChooser.SetDefaultProject(defaultProject);
            }
        }
        
        public SsrsProject Project
        {
            get { return connectionChooser.SelectedProject; }
        }
        
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (connectionChooser.Validate())
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
