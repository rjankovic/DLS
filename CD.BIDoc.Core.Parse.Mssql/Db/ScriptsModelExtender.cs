
using CD.DLS.DAL.Configuration;
using CD.DLS.Model.Mssql.Db;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Parse.Mssql.Db
{
    /// <summary>
    /// Extends the database model by parsing script definitions.
    /// </summary>
    class ScriptsModelExtender
    {
        /// <summary>
        /// Holds a model element and its corresponding definition statement.
        /// </summary>
        private struct ElementStatement {
            internal TSqlStatement statement;
            internal DbScriptedElement element;

            public ElementStatement(TSqlStatement statement, DbScriptedElement element)
            {
                this.statement = statement;
                this.element = element;
            }
        }


        private readonly ISqlScriptModelParser _scriptModelExtractor;
        private readonly List<ElementStatement> _elementStatements = new List<ElementStatement>();

        public ScriptsModelExtender(ISqlScriptModelParser scriptModelExtractor)
        {
            this._scriptModelExtractor = scriptModelExtractor;
        }

        public void Add(TSqlStatement statement, DbScriptedElement element)
        {
            _elementStatements.Add(new ElementStatement(statement, element));
        }

        public void ParseStatementsModel(IReferrableIndex referrableIndex)
        {
            foreach (var nodeStatement in _elementStatements)
            {
                var statement = nodeStatement.statement;
                var element = nodeStatement.element;

                var dbNode = element.Parent.Parent as DatabaseElement;

                if (dbNode.Parent != null)
                {
                    referrableIndex.SetContextServer(((ServerElement)dbNode.Parent).Caption);
                    //referrableIndex.ContextServerName = ((ServerElement)dbNode.Parent).Caption;
                    _scriptModelExtractor.ContextServerName = ((ServerElement)dbNode.Parent).Caption;
                }
                Dictionary<string, Model.Mssql.MssqlModelElement> outputColumns;
                List<Tuple<string, Model.Mssql.MssqlModelElement>> outputColumnsOrdinal;
                Dictionary<string, TableSourceColumnList> createdTempTables;
                var graph = _scriptModelExtractor.ExtractScriptModel(statement, element, referrableIndex, new Identifier() { Value = dbNode.DbName }, out outputColumns, out outputColumnsOrdinal, out createdTempTables);
                element.AddChild(graph);
                //if (element.RefPath.Path == "Server[@Name='RJ-THINK']/Database[@Name='ManpowerDWH']/View[@Name='vw_OGMNewBusiness' and @Schema='dbo']")
                //{
                //    var viewSelect = ((CreateViewStatement)statement).SelectStatement;

                //    Dictionary<string, Model.Mssql.MssqlModelElement> innerOutputColumns;
                //    var innerGraph = _scriptModelExtractor.ExtractScriptModel(viewSelect, element, referrableIndex, new Identifier() { Value = dbNode.DbName }, out innerOutputColumns);

                //    var x = 0;
                //}
                if (element is ViewElement)
                {
                    var viewElem = element as ViewElement;
                    foreach (var elemColumn in viewElem.Columns)
                    {
                        if (outputColumns == null)
                        {
                            continue;
                        }
                        if (elemColumn == null)
                        {
                            continue;
                        }
                        if (elemColumn.Caption == null)
                        {
                            continue;
                        }
                        if (!outputColumns.ContainsKey(elemColumn.Caption))
                        {
                            //throw new Exception();
                            // SELECT * FROM (VALUES (A,B,C) T(C1,C2,C3) ...
                            ConfigManager.Log.Warning(string.Format("View Column Not Found: {0}, columns: {1}", elemColumn.RefPath.Path, string.Join(", ", outputColumns.Keys)));
                            continue;
                        }
                        var outputColumn = outputColumns[elemColumn.Caption];
                        elemColumn.Reference = outputColumn;
                        //ConfigManager.Log.Info(string.Format("View Column Reference: {0} -> {1}", elemColumn.RefPath.Path, outputColumn.RefPath.Path));
                    }
                }
                if (element is ProcedureElement)
                {
                    var spElem = (ProcedureElement)element;
                    //spElem.OutputColumns = outputColumns;
                    //spElem.OutputColumnsOrdinal = outputColumnsOrdinal;
                }

                // TODO: definitions
            }
        }
    }
}
