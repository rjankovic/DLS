using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CD.DLS.Parse.Mssql.Db
{
    class SqlLocalIndex
    {
        private readonly List<TableSourceColumnList> _localTableSourcesAvailable = new List<TableSourceColumnList>();
        private readonly Dictionary<int, List<TableSourceColumnList>> _tableSourcesBeingDefined = new Dictionary<int, List<TableSourceColumnList>>();
        private readonly List<TableSourceColumnList> _selectIntoBeingDefined = new List<TableSourceColumnList>();
        private readonly List<TableSourceColumnList> _insertTableSources = new List<TableSourceColumnList>();
        //private readonly List<TableSourceColumnList> _tablesCreatedInTheBatch = new List<TableSourceColumnList>();
        private readonly List<TableReference> _insertTableTargets = new List<TableReference>();
        private readonly List<IList<ColumnReferenceExpression>> _insertTargetColumnLists = new List<IList<ColumnReferenceExpression>>();
        private readonly List<ReferrableObject> _variablesAvailable = new List<ReferrableObject>();
        private readonly IReferrableIndex _globalIndex;
        private Identifier _dbInUse;

        public Identifier DbInUse
        {
            get
            {
                return _dbInUse;
            }
            set
            {
                _dbInUse = value;
            }
        }
        


        public ReferrableObject FindVariable(ScriptSpan span, string name)
        {
            var identifierComparer = new IdentifierComparer(_dbInUse);
            return _variablesAvailable.Where(x => x.ValidSpan.Contains(span) && identifierComparer.IdentifiersEqual(x.Identifier, new Identifier() { Value = name })).FirstOrDefault();
        }


        public List<TableSourceColumnList> GetCandidateTables(ScriptSpan span)
        {
            List<TableSourceColumnList> candidateTables = new List<TableSourceColumnList>(_localTableSourcesAvailable.Where(x => x.ValidSpan.Contains(span)));
            candidateTables.AddRange(_globalIndex.TableSourcesAvailable);
            return candidateTables;
        }

        public List<TableSourceColumnList> GetLocalCandidateTables(ScriptSpan span)
        {
            List<TableSourceColumnList> candidateTables = new List<TableSourceColumnList>();
            candidateTables.AddRange(_localTableSourcesAvailable.Where(x => x.ValidSpan.Contains(span)));
            return candidateTables;
        }


        public TableSourceColumnList FindTableSource(ScriptSpan span, SchemaObjectName name)
        {
            var candidateTables = GetCandidateTables(span);
            return FindTableByIdentifier(candidateTables, name);
        }

        public SqlLocalIndex(IReferrableIndex dbIndex, ScriptSpan localSpan)
        {
            this._globalIndex = dbIndex;
            //TableSourcesAvailable = new List<TableSourceColumnList>(dbIndex.TableSourcesAvailable);
            //TableSourcesBeingDefined = new List<TableSourceColumnList>();
            //VariablesAvailable = new List<ReferrableObject>();


            foreach (PresetVariable externalVar in dbIndex.PresetVariables)
            {
                _variablesAvailable.Add(new ReferrableObject()
                {
                    Identifier = new Identifier() { Value = externalVar.VariableName },
                    ObjectContent = new Identifier() { Value = externalVar.VariableName },
                    ValidSpan = localSpan,
                    Urn = externalVar.VariableObject.RefPath,
                    ModelElement = externalVar.VariableObject,
                    ReferenceType = ScriptReferenceTypeEnum.Variable
                });
            }

        }

        public ReferrableObject FindTableColumnByIdentifier(TableSourceColumnList p, Identifier identifier)
        {
            //TODO: local tables
            return _globalIndex.FindTableColumnByIdentifier(p, identifier, _dbInUse);
        }

        public void AddSelectIntoBeingDefined(TableSourceColumnList statement)
        {
            _selectIntoBeingDefined.Add(statement);
        }

        public TableSourceColumnList FindTableByIdentifier(List<TableSourceColumnList> tablesInRange, TSqlFragment tableIdentifier, int referencePosition = -1)
        {
            //TODO: local tables  
            TableSourceColumnList table = _globalIndex.FindTableByIdentifier(tablesInRange, tableIdentifier, _dbInUse, referencePosition);
         
            return table;
        }

        internal TableSourceColumnList GetTableSourceBeingDefined(int nestedLevel, ScriptSpan querySpan)
        {
            if (nestedLevel == 0)
            {
                var selectInto = _selectIntoBeingDefined.FirstOrDefault(x => new ScriptSpan(x.ObjectContent).Contains(querySpan));
                if (selectInto != null)
                {
                    return selectInto;
                }
            }

            if (!_tableSourcesBeingDefined.ContainsKey(nestedLevel))
            {
                return null;
            }
            return _tableSourcesBeingDefined[nestedLevel].FirstOrDefault(ts => new ScriptSpan(ts.ObjectContent).Contains(querySpan));
        }

        internal void AddTableSourceBeingDefined(int nestedLevel, TableSourceColumnList tableSource)
        {
            if (!_tableSourcesBeingDefined.ContainsKey(nestedLevel))
            {
                _tableSourcesBeingDefined.Add(nestedLevel, new List<TableSourceColumnList>());
            }
            _tableSourcesBeingDefined[nestedLevel].Add(tableSource);
        }

        internal void AddInsertTableSource(TableSourceColumnList tableSource)
        {
            _insertTableSources.Add(tableSource);
        }

        internal void AddInsertTargetColumnList(InsertSpecification specification)
        {
            _insertTargetColumnLists.Add(specification.Columns);
        }
        internal void AddInsertTargetTableReference(InsertSpecification specification)
        {
            _insertTableTargets.Add(specification.Target);
        }

        internal TableSourceColumnList GetInsertTableSource(ScriptSpan statementSpan)
        {
            return _insertTableSources.FirstOrDefault(ts => statementSpan.Contains(new ScriptSpan(ts.ObjectContent)));
        }

        internal IList<ColumnReferenceExpression> GetInsertTargetColumnList(ScriptSpan statementSpan)
        {
            return _insertTargetColumnLists.FirstOrDefault(x => x.Count > 0 && statementSpan.Contains(new ScriptSpan(x.First().FirstTokenIndex, x.Last().LastTokenIndex)));
        }
        internal TableReference GetInsertTargetTableReference(ScriptSpan statementSpan)
        {
            return _insertTableTargets.FirstOrDefault(x => statementSpan.Contains(new ScriptSpan(x)));
        }

        internal void AddLocalTableSource(TableSourceColumnList tableSource)
        {
            _localTableSourcesAvailable.Add(tableSource);
        }

        internal void AddLocalVariable(ReferrableObject referrableObject)
        {
            _variablesAvailable.Add(referrableObject);
        }

        internal void RemoveTableSourcesBeingDefined(int nestedLevel, TableSourceColumnList correspondingTableSource)
        {
            if (_tableSourcesBeingDefined.ContainsKey(nestedLevel) && _tableSourcesBeingDefined[nestedLevel].Contains(correspondingTableSource))
            {
                _tableSourcesBeingDefined[nestedLevel].Remove(correspondingTableSource);
            }
            else if (_selectIntoBeingDefined.Contains(correspondingTableSource) && nestedLevel == 0)
            {
                _selectIntoBeingDefined.Remove(correspondingTableSource);
            }
            else throw new Exception(string.Format("Table source {0} not found at level {1}", correspondingTableSource.ObjectContent.GetText(), nestedLevel));
        }

        internal TableSourceColumnList TableSourceByObjectContent(TSqlFragment referenceFromObject)
        {
            //var a = _globalIndex.TableSourcesAvailable.First().ObjectContent == referenceFromObject;
            var globalResolution = _globalIndex.TableSourcesAvailable.FirstOrDefault(ts => ts.ObjectContent == referenceFromObject);
            if (globalResolution != null)
            {
                return globalResolution;
            }
            if (referenceFromObject is NamedTableReference)
            {
                TableSourceColumnList localResolution = null;
                var idComparer = new IdentifierComparer(_dbInUse);
                var namedTableRef = (NamedTableReference)referenceFromObject;
                if (namedTableRef.SchemaObject != null)
                {
                    localResolution = _localTableSourcesAvailable.Where(x => new ScriptSpan(x.ObjectContent).Contains(new ScriptSpan(referenceFromObject))).OrderBy(x => x.ObjectContent.FirstTokenIndex).FirstOrDefault(x => idComparer.IdentifiersEqual(namedTableRef.SchemaObject, x.Identifier));
                }
                if (localResolution == null && namedTableRef.Alias != null)
                {
                    var localResolutionCandidates = _localTableSourcesAvailable.Where(x => x.ValidSpan.Contains(new ScriptSpan(referenceFromObject)))
                        .Where(x => idComparer.IdentifiersEqual(namedTableRef.Alias, x.Identifier));
                        //.OrderBy(x => x.ObjectContent.FirstTokenIndex)
                        //.FirstOrDefault();
                    var precedingCandidates = localResolutionCandidates.Where(c => c.ObjectContent.FirstTokenIndex <= referenceFromObject.FirstTokenIndex);
                    if (precedingCandidates.Any())
                    {
                        localResolution = precedingCandidates.OrderByDescending(x => x.ObjectContent.FirstTokenIndex).FirstOrDefault();
                    }
                    else
                    {
                        localResolution = localResolutionCandidates.OrderBy(x => x.ObjectContent.FirstTokenIndex).FirstOrDefault();
                    }
                }
                return localResolution;
            }
            if (referenceFromObject is SchemaObjectName)
            {
                TableSourceColumnList localResolution = null;
                var idComparer = new IdentifierComparer(_dbInUse);
                var schemaObject = (SchemaObjectName)referenceFromObject;
                localResolution = _localTableSourcesAvailable.Where(x => new ScriptSpan(x.ObjectContent).Contains(new ScriptSpan(referenceFromObject))).OrderBy(x => x.ObjectContent.FirstTokenIndex).FirstOrDefault(x => idComparer.IdentifiersEqual(schemaObject, x.Identifier));
                return localResolution;
            }
            return null;
        }

        // TODO: I dno't really work that well
        internal TableSourceColumnList TableSourceBeingDefinedByStarObject(TSqlFragment referenceFromObject)
        {
            return _tableSourcesBeingDefined.SelectMany(x => x.Value).FirstOrDefault(x => x.SelectStars.Contains(referenceFromObject));
        }
    }
}
