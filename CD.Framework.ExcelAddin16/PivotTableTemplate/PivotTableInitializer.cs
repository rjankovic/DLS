using CD.DLS.API.Structures;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CD.Framework.ExcelAddin16.PivotTableTemplate
{
    public class PivotTableInitializer
    {
        private Application _application;

        public PivotTableInitializer(Application application)
        {
            _application = application;
        }

        private PivotTable _pivotTable;
        //private Dictionary<string, CubeField> _cubeFields;

        public PivotTable CreatePivotTable(PivotTableStructure structure, Worksheet worksheet)
        {
            PivotTable pivotTable;
            try
            {
                PivotCache pivotCache =
                worksheet.Application.ActiveWorkbook.PivotCaches().Add(XlPivotTableSourceType.xlExternal, Missing.Value);
                pivotCache.Connection = structure.ConnectionString;
                pivotCache.MaintainConnection = true;
                pivotCache.CommandText = structure.CubeName;
                pivotCache.CommandType = XlCmdType.xlCmdCube;
                pivotCache.MissingItemsLimit = XlPivotTableMissingItems.xlMissingItemsNone;

                PivotTables pivotTables = (PivotTables)worksheet.PivotTables(Missing.Value);
                var cell = worksheet.Cells[1, 1];

                pivotCache.MakeConnection();

                pivotTable = pivotTables.Add(pivotCache, cell, "___dlstemp",
                    Missing.Value, Missing.Value);
                pivotTable.ManualUpdate = true;
            }
            catch
            {
                return null;
            }
                //pivotTable.PivotCache().MissingItemsLimit = XlPivotTableMissingItems.xlMissingItemsNone;
           
            //var cubeFieldCount = pivotTable.CubeFields.Count;



            //pivotCache.Refresh();


            //pivotTable.PivotCache().Refresh();

            try
            {
                var style = _application.ActiveWorkbook.TableStyles[structure.TableStyle];
                pivotTable.TableStyle2 = style;
            }
            catch
            {
                try
                {
                    var classicStyle = _application.ActiveWorkbook.TableStyles["PivotStyleLight16"];
                    pivotTable.TableStyle2 = classicStyle;
                }
                catch
                {
                }
            }
            //pivotTable.TableStyle = structure.TableStyle;



try {             pivotTable.LayoutRowDefault = (Microsoft.Office.Interop.Excel.XlLayoutRowType)Enum.Parse(typeof(Microsoft.Office.Interop.Excel.XlLayoutRowType), structure.LayoutRowDefault.ToString());
 } catch {}
try {             pivotTable.RowGrand = structure.RowGrand;
 } catch {}
try {             pivotTable.ColumnGrand = structure.ColumnGrand;
 } catch {}
try {             pivotTable.HasAutoFormat = structure.HasAutoFormat;
 } catch {}
try {             pivotTable.DisplayErrorString = structure.DisplayErrorString;
 } catch {}
try {             pivotTable.DisplayNullString = structure.DisplayNullString;
 } catch {}
try {             pivotTable.EnableDrilldown = structure.EnableDrilldown;
 } catch {}
try {             pivotTable.ErrorString = structure.ErrorString;
 } catch {}
try {             pivotTable.MergeLabels = structure.MergeLabels;
 } catch {}
try {             pivotTable.NullString = structure.NullString;
 } catch {}
try {             pivotTable.PageFieldOrder = structure.PageFieldOrder;
 } catch {}
try {             pivotTable.PageFieldWrapCount = structure.PageFieldWrapCount;
 } catch {}
try {             pivotTable.PreserveFormatting = structure.PreserveFormatting;
 } catch {}
try {             pivotTable.PrintTitles = structure.PrintTitles;
 } catch {}
try {             pivotTable.RepeatItemsOnEachPrintedPage = structure.RepeatItemsOnEachPrintedPage;
 } catch {}
try {             pivotTable.TotalsAnnotation = structure.TotalsAnnotation;
 } catch {}
try {             pivotTable.CompactRowIndent = structure.CompactRowIndent;
 } catch {}
try {             pivotTable.VisualTotals = structure.VisualTotals;
 } catch {}
try {             pivotTable.InGridDropZones = structure.InGridDropZones;
 } catch {}
try {             pivotTable.DisplayFieldCaptions = structure.DisplayFieldCaptions;
 } catch {}
try {             pivotTable.DisplayContextTooltips = structure.DisplayContextTooltips;
 } catch {}
try {             pivotTable.ShowDrillIndicators = structure.ShowDrillIndicators;
 } catch {}
try {             pivotTable.PrintDrillIndicators = structure.PrintDrillIndicators;
 } catch {}
try {             pivotTable.DisplayEmptyRow = structure.DisplayEmptyRow;
 } catch {}
try {             pivotTable.DisplayEmptyColumn = structure.DisplayEmptyColumn;
 } catch {}
try {             pivotTable.SortUsingCustomLists = structure.SortUsingCustomLists;
 } catch {}
try {             pivotTable.DisplayImmediateItems = structure.DisplayImmediateItems;
 } catch {}
try {             pivotTable.ViewCalculatedMembers = structure.ViewCalculatedMembers;
 } catch {}
try {             pivotTable.EnableWriteback = structure.EnableWriteback;
 } catch {}
try {             pivotTable.ShowValuesRow = structure.ShowValuesRow;
 } catch {}
try {             pivotTable.CalculatedMembersInFilters = structure.CalculatedMembersInFilters;
 } catch {}


            // pivotTable.DisplayMemberPropertyTooltips = structure.DisplayMemberPropertyTooltips;
            // pivotTable.AllowMultipleFilters = structure.AllowMultipleFilters;


            //}
            //catch
            //{
            //}

            //pivotTable.AddFields("[Date].[Year - Month].[Month]");

            //pivotTable.RefreshDataSourceValues();
            //var refreshResult = pivotTable.RefreshTable();

            PivotFields pfs = pivotTable.PivotFields();
            var count = pfs.Count;

            _pivotTable = pivotTable;
            //_cubeFields = new Dictionary<string, CubeField>();
            //foreach (CubeField cf in _pivotTable.CubeFields)
            //{
            //    _cubeFields.Add(cf.Name, cf);
            //    var cfn = cf.Name;
            //    //cf.CreatePivotFields();

            //    //foreach (PivotField cpf in cf.PivotFields)
            //    //{
            //    //    var name = cpf.Name;
            //    //    var sourceName = cpf.SourceName;
            //    //}

            //}
            
            SetValues(pivotTable, structure.VisibleFields, structure.ValuesOrientation);
            SetFilters(pivotTable, structure.VisibleFields);
            SetDimensionHierarchyOrientation(structure.VisibleFields);
            SetAxes(pivotTable, structure.VisibleFields);




            //        Excel.PivotField pageField =
            //(Excel.PivotField)pivotTable.PivotFields("SalesTerritory");


            //        pageField.Orientation = Excel.XlPivotFieldOrientation.xlPageField;


            //        Excel.PivotField rowField =
            //            (Excel.PivotField)pivotTable.PivotFields("FullName");


            //        rowField.Orientation = Excel.XlPivotFieldOrientation.xlRowField;

            //        pivotTable.AddDataField(


            //            pivotTable.PivotFields("2004"), "Sum of 2004", Excel.XlConsolidationFunction.xlSum);
            pivotTable.Name = "PivotTable1";
            pivotTable.ManualUpdate = false;
            return pivotTable;
        }

        private void SetValues(PivotTable pivotTable, List<PivotTableField> visibleFields, PivotFieldOrientation valuesOrientations)
        { 
            pivotTable.ManualUpdate = true;
            foreach (var measure in visibleFields.Where(x => x.Orientation == PivotFieldOrientation.Data))
            {
                CubeField cf = pivotTable.CubeFields[measure.CubeFieldName];
                cf.Orientation = XlPivotFieldOrientation.xlDataField;

                //PivotField dataField = (PivotField)pivotTable.PivotFields(measure.PivotFieldName);
                //dataField.Orientation = XlPivotFieldOrientation.xlDataField;
            }

            if (valuesOrientations != PivotFieldOrientation.Data)
            {
                foreach (PivotField rf in pivotTable.RowFields)
                {
                    if (rf.Value == "Data")
                    {
                        rf.Orientation =
                            valuesOrientations == PivotFieldOrientation.Row ? XlPivotFieldOrientation.xlRowField : XlPivotFieldOrientation.xlColumnField;
                    }
                }
                //PivotField valuesField = pivotTable.PivotFields("Values");
                //valuesField.Orientation = valuesOrientations == PivotFieldOrientation.Row ? XlPivotFieldOrientation.xlRowField : XlPivotFieldOrientation.xlColumnField;
            }
        }

        private void SetAxes(PivotTable pivotTable, List<PivotTableField> visibleFields)
        {
            pivotTable.ManualUpdate = true;
            foreach (var field in visibleFields.Where(x =>
                x.Orientation == PivotFieldOrientation.Column
                || x.Orientation == PivotFieldOrientation.Row))
            {
                CubeField cf = pivotTable.CubeFields[field.CubeFieldName];
                //cf.hie

                //cf.HiddenLevels
                //var hl = cf. cf.HiddenLevels;
                if (cf.CubeFieldType == XlCubeFieldType.xlHierarchy)
                {
                    cf.LayoutForm = XlLayoutFormType.xlOutline;
                    /*
                    if (field.CubeFieldName != field.PivotFieldName)
                    {
                        foreach (PivotField pf in cf.PivotFields)
                        {
                            PivotField tpf = _pivotTable.PivotFields(pf.Name);
                            var pfn = pf.Name;
                            pf.DrillTo(field.PivotFieldName);

                            var pfn1 = pf.Name;
                            if (field.PivotFieldName == pfn1)
                            {

                            }
                            else
                            {

                            }
                        }
                        
                    }
                */
                }


                /*Část k filtrování
                 PivotField pfield = _pivotTable.PivotFields(field.PivotFieldName);

                if (field.VisibleItems != null && field.VisibleItems.Count > 0)
                {
                   pfield.VisibleItemsList = field.VisibleItems.Select(x => x.ItemName).ToArray();
                    pfield.VisibleItemsList = ConvertToFoxArray(field.VisibleItems.Select(x => x.ItemName).ToArray());
                }
               */

                Dictionary<string, PivotField> pfs = new Dictionary<string, PivotField>();
                //cf.CreatePivotFields();

                cf.Orientation = field.Orientation == PivotFieldOrientation.Row ? XlPivotFieldOrientation.xlRowField : XlPivotFieldOrientation.xlColumnField;

                //pivotTable.RefreshTable();

                //int cnt = 0;
                //while (cnt < cf.PivotFields.Count)
                //{
                //foreach (PivotField pf in cf.PivotFields)
                //{
                //    PivotField tpf = _pivotTable.PivotFields(pf.Name);
                //var pfn = pf.Name;
                //pf.DrillTo(field.PivotFieldName);
                //_pivotTable.RefreshTable();
                //var drilledField = _pivotTable.PivotFields(field.PivotFieldName);

                ////var osh = pf.ShowDetail;
                //try
                //{
                //    var n = tpf.Name;
                //    tpf.DrillTo(field.PivotFieldName);
                //    tpf.ShowDetail = true;
                //}
                //catch
                //{ }

                //_pivotTable.RefreshTable();
                //foreach (PivotItem pi in tpf.PivotItems())
                //{
                //    var piName = pi.SourceName;
                //    var parentDetail = pi.ParentShowDetail;
                //    pi.ShowDetail = true;
                //}
                //}
                //}
                //foreach (PivotField pf in cf.PivotFields)
                //{
                //    pfs.Add(pf.SourceName, pf);
                //    //cf.hie
                //    //foreach (PivotItem ci in pf.ChildItems)
                //    //{
                //    //    var n1 = ci.SourceName;
                //    //    foreach (PivotItem ci2 in ci.ChildItems)
                //    //    {
                //    //        var n2 = ci2.SourceName;
                //    //    }
                //    //}
                //    //pf.Orientation = cf.Orientation;
                //    //pf.ChildField.Orientation = cf.Orientation;
                //    //cf.
                //    //pf.ShowDetail = true;
                //    //ExpandChildFields(pf, cf.Orientation);

                //}


                //var pfld = pfs.First().Value;
                //foreach (PivotItem item1 in pfld.VisibleItems)
                //{
                //    var item1N = item1.SourceName;
                //    try
                //    {
                //        foreach (PivotItem item2 in item1.ChildItems)
                //        {
                //            var item2N = item2.SourceName;
                //            //PivotField pf2 = item2.fie
                //        }
                //    }
                //    catch
                //    {

                //    }
                //}

                //axisField.Orientation = field.Orientation == PivotFieldOrientation.Row ? 
                //    XlPivotFieldOrientation.xlRowField : XlPivotFieldOrientation.xlColumnField;

                /*
                try
                {
                    PivotField axisField = (PivotField)pivotTable.PivotFields(field.PivotFieldName);
                    SetFieldFilters(axisField, field);
                }
                catch
                {
                }
                */
            }
        }

        private void ExpandChildFields(PivotField axisField, XlPivotFieldOrientation orientation)
        {

            try
            {
                if (axisField.ChildField != null)
                {
                    var child = axisField.ChildField;
                    child.Orientation = orientation;
                    ExpandChildFields(child, orientation);
                }
            }
            catch
            {

            }
        }

        //sets the same orientation to all attributes belonging to same hierarchy
        private void SetDimensionHierarchyOrientation(List<PivotTableField> visibleFields)
        {
            Dictionary<String, PivotFieldOrientation> hierarchyOrientation = new Dictionary<string, PivotFieldOrientation>();
            
            foreach (PivotTableField field in visibleFields)
            {
                if (field.Position == 1)
                {
                    if (!hierarchyOrientation.ContainsKey(field.Hierarchy.ToString()))
                    {
                        hierarchyOrientation.Add(field.Hierarchy.ToString(), field.Orientation);
                    }

                }
            }

            foreach (PivotTableField field in visibleFields)
            {
                if (!(field.Position == 1))
                {
                    PivotFieldOrientation orientation;
                    if (hierarchyOrientation.TryGetValue(field.Hierarchy.ToString(), out orientation))
                    {
                        field.Orientation = orientation;
                    }
                }
            }

        }

        private void SetFilters(PivotTable pivotTable, List<PivotTableField> visibleFields)
        {
            pivotTable.ManualUpdate = true;
            foreach (var filter in visibleFields.Where(x => x.Orientation == PivotFieldOrientation.Filter))
            {
                CubeField cf = pivotTable.CubeFields[filter.CubeFieldName /*.SourceName*/]; // pivotTable.CubeFields[filter.SourceName];

                cf.Orientation = XlPivotFieldOrientation.xlPageField;

                //if (filter.VisibleItems != null)
                //{
                //    if (filter.VisibleItems.Count > 1)
                //    {
                //        cf.EnableMultiplePageItems = true;
                //    }
                //}

                var cfType = cf.CubeFieldType;
                //List<string> pivotFieldnames = new List<string>();
                //foreach (PivotField pf in cf.PivotFields)
                //{
                //    var pfName = pf.SourceName;
                //    pivotFieldnames.Add(pfName);
                //    //if (cfType == XlCubeFieldType.xlHierarchy)
                //    //{
                //    //    pf.DrillTo(filter.PivotFieldName);
                //    //}
                //}
                //pivotTable.PivotCache().Refresh();
                //pivotTable.RefreshTable();

                PivotField pageField = (PivotField)pivotTable.PivotFields(filter.CubeFieldName);
                var orientation = pageField.Orientation;


                if (filter.VisibleItems != null && filter.VisibleItems.Any(x => x.ItemName != ""))
                {
                    //pageField.ClearAllFilters();
                    //try
                    //{
                    //    var multiItemsEnabled = pageField.EnableMultiplePageItems;
                    //}
                    //catch
                    //{
                    //}
                    string[] visibleItems = filter.VisibleItems.Select(x => x.ItemName).ToArray();
                    if (visibleItems.Length > 0)
                    {
                        pageField.CurrentPageName = visibleItems[0];
                    }
                    if (visibleItems.Length > 1)
                    {
                        //var cfa = pivotTable.cube.CubeFields(pageField.Name);
                        var cfa = pageField.CubeField;
                        cfa.EnableMultiplePageItems = true;

                        for (int i = 1; i < visibleItems.Length; i++)
                        {
                            pageField.AddPageItem(visibleItems[i]);
                        }
                    }

                    //var visibleFoxArray = ConvertToFoxArray(visibleItems);
                    //pageField.CurrentPageList = visibleItems;
                }
                //pageField.Orientation = XlPivotFieldOrientation.xlPageField;
                //SetFieldFilters(pageField, filter);
            }
        }

        private void SetFieldFilters(PivotField xlsField, PivotTableField structField)
        {
            
            if (structField.VisibleItems != null && structField.Filters.Count == 0)
            {
                _pivotTable.RefreshTable();

                //if (structField.Orientation == PivotFieldOrientation.Filter && structField.VisibleItems.Count == 1)
                //{
                //    xlsField.CurrentPageName = structField.VisibleItems[0].ItemName;
                //}
                //else
                //{

                try
                {
                    HashSet<string> visibleSourceNames = new HashSet<string>(structField.VisibleItems.Select(x => x.ItemName));
                    //List<PivotItem> itemsToHide = new List<PivotItem>();

                    var fieldName = xlsField.Name;
                    List<string> items = new List<string>();

                    var visibleArr = structField.VisibleItems.Select(x => x.ItemName).ToArray();


                    foreach (PivotItem pi in ((PivotField)_pivotTable.PivotFields(xlsField.Name)).PivotItems())
                    {
                        var itemName = pi.Name;
                        var srcName = pi.SourceName;
                        if (!visibleSourceNames.Contains(pi.SourceName))
                        {
                            pi.Visible = false;
                        }
                    }
                    //xlsField.VisibleItemsList = visibleArr;

                    foreach (PivotItem pi in xlsField.VisibleItems)
                    {
                        //foreach (PivotItem childItem in pi.ChildItems)
                        //{
                        //    var chiName = childItem.Name;
                        //}
                        items.Add(pi.Name);
                        //if (!visibleSourceNames.Contains(pi.SourceName))
                        //{
                        //    itemsToHide.Add(pi);
                        //}

                    }
                }
                catch
                {

                }
                //foreach (var item in itemsToHide)
                //{
                //    var itemName = item.Name;

                //    item.Visible = false;
                //}
                //var visibleItemsList = structField.VisibleItems.Select(x => x.ItemName).ToArray();
                //xlsField.VisibleItemsList = visibleItemsList;
                //}
            }

            foreach (var filter in structField.Filters)
            {
                XlPivotFilterType type = XlPivotFilterType.xlAfter;
                switch (filter.Type)
                {
                    case ValuesFilterType.LessThan:
                        type = XlPivotFilterType.xlValueIsLessThan;
                        break;
                    case ValuesFilterType.LessEqual:
                        type = XlPivotFilterType.xlValueIsLessThanOrEqualTo;
                        break;
                    case ValuesFilterType.Equal:
                        type = XlPivotFilterType.xlValueEquals;
                        break;
                    case ValuesFilterType.GreaterEqual:
                        type = XlPivotFilterType.xlValueIsGreaterThanOrEqualTo;
                        break;
                    case ValuesFilterType.Greater:
                        type = XlPivotFilterType.xlValueIsGreaterThan;
                        break;
                    case ValuesFilterType.Between:
                        type = XlPivotFilterType.xlValueIsBetween;
                        break;
                    case ValuesFilterType.NotBetween:
                        type = XlPivotFilterType.xlValueIsNotBetween;
                        break;
                    case ValuesFilterType.NotEqual:
                        type = XlPivotFilterType.xlValueDoesNotEqual;
                        break;
                }

                try
                {
                    // TODO

                    //var axisFieldName = xlsField.SourceName;
                    //_pivotTable.RefreshTable();
                    //PivotField pivotFld = _pivotTable.PivotFields(axisFieldName);
                    //var orientation = pivotFld.Orientation;
                    ////pivotFld.CubeField.
                    //pivotFld.PivotFilters.Add2(Type: XlPivotFilterType.xlCaptionEquals, Value1: "10");


                    //var dataField = _pivotTable.DataFields[filter.MeasureName];
                    //foreach (var flt in xlsField.PivotFilters)
                    //{

                    //}
                    //xlsField.ClearValueFilters();

                    //xlsField.PivotFilters.Add2(Type: XlPivotFilterType.xlCaptionEquals, Value1: "10");


                    //xlsField.PivotFilters.Add2(XlPivotFilterType.xlValueIsBetween, filter.MeasureName, "10",
                    //    "20",
                    //    Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);

                    //xlsField.PivotFilters.Add2(type, dataField, double.Parse(filter.Value1),
                    //    string.IsNullOrEmpty(filter.Value2) ? (object)Missing.Value : (double)double.Parse(filter.Value2),
                    //    Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                }
                catch
                {
                    continue;
                }
            }
        }

        private object[] ConvertFoxArray(Array arr)
        {
            if (arr.Rank != 1) throw new ArgumentException();
            object[] retval = new object[arr.GetLength(0)];
            for (int ix = arr.GetLowerBound(0); ix <= arr.GetUpperBound(0); ++ix)
                retval[ix - arr.GetLowerBound(0)] = arr.GetValue(ix);
            return retval;
        }

        private Array ConvertToFoxArray(string[] arr)
        {
            Array res = Array.CreateInstance(typeof(string), new int[] { arr.Length }, new int[] { 1 });
            for (int i = 0; i < arr.Length; i++)
            {
                res.SetValue(arr[i], i + 1);
            }

            return res;
        }

        
    }

}
