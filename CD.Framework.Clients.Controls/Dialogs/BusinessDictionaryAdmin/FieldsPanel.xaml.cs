using CD.DLS.Clients.Controls.Dialogs.Security;
using CD.DLS.Clients.Controls.Windows;
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
using static CD.DLS.DAL.Managers.SecurityManager;

namespace CD.DLS.Clients.Controls.Dialogs.BusinessDictionaryAdmin
{

    public partial class FieldsPanel : UserControl
    {
        public FieldsPanel()
        {
            InitializeComponent();
        }

        private AnnotationManager AnnotationManager;
        private List<AnnotationField> _fields;
        private ProjectConfig _projectConfig;
        
        public void LoadData(ProjectConfig projectConfig)
        {
            waitingPanel.Visibility = Visibility.Visible;
            AnnotationManager = new AnnotationManager();
            Task loadingTask = Task.Factory.StartNew(() => { LoadCurrentTable(projectConfig); });
            loadingTask.ContinueWith((t) => { Dispatcher.Invoke(UpdateGrid); });
            _projectConfig = projectConfig;
        }

        private void LoadCurrentTable(ProjectConfig projectConfig)
        {
            _fields = AnnotationManager.ListFields(projectConfig.ProjectConfigId);
        }

        private void UpdateGrid()
        {
            fieldsGrid.ItemsSource = _fields;
            waitingPanel.Visibility = Visibility.Hidden;
        }

        private void AddField_Click(object sender, RoutedEventArgs e)
        {

            var nameChooser = new NameChooserWindow(_fields.Select(x => x.FieldName).ToList());
            var res = nameChooser.ShowDialog();
            if (res.HasValue)
            {
                if ((nameChooser.SelectedName != null) && (nameChooser.SelectedName != string.Empty) && res.Value)
                {
                    AnnotationManager.AddField(_projectConfig.ProjectConfigId, nameChooser.SelectedName);
                    LoadData(_projectConfig);
                }
            }
        }

        private void RemoveField_Click(object sender, RoutedEventArgs e)
        {
            AnnotationField rowView = (AnnotationField)fieldsGrid.SelectedItem;
            if (!(rowView == null))
            {
                //string result = rowView.DisplayName.ToString();

                if (rowView.UsedInViews)
                {
                    MessageBox.Show("The field is used in one or more views. Modify the views before deleting the field.", "Field used in views", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                MessageBoxResult MBresult = MessageBox.Show("Do you want to delete field " + rowView.FieldName + "?",
                                      "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (MBresult == MessageBoxResult.Yes)
                {
                    AnnotationManager.DeleteField(rowView.FieldId);
                    LoadData(_projectConfig);
                }
            }
            else
            {
                MessageBox.Show("Please, select an item in the grid.",
                     "Information", MessageBoxButton.OK, MessageBoxImage.Question);
            }
        }
    }
}
