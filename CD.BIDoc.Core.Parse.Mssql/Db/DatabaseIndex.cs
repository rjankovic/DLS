using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CD.DLS.Common.Tools;
using CD.DLS.Model.Interfaces;
using CD.DLS.DAL.Managers;
using CD.DLS.Common.Structures;
using CD.DLS.Model.Mssql;

namespace CD.DLS.Parse.Mssql.Db
{


    public class PresetVariable
    {
        public string VariableName { get; set; }
        public IModelElement VariableObject { get; set; }
    }

    public interface IReferrableIndex
    {
        string ContextServerName { get; }
        void SetContextServer(string name, string localhostInterpretation);
        void SetContextServer(string name);

        IEnumerable<PresetVariable> PresetVariables { get; }
        IEnumerable<TableSourceColumnList> TableSourcesAvailable { get; }
        IEnumerable<TableSourceColumnList> TableTypesAvailable { get; }


        TableSourceColumnList FindTableByObjectName(string openRowset, TSqlParser parser, Identifier dbInUse);
        ReferrableObject FindStoredProcedure(MultiPartIdentifier name, Identifier dbInUse);
        TableSourceColumnList FindTableSource(SchemaObjectName identifier, Identifier dbInUse);
        TableSourceColumnList FindTableType(SchemaObjectName identifier, Identifier dbInUse);
        ReferrableObject FindScalarUdf(MultiPartIdentifier mpi, Identifier dbInUse);

        TableSourceColumnList FindTableByIdentifier(List<TableSourceColumnList> tables, TSqlFragment identifier, Identifier dbInUse, int referencePosition = -1);
        ReferrableObject FindTableColumnByIdentifier(TableSourceColumnList table, TSqlFragment identifier, Identifier dbInUse);

        IModelElement FindNodeByTableObjectName(string openRowset, TSqlParser parser, Identifier dbInUse);

        void AddTableSource(TableSourceColumnList tableSourceColumnList);
        void RemoveTableSource(TableSourceColumnList tableSourceColumnList);
        //IModelElement GetNode(Urn urn);
    }

    public class AddPresetVariablesReferrableIndex : IReferrableIndex
    {
        private string _contextServerName;
        public string ContextServerName { get { return _contextServerName; } }

        private readonly IReferrableIndex _parentIndex;
        private readonly List<PresetVariable> _presetVariables;
        private readonly List<TableSourceColumnList> _presetTables = new List<TableSourceColumnList>();

        public AddPresetVariablesReferrableIndex(IReferrableIndex parentIndex)
        {
            this._parentIndex = parentIndex;
            this._presetVariables = new List<PresetVariable>(parentIndex.PresetVariables);
        }

        public void AddTable(TableSourceColumnList table)
        {


            var tableCopy =
new TableSourceColumnList()
{
    Columns = table.Columns,
    Identifier = table.Identifier,
    ObjectContent = null,
    ReferenceType = ScriptReferenceTypeEnum.Table,
    ValidSpan = ScriptSpan.DbWide,
    ModelElement = table.ModelElement,
    Urn = table.ModelElement.RefPath

};
            
            _presetTables.Add(tableCopy);
            _parentIndex.AddTableSource(tableCopy);
        }

        public void ClearPresetTables()
        {
            while (_presetTables.Any())
            {
                RemoveTableSource(_presetTables.First());
            }

        }

        public IEnumerable<PresetVariable> PresetVariables
        {
            get
            {
                return _presetVariables;
            }
        }

        public IEnumerable<TableSourceColumnList> TableSourcesAvailable
        {
            get
            {
                return _parentIndex.TableSourcesAvailable;
            }
        }

        public IEnumerable<TableSourceColumnList> TableTypesAvailable
        {
            get
            {
                return _parentIndex.TableTypesAvailable;
            }
        }

        public void AddTableSource(TableSourceColumnList tableSourceColumnList)
        {
            AddTable(tableSourceColumnList);
        }

        public void RemoveTableSource(TableSourceColumnList tableSourceColumnList)
        {
            _parentIndex.RemoveTableSource(tableSourceColumnList);
            _presetTables.Remove(tableSourceColumnList);
        }

        public void AddPresetVariable(string name, IModelElement variable)
        {
            _presetVariables.Add(new PresetVariable { VariableName = name, VariableObject = variable });
        }

        public IModelElement FindNodeByTableObjectName(string openRowset, TSqlParser parser, Identifier dbInUse)
        {
            return _parentIndex.FindNodeByTableObjectName(openRowset, parser, dbInUse);
        }

        public ReferrableObject FindScalarUdf(MultiPartIdentifier mpi, Identifier dbInUse)
        {
            return _parentIndex.FindScalarUdf(mpi, dbInUse);
        }

        public ReferrableObject FindStoredProcedure(MultiPartIdentifier name, Identifier dbInUse)
        {
            return _parentIndex.FindStoredProcedure(name, dbInUse);
        }

        public TableSourceColumnList FindTableByObjectName(string openRowset, TSqlParser parser, Identifier dbInUse)
        {
            return _parentIndex.FindTableByObjectName(openRowset, parser, dbInUse);
        }

        public TableSourceColumnList FindTableSource(SchemaObjectName identifier, Identifier dbInUse)
        {
            return _parentIndex.FindTableSource(identifier, dbInUse);
        }

        public TableSourceColumnList FindTableType(SchemaObjectName identifier, Identifier dbInUse)
        {
            return _parentIndex.FindTableType(identifier, dbInUse);
        }

        public TableSourceColumnList FindTableByIdentifier(List<TableSourceColumnList> tables, TSqlFragment identifier, Identifier dbInUse, int referencePosition = -1)
        {
            return _parentIndex.FindTableByIdentifier(tables, identifier, dbInUse, referencePosition);
        }

        public ReferrableObject FindTableColumnByIdentifier(TableSourceColumnList table, TSqlFragment identifier, Identifier dbInUse)
        {
            return _parentIndex.FindTableColumnByIdentifier(table, identifier, dbInUse);
        }

        public void SetContextServer(string name, string localhostInterpretation)
        {
            _contextServerName = ConnectionStringTools.NormalizeServerName(name, localhostInterpretation);
        }

        public void SetContextServer(string name)
        {
            SetContextServer(name, null);
        }
    }
    
    //TODO: VD: rename to GlobalReferrableIndex
    public class DatabaseIndex : IReferrableIndex
    {
        private Dictionary<string, List<TableSourceColumnList>> _tableSourcesAvailableByServer = new Dictionary<string, List<TableSourceColumnList>>();
        private Dictionary<string, List<TableSourceColumnList>> _tableTypesAvailableByServer = new Dictionary<string, List<TableSourceColumnList>>();
        private Dictionary<string, List<ReferrableObject>> _scalarUdfsAvailableByServer = new Dictionary<string, List<ReferrableObject>>();
        private Dictionary<string, List<ReferrableObject>> _storedProceduresAvailableByServer = new Dictionary<string, List<ReferrableObject>>();
        public Dictionary<MssqlModelElement, int> PremappedIds { get; set; }
        private string _serverName;
        public string ContextServerName
        {
            get
            {
                return _serverName;
            }
            //set
            //{
            //    _serverName = ConnectionStringTools.NormalizeServerName(value); // value;
            //    /*
            //    var existentMatch = _tableTypesAvailableByServer.Keys.FirstOrDefault(x => CD.Framework.Common.Tools.ConnectionStringTools.AreServersNamesEqual(x, value));
            //    if (existentMatch != null)
            //    {
            //        _serverName = existentMatch;
            //        return;
            //    }
            //    */
            //    if (!_tableSourcesAvailableByServer.ContainsKey(_serverName))
            //    {
            //        _tableSourcesAvailableByServer.Add(_serverName, new List<TableSourceColumnList>());
            //        _tableTypesAvailableByServer.Add(_serverName, new List<TableSourceColumnList>());
            //        _scalarUdfsAvailableByServer.Add(_serverName, new List<ReferrableObject>());
            //        _storedProceduresAvailableByServer.Add(_serverName, new List<ReferrableObject>());
            //    }
            //}
        }

        //private List<TableSourceColumnList> _tableSourcesAvailable = new List<TableSourceColumnList>();
        //private List<TableSourceColumnList> _tableTypesAvailable = new List<TableSourceColumnList>();

        public List<ReferrableObject> ScalarUdfsAvailable
        {
            get {
                if (_serverName == null)
                {
                    return new List<ReferrableObject>();
                }
                return _scalarUdfsAvailableByServer[_serverName]; }
            set {
                if (_serverName == null)
                {
                    return;
                }
                _scalarUdfsAvailableByServer[_serverName] = value;
            }
        }
        public List<ReferrableObject> StoredProceduresAvailable
        {
            get {
                if (_serverName == null)
                {
                    return new List<ReferrableObject>();
                }
                return _storedProceduresAvailableByServer[_serverName];
            }
            set {
                if (_serverName == null)
                {
                    return;
                }
                _storedProceduresAvailableByServer[_serverName] = value;
            }
        }

        /// <summary>
        /// Should be initialized by the list of all schema objects that may appear in the script
        /// </summary>
        public IEnumerable<TableSourceColumnList> TableSourcesAvailable { get {
                if (_serverName == null)
                {
                    return new List<TableSourceColumnList>();
                }
                if (!_tableSourcesAvailableByServer.ContainsKey(_serverName))
                {
                    return new List<TableSourceColumnList>();
                }
                return _tableSourcesAvailableByServer[_serverName];
            } }

        /// <summary>
        /// Not actual types, just user-defined table types
        /// </summary>
        public IEnumerable<TableSourceColumnList> TableTypesAvailable { get { return _tableTypesAvailableByServer[_serverName]; } }

        //private List<PresetVariable> _presetVariables = new List<PresetVariable>();
        public IEnumerable<PresetVariable> PresetVariables { get { return Enumerable.Empty<PresetVariable>(); } }

        public DatabaseIndex(string initialServerName = null)
        {
            if (initialServerName != null)
            {
                SetContextServer(initialServerName, null);
                //ContextServerName = initialServerName;
            }
        }

        public ReferrableObject FindStoredProcedure(MultiPartIdentifier name, Identifier dbInUse)
        {

            var identifierComparer = new IdentifierComparer(dbInUse);

            return StoredProceduresAvailable.FirstOrDefault(sp => identifierComparer.IdentifiersEqual(sp.Identifier, name));
        }

        public ReferrableObject FindScalarUdf(MultiPartIdentifier mpi, Identifier dbInUse)
        {
            var identifierComparer = new IdentifierComparer(dbInUse);

            return ScalarUdfsAvailable.FirstOrDefault(udf => identifierComparer.IdentifiersEqual(udf.Identifier, mpi));
        }






        /// <summary>
        /// Find table or other table source by identifier within the context of the current DB
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="identifier">Identifier or MultiPartIdentifier</param>
        /// <returns></returns>
        public TableSourceColumnList FindTableByIdentifier(List<TableSourceColumnList> tables, TSqlFragment identifier, Identifier dbInUse, int referencePosition = -1)
        {
            var identifierComparer = new IdentifierComparer(dbInUse);
            var matchingTable = tables.Where(tab => identifierComparer.IdentifiersEqual(tab.Identifier, identifier));
            if (referencePosition == -1)
            {
                return matchingTable.FirstOrDefault();
            }
            return matchingTable.FirstOrDefault(x => x.ModelElement != null || x.ObjectContent.LastTokenIndex < referencePosition);
        }

        /// <summary>
        /// Find a particular column in the table source
        /// </summary>
        /// <param name="table"></param>
        /// <param name="identifier">Identifier or MultiPartIdentifier</param>
        /// <returns></returns>
        public ReferrableObject FindTableColumnByIdentifier(TableSourceColumnList table, TSqlFragment identifier, Identifier dbInUse)
        {
            var identifierComparer = new IdentifierComparer(dbInUse);
            var column = table.Columns.FirstOrDefault(col => identifierComparer.IdentifiersEqual(col.Identifier, identifier));
            return column;
        }

        public TableSourceColumnList FindTableSource(SchemaObjectName identifier, Identifier dbInUse)
        {
            return FindTableByIdentifier(TableSourcesAvailable./*TODO: Where(x => x.IsSchemaObject).*/ToList(), identifier, dbInUse);
        }

        private SchemaObjectName ParseObjectName(string name, TSqlParser parser)
        {
            using (TextReader sr = new StringReader(name))
            {
                IList<ParseError> errors = new List<ParseError>();
                var parsed = parser.Parse(sr, out errors);

                var script = (TSqlScript)parsed;
                var stmt = (ExecuteStatement)(script.Batches[0].Statements[0]);
                var epr = (ExecutableProcedureReference)(stmt.ExecuteSpecification.ExecutableEntity);
                return epr.ProcedureReference.ProcedureReference.Name;

            }
        }

        public TableSourceColumnList FindTableByObjectName(string openRowset, TSqlParser parser, Identifier dbInUse)
        {
            return FindTableByIdentifier(TableSourcesAvailable.ToList(), ParseObjectName(openRowset, parser), dbInUse);

        }

        public IModelElement FindNodeByTableObjectName(string openRowset, TSqlParser parser, Identifier dbInUse)
        {
            var tableSource = FindTableByObjectName(openRowset, parser, dbInUse);
            if (tableSource == null)
                return null;
            return tableSource.ModelElement;
        }

        public void AddTableSource(TableSourceColumnList tableSourceColumnList)
        {
            if (_serverName == null)
            {
                return;
            }
            _tableSourcesAvailableByServer[_serverName].Add(tableSourceColumnList);
        }

        public void RemoveTableSource(TableSourceColumnList tableSourceColumnList)
        {
            if (_serverName == null)
            {
                return;
            }
            _tableSourcesAvailableByServer[_serverName].Remove(tableSourceColumnList);
        }

        public void AddTableType(TableSourceColumnList tableSourceColumnList)
        {
            if (_serverName == null)
            {
                return;
            }
            _tableTypesAvailableByServer[_serverName].Add(tableSourceColumnList);
        }

        public TableSourceColumnList FindTableType(SchemaObjectName identifier, Identifier dbInUse)
        {
            var identifierComparer = new IdentifierComparer(dbInUse);
            return _tableTypesAvailableByServer[_serverName].FirstOrDefault(tt => identifierComparer.IdentifiersEqual(tt.Identifier, identifier));
        }

        public void SetContextServer(string name, string localhostInterpretation)
        {
            _serverName = ConnectionStringTools.NormalizeServerName(name, localhostInterpretation);

            if (!_tableSourcesAvailableByServer.ContainsKey(_serverName))
            {
                _tableSourcesAvailableByServer.Add(_serverName, new List<TableSourceColumnList>());
                _tableTypesAvailableByServer.Add(_serverName, new List<TableSourceColumnList>());
                _scalarUdfsAvailableByServer.Add(_serverName, new List<ReferrableObject>());
                _storedProceduresAvailableByServer.Add(_serverName, new List<ReferrableObject>());
            }

        }

        public void SetContextServer(string name)
        {
            SetContextServer(name, null);
        }
    }

    public class AvailableDatabaseModelIndex
    {
        private Dictionary<string, Dictionary<string, Model.Mssql.Db.DatabaseElement>> _availableDatabasesByServer = new Dictionary<string, Dictionary<string, Model.Mssql.Db.DatabaseElement>>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, DatabaseIndex> _loadedContextPerServer = new Dictionary<string, DatabaseIndex>(StringComparer.InvariantCultureIgnoreCase);
        private ProjectConfig _projectConfig = null;
        private GraphManager _graphManager = null;

        public AvailableDatabaseModelIndex(ProjectConfig projectConfig, GraphManager graphManager)
        {
            _graphManager = graphManager;
            _projectConfig = projectConfig;

            foreach (var dbComponent in projectConfig.DatabaseComponents)
            {
                if (!_availableDatabasesByServer.ContainsKey(dbComponent.ServerName))
                {
                    Model.Serialization.SerializationHelper sh = new Model.Serialization.SerializationHelper(_projectConfig, _graphManager);
                    var serverModel = sh.LoadElementModelToChildrenOfType(
                        UrnBuilder.GetServerUrn(dbComponent.ServerName).Path,
                        typeof(Model.Mssql.Db.DatabaseElement)) as Model.Mssql.Db.ServerElement;

                    _availableDatabasesByServer.Add(dbComponent.ServerName, new Dictionary<string, Model.Mssql.Db.DatabaseElement>());

                    foreach (var db in serverModel.Databases)
                    {
                        _availableDatabasesByServer[dbComponent.ServerName].Add(db.DbName, db);
                    }
                }
            }
        }

        public Dictionary<MssqlModelElement, int> GetAllPremappedIds()
        {
            Dictionary<MssqlModelElement, int> res = new Dictionary<MssqlModelElement, int>();
            foreach (var dbIxKey in _loadedContextPerServer.Keys)
            {
                var dbIx = _loadedContextPerServer[dbIxKey];
                foreach (var kv in dbIx.PremappedIds)
                {
                    if (res.ContainsKey(kv.Key))
                    {
                        continue;
                    }
                    res.Add(kv.Key, kv.Value);
                }
            }
            foreach (var db in _availableDatabasesByServer.Values.SelectMany(dct => dct.Values))
            {
                if (res.ContainsKey(db))
                {
                    continue;
                }
                res.Add(db, db.Id);
            }
            return res;
        }

        public Model.Mssql.Db.DatabaseElement GetDatabaseElement(string serverName, string dbName)
        {
            if (!_availableDatabasesByServer.ContainsKey(serverName))
            {
                return null;
            }

            if (_availableDatabasesByServer[serverName].ContainsKey(dbName))
            {
                return _availableDatabasesByServer[serverName][dbName];
            }

            return null;
        }

        private bool DatabaseElementExists(string serverName, string dbName)
        {
            return GetDatabaseElement(serverName, dbName) != null;
        }

        public DatabaseIndex GetDatabaseIndex(string serverName, string localhostInterpretation = null)
        {
            var interpretedName = ConnectionStringTools.NormalizeServerName(serverName, localhostInterpretation);

            if (interpretedName == null)
            {
                var res = new DatabaseIndex();
                res.SetContextServer(string.Empty);
                res.PremappedIds = new Dictionary<MssqlModelElement, int>();
                return res;
            }

            if (!_availableDatabasesByServer.ContainsKey(interpretedName))
            {
                var res = new DatabaseIndex();
                res.SetContextServer(interpretedName);
                res.PremappedIds = new Dictionary<MssqlModelElement, int>();
                return res;
            }

            if (!_loadedContextPerServer.ContainsKey(interpretedName))
            {
                var idMap = LoadServerContext(interpretedName);
                //_loadedContextPerServer[interpretedName].PremappedIds = idMap;
            }
            
            var x = _loadedContextPerServer[interpretedName];
            x.SetContextServer(serverName, localhostInterpretation);
            return x;
        }

        private Dictionary<MssqlModelElement, int> LoadServerContext(string serverName)
        {
            var serverUrn = UrnBuilder.GetServerUrn(serverName);
            var sh = new Model.Serialization.SerializationHelper(_projectConfig, _graphManager);
            //var model = (Model.Mssql.Db.ServerElement)sh.LoadElementModelToChildrenOfTypesNoDef(serverUrn.Path, new List<Type>()
            //{
            //    typeof(Model.Mssql.Db.ScalarUdfElement),
            //    typeof(Model.Mssql.Db.ProcedureElement),
            //    typeof(Model.Mssql.Db.ColumnElement)
            //});

            var model = (Model.Mssql.Db.ServerElement)sh.LoadElementModel(serverUrn.Path, false);
            ReferrableIndexBuilder rib = new ReferrableIndexBuilder();
            var ix = rib.BuildIndex(new List<Model.Mssql.Db.ServerElement>() { model });
            ix.SetContextServer(serverName);
            ix.PremappedIds = rib.ElementIdMap;
            _loadedContextPerServer.Add(serverName, ix);
            return ix.PremappedIds;
        }
    }
}
