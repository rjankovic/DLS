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

namespace CD.DLS.Clients.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for Multiselect.xaml
    /// </summary>
    public partial class Multiselect : UserControl
    {
        public Multiselect()
        {
            InitializeComponent();
        }

        public class SelectItem
        {
            public int Id { get; set; }
            public string Label { get; set; }
        }

        private List<SelectItem> _includedItems = new List<SelectItem>();
        private List<SelectItem> _excludedItems = new List<SelectItem>();

        public List<SelectItem> IncludedItems { get { return _includedItems; } }
        public List<SelectItem> ExcludedItems { get { return _excludedItems; } }

        public void SetData(List<SelectItem> includedItems, List<SelectItem> excludedItems)
        {
            _includedItems = includedItems;
            _excludedItems = excludedItems;

            UpdateView();
        }

        private void UpdateView()
        {
            IncludedItemsListBox.UnselectAll();
            ExcludedItemsListBox.UnselectAll();
            IncludedItemsListBox.Items.Clear();
            ExcludedItemsListBox.Items.Clear();

            foreach (var item in _includedItems)
            {
                IncludedItemsListBox.Items.Add(item);
            }
            foreach (var item in _excludedItems)
            {
                ExcludedItemsListBox.Items.Add(item);
            }

            //IncludedItemsListBox.DataContext = _includedItems;
            //ExcludedItemsListBox.DataContext = _excludedItems;
            //IncludedItemsListBox.ItemsSource = _includedItems;
            //ExcludedItemsListBox.ItemsSource = _excludedItems;
        }

        private void IncludedItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ExcludeButton_Click(this, new RoutedEventArgs());
        }

        private void ExcludedItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IncludeButton_Click(this, new RoutedEventArgs());
        }

        private void ExcludeButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = IncludedItemsListBox.SelectedItem as SelectItem;
            if (selected == null)
            {
                return;
            }

            _includedItems.Remove(selected);
            _excludedItems.Add(selected);
            UpdateView();
        }

        private void IncludeButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = ExcludedItemsListBox.SelectedItem as SelectItem;
            if (selected == null)
            {
                return;
            }

            _excludedItems.Remove(selected);
            _includedItems.Add(selected);
            UpdateView();
        }
    }
}
