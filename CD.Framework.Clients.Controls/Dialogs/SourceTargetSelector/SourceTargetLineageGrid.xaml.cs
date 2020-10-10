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
    public class LineageBetweenGoupsTaskSpec
    {
        public int SourceElementId { get; set; }
        public int TargetElementId { get; set; }
        public string SourceElementRefPath { get; set; }
        public string TargetElementRefPath { get; set; }
        public string SourceNodeType { get; set; }
        public string TargetNodeType { get; set; }
        public string SourceElementType { get; set; }
        public string TargetElementType { get; set; }
        public string SourceNodeTypeDescription { get; set; }
        public string TargetNodeTypeDescription { get; set; }

        public bool Equals(LineageBetweenGoupsTaskSpec other)
        {
            if (other == null)
            {
                return false;
            }

            return
                (
                SourceElementId == other.SourceElementId
                && TargetElementId == other.TargetElementId
                && SourceNodeType == other.SourceNodeType
                && TargetNodeType == other.TargetNodeType
                );
        }
    }


    public partial class SourceTargetLineageGrid : UserControl
    {
        private ProjectConfig _config;
        private LineageBetweenGoupsTaskSpec _currentSpec = null;
        private List<DataFlowBetweenGroupsItem> _currentDataFlow = null;

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

        public SourceTargetLineageGrid()
        {
            InitializeComponent();
            
        }

        public DataFlowBetweenGroupsItem SelectedItem { get { return dataGrid.SelectedItem as DataFlowBetweenGroupsItem; } }

        public List<DataFlowBetweenGroupsItem> CurrentDataFlow
        {
            get
            {
                return _currentDataFlow;
            }
        }

        public event EventHandler SelectionChanged;

        public Task LoadData(ProjectConfig config, LineageBetweenGoupsTaskSpec spec)
        {
            if (_graphManager == null)
            {
                _graphManager = new GraphManager();
                _inspectManager = new InspectManager();
            }

            _config = config;
            if (spec.Equals(_currentSpec))
            {
                return null;
            }
            _currentSpec = spec;
            waitingPanel.Visibility = System.Windows.Visibility.Visible;
            Task loadingTask = Task.Factory.StartNew(LoadCurrentSpec);
            loadingTask.ContinueWith((t) => { Dispatcher.Invoke(UpdateGrid); });
            return loadingTask;
        }

        private void LoadCurrentSpec()
        {
            var sourceElem = GraphManager.GetModelElementById(_currentSpec.SourceElementId);
            var targetElem = GraphManager.GetModelElementById(_currentSpec.TargetElementId);

            _currentDataFlow = new List<DataFlowBetweenGroupsItem>();
            foreach (var sourceNodeType in _currentSpec.SourceNodeType.Split(';'))
            {
                foreach (var targetNodeType in _currentSpec.TargetNodeType.Split(';'))
                {
                    _currentDataFlow.AddRange(InspectManager.GetDataFlowBetweenGroupsFlat(_config.ProjectConfigId, sourceElem.RefPath,
                        targetElem.RefPath, sourceNodeType, targetNodeType));
                }
            }

            //_currentDataFlow = InspectManager.GetDataFlowBetweenGroupsFlat(_config.ProjectConfigId, sourceElem.RefPath,
            //    targetElem.RefPath, _currentSpec.SourceNodeType, _currentSpec.TargetNodeType);
        }

        private void UpdateGrid()
        {
            waitingPanel.Visibility = System.Windows.Visibility.Hidden;
            dataGrid.ItemsSource = CurrentDataFlow;
            dataGrid.DataContext = CurrentDataFlow;
            dataGrid.Columns[0].Header = "Source " + _currentSpec.SourceNodeTypeDescription;
            dataGrid.Columns[2].Header = "Target " + _currentSpec.TargetNodeTypeDescription;

            var sourceNodeDescriptivePath = GraphManager.GetModelElementDescriptivePath(_currentSpec.SourceElementId);
            var targetNodeDescriptivePath = GraphManager.GetModelElementDescriptivePath(_currentSpec.TargetElementId);
            //var sourceElement = GraphManager.GetModelElementById(_currentSpec.SourceElementId);
            //var targetElement = GraphManager.GetModelElementById(_currentSpec.TargetElementId);

            GraphManager.SaveLineageGridHistory(_config.ProjectConfigId,
                _currentSpec.SourceElementRefPath,
                _currentSpec.TargetElementRefPath,
                _currentSpec.SourceElementType,
                _currentSpec.TargetElementType,
                _currentSpec.SourceElementId,
                _currentSpec.TargetElementId,
                DAL.Identity.IdentityProvider.GetCurrentUser().UserId);

            statusLabelLeft.Content = "Source: " + sourceNodeDescriptivePath;
            statusLabelRight.Content = "Target: " + targetNodeDescriptivePath;

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
                    var filtered = _currentDataFlow.Where(CurrentDataFlow => CurrentDataFlow.SourceNodeName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentDataFlow.TargetNodeName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
                else
                {
                    var filtered = _currentDataFlow.Where(CurrentDataFlow => CurrentDataFlow.SourceNodeName.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
            }
            if (textboxSourceFilter.Text == "")
            {
                if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
                {
                    var filtered = _currentDataFlow.Where(CurrentDataFlow => CurrentDataFlow.TargetNodeName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
                else
                {
                    dataGrid.ItemsSource = CurrentDataFlow;
                }
            }
        }

        private void textboxTargetFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "" /*&& textboxTargetFilter.Text != targetFilter*/)
            {
                if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                {
                    var filtered = _currentDataFlow.Where(CurrentDataFlow => CurrentDataFlow.TargetNodeName.ToLower().Contains(textboxTargetFilter.Text.ToLower()) & CurrentDataFlow.SourceNodeName.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
                else
                {
                    var filtered = _currentDataFlow.Where(CurrentDataFlow => CurrentDataFlow.TargetNodeName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
            }
            if (textboxTargetFilter.Text == "")
            {
                if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                {
                    var filtered = _currentDataFlow.Where(CurrentDataFlow => CurrentDataFlow.SourceNodeName.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
                else
                {
                    dataGrid.ItemsSource = CurrentDataFlow;
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
                    var filtered = _currentDataFlow.Where(CurrentDataFlow => CurrentDataFlow.SourceNodeName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentDataFlow.TargetNodeName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
                else
                {
                    var filtered = _currentDataFlow.Where(CurrentDataFlow => CurrentDataFlow.TargetNodeName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
            }
            else
            {
                textboxTargetFilter.Text = "";
                if (textboxSourceFilter.Text != "Search..." && textboxSourceFilter.Text != "")
                {
                    var filtered = _currentDataFlow.Where(CurrentDataFlow => CurrentDataFlow.SourceNodeName.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
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
                    var filtered = _currentDataFlow.Where(CurrentDataFlow => CurrentDataFlow.SourceNodeName.ToLower().Contains(textboxSourceFilter.Text.ToLower()) & CurrentDataFlow.TargetNodeName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
                else
                {
                    var filtered = _currentDataFlow.Where(CurrentDataFlow => CurrentDataFlow.SourceNodeName.ToLower().Contains(textboxSourceFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
            }
            else
            {
                textboxSourceFilter.Text = "";
                if (textboxTargetFilter.Text != "Search..." && textboxTargetFilter.Text != "")
                {
                    var filtered = _currentDataFlow.Where(CurrentDataFlow => CurrentDataFlow.TargetNodeName.ToLower().Contains(textboxTargetFilter.Text.ToLower()));
                    dataGrid.ItemsSource = filtered;
                }
            }                
        }
    }
}
