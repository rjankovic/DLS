using CD.DLS.API.Structures;
using CD.DLS.Clients.Controls.Dialogs.ExcelBusinessDictionary;
using CD.DLS.Clients.Controls.Dialogs.ExcelPanes;
using CD.DLS.Clients.Controls.Interfaces;
using CD.DLS.Common.Structures;
using CD.DLS.Common.Tools;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Identity;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BusinessDictionaryIndex;
using CD.DLS.DAL.Objects.Inspect;
using CD.DLS.DAL.Receiver;
using CD.Framework.ExcelAddin16.Panels;
using CD.Framework.ExcelAddin16.Panes;
using CD.Framework.ExcelAddin16.PivotTableTemplate;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Excel = Microsoft.Office.Interop.Excel;

namespace CD.DLS.ExcelAddin16
{
    public partial class ThisAddIn
    {

        private string _olapFieldName;
        private string _olapConnection;
        private string _olapCommand;
        private Excel.PivotTable _pivotTable;

        private PivotTable _suggestionsPivotTable = null;
        private Microsoft.Office.Tools.CustomTaskPane _suggestionsPane = null;
        private WinFormsPivotSuggestionsPane _suggestiosUserControl = null;

        private Microsoft.Office.Tools.CustomTaskPane _businessDictionaryPane = null;
        private BusinessDictionaryPane _businessDictionaryControl = null;
        
        internal ProjectConfig ProjectConfig;
        internal IReceiver Receiver;
        internal Guid ServiceReceiverId;
        internal ProjectConfigManager ProjectConfigManager = null;
        internal InspectManager InspectManager = null;
        internal LearningManager LearningManager = null;
        internal AnnotationManager AnnotationManager = null;
        internal RequestManager RequestManager = null;
        internal SecurityManager SecurityManager = null;
        internal BusinessDictionaryIndex BusinessDictionaryIndex = null;
        internal bool BusinessDictionaryPaneActive = true;
        internal bool CanEditAnnotations = false;

        internal Workbook RenderedReportWorkbook = null;
        internal Dictionary<string, Dictionary<int, Dictionary<int, ReportElementAbsolutePosition>>> RenderedReportTextBoxPositions = null;
        internal string RightClickSheetName = string.Empty;
        internal int RightClickRow = 0;
        internal int RightClickColumn = 0;
        internal bool UpdateInProgress = false;

        CommandBarButton _exploreLineageMenuItem;
        CommandBarButton _savePivotTemplateMenuItem;
        CommandBarButton _exploreReportFieldMenuItem;
        CommandBarButton _olapSuggestionsMenuItem;


        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            this.Application.SheetBeforeRightClick += Application_SheetBeforeRightClick;
            // this.CustomTaskPanes ...

            //this.Application.SheetPivotTableUpdate += Application_SheetPivotTableUpdate;

            this.Application.SheetSelectionChange += Application_SheetSelectionChange;

            _suggestiosUserControl = new WinFormsPivotSuggestionsPane();
            _suggestionsPane = this.CustomTaskPanes.Add(_suggestiosUserControl, "Pivot Suggestions");
            _suggestiosUserControl.SuggestionDoubleClicked += SuggestiosUserControl_SuggestionDoubleClicked;


            ConfigManager.ClientClass = ClientClassEnum.Excel;



            _businessDictionaryControl = new BusinessDictionaryPane();
            var width = _businessDictionaryControl.Width;
            _businessDictionaryPane = this.CustomTaskPanes.Add(_businessDictionaryControl, "Business Dictionary");
            _businessDictionaryPane.DockPosition = MsoCTPDockPosition.msoCTPDockPositionRight;
            //_businessDictionaryPane.Width = 600;
            _businessDictionaryPane.Visible = false;
            _businessDictionaryPane.Width = width * 4 / 3;

            _businessDictionaryControl.SaveClicked += BusinessDictionaryControl_SaveClicked;
            _businessDictionaryControl.DetailsClicked += BusinessDictionaryControl_DetailsClicked;

            //string s = System.IO.Packaging.PackUriHelper.UriSchemePack;
            //ResourceDictionary myResourceDictionary = new ResourceDictionary();
            //myResourceDictionary.Source = new Uri("pack://application:,,,/Resources/DefaultResourceDictionary.xaml", UriKind.RelativeOrAbsolute);
            //System.Windows.Application.Current.Resources.MergedDictionaries.Add(myResourceDictionary);
        }

        private void BusinessDictionaryControl_DetailsClicked(object sender, BusinessDictionaryPaneEventArgs e)
        {
            Clients.Controls.Dialogs.CentricGraphBrowser.CentricBrowser cbr = new Clients.Controls.Dialogs.CentricGraphBrowser.CentricBrowser();
            Mouse.OverrideCursor = Cursors.Wait;
            cbr.Init(new ServiceHelper(Globals.ThisAddIn.Receiver, ConfigManager.ServiceReceiverId, Globals.ThisAddIn.ProjectConfig), e.ModelElementId, Globals.ThisAddIn.ProjectConfig.ProjectConfigId);
            Mouse.OverrideCursor = Cursors.Arrow;
            var pane = ShowWindow(cbr, "Lineage Explorer", true);

        }

        private void BusinessDictionaryControl_SaveClicked(object sender, BusinessDictionaryPaneEventArgs e)
        {
            if (Globals.ThisAddIn.CanEditAnnotations)
            {
                var values = e.Values;
                Globals.ThisAddIn.AnnotationManager.UpdateElementFields(values, Globals.ThisAddIn.ProjectConfig.ProjectConfigId, IdentityProvider.GetCurrentUser().UserId, new List<AnnotationLinkFromTo>(), new List<int>() { e.ModelElementId });
                _businessDictionaryControl.RefreshData();
                _businessDictionaryControl.ShowSavedIndicator();
            }
            else
            {
                _businessDictionaryControl.ShowMissingPermissionsIndicator();
            }
        }

        private void SuggestiosUserControl_SuggestionDoubleClicked(object sender, PivotSuggestionsEventArgs e)
        {
            var selectedField = e.SuggestedFields.First();
            var initialReference = selectedField.FieldReference;
            if (selectedField.FieldType == DAL.Objects.Learning.OlapFieldType.Filter)
            {
                initialReference = selectedField.FieldReference.Substring(0, selectedField.FieldReference.LastIndexOf(".&[") /*- 3*/);
            }

            ConfigManager.Log.Info(string.Format("OLAP Suggestion: Activating cube field {0}", initialReference));
            CubeField cubeField;
            try
            {
                cubeField = _suggestionsPivotTable.CubeFields[initialReference];
            }
            catch
            {
                ConfigManager.Log.Warning(string.Format("OLAP Suggestion: Could not find OLAP field {0}", initialReference));
                return;
            }
            if (cubeField == null)
            {
                ConfigManager.Log.Warning(string.Format("OLAP Suggestion: Could not find OLAP field {0}", initialReference));
                return;
            }
            if (cubeField.Orientation != Excel.XlPivotFieldOrientation.xlHidden)
            {
                ConfigManager.Log.Warning(string.Format("OLAP Suggestion: OLAP field {0} is not hidden, rather {1}", initialReference, cubeField.Orientation));
                return;
            }
            switch (selectedField.FieldType)
            {
                case DAL.Objects.Learning.OlapFieldType.Axis:
                    cubeField.Orientation = Excel.XlPivotFieldOrientation.xlRowField;
                    break;
                case DAL.Objects.Learning.OlapFieldType.Measure:
                    cubeField.Orientation = Excel.XlPivotFieldOrientation.xlDataField;
                    break;
                case DAL.Objects.Learning.OlapFieldType.Filter:
                    cubeField.Orientation = Excel.XlPivotFieldOrientation.xlPageField;
                    try
                    {
                        var pivotField = cubeField.PivotFields.Item(1);
                        //var pivotField = _suggestionsPivotTable.PivotFields(initialReference);
                        pivotField.CurrentPageName = selectedField.FieldReference;
                    }
                    catch(Exception ex)
                    {
                        ConfigManager.Log.Warning(string.Format("OLAP Suggestion: Error when setting filter page {0}: {1}", selectedField.FieldReference, ex.Message));
                    }
                    break;
            }

            PivotTableStructureReader reader = new PivotTableStructureReader();
            var currentFields = reader.ListPivotFields(_suggestionsPivotTable);
            _suggestiosUserControl.FieldsChanged(currentFields);

        }

        private void Application_SheetSelectionChange(object Sh, Range Target)
        {
            CheckBusinessDictionaryDisplay(Target);

            // show the suggestions pane if there are some suggestions and the pivot table is active

            /*
            if (_suggestiosUserControl.CurrentSuggestions != null)
            {
                if (_suggestiosUserControl.CurrentSuggestions.Count > 0)
                {
                    if (Application.Intersect(Application.ActiveCell, _suggestionsPivotTable.TableRange2) != null)
                    {
                        _suggestionsPane.Visible = true;
                    }
                }
            }

            // hide the suggestions pane if the active cell leaves the pivot table
            if (_suggestionsPane.Visible)
            {
                try
                {
                    if (Application.Intersect(Application.ActiveCell, _suggestionsPivotTable.TableRange2) == null)
                    {
                        _suggestionsPane.Visible = false;
                    }
                }
                catch
                {
                    _suggestionsPane.Visible = false;
                }
            }*/
        }

        private void CheckBusinessDictionaryDisplay(Range selection)
        {
            try
            {
                if (!IsUserLoggedIn())
                {
                    return;
                }

                _businessDictionaryControl.HasEditPermissions = Globals.ThisAddIn.CanEditAnnotations;

                if (selection.Count != 1)
                {
                    _businessDictionaryPane.Visible = false;
                    return;
                }

                PivotField pivotField;

                if (!Globals.ThisAddIn.BusinessDictionaryPaneActive)
                {
                    _businessDictionaryPane.Visible = false;
                    return;
                }

                try
                {
                    pivotField = selection.PivotField;
                }
                catch
                {
                    _businessDictionaryPane.Visible = false;
                    return;
                }

                string fieldName;
                Excel.PivotTable pivotTable;
                PivotCache cache;
                string connection;
                string command;

                try
                {
                    fieldName = pivotField.SourceName;
                    pivotTable = selection.PivotTable;
                    cache = pivotTable.PivotCache();
                    connection = (string)cache.Connection.ToString();
                    command = (string)cache.CommandText;
                }
                catch
                {
                    _businessDictionaryPane.Visible = false;
                    return;
                }


                if (fieldName != "N/A" && command != "N/A" && connection.Contains("Provider=MSOLAP"))
                {
                    var cubeRefpath = ConnectionStringTools.GetCubeRefPath(connection, command);
                    var foundField = Globals.ThisAddIn.BusinessDictionaryIndex.OlapFieldLookup.FindOlapField(cubeRefpath, fieldName);

                    if (foundField == null)
                    {
                        _businessDictionaryPane.Visible = false;
                        return;
                    }

                    _businessDictionaryControl.LoadContent(foundField, Globals.ThisAddIn.BusinessDictionaryIndex, fieldName);
                    _businessDictionaryPane.Visible = true;
                    return;
                }
                else
                {
                    _businessDictionaryPane.Visible = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                ConfigManager.Log.Error(ex.Message);
                ConfigManager.Log.Error(ex.StackTrace);
                _businessDictionaryPane.Visible = false;
                return;
            }
        }
        

        private void Application_SheetPivotTableUpdate(object Sh, PivotTable pivotTable)
        {
            Application_SheetPivotTableUpdate(Sh, pivotTable, false);
        }

        private void Application_SheetPivotTableUpdate(object Sh, PivotTable pivotTable, bool forceOpenSuggestionsPane = false)
        {
            if (pivotTable.Name == "___dlstemp")
            {
                return;
            }

            if (forceOpenSuggestionsPane)
            {
                _suggestionsPane.Visible = true;
            }

            if (!IsUserLoggedIn())
            {
                return;
            }

            if (Application.Intersect(Application.ActiveCell, pivotTable.TableRange2) == null)
            {
                // current selection does not overlap with the pivot table
                return;
            }

            // only consider MDX - based tables
            //try
            //{
                var cache = pivotTable.PivotCache();
                if (cache.CommandType != XlCmdType.xlCmdCube)
                {
                    return;
                }
            //}
            //catch
            //{
            //    return;
            //}

            var ptName = pivotTable.Name;
            var fullRange = pivotTable.TableRange2;
            //Globals.ThisAddIn.proje

            //var cache = pivotTable.PivotCache();
            var connection = (string)cache.Connection.ToString();
            var cubeName = cache.CommandText;
            string serverName;
            string dbName;

            var cubeRefPath = ConnectionStringTools.GetSsasDbRefPath(connection, out serverName, out dbName);
            var cubeFilter = new OlapCubeRuleFilter()
            {
                ServerName = serverName,
                DbName = dbName,
                CubeName = cubeName
            };

            _suggestionsPivotTable = pivotTable;
            _suggestiosUserControl.Init(LearningManager, ProjectConfig, cubeFilter);
            PivotTableStructureReader reader = new PivotTableStructureReader();
            var currentFields = reader.ListPivotFields(pivotTable);
            _suggestiosUserControl.FieldsChanged(currentFields);
            if (_suggestiosUserControl.CurrentSuggestions.Count > 0)
            {
                _suggestionsPane.Visible = true;
            }
        }


        //}

        private bool IsUserLoggedIn()
        {
            return IdentityProvider.GetCurrentUser() != null;
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            if (ConfigManager.LogInitialized)
            {
                ConfigManager.Log.FlushMessages();
            }
        }

        protected override IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return new MainRibbon();
        }


        private CommandBar GetPivotTableCellContextMenu()
        {
            var accNames = new List<string>();
            var names = new List<string>();
            var localNames = new List<string>();

            return this.Application.CommandBars["PivotTable Context Menu"];
        }

        private CommandBar GetCellContextMenu()
        {
            var accNames = new List<string>();
            var names = new List<string>();
            var localNames = new List<string>();

            //List<string> cbNames = new List<string>();
            //foreach (CommandBar cb in Application.CommandBars)
            //{
            //    cbNames.Add(cb.Name);
            //}

            return this.Application.CommandBars["Cell"];
        }

        private void ResetPivotTableCellMenu()
        {
            GetPivotTableCellContextMenu().Reset(); // reset the cell context menu back to the default
        }


        private void Application_SheetBeforeRightClick(object Sh, Excel.Range Target, ref bool Cancel)
        {
            ResetPivotTableCellMenu();
            GetCellContextMenu().Reset();

            Excel.Range cell = (Excel.Range)Target.Cells[1, 1];
            Excel.PivotField pivotField = null;

            
            string fieldName = "N/A";
            string itemName = "N/A";
            string dataType = "N/A";
            string command = "N/A";
            string connection = "N/A";

            Worksheet rightClickSheet = this.Application.ActiveSheet;
            RightClickSheetName = rightClickSheet.Name;
            RightClickRow = cell.Row;
            RightClickColumn = cell.Column;

            if (RenderedReportWorkbook == this.Application.ActiveWorkbook)
            {
                if (RenderedReportTextBoxPositions.ContainsKey(RightClickSheetName))
                {
                    if (RenderedReportTextBoxPositions[RightClickSheetName].ContainsKey(RightClickRow))
                    {
                        if (RenderedReportTextBoxPositions[RightClickSheetName][RightClickRow].ContainsKey(RightClickColumn))
                        {
                            AddExploreReportFieldMenuItem();
                            return;
                        }
                    }
                }
            }

            try
            {
                pivotField = cell.PivotField;
            }
            catch
            { }

            //try
            //{
            //    dataType = cell.PivotField.DataType.ToString();
            //}
            //catch
            //{ }
            try
            {
                fieldName = pivotField.SourceName;
            }
            catch
            {
            }
            //try
            //{
            //    itemName = cell.PivotItem.SourceName;
            //}
            //catch
            //{
            //}

            bool pivotTableFound = false;

            try
            {
                Excel.PivotTable pivotTable = cell.PivotTable;
                _pivotTable = pivotTable;
                pivotTableFound = true;
                var cache = pivotTable.PivotCache();
                command = (string)cache.CommandText; // cube name
            }
            catch
            { }

            
            try
            {
                Excel.PivotTable pivotTable = cell.PivotTable;
                _pivotTable = pivotTable;
                pivotTableFound = true;
                var cache = pivotTable.PivotCache();
                connection = (string)cache.Connection.ToString();
                
                //var commandType = cache.CommandType;
                //var queryType = cache.QueryType;
                //var type = cache.SourceType;
                //var connection2 = cache.Connection;

                //var ado = cache.ADOConnection;
                //var sourceData = pivotTable.SourceData;
                
            }
            catch
            { }

            //////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////

            /////////////////
            //AddSomething();
            /////////////////

            if (!IsUserLoggedIn())
            {
                return;
            }

            ////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////


            //MessageBox.Show(string.Format("Pivot: {5}\n\n, DataType: {3}\n\n, FiledName: {0}\n\n, ItemName: {1}\n\n, Commnad: {2}\n\n, Connection: {4}\n\n", 
            //    fieldName, itemName, command, dataType, connection, pivotTable != null));

            if (pivotTableFound)
            {
                AddSavePivotTemplateMenuItem();
                //AddOlapSuggestionsPaneButton();
            }

            if (fieldName != "N/A" && command != "N/A" && connection.Contains("Provider=MSOLAP"))
            {
                AddExploreOlapFieldMenuItem(fieldName);
                _olapFieldName = fieldName;
                _olapConnection = connection;
                _olapCommand = command;
            }

            
        }

        private void AddSavePivotTemplateMenuItem()
        {
            MsoControlType menuItem = MsoControlType.msoControlButton;

            _savePivotTemplateMenuItem = (CommandBarButton)GetPivotTableCellContextMenu().Controls.Add(menuItem, missing, missing, 1, true);
            _savePivotTemplateMenuItem.Style = MsoButtonStyle.msoButtonIconAndCaption;
            _savePivotTemplateMenuItem.FaceId = 2648;
            _savePivotTemplateMenuItem.Caption = string.Format("Save Pivot Table Template");
            _savePivotTemplateMenuItem.Click += new Microsoft.Office.Core._CommandBarButtonEvents_ClickEventHandler(SavePivotTemplate);

            /*
            CommandBarButton savePivotTemplateTestMenuItem = (CommandBarButton)GetPivotTableCellContextMenu().Controls.Add(menuItem, missing, missing, 1, true);
            savePivotTemplateTestMenuItem.Style = MsoButtonStyle.msoButtonIconAndCaption;
            savePivotTemplateTestMenuItem.FaceId = 2648;
            savePivotTemplateTestMenuItem.Caption = string.Format("Pivot Table Template TEST");
            savePivotTemplateTestMenuItem.Click += new Microsoft.Office.Core._CommandBarButtonEvents_ClickEventHandler(SavePivotTemplateTest);
            */
        }

        private void AddSomething()
        {
            MsoControlType menuItem = MsoControlType.msoControlButton;
            /*
            CommandBarButton savePivotTemplateTestMenuItemX = (CommandBarButton)GetPivotTableCellContextMenu().Controls.Add(menuItem, missing, missing, 1, true);
            savePivotTemplateTestMenuItemX.Style = MsoButtonStyle.msoButtonIconAndCaption;
            savePivotTemplateTestMenuItemX.FaceId = 2648;
            savePivotTemplateTestMenuItemX.Caption = string.Format("Something");
            savePivotTemplateTestMenuItemX.Click += new Microsoft.Office.Core._CommandBarButtonEvents_ClickEventHandler(Something);
            */
            CommandBarButton savePivotTemplateTestMenuItem = (CommandBarButton)GetPivotTableCellContextMenu().Controls.Add(menuItem, missing, missing, 1, true);
            savePivotTemplateTestMenuItem.Style = MsoButtonStyle.msoButtonIconAndCaption;
            savePivotTemplateTestMenuItem.FaceId = 2648;
            savePivotTemplateTestMenuItem.Caption = string.Format("Pivot Table Template TEST");
            savePivotTemplateTestMenuItem.Click += new Microsoft.Office.Core._CommandBarButtonEvents_ClickEventHandler(SavePivotTemplateTest);
        }


        private void AddExploreOlapFieldMenuItem(string fieldName)
        {
            MsoControlType menuItem = MsoControlType.msoControlButton;
            _exploreLineageMenuItem = (CommandBarButton)GetPivotTableCellContextMenu().Controls.Add(menuItem, missing, missing, 1, true);
            _exploreLineageMenuItem.Style = MsoButtonStyle.msoButtonIconAndCaption;
            _exploreLineageMenuItem.FaceId = 570;
            _exploreLineageMenuItem.Caption = string.Format("OLAP Field Lineage", fieldName);
            _exploreLineageMenuItem.Click += new Microsoft.Office.Core._CommandBarButtonEvents_ClickEventHandler(ExploreOlapField);

        }

        void ExploreOlapField(Microsoft.Office.Core.CommandBarButton Ctrl, ref bool CancelDefault)
        {

            var cubeRefpath = ConnectionStringTools.GetCubeRefPath(_olapConnection, _olapCommand);
            var foundFields = InspectManager.FindOlapField(cubeRefpath, _olapFieldName);


            FoundOlapFiled fld = null;
            if (Globals.ThisAddIn.ProjectConfig == null && foundFields.Count > 0)
            {
                fld = foundFields[0];
                List<ProjectConfig> configs = ProjectConfigManager.ListProjectConfigs();
                Globals.ThisAddIn.ProjectConfig = configs.First(x => x.ProjectConfigId == fld.ProjectConfigId);
                Globals.ThisAddIn.ServiceReceiverId = fld.ProjectConfigId;
                Globals.ThisAddIn.Receiver = new HttpReceiver(ConfigManager.CustomerCode); // Receiver(Guid.NewGuid(), "CDF Manager Receiver");
            }
            else if (Globals.ThisAddIn.ProjectConfig == null)
            {
                return;
            }
            else
            {
                fld = foundFields.FirstOrDefault(x => x.ProjectConfigId == Globals.ThisAddIn.ProjectConfig.ProjectConfigId);
            }

            if (fld == null)
            {
                return;
            }


            Clients.Controls.Dialogs.CentricGraphBrowser.CentricBrowser cbr = new Clients.Controls.Dialogs.CentricGraphBrowser.CentricBrowser();
            cbr.Init(new ServiceHelper(Globals.ThisAddIn.Receiver, ConfigManager.ServiceReceiverId, Globals.ThisAddIn.ProjectConfig), fld.ModelElementId, Globals.ThisAddIn.ProjectConfig.ProjectConfigId);
            var pane = ShowWindow(cbr, "Lineage Explorer", true);

            //Globals.ThisAddIn.
            //Clients.Controls.Dialogs.SourceTargetSelector.SourceTargetSelector sts =
            //    new Clients.Controls.Dialogs.SourceTargetSelector.SourceTargetSelector(Globals.ThisAddIn.ProjectConfig, Globals.ThisAddIn.Receiver, true);
            //sts.SetTargetRootElementId(fld.RefPath);
            //var pane = ShowWindow(sts, "Lineage Explorer", true);

            //LineageWindow lineage = new LineageWindow(_connectionString, _cubeName, _fieldName, _pivotTable);
            //lineage.ShowDialog();
        }

        private void AddExploreReportFieldMenuItem()
        {
            MsoControlType menuItem = MsoControlType.msoControlButton;
            _exploreReportFieldMenuItem = (CommandBarButton)GetCellContextMenu().Controls.Add(menuItem, missing, missing, 1, true);
            _exploreReportFieldMenuItem.Style = MsoButtonStyle.msoButtonIconAndCaption;
            _exploreReportFieldMenuItem.FaceId = 570;
            _exploreReportFieldMenuItem.Caption = "Report Field Lineage";
            _exploreReportFieldMenuItem.Click += new Microsoft.Office.Core._CommandBarButtonEvents_ClickEventHandler(ExploreReportField);
        }

        void ExploreReportField(Microsoft.Office.Core.CommandBarButton Ctrl, ref bool CancelDefault)
        {
            var rdlTb = RenderedReportTextBoxPositions[RightClickSheetName][RightClickRow][RightClickColumn];
            
            Clients.Controls.Dialogs.CentricGraphBrowser.CentricBrowser cbr = new Clients.Controls.Dialogs.CentricGraphBrowser.CentricBrowser();
            cbr.BusinessLinkClikThrough = true;

            cbr.Init(new ServiceHelper(Globals.ThisAddIn.Receiver, ConfigManager.ServiceReceiverId, Globals.ThisAddIn.ProjectConfig), rdlTb.ModelElementId, Globals.ThisAddIn.ProjectConfig.ProjectConfigId);
            var pane = ShowWindow(cbr, "Lineage Explorer: " + rdlTb.Name, true);
        }

        void SavePivotTemplate(Microsoft.Office.Core.CommandBarButton Ctrl, ref bool CancelDefault)
        {
            PivotTableStructureReader reader = new PivotTableStructureReader();
            var structure = reader.ReadStructure(_pivotTable);

            if (structure == null)
            {
                MessageBox.Show(string.Format("Could not read pivot table structure{0}", reader.Error == null ? "" : (": " + reader.Error.Message)));
                return;
            }

            Clients.Controls.Dialogs.BusinessObjects.BusinessFolderEditor editor = new Clients.Controls.Dialogs.BusinessObjects.BusinessFolderEditor();
            editor.CanDeleteContent = true;
            editor.ItemSelected += (o, e) =>
            {
                //var origCursor = System.Windows.Input.Mouse.OverrideCursor;
                //System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                Task t = editor.SavePivotTableTemplateToSelectedNode(structure);
                //t.ContinueWith(tt => Dispatcher.CurrentDispatcher.Invoke(() => { System.Windows.Input.Mouse.OverrideCursor = origCursor; }));
            };

            editor.Init(Globals.ThisAddIn.Receiver, Globals.ThisAddIn.ProjectConfig);
            ShowWindow(editor, "Select Folder", true);
        }

        void AddOlapSuggestionsPaneButton()
        {
            MsoControlType menuItem = MsoControlType.msoControlButton;

            _olapSuggestionsMenuItem = (CommandBarButton)GetPivotTableCellContextMenu().Controls.Add(menuItem, missing, missing, 1, true);
            _olapSuggestionsMenuItem.Style = MsoButtonStyle.msoButtonIconAndCaption;
            _olapSuggestionsMenuItem.FaceId = 0598;
            _olapSuggestionsMenuItem.Caption = string.Format("Show Pivot Suggestions");
            _olapSuggestionsMenuItem.Click += new Microsoft.Office.Core._CommandBarButtonEvents_ClickEventHandler(ShowOlapSuggestionsPane);
        }

        void ShowOlapSuggestionsPane(Microsoft.Office.Core.CommandBarButton Ctrl, ref bool CancelDefault)
        {
            if (_pivotTable != null)
            {
                var x = _pivotTable.Name;
            }
            else
            {
                return;
            }
            Application_SheetPivotTableUpdate(this, _pivotTable, true);
        }

        void SavePivotTemplateTest(Microsoft.Office.Core.CommandBarButton Ctrl, ref bool CancelDefault)
        {
            PivotTableStructureReader reader = new PivotTableStructureReader();
            var structure = reader.ReadStructure(_pivotTable);

            Worksheet newWorksheet;
            var excel = Globals.ThisAddIn.Application.ActiveWorkbook;

            newWorksheet = (Worksheet)excel.Worksheets.Add();
            newWorksheet.Activate();

            PivotTableInitializer init = new PivotTableInitializer(Globals.ThisAddIn.Application);
            init.CreatePivotTable(structure, newWorksheet);

            //Excel.TableStyle ptStyle = wbook.TableStyles["QA Pivot Style"];

            //var baseName = "BusinessDictionary";
            //int sheetNo = 1;
            //var sheetsHash = new HashSet<string>();
            //foreach (Worksheet sh in excel.Worksheets)
            //{
            //    sheetsHash.Add(sh.Name);
            //}
            //while (sheetsHash.Contains(baseName + sheetNo.ToString()))
            //{
            //    sheetNo++;
            //}

            //var newSheetName = baseName + sheetNo.ToString();
            //newWorksheet.Name = newSheetName;
        }

        private Array ConvertToFoxArray(string[] arr)
        {
            Array res = Array.CreateInstance(typeof(string), new int[] { arr.Length }, new int[] { 0 });
            for (int i = 0; i < arr.Length; i++)
            {
                res.SetValue(arr[i], i);
            }

            return res;
        }

        void Something(Microsoft.Office.Core.CommandBarButton Ctrl, ref bool CancelDefault)
        {
            //PivotTableStructureReader reader = new PivotTableStructureReader();
            //var structure = reader.ReadStructure(_pivotTable);

            Worksheet worksheet;
            var excel = Globals.ThisAddIn.Application.ActiveWorkbook;
            worksheet = (Worksheet)excel.Worksheets.Add();
            worksheet.Activate();

            PivotCache pivotCache =
            worksheet.Application.ActiveWorkbook.PivotCaches().Add(XlPivotTableSourceType.xlExternal, Missing.Value);
            pivotCache.Connection = "OLEDB;Provider=MSOLAP.8;Integrated Security=SSPI;Persist Security Info=True;Initial Catalog=Sample_SSAS;Data Source=localhost;MDX Compatibility=1;Safety Options=2;MDX Missing Member Mode=Error;Update Isolation Level=2"; //structure.ConnectionString;
            pivotCache.MaintainConnection = true;
            pivotCache.CommandText = "Manpower"; // structure.CubeName;
            pivotCache.CommandType = XlCmdType.xlCmdCube;
            pivotCache.MissingItemsLimit = XlPivotTableMissingItems.xlMissingItemsNone;

            PivotTables pivotTables = (PivotTables)worksheet.PivotTables(Missing.Value);

            var cell = worksheet.Cells[1, 1];

            pivotCache.MakeConnection();

            PivotTable pivotTable = pivotTables.Add(pivotCache, cell, "PivotTable1",
                Missing.Value, Missing.Value);

            CubeField measureField = pivotTable.CubeFields["[Measures].[Working Days]"];
            measureField.Orientation = Excel.XlPivotFieldOrientation.xlDataField;

            CubeField axisField = pivotTable.CubeFields["[Date].[Year - Quarter - Month - Date]"];
            axisField.Orientation = Excel.XlPivotFieldOrientation.xlRowField;

            var topField = (PivotField)axisField.PivotFields.Item(1);
            var topFieldName = topField.Name;

            var topHiddenItems = new string[]
                {
        "[Date].[Year - Quarter - Month - Date].[Year].&[-1]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2000]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2001]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2002]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2003]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2004]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2005]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2006]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2007]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2008]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2009]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2010]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2011]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2012]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2013]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2014]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2019]",
        "[Date].[Year - Quarter - Month - Date].[Year].&[2020]"
                };

            topField.HiddenItemsList = topHiddenItems;

            foreach (PivotItem topItem in topField.PivotItems())
            {
                var topItemName = (string)(topItem.SourceName);

                if (topItem.SourceName == "[Date].[Year - Quarter - Month - Date].[Year].&[2018]")
                {
                    topItem.DrillTo("[Date].[Year - Quarter - Month - Date].[Quarter]");

                    PivotField quarterField = pivotTable.PivotFields("[Date].[Year - Quarter - Month - Date].[Quarter]");

                    quarterField.HiddenItemsList = new string[]
                        {
                            "[Date].[Year - Quarter - Month - Date].[Quarter].&[2018]&[101]",
                            "[Date].[Year - Quarter - Month - Date].[Quarter].&[2018]&[104]"
                        };

                    foreach (PivotItem quarterItem in quarterField.PivotItems())
                    {
                        var quarterItemName = quarterItem.SourceName;

                        if (quarterItemName == "[Date].[Year - Quarter - Month - Date].[Quarter].&[2018]&[103]")
                        {
                            quarterItem.DrillTo("[Date].[Year - Quarter - Month - Date].[Month]");

                            var monthField = pivotTable.PivotFields("[Date].[Year - Quarter - Month - Date].[Month]");

                            foreach (PivotItem monthItem in monthField.PivotItems())
                            {
                                var monthItemName = monthItem.SourceName;

                                if (monthItemName == "[Date].[Year - Quarter - Month - Date].[Month].&[2018]&[8]")
                                {
                                    monthItem.DrillTo("[Date].[Year - Quarter - Month - Date].[Date]");

                                    PivotField dateField = pivotTable.PivotFields("[Date].[Year - Quarter - Month - Date].[Date]");

                                    dateField.HiddenItemsList = new string[]
                                        {
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180801]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180802]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180803]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180804]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180805]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180806]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180807]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180808]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180809]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180810]",

                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180811]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180812]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180813]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180814]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180815]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180816]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180817]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180818]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180819]",
                                            "[Date].[Year - Quarter - Month - Date].[Date].&[20180820]"
                                        };
                                }
                            }
                        }

                    }
                }
            }
        }

        private System.Windows.Window ShowWindow(ContentControl control, string title, bool dialog = false)
        {
            System.Windows.Window window = new System.Windows.Window
            {
                Title = title,
                Content = control
            };

            if (control is ICloseable)
            {
                ((ICloseable)control).Closing += (o, e) => { window.Close(); };
            }

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


        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion
    }
}
