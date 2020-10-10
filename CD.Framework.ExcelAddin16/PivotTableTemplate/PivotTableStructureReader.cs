using CD.DLS.API.Structures;
using CD.DLS.DAL.Configuration;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.API.Structures;
using CD.DLS.Common.Tools;

namespace CD.Framework.ExcelAddin16.PivotTableTemplate
{
    public class PivotTableStructureReader
    {
        private Exception _error;
        private PivotTableStructure _structure;

        public Exception Error { get { return _error; } }


        public List<string> ListPivotFields(PivotTable pivotTable)
        {
            List<string> res = new List<string>();
            foreach (PivotField pageField in pivotTable.PageFields)
            {
                try
                {
                    var currentPage = pageField.CurrentPageName;
                    res.Add(currentPage);
                }
                catch
                {

                }
            }

            foreach (PivotField columnField in pivotTable.ColumnFields)
            {
                res.Add(columnField.SourceName);
            }
            foreach (PivotField rowField in pivotTable.RowFields)
            {
                res.Add(rowField.SourceName);
            }
            foreach (PivotField measureField in pivotTable.DataFields)
            {
                res.Add(measureField.SourceName);
            }

            return res;
        }

        public PivotTableStructure ReadStructure(PivotTable pivotTable)
        {
            //PivotFields pfs = pivotTable.PivotFields();
            //var count = pfs.Count;

            //pivotTable.
            try
            {
                PivotTableStructure structure = new PivotTableStructure();
                _structure = structure;

                structure.ValuesOrientation = PivotFieldOrientation.Data;

                try
                {
                    var cache = pivotTable.PivotCache();
                    if (cache.CommandType == XlCmdType.xlCmdCube)
                    {
                        structure.ConnectionType = PivotTableConnectionType.Multidimensional;
                    }
                    /*
                    else if (cache.CommandType == XlCmdType.xlCmdDAX)
                    {
                        structure.ConnectionType = PivotTableConnectionType.Tabular;
                    }
                    */
                    else
                    {
                        _error = new Exception(
                            string.Format("Connection type {0} is not supported - only OLAP-based pivot tables can be saved as templates.", 
                            cache.CommandType.ToString()));
                        return null;
                    }
                    var connectionString = (string)cache.Connection.ToString();
                    var command = cache.CommandText.ToString();
                    var normalizedConnectionString = ConnectionStringTools.NormalizeServerInConnectionString(connectionString);
                    structure.ConnectionString = normalizedConnectionString;
                    structure.CubeName = command;
                    
                }
                catch (Exception ex)
                {
                    var err = "Could not find the OLAP connection in the pivot table - " + ex.Message;
                    ConfigManager.Log.Error("Could not find the OLAP connection in the pivot table - " + ex.Message);
                    var nex = new Exception(err, ex);
                    _error = nex;
                    return null;
                }

                structure.VisibleFields = new List<PivotTableField>();

                foreach (PivotField pageField in pivotTable.PageFields)
                {
                    PivotTableField filterField = ReadFilterField(pageField);
                    
                    if (filterField != null)
                    {
                        filterField.Orientation = PivotFieldOrientation.Filter;
                        structure.VisibleFields.Add(filterField);
                    }
                }

                foreach (PivotField columnField in pivotTable.ColumnFields)
                {
                    PivotTableField columnFld = ReadAxisField(columnField);
                    
                    if (columnFld != null)
                    {
                        columnFld.Orientation = PivotFieldOrientation.Column;
                        structure.VisibleFields.Add(columnFld);
                    }
                }

                foreach (PivotField rowField in pivotTable.RowFields)
                {
                    PivotTableField rowFld = ReadAxisField(rowField);
                    
                    if (rowFld != null)
                    {
                        rowFld.Orientation = PivotFieldOrientation.Row;
                        structure.VisibleFields.Add(rowFld);
                    }
                }

                foreach (PivotField dataField in pivotTable.DataFields)
                {
                    PivotTableField dataFld = ReadDataField(dataField);
                    
                    if (dataFld != null)
                    {
                        dataFld.Orientation = PivotFieldOrientation.Data;
                        structure.VisibleFields.Add(dataFld);
                    }
                }

                var    style =  pivotTable.TableStyle;
                var    style2 = (TableStyle)pivotTable.TableStyle2;
                var    style2Name = style2.Name;
                var    rowAxisLayout = pivotTable.LayoutRowDefault;
                var    columnGrand = pivotTable.ColumnGrand;
                var    rowGrand = pivotTable.RowGrand;
                bool   hasAutoFormat = pivotTable.HasAutoFormat;
                bool   displayErrorString = pivotTable.DisplayErrorString;
                bool   displayNullString = pivotTable.DisplayNullString;
                bool   enableDrilldown = pivotTable.EnableDrilldown;
                string errorString = pivotTable.ErrorString;
                bool   mergeLabels = pivotTable.MergeLabels;
                string nullString = pivotTable.NullString;
                int    pageFieldOrder = pivotTable.PageFieldOrder;
                int    pageFieldWrapCount = pivotTable.PageFieldWrapCount;
                bool   preserveFormatting = pivotTable.PreserveFormatting;
                bool   printTitles = pivotTable.PrintTitles;
                bool   repeatItemsOnEachPrintedPage = pivotTable.RepeatItemsOnEachPrintedPage;
                bool   totalsAnnotation = pivotTable.TotalsAnnotation;
                int    compactRowIndent = pivotTable.CompactRowIndent;
                bool   visualTotals = pivotTable.VisualTotals;
                bool   inGridDropZones = pivotTable.InGridDropZones;
                bool   displayFieldCaptions = pivotTable.DisplayFieldCaptions;
                bool   displayMemberPropertyTooltips = pivotTable.DisplayMemberPropertyTooltips;
                bool   displayContextTooltips = pivotTable.DisplayContextTooltips;
                bool   showDrillIndicators = pivotTable.ShowDrillIndicators;
                bool   printDrillIndicators = pivotTable.PrintDrillIndicators;
                bool   displayEmptyRow = pivotTable.DisplayEmptyRow;
                bool   displayEmptyColumn = pivotTable.DisplayEmptyColumn;
                bool allowMultipleFilters = false;
                try
                {
                    allowMultipleFilters = pivotTable.AllowMultipleFilters;
                }
                catch
                {
                }
                bool   sortUsingCustomLists = pivotTable.SortUsingCustomLists;
                bool   displayImmediateItems = pivotTable.DisplayImmediateItems;
                bool   viewCalculatedMembers = pivotTable.ViewCalculatedMembers;
                bool   enableWriteback = pivotTable.EnableWriteback;
                bool   showValuesRow = pivotTable.ShowValuesRow;
                bool calculatedMembersInFilters = false;
                try
                {
                    calculatedMembersInFilters = pivotTable.CalculatedMembersInFilters;
                }
                catch
                {
                }

                structure.TableStyle = style2Name;
                structure.LayoutRowDefault = (DLS.API.Structures.XlLayoutRowType)Enum.Parse(typeof(DLS.API.Structures.XlLayoutRowType), rowAxisLayout.ToString());
                structure.ColumnGrand = columnGrand;
                structure.RowGrand = rowGrand;
                structure.HasAutoFormat = hasAutoFormat;
                structure.DisplayErrorString = displayErrorString;
                structure.DisplayNullString = displayNullString;
                structure.EnableDrilldown = enableDrilldown;
                structure.ErrorString = errorString;
                structure.MergeLabels = mergeLabels;
                structure.NullString = nullString;
                structure.PageFieldOrder = pageFieldOrder;
                structure.PageFieldWrapCount = pageFieldWrapCount;
                structure.PreserveFormatting = preserveFormatting;
                structure.PrintTitles = printTitles;
                structure.RepeatItemsOnEachPrintedPage = repeatItemsOnEachPrintedPage;
                structure.TotalsAnnotation = totalsAnnotation;
                structure.CompactRowIndent = compactRowIndent;
                structure.VisualTotals = visualTotals;
                structure.InGridDropZones = inGridDropZones;
                structure.DisplayFieldCaptions = displayFieldCaptions;
                //structure.DisplayMemberPropertyTooltips = displayMemberPropertyTooltips;
                structure.DisplayContextTooltips = displayContextTooltips;
                structure.ShowDrillIndicators = showDrillIndicators;
                structure.PrintDrillIndicators = printDrillIndicators;
                structure.DisplayEmptyRow = displayEmptyRow;
                structure.DisplayEmptyColumn = displayEmptyColumn;
                structure.AllowMultipleFilters = allowMultipleFilters;
                structure.SortUsingCustomLists = sortUsingCustomLists;
                structure.DisplayImmediateItems = displayImmediateItems;
                structure.ViewCalculatedMembers = viewCalculatedMembers;
                structure.EnableWriteback = enableWriteback;
                structure.ShowValuesRow = showValuesRow;
                structure.CalculatedMembersInFilters = calculatedMembersInFilters;

                //workbook.TableStyles[BuiltInPivotStyleId.PivotStyleMedium3];


                return structure;
            }
            catch (Exception e)
            {
                _error = e;
                return null;
            }
        }
        //    foreach (PivotField field in pivotTable.PageFields /*pivotTable.PivotFields()*/ /*pivotTable.VisibleFields*/)
        //    {

        //        //foreach (Slicer slicer in pivotTable.Slicers)
        //        //{

        //        //}
        //        try
        //        {
        //            var pageCol = pivotTable.PageRange.Column;
        //        var pageRow = pivotTable.PageRange.Row;

        //            //var page = field.CurrentPage;
        //            try
        //            {
        //                var pageName = field.CurrentPageName;
                        
        //            }
        //            catch
        //            { }
        //            try
        //            {
        //                var pageList = field.CurrentPageList;

        //                var pageListStd = ConvertFoxArray(pageList);
        //                foreach (var pageItem in pageListStd)
        //                {

        //                }
        //            }
        //            catch
        //            { }
        //            try

        //            {
        //                var page = field.CurrentPage;
        //            }
        //            catch
        //            { }

        //            try

        //            {
        //                var arr = field.VisibleItemsList as Array;
        //                if (arr != null)
        //                {
        //                    var stdArr = ConvertFoxArray(arr);

        //                    foreach (var visItem in stdArr)
        //                    {

        //                    }
        //                }
        //            }
        //            catch
        //            {

        //            }

        //            try
        //            {
        //                foreach (var visItem in field.VisibleItems)
        //                {
                            
        //                }
        //            }
        //            catch
        //            { }


        //        var position = field.Position;
        //        var fieldName = field.Name;
        //        //if (!fieldName.Contains("WM Year"))
        //        //{
        //        //    continue;
        //        //}

        //        var value = field.Value;
        //        if (value == "Values")
        //        {
        //            continue;
        //        }

        //        var dataType = field.DataType;
        //        var cubeField = field.CubeField;
        //        var cubeFieldName = cubeField.Name;
        //        var orientation = cubeField.Orientation;

        //            foreach (PivotItem childItem in field.ChildItems)
        //            {
        //                var hiddenValue = childItem.Value;
        //            }

        //            //foreach (PivotItem hiddenItem in field.HiddenItems)
        //            //{
        //            //    var hiddenValue = hiddenItem.Value;
        //            //}

        //            var visibleItemsList = field.VisibleItemsList;
        //            var listAsStr = visibleItemsList as string[];
        //            var listAsObjects = visibleItemsList as object[];
        //            var listAsArray = visibleItemsList as Array;
        //            var len = listAsArray.Length;
        //            for (int i = 0; i < len; i++)
        //            {
        //                var listItem = listAsArray.GetValue(i + 1);
        //                var listItemPivotItem = listItem as PivotItem;
        //            }

        //            foreach (object visibleItem in field.PivotItems(System.Reflection.Missing.Value)  /*field.VisibleItems*/)
        //            {

        //                //var hiddenValue = visibleItem.Value;
        //            }

        //            //int idx = 1;
        //            //while (true)
        //            //{
        //            //    PivotItem nthItem = field.PivotItems(idx++);
        //            //}

        //            foreach (PivotFilter filter1 in field.PivotFilters)
        //            {
        //                var type = filter1.FilterType;
        //            }

        //            //foreach (object visibleItem in (field.VisibleItemsList as Array))
        //            //{

        //            //}

                    

        //            //field.

        //        }
        //        catch
        //        {
        //        }
        //    }

        //    foreach (PivotField field in pivotTable.DataFields)
        //    {
        //        var position = field.Position;
        //        var fieldName = field.Name;
        //        var value = field.Value;
        //        if (value == "Values")
        //        {
        //            continue;
        //        }

        //        var dataType = field.DataType;
        //        var cubeField = field.CubeField;
        //        var cubeFieldName = cubeField.Name;
        //        var orientation = cubeField.Orientation;
                
        //        //try
        //        //{
        //        //    foreach (PivotFilter filter in field.PivotFilters)
        //        //    {
        //        //        ReadFilter(filter);
        //        //    }
        //        //}
        //        //catch
        //        //{
        //        //}
                
        //    }

        //    foreach (PivotField field in pivotTable.ColumnFields)
        //    {
        //        var position = field.Position;
        //        var fieldName = field.Name;
        //        var value = field.Value;
        //        if (value == "Values")
        //        {
        //            continue;
        //        }

        //        var dataType = field.DataType;
        //        var cubeField = field.CubeField;
        //        var cubeFieldName = cubeField.Name;
        //        var orientation = cubeField.Orientation;

        //        //if (!field.AllItemsVisible)
        //        //{
        //        try
        //        {
        //            foreach (PivotItem item in field.VisibleItems)
        //            {

        //                var itemValue = item.Value;
        //                var itemName = item.Name;
        //                var itemSourceName = item.SourceName;
        //            }
        //        }
        //        catch
        //        {
        //        }
        //        //}


        //        //try
        //        //{
        //        //    foreach (PivotFilter filter in field.PivotFilters)
        //        //    {
        //        //        ReadFilter(filter);
        //        //    }
        //        //}
        //        //catch
        //        //{
        //        //}
        //    }

        //    foreach (PivotField field in pivotTable.RowFields)
        //    {
        //        var position = field.Position;
        //        var fieldName = field.Name;
        //        var value = field.Value;
        //        if (value == "Values")
        //        {
        //            continue;
        //        }

        //        var dataType = field.DataType;
        //        var cubeField = field.CubeField;
        //        var cubeFieldName = cubeField.Name;
        //        var orientation = cubeField.Orientation;

        //        //if (!field.AllItemsVisible)
        //        //{
        //        try
        //        {
        //            foreach (PivotItem item in field.VisibleItems)
        //            {
        //                var itemValue = item.Value;
        //                var itemName = item.Name;
        //                var itemSourceName = item.SourceName;
        //            }
                    
        //        }
        //        catch
        //        {
        //        }
        //        //}

        //        //try
        //        //{
        //        //    foreach (PivotFilter filter in field.PivotFilters)
        //        //    {
        //        //        ReadFilter(filter);
        //        //    }
        //        //}
        //        //catch
        //        //{
        //        //}

        //    //field.TotalLevels
        //    }
            

        //    // measure filters
        //    foreach (PivotFilter filter in pivotTable.ActiveFilters)
        //    {
        //        /*
        //                //
        //// Summary:
        ////     Filters for the specified number of values from the top of a list
        //xlTopCount = 1,
        ////
        //// Summary:
        ////     Filters for the specified number of values from the bottom of a list
        //xlBottomCount = 2,
        ////
        //// Summary:
        ////     Filters for the specified percentage of values from a list
        //xlTopPercent = 3,
        ////
        //// Summary:
        ////     Filters for the specified percentage of values from the bottom of a list
        //xlBottomPercent = 4,
        ////
        //// Summary:
        ////     Sum of the values from the top of the list
        //xlTopSum = 5,
        ////
        //// Summary:
        ////     Sum of the values from the bottom of the list
        //xlBottomSum = 6,
        ////
        //// Summary:
        ////     Filters for all values that match the specified value
        //xlValueEquals = 7,
        ////
        //// Summary:
        ////     Filters for all values that do not match the specified value
        //xlValueDoesNotEqual = 8,
        ////
        //// Summary:
        ////     Filters for all values that are greater than the specified value
        //xlValueIsGreaterThan = 9,
        ////
        //// Summary:
        ////     Filters for all values that are greater than or match the specified value
        //xlValueIsGreaterThanOrEqualTo = 10,
        ////
        //// Summary:
        ////     Filters for all values that are less than the specified value
        //xlValueIsLessThan = 11,
        ////
        //// Summary:
        ////     Filters for all values that are less than or match the specified value
        //xlValueIsLessThanOrEqualTo = 12,
        ////
        //// Summary:
        ////     Filters for all values that are between a specified range of values
        //xlValueIsBetween = 13,
        ////
        //// Summary:
        ////     Filters for all values that are not between a specified range of values
        //xlValueIsNotBetween = 14,
        ////
        //// Summary:
        ////     Filters for all captions that match the specified string
        //xlCaptionEquals = 15,
        ////
        //// Summary:
        ////     Filters for all captions that do not match the specified string
        //xlCaptionDoesNotEqual = 16,
        ////
        //// Summary:
        ////     Filters for all captions beginning with the specified string
        //xlCaptionBeginsWith = 17,
        ////
        //// Summary:
        ////     Filters for all captions that do not begin with the specified string
        //xlCaptionDoesNotBeginWith = 18,
        ////
        //// Summary:
        ////     Filters for all captions that end with the specified string
        //xlCaptionEndsWith = 19,
        ////
        //// Summary:
        ////     Filters for all captions that do not end with the specified string
        //xlCaptionDoesNotEndWith = 20,
        ////
        //// Summary:
        ////     Filters for all captions that contain the specified string
        //xlCaptionContains = 21,
        ////
        //// Summary:
        ////     Filters for all captions that do not contain the specified string
        //xlCaptionDoesNotContain = 22,
        ////
        //// Summary:
        ////     Filters for all captions that are greater than the specified value
        //xlCaptionIsGreaterThan = 23,
        ////
        //// Summary:
        ////     Filters for all captions that are greater than or match the specified value
        //xlCaptionIsGreaterThanOrEqualTo = 24,
        ////
        //// Summary:
        ////     Filters for all captions that are less than the specified value
        //xlCaptionIsLessThan = 25,
        ////
        //// Summary:
        ////     Filters for all captions that are less than or match the specified value
        //xlCaptionIsLessThanOrEqualTo = 26,
        ////
        //// Summary:
        ////     Filters for all captions that are between a specified range of values
        //xlCaptionIsBetween = 27,
        ////
        //// Summary:
        ////     Filters for all captions that are not between a specified range of values
        //xlCaptionIsNotBetween = 28,
        //         */

        //        var filterType = filter.FilterType;
        //        var value1 = filter.Value1;
        //        var value2 = filter.Value2;
                
        //        // dimension field
        //        var field = filter.PivotField;
        //        var position = field.Position;
        //        var fieldName = field.Name;

        //        if (filter.FilterType == XlPivotFilterType.xlValueIsGreaterThan)
        //        {
        //            var dataCubeField = filter.DataCubeField;
        //            var dataCubeFieldName = dataCubeField.Name;
        //        }
        //        //else if (filter.FilterType == XlPivotFilterType.xlCaptionDoesNotContain)
        //        //{
        //        //    var property = filter.MemberPropertyField;
        //        //    var propertyCubeField = filter.MemberPropertyField.CubeField;
        //        //    var propertyCubeName = propertyCubeField.Name;
        //        //}
        //        var value = field.Value;
        //        if (value == "Values")
        //        {
        //            continue;
        //        }

        //        var dataType = field.DataType;
        //        var cubeField = field.CubeField;
        //        var cubeFieldName = cubeField.Name;
        //        var orientation = cubeField.Orientation;
        //    }


        //    // pivotTable.GrandTotalName;

        //    return structure;
        //}

        private PivotTableField ReadDataField(PivotField dataField)
        {

            try
            {
                if (dataField.IsCalculated)
                {
                    return null;
                }
            }
            catch
            {
            }

            int position = (int)(dataField.Position);
            var name = dataField.SourceName;
            var split = SplitIdentifier(name);
            
            return new PivotTableField()
            {
                Dimension = split[0],
                Hierarchy = split[1],
                Attribute = split[1],
                Filters = new List<PivotTableFilter>(),
                Orientation = PivotFieldOrientation.Data,
                Position = position,
                VisibleItems = null,
                SourceName = dataField.SourceName,
                
                
            };
        }

        private PivotTableField ReadAxisField(PivotField axisField)
        {
            if (axisField.Value == "Values")
            {
                _structure.ValuesOrientation = axisField.Orientation == XlPivotFieldOrientation.xlRowField ? PivotFieldOrientation.Row : PivotFieldOrientation.Column;
                return null;
            }
            
            if (axisField.Value == "Hodnoty")
            {
                _structure.ValuesOrientation = axisField.Orientation == XlPivotFieldOrientation.xlRowField ? PivotFieldOrientation.Row : PivotFieldOrientation.Column;
                return null;
            }
            
            if (axisField.Value == "Data")
            {
                _structure.ValuesOrientation = axisField.Orientation == XlPivotFieldOrientation.xlRowField ? PivotFieldOrientation.Row : PivotFieldOrientation.Column;
                return null;
            }

            List<PivotFieldItem> visibleItems = new List<PivotFieldItem>();

            foreach (PivotItem visItem in axisField.VisibleItems)
            {
                //var sourceName = visItem.SourceName;
                //if (visItem.ChildItems is PivotItems)
                //{
                //    PivotItems childItems = (PivotItems)(visItem.ChildItems);
                //    var childCount = childItems.Count;
                //    foreach (PivotItem childItem in childItems)
                //    {
                //        var childName = childItem.SourceName;
                //    }
                //}

                //visItem.visi
                
                visibleItems.Add(new PivotFieldItem() { ItemName = visItem.SourceName.ToString() });
            }

            List<PivotTableFilter> filters = ReadFieldFilters(axisField);

            
            var value = axisField.Value;
            var attributeSplit = SplitIdentifier(value);
            int position = (int)(axisField.Position);
            var orientation = axisField.Orientation;


            var res = new PivotTableField()
            {
                Attribute = attributeSplit[2],
                Dimension = attributeSplit[0],
                Filters = filters,
                Hierarchy = attributeSplit[1],
                Orientation = orientation == XlPivotFieldOrientation.xlRowField ? PivotFieldOrientation.Row : PivotFieldOrientation.Column,
                Position = position,
                VisibleItems = visibleItems,
                SourceName = axisField.SourceName,

            };

            return res;


        }

        private PivotTableField ReadFilterField(PivotField pageField)
        {
            List<PivotFieldItem> visibleItems = new List<PivotFieldItem>();
            
            try
            {
                var pageName = pageField.CurrentPageName;
                visibleItems.Add(new PivotFieldItem() { ItemName = pageName });
            }
            catch
            {
                Array arr = pageField.VisibleItemsList as Array;
                if (arr != null)
                {
                    var stdArr = ConvertFoxArray(arr);

                    foreach (string visItem in stdArr)
                    {
                        visibleItems.Add(new PivotFieldItem() { ItemName = visItem });
                    }

                    //foreach (PivotItem visItem in stdArr)
                    //{
                    //    visibleItems.Add(new PivotFieldItem() { ItemName = visItem.SourceName });
                    //}
                }
            }
            
            List<PivotTableFilter> filters = ReadFieldFilters(pageField);

            var value = pageField.Value;
            var attributeSplit = SplitIdentifier(value);
            int position = (int)(pageField.Position);

            return new PivotTableField()
            {
                Attribute = null, // attributeSplit[2],
                Dimension = attributeSplit[0],
                Filters = filters,
                Hierarchy = attributeSplit[1],
                Orientation = PivotFieldOrientation.Filter,
                Position = position,
                VisibleItems = visibleItems,
                SourceName = pageField.SourceName
            };
        }
        
        private List<PivotTableFilter> ReadFieldFilters(PivotField field)
        {
            List<PivotTableFilter> res = new List<PivotTableFilter>();
            /*
            foreach (PivotFilter filter in field.PivotFilters)
            {
                //if (!filter.Active)
                //{
                //    continue;
                //}
                //var olapField = field.CubeField;
                //var fldName = field.SourceName;
                //var olapFieldName = olapField.Name;

                //foreach (PivotField pvtFld in olapField.PivotFields)
                //{
                //    var pvtFldName = pvtFld.SourceName;
                //}

                ValuesFilterType type = ValuesFilterType.Between;

                switch (filter.FilterType)
                {
                    case XlPivotFilterType.xlValueDoesNotEqual:
                        type = ValuesFilterType.NotEqual;
                        break;
                    case XlPivotFilterType.xlValueEquals:
                        type = ValuesFilterType.Equal;
                        break;
                    case XlPivotFilterType.xlValueIsBetween:
                        type = ValuesFilterType.Between;
                        break;
                    case XlPivotFilterType.xlValueIsGreaterThan:
                        type = ValuesFilterType.Greater;
                        break;
                    case XlPivotFilterType.xlValueIsGreaterThanOrEqualTo:
                        type = ValuesFilterType.GreaterEqual;
                        break;
                    case XlPivotFilterType.xlValueIsLessThan:
                        type = ValuesFilterType.LessThan;
                        break;
                    case XlPivotFilterType.xlValueIsLessThanOrEqualTo:
                        type = ValuesFilterType.LessEqual;
                        break;
                    case XlPivotFilterType.xlValueIsNotBetween:
                        type = ValuesFilterType.NotBetween;
                        break;
                    default:
                        continue;
                }

                var measureField = filter.DataCubeField; // .Value;
                var measure = measureField.Name;

                var addFilter = new PivotTableFilter()
                {
                    MeasureName = measure,
                    Value1 = filter.Value1.ToString(),
                    Type = type
                };
                if (type == ValuesFilterType.Between || type == ValuesFilterType.NotBetween)
                {
                    addFilter.Value2 = filter.Value2.ToString();
                }

                res.Add(addFilter);
            }
            */
            return res;
        }

        private object[] ConvertFoxArray(Array arr)
        {
            if (arr.Rank != 1) throw new ArgumentException();
            object[] retval = new object[arr.GetLength(0)];
            for (int ix = arr.GetLowerBound(0); ix <= arr.GetUpperBound(0); ++ix)
                retval[ix - arr.GetLowerBound(0)] = arr.GetValue(ix);
            return retval;
        }

        private string[] SplitIdentifier(string identifier)
        {
            var trim = identifier.Substring(1, identifier.Length - 2);
            var res = trim.Split(new string[] { "].[" }, 3, StringSplitOptions.RemoveEmptyEntries);
            return res;
        }

        //private void ReadFilter(PivotFilter filter)
        //{
        //    var filterType = filter.FilterType;
        //    var value1 = filter.Value1;
        //    var value2 = filter.Value2;
        //    var field = filter.PivotField;

        //}



        //public PivotTableStructure ReadStructure(PivotTable pivotTable)
        //{
        //    PivotTableStructure structure = new PivotTableStructure();



        //    foreach (PivotField field in pivotTable.PageFields /*pivotTable.PivotFields()*/ /*pivotTable.VisibleFields*/)
        //    {

        //        //foreach (Slicer slicer in pivotTable.Slicers)
        //        //{

        //        //}
        //        try
        //        {
        //            var pageCol = pivotTable.PageRange.Column;
        //            var pageRow = pivotTable.PageRange.Row;

        //            //var page = field.CurrentPage;
        //            try
        //            {
        //                var pageName = field.CurrentPageName;

        //            }
        //            catch
        //            { }
        //            try
        //            {
        //                var pageList = field.CurrentPageList;

        //                var pageListStd = ConvertFoxArray(pageList);
        //                foreach (var pageItem in pageListStd)
        //                {

        //                }
        //            }
        //            catch
        //            { }
        //            try

        //            {
        //                var page = field.CurrentPage;
        //            }
        //            catch
        //            { }

        //            try

        //            {
        //                var arr = field.VisibleItemsList as Array;
        //                if (arr != null)
        //                {
        //                    var stdArr = ConvertFoxArray(arr);

        //                    foreach (var visItem in stdArr)
        //                    {

        //                    }
        //                }
        //            }
        //            catch
        //            {

        //            }

        //            try
        //            {
        //                foreach (var visItem in field.VisibleItems)
        //                {

        //                }
        //            }
        //            catch
        //            { }


        //            var position = field.Position;
        //            var fieldName = field.Name;
        //            //if (!fieldName.Contains("WM Year"))
        //            //{
        //            //    continue;
        //            //}

        //            var value = field.Value;
        //            if (value == "Values")
        //            {
        //                continue;
        //            }

        //            var dataType = field.DataType;
        //            var cubeField = field.CubeField;
        //            var cubeFieldName = cubeField.Name;
        //            var orientation = cubeField.Orientation;

        //            foreach (PivotItem childItem in field.ChildItems)
        //            {
        //                var hiddenValue = childItem.Value;
        //            }

        //            //foreach (PivotItem hiddenItem in field.HiddenItems)
        //            //{
        //            //    var hiddenValue = hiddenItem.Value;
        //            //}

        //            var visibleItemsList = field.VisibleItemsList;
        //            var listAsStr = visibleItemsList as string[];
        //            var listAsObjects = visibleItemsList as object[];
        //            var listAsArray = visibleItemsList as Array;
        //            var len = listAsArray.Length;
        //            for (int i = 0; i < len; i++)
        //            {
        //                var listItem = listAsArray.GetValue(i + 1);
        //                var listItemPivotItem = listItem as PivotItem;
        //            }

        //            foreach (object visibleItem in field.PivotItems(System.Reflection.Missing.Value)  /*field.VisibleItems*/)
        //            {

        //                //var hiddenValue = visibleItem.Value;
        //            }

        //            //int idx = 1;
        //            //while (true)
        //            //{
        //            //    PivotItem nthItem = field.PivotItems(idx++);
        //            //}

        //            foreach (PivotFilter filter1 in field.PivotFilters)
        //            {
        //                var type = filter1.FilterType;
        //            }

        //            //foreach (object visibleItem in (field.VisibleItemsList as Array))
        //            //{

        //            //}



        //            //field.

        //        }
        //        catch
        //        {
        //        }
        //    }

        //    foreach (PivotField field in pivotTable.DataFields)
        //    {
        //        var position = field.Position;
        //        var fieldName = field.Name;
        //        var value = field.Value;
        //        if (value == "Values")
        //        {
        //            continue;
        //        }

        //        var dataType = field.DataType;
        //        var cubeField = field.CubeField;
        //        var cubeFieldName = cubeField.Name;
        //        var orientation = cubeField.Orientation;

        //        //try
        //        //{
        //        //    foreach (PivotFilter filter in field.PivotFilters)
        //        //    {
        //        //        ReadFilter(filter);
        //        //    }
        //        //}
        //        //catch
        //        //{
        //        //}

        //    }

        //    foreach (PivotField field in pivotTable.ColumnFields)
        //    {
        //        var position = field.Position;
        //        var fieldName = field.Name;
        //        var value = field.Value;
        //        if (value == "Values")
        //        {
        //            continue;
        //        }

        //        var dataType = field.DataType;
        //        var cubeField = field.CubeField;
        //        var cubeFieldName = cubeField.Name;
        //        var orientation = cubeField.Orientation;

        //        //if (!field.AllItemsVisible)
        //        //{
        //        try
        //        {
        //            foreach (PivotItem item in field.VisibleItems)
        //            {

        //                var itemValue = item.Value;
        //                var itemName = item.Name;
        //                var itemSourceName = item.SourceName;
        //            }
        //        }
        //        catch
        //        {
        //        }
        //        //}


        //        //try
        //        //{
        //        //    foreach (PivotFilter filter in field.PivotFilters)
        //        //    {
        //        //        ReadFilter(filter);
        //        //    }
        //        //}
        //        //catch
        //        //{
        //        //}
        //    }

        //    foreach (PivotField field in pivotTable.RowFields)
        //    {
        //        var position = field.Position;
        //        var fieldName = field.Name;
        //        var value = field.Value;
        //        if (value == "Values")
        //        {
        //            continue;
        //        }

        //        var dataType = field.DataType;
        //        var cubeField = field.CubeField;
        //        var cubeFieldName = cubeField.Name;
        //        var orientation = cubeField.Orientation;

        //        //if (!field.AllItemsVisible)
        //        //{
        //        try
        //        {
        //            foreach (PivotItem item in field.VisibleItems)
        //            {
        //                var itemValue = item.Value;
        //                var itemName = item.Name;
        //                var itemSourceName = item.SourceName;
        //            }

        //        }
        //        catch
        //        {
        //        }
        //        //}

        //        //try
        //        //{
        //        //    foreach (PivotFilter filter in field.PivotFilters)
        //        //    {
        //        //        ReadFilter(filter);
        //        //    }
        //        //}
        //        //catch
        //        //{
        //        //}

        //        //field.TotalLevels
        //    }


        //    // measure filters
        //    foreach (PivotFilter filter in pivotTable.ActiveFilters)
        //    {
        //        /*
        //                //
        //// Summary:
        ////     Filters for the specified number of values from the top of a list
        //xlTopCount = 1,
        ////
        //// Summary:
        ////     Filters for the specified number of values from the bottom of a list
        //xlBottomCount = 2,
        ////
        //// Summary:
        ////     Filters for the specified percentage of values from a list
        //xlTopPercent = 3,
        ////
        //// Summary:
        ////     Filters for the specified percentage of values from the bottom of a list
        //xlBottomPercent = 4,
        ////
        //// Summary:
        ////     Sum of the values from the top of the list
        //xlTopSum = 5,
        ////
        //// Summary:
        ////     Sum of the values from the bottom of the list
        //xlBottomSum = 6,
        ////
        //// Summary:
        ////     Filters for all values that match the specified value
        //xlValueEquals = 7,
        ////
        //// Summary:
        ////     Filters for all values that do not match the specified value
        //xlValueDoesNotEqual = 8,
        ////
        //// Summary:
        ////     Filters for all values that are greater than the specified value
        //xlValueIsGreaterThan = 9,
        ////
        //// Summary:
        ////     Filters for all values that are greater than or match the specified value
        //xlValueIsGreaterThanOrEqualTo = 10,
        ////
        //// Summary:
        ////     Filters for all values that are less than the specified value
        //xlValueIsLessThan = 11,
        ////
        //// Summary:
        ////     Filters for all values that are less than or match the specified value
        //xlValueIsLessThanOrEqualTo = 12,
        ////
        //// Summary:
        ////     Filters for all values that are between a specified range of values
        //xlValueIsBetween = 13,
        ////
        //// Summary:
        ////     Filters for all values that are not between a specified range of values
        //xlValueIsNotBetween = 14,
        ////
        //// Summary:
        ////     Filters for all captions that match the specified string
        //xlCaptionEquals = 15,
        ////
        //// Summary:
        ////     Filters for all captions that do not match the specified string
        //xlCaptionDoesNotEqual = 16,
        ////
        //// Summary:
        ////     Filters for all captions beginning with the specified string
        //xlCaptionBeginsWith = 17,
        ////
        //// Summary:
        ////     Filters for all captions that do not begin with the specified string
        //xlCaptionDoesNotBeginWith = 18,
        ////
        //// Summary:
        ////     Filters for all captions that end with the specified string
        //xlCaptionEndsWith = 19,
        ////
        //// Summary:
        ////     Filters for all captions that do not end with the specified string
        //xlCaptionDoesNotEndWith = 20,
        ////
        //// Summary:
        ////     Filters for all captions that contain the specified string
        //xlCaptionContains = 21,
        ////
        //// Summary:
        ////     Filters for all captions that do not contain the specified string
        //xlCaptionDoesNotContain = 22,
        ////
        //// Summary:
        ////     Filters for all captions that are greater than the specified value
        //xlCaptionIsGreaterThan = 23,
        ////
        //// Summary:
        ////     Filters for all captions that are greater than or match the specified value
        //xlCaptionIsGreaterThanOrEqualTo = 24,
        ////
        //// Summary:
        ////     Filters for all captions that are less than the specified value
        //xlCaptionIsLessThan = 25,
        ////
        //// Summary:
        ////     Filters for all captions that are less than or match the specified value
        //xlCaptionIsLessThanOrEqualTo = 26,
        ////
        //// Summary:
        ////     Filters for all captions that are between a specified range of values
        //xlCaptionIsBetween = 27,
        ////
        //// Summary:
        ////     Filters for all captions that are not between a specified range of values
        //xlCaptionIsNotBetween = 28,
        //         */

        //        var filterType = filter.FilterType;
        //        var value1 = filter.Value1;
        //        var value2 = filter.Value2;

        //        // dimension field
        //        var field = filter.PivotField;
        //        var position = field.Position;
        //        var fieldName = field.Name;

        //        if (filter.FilterType == XlPivotFilterType.xlValueIsGreaterThan)
        //        {
        //            var dataCubeField = filter.DataCubeField;
        //            var dataCubeFieldName = dataCubeField.Name;
        //        }
        //        //else if (filter.FilterType == XlPivotFilterType.xlCaptionDoesNotContain)
        //        //{
        //        //    var property = filter.MemberPropertyField;
        //        //    var propertyCubeField = filter.MemberPropertyField.CubeField;
        //        //    var propertyCubeName = propertyCubeField.Name;
        //        //}
        //        var value = field.Value;
        //        if (value == "Values")
        //        {
        //            continue;
        //        }

        //        var dataType = field.DataType;
        //        var cubeField = field.CubeField;
        //        var cubeFieldName = cubeField.Name;
        //        var orientation = cubeField.Orientation;
        //    }


        //    // pivotTable.GrandTotalName;

        //    return structure;
        //}

    }
}
