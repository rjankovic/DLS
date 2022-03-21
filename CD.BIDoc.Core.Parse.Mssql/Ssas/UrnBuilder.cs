using CD.DLS.Model.Mssql;
using System.Data;
using CD.DLS.Model.Mssql.Ssas;
using System.Linq;
using System;
using CD.DLS.Model.Interfaces;
using CD.DLS.DAL.Objects.Extract;
//using Microsoft.AnalysisServices;

namespace CD.DLS.Parse.Mssql.Ssas
{
    public class UrnBuilder
    {
        //internal RefPath GetUrn(Server server)
        //{
        //    return new RefPath().NamedChild("SSASServer", server.ID);
        //}

        public RefPath GetServerUrn(string serverId)
        {
            return new RefPath().NamedChild("SSASServer", serverId);
        }

        public RefPath GetDatabaseUrn(string dbId, RefPath parent)
        {
            return parent.NamedChild("Db", dbId);
        }

        public RefPath GetUrn(MultidimensionalDsv dsv, RefPath parent)
        {
            return parent.NamedChild("Dsv", dsv.ID);
        }

        public RefPath GetUrn(MultidimensionalDsvTable table, RefPath parent)
        {
            return parent.NamedChild("Table", table.TableName);
        }

        public RefPath GetUrn(MultidimensionalDsvColumn column, RefPath parent)
        {
            return parent.NamedChild("Column", column.ColumnName);
        }

        public RefPath GetUrn(MultidimensionalDimension dimension, RefPath parent)
        {
            return parent.NamedChild("Dimension", dimension.ID);
        }

        public RefPath GetUrn(DAL.Objects.Extract.DimensionAttribute attribute, RefPath parent)
        {
            return parent.NamedChild("Attribute", attribute.ID);
        }

        public RefPath GetRelatedAttributeUrn(string relatedAttributeId, RefPath parent)
        {
            return parent.NamedChild("RelatedAttribute", relatedAttributeId);
        }

        public RefPath GetKeyColumnUrn(MultidimensionalColumnBinding binding, RefPath parent)
        {
            return parent.NamedChild("KeyColumn", binding.ColumnID);
        }

        public RefPath GetNameColumnUrn(MultidimensionalColumnBinding binding, RefPath parent)
        {
            return parent.NamedChild("NameColumn", binding.ColumnID);
        }

        public RefPath GetValueColumnUrn(MultidimensionalColumnBinding binding, RefPath parent)
        {
            return parent.NamedChild("ValueColumn", binding.ColumnID);
        }

        public RefPath GetCustomRollupColumnUrn(MultidimensionalColumnBinding binding, RefPath parent)
        {
            return parent.NamedChild("CustomRollupColumn", binding.ColumnID);
        }

        public RefPath GetCustomRollupPropertiesColumnUrn(MultidimensionalColumnBinding binding, RefPath parent)
        {
            return parent.NamedChild("CustomRollupPropertiesColumn", binding.ColumnID);
        }

        public RefPath GetUnaryOperatorColumnUrn(MultidimensionalColumnBinding binding, RefPath parent)
        {
            return parent.NamedChild("UnaryOperatorColumn", binding.ColumnID);
        }

        public RefPath GetUrn(DimensionHierarchy hierarchy, RefPath parent)
        {
            return parent.NamedChild("Hierarchy", hierarchy.ID);
        }

        public RefPath GetUrn(DimensionHierarchyLevel level, RefPath parent)
        {
            return parent.NamedChild("Level", level.ID);
        }

        public RefPath GetUrn(MultidimensionalCube cube, RefPath parent)
        {
            return parent.NamedChild("Cube", cube.Name /*cube.ID*/);
        }

        public RefPath GetUrn(DAL.Objects.Extract.CubeDimension cubeDimension, RefPath parent)
        {
            return parent.NamedChild("CubeDimension", cubeDimension.ID);
        }

        public RefPath GetCubeDimensionAttributeUrn(DimensionAttributeElement dbAttribute, RefPath parent)
        {
            return parent.NamedChild("CubeDimensionAttribute", dbAttribute.SsasObjectID);
        }

        public RefPath GetCubeDimensionHierarchyUrn(HierarchyElement dbHierarchy, RefPath parent)
        {
            return parent.NamedChild("CubeDimensionHiearchy", dbHierarchy.SsasObjectID);
        }

        public RefPath GetCubeDimensionHierarchyLevelUrn(HierarchyLevelElement level, RefPath parent)
        {
            return parent.NamedChild("CubeDimensionHierarchyLevel", level.SsasObjectID);
        }

        public RefPath GetUrn(CubeMeasureGroup measureGroup, RefPath parent)
        {
            return parent.NamedChild("MeasureGroup", measureGroup.ID);
        }
        public RefPath GetUrn(MeasureGroupPartition partition, RefPath parent)
        {
            return parent.NamedChild("Partition", partition.ID);
        }

        public RefPath GetPartitionColumnUrn(string columneName, RefPath parent)
        {
            return parent.NamedChild("Column", columneName);
        }

        public RefPath GetUrn(DAL.Objects.Extract.MeasureGroupDimension mgDimension, RefPath parent)
        {
            return parent.NamedChild("MGDimension", mgDimension.CubeDimensionID);
        }

        public RefPath GetColumnBindingUrn(string from, string to, RefPath parent)
        {
            return new RefPath(string.Format("{0}/ColumnBinding[@From='{1}' and @To='{2}']", parent.Path, from, to));
        }

        public RefPath GetUrn(PhysicalCubeMeasure measure, RefPath parent)
        {
            return parent.NamedChild("Measure", measure.ID);
        }

        public RefPath GetMeasurePartitionSourceUrn(PartitionElement partition, RefPath parent)
        {
            return parent.NamedChild("PartitionSource", partition.Caption);
        }

        public RefPath GetCalculatedMeasureUrn(string name, RefPath parent)
        {
            return parent.NamedChild("CalculatedMeasure", name);
        }

        public RefPath GetMdxScriptUrn(string name, RefPath parent)
        {
            return parent.NamedChild("MdxScript", name);
        }

        public RefPath GetMdxCommandUrn(string name, RefPath parent)
        {
            return parent.NamedChild("Command", name);
        }

        public RefPath GetScriptSegmentUrn(int ordinal, RefPath parent)
        {
            return parent.NamedChild("Segment", "No_" + ordinal);
        }

        public RefPath GetStatementUrn(MssqlModelElement parent)
        {
            var ordinal = parent.Children.Where(x => x.RefPath.RefId.StartsWith("MdxStatement")).Count() + 1;
            return parent.RefPath.NamedChild("MdxStatement", "No_" + ordinal);
        }

        public RefPath GetScopeStatementUrn(int ordinal, RefPath parent)
        {
            return parent.NamedChild("Scope", "No_" + ordinal);
        }

        public static string GetDbRefPath(string connectionString)
        {
            var localhostName = System.Net.Dns.GetHostName();
            var segments = connectionString.Split(';');
            //var dataSourceSegment = segments.First(x => x.ToLower().StartsWith("data source"));
            var dbNameSegment = segments.FirstOrDefault(x => x.ToLower().StartsWith("initial catalog"));
            //var dataSource = dataSourceSegment.Substring(dataSourceSegment.IndexOf('=') + 1).Trim();
            string dbName = null;
            if (dbNameSegment != null)
            {
                dbName = dbNameSegment.Substring(dbNameSegment.IndexOf('=') + 1).Trim();
            }
            //bool isLocalhost = dataSource == "." || dataSource == "localhost" || dataSource == "(local)";
            string path = string.Empty;
            //if (isLocalhost)
            //{
            //    dataSource = System.Net.Dns.GetHostName();
            //}

            var serverName = Common.Tools.ConnectionStringTools.GetServerName(connectionString);
            var refPath = String.Format("SSASServer[@Name='{0}']", serverName);
            if (dbName != null)
            {
                refPath += String.Format("/Db[@Name='{0}']", dbName);
            }
            return refPath;
        }

        public static string GetCubeRefPath(string connectionString, string cubeName)
        {
            var dbRefPath = GetDbRefPath(connectionString);
            var cubeRefPath = string.Format("{0}/Cube[@Name='{1}']", dbRefPath, cubeName);
            return cubeRefPath;
        }

        internal RefPath GetUrnRoot(String serverName)
        {

            return new RefPath().NamedChild("TabularServer", serverName);

        }

        internal RefPath GetUrnModel(String modelName, RefPath parent)
        {

            return parent.NamedChild("Model", modelName);
        }

        internal RefPath GetUrnDataSource(String sourceName, RefPath parent)
        {

            return parent.NamedChild("DataSource", sourceName);
        }

        internal RefPath GetUrnTable(String tableName, RefPath parent)
        {

            return parent.NamedChild("Table", tableName);
        }

        internal RefPath GetUrnColumn(String columnName, RefPath parent)
        {

            return parent.NamedChild("Column", columnName);
        }

        internal RefPath GetUrnAnnotation(String annotationName, RefPath parent)
        {

            return parent.NamedChild("Annotation", annotationName);
        }

        internal RefPath GetUrnRelationship(string relationshipName, RefPath parent)
        {

            return parent.NamedChild("Relationship", relationshipName);
        }

        internal RefPath GetUrnHierarchy(String hierarchyName, RefPath parent)
        {

            return parent.NamedChild("Hierarchy", hierarchyName);
        }

        internal RefPath GetUrnPartition(String partitionName, RefPath parent)
        {

            return parent.NamedChild("Partition", partitionName);
        }

        internal RefPath GetUrnPartitionColumn(String columnName, RefPath parent)
        {

            return parent.NamedChild("Column", columnName);
        }

        internal RefPath GetUrnMeasure(String measureName, RefPath parent)
        {

            return parent.NamedChild("Measure", measureName);
        }

        internal RefPath GetUrnRelationship(String from, String to, RefPath parent)
        {

            return parent.NamedChild("Relationship", from + "->" + to);
        }

        internal RefPath GetUrnPerspective(String perspectiveName, RefPath parent)
        {

            return parent.NamedChild("Perspective", perspectiveName);
        }

        internal RefPath GetUrnHierarchyLevel(String levelName, RefPath parent)
        {

            return parent.NamedChild("HierarchyLevel", levelName);
        }

        internal RefPath GetDaxOperationOutputColumnUrn(string name, RefPath parent)
        {

            return parent.NamedChild("OutputColumn", name);
        }

        public RefPath GetDaxScriptUrn(MssqlModelElement parent)
        {
            var ordinal = parent.Children.OfType<DaxScriptElement>().Count() + 1;
            return parent.RefPath.NamedChild("DaxScript", "No_" + ordinal);
        }

        public RefPath GetDaxLocalMeasureUrn(DaxFragmentElement parent)
        {
            var ordinal = parent.Children.OfType<DaxLocalMeasureElement>().Count() + 1;
            return parent.RefPath.NamedChild("LocalMeasure", "No_" + ordinal);
        }

        public RefPath GetDaxLocalVariableUrn(DaxFragmentElement parent)
        {
            var ordinal = parent.Children.OfType<DaxLocalVariableElement>().Count() + 1;
            return parent.RefPath.NamedChild("LocalVariable", "No_" + ordinal);
        }

        public RefPath GetDaxTableReferenceUrn(DaxFragmentElement parent)
        {
            var ordinal = parent.Children.OfType<DaxTableReferenceElement>().Count() + 1;
            return parent.RefPath.NamedChild("TableReference", "No_" + ordinal);
        }

        public RefPath GetDaxColumnReferenceUrn(DaxFragmentElement parent)
        {
            var ordinal = parent.Children.OfType<DaxColumnReferenceElement>().Count() + 1;
            return parent.RefPath.NamedChild("ColumnReference", "No_" + ordinal);
        }

        public RefPath GetDaxOperationArgumentUrn(DaxFragmentElement parent)
        {
            var ordinal = parent.Children.OfType<DaxOperationArgumentElement>().Count() + 1;
            return parent.RefPath.NamedChild("Argument", "No_" + ordinal);
        }

        public RefPath GetDaxFragmentUrn(DaxFragmentElement parent)
        {
            var ordinal = parent.Children.Count() + 1;
            return parent.RefPath.NamedChild("Fragment", "No_" + ordinal);
        }
    }
}
