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
using CD.DLS.Parse.Mssql.Ssis;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Misc;
using CD.DLS.RequestProcessor.ModelUpdate;

namespace CD.DLS.Tests.ParserTests
{
    /// <summary>
    /// Summary description for PackageParserTests
    /// </summary>
    [TestClass]
    public class PackageParserTests
    {
        public PackageParserTests()
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
        public void Parse_SampleDWH_XML()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Data Source=localhost;Initial Catalog=DLS;Integrated Security=True;Pooling=False");
            GraphManager graphManager = new GraphManager(nb);
            graphManager.ClearModelPartWithAggregations(new Guid("A4D16D79-497C-4F6B-B816-D7AABB17A67B"), "IntegrationServices");
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");
            var projectConfig = pcm.GetProjectConfig(new Guid("A4D16D79-497C-4F6B-B816-D7AABB17A67B"));
            var sh = new Model.Serialization.SerializationHelper(projectConfig, graphManager);
            Parse.Mssql.Db.AvailableDatabaseModelIndex adbix = new Parse.Mssql.Db.AvailableDatabaseModelIndex(projectConfig, graphManager);

            UpdateModelRequest request = new UpdateModelRequest()
            {
                ExtractId = new Guid("34C95EC6-76B4-4113-81D3-7B4D989E4EA1")
                //ExtractId = new Guid("EF920A55-0D03-49C4-83C3-F9CB638585C5")
            };

            UpdateModelRequestDirectProcessor processor = new UpdateModelRequestDirectProcessor();
            processor.InitWithNb(nb);
            processor.ParseSsis(projectConfig, sh, adbix, request);

        }

        [TestMethod]
        public void Parse_BE_XML()
        {
            ConfigManager.DeploymentMode = DeploymentModeEnum.OnPremises;
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Data Source=localhost;Initial Catalog=DLS;Integrated Security=True;Pooling=False");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");
            var projectConfig = pcm.GetProjectConfig(new Guid("9640AE14-1C5A-473E-88E6-2D0D231556B9"));
            var sh = new Model.Serialization.SerializationHelper(projectConfig, graphManager);
            Parse.Mssql.Db.AvailableDatabaseModelIndex adbix = new Parse.Mssql.Db.AvailableDatabaseModelIndex(projectConfig, graphManager);

            UpdateModelRequest request = new UpdateModelRequest()
            {
                ExtractId = new Guid("4A4BF35A-8602-4B09-B3B7-77FC312E4B47")
            };

            UpdateModelRequestDirectProcessor processor = new UpdateModelRequestDirectProcessor();
            processor.InitWithNb(nb);
            processor.ParseSsis(projectConfig, sh, adbix, request);

        }

        [TestMethod]
        public void Parse_SampleDWH_FactGeneralLedger()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");


            ParseSsisPackagesDeepRequest request = new ParseSsisPackagesDeepRequest()
            {
                ProjectRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Sample_SSIS']/Project[@Name='DWH']",
                ServerRefPath = "IntegrationServices[@Name='DWH-SERVER']",
                FolderRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Sample_SSIS']",
                ExtractId = new Guid("ED6EDF9F-9858-4DEC-BE7C-F2A65C25233A"),
                SsisComponentId = 8
            };

            ParseSsisPackageItem packageItem = new ParseSsisPackageItem()
            {
                PackageExractItemId = 2478,
                XmlExtractItemId = 2278,
                PackageRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Sample_SSIS']/Project[@Name='DWH']/Package[@Name='LoadHelios_FactGeneralLedger.dtsx']"
            };
            
            var projectConfig = pcm.GetProjectConfig(new Guid("4290174E-AA85-4704-A4BA-DD910C1A0850"));

            ParsePackage(request, packageItem, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_SampleDWH_DimEmployee()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");


            ParseSsisPackagesDeepRequest request = new ParseSsisPackagesDeepRequest()
            {
                ProjectRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Sample_SSIS']/Project[@Name='DWH']",
                ServerRefPath = "IntegrationServices[@Name='DWH-SERVER']",
                FolderRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Sample_SSIS']",
                ExtractId = new Guid("0CCEADC9-AE37-4776-B338-79748B7FAAEE"), 
                SsisComponentId = 8
            };

            ParseSsisPackageItem packageItem = new ParseSsisPackageItem()
            {
                PackageExractItemId = 604, 
                XmlExtractItemId = 404, 
                PackageRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Sample_SSIS']/Project[@Name='DWH']/Package[@Name='LoadHelios_DimEmployee.dtsx']" /*DOPLNIT */
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("4290174E-AA85-4704-A4BA-DD910C1A0850"));

            ParsePackage(request, packageItem, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_SampleDWH_DimOrganization()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");


            ParseSsisPackagesDeepRequest request = new ParseSsisPackagesDeepRequest()
            {
                ProjectRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Sample_SSIS']/Project[@Name='DWH']",
                ServerRefPath = "IntegrationServices[@Name='DWH-SERVER']",
                FolderRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Sample_SSIS']",
                ExtractId = new Guid("C4113647-97E9-4CC4-B639-368C326CAC34"),
                SsisComponentId = 8
            };

            ParseSsisPackageItem packageItem = new ParseSsisPackageItem()
            {
                PackageExractItemId = 607,
                XmlExtractItemId = 407,
                PackageRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Sample_SSIS']/Project[@Name='DWH']/Package[@Name='LoadHelios_DimOrganization.dtsx']"
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("4290174E-AA85-4704-A4BA-DD910C1A0850"));

            ParsePackage(request, packageItem, projectConfig, graphManager, stageManager);
        }


        [TestMethod]
        public void Parse_SampleDWH_FACTOGMSegment()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");


            ParseSsisPackagesDeepRequest request = new ParseSsisPackagesDeepRequest()
            {
                ProjectRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Sample_SSIS']/Project[@Name='DWH']",
                ServerRefPath = "IntegrationServices[@Name='DWH-SERVER']",
                FolderRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Sample_SSIS']",
                ExtractId = new Guid("C4113647-97E9-4CC4-B639-368C326CAC34"), /*DOPLNIT - asi ok */
                SsisComponentId = 8
            };

            ParseSsisPackageItem packageItem = new ParseSsisPackageItem()
            {
                PackageExractItemId = 1575, /* ssis package */
                XmlExtractItemId = 1375, /*ssis package file */
                PackageRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Sample_SSIS']/Project[@Name='DWH']/Package[@Name='LoadHelios_FactOGMSegment.dtsx']" /*DOPLNIT */
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("4290174E-AA85-4704-A4BA-DD910C1A0850"));

            ParsePackage(request, packageItem, projectConfig, graphManager, stageManager);
        }


        [TestMethod]
        public void Parse_NRWH_RW_Contract()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");


            ParseSsisPackagesDeepRequest request = new ParseSsisPackagesDeepRequest()
            {
                ProjectRefPath = "IntegrationServices[@Name='FSCZPRCT0041']/Catalog/Folder[@Name='NDWH_SSIS']/Project[@Name='NRWH_SSIS']",
                ServerRefPath = "IntegrationServices[@Name='FSCZPRCT0041']",
                FolderRefPath = "IntegrationServices[@Name='FSCZPRCT0041']/Catalog/Folder[@Name='NDWH_SSIS']",
                ExtractId = new Guid("058DD781-1457-43E3-A13E-600945897707"), /*DOPLNIT - asi ok */
                SsisComponentId = 30
            };

            ParseSsisPackageItem packageItem = new ParseSsisPackageItem()
            {
                PackageExractItemId = 6421, /* ssis package */
                XmlExtractItemId = 6348, /*ssis package file */
                PackageRefPath = "IntegrationServices[@Name='FSCZPRCT0041']/Catalog/Folder[@Name='NDWH_SSIS']/Project[@Name='NRWH_SSIS']/Package[@Name='RW_Contract.dtsx']" /*DOPLNIT */
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("18258D08-66CC-4B97-A695-226C7BA64AFE"));

            ParsePackage(request, packageItem, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_MP_Extract_Forecast()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=manpowercustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");


            ParseSsisPackagesDeepRequest request = new ParseSsisPackagesDeepRequest()
            {
                ProjectRefPath = "IntegrationServices[@Name='CZCS-DW-PREPROD']/Catalog/Folder[@Name='Manpower_SSIS']/Project[@Name='Manpower_SSIS']",
                ServerRefPath = "IntegrationServices[@Name='CZCS-DW-PREPROD']",
                FolderRefPath = "IntegrationServices[@Name='CZCS-DW-PREPROD']/Catalog/Folder[@Name='Manpower_SSIS']",
                ExtractId = new Guid("1488FD55-AE2C-4AAA-B5B9-6388C432969D"), /*DOPLNIT - asi ok */
                SsisComponentId = 1
            };

            ParseSsisPackageItem packageItem = new ParseSsisPackageItem()
            {
                PackageExractItemId = 821, /* ssis package */
                XmlExtractItemId = 614, /*ssis package file */
                PackageRefPath = "IntegrationServices[@Name='CZCS-DW-PREPROD']/Catalog/Folder[@Name='Manpower_SSIS']/Project[@Name='Manpower_SSIS']/Package[@Name='Extract_forecast.dtsx']" /*DOPLNIT */
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("7395C65E-05ED-413A-85E0-75A427BC1FE9"));

            ParsePackage(request, packageItem, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_MP_LoadHelios_FactSalesPipeline()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");


            ParseSsisPackagesDeepRequest request = new ParseSsisPackagesDeepRequest()
            {
                ProjectRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Manpower_SSIS']/Project[@Name='DWH']",
                ServerRefPath = "IntegrationServices[@Name='DWH-SERVER']",
                FolderRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Manpower_SSIS']",
                ExtractId = new Guid("4846DFB9-C49D-4630-A19E-E78F62C99B4D"), 
                SsisComponentId = 40
            };

            ParseSsisPackageItem packageItem = new ParseSsisPackageItem()
            {
                PackageExractItemId = 772, /* ssis package */
                XmlExtractItemId = 565, /*ssis package file */
                PackageRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Manpower_SSIS']/Project[@Name='DWH']/Package[@Name='LoadHelios_FactSalesPipeline.dtsx']" /*DOPLNIT */
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("0A604752-49AD-4C15-A45F-D8E5F0F91ECC"));

            ParsePackage(request, packageItem, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_MP_LoadHelios_FactWages()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");


            ParseSsisPackagesDeepRequest request = new ParseSsisPackagesDeepRequest()
            {
                ProjectRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Manpower_SSIS']/Project[@Name='DWH']",
                ServerRefPath = "IntegrationServices[@Name='DWH-SERVER']",
                FolderRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Manpower_SSIS']",
                ExtractId = new Guid("A97324FC-2859-42D0-8910-75108B5B8F95"),
                SsisComponentId = 40
            };

            ParseSsisPackageItem packageItem = new ParseSsisPackageItem()
            {
                PackageExractItemId = 768, /* ssis package */
                XmlExtractItemId = 561, /*ssis package file */
                PackageRefPath = "IntegrationServices[@Name='DWH-SERVER']/Catalog/Folder[@Name='Manpower_SSIS']/Project[@Name='DWH']/Package[@Name='LoadHelios_FactWages.dtsx']" /*DOPLNIT */
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("0A604752-49AD-4C15-A45F-D8E5F0F91ECC"));

            ParsePackage(request, packageItem, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_CPI_DW_DimCostCentre()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=cpicustdb;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=cpicustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");


            ParseSsisPackagesDeepRequest request = new ParseSsisPackagesDeepRequest()
            {
                ProjectRefPath = "IntegrationServices[@Name='CZSTRBI01']/Catalog/Folder[@Name='CPI_SSIS']/Project[@Name='CPI_SSIS']",
                ServerRefPath = "IntegrationServices[@Name='CZSTRBI01']",
                FolderRefPath = "IntegrationServices[@Name='CZSTRBI01']/Catalog/Folder[@Name='CPI_SSIS']",
                ExtractId = new Guid("01C611D1-1DAE-4EAD-9603-EF061FD747F9"),
                SsisComponentId = 32
            };

            ParseSsisPackageItem packageItem = new ParseSsisPackageItem()
            {
                PackageExractItemId = 1996, /* ssis package */
                XmlExtractItemId = 1256, /*ssis package file */
                PackageRefPath = "IntegrationServices[@Name='CZSTRBI01']/Catalog/Folder[@Name='CPI_SSIS']/Project[@Name='CPI_SSIS']/Package[@Name='DW_DimCostCentre.dtsx']" /*DOPLNIT */
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("FD1312FC-1182-4B9C-82D9-2089F3468BFB"));

            ParsePackage(request, packageItem, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_NRWH_RW_Contract2()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=samplecustdb;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");


            ParseSsisPackagesDeepRequest request = new ParseSsisPackagesDeepRequest()
            {
                ProjectRefPath = "IntegrationServices[@Name='10.41.57.190']/Catalog/Folder[@Name='NDWH_SSIS']/Project[@Name='NRWH_SSIS']",
                ServerRefPath = "IntegrationServices[@Name='10.41.57.190']",
                FolderRefPath = "IntegrationServices[@Name='10.41.57.190']/Catalog/Folder[@Name='NDWH_SSIS']",
                ExtractId = new Guid("782D19B3-08F8-40ED-9C74-283A4A45AC52"),
                SsisComponentId = 45
            };

            ParseSsisPackageItem packageItem = new ParseSsisPackageItem()
            {
                PackageExractItemId = 1111, /* ssis package */
                XmlExtractItemId = 1038, /*ssis package file */
                PackageRefPath = "IntegrationServices[@Name='10.41.57.190']/Catalog/Folder[@Name='NDWH_SSIS']/Project[@Name='NRWH_SSIS']/Package[@Name='RW_Contract.dtsx']" /*DOPLNIT */
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("56F38C2C-5D14-4767-BA6E-02FFE1E44919"));

            ParsePackage(request, packageItem, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_NRWH_RW_InvoiceReceived()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=samplecustdb;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");


            ParseSsisPackagesDeepRequest request = new ParseSsisPackagesDeepRequest()
            {
                ProjectRefPath = "IntegrationServices[@Name='10.41.57.190']/Catalog/Folder[@Name='NDWH_SSIS']/Project[@Name='NRWH_SSIS']",
                ServerRefPath = "IntegrationServices[@Name='10.41.57.190']",
                FolderRefPath = "IntegrationServices[@Name='10.41.57.190']/Catalog/Folder[@Name='NDWH_SSIS']",
                ExtractId = new Guid("782D19B3-08F8-40ED-9C74-283A4A45AC52"),
                SsisComponentId = 45
            };

            ParseSsisPackageItem packageItem = new ParseSsisPackageItem()
            {
                PackageExractItemId = 1056, /* ssis package */
                XmlExtractItemId = 983, /*ssis package file */
                PackageRefPath = "IntegrationServices[@Name='10.41.57.190']/Catalog/Folder[@Name='NDWH_SSIS']/Project[@Name='NRWH_SSIS']/Package[@Name='RW_InvoiceReceived.dtsx']" /*DOPLNIT */
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("56F38C2C-5D14-4767-BA6E-02FFE1E44919"));

            ParsePackage(request, packageItem, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_CPI_DWFramy_DimPartner()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=cpicustdb;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=cpicustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");


            ParseSsisPackagesDeepRequest request = new ParseSsisPackagesDeepRequest()
            {
                ProjectRefPath = "IntegrationServices[@Name='CZSTRDWHSQL01']/Catalog/Folder[@Name='CPI_SSIS']/Project[@Name='CPI_SSIS']",
                ServerRefPath = "IntegrationServices[@Name='CZSTRDWHSQL01']",
                FolderRefPath = "IntegrationServices[@Name='CZSTRDWHSQL01']/Catalog/Folder[@Name='CPI_SSIS']",
                ExtractId = new Guid("590FD312-BE54-4A05-9290-F8FF3A3146E9"),
                SsisComponentId = 38
            };

            ParseSsisPackageItem packageItem = new ParseSsisPackageItem()
            {
                PackageExractItemId = 2675, /* ssis package */
                XmlExtractItemId = 1868, /*ssis package file */
                PackageRefPath = "IntegrationServices[@Name='CZSTRDWHSQL01']/Catalog/Folder[@Name='CPI_SSIS']/Project[@Name='CPI_SSIS']/Package[@Name='DWFarmy_DimPartner.dtsx']" /*DOPLNIT */
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("FD1312FC-1182-4B9C-82D9-2089F3468BFB"));

            ParsePackage(request, packageItem, projectConfig, graphManager, stageManager);
        }

        [TestMethod]
        public void Parse_CPI_DWFramy_FactGeneralLedger()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=cpicustdb;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=cpicustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            StageManager stageManager = new StageManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");


            ParseSsisPackagesDeepRequest request = new ParseSsisPackagesDeepRequest()
            {
                ProjectRefPath = "IntegrationServices[@Name='CZSTRDWHSQL01']/Catalog/Folder[@Name='CPI_SSIS']/Project[@Name='CPI_SSIS']",
                ServerRefPath = "IntegrationServices[@Name='CZSTRDWHSQL01']",
                FolderRefPath = "IntegrationServices[@Name='CZSTRDWHSQL01']/Catalog/Folder[@Name='CPI_SSIS']",
                ExtractId = new Guid("590FD312-BE54-4A05-9290-F8FF3A3146E9"),
                SsisComponentId = 38
            };

            ParseSsisPackageItem packageItem = new ParseSsisPackageItem()
            {
                PackageExractItemId = 2676, /* ssis package */
                XmlExtractItemId = 1869, /*ssis package file */
                PackageRefPath = "IntegrationServices[@Name='CZSTRDWHSQL01']/Catalog/Folder[@Name='CPI_SSIS']/Project[@Name='CPI_SSIS']/Package[@Name='DWFarmy_FactGeneralLedger.dtsx']" /*DOPLNIT */
            };

            var projectConfig = pcm.GetProjectConfig(new Guid("FD1312FC-1182-4B9C-82D9-2089F3468BFB"));

            ParsePackage(request, packageItem, projectConfig, graphManager, stageManager);
        }

        private void ParsePackage(ParseSsisPackagesDeepRequest request, ParseSsisPackageItem item, ProjectConfig projectConfig, GraphManager graphManager, StageManager stageManager)
        {
            Model.Serialization.SerializationHelper serializationHelper = new Model.Serialization.SerializationHelper(projectConfig, graphManager);
            var projectElement = (ProjectElement)serializationHelper.LoadElementModelToChildrenOfType(request.ProjectRefPath, typeof(PackageElement));

            var premappedIds = serializationHelper.CreatePremappedModel(projectElement);

            var foldersModel = (ServerElement)serializationHelper.LoadElementModelToChildrenOfType(request.ServerRefPath, typeof(FolderElement));

            var folderElement = foldersModel.Children.OfType<CatalogElement>().First().Children.OfType<FolderElement>().First(x => x.RefPath.Path == request.FolderRefPath);
            projectElement.Parent = folderElement;

            var projectElementWithCMs = (ProjectElement)serializationHelper.LoadElementModelToChildrenOfType(request.ProjectRefPath, typeof(ConnectionManagerElement));
            var projectCMs = new List<ConnectionManagerElement>();

            if (projectElementWithCMs != null)
            {
                projectElementWithCMs.ConnectionManagers.ToList();
                foreach (var projectCM in projectCMs)
                {
                    var map = serializationHelper.CreatePremappedModel(projectCM);
                    foreach (var mapItem in map)
                    {
                        premappedIds.Add(mapItem.Key, mapItem.Value);
                    }
                }
            }


            var cms = new List<ConnectionManagerElement>();
            if (projectElementWithCMs != null)
            {
                cms = projectElementWithCMs.ConnectionManagers.ToList();
            }

            AvailableDatabaseModelIndex adbix = new AvailableDatabaseModelIndex(projectConfig, graphManager);



            projectElement.Parent = folderElement;

            //var packageExtract = (SsisPackage)stageManager.GetExtractItem(item.PackageExractItemId);
            var xmlExtract = (SsisPackageFile)stageManager.GetExtractItem(item.XmlExtractItemId);
            var packageElement = projectElement.Packages.First(x => xmlExtract.Name.Contains(x.Caption));

            //ConfigManager.Log.Important("Parsing " + packageExtract.Urn + " deep");

            SsisXmlProvider xmlProvider = new SsisXmlProvider(request.ExtractId, request.SsisComponentId, stageManager, xmlExtract);
            ProjectModelParser projectModelParser = new ProjectModelParser(xmlProvider, adbix, projectConfig, request.ExtractId, stageManager);
            var packageXml = xmlProvider.Project.Packages.First(x => x.PackageName == xmlExtract.Name);

            projectModelParser.ParsePackage(request.SsisComponentId, projectElement, packageElement,
                packageXml, cms);

            projectElement.Parent = null;


            var dbIdMap = adbix.GetAllPremappedIds();
            foreach (var kv in dbIdMap)
            {
                if (!premappedIds.ContainsKey(kv.Key))
                {
                    premappedIds.Add(kv.Key, kv.Value);
                }
            }

            serializationHelper.SaveModelPart(packageElement, premappedIds, true);

        }
    }
}
