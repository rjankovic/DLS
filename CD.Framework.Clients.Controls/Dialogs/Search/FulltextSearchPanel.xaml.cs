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

namespace CD.DLS.Clients.Controls.Dialogs.Search
{
    /// <summary>
    /// Interaction logic for FulltextSearchPanel.xaml
    /// </summary>
    public partial class FulltextSearchPanel : UserControl
    {
        private Guid _projectConfigId;

        private SearchManager _searchManager;
        private List<DAL.Objects.FulltextSearchResult> _searchResult;
        private bool _isAddSearch;
            
        public FulltextSearchPanel(bool isAddSearch = false)
        {
            InitializeComponent();
            _searchManager = new SearchManager();
            _isAddSearch = isAddSearch;
            /*SearchBar.ExpanderExpanded += SetExpandedLenght;
            SearchBar.ExpanderCollapsed += SetColapsedLenght;*/
        }

        public void LoadData(Guid projectConfigId)
        {
            _projectConfigId = projectConfigId;
            SearchBar.LoadData(_projectConfigId);
        }

        private void SearchBar_SearchBoxSubmitted(object sender, SearchBoxEventArgs e)
        {
            waitingPanel.Visibility = Visibility.Visible;
            Task loadingTask = Task.Factory.StartNew(() => { Search(sender, e); });
            loadingTask.ContinueWith((t) => 
            {
                Dispatcher.Invoke(DisplayData);
            });
        }

        private void DisplayData()
        {
            SearchResults.DisplayData(_searchResult);
            waitingPanel.Visibility = Visibility.Hidden;
        }

        private void Search(object sender, SearchBoxEventArgs e)
        {
            var searchResult = _searchManager.FindFulltext(_projectConfigId, e.SearchPattern, e.RefPathPrefix, e.TypeFilter);
            _searchResult = searchResult;
        }

        public event SearchResultEventHander SearchResultSelected;
        public event SearchResultEventHander AddSearchResultSelected;

        private void SearchResults_ResultSelected(object sender, SearchResultEventArgs e)
        {
            if (!(this.Parent is Xceed.Wpf.AvalonDock.DockingManager))
            {
                Window wnd = (Window)this.Parent;
                wnd.Close();
            }

            if (SearchResultSelected != null)
            {
                SearchResultSelected(sender, e);           
            }
            else
            {
                if (_isAddSearch == true)
                {
                    AddSearchResultSelected(sender, e);
                }
            }       
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void SearchBar_Loaded(object sender, RoutedEventArgs e)
        {

        }

        /*private void SetExpandedLenght(object sender, RoutedEventArgs e)
        {
            GridLength length = new GridLength(150, GridUnitType.Pixel);
            GridLength length1 = new GridLength(1, GridUnitType.Star);
            SearchGrid.RowDefinitions[0].Height = length;
            SearchGrid.RowDefinitions[1].Height = length1;
        }

        private void SetColapsedLenght(object sender, RoutedEventArgs e)
        {
            GridLength length = new GridLength(1, GridUnitType.Star);
            GridLength length1 = new GridLength(1, GridUnitType.Auto);
            SearchGrid.RowDefinitions[0].Height = length1;
            SearchGrid.RowDefinitions[1].Height = length;
        }*/
    }
}
