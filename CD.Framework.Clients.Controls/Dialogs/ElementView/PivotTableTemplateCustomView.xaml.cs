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

namespace CD.DLS.Clients.Controls.Dialogs.ElementView
{
    /// <summary>
    /// Interaction logic for PivotTableTemplateCustomView.xaml
    /// </summary>
    public partial class PivotTableTemplateCustomView : UserControl
    {
        public event RoutedEventHandler OpenPivotButtonClick;

        public PivotTableTemplateCustomView()
        {
            InitializeComponent();
        }

        private void OpenPivotButton_Click(object sender, RoutedEventArgs e)
        {
            if (OpenPivotButtonClick != null)
            {
                OpenPivotButtonClick(sender, e);
            }
        }
    }
}
