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

namespace CD.DLS.Parse.Mssql.Db
{
    using PendingForeignKeys = List<Tuple<SqlForeignKey, ForeignKeyElement>>;

    public abstract class DbModelParserBase
    {
        public static RefPath RefPathFor(string urn)
        {

            //Server[@Name='LAPTOP-BRLOS479']/Database[@Name='ManpowerDWH']/Table[@Name='DimEmployeeBranch' and @Schema='dbo']
            //Server[@Name='LAPTOP-BRLOS479']/Database[@Name='ManpowerDWH']/Schema[@Name='dbo']

            var tabIdx = urn.IndexOf("/Table[");
            var vieIdx = urn.IndexOf("/View[");

            string refPath = null;
            if (tabIdx > 0 || vieIdx > 0)
            {
                string typeName;
                var typIdx = urn.IndexOf("");
                if (tabIdx > 0)
                {
                    typeName = "/Table[";
                    typIdx = tabIdx;
                }
                else
                {
                    typeName = "/View[";
                    typIdx = vieIdx;
                }
                var tableCutLeft = urn.Substring(typIdx);
                var tableCutRight = tableCutLeft.Substring(0 + typeName.Length, tableCutLeft.IndexOf("']") + 1 - typeName.Length);
                var split = tableCutRight.Split(new string[] { " and " }, StringSplitOptions.None);
                var tablePart = split.First(x => x.StartsWith("@Name"));
                var schemaPart = split.First(x => x.StartsWith("@Schema"));
                var tableName = tablePart.Substring(tablePart.IndexOf('\'')).Trim('\'');
                var schemaName = schemaPart.Substring(schemaPart.IndexOf('\'')).Trim('\'');

                var tabIdxEnd = urn.IndexOf("']", typIdx) + 2;
                var prefix = urn.Substring(0, typIdx);
                var suffix = string.Empty;
                if (urn.Length >= tabIdxEnd)
                {
                    suffix = urn.Substring(tabIdxEnd);
                }

                refPath = string.Format("{0}/Schema[@Name='{1}']"+ typeName +"@Name='{2}']{3}", prefix, schemaName, tableName, suffix);
            }
            else
            {
                refPath = urn;
            }

            return new RefPath(refPath);
            //return new RefPath(obj.Urn.Value);
        }

    }

    /// <summary>
    /// Extracts the full database model.
    /// </summary>
    /// <remarks>
    /// Gets scripts from the server, uses declaration extractor to extend the model.
    /// </remarks>
    public class DeepModelParser : DbModelParserBase
    {
        #region Private state
        private readonly TSqlParser _parser;
        private readonly ScriptsModelExtender _extender;
        private readonly StageManager _stageManager;
        private readonly ProjectConfig _projectConfig;
        private readonly Guid _extractId;
        #endregion

        public DeepModelParser(ISqlScriptModelParser scriptModelExtractor, ProjectConfig projectConfig, Guid extractId, StageManager stageManager)
        {
            this._parser = scriptModelExtractor.Parser;
            _extender = new ScriptsModelExtender(scriptModelExtractor);
            _stageManager = stageManager;
            _projectConfig = projectConfig;
            _stageManager = stageManager;
            _extractId = extractId;
        }
        
        /// <summary>
        /// Extracts SQL definitions into the specified model.
        /// </summary>
        //public void ExtractModel(List<ServerElement> serverElements)
        //{
        //    ExtractModel(conn, serverElements);
        //}
        

        public void ExtractModel(List<ServerElement> serverElements)
        {
            //Server smoServer = new Server(conn);

            foreach (var serverElement in serverElements)
            {

                DeclarationExtractor declarationExtractor = new DeclarationExtractor(_parser, _extender, serverElement.Caption);
                PendingForeignKeys pendingForeignKeys = new PendingForeignKeys();

                foreach (var dbConfig in _projectConfig.DatabaseComponents.Where(x => Common.Tools.ConnectionStringTools.AreServersNamesEqual(x.ServerName, serverElement.Caption)))// .DatabaseFilter.EnumerateDatabases(serverElements.Databases)
                {
                    
                    var dbElement = serverElement.DatabaseByCaption(dbConfig.DbName);

                    ConfigManager.Log.Important(string.Format("Extracting SQL DB {0}", dbElement.DbName));

                    //var dbExtract = (SqlDbStructure)(_stageManager.GetExtractItems(_extractId, dbConfig.MssqlDbProjectComponentId, DAL.Objects.Extract.ExtractTypeEnum.SqlDbStructure)[0]);

                    //ExtractForeignKeyNodes(dbExtract, dbElement, pendingForeignKeys);
                    ExtractDatabaseScripts(dbElement, declarationExtractor, dbConfig);
                }

                ResolveForeignKeys(pendingForeignKeys);
            }

            ReferrableIndexBuilder referrableIndexBuilder = new ReferrableIndexBuilder();

            var referrableIndex = referrableIndexBuilder.BuildIndex(serverElements);

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
        
        private void ExtractDatabaseScripts(DatabaseElement dbNode, DeclarationExtractor declarationExtractor, MssqlDbProjectComponent dbComponent)
        {

            throw new NotImplementedException();

            //StringCollection scripts = new StringCollection();
            //var scriptExtracts = _stageManager.GetExtractItems(_extractId, dbComponent.MssqlDbProjectComponentId, ExtractTypeEnum.SqlDbScript).Select(x => (SqlDbScript)x).OrderBy(x => x.Name).ToList();
            //foreach (var scriptExtract in scriptExtracts)
            //{
            //    scripts.Add(scriptExtract.Content);
            //}

            //foreach (var script in scripts)
            //{
            //    //if (script.Contains("sp_renamediagram"))
            //    //{
            //    //}
            //    declarationExtractor.ExtractDatabaseScriptDeclaration(dbNode, script);
            //}
        }

        private void ExtractDatabaseScripts(DbModelElement dbObjectElement, DeclarationExtractor declarationExtractor, SmoObject objectExtract)
        {

            StringCollection scripts = new StringCollection();
            foreach (var scriptExtract in objectExtract.DefinitionScripts)
            {
                scripts.Add(scriptExtract);
            }

            foreach (var script in scripts)
            {
                //if (script.Contains("sp_renamediagram"))
                //{
                //}
                declarationExtractor.ExtractDatabaseScriptDeclaration(dbObjectElement, script);
            }
        }

        /// <summary>
        /// Resolves source and target columns for foreign keys.
        /// </summary>
        /// <param name="foreignKeys">Collection of pending foreign keys.</param>
        private void ResolveForeignKeys(PendingForeignKeys foreignKeys)
        {
                          
            foreach (var fkTuple in foreignKeys)
            {
                ForeignKeyElement fkElement = fkTuple.Item2;
                SqlForeignKey smoFk = fkTuple.Item1;

                var tableElement = (SchemaTableElement)fkElement.Parent;
                var dbElement = (DatabaseElement)tableElement.Parent.Parent;

                var targetTable = dbElement.TableBySchemaName(smoFk.ReferencedTableSchema, smoFk.ReferencedTable);
                if (targetTable != null)
                {
                    foreach (var fkCol in smoFk.Columns)
                    {
                        var targetColumnNode = targetTable.GetColumnByName(fkCol.ReferencedColumnName);
                        var sourceColumnNode = tableElement.GetColumnByName(fkCol.ColumnName);
                        fkElement.SourceColumn = sourceColumnNode;
                        fkElement.TargetColumn = targetColumnNode;
                    }
                }
                else
                {
                    ConfigManager.Log.Warning("Could not find table {0}.{1} referenced by foreign key {2}",
                        smoFk.ReferencedTableSchema, smoFk.ReferencedTable, smoFk.ObjectName);
                }

            }
        }

    }
}
