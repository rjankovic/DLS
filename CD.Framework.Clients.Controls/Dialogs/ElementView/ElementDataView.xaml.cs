using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BIDoc;
using System.Threading.Tasks;
using System.Windows.Controls;
using CD.DLS.Common.Tools;
using Microsoft.SqlServer.Management.Smo;
using System.Data;
using System;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.Text.RegularExpressions;

namespace CD.DLS.Clients.Controls.Dialogs.ElementView
{
    /// <summary>
    /// Interaction logic for ElementDataView.xaml
    /// </summary>
    public partial class ElementDataView : UserControl
    {
        // node that is being loaded
        private int _currentElementId = -1;
        // node that has been loaded
        private int _displayedElementId = -1;

        private BIDocModelElement _currentModelElement;
        private string connString;
        private string refPath;
        private RefPathStringTools _refPathStringTools;
        private DataTable _currentTable;
        private string schemaTable;
        private bool isTable = false;
        private Exception _exception = null;
        private string column;
        private bool colorColumns;

        private AnnotationManager _annotationManager;
        private SearchManager _searchManager;
        private GraphManager _graphManager;
        private InspectManager _inspectManager;

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


        public ElementDataView()
        {
            InitializeComponent();
            _refPathStringTools = new RefPathStringTools();
            
        }

        public void LoadData(int elementId)
        {
            if (_annotationManager == null)
            {
                _annotationManager = new AnnotationManager();
                _searchManager = new SearchManager();
                _graphManager = new GraphManager();
                _inspectManager = new InspectManager();
            }

            infoPanel.Visibility = System.Windows.Visibility.Hidden;
            permisionPanel.Visibility = System.Windows.Visibility.Hidden;
            dataPanel.Visibility = System.Windows.Visibility.Hidden;
            waitingPanel.Visibility = System.Windows.Visibility.Visible;
            dataGrid.Visibility = System.Windows.Visibility.Visible;
            
            getTempConnString(elementId);
            if (isTable == true)
            {
                _currentElementId = elementId;
                _displayedElementId = -1;
                Task loadingTask = Task.Factory.StartNew(LoadCurrentTable);
                loadingTask.ContinueWith((t) => { Dispatcher.Invoke(UpdateGrid); });
            }
            else
            {
                infoPanel.Visibility = System.Windows.Visibility.Visible;
                dataGrid.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void getTempConnString(int elementId)
        {
            _currentModelElement = GraphManager.GetModelElementById(elementId);
            refPath = _currentModelElement.RefPath.ToString();
            if (_currentModelElement.Type != "CD.DLS.Model.Mssql.Db.SchemaTableElement" 
                && _currentModelElement.Type != "CD.DLS.Model.Mssql.Db.ColumnElement" 
                && _currentModelElement.Type != "CD.DLS.Model.Mssql.Db.ViewElement")
            {
                isTable = false;
            }
            else
            {
                connString = _refPathStringTools.GetConnStringByRefPath(refPath);
                schemaTable = _refPathStringTools.GetSchemaTable(refPath);
                column = _refPathStringTools.GetColumn(refPath);
                isTable = true;
            } 
        }

        private void LoadCurrentTable()
        {
            try
            {
                var table = InspectManager.GetDataTable(connString, schemaTable); ;
                if (_currentElementId != _displayedElementId)
                {
                    _currentTable = table;
                }
            }
            catch(Exception e)
            {
                if (_currentElementId != _displayedElementId)
                {
                    _exception = e;
                }
            }    
        }

        private void UpdateGrid()
        {
            if (_currentElementId == _displayedElementId)
            {
                return;
            }

            Style cellStyleW = new Style(typeof(DataGridCell));
            cellStyleW.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.White));

            var e = _exception;
            if (e != null)
            {
                waitingPanel.Visibility = System.Windows.Visibility.Hidden;
                permisionPanel.Visibility = System.Windows.Visibility.Visible;
                string errorMessage;
                errorMessage = Regex.Replace(e.Message.ToString(), "(.{" + 50 + "})", "$1" + Environment.NewLine);                
                erorrBlock.Text = "Error Message: " + Environment.NewLine + errorMessage;
            }
            else
            {
                waitingPanel.Visibility = System.Windows.Visibility.Hidden;
                dataGrid.ItemsSource = _currentTable.AsDataView();               

                if (_currentTable.Rows.Count == 0)
                {
                    dataGrid.Visibility = System.Windows.Visibility.Hidden;
                    dataPanel.Visibility = System.Windows.Visibility.Visible;
                    colorColumns = false;                   
                }
                else
                {
                    dataGrid.CellStyle = cellStyleW;
                    colorColumns = true;
                    try
                    {
                        colorColumn();
                    }
                    catch
                    {
                        dataGrid.AutoGeneratedColumns += DataGrid_AutoGeneratedColumns;
                    }  
                }
            }

            _displayedElementId = _currentElementId;
        }

        private void DataGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            colorColumn();
        }

        private void colorColumn()
        {
            Style cellStyleY = new Style(typeof(DataGridCell));
            cellStyleY.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.Yellow));
            if (colorColumns == true)
            {
                if (column != "")
                {
                    int columnIndex = 0;
                    int i = 0;
                    foreach (DataColumn dc in _currentTable.Columns)
                    {
                        if (dc.ColumnName == column)
                        {
                            columnIndex = i;
                            dataGrid.Columns[columnIndex].CellStyle = cellStyleY;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
            }
            dataGrid.AutoGeneratedColumns -= DataGrid_AutoGeneratedColumns;
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Style cellStyleT = new Style(typeof(DataGridCell));
            cellStyleT.Setters.Add(new Setter(DataGridCell.IsSelectedProperty, false));
            dataGrid.CellStyle = cellStyleT;
        }
    }
}
