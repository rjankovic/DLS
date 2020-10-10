using System;
using System.Collections.Generic;
using System.Data;
using CD.DLS.Common.Structures;

namespace CD.DLS.DAL.Objects.Inspect
{
    /*
          ModelElementId	Caption	Type	TypeDescription	MaxParentLevel	ParentElementId	RefPath
3	NDWH_L1	CD.BIDoc.Core.Model.Mssql.Db.DatabaseElement	SQL Database	2	2	Server[@Name='FSCZPRCT0041']/Database[@Name='NDWH_L1']

     */

    public class ElementTreeListItem
    {
        public int ModelElementId { get; set; }
        public string Caption { get; set; }
        public string TypeDescription { get; set; }
        public string Type { get; set; }
        public int MaxParentLevel { get; set; }
        public int? ParentElementId { get; set; }
        public string RefPath { get; set; }
        public string Alias { get; set; }

        public bool IsBusinessFolder { get { return Type.EndsWith("BusinessFolderElement"); } }
        public bool IsPivotTableTemplate { get { return Type.EndsWith("PivotTableTemplateElement"); } }
    }

    //  td.ElementType, td.TypeDescription, n.NodeType

    public class ElementTypeDescription
    {
        public string ElementType { get; set; }
        public string NodeType { get; set; }
        public string TypeDescription { get; set; }
    }

    public class DataFlowBetweenGroupsItem
    {
        public int SourceElementId { get; set; }
        public int SourceNodeId { get; set; }
        public string SourceNodeName { get; set; }
        public string SourceElementRefPath { get; set; }
        public string SourceDescriptivePath { get; set; }

        public int TargetElementId { get; set; }
        public int TargetNodeId { get; set; }
        public string TargetNodeName { get; set; }
        public string TargetElementRefPath { get; set; }
        public string TargetDescriptivePath { get; set; }

    }

    public class FoundOlapFiled
    {
        public string RefPath { get; set; }
        public Guid ProjectConfigId { get; set; }
        public int ModelElementId { get; set; }
    }

    public class WarningMessagesItem
    {
        public string SourceName { get; set; }
        public string SourcePath { get; set; }
        public string TargetName { get; set; }
        public string TargetPath { get; set; }
        public string DataMessageType { get; set; }
        public string Message { get; set; }
    }

    
    public class ModelElementListingItem
    {
        public int ModelElementId { get; set; }
        public string RefPath { get; set; }
        public string Caption { get; set; }
        public string Type { get; set; }
        public string TypeDescription { get; set; }
        public string DescriptivePath { get; set; }
    }

    public class AnnotatedDependencySet
    {
        public List<string> FieldNames { get; set; }
        /// <summary>
        /// [ModelElementId], [ModelElementName], [TypeDescription], [Field1], [Field2], ...
        /// </summary>
        public DataTable ElementsTable { get; set; }
    }
}
