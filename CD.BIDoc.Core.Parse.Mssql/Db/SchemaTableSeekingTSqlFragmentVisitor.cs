using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace CD.DLS.Parse.Mssql.Db
{
    public class SchemaTableSeekingTsqlFragmentVisitor : TSqlFragmentVisitor
    {

        public List<SchemaObjectName> Tables = new List<SchemaObjectName>();

        /// <summary>
        /// Cleans up the references and objects discovered during the last script visit and visits the given script
        /// </summary>
        /// <param name="script"></param>
        public void CleanupAndVisit(TSqlFragment script)
        {
            Tables = new List<SchemaObjectName>();
            script.Accept(this);
            
            
        }
        
        /// <summary>
        /// If this references a schema object, a new table source is created (with the local span of the query);
        /// in either case a new unresolved reference is created for this to be linked either to the proper schema object or other local query specification 
        /// </summary>
        /// <param name="tableReference"></param>
        public override void Visit(NamedTableReference tableReference)
        {
            if (tableReference.SchemaObject != null)
            {
                Tables.Add(tableReference.SchemaObject);
            }
        }
    }
}
