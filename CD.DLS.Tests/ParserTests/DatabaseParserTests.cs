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
using CD.DLS.Model.Mssql.Db;
using CD.DLS.Model.Interfaces;
using CD.DLS.RequestProcessor.ModelUpdate;

namespace CD.DLS.Tests.ParserTests
{
    /// <summary>
    /// Summary description for PackageParserTests
    /// </summary>
    [TestClass]
    public class DatabaseParserTests
    {
        public DatabaseParserTests()
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
        public void Parse_SampleDWH_ParseSampleDWH()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");

            
            var projectConfig = pcm.GetProjectConfig(new Guid("4290174E-AA85-4704-A4BA-DD910C1A0850"));

            ParseDatabase(projectConfig.DatabaseComponents.First(x => x.DbName == "SampleDWH"), new Guid("BE0897CC-70D8-4290-A72B-94ACE1B26475"), stageManager, projectConfig);
        }

        [TestMethod]
        public void Parse_SampleDWH_MIS_TEST()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");


            var projectConfig = pcm.GetProjectConfig(new Guid("B2E3EDD2-CF61-459A-A13B-164AB90B2546"));

            ParseDatabase(projectConfig.DatabaseComponents.First(x => x.DbName == "MIS_DB_TEST"), new Guid("6ABDD092-CFDB-4AC5-8845-B9E1074CADD5"), stageManager, projectConfig);
        }


        [TestMethod]
        public void Parse_CPI_p_GetGeneralLedger()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=cpicustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");


            var projectConfig = pcm.GetProjectConfig(new Guid("FD1312FC-1182-4B9C-82D9-2089F3468BFB"));

            ParseDatabase(projectConfig.DatabaseComponents.First(x => x.DbName == "CPI_DSA_PP"), new Guid("BE0897CC-70D8-4290-A72B-94ACE1B26475"), stageManager, projectConfig, "p_GetGeneralLedger");
        }

        [TestMethod]
        public void Parse_CPI_DSA_PP()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=cpicustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");


            var projectConfig = pcm.GetProjectConfig(new Guid("FD1312FC-1182-4B9C-82D9-2089F3468BFB"));

            ParseDatabase(projectConfig.DatabaseComponents.First(x => x.DbName == "CPI_DSA_PP"), new Guid("BE0897CC-70D8-4290-A72B-94ACE1B26475"), stageManager, projectConfig);
        }

        private DatabaseElement ParseDatabase(MssqlDbProjectComponent dbComponent, Guid extractId, StageManager stageManager, ProjectConfig projectConfig, string limitToObjectName = null)
        {
            var solutionModel = new Model.SolutionModelElement(new RefPath(""), "");

            var serverElement = new Model.Mssql.Db.ServerElement(UrnBuilder.GetServerUrn(dbComponent.ServerName), dbComponent.ServerName);
            serverElement.Parent = solutionModel;
            solutionModel.AddChild(serverElement);


            ParseSqlDatabaseShallowRequestProcessor shallowParser = new ParseSqlDatabaseShallowRequestProcessor();

            
            var existentServerElement = solutionModel.DbServers.First(x => x.Caption == dbComponent.ServerName);

            var dbElement = new Model.Mssql.Db.DatabaseElement(UrnBuilder.GetDbUrn(existentServerElement.RefPath, dbComponent.DbName), dbComponent.DbName, existentServerElement);
            existentServerElement.AddChild(dbElement);

            // need to save schemas before the parts due to race conditions
            var schemas = stageManager.GetExtractItems(extractId, dbComponent.MssqlDbProjectComponentId, DAL.Objects.Extract.ExtractTypeEnum.SqlSchema);
            foreach (DAL.Objects.Extract.SqlSchema schema in schemas)
            {
                var schemaElement = new SchemaElement(DbModelParserBase.RefPathFor(schema.Urn), schema.Name, dbElement);
                dbElement.AddChild(schemaElement);
            }

            var procedures = stageManager.GetExtractItems(extractId, dbComponent.MssqlDbProjectComponentId,
                            DAL.Objects.Extract.ExtractTypeEnum.SqlProcedure);
            var scalarUdfs = stageManager.GetExtractItems(extractId, dbComponent.MssqlDbProjectComponentId,
                            DAL.Objects.Extract.ExtractTypeEnum.SqlScalarUdf);
            var tables = stageManager.GetExtractItems(extractId, dbComponent.MssqlDbProjectComponentId,
                            DAL.Objects.Extract.ExtractTypeEnum.SqlTable);
            var tableTypes = stageManager.GetExtractItems(extractId, dbComponent.MssqlDbProjectComponentId,
                            DAL.Objects.Extract.ExtractTypeEnum.SqlTableType);
            var tableUdfs = stageManager.GetExtractItems(extractId, dbComponent.MssqlDbProjectComponentId,
                            DAL.Objects.Extract.ExtractTypeEnum.SqlTableUdf);
            var views = stageManager.GetExtractItems(extractId, dbComponent.MssqlDbProjectComponentId,
                            DAL.Objects.Extract.ExtractTypeEnum.SqlView);

            var extractItems = procedures.Union(scalarUdfs).Union(tables).Union(tableTypes)
                .Union(tableUdfs).Union(views);

            foreach (SmoObject extractItem in extractItems)
            {
                ConfigManager.Log.Important("Parsing " + extractItem.Urn);

                var schemaElement = dbElement.SchemaByName(extractItem.SchemaName);
                DbModelElement objectElement = shallowParser.ParseSqlObjectShallow(extractItem, schemaElement);

                //sh.SaveModelPart(objectElement, premappedModel);

            }
            
            ConfigManager.Log.Important(string.Format("SQL Parser - creating DB index"));


            ReferrableIndexBuilder rib = new ReferrableIndexBuilder();
            var ix = rib.BuildIndex(new List<Model.Mssql.Db.ServerElement>() { serverElement });
            ix.SetContextServer(dbComponent.ServerName);
            ix.PremappedIds = rib.ElementIdMap;
            
            
            foreach (SmoObject extractObject in extractItems)
            {
                if (limitToObjectName != null)
                {
                    if (extractObject.ObjectName != limitToObjectName)
                    {
                        continue;
                    }
                }

                ConfigManager.Log.Important(string.Format("Parsing SQL scripts from {0}", extractObject.Urn));


                var objectRefPath = DbModelParserBase.RefPathFor(extractObject.Urn).Path;

                var schemaModel = dbElement.SchemaByName(extractObject.SchemaName);

                var modelElement = schemaModel.Children.First(x => x.RefPath.Path == objectRefPath);
                
                LocalDeepModelParser ldmp = new LocalDeepModelParser(projectConfig, dbComponent.ServerName);
                ldmp.ExtractModel(extractObject, dbComponent.ServerName, ix, modelElement);

                
            }


            return dbElement;


        }
    }
}
