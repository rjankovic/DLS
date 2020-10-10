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

namespace CD.DLS.Clients.Controls.Dialogs.BusinessDictionaryAdmin
{
    /// <summary>
    /// Interaction logic for TypePanel.xaml
    /// </summary>
    public partial class TypePanel : UserControl
    {
        public TypePanel()
        {
            InitializeComponent();
        }

        private AnnotationManager AnnotationManager;
        private List<AnnotationLinkType> _types;
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
            _types = AnnotationManager.ListLinkTypes(projectConfig.ProjectConfigId);
        }

        private void UpdateGrid()
        {
            TypesGrid.ItemsSource = _types;
            waitingPanel.Visibility = Visibility.Hidden;
        }

        private void AddType_Click(object sender, RoutedEventArgs e)
        {
            List<string> link = null;
            if(_types == null)
            {
            }
            else
            {
                link = _types.Select(x => x.LinkTypeName).ToList();
            }
            var nameChooser = new NameChooserWindow(link);
            var res = nameChooser.ShowDialog();
            if (res.HasValue)
            {
                if ((nameChooser.SelectedName != null) && (nameChooser.SelectedName != string.Empty) && res.Value)
                {
                    AnnotationManager.AddType(_projectConfig.ProjectConfigId, nameChooser.SelectedName);
                    LoadData(_projectConfig);
                }
            }
        }

        private void RemoveType_Click(object sender, RoutedEventArgs e)
        {
            AnnotationLinkType rowView = (AnnotationLinkType)TypesGrid.SelectedItem;
            if (!(rowView == null))
            {
                //string result = rowView.DisplayName.ToString();

                if (rowView.UsedInLinks)
                {
                    MessageBox.Show("The link type is used in one or more links. Modify the links before deleting the type.", "Link type used in links", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                MessageBoxResult MBresult = MessageBox.Show("Do you want to delete link type " + rowView.LinkTypeName + "?",
                                      "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (MBresult == MessageBoxResult.Yes)
                {
                    AnnotationManager.DeleteLinkType(rowView.LinkTypeId);
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
