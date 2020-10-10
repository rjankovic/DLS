using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace CD.DLS.Parse.Mssql.Db
{
    public class NAryQueryExpressionSequence : TSqlFragmentVisitor
    {
        public BinaryQueryExpression CoveringBinaryQueryExpression { get; set; }
        public List<QuerySpecification> BinaryQueryExpressionParts { get; set; }
        public QuerySpecification BinaryQueryExpressionHead { get; set; }
        public TableSourceColumnList MainTableSource { get; set; }
    }
}
