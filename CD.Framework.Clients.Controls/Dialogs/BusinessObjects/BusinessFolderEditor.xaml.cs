using CD.DLS.API.Structures;
using CD.DLS.Clients.Controls.DataManagers;
using CD.DLS.Clients.Controls.Interfaces;
using CD.DLS.Clients.Controls.Windows;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Identity;
using CD.DLS.DAL.Objects.Inspect;
using CD.DLS.DAL.Receiver;
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

namespace CD.DLS.Clients.Controls.Dialogs.BusinessObjects
{
    public class ElementTreeItemEventArgs : EventArgs
    {
        public ElementTreeListItem Element { get; set; }
    }

    public delegate void ElementTreeItemEventHandler(object sender, ElementTreeItemEventArgs e);

    /// <summary>
    /// Interaction logic for BusinessFolderEditor.xaml
    /// </summary>
    public partial class BusinessFolderEditor : UserControl, ICloseable
    {
        private IReceiver _receiver;
        private ProjectConfig _projectConfig;
        private BusinessObjectsTreeManager _treeManager;
        private List<ElementTreeListItem> _allItems = new List<ElementTreeListItem>();
        private Dictionary<int, ElementTreeListItem> _itemsDictionary = new Dictionary<int, ElementTreeListItem>();
        private Dictionary<int, List<ElementTreeListItem>> _folderChildrenDictionary = new Dictionary<int, List<ElementTreeListItem>>();
        private List<TreeNode> _allTreeNodes = new List<TreeNode>();
        private List<string> _leafTypesFilter = null;

        public BusinessFolderEditor()
        {
            InitializeComponent();
        }

        public event ElementTreeItemEventHandler ItemSelected;
        public event EventHandler Closing;

        public ElementTreeListItem SelectedItem { get
            {
                if (RecursiveTree.SelectedItem == null)
                {
                    return null;
                }
                return _itemsDictionary[RecursiveTree.SelectedItem.Id];
            }
        }

        public ElementTreeListItem SelectedItemParent
        {
            get
            {
                var selectedItem = SelectedItem;
                if (selectedItem == null)
                {
                    return null;
                }
                if (!selectedItem.ParentElementId.HasValue)
                {
                    return null;
                }
                if (!_itemsDictionary.ContainsKey(selectedItem.ParentElementId.Value))
                {
                    return null;
                }

                return _itemsDictionary[selectedItem.ParentElementId.Value];
            }
        }

        #region Properties

        public static readonly DependencyProperty CanAddDeleteEmptyFoldersProperty
    = DependencyProperty.Register(
          "CanAddDeleteEmptyFolders",
          typeof(bool),
          typeof(BusinessFolderEditor),
          new PropertyMetadata(true, OnPropertyChanged)
      );
        
        public static readonly DependencyProperty CanRenameItemsProperty
    = DependencyProperty.Register(
          "CanRenameItems",
          typeof(bool),
          typeof(BusinessFolderEditor),
          new PropertyMetadata(true, OnPropertyChanged)
      );

        public static readonly DependencyProperty CanDeleteContentProperty
    = DependencyProperty.Register(
          "CanDeleteContent",
          typeof(bool),
          typeof(BusinessFolderEditor),
          new PropertyMetadata(false, OnPropertyChanged)
      );
        
        public static readonly DependencyProperty CanSelectContentProperty
    = DependencyProperty.Register(
          "CanSelectContent",
          typeof(bool),
          typeof(BusinessFolderEditor),
          new PropertyMetadata(true, OnPropertyChanged)
      );

        public static readonly DependencyProperty CanSelectFolderProperty
    = DependencyProperty.Register(
          "CanSelectFolder",
          typeof(bool),
          typeof(BusinessFolderEditor),
          new PropertyMetadata(true, OnPropertyChanged)
      );

        public bool CanAddDeleteEmptyFolders
        {
            get { return (bool)GetValue(CanAddDeleteEmptyFoldersProperty); }
            set { SetValue(CanAddDeleteEmptyFoldersProperty, value); }
        }

        public bool CanRenameItems
        {
            get { return (bool)GetValue(CanRenameItemsProperty); }
            set { SetValue(CanRenameItemsProperty, value); }
        }

        public bool CanDeleteContent
        {
            get { return (bool)GetValue(CanDeleteContentProperty); }
            set { SetValue(CanDeleteContentProperty, value); }
        }

        public bool CanSelectContent
        {
            get { return (bool)GetValue(CanSelectContentProperty); }
            set { SetValue(CanSelectContentProperty, value); }
        }

        public bool CanSelectFolder
        {
            get { return (bool)GetValue(CanSelectFolderProperty); }
            set { SetValue(CanSelectFolderProperty, value); }
        }

        #endregion


        public void Init(IReceiver receiver, ProjectConfig projectConfig, List<string> leafTypesFilter = null)
        {
            _receiver = receiver;
            _projectConfig = projectConfig;
            _treeManager = new BusinessObjectsTreeManager(_receiver, ConfigManager.ServiceReceiverId, _projectConfig);

            _allItems = _treeManager.ListBusinessTree();
            
            // filtered leaf types, if leaf type applies
            _leafTypesFilter = leafTypesFilter;
            //_leafTypesFilter.Add("BusinessFolderElement");
            
            RefreshTreeAndDictionaries();
        }

        private void RefreshTreeAndDictionaries()
        {
            _itemsDictionary = _allItems.ToDictionary(x => x.ModelElementId, x => x);

            _folderChildrenDictionary =
                _allItems
                .Where(x => x.IsBusinessFolder)
                .ToDictionary(
                    x => x.ModelElementId,
                    x => _allItems.Where(y => y.ParentElementId == x.ModelElementId
                    ).ToList());

            _allTreeNodes = _allItems.Select(x => new TreeNode()
            {
                Id = x.ModelElementId,
                Name = "[" + x.TypeDescription + "] " + x.Alias,
                ParentId = x.ParentElementId
            }).ToList();

            if (_leafTypesFilter != null)
            {
                _allTreeNodes = _allItems
                    .Where(x => _leafTypesFilter.Any(t => x.Type.Contains(t)))
                    .Select(x => new TreeNode()
                    {
                        Id = x.ModelElementId,
                        Name = "[" + x.TypeDescription + "] " + x.Alias,
                        ParentId = x.ParentElementId
                    }).ToList();
            }

            //foreach (var item in _allTreeNodes)
            //{
            //    // parent not present in the tree - this is one of the roots
            //    if (!_allTreeNodes.Any(x => x.Id == item.ParentId))
            //    {
            //        item.ParentId = null;
            //    }
            //}

            RecursiveTree.SetData(_allTreeNodes);
        }

        public static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }


        private void DisableAllButtons()
        {
            NewFolderButton.IsEnabled = false;
            RenameButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            SelectButton.IsEnabled = false;
            return;
        }

        private void EnableDisableButtons()
        {
            if (RecursiveTree.SelectedItem == null)
            {
                NewFolderButton.IsEnabled = false;
                RenameButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
                SelectButton.IsEnabled = false;
                return;
            }

            var selectedElement = _itemsDictionary[RecursiveTree.SelectedItem.Id];
            if (selectedElement.IsBusinessFolder)
            {
                NewFolderButton.IsEnabled = CanAddDeleteEmptyFolders;
                RenameButton.IsEnabled = CanRenameItems;
                DeleteButton.IsEnabled = CanAddDeleteEmptyFolders;
                SelectButton.IsEnabled = CanSelectFolder;
            }
            else
            {
                NewFolderButton.IsEnabled = false;
                RenameButton.IsEnabled = CanRenameItems;
                DeleteButton.IsEnabled = CanDeleteContent;
                SelectButton.IsEnabled = CanSelectContent;
            }
        }

        private void RecursiveTree_TreeNodeRightClick(Misc.StretchingTreeViewItem sender, MouseButtonEventArgs e)
        {
            ContextMenu cm = this.FindResource("treeNodeContextMenu") as ContextMenu;
            cm.PlacementTarget = sender;

            var selectedElement = _itemsDictionary[RecursiveTree.SelectedItem.Id];
            bool isFolder = selectedElement.IsBusinessFolder;

            ((MenuItem)(cm.Items[0])).IsEnabled = CanAddDeleteEmptyFolders && isFolder;

            ((MenuItem)(cm.Items[1])).IsEnabled = CanRenameItems;

            ((MenuItem)(cm.Items[2])).IsEnabled = (CanDeleteContent && !isFolder) || (CanAddDeleteEmptyFolders && isFolder);
            
            cm.IsOpen = true;
        }
        
        private void RecursiveTree_SelectedItemChanged(object sender, EventArgs e)
        {
            EnableDisableButtons();
        }

        public async Task SavePivotTableTemplateToSelectedNode(PivotTableStructure structure)
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("No folder selected", "No folder selected", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            if (SelectedItem.IsBusinessFolder)
            {
                var takenNames = _folderChildrenDictionary[SelectedItem.ModelElementId].Select(x => x.Caption).ToList();
                NameChooserWindow nameChooser = new NameChooserWindow(takenNames);
                nameChooser.Label = "Enter the name of the pivot table template:";
                var dialogResult = nameChooser.ShowDialog();

                if (dialogResult.HasValue && dialogResult.Value)
                {
                    var origCursor = Mouse.OverrideCursor;
                    Mouse.OverrideCursor = Cursors.Wait;
                    DisableAllButtons();

                    var newItem = await _treeManager.SavePivotTableTemplate(structure, SelectedItem, nameChooser.SelectedName);
                    Mouse.OverrideCursor = origCursor;
                    
                    MessageBox.Show("Template saved successfully.", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    _allItems.Add(newItem);
                    RefreshTreeAndDictionaries();
                    if (Closing != null)
                    {
                        Closing(this, new EventArgs());
                    }
                }

                return;
            }
            // overwrite the item if it is a pivot table
            else if (SelectedItem.IsPivotTableTemplate)
            {
                var messageResult = MessageBox.Show("Do you want to overwrite the template " + SelectedItem.Alias + "?", "Overwrite template", 
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (messageResult == MessageBoxResult.Yes)
                {
                    var origCursor = Mouse.OverrideCursor;
                    Mouse.OverrideCursor = Cursors.Wait;
                    DisableAllButtons();
                    
                    var element = _itemsDictionary[SelectedItem.ModelElementId];
                    await _treeManager.DeleteElement(element);

                    await _treeManager.SavePivotTableTemplate(structure, SelectedItemParent, SelectedItem.Caption);
                    MessageBox.Show("Template saved successfully.", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                    Mouse.OverrideCursor = origCursor;
                    if (Closing != null)
                    {
                        Closing(this, new EventArgs());
                    }
                }

                return;
            }
            else
            {
                return;
            }
        }

        public async Task<PivotTableStructure> LoadPivotTableStructureFromSelectedNode()
        {
            if (SelectedItem == null)
            {
                return null;
            }
            if (!SelectedItem.IsPivotTableTemplate)
            {
                return null;
            }
            var origCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
            DisableAllButtons();

            var res = await _treeManager.GetPivotTableTemplate(SelectedItem.RefPath);

            EnableDisableButtons();
            Mouse.OverrideCursor = origCursor;
            if (Closing != null)
            {
                Closing(this, new EventArgs());
            }
            return res;
        }

        private async void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var treeNode = RecursiveTree.SelectedItem;
            if (treeNode == null)
            {
                return;
            }

            var element = _itemsDictionary[treeNode.Id];
            if (!element.IsBusinessFolder)
            {
                MessageBox.Show("Only folders can contain folders.", "Cannot create folder here", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var takenNames = _folderChildrenDictionary[element.ModelElementId].Select(x => x.Caption).ToList();
            NameChooserWindow nameChooser = new NameChooserWindow(takenNames);
            nameChooser.Label = "Enter the name of the folder:";
            var dialogResult = nameChooser.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                var origCursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = Cursors.Wait;
                DisableAllButtons();

                var newItem = await _treeManager.CreateFolder(nameChooser.SelectedName, element);
                _allItems.Add(newItem);
                RefreshTreeAndDictionaries();
                Mouse.OverrideCursor = origCursor;
            }
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            var treeNode = RecursiveTree.SelectedItem;
            if (treeNode == null)
            {
                return;
            }

            var element = _itemsDictionary[treeNode.Id];

            var identity = IdentityProvider.GetCurrentUser().Identity;
            if (element.RefPath == "Business/SharedFolder" || element.Caption == identity)
            {
                MessageBox.Show("Cannot rename the root folder.", "Cannot rename this folder", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            var takenNames = _folderChildrenDictionary[element.ParentElementId.Value].Select(x => x.Caption).ToList();
            NameChooserWindow nameChooser = new NameChooserWindow(takenNames);
            nameChooser.Label = "Enter the new name of the folder:";
            var dialogResult = nameChooser.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                var selectedName = nameChooser.SelectedName;
                _treeManager.RenameElement(element.ModelElementId, selectedName);
                element.Caption = selectedName;
                element.Alias = selectedName;

                RefreshTreeAndDictionaries();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var treeNode = RecursiveTree.SelectedItem;
            if (treeNode == null)
            {
                return;
            }

            var element = _itemsDictionary[treeNode.Id];

            var identity = IdentityProvider.GetCurrentUser().Identity;
            if (element.RefPath == "Business/SharedFolder" || element.Caption == identity)
            {
                MessageBox.Show("Cannot delete the root folder.", "Cannot delete this folder", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            if (_folderChildrenDictionary.ContainsKey(element.ModelElementId))
            {
                if (_folderChildrenDictionary[element.ModelElementId].Count > 0)
                {
                    MessageBox.Show("Cannot delete this folder - the folder is not empty.", "Cannot delete this folder", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }
            }

            var warningResult = 
                MessageBox.Show("Deleting an item may take a long time, as all the data lineage references to this object have to be cleared. Do you want to delete " + element.Alias + "?",
                "Delete object", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (warningResult != MessageBoxResult.Yes)
            {
                return;
            }

            var origCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
            DisableAllButtons();

            var deleteTask = _treeManager.DeleteElement(element);

            deleteTask.ContinueWith((t) => {
                Dispatcher.Invoke(() =>
                {
                    _allItems.Remove(element);
                    RefreshTreeAndDictionaries();
                    Mouse.OverrideCursor = origCursor;
                });
            });
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            var treeNode = RecursiveTree.SelectedItem;
            if (treeNode == null)
            {
                return;
            }

            var element = _itemsDictionary[treeNode.Id];
            if (element.IsBusinessFolder)
            {
                if (!CanSelectFolder)
                {
                    return;
                }
            }
            else
            {
                if (!CanSelectContent)
                {
                    return;
                }
            }

            ItemSelected(this, new ElementTreeItemEventArgs { Element = element });
        }

        private void TreeNodeContextAddFolder_Click(object sender, RoutedEventArgs e)
        {
            NewFolderButton_Click(sender, e);
        }

        private void TreeNodeContextRename_Click(object sender, RoutedEventArgs e)
        {
            RenameButton_Click(sender, e);
        }

        private void TreeNodeContextDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteButton_Click(sender, e);
        }

        private void RecursiveTree_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectButton_Click(sender, e);
        }
    }
}
