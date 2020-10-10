using CD.DLS.Clients.Controls.Dialogs.Misc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    

    public class TreeNode
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int? ParentId { get; set; }
    }

    public class RecursiveTreeNode
    {
        public RecursiveTreeNode()
        {
            Items = new ObservableCollection<RecursiveTreeNode>();
        }
        public TreeNode Value { get; set; }
        public ObservableCollection<RecursiveTreeNode> Items { get; set; }
        
    }
    
    public partial class RecursiveTree : UserControl, INotifyPropertyChanged
    {
        public event EventHandler SelectedItemChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private List<TreeNode> _items = null;

        private TreeNode _selectedItem = null;

        public ObservableCollection<RecursiveTreeNode> Hierarchy { get; set; }
        
        public event TreeNodeMouseButtonEventHandler TreeNodeRightClick;
        public event MouseButtonEventHandler DoubleClick;
        
        private string _filter = null;

        public TreeNode SelectedItem
        {
            get { return _selectedItem; }
        }
        private ListCollectionView _view = null;

        private System.Windows.Threading.DispatcherTimer _filterTimer = new System.Windows.Threading.DispatcherTimer();

        public bool ExpandAll { get; set; }

        public RecursiveTree()
        {
            InitializeComponent();
            filterTextBox.Visibility = SearchVisible ? Visibility.Visible : Visibility.Hidden;
            
            ExpandAll = false;
            OnPropertyChanged("ExpandAll");

            _filterTimer.Tick += UpdateFilter;
            _filterTimer.Interval = new TimeSpan(0, 0, 1);

            treeView.TreeNodeRightClick += TreeView_TreeNodeRightClick;
            treeView.MouseDoubleClick += TreeView_MouseDoubleClick;
        }

        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DoubleClick != null)
            {
                DoubleClick(sender, e);
            }
        }

        private void TreeView_TreeNodeRightClick(StretchingTreeViewItem sender, MouseButtonEventArgs e)
        {
            if (TreeNodeRightClick != null)
            {
                TreeNodeRightClick(sender, e);
            }
        }

        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public bool SearchVisible
        {
            get { return (bool)GetValue(SearchVisibleProperty); }
            set { SetValue(SearchVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Property1.  
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchVisibleProperty
            = DependencyProperty.Register(
                  "SearchVisible",
                  typeof(bool),
                  typeof(RecursiveTree),
                  new PropertyMetadata(true)
              );

        public void SetData(List<TreeNode> items)
        {
            var origCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
            _items = items;
            Hierarchy = Hierarchize(items);

            this.DataContext = this;
            treeView.ItemsSource = Hierarchy;
            treeView.InvalidateVisual();
            //treeView.ItemsSource = _hierarchy;
            Mouse.OverrideCursor = origCursor;
        }

        private ObservableCollection<RecursiveTreeNode> Hierarchize(List<TreeNode> items, RecursiveTreeNode parent = null)
        {
            var roots = items.Where(x => x.ParentId == (parent == null ? (int?)null : parent.Value.Id)).OrderBy(x => x.Name).ToList();
            //if (roots.Count == 0)
            //{
            //    roots = items.Where(x => !items.Any(y => y.Id == x.ParentId)).OrderBy(x => x.Name).ToList();
            //}
            ObservableCollection<RecursiveTreeNode> res = new ObservableCollection<RecursiveTreeNode>();
            foreach (var r in roots)
            {
                var node = new RecursiveTreeNode()
                {
                    Value = r
                };
                node.Items = Hierarchize(items, node);
                res.Add(node);
            }
            return res;
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public void SetSelectedItem(int key)
        {
            var node = _items.FirstOrDefault(x => x.Id == key);
            if (node == null)
            {
                return;
            }
            var item = treeView.ItemContainerGenerator.ContainerFromItem(node);
            item.SetValue(TreeViewItem.IsSelectedProperty, true);
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedItem = null;
            var recNode = treeView.SelectedItem as RecursiveTreeNode;
            if (recNode != null)
            {
                _selectedItem = recNode.Value;
            }
            
            if (_selectedItem != null && SelectedItemChanged != null)
            {
                SelectedItemChanged(this, new EventArgs());
            }
        }

        public void RemoveFilterPaceholder(object sender, EventArgs e)
        {
            if(filterTextBox.Text == "Search...")
            {
                filterTextBox.Text = "";
            }          
        }

        public void AddFilterPaceholder(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(filterTextBox.Text))
                filterTextBox.Text = "Search...";
        }

        private void Expand(StretchingTreeViewItem item)
        {
            //item.IsExpanded = true;
            foreach (var ch in item.Items)
            {
                    StretchingTreeViewItem child = ch as StretchingTreeViewItem;

                    if (child == null)
                    {
                        var x = item.ItemContainerGenerator.ContainerFromItem(ch);
                        child = item.ItemContainerGenerator.ContainerFromItem(ch) as StretchingTreeViewItem;
                    }

                    if (child is StretchingTreeViewItem)
                    {
                        Expand((StretchingTreeViewItem)child);
                    }
            }
        }

        private void Collapse(StretchingTreeViewItem item)
        {
            item.IsExpanded = false;
            foreach (var ch in item.Items)
            {
                StretchingTreeViewItem child = ch as StretchingTreeViewItem;

                if (child == null)
                {
                    child = item.ItemContainerGenerator.ContainerFromItem(ch) as StretchingTreeViewItem;
                }

                if (child is StretchingTreeViewItem)
                {
                    Collapse((StretchingTreeViewItem)child);
                }
            }
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _filterTimer.Start();
        }

        private void UpdateFilter(object sender, EventArgs e)
        {
            _filterTimer.Stop();

            var newFilter = filterTextBox.Text.Trim();
            if (newFilter == "Search..." || string.IsNullOrWhiteSpace(newFilter))
            {
                if (_filter != null)
                {
                    _filter = null;
                    Hierarchy.Clear();
                    var nh = Hierarchize(_items);
                    foreach (var nhi in nh)
                    {
                        Hierarchy.Add(nhi);
                    }
                    
                    ExpandAll = false;
                    OnPropertyChanged("ExpandAll");
                }

                return;
            }
            if (_filter != newFilter)
            {
                _filter = newFilter;

                Hierarchy.Clear();
                var filteredItems = GetFilteredNodes();
                var nh = Hierarchize(filteredItems);
                foreach (var nhi in nh)
                {
                    Hierarchy.Add(nhi);
                }

                if (!string.IsNullOrWhiteSpace(newFilter))
                {
                    ExpandAll = true;
                    OnPropertyChanged("ExpandAll");
                }

                _selectedItem = null;
                if (SelectedItemChanged != null)
                {
                    SelectedItemChanged(this, new EventArgs());
                }
            }
            _filter = newFilter;
            
        }


        private List<TreeNode> GetFilteredNodes()
        {
            List<TreeNode> currentPass = _items.Where(x => x.Name.IndexOf(_filter, StringComparison.InvariantCultureIgnoreCase) > 0).ToList();
            HashSet<TreeNode> passedNodes = new HashSet<TreeNode>(currentPass);
            var itemDictionary = _items.ToDictionary(x => x.Id, x => x);
            while (currentPass.Any())
            {
                var propagatedAncestors = currentPass.Where(x => x.ParentId.HasValue).Select(x => itemDictionary[x.ParentId.Value]).ToList();
                foreach (var propAnc in propagatedAncestors)
                {
                    passedNodes.Add(propAnc);
                }
                currentPass = propagatedAncestors;
            }
            List<TreeNode> res = new List<TreeNode>();
            foreach (var item in _items)
            {
                if (passedNodes.Contains(item))
                {
                    res.Add(item);
                }
            }
            return res;
        }
    }
}
