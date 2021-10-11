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
using CD.DLS.Parse.Mssql.Ssas;
using CD.DLS.Model.Mssql.Ssas;
using CD.DLS.Core.Parse.Mssql.PowerQuery;
using CD.DLS.Model.Mssql.PowerQuery;

namespace CD.DLS.Tests.ParserTests
{
    [TestClass]
    public class PowerQueryParserTests
    {
        public PowerQueryParserTests()
        {
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

        private static GraphManager _graphManager = null;
        private static StageManager _stageManager = null;
        private static ProjectConfigManager _projectConfigManager = null;
        private static ProjectConfig _projectConfig = null;

        private static AvailableDatabaseModelIndex _sqlIndex = null;
        
        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Data Source=localhost;Initial Catalog=DLS;Integrated Security=True;Pooling=False");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");

            var projectConfigId = new Guid("30383D00-3AB1-45A4-9709-643510706AE7");
            var projectConfig = pcm.GetProjectConfig(projectConfigId);

            AvailableDatabaseModelIndex sqlIndex = new AvailableDatabaseModelIndex(projectConfig, graphManager);

            _graphManager = graphManager;
            _stageManager = stageManager;
            _projectConfigManager = pcm;
            _projectConfig = projectConfig;
            _sqlIndex = sqlIndex;
        }

        private PowerQueryElement ParsePowerQuery(string script, out Dictionary<string, OperationOutputColumnElement> resultColumns)
        {
            PowerQueryExtractor extractor = new PowerQueryExtractor(_projectConfig, _graphManager);
            Model.SolutionModelElement fakeParent = new Model.SolutionModelElement(new RefPath(), "Root");
            var scriptModel = extractor.ExtractPowerQuery(script, _sqlIndex, fakeParent, out resultColumns);
            return scriptModel;
        }

        [TestMethod]
        public void Parse_DirectSelect()
        {
            var script =
                "let\n" +
    "Source = Sql.Database(\"localhost\", \"BE\", [Query=\"SELECT* FROM GeneralService_T\"])\n" +
"in\n" +
    "Source";

            Dictionary<string, OperationOutputColumnElement> resultColumns;
            var model = ParsePowerQuery(script, out resultColumns);
        }

        [TestMethod]
        public void Parse_SqlTableRef()
        {
            var script = @"
let
    Source = Sql.Database(""localhost"", ""BE""),
    dbo_GeneralService_T = Source{[Schema= ""dbo"", Item = ""GeneralService_T""]}
            [Data]
in
    dbo_GeneralService_T
";

            Dictionary<string, OperationOutputColumnElement> resultColumns;
            var model = ParsePowerQuery(script, out resultColumns);
        }
    }
}
