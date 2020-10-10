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
    /// <summary>
    /// Interaction logic for ListPicker.xaml
    /// </summary>
    public partial class ListPicker : UserControl
    {
        public event EventHandler Selected;

        private ListPickerItem _selectedItem = null;
        public ListPickerItem SelectedItem
        {
            get { return _selectedItem; }
        }
        private ListCollectionView _view = null;

        private bool _searchbarVisible = true;

        public ListPicker()
        {
            InitializeComponent();
        }

        public void Init(List<ListPickerItem> items, bool searchbarVisible = true)
        {
            _searchbarVisible = searchbarVisible;
            filterTextBox.Visibility = searchbarVisible ? Visibility.Visible : Visibility.Collapsed;

            //var origCursor = Mouse.OverrideCursor;
            //Mouse.OverrideCursor = Cursors.Wait;
            _view = new ListCollectionView(items);

            gridReports.ItemsSource = _view;
            this.InvalidateVisual();
            
            //Mouse.OverrideCursor = origCursor;
        }

        public class ListPickerItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Tooltip { get; set; }
        }

        private void ItemLink_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is Hyperlink)
            {
                var context = ((Hyperlink)e.Source).DataContext;
                if (context is ListPickerItem)
                {
                    _selectedItem = (ListPickerItem)context;
                    Selected?.Invoke(this, new EventArgs());
                }
            }
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(this.Parent is Xceed.Wpf.AvalonDock.DockingManager))
            {
                Window wnd = (Window)this.Parent;
                wnd.Close();
            }
            
            DataGridRow row = sender as DataGridRow;
            if (row == null)
            {
                return;
            }
            var context = row.DataContext as ListPickerItem;
            if (context == null)
            {
                return;
            }
            _selectedItem = context;
            Selected?.Invoke(this, new EventArgs());
        }

        public void RemoveFilterPaceholder(object sender, EventArgs e)
        {
            filterTextBox.Text = "";
        }

        public void AddFilterPaceholder(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(filterTextBox.Text))
                filterTextBox.Text = "Search...";
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_view == null)
            {
                return;
            }

            var filter = filterTextBox.Text;
            if (filter == "Search..." || filter == "")
            {
                _view.Filter = x => true;
                return;
            }
            _view.Filter = x => ((ListPickerItem)x).Name.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }


    }
}
