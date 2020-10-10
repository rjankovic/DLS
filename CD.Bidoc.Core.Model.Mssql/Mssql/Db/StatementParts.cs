using CD.DLS.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Mssql.Db
{
#if false
    /// <summary>
    /// A complete SELECT statement - can contain unions, not CTEs or INTO clauses.
    /// Also used to represent CTEs and subqueries
    /// </summary>
    public class QueryExpressionNode : MssqlModelElement
{
    public QueryExpressionNode(DependencyNode self)
            : base(self)
    { }
}
#endif
    /// <summary>
    /// The main part of a QueryExpressionNode - no CTEs, can contain subqueries.
    /// </summary>
    public class QuerySpecificationNode : DbModelElement
    {
        public QuerySpecificationNode(RefPath refPath, string caption, string definition)
        : base(refPath, caption, definition)
        { }
    }
#if false
    /// <summary>
    /// The FROM clause of a QuerySpecification
    /// </summary>
    public class FromClauseNode : MssqlModelElement
    {
    public FromClauseNode(DependencyNode self)
            : base(self)
    { }
}
    /// <summary>
    /// A JOIN of 2 tables in the FROM clause.
    /// Typically, there will be only one, with the first "table" being the result
    /// of all the previous JOINs and the second containing only a reference to the last table
    /// in the FROM clause.
    /// </summary>
    public class JoinNode : MssqlModelElement
    {
        public JoinNode(DependencyNode self)
                : base(self)
        { }
    }

    /// <summary>
    /// The WHERE clause of a QuerySpecification
    /// </summary>
    public class WhereClauseNode : MssqlModelElement
    {
        public WhereClauseNode(DependencyNode self)
                : base(self)
        { }
    }

    /// <summary>
    /// The GROUP BY clause of a QuerySpecification
    /// </summary>
    public class GroupByClauseNode : MssqlModelElement
    {
        public GroupByClauseNode(DependencyNode self)
                : base(self)
        { }
    }

    /// <summary>
    /// The HAVING clause of a QuerySpecification
    /// </summary>
    public class HavingClauseNode : MssqlModelElement
    {
        public HavingClauseNode(DependencyNode self)
                : base(self)
        { }
    }


    /// <summary>
    /// The list of selected columns.
    /// </summary>
    public class SelectListNode : MssqlModelElement
    {
        public SelectListNode(DependencyNode self)
                : base(self)
        { }
    }

    /// <summary>
    /// A single column in the result of a SELECT.
    /// </summary>
    public class SelectListElementNode : MssqlModelElement
    {
        public SelectListElementNode(DependencyNode self)
                : base(self)
        { }
    }
    
    /// <summary>
    /// A reference to a table in the DB with an alias or a subquery
    /// </summary>
    public class TableReferenceNode : MssqlModelElement
    {
        public TableReferenceNode(DependencyNode self)
                : base(self)
        { }
    }

    /// <summary>
    /// A reference to an object in the database. All the links between statements 
    /// and other self-contained nodes are based on this.
    /// </summary>
    public class SqchemaObjectReferenceNode : MssqlModelElement
    {
        public Identifier DomIdentifier { get; set; }

        public SqchemaObjectReferenceNode(DependencyNode self)
                : base(self)
        { }
    }
#endif
}
