using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Parse.Mssql.Db
{
    public class UrnBuilder
    {
        public UrnBuilder(string server)
        {
            _server = server;
        }

        private string _server;

        public string GetUrnOfView(string dbName, string schemaName, string viewName)
        {
            return string.Format(@"Server[@Name='{0}']/Database[@Name='{1}']/View[@Name='{3}' and @Schema='{2}']", _server, dbName, schemaName, viewName);
        }

        public string GetUrnOfTable(string dbName, string schemaName, string viewName)
        {
            return string.Format(@"Server[@Name='{0}']/Database[@Name='{1}']/Table[@Name='{3}' and @Schema='{2}']", _server, dbName, schemaName, viewName);
        }

        public string GetUrnOfSp(string dbName, string schemaName, string spName)
        {
            return string.Format("Server[@Name='{0}']/Database[@Name='{1}']/StoredProcedure[@Name='{3}' and @Schema = '{2}']", _server, dbName, schemaName, spName);
        }
        public string GetUrnOfUdf(string dbName, string schemaName, string spName)
        {
            return string.Format("Server[@Name='{0}']/Database[@Name='{1}']/UserDefinedFunction[@Name='{3}' and @Schema='{2}']", _server, dbName, schemaName, spName);
        }
        public string GetColumnUrn(string tableUrn, string columnName)
        {
            return string.Format(@"{0}/Column[@Name='{1}']", tableUrn, columnName);
        }

        public static RefPath GetScriptResultRefPath(RefPath parent, int ordinal)
        {
            return parent.NamedChild("ScriptResult", ordinal.ToString());
        }

        public static RefPath GetScriptResultColumnRefPath(RefPath parent, int ordianl)
        {
            return parent.NamedChild("Column", ordianl.ToString());
        }

        public static RefPath GetServerUrn(string serverName)
        {
            return new RefPath().NamedChild("Server", serverName);
        }

        public static RefPath GetDbUrn(RefPath parent, string name)
        {
            return parent.NamedChild("Database", name);
        }

        public static string GetDbRefPath(string connectionString)
        {
            var localhostName = System.Net.Dns.GetHostName();
            var segments = connectionString.Split(';');
            var dataSourceSegment = segments.First(x => x.Trim().ToLower().StartsWith("data source"));
            var dbNameSegment = segments.FirstOrDefault(x => x.Trim().ToLower().StartsWith("initial catalog"));
            var dataSource = dataSourceSegment.Substring(dataSourceSegment.IndexOf('=') + 1).Trim().ToLower();
            string dbName = null;
            if (dbNameSegment != null)
            {
                dbName = dbNameSegment.Substring(dbNameSegment.IndexOf('=') + 1).Trim();
            }
            bool isLocalhost = dataSource.StartsWith(".") || dataSource.StartsWith("localhost") || dataSource.StartsWith("(local)");
            string path = string.Empty;
            if (isLocalhost)
            {
                if (dataSource.Contains("\\"))
                {
                    var instanceName = dataSource.Substring(dataSource.IndexOf("\\") + 1);
                    dataSource = System.Net.Dns.GetHostName() + "\\" + instanceName;
                }
                else
                {
                    dataSource = System.Net.Dns.GetHostName();
                }
            }
            var refPath = String.Format("Server[@Name='{0}']", dataSource);
            if (dbName != null)
            {
                refPath += String.Format("/Database[@Name='{0}']", dbName);
            }
            return refPath;
        }

        public static string GetDbName(string connectionString)
        {
            var localhostName = System.Net.Dns.GetHostName();
            var segments = connectionString.Split(';');
            //var dataSourceSegment = segments.First(x => x.ToLower().StartsWith("data source"));
            var dbNameSegment = segments.FirstOrDefault(x => x.Trim().ToLower().StartsWith("initial catalog"));
            //var dataSource = dataSourceSegment.Substring(dataSourceSegment.IndexOf('=') + 1).Trim();
            string dbName = null;
            if (dbNameSegment != null)
            {
                dbName = dbNameSegment.Substring(dbNameSegment.IndexOf('=') + 1).Trim();
            }

            return dbName;
        }

        public static string GetServerName(string connectionString, string localhostInterpretation)
        {
            var localhostName = System.Net.Dns.GetHostName();
            if (localhostInterpretation != null)
            {
                localhostName = localhostInterpretation;
            }

            var segments = connectionString.Trim().Split(';');
            var dataSourceSegment = segments.FirstOrDefault(x => x.Trim().ToLower().StartsWith("data source"));
            if (dataSourceSegment == null)
            {
                return null;
            }
            var dbNameSegment = segments.FirstOrDefault(x => x.Trim().ToLower().StartsWith("initial catalog"));
            var dataSource = dataSourceSegment.Substring(dataSourceSegment.IndexOf('=') + 1).Trim().ToLower();
            string dbName = null;
            if (dbNameSegment != null)
            {
                dbName = dbNameSegment.Substring(dbNameSegment.IndexOf('=') + 1).Trim();
            }

            if (dataSource.Contains("\\"))
            {
                bool isLocalhost = dataSource.StartsWith(".\\") || dataSource.StartsWith("localhost\\") || dataSource.StartsWith("(local)\\");
                if (isLocalhost)
                {
                    var instanceName = dataSource.Substring(dataSource.IndexOf('\\') + 1);
                    dataSource = localhostName + "\\" + instanceName;
                }

            }
            else
            {
                bool isLocalhost = dataSource == "." || dataSource == "localhost" || dataSource == "(local)";
                string path = string.Empty;
                if (isLocalhost)
                {
                    dataSource = localhostName; // System.Net.Dns.GetHostName();
                }
            }
            return dataSource;
        }

        public static bool AreServersNamesEqual(string s1, string s2)
        {
            s1 = s1.Trim().ToLower();
            s2 = s2.Trim().ToLower();
            bool s1IsLocalhost = s1 == "." || s1 == "localhost" || s1 == "(local)" || s1 == System.Net.Dns.GetHostName().Trim().ToLower();
            bool s2IsLocalhost = s2 == "." || s2 == "localhost" || s2 == "(local)" || s2 == System.Net.Dns.GetHostName().Trim().ToLower();
            return (s1IsLocalhost && s2IsLocalhost) || (s1 == s2);
        }

    }
}
