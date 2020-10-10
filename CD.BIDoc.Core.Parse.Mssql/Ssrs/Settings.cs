//using CD.BIDoc.Core.Interfaces;
//using CD.Framework.Common.Interfaces;
//using Microsoft.AnalysisServices;
//using Microsoft.SqlServer.ReportingServices2010;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace CD.BIDoc.Core.Extract.Mssql.Ssrs
//{
//    public interface ISsrsModelExtractSettings : IModelExtractSettings
//    {
//        ISsrsFolderFilter FolderFilter { get; }
        
//        string ServiceUrl { get; }
//        string ExecutionServiceUrl { get; }
//    }

//    public interface ISsrsFolderFilter
//    {
//        IEnumerable<CatalogItem> EnumerateCatalogItems(ReportingService2010 webService);
//    }



//    public class SsrsFolderFilter : ISsrsFolderFilter
//    {
//        private List<string> _folders = new List<string>();

//        public SsrsFolderFilter(params string[] folders):
//            this((IEnumerable<string>)folders)
//        {
//        }

//        public SsrsFolderFilter(IEnumerable<string> folders)
//        {
//            _folders = folders.ToList();
//        }

//        public void AddFolder(string folder)
//        {
//            _folders.Add(folder);
//        }

//        public IEnumerable<CatalogItem> EnumerateCatalogItems(ReportingService2010 webService)
//        {
//            List<CatalogItem> items = new List<CatalogItem>();
//            foreach (var folder in _folders)
//            {
//                items.AddRange(webService.FindItems(folder, BooleanOperatorEnum.Or, new Property[] { }, new SearchCondition[] { }));
//            }
//            return items;
//        }
//    }

//    public class SsrsModelExtractSettings : IModelExtractSettings
//    {
//        public string ServerName { get; private set; }
//        public ILogger Log { get; private set; }
//        public readonly string ServiceUrl;
//        public readonly string ExecutionServiceUrl;
//        public ISsrsFolderFilter FolderFilter;
//        public readonly Db.IReferrableIndex DbContext;
//        public readonly Db.ISqlScriptModelExtractor DbSqlScriptExtractor;
//        public readonly Ssas.SsasServerIndex SsasContext;
//        public readonly Ssas.MdxScriptModelExtractor MdxExtractor;


//        public SsrsModelExtractSettings(
//            string serviceUrl
//            ,string executionServiceUrl
//            , ISsrsFolderFilter folderFilter
//            , Db.IReferrableIndex dbContextIndex
//            , Db.ISqlScriptModelExtractor dbScriptExtractor
//            , Ssas.SsasServerIndex ssasContext
//            , Ssas.MdxScriptModelExtractor mdxExtractor
//            , string serverName
//            , ILogger log)
//        {
//            ServiceUrl = serviceUrl;
//            ExecutionServiceUrl = executionServiceUrl;
//            FolderFilter = folderFilter;
//            DbContext = dbContextIndex;
//            DbSqlScriptExtractor = dbScriptExtractor;
//            SsasContext = ssasContext;
//            MdxExtractor = mdxExtractor;
//            ServerName = serverName;
//            Log = log;
//        }
//    }



//}
