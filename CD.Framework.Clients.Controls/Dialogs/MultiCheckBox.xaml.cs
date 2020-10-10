using CD.Framework.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using System.Windows.Threading;

namespace CD.Framework.Clients.Controls.Dialogs
{
    public class CheckBoxItem
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Interaction logic for ListPicker.xaml
    /// </summary>
    public partial class MultiCheckBox : UserControl
    {

        public ObservableCollection<CheckBoxItem> Items { get; set; }

        public MultiCheckBox()
        {
            InitializeComponent();
            Items = new ObservableCollection<CheckBoxItem>();
        }

        public void SetItems(IEnumerable<CheckBoxItem> items)
        {
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }
            this.DataContext = this;
        }
    
        }
    }
}
