using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Inspect;

namespace CD.DLS.Clients.Controls.Dialogs.SourceTargetSelector
{
    /// <summary>
    /// Interaction logic for WarningGrid.xaml
    /// </summary>
    /// 

    public partial class WarningGrid : UserControl
    {
        private ProjectConfig _config;
        private List<WarningMessagesItem> _currentWarningMessages = null;
        private ProjectConfig config = null;
        private GraphManager _graphManager;
        private InspectManager _inspectManager;

        private GraphManager GraphManager
        {
            get { return _graphManager; }
        }
        private InspectManager InspectManager
        {
            get { return _inspectManager; }
        }

        public WarningGrid()
        {
            InitializeComponent();

            _graphManager = new GraphManager();
            _inspectManager = new InspectManager();

        }

        public WarningMessagesItem SelectedItem { get { return dataGrid.SelectedItem as WarningMessagesItem; } }

        public List<WarningMessagesItem> CurrentWarningMessages
        {
            get
            {
                return _currentWarningMessages;
            }
        }

        public event EventHandler SelectionChanged;

        public Task LoadData(ProjectConfig config)
        {
            _config = config;
            waitingPanel.Visibility = System.Windows.Visibility.Visible;
            Task loadingTask = Task.Factory.StartNew(LoadCurrentSpec);
            loadingTask.ContinueWith((t) => { Dispatcher.Invoke(UpdateGrid); });
            return loadingTask;
        }

        private void LoadCurrentSpec()
        {
            _currentWarningMessages = InspectManager.GetWarningMessages(_config.ProjectConfigId);
        }

        private void UpdateGrid()
        {
            waitingPanel.Visibility = System.Windows.Visibility.Hidden;
            dataGrid.ItemsSource = CurrentWarningMessages;
            dataGrid.DataContext = CurrentWarningMessages;
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new EventArgs());
            }
        }

        private void textboxSourceFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "" /*&& textboxSourceFilter.Text != sourceFilter*/)
            {
                if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
                {
                    var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
                else
                {
                    var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
            }
            if (textboxSourceFilter.Text == "")
            {
                if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
                {
                    if (textboxTypeFilter.Text != "Search..." && textboxTypeFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()) & CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                }
                else
                {
                    if (textboxTypeFilter.Text != "Search..." && textboxTypeFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        dataGrid.ItemsSource = CurrentWarningMessages;
                    }
                }
            }
        }

        private void textboxTargetFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
            {
                if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                {
                    if (textboxTypeFilter.Text != "Search..." && textboxTypeFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()) & CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()) & CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                }
                else
                {
                    if (textboxTypeFilter.Text != "Search..." && textboxTypeFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()) & CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                }
            }
            if (textboxTargetFilter.Text == "")
            {
                if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                {
                    if (textboxTypeFilter.Text != "Search..." && textboxTypeFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                }
                else
                {
                    if (textboxTypeFilter.Text != "Search..." && textboxTypeFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        dataGrid.ItemsSource = CurrentWarningMessages;
                    }
                }
            }
        }

        private void textboxTypeFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textboxTypeFilter.Text != "Search..." && textboxTypeFilter.Text != "")
            {
                if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
                {
                    if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()) & CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()) & CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                }
                else
                {
                    if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()) & CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                }
            }
            if (textboxTypeFilter.Text == "")
            {
                if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
                {
                    if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentDataFlow => CurrentDataFlow.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()) & CurrentDataFlow.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        var filtered = _currentWarningMessages.Where(CurrentDataFlow => CurrentDataFlow.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                }
                else
                {
                    if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentDataFlow => CurrentDataFlow.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        dataGrid.ItemsSource = CurrentWarningMessages;
                    }
                }
            }
        }

        private void textboxSourceFilter_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
            {
                if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
                {
                    if (textboxTypeFilter.Text != "Search..." && textboxTypeFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()) & CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                }
                else
                {
                    if (textboxTypeFilter.Text != "Search..." && textboxTypeFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                }
            }
            else
            {
                textboxSourceFilter.Text = "";
                if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
                {
                    if (textboxTypeFilter.Text != "Search..." && textboxTypeFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()) & CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                }
            }
        }

        private void textboxTargetFilter_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
            {
                if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                {
                    if (textboxTypeFilter.Text != "Search..." && textboxTypeFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()) & CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }          
                }
                else
                {
                    if (textboxTypeFilter.Text != "Search..." && textboxTypeFilter.Text != "")
                    { 
                            var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()) & CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                }
            }
            else
            {
                textboxTargetFilter.Text = "";
                if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                {
                    if (textboxTypeFilter.Text != "Search..." && textboxTypeFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                }
            }
        }

        private void textboxTypeFilter_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (textboxTypeFilter.Text != "Search..." && textboxTypeFilter.Text != "")
            {
                if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
                {
                    if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()) & CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()) & CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                }
                else
                {
                    textboxTargetFilter.Text = "";
                    if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentWarningMessages.DataMessageType.ToLower().Contains(textboxTypeFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                }
            }
            else
            {
                if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
                {
                    if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                    else
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.TargetName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                }
                else
                {
                    textboxTypeFilter.Text = "";
                    if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                    {
                        var filtered = _currentWarningMessages.Where(CurrentWarningMessages => CurrentWarningMessages.SourceName.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                        dataGrid.ItemsSource = filtered;
                    }
                }
            }
        }

        private void textboxSourceFilter_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textboxSourceFilter.Text))
                textboxSourceFilter.Text = "Search...";
        }

        private void textboxTargetFilter_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textboxTargetFilter.Text))
                textboxTargetFilter.Text = "Search...";
        }

        private void textboxTypeFilter_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textboxTypeFilter.Text))
                textboxTypeFilter.Text = "Search...";
        }
    }
}
