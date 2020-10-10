using CD.DLS.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Mssql.Db
{

    public class SqlElement : MssqlModelElement
    {
        public SqlElement(RefPath refPath, string caption)
            : base(refPath, caption) {
        }
       /* public SqlElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null)
                : base(refPath, caption, definition, parent)
        { }*/
    }

    public class ForeignProviderSqlScriptElement : SqlElement
    {
        public ForeignProviderSqlScriptElement(RefPath refPath, string caption)
            : base(refPath, caption)
        {
        }
        /* public SqlElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null)
                 : base(refPath, caption, definition, parent)
         { }*/
    }


    [DataContract]
    public class SqlFragmentElement : SqlElement
    {
        public SqlFragmentElement(RefPath refPath, string caption)
            : base(refPath, caption) { }
        /*public SqlFragmentElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null)
                : base(refPath, caption, definition, parent)
        { }*/

        [DataMember]
        public int OffsetFrom { get; set; }
        [DataMember]
        public int Length { get; set; }
    }
    
    /// <summary>
    /// Can contain set operations
    /// </summary>
    public class SQLStatementElement : SqlFragmentElement
    {
        public SQLStatementElement(RefPath refPath, string caption)
                : base(refPath, caption)
        { }
    }

    /// <summary>
    /// Can contain set operations
    /// </summary>
    public class SQLSelectStatementElement : SQLStatementElement
    {
        public SQLSelectStatementElement(RefPath refPath, string caption)
                : base(refPath, caption)
        {
        }
    }



    public class SqlScriptElement : SqlFragmentElement
    {
        public SqlScriptElement(RefPath refPath, string caption)
                : base(refPath, caption)
        { }

    }

    public class SqlScriptResultElement : SqlElement
    {
        public SqlScriptResultElement(RefPath refPath, string caption)
                : base(refPath, caption)
        { }

        /// <summary>
        /// 0-based
        /// </summary>
        [DataMember]
        public int Ordinal { get; set; }

        public Dictionary<string, MssqlModelElement> OutputColumns
        {
            get
            {
                //var resultChild = ChildrenOfType<SqlScriptResultElement>().OrderBy(x => x.Ordinal).FirstOrDefault();
                //if (resultChild == null)
                //{
                //    return new Dictionary<string, MssqlModelElement>();
                //}
                return ChildrenOfType<SqlScriptResultColumnElement>()
                    .ToDictionary(x => x.Caption, x => x.ColumnSource);
            }
        }
        public List<Tuple<string, MssqlModelElement>> OutputColumnsOrdinal { get
            {
                return ChildrenOfType<SqlScriptResultColumnElement>()
                    .OrderBy(x => x.Ordinal)
                    .Select(y => new Tuple<string, MssqlModelElement>(y.Caption, y.ColumnSource))
                    .ToList();
            } }
    }

    /// <summary>
    /// Will not serve directly as a dataflow link, only provides ordianl access to the columns from a serialized model
    /// </summary>
    public class SqlScriptResultColumnElement : SqlElement
    {
        public SqlScriptResultColumnElement(RefPath refPath, string caption)
                : base(refPath, caption)
        { }
        /// <summary>
        /// 0-based
        /// </summary>
        [DataMember]
        public int Ordinal { get; set; }

        [ModelLink]
        public MssqlModelElement ColumnSource { get; set; }
    }

    // TODO: modified temporarily so that model building tests using scripts pass
    public class DeclareVariableElement : SqlScriptElement //: SqlFragmentElement
    {
        public DeclareVariableElement(RefPath refPath, string caption)
                : base(refPath, caption)
        { }

        [ModelLink]
        public SqlFragmentElement SqlTypeDefinition { get; set; }
    }

    public class SqlDmlSourceElement : SqlFragmentElement
    {
        public SqlDmlSourceElement(RefPath refPath, string caption)
            : base(refPath, caption) {
            if (refPath.Path == "Server[@Name='PC12\\SQL2012']/Database[@Name='ESS']/UserDefinedFunction[@Name='udf_GetObjectsToProcess' and @Schema='Admin']/[CREATE_0]/[SELECT_29]/[SELECT_30]/[SELECT_31]/[SELECT_32]/[[ParentDatabaseID]_33]")
            {

            }
        }
        
        [ModelLink]
        public MssqlModelElement TargetReference { get; set; }
    }

    public class SqlDmlTargetReferenceElement : SqlFragmentElement
    {
        public SqlDmlTargetReferenceElement(RefPath refPath, string caption)
            : base(refPath, caption) { }
        
    }

    
    public class SqlNAryOperationOutputColumnElement : SqlFragmentElement
    {
        public SqlNAryOperationOutputColumnElement(RefPath refPath, string caption)
            : base(refPath, caption)
        { }
        
        public List<SqlNAryOperationOperandColumnElement> OperandSources_DEBUG { get; set; }
    }

    public class SqlNAryOperationOperandColumnElement : SqlFragmentElement
    {
        public SqlNAryOperationOperandColumnElement(RefPath refPath, string caption)
            : base(refPath, caption) { }

        [ModelLink]
        public SqlNAryOperationOutputColumnElement OperationOutputColumn { get; set; }
    }


}
