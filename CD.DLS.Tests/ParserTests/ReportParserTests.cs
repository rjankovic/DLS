using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Parse.Mssql.Db;
using CD.DLS.Model.Serialization;
using System.Linq;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Parse.Mssql.Ssis;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.Model.Mssql.Ssrs;
using CD.DLS.Parse.Mssql.Ssas;
using CD.DLS.Parse.Mssql.Ssrs;

namespace CD.DLS.Tests.ParserTests
{
    /// <summary>
    /// Summary description for PackageParserTests
    /// </summary>
    [TestClass]
    public class ReportParserTests
    {
        public ReportParserTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Parse_SampleDWH_838()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new DAL.Misc.ConsoleLogger("DLS test");

            ParseSsrsReportRequest request = new ParseSsrsReportRequest()
            {
                ExtractId = new Guid("782D19B3-08F8-40ED-9C74-283A4A45AC52"),
                SsrsComponentId = 95,
                ServerRefPath = "SSRSServer[@Name='10.41.57.190']"
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("56F38C2C-5D14-4767-BA6E-02FFE1E44919"));

            ParseReport(request, 1212, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_SampleDWH_ROIByConsulantSeniority()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new DAL.Misc.ConsoleLogger("DLS test");

            ParseSsrsReportRequest request = new ParseSsrsReportRequest()
            {
                ExtractId = new Guid("EC9F3949-5DF1-4378-8E25-D405B8CADA82"),
                SsrsComponentId = 19,
                ServerRefPath = "SSRSServer[@Name='DWH-SERVER']"
            };
            
            var projectConfig = pcm.GetProjectConfig(new Guid("4290174E-AA85-4704-A4BA-DD910C1A0850"));

            ParseReport(request, 1840, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_SampleDWH_OperaingExpenses()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new DAL.Misc.ConsoleLogger("DLS test");

            ParseSsrsReportRequest request = new ParseSsrsReportRequest()
            {
                ExtractId = new Guid("A93B82EB-8E14-402E-89A2-D8F3224E99E3"),
                SsrsComponentId = 19,
                ServerRefPath = "SSRSServer[@Name='DWH-SERVER']"
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("4290174E-AA85-4704-A4BA-DD910C1A0850"));

            ParseReport(request, 2747, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_CPI_KontrolaNevystavenychFaktur()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=cpicustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new DAL.Misc.ConsoleLogger("DLS test");

            ParseSsrsReportRequest request = new ParseSsrsReportRequest()
            {
                ExtractId = new Guid("4DF4DFE6-5BB7-47FE-BB5C-EC1D7C5E99CD"),
                SsrsComponentId = 10,
                ServerRefPath = "SSRSServer[@Name='INTRANET-TEST.CPI.CZ']"
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("FD1312FC-1182-4B9C-82D9-2089F3468BFB"));

            ParseReport(request, 36835, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_VWFS_PrepaidServices()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new DAL.Misc.ConsoleLogger("DLS test");

            ParseSsrsReportRequest request = new ParseSsrsReportRequest()
            {
                ExtractId = new Guid("6ABDD092-CFDB-4AC5-8845-B9E1074CADD5"),
                SsrsComponentId = 53,
                ServerRefPath = "SSRSServer[@Name='FSCZPRCT0013']"
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("B2E3EDD2-CF61-459A-A13B-164AB90B2546"));

            ParseReport(request, 857, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_AdventureWorks_TabularReport1()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new DAL.Misc.ConsoleLogger("DLS test");

            ParseSsrsReportRequest request = new ParseSsrsReportRequest()
            {
                ExtractId = new Guid("8A4AE151-F043-411F-A7A1-030AEDD9D8FA"),
                SsrsComponentId = 25,
                ServerRefPath = "SSRSServer[@Name='DWH-SERVER']"
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("A8BA999B-9D29-4FDC-8A19-6659E29A17B1"));

            ParseReport(request, 570, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_AdventureWorks_TabularReport_LS1()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new DAL.Misc.ConsoleLogger("DLS test");

            ParseSsrsReportRequest request = new ParseSsrsReportRequest()
            {
                ExtractId = new Guid("43C2E7A1-1CE1-40BC-88B8-370933ABBF38"),
                SsrsComponentId = 25,
                ServerRefPath = "SSRSServer[@Name='DWH-SERVER']"
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("A8BA999B-9D29-4FDC-8A19-6659E29A17B1"));

            ParseReport(request, 3248, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_NRWH_803()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new DAL.Misc.ConsoleLogger("DLS test");

            ParseSsrsReportRequest request = new ParseSsrsReportRequest()
            {
                ExtractId = new Guid("782D19B3-08F8-40ED-9C74-283A4A45AC52"),
                SsrsComponentId = 95,
                ServerRefPath = "SSRSServer[@Name='10.41.57.190']"
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("56F38C2C-5D14-4767-BA6E-02FFE1E44919"));

            ParseReport(request, 1236, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_NRWH_800()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new DAL.Misc.ConsoleLogger("DLS test");

            ParseSsrsReportRequest request = new ParseSsrsReportRequest()
            {
                ExtractId = new Guid("782D19B3-08F8-40ED-9C74-283A4A45AC52"),
                SsrsComponentId = 95,
                ServerRefPath = "SSRSServer[@Name='10.41.57.190']"
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("56F38C2C-5D14-4767-BA6E-02FFE1E44919"));

            ParseReport(request, 1234, projectConfig, graphManager, stageManager);
        }

        private void ParseReport(ParseSsrsReportRequest request, int extractItemId, ProjectConfig projectConfig, 
            GraphManager graphManager, StageManager stageManager)
        {   
            SerializationHelper sh = new SerializationHelper(projectConfig, graphManager);
            var serverFolders = (ServerElement)sh.LoadElementModelToChildrenOfType(request.ServerRefPath, typeof(FolderElement));
            var serverDataSources = (ServerElement)sh.LoadElementModelToChildrenOfType(request.ServerRefPath, typeof(SharedDataSourceElement));
            var serverDataSets = (ServerElement)sh.LoadElementModelToChildrenOfType(request.ServerRefPath, typeof(SharedDataSourceElement));

            AvailableDatabaseModelIndex adbix = new AvailableDatabaseModelIndex(projectConfig, graphManager);
            ConfigManager.Log.Important("Creating SSAS index");
            Parse.Mssql.Ssas.SsasServerIndex ssasIndex = new SsasServerIndex(projectConfig, graphManager);

            ConfigManager.Log.Important("Collecting data sources");
            var rootDataSources = serverDataSources.Children.OfType<SharedDataSourceElement>();
            ConfigManager.Log.Important("Collecting data sets");
            var rootDataSets = serverDataSets.Children.OfType<SharedDataSetElement>();
            var allDataSources = rootDataSources.Union(CollectElements<SharedDataSourceElement>(serverDataSources.Folders)).ToList();
            var allDataSets = rootDataSets.Union(CollectElements<SharedDataSetElement>(serverDataSets.Folders)).ToList();


            var premappedModel = sh.CreatePremappedModel(serverFolders);
            foreach (var dataSource in allDataSources)
            {
                premappedModel.Add(dataSource, dataSource.Id);
            }
            foreach (var dataSet in allDataSets)
            {
                premappedModel.Add(dataSet, dataSet.Id);
            }
            
            SsrsModelExtractor extractor = new SsrsModelExtractor(adbix, ssasIndex, new MdxScriptModelExtractor(),
            projectConfig, request.ExtractId, stageManager);
            extractor.SetFolderIndex(serverFolders.Folders.ToList());
            extractor.SetSharedDataSourceIndex(allDataSources);
            extractor.SetSharedDataSetIndex(allDataSets);

                var report = (SsrsReport)stageManager.GetExtractItem(extractItemId);

                ConfigManager.Log.Important("Parsing " + report.FullPath);

                var ssrsComponent = projectConfig.SsrsComponents.First(x => x.SsrsProjectComponentId == request.SsrsComponentId);
                var reportElement = extractor.LoadReport(report, ssrsComponent);

                foreach (var dbItem in adbix.GetAllPremappedIds())
                {
                    if (!premappedModel.ContainsKey(dbItem.Key))
                    {
                        premappedModel.Add(dbItem.Key, dbItem.Value);
                    }
                }

                var ssasItems = ssasIndex.GetPremappedIds();
                foreach (var ssasItem in ssasItems)
                {
                    if (!premappedModel.ContainsKey(ssasItem.Key))
                    {
                        premappedModel.Add(ssasItem.Key, ssasItem.Value);
                    }
                }

                sh.SaveModelPart(reportElement, premappedModel, true);
            
        }

        private IEnumerable<T> CollectElements<T>(IEnumerable<FolderElement> folders)
        {
            var init = folders.SelectMany(x => x.Children.OfType<T>());
            return init.Union(folders.SelectMany(x => CollectElements<T>(x.Folders)));
        }

 
    }
}
