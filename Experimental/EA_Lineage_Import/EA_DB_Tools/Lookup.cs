using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Tools
{

    public class Lookup
    {
        private Repository _repository;
        private NetBridge _db;
        private static Dictionary<string, int> _eaPackageIdCache = new Dictionary<string, int>();

        public Lookup(Repository repository)
        {
            _repository = repository;
            _db = new NetBridge(_repository.ConnectionString);
        }

        public int GetPackageId(string path)
        {
            path = path.TrimEnd('/');
            // Sandbox/Radovan Jankovic/NDWH_L1

            // search the cache for a prefix of the path
            string cachedPrefix = string.Empty;
            int knownPrefixEnd = 0;
            int cachedPrefixId = -1;
            while (knownPrefixEnd < path.Length)
            {
                var slashPos = path.IndexOf('/', knownPrefixEnd + 1);
                string checkPrefix = path;
                if (slashPos > -1)
                {
                    checkPrefix = path.Substring(0, slashPos);
                }
                if (_eaPackageIdCache.ContainsKey(checkPrefix))
                {
                    cachedPrefix = checkPrefix;
                    cachedPrefixId = _eaPackageIdCache[checkPrefix];
                    knownPrefixEnd = cachedPrefix.Length;
                }
                else
                {
                    break;
                }
            }

            var knownPrefix = cachedPrefix;
            var lastKnownId = cachedPrefixId;
            var remainingPath = path.Remove(0, cachedPrefix.Length).TrimStart('/');
            while (knownPrefix.Length < path.Length)
            {
                var segment = remainingPath;
                var slashPos = remainingPath.IndexOf('/');
                if (slashPos > -1)
                {
                    segment = remainingPath.Substring(0, slashPos);
                }

                var segmentIdTTbl = _db.ExecuteSelectStatement(
                    "SELECT PDATA1 FROM t_object WHERE Object_Type = 'Package' AND Name = @name AND Package_ID = ISNULL(@package_ID, Package_ID)",
                    new Dictionary<string, object>()
                    {
                        { "@name", segment},
                        { "@package_ID", lastKnownId == -1 ? (object)DBNull.Value : lastKnownId}
                    });

                lastKnownId = int.Parse((string)segmentIdTTbl.Rows[0][0]);
                if (knownPrefix.Length > 0)
                {
                    knownPrefix += '/';
                }
                knownPrefix += segment;
                remainingPath = path.Remove(0, knownPrefix.Length).TrimStart('/');
            }

            return lastKnownId;
        }

        public EA.Package GetPackage(string path)
        {
            var id = GetPackageId(path);
            var pkg = _repository.GetPackageById(id);
            return pkg;
        }

        public DataTable ExecuteSelectStatement(string sql, Dictionary<string, object> parameters)
        {
            return _db.ExecuteSelectStatement(sql, parameters);
        }
        public DataTable ExecuteSelectStatement(string sql)
        {
            return _db.ExecuteSelectStatement(sql);
        }

        public int GetElementIdByName(int packageId, string name)
        {
            var dbRes = _db.ExecuteSelectStatement("SELECT Object_ID FROM t_object WHERE Package_ID = @packageId AND Name = @name",
                new Dictionary<string, object>()
            {
                { "packageId", packageId },
                { "name", name}
            });

            if (dbRes.Rows.Count == 0)
            {
                return -1;
            }
            return (int)dbRes.Rows[0][0];
        }

        public int GetElementIdByAttributeValue(int packageId, string attributeName, object attributeValue)
        {
            var dbRes = _db.ExecuteSelectStatement(
                @"SELECT
c.Object_ID
FROM t_object c 
INNER JOIN t_attribute att ON att.Object_ID = c.Object_ID
WHERE c.Package_ID = @packageId
    AND att.Name = @attributeName
    AND att.[Default] = @attributeValue
", new Dictionary<string, object>()
                {
                { "packageId", packageId},
                { "attributeName", attributeName},
                { "attributeValue", attributeValue}
                });
            if (dbRes.Rows.Count == 0)
            {
                return -1;
            }
            return (int)dbRes.Rows[0][0];
        }

        public EA.Element GetElementById(int id)
        {
            return _repository.GetElementById(id);
        }

        public EA.Element GetElementByName(int packageId, string name)
        {
            var id = GetElementIdByName(packageId, name);
            if (id == -1)
            {
                return null;
            }
            return _repository.GetElementById(id);
        }
        
        public EA.Element GetElementByAttributeValue(int packageId, string attributeName, object attributeValue)
        {
            var id = GetElementIdByAttributeValue(packageId, attributeName, attributeValue);
            if (id == -1)
            {
                return null;
            }
            return _repository.GetElementById(id);
        }
    }
}
