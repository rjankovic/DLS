using CD.DLS.DAL.Configuration;
using CD.DLS.Model.Interfaces;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Parse.Mssql.Db
{
    /// <summary>
    /// Types of references in SQL scripts.
    /// </summary>
    public enum ScriptReferenceTypeEnum
    {
        Table,
        Column,
        StoredProcedure,
        Function,
        Variable,
        InsertTarget,
        AssignmentTarget,
        TableType,
        NAryOperationOutputColumn
    }

    /// <summary>
    /// An object (table with an alias, subquery, ...) that is defined in the script and
    /// may be referred to by other parts of the statement or by other statements.
    /// </summary>
    public class ReferrableObject
    {
        public ScriptReferenceTypeEnum ReferenceType { get; set; }
        /// <summary>
        /// To what area this object applies (e.g. a CTE is applicable to all subsequent queries
        /// in a statement, but not to other statements in the batch).
        /// </summary>
        public ScriptSpan ValidSpan { get; set; }
        /// <summary>
        /// Single or multipart identifier of the object that can be used to refer to it (alias).
        /// </summary>
        public TSqlFragment Identifier { get; set; }
        /// <summary>
        /// The definition of the object.
        /// </summary>
        public TSqlFragment ObjectContent { get; set; }

        public RefPath Urn { get; set; }

        public IModelElement ModelElement { get; set; }

        public override string ToString()
        {
            return Urn.Path;
        }
    }

    /// <summary>
    /// Table (local or schema) / table-valued function / SP with resultset
    /// </summary>
    public class TableSourceColumnList : ReferrableObject
    {
        public List<ReferrableObject> Columns { get; set; }

        public List<TableSourceColumnList> ReferencedByAliases
        {
            get
            {
                return _referencedByAliases;
            }

            set
            {
                _referencedByAliases = value;
            }
        }

        public TableSourceColumnList ReferencesTable { get; set; }

        public List<SelectStarExpression> SelectStars
        {
            get
            {
                return _selectStars;
            }

            set
            {
                _selectStars = value;
            }
        }

        public List<TableSourceColumnList> NAryOperationSourceTables
        {
            get { return _nAryOperationSourceTables; }
            set { _nAryOperationSourceTables = value; }
        }

        private List<TableSourceColumnList> _nAryOperationSourceTables = new List<TableSourceColumnList>();
        

        private List<TableSourceColumnList> _referencedByAliases = new List<TableSourceColumnList>();

        private List<SelectStarExpression> _selectStars = new List<SelectStarExpression>();

        public void PropagateResolvedColumns(HashSet<ReferrableObject> objectsInStack = null)
        {
            if (objectsInStack == null)
            {
                objectsInStack = new HashSet<ReferrableObject>();
            }

            objectsInStack.Add(this);
            if (Columns != null)
            {
                var ic = new IdentifierComparer(
                    new Identifier() { Value = "master" });
                foreach (var alias in ReferencedByAliases)
                {
                    if (alias.Columns == null)
                    {
                        alias.Columns = this.Columns;
                    }
                    else if (alias.Columns.Count == 0)
                    {
                        alias.Columns = this.Columns;
                    }
                    else
                    {
                        // add columns that are not found in the target table
                        foreach (var missingColumn in this.Columns
                            .Where(x => x.Identifier != null).Where(
                            sc => !alias.Columns.Any(tc => ic.IdentifiersEqual(
                                tc.Identifier, sc.Identifier))))
                        {
                            alias.Columns.Add(missingColumn);
                        }
                    }
                    if (objectsInStack.Contains(alias))
                    {
                        ConfigManager.Log.Warning("Cyclic alias reference: " + this.Identifier.GetText());
                    }
                    else
                    {
                        var nhs = new HashSet<ReferrableObject>(objectsInStack);
                        alias.PropagateResolvedColumns(nhs);
                    }
                }
            }
        }

    }
}
