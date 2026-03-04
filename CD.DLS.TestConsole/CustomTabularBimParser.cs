using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.DAL.Objects.Inspect;
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


    class CustomTabularBimParser
    {

        const string EXPORT_FOLDER = @"C:\projects\Metadata\BIM\run_20260219_184744";

        public void Run()
        {



            
            var summaryFilePath = Path.Combine(EXPORT_FOLDER, "run_summary.json");
            var summaryJson = File.ReadAllText(summaryFilePath);
            JObject summary = JObject.Parse(summaryJson);
            var extracts = ExtractWorkspacesScan(summary);

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
                //if (dataset.modelName != "Distribution Quality")
                //{
                //    continue;
                //}

                //if (dataset.modelName == "Distribution Quality")
                //{
                //    continue;
                //}

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
        }

        private List<TabularModel> ExtractWorkspacesScan(JObject summary)
        {
            JArray workspaces = (JArray)summary["Workspaces"];

            Dictionary<string, TabularDataSource> datasources = new Dictionary<string, TabularDataSource>();
            List<TabularModel> res = new List<TabularModel>();

            foreach (var workspace in workspaces)
            {
                var workspaceId = (string)workspace["WorkspaceId"];
                var workspaceName = (string)workspace["WorkspaceName"];

                // FOR NOW
                if(workspaceName != "Packeta Reports")
                {
                    continue;
                }
                
                var datasets = ((JArray)summary["Results"]).Where(x => (string)x["WorkspaceId"] == workspaceId);
                foreach (JObject dataset in datasets)
                {

                    var status = (string)dataset["Status"];
                    if(status != "Success")
                    {
                        continue;
                    }

                    var modelName = (string)dataset["DatasetName"];
                    var bimFilePath = $"{EXPORT_FOLDER}/{workspaceName}/{modelName}.bim";
                    var bimJson = File.ReadAllText(bimFilePath);
                    var bim = JObject.Parse(bimJson);
                    


                    // TODO remove later!
                    //if (modelName != "Consigned Packets by Clients")
                    //if (modelName != "Management Report")
                    //{
                    //    continue;
                    //}

                    TabularModel datasetModel = ExtractDatasetModel(bim, workspaceId, workspaceName);
                    res.Add(datasetModel);
                }
            }

            return res;
        }

        private Dictionary<string, TabularDataSource> ExtractSources(JArray jDataSources)
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

        private TabularModel ExtractDatasetModel(JObject jDataset, string workspaceId, string workspaceName)
        {
            TabularModel res = new TabularModel()
            {
                Id = (string)jDataset["id"],
                modelName = (string)jDataset["name"],
                WorkspaceName = workspaceName,
                ContentProviderType = "", //(string)jDataset["contentProviderType"],
                TabularDataSources = new List<TabularDataSource>(),
                TabularTables = new List<TabularTable>(),
                Relationships = new List<TabularRelationship>(),
                Perspectives = new List<TabularPerspective>(),
                Cultures = new List<TabularCulture>()
            };

            //foreach (var datasource in datasources.Values)
            //{
            //    res.TabularDataSources.Add(datasource);
            //}

            var model = jDataset["model"];

            foreach (JObject jTable in model["tables"])
            {
                TabularTable table = ExtractTable(jTable);
                
                res.TabularTables.Add(table);
            }

            return res;
        }

        private TabularTable ExtractTable(JObject jTable)
        {
            TabularTable table = new TabularTable()
            {
                Name = (string)jTable["name"],
                Columns = new List<TabularTableColumn>(),
                Measures = new List<TabularTableMeasure>()
            };

            if (jTable.ContainsKey("columns"))
            {
                foreach (JObject jColumn in jTable["columns"])
                {
                    TabularTableColumn column = new TabularTableColumn()
                    {
                        Name = (string)jColumn["name"],
                        DataType = (string)jColumn["dataType"]
                    };
                    column.ColumnType = TabularTableColumnTypeEnum.Data;
                    if (jColumn["type"] != null)
                    {
                        var type = (string)jColumn["type"];
                        if (type == "calculated")
                        {
                            type = "Calculated";
                        }
                        if (type == "calculatedTableColumn")
                        {
                            type = "CalculatedTableColumn";
                        }
                        column.ColumnType = (TabularTableColumnTypeEnum)Enum.Parse(typeof(TabularTableColumnTypeEnum), type);
                    }
                    if (column.ColumnType == TabularTableColumnTypeEnum.Calculated)
                    {
                        if (jColumn["expression"] == null)
                        {
                            column.Expression = "";
                        }
                        else if (jColumn["expression"] is JValue)
                        {
                            column.Expression = (string)jColumn["expression"];
                        }
                        else
                        {
                            column.Expression = string.Join("\n", jColumn["expression"].ToArray().ToList());
                        }
                        //column.Expression = (string)jColumn["expression"];
                    }
                    if (column.ColumnType == TabularTableColumnTypeEnum.Data)
                    {
                        column.SourceColumn = column.Name;
                    }
                    table.Columns.Add(column);
                }
            }

            if (jTable.ContainsKey("measures"))
            {
                foreach (JObject jMeasure in jTable["measures"])
                {
                    string expression = "";
                    if (jMeasure["expression"] == null)
                    {
                        expression = (string)jMeasure["value"];
                    }
                    else if (jMeasure["expression"] is JValue)
                    {
                        expression = (string)jMeasure["expression"];
                    }
                    else
                    {
                        expression = string.Join("\n", jMeasure["expression"].ToArray().ToList());
                    }

                    TabularTableMeasure measure = new TabularTableMeasure()
                    {
                        Name = (string)jMeasure["name"],
                        Expression = expression
                        //Expression = (string)jMeasure["expression"]
                    };
                    table.Measures.Add(measure);
                }
            }

            var partitionCount = 0;
            foreach (JObject jPartition in jTable["partitions"])
            {
                partitionCount++;


                // output = input
                //.Replace("#(lf)", "\n")
                //.Replace("#(tab)", "\t");
                


                var mode = jPartition["mode"] != null ? (string)jPartition["mode"] : "import";
                if (mode == "import" || mode == "directQuery")
                {
                    var source = jPartition["source"];
                    var sourceType = jPartition["source"]["type"] != null ? (string)jPartition["source"]["type"] : "m";

                    TabularPartitionSourceTypeEnum partitionType = TabularPartitionSourceTypeEnum.MLanguagePartitionSource;
                    if (sourceType == "m")
                    {

                    }
                    else if (sourceType == "calculated")
                    {
                        partitionType = TabularPartitionSourceTypeEnum.CalculatedPartitionSource;
                    }
                    else if (sourceType == "entity")
                    {
                        // TODO: direct lake!
                        continue;
                    }
                    else if (sourceType == "policyRange")
                    {
                        // incremental refresh (probably)
                        continue;
                    }
                    else
                    {

                    }

                    //var expression = string.Join("\n", ((JArray)(source["expression"])).Select(x => (string)x));
                    var expression = "";
                    if (source["expression"] is JValue)
                    {
                        expression = (string)source["expression"];
                    }
                    else if (source["expression"] == null)
                    {
                        // TODO: probably direct query from one semantic model to another
                        continue;
                    }
                    else
                    {
                        expression = string.Join("\n", source["expression"].ToArray().ToList());
                    }
                    //var expression = string.Join("\n", source["expression"].ToArray().ToList());

                    // TODO: replace sever name w/ localhost


                    expression = expression.Replace("packeta-dwh-01.public.3369f8c4c9d0.database.windows.net, 3342", "PRAHAK-1042");
                    expression = expression.Replace("packeta-dwh-01.public.3369f8c4c9d0.database.windows.net, 3342".ToUpper(), "PRAHAK-1042");

                    expression = expression.Replace("packeta-dwh-01.public.3369f8c4c9d0.database.windows.net,3342", "PRAHAK-1042");
                    expression = expression.Replace("packeta-dwh-01.public.3369f8c4c9d0.database.windows.net,3342".ToUpper(), "PRAHAK-1042");


                    expression = expression.Replace("packeta-dwh-01.public.3369f8c4c9d0.database.windows.net", "PRAHAK-1042");
                    expression = expression.Replace("packeta-dwh-01.public.3369f8c4c9d0.database.windows.net".ToUpper(), "PRAHAK-1042");

                    expression = expression.Replace("anevpbm6lp7ehhg2wzj73hd3cm-j5bzt6dcd2rufdr6xyhhffdm7y.datawarehouse.fabric.microsoft.com", "PRAHAK-1042");
                    expression = expression.Replace("anevpbm6lp7ehhg2wzj73hd3cm-j5bzt6dcd2rufdr6xyhhffdm7y.datawarehouse.fabric.microsoft.com".ToUpper(), "PRAHAK-1042");
                    
                    expression = expression
                    .Replace("#(lf)", "\n")
                    .Replace("#(tab)", "\t");

                    TabularTablePartition partition = new TabularTablePartition()
                    {
                        Expression = expression,
                        PartitionSourceType = partitionType,
                        Name = $"Partition{partitionCount}",
                        Query = expression
                    };
                    table.Partitions.Add(partition);
                }
                else
                { 
                
                }
            }

            return table;
        }
    }
}
