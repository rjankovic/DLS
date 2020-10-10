using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql.Ssis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Db = CD.DLS.Model.Mssql.Db;

namespace CD.DLS.Model.Mssql.Ssrs
{
    abstract public class SsrsModelElement : MssqlModelElement
    {
        public SsrsModelElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null)
           : base(refPath, caption)
        {
            Definition = definition;
            Parent = parent;
        }

        public SsrsModelElement(RefPath refPath, string caption)
           : base(refPath, caption)
        { }

        public string LocalhostServer()
        {
            if (this is ServerElement)
            {
                return this.Caption;
            }
            return ((SsrsModelElement)Parent).LocalhostServer();
        }
    }

    public class ServerElement : SsrsModelElement
    {
        public ServerElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public ServerElement(RefPath refPath, string caption)
                : base(refPath, caption)
        { }

        public IEnumerable<FolderElement> Folders { get { return ChildrenOfType<FolderElement>(); } }
    }

    public class FolderElement : SsrsModelElement
    {
        public FolderElement(RefPath refPath, string caption, string definition, ServerElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public FolderElement(RefPath refPath, string caption, string definition, FolderElement parent)
                : base(refPath, caption, definition, parent)
        {
        }
        
        [DataMember]
        public string FullPath { get; set; }

        public IEnumerable<FolderElement> Folders { get { return ChildrenOfType<FolderElement>(); } }
        public IEnumerable<SharedDataSourceElement> DataSources { get { return ChildrenOfType<SharedDataSourceElement>(); } }
        public IEnumerable<SharedDataSetElement> DataSets { get { return ChildrenOfType<SharedDataSetElement>(); } }
        public IEnumerable<ReportElement> Reports { get { return ChildrenOfType<ReportElement>(); } }

    }

    public abstract class DataSourceElement : SsrsModelElement
    {
        public DataSourceElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public enum SourceTypeEnum { Sql, SsasMultidimensional, SsasTabular }

        [DataMember]
        public SourceTypeEnum SourceType { get; set; }
        [DataMember]
        public string ConnectionString { get; set; }
    }

    public class SharedDataSourceElement : DataSourceElement
    {
        public SharedDataSourceElement(RefPath refPath, string caption, string definition, FolderElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        public string FullPath { get; set; }
    }

    public class ReportDataSourceElement : DataSourceElement
    {
        public ReportDataSourceElement(RefPath refPath, string caption, string definition, ReportElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public abstract class DataSetElement : SsrsModelElement
    {
        public DataSetElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public DataSourceElement DataSource { get; set; }
        
        public IEnumerable<DataSetFieldElement> Fields { get { return ChildrenOfType<DataSetFieldElement>(); } }
        public IEnumerable<QueryParameterElement> Parameters { get { return ChildrenOfType<QueryParameterElement>(); } }
    }

    public class SharedDataSetElement : DataSetElement
    {
        public SharedDataSetElement(RefPath refPath, string caption, string definition, FolderElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class ReportDataSetElement : DataSetElement
    {
        public ReportDataSetElement(RefPath refPath, string caption, string definition, ReportElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class DataSetFieldElement : SsrsModelElement
    {
        public DataSetFieldElement(RefPath refPath, string caption, string definition, DataSetElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public MssqlModelElement Source { get; set; }
    }

    public class ReportElement : SsrsModelElement
    {
        public ReportElement(RefPath refPath, string caption, string definition, FolderElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        public string SsrsPath { get; set; }
        [DataMember]
        public int SsrsComponentId { get; set; }

        public IEnumerable<ReportDataSourceElement> DataSources { get { return ChildrenOfType<ReportDataSourceElement>(); } }
        public IEnumerable<ReportDataSetElement> DataSets { get { return ChildrenOfType<ReportDataSetElement>(); } }
        public IEnumerable<ReportParameterElement> Parameters { get { return ChildrenOfType<ReportParameterElement>(); } }
        public IEnumerable<ReportSectionElement> Sections { get { return ChildrenOfType<ReportSectionElement>(); } }
        public ReportSectionBodyElement SectionBody { get { return ChildrenOfType<ReportSectionBodyElement>().FirstOrDefault(); } }
    }

    public class QueryParameterElement : SsrsModelElement
    {
        public QueryParameterElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class ReportParameterElement : SsrsModelElement
    {
        public ReportParameterElement(RefPath refPath, string caption, string definition, ReportElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        ReportParameterValidValuesElement ValidValues { get; set; }
        [DataMember]
        ReportParameterDefaultValuesElement DefaultValues { get; set; }
        [DataMember]
        public string Prompt { get; set; }
        [DataMember]
        public bool Multivalue { get; set; }
        [DataMember]
        public bool AllowBlank { get; set; }

    }

    public abstract class ReportParameterValidValuesElement : SsrsModelElement
    {
        public ReportParameterValidValuesElement(RefPath refPath, string caption, string definition, ReportParameterElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class ReportParameterValidValuesDataSetElement : ReportParameterValidValuesElement
    {
        public ReportParameterValidValuesDataSetElement(RefPath refPath, string caption, string definition, ReportParameterElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public DataSetElement DataSet { get; set; }
        [ModelLink]
        public DataSetFieldElement LabelField { get; set; }
        [ModelLink]
        public DataSetFieldElement ValueField { get; set; }
    }

    public class ReportParameterValidValuesStaticElement : ReportParameterValidValuesElement
    {
        public ReportParameterValidValuesStaticElement(RefPath refPath, string caption, string definition, ReportParameterElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public IEnumerable<ParameterValueElement> Values { get { return ChildrenOfType<ParameterValueElement>(); } }
    }

    public class ParameterValueElement : SsrsModelElement
    {
        public ParameterValueElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        public string Label { get; set; }
        [DataMember]
        public string Value { get; set; }
    }

    public abstract class ReportParameterDefaultValuesElement : SsrsModelElement
    {
        public ReportParameterDefaultValuesElement(RefPath refPath, string caption, string definition, ReportParameterElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class ReportParameterDefaultValuesDataSetElement : ReportParameterDefaultValuesElement
    {
        public ReportParameterDefaultValuesDataSetElement(RefPath refPath, string caption, string definition, ReportParameterElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public DataSetElement DataSet { get; set; }
        [ModelLink]
        public DataSetFieldElement ValueField { get; set; }
    }

    public class ReportParameterDefaultValuesStaticElement : ReportParameterValidValuesElement
    {
        public ReportParameterDefaultValuesStaticElement(RefPath refPath, string caption, string definition, ReportParameterElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public IEnumerable<ParameterValueElement> Values { get { return ChildrenOfType<ParameterValueElement>(); } }
    }

    [DataContract]
    public class SsrsExpressionFragmentElement : SsrsModelElement
    {
        public SsrsExpressionFragmentElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
            : base(refPath, caption, definition, parent) { }
        
        [DataMember]
        public int OffsetFrom { get; set; }
        [DataMember]
        public int Length { get; set; }
    }
    
    public class SsrsExpressionElement : SsrsExpressionFragmentElement
    {
        public SsrsExpressionElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public abstract class ExpressionElement : SsrsModelElement
    {
        public ExpressionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        { }
        
    }
    
    public class ReportSectionElement : ReportDesignElement
    {
        public ReportSectionElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
            : base(refPath, caption, definition, parent) { }

        public ReportSectionBodyElement Body { get { return ChildrenOfType<ReportSectionBodyElement>().First(); } }
    }

    public class ReportSectionBodyElement : ReportDesignElement
    {
        public ReportSectionBodyElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
            : base(refPath, caption, definition, parent) { }

        public IEnumerable<ReportItemElement> Items { get { return ChildrenOfType<ReportItemElement>(); } }
    }

    public abstract class ReportItemElement : ReportDesignElement
    {
        public ReportItemElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
            : base(refPath, caption, definition, parent) { }

        [ModelLink]
        public DataSetElement DataSet { get; set; }

        [ModelLink]
        public TextBoxElement CaptionTextBox { get; set; }
    }

    public class TablixElement : ReportItemElement
    {
        public TablixElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
            : base(refPath, caption, definition, parent) { }

        public IEnumerable<TablixRowElement> Rows { get { return ChildrenOfType<TablixRowElement>(); } }
        public TablixRowHierarchyElement RowHierarchy { get { return ChildrenOfType<TablixRowHierarchyElement>().First(); } }
        public TablixColumnHierarchyElement ColumnHierarchy { get { return ChildrenOfType<TablixColumnHierarchyElement>().First(); } }
    }

    public abstract class ReportItemHierarchyElement : SsrsModelElement
    {
        public ReportItemHierarchyElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
            : base(refPath, caption, definition, parent) { }

        public IEnumerable<HierarchyMemberElement> Members { get { return ChildrenOfType<HierarchyMemberElement>(); } }
    }

    public class TablixRowHierarchyElement : ReportItemHierarchyElement
    {
        public TablixRowHierarchyElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }

    public class TablixColumnHierarchyElement : ReportItemHierarchyElement
    {
        public TablixColumnHierarchyElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }

    public class HierarchyMemberElement : SsrsModelElement
    {
        public HierarchyMemberElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
            : base(refPath, caption, definition, parent) { }

        public TextBoxElement HeaderTextBox { get { return ChildrenOfType<TextBoxElement>().FirstOrDefault(); } }
        public IEnumerable<HierarchyMemberElement> Members { get { return ChildrenOfType<HierarchyMemberElement>(); } }
        public HierarchyGroupElement Group { get { return ChildrenOfType<HierarchyGroupElement>().FirstOrDefault(); } }
        public ReportItemElement ReportItem
        {
            get
            {
                if (Parent is HierarchyMemberElement)
                {
                    return ((HierarchyMemberElement)Parent).ReportItem;
                }
                var hierarchyElement = (ReportItemHierarchyElement)Parent;
                return (ReportItemElement)hierarchyElement.Parent;
            }
        }

    }

    public class HierarchyGroupElement : SsrsModelElement
    {
        public HierarchyGroupElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
            : base(refPath, caption, definition, parent) { }

        public IEnumerable<HierarchyMemberElement> Members { get { return ChildrenOfType<HierarchyMemberElement>(); } }

        [DataMember]
        public string DataElementName { get; set; }
    }
    
    public class TablixRowElement : ReportDesignElement
    {
        public TablixRowElement(RefPath refPath, string caption, string definition, TablixElement parent)
            : base(refPath, caption, definition, parent) { }

        public IEnumerable<CellElement> Cells { get { return ChildrenOfType<CellElement>(); } }

        public List<string> CellItemNames { get { return Cells.SelectMany(x => x.Children).Select(x => x.Caption).ToList(); } }

        [DataMember]
        public bool Hidden { get; set; }
    }

    public class CellElement : ReportDesignElement
    {
        public CellElement(RefPath refPath, string caption, string definition, TablixRowElement parent)
            : base(refPath, caption, definition, parent) { }

        [ModelLink]
        public CellElement CaptionCell { get; set; }
    }

    public class TextBoxElement : ReportDesignElement
    {
        public TextBoxElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
            : base(refPath, caption, definition, parent) { }
        // create value even if it is not a valid ssrs expression element according to the grammar (constant literal cell)
        public SsrsExpressionElement Value { get { return ChildrenOfType<SsrsExpressionElement>().First(); } }
        [DataMember]
        public string Format { get; set; } 
    }

    public class RectangleElement : ReportDesignElement
    {
        public RectangleElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
            : base(refPath, caption, definition, parent) { }

        public IEnumerable<ReportItemElement> Items { get { return ChildrenOfType<ReportItemElement>(); } }
    }

    public class ChartElement : ReportItemElement
    {
        public ChartElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
            : base(refPath, caption, definition, parent) { }

        public ChartDataElement ChartData { get { return ChildrenOfType<ChartDataElement>().First(); } }
    }

    public class ChartDataElement : SsrsModelElement
    {
        public ChartDataElement(RefPath refPath, string caption, string definition, ChartElement parent)
            : base(refPath, caption, definition, parent) { }

        public IEnumerable<ChartSeriesElement> Series { get { return ChildrenOfType<ChartSeriesElement>(); } }
    }

    public class ChartSeriesElement : SsrsModelElement
    {
        public ChartSeriesElement(RefPath refPath, string caption, string definition, ChartDataElement parent)
            : base(refPath, caption, definition, parent) { }

        public IEnumerable<ChartDataPointElement> Points { get { return ChildrenOfType<ChartDataPointElement>(); } }

        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Subtype { get; set; }
    }
    
    public class ChartDataPointElement : SsrsModelElement
    {
        public ChartDataPointElement(RefPath refPath, string caption, string definition, ChartSeriesElement parent)
            : base(refPath, caption, definition, parent) { }

        [ModelLink]
        public SsrsExpressionElement LowValue { get; set; }
        [ModelLink]
        public SsrsExpressionElement HighValue { get; set; }
        [ModelLink]
        public SsrsExpressionElement X { get; set; }
        [ModelLink]
        public SsrsExpressionElement Y { get; set; }
    }

    public class MapElement : ReportItemElement
    {
        public MapElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
            : base(refPath, caption, definition, parent) { }

        public IEnumerable<MapDataRegionElement> Regions { get { return ChildrenOfType<MapDataRegionElement>(); } }
    }

    public class MapDataRegionElement : SsrsModelElement
    {
        public MapDataRegionElement(RefPath refPath, string caption, string definition, MapElement parent)
            : base(refPath, caption, definition, parent) { }

        [ModelLink]
        public DataSetElement DataSet { get; set; }

        public IEnumerable<MapGroupElement> Groups { get { return ChildrenOfType<MapGroupElement>(); } }
    }

    public class MapGroupElement : SsrsModelElement
    {
        public MapGroupElement(RefPath refPath, string caption, string definition, MapDataRegionElement parent)
            : base(refPath, caption, definition, parent) { }
        
        public IEnumerable<SsrsExpressionElement> Expressions { get { return ChildrenOfType<SsrsExpressionElement>(); } }
    }

    public class GaugePanelElement : ReportItemElement
    {
        public GaugePanelElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
            : base(refPath, caption, definition, parent) { }

        public IEnumerable<GaugeElement> Gauges { get { return ChildrenOfType<GaugeElement>(); } }
    }

    public abstract class GaugeElement : SsrsModelElement
    {
        public GaugeElement(RefPath refPath, string caption, string definition, GaugePanelElement parent)
            : base(refPath, caption, definition, parent) { }

        public enum GaugeTypeEnum { Linear, Radial, Indicator }
        [DataMember]
        public GaugeTypeEnum GaugeType { get; set; }
        
    }

    public class ScalesGaugeElement : GaugeElement
    {
        public ScalesGaugeElement(RefPath refPath, string caption, string definition, GaugePanelElement parent)
            : base(refPath, caption, definition, parent) { }
        
        public IEnumerable<GaugeScaleElement> Scales { get { return ChildrenOfType<GaugeScaleElement>(); } }
    }

    public class IndicatorGaugeElement : GaugeElement
    {
        public IndicatorGaugeElement(RefPath refPath, string caption, string definition, GaugePanelElement parent)
            : base(refPath, caption, definition, parent) { }

        [ModelLink]
        public SsrsExpressionElement InputValue { get; set; }
        [ModelLink]
        public SsrsExpressionElement MinValue { get; set; }
        [ModelLink]
        public SsrsExpressionElement MaxValue { get; set; }
    }

    public class GaugeScaleElement : SsrsModelElement
    {
        public GaugeScaleElement(RefPath refPath, string caption, string definition, GaugeElement parent)
            : base(refPath, caption, definition, parent) { }

        public IEnumerable<GaugePointerElement> Pointers { get { return ChildrenOfType<GaugePointerElement>(); } }
    }

    public class GaugePointerElement : SsrsModelElement
    {
        public GaugePointerElement(RefPath refPath, string caption, string definition, GaugeScaleElement parent)
            : base(refPath, caption, definition, parent) { }

        public IEnumerable<SsrsExpressionElement> Expressions { get { return ChildrenOfType<SsrsExpressionElement>(); } }
    }

    public abstract class ReportDesignElement : SsrsModelElement
    {
        public ReportDesignElement(RefPath refPath, string caption, string definition, SsrsModelElement parent)
            : base(refPath, caption, definition, parent)
        {
            Position = new ReportDesignArea();
        }
        
        [DataMember]
        public ReportDesignArea Position { get; set; }
    }

    public class ReportDesignArea
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public enum MeasureEnum { Cm, Pt, In, Mm }
        public MeasureEnum Measure { get; set; }
    }



}
