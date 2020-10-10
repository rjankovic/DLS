using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CD.DLS.Extract.PowerBi.PowerBiAPI;

namespace CD.DLS.Tests.ExtractTests
{
    /// <summary>
    /// Summary description for PBIApiClientTests
    /// </summary>
    [TestClass]
    public class PBIApiClientTests
    {
        public PBIApiClientTests()
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
        public void TestReportListing()
        {
            var pbic = new PBIAPIClient("660e05a5-652d-4ae1-a02c-abb99f694644", "https://login.live.com/oauth20_desktop.srf");

            PBIGroup myGroup = pbic;
            var reports = myGroup.Reports;
        }
    }
}
