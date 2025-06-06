﻿using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Extract.PowerBi;
using CD.DLS.Model;
using CD.DLS.Model.Mssql.Pbi;
using CD.DLS.Model.Mssql.Tabular;
using CD.DLS.Model.Serialization;
using CD.DLS.Parse.Mssql.Db;
using CD.DLS.Parse.Mssql.Ssrs.Rdl2008;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CD.DLS.TestConsole
{
    //public class AsConnection
    //{ 
    //    public string Path { get; set; }
    //    public string Name { get; set; }
    //    public string ConnectionType { get; set; }
    //    public string ConnectionServer { get; set; }
    //    public string ConnectionDb { get; set; }
    //}


    class Program
    {
        static void Main(string[] args)
        {


            string input = @"let
    a = [Client ID = 1, 
\#""Client Name"" = 2],
    b = something(Client ID = 3),
    c = [Plain Field = [something = 5]]
    d = [Client ID, User ID]
    e = [Client ID]
in
    result";

            string result = Regex.Replace(input, @"\[(.*?)\]", match =>
            {
                string listContent = match.Groups[1].Value;

                // Match unquoted items (whether or not they are assignments)
                string processed = Regex.Replace(listContent, @"(?<=^|,)\s*(?!\"")([A-Za-z0-9_]+(?:\s+[A-Za-z0-9_]+)+)(?=\s*(=|,|$))", m =>
                {
                    string item = m.Groups[1].Value;
                    return item.Replace(" ", "_");
                });

                return "[" + processed + "]";
            }, RegexOptions.Singleline);

            //Console.WriteLine(result);



            var scanJson = File.ReadAllText(@"C:\projects\Metadata\scanResult.json");
            var extracts = ExtractWorkspacesScan(scanJson);

            ProjectConfigManager projectConfigManager = new ProjectConfigManager();
            
            // Z1
            var projectConfig = projectConfigManager.GetProjectConfig(new Guid("BFDCDB34-0ABD-43D5-B6EC-75413CEC9578"));
            GraphManager graphManager = new GraphManager();

            var pbiUrnBuilder = new Parse.Mssql.Pbi.UrnBuilder();
            SerializationHelper sh = new SerializationHelper(projectConfig, graphManager);

            var solutionElement = (SolutionModelElement)sh.LoadElementModelToChildrenOfType("", typeof(SolutionModelElement));
            var premappedIds = sh.CreatePremappedModel(solutionElement);

            var tenantName = "Packeta";

            var tenantUrn = pbiUrnBuilder.GetTenantUrn(tenantName);
            var tenantElement = new Model.Mssql.Pbi.TenantElement(tenantUrn, tenantName, null, solutionElement);
            solutionElement.AddChild(tenantElement);

            var workspaceName = "Packeta Reports";

            var datasets = extracts.Where(x => x.WorkspaceName == workspaceName).ToList();
            var workspaceId = datasets.FirstOrDefault().Id;


            var workspaceUrn = pbiUrnBuilder.GetWorkspaceUrn(workspaceName, tenantElement.RefPath);
            var workspaceElement = new WorkspaceElement(workspaceUrn, workspaceName, null, tenantElement);
            tenantElement.AddChild(workspaceElement);

            Parse.Mssql.Ssas.UrnBuilder ssasUrnBuilder = new DLS.Parse.Mssql.Ssas.UrnBuilder();

            AvailableDatabaseModelIndex sqlDatabaseIndex = new Parse.Mssql.Db.AvailableDatabaseModelIndex(projectConfig, graphManager);
            BIDoc.Core.Parse.Mssql.Tabular.TabularParser tparser = new BIDoc.Core.Parse.Mssql.Tabular.TabularParser(sqlDatabaseIndex, projectConfig, Guid.Empty, null);


            var dsc = datasets.Count;
            var i = 0;
            foreach (var dataset in datasets)
            {
                i++;
                Console.WriteLine($"{dataset.modelName} ({i}/{dsc})");

                var datasetUrn = pbiUrnBuilder.GetDatasetUrn(dataset.modelName, workspaceElement.RefPath);
                var datasetElement = new DatasetElement(datasetUrn, dataset.modelName, null, workspaceElement);
                workspaceElement.AddChild(datasetElement);

                var dbRefPath = ssasUrnBuilder.GetDatabaseUrn("Model", datasetUrn);
                SsasTabularDatabaseElement tDatabaseElement = new SsasTabularDatabaseElement(dbRefPath, "Model", null, datasetElement);
                datasetElement.AddChild(tDatabaseElement);

                tparser.ExtractTabularModel(dataset, tDatabaseElement);

            }


            Console.WriteLine("Saving the model...");

            var dbIdMap = sqlDatabaseIndex.GetAllPremappedIds();
            foreach (var kv in dbIdMap)
            {
                if (!premappedIds.ContainsKey(kv.Key))
                {
                    premappedIds.Add(kv.Key, kv.Value);
                }
            }

            sh.SaveModelPart(tenantElement, premappedIds);

            //graphManager.SaveModelElements()
            //graphManager.ClearModel()

            return;

            
            // Z1
            
            //AvailableDatabaseModelIndex sqlDatabaseIndex = new Parse.Mssql.Db.AvailableDatabaseModelIndex(projectConfig, graphManager);

            var serverName = projectConfig.DatabaseComponents[0].ServerName;

            // PACKETA-DWH-01.3369F8C4C9D0.DATABASE.WINDOWS.NET
            var dbIx0 = sqlDatabaseIndex.GetDatabaseIndex(serverName);


            #region OLD_COMMENTED


            // var dbIx = adbix.GetDatabaseIndex("packeta-dwh-01.public.3369f8c4c9d0.database.windows.net");


            //List<AsConnection> asConnections = new List<AsConnection>();





            //var root = @"C:\Projects\Business Intelligence\Power BI\";
            ////var pbix = @"C:\Projects\Business Intelligence\Power BI\Customer Reporting\Customer NetworkTraffic Usage Report.pbix";
            //var pbixs = System.IO.Directory.GetFiles(root, "*.pbix", System.IO.SearchOption.AllDirectories); 
            //var extract = new PowerBiExtractor(null, null, "C:\\TEMP", null, null, null);
            //List<Report> reports = new List<Report>();
            //foreach (var pbix in pbixs)
            //{
            //    var report = extract.ExtractPbixForTest(pbix);
            //    reports.Add(report);
            //    foreach (var asc in report.Connections.Where(x => x.Type.Contains("analysisServices")))
            //    {
            //        var src = asc.Source;
            //        var srcSplit = src.Split('\\');
            //        asConnections.Add(new AsConnection()
            //        {
            //            Path = pbix,
            //            Name = report.Name,
            //            ConnectionType = asc.Type,
            //            ConnectionServer = srcSplit[0],
            //            ConnectionDb = srcSplit[1]
            //        });
            //    }
            //}

            //DataTable tbl = new DataTable();
            //tbl.Columns.Add("Path");
            //tbl.Columns.Add("Name");
            //tbl.Columns.Add("ConnectionType");
            //tbl.Columns.Add("ConnectionServer");
            //tbl.Columns.Add("ConnectionDb");

            //foreach (var asc in asConnections)
            //{
            //    var nr = tbl.NewRow();
            //    nr[0] = asc.Path;
            //    nr[1] = asc.Name;
            //    nr[2] = asc.ConnectionType;
            //    nr[3] = asc.ConnectionServer;
            //    nr[4] = asc.ConnectionDb;
            //    tbl.Rows.Add(nr);
            //}


            //var report = extract.ExtractPbix(pbix);

            #endregion
        }

        private static List<TabularModel> ExtractWorkspacesScan(string scanJson)
        {
            JObject jScan = JObject.Parse(scanJson);
            JArray jWorkspaces = (JArray)jScan["workspaces"];
            JArray jDataSources = (JArray)jScan["datasourceInstances"];

            Dictionary<string, TabularDataSource> datasources = ExtractSources(jDataSources);
            List<TabularModel> res = new List<TabularModel>();

            foreach (var workspace in jWorkspaces)
            {
                var workspaceId = (string)workspace["id"];
                var workspaceName = (string)workspace["name"];
                var datasets = (JArray)workspace["datasets"];
                foreach (JObject jDataset in datasets)
                {

                    var modelName = (string)jDataset["name"];

                    // TODO remove later!
                    //if (modelName != "Consigned Packets by Clients")
                    //if (modelName != "Management Report")
                    //{
                    //    continue;
                    //}

                    TabularModel datasetModel = ExtractDatasetModel(jDataset, workspaceId, workspaceName, datasources);
                    res.Add(datasetModel);
                }
            }

            return res;
        }

        private static Dictionary<string, TabularDataSource> ExtractSources(JArray jDataSources)
        {
            Dictionary<string, TabularDataSource> res = new Dictionary<string, TabularDataSource>();
            foreach (JObject ds in jDataSources)
            {
                var dsType = (string)ds["datasourceType"];
                var dsId = (string)ds["datasourceId"];
                var details = (JObject)ds["connectionDetails"];

                if (dsType != "Sql")
                {
                    continue;
                }
                var server = (string)details["server"];
                var database = (string)details["database"];

                res.Add(dsId, new TabularDataSource()
                {
                    DSname = dsId,
                    SourceType = dsType,
                    ServerName = server,
                    DatabaseName = database
                });
            }

            return res;
        }

        private static TabularModel ExtractDatasetModel(JObject jDataset, string workspaceId, string workspaceName, Dictionary<string, TabularDataSource> datasources)
        {
            TabularModel res = new TabularModel()
            {
                Id = (string)jDataset["id"],
                modelName = (string)jDataset["name"],
                WorkspaceName = workspaceName,
                ContentProviderType = (string)jDataset["contentProviderType"],
                TabularDataSources = new List<TabularDataSource>(),
                TabularTables = new List<TabularTable>(),
                Relationships = new List<TabularRelationship>(),
                Perspectives = new List<TabularPerspective>(),
                Cultures = new List<TabularCulture>()
            };

            foreach (var datasource in datasources.Values)
            {
                res.TabularDataSources.Add(datasource);
            }

            foreach (JObject jTable in jDataset["tables"])
            {
                TabularTable table = ExtractTable(jTable, datasources);
                
                res.TabularTables.Add(table);
            }

            return res;
        }

        private static TabularTable ExtractTable(JObject jTable, Dictionary<string, TabularDataSource> datasources)
        {
            TabularTable table = new TabularTable()
            {
                Name = (string)jTable["name"],
                Columns = new List<TabularTableColumn>(),
                Measures = new List<TabularTableMeasure>()
            };

            foreach (JObject jColumn in jTable["columns"])
            {
                TabularTableColumn column = new TabularTableColumn()
                {
                    Name = (string)jColumn["name"],
                    DataType = (string)jColumn["dataType"]
                };
                column.ColumnType = (TabularTableColumnTypeEnum)Enum.Parse(typeof(TabularTableColumnTypeEnum), (string)jColumn["columnType"]);
                if (column.ColumnType == TabularTableColumnTypeEnum.Calculated)
                {
                    column.Expression = (string)jColumn["expression"];
                }
                if (column.ColumnType == TabularTableColumnTypeEnum.Data)
                {
                    column.SourceColumn = column.Name;
                }
                table.Columns.Add(column);
            }

            foreach (JObject jMeasure in jTable["measures"])
            {
                TabularTableMeasure measure = new TabularTableMeasure()
                {
                    Name = (string)jMeasure["name"],
                    Expression = (string)jMeasure["expression"]
                };
                table.Measures.Add(measure);
            }

            var partitionCount = 0;
            foreach (JObject jPartition in jTable["source"])
            {
                partitionCount++;


                // output = input
                //.Replace("#(lf)", "\n")
                //.Replace("#(tab)", "\t");
                var expression = (string)jPartition["expression"];
                var expressionDeser = expression.Replace("#(lf)", "\n")
                    .Replace("#(tab)", "\t")
                    .Replace("#(cr)", "\r");


                TabularTablePartition partition = new TabularTablePartition()
                {
                    Expression = expression,
                    PartitionSourceType = TabularPartitionSourceTypeEnum.MLanguagePartitionSource,
                    Name = $"Partition{partitionCount}"
                };

                if (string.IsNullOrEmpty(partition.Expression))
                { 
                
                }
                table.Partitions.Add(partition);
            }

            return table;
        }
    }
}
