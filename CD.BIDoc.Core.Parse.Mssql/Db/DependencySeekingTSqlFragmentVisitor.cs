using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using CD.DLS.Model.Interfaces;

namespace CD.DLS.Parse.Mssql.Db
{
    /// <summary>
    /// Identifies significant parts of a TSQL script (may contain multiple batches)
    /// and the connections between objects contained in this script
    /// </summary>
    public class DependencySeekingTSqlFragmentVisitor : TSqlFragmentVisitor
    {
        /// <summary>
        /// Future dependency nodes
        /// </summary>
        private List<TSqlFragment> _fragmentsOfInterest;
 
        private ScriptSpan BatchSpan { get; set; }
        private ScriptSpan StatementSpan { get; set; }
        private ScriptSpan QuerySpan { get; set; }
        //private ScriptSpan CteSpan { get; set; }
        private Stack<ScriptSpan> NestedQuerySpans { get; set; }
        private ScriptSpan QueryFromClauseSpan { get; set; }
        private ScriptSpan TopLevelSourceExclusionSpan { get; set; }
        private ScriptSpan CurrentInsertSpan { get; set; }
        private ScriptSpan SetVariableSpan { get; set; }
        private InsertSpecification CurrentInsertSpecification { get; set; }
        public Dictionary<ScriptSpan, ScriptSpan> ParentQuerySpans { get; set; }
        public InsertMergeAction CurrentMergeInsertAction { get; set; }
        public TableSourceColumnList CurrentMergeTarget { get; set; }
        // info only
        public TableSourceColumnList CurrentMergeSource { get; set; }
        public TableReference CurrentMergeSourceReference { get; set; }
        //public TableSourceColumnList CurrentUpdateTarget { get; set; }


        //private Stack<ScriptSpan> BinaryQueryExpressionSpanStack { get; set; }
        //private bool CanNAryQueryExpressionBegin { get; set; }
        //private int NAryExpressionPartNumber { get; set; }

        //private BinaryQueryExpression CoveringBinaryQueryExpression { get; set; }
        //private List<QuerySpecification> BinaryQueryExpressionParts { get; set; }
        //private QuerySpecification BinaryQueryExpressionHead { get; set; }
        
        private Stack<NAryQueryExpressionSequence> NaryExpressionStack = new Stack<NAryQueryExpressionSequence>();
        
        private ScriptSpan DeleteUpdateStatementSpan { get; set; }
        private ScriptSpan DeleteUpdatePrefromSpan { get; set; }
        private ScriptSpan BooleanExpressionSpan { get; set; }
        private Tuple<IList<Identifier>, ScriptSpan> CteColumns { get; set; }
        public TableSourceColumnList FirstTopLevelTableSource { get; private set; }
        public List<TableSourceColumnList> TempTablesDefined { get; private set; }
        public ReferrableObject FirstProcedureCall { get; private set; }
        public TableSourceColumnList FirstProcedureCallResultSet { get; private set; }
        private ScriptSpan CreateProcedureReferenceSpan {get; set;}

        public List<ScriptSpan> InsertSpans { get; set; }

        private ReferenceResolver _referenceResolver;
        private IReferrableIndex _index;
        private SqlLocalIndex _localIndex;

        private Identifier _dbInUse;

        /// <summary>
        /// Public properties need to be defined before use
        /// </summary>
        public DependencySeekingTSqlFragmentVisitor(Identifier databaseInUse, IReferrableIndex index, ReferenceResolver resolver)
        {
            this._index = index;

            _dbInUse = databaseInUse;

            _fragmentsOfInterest = new List<TSqlFragment>();
            _referenceResolver = resolver;
        
            NestedQuerySpans = new Stack<ScriptSpan>();

    
            //BinaryQueryExpressionSpanStack = new Stack<ScriptSpan>();
        }

        /// <summary>
        /// Cleans up the references and objects discovered during the last script visit
        /// </summary>
        public void Cleanup()
        {
            BatchSpan = null;
            CurrentSpanFragment = null;
            QueryFromClauseSpan = null;
            QuerySpan = null;
            StatementSpan = null;
            NestedQuerySpans = new Stack<ScriptSpan>();
            FirstTopLevelTableSource = null;
            TempTablesDefined = new List<TableSourceColumnList>();
            FirstProcedureCall = null;
            FirstProcedureCallResultSet = null;
            TopLevelSourceExclusionSpan = null;
            SetVariableSpan = null;
            /*
            ScalarUdfsAvailable.Clear();
            StoredProceduresAvailable.Clear();
            */
            //CanNAryQueryExpressionBegin = true;
            //BinaryQueryExpressionSpanStack = new Stack<ScriptSpan>();
            //NAryExpressionPartNumber = 0;
            CteColumns = null;
            CreateProcedureReferenceSpan = null;
            CurrentInsertSpan = null;
            InsertSpans = new List<ScriptSpan>();
            CurrentInsertSpecification = null;
            CurrentMergeSourceReference = null;
            CurrentMergeSource = null;
            _fragmentsOfInterest = new List<TSqlFragment>();
            //CurrentUpdateTarget = null;
            //CteSpan = null;
            BooleanExpressionSpan = null;
            /*
            CoveringBinaryQueryExpression = null;
            BinaryQueryExpressionParts = null;
            BinaryQueryExpressionHead = null;
            */

            NaryExpressionStack = new Stack<NAryQueryExpressionSequence>();
        }

        /// <summary>
        /// Cleans up the references and objects discovered during the last script visit and visits the given script
        /// </summary>
        /// <param name="script"></param>
        public FragmentTreeNode CleanupAndVisit(TSqlFragment script)
        {
            Cleanup();
            if (!(script is TSqlBatch))
            {
                BatchSpan = new ScriptSpan(script);
            }
            
            _localIndex = new SqlLocalIndex(_index, BatchSpan);
            _localIndex.DbInUse = _dbInUse;
            _referenceResolver.LocalIndex = _localIndex;
            script.Accept(this);

            // finalize nary expressions
            while (NaryExpressionStack.Count > 0)
            {
                    PopNaryExpressionStack();
            }

            FragmentTreeBuilder ftb = new FragmentTreeBuilder();

            _fragmentsOfInterest = _fragmentsOfInterest.Distinct().ToList();
            return ftb.BuildFragmentTree(script, _fragmentsOfInterest);
            /*
            var list = FragmentTree.ToList();
            var firstQs = FragmentTree.FirstOrDefault(x => x.Fragment is QuerySpecification);
            List<SelectElement> cols;
            if (firstQs != null)
            {
                cols = firstQs.Children.Where(x => x.Fragment is SelectElement).Select(x => (SelectElement)(x.Fragment)).ToList();
            }
            */
            
        }
        
        private TSqlFragment CurrentSpanFragment
        {
            get
            {
                return _currentSpanFragment;
            }

            set
            {
                _currentSpanFragment = value;
            }
        }

        private TSqlFragment _currentSpanFragment;
        
        public override void Visit(TSqlBatch batch)
        {
            BatchSpan = new ScriptSpan(batch);
        }

        public override void Visit(TSqlStatement statement)
        {
            StatementSpan = new ScriptSpan(statement);
            ParentQuerySpans = new Dictionary<ScriptSpan, ScriptSpan>();
            DeleteUpdatePrefromSpan = null;
            DeleteUpdateStatementSpan = null;
            CurrentMergeInsertAction = null;
            CurrentMergeTarget = null;
            CurrentInsertSpan = null;
        }

        public override void Visit(TSqlFragment frag)
        {

            if (frag.FirstTokenIndex > -1)
            {
                if (frag.GetText().Contains("SELECT Sum(plneni_m) FROM @krivka_plneni WHERE period > @Period"))
                {
                }
            }
            if (QuerySpan == null || frag is QuerySpecification)
            {
                return;
            }
            while (frag.LastTokenIndex > QuerySpan.TokenTo)
            {
                NestedQuerySpans.Pop();
                if (NestedQuerySpans.Count > 0)
                {
                    QuerySpan = NestedQuerySpans.Peek();
                }
                else
                {
                    QuerySpan = null;
                    return;
                }
            }

            // gone beyond the nAry query expression
            while (NaryExpressionStack.Count > 0)
            {
                var topNaryItem = NaryExpressionStack.Peek();
                var nArySpan = new ScriptSpan(topNaryItem.CoveringBinaryQueryExpression);
                var fragSpan = new ScriptSpan(frag);
                if (!nArySpan.Contains(fragSpan))
                {
                    PopNaryExpressionStack();
                    //NaryExpressionStack.Pop();
                    //BinaryQueryExpressionParts.Clear();
                    //CoveringBinaryQueryExpression = null;
                    //BinaryQueryExpressionHead = null;
                }
                else
                {
                    break;
                }
            }
            

            //while (BinaryQueryExpressionSpanStack.Count > 0)
            //{
            //    // still in the binary scope
            //    if (BinaryQueryExpressionSpanStack.Peek().Contains(new ScriptSpan(frag)))
            //    {
            //        break;
            //    }
            //    BinaryQueryExpressionSpanStack.Pop();
            //    if (BinaryQueryExpressionSpanStack.Count == 0)
            //    {
            //        NAryExpressionPartNumber = 0;
            //    }
            //}
        }

        /// <summary>
        /// Besides popping the top of the stack, fix the scope of the top table operand and the reference types of the (virtual) main table output columns
        /// </summary>
        private void PopNaryExpressionStack()
        {
            var peek = NaryExpressionStack.Peek();
            var covering = peek.CoveringBinaryQueryExpression;
            var topPart = peek.BinaryQueryExpressionParts.OrderBy(x => x.FirstTokenIndex).First();

            var sourceTables = peek.MainTableSource.NAryOperationSourceTables;
            var coveringSourceTable = sourceTables.FirstOrDefault(x => new ScriptSpan(x.ObjectContent).Contains(new ScriptSpan(covering)));
            if (coveringSourceTable != null)
            {
                sourceTables.Remove(coveringSourceTable);

                var topSourceTable = new TableSourceColumnList()
                {
                    ObjectContent = topPart,
                    ValidSpan = new ScriptSpan(topPart),
                    SelectStars = new List<SelectStarExpression>(),
                    Columns = new List<ReferrableObject>()
                };

                foreach (var column in coveringSourceTable.Columns)
                {
                    topSourceTable.Columns.Add(new ReferrableObject()
                    {
                        Identifier = column.Identifier,
                        ObjectContent = column.ObjectContent,
                        ReferenceType = ScriptReferenceTypeEnum.Column,
                        ValidSpan = topSourceTable.ValidSpan
                    });
                }

                foreach (var selectStar in coveringSourceTable.SelectStars)
                {
                    topSourceTable.SelectStars.Add(selectStar);
                }

                sourceTables.Add(topSourceTable);

                /*
                 else if (!(selectElement is SelectStarExpression)) // (columnName != null) // anonymous columns in inserts
                    {
                        correspondingTableSource.Columns.Add(new ReferrableObject()
                        {
                            Identifier = columnName,
                            ObjectContent = selectElement,
                            ReferenceType = ScriptReferenceTypeEnum.Column,
                            ValidSpan = QuerySpan ?? StatementSpan
                        });
                    }
                 */
            }

            foreach (var column in peek.MainTableSource.Columns)
            {
                column.ReferenceType = ScriptReferenceTypeEnum.NAryOperationOutputColumn;
                
            }

            foreach (var partTable in peek.MainTableSource.NAryOperationSourceTables)
            {
                for (int i = 0; i < peek.MainTableSource.Columns.Count; i++)
                {
                    if (partTable.Columns.Count > i)
                    {
                        _referenceResolver.AddResolvedReference(new ResolvedReference()
                        {
                            FromFragment = partTable.Columns[i].ObjectContent,
                            ToFragment = peek.MainTableSource.Columns[i].ObjectContent,
                            ReferenceFragment = partTable.Columns[i].ObjectContent,
                            ScriptReferenceType = ScriptReferenceTypeEnum.NAryOperationOutputColumn
                        });
                    }
                }
            }


            //_referenceResolver.AddResolvedReference(new ResolvedReference()
            //{
            //    FromFragment = valuesInsertSource,
            //    ToFragment = CurrentInsertSpecification.Target,
            //    ReferenceFragment = valuesInsertSource,
            //    ScriptReferenceType = ScriptReferenceTypeEnum.InsertTarget
            //});
            //_fragmentsOfInterest.Add(valuesInsertSource);

            NaryExpressionStack.Pop();
        }

        /// <summary>
        /// Sets the script span of the current query and fills the table source
        /// that is being defined now and can be referenced by other queries, 
        /// based on the select elements
        /// </summary>
        /// <param name="querySpecification"></param>
        public override void Visit(QuerySpecification querySpecification)
        {
            //QuerySpan = QuerySpan == null ? StatementSpan : new ScriptSpan(querySpecification);
            QuerySpan = new ScriptSpan(querySpecification);
            if (querySpecification.FromClause != null)
            {
                QueryFromClauseSpan = new ScriptSpan(querySpecification.FromClause);
            }
            else
            {
                QueryFromClauseSpan = null;
            }
            //FragmentsOfInterest.Add(querySpecification);
            var correspondingTableSource = _localIndex.GetTableSourceBeingDefined(NestedQuerySpans.Count, QuerySpan);

            //// unions
            //if (NAryExpressionPartNumber > 0)
            //{
            //    correspondingTableSource = null;
            //}
            //if (BinaryQueryExpressionSpanStack.Count > 0)
            //{
            //    if (BinaryQueryExpressionSpanStack.Peek().Contains(QuerySpan))
            //    {
            //        NAryExpressionPartNumber++;
            //    }
            //}



            while (NaryExpressionStack.Count > 0)
            {
                var topNaryItem = NaryExpressionStack.Peek();
                var nArySpan = new ScriptSpan(topNaryItem.CoveringBinaryQueryExpression);
                if (!nArySpan.Contains(QuerySpan))
                {
                    PopNaryExpressionStack();
                }
                else
                {
                    break;
                }
            }

            NAryQueryExpressionSequence nAryStackPeek = null;
            int nAryHeadFirstTokenIndex = -1;
            if (NaryExpressionStack.Count > 0)
            {
                nAryStackPeek = NaryExpressionStack.Peek();
                nAryHeadFirstTokenIndex = nAryStackPeek.BinaryQueryExpressionHead.FirstTokenIndex;
            }

            if (NestedQuerySpans.Count == 0 && correspondingTableSource == null && _localIndex.GetTableSourceBeingDefined(NestedQuerySpans.Count, QuerySpan) == null &&
                (TopLevelSourceExclusionSpan == null || !TopLevelSourceExclusionSpan.Contains(QuerySpan)) && FirstTopLevelTableSource == null)
            {
                // "SELECT @x = a, @y = b" does not count as top level source
                bool allSetVariables = querySpecification.SelectElements.All(x => x is SelectSetVariable);
                
                if (!allSetVariables)
                {
                    if (
                        (BooleanExpressionSpan == null || !BooleanExpressionSpan.Contains(QuerySpan)) 
                        && (DeleteUpdateStatementSpan == null || !DeleteUpdateStatementSpan.Contains(QuerySpan))
                        && (SetVariableSpan == null || !SetVariableSpan.Contains(QuerySpan))
                        && (nAryStackPeek == null || nAryHeadFirstTokenIndex == querySpecification.FirstTokenIndex)
                        )
                    {
                        if (nAryStackPeek == null)
                        {
                            FirstTopLevelTableSource = new TableSourceColumnList()
                            {
                                Columns = new List<ReferrableObject>(),
                                ObjectContent = querySpecification,
                                SelectStars = new List<SelectStarExpression>(),
                                ValidSpan = new ScriptSpan(0, 0)
                            }
                            ;
                        }
                        else
                        {
                            FirstTopLevelTableSource = new TableSourceColumnList()
                            {
                                Columns = new List<ReferrableObject>(),
                                ObjectContent = nAryStackPeek.CoveringBinaryQueryExpression,
                                SelectStars = new List<SelectStarExpression>(),
                                ValidSpan = new ScriptSpan(0, 0)
                            }
                            ;
                        }

                        FirstProcedureCall = null;
                        _localIndex.AddTableSourceBeingDefined(NestedQuerySpans.Count, FirstTopLevelTableSource);
                        correspondingTableSource = FirstTopLevelTableSource;
                    }
                }
            }

            bool isInsertSource = false;

            if (CurrentInsertSpan != null && NestedQuerySpans.Count == 0 && correspondingTableSource == null && _localIndex.GetTableSourceBeingDefined(NestedQuerySpans.Count, QuerySpan) == null && CurrentInsertSpan.Contains(QuerySpan)
                && CurrentInsertSpecification.InsertSource is SelectInsertSource)
            {
                var insertSource /*= FirstTopLevelTableSource*/ = new TableSourceColumnList()
                {
                    Columns = new List<ReferrableObject>(),
                    ObjectContent = querySpecification,
                    SelectStars = new List<SelectStarExpression>(),
                    ValidSpan = new ScriptSpan(0, 0)
                };
                isInsertSource = true;
                _localIndex.AddInsertTableSource(insertSource);
                _localIndex.AddTableSourceBeingDefined(NestedQuerySpans.Count, insertSource);
                correspondingTableSource = insertSource;
            }

            if (nAryStackPeek != null)
            {
                if (correspondingTableSource == null && nAryStackPeek.BinaryQueryExpressionParts.Any(x => x.FirstTokenIndex == querySpecification.FirstTokenIndex))
                {
                    correspondingTableSource = new TableSourceColumnList()
                    {
                        Columns = new List<ReferrableObject>(),
                        ObjectContent = querySpecification,
                        SelectStars = new List<SelectStarExpression>(),
                        ValidSpan = new ScriptSpan(querySpecification)
                    };
                    _localIndex.AddTableSourceBeingDefined(NestedQuerySpans.Count, correspondingTableSource);
                }
            }

            //union end
            int selectElementIdx = 0;
            foreach (var selectElement in querySpecification.SelectElements)
            {
                _fragmentsOfInterest.Add(selectElement);
                if (correspondingTableSource != null)
                {
                    // TODO: Solve select set variable (SELECT @x = t.a) and SELECT * later
                    Identifier columnName = null;

                    if (selectElement is SelectScalarExpression)
                    {
                        var selectScalarElement = selectElement as SelectScalarExpression;
                        if (CteColumns != null && CteColumns.Item2.Contains(QuerySpan))
                        {
                            columnName = CteColumns.Item1[selectElementIdx];
                        }
                        else if (selectScalarElement.ColumnName != null)
                        {
                            columnName = selectScalarElement.ColumnName.Identifier;
                            if (columnName == null)
                            {
                                columnName = new Identifier() { Value = selectScalarElement.ColumnName.Value };
                            }
                            //_referenceResolver.AddResolvedReference(new ResolvedReference()
                            //{
                            //    FromFragment = selectScalarElement,
                            //    ToFragment = selectScalarElement.Expression,
                            //    ReferenceFragment = selectScalarElement,
                            //    ScriptReferenceType = ScriptReferenceTypeEnum.Column
                            //});
                        }
                        else if (selectScalarElement.Expression is ColumnReferenceExpression)
                        {
                            var referenceExpression = selectScalarElement.Expression as ColumnReferenceExpression;
                            columnName = referenceExpression.MultiPartIdentifier.Identifiers.Last();
                        }
                        else if (isInsertSource && _localIndex.GetInsertTargetColumnList(CurrentInsertSpan) != null)
                        {
                            columnName = _localIndex.GetInsertTargetColumnList(CurrentInsertSpan)[selectElementIdx].MultiPartIdentifier.Identifiers.Last();
                        }

                    }
                    else if (selectElement is SelectStarExpression)
                    {
                        var starElemnt = selectElement as SelectStarExpression;
                        correspondingTableSource.SelectStars.Add(starElemnt);
                        _referenceResolver.UnresolvedReferences.Add(new UnresolvedReference()
                        {
                            ScriptReferenceType = ScriptReferenceTypeEnum.Table,
                            Identifier = starElemnt.Qualifier,
                            Position = new ScriptSpan(starElemnt),
                            IsDbObject = false,
                            ReferenceFromObject = selectElement,
                            SearchSpan = StatementSpan
                        });
                        /*
                        correspondingTableSource.Columns.Add(new ReferrableObject()
                        {
                            Identifier = columnName,
                            IsSchemaObject = false,
                            ObjectContent = selectElement,
                            ReferenceType = ScriptReferenceTypeEnum.Column,
                            ValidSpan = QuerySpan
                        });
                        */
                    }
                    if (correspondingTableSource != null 
                        && columnName == null 
                        && !(selectElement is SelectStarExpression) 
                        && !(selectElement is SelectScalarExpression && (((SelectScalarExpression)selectElement).Expression is FunctionCall 
                            || ((SelectScalarExpression)selectElement).Expression is VariableReference
                            || ((SelectScalarExpression)selectElement).Expression is ValueExpression
                            || ((SelectScalarExpression)selectElement).Expression is CaseExpression
                            ))
                        && correspondingTableSource != FirstTopLevelTableSource 
                        && !(isInsertSource && _localIndex.GetInsertTargetColumnList(CurrentInsertSpan) == null))
                    {
                        // not for now
                        // throw new Exception(String.Format("Could not recognize column name in {0}", selectElement.GetText()));
                    }
                    else if (!(selectElement is SelectStarExpression)) // (columnName != null) // anonymous columns in inserts
                    {
                        correspondingTableSource.Columns.Add(new ReferrableObject()
                        {
                            Identifier = columnName,
                            ObjectContent = selectElement,
                            ReferenceType = ScriptReferenceTypeEnum.Column,
                            ValidSpan = QuerySpan ?? StatementSpan
                        });
                    }
                }

                selectElementIdx++;
            }

            if (nAryHeadFirstTokenIndex == querySpecification.FirstTokenIndex)
            {
                nAryStackPeek.MainTableSource = correspondingTableSource;
            }

            if (nAryStackPeek != null)
            {
                var naryParts = nAryStackPeek.BinaryQueryExpressionParts;
                if (naryParts.Any(x => new ScriptSpan(x).Contains(QuerySpan)))
                {
                    var correspondingNAryPart = naryParts.First(x => new ScriptSpan(x).Contains(QuerySpan));
                    if (!nAryStackPeek.MainTableSource.NAryOperationSourceTables.Any(x => x.ObjectContent.FirstTokenIndex == correspondingNAryPart.FirstTokenIndex) && correspondingTableSource != null)
                    {
                        nAryStackPeek.MainTableSource.NAryOperationSourceTables.Add(correspondingTableSource);
                    }
                }
            }

            if (correspondingTableSource != null && correspondingTableSource.SelectStars.Count == 0)
            {
                _localIndex.RemoveTableSourcesBeingDefined(NestedQuerySpans.Count, correspondingTableSource);
            }

            if (NestedQuerySpans.Count > 0)
            {
                ParentQuerySpans[QuerySpan] = NestedQuerySpans.Peek();
            }
            else
            {
                ParentQuerySpans[QuerySpan] = StatementSpan;
            }
            NestedQuerySpans.Push(QuerySpan);
        }

        public override void Visit(ValuesInsertSource valuesInsertSource)
        {
            // MERGE can also have a ValuesInsertSource
            if (CurrentInsertSpan != null && CurrentInsertSpan.Contains(new ScriptSpan(valuesInsertSource)) && CurrentInsertSpecification.InsertSource is ValuesInsertSource)
            {
                _referenceResolver.AddResolvedReference(new ResolvedReference()
                {
                    FromFragment = valuesInsertSource,
                    ToFragment = CurrentInsertSpecification.Target,
                    ReferenceFragment = valuesInsertSource,
                    ScriptReferenceType = ScriptReferenceTypeEnum.InsertTarget
                });
                _fragmentsOfInterest.Add(valuesInsertSource);
            }

            if (CurrentMergeInsertAction != null && new ScriptSpan(CurrentMergeInsertAction).Contains(new ScriptSpan(valuesInsertSource)))
            {
                foreach (var rowValue in valuesInsertSource.RowValues)
                {
                    for (int i = 0; i < CurrentMergeInsertAction.Columns.Count; i++)
                    {
                        var sourceRef = rowValue.ColumnValues[i];
                        var targetRef = CurrentMergeInsertAction.Columns[i];
                        _referenceResolver.AddResolvedReference(new ResolvedReference()
                        {
                            FromFragment = sourceRef,
                            ToFragment = targetRef,
                            ReferenceFragment = valuesInsertSource,
                            ScriptReferenceType = ScriptReferenceTypeEnum.InsertTarget
                        });
                        //_fragmentsOfInterest.Add(sourceRef);
                    }
                }
                _fragmentsOfInterest.Add(valuesInsertSource);
            }
        }

        public override void Visit(AssignmentSetClause setClause)
        {
            //if (CurrentUpdateTarget == null)
            //{
            //    return;
            //}

            var source = setClause.NewValue;
            TSqlFragment target = setClause.Column;
            if (setClause.Column == null)
            {
                target = setClause.Variable;
            }
            _referenceResolver.AddResolvedReference(new ResolvedReference()
            {
                FromFragment = source,
                ToFragment = target,
                ReferenceFragment = source,
                ScriptReferenceType = ScriptReferenceTypeEnum.AssignmentTarget
            });
            _fragmentsOfInterest.Add(source);
        
        }

        public override void Visit(CreateTableStatement createTable)
        {
            var createdTableSource = new TableSourceColumnList()
            {
                Columns = new List<ReferrableObject>(),
                ObjectContent = createTable,
                Identifier = createTable.SchemaObjectName,
                SelectStars = new List<SelectStarExpression>(),
                ValidSpan = new ScriptSpan(createTable.LastTokenIndex, BatchSpan.TokenTo)
            };

            _localIndex.AddLocalTableSource(createdTableSource);
            _fragmentsOfInterest.Add(createTable);


            foreach (ColumnDefinition column in createTable.Definition.ColumnDefinitions)
            {
                var columnName = column.ColumnIdentifier;

                createdTableSource.Columns.Add(new ReferrableObject()
                {
                    Identifier = columnName,
                    ObjectContent = column,
                    ReferenceType = ScriptReferenceTypeEnum.Column,
                    ValidSpan = new ScriptSpan(createTable)
                });

                _fragmentsOfInterest.Add(column);
                _fragmentsOfInterest.Add(column.ColumnIdentifier);
            }

            var firstId = createTable.SchemaObjectName.Identifiers[0];
            // temp table
            if (firstId.Value.StartsWith("#"))
            {
                TempTablesDefined.Add(createdTableSource);
            }
        }

        public override void Visit(SelectStatement selectStatement)
        {
            // dont add - added while visiting TsqlStatement
            //FragmentsOfInterest.Add(selectStatement);

            if (selectStatement.GetText().Contains("INTO #Payment_T"))
            { 
            
            }
            if (selectStatement.Into is SchemaObjectName)
            {
                var selectIntoSource = new TableSourceColumnList()
                {
                    Columns = new List<ReferrableObject>(),
                    ObjectContent = selectStatement,
                    Identifier = selectStatement.Into,
                    SelectStars = new List<SelectStarExpression>(),
                    ValidSpan = new ScriptSpan(selectStatement.LastTokenIndex, BatchSpan.TokenTo)
                };

                _localIndex.AddLocalTableSource(selectIntoSource);
                _localIndex.AddSelectIntoBeingDefined(selectIntoSource);

                var lastId = selectStatement.Into.Identifiers[selectStatement.Into.Identifiers.Count - 1];
                // temp table
                if (lastId.Value.StartsWith("#"))
                {
                    TempTablesDefined.Add(selectIntoSource);
                }
                else
                {
                    _referenceResolver.AddUnresolvedReference(new UnresolvedReference()
                    {
                        ScriptReferenceType = ScriptReferenceTypeEnum.Table,
                        Identifier = selectStatement.Into,
                        Position = new ScriptSpan(selectStatement.Into),
                        IsDbObject = true,
                        ReferenceFromObject = selectStatement.Into,
                        SearchSpan = ScriptSpan.DbWide,
                        TableCanBeDefinedAfter = false,
                        SelectIntoSource = selectIntoSource
                    });

                    _fragmentsOfInterest.Add(selectStatement.Into);
                }
            }

            _fragmentsOfInterest.Add(selectStatement);
        }
        
        public override void Visit(UpdateStatement updateStatement)
        {
            // similar logic to DELTE applies
            DeleteUpdateStatementSpan = new ScriptSpan(updateStatement);
            if (updateStatement.UpdateSpecification.FromClause != null)
            {
                DeleteUpdatePrefromSpan = new ScriptSpan(updateStatement.FirstTokenIndex, updateStatement.UpdateSpecification.FromClause.FirstTokenIndex - 1);
            }
        }

        public override void Visit(InsertStatement insertStatement)
        {
            TopLevelSourceExclusionSpan = new ScriptSpan(insertStatement.InsertSpecification);
            CurrentInsertSpan = new ScriptSpan(insertStatement);
            CurrentInsertSpecification = insertStatement.InsertSpecification;
            InsertSpans.Add(CurrentInsertSpan);
            _localIndex.AddInsertTargetColumnList(insertStatement.InsertSpecification);
            _localIndex.AddInsertTargetTableReference(insertStatement.InsertSpecification);
            if (insertStatement.GetText().Contains("INSERT INTO [dbo].[FactOrganizationAcMMonth]"))
            {

            }
        }
        public override void Visit(DeleteStatement deleteStatement)
        {
            // table references can usually appear only in from clauses (other clauses use column refs)
            // and only after the table is mentioned for the first time,
            // but DELETE s FROM tab s WHERE ... violates this - if a table reference appears in
            // a delete prefrom span, allow searching for the table resolution after the reference
            DeleteUpdateStatementSpan = new ScriptSpan(deleteStatement);
            if (deleteStatement.DeleteSpecification.FromClause != null)
            {
                DeleteUpdatePrefromSpan = new ScriptSpan(deleteStatement.FirstTokenIndex, deleteStatement.DeleteSpecification.FromClause.FirstTokenIndex - 1);
            }
        }

        public override void Visit(MergeStatement mergeStatement)
        {
            TopLevelSourceExclusionSpan = new ScriptSpan(mergeStatement);
            // the target table reference is not part of a queryexpression
            // treat is a namedtableexpression with guaranteed presence

            var mergeTarget = (NamedTableReference)(mergeStatement.MergeSpecification.Target);
            
            _referenceResolver.AddUnresolvedReference(new UnresolvedReference()
            {
                ScriptReferenceType = ScriptReferenceTypeEnum.Table,
                Identifier = mergeTarget.SchemaObject,
                Position = new ScriptSpan(mergeTarget.SchemaObject),
                IsDbObject = true,
                ReferenceFromObject = mergeTarget,
                SearchSpan = ScriptSpan.DbWide,
                TableCanBeDefinedAfter = false
            });

            var schemaOrPredefinedTable = _localIndex.FindTableSource(StatementSpan, mergeTarget.SchemaObject) ;

            if (schemaOrPredefinedTable == null)
            {
                // this can happen only in an invalid package referencing a table that does not exist in the database
                return;
                //throw new Exception();
            }

            var newTableSource = new TableSourceColumnList()
            {
                Columns = schemaOrPredefinedTable.Columns,
                ValidSpan = StatementSpan,
                Identifier = ((TSqlFragment)(mergeStatement.MergeSpecification.TableAlias)) ?? mergeTarget.SchemaObject,
                ObjectContent = mergeTarget,
                ReferenceType = ScriptReferenceTypeEnum.Table
            };

            schemaOrPredefinedTable.ReferencedByAliases.Add(newTableSource);
            newTableSource.ReferencesTable = schemaOrPredefinedTable;
            _localIndex.AddLocalTableSource(newTableSource);

            CurrentMergeSourceReference = mergeStatement.MergeSpecification.TableReference;

            var insert = mergeStatement.MergeSpecification.ActionClauses.FirstOrDefault(x => x.Action is InsertMergeAction);
            if (insert != null)
            {
                CurrentMergeInsertAction = insert.Action as InsertMergeAction;
                CurrentMergeTarget = newTableSource;
            }
        }

        public override void Visit(DeclareVariableStatement declareVariableStatement)
        {
            TopLevelSourceExclusionSpan = new ScriptSpan(declareVariableStatement);
        }

        public override void Visit(SetVariableStatement setVariableStatement)
        {
            SetVariableSpan = new ScriptSpan(setVariableStatement);
        }

        /// <summary>
        /// Creates a new table source - the columns are not available at this point,
        /// will be created when visiting the first QueryExpression
        /// </summary>
        /// <param name="cte"></param>
        public override void Visit(CommonTableExpression cte)
        {
            List<ReferrableObject> columns = new List<ReferrableObject>();
            if (cte.Columns != null && cte.Columns.Count > 0)
            {
                CteColumns = new Tuple<IList<Identifier>, ScriptSpan>(cte.Columns, new ScriptSpan(cte));
            }
            var tableSource = new TableSourceColumnList()
            {
                Columns = new List<ReferrableObject>(),
                Identifier = cte.ExpressionName,
                ValidSpan = StatementSpan,
                ObjectContent = cte,
                ReferenceType = ScriptReferenceTypeEnum.Table
            };
            _localIndex.AddLocalTableSource(tableSource);
            _localIndex.AddTableSourceBeingDefined(NestedQuerySpans.Count, tableSource);
            _fragmentsOfInterest.Add(cte);
        }

        /// <summary>
        /// E.g. a subquery in a JOIN
        /// </summary>
        /// <param name="queryDerivedTable"></param>
        public override void Visit(QueryDerivedTable queryDerivedTable)
        {
            List<ReferrableObject> columns = new List<ReferrableObject>();
            var tableSource = new TableSourceColumnList()
            {
                Columns = new List<ReferrableObject>(),
                Identifier = queryDerivedTable.Alias,
                ValidSpan = QuerySpan ?? StatementSpan,
                ObjectContent = queryDerivedTable,
                ReferenceType = ScriptReferenceTypeEnum.Table
            };

            _localIndex.AddLocalTableSource(tableSource);
            _localIndex.AddTableSourceBeingDefined(NestedQuerySpans.Count, tableSource);
            _fragmentsOfInterest.Add(queryDerivedTable);
            //CanNAryQueryExpressionBegin = true;

        }

        public override void Visit(QueryExpression queryExpression)
        {
            _fragmentsOfInterest.Add(queryExpression);
        }

        public override void Visit(BinaryQueryExpression binaryQueryExpression)
        {
            var queryParts = GetNAryOperands(binaryQueryExpression);
            if (NaryExpressionStack.Count > 0)
            {
                var peekTable = NaryExpressionStack.Peek();
                // this is a continuation of a previously handled NAry query
                if (peekTable.BinaryQueryExpressionParts.Any(x => x.FirstTokenIndex == queryParts[0].FirstTokenIndex))
                {
                    return;
                }
            }

            NaryExpressionStack.Push(new NAryQueryExpressionSequence()
            {
                BinaryQueryExpressionHead = queryParts[0],
                CoveringBinaryQueryExpression = binaryQueryExpression,
                BinaryQueryExpressionParts = queryParts
            });
            

            /*
            // a sub-binary under the main binary expr
            if (CoveringBinaryQueryExpression != null)
            {
                return;
            }

            CoveringBinaryQueryExpression = binaryQueryExpression;
            BinaryQueryExpressionParts = GetNAryOperands(binaryQueryExpression);
            BinaryQueryExpressionHead = BinaryQueryExpressionParts.OrderBy(x => x.FirstTokenIndex).First();
            */

            //if (BinaryQueryExpressionSpanStack.Count > 0)
            //{
            //    // this is a continuation of a N-ary query expression
            //    if (BinaryQueryExpressionSpanStack.Peek().Contains(new ScriptSpan(binaryQueryExpression))
            //        && CanNAryQueryExpressionBegin == false)
            //    {
            //        return;
            //    }
            //}

            //BinaryQueryExpressionSpanStack.Push(new ScriptSpan(binaryQueryExpression));
            //CanNAryQueryExpressionBegin = false;
            //NAryExpressionPartNumber = 0;
        }

        public List<QuerySpecification> GetNAryOperands(QueryExpression expr)
        {
            if (expr is QueryParenthesisExpression)
            {
                var parenthesis = expr as QueryParenthesisExpression;
                return GetNAryOperands(parenthesis.QueryExpression);
            }
            else if (expr is QuerySpecification)
            {
                return new List<QuerySpecification>() { (QuerySpecification)expr };
            }
            else
            {
                var binary = expr as BinaryQueryExpression;
                return GetNAryOperands(binary.FirstQueryExpression).Union(GetNAryOperands(binary.SecondQueryExpression)).ToList();
            }
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
                _referenceResolver.AddUnresolvedReference(new UnresolvedReference()
                {
                    ScriptReferenceType = ScriptReferenceTypeEnum.Table,
                    Identifier = tableReference.SchemaObject,
                    Position = new ScriptSpan(tableReference.SchemaObject),
                    IsDbObject = true,
                    ReferenceFromObject = tableReference,
                    SearchSpan = ScriptSpan.DbWide,
                    TableCanBeDefinedAfter = DeleteUpdatePrefromSpan != null && DeleteUpdatePrefromSpan.Contains(new ScriptSpan(tableReference))
                });

                
                var schemaOrPredefinedTable = _localIndex.FindTableSource(QuerySpan ?? StatementSpan, tableReference.SchemaObject);

                var newTableSource = new TableSourceColumnList()
                {
                    Columns = ((schemaOrPredefinedTable == null) ? null : (schemaOrPredefinedTable.Columns)),
                    ValidSpan = QuerySpan ?? StatementSpan,
                    Identifier = ((TSqlFragment)tableReference.Alias) ?? tableReference.SchemaObject,
                    ObjectContent = tableReference,
                    ReferenceType = ScriptReferenceTypeEnum.Table
                };

                if (CurrentMergeSourceReference != null)
                {
                    var mergeSourceRefSpan = new ScriptSpan(CurrentMergeSourceReference);
                    if (mergeSourceRefSpan.Contains(new ScriptSpan(tableReference)))
                    {
                        CurrentMergeSource = newTableSource;
                    }
                }

                // the alias references a table whose columns have not been determinded yet - set the columns once the source table is resolved
                if (schemaOrPredefinedTable != null && schemaOrPredefinedTable.Columns == null)
                {
                    schemaOrPredefinedTable.ReferencedByAliases.Add(newTableSource);
                    newTableSource.ReferencesTable = schemaOrPredefinedTable;
                }
                else if (schemaOrPredefinedTable == null && tableReference.Alias != null && tableReference.SchemaObject != null)
                {
                    _referenceResolver.AddUnresolvedReference(new UnresolvedReference()
                    {
                        ScriptReferenceType = ScriptReferenceTypeEnum.Table,
                        Identifier = tableReference.SchemaObject,
                        Position = new ScriptSpan(tableReference.Alias),
                        IsDbObject = false,
                        ReferenceFromObject = tableReference,
                        SearchSpan = StatementSpan,
                        TableCanBeDefinedAfter = DeleteUpdatePrefromSpan != null && DeleteUpdatePrefromSpan.Contains(new ScriptSpan(tableReference))
                    });
                }

                // if it is "DELETE t FROM SomeTable t JOIN ..." then this (the first) t is not a new source
                // and the second will pass (it comesa after the prefrom phase)
                if (DeleteUpdatePrefromSpan == null || !DeleteUpdatePrefromSpan.Contains(new ScriptSpan(newTableSource.ObjectContent)))
                {
                    _localIndex.AddLocalTableSource(newTableSource);
                }
            }
            else
            {
                // only an alias - needs to be resolved within the query
                _referenceResolver.AddUnresolvedReference(new UnresolvedReference()
                {
                    ScriptReferenceType = ScriptReferenceTypeEnum.Table,
                    Identifier = tableReference.Alias,
                    Position = new ScriptSpan(tableReference.Alias),
                    IsDbObject = false,
                    ReferenceFromObject = tableReference,
                    SearchSpan = StatementSpan,
                    TableCanBeDefinedAfter = DeleteUpdatePrefromSpan != null && DeleteUpdatePrefromSpan.Contains(new ScriptSpan(tableReference))
                });
            }

            _fragmentsOfInterest.Add(tableReference);
        }

        public override void Visit(VariableTableReference variableTableReference)
        {


            //_referenceResolver.AddUnresolvedReference(new UnresolvedReference()
            //{
            //    ScriptReferenceType = ScriptReferenceTypeEnum.Table,
            //    Identifier = new Identifier() { Value = variableTableReference.Variable.Name },
            //    Position = new ScriptSpan(variableTableReference),
            //    IsDbObject = false,
            //    ReferenceFromObject = variableTableReference,
            //    SearchSpan = BatchSpan,
            //    TableCanBeDefinedAfter = false
            //});

            var candidates = _localIndex.GetCandidateTables(new ScriptSpan(variableTableReference));
            var variableIdentifier = new Identifier() { Value = variableTableReference.Variable.Name };
            var tableVariable = _localIndex.FindTableByIdentifier(candidates, variableIdentifier);

            if (tableVariable == null) // can happen in the return declaration of table-valued functions
            {
                return;
            }

            _referenceResolver.AddResolvedReference(new ResolvedReference()
            {
                FromFragment = variableTableReference,
                ReferenceFragment = variableTableReference,
                ScriptReferenceType = ScriptReferenceTypeEnum.Table,
                ToFragment = tableVariable.ObjectContent,
                
            });

            var newTableSource = new TableSourceColumnList()
            {
                Columns = ((tableVariable == null) ? null : (tableVariable.Columns)),
                ValidSpan = QuerySpan ?? StatementSpan,
                Identifier = ((TSqlFragment)variableTableReference.Alias) ?? variableIdentifier,
                ObjectContent = variableTableReference,
                ReferenceType = ScriptReferenceTypeEnum.Table
            };
            
            tableVariable.ReferencedByAliases.Add(newTableSource);
            newTableSource.ReferencesTable = tableVariable;

            if (tableVariable.Columns.Count > 0)
            {
                newTableSource.Columns = tableVariable.Columns;
                //newTableSource.PropagateResolvedColumns();
            }

            if (DeleteUpdatePrefromSpan == null || !DeleteUpdatePrefromSpan.Contains(new ScriptSpan(newTableSource.ObjectContent)))
            {
                _localIndex.AddLocalTableSource(newTableSource);
            }

            _fragmentsOfInterest.Add(variableTableReference);
        }

        /// <summary>
        /// This is a new reference to be resolved; if it is in a FROM clause, the resolution is attempted immediately, because a column that is unique at the time when first two tables are joined may not be unique when a third table joins in
        /// </summary>
        /// <param name="columnReference"></param>
        public override void Visit(ColumnReferenceExpression columnReference)
        {
            // e.g. ... ORDER BY COUNT(*) ...
            if (columnReference.MultiPartIdentifier == null)
            {
                return;
            }

            var newReference = new UnresolvedReference()
            {
                ScriptReferenceType = ScriptReferenceTypeEnum.Column,
                Identifier = columnReference.MultiPartIdentifier,
                Position = new ScriptSpan(columnReference.MultiPartIdentifier),
                IsDbObject = columnReference.MultiPartIdentifier is SchemaObjectName,
                ReferenceFromObject = columnReference,
                SearchSpan = (QuerySpan == null || !QuerySpan.Contains(new ScriptSpan(columnReference.MultiPartIdentifier))) ? StatementSpan : QuerySpan
            };
            _referenceResolver.AddUnresolvedReference(newReference);


            if (QueryFromClauseSpan != null && columnReference.MultiPartIdentifier.Identifiers.Count == 1 && QueryFromClauseSpan.Contains(newReference.Position))
            {
                var resolved = _referenceResolver.TryResolveReference(newReference, false, this);
            }
            
            _fragmentsOfInterest.Add(columnReference);
        }
        
        public override void Visit(SchemaObjectFunctionTableReference functionReference)
        {
            var identifier = functionReference.SchemaObject;
            var tableUdf = _index.FindTableSource(identifier, _dbInUse);

            _fragmentsOfInterest.Add(functionReference);

            ReferrableObject resolution = tableUdf;

            if (resolution == null)
            {
                // this can happen with calls to system functions (sys schema)
                return;
                //throw new Exception();
            }
            
            var rr = new ResolvedReference()
            {
                FromFragment = functionReference,
                ToFragment = resolution.ObjectContent ?? resolution.Identifier,
                ReferenceFragment = identifier,
                ToUrn = resolution.Urn,
                ToObject = resolution.ModelElement,
                ScriptReferenceType = ScriptReferenceTypeEnum.Function
            };
                var newTableSource = new TableSourceColumnList()
                {
                    Columns = tableUdf.Columns,
                    ValidSpan = QuerySpan ?? StatementSpan,
                    Identifier = (TSqlFragment)(functionReference.Alias) ?? functionReference.SchemaObject,
                    
                    ObjectContent = functionReference,
                    ReferenceType = ScriptReferenceTypeEnum.Table
                };

                tableUdf.ReferencedByAliases.Add(newTableSource);
            newTableSource.ReferencesTable = tableUdf;
            _referenceResolver.AddResolvedReference(rr);
        }

        /// <summary>
        /// Scalar UDF or system functions - resolve only UDFs.
        /// </summary>
        /// <param name="functionCall"></param>
        public override void Visit(FunctionCall functionCall)
        {
            // added in primary expression
            //_fragmentsOfInterest.Add(functionCall);

            var functionName = functionCall.FunctionName;
            if (!(functionCall.CallTarget is MultiPartIdentifierCallTarget))
            {
                return;
            }
            var targetName = (functionCall.CallTarget as MultiPartIdentifierCallTarget).MultiPartIdentifier;
            // something, but not schema name
            if (targetName.Identifiers.Count != 1)
            {
                return;
            }
            
            var schemaName = targetName.Identifiers[0];
            var mpi = new MultiPartIdentifier();
            mpi.Identifiers.Add(schemaName);
            mpi.Identifiers.Add(functionName);
            mpi.FirstTokenIndex = schemaName.FirstTokenIndex;
            mpi.LastTokenIndex = functionName.LastTokenIndex;
            mpi.ScriptTokenStream = functionCall.ScriptTokenStream;

            var scalarUdf = _index.FindScalarUdf(mpi, _dbInUse);
            if (scalarUdf == null)
            {
                return;
            }
            var rr = new ResolvedReference()
            {
                FromFragment = functionCall,
                ToFragment = scalarUdf.ObjectContent ?? scalarUdf.Identifier,
                ReferenceFragment = mpi,
                ToUrn = scalarUdf.Urn,
                ToObject = scalarUdf.ModelElement,
                ScriptReferenceType = ScriptReferenceTypeEnum.Function
            };
            _referenceResolver.AddResolvedReference(rr);
        }

        public override void Visit(CreateProcedureStatement createProcedureStatement)
        {
            CreateProcedureReferenceSpan = new ScriptSpan(createProcedureStatement.ProcedureReference);
        }

        public override void Visit(ProcedureReference procedureReference)
        {
            var procedure = _index.FindStoredProcedure(procedureReference.Name, _dbInUse);
            // sys objects
            if (procedure == null || (CreateProcedureReferenceSpan != null && CreateProcedureReferenceSpan.Contains(new ScriptSpan(procedureReference))))
            {
                return;
            }
            var rr = new ResolvedReference()
            {
                FromFragment = procedureReference.Name,
                ToFragment = procedure.ObjectContent ?? procedure.Identifier,
                ReferenceFragment = procedureReference.Name,
                ToUrn = procedure.Urn,
                ToObject = procedure.ModelElement,
                ScriptReferenceType = ScriptReferenceTypeEnum.StoredProcedure
            };
            _fragmentsOfInterest.Add(rr.FromFragment);
            _referenceResolver.AddResolvedReference(rr);
        }

        public override void Visit(ExecuteStatement executeStatement)
        {
            if (executeStatement.ExecuteSpecification != null)
            {
                if (executeStatement.ExecuteSpecification.ExecutableEntity is ExecutableProcedureReference)
                {

                    FirstProcedureCallResultSet = null;
                    var resultSetsOption = executeStatement.Options.FirstOrDefault(x => x is ResultSetsExecuteOption) as ResultSetsExecuteOption;
                    if (resultSetsOption != null)
                    {
                        if (resultSetsOption.ResultSetsOptionKind == ResultSetsOptionKind.ResultSetsDefined)
                        {
                            var resultSetDef = resultSetsOption.Definitions.First();
                            FirstProcedureCallResultSet = new TableSourceColumnList()
                            {
                                Columns = new List<ReferrableObject>()
                            };

                            if (resultSetDef is InlineResultSetDefinition)
                            {
                                foreach (var columnDef in (resultSetDef as InlineResultSetDefinition).ResultColumnDefinitions)
                                {
                                    _fragmentsOfInterest.Add(columnDef);
                                    FirstProcedureCallResultSet.Columns.Add(new ReferrableObject()
                                    {
                                        Identifier = columnDef.ColumnDefinition.ColumnIdentifier,
                                        ObjectContent = columnDef,
                                        ReferenceType = ScriptReferenceTypeEnum.Column,
                                        ValidSpan = StatementSpan
                                    });
                                }
                            }
                            //foreach(var column in resultSetDef.)
                        }
                    }

                    var epr = executeStatement.ExecuteSpecification.ExecutableEntity as ExecutableProcedureReference;
                    if (epr.ProcedureReference != null)
                    {
                        FirstTopLevelTableSource = null;
                        var procedureReference = epr.ProcedureReference.ProcedureReference;
                        var procedure = _index.FindStoredProcedure(procedureReference.Name, _dbInUse);
                        if (FirstProcedureCall == null)
                        {
                            FirstProcedureCall = procedure;
                            
                        }
                    }
                }
            }
        }

        public override void Visit(DeclareVariableElement declareVarElement)
        {
            // is table type variable
            if (declareVarElement.DataType is UserDataTypeReference)
            {
                var udtReference = (UserDataTypeReference)declareVarElement.DataType;
                var tableTT = _index.FindTableType(udtReference.Name, _dbInUse);
                if (tableTT != null)
                {
                    var tableSource = new TableSourceColumnList()
                    {
                        Columns = new List<ReferrableObject>(),
                        ObjectContent = declareVarElement,
                        SelectStars = new List<SelectStarExpression>(),
                        ValidSpan = BatchSpan,
                        ReferenceType = ScriptReferenceTypeEnum.TableType,
                        Identifier = declareVarElement.VariableName
                    };

                    foreach (var column in tableTT.Columns)
                    {
                        tableSource.Columns.Add(new ReferrableObject()
                        {
                            Identifier = column.Identifier,
                            ObjectContent = column.ObjectContent,
                            ReferenceType = ScriptReferenceTypeEnum.Column,
                            ValidSpan = BatchSpan,
                            ModelElement = column.ModelElement
                        });
                        
                    }
                    var rr = new ResolvedReference()
                    {
                        FromFragment = udtReference,
                        ToFragment = tableTT.Identifier,
                        ReferenceFragment = udtReference,
                        ToUrn = tableTT.ModelElement.RefPath,
                        ToObject = tableTT.ModelElement,
                        ScriptReferenceType = ScriptReferenceTypeEnum.TableType
                    };
                    _referenceResolver.AddResolvedReference(rr);
                    
                    _localIndex.AddLocalTableSource(tableSource);
                    _fragmentsOfInterest.Add(declareVarElement);
                    _fragmentsOfInterest.Add((UserDataTypeReference)declareVarElement.DataType);

                    return;
                }
            }
                _localIndex.AddLocalVariable(new ReferrableObject()
                {
                    Identifier = declareVarElement.VariableName,
                    ObjectContent = declareVarElement,
                    ValidSpan = BatchSpan,
                    Urn = new RefPath(declareVarElement.VariableName.GetText()),
                    ReferenceType = ScriptReferenceTypeEnum.Variable
                });
            
            _fragmentsOfInterest.Add(declareVarElement);
        }
        
        public override void Visit(DeclareTableVariableStatement declareTableVariable)
        {
            var tableSource = new TableSourceColumnList()
            {
                Columns = new List<ReferrableObject>(),
                ObjectContent = declareTableVariable,
                SelectStars = new List<SelectStarExpression>(),
                ValidSpan = BatchSpan,
                ReferenceType = ScriptReferenceTypeEnum.TableType,
                Identifier = declareTableVariable.Body.VariableName
            };

            foreach (var column in declareTableVariable.Body.Definition.ColumnDefinitions)
            {
                tableSource.Columns.Add(new ReferrableObject()
                {
                    Identifier = column.ColumnIdentifier,
                    ObjectContent = column,
                    ReferenceType = ScriptReferenceTypeEnum.Column,
                    ValidSpan = BatchSpan
                });
                _fragmentsOfInterest.Add(column);
            }

            _localIndex.AddLocalTableSource(tableSource);
            _fragmentsOfInterest.Add(declareTableVariable);
        }

        public override void Visit(VariableReference varRef)
        {
            var refVar = _localIndex.FindVariable(new ScriptSpan(varRef), varRef.Name);
            if (refVar == null)
            {
                //TODO: solve SP / F param declarations later
                return;
            }
            _referenceResolver.AddResolvedReference(new ResolvedReference()
            {
                FromFragment = varRef,
                ReferenceFragment = varRef,
                ScriptReferenceType = ScriptReferenceTypeEnum.Variable,
                ToFragment = refVar.ObjectContent,
                ToUrn = refVar.Urn,
                ToObject = refVar.ModelElement
            });
            _fragmentsOfInterest.Add(varRef);
        }
        
        public override void Visit(FromClause fromClause)
        {
            _fragmentsOfInterest.Add(fromClause);
            foreach (var table in fromClause.TableReferences)
            {
                if (table is QualifiedJoin)
                {
                    var qualifiedJoin = table as QualifiedJoin;
                    // to capture the search condition in a separate node
                    _fragmentsOfInterest.Add(qualifiedJoin);
                }

                // non-join table references (SELECT * FROM dbo.x) are solved in Visit(NamedTableReference)
            }
        }

        public override void Visit(WhereClause whereClause)
        {
            _fragmentsOfInterest.Add(whereClause);
        }

        public override void Visit(GroupByClause groupByClause)
        {
            _fragmentsOfInterest.Add(groupByClause);
        }

        public override void Visit(HavingClause havingClause)
        {
            _fragmentsOfInterest.Add(havingClause);
        }

        public override void Visit(PrimaryExpression primaryExpression)
        {
            _fragmentsOfInterest.Add(primaryExpression);
        }
        
        public override void Visit(UseStatement useStatement)
        {
            _localIndex.DbInUse = useStatement.DatabaseName;
            _dbInUse = useStatement.DatabaseName;
        }

        public override void Visit(BooleanExpression booleanExp)
        {
            BooleanExpressionSpan = new ScriptSpan(booleanExp);
        }

        public override void Visit(PivotedTableReference pivotedTableReference)
        {
            var coveringtableSource = _localIndex.GetTableSourceBeingDefined(NestedQuerySpans.Count - 1, new ScriptSpan(pivotedTableReference));

            if (coveringtableSource == null)
            {
                return;
            }

            var firstValueColumn = pivotedTableReference.ValueColumns.FirstOrDefault();
            if (firstValueColumn == null)
            {
                return;
            }


            var valueColumnReferrable = new ReferrableObject()
            {
                Identifier = firstValueColumn.MultiPartIdentifier,
                ObjectContent = firstValueColumn,
                ReferenceType = ScriptReferenceTypeEnum.Column,
                ValidSpan = StatementSpan
            };

            _fragmentsOfInterest.Add(firstValueColumn);

            foreach (var inColumn in pivotedTableReference.InColumns)
            {
                var inColumnReference = new ReferrableObject()
                {
                    Identifier = inColumn,
                    ObjectContent = inColumn,
                    ReferenceType = ScriptReferenceTypeEnum.Column,
                    ValidSpan = StatementSpan
                };
                _fragmentsOfInterest.Add(inColumn);
                coveringtableSource.Columns.Add(inColumnReference);

                var rr = new ResolvedReference()
                {
                    FromFragment = inColumn,
                    ToFragment = firstValueColumn,
                    ReferenceFragment = inColumn,
                    ScriptReferenceType = ScriptReferenceTypeEnum.Column
                };
                _referenceResolver.AddResolvedReference(rr);
            }

        }
    }
}
