using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Inspect;

namespace CD.DLS.Clients.Controls.Dialogs.SourceTargetSelector
{
    /// <summary>
    /// Interaction logic for AnnotationList.xaml
    /// </summary>
    public partial class SourceTargetRootSelector : UserControl
    {
        ProjectConfig _config;
        private List<ElementTreeListItem> _items;
        private Dictionary<int, ElementTreeListItem> _itemsById;
        private int? _sourceIdByPath = null;
        private int? _targetIdByPath = null;

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

        public SourceTargetRootSelector()
        {
            InitializeComponent();
            
        }

        public event EventHandler SelectionChanged;
        
        public int? SourceSelectedElementId { get { return (sourceRecursiveTree != null && _sourceIdByPath == null) ? sourceRecursiveTree.SelectedItem.Id : _sourceIdByPath; } }
        public int? TargetSelectedElementId { get { return (targetRecursiveTree != null && _targetIdByPath == null) ? targetRecursiveTree.SelectedItem.Id : _targetIdByPath; } }
        public bool SourceAndTargetSelected { get { return (sourceRecursiveTree.SelectedItem != null || _sourceIdByPath != null) && (targetRecursiveTree.SelectedItem != null || _targetIdByPath != null); } }
        public string SourceSelectedElementType { get { return (sourceRecursiveTree != null && _sourceIdByPath == null) ? _itemsById[sourceRecursiveTree.SelectedItem.Id].Type : (_sourceIdByPath == null ? null : _itemsById[_sourceIdByPath.Value].Type); } }
        public string TargetSelectedElementType { get { return (targetRecursiveTree != null && _targetIdByPath == null) ? _itemsById[targetRecursiveTree.SelectedItem.Id].Type : (_targetIdByPath == null ? null : _itemsById[_targetIdByPath.Value].Type); } }
        public string SourceSelectedElementPath { get { return (sourceRecursiveTree != null && _sourceIdByPath == null) ? _itemsById[sourceRecursiveTree.SelectedItem.Id].RefPath : (_sourceIdByPath == null ? null : _itemsById[_sourceIdByPath.Value].RefPath); } }
        public string TargetSelectedElementPath { get { return (targetRecursiveTree != null && _targetIdByPath == null) ? _itemsById[targetRecursiveTree.SelectedItem.Id].RefPath : (_targetIdByPath == null ? null : _itemsById[_targetIdByPath.Value].RefPath); } }

        public void LoadData(ProjectConfig config, bool sync = false)
        {
            if (_graphManager == null)
            {
                _graphManager = new GraphManager();
                _inspectManager = new InspectManager();
            }

            _config = config;

            if (sync)
            {
                LoadCurrentSpec();
                DisplayData();
                return;
            }

            Task loadingTask = Task.Factory.StartNew(LoadCurrentSpec);
            loadingTask.ContinueWith((t) => { Dispatcher.Invoke(DisplayData); });
            
        }

        private void LoadCurrentSpec()
        {
            var highLevelTree = InspectManager.GetHighLevelSolutionTree(_config.ProjectConfigId);
            _items = highLevelTree;
        }

        public void DisplayData()
        {
            waitingPanel.Visibility = System.Windows.Visibility.Hidden;
            _itemsById = _items.ToDictionary(x => x.ModelElementId, x => x);

            var sourceItems = _items.Select(x => new TreeNode() { Id = x.ModelElementId, Name = "[" + x.TypeDescription + "] " + x.Caption, ParentId = x.ParentElementId }).ToList();
            var targetItems = _items.Select(x => new TreeNode() { Id = x.ModelElementId, Name = "[" + x.TypeDescription + "] " + x.Caption, ParentId = x.ParentElementId }).ToList();

            sourceRecursiveTree.SetData(sourceItems);
            targetRecursiveTree.SetData(targetItems);
            sourceRecursiveTree.SelectedItemChanged += SourceSelectionChanged;
            targetRecursiveTree.SelectedItemChanged += TargetSelectionChanged;
        }

        public void SetSourceRootElementPath(string refPath)
        {
            // cannot expand the tree to the item - it may not be included in the tree

            //var sourceItem = _itemsById[elementId];
            sourceRefPathTb.Text = refPath;

            var elemId = GraphManager.GetModelElementIdByRefPath(_config.ProjectConfigId, refPath);
            if (!_itemsById.ContainsKey(elemId))
            {
                var elemData = GraphManager.GetModelElementById(elemId);
                _itemsById[elemId] = new ElementTreeListItem()
                { Caption = elemData.Caption, Type = elemData.Type, RefPath = elemData.RefPath };
            }

            sourceRefPathButton_Click(null, null);
            //sourceRecursiveTree.SetSelectedItem(elementId);
        }

        public void SetTargetRootElementPath(string refPath)
        {
            //var targetItem = _itemsById[elementId];
            targetRefPathTb.Text = refPath;

            var elemId = GraphManager.GetModelElementIdByRefPath(_config.ProjectConfigId, refPath);
            if (!_itemsById.ContainsKey(elemId))
            {
                var elemData = GraphManager.GetModelElementById(elemId);
                _itemsById[elemId] = new ElementTreeListItem()
                { Caption = elemData.Caption, Type = elemData.Type, RefPath = elemData.RefPath };
            }

            targetRefPathButton_Click(null, null);
            //targetRecursiveTree.SetSelectedItem(elementId);
        }

        private void TargetSelectionChanged(object sender, System.EventArgs e)
        {
            _targetIdByPath = null;
            if (targetRecursiveTree.SelectedItem is TreeNode)
            {
                targetRefPathTb.Text = _itemsById[targetRecursiveTree.SelectedItem.Id].RefPath;
            }
            FireChange();
        }

        private void SourceSelectionChanged(object sender, System.EventArgs e)
        {
            _sourceIdByPath = null;
            if (sourceRecursiveTree.SelectedItem is TreeNode)
            {
                sourceRefPathTb.Text = _itemsById[sourceRecursiveTree.SelectedItem.Id].RefPath;
            }
            FireChange();
        }

        private void targetRefPathButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var targetId = GraphManager.TryGetModelElementIdByRefPath(_config.ProjectConfigId, targetRefPathTb.Text);
            if (targetId == null)
            {
                System.Windows.MessageBox.Show(string.Format("Element \"{0}\" could not be found.", targetRefPathTb.Text), "Element not found", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
            }
            _targetIdByPath = targetId;
            FireChange();
        }

        private void sourceRefPathButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var sourceId = GraphManager.TryGetModelElementIdByRefPath(_config.ProjectConfigId, sourceRefPathTb.Text);
            if (sourceId == null)
            {
                System.Windows.MessageBox.Show(string.Format("Element \"{0}\" could not be found.", sourceRefPathTb.Text), "Element not found", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
            }
            _sourceIdByPath = sourceId;
            FireChange();
        }

        private void FireChange()
        {
            if (SelectionChanged != null)
            {
                //if (SourceAndTargetSelected)
                //{
                    SelectionChanged(this, new EventArgs());
                //}
            }
        }
    }
}
