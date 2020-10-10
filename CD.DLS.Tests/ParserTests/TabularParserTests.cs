using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Parse.Mssql.Db;
using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.Model.Serialization;
using System.Linq;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Misc;
using CD.DLS.Model.Interfaces;
using CD.DLS.RequestProcessor.ModelUpdate;
using CD.DLS.Model.Mssql.Tabular;

namespace CD.DLS.Tests.ParserTests
{
    /// <summary>
    /// Summary description for PackageParserTests
    /// </summary>
    [TestClass]
    public class TabularParserTests
    {
        public TabularParserTests()
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
        public void Parse_AdventureWorks2014_ParseSampleDWH()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");

            var projectConfigId = new Guid("A8BA999B-9D29-4FDC-8A19-6659E29A17B1");
            var projectConfig = pcm.GetProjectConfig(projectConfigId);
            var extractId = new Guid("6864C9AF-F2D5-47E2-94AD-76574AC21E29");

            ParseDatabase(projectConfig.SsasComponents.First(x => x.DbName == "AdventureWorks"), extractId, 
                stageManager, graphManager, projectConfig);
        }
        
        private SsasTabularDatabaseElement ParseDatabase(SsasDbProjectComponent ssasComponent, Guid extractId, 
            StageManager stageManager, GraphManager graphManager, ProjectConfig projectConfig)
        {

            Parse.Mssql.Db.AvailableDatabaseModelIndex adbix = new Parse.Mssql.Db.AvailableDatabaseModelIndex(projectConfig, graphManager);
            BIDoc.Core.Parse.Mssql.Tabular.TabularParser tparser = new BIDoc.Core.Parse.Mssql.Tabular.TabularParser(adbix, projectConfig, extractId, stageManager);
            Model.Serialization.SerializationHelper sh = new Model.Serialization.SerializationHelper(projectConfig, graphManager);

            var solutionModel = sh.LoadElementModelToChildrenOfType(
                    string.Empty,
                    typeof(Model.Mssql.Ssas.ServerElement)) as Model.SolutionModelElement;

            var premappedModel = sh.CreatePremappedModel(solutionModel);

            Model.Mssql.Ssas.ServerElement serverElement = solutionModel.SsasServers.First(x => x.Caption == ssasComponent.ServerName);


            ConfigManager.Log.Important(string.Format("Parsing tabular database {0}", ssasComponent.DbName));
            var dbExtractT = (TabularDB)(stageManager.GetExtractItems(
            extractId, ssasComponent.SsaslDbProjectComponentId, ExtractTypeEnum.TabularDB)[0]);
            var dbElement = tparser.Parse(ssasComponent.SsaslDbProjectComponentId, ssasComponent.ServerName, serverElement);

            return dbElement;
        }
    }
}
