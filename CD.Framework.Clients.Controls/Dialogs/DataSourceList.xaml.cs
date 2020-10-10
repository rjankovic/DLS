using CD.DLS.Common.Interfaces;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using CD.DLS.DAL.Objects.Inspect;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;

namespace CD.DLS.Clients.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for ListPicker.xaml
    /// </summary>
    public partial class DataSourceList : UserControl
    {

        private List<DfSource> _data = null;
        private ListCollectionView _lcv = null;
        private ProjectConfig _config;

        public DataSourceList()
        {
            InitializeComponent();
        }

        public void LoadData(ProjectConfig config)
        {
            _config = config;

            Task loadingTask = Task.Factory.StartNew(QueryData);
            loadingTask.ContinueWith((t) => { Dispatcher.Invoke(DisplayData); });
        }

        private void DisplayData()
        {
            _lcv = new ListCollectionView(_data);
            _lcv.GroupDescriptions.Add(new PropertyGroupDescription("SourceType"));
            grid.ItemsSource = _lcv;
            waitingPanel.Visibility = Visibility.Hidden;
        }

        private void QueryData()
        {
            var im = new InspectManager();
            _data = im.ListExternalDfSources(_config.ProjectConfigId);
        }

        private void DetailsTextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var gridRow = sender;
            while (!(gridRow is DataGridRow))
            {
                gridRow = VisualTreeHelper.GetParent((DependencyObject)gridRow);
            }
            var row = (DataGridRow)gridRow;
            row.DetailsVisibility = row.DetailsVisibility == Visibility.Collapsed ?
                Visibility.Visible : Visibility.Collapsed;

        }
    }
}
