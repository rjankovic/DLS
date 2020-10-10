using CD.DLS.Clients.Controls.Windows;
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

namespace CD.DLS.Clients.Controls.Dialogs.BusinessDictionaryAdmin
{
    public partial class AddLinkPanel : UserControl
    {
        public AddLinkPanel()
        {
            InitializeComponent();
        }

        public event AddLinkEventHandler AddFromElementClicked;
        public event AddLinkEventHandler AddToElementClicked;

        private void AddFromElement_Click(object sender, RoutedEventArgs e)
        {
            AddFromElementClicked(this, new AddLinkEventArgs() { IsFrom = true});
        }

        private void AddToElement_Click(object sender, RoutedEventArgs e)
        {
            AddToElementClicked(this, new AddLinkEventArgs() { IsFrom = false });
        }
    }
}
