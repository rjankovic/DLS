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

namespace CD.DLS.Tests.ParserTests
{
    [TestClass]
    public class DaxParserTests
    {
        public DaxParserTests()
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

        private static SsasServerIndex _serverIndex = null;
        private static SsasDatabaseIndex _dbIndex = null;
        private static DaxScriptModelExtractor _daxExtractor = new DaxScriptModelExtractor();

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");

            var projectConfigId = new Guid("AAF22F0C-C222-49E6-B981-346B1B602813");
            var projectConfig = pcm.GetProjectConfig(projectConfigId);

            SsasServerIndex serverIndex = new SsasServerIndex(projectConfig, graphManager);
            var dbIndex = serverIndex.GetDatabase("DESKTOP-ACTSJFM", "AdventureWorksInternetSales");

            _graphManager = graphManager;
            _stageManager = stageManager;
            _projectConfigManager = pcm;
            _projectConfig = projectConfig;

            _serverIndex = serverIndex;
            _dbIndex = dbIndex;
        }

        private DaxScriptElement ParseDaxScript(string script, out Dictionary<string, SsasModelElement> resultColumns)
        {
            Model.SolutionModelElement fakeParent = new Model.SolutionModelElement(new RefPath(), "Root");
            var scriptModel = _daxExtractor.ExtractDaxScript(script, _dbIndex, fakeParent, out resultColumns);
            return scriptModel;
        }

        [TestMethod]
        public void Parse_SummarizeColumns()
        {
            var script =
                @"EVALUATE SUMMARIZECOLUMNS('Customer'[Education], 'Customer'[Last Name], " +
                "\"Days Current Quarter to Date\", [Days Current Quarter to Date], " +
                "\"Days in Current Quarter\", [Days in Current Quarter], " +
                "\"Internet Total Margin\", [Internet Total Margin], " +
                "\"Internet Total Product Cost\", [Internet Total Product Cost])";

            Dictionary<string, SsasModelElement> resultColumns;
            var model = ParseDaxScript(script, out resultColumns);
        }


        [TestMethod]
        public void Parse_SummarizeColumnsFilter()
        {
            var script =
                "EVALUATE SUMMARIZECOLUMNS('Customer'[Education], " +
                "FILTER(VALUES('Customer'[Last Name]), ('Customer'[Last Name] = \"Andersen\") " +
                "|| ('Customer'[Last Name] = \"Anand\") " +
                "|| ('Customer'[Last Name] = \"Alvarez\") " +
                "|| ('Customer'[Last Name] = \"Alonso\")), " +
                "\"Days Current Quarter to Date\", [Days Current Quarter to Date], " +
                "\"Days in Current Quarter\", [Days in Current Quarter], " +
                "\"Internet Total Margin\", [Internet Total Margin], " +
                "\"Internet Total Product Cost\", [Internet Total Product Cost])";

            Dictionary<string, SsasModelElement> resultColumns;
            var model = ParseDaxScript(script, out resultColumns);
        }

        [TestMethod]
        public void Parse_IsLogical()
        {
            var script =
              "EVALUATE "
            + "ADDCOLUMNS "
            + "( "
            + "Date, "
            + "\"isLogical\", ISLOGICAL('Date'[Date]) "
            + ")";

            Dictionary<string, SsasModelElement> resultColumns;
            var model = ParseDaxScript(script, out resultColumns);
        }

        [TestMethod]
        public void Parse_AddColumns()
        {
            var script =
                "EVALUATE " +
                "ADDCOLUMNS('Internet Sales', " +
                "\"Year\", RELATED('Date'[Fiscal Year]), " +
                "\"Color\", RELATED(Product[Color]))";

            Dictionary<string, SsasModelElement> resultColumns;
            var model = ParseDaxScript(script, out resultColumns);
        }

        [TestMethod]
        public void Parse_if()
        {
            var script = "EVALUATE ADDCOLUMNS ( Customer, \"Edu\", IF('Customer'[Education] = \"Bachelors\", \"Bc\", \"NeBc\" ), \"Error\", IFERROR('Customer'[First Name], \"chyba\"))";
               
            Dictionary<string, SsasModelElement> resultColumns;
            var model = ParseDaxScript(script, out resultColumns);
        }

        [TestMethod]
        public void Parse_DefineMeasure()
        {
            var script =
                @"DEFINE
    MEASURE 'Internet Sales'[Total Quantity] =
        CALCULATE(
            SUM('Internet Sales'[Order Quantity]),
            FILTER(
                ALL('Internet Sales'[Product Id]),
                CONTAINS(
                    VALUES(Product[Product Id]),
                    Product[Product Id], 'Internet Sales'[Product Id]
                )
            )
        )
EVALUATE
FILTER(
    ADDCOLUMNS(
        CROSSJOIN(ALL('Date'[Fiscal Year]), ALL(Product[Color])), " +
        "\"Total Quantity\", [Total Quantity] " +
    "), " +
    "NOT ISBLANK( [Total Quantity]) " +
") " +
"ORDER BY 'Date'[Fiscal Year], Product[Color]";

            Dictionary<string, SsasModelElement> resultColumns;
            var model = ParseDaxScript(script, out resultColumns);
        }
    }
}
