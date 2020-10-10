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

namespace CD.DLS.Clients.Controls.Dialogs
{
    public class CheckListViewItem
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }

    /// <summary>
    /// Interaction logic for ListPicker.xaml
    /// </summary>
    public partial class CheckedListView : UserControl
    {
        private List<CheckListViewItem> _items = new List<CheckListViewItem>();

        public List<CheckListViewItem> SelectedItems
        {
            get
            {
                return (List<CheckListViewItem>)(checkedListView.SelectedItems);
            }
        }

        public CheckedListView()
        {
            InitializeComponent();
        }

        public void SetItems(IEnumerable<CheckListViewItem> items)
        {
            _items = new List<CheckListViewItem>(items);
            checkedListView.DataContext = _items;
            checkedListView.ItemsSource = _items;
        }

        private void OnShowSelectedItems(object sender, RoutedEventArgs e)
        {
            StringBuilder items = new StringBuilder();
            items.AppendFormat("Items selected count: {0}", checkedListView.SelectedItems.Count).AppendLine();
            foreach (string item in checkedListView.SelectedItems)
            {
                items.AppendLine(item);
            }
            MessageBox.Show(items.ToString());
        }

        private void OnUncheckItem(object sender, RoutedEventArgs e)
        {
            selectAll.IsChecked = false;
        }

        private void OnSelectAllChanged(object sender, RoutedEventArgs e)
        {
            if (selectAll.IsChecked.HasValue && selectAll.IsChecked.Value)
                checkedListView.SelectAll();
            else
                checkedListView.UnselectAll();
        }

    }
}
