//using CD.BIDoc.Core.Interfaces;
//using CD.Framework.Common.Interfaces;
//using CD.Framework.Common.Storage.FileSystem;
//using CD.Framework.Common.Structures;
//using Microsoft.SqlServer.Management.Common;
//using Microsoft.SqlServer.Management.Smo;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CD.BIDoc.Core.Extract.Mssql.Db
//{
//    /// <summary>
//    /// Filter of databases that should be extracted.
//    /// </summary>
//    public interface IDatabaseFilter
//    {
//        IEnumerable<MssqlDbProjectComponent> EnumerateDatabases(/*Server server*/);
//    }

//    /// <summary>
//    /// Database filter matching all databases.
//    /// </summary>
//    //class AllDatabasesFilter : IDatabaseFilter
//    //{
//    //    public IEnumerable<Tuple<Database, string>> EnumerateDatabases(/*Server server*/)
//    //    {
//    //        return server.Databases.Cast<Database>().Select(db => new Tuple<Database, string>(db, db.Name));
//    //    }
//    //}

//    /// <summary>
//    /// Database filter matching the provided list of names.
//    /// </summary>
//    public class NameListDatabaseFilter : IDatabaseFilter
//    {
//        private readonly List<MssqlDbProjectComponent> _dbs;

//        public NameListDatabaseFilter(MssqlDbProjectComponent[] dbs):
//            this((IEnumerable<MssqlDbProjectComponent>)dbs)
//        {
//        }
//        public NameListDatabaseFilter(IEnumerable<MssqlDbProjectComponent> dbs)
//        {
//            this._dbs = new List<MssqlDbProjectComponent>(dbs);
//        }

//        public IEnumerable<MssqlDbProjectComponent> EnumerateDatabases()
//        {
//            return _dbs;
//            //return _dbs.Select(name => new Tuple<Database, string>(server.Databases[name], name));
//        }

//    }

//    /// <summary>
//    /// Settings for DB model extract.
//    /// </summary>
//    public interface IDbModelExtractSettings : IModelExtractSettings
//    {
//        /// <summary>
//        /// Which databases should be extracted.
//        /// </summary>
//        IDatabaseFilter DatabaseFilter { get; }
//    }

//    /// <summary>
//    /// Stores settings for DB model extract.
//    /// </summary>
//    public class ModelExtractSettings : IDbModelExtractSettings
//    {
//        /// <summary>
//        /// Log for messages during extraction.
//        /// </summary>
//        public ILogger Log { get; set; }
//        /// <summary>
//        /// Which databases should be extracted.
//        /// </summary>
//        public IDatabaseFilter DatabaseFilter { get; set; }
//        /// <summary>
//        /// Name of the server to extract from.
//        /// </summary>
//        public string ServerName { get; set; }

//        public ModelExtractSettings() { }
//        public ModelExtractSettings(string serverName, IDatabaseFilter databaseFilter, ILogger log = null)
//        {
//            ServerName = serverName;
//            DatabaseFilter = databaseFilter;
//            Log = log;
//            if (log == null)
//            {
//                Log = new FileLogger();
//            }
//        }

//    }
//}
