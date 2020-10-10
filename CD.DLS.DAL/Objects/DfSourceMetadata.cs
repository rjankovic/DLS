using System;
using System.Collections.Generic;
using System.Data;
using CD.DLS.Common.Structures;

namespace CD.DLS.DAL.Objects.Inspect
{

    /*
ModelElementId INT,
Command NVARCHAR(MAX),
RefPath NVARCHAR(MAX),
Caption NVARCHAR(MAX),
ManagerCaption NVARCHAR(MAX),
SourceType NVARCHAR(MAX),
ConnectionString NVARCHAR(MAX),
LocaleID INT,
CodePage INT,
FileFormat NVARCHAR(MAX),
PackageElementId INT,
PackageRefPath NVARCHAR(MAX),
PackageCaption NVARCHAR(MAX)
     */

    public class DfSource
    {
        public int ModelElementId { get; set; }
        public string Command { get; set; }
        public string Refpath { get; set; }
        public string Name { get; set; }
        public string ManagerName { get; set; }
        public string SourceType { get; set; }
        public string ConnectionString { get; set; }
        public int LocaleID { get; set; }
        public int CodePage { get; set; }
        public string FileFormat { get; set; }
        public int PackageElementId { get; set; }
        public string PackageRefPath { get; set; }
        public string PackageName { get; set; }
        public List<DfSourceColumn> Columns { get; set; }
    }

    /*
     * 
SELECT se.ModelElementId SourceElementId, ce.ModelElementId, ce.Caption ColumnName
,JSON_VALUE(ce.ExtendedProperties, '$.DtsDataType') DataType
,JSON_VALUE(ce.ExtendedProperties, '$.Length') Length
,JSON_VALUE(ce.ExtendedProperties, '$.Precision') Precision
,JSON_VALUE(ce.ExtendedProperties, '$.Scale') Scale
,ce.RefPath
    */

    public class DfSourceColumn
    {
        public int ModelElementId { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public int Length { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
    }


}
