using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects.Extract
{

    #region Multidimensional

    public class MultidimensionalDatabase : ExtractObject
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsasMultidimensionalDatabase;
        public override string Name => DbName;

        public string DbName { get; set; }
        public string DbID { get; set; }
        public string ServerName { get; set; }
        public string ServerID { get; set; }
    }

    public class MultidimensionalDsv : ExtractObject
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsasMultidimensionalDsv;
        public override string Name => DsvName;

        public string DsvName { get; set; }
        public string ID { get; set; }
        public List<MultidimensionalDsvTable> Tables { get; set; }
        public string ConnectionString { get; set; }
    }

    public class MultidimensionalNamedComponent
    {
        public string Name { get; set; }
        public string ID { get; set; }
    }

    public class MultidimensionalDsvTable
    {
        public string TableName { get; set; }
        public string TableType { get; set; }
        public string DbTableName { get; set; }
        public string DbSchemaName { get; set; }
        public string QueryDefinition { get; set; }
        public List<MultidimensionalDsvColumn> Columns { get; set; }
    }

    public class MultidimensionalDsvColumn
    {
        public string ColumnName { get; set; }
        public string ComputedColumnExpression { get; set; }
    }


    public class MultidimensionalDimension : ExtractObject
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsasMultidimensionalDimension;
        public override string Name => DimensionName;

        public string DimensionName { get; set; }
        public string ID { get; set; }
        public string DataSourceViewID { get; set; }
        public List<DimensionAttribute> Attributes { get; set; }
        public List<DimensionHierarchy> Hierarchies { get; set; }
    }

    public class DimensionAttribute : MultidimensionalNamedComponent
    {
        public List<MultidimensionalColumnBinding> KeyColumns { get; set; }
        public MultidimensionalColumnBinding ValueColumn { get; set; }
        public MultidimensionalColumnBinding NameColumn { get; set; }
        public MultidimensionalColumnBinding CustomRollupColumn { get; set; }
        public MultidimensionalColumnBinding CustomRollupPropertiesColumn { get; set; }
        public MultidimensionalColumnBinding UnaryOperatorColumn { get; set; }
        public List<string> RelatedAttributeIDs { get; set; }
        public bool AttributeHierarchyEnabled { get; set; }
    }

    public class MultidimensionalColumnBinding
    {
        public string ColumnID { get; set; }
        public string TableID { get; set; }
    }

    public class DimensionHierarchy : MultidimensionalNamedComponent
    {
        public List<DimensionHierarchyLevel> Levels { get; set; }
    }

    public class DimensionHierarchyLevel : MultidimensionalNamedComponent
    {
        public string SourceAttributeID { get; set; }
    }


    public class MultidimensionalCube : ExtractObject
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsasMultidimensionalCube;
        public override string Name => CubeName;

        public string CubeName { get; set; }
        public string ID { get; set; }
        public string DataSourceViewID { get; set; }

        public List<CubeDimension> Dimensions { get; set; }
        public List<CubeMeasureGroup> MeasureGroups { get; set; }
        public List<CubeMdxScript> MdxScripts { get; set; }
    }

    public class CubeDimension : MultidimensionalNamedComponent
    {
        public string DimensionID { get; set; }
    }

    public class CubeMeasureGroup : MultidimensionalNamedComponent
    {
        public List<MeasureGroupPartition> Partitions { get; set; }
        public List<MeasureGroupDimension> Dimensions { get; set; }
        public List<PhysicalCubeMeasure> Measures { get; set; }
    }

    public enum MeasureGroupPartitionBindingType { DsvTable, Query, DbTable }

    public class MeasureGroupPartition : MultidimensionalNamedComponent
    {
        public MeasureGroupPartitionBindingType BindingType { get; set; }
        public string DataSourceID { get; set; }
        public string TableID { get; set; }
        public string QueryDefinition { get; set; }
        public string TableBoundDbSchemaName { get; set; }
        public string DbTableName { get; set; }
        public string ConnectionString { get; set; }
    }
    
    public abstract class MeasureGroupDimension
    {
        public string DimensionID { get; set; }
        public string DimensionName { get; set; }
        public string CubeDimensionID { get; set; }
    }

    public class RegularMeasureGroupDimension : MeasureGroupDimension
    {
        public string GranularityAttributeID { get; set; }
        public List<MeasureGroupDimensionColumnMapping> DimensionColumnMappings { get; set; }
    }

    public class MeasureGroupDimensionColumnMapping
    {
        public string PartitionColumnId { get; set; }
        public string DimensionColumnId { get; set; }
    }

    public class ReferenceMeasureGroupDimension : MeasureGroupDimension
    {
        public string IntermediateCubeDimensionID { get; set; }
        public string IntermediateGranularityAttributeID { get; set; }
        public string IntermediateGranularityAttributeDimensionID { get; set; }
    }

    public class ManyToManyMeasureGroupDimension : MeasureGroupDimension
    {
        public string MeasureGroupID { get; set; }
    }

    public class DegenerateMeasureGroupDimension : MeasureGroupDimension
    {

    }

    public class DataMiningMeasureGroupDimension : MeasureGroupDimension
    {
        public string CaseCubeDimensionID { get; set; }
    }

    public enum PhysicalMeasureBindingType { RowBinding, ColumnBinding }

    public class PhysicalCubeMeasure : MultidimensionalNamedComponent
    {
        public PhysicalMeasureBindingType BindingType { get; set; }
        public string MeasureExpression { get; set; }
        public string ColumnID { get; set; }
    }

    public class CubeMdxScript : MultidimensionalNamedComponent
    {
        public string Command { get; set; }
    }
    

    #endregion

}
