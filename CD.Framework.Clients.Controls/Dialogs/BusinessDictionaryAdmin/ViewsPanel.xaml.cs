using CD.DLS.Common.Structures;
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
using static CD.DLS.DAL.Managers.AnnotationManager;

namespace CD.DLS.Clients.Controls.Dialogs.BusinessDictionaryAdmin
{
    /// <summary>
    /// Interaction logic for ViewsPanel.xaml
    /// </summary>
    public partial class ViewsPanel : UserControl
    {
        private List<AnnotationView> _views = new List<AnnotationView>();
        private AnnotationManager _annotationManager = null;
        private ProjectConfig _projectConfig = null;
        private List<AnnotationField> _allFields = new List<AnnotationField>();
        private AnnotationView _selectedView = null;

        public event EventHandler Cancelled;

        public ViewsPanel()
        {
            InitializeComponent();
        }

        public void Init(ProjectConfig projectConfig)
        {
            _projectConfig = projectConfig;
            _annotationManager = new AnnotationManager();
            _views = _annotationManager.ListProjectViews(_projectConfig.ProjectConfigId);
            _allFields = _annotationManager.ListFields(_projectConfig.ProjectConfigId);
            TypeSelector.ItemsSource = _views;
            TypeSelector.SelectedIndex = 0;
        }

        private void LoadFieldList()
        {
            var selectedItem = (AnnotationView)(TypeSelector.SelectedItem);
            _selectedView = selectedItem;

            var viewFields = _annotationManager.ListViewFields(selectedItem.AnnotationViewId);
            var excludedFields = _allFields.Where(a => viewFields.All(x => x.FieldId != a.FieldId)).ToList();

            multiselect.SetData(viewFields.Select(x => new Multiselect.SelectItem() { Id = x.FieldId, Label = x.FieldName }).ToList(),
                excludedFields.Select(x => new Multiselect.SelectItem() { Id = x.FieldId, Label = x.FieldName }).ToList());
        }

        private void TypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadFieldList();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = multiselect.IncludedItems;
            List<OrderedListItem> orderedList = new List<OrderedListItem>();

            int i = 0;
            foreach (var item in selectedItems)
            {
                orderedList.Add(new OrderedListItem() { Id = item.Id, Order = ++i });
            }
            _annotationManager.UpdateViewFields(_selectedView.AnnotationViewId, orderedList);
            MessageBox.Show("View saved successfully.", "Values saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }
    }
}
