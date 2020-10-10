using System;
using System.Collections.Generic;
using System.Linq;
using CD.DLS.Model.Mssql.Ssas;
using System.Text.RegularExpressions;
using CD.DLS.DAL.Configuration;
using CD.DLS.Model.Serialization;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.Model.Mssql;
using CD.DLS.Model;
using CD.DLS.Model.Mssql.Tabular;

namespace CD.DLS.Parse.Mssql.Ssas
{
    public enum SsasQueryMode { MDX = 1, DAX = 2 }

    public abstract class SsasDatabaseIndex
    {
        public abstract Dictionary<MssqlModelElement, int> GetPremappedIds();
        public abstract void ClearLocalIndexes();

        protected SsasTypeEnum _ssasType;
        public SsasTypeEnum SsasType { get { return _ssasType; } }
        
        private SsasQueryMode _queryMode = SsasQueryMode.MDX;
        public SsasQueryMode QueryMode {
            get { return _queryMode; }
            set { _queryMode = value; }
        }
    }
    
    public class SsasServerIndex
    {
        private Dictionary<string, Dictionary<string, SsasDatabaseIndex>> _databasesPerServerDictionary = new Dictionary<string, Dictionary<string, SsasDatabaseIndex>>(StringComparer.OrdinalIgnoreCase);
        private ProjectConfig _projectConfig;
        private GraphManager _graphManager;

        public void AddDatabase(SsasDatabaseElement database)
        {
            var serverName = ((ServerElement)database.Parent).Caption;
            if (!_databasesPerServerDictionary.ContainsKey(serverName))
            {
                _databasesPerServerDictionary.Add(serverName, new Dictionary<string, SsasDatabaseIndex>());
            }
            if (database.SsasType == SsasTypeEnum.Multidimensional)
            {
                _databasesPerServerDictionary[serverName].Add(database.Caption, new SsasMultidimensionalDatabaseIndex((SsasMultidimensionalDatabaseElement)database, _graphManager, _projectConfig));
            }
            else
            {
                _databasesPerServerDictionary[serverName].Add(database.Caption, new SsasTabularDatabaseIndex((SsasTabularDatabaseElement)database, _graphManager, _projectConfig));
            }
        }

        public SsasDatabaseIndex GetDatabase(string serverName, string databaseName)
        {
            if (databaseName == null || serverName == null)
            {
                return null;
            }
            if (_databasesPerServerDictionary.ContainsKey(serverName))
            {
                if (_databasesPerServerDictionary[serverName].ContainsKey(databaseName))
                {
                    return _databasesPerServerDictionary[serverName][databaseName];
                }
            }
            return null;
        }

        public SsasServerIndex(ProjectConfig projectConfig, GraphManager graphManager)
        {
            _projectConfig = projectConfig;
            _graphManager = graphManager;
            LoadDatabaseList();
        }

        public Dictionary<MssqlModelElement, int> GetPremappedIds()
        {
            Dictionary<MssqlModelElement, int> res = new Dictionary<MssqlModelElement, int>();
            foreach (var dbDict in _databasesPerServerDictionary.Values)
            {
                foreach (var dbIdx in dbDict.Values)
                {
                    foreach (var kv in dbIdx.GetPremappedIds())
                    {
                        if (!res.ContainsKey(kv.Key))
                        {
                            res.Add(kv.Key, kv.Value);
                        }
                    }
                }
            }

            return res;
        }

        private void LoadDatabaseList()
        {
            SerializationHelper sh = new SerializationHelper(_projectConfig, _graphManager);

            var multidimensionals = (SolutionModelElement)sh.LoadElementModelToChildrenOfType("", typeof(SsasMultidimensionalDatabaseElement));
            foreach (var serverElement in multidimensionals.SsasServers)
            {
                foreach (var db in serverElement.Databases)
                {
                    AddDatabase(db);
                }
            }

            var tabulars = (SolutionModelElement)sh.LoadElementModelToChildrenOfType("", typeof(SsasTabularDatabaseElement));
            foreach (var serverElement in tabulars.SsasServers)
            {
                foreach (var db in serverElement.Databases)
                {
                    AddDatabase(db);
                }
            }
        }

        public SsasServerIndex(List<ServerElement> serverElements, ProjectConfig projectConfig, GraphManager graphManager)
        {
            _projectConfig = projectConfig;
            _graphManager = graphManager;
            foreach (var serverElement in serverElements)
            {
                foreach (var db in serverElement.Databases)
                {
                    AddDatabase(db);
                }
            }
        }
    }

    public class SsasIndex
    {
        private Dictionary<string, SsasServerIndex> _serverDictionary = new Dictionary<string, SsasServerIndex>(StringComparer.OrdinalIgnoreCase);

        //public void AddServer(ServerElement server)
        //{
        //    _serverDictionary.Add(server.Caption, new SsasServerIndex(server));
        //}

        public SsasServerIndex GetServer(string serverName)
        {
            if (_serverDictionary.ContainsKey(serverName))
            {
                return _serverDictionary[serverName];
            }

            return null;
        }

        public SsasDatabaseIndex GetDatabase(string connectionString)
        {
            //Data Source=localhost;Initial Catalog=Manpower_SSAS

            var localhostName = System.Net.Dns.GetHostName();
            var segments = connectionString.Split(';');
            //var dataSourceSegment = segments.First(x => x.ToLower().StartsWith("data source"));
            var dbNameSegment = segments.FirstOrDefault(x => x.ToLower().StartsWith("initial catalog"));
            //var dataSource = dataSourceSegment.Substring(dataSourceSegment.IndexOf('=') + 1).Trim();
            string dbName = null;
            if (dbNameSegment != null)
            {
                dbName = dbNameSegment.Substring(dbNameSegment.IndexOf('=') + 1).Trim();
            }
            else
            {
                throw new Exception();
            }

            //bool isLocalhost = dataSource == "." || dataSource == "localhost" || dataSource == "(local)";
            //string path = string.Empty;
            //if (isLocalhost)
            //{
            //    dataSource = System.Net.Dns.GetHostName();
            //}

            var serverName = Common.Tools.ConnectionStringTools.GetServerName(connectionString);

            var serverIndex = GetServer(serverName);
            var dbIndex = serverIndex.GetDatabase(serverName, dbName);
            return dbIndex;
        }
    }

}
