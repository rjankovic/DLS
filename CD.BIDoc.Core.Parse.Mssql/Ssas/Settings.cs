//using CD.BIDoc.Core.Interfaces;
//using CD.Framework.Common.Interfaces;
//using Microsoft.AnalysisServices;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace CD.BIDoc.Core.Extract.Mssql.Ssas
//{
//    public interface ISsasModelExtractSettings : IModelExtractSettings
//    {
//        ISsasDbFilter DbFilter { get; }

//    }

//    public interface ISsasDbFilter
//    {
//        IEnumerable<Tuple<Database, string>> EnumerateDatabases(Server server);
//    }



//    public class SsasDbFilter : ISsasDbFilter
//    {
//        private List<string> _ids = new List<string>();

//        public SsasDbFilter(params string[] databases):
//            this((IEnumerable<string>)databases)
//        {
//        }

//        public SsasDbFilter(IEnumerable<string> databases)
//        {
//            _ids = databases.ToList();
//        }

//        public void AddDb(string dbId)
//        {
//            _ids.Add(dbId);
//        }

//        public IEnumerable<Tuple<Database, string>> EnumerateDatabases(Server server)
//        {
//            return _ids.Select(id => new Tuple<Database, string>(server.Databases[id], id));
//        }
//    }

//    public class SsasModelExtractSettings : ISsasModelExtractSettings
//    {

//        public string ServerName { get; private set; }
//        public ISsasDbFilter DbFilter { get; private set; }
//        public readonly Db.IReferrableIndex DbContext;
//        public readonly Db.ISqlScriptModelExtractor DbSqlScriptExtractor;
//        public ILogger Log { get; private set; }


//        public SsasModelExtractSettings(
//            string serverName
//            , ISsasDbFilter dbFilter
//            , Db.IReferrableIndex dbContextIndex
//            , Db.ISqlScriptModelExtractor dbScriptExtractor
//            , ILogger log)
//        {
//            ServerName = serverName;
//            this.DbFilter = dbFilter;
//            DbContext = dbContextIndex;
//            DbSqlScriptExtractor = dbScriptExtractor;
//            Log = log;
//        }
//    }



//}
