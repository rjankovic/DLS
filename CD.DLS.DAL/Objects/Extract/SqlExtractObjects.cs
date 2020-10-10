using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects.Extract
{
    
    //public class SqlDbStructure : ExtractObject
    //{
    //    public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SqlDbStructure;
        
    //    public string DbName { get; set; }
    //    public string ServerName { get; set; }
    //    public string ServerUrn { get; set; }
    //    public string DbUrn { get; set; }
    //    public List<SqlSchema> Schemas { get; set; }

    //    public override string Name => DbName;
    //}
    
    public abstract class SmoObject : ExtractObject
    {
        public string ObjectName { get; set; }
        public string Urn { get; set; }
        //public string ServerName { get; set; }
        //public string DbName { get; set; }
        public string SchemaName { get; set; }

        public List<string> DefinitionScripts { get; set; }
        
        public override string Name => ObjectName;

        public override ExtractTypeEnum ExtractType
        {
            get {
                if (this is SqlTable)
                    return ExtractTypeEnum.SqlTable;
                if (this is SqlView)
                    return ExtractTypeEnum.SqlView;
                if (this is SqlTableType)
                    return ExtractTypeEnum.SqlTableType;
                if (this is SqlScalarUdf)
                    return ExtractTypeEnum.SqlScalarUdf;
                if (this is SqlTableUdf)
                    return ExtractTypeEnum.SqlTableUdf;
                if (this is SqlProcedure)
                    return ExtractTypeEnum.SqlProcedure;
                if (this is SqlSchema)
                    return ExtractTypeEnum.SqlSchema;

                return ExtractTypeEnum.NA;
            }
        }
    }

    public class SqlSchema : SmoObject
    {
    }


    public class SmoColumnsObject : SmoObject
    {
        public List<SqlColumn> Columns { get; set; }
    }

    public class SqlTable : SmoColumnsObject
    {
        public List<SqlForeignKey> ForeignKeys { get; set; }
    }

    public class SqlView : SmoColumnsObject
    {

    }

    public class SqlScalarUdf : SmoObject
    {

    }

    public class SqlTableUdf : SmoObject
    {

    }

    public class SqlProcedure : SmoObject
    {

    }

    public class SqlTableType : SmoColumnsObject
    {

    }

    public class SqlForeignKey : SmoObject
    {
        public string ReferencedTableSchema { get; set; }
        public string ReferencedTable { get; set; }
        public List<SqlForeignKeyColumn> Columns { get; set; }
    }

    public class SqlForeignKeyColumn
    {
        public string ColumnName { get; set; }
        public string ReferencedColumnName { get; set; }
    }

    public class SqlColumn : SmoObject
    {
        public int Length { get; set; }
        public int Scale { get; set; }
        public int Precision { get; set; }
        public string SqlDataType { get; set; }
    }

    //public class SqlDbScript : ExtractObject
    //{
    //    public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SqlDbScript;

    //    public string ScriptName { get; set; }
    //    public string Content { get; set; }

    //    public override string Name => ScriptName;
    //}

}
