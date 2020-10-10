using CD.DLS.Model.Mssql.Tabular;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql.Ssis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Db = CD.DLS.Model.Mssql.Db;
using CD.DLS.Common.Structures;

namespace CD.DLS.Model.Mssql.Ssas
{
    abstract public class SsasModelElement : MssqlModelElement
    {
        public SsasModelElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null)
           : base(refPath, caption)
        {
            Definition = definition;
            Parent = parent;
        }

        public SsasModelElement(RefPath refPath, string caption)
           : base(refPath, caption)
        { }

        public string LocalhostServer()
        {
            if (this is ServerElement)
            {
                if (this.Caption.Contains("\\"))
                {
                    return this.Caption.Substring(0, this.Caption.IndexOf('\\'));
                }
                return this.Caption;
            }
            return ((SsasModelElement)Parent).LocalhostServer();
        }

        [DataMember]
        public String SsasObjectID { get; set; }
    }

    public class ServerElement : SsasModelElement
    {
        public ServerElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public ServerElement(RefPath refPath, string caption)
                : base(refPath, caption)
        { }
        
        public IEnumerable<SsasDatabaseElement> Databases { get { return ChildrenOfType<SsasDatabaseElement>(); } }
    }

    public class SsasMultidimensionalDatabaseElement : SsasDatabaseElement
    {
        public SsasMultidimensionalDatabaseElement(RefPath refPath, string caption, string definition, ServerElement parent)
                : base(refPath, caption, definition, parent)
        {
            SsasType = SsasTypeEnum.Multidimensional;
        }
        
        public IEnumerable<DatasourceViewElement> DatasourceViews { get { return ChildrenOfType<DatasourceViewElement>(); } }
        public IEnumerable<DimensionElement> Dimensions { get { return ChildrenOfType<DimensionElement>(); } }
        public IEnumerable<CubeElement> Cubes { get { return ChildrenOfType<CubeElement>(); } }
    }

    public abstract class SsasDatabaseElement : SsasModelElement
    {
        public SsasDatabaseElement(RefPath refPath, string caption, string definition, ServerElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        public SsasTypeEnum SsasType { get; set; }
    }

    public class DatasourceViewElement : SsasModelElement
    {
        public DatasourceViewElement(RefPath refPath, string caption, string definition, SsasMultidimensionalDatabaseElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public IEnumerable<DatasourceViewTableElement> Tables { get { return ChildrenOfType<DatasourceViewTableElement>(); } }
    }

    public class DatasourceViewTableElement : SsasModelElement
    {
        public DatasourceViewTableElement(RefPath refPath, string caption, string definition, DatasourceViewElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public IEnumerable<DatasourceViewColumnElement> Columns { get { return ChildrenOfType<DatasourceViewColumnElement>(); } }
    }

    public class DatasourceViewColumnElement : SsasModelElement
    {
        public DatasourceViewColumnElement(RefPath refPath, string caption, string definition, DatasourceViewTableElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public MssqlModelElement Source { get; set; }
    }

    public class DimensionElement : SsasModelElement
    {
        public DimensionElement(RefPath refPath, string caption, string definition, SsasMultidimensionalDatabaseElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public IEnumerable<DimensionAttributeElement> Attributes { get { return ChildrenOfType<DimensionAttributeElement>(); } }
        public IEnumerable<HierarchyElement> Hierarchies { get { return ChildrenOfType<HierarchyElement>(); } }
    }

    public class DimensionAttributeElement : SsasModelElement
    {
        public DimensionAttributeElement(RefPath refPath, string caption, string definition, DimensionElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        public bool AttributeHierarchyEnabled { get; set; }

        public IEnumerable<KeyColumnElement> KeyColumns { get { return ChildrenOfType<KeyColumnElement>(); } }
        public ValueColumnElement ValueColumn { get { return ChildrenOfType<ValueColumnElement>().FirstOrDefault(); } }
        public NameColumnElement NameColumn { get { return ChildrenOfType<NameColumnElement>().FirstOrDefault(); } }
        public CustomRollupColumnElement CustomRollupColumn { get { return ChildrenOfType<CustomRollupColumnElement>().FirstOrDefault(); } }
        public CustomRollupPropertiesColumnElement CustomRollupPropertiesColumn { get { return ChildrenOfType<CustomRollupPropertiesColumnElement>().FirstOrDefault(); } }
        public UnaryOperatorColumnElement UnaryOperatorColumn { get { return ChildrenOfType<UnaryOperatorColumnElement>().FirstOrDefault(); } }
        public IEnumerable<RelatedAttributeElement> RelatedAttributes { get { return ChildrenOfType<RelatedAttributeElement>(); } }
    }

    public abstract class ColumnElement : SsasModelElement
    {
        public ColumnElement(RefPath refPath, string caption, string definition, SsasModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public MssqlModelElement DsvColumn { get; set; }
    }


    public class KeyColumnElement : ColumnElement
    {
        public KeyColumnElement(RefPath refPath, string caption, string definition, SsasModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class ValueColumnElement : ColumnElement
    {
        public ValueColumnElement(RefPath refPath, string caption, string definition, SsasModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class NameColumnElement : ColumnElement
    {
        public NameColumnElement(RefPath refPath, string caption, string definition, SsasModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class CustomRollupColumnElement : ColumnElement
    {
        public CustomRollupColumnElement(RefPath refPath, string caption, string definition, SsasModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class CustomRollupPropertiesColumnElement : ColumnElement
    {
        public CustomRollupPropertiesColumnElement(RefPath refPath, string caption, string definition, SsasModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class UnaryOperatorColumnElement : ColumnElement
    {
        public UnaryOperatorColumnElement(RefPath refPath, string caption, string definition, SsasModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class RelatedAttributeElement : SsasModelElement
    {
        public RelatedAttributeElement(RefPath refPath, string caption, string definition, DimensionAttributeElement parent)
                : base(refPath, caption, definition, parent)
        { }
        
        [ModelLink]
        public DimensionAttributeElement RelatedAttribute { get; set; }
    }

    public class HierarchyElement : SsasModelElement
    {
        public HierarchyElement(RefPath refPath, string caption, string definition, DimensionElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public IEnumerable<HierarchyLevelElement> Levles { get { return ChildrenOfType<HierarchyLevelElement>().OrderBy(hl => hl.Ordinal); } }
    }

    public class HierarchyLevelElement : SsasModelElement
    {
        public HierarchyLevelElement(RefPath refPath, string caption, string definition, HierarchyElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public DimensionAttributeElement Attribute { get; set; }

        [DataMember]
        public int Ordinal { get; set; }
    }

    public class CubeElement : SsasModelElement
    {
        public CubeElement(RefPath refPath, string caption, string definition, SsasMultidimensionalDatabaseElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public IEnumerable<CubeDimensionElement> Dimensions { get { return ChildrenOfType<CubeDimensionElement>(); } }
        public IEnumerable<MeasureGroupElement> MeasureGroups { get { return ChildrenOfType<MeasureGroupElement>(); } }
        public IEnumerable<CalculatedMeasureElement> CalculatedMeasures { get { return ChildrenOfType<MdxScriptElement>().SelectMany(x => x.Children.OfType<CalculatedMeasureElement>()); } }
    }

    public class CubeDimensionElement : SsasModelElement
    {
        public CubeDimensionElement(RefPath refPath, string caption, string definition, CubeElement parent)
                : base(refPath, caption, definition, parent)
        { }
        
        [ModelLink]
        public DimensionElement DatabaseDimension { get; set; }

        public IEnumerable<CubeDimensionAttributeElement> Attributes { get { return ChildrenOfType<CubeDimensionAttributeElement>(); } }
        public IEnumerable<CubeDimensionHierarchyElement> Hierarchies { get { return ChildrenOfType<CubeDimensionHierarchyElement>(); } }
    }

    public class CubeDimensionAttributeElement : SsasModelElement
    {
        public CubeDimensionAttributeElement(RefPath refPath, string caption, string definition, CubeDimensionElement parent)
                : base(refPath, caption, definition, parent)
        { }
        
        [ModelLink]
        public DimensionAttributeElement DatabaseDimensionAttribute { get; set; }
    }

    public class CubeDimensionHierarchyElement : SsasModelElement
    {
        public CubeDimensionHierarchyElement(RefPath refPath, string caption, string definition, CubeDimensionElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public HierarchyElement DatabaseDimensionHierarchy { get; set; }

        public IEnumerable<CubeDimensionHierarchyLevelElement> Levles { get { return ChildrenOfType<CubeDimensionHierarchyLevelElement>().OrderBy(hl => hl.Ordinal); } }
    }

    public class CubeDimensionHierarchyLevelElement : SsasModelElement
    {
        public CubeDimensionHierarchyLevelElement(RefPath refPath, string caption, string definition, CubeDimensionHierarchyElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public CubeDimensionAttributeElement Attribute { get; set; }

        [DataMember]
        public int Ordinal { get; set; }
    }

    public class MeasureGroupElement : SsasModelElement
    {
        public MeasureGroupElement(RefPath refPath, string caption, string definition, CubeElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public IEnumerable<MeasureGroupDimensionElement> MeasureGroupDimensions { get { return ChildrenOfType<MeasureGroupDimensionElement>(); } }
        public IEnumerable<MeasureElement> Measures { get { return ChildrenOfType<MeasureElement>(); } }
    }

    public abstract class MeasureGroupDimensionElement : SsasModelElement
    {
        public MeasureGroupDimensionElement(RefPath refPath, string caption, string definition, MeasureGroupElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public CubeDimensionElement CubeDimension { get; set; }

    }

    public class RegularMeasureGroupDimensionElement : MeasureGroupDimensionElement
    {
        public RegularMeasureGroupDimensionElement(RefPath refPath, string caption, string definition, MeasureGroupElement parent)
                : base(refPath, caption, definition, parent)
        { }
        
        [ModelLink]
        public DimensionAttributeElement ReferenceDimensionAttribute { get; set; }
        IEnumerable<MeasureGroupDimensionColumnBindingElement> ColumnBindings { get { return ChildrenOfType<MeasureGroupDimensionColumnBindingElement>(); } }
    }

    public class ReferencedMeasureGroupDimensionElement : MeasureGroupDimensionElement
    {
        public ReferencedMeasureGroupDimensionElement(RefPath refPath, string caption, string definition, MeasureGroupElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public DimensionAttributeElement ReferenceDimensionAttribute { get; set; }
        [ModelLink]
        public DimensionAttributeElement IntermediateDimensionAttribute { get; set; }
    }

    public class ManyToManyMeasureGroupDimensionElement : MeasureGroupDimensionElement
    {
        public ManyToManyMeasureGroupDimensionElement(RefPath refPath, string caption, string definition, MeasureGroupElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public MeasureGroupElement IntermediateMeasureGroup { get; set; }
    }

    public class DegenerateMeasureGroupDimensionElement : MeasureGroupDimensionElement
    {
        public DegenerateMeasureGroupDimensionElement(RefPath refPath, string caption, string definition, MeasureGroupElement parent)
                : base(refPath, caption, definition, parent)
        { }    
    }

    public class DataMiningMeasureGroupDimensionElement : MeasureGroupDimensionElement
    {
        public DataMiningMeasureGroupDimensionElement(RefPath refPath, string caption, string definition, MeasureGroupElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }
    
    public class MeasureGroupDimensionColumnBindingElement : SsasModelElement
    {
        public MeasureGroupDimensionColumnBindingElement(RefPath refPath, string caption, string definition, MeasureGroupDimensionElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public KeyColumnElement DimensionAttributeKeyColumn { get; set; }

        [ModelLink]
        public MssqlModelElement MeasureGroupSourceColumn { get; set; }

    }

    public abstract class PartitionElement : SsasModelElement
    {
        public PartitionElement(RefPath refPath, string caption, string definition, MeasureGroupElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public IEnumerable<PartitionColumnElement> Columns { get { return ChildrenOfType<PartitionColumnElement>(); } }
    }


    public class DsvTableBoundPartitionElement : PartitionElement
    {
        public DsvTableBoundPartitionElement(RefPath refPath, string caption, string definition, MeasureGroupElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public DatasourceViewTableElement DsvTable { get; set; }
    }

    public class QueryBoundPartitionElement : PartitionElement
    {
        public QueryBoundPartitionElement(RefPath refPath, string caption, string definition, MeasureGroupElement parent)
                : base(refPath, caption, definition, parent)
        { }
        
        public Db.SqlScriptElement SourceQuery { get { return ChildrenOfType<Db.SqlScriptElement>().First(); } }
    }

    public class TableBoundPartitionElement : PartitionElement
    {
        public TableBoundPartitionElement(RefPath refPath, string caption, string definition, MeasureGroupElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public Db.MssqlColumnScriptElement SourceTable { get; set; }
    }

    public class PartitionColumnElement : SsasModelElement
    {
        public PartitionColumnElement(RefPath refPath, string caption, string definition, PartitionElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public MssqlModelElement Source { get; set; }
    }

    public abstract class MeasureElement : SsasModelElement
    {
        public MeasureElement(RefPath refPath, string caption, string definition, MeasureGroupElement parent)
                : base(refPath, caption, definition, parent)
        { }

    }

    public class PhysicalMeasureElement : MeasureElement
    {
        public PhysicalMeasureElement(RefPath refPath, string caption, string definition, MeasureGroupElement parent)
                : base(refPath, caption, definition, parent)
        { }
        
        public IEnumerable<PhysicalMeasurePartitionSourceElement> Sources { get { return ChildrenOfType<PhysicalMeasurePartitionSourceElement>(); } }
        
    }

    public class PhysicalMeasurePartitionSourceElement : SsasModelElement
    {
        public PhysicalMeasurePartitionSourceElement(RefPath refPath, string caption, string definition, PhysicalMeasureElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public MssqlModelElement Source { get; set; }
    }
        
}
