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
using CD.DLS.Model.Mssql;
using CD.DLS.Model;

namespace CD.DLS.Tests.ParserTests
{
    /// <summary>
    /// Summary description for PackageParserTests
    /// </summary>
    [TestClass]
    public class SqlParserTests
    {
        public SqlParserTests()
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
        public void Parse_NRWH_Contract_A()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");

            var projectId = new Guid("18258D08-66CC-4B97-A695-226C7BA64AFE");

            var projectConfig = pcm.GetProjectConfig(projectId);

            var query = "SELECT * FROM F_ICMSTB_TC";

            ParseSqlQuery(projectConfig.DatabaseComponents.First(x => x.DbName == "NDWH_L1"), projectConfig, graphManager, query);
        }

        [TestMethod]
        public void Parse_NRWH_Contract_C()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");

            var projectId = new Guid("18258D08-66CC-4B97-A695-226C7BA64AFE");

            var projectConfig = pcm.GetProjectConfig(projectId);

            var query = @"select *, ROW_NUMBER() OVER(PARTITION BY ISERL ORDER BY IDFUNC DESC) RN, 0 IsApplication from F_ICMSTB_TC union all
select *, ROW_NUMBER() OVER(PARTITION BY ISERL ORDER BY IDFUNC DESC) RN, 1 IsApplication from F_ICMAST_TC
";

            ParseSqlQuery(projectConfig.DatabaseComponents.First(x => x.DbName == "NDWH_L1"), projectConfig, graphManager, query);
        }

        [TestMethod]
        public void Parse_NRWH_Contract_B()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");

            var projectId = new Guid("18258D08-66CC-4B97-A695-226C7BA64AFE");

            var projectConfig = pcm.GetProjectConfig(projectId);

            var query = "; WITH x AS(SELECT * FROM F_ICMSTB_TC) SELECT * FROM x";

            ParseSqlQuery(projectConfig.DatabaseComponents.First(x => x.DbName == "NDWH_L1"), projectConfig, graphManager, query);
        }


        [TestMethod]
        public void Parse_NRWH_Contract_D()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");

            var projectId = new Guid("18258D08-66CC-4B97-A695-226C7BA64AFE");

            var projectConfig = pcm.GetProjectConfig(projectId);

            var query = @"INSERT INTO Temp_Contract_X select *, ROW_NUMBER() OVER(PARTITION BY ISERL ORDER BY IDFUNC DESC) RN, 0 IsApplication from F_ICMSTB_TC
";

            ParseSqlQuery(projectConfig.DatabaseComponents.First(x => x.DbName == "NDWH_L1"), projectConfig, graphManager, query);
        }


        [TestMethod]
        public void Parse_NRWH_Contract_E()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");

            var projectId = new Guid("18258D08-66CC-4B97-A695-226C7BA64AFE");

            var projectConfig = pcm.GetProjectConfig(projectId);

            var query = @"
;
with x as (
select *, ROW_NUMBER() OVER(PARTITION BY ISERL ORDER BY IDFUNC DESC) RN, 0 IsApplication from F_ICMSTB_TC union all
select *, ROW_NUMBER() OVER(PARTITION BY ISERL ORDER BY IDFUNC DESC) RN, 1 IsApplication from F_ICMAST_TC
)
INSERT INTO Temp_Contract_X SELECT * FROM x
";

            ParseSqlQuery(projectConfig.DatabaseComponents.First(x => x.DbName == "NDWH_L1"), projectConfig, graphManager, query);
        }

        [TestMethod]
        public void Parse_NRWH_Contract_F()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");

            var projectId = new Guid("18258D08-66CC-4B97-A695-226C7BA64AFE");

            var projectConfig = pcm.GetProjectConfig(projectId);

            var query = @"
;
with x as (
select *, ROW_NUMBER() OVER(PARTITION BY ISERL ORDER BY IDFUNC DESC) RN, 0 IsApplication from F_ICMSTB_TC
)
INSERT INTO Temp_Contract_X SELECT * FROM x
";

            ParseSqlQuery(projectConfig.DatabaseComponents.First(x => x.DbName == "NDWH_L1"), projectConfig, graphManager, query);
        }

        [TestMethod]
        public void Parse_NRWH_Contract_Merge_A()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");

            var projectId = new Guid("18258D08-66CC-4B97-A695-226C7BA64AFE");

            var projectConfig = pcm.GetProjectConfig(projectId);

            var query = @"
;
MERGE [Contract] t
USING [temp].[Contract_MergeA]  s ON 
                t.[ContractNumber] = s.[ContractNumber]
WHEN MATCHED
THEN
UPDATE SET
                  t.[ClientNumber] = s.[ClientNumber]
      ,t.[DepositAdvance] = s.[DepositAdvance]
      ,t.[Price] = s.[Price]
      ,t.[DealerCommission] = s.[DealerCommission]
      ,t.[InternalStatus]  = s.[InternalStatus]
      ,t.[InternalStatusCode] = s.[InternalStatusCode]
      ,t.[MarketingReference] = s.[MarketingReference]
      ,t.[GeneralConditions] = s.[GeneralConditions]
      ,t.[ContractStatus] = s.[ContractStatus]
      ,t.[ApplicationStatus] = s.[ApplicationStatus]
      ,t.[MarketingReferenceDescription] = s.[MarketingReferenceDescription]
      ,t.[ExpiryDate] = s.[ExpiryDate]
      ,t.[Age] = s.[Age]
      ,t.[StatusCode] = s.[StatusCode]
      ,t.[Length] = s.[Length]
      ,t.[ContractFrom] = s.[ContractFrom]
      ,t.[ContractTo] = s.[ContractTo]
      ,t.[DriverName] = s.[DriverName]
      ,t.[OpenCalculationPeriod] = s.[OpenCalculationPeriod]
      ,t.[ProductVariantCode] = s.[ProductVariantCode]
      ,t.[ProductVariantName] = s.[ProductVariantName]
      ,t.[ProductTypeCode] = s.[ProductTypeCode]
      ,t.[ProductTypeName] = s.[ProductTypeName]
      ,t.[StatusWFlag] = s.[StatusWFlag]
                  ,t.[MarketingReferenceSegment] = s.[MarketingReferenceSegment]
WHEN NOT MATCHED BY TARGET
THEN
INSERT
(
                   [ContractNumber]
      ,[ClientNumber]
      ,[DepositAdvance]
      ,[Price]
      ,[DealerCommission]
      ,[InternalStatus]
      ,[InternalStatusCode]
      ,[MarketingReference]
      ,[GeneralConditions]
      ,[ContractStatus]
      ,[ApplicationStatus]
      ,[MarketingReferenceDescription]
      ,[ExpiryDate]
      ,[Age]
      ,[StatusCode]
      ,[Length]
      ,[ContractFrom]
      ,[ContractTo]
      ,[DriverName]
,[OpenCalculationPeriod]
      ,[ProductVariantCode]
      ,[ProductVariantName]
      ,[ProductTypeCode]
      ,[ProductTypeName]
,[StatusWFlag]
,[MarketingReferenceSegment]
)
VALUES
(
                   [ContractNumber]
      ,[ClientNumber]
      ,[DepositAdvance]
      ,[Price]
      ,[DealerCommission]
      ,[InternalStatus]
      ,[InternalStatusCode]
      ,[MarketingReference]
      ,[GeneralConditions]
      ,[ContractStatus]
      ,[ApplicationStatus]
      ,[MarketingReferenceDescription]
      ,[ExpiryDate]
      ,[Age]
      ,[StatusCode]
      ,[Length]
      ,[ContractFrom]
      ,[ContractTo]
      ,[DriverName]
,[OpenCalculationPeriod]
      ,[ProductVariantCode]
      ,[ProductVariantName]
      ,[ProductTypeCode]
      ,[ProductTypeName]
,[StatusWFlag]
,[MarketingReferenceSegment]
)
;

";

            ParseSqlQuery(projectConfig.DatabaseComponents.First(x => x.DbName == "NRWH_L2"), projectConfig, graphManager, query);
        }
        
        [TestMethod]
        public void Parse_NRWH_DealerGroupData()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");

            var projectId = new Guid("56F38C2C-5D14-4767-BA6E-02FFE1E44919");

            var projectConfig = pcm.GetProjectConfig(projectId);

            var query = @"
WITH 
monthList AS(
SELECT 
	2016 YearNo, 1 MonthNo

UNION ALL

SELECT 
	YEAR(DATEADD(MONTH, 1, DATEFROMPARTS(YearNo, MonthNo, 1))), MONTH(DATEADD(MONTH, 1, DATEFROMPARTS(YearNo, MonthNo, 1))) 
FROM 
	monthList 
WHERE
	YearNo <= [rep].[YTYear]()
)
, monthlyLocalAuctions AS(
SELECT la.DealerGroupSID,  SUM(la.CarsSold) CarsSold, (la.YearNo * 100 + la.MonthNo) Period, SUM(la.Commission) as Commission
  FROM LocalAuctions la
  group by la.DealerGroupSID, (la.YearNo * 100 + la.MonthNo)
)
, monthlyPresales AS(
SELECT dg.DealerGroupSID, COUNT(*) PresalesCount, (ps.YearNo * 100 + ps.MonthNo) Period
  FROM Presales ps
  INNER JOIN DealerToDealerGroup ddg ON ddg.DealerSID = ps.DealerSID
  INNER JOIN DealerGroup dg ON dg.DealerGroupSID = ddg.DealerGroupSID AND dg.Type = N'YT2018_DealerGroup'  
  group by dg.DealerGroupSID, (ps.YearNo * 100 + ps.MonthNo)
)
,excludedDealers AS
(
	SELECT 
	DISTINCT DealerSID 
	FROM ClientExcluded
)
, CInsuranceContactCount as
(
 SELECT  d2dg.DealerGroupSID, count(*) CashInsuranceContractCount
  FROM [dbo].[InsuranceContractNIA] nia
  LEFT JOIN [dbo].[Dealer] d on nia.DealerSID = d.DealerSID
 LEFT JOIN [dbo].[DealerToDealerGroup] d2dg on d2dg.DealerSID = d.DealerSID
 WHERE nia.ConstractStart_DateId>=20160101 and IIF(nia.ContractEnd_DateId<>-1,  DATEDIFF(day, CONVERT(datetime,cast( nia.ConstractStart_DateId as nvarchar(8))),CONVERT(datetime,cast( nia.ContractEnd_DateId as nvarchar(8)))),0)>60 and nia.ContractStatus<>'Chybná - nerealizovaná smlouva'
  group by d2dg.DealerGroupSID
) /*, 
guarantedBuyback as(
	SELECT 
		cd.[DealerGroupSID]
		,cd.Period
		,cd.YearNo
		,cd.MonthNo
      ,COUNT([hasGuarantedBuyback]) GuarantedBuybackCount
	FROM [dbo].[Contract] c
	INNER JOIN rep.vw_YT2018_ContractData_Base cd ON c.ContractSID = cd.ContractSID
	where cd.DealerGroupSID is not null and hasGuarantedBuyback =1
	group by cd.[DealerGroupSID], cd.Period, cd.YearNo, cd.MonthNo
  )*/
, CPayedInsuranceContractCount as(
 SELECT  d2dg.DealerGroupSID, count(*) PayedCashInsuranceContractCount
  FROM [dbo].[InsuranceContractNIA] nia
  LEFT JOIN [dbo].[Dealer] d on nia.DealerSID = d.DealerSID
 LEFT JOIN [dbo].[DealerToDealerGroup] d2dg on d2dg.DealerSID = d.DealerSID
 -- LEFT JOIN ClientExcluded ce ON ce.ClientSID=c.ClientSID and ce.DealerSID=d.DealerSID toto treba nejako napojit na klienta zatial nefacha
 WHERE (nia.Dealer_is_Client is null or nia.Dealer_is_Client='Ne')  and nia.ConstractStart_DateId>=20160101 and IIF(nia.ContractEnd_DateId<>-1,  DATEDIFF(day, CONVERT(datetime,cast( nia.ConstractStart_DateId as nvarchar(8))),CONVERT(datetime,cast( nia.ContractEnd_DateId as nvarchar(8)))),0)>60 and nia.ContractStatus<>'Chybná - nerealizovaná smlouva'
group by d2dg.DealerGroupSID
)
,monthGroupList AS
(
SELECT
	ml.YearNo
	,ml.MonthNo
	,ml.YearNo * 100 + ml.MonthNo Period
	,dg.DealerGroupSID
	,dg.Name DealerGroupName
FROM 
	monthList ml
	CROSS JOIN DealerGroup dg
WHERE
	dg.DealerGroupSID <> -1
)
,incMal AS(
SELECT 
c.ContractSID
,dg.DealerGroupSID
,dg.Name DealerGroupName
,1 ContractCount
,c.CountToYTPayedLvl1 PayedCountractCount
,CONVERT(INT, c.TakeOnDate) / 100 Period
,CONVERT(INT, c.TakeOnDate) / 10000 YearNo
,(CONVERT(INT, c.TakeOnDate) / 100) % 100 MonthNo
,CONVERT(INT, c.TakeOnDate) TakeOnDate
,c.FinancedValue 
,c.VehicleSID
,c.DealerSID
,dg.GroupLeaderSID
,asm.Area AsmArea
,asm.Name AsmName
,CASE 
			WHEN c.InstallmentCount < 3 AND p.TypeCode = N'C' THEN N'KDF'
			WHEN p.TypeCode = N'O' AND
				c.SubCategoryCode = 4 AND c.CategoryCode IN (31, 32, 33, 34, 35) THEN N'MAL' 
			WHEN p.TypeCode = N'O' THEN N'OL' 
			WHEN p.TypeCode = N'C' THEN N'CR' 
			WHEN p.TypeCode = N'F' THEN N'FL' 
			ELSE N'N/A' 
END [ProductCode]
,CASE 
	WHEN ConcernBrandFlag = N'Y' AND State = N'NEW' THEN 1
	ELSE 0
END AS IsNew
,CASE 
	WHEN ConcernBrandFlag = N'Y' AND State = N'NEW' THEN N'New'
	ELSE N'Used'
END AS ConcernBrand
,IIF(c.StatusCode IN (N'V', N'W', N'F') AND (NOT c.Age >= 6) AND (NOT (c.Length = 5 AND c.Age = 6)) AND v.InternalStatus IN (N'PO', N'PP', N'CD', N'VYK') AND cl.IsDealer = N'N', 1, 0) PUKClient
,IIF(c.StatusCode IN (N'V', N'W', N'F') AND (NOT c.Age >= 6) AND (NOT (c.Length = 5 AND c.Age = 6)) AND v.InternalStatus IN (N'PO', N'PP', N'CD', N'VYK') AND cl.IsDealer = N'Y', 1, 0) PUKDealer
FROM Contract c
INNER JOIN DealerToDealerGroup ddg ON ddg.DealerSID = c.DealerSID
INNER JOIN DealerGroup dg ON dg.DealerGroupSID = ddg.DealerGroupSID
INNER JOIN Product p ON p.ProductSID = c.ProductSID
INNER JOIN Vehicle v ON v.VehicleSID = c.VehicleSID
INNER JOIN Dealer leader ON leader.DealerSID = dg.GroupLeaderSID
INNER JOIN AreaSalesManager asm ON asm.AreaSalesManagerSID = leader.AreaSalesManagerSID
INNER JOIN Client cl ON cl.ClientSID = c.ClientSID
WHERE 
dg.Type = N'YT2018_DealerGroup' 
AND v.InternalStatus <> N'O14' 
AND c.StatusCode <> N'C' 
AND c.ContractSID <> -1 
AND c.IsApplication = 0
)
,monthlyValues AS(
SELECT 
	mgl.DealerGroupSID
	,mgl.DealerGroupName
	,mgl.Period
	,mgl.YearNo
	,mgl.MonthNo

	,SUM(ISNULL(c.ContractCount * c.IsNew, 0)) ContractCountNew
	,SUM(ISNULL(c.PayedCountractCount * c.IsNew, 0)) PayedContractCountNew
	,SUM(ISNULL(c.ContractCount * (1-c.IsNew), 0)) ContractCountUsed
	,SUM(ISNULL(c.PayedCountractCount * (1-c.IsNew), 0)) PayedContractCountUsed

	,SUM(ISNULL(c.FinancedValue * c.IsNew, 0)) FinancedValueNew
	,SUM(ISNULL(c.PayedCountractCount * c.FinancedValue * c.IsNew, 0)) PayedFinancedValueNew
	,SUM(ISNULL(c.FinancedValue * (1-c.IsNew), 0)) FinancedValueUsed
	,SUM(ISNULL(c.PayedCountractCount * c.FinancedValue * (1-c.IsNew), 0)) PayedFinancedValueUsed
	--,SUM(ISNULL(c.ProvisionAmount, 0)) ProvisionAmount
	--,SUM(ISNULL(c.MotivationAmount, 0)) MotivationAmount

	,0 /*SUM(ISNULL(a.ContractCount * a.IsNew, 0))*/ AppContractCountNew
	,0 /*SUM(ISNULL(a.PayedCountractCount * a.IsNew, 0))*/ AppPayedContractCountNew
	,0 /*SUM(ISNULL(a.ContractCount * (1-a.IsNew), 0))*/ AppContractCountUsed
	,0 /*SUM(ISNULL(a.PayedCountractCount * (1-a.IsNew), 0))*/ AppPayedContractCountUsed

	,0 /*SUM(ISNULL(a.FinancedValue * a.IsNew, 0))*/ AppFinancedValueNew
	,0 /*SUM(ISNULL(a.PayedCountractCount * a.FinancedValue * a.IsNew, 0))*/ AppPayedFinancedValueNew
	,0 /*SUM(ISNULL(a.FinancedValue * (1-a.IsNew), 0))*/ AppFinancedValueUsed
	,0 /*SUM(ISNULL(a.PayedCountractCount * a.FinancedValue * (1-a.IsNew), 0))*/ AppPayedFinancedValueUsed
	
	--,SUM(c.PUKClient) PUKClient
	--,SUM(c.PUKDealer) PUKDealer
	,SUM(ISNULL(c.GuaranteedBuybackCount, 0)) GuaranteedBuybackCount
	--,SUM(ISNULL(c.PresalesCount, 0)) PresalesCount

FROM 
	monthGroupList mgl
	LEFT JOIN [rep].[vw_YT2018_ContractData_Base] c ON c.Period = mgl.Period AND c.DealerGroupSID = mgl.DealerGroupSID
	--LEFT JOIN [rep].vw_YT2018_ApplicationData a ON a.Period = mgl.Period AND a.DealerGroupSID = mgl.DealerGroupSID
GROUP BY
	mgl.DealerGroupSID
	,mgl.DealerGroupName
	,mgl.Period
	,mgl.YearNo
	,mgl.MonthNo	
)
,monthlyProvMot AS(
SELECT 
CONVERT(INT, c.TakeOnDate / 100) Period, dg.DealerGroupSID, 
-1 * SUM(IIF(s.SubventionType LIKE 'M%', s.SubventionAmount, 0)) MotivationAmount 
,-1 * SUM(IIF(s.SubventionType LIKE 'PD%', s.SubventionAmount, 0)) ProvisionAmount 
--*
--SUM(-1 * s.SubventionAmount)
--dg.Name DealerGroup, c.ContractNumber, c.TakeOnDate, s.ProviderCode, s.SubventionType, s.SubventionStatus, -1 * s.SubventionAmount SubventionAmount
FROM Contract c
INNER JOIN Dealer d ON d.DealerSID = c.Broker_DealerSID
INNER JOIN Dealer d2 ON d2.DealerSID = IIF(d.Code = N'6', c.DealerSID, c.Broker_DealerSID)
INNER JOIN DealerToDealerGroup ddg ON ddg.DealerSID = d2.DealerSID -- c.DealerSID
INNER JOIN DealerGroup dg ON dg.DealerGroupSID = ddg.DealerGroupSID
INNER JOIN Product p ON p.ProductSID = c.ProductSID
INNER JOIN Vehicle v ON v.VehicleSID = c.VehicleSID
INNER JOIN Dealer leader ON leader.DealerSID = dg.GroupLeaderSID
INNER JOIN AreaSalesManager asm ON asm.AreaSalesManagerSID = leader.AreaSalesManagerSID
INNER JOIN Client cl ON cl.ClientSID = c.ClientSID

INNER JOIN Subvention s ON s.ContractSID = c.ContractSID
--LEFT JOIN excluded excl on excl.DealerSID=d2.DealerSID  and excl.MarketingReferenceCode =c.MarketingReference
--LEFT JOIN excluded2 excl2 on excl2.ClientSID=c.ClientSID and excl2.DealerSID=d2.DealerSID
LEFT JOIN ClientExcluded excl1 ON excl1.DealerSID = d2.DealerSID AND excl1.ClientSID = c.ClientSID

WHERE 
dg.Type = N'YT2018_DealerGroup' 

AND 
v.InternalStatus <> N'O14' 
AND c.StatusCode <> N'C' 
AND c.ContractSID <> -1 
AND NOT(p.TypeCode = N'O' AND c.SubCategoryCode = 4 AND c.CategoryCode IN (31, 32, 33, 34, 35))  -- exclude MAL
AND c.IsApplication = 0
--AND excl.isExcluded IS NULL
--AND excl2.isExcluded2 IS NULL
AND SubventionGroup = N'COM'
AND SubventionStatus = N'L'

AND p.TypeCode IN (N'O', N'C', N'F')
--AND dg.DealerGroupSID = 131
--AND c.TakeOnDate >= 20170000 AND c.TakeOnDate <= 20170599
--AND c.ContractNumber = N'1012520'

GROUP BY CONVERT(INT, c.TakeOnDate / 100), dg.DealerGroupSID
)
,monthlyApps AS(
SELECT 
	mgl.DealerGroupSID
	,mgl.DealerGroupName
	,mgl.Period
	,mgl.YearNo
	,mgl.MonthNo

	,SUM(ISNULL(a.ContractCount * a.IsNew, 0)) AppContractCountNew
	,SUM(ISNULL(a.PayedCountractCount * a.IsNew, 0)) AppPayedContractCountNew
	,SUM(ISNULL(a.ContractCount * (1-a.IsNew), 0)) AppContractCountUsed
	,SUM(ISNULL(a.PayedCountractCount * (1-a.IsNew), 0)) AppPayedContractCountUsed

	,SUM(ISNULL(a.FinancedValue * a.IsNew, 0)) AppFinancedValueNew
	,SUM(ISNULL(a.PayedCountractCount * a.FinancedValue * a.IsNew, 0)) AppPayedFinancedValueNew
	,SUM(ISNULL(a.FinancedValue * (1-a.IsNew), 0)) AppFinancedValueUsed
	,SUM(ISNULL(a.PayedCountractCount * a.FinancedValue * (1-a.IsNew), 0)) AppPayedFinancedValueUsed
	
FROM 
	monthGroupList mgl
	LEFT JOIN [rep].vw_YT2018_ApplicationData a ON a.Period = mgl.Period AND a.DealerGroupSID = mgl.DealerGroupSID
GROUP BY
	mgl.DealerGroupSID
	,mgl.DealerGroupName
	,mgl.Period
	,mgl.YearNo
	,mgl.MonthNo	
)
,monthlyIncMal AS
(
SELECT 
	mgl.DealerGroupSID
	,mgl.DealerGroupName
	,mgl.Period
	,mgl.YearNo
	,mgl.MonthNo

	,COUNT(*) ContractCount
FROM 
	monthGroupList mgl
	LEFT JOIN incMal c ON c.Period = mgl.Period AND c.DealerGroupSID = mgl.DealerGroupSID
GROUP BY
	mgl.DealerGroupSID
	,mgl.DealerGroupName
	,mgl.Period
	,mgl.YearNo
	,mgl.MonthNo	
)
,monthlyInsurance AS
(
SELECT 
dg.DealerGroupSID 
,nia.Reporting_DateId / 100 Period
,COUNT(*) InsuranceContractCount
,SUM(IIF(nia.DealerIsClient = N'N' /*AND ed.DealerSID IS NULL*/, 1, 0)) PayedInsuranceContractCount
,SUM(nia.CommissionAmountPaidTotal / IIF(nia.InsuranceCompanyName = N'Allianz pojišťovna, a.s.', 0.17, 0.18) * IIF(nia.DealerIsClient = N'N' /*AND ed.DealerSID IS NULL*/, 1, 0)) InsuranceAmount
,SUM(nia.CommissionAmountPaidTotal / IIF(nia.InsuranceCompanyName = N'Allianz pojišťovna, a.s.', 0.17, 0.18)) InsuranceCountAmount
,SUM(IncentiveAmountDealer) IncentiveAmountDealer
,SUM(nia.CommissionAmountDealer) CommissionAmountDealer
,SUM(nia.SalesPersonIncentive) SalesPersonIncentive
FROM DealerGroup dg 
  INNER JOIN DealerToDealerGroup ddg ON ddg.DealerGroupSID = dg.DealerGroupSID
  INNER JOIN Dealer d ON d.DealerSID = ddg.DealerSID
  INNER JOIN InsuranceContractNIA nia ON nia.DealerSID = d.DealerSID
  LEFT JOIN excludedDealers ed ON ed.DealerSID = nia.DealerSID
  WHERE 
  (nia.Reporting_DateId>=20160101  
  and (nia.ContractEnd_DateId = -1 OR (
  DATEDIFF(DAY
	,DATEFROMPARTS(nia.ConstractStart_DateId / 10000, (nia.ConstractStart_DateId / 100) % 100, nia.ConstractStart_DateId % 100)
	,DATEFROMPARTS(nia.ConstractStart_DateId / 10000, (nia.ContractEnd_DateId / 100) % 100, nia.ContractEnd_DateId % 100)
  ) >= 61
  ))
  AND
  nia.ContractStatus<>'Chybná - nerealizovaná smlouva')
  AND
  dg.Type = N'YT2018_DealerGroup'
GROUP BY 
	dg.DealerGroupSID
	,nia.Reporting_DateId / 100
/*
SELECT 
	dg.DealerGroupSID
	,i.ContractSignedDateId / 100 Period
	,COUNT(DISTINCT i.InsuranceSID) InsuranceContractCount
	,SUM(ic.InsuranceAmount) InsuranceAmount
FROM 
	Insurance i
	INNER JOIN InsuranceCoverage ic ON ic.InsuranceSID = i.InsuranceSID
	INNER JOIN DealerToDealerGroup ddg ON ddg.DealerSID = i.DealerSID
	INNER JOIN DealerGroup dg ON ddg.DealerGroupSID = dg.DealerGroupSID
WHERE 
	dg.Type = N'YT2018_DealerGroup'
GROUP BY 
	dg.DealerGroupSID
	,i.ContractSignedDateId / 100
*/
)
,monthlyPenetration AS
(
SELECT 

	dg.DealerGroupSID
	,p.Period
	,SUM(p.DealerVehicleCount) DealerVehicleCount
	,SUM(p.ContractCount) PenetrationCount
FROM 
	rep.MonthlyPenetration p
	INNER JOIN DealerToDealerGroup ddg ON ddg.DealerSID = p.DealerSID
	INNER JOIN DealerGroup dg ON ddg.DealerGroupSID = dg.DealerGroupSID
WHERE 
	dg.Type = N'YT2018_DealerGroup'
GROUP BY 
	dg.DealerGroupSID
	,p.Period
)
,monthlyPUK AS
(
SELECT 

	dg.DealerGroupSID
	,p.Period
	,SUM(p.PUKClient) PUKClient
	,SUM(p.PUKDealer) PUKDealer
FROM 
	rep.vw_YT2018_PUKData p
	INNER JOIN DealerToDealerGroup ddg ON ddg.DealerSID = p.DealerSID
	INNER JOIN DealerGroup dg ON ddg.DealerGroupSID = dg.DealerGroupSID
WHERE 
	dg.Type = N'YT2018_DealerGroup'
GROUP BY 
	dg.DealerGroupSID
	,p.Period
)
,monthlyRetention AS
(
SELECT 
	dg.DealerGroupSID
	,IIF(p.ValidFrom_DateId < ([rep].[YTYear]() - 1) * 10000 + 101, ([rep].[YTYear]() - 1) * 10000 + 101, CONVERT(INT, p.ValidFrom_DateId)) / 100 Period
	,SUM(1) RetentionOldContractCount
	,SUM(IIF(p.NewContractFrom <= [rep].[YTYear]() * 10000 + 1231, 0, 1)) RetentionNewContractCount
FROM 
	[rep].[vw_YT2018_Retention] p
	INNER JOIN DealerToDealerGroup ddg ON ddg.DealerSID = p.DealerSID
	INNER JOIN DealerGroup dg ON ddg.DealerGroupSID = dg.DealerGroupSID
WHERE 
	dg.Type = N'YT2018_DealerGroup'
GROUP BY 
	dg.DealerGroupSID
	,IIF(p.ValidFrom_DateId < ([rep].[YTYear]() - 1) * 10000 + 101, ([rep].[YTYear]() - 1) * 10000 + 101, CONVERT(INT, p.ValidFrom_DateId)) / 100
)
,ytm AS(
SELECT 
	c.DealerGroupSID
	,c.DealerGroupName
	,c.Period
	,c.YearNo
	,c.MonthNo

	,c.ContractCountNew
	,c.PayedContractCountNew
	,c.ContractCountUsed
	,c.PayedContractCountUsed

	,c.FinancedValueNew
	,c.PayedFinancedValueNew
	,c.FinancedValueUsed
	,c.PayedFinancedValueUsed

	,ISNULL(ci.CashInsuranceContractCount, 0) InsuranceContractCount
	,ISNULL(i.InsuranceAmount, 0) InsuranceAmount
	,ISNULL(i.InsuranceCountAmount, 0) InsuranceCountAmount
	
	,SUM(incMal.ContractCount) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) ContractCountIncMalYTM
	
	,SUM(c.ContractCountNew) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) ContractCountNewYTM
	,SUM(c.PayedContractCountNew) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) PayedContractCountNewYTM
	,SUM(c.ContractCountUsed) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) ContractCountUsedYTM
	,SUM(c.PayedContractCountUsed) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) PayedContractCountUsedYTM
	
	,SUM(c.FinancedValueNew) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) FinancedValueNewYTM
	,SUM(c.PayedFinancedValueNew) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) PayedFinancedValueNewYTM
	,SUM(c.FinancedValueUsed) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) FinancedValueUsedYTM
	,SUM(c.PayedFinancedValueUsed) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) PayedFinancedValueUsedYTM

	,SUM(a.AppContractCountNew) OVER(PARTITION BY c.DealerGroupSID ORDER BY c.YearNo, c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AppContractCountNewYTM
	,SUM(a.AppPayedContractCountNew) OVER(PARTITION BY c.DealerGroupSID ORDER BY c.YearNo, c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AppPayedContractCountNewYTM
	,SUM(a.AppContractCountUsed) OVER(PARTITION BY c.DealerGroupSID ORDER BY c.YearNo, c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AppContractCountUsedYTM
	,SUM(a.AppPayedContractCountUsed) OVER(PARTITION BY c.DealerGroupSID ORDER BY c.YearNo, c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AppPayedContractCountUsedYTM
	
	
	,SUM(a.AppFinancedValueNew) OVER(PARTITION BY c.DealerGroupSID ORDER BY c.YearNo, c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AppFinancedValueNewYTM
	,SUM(a.AppPayedFinancedValueNew) OVER(PARTITION BY c.DealerGroupSID ORDER BY c.YearNo, c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AppPayedFinancedValueNewYTM
	,SUM(a.AppFinancedValueUsed) OVER(PARTITION BY c.DealerGroupSID ORDER BY c.YearNo, c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AppFinancedValueUsedYTM
	,SUM(a.AppPayedFinancedValueUsed) OVER(PARTITION BY c.DealerGroupSID ORDER BY c.YearNo, c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AppPayedFinancedValueUsedYTM

	,ISNULL(SUM(i.InsuranceContractCount) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) InsuranceContractCountYTM
	,ISNULL(SUM(i.InsuranceAmount) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) InsuranceAmountYTM
	,ISNULL(SUM(i.InsuranceCountAmount) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) InsuranceCountAmountYTM
	,ISNULL(SUM(i.PayedInsuranceContractCount) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) PayedInsuranceContractCountYTM
	,ISNULL(SUM(i.SalesPersonIncentive) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) SalesPersonIncentive
	,ISNULL(SUM(i.IncentiveAmountDealer) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) IncentiveAmountDealer
	,ISNULL(SUM(i.CommissionAmountDealer) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) CommissionAmountDealer
	
	,ISNULL(SUM(puk.PUKClient) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) PUKClientYTM
	,ISNULL(SUM(puk.PUKDealer) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) PUKDelaerYTM

	,ISNULL(SUM(p.DealerVehicleCount) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) DealerVehicleCountYTM
	,ISNULL(SUM(p.PenetrationCount) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) PenetrationCountYTM
	,ISNULL(SUM(c.GuaranteedBuybackCount) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) GuaranteedBuybackCountYTM
	,ISNULL(SUM(mps.PresalesCount) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) PresalesCountYTM
	,ISNULL(SUM(mla.CarsSold) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) LocalAuctionsCountYTM
	,ISNULL(SUM(mla.Commission) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) CommissionsCountYTM

	,ISNULL(SUM(mpm.ProvisionAmount) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) ProvisionYTM
	,ISNULL(SUM(mpm.MotivationAmount) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) MotivationYTM
	
	,ISNULL(SUM(r.RetentionOldContractCount) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) RetentionOldContractCountYTM
	,ISNULL(SUM(r.RetentionNewContractCount) OVER(PARTITION BY c.YearNo, c.DealerGroupSID ORDER BY c.MonthNo ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW), 0) RetentionNewContractCountYTM

FROM 
	monthlyValues c
	LEFT JOIN  monthlyIncMal incMal ON incMal.Period = c.Period AND incMal.DealerGroupSID = c.DealerGroupSID
	LEFT JOIN  monthlyApps a ON a.Period = c.Period AND a.DealerGroupSID = c.DealerGroupSID
	LEFT JOIN monthlyInsurance i ON i.Period = c.Period AND i.DealerGroupSID = c.DealerGroupSID
	LEFT JOIN CInsuranceContactCount ci ON ci.DealerGroupSID = c.DealerGroupSID
	LEFT JOIN CPayedInsuranceContractCount cpi ON cpi.DealerGroupSID = c.DealerGroupSID
	LEFT JOIN monthlyPenetration p ON p.Period = c.Period AND p.DealerGroupSID = c.DealerGroupSID
	LEFT JOIN monthlyRetention r ON r.Period = c.Period AND r.DealerGroupSID = c.DealerGroupSID
	LEFT JOIN monthlyLocalAuctions mla ON mla.Period = c.Period AND mla.DealerGroupSID = c.DealerGroupSID
	LEFT JOIN monthlyPUK puk ON puk.Period = c.Period AND puk.DealerGroupSID = c.DealerGroupSID
	LEFT JOIN monthlyPresales mps ON mps.Period = c.Period AND mps.DealerGroupSID = c.DealerGroupSID
	LEFT JOIN monthlyProvMot mpm ON mpm.Period = c.Period AND mpm.DealerGroupSID = c.DealerGroupSID
)
,fcst AS(
SELECT
	c.DealerGroupSID
	,c.DealerGroupName
	,c.Period
	,c.YearNo
	,c.MonthNo

	,c.ContractCountNew
	,c.PayedContractCountNew
	,c.ContractCountUsed
	,c.PayedContractCountUsed

	,c.FinancedValueNew
	,c.PayedFinancedValueNew
	,c.FinancedValueUsed
	,c.PayedFinancedValueUsed

	,c.InsuranceContractCount
	,c.InsuranceAmount
	,c.InsuranceCountAmount
	,c.ContractCountIncMalYTM

	,c.ContractCountNewYTM
	,c.PayedContractCountNewYTM
	,c.ContractCountUsedYTM
	,c.PayedContractCountUsedYTM
	
	,c.FinancedValueNewYTM
	,c.PayedFinancedValueNewYTM
	,c.FinancedValueUsedYTM
	,c.PayedFinancedValueUsedYTM

	,c.AppContractCountNewYTM
	,c.AppPayedContractCountNewYTM
	,c.AppContractCountUsedYTM
	,c.AppPayedContractCountUsedYTM
	
	,c.AppFinancedValueNewYTM
	,c.AppPayedFinancedValueNewYTM
	,c.AppFinancedValueUsedYTM
	,c.AppPayedFinancedValueUsedYTM

	,c.InsuranceContractCountYTM
	,c.InsuranceAmountYTM
	,c.InsuranceCountAmountYTM
	,c.PayedInsuranceContractCountYTM

	,c.PUKClientYTM
	,c.PUKDelaerYTM
	,c.PUKClientYTM + c.PUKDelaerYTM PUKTotalYTM
	,c.PenetrationCountYTM
	,c.DealerVehicleCountYTM
	,c.GuaranteedBuybackCountYTM
	,c.PresalesCountYTM
	,c.LocalAuctionsCountYTM
	,c.CommissionsCountYTM

	,c.ProvisionYTM
	,c.MotivationYTM
	,c.RetentionOldContractCountYTM
	,c.RetentionNewContractCountYTM

	,/*FLOOR(*/c.ContractCountNewYTM / fc.CumulativeCurve/*)*/ ContractCountNewForecast
	,/*FLOOR(*/c.PayedContractCountNewYTM / fc.CumulativeCurve/*)*/ PayedContractCountNewForecast
	,/*FLOOR(*/c.ContractCountUsedYTM / fc.CumulativeCurve/*)*/ ContractCountUsedForecast
	,/*FLOOR(*/c.PayedContractCountUsedYTM / fc.CumulativeCurve/*)*/ PayedContractCountUsedForecast
	
	,c.FinancedValueNewYTM / fc.CumulativeCurve FinancedValueNewForecast
	,c.PayedFinancedValueNewYTM / fc.CumulativeCurve PayedFinancedValueNewForecast
	,c.FinancedValueUsedYTM / fc.CumulativeCurve FinancedValueUsedForecast
	,c.PayedFinancedValueUsedYTM / fc.CumulativeCurve PayedFinancedValueUsedForecast

	,/*FLOOR(*/c.InsuranceContractCountYTM / fci.CumulativeCurve/*)*/ InsuranceContractCountForecast
	,c.InsuranceAmountYTM / fci.CumulativeCurve InsuranceAmountForecast
	,c.InsuranceCountAmountYTM / fci.CumulativeCurve InsuranceCountAmountForecast
	,/*FLOOR(*/c.PayedInsuranceContractCountYTM / fci.CumulativeCurve/*)*/ PayedInsuranceContractCountForecast
	,(CONVERT(REAL, c.GuaranteedBuybackCountYTM) / c.MonthNo) * 12 GuaranteedBuybackCountForecast
	,(CONVERT(REAL, c.PresalesCountYTM )/ c.MonthNo) * 12 PresalesCountForecast
	,c.LocalAuctionsCountYTM / fcr.CumulativeCurve LocalAuctionsCountForecast
	,(CONVERT(REAL, c.CommissionsCountYTM )/ c.MonthNo) * 12 CommissionsCountForecast
	,c.SalesPersonIncentive
	,c.IncentiveAmountDealer
	,c.CommissionAmountDealer

FROM ytm c
	INNER JOIN FulfillmentCurve fc ON c.MonthNo = fc.[Month] AND fc.[Year] = [rep].[YTYear]()  AND fc.CurveType = N'FinSml'
	INNER JOIN FulfillmentCurve fci ON c.MonthNo = fci.[Month] AND fci.[Year] = [rep].[YTYear]()  AND fci.CurveType = N'CashIns'
	INNER JOIN FulfillmentCurve fcr ON c.MonthNo = fcr.[Month] AND fcr.[Year] = [rep].[YTYear]()  AND fcr.CurveType = N'Remarketing'
)
--SELECT f.* FROM fcst f
,zones AS
(
SELECT 
	z.DealerGroupZone2SID
	,z.[DealerGroupSID]
	,z.ZoneType
	,z.ZoneNumber
	,z.LowerLimit
	,z.UpperLimit
	,z.Bonus
	,z2.LowerLimit NextLevelLowerLimit
	,z2.ZoneNumber NextLevel
	,z2.Bonus NextLevelBonus  
FROM 
	YT2018_DealerGroupZone2 z
	LEFT JOIN YT2018_DealerGroupZone2 z2 ON z.[DealerGroupSID] = z2.[DealerGroupSID] AND z.ZoneType = z2.ZoneType AND z2.ZoneNumber = z.ZoneNumber + 1

UNION ALL

SELECT 
	z.DealerGroupZone2SID
	,z.[DealerGroupSID]
	,z.ZoneType
	,NULL ZoneNumber
	,0 LowerLimit
	,z.LowerLimit UpperLimit
	,NULL Bonus
	,z.LowerLimit NextLevelLowerLimit
	,z.ZoneNumber NextLevel
	,z.Bonus NextLevelBonus  
FROM 
	YT2018_DealerGroupZone2 z
	WHERE z.ZoneNumber = 1
)
,zonesAsgn AS
(
SELECT 
	fcst.*
	
	,newZone.UpperLimit NewZoneUpperLimit
	,ISNULL(newZone.ZoneNumber, 0) NewZoneNumber
	,ISNULL(newZone.NextLevel, 1) NewZoneNumberPotential
	,ISNULL(newZone.NextLevelLowerLimit, NULL) NewZoneLimitPotential
	,ISNULL(usedZone.ZoneNumber, 0) UsedZoneNumber
	,ISNULL(usedZone.NextLevel, 1) UsedZonePotential
	,ISNULL(usedZone.NextLevelLowerLimit, NULL) UsedZoneLimitPotential
	,ISNULL(cashInsZone.ZoneNumber, 0) InsuranceZoneNumber
	,ISNULL(cashInsZone.NextLevel, 1) InsuranceZonePotential
	,ISNULL(cashInsZone.NextLevelLowerLimit, NULL) InsuranceZoneLimitPotential
	,ISNULL(piaZone.ZoneNumber, 0) PiaZoneNumber
	,ISNULL(piaZone.NextLevel, 1) PiaZonePotential
	,ISNULL(piaZone.UpperLimit, NULL) PiaZoneLimitPotential

	,IIF(newZone.ZoneNumber IS NULL or usedZone.ZoneNumber IS NULL, 0, ISNULL(fcst.PayedFinancedValueNewForecast * newZone.Bonus, 0)) NewZoneBonus
	,IIF(usedZone.ZoneNumber IS NULL or newZone.ZoneNumber IS NULL, 0, ISNULL(fcst.PayedFinancedValueUsedForecast * usedZone.Bonus, 0)) UsedZoneBonus
	,ISNULL(ISNULL((IIF(fcst.ContractCountNewForecast = 0, 0, newZone.UpperLimit) / IIF(fcst.ContractCountNewForecast = 0, 1, fcst.ContractCountNewForecast)) * fcst.PayedFinancedValueNewForecast, fcst.PayedFinancedValueNewForecast) * newZone.NextLevelBonus, NULL) NewZoneNextLevelBonus
--	,ISNULL(ISNULL((IIF(fcst.ContractCountUsedForecast = 0, 0, usedZone.UpperLimit) / IIF(fcst.ContractCountUsedForecast = 0, 1, fcst.ContractCountUsedForecast)) * fcst.PayedFinancedValueUsedForecast, fcst.PayedFinancedValueUsedForecast) * usedZone.NextLevelBonus, NULL) UsedZoneNextLevelBonus
    ,ISNULL(PayedFinancedValueUsedForecast,1)/IIF(ISNULL(ContractCountUsedForecast,1)=0,1,ISNULL(ContractCountUsedForecast,1))* usedZone.UpperLimit* (usedZone.NextLevelBonus+ 0.0135)  UsedZoneNextLevelBonus
	,ISNULL(/*ISNULL(*/(IIF(fcst.InsuranceContractCountForecast = 0, 0, cashInsZone.NextLevelLowerLimit) / IIF(fcst.InsuranceContractCountForecast = 0, 1, fcst.InsuranceContractCountForecast)) * fcst.InsuranceAmountForecast/*, fcst.InsuranceAmountForecast)*/ * cashInsZone.NextLevelBonus, NULL) InsuranceZoneNextLevelBonus
	,ISNULL(fcst.InsuranceAmountForecast * cashInsZone.Bonus, 0) InsuranceZoneBonus
	,ISNULL(fcst.PayedFinancedValueNewForecast * piaZone.Bonus, 0) PiaZoneBonus
	,ISNULL(onTopZone.Bonus, 0) OnTopBonus

	,IIF(newZone.NextLevel IS NULL, NULL, ISNULL((newZone.UpperLimit / IIF(fcst.ContractCountNewForecast = 0, 1, fcst.ContractCountNewForecast)) * fcst.GuaranteedBuybackCountForecast, fcst.GuaranteedBuybackCountYTM)) GuaranteedBuybackNewxtSegment
	,IIF(newZone.NextLevel IS NULL, NULL, ISNULL((newZone.UpperLimit / IIF(fcst.ContractCountNewForecast = 0, 1, fcst.ContractCountNewForecast)) * fcst.PresalesCountForecast, fcst.PresalesCountYTM)) PresalesCountNewxtSegment
	,IIF(newZone.NextLevel IS NULL, NULL, ISNULL((newZone.UpperLimit / IIF(fcst.ContractCountNewForecast = 0, 1, fcst.ContractCountNewForecast)) * fcst.LocalAuctionsCountForecast, fcst.LocalAuctionsCountYTM)) LocalAuctionsCountNewxtSegment
	,IIF(newZone.NextLevel IS NULL, NULL, ISNULL((newZone.UpperLimit / IIF(fcst.ContractCountNewForecast = 0, 1, fcst.ContractCountNewForecast)) * fcst.CommissionsCountForecast, fcst.CommissionsCountForecast)) CommissionsCountNewxtSegment
	,IIF(newZone.NextLevel IS NULL, fcst.PayedFinancedValueNewForecast, ISNULL((newZone.UpperLimit / IIF(fcst.ContractCountNewForecast = 0, 1, fcst.ContractCountNewForecast)) * fcst.PayedFinancedValueNewForecast, fcst.PayedFinancedValueNewForecast)) + 
		IIF(usedZone.NextLevel IS NULL, fcst.PayedFinancedValueUsedForecast, ISNULL((usedZone.UpperLimit / IIF(fcst.ContractCountUsedForecast = 0, 1, fcst.ContractCountUsedForecast)) * fcst.PayedFinancedValueUsedForecast, fcst.PayedFinancedValueUsedForecast)) FVNextSegment
	,IIF(newZone.NextLevel IS NULL, NULL /*fcst.PenetrationCountYTM*/, ISNULL((newZone.UpperLimit / IIF(fcst.ContractCountNewForecast = 0, 1, fcst.ContractCountNewForecast)) * fcst.PenetrationCountYTM, fcst.PenetrationCountYTM)) PenetrationNewxtSegment
	,IIF(newZone.NextLevel IS NULL, NULL /*fcst.DealerVehicleCountYTM*/, ISNULL((newZone.UpperLimit / IIF(fcst.ContractCountNewForecast = 0, 1, fcst.ContractCountNewForecast)) * fcst.DealerVehicleCountYTM, fcst.DealerVehicleCountYTM)) DealerVehicleCountNewxtSegment

FROM 
	fcst
	LEFT JOIN zones newZone ON newZone.ZoneType = N'NEW' AND newZone.LowerLimit <= fcst.ContractCountNewForecast AND newZone.UpperLimit > fcst.ContractCountNewForecast AND fcst.DealerGroupSID = newZone.[DealerGroupSID]
	LEFT JOIN zones usedZone ON usedZone.ZoneType = N'USED' AND usedZone.LowerLimit <= fcst.ContractCountUsedForecast AND usedZone.UpperLimit > fcst.ContractCountUsedForecast AND fcst.DealerGroupSID = usedZone.[DealerGroupSID]
	LEFT JOIN zones cashInsZone ON cashInsZone.ZoneType = N'CASH_INS' AND cashInsZone.LowerLimit <= fcst.InsuranceContractCountForecast /*fcst.PayedInsuranceContractCountForecast*/ AND cashInsZone.UpperLimit > fcst.InsuranceContractCountForecast /*fcst.PayedInsuranceContractCountForecast*/ AND fcst.DealerGroupSID = cashInsZone.[DealerGroupSID]
	LEFT JOIN zones piaZone ON piaZone.ZoneType = N'PIA' AND piaZone.LowerLimit <= fcst.ContractCountNewForecast AND piaZone.UpperLimit > fcst.ContractCountNewForecast AND fcst.DealerGroupSID = piaZone.[DealerGroupSID]
	LEFT JOIN zones onTopZone ON onTopZone.ZoneType = N'ONTOP' AND fcst.DealerGroupSID = onTopZone.[DealerGroupSID]
)
SELECT z.* FROM zonesAsgn z 
";
            Dictionary<string, MssqlModelElement> outputColumns;
            ParseSqlQuery(projectConfig.DatabaseComponents.First(x => x.DbName == "NRWH_L2"), projectConfig, graphManager, query, out outputColumns);
        }

        [TestMethod]
        public void Parse_CPI_DimCC_LookupRoles()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=cpicustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            ConfigManager.Log = new ConsoleLogger("DLS test");

            var projectId = new Guid("FD1312FC-1182-4B9C-82D9-2089F3468BFB");

            var projectConfig = pcm.GetProjectConfig(projectId);

            var query = @"
;
WITH users AS
(
SELECT DISTINCT
 centerType
 ,centerId
 ,cu1.roleId
 , STUFF(
     (SELECT DISTINCT 
	    ', '  + u.userSurname + ' ' + u.userName 
      FROM TG.Center_User cu2 
	  LEFT JOIN TG.[User] u ON cu2.userId = u.id 
	  WHERE 
	    cu1.centerType = cu2.centerType 
		AND cu1.centerId = cu2.centerId 
		AND cu1.RoleId = cu2.RoleId 
		AND cu2.isMain = 1
		AND GETDATE() BETWEEN cu2.validFrom AND ISNULL( cu2.ValidTo, CONVERT( datetime, '9999-12-31', 120))
      FOR XML PATH('')
	  ), 1, 2, '') UserName
FROM TG.Center_User cu1
),
roles AS
(
select * from 
(
SELECT
centerType
,centerId
,r.roleName
,UserName
from users u
INNER JOIN [TG].[Role] r ON u.roleId = r.Id AND u.centerType = 2
) as x 
pivot
(
MAX(userName)
FOR roleName IN 
(
[Asset manager / Director]
,[Letting manager]
,[Property manager]
,[Oblastní facility manager]
,[Contract administrator]
,[Biller]
,[Reporting administrator - Property]
,[Receivable administrator]
)
) as y
)
SELECT
cc.ccCostCentreCode
,CONVERT(NVARCHAR(255), ISNULL(r.[Asset manager / Director], '')) CostCentreAssetManager_Director
,CONVERT(NVARCHAR(255), ISNULL(r.[Letting manager], '')) CostCentreLettingManager
,CONVERT(NVARCHAR(255), ISNULL(r.[Property manager], '')) CostCentrePropertyManager
,CONVERT(NVARCHAR(255), ISNULL(r.[Oblastní facility manager], '')) CostCentreFacilityManager
,CONVERT(NVARCHAR(255), ISNULL(r.[Contract administrator], '')) CostCentreContractAdministrator
,CONVERT(NVARCHAR(255), ISNULL(r.[Biller], '')) CostCentreBiller
,CONVERT(NVARCHAR(255), ISNULL([Reporting administrator - Property], '')) CostCentreReportingAdministrator_Property
,CONVERT(NVARCHAR(255), ISNULL([Receivable administrator], '')) CostCentreReceivableAdministrator
FROM roles r
INNER JOIN TG.CostCentre cc ON cc.id = r.centerId
WHERE ccSrcId IS NOT NULL AND cc.ccInAx = 1
";

            ParseSqlQuery(projectConfig.DatabaseComponents.First(x => x.DbName == "CPI_DSA"), projectConfig, graphManager, query);
        }

        private SqlScriptElement ParseSqlQuery(MssqlDbProjectComponent dbComponent, ProjectConfig projectConfig, GraphManager graphManager, string query, out Dictionary<string, MssqlModelElement> outputColumns)
        {

            AvailableDatabaseModelIndex adbix = new AvailableDatabaseModelIndex(projectConfig, graphManager);
            
            var sqlParser = new ScriptModelParser();
            
            var dbIndex = adbix.GetDatabaseIndex(dbComponent.ServerName, dbComponent.ServerName);
            sqlParser.ContextServerName = dbIndex.ContextServerName;

            var dbIdentifier = new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier() { Value = dbComponent.DbName };

            //Dictionary<string, MssqlModelElement> outputColumns;

            MssqlModelElement fakeParent = new SolutionModelElement(new RefPath(""), "root");
            var sqlNode = sqlParser.ExtractScriptModel(query, fakeParent, dbIndex, dbIdentifier, out outputColumns);

            return sqlNode;
        }

        private SqlScriptElement ParseSqlQuery(MssqlDbProjectComponent dbComponent, ProjectConfig projectConfig, GraphManager graphManager, string query)
        {
            Dictionary<string, MssqlModelElement> outputColumns;
            var sqlNode = ParseSqlQuery(dbComponent, projectConfig, graphManager, query, out outputColumns);
                
            return sqlNode;
        }
    }
}
