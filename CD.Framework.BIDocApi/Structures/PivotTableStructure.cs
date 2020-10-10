using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.Structures
{
    public enum PivotFieldOrientation { Row = 0, Column = 1, Data = 2, Filter = 3 }


    public enum XlLayoutRowType
    {    xlCompactRow = 0, xlTabularRow = 1, xlOutlineRow = 2}

    public class PivotTableField
    {
        public int Position { get; set; }
        public PivotFieldOrientation Orientation { get; set; }
        public string Hierarchy { get; set; }
        public string Dimension { get; set; }
        public string Attribute { get; set; }
        public string SourceName { get; set; }
        public bool ShowDetail { get; set; } 
        


        public string PivotFieldName
        {
            get
            {
                if (Orientation == PivotFieldOrientation.Data || Orientation == PivotFieldOrientation.Filter /*Hierarchy == Attribute*/)
                {
                    return string.Format("[{0}].[{1}]", Dimension, Hierarchy);
                }
                return string.Format("[{0}].[{1}].[{2}]", Dimension, Hierarchy, Attribute);
            }
        }

        public string CubeFieldName
        {
            get
            {
                return string.Format("[{0}].[{1}]", Dimension, Hierarchy);
            }
        }


        public List<PivotTableFilter> Filters { get; set; }
        public List<PivotFieldItem> VisibleItems { get; set; }
    }

    public enum ValuesFilterType { LessThan, LessEqual, Equal, GreaterEqual, Greater, Between, NotBetween, NotEqual }

    public class PivotTableFilter
    {
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string MeasureName { get; set; }
        public ValuesFilterType Type { get; set; }
    }

    public class PivotFieldItem
    {
        public string ItemName { get; set; }
    }

    //public class CalculatedMeasure
    //{
    //    public string Name { get; set; }
    //    public string Definition { get; set; }
    //}

    public enum PivotTableConnectionType { Multidimensional, Tabular }

    public class PivotTableStructure
    {
        public string ConnectionString { get; set; }
        public string CubeName { get; set; }
        public PivotFieldOrientation ValuesOrientation { get; set; }
        //List<CalculatedMeasure> CalculatedMeasures { get; set; }
        public List<PivotTableField> VisibleFields { get; set; }
        public string TableStyle { get; set; }
        public XlLayoutRowType LayoutRowDefault { get; set; }
        public bool ColumnGrand { get; set; }
        public bool RowGrand { get; set; }
        public bool HasAutoFormat { get; set; }
        public bool DisplayErrorString { get; set; }
        public bool DisplayNullString { get; set; }
        public bool EnableDrilldown { get; set; }
        public string ErrorString { get; set; }
        public bool MergeLabels { get; set; }
        public string NullString { get; set; }
        public int PageFieldOrder { get; set; }
        public int PageFieldWrapCount { get; set; }
        public bool PreserveFormatting { get; set; }
        public bool PrintTitles { get; set; }
        public bool RepeatItemsOnEachPrintedPage { get; set; }
        public bool TotalsAnnotation { get; set; }
        public int CompactRowIndent { get; set; }
        public bool VisualTotals { get; set; }
        public bool InGridDropZones { get; set; }
        public bool DisplayFieldCaptions { get; set; }
        public bool DisplayMemberPropertyTooltips { get; set; }
        public bool DisplayContextTooltips { get; set; }
        public bool ShowDrillIndicators { get; set; }
        public bool PrintDrillIndicators { get; set; }
        public bool DisplayEmptyRow { get; set; }
        public bool DisplayEmptyColumn { get; set; }
        public bool AllowMultipleFilters { get; set; }
        public bool SortUsingCustomLists { get; set; }
        public bool DisplayImmediateItems { get; set; }
        public bool ViewCalculatedMembers { get; set; }
        public bool EnableWriteback { get; set; }
        public bool ShowValuesRow { get; set; }
        public bool CalculatedMembersInFilters { get; set; }
        public PivotTableConnectionType ConnectionType { get; set; }
        
    }
}
