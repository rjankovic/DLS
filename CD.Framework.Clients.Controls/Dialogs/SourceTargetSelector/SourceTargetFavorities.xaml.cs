using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BIDoc;
using CD.DLS.DAL.Objects.Inspect;

namespace CD.DLS.Clients.Controls.Dialogs.SourceTargetSelector
{
    public partial class SourceTargetFavorities : UserControl
    {
        private ProjectConfig _config;
        private List<LineageGridFavorite> _favoritiesList = null;
        
        private GraphManager _graphManager;
        
        private GraphManager GraphManager
        {
            get { return _graphManager; }
        }

        public SourceTargetFavorities()
        {
            InitializeComponent();
            
        }

        public LineageGridFavorite SelectedItem { get { return dataGrid.SelectedItem as LineageGridFavorite; } }
        
        public event EventHandler SelectionChanged;

        public Task LoadData(ProjectConfig config)
        {
            if (_graphManager == null)
            {
                _graphManager = new GraphManager();
            }

            _config = config;
            waitingPanel.Visibility = System.Windows.Visibility.Visible;
            Task loadingTask = Task.Factory.StartNew(LoadFavorities);
            loadingTask.ContinueWith((t) => { Dispatcher.Invoke(UpdateGrid); });
            return loadingTask;
        }

        private void LoadFavorities()
        {
            _favoritiesList = GraphManager.GetLineageGridFavorites(_config.ProjectConfigId);
        }

        private void UpdateGrid()
        {
            waitingPanel.Visibility = System.Windows.Visibility.Hidden;
            dataGrid.ItemsSource = _favoritiesList;
            dataGrid.DataContext = _favoritiesList;
        }

        //private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (SelectionChanged != null)
        //    {
        //        SelectionChanged(this, new EventArgs());
        //    }
        //}

        private void textboxSourceFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "" /*&& textboxSourceFilter.Text != sourceFilter*/)
            {
                if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
                {
                    var filtered = _favoritiesList.Where(x => x.SourceRootDescriptivePath.ToLower().Contains(textboxSourceFilter.Text.ToLower()) && x.TargetRootDescriptivePath.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
                else
                {
                    var filtered = _favoritiesList.Where(x => x.SourceRootDescriptivePath.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
            }
            if (textboxSourceFilter.Text == "")
            {
                if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
                {
                    var filtered = _favoritiesList.Where(x => x.TargetRootDescriptivePath.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
                else
                {
                    dataGrid.ItemsSource = _favoritiesList;
                }
            }
        }

        private void textboxTargetFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "" /*&& textboxTargetFilter.Text != targetFilter*/)
            {
                if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                {
                    var filtered = _favoritiesList.Where(x => x.TargetRootDescriptivePath.ToLower().Contains(textboxTargetFilter.Text.ToLower()) && x.SourceRootDescriptivePath.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
                else
                {
                    var filtered = _favoritiesList.Where(x => x.TargetRootDescriptivePath.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
            }
            if (textboxTargetFilter.Text == "")
            {
                if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                {
                    var filtered = _favoritiesList.Where(x => x.TargetRootDescriptivePath.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
                else
                {
                    dataGrid.ItemsSource = _favoritiesList;
                }
            }
        }

        private void textboxTargetFilter_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textboxTargetFilter.Text))
                textboxTargetFilter.Text = "Search...";   
        }

        private void textboxSourceFilter_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textboxSourceFilter.Text))
                textboxSourceFilter.Text = "Search...";
        }

        private void textboxTargetFilter_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
            {
                if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                {
                    var filtered = _favoritiesList.Where(x => x.SourceRootDescriptivePath.ToLower().Contains(textboxSourceFilter.Text.ToLower()) && x.TargetRootDescriptivePath.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
                else
                {
                    var filtered = _favoritiesList.Where(x => x.TargetRootDescriptivePath.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
            }
            else
            {
                textboxTargetFilter.Text = "";
                if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                {
                    var filtered = _favoritiesList.Where(x => x.SourceRootDescriptivePath.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
            }
        }

        private void textboxSourceFilter_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if(textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
            {
                if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
                {
                    var filtered = _favoritiesList.Where(x => x.SourceRootDescriptivePath.ToLower().Contains(textboxSourceFilter.Text.ToLower()) && x.TargetRootDescriptivePath.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
                else
                {
                    var filtered = _favoritiesList.Where(x => x.SourceRootDescriptivePath.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
            }
            else
            {
                textboxSourceFilter.Text = "";
                if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
                {
                    var filtered = _favoritiesList.Where(x => x.TargetRootDescriptivePath.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
            }                
        }
        
        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new EventArgs());
            }
        }
    }
}
