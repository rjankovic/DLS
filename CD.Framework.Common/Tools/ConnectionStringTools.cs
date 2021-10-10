using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Common.Tools
{
    public static class ConnectionStringTools
    {
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

        public static string NormalizeServerInConnectionString(string connectionString)
        {
            var localhostName = System.Net.Dns.GetHostName();
            var segments = connectionString.Trim().Split(';');
            var dataSourceSegment = segments.FirstOrDefault(x => x.Trim().ToLower().StartsWith("data source"));
            if (dataSourceSegment == null)
            {
                return connectionString;
            }
            var dbNameSegment = segments.FirstOrDefault(x => x.Trim().ToLower().StartsWith("initial catalog"));
            var dataSource = dataSourceSegment.Substring(dataSourceSegment.IndexOf('=') + 1).Trim().ToLower();

            //bool isLocalhost = dataSource == "." || dataSource == "localhost" || dataSource == "(local)";

            bool isLocalhost;

            if (dataSource.Contains("\\"))
            {
                isLocalhost = dataSource.StartsWith(".\\") || dataSource.StartsWith("localhost\\") || dataSource.StartsWith("(local)\\");
                if (isLocalhost)
                {
                    var instanceName = dataSource.Substring(dataSource.IndexOf('\\') + 1);
                    localhostName = localhostName + "\\" + instanceName;
                }

            }
            else
            {
                isLocalhost = dataSource == "." || dataSource == "localhost" || dataSource == "(local)";
                //string path = string.Empty;
                //if (isLocalhost)
                //{
                //    dataSource = localhostName; // System.Net.Dns.GetHostName();
                //}
            }

            if (!isLocalhost)
            {
                return connectionString;
            }

            var dataSourceIdx = connectionString.ToLower().IndexOf("data source");
            var dataSourceValueIdx = connectionString.IndexOf('=', dataSourceIdx) + 1;
            var localhostIdx = connectionString.ToLower().IndexOf(dataSource, dataSourceValueIdx);
            var resConnString = connectionString.Substring(0, dataSourceValueIdx) + localhostName;
            if (connectionString.Length > localhostIdx + dataSource.Length)
            {
                resConnString += connectionString.Substring(localhostIdx + dataSource.Length, connectionString.Length - localhostIdx - dataSource.Length);
            }

            return resConnString;
        }

        public static string GetServerName(string connectionString)
        {
            var localhostName = System.Net.Dns.GetHostName();
            var segments = connectionString.Trim().Split(';');
            var dataSourceSegment = segments.FirstOrDefault(x => x.Trim().ToLower().StartsWith("data source"));
            if (dataSourceSegment == null)
            {
                return null;
            }
            var dbNameSegment = segments.FirstOrDefault(x => x.Trim().ToLower().StartsWith("initial catalog"));
            var dataSource = dataSourceSegment.Substring(dataSourceSegment.IndexOf('=') + 1).Trim();
            string dbName = null;
            if (dbNameSegment != null)
            {
                dbName = dbNameSegment.Substring(dbNameSegment.IndexOf('=') + 1).Trim();
            }
            //bool isLocalhost = dataSource == "." || dataSource == "localhost" || dataSource == "(local)";
            //string path = string.Empty;
            //if (isLocalhost)
            //{
            //    dataSource = System.Net.Dns.GetHostName();
            //}

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
            var local1 = ".";
            var local2 = "localhost";
            var local3 = "(local)";
            var localName = System.Net.Dns.GetHostName().Trim().ToLower();

            if (s1.StartsWith(local1))
            {
                s1 = localName + s1.Substring(local1.Length);
            }
            if (s2.StartsWith(local1))
            {
                s2 = localName + s2.Substring(local1.Length);
            }
            if (s1.StartsWith(local2))
            {
                s1 = localName + s1.Substring(local2.Length);
            }
            if (s2.StartsWith(local2))
            {
                s2 = localName + s2.Substring(local2.Length);
            }
            if (s1.StartsWith(local3))
            {
                s1 = localName + s1.Substring(local3.Length);
            }
            if (s2.StartsWith(local3))
            {
                s2 = localName + s2.Substring(local3.Length);
            }
            
            //bool s1IsLocalhost = s1 == "." || s1 == "localhost" || s1 == "(local)" || s1 == System.Net.Dns.GetHostName().Trim().ToLower();
            //bool s2IsLocalhost = s2 == "." || s2 == "localhost" || s2 == "(local)" || s2 == System.Net.Dns.GetHostName().Trim().ToLower();
            //return (s1IsLocalhost && s2IsLocalhost) || (s1 == s2);

            return s1 == s2;
        }

        public static string NormalizeServerName(string serverName, string localhostInterpretation = null)
        {

            //serverName = serverName.Trim('"');
            if (localhostInterpretation == null)
            {
                localhostInterpretation = System.Environment.MachineName;
            }

            if (serverName == null)
            {
                return null;
            }
            serverName = serverName.Trim().ToUpper();
            bool prefixLocalhost = false;
            if (serverName.StartsWith("."))
            {
                prefixLocalhost = true;
                serverName = serverName.Substring(1);
            }
            else if (serverName.StartsWith("LOCALHOST"))
            {
                prefixLocalhost = true;
                serverName = serverName.Substring("LOCALHOST".Length);
            }
            else if (serverName.StartsWith("(LOCAL)"))
            {
                prefixLocalhost = true;
                serverName = serverName.Substring("(LOCAL)".Length);
            }
            if (prefixLocalhost)
            {
                serverName = localhostInterpretation + serverName;
            }
            return serverName;
        }


            public static string GetSsasDbRefPath(string connectionString, out string serverName, out string dbName)
        {
            var segments = connectionString.Split(';');
            var dataSourceSegment = segments.First(x => x.Trim().ToLower().StartsWith("data source"));
            var dbNameSegment = segments.FirstOrDefault(x => x.Trim().ToLower().StartsWith("initial catalog"));
            var dataSource = dataSourceSegment.Substring(dataSourceSegment.IndexOf('=') + 1).Trim();
            dataSource = NormalizeServerName(dataSource);
            serverName = dataSource;
            dbName = null;
            if (dbNameSegment != null)
            {
                dbName = dbNameSegment.Substring(dbNameSegment.IndexOf('=') + 1).Trim();
            }
            string path = string.Empty;
            var refPath = String.Format("SSASServer[@Name='{0}']", dataSource);
            if (dbName != null)
            {
                refPath += String.Format("/Db[@Name='{0}']", dbName);
            }
            return refPath;
        }

        public static string GetCubeRefPath(string connectionString, string cubeName)
        {
            string dummy1, dummy2;
            var dbRefPath = GetSsasDbRefPath(connectionString, out dummy1, out dummy2);
            var cubeRefPath = string.Format("{0}/Cube[@Name='{1}']", dbRefPath, cubeName);
            return cubeRefPath;
        }
        
    }
}
