using CD.DLS.Clients.Controls.Dialogs.BusinessDictionaryAdmin;
using CD.DLS.Clients.Controls.Dialogs.Search;
using CD.DLS.DAL.Identity;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Inspect;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace CD.DLS.Clients.Controls.Dialogs.ElementView
{

    public class BusinessViewLinkClickedArgs : EventArgs
    {
        public int LinkedElementId { get; set; }
        public string LinkedElementName { get; set; }
    }
    public delegate void BusinessViewLinkClickedHander(object sender, BusinessViewLinkClickedArgs e);


    /// <summary>
    /// Interaction logic for ElementBusinessView.xaml
    /// </summary>
    public partial class ElementBusinessView : UserControl
    {
        // node that is being loaded
        private int _currentElementId = -1;
        // node that has been loaded
        private int _displayedElementId = -1;
        private Guid _projectId;
        private int _viewId = 0;
        private List<AnnotationViewField> _viewFields = null;
        private Dictionary<int, TextBox> _textBoxMap = new Dictionary<int, TextBox>();
        private Dictionary<int, Label> _labelMap = new Dictionary<int, Label>();
        private List<AnnotationViewFieldValue> _fieldValues = new List<AnnotationViewFieldValue>();
        private bool click = false;
        private DataTable _currentTable;
        private Exception _exception = null;
        private List<AnnotationLinkFromTo> _linkFrom;
        private List<AnnotationLinkFromTo> _linkTo;
        private AnnotatedDependencySet _annotatedDependencySet = null;
        //private List<AnnotationLinkFromTo> _links;
        private bool isFrom;
        private int _selectedLinkType;
        //private string _modelElementType;
        //private string _supportedType;

        private AnnotationManager _annotationManager;
        private SearchManager _searchManager;
        private GraphManager _graphManager;
        private InspectManager _inspectManager;
        private SecurityManager _securityManager;
        
        public event AddLinkEventHandler AddElementClicked;
        public event BusinessViewLinkClickedHander BusinessViewLinkClicked;
        
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
        private SecurityManager SecurityManager
        {
            get { return _securityManager; }
        }

        public bool Editable { get; set; }

        private System.Windows.Window ShowWindow(ContentControl control, string title, bool dialog = false)
        {
            System.Windows.Window window = new System.Windows.Window
            {
                Title = title,
                Content = control
            };

            if (dialog)
            {
                window.ShowDialog();
            }
            else
            {
                window.Show();
            }
            return window;
        }

        public ElementBusinessView()
        {
            InitializeComponent();

            _annotationManager = new AnnotationManager();
            _searchManager = new SearchManager();
            _graphManager = new GraphManager();
            _inspectManager = new InspectManager();
            _securityManager = new SecurityManager();

            waitingPanel.Visibility = Visibility.Hidden;
            fieldsGrid.Visibility = Visibility.Hidden;
        }

        public void GetViewFields(Guid projectId, string elementType)
        {
            _projectId = projectId;

            var views = AnnotationManager.ListProjectViews(_projectId);
            var origViewId = _viewId;
            _viewId = views.First(x => x.ViewName == "Default").AnnotationViewId;
            var typeView = views.FirstOrDefault(x => x.ViewName == "Type_" + elementType);
            if (typeView != null)
            {
                _viewId = typeView.AnnotationViewId;
            }

            if (_viewId != origViewId)
            {
                if (_viewFields != null)
                {
                    var oldFieldCount = _viewFields.Count;

                    foreach (var tb in _textBoxMap.Values)
                    {
                        fieldsGrid.Children.Remove(tb);
                    }
                    foreach (var lbl in _labelMap.Values)
                    {
                        fieldsGrid.Children.Remove(lbl);
                    }

                    for (int i = 0; i < oldFieldCount; i++)
                    {
                        fieldsGrid.ColumnDefinitions.RemoveAt(fieldsGrid.ColumnDefinitions.Count - 1);
                        fieldsGrid.RowDefinitions.RemoveAt(fieldsGrid.RowDefinitions.Count - 1);
                    }
                    _textBoxMap.Clear();
                    _labelMap.Clear();
                }

                _viewFields = AnnotationManager.ListViewFields(_viewId).OrderBy(x => x.FieldOrder).ToList();

                int fieldNo = 0;
                foreach (var field in _viewFields)
                {
                    fieldsGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    fieldsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                    Label lbl = new Label() { Content = " " + field.FieldName };
                    Grid.SetColumn(lbl, 0);
                    Grid.SetRow(lbl, fieldNo);
                    fieldsGrid.Children.Add(lbl);

                    TextBox tb = new TextBox();
                    Grid.SetColumn(tb, 1);
                    Grid.SetRow(tb, fieldNo);
                    tb.TextWrapping = TextWrapping.Wrap;
                    tb.AcceptsReturn = true;
                    tb.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    tb.Height = 50;
                    fieldsGrid.Children.Add(tb);
                    _textBoxMap.Add(field.FieldId, tb);
                    _labelMap.Add(field.FieldId, lbl);
                    //tb.TextChanged += FieldTextBox_Changed;

                    fieldNo++;
                }

                Grid.SetRow(buttonStack, fieldNo);

                Grid.SetRow(historyGrid, fieldNo + 1);
            }
        }

        //private void FieldTextBox_Changed(object sender, TextChangedEventArgs e)
        //{
        //    if (SavedIndicatorLabel.Visibility == Visibility.Visible)
        //    {
        //        SavedIndicatorLabel.Visibility = Visibility.Hidden;
        //    }       
        //}

        public void LoadElement(int elementId)
        {
            //if (_annotationManager == null)
            //{

            //}

            //SavedIndicatorLabel.Visibility = Visibility.Hidden;

            if (elementId == _currentElementId)
            {
                return;
            }

            fieldsGrid.Visibility = Visibility.Hidden;
            waitingPanel.Visibility = Visibility.Visible;
            _currentElementId = elementId;
            _displayedElementId = -1;
            Task loadingTask = Task.Factory.StartNew(() => { Load(elementId); });
            loadingTask.ContinueWith((t) => 
            {
                Dispatcher.Invoke(() => 
                {
                    UpdateGrid(elementId);
                });
            });
            
        }

        private void Load(int elementId)
        {
            //_modelElementType = GraphManager.GetType(elementId);
            //_supportedType = SearchManager.GetType(_modelElementType);
            //if (_modelElementType == _supportedType)
            //{
            //    _fieldValues = AnnotationManager.GetViewFieldValues(_viewId, _currentElementId);
            //}
            var values = AnnotationManager.GetViewFieldValues(_viewId, elementId);
            if (_displayedElementId != _currentElementId)
            {
                _fieldValues = values;
            }
            _linkFrom = AnnotationManager.ListLinksFrom(_projectId, _currentElementId);
            _linkTo = AnnotationManager.ListLinksTo(_projectId, _currentElementId);
        }

        private void LoadAnnotatedDependencies(int elementId)
        {
            _annotatedDependencySet = InspectManager.GetCloseAnnotatedSources(elementId);
        }

        private void UpdateGrid(int elementId)
        {
            if (_currentElementId == _displayedElementId)
            {
                return;
            }

            foreach (UIElement uIElement in fieldsGrid.Children)
            {
                uIElement.Visibility = System.Windows.Visibility.Visible;
                historyGrid.Visibility = System.Windows.Visibility.Hidden;
            }
            

                _currentElementId = elementId;                

                foreach (var textBox in _textBoxMap.Values)
                {
                    textBox.Clear();
                }

                foreach (var fv in _fieldValues)
                {
                    _textBoxMap[fv.FieldId].Text = fv.Value;
                }

            ElementFromGrid.ItemsSource = _linkFrom;
            LinksToGrid.ItemsSource = _linkTo;
            //CancelButton.IsEnabled = true;
            //SaveButton.IsEnabled = true;
            //ViewButton.IsEnabled = true;


            annotatedDependenciesGrid.Visibility = Visibility.Collapsed;

            Task annotatedDependencyLoadingTask = Task.Factory.StartNew(() => { LoadAnnotatedDependencies(elementId); });
            annotatedDependencyLoadingTask.ContinueWith((t) =>
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateAnnotatedDependenciesGrid();
                });
            });

            waitingPanel.Visibility = Visibility.Hidden;
            fieldsGrid.Visibility = Visibility.Visible;

            _displayedElementId = _currentElementId;
        }

        private void UpdateAnnotatedDependenciesGrid()
        {
            if (_annotatedDependencySet == null)
            {
                return;
            }

            while (annotatedDependencies.Columns.Count > 2)
            {
                annotatedDependencies.Columns.RemoveAt(2);
            }
            foreach (var field in _annotatedDependencySet.FieldNames)
            {
                DataGridTextColumn textCol = new DataGridTextColumn();
                textCol.Header = field;
                textCol.Binding = new Binding(field);

                annotatedDependencies.Columns.Add(textCol);
            }

            annotatedDependencies.ItemsSource = _annotatedDependencySet.ElementsTable.DefaultView;
            annotatedDependencies.InvalidateVisual();

            annotatedDependenciesGrid.Visibility = Visibility.Visible;
        }

        private void UpdateLinksGrid()
        {
            _linkFrom = AnnotationManager.ListLinksFrom(_projectId, _currentElementId);
            _linkTo = AnnotationManager.ListLinksTo(_projectId, _currentElementId);
            ElementFromGrid.ItemsSource = _linkFrom;
            LinksToGrid.ItemsSource = _linkTo;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            List<AnnotationViewFieldValue> updateValues = new List<AnnotationViewFieldValue>();

            foreach (var tbKv in _textBoxMap)
            {
                var fldId = tbKv.Key;
                var tb = tbKv.Value;

                var existentField = _fieldValues.FirstOrDefault(x => x.FieldId == fldId);
                if (existentField != null)
                {
                    // skip unchanged fields
                    if (existentField.Value != tb.Text)
                    {
                        existentField.Value = tb.Text;
                        updateValues.Add(existentField);
                    }
                }
                else
                {
                    // skip empty fields
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        continue;
                    }

                    var newField = new AnnotationViewFieldValue()
                    {
                        FieldId = fldId,
                        ModelElementId = _currentElementId,
                        Value = tb.Text
                    };
                    _fieldValues.Add(newField);
                    updateValues.Add(newField);
                }
            }
            var links = _linkFrom;
            if (updateValues.Any())
            {
                if (Editable)
                {
                    AnnotationManager.UpdateElementFields(updateValues, _projectId, IdentityProvider.GetCurrentUser().UserId, links, new List<int>() { _currentElementId });
                    //SavedIndicatorLabel.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show("You do not have sufficient permissions to modify the entry. " +
                        "Request permissions from your DLS administrator.", "Insufficient permissions", 
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }

            if(click == true)
            {
                LoadTable(_currentElementId, _projectId);
                historyGrid.ItemsSource = _currentTable.AsDataView();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var tb in _textBoxMap.Values)
            {
                tb.Text = string.Empty;
            }

            foreach (var fv in _fieldValues)
            {
                var tb = _textBoxMap[fv.FieldId];
                tb.Text = fv.Value;
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (click == true)
            {
                ViewButton.Content = "View history";
                click = false;

                historyGrid.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                ViewButton.Content = "Close history";
                click = true;

                LoadTable(_currentElementId, _projectId);
                historyGrid.Visibility = System.Windows.Visibility.Visible;
                historyGrid.ItemsSource = _currentTable.AsDataView();
            }
        }

        private void LoadTable(int elementId, Guid projectConfigId )
        {
            try
            {
                _currentTable = AnnotationManager.GetHistoryTable(projectConfigId, elementId);
            }
            catch (Exception e)
            {
                _exception = e;
            }
        }

        private void AddLink_Click(object sender, RoutedEventArgs ev)
        {
            bool linkTypeChosen = false;
            var linkTypes = AnnotationManager.ListLinkTypes(_projectId);
            NamePicker window = new NamePicker(null
                , linkTypes.Select(x => new PickerItem() { Id = x.LinkTypeId, Label = x.LinkTypeName }).ToList());
            window.SetCaption("Choose the link type");
            window.Submitted += (s, e1) => { window.Close(); window.IsSubmitted = true; };
            window.Cancelled += (s, e2) => { window.Close(); };
            window.Loaded += (s, e3) => { };
            try
            {
                window.ShowDialog();
                if (!(window.SelectedItem == null) /*|| !(window.SelectedItem == "")*/)
                {
                    if (window.IsSubmitted == true)
                    {
                        _selectedLinkType = (int)window.SelectedItem.Id; // ToString();
                        linkTypeChosen = true;
                    }
                }
            }
            catch
            {
                window.Close();
                MessageBox.Show("You did not choose link type in the box.",
                     "Information", MessageBoxButton.OK, MessageBoxImage.Question);
                linkTypeChosen = false;
            }
            if(linkTypeChosen == true)
            {
                AddFromElementClicked(this, new AddLinkEventArgs());
                //TypeChooser();
            }

        }

        //private void TypeChooser()
        //{
        //    AddLinkPanel lp = new AddLinkPanel();
        //    var pane = ShowWindow(lp, "Add Link");
        //    lp.AddFromElementClicked += AddFromElementClicked;
        //    lp.AddFromElementClicked += (o, e) => { pane.Close(); };
        //    lp.AddToElementClicked += AddToElementClicked;
        //    lp.AddToElementClicked += (o, e) => { pane.Close(); }; ;
        //}

        private void DeleteTo_Click(object sender, RoutedEventArgs e)
        {
            AnnotationLinkFromTo rowView = (AnnotationLinkFromTo)LinksToGrid.SelectedItem;

            MessageBoxResult MBresult = MessageBox.Show("Do you want to delete link " + rowView.ElementFromToCaption + ", link type: " + rowView.LinkTypeName + "?",
                      "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (MBresult == MessageBoxResult.Yes)
            {
                Task loadingTask = Task.Factory.StartNew(() => {
                    var links = AnnotationManager.ListLinksTo(_projectId, _currentElementId);
                    links = links.Where(link => link.ModelElementToId != rowView.ModelElementToId).ToList();
                    AnnotationManager.UpdateElementFields(_fieldValues, _projectId, IdentityProvider.GetCurrentUser().UserId, links, new List<int>() { _currentElementId });
                    Load(_currentElementId);
                });
                loadingTask.ContinueWith((t) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        UpdateLinksGrid();
                    });
                });

            }
        }

        private void DeleteFrom_Click(object sender, RoutedEventArgs e)
        {
            //nefunguje dodelat
            AnnotationLinkFromTo rowView = (AnnotationLinkFromTo)ElementFromGrid.SelectedItem;
            
            MessageBoxResult MBresult = MessageBox.Show("Do you want to delete link " + rowView.ElementFromToCaption + ", link type: " + rowView.LinkTypeName + " ?",
                                  "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (MBresult == MessageBoxResult.Yes)
            {
                Task loadingTask = Task.Factory.StartNew(() => {
                    var links = AnnotationManager.ListLinksFrom(_projectId, rowView.ModelElementFromId);
                    links = links.Where(link => link.ModelElementToId != rowView.ModelElementToId).ToList();
                    var values = AnnotationManager.GetViewFieldValues(_viewId, rowView.ModelElementFromId);
                    AnnotationManager.UpdateElementFields(values, _projectId, IdentityProvider.GetCurrentUser().UserId, links, new List<int>() { _currentElementId });
                    Load(_currentElementId);
                });
                loadingTask.ContinueWith((t) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        UpdateLinksGrid();
                    });
                });
                
            }
        }

        private void LinkFrom_Click(object sender, RoutedEventArgs e)
        {
            //nefunguje dodelat
            AnnotationLinkFromTo rowView = (AnnotationLinkFromTo)ElementFromGrid.SelectedItem;
            var elementId = rowView.ModelElementToId;
            var elementName = rowView.ElementFromToCaption;

            if (BusinessViewLinkClicked != null)
            {
                BusinessViewLinkClicked(this, new BusinessViewLinkClickedArgs()
                {
                    LinkedElementId = elementId,
                    LinkedElementName = elementName
                });
            }
        }

        private void AnnotatedDependency_Click(object sender, RoutedEventArgs e)
        {
            DataRowView rowView = (DataRowView)annotatedDependencies.SelectedItem;
            var elementId = (int)rowView["ModelElementId"];
            var elementName = (string)rowView["ModelElementName"];

            if (BusinessViewLinkClicked != null)
            {
                BusinessViewLinkClicked(this, new BusinessViewLinkClickedArgs()
                {
                    LinkedElementId = elementId,
                    LinkedElementName = elementName
                });
            }
        }

        private void AddFromElementClicked(object sender, AddLinkEventArgs e)
        {
            //z jinych elememntu do tohoto linku, dodelat nefunguje
            FulltextSearchPanel sp = new FulltextSearchPanel(true);
            var pane = ShowWindow(sp, "Find Target Element");
            sp.LoadData(_projectId);
            sp.AddSearchResultSelected += (o, e1) =>
            {                
                pane.Close();
                AnnotationLinkFromTo newLink = new AnnotationLinkFromTo();
                newLink.LinkTypeId = _selectedLinkType;

                newLink.ModelElementFromId = _currentElementId;
                newLink.ModelElementToId = e1.SelectedResult.ModelElementId;
                             
                var links = AnnotationManager.ListLinksFrom(_projectId, _currentElementId);

                foreach(AnnotationLinkFromTo link in links)
                {
                    if (link.ModelElementToId == newLink.ModelElementToId && link.ModelElementFromId == newLink.ModelElementFromId  && link.LinkTypeId == newLink.LinkTypeId)
                    {
                        MessageBox.Show("This Business link has already been added");
                        return;
                    }
                }

                links.Add(newLink);
                //var values = AnnotationManager.GetViewFieldValues(_viewId, e1.SelectedResult.ModelElementId);
                AnnotationManager.UpdateElementFields(_fieldValues, _projectId, IdentityProvider.GetCurrentUser().UserId, links, new List<int>() { _currentElementId });
                UpdateLinksGrid();
            };
        }

        private void AddToElementClicked(object sender, AddLinkEventArgs e)
        {
            //z tohoto elementu do jinych
            FulltextSearchPanel sp = new FulltextSearchPanel(true);
            var pane = ShowWindow(sp, "Find Source Element");
            sp.LoadData(_projectId);
            sp.AddSearchResultSelected += (o, e1) =>
            {
                pane.Close();
                AnnotationLinkFromTo newLink = new AnnotationLinkFromTo();
                newLink.LinkTypeId = _selectedLinkType;

                newLink.ModelElementFromId = e1.SelectedResult.ModelElementId;
                newLink.ModelElementToId = _currentElementId;

                var links = AnnotationManager.ListLinksTo(_projectId, _currentElementId);
                links.Add(newLink);
                AnnotationManager.UpdateElementFields(_fieldValues, _projectId, IdentityProvider.GetCurrentUser().UserId, links, new List<int>() { _currentElementId });
                UpdateLinksGrid();
            };
        }
    }
}
