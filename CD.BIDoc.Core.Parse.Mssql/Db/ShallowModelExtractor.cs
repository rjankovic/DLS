using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Mssql.Db;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CD.DLS.Parse.Mssql.Db
{
    /// <summary>
    /// Extracts model from Mssql database. Just declarations, not including scripts.
    /// </summary>
    public class ShallowModelParser : DbModelParserBase
    {
        private HashSet<string> _coveredSchemas = new HashSet<string>();

        private ProjectConfig _projectConfig;
        private Guid _extractId;
        private StageManager _stageManager;
        
        public ShallowModelParser(ProjectConfig projectConfig, Guid extractId, StageManager stageManager)
        {
            _projectConfig = projectConfig;
            _extractId = extractId;
            _stageManager = stageManager;
        }

        /// <summary>
        /// Extracts the model according to the settings.
        /// </summary>
        public List<ServerElement> ParseModel()
        {
            throw new NotImplementedException();
            //return ParseServerDatabases();
        }

        ///// <summary>
        ///// Parses referrable model elements from the server.
        ///// </summary>
        ///// <remarks>
        ///// Parses tables, views, stored procedures and functions, and for each of them the columns and parameters. And foreign keys.
        ///// </remarks>
        ///// <returns>The element representing the server</returns>
        //private List<ServerElement> ParseServerDatabases()
        //{
        //    Dictionary<string, ServerElement> servers = new Dictionary<string, ServerElement>();
            



        //    foreach (var dbComponent in _projectConfig.DatabaseComponents)
        //    {
        //        ConfigManager.Log.Important(string.Format("Parsing MSSQL DB structures {0} from {1}", dbComponent.DbName, dbComponent.ServerName));

        //        var dbExtract = (SqlDbStructure)(_stageManager.GetExtractItems(_extractId, dbComponent.MssqlDbProjectComponentId, DAL.Objects.Extract.ExtractTypeEnum.SqlDbStructure)[0]);

        //        ServerElement serverNode;
        //        if (!servers.ContainsKey(dbComponent.ServerName))
        //        {
        //            servers[dbComponent.ServerName] = new ServerElement(RefPathFor(dbExtract.ServerUrn), dbExtract.ServerName);
        //        }
        //        serverNode = servers[dbComponent.ServerName];

                
        //        DatabaseElement dbObject = ExtractDatabaseElement(dbExtract, dbComponent.DbName, serverNode);
        //        serverNode.AddChild(dbObject);
        //    }
        //    return servers.Values.ToList();
        //}

        ///// <summary>
        ///// Extracts elements from a database.
        ///// </summary>
        //private DatabaseElement ExtractDatabaseElement(SqlDbStructure dbStructure, string dbName, ServerElement serverElement)
        //{
        //    var dbElement = new DatabaseElement(RefPathFor(dbStructure.DbUrn), dbStructure.Name, serverElement);

        //    ExtractDatabaseSchemas(dbElement, dbStructure);

        //    return dbElement;
        //}

        ///// <summary>
        ///// Extracts schemas and children from a database.
        ///// </summary> 
        //private void ExtractDatabaseSchemas(DatabaseElement dbElement, SqlDbStructure dbStructure)
        //{
        //    _coveredSchemas = new HashSet<string>();
        //    foreach (var dbSchema in dbStructure.Schemas)
        //    {
        //        var schemaElement = ExtractSchemaElement(dbSchema, dbElement);
        //        _coveredSchemas.Add(dbSchema.Name);
        //        dbElement.AddSchema(schemaElement);
                
        //        ExtractDatabaseTables(schemaElement, dbSchema);
        //        ExtractDatabaseViews(schemaElement, dbSchema);
        //        ExtractDatabaseUdfs(schemaElement, dbSchema);
        //        ExtractDatabaseStoredProcedures(schemaElement, dbSchema);
        //        ExtractDatabaseTableTypes(schemaElement, dbSchema);
        //    }
        //}

        ///// <summary>
        ///// Extracts the schema element from a database.
        ///// </summary>
        //private SchemaElement ExtractSchemaElement(SqlSchema schema, DatabaseElement dbNode)
        //{
        //    var schemaNode = new SchemaElement(RefPathFor(schema.Urn), schema.ObjectName, dbNode);
        //    return schemaNode;
        //}

        ///// <summary>
        ///// Extracts table elements from a database.
        ///// </summary>
        //private void ExtractDatabaseTables(SchemaElement schemaElement, SqlSchema schemaExtract)
        //{
        //    foreach (var schemaTable in schemaExtract.Tables)
        //    {
               
        //        var tableNode = new SchemaTableElement(RefPathFor(schemaTable.Urn), schemaTable.ObjectName, null, schemaElement);

        //        foreach (var smoColumn in schemaTable.Columns)
        //        {
        //            var columnNode = new ColumnElement(RefPathFor(smoColumn.Urn), smoColumn.ObjectName, null, tableNode);
        //            tableNode.AddChild(columnNode);

        //            columnNode.Length = smoColumn.Length;
        //            columnNode.Scale = smoColumn.Scale;
        //            columnNode.Precision = smoColumn.Precision;
        //            columnNode.SqlDataType = smoColumn.SqlDataType;
        //        }

        //        schemaElement.AddChild(tableNode);
        //    }
        //}

        ///// <summary>
        ///// Extracts user defined table types from a database.
        ///// </summary>
        //private void ExtractDatabaseTableTypes(SchemaElement schemaElement, SqlSchema schemaExtract)
        //{
        //    foreach (var smoUdtt in schemaExtract.TableTypes)
        //    {
        //        var tableTypeNode = new UserDefinedTableTypeElement(RefPathFor(smoUdtt.Urn), smoUdtt.ObjectName, null, schemaElement);
                
        //        foreach (var smoColumn in smoUdtt.Columns)
        //        {
        //            var columnNode = new ColumnElement(RefPathFor(smoColumn.Urn), smoColumn.ObjectName, null, tableTypeNode);
        //            tableTypeNode.AddChild(columnNode);

        //            columnNode.Length = smoColumn.Length;
        //            columnNode.Scale = smoColumn.Scale;
        //            columnNode.Precision = smoColumn.Precision;
        //            columnNode.SqlDataType = smoColumn.SqlDataType;
        //        }

        //        schemaElement.AddChild(tableTypeNode);
        //    }
        //}

        ///// <summary>
        ///// Extracts view elements from a database.
        ///// </summary>
        //private void ExtractDatabaseViews(SchemaElement schemaElement, SqlSchema schemaExtract)
        //{
        //    foreach (var smoView in schemaExtract.Views)
        //    {
        //        var viewNode = new ViewElement(RefPathFor(smoView.Urn), smoView.ObjectName, null, schemaElement);

        //        for (int i = 0; i < smoView.Columns.Count; i++)
        //        {
        //            var smoColumn = smoView.Columns[i];
                    
        //            var columnNode = new ColumnElement(RefPathFor(smoColumn.Urn), smoColumn.ObjectName, null, viewNode);

        //            viewNode.AddChild(columnNode);

        //            columnNode.Length = smoColumn.Length;
        //            columnNode.Scale = smoColumn.Scale;
        //            columnNode.Precision = smoColumn.Precision;
        //            columnNode.SqlDataType = smoColumn.SqlDataType;
        //        }

        //        schemaElement.AddChild(viewNode);

        //    }
        //}

        ///// <summary>
        ///// Extracts udf elements from a database.
        ///// </summary>
        //private void ExtractDatabaseUdfs(SchemaElement schemaElement, SqlSchema schemaExtract)
        //{
        //    foreach (var scalarUdf in schemaExtract.ScalarUdfs)
        //    {
        //        var udfElement = new ScalarUdfElement(RefPathFor(scalarUdf.Urn), scalarUdf.ObjectName, null, schemaElement);
        //        schemaElement.AddChild(udfElement);
        //    }

        //    foreach (var tableUdf in schemaExtract.TableUdfs)
        //    {
        //        var udfElement = new TableUdfElement(RefPathFor(tableUdf.Urn), tableUdf.ObjectName, null, schemaElement);
        //        schemaElement.AddChild(udfElement);
        //    }
        //}

        ///// <summary>
        ///// Extracts procedure elements from a database.
        ///// </summary>
        //private void ExtractDatabaseStoredProcedures(SchemaElement schemaElement, SqlSchema schemaExtract)
        //{
        //    foreach (var smoSp in schemaExtract.Procedures)
        //    {
        //        var spNode = new ProcedureElement(RefPathFor(smoSp.Urn), smoSp.ObjectName, null, schemaElement);

        //        schemaElement.AddChild(spNode);
        //    }
        //}
        
    }
}
