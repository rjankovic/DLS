using System;
using System.Collections.Generic;
using System.Linq;
using CD.DLS.Model.Mssql.Db;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Specialized;
using CD.DLS.DAL.Managers;
using CD.DLS.Model.Interfaces;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Mssql;

namespace CD.DLS.Parse.Mssql.Db
{
    

    /// <summary>
    /// Extracts the full database model.
    /// </summary>
    /// <remarks>
    /// Gets scripts from the server, uses declaration extractor to extend the model.
    /// </remarks>
    public class LocalDeepModelParser : DbModelParserBase
    {
        #region Private state
        private readonly TSqlParser _parser;
        private readonly ScriptsModelExtender _extender;
        private readonly ProjectConfig _projectConfig;
        //private SmoObject _extractObject;
        #endregion

        public LocalDeepModelParser(ProjectConfig projectConfig, string contextServerName)
        {

            ScriptModelParser sqlExtractor = new ScriptModelParser();
            sqlExtractor.ContextServerName = contextServerName;
            this._parser = sqlExtractor.Parser;
            _extender = new ScriptsModelExtender(sqlExtractor);
            _projectConfig = projectConfig;
        }
        
        /// <summary>
        /// Extracts SQL definitions into the specified model.
        /// </summary>
        //public void ExtractModel(List<ServerElement> serverElements)
        //{
        //    ExtractModel(conn, serverElements);
        //}
        

        public void ExtractModel(SmoObject extractObject, string serverName, IReferrableIndex referrableIndex, MssqlModelElement modelElement)
        {
            //Server smoServer = new Server(conn);
            
                DeclarationExtractor declarationExtractor = new DeclarationExtractor(_parser, _extender, serverName);

                    ExtractDatabaseObjectScripts((DbModelElement)modelElement, declarationExtractor, extractObject);
            
                //ResolveForeignKeys(pendingForeignKeys);
            //}

            //ReferrableIndexBuilder referrableIndexBuilder = new ReferrableIndexBuilder();

            //var referrableIndex = referrableIndexBuilder.BuildIndex(serverElements);

            _extender.ParseStatementsModel(referrableIndex);
            
        }

        //private void ExtractForeignKeyNodes(SqlDbStructure db, DatabaseElement dbNode, PendingForeignKeys pendingForeignKeys)
        //{
        //    foreach (var schema in db.Schemas)
        //    {
        //        foreach (var table in schema.Tables)
        //        {
        //            SchemaTableElement tableNode = dbNode.SchemaByName(schema.Name).TableByName(table.Name);

        //            foreach (var fk in table.ForeignKeys)
        //            {

        //                var fkNode = new ForeignKeyElement(RefPathFor(fk.Urn), fk.Name, null, tableNode);
        //                tableNode.AddForeignKey(fkNode);

        //                pendingForeignKeys.Add(new Tuple<SqlForeignKey, ForeignKeyElement>(fk, fkNode));
        //            }
        //        }
        //    }
        //}
        
        //private void ExtractDatabaseScripts(DatabaseElement dbNode, DeclarationExtractor declarationExtractor, MssqlDbProjectComponent dbComponent)
        //{

        //    StringCollection scripts = new StringCollection();
        //    var scriptExtracts = _stageManager.GetExtractItems(_extractId, dbComponent.MssqlDbProjectComponentId, ExtractTypeEnum.SqlDbScript).Select(x => (SqlDbScript)x).OrderBy(x => x.Name).ToList();
        //    foreach (var scriptExtract in scriptExtracts)
        //    {
        //        scripts.Add(scriptExtract.Content);
        //    }

        //    foreach (var script in scripts)
        //    {
        //        //if (script.Contains("sp_renamediagram"))
        //        //{
        //        //}
        //        declarationExtractor.ExtractDatabaseScriptDeclaration(dbNode, script);
        //    }
        //}

        private void ExtractDatabaseObjectScripts(DbModelElement dbObjectElement, DeclarationExtractor declarationExtractor, SmoObject objectExtract)
        {

            StringCollection scripts = new StringCollection();
            foreach (var scriptExtract in objectExtract.DefinitionScripts)
            {
                // TODO OPTIMIZE_FOR_SEQUENTIAL_KEY
                var scriptExtractM = scriptExtract.Replace(", OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF", "").Replace(", OPTIMIZE_FOR_SEQUENTIAL_KEY = ON", "");
                scripts.Add(scriptExtractM);
            }

            foreach (var script in scripts)
            {
                //if (script.Contains("sp_renamediagram"))
                //{
                //}
                declarationExtractor.ExtractDatabaseScriptDeclaration(dbObjectElement, script);
            }
        }

        ///// <summary>
        ///// Resolves source and target columns for foreign keys.
        ///// </summary>
        ///// <param name="foreignKeys">Collection of pending foreign keys.</param>
        //private void ResolveForeignKeys(PendingForeignKeys foreignKeys)
        //{
                          
        //    foreach (var fkTuple in foreignKeys)
        //    {
        //        ForeignKeyElement fkElement = fkTuple.Item2;
        //        SqlForeignKey smoFk = fkTuple.Item1;

        //        var tableElement = (SchemaTableElement)fkElement.Parent;
        //        var dbElement = (DatabaseElement)tableElement.Parent.Parent;

        //        var targetTable = dbElement.TableBySchemaName(smoFk.ReferencedTableSchema, smoFk.ReferencedTable);
        //        if (targetTable != null)
        //        {
        //            foreach (var fkCol in smoFk.Columns)
        //            {
        //                var targetColumnNode = targetTable.GetColumnByName(fkCol.ReferencedColumnName);
        //                var sourceColumnNode = tableElement.GetColumnByName(fkCol.ColumnName);
        //                fkElement.SourceColumn = sourceColumnNode;
        //                fkElement.TargetColumn = targetColumnNode;
        //            }
        //        }
        //        else
        //        {
        //            ConfigManager.Log.Warning("Could not find table {0}.{1} referenced by foreign key {2}",
        //                smoFk.ReferencedTableSchema, smoFk.ReferencedTable, smoFk.ObjectName);
        //        }

        //    }
        //}

    }
}
