using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Inspect;
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

namespace CD.DLS.Clients.Controls.Dialogs.TreeFilterSelector
{
    /// <summary>
    /// Interaction logic for TreeFilterRootSelector.xaml
    /// </summary>
    public partial class TreeFilterRootSelector : UserControl
    {
        ProjectConfig _config;
        private List<ElementTreeListItem> _items;
        private Dictionary<int, ElementTreeListItem> _itemsById;

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

        public TreeFilterRootSelector()
        {
            InitializeComponent();
            _graphManager = new GraphManager();
            _inspectManager = new InspectManager();
        }

        public event EventHandler SelectionChanged;

        public int? SourceSelectedElementId { get { return (sourceRecursiveTree != null) ? (int?)(sourceRecursiveTree.SelectedItem.Id) : null; } }
        public bool SourceSelected { get { return (sourceRecursiveTree.SelectedItem != null); } }
        public string SourceSelectedElementType { get { return _itemsById[sourceRecursiveTree.SelectedItem.Id].Type; } }
        public string SourceSelectedElementPath { get { return _itemsById[sourceRecursiveTree.SelectedItem.Id].RefPath; } }
        
        public void LoadData(ProjectConfig config, bool sync = false)
        {
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
            
            sourceRecursiveTree.SetData(sourceItems);
            sourceRecursiveTree.SelectedItemChanged += SourceSelectionChanged;
            
        }

        public void SetSourceRootElementId(string refPath)
        {
            // cannot expand the tree to the item - it may not be included in the tree
            var elemId = GraphManager.GetModelElementIdByRefPath(_config.ProjectConfigId, refPath);
            if (!_itemsById.ContainsKey(elemId))
            {
                var elemData = GraphManager.GetModelElementById(elemId);
                _itemsById[elemId] = new ElementTreeListItem()
                { Caption = elemData.Caption, Type = elemData.Type, RefPath = elemData.RefPath };
            }
        }
        
        private void SourceSelectionChanged(object sender, System.EventArgs e)
        {
            FireChange();
        }
        
        private void FireChange()
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new EventArgs());
            }
        }
    }
}
