using CD.DLS.API.BusinessObjects;
using CD.DLS.API.Query;
using CD.DLS.API.Structures;
using CD.DLS.Clients.Controls.Dialogs;
using CD.DLS.Clients.Controls.Dialogs.CentricGraphBrowser;
using CD.DLS.Clients.Controls.Dialogs.Search;
using CD.DLS.Clients.Controls.Dialogs.SsrsRenderer;
using CD.DLS.Clients.Controls.Dialogs.TreeFilterSelector;
using CD.DLS.Clients.Controls.Interfaces;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Identity;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.SsrsStructures;
using CD.DLS.DAL.Receiver;
using CD.Framework.ExcelAddin16.Helpers;
using CD.Framework.ExcelAddin16.PivotTableTemplate;
using Microsoft.Office.Interop.Excel;
using Microsoft.SqlServer.ReportExecution2005;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using Office = Microsoft.Office.Core;

// TODO:  Follow these steps to enable the Ribbon (XML) item:

// 1: Copy the following code block into the ThisAddin, ThisWorkbook, or ThisDocument class.

//  protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
//  {
//      return new MainRibbon();
//  }

// 2. Create callback methods in the "Ribbon Callbacks" region of this class to handle user
//    actions, such as clicking a button. Note: if you have exported this Ribbon from the Ribbon designer,
//    move your code from the event handlers to the callback methods and modify the code to work with the
//    Ribbon extensibility (RibbonX) programming model.

// 3. Assign attributes to the control tags in the Ribbon XML file to identify the appropriate callback methods in your code.  

// For more information, see the Ribbon XML documentation in the Visual Studio Tools for Office Help.


namespace CD.DLS.ExcelAddin16
{
    public class DataDictionaryEditorLocation
    {
        public Worksheet Sheet { get; set; }
        public Range DataRange { get; set; }
        public int ModelElementIdColumnOffset { get; set; }
        public Dictionary<int, AnnotationViewField> FieldOffsets { get; set; }
        public Dictionary<int, Dictionary<int, AnnotationViewFieldValue>> FieldValues { get; set; }
        public TreeFilterEventArgs FilterValues { get; set; }
    }

    [ComVisible(true)]
    public class MainRibbon : Office.IRibbonExtensibility
    {
        private Office.IRibbonUI ribbon;
        private System.Windows.Window _treeFilterWindow = null;
        private Dictionary<string, DataDictionaryEditorLocation> _dataDictionaryEditors = new Dictionary<string, DataDictionaryEditorLocation>();
        private List<AnnotationLinkFromTo> _businessDictionarylinks;
        
        public MainRibbon()
        {
        }

        private void RefreshButtonVisibility()
        {
            ribbon.Invalidate();
        }

        public bool IsUserLoggedIn()
        {
            return IdentityProvider.GetCurrentUser() != null;
        }

        public bool getEnabledTest(Office.IRibbonControl control)
        {
            return Globals.ThisAddIn.ProjectConfig != null /* && !Globals.ThisAddIn.UpdateInProgress*/;
        }

        public bool getDictionaryEditorEndabledTest(Office.IRibbonControl control)
        {
            return Globals.ThisAddIn.ProjectConfig != null && Globals.ThisAddIn.CanEditAnnotations;
        }

        public string getSelectProjectLabel(Office.IRibbonControl control)
        {
            if (Globals.ThisAddIn.ProjectConfig != null /* && !Globals.ThisAddIn.UpdateInProgress*/)
            {
                return Globals.ThisAddIn.ProjectConfig.Name;
            }
            else
            {
                return "Open Project";
            }
        }

        private System.Windows.Window ShowWindow(ContentControl control, string title, bool dialog = false)
        {
            control.PreviewMouseLeftButtonDown += Control_PreviewMouseLeftButtonDown;
            control.PreviewMouseRightButtonDown += Control_PreviewMouseRightButtonDown;
            System.Windows.Window window = new System.Windows.Window
            {
                Title = title,
                Content = control
            };

            if (control is ICloseable)
            {
                ((ICloseable)control).Closing += (o, e) => { window.Close(); };
            }

            window.Closing += (o, e) =>
            {
                if (!Globals.ThisAddIn.Application.Interactive)
                {
                    Globals.ThisAddIn.Application.Interactive = true;
                }
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

        
        private void Control_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement fe = e.OriginalSource as FrameworkElement;
            if (fe == null)
            {
                return;
            }
            ConfigManager.Log.LogUserAction(Common.Interfaces.UserActionEventType.LeftClick, fe.Name, (fe.DataContext == null ? null : fe.DataContext.ToString()), null);
        }

        private void Control_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement fe = e.OriginalSource as FrameworkElement;
            if (fe == null)
            {
                return;
            }
            ConfigManager.Log.LogUserAction(Common.Interfaces.UserActionEventType.RightClick, fe.Name, (fe.DataContext == null ? null : fe.DataContext.ToString()), null);
        }
        
        //private void ShowCustomPanel(UserControl control, string title)
        //{
        //    ElementHost elementHost = new ElementHost();
        //    elementHost.Child = control;

        //    var customTaskPane = Globals.ThisAddIn.CustomTaskPanes.Add(elementHost, "My Task Pane");
        //    myCustomTaskPane.Visible = true;

        //    System.Windows.Window window = new System.Windows.Window
        //    {
        //        Title = title,
        //        Content = control
        //    };

        //    if (control is ICloseable)
        //    {
        //        ((ICloseable)control).Closing += (o, e) => { window.Close(); };
        //    }
        //    if (dialog)
        //    {
        //        window.ShowDialog();
        //    }
        //    else
        //    {
        //        window.Show();
        //    }
        //    return window;
        //}

        public void OnSelectProjectButton(Office.IRibbonControl control)
        {

            try
            {
                if (!IsUserLoggedIn())
                {
                    IdentityProvider.Login();
                    if (!IsUserLoggedIn())
                    {
                        return;
                    }
                    Mouse.OverrideCursor = Cursors.Wait;
                    Globals.ThisAddIn.ProjectConfigManager = new ProjectConfigManager();
                    Globals.ThisAddIn.InspectManager = new InspectManager();
                    Globals.ThisAddIn.LearningManager = new LearningManager();
                    Globals.ThisAddIn.AnnotationManager = new AnnotationManager();
                    Globals.ThisAddIn.RequestManager = new RequestManager();
                    Globals.ThisAddIn.SecurityManager = new SecurityManager();
                }
                Mouse.OverrideCursor = Cursors.Wait;
                List<ProjectConfig> configs = Globals.ThisAddIn.ProjectConfigManager.ListProjectConfigs();
                ListPicker lp = new ListPicker();
                lp.Init(new List<ListPicker.ListPickerItem>(configs.Select(x => new ListPicker.ListPickerItem { Name = x.Name, Value = x.ProjectConfigId.ToString() })));
                
                //MessageBox.Show("OK");
                lp.Selected += (s, e1) =>
                {
                    try
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        Globals.ThisAddIn.ProjectConfig = configs.First(x => x.ProjectConfigId == Guid.Parse(lp.SelectedItem.Value));
                        var receiverIdConfigValue = ConfigManager.ServiceReceiverId; // ConfigManager.GetGlobalConfigValue(ConfigManager.CONFIG_KEY_SVC_INSTANCE_ID);
                        Globals.ThisAddIn.ServiceReceiverId = receiverIdConfigValue; //Guid.Parse(receiverIdConfigValue);
                        Globals.ThisAddIn.Receiver = new HttpReceiver(ConfigManager.CustomerCode);
                        Globals.ThisAddIn.BusinessDictionaryIndex = new DAL.Objects.BusinessDictionaryIndex.BusinessDictionaryIndex(Globals.ThisAddIn.AnnotationManager, Globals.ThisAddIn.ProjectConfig.ProjectConfigId);

                        //Globals.ThisAddIn.Receiver.BroadcastMessageReceived += Receiver_BroadcastMessageReceived;
                        _dataDictionaryEditors.Clear();
                        CheckForModelUpdate();

                        var permissions = Globals.ThisAddIn.SecurityManager.UserPermissions(Globals.ThisAddIn.ProjectConfig.ProjectConfigId);
                        var editAnnotationsPermission = permissions.FirstOrDefault(x => x.Type == DAL.Security.PermissionEnum.EditAnnotations);
                        Globals.ThisAddIn.CanEditAnnotations = editAnnotationsPermission != null;

                        Mouse.OverrideCursor = Cursors.Arrow;
                        RefreshButtonVisibility();
                    }
                    catch (Exception ex)
                    {
                        ShowException(ex);
                    }
                };

                Mouse.OverrideCursor = Cursors.Arrow;
                var pane = ShowWindow(lp, "Select project", true);
            }
            catch (Exception ex)
            {
                ShowException(ex);
                //throw;
            }
        }

        private void Receiver_BroadcastMessageReceived(BroadcastMessage message)
        {
            if (Globals.ThisAddIn.ProjectConfig == null)
            {
                return;
            }

            if (Globals.ThisAddIn.ProjectConfig.ProjectConfigId != message.ProjectConfigId)
            {
                return;
            }

            if (message.Type == BroadcastMessageType.ProjectUpdateFinished)
            {
                ModelUpdateFinished();
            }
            else if (message.Type == BroadcastMessageType.ProjectUpdateStarted)
            {
                ModelUpdateStarted();
            }
        }

        private void CheckForModelUpdate()
        {
            var msgs = Globals.ThisAddIn.RequestManager.GetActiveBroadcastMessages();
            if (msgs.Any(x => x.Type == BroadcastMessageType.ProjectUpdateStarted && x.ProjectConfigId == Globals.ThisAddIn.ProjectConfig.ProjectConfigId))
            {
                ModelUpdateStarted();
            }
            else
            {
                Globals.ThisAddIn.UpdateInProgress = false;
            }
        }

        private void ModelUpdateStarted()
        {
            MessageBox.Show("The service has started to update the project model. The model will be unavailable during this time.", "Model update", MessageBoxButton.OK, MessageBoxImage.Warning);
            Globals.ThisAddIn.UpdateInProgress = true;
            RefreshButtonVisibility();
        }

        private void ModelUpdateFinished()
        {
            MessageBox.Show("The service has finished updating the model. The updated version is available now.", "Model update finished", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            Globals.ThisAddIn.UpdateInProgress = false;
            RefreshButtonVisibility();
        }

        public void OnLineageExplorerButton(Office.IRibbonControl control)
        {
            try
            {
                if (Globals.ThisAddIn.ProjectConfig == null)
                {
                    return;
                }

                Clients.Controls.Dialogs.SourceTargetSelector.SourceTargetSelector sts =
                    new Clients.Controls.Dialogs.SourceTargetSelector.SourceTargetSelector(Globals.ThisAddIn.ProjectConfig, Globals.ThisAddIn.Receiver);               
                var pane = ShowWindow(sts, "Lineage Explorer", true);
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        public void OnSearchButton(Office.IRibbonControl control)
        {
            try
            {                
                FulltextSearchPanel fts = new FulltextSearchPanel();
                fts.LoadData(Globals.ThisAddIn.ProjectConfig.ProjectConfigId);
                fts.SearchResultSelected += (o, e1) =>
                {                               
                    ShowCentricBrowserForElement(e1.SelectedResult.ModelElementId, e1.SelectedResult.ElementName);
                };
                var pane = ShowWindow(fts, "Search", true);                                                   
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }
        private void ShowCentricBrowserForElement(int elementId, string elementName)
        {
            ServiceHelper serviceHelper = new ServiceHelper(Globals.ThisAddIn.Receiver, ConfigManager.ServiceReceiverId, Globals.ThisAddIn.ProjectConfig);
            var previousCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
            CentricBrowser centricBrowser = new CentricBrowser();
            centricBrowser.Init(serviceHelper, elementId, Globals.ThisAddIn.ProjectConfig.ProjectConfigId);
            centricBrowser.BusinessLinkClikThrough = true;
            //centricBrowser.BusinessViewLinkClicked += BusinessViewLinkClicked;
            Mouse.OverrideCursor = previousCursor;

            System.Windows.Window _pane = null;
            centricBrowser.OpenPivotButtonClick += (o, e) =>
            {
                //Globals.ThisAddIn.Application.Interactive = true;
                
                Task.Factory.StartNew(() => {
                    OpenPivotTemplate(e.Element.RefPath, e.Element.Caption);
                    if (_pane != null)
                    {
                        _pane.Close();
                    }
                });
            };
            _pane = ShowWindow(centricBrowser, elementName + " | Data Flow", true);
            
        }

        private async void OpenPivotTemplate(string templateRefPath, string tableName)
        {
            ServiceHelper serviceHelper = new ServiceHelper(Globals.ThisAddIn.Receiver, ConfigManager.ServiceReceiverId, Globals.ThisAddIn.ProjectConfig);
            var request = new GetPivotTableTemplateRequest() { PivotTableTemplateRefPath = templateRefPath };
            var resp = await serviceHelper.PostRequest(request);
            var pivotTableStructure = resp.Structure;

            Worksheet newWorksheet = CreateWorksheet(tableName);
            newWorksheet.Activate();

            PivotTableInitializer init = new PivotTableInitializer(Globals.ThisAddIn.Application);
            var pt = init.CreatePivotTable(pivotTableStructure, newWorksheet);
        }

        public void OnOpenPivotTemplateButton(Office.IRibbonControl control)
        {
            if (!SetInteractiveFalseWitWarning())
            {
                return;
            }

            Clients.Controls.Dialogs.BusinessObjects.BusinessFolderEditor editor = new Clients.Controls.Dialogs.BusinessObjects.BusinessFolderEditor();
            editor.CanDeleteContent = true;
            editor.CanSelectFolder = false;

            editor.ItemSelected += (o, e) =>
            {
                var pivotTableStructureTask = editor.LoadPivotTableStructureFromSelectedNode();
                pivotTableStructureTask.ContinueWith(structureTask =>
                {
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        var pivotTableStructure = structureTask.Result;
                        if (pivotTableStructure == null)
                        {
                            return;
                        }
                        var tableName = editor.SelectedItem.Alias;

                        Worksheet newWorksheet = CreateWorksheet(tableName);
                        newWorksheet.Activate();

                        PivotTableInitializer init = new PivotTableInitializer(Globals.ThisAddIn.Application);
                        var pt = init.CreatePivotTable(pivotTableStructure, newWorksheet);
                        //try
                        //{
                        //    var ptStyle = Globals.ThisAddIn.Application.ActiveWorkbook.TableStyles["PivotStyleLight16"];
                        //    pt.TableStyle2 = ptStyle;
                        //}
                        //catch
                        //{
                        //}
                    });
                }
                );
                //if (pivotTableStructure == null)
                //{
                //    return;
                //}
                //var tableName = editor.SelectedItem.Alias;

                //Worksheet newWorksheet = CreateWorksheet(tableName);
                //newWorksheet.Activate();

                //PivotTableInitializer init = new PivotTableInitializer();
                //init.CreatePivotTable(pivotTableStructure, newWorksheet);
            };

            editor.Init(Globals.ThisAddIn.Receiver, Globals.ThisAddIn.ProjectConfig);
            ShowWindow(editor, "Select Pivot Table Template", true);
        }

        public void OnBusinessDictionaryEditButton(Office.IRibbonControl control)
        {          
            try
            {
                if (_treeFilterWindow != null)
                {
                    _treeFilterWindow.Close();
                }

                if (Globals.ThisAddIn.ProjectConfig == null)
                {
                    return;
                }

                Clients.Controls.Dialogs.TreeFilterSelector.TreeFilterSelector filterSelector =
                    new Clients.Controls.Dialogs.TreeFilterSelector.TreeFilterSelector(Globals.ThisAddIn.ProjectConfig, Globals.ThisAddIn.Receiver);
                if (!SetInteractiveFalseWitWarning())
                {
                    return;
                }

                filterSelector.CancelButtonClicked += FilterSelector_CancelButtonClicked;
                filterSelector.OkButtonClicked += FilterSelector_OkButtonClicked;

                var pane = ShowWindow(filterSelector, "Element Filter", true);
                _treeFilterWindow = pane;
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }


        }

        private void FilterSelector_OkButtonClicked(object sender, TreeFilterEventArgs e)
        {
            try
            {
                if (_treeFilterWindow != null)
                {
                    _treeFilterWindow.Close();
                }
                DisplayElementDictionaryListing(e);
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }


        private void DisplayElementDictionaryListing(TreeFilterEventArgs filter)
        {

            SetStatusBar("Loading business dictionary");
            Task.Factory.StartNew(() => { DisplayElementDictionaryListingInner(filter); });
        }



        private void DisplayElementDictionaryListingInner(TreeFilterEventArgs filter)
        {
            var projectId = Globals.ThisAddIn.ProjectConfig.ProjectConfigId;
            var elementsList = Globals.ThisAddIn.InspectManager.ListElementsUnderPath(projectId, filter.SelectedRefPath, filter.SelectedType.ElementType).OrderBy(x => x.RefPath).ToList();
            var viewList = Globals.ThisAddIn.AnnotationManager.ListProjectViews(projectId);

            var viewId = viewList.First(x => x.ViewName == "Default").AnnotationViewId;
            var typeView = viewList.FirstOrDefault(x => x.ViewName == "Type_" + filter.SelectedType.ElementType);
            if (typeView != null)
            {
                viewId = typeView.AnnotationViewId;
            }

            var viewFields = Globals.ThisAddIn.AnnotationManager.ListViewFields(viewId).OrderBy(x => x.FieldOrder).ToList();
            var fieldData = Globals.ThisAddIn.AnnotationManager.GetViewFieldValues(viewId, filter.SelectedRefPath, filter.SelectedType.ElementType);
            _businessDictionarylinks = Globals.ThisAddIn.AnnotationManager.GetLinksFrom(projectId, filter.SelectedType.ElementType, filter.SelectedRefPath);
            var fieldDataDictionaryTemp = fieldData.GroupBy(x => x.ModelElementId).ToDictionary(x => x.Key, y => y);
            var fieldDataDictionary = fieldDataDictionaryTemp.ToDictionary(x => x.Key.Value, x => x.Value.ToDictionary(y => y.FieldId, y => y));
            var excel = Globals.ThisAddIn.Application.ActiveWorkbook;

            foreach (var kv in _dataDictionaryEditors)
            {
                if (kv.Value.FilterValues.SelectedType.NodeType == filter.SelectedType.NodeType
                    && kv.Value.FilterValues.SelectedRefPath == filter.SelectedRefPath)
                {
                    var ws = kv.Value.Sheet;
                    foreach (var sheet in excel.Sheets)
                    {
                        if (sheet.Equals(ws))
                        {
                            ws.Activate();
                            SetStatusBar(string.Empty);
                            return;
                        }
                    }                  
                    
                }
            }
            

            Worksheet newWorksheet = CreateWorksheet("BusinessDictionary");
            newWorksheet.Activate();

            var topRng = newWorksheet.Range["A1"];
            topRng.Value = "Business definitions of " + filter.SelectedType.TypeDescription + "s in " + filter.SelectedRefPath;

            string[,] sheetValues = new string[elementsList.Count + 1, 4 + viewFields.Count];
            //sheetValues[0] = new string[4 + viewFields.Count];
            sheetValues[0, 0] = "ModelElementId";
            sheetValues[0, 1] = "RefPath";
            sheetValues[0, 2] = "ElementType";
            sheetValues[0, 3] = "ElementName";
            var fldIdx = 0;
            foreach (var fld in viewFields)
            {
                sheetValues[0, 4 + fldIdx] = fld.FieldName;
                fldIdx++;
            }

            int rowIdx = 1;
            foreach (var element in elementsList)
            {
                //var rowVals = new string[4 + viewFields.Count];
                sheetValues[rowIdx, 0] = element.ModelElementId.ToString();
                sheetValues[rowIdx, 1] = element.RefPath;
                sheetValues[rowIdx, 2] = element.TypeDescription;
                sheetValues[rowIdx, 3] = element.Caption;

                for (int i = 0; i < viewFields.Count; i++)
                {
                    if (fieldDataDictionary.ContainsKey(element.ModelElementId))
                    {
                        if (fieldDataDictionary[element.ModelElementId].ContainsKey(viewFields[i].FieldId))
                        {
                            sheetValues[rowIdx, 4 + i] = fieldDataDictionary[element.ModelElementId][viewFields[i].FieldId].Value;
                        }
                    }
                    //else
                    //{
                    //    sheetValues[rowIdx, 4 + i] = string.Empty;
                    //}

                }
                //sheetValues[rowIdx] = rowVals;
                rowIdx++;
            }

            //var startCell = "A3";
            var startCell = (Range)(newWorksheet.Cells[3, 1]); // GetExcelColumnName(sheetValues[0].Length);
            var contentStartCell = (Range)(newWorksheet.Cells[4, 1]); // GetExcelColumnName(sheetValues[0].Length);
            var endCell = (Range)(newWorksheet.Cells[3 + elementsList.Count, 4 + viewFields.Count]); //endColumn + (3 + elementsList.Count).ToString();
            var dataRange = newWorksheet.Range[startCell, endCell];
            dataRange.Value = sheetValues;
            dataRange.Columns.AutoFit();
            dataRange.Style = "Normal";

            var contentRange = newWorksheet.Range[contentStartCell, endCell];

            var topRange = newWorksheet.Range[startCell, (Range)(newWorksheet.Cells[3, 4 + viewFields.Count])];
            topRange.Interior.Color = XlRgbColor.rgbLightSlateGray;
            topRange.Font.Color = XlRgbColor.rgbWhite;
            topRange.Font.Bold = true;

            var leftRange = newWorksheet.Range[(Range)(newWorksheet.Cells[4, 1]), (Range)(newWorksheet.Cells[3 + elementsList.Count, 4])];
            leftRange.Font.Color = XlRgbColor.rgbWhite;
            leftRange.Interior.Color = XlRgbColor.rgbGray;
            //FormatCondition leftFormat = leftRange.Rows.FormatConditions.Add(XlFormatConditionType.xlExpression, XlFormatConditionOperator.xlEqual, "=MOD(ROW(),2) = 0");
            //leftFormat.Interior.Color = XlRgbColor.rgbDimGray;

            var rightRange = newWorksheet.Range[(Range)(newWorksheet.Cells[4, 5]), endCell];
            rightRange.Font.Color = XlRgbColor.rgbBlack;
            rightRange.Interior.Color = XlRgbColor.rgbWhite;
            //FormatCondition format = rightRange.Rows.FormatConditions.Add(XlFormatConditionType.xlExpression, XlFormatConditionOperator.xlEqual, "=MOD(ROW(),2) = 0");
            //format.Interior.Color = XlRgbColor.rgbLightGray;



            //foreach (Microsoft.Office.Interop.Excel.Style s in excel.Styles)
            //{
            //    var n = s.Name;
            //}

            int colOffset = 4;
            Dictionary<int, AnnotationViewField> offsetsDictionary = new Dictionary<int, AnnotationViewField>();
            foreach (var fld in viewFields)
            {
                offsetsDictionary[colOffset++] = fld;
            }

            DataDictionaryEditorLocation dictionaryLocation = new DataDictionaryEditorLocation()
            {
                Sheet = newWorksheet,
                ModelElementIdColumnOffset = 0,
                DataRange = contentRange,
                FieldOffsets = offsetsDictionary,
                FieldValues = fieldDataDictionary,
                FilterValues = filter
            };

            _dataDictionaryEditors.Remove(newWorksheet.Name);

            _dataDictionaryEditors.Add(newWorksheet.Name, dictionaryLocation);

            SetStatusBar(string.Empty);

        }

        private Worksheet CreateWorksheet(string defaultName)
        {
            Worksheet newWorksheet;
            var excel = Globals.ThisAddIn.Application.ActiveWorkbook;
            newWorksheet = (Worksheet)excel.Worksheets.Add();

            var baseName = defaultName;
            int sheetNo = 1;
            var sheetsHash = new HashSet<string>();
            int lenghtOfName = baseName.Length;
            foreach (Worksheet sh in excel.Worksheets)
            {
                sheetsHash.Add(sh.Name);
            }

            while (sheetsHash.Contains(baseName + sheetNo.ToString()))
            {
                sheetNo++;
            }
            
            
            var adjustedSheetName = baseName.Replace(':', ' ').Replace('\\', ' ').Replace('?', ' ').Replace('*', ' ').Replace('.', ' ').Replace('/', ' ');
            

            if(lenghtOfName >= 31)
            {
                adjustedSheetName.Remove(30);
                newWorksheet.Name = adjustedSheetName.Remove(30) + sheetNo.ToString(); ;
                return newWorksheet;
            }
            else
            {
                newWorksheet.Name = adjustedSheetName + sheetNo.ToString(); ;
                return newWorksheet;
            }

            
        }

        public void OnBusinessDictionarySaveButton(Office.IRibbonControl control)
        {
            try
            {
                if (Globals.ThisAddIn.ProjectConfig == null)
                {
                    return;
                }

                var wb = Globals.ThisAddIn.Application.ActiveWorkbook;
                Worksheet currentSheet = wb.ActiveSheet;
                var currentSheetName = currentSheet.Name;
                if (!_dataDictionaryEditors.ContainsKey(currentSheetName))
                {
                    MessageBox.Show("The current sheet does not contain a business dictionary editor", "No dictionary on this sheet", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dictionaryLocation = _dataDictionaryEditors[currentSheetName];
                var userId = IdentityProvider.GetCurrentUser().UserId; // ApplicationData.GetUser().UserId;
                var projectConfigId = Globals.ThisAddIn.ProjectConfig.ProjectConfigId;
                var dataRange = dictionaryLocation.DataRange;

                var values = (System.Array)dataRange.Cells.Value;

                List<AnnotationViewFieldValue> newFieldValues = new List<AnnotationViewFieldValue>();

                for (int r = 0; r < dataRange.Rows.Count; r++)
                {
                    var elementId = int.Parse((string)(values.GetValue(r + 1, dictionaryLocation.ModelElementIdColumnOffset + 1)));
                    foreach (var fieldOffset in dictionaryLocation.FieldOffsets.Keys)
                    {

                        AnnotationViewFieldValue originalValueWrap = null;
                        var field = dictionaryLocation.FieldOffsets[fieldOffset];
                        var value = values.GetValue(r + 1, fieldOffset + 1) as string;
                        string originalValue = null;
                        if (dictionaryLocation.FieldValues.ContainsKey(elementId))
                        {
                            if (dictionaryLocation.FieldValues[elementId].ContainsKey(field.FieldId))
                            {
                                originalValue = dictionaryLocation.FieldValues[elementId][field.FieldId].Value;
                                originalValueWrap = dictionaryLocation.FieldValues[elementId][field.FieldId];
                            }
                        }

                        if (string.IsNullOrWhiteSpace(originalValue) && string.IsNullOrWhiteSpace(value))
                        {
                            continue;
                        }
                        if (originalValue == value)
                        {
                            continue;
                        }

                        if (value == null)
                        {
                            value = string.Empty;
                        }

                        newFieldValues.Add(new AnnotationViewFieldValue()
                        {
                            FieldId = field.FieldId,
                            ModelElementId = elementId,
                            Value = value
                        });

                        if (originalValueWrap != null)
                        {
                            // prevent this from being considered a change again
                            originalValueWrap.Value = value;
                        }
                    }

                }
                List<int> changedElements = newFieldValues.Where(x => x.ModelElementId.HasValue)
                    .Select(x => x.ModelElementId.Value).Distinct().ToList();
                var changedLinks = _businessDictionarylinks.Where(x => changedElements.Contains(x.ModelElementFromId)).ToList();
                Globals.ThisAddIn.AnnotationManager.UpdateElementFields(newFieldValues, projectConfigId, userId, changedLinks, changedElements);
                MessageBox.Show("Values saved successfully", "Values saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        public void OnReportButton(Office.IRibbonControl control)
        {
            ReportSelector reportSelector = new ReportSelector(Globals.ThisAddIn.ProjectConfig);
            if (!SetInteractiveFalseWitWarning())
            {
                return;
            }

            var executionService = new ReportExecutionService();
            executionService.Credentials = CredentialCache.DefaultCredentials;
            SsrsProjectComponent ssrsComponent = null;

            var selectorPane = ShowWindow(reportSelector, "Select report", false);
            reportSelector.ItemSelected += (o, e) =>
            {
                var selectedItem = reportSelector.SelectedReport;
                ssrsComponent = Globals.ThisAddIn.ProjectConfig.SsrsComponents.First(x => x.SsrsProjectComponentId == selectedItem.SsrsComponentId);
                executionService.Url = ssrsComponent.SsrsExecutionServiceUrl;
                ExecutionInfo execInfo = new ExecutionInfo();
                //var previousCursor = Mouse.OverrideCursor;

                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;

                    ExecutionHeader execHeader = new ExecutionHeader();
                    executionService.ExecutionHeaderValue = execHeader;
                    string historyID = null;
                    execInfo = executionService.LoadReport(selectedItem.SsrsPath, historyID);

                    Mouse.OverrideCursor = Cursors.Arrow;
                    selectorPane.Close();

                    ParameterSelector parameterSelector = new ParameterSelector(selectedItem.SsrsPath, executionService);

                    Globals.ThisAddIn.Application.Interactive = false;

                    var parametersPane = ShowWindow(parameterSelector, "Select parameters", false);
                    parameterSelector.RenderReportClick += (o1, e1) =>
                    {
                        Mouse.OverrideCursor = Cursors.Wait;

                        string reportXmlPath = null;
                        var renderedReportPath = RenderReport(executionService, selectedItem, parameterSelector, out reportXmlPath);

                        parametersPane.Close();
                        Workbook workbook = null;

                        // rendering failed
                        if (renderedReportPath == null)
                        {
                            return;
                        }
                        else
                        {
                            // open the rendered report as new workbook
                            workbook = Globals.ThisAddIn.Application.Workbooks.Open(renderedReportPath);

                        }

                        if (parameterSelector.MapLineage)
                        {
                            Task.Factory.StartNew(() => MapReportLineage(workbook, reportXmlPath, selectedItem));
                        }
                    };
                }
                catch (Exception ex)
                {
                    Mouse.OverrideCursor = Cursors.Arrow;
                    MessageBox.Show(string.Format("Could not load report {0}: {1}", selectedItem.SsrsPath, ex.Message
                        //+ (ex.InnerException == null ? "" : (Environment.NewLine + ex.InnerException.Message))                        
                        ), "Could not load report", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            };
           
        }

        private bool SetInteractiveFalseWitWarning()
        {
            try
            {
                Globals.ThisAddIn.Application.Interactive = false;
                return true;
            }
            catch (COMException ex)
            {
                MessageBox.Show("Cannot use this feature while the formula bar in Excel is active. Please select another cell in the worksheet and repeat the action.",
                    "Formula Bar Active", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        private async Task MapReportLineage(Workbook workbook, string reportXmlPath, SsrsReportListItem report)
        {
            try
            {
                Globals.ThisAddIn.RenderedReportWorkbook = workbook;
                Globals.ThisAddIn.RenderedReportTextBoxPositions = new Dictionary<string, Dictionary<int, Dictionary<int, ReportElementAbsolutePosition>>>();

                SetStatusBar("DLS: Analysing report lineage");
                ServiceHelper serviceHelper = new ServiceHelper(Globals.ThisAddIn.Receiver, ConfigManager.ServiceReceiverId, Globals.ThisAddIn.ProjectConfig);

                SetStatusBar("DLS: Reading RDL");
                var itemPositions = await serviceHelper.PostRequest(new ReportItemPositionsRequest() { ReportElementId = report.ModelElementId });

                SetStatusBar("DLS: Reading report data");
                XmlDocument renderedXml = new XmlDocument();
                using (FileStream fs = new FileStream(reportXmlPath, FileMode.Open))
                {
                    renderedXml.Load(fs);
                }

                TablixValueRenderer renderer = new TablixValueRenderer();
                TablixValueMapper mapper = new TablixValueMapper(workbook);

                var tablixes = renderer.FindTablixes(itemPositions.RootElement);
                foreach (var tablix in tablixes)
                {
                    SetStatusBar("DLS: Rendering tablix " + tablix.Name);
                    var valuesTable = renderer.GetTablixValuesTable(tablix, renderedXml);

                    SetStatusBar("DLS: Locating tablix " + tablix.Name);
                    var tablixFound = mapper.FindTablix(valuesTable);

                    if (tablixFound)
                    {
                        SetStatusBar("DLS: Mapping tablix " + tablix.Name);

                        for (int i = 0; i < valuesTable.GetLength(0); i++)
                        {
                            for (int j = 0; j < valuesTable.GetLength(1); j++)
                            {
                                if (valuesTable[i, j] != null)
                                {
                                    var tb = valuesTable[i, j];
                                    if (!string.IsNullOrWhiteSpace(tb.Value))
                                    {
                                        if (!Globals.ThisAddIn.RenderedReportTextBoxPositions.ContainsKey(tb.RenderedSheet))
                                        {
                                            Globals.ThisAddIn.RenderedReportTextBoxPositions.Add(tb.RenderedSheet, new Dictionary<int, Dictionary<int, ReportElementAbsolutePosition>>());
                                        }
                                        if (!Globals.ThisAddIn.RenderedReportTextBoxPositions[tb.RenderedSheet].ContainsKey(tb.RenderedRow))
                                        {
                                            Globals.ThisAddIn.RenderedReportTextBoxPositions[tb.RenderedSheet].Add(tb.RenderedRow, new Dictionary<int, ReportElementAbsolutePosition>());
                                        }

                                        if (Globals.ThisAddIn.RenderedReportTextBoxPositions[tb.RenderedSheet][tb.RenderedRow].ContainsKey(tb.RenderedColumn))
                                        {
                                            DumpTablixValues(valuesTable, tablix);

                                            var origTb = Globals.ThisAddIn.RenderedReportTextBoxPositions[tb.RenderedSheet][tb.RenderedRow][tb.RenderedColumn];
                                            ConfigManager.Log.Error(string.Format("Duplicate cell mapping: [{0},{1},{2}]: orig: {3}, new {4}, value: {5}",
                                                tb.RenderedSheet, tb.RenderedRow, tb.RenderedColumn, origTb.Name, tb.RdlPosition.Name, tb.Value));
                                            //                    MessageBox.Show(string.Format("Duplicate cell mapping: [{0},{1},{2}]: orig: {3}, new {4}, value: {5}",
                                            //                        tb.RenderedSheet, tb.RenderedRow, tb.RenderedColumn, origTb.Name, tb.RdlPosition.Name, tb.Value),
                                            //"Report Analysis Error", MessageBoxButton.OK, MessageBoxImage.Warning);

                                            return;
                                        }
                                        else
                                        {
                                            Globals.ThisAddIn.RenderedReportTextBoxPositions[tb.RenderedSheet][tb.RenderedRow].Add(tb.RenderedColumn, tb.RdlPosition);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        ConfigManager.Log.Warning(string.Format("Tablix {0} not found", tablix.Name));
                        DumpTablixValues(valuesTable, tablix);
                    }
                }

                SetStatusBar(string.Empty);
            }
            catch (Exception ex)
            {
                ConfigManager.Log.Error(string.Format("Report Lineage Analysis Failed: {0} \n\n {1}", ex.Message, ex.StackTrace));
                //MessageBox.Show(string.Format("Report Lineage Analysis Failed: {0} \n\n {1}", ex.Message, ex.StackTrace),
                //    "Report Analysis Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                SetStatusBar(string.Empty);
            }
        }

        private void DumpTablixValues(ReportTextBoxValue[,] valuesTable, ReportElementAbsolutePosition tablix)
        {
            ConfigManager.Log.Warning("Tablix values dump: " + tablix.Name);

            for (int i = 0; i < valuesTable.GetLength(0); i++)
            {
                for (int j = 0; j < valuesTable.GetLength(1); j++)
                {
                    if (valuesTable[i, j] != null)
                    {
                        var val = valuesTable[i, j];
                        ConfigManager.Log.Warning(string.Format("[{0}, {1}]: [{2}, {3}]",
                            val.RenderedRow == 0 ? val.Y : val.RenderedRow, 
                            val.RenderedColumn == 0 ? val.X : val.RenderedColumn, 
                            val.RdlPosition.Name, 
                            val.Value));
                    }
                }
            }

            ConfigManager.Log.Warning("Tablix positions dump: " + tablix.Name);

            foreach (var tb in tablix.GetDisplayableItemsLeftRight())
            {
                ConfigManager.Log.Warning(string.Format("{0}: Left: {1}, Top: {2}, Width: {3}, Height: {4}, Value: {5}",
                    tb.Name, tb.Left, tb.Top, tb.Width, tb.Height, tb.Text));
            }

        }

        private string RenderReport(ReportExecutionService executionService, SsrsReportListItem report, ParameterSelector parameterSelector, out string renderedXmlPath)
        {

            // Render arguments
            byte[] result = null;
            string format = "EXCELOPENXML";

            byte[] xmlResult = null;
            string xmlFormat = "XML";
            renderedXmlPath = null;

            /*
            switch (request.ReportFormat)
            {
                case RenderReportRequest.ReportFormatEnum.Excel:
                    //format = "EXCELOPENXML"; // .xlsx; "EXCEL" for .xls
                    format = "EXCELOPENXMLCDF"; // .xlsx; "EXCEL" for .xls
                    attachmentType = AttachmentTypeEnum.Excel;
                    break;
                case RenderReportRequest.ReportFormatEnum.Nhtml:
                    format = "NTHML";
                    attachmentType = AttachmentTypeEnum.Html;
                    break;
                case RenderReportRequest.ReportFormatEnum.Xml:
                    format = "XML";
                    attachmentType = AttachmentTypeEnum.XML;
                    break;
                case RenderReportRequest.ReportFormatEnum.ReportDataMap:
                    format = "XML";
                    attachmentType = AttachmentTypeEnum.JSON;
                    break;

            }
            */

            string historyID = null;
            string devInfo = @"<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";
            var parameters = parameterSelector.GetParametersArray();

            DataSourceCredentials[] credentials = null;
            string showHideToggle = null;
            string encoding;
            string mimeType;
            string resourceMimeType;
            string extension;
            Warning[] warnings = null;
            ParameterValue[] reportHistoryParameters = null;
            string[] streamIDs = null;

            ExecutionInfo execInfo = new ExecutionInfo();
            ExecutionHeader execHeader = new ExecutionHeader();

            executionService.ExecutionHeaderValue = execHeader;

            execInfo = executionService.LoadReport(report.SsrsPath, historyID);

            executionService.SetExecutionParameters(parameters, "en-us");
            String SessionId = executionService.ExecutionHeaderValue.ExecutionID;

            ConfigManager.Log.Info(string.Format("Report Execution ({1}) SessionID: {0}", executionService.ExecutionHeaderValue.ExecutionID, report.SsrsPath));

            try
            {
                result = executionService.Render(format, devInfo, out extension, out encoding, out mimeType, out warnings, out streamIDs);
                if (parameterSelector.MapLineage)
                {
                    xmlResult = executionService.Render(xmlFormat, devInfo, out extension, out encoding, out mimeType, out warnings, out streamIDs);
                }
                execInfo = executionService.GetExecutionInfo();

                ConfigManager.Log.Info("Report execution date and time: {0}", execInfo.ExecutionDateTime);
            }
            catch (SoapException e)
            {
                ConfigManager.Log.Error(e.Detail.OuterXml);
                MessageBox.Show("Failed to render the report:" + Environment.NewLine + e.Detail.OuterXml, "Rendering report", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            Uri attachmentUri;
            // Write the contents of the report to an MHTML file.

            try
            {
                if (parameterSelector.MapLineage)
                {
                    using (MemoryStream ms = new MemoryStream(xmlResult))
                    {
                        var baseTemp = Path.GetTempFileName();
                        var tempDirectory = Path.GetDirectoryName(baseTemp);
                        var tempFileName = Path.GetFileNameWithoutExtension(baseTemp);
                        var tempPath = Path.Combine(tempDirectory, string.Format("{0}-{1}.xml", report.Name, tempFileName));
                        using (FileStream tgtStream = new FileStream(tempPath, FileMode.Create))
                        {
                            ms.CopyTo(tgtStream);
                        }
                        renderedXmlPath = tempPath;
                        //attachmentUri = _core.StorageProvider.Save(ms);
                    }
                }


                using (MemoryStream ms = new MemoryStream(result))
                {
                    var baseTemp = Path.GetTempFileName();
                    var tempDirectory = Path.GetDirectoryName(baseTemp);
                    var tempFileName = Path.GetFileNameWithoutExtension(baseTemp);
                    var tempPath = Path.Combine(tempDirectory, string.Format("{0}-{1}.xlsx", report.Name, tempFileName));
                    using (FileStream tgtStream = new FileStream(tempPath, FileMode.Create))
                    {
                        ms.CopyTo(tgtStream);
                    }
                    return tempPath;
                    //attachmentUri = _core.StorageProvider.Save(ms);
                }

            }
            catch (Exception e)
            {
                ConfigManager.Log.Error(e.Message);
                return null;
            }
        }

        private void FilterSelector_CancelButtonClicked(object sender, Clients.Controls.Dialogs.TreeFilterSelector.TreeFilterEventArgs e)
        {
            _treeFilterWindow.Close();
        }

        public bool GetBusinessDictionaryCheckBoxPressed(Office.IRibbonControl control)
        {
            return Globals.ThisAddIn.BusinessDictionaryPaneActive;
        }

        public void OnBusinessDictionaryCheckBoxAction(Office.IRibbonControl control, bool pressed)
        {
            Globals.ThisAddIn.BusinessDictionaryPaneActive = pressed;
        }


        private void SetStatusBar(string status)
        {
            try
            {
                Globals.ThisAddIn.Application.StatusBar = status;
            }
            catch
            {

            }
        }


        #region IRibbonExtensibility Members

        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("CD.Framework.ExcelAddin16.MainRibbon.xml");
        }

        #endregion

        #region Ribbon Callbacks
        //Create callback methods here. For more information about adding callback methods, visit https://go.microsoft.com/fwlink/?LinkID=271226

        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            this.ribbon = ribbonUI;
        }

        #endregion

        #region Helpers

        private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        private void ShowException(Exception ex)
        {
            try
            {
                var msg = ex.Message + Environment.NewLine + ex.StackTrace;
                if (ex.InnerException != null)
                {
                    msg += Environment.NewLine;
                    msg += ex.InnerException.Message;
                    msg += Environment.NewLine + ex.InnerException.StackTrace;
                }
                if (ex is AggregateException)
                {
                    var agg = ex as AggregateException;
                    if (agg.InnerExceptions != null)
                    {
                        foreach (var inner in agg.InnerExceptions)
                        {
                            msg += Environment.NewLine + inner.Message + Environment.NewLine + inner.StackTrace;
                        }
                    }
                }
                msg += Environment.NewLine + ex.GetType().Name;

                MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex2)
            {
                MessageBox.Show(ex2.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        #endregion
    }
}
