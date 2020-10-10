using CD.DLS.DAL.Configuration;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql.Db;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace CD.DLS.Parse.Mssql.Db
{
    /// <summary>
    /// A reference that can (but may not) be defined in the script.
    /// E.g. the reference to a CTE in the FROM clause of a query.
    /// </summary>
    public class UnresolvedReference
    {
        /// <summary>
        /// What kind of object to search for - table or column
        /// </summary>
        public ScriptReferenceTypeEnum ScriptReferenceType { get; set; }
        /// <summary>
        /// Position of the reference identifier in the script. 
        /// </summary>
        public ScriptSpan Position { get; set; }

        public ScriptSpan SearchSpan { get; set; }
        /// <summary>
        /// Is this a reference to a persistent schema object or to
        /// a local subquery / other statement?
        /// </summary>
        public bool IsDbObject { get; set; }
        /// <summary>
        /// The single or multipart identifier of the referred object.
        /// </summary>
        public TSqlFragment Identifier { get; set; }
        /// <summary>
        /// The fragment (preferrably smaller) that contains the reference
        /// and is on of the "fragments of interest".
        /// </summary>
        public TSqlFragment ReferenceFromObject { get; set; }
        ///// <summary>
        ///// Higher priority - is resolved first
        ///// CTEs have higher priority than the main query to make the CTE table sources available before resolving the main query
        ///// (visitor visits the main query first; not needed yet)
        ///// </summary>
        //public int ResolutionPriority { get; set; }

        public bool TableCanBeDefinedAfter
        {
            get
            { return _tableCanBeDefinedAfter; }

            set
            { _tableCanBeDefinedAfter = value; }
        }

        private bool _tableCanBeDefinedAfter = false;

        public TableSourceColumnList SelectIntoSource { get; set; }
    }

    /// <summary>
    /// A reference for which the source referred object has been identified.
    /// </summary>
    public class ResolvedReference
    {
        public ScriptReferenceTypeEnum ScriptReferenceType { get; set; }
        /// <summary>
        /// The referring object.
        /// </summary>
        public TSqlFragment FromFragment { get; set; }
        /// <summary>
        /// The reference object.
        /// </summary>
        public TSqlFragment ToFragment { get; set; }

        public RefPath ToUrn { get; set; }
        public IModelElement ToObject { get; set; }
        public Model.Mssql.MssqlModelElement FromObject { get; set; }

        /// <summary>
        /// The fragment containg the reference identifier in the referring object
        /// </summary>
        public TSqlFragment ReferenceFragment { get; set; }

        public TableSourceColumnList SelectIntoSource { get; set; }
    }


    public class ReferenceResolver
    {
        public List<UnresolvedReference> UnresolvedReferences { get; set; }
        public List<ResolvedReference> ResolvedReferences { get; set; }

        private SqlLocalIndex _localIndex;

        internal SqlLocalIndex LocalIndex { set { _localIndex = value; } }


        public ReferenceResolver()
        {
            UnresolvedReferences = new List<UnresolvedReference>();
            ResolvedReferences = new List<ResolvedReference>();
        }


        public void AddUnresolvedReference(UnresolvedReference unresolved)
        {
            UnresolvedReferences.Add(unresolved);
        }
        public void AddResolvedReference(ResolvedReference resolved)
        {
            //if (resolved.FromFragment == null)
            //{

            //}
            //if (resolved.ToFragment == null)
            //{

            //}
            ResolvedReferences.Add(resolved);
        }


        /// <summary>
        /// Iterates the collection of unresolved references until no other references can be resolved.
        /// The available table sources need to be initialized before using this.
        /// The visitor does not call this method automatically, so after visiting a script,
        /// there will be many unresolved references
        /// </summary>
        public void TryResolveReferences(DependencySeekingTSqlFragmentVisitor v)
        {
            var tablesResolved = false;
            int resolutionIdx = 0;
            int resolvedCount = 0;
            while (resolutionIdx < UnresolvedReferences.Count)
            {
                tablesResolved = !(UnresolvedReferences.Any(x => x.ScriptReferenceType == ScriptReferenceTypeEnum.Table));
                var reference = UnresolvedReferences[resolutionIdx];

                bool resolved = TryResolveReference(reference, tablesResolved, v);

                if (resolved)
                {
                    if (reference.ScriptReferenceType == ScriptReferenceTypeEnum.Table)
                    {
                        // resolved a table reference; start again from top
                        resolvedCount = 0;
                        resolutionIdx = 0;
                        continue;
                    }
                    resolvedCount++;
                }
                else
                {
                    resolutionIdx++;
                }

                // if something has been successfully resolved, start from the beginning
                if (resolutionIdx == UnresolvedReferences.Count)
                {
                    if (resolvedCount > 0)
                    {
                        resolvedCount = 0;
                        resolutionIdx = 0;
                    }
                }

            }

            TryResolveInsertTargets(v);
            //var resolutions = ResolvedReferences.Select(x => new Tuple<string, int, int>(x.FromFragment.GetText(), x.FromFragment.FirstTokenIndex, x.FromFragment.LastTokenIndex)).ToList();
        }

        public void TryResolveInsertTargets(DependencySeekingTSqlFragmentVisitor v)
        {
            foreach (var insertSpan in v.InsertSpans)
            {
                var targetColumns = _localIndex.GetInsertTargetColumnList(insertSpan);
                var sourceTable = _localIndex.GetInsertTableSource(insertSpan);
                // INSERT INTO t VALUES(a,b,c) - without target table specifiaction;
                // resolve schema table and use direct column mapping
                List<ReferrableObject> targetColumnsDirect = null;
                if (targetColumns == null)
                {
                    var schemaTableReference = _localIndex.GetInsertTargetTableReference(insertSpan);
                    if (schemaTableReference is NamedTableReference)
                    {
                        var tableDirect = _localIndex.FindTableSource(insertSpan, ((NamedTableReference)schemaTableReference).SchemaObject);
                        targetColumnsDirect = tableDirect.Columns;
                    }
                }
                if ((targetColumns == null && targetColumnsDirect == null) || sourceTable == null)
                {
                    continue;
                }
                
                if (targetColumns != null)
                {
                    if (targetColumns.Count != sourceTable.Columns.Count)
                    {
                        // for now - ignore faulty DML column mappings
                        // this can happen with temp tables and table-valued variables
                        continue;

                        // throw new Exception("Insert list column number does not match source column number");
                    }

                    for (int i = 0; i < sourceTable.Columns.Count; i++)
                    {
                        var sourceColumn = sourceTable.Columns[i];
                        var targetColumn = targetColumns[i];

                        // SELECT * in INSERT ?
                        if (sourceColumn.ObjectContent != null)
                        {
                            ResolvedReferences.Add(new ResolvedReference()
                            {
                                FromFragment = sourceColumn.ObjectContent,
                                ToFragment = targetColumn,
                                ReferenceFragment = sourceColumn.ObjectContent,
                                ScriptReferenceType = ScriptReferenceTypeEnum.InsertTarget
                            });
                        }
                        else if (sourceColumn.ModelElement != null)
                        {
                            ResolvedReferences.Add(new ResolvedReference()
                            {
                                FromObject = (DbModelElement)(sourceColumn.ModelElement),
                                ToFragment = targetColumn,
                                ReferenceFragment = null, // sourceTable.ObjectContent, // sourceColumn.ObjectContent,
                                ScriptReferenceType = ScriptReferenceTypeEnum.InsertTarget
                            });
                        }
                    }
                }
                // use direct column mapping
                else
                {
                    if (targetColumnsDirect.Count != sourceTable.Columns.Count)
                    {
                        continue;
                    }

                    for (int i = 0; i < sourceTable.Columns.Count; i++)
                    {
                        var sourceColumn = sourceTable.Columns[i];
                        var targetColumn = targetColumnsDirect[i];

                        // SELECT * in INSERT ?
                        if (sourceColumn.ObjectContent != null)
                        {
                            ResolvedReferences.Add(new ResolvedReference()
                            {
                                FromFragment = sourceColumn.ObjectContent,
                                ToFragment = targetColumn.Identifier,
                                ReferenceFragment = sourceColumn.ObjectContent,
                                ScriptReferenceType = ScriptReferenceTypeEnum.InsertTarget,
                                ToObject = targetColumn.ModelElement
                            });
                        }
                        else if (sourceColumn.ModelElement != null)
                        {
                            ResolvedReferences.Add(new ResolvedReference()
                            {
                                FromObject = (DbModelElement)(sourceColumn.ModelElement),
                                ToFragment = targetColumn.Identifier,
                                ReferenceFragment = null, // sourceTable.ObjectContent, // sourceColumn.ObjectContent,
                                ScriptReferenceType = ScriptReferenceTypeEnum.InsertTarget,
                                ToObject = targetColumn.ModelElement
                            });
                        }
                    }
                }
            }

            var insertIntoShcemaObjects = ResolvedReferences.Where(x => x.ScriptReferenceType == ScriptReferenceTypeEnum.Table && x.SelectIntoSource != null).ToList();
            foreach (var selectIntoSchemaTableReference in insertIntoShcemaObjects)
            {
                var tableSource = selectIntoSchemaTableReference.SelectIntoSource;
                var targetTable = selectIntoSchemaTableReference.ToObject as SchemaTableElement;

                if (targetTable == null)
                {
                    ConfigManager.Log.Warning(string.Format("Could not cast select into target reference {0} as schema table", selectIntoSchemaTableReference.ReferenceFragment.GetText()));
                    continue;
                }

                if (tableSource.Columns.Count != targetTable.Columns.Count())
                {
                    ConfigManager.Log.Warning(string.Format("Column count for select into target {0} does not match", selectIntoSchemaTableReference.ReferenceFragment.GetText()));
                    continue;
                }

                var comparer = new IdentifierComparer(_localIndex.DbInUse);

                for (int i = 0; i < tableSource.Columns.Count; i++)
                {
                    var sourceColumn = tableSource.Columns[i];
                    var columnIdentifier = new Identifier() { Value = sourceColumn.Identifier.GetText() };    
                    var targetColumn = targetTable.Columns.First(x => comparer.IdentifiersEqual(new Identifier() { Value = x.Caption }, sourceColumn.Identifier));

                    ResolvedReferences.Add(new ResolvedReference()
                    {
                        FromFragment = sourceColumn.ObjectContent,
                        ToFragment = columnIdentifier,
                        ReferenceFragment = sourceColumn.ObjectContent,
                        ScriptReferenceType = ScriptReferenceTypeEnum.InsertTarget,
                        ToObject = targetColumn
                    });
                }

            }
        }


        public bool TryResolveReference(UnresolvedReference reference, bool tablesResolved, DependencySeekingTSqlFragmentVisitor v)
        {

            bool resolved = false;

            var tablesInRange = _localIndex.GetCandidateTables(reference.Position).Where(t=>reference.SearchSpan.Contains(t.ValidSpan) && t.Columns != null && t.Columns.Count > 0).ToList();

            ReferrableObject resolution = null;


            if (reference.ReferenceFromObject is ColumnReferenceExpression && reference.Identifier is MultiPartIdentifier && reference.ScriptReferenceType == ScriptReferenceTypeEnum.Column)
            {
                bool cont = true;
                while (cont)
                {
                    var mpi = reference.Identifier as MultiPartIdentifier;

                    // SELECT col FROM table1 INNER JOIN table2 ON table1.a = table2.a - where does col come from?
                    // special treatment
                    if (mpi.Count == 1)
                    {

                        if (v.CurrentMergeInsertAction != null)
                        {
                            if (new ScriptSpan(v.CurrentMergeInsertAction).Contains(new ScriptSpan(reference.Identifier)))
                            {
                                // resolve using merge source columns
                                if (new ScriptSpan(v.CurrentMergeInsertAction.Source).Contains(new ScriptSpan(reference.Identifier)))
                                {
                                    resolution = _localIndex.FindTableColumnByIdentifier(v.CurrentMergeSource, mpi.Identifiers[0]); // v.CurrentMergeTarget;
                                }
                                // resolve merge target columns
                                else
                                {
                                    resolution = _localIndex.FindTableColumnByIdentifier(v.CurrentMergeTarget, mpi.Identifiers[0]); // v.CurrentMergeTarget;
                                }
                                break;
                            }
                        }

                        tablesInRange = _localIndex.GetLocalCandidateTables(reference.Position).Where(t => (reference.SearchSpan.Contains(new ScriptSpan(t.ObjectContent))) && t.Columns != null).ToList();

                        var identifierComparer = new IdentifierComparer(_localIndex.DbInUse);

                        var tablesWithThisColumn = tablesInRange/*.Where(x => x.Columns != null)*/.Where(x => x.Columns.Any(col => identifierComparer.IdentifiersEqual(col.Identifier, mpi.Identifiers[0]))).ToList();
                        if (tablesWithThisColumn.Count == 1)
                        {
                            resolution = _localIndex.FindTableColumnByIdentifier(tablesWithThisColumn.First(), mpi.Identifiers[0]);
                            break;
                        }
                        tablesWithThisColumn = tablesWithThisColumn.Where(x => x.ObjectContent.FirstTokenIndex < reference.ReferenceFromObject.FirstTokenIndex).ToList();
                        if (tablesWithThisColumn.Count == 1)
                        {
                            resolution = _localIndex.FindTableColumnByIdentifier(tablesWithThisColumn.First(), mpi.Identifiers[0]);
                            break;
                        }
                    }
                    else
                    {
                        var tableIdentifier = new MultiPartIdentifier();
                        for (int i = 0; i < mpi.Identifiers.Count - 1; i++)
                        {
                            tableIdentifier.Identifiers.Add(mpi.Identifiers[i]);
                        }
                        var matchingTable = _localIndex.FindTableByIdentifier(tablesInRange, tableIdentifier);
                        if (matchingTable != null)
                        {
                            if (matchingTable.Columns != null)
                            {
                                var matchingColumn = _localIndex.FindTableColumnByIdentifier(matchingTable, mpi.Identifiers.Last());
                                // this means invalid query
                                // TODO: notify about incorrect queries (one day) 
                                if (matchingColumn != null)
                                {
                                    resolution = matchingColumn;
                                    break;
                                }
                            }
                        }
                    }

                    cont = tablesResolved && v.ParentQuerySpans.ContainsKey(reference.SearchSpan);
                    if (cont)
                    {
                        reference.SearchSpan = v.ParentQuerySpans[reference.SearchSpan];
                    }

                }
            }
            else if (reference.ScriptReferenceType == ScriptReferenceTypeEnum.Table)
            {
                if (reference.Identifier is SchemaObjectName)
                {
                    var schemaObjectname = reference.Identifier as SchemaObjectName;
                    var matchingTable = _localIndex.FindTableByIdentifier(tablesInRange, schemaObjectname, reference.TableCanBeDefinedAfter ? -1 : reference.Position.TokenFrom);
                    /*
                    if (matchingTable == null)
                    {

                    }
                    */
                    if (matchingTable != null && matchingTable.Columns != null)
                    {
                        

                        var tableSourceResolved = _localIndex.TableSourceByObjectContent(reference.ReferenceFromObject);
                        if (tableSourceResolved != null)
                        {
                            resolution = matchingTable;
                            if (reference.ReferenceFromObject is NamedTableReference)
                            {
                                var ntr = reference.ReferenceFromObject as NamedTableReference;
                                if (ntr.Alias != null && tableSourceResolved.Columns != null)
                                {
                                    _localIndex.AddLocalTableSource(new TableSourceColumnList()
                                    {
                                        Columns = tableSourceResolved.Columns,
                                        Identifier = ntr.Alias,
                                        ValidSpan = reference.SearchSpan,
                                        ObjectContent = ntr,
                                        ReferenceType = ScriptReferenceTypeEnum.Table
                                    });
                                }
                            }

                            if (tableSourceResolved.Columns == null)
                            {
                                tableSourceResolved.Columns = matchingTable.Columns;
                                tableSourceResolved.PropagateResolvedColumns();
                            }

                            if (!matchingTable.ReferencedByAliases.Contains(tableSourceResolved))
                            {
                                matchingTable.ReferencedByAliases.Add(tableSourceResolved);
                                matchingTable.PropagateResolvedColumns();
                            }
                        }
                    }

                }
                // select star expression, assumably
                else
                {
                    var tablesInRangeAfterReference = tablesInRange.Where(x => x.ObjectContent.FirstTokenIndex > reference.Position.TokenFrom).OrderBy(x => x.ObjectContent.FirstTokenIndex).ToList();

                    // SELECT x.* FROM x
                    var matchingTable = _localIndex.FindTableByIdentifier(tablesInRangeAfterReference, reference.Identifier);
                    // SELECT * FROM x
                    if (matchingTable == null && reference.Identifier == null && tablesInRangeAfterReference.Count >= 1)
                    {
                        matchingTable = tablesInRangeAfterReference[0];
                    }
                    if (matchingTable == null)
                    {
                        matchingTable = _localIndex.FindTableByIdentifier(tablesInRange, reference.Identifier);
                    }
                    // its not a real resolution if its columns are yet to be determined
                    if (matchingTable != null && matchingTable.Columns != null)
                    {
                        resolution = matchingTable;

                        var tableSourceResolved = _localIndex.TableSourceByObjectContent(reference.ReferenceFromObject);
                        if (tableSourceResolved == null)
                        {
                            
                            tableSourceResolved = _localIndex.TableSourceBeingDefinedByStarObject(reference.ReferenceFromObject);
                        }
                        if (tableSourceResolved != null)
                        {
                            if (!(reference.ReferenceFromObject is SelectStarExpression))
                            {
                                if (tableSourceResolved.Columns == null)
                                {
                                    tableSourceResolved.Columns = matchingTable.Columns;
                                    tableSourceResolved.PropagateResolvedColumns();
                                }
                                //// the resolved table source already has some other resolved columns (SELECT x.*, a, b, FOM x, t...)
                                //else
                                //{
                                //    foreach (var column in matchingTable.Columns)
                                //    {
                                //        tableSourceResolved.Columns.Add(column);
                                //    }
                                //    tableSourceResolved.PropagateResolvedColumns();
                                //}
                            }
                            else
                            {
                                if (tableSourceResolved.Columns == null)
                                {
                                    tableSourceResolved.Columns = new List<ReferrableObject>();
                                }
                                int insertAtPosition = 0;

                                // determine the position of the star (count the columns that precede the star)
                                foreach (var existentColumn in tableSourceResolved.Columns)
                                {
                                    if (existentColumn.ObjectContent != null)
                                    {
                                        if (existentColumn.ObjectContent.FragmentLength < reference.ReferenceFromObject.FirstTokenIndex)
                                        {
                                            insertAtPosition++;
                                        }
                                    }
                                }

                                tableSourceResolved.Columns.InsertRange(insertAtPosition, matchingTable.Columns);
                                
                                tableSourceResolved.PropagateResolvedColumns();
                            }

                            // whenever the matching table gets extended, the columns will be added
                            if (!matchingTable.ReferencedByAliases.Contains(tableSourceResolved))
                            {
                                matchingTable.ReferencedByAliases.Add(tableSourceResolved);
                                matchingTable.PropagateResolvedColumns();
                            }
                        }
                        //foreach (var alias in matchingTable.ReferencedByAliases)
                        //{

                        //}
                    }
                }

            }
            if (resolution != null)
            {
                var rr = new ResolvedReference()
                {
                    FromFragment = reference.ReferenceFromObject ?? reference.Identifier,
                    ToFragment = resolution.ObjectContent ?? resolution.Identifier,
                    ReferenceFragment = reference.Identifier,
                    ToUrn = resolution.Urn,
                    ToObject = resolution.ModelElement,
                    ScriptReferenceType = reference.ScriptReferenceType,
                    SelectIntoSource = reference.SelectIntoSource
                };
                ResolvedReferences.Add(rr);
                resolved = true;
            }

            if (resolved)
            {
                UnresolvedReferences.Remove(reference);
            }
            return resolved;
        }





    }
}
