using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.SsrsStructures;
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

namespace CD.DLS.Clients.Controls.Dialogs.SsrsRenderer
{
    //public class SelectedReportSpec
    //{
    //    public string Name { get; set; }
    //    public string ItemPath { get; set; }
    //    public int SsrsProjectComponentId { get; set; }
    //    public int ModelElementId { get; set; }
    //}

    /// <summary>
    /// Interaction logic for ReportSelector.xaml
    /// </summary>
    public partial class ReportSelector : UserControl
    {
        public ReportSelector()
        {
            InitializeComponent();
        }

        private ProjectConfig _projectConfig = null;
        private List<SsrsReportListItem> _reports = null;
        private GraphManager _graphManager = null;

        private SsrsReportListItem _selectedReport = null;
        public SsrsReportListItem SelectedReport
        {
            get { return _selectedReport; }
        }
        private ListCollectionView _view = null;

        public event EventHandler ItemSelected;

        //ListReportElementsResponse

        public ReportSelector(ProjectConfig projectConfig)
        {
            InitializeComponent();
            _projectConfig = projectConfig;
            
            //var origCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;

            _graphManager = new GraphManager();
            _reports = _graphManager.ListSsrsReports(_projectConfig.ProjectConfigId);
            
            _view = new ListCollectionView(_reports);

            gridReports.ItemsSource = _view;
            Mouse.OverrideCursor = Cursors.Arrow;

        }
        
        //private List<SelectedReportSpec> ExtractReports(ListSsrsReportsRequestResponse.SsrsFolder rootFolder)
        //{
        //    return rootFolder.Children.Where(x => x is ListSsrsReportsRequestResponse.SsrsReport).Select(x => new SelectedReportSpec { ItemPath = x.ItemPath, Name = x.Name, SsrsProjectComponentId = x.SsrsProjectComponentId, ModelElementId = -1 }).Union(
        //        rootFolder.Children.Where(x => x is ListSsrsReportsRequestResponse.SsrsFolder).SelectMany(x => ExtractReports((ListSsrsReportsRequestResponse.SsrsFolder)x))
        //        ).OrderBy(x => x.ItemPath).ToList();
        //}

        //public List<SelectedReportSpec> ExtractReports(ListReportElementsRequestResponse reportElementList)
        //{
        //    return reportElementList.Reports.Select(x => new SelectedReportSpec() { Name = x.Name, ItemPath = x.ItemPath, ModelElementId = x.ModelElementId, SsrsProjectComponentId = -1 }).ToList();
        //}
        

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            
            DataGridRow row = sender as DataGridRow;
            if (row == null)
            {
                return;
            }
            var context = row.DataContext as SsrsReportListItem;
            if (context == null)
            {
                return;
            }
            _selectedReport = context;
            if (ItemSelected != null)
            {
                ItemSelected(this, new EventArgs());
            }
        }
        

        public void RemoveFilterPaceholder(object sender, EventArgs e)
        {
            filterTextBox.Text = "";
        }

        public void AddFilterPaceholder(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(filterTextBox.Text))
                filterTextBox.Text = "Search...";
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_view == null)
            {
                return;
            }

            var filter = filterTextBox.Text;
            if (filter == "Search..." || filter == "")
            {
                _view.Filter = x => true;
                return;
            }
            _view.Filter = x => ((SsrsReportListItem)x).SsrsPath.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
