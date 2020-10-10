
using CD.DLS.Model.Mssql;
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
    /// Provides model extraction from SQL scripts.
    /// </summary>
    public interface ISqlScriptModelParser
    {
        string ContextServerName { get; set; }

        /// <summary>
        /// The parser used to parse the SQL scripts
        /// </summary>
        TSqlParser Parser { get; }

        /// <summary>
        /// Extracts model from a SQL script.
        /// </summary>
        SqlScriptElement ExtractScriptModel(string sqlScript, MssqlModelElement parent, IReferrableIndex context, Identifier dbIdentifier);
        
        /// <summary>
        /// Extracts model from a SQL script and provides the columns of the first resultset.
        /// </summary>
        SqlScriptElement ExtractScriptModel(string sqlScript, MssqlModelElement parent, IReferrableIndex context, Identifier dbIdentifier, out Dictionary<string, MssqlModelElement> resultColumns);

        SqlScriptElement ExtractScriptModel(string sqlScript, MssqlModelElement parent, IReferrableIndex context, Identifier dbIdentifier, out Dictionary<string, MssqlModelElement> resultColumns, out List<Tuple<string, MssqlModelElement>> resultColumnsOrdinal);


        /// <summary>
        /// Extracts model from a SQL script and provides the columns of the first resultset and a the temp tables created within the script.
        /// </summary>
        /// <param name="script"></param>
        /// <param name="parent"></param>
        /// <param name="environment"></param>
        /// <param name="dbIdentifier"></param>
        /// <param name="resultColumns"></param>
        /// <param name="createdTempTables"></param>
        /// <returns></returns>
        SqlScriptElement ExtractScriptModel(string script, MssqlModelElement parent, IReferrableIndex environment, Identifier dbIdentifier,
            out Dictionary<string, MssqlModelElement> resultColumns, out Dictionary<string, TableSourceColumnList> createdTempTables);

        /// <summary>
        /// For unknown database engines, attempt to parse SQL
        /// </summary>
        /// <param name="sqlScript"></param>
        /// <param name="parent"></param>
        /// <param name="resultColumns"></param>
        /// <returns></returns>
        ForeignProviderSqlScriptElement ExtractForeignScriptModel(string sqlScript, MssqlModelElement parent, out Dictionary<string, MssqlModelElement> resultColumns);
        
        /// <summary>
        /// Extracts model from a SQL script fragment.
        /// </summary>
        SqlScriptElement ExtractScriptModel(TSqlFragment script, MssqlModelElement parent, IReferrableIndex context, Identifier dbIdentifier);

        /// <summary>
        /// Extracts model from a SQL script fragment.
        /// </summary>
        SqlScriptElement ExtractScriptModel(TSqlFragment script, MssqlModelElement parent, IReferrableIndex context, Identifier dbIdentifier, out Dictionary<string, MssqlModelElement> resultColumns);


        SqlScriptElement ExtractScriptModel(TSqlFragment script, MssqlModelElement parent, IReferrableIndex context, Identifier dbIdentifier, out Dictionary<string, MssqlModelElement> resultColumns, 
            out List<Tuple<string, MssqlModelElement>> resultColumnsOrdinal, out Dictionary<string, TableSourceColumnList> createdTempTables);


        /// <summary>
        /// Helper for components that do not have direct access to the DB identifier
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        string GetDbNameFromConnectionString(string connectionString);
        string GetServerNameFromConnectionString(string connectionString, string localhostInterpretation);
        string GetServerNameFromConnectionString(string connectionString);

        bool CompareServerNames(string server1, string server2);

        SqlScriptElement ParseAndResolveOverTableSource(string expression, TableSourceColumnList tableSource, MssqlModelElement parent);
    }
}