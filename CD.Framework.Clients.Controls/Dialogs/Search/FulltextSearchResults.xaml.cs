using CD.DLS.DAL.Objects;
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

namespace CD.DLS.Clients.Controls.Dialogs.Search
{



    public class SearchResultEventArgs : EventArgs
    {
        public FulltextSearchResult SelectedResult { get; set; }
    }

    public delegate void SearchResultEventHander(object sender, SearchResultEventArgs e);
    /// <summary>
    /// Interaction logic for FulltextSearchResults.xaml
    /// </summary>
    public partial class FulltextSearchResults : UserControl
    {
        private List<FulltextSearchResult> _data;
        
        public FulltextSearchResults()
        {
            InitializeComponent();
        }

        public event SearchResultEventHander ResultSelected;

        public void DisplayData(List<FulltextSearchResult> data)
        {
            _data = data;
            listBox.ItemsSource = _data;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = listBox.SelectedItem as FulltextSearchResult;
            if (selected != null)
            {
                /*if (ResultSelected != null)
                {
                    listBox.UnselectAll();
                    ResultSelected(this, new SearchResultEventArgs() { SelectedResult = selected });
                }*/
            }

        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = listBox.SelectedItem as FulltextSearchResult;
            if (selected != null)
            {
                if (ResultSelected != null)
                {
                    listBox.UnselectAll();
                    ResultSelected(this, new SearchResultEventArgs() { SelectedResult = selected });
                }
            }           
        }
    }
}
