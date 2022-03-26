using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Extract.PowerBi.PowerBiAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Microsoft.SqlServer.ReportingServices2010;
using System.Net;
using System.Security.Principal;
using Newtonsoft.Json.Linq;

namespace CD.DLS.Extract.PowerBi
{
    public class PowerBiExtractor
    {
        private PowerBiProjectComponent powerBiProject;
        private string inputDirPath;
        private string outputDirPath;
        private string dataMashupPath;
        private string reportLayoutPath;
        private string diagramLayoutPath;
        private string connectionsPath;
        private string columnNames = "FillColumnNames";
        private string formulatext = "LastAnalysisServicesFormulaText";
        private PBIAPIClient pbic = null;
        private Manifest manifest;
        private string relativePathBase;

        private string userName;
        private string password;

        private string currentReportName = null;

        public PowerBiExtractor(PowerBiProjectComponent powerBiproject, string relativePathBase, string outputDirPath, Manifest manifest, string userName, string password)
        {
            this.powerBiProject = powerBiproject;
            this.outputDirPath = outputDirPath;
            this.manifest = manifest;
            this.relativePathBase = relativePathBase;

            this.userName = userName;
            this.password = password;         
        }

        public void Extract()
        {
            if (powerBiProject.ConfigType == PowerBiProjectConfigType.ReportServer)
            {
                ReportsFromService(outputDirPath);
            }
            else if (powerBiProject.ConfigType == PowerBiProjectConfigType.DiskFolder)
            {
                ReportsFromDiskFolder(outputDirPath);
            }
            else
            {
                ReportsFromReportServer(outputDirPath);
            }


        }

        private string GetUniqueReportItemName(string folder, string itemName)
        {
            var dirExtract = Path.Combine(folder, itemName);
            int count = 1;
            while (Directory.Exists(dirExtract))
            {
                dirExtract = Path.Combine(folder, itemName + $" ({count})");
                count++;
            }
            var resName = Path.GetFileName(dirExtract);
            return resName;
        }

        private void ExtractReportZip(string pbixPath, string dirExtract)
        {
            
            using (ZipArchive archive = ZipFile.OpenRead(pbixPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!entry.FullName.Contains("/") || entry.Name == "Layout")
                    {
                        string destinationPath = Path.GetFullPath(Path.Combine(dirExtract, entry.FullName));
                        var destinationDir = Path.GetDirectoryName(destinationPath);
                        if (!Directory.Exists(destinationDir))
                        {
                            Directory.CreateDirectory(destinationDir);
                        }

                        entry.ExtractToFile(destinationPath);
                    }
                    else
                    { 
                    
                    }
                    //if (entry.Name .FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    // Gets the full path to ensure that relative segments are removed.
                    //    string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                    //    // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                    //    // are case-insensitive.
                    //    if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                    //        entry.ExtractToFile(destinationPath);
                    //}
                }
            }

        }

        public void ReportsFromReportServer(string outputDirPath)
        {
            var dir = Path.Combine(outputDirPath, "dir");
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
            Directory.CreateDirectory(dir);
            var url = ""; //example: "http://127.0.0.1/Reports/api/v2.0/catalogitems(89304C36-026D-4A73-AE96-F90CBE2DD774)/Content/$value";
            var urlSplit = Regex.Matches(powerBiProject.ReportServerURL, ".+?(?=\\/)", RegexOptions.CultureInvariant);

            url = urlSplit[0].Value + urlSplit[1].Value + urlSplit[2].Value + "/reportserver/reportservice2010.asmx";

            ConfigManager.Log.Important(string.Format("Extracting PowerBI items  {0},  {1}", powerBiProject.ReportServerURL, powerBiProject.ReportServerFolder));

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            using (identity.Impersonate())
            {
                using (var client = new WebClient())
                {
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;
                    client.UseDefaultCredentials = true;
                    var rs = new ReportingService2010
                    {
                        Credentials = CredentialCache.DefaultCredentials,
                        Url = url
                    };                  
                    var items = rs.FindItems(powerBiProject.ReportServerFolder, BooleanOperatorEnum.Or, new Microsoft.SqlServer.ReportingServices2010.Property[] { }, new SearchCondition[] { });

                    foreach (var item in items)
                    {
                        if (item.TypeName == "PowerBIReport")
                        {
                            var uniqueItemName = GetUniqueReportItemName(dir, item.Name);
                            this.currentReportName = item.Name;
                            var file = Path.Combine(dir, uniqueItemName /*item.Name*/ + ".zip");                          
                            url = String.Format("{0}/api/v2.0/catalogitems({1})/Content/$value", powerBiProject.ReportServerURL, item.ID);
                            client.DownloadFile(url, file);
                            var dirExtract = Path.Combine(dir, uniqueItemName /*item.Name*/);
                            ExtractReportZip(file, dirExtract);
                            //ZipFile.ExtractToDirectory(file, dirExtract);
                        }
                    }
                }
            }

            List<Report> extractedReports = ExtractReports();
            Tenant tenant = new Tenant(powerBiProject.ReportServerURL, extractedReports);

            string tenantString = tenant.Serialize();
            var fileName = urlSplit[2].Value.Trim('/') + ".json";
            var path = Path.Combine(outputDirPath, fileName);
            File.WriteAllText(path, tenantString);

            manifest.Items.Add(new ManifestItem()
            {
                ComponentId = powerBiProject.PowerBiProjectComponentId,
                Name = fileName,
                ExtractType = "PowerBi",
                RelativePath = Path.Combine(relativePathBase, fileName)
            });

            Directory.Delete(Path.Combine(outputDirPath, "dir"), true);
        }

        public void ReportsFromDiskFolder(string outputDirPath)
        {
            var dir = Path.Combine(outputDirPath, "dir");
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
            Directory.CreateDirectory(dir);
            var pbixFiles = Directory.GetFiles(powerBiProject.DiskFolder, "*.pbix", SearchOption.AllDirectories);
            ConfigManager.Log.Important($"Loading Power BI reports from {powerBiProject.DiskFolder}");
            foreach (var pbix in pbixFiles)
            {
                var name = Path.GetFileNameWithoutExtension(pbix);
                name = GetUniqueReportItemName(dir, name);
                
                this.currentReportName = name;
                //var file = Path.Combine(dir, name + ".zip");

                var dirExtract = Path.Combine(dir, name);
                ConfigManager.Log.Important(pbix);
                ExtractReportZip(pbix, dirExtract);
                //ZipFile.ExtractToDirectory(pbix, dirExtract);
                /*
                try
                {
                    ZipFile.ExtractToDirectory(pbix, dirExtract);
                }
                catch (Exception ex)
                {
                    ConfigManager.Log.Important($"{pbix} had to be skipped - {ex.Message}");
                    Directory.Delete(dirExtract, true);
                }
                */
            }

            List<Report> extractedReports = ExtractReports();
            Tenant tenant = new Tenant(powerBiProject.DiskFolder, extractedReports);

            string tenantString = tenant.Serialize();
            var fileName = Path.GetFileName(powerBiProject.DiskFolder) + "_" + powerBiProject.PowerBiProjectComponentId.ToString() + ".json";
            var path = Path.Combine(outputDirPath, fileName);
            File.WriteAllText(path, tenantString);

            manifest.Items.Add(new ManifestItem()
            {
                ComponentId = powerBiProject.PowerBiProjectComponentId,
                Name = fileName,
                ExtractType = "PowerBi",
                RelativePath = Path.Combine(relativePathBase, fileName)
            });

            Directory.Delete(Path.Combine(outputDirPath, "dir"), true);
        }

        private void ReportsFromService(string outputDirPath)
        {
            pbic = new PBIAPIClient(powerBiProject.ApplicationID, powerBiProject.RedirectUri, userName, password);
            if (powerBiProject.WorkspaceID != null)
            {
                pbic.SetWorkspaceID(powerBiProject.WorkspaceID);
            }
            ConfigManager.Log.Important(string.Format("Extracting PowerBI items  {0},  {1}", powerBiProject.ApplicationID, powerBiProject.RedirectUri));

            PBIGroup myGroup = pbic;

            var dir = Path.Combine(outputDirPath, "dir");
            var reports = myGroup.Reports;
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
            Directory.CreateDirectory(dir);
            foreach (PBIReport r in reports)
            {
                var uniqueItemName = GetUniqueReportItemName(dir, r.Name);
                var file = Path.Combine(dir, uniqueItemName /*r.Name*/ + ".zip");
                try
                {                  
                    r.Export(file);
                    this.currentReportName = r.Name;
                }
                catch 
                {
                    continue;
                }
                

                var dirExtract = Path.Combine(dir, uniqueItemName /*r.Name*/);
                ExtractReportZip(file, dirExtract);
                //ZipFile.ExtractToDirectory(file, dirExtract);
            }

            List<Report> extractedReports = ExtractReports();
            Tenant tenant = new Tenant(powerBiProject.ApplicationID, extractedReports);

            string tenantString = tenant.Serialize();
            var fileName = pbic.TenantId + ".json";
            var path = Path.Combine(outputDirPath, fileName);
            File.WriteAllText(path, tenantString);

            manifest.Items.Add(new ManifestItem()
            {
                ComponentId = powerBiProject.PowerBiProjectComponentId,
                Name = fileName,
                ExtractType = "PowerBi",
                RelativePath = Path.Combine(relativePathBase, fileName)
            });

            Directory.Delete(Path.Combine(outputDirPath, "dir"), true);
        }

        public List<Report> ExtractReports()
        {
            List<Report> reports = new List<Report>();
            var directories = Directory.GetDirectories(Path.Combine(outputDirPath, "dir"));
            for (int i = 0; i < directories.Length; i++)
            {
                inputDirPath = directories[i];
                dataMashupPath = inputDirPath + @"\DataMashup";
                reportLayoutPath = inputDirPath + @"\Report\Layout";
                diagramLayoutPath = inputDirPath + @"\DiagramLayout";
                connectionsPath = inputDirPath + @"\Connections";

                List<Connection> connections = null;

                if (!File.Exists(connectionsPath)) // If this file DOES not exists, the connections are IMPORT type
                {
                    connections = ExtractConnectionsImportMode(GetEntryValue(LoadXml(dataMashupPath), formulatext));
                }
                else // If file exist then LIVE CONNECTION type
                {
                    connections = ExtractConnectionsLiveConnection();

                }
               
                List<ReportSection> sections = ExtractSections(GetVisualLayout(reportLayoutPath), connections);
                Filter[] filters = GetVisualLayout(reportLayoutPath).GetFilters();
                string name = Path.GetFileName(inputDirPath); // this.currentReportName;
                reports.Add(new Report(name, connections, sections, filters));
            }

            return reports;
        }

        public Report ExtractPbixForTest(string pbixPath)
        {
            var fileName = Path.GetFileName(pbixPath);
            this.currentReportName = fileName;
            var extractDirRoot = Path.Combine(outputDirPath, "dir");
            if (Directory.Exists(extractDirRoot))
            {
                Directory.Delete(extractDirRoot, true);
            }
            Directory.CreateDirectory(extractDirRoot);
            var dirExtract = Path.Combine(extractDirRoot, fileName);
            ZipFile.ExtractToDirectory(pbixPath, dirExtract);

            var reports = ExtractReports();
            return reports.First();
        }

        private  XmlDocument LoadXml(string dir)
        {
            if (!File.Exists(dir))
            {
                return null;
            }
            var xmlString = System.IO.File.ReadAllText(dir);
            var splits = Regex.Split(xmlString, @"(?=(<\?xml))");
            var regExp = Regex.Match(splits[4], "<.*>", RegexOptions.CultureInvariant).Value;
            regExp = regExp.Replace((char)0x16, '\0');
            regExp = regExp.Replace("&quot;", "'");
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(regExp);
            return xdoc;
        }

        private List<string> GetEntryValue(XmlDocument inputXml, string entryType)
        {
            List<string> parametersValue = new List<string>();
            if (inputXml == null)
            {
                return parametersValue;
            }
            var nodes = inputXml.SelectNodes("//Entry[@Type='" + entryType + "']");
            foreach (XmlNode node in nodes)
            {
                parametersValue.Add(node.Attributes["Value"].Value);
            }
            return parametersValue;
        }

        /*
        private  string ExtractReportName(int index)
        {
            List<string> names = new List<string>();
            PBIGroup myGroup = pbic;
            return myGroup.Reports[index].Name;
        }
        */

        private List<Connection> ExtractConnectionsLiveConnection()
        {
            List<Connection> connections = new List<Connection>();

            var jsonString = System.IO.File.ReadAllText(connectionsPath);
            LiveConnectionDataSource liveConnection = JsonConvert.DeserializeObject<LiveConnectionDataSource>(jsonString);

            if (liveConnection.Connections == null)
            {
                return connections;
            }

            for (int i = 0; i<liveConnection.Connections.Length; i++)
            {
                Connection conn = new Connection(i, null, null, null);
                conn.Type = liveConnection.Connections[i].ConnectionType;
                var dataSource = Regex.Match(liveConnection.Connections[i].ConnectionString, "(?<=Source=).+?(?=;)", RegexOptions.CultureInvariant).Value;
                var catalog = Regex.Match(liveConnection.Connections[i].ConnectionString, "(?<=Catalog=).+?(?=;)", RegexOptions.CultureInvariant).Value;
                if (catalog == "")
                {
                    var connstring = liveConnection.Connections[i].ConnectionString;
                    var split = connstring.Split(';');
                    var catalogSplit = split.FirstOrDefault(x => x.Contains("Catalog="));
                    if (catalogSplit != null)
                    {
                        catalog = catalogSplit.Substring(catalogSplit.LastIndexOf("=")+1).Trim();
                    }
                }
                conn.Source = dataSource + "\\" + catalog;
                conn.Tables = ExtractTableNamesLiveConnection();
                connections.Add(conn);
            }
            return connections;
        }

        private  List<Connection> ExtractConnectionsImportMode(List<string> entry)
        {
            List<Connection> connections = new List<Connection>();
            for (int i = 0; i < entry.Count; i++)
            {
                Connection conn = new Connection(i, null, null, null);
                conn.Type = Regex.Match(entry[i], "(?<=Source = ).+?(?=\\()", RegexOptions.CultureInvariant).Value;
                conn.Source = ExtractConnectionString(entry[i], conn);

                var table = ExtractColumnsImportMode(GetEntryValue(LoadXml(dataMashupPath), columnNames)[i], conn);

                bool duplicity = false;
                foreach (Connection connection in connections)
                {
                    if (connection.Source == conn.Source && connection.Type == conn.Type)
                    {
                        connection.Tables.Add(table);
                        duplicity = true;
                        break;
                    }
                }
                if (duplicity == true)
                {
                    continue;
                }
                conn.Tables.Add(table);
                connections.Add(conn);
            }
            return connections;
        }

        private List<PbiTable> ExtractTableNamesLiveConnection()
        {
            List<PbiTable> tmp = new List<PbiTable>();
            if (!File.Exists(diagramLayoutPath))
            {
                return tmp;
            }
            var jsonString = System.IO.File.ReadAllText(diagramLayoutPath,Encoding.Unicode);
            jsonString = jsonString.Replace(" ", "");
            Example table = JsonConvert.DeserializeObject<Example>(jsonString);
            foreach (Diagram d in table.diagrams)
            {
                if (d.tables == null)
                {
                    continue;
                }
                foreach (string t in d.tables)
                {
                    PbiTable tbl = new PbiTable(t);
                    tmp.Add(tbl);
                }
            }

            return tmp;
        }


        private string ExtractTableNameImportMode(string entry, Connection con)
        {
            string tableName = null;
            switch (con.Type)
            {
                case "Sql.Databases":
                //case "Sql.Database":
                    var splitDb = entry.Split(new string[] { "\\r\\n" }, StringSplitOptions.None)[3].Trim(' ');
                    tableName = Regex.Match(splitDb, "(?<=).+?(?= )", RegexOptions.CultureInvariant).Value;
                    break;
                case "Excel.Workbook":
                    var splitExcel = entry.Split(new string[] { "\\r\\n" }, StringSplitOptions.None)[2].Trim(' ');
                    var splitPathExcel = splitExcel.Split(new string[] { "\\" }, StringSplitOptions.None);
                    tableName = Regex.Match(splitExcel, "(?<=Item=\\\\').+?(?=\\\\')", RegexOptions.Multiline).Value;
                    break;
                case "OData.Feed":
                    var splitOdata = entry.Split(new string[] { "\\r\\n" }, StringSplitOptions.None)[2].Trim(' ');
                    tableName = Regex.Match(splitOdata, "(?<=).+?(?= )", RegexOptions.CultureInvariant).Value;
                    break;
                case "Table.FromColumns":
                    var splitFromColumns = entry.Split(new string[] { "\\r\\n" }, StringSplitOptions.None)[1];
                    var splitPath = splitFromColumns.Split(new string[] { "\\" }, StringSplitOptions.None);
                    tableName = System.IO.Path.GetFileNameWithoutExtension(splitPath[splitPath.Length - 2]);
                    break;
                case "AnalysisServices.Databases":
                //case "AnalysisServices.Database":
                    var splitAs = entry.Split(new string[] { "\\r\\n" }, StringSplitOptions.None)[4];
                    tableName = Regex.Match(splitAs, "(?<=Id=).+?(?=])", RegexOptions.Multiline).Value.Trim('\'', '\\'); 
                    break;
            }
            return tableName;
        }

        private string ExtractConnectionString(string entry, Connection con)
        {
            string connectionString = null;
            switch (con.Type)
            {
                case "Sql.Databases":
                case "AnalysisServices.Databases":
                case "Sql.Database":
                case "AnalysisServices.Database":
                case "Excel.Workbook":
                case "Table.FromColumns":
                    var splits = entry.Split(new string[] { "\\r\\n" }, StringSplitOptions.None);
                    foreach (string split in splits)
                    {
                        if (split.Contains("Source") && connectionString != null)
                        {
                            var regExp = Regex.Matches(split, "(?<=\').+?(?=\\')", RegexOptions.CultureInvariant);
                            int count = 0;
                            foreach (Match match in regExp)
                            {
                                if (match.Value.Contains(","))
                                {
                                    continue;
                                }
                                connectionString += match.Value;
                                count++;
                                if (count == 2)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    break;

                case "OData.Feed":
                    var splitsOdata = entry.Split(new string[] { "\\r\\n" }, StringSplitOptions.None);
                    foreach (string split in splitsOdata)
                    {
                        if (split.Contains("Source"))
                        {
                            var regExp = Regex.Match(split, "(?<=\').+?(?=\\')", RegexOptions.CultureInvariant);
                            connectionString += regExp.Value;
                            connectionString = connectionString.Trim('\\');
                            break;
                        }
                    }
                    break;
            }
            return connectionString;
        }

        private PbiTable ExtractColumnsImportMode(string entry, Connection con)
        {
            PbiTable tablesList = new PbiTable(ExtractTableNameImportMode(GetEntryValue(LoadXml(dataMashupPath), formulatext)[con.ConnId], con));
            List<PbiColumn> column = new List<PbiColumn>();
            var columnnames = Regex.Match(entry, "(?<=\\[).+?(?=\\])", RegexOptions.CultureInvariant).Value;
            var splits = columnnames.Split(',');
            var customQuery = ExtractCustomCollumnQuery(GetEntryValue(LoadXml(dataMashupPath), formulatext)[con.ConnId]);
            var query = ExtractQuery(GetEntryValue(LoadXml(dataMashupPath), formulatext)[con.ConnId], con);
            if (customQuery.Count == 0)
            {
                for (int j = 0; j < splits.Length; j++)
                {
                    splits[j] = splits[j].Trim('\'');
                    PbiColumn col = new PbiColumn(splits[j], null)
                    {
                        Querry = query
                    };
                    column.Add(col);
                }
            }
            else
            {
                for (int j = 0; j < splits.Length; j++)
                {
                    splits[j] = splits[j].Trim('\'');
                    PbiColumn col = new PbiColumn(splits[j], null);
                    for (int k = 0; k < customQuery.Count; k++)
                    {
                        if (customQuery[k].Contains("\\'" + splits[j] + "\\'"))
                        {
                            col.Querry = customQuery[k];
                            break;
                        }
                        else
                        {
                            col.Querry = query;
                        }
                    }
                    column.Add(col);
                }
            }
            tablesList.Columns = column;
            return tablesList;
        }

        private  List<string> ExtractCustomCollumnQuery(string entry)
        {
            List<string> query = new List<string>();
            var splits = entry.Split(new string[] { "\\r\\n" }, StringSplitOptions.None);
            foreach (string split in splits)
            {
                if (split.Contains("Added Custom"))
                {
                    var regExp = Regex.Match(split, "(?<= = ).+?(?=\\))", RegexOptions.CultureInvariant).Value;
                    query.Add(regExp);
                }
            }
            return query;
        }

        private string ExtractQuery(string entry, Connection con)
        {
            string query = null;
            switch (con.Type)
            {
                case "Sql.Databases":
                //case "Sql.Database":
                case "Excel.Workbook":
                    var splitsDb = entry.Split(new string[] { "\\r\\n" }, StringSplitOptions.None);
                    var splitDb = splitsDb[3].Trim(' ');
                    query = Regex.Match(splitDb, " =.+", RegexOptions.CultureInvariant).Value;
                    break;

                case "AnalysisServices.Databases":
                //case "AnalysisServices.Database":
                    var splitsAs = entry.Split(new string[] { "\\r\\n" }, StringSplitOptions.None);
                    var ssas = Regex.Match(entry, "(= Cube).+?(}\\))", RegexOptions.CultureInvariant).Value;
                    query = Regex.Replace(ssas, @"\\r\\n", String.Empty);
                    break;

                case "OData.Feed":
                    var splitsOdata = entry.Split(new string[] { "\\r\\n" }, StringSplitOptions.None);
                    query = Regex.Match(splitsOdata[2], "=.+").Value;
                    break;

                case "Table.FromColumns":
                    var splits = entry.Split(new string[] { "\\r\\n" }, StringSplitOptions.None)[1].Trim(' '); ;
                    query = Regex.Match(splits, "=.+").Value;
                    break;
            }
            return query;
        }

        private Layout GetVisualLayout(string jsonPath)
        {
            string json = File.ReadAllText(jsonPath, Encoding.Unicode);
            Layout layout = JsonConvert.DeserializeObject<Layout>(json);
            foreach (var section in layout.Sections)
            {
                foreach (var visualContainer in section.VisualContainers)
                {
                    visualContainer.ExtensionMeasures = new List<VisualContainerExtensionMeasure>();

                    if (visualContainer.Query != null)
                    {
                        var queryParsed = JObject.Parse(visualContainer.Query);
                        var commands = queryParsed.GetValue("Commands") as JArray;
                        if (commands == null)
                        {
                            continue;
                        }
                        if (commands.Count > 1)
                        {
                            ConfigManager.Log.Error("Unexpected multiple commands in PBI visual container query definition");
                            //continue;
                        }

                        var cmd = (JObject)commands.First;
                        var semanticCommand = cmd.GetValue("SemanticQueryDataShapeCommand") as JObject;
                        if (semanticCommand == null)
                        {
                            continue;
                        }

                        var extension = semanticCommand.GetValue("Extension") as JObject;
                        if (extension == null)
                        {
                            continue;
                        }

                        var entities = extension.GetValue("Entities") as JArray;
                        if (entities == null)
                        {
                            continue;
                        }

                        //var names = new string[] { "Extends", "Name", "Measures" };

                        foreach (JObject entity in entities)
                        {
                            var entityExtends = entity.GetValue("Extends").Value<string>();
                            var measures = entity.GetValue("Measures") as JArray;
                            if (measures == null)
                            {
                                continue;
                            }

                            foreach (JObject measure in measures)
                            {
                                var measureName = measure.GetValue("Name").Value<string>();
                                var measureExpression = measure.GetValue("Expression").Value<string>();

                                var extensionMeasure = new VisualContainerExtensionMeasure()
                                {
                                    Expression = measureExpression,
                                    MeasureName = measureName,
                                    TableName = entityExtends
                                };
                                visualContainer.ExtensionMeasures.Add(extensionMeasure);

                            }

                            //var entityName = entity.GetValue("Name").Value<string>();

                            //if (extends != entityName)
                            //{ 
                            
                            //}
                            //foreach (JProperty child in entity.Children())
                            //{
                            //    var name = child.Name;
                            //    if (!names.Contains(name))
                            //    { 
                                
                            //    }
                            //}
                        }


                    }
                }
            }
            return layout;
        }

        private List<ReportSection> ExtractSections(Layout layout, List<Connection> connections)
        {
            List<ReportSection> sections = new List<ReportSection>();
            foreach (Section section in layout.Sections)
            {
                var visuals = ExtractVisuals(section.VisualContainers, connections, section);
                sections.Add(new ReportSection(visuals, section.Name, section.DisplayName, section.GetFilters()));
            }
            return sections;
        }

        private List<Visual> ExtractVisuals(VisualContainer[] visualContainers, List<Connection> connections, Section section)
        {
            List<Visual> visuals = new List<Visual>();
            foreach (VisualContainer vc in visualContainers)
            {
                var config = vc.GetConfig();
                if (config.SingleVisual1 == null)
                {
                    ConfigManager.Log.Warning($"Skipping visual {vc.Id} in {section.Name} - visual not defined");
                    continue;
                }
                Visual visual = new Visual(vc.Id, config.SingleVisual1.VisualType, vc.GetFilters());
                PropertyInfo[] properties = typeof(VisualContainerConfig.Projections).GetProperties();
                if (config.SingleVisual1.Projections == null)
                {
                    continue;
                }
                foreach (PropertyInfo property in properties)
                {
                    Projection[] projections = (Projection[])property.GetValue(config.SingleVisual1.Projections);
                    if (projections != null)
                    {
                        foreach (var projection in projections)
                        {
                            visual.Projections.Add(new Projection(projection.QueryRef,property.Name));
                        }
                    }
                }
                visuals.Add(visual);
            }
            return visuals;
        }

    }
}
