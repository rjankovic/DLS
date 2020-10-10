using CD.DLS.DAL.Managers;
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
    /// <summary>
    /// Interaction logic for FulltextSearchBox.xaml
    /// </summary>
    public partial class FulltextSearchBar : UserControl
    {

        private List<SearchRootElement> _rootElements;
        private List<SearchParentChildTypeMapping> _parentChildTypes;
        private Guid _projectConfigId;

        private AnnotationManager _annotationManager;
        private SearchManager _searchManager;
        private GraphManager _graphManager;
        private InspectManager _inspectManager;

        public event RoutedEventHandler ExpanderCollapsed;
        public event RoutedEventHandler ExpanderExpanded;

        private AnnotationManager AnnotationManager
        {
            get { return _annotationManager; }
        }
        private SearchManager SearchManager
        {
            get { return _searchManager; }
        }
        private GraphManager GraphManager
        {
            get { return _graphManager; }
        }
        private InspectManager InspectManager
        {
            get { return _inspectManager; }
        }


        public FulltextSearchBar()
        {
            InitializeComponent();
            _annotationManager = new AnnotationManager();
            _searchManager = new SearchManager();
            _graphManager = new GraphManager();
            _inspectManager = new InspectManager();
        }

        public void LoadData(Guid projectConfigId)
        {
            _projectConfigId = projectConfigId;
            _rootElements = SearchManager.GetRootElements(_projectConfigId);
            _rootElements.Insert(0, new SearchRootElement()
            {
                Caption = "Select All",
                ElementType = null,
                ModelElementId = -1,
                RefPath = ""
            });
            _parentChildTypes = SearchManager.GetParentChildTypeMapping();
            
            ComponentList.ItemsSource = _rootElements; 

            TypeList.ItemsSource = _parentChildTypes;
            SetSelectedTypes();

            /*Expander.Expanded += ExpanderExpanded;
            Expander.Collapsed += ExpanderCollapsed;*/
        }
        
        public void SelectAll(object sender, RoutedEventArgs e)
        {
            if (_parentChildTypes == null)
            {
                return;
            }

            //TypeList.SelectAll();
            foreach (SearchParentChildTypeMapping si in _parentChildTypes)
            {
                ListBoxItem myListBoxItem = (ListBoxItem)(TypeList.ItemContainerGenerator.ContainerFromItem(si));
                ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(myListBoxItem);
                DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
                CheckBox myCb = (CheckBox)myDataTemplate.FindName("TypeCheckBox", myContentPresenter);
                //bool selectedCb = false;
                //if (myCb.IsChecked.HasValue)
                //{
                //    if (myCb.IsChecked.Value)
                //    {
                //        selectedCb = true;
                //    }
                //    }
                     //bool selectedItem = TypeList.SelectedItems.Contains(si);
                     
                         myCb.IsChecked = true;
                
                    

                
            }
            return;

        }
        
            public void UnselectAll(object sender, RoutedEventArgs e)
        {
            if (_parentChildTypes == null)
            {
                return;
            }

            //TypeList.SelectAll();
            foreach (SearchParentChildTypeMapping si in _parentChildTypes)
            {
                ListBoxItem myListBoxItem = (ListBoxItem)(TypeList.ItemContainerGenerator.ContainerFromItem(si));
                ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(myListBoxItem);
                DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
                CheckBox myCb = (CheckBox)myDataTemplate.FindName("TypeCheckBox", myContentPresenter);
                //bool selectedCb = false;
                //if (myCb.IsChecked.HasValue)
                //{
                //    if (myCb.IsChecked.Value)
                //    {
                //        selectedCb = false;
                //    }
                //}
                //bool selectedItem = TypeList.SelectedItems.Contains(si);

                //if (selectedCb && !selectedItem)
                //{
                //    myCb.IsChecked = true;
                //}
                //if (!selectedCb && selectedItem)
                //{
                //    myCb.IsChecked = false;


                //}

                myCb.IsChecked = false;



            }
            return;
            
        }


        private void SetSelectedTypes()
        {
            if (ComponentList.SelectedItem == null)
            {
                //TypeList.SelectAll();
                return;
            }

            TypeList.UnselectAll();
            SearchRootElement selectedComponent = (SearchRootElement)(ComponentList.SelectedItem);
            var availableComponents = _parentChildTypes.Where(x => x.ParentType == selectedComponent.ElementType);
            if (selectedComponent.ElementType == null)
            {
                availableComponents = _parentChildTypes.Distinct();
            }

            foreach (var availableComponent in availableComponents)
            {
                TypeList.SelectedItems.Add(availableComponent);
            }

            // synchronize the checkboxes with the checked list items
            foreach (SearchParentChildTypeMapping si in _parentChildTypes)
            {
                ListBoxItem myListBoxItem =
                    (ListBoxItem)(TypeList.ItemContainerGenerator.ContainerFromItem(si));

                // Getting the ContentPresenter of myListBoxItem
                ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(myListBoxItem);

                // Finding textBlock from the DataTemplate that is set on that ContentPresenter
                DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
                CheckBox myCb = (CheckBox)myDataTemplate.FindName("TypeCheckBox", myContentPresenter);
                bool selectedCb = false;
                if (myCb.IsChecked.HasValue)
                {
                    if (myCb.IsChecked.Value)
                    {
                        selectedCb = true;
                    }
                }
                bool selectedItem = TypeList.SelectedItems.Contains(si);

                if (selectedCb && !selectedItem)
                {
                    myCb.IsChecked = false;
                }
                if (!selectedCb && selectedItem)
                {
                    myCb.IsChecked = true;
                }
                
            }
        }

        public event SearchBoxEventHander SearchBoxSubmitted;

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SubmitIfNotEmpty();
            }
        }

        private void SubmitIfNotEmpty()
        {
            if (!string.IsNullOrWhiteSpace(searchBox.Text))
            {
                if (SearchBoxSubmitted != null)
                {
                    List<string> typeFilter = new List<string>();

                    foreach (SearchParentChildTypeMapping si in _parentChildTypes)
                    {
                        ListBoxItem myListBoxItem = (ListBoxItem)(TypeList.ItemContainerGenerator.ContainerFromItem(si));
                        if (myListBoxItem == null)
                        {
                            typeFilter.Add(si.ChildType);
                        }
                        else
                        {
                            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(myListBoxItem);
                            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
                            CheckBox myCb = (CheckBox)myDataTemplate.FindName("TypeCheckBox", myContentPresenter);
                            if (myCb.IsChecked.HasValue)
                            {
                                if (myCb.IsChecked.Value)
                                {
                                    typeFilter.Add(si.ChildType);
                                }
                            }
                        }
                    }

                            SearchBoxSubmitted(this, new SearchBoxEventArgs()
                    {
                        SearchPattern = searchBox.Text,
                        RefPathPrefix = ComponentList.SelectedItem == null ? null : ((SearchRootElement)(ComponentList.SelectedItem)).RefPath,
                        TypeFilter = typeFilter
                        //TypeList.SelectedItems.OfType<SearchParentChildTypeMapping>().Select(x => x.ChildType).ToList()
                    });                   
                }
            }
            
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            SubmitIfNotEmpty();
        }

        private void ComponentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // make radiobutton behind the currently selected item checked
            foreach (SearchRootElement si in ComponentList.SelectedItems)
            {
                ListBoxItem myListBoxItem =
                    (ListBoxItem)(ComponentList.ItemContainerGenerator.ContainerFromItem(si));
                
                // Getting the ContentPresenter of myListBoxItem
                ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(myListBoxItem);

                // Finding textBlock from the DataTemplate that is set on that ContentPresenter
                DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
                RadioButton myRb = (RadioButton)myDataTemplate.FindName("ComponentRadioButton", myContentPresenter);
                if (!myRb.IsChecked.HasValue)
                {
                    myRb.IsChecked = true;
                    continue;
                }
                if (!myRb.IsChecked.Value)
                {
                    myRb.IsChecked = true;
                    continue;
                }
            }
        
            SetSelectedTypes();
        }

        private void TypeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (!TypeList.SelectedItems.Contains(((CheckBox)sender).DataContext))
            {
                TypeList.SelectedItems.Add(((CheckBox)sender).DataContext);
            }
        }

        private void TypeCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (TypeList.SelectedItems.Contains(((CheckBox)sender).DataContext))
            {
                TypeList.SelectedItems.Remove(((CheckBox)sender).DataContext);
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!ComponentList.SelectedItems.Contains(((RadioButton)sender).DataContext))
            {
                ComponentList.SelectedItem = ((RadioButton)sender).DataContext;
            }
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj)
    where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }

    public class SearchBoxEventArgs : EventArgs
    {
        public string SearchPattern { get; set; }
        public string RefPathPrefix { get; set; }
        public List<string> TypeFilter { get; set; }
    }

    public delegate void SearchBoxEventHander(object sender, SearchBoxEventArgs e);
    
}
