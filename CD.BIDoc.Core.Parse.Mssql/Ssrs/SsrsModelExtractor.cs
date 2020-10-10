using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Model.Mssql;
using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using CD.DLS.Parse.Mssql.Db;
using Microsoft.SqlServer.ReportingServices2010;
using System.Xml.Linq;
using CD.DLS.Model.Mssql.Ssrs;
using System.IO;
using CD.DLS.Parse.Mssql.Ssrs.Rdl2016;
using System.Xml.Serialization;
using System.Xml;
using CD.DLS.Parse.Mssql.Ssas;
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;
using Irony.Parsing;
using Newtonsoft.Json;
//using static CD.Framework.BIDocApi.ListSsrsReportsRequestResponse;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using static CD.DLS.Model.Mssql.Ssrs.ReportDesignArea;
using CD.DLS.Model.Interfaces;
using Report = CD.DLS.Parse.Mssql.Ssrs.Rdl2016.Report;
//using CD.DLS.Model.Mssql.Ssas;

namespace CD.DLS.Parse.Mssql.Ssrs
{
    public class SsrsModelExtractor
    {
        private UrnBuilder _urnBuilder = new UrnBuilder();
        private Parser _expressionParser = new Parser(new ExpressionGrammar());
        /// <summary>
        /// For the resolution of the current report item
        /// </summary>
        private DataSetFieldIndex _currentContextDataSet;
        private List<TextBoxElement> _independentTextBoxes = new List<TextBoxElement>();
        private bool _independentReportItemLevel = true;
        private DLS.Common.Structures.SsrsProjectComponent _currentComponent;
        private AvailableDatabaseModelIndex _sqlContext;
        private ISqlScriptModelParser _sqlExtractor;
        private SsasServerIndex _ssasContext;
        private MdxScriptModelExtractor _mdxExtractor;
        private DaxScriptModelExtractor _daxExtractor = new DaxScriptModelExtractor();
        
        private ProjectConfig _projectConfig;
        private Guid _extractId;
        private StageManager _stageManager;

        public SsrsModelExtractor(AvailableDatabaseModelIndex sqlContext,
            SsasServerIndex ssasIndxex, MdxScriptModelExtractor mdxExtractor, 
            ProjectConfig projectConfig, Guid extractId, StageManager stageManager)
        {
            _sqlContext = sqlContext;
            _sqlExtractor = new ScriptModelParser();

            _ssasContext = ssasIndxex;
            _mdxExtractor = mdxExtractor;

            _projectConfig = projectConfig;
            _extractId = extractId;
            _stageManager = stageManager;
        }

        #region MAPPING_STRUCTURES
        private ServerElement _serverElement;
        private Dictionary<string, FolderElement> _folderPathMap = new Dictionary<string, FolderElement>();

        public class DataSourceIndex
        {
            private Dictionary<string, SharedDataSourceElement> _sharedDataSources = new Dictionary<string, SharedDataSourceElement>();
            private Dictionary<string, DataSourceElement> _reportDataSources = new Dictionary<string, DataSourceElement>();

            public void AddSharedDataSource(string path, SharedDataSourceElement ds)
            {
                // TODO: different datasources in different folders can have the same name
                if (!_sharedDataSources.ContainsKey(Path.GetFileNameWithoutExtension(path)))
                {
                    _sharedDataSources.Add(Path.GetFileNameWithoutExtension(path), ds);
                }
            }

            public void AddReportDataSource(ReportDataSourceElement ds)
            {
                _reportDataSources.Add(ds.Caption, ds);
            }

            public void AddSharedReportDataSource(string localName, string referenceName)
            {
                _reportDataSources.Add(localName, GetSharedDataSource(referenceName));
            }

            public void ClearReportDataSources()
            {
                _reportDataSources.Clear();
            }

            public DataSourceElement GetSharedDataSource(string name)
            {
                if (!_sharedDataSources.ContainsKey(Path.GetFileNameWithoutExtension(name)))
                {
                    ConfigManager.Log.Error(string.Format("Could not find shared datasource {0} in {1}", name, string.Join(", ", _sharedDataSources.Keys)));
                    return null;
                }
                return _sharedDataSources[Path.GetFileNameWithoutExtension(name)];
            }

            public DataSourceElement GetReportDataSource(string name)
            {
                return _reportDataSources[name];
            }
        }

        public class DataSetIndex
        {
            private Dictionary<string, DataSetFieldIndex> _sharedDataSets = new Dictionary<string, DataSetFieldIndex>();
            private Dictionary<string, DataSetFieldIndex> _reportDataSets = new Dictionary<string, DataSetFieldIndex>();

            public void AddSharedDataSet(SharedDataSetElement ds)
            {
                DataSetFieldIndex fieldIndex = new DataSetFieldIndex() { Dictionary = ds.Fields.ToDictionary(x => x.Caption, x => x), ModelElement = ds };
                _sharedDataSets.Add(ds.Caption, fieldIndex);
            }

            public void AddReportDataSet(ReportDataSetElement ds)
            {
                DataSetFieldIndex fieldIndex = new DataSetFieldIndex() { Dictionary = ds.Fields.ToDictionary(x => x.Caption, x => x), ModelElement = ds };
                _reportDataSets.Add(ds.Caption, fieldIndex);
            }

            public void AddSharedReportDataSet(string name, SharedDataSetType dataSet, FieldsType fields)
            {
                var dsReference = dataSet.Items.First(x => x is string) as string;
                if (!_sharedDataSets.ContainsKey(dsReference))
                {
                    // TODO
                    return;
                }
                var sharedDs = _sharedDataSets[dsReference];
                DataSetFieldIndex localFields = new DataSetFieldIndex();
                foreach (var field in fields.Field)
                {
                    var localName = field.Name;
                    var sharedName = field.Items.First(x => x is string) as string;
                    localFields.Dictionary[localName] = sharedDs[sharedName];
                }
                localFields.ModelElement = sharedDs.ModelElement;
                _reportDataSets.Add(name, localFields);
            }

            public void ClearReportDataSets()
            {
                _reportDataSets.Clear();
            }

            public DataSetFieldIndex GetSharedDataSet(string name)
            {
                return _sharedDataSets[name];
            }

            public DataSetFieldIndex GetReportDataSet(string name)
            {
                if (_reportDataSets.ContainsKey(name))
                {
                    return _reportDataSets[name];
                }

                return null;
                //return new DataSetFieldIndex() { Dictionary = new Dictionary<string, DataSetFieldElement>(), ModelElement = null };
            }
        }

        public class DataSetFieldIndex
        {
            public DataSetElement ModelElement { get; set; }
            public Dictionary<string, DataSetFieldElement> Dictionary { get; set; }
            public DataSetFieldElement this[string name]
            {
                get { return Dictionary[name]; }
                set { Dictionary[name] = value; }
            }

            public DataSetFieldIndex()
            {
                Dictionary = new Dictionary<string, DataSetFieldElement>();
            }

            public DataSetFieldElement GetField(string name)
            {
                if (Dictionary.ContainsKey(name))
                {
                    return Dictionary[name];
                }
                return null;
            }
        }

        public class ReportParameterIndex
        {
            private Dictionary<string, ReportParameterElement> _reportParameters = new Dictionary<string, ReportParameterElement>();

            public void AddReportParameter(ReportParameterElement parameter)
            {
                _reportParameters.Add(parameter.Caption, parameter);
            }

            public void ClearReportParameters()
            {
                _reportParameters.Clear();
            }

            public ReportParameterElement GetReportParameter(string name)
            {
                if (_reportParameters.ContainsKey(name))
                {
                    return _reportParameters[name];
                }
                return null;
            }
        }

        #endregion

        private DataSourceIndex _dataSourceIndex = new DataSourceIndex();
        private DataSetIndex _dataSetIndex = new DataSetIndex();
        private ReportParameterIndex _parameterIndex = new ReportParameterIndex();

        public class DownloadedReportItem
        {
            public string ID { get; set; }
            public string Path { get; set; }
            public string Name { get; set; }
            public string Definition { get; set; }
            public string TypeName { get; set; }
            public string DataSourceConnectionString { get; set; }
            public string DataSourceExtension { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        public List<ServerElement> ExtractModel()
        {
            List<ServerElement> res = new List<ServerElement>();
            foreach (var component in _projectConfig.SsrsComponents)
            {
                ConfigManager.Log.Important(string.Format("Extracting SSRS folder {0} from {1}", component.FolderPath, component.ServerName));
                _currentComponent = component;
                var cModel = ExtractComponentModel(res);
                if (!res.Contains(cModel))
                {
                    res.Add(cModel);
                }
            }
            return res;
        }

        public ServerElement ExtractComponentModel(List<ServerElement> serverElementsSoFar)
        {
            _urnBuilder = new UrnBuilder();
            _expressionParser = new Parser(new ExpressionGrammar());
            
            var serverUrn = _urnBuilder.GetServerUrn(_currentComponent.ServerName);
            
            _serverElement = serverElementsSoFar.FirstOrDefault(x => CD.DLS.Common.Tools.ConnectionStringTools.AreServersNamesEqual(x.Caption, _currentComponent.ServerName));
            if (_serverElement == null)
            {
                _serverElement = new ServerElement(serverUrn, _currentComponent.ServerName /*serverName*/);
            }
            
            //var ssrsFiles = _stageManager.GetExtractItems(_extractId, _currentComponent.SsrsProjectComponentId, 
            //    DLS.DAL.Objects.Extract.ExtractTypeEnum.SsrsFile).Select(x => (SsrsItem)x).ToList();


            //if (_currentComponent.SsrsMode == DLS.Common.Structures.SsrsModeEnum.SpIntegrated)
            //{
            //    foreach (var file in ssrsFiles)
            //    {
            //        file.FullPath = "/" + file.FullPath.TrimStart('/');
            //    }
            //}
            
            LoadFolders(_currentComponent.FolderPath);
            LoadSharedDataSources();
            LoadSharedDataSets();
            LoadReports();

            return _serverElement;
        }

        public void ExtractComponentModelShallow(ServerElement serverElement, SsrsProjectComponent ssrsComponent)
        {
            _urnBuilder = new UrnBuilder();
            _expressionParser = new Parser(new ExpressionGrammar());
            _currentComponent = ssrsComponent;

            _serverElement = serverElement;

            //var ssrsFiles = _stageManager.GetExtractItems(_extractId, _currentComponent.SsrsProjectComponentId,
            //    DLS.DAL.Objects.Extract.ExtractTypeEnum.SsrsFile).Select(x => (SsrsItem)x).ToList();


            //if (_currentComponent.SsrsMode == DLS.Common.Structures.SsrsModeEnum.SpIntegrated)
            //{
            //    foreach (var file in ssrsFiles)
            //    {
            //        file.FullPath = "/" + file.FullPath.TrimStart('/');
            //    }
            //}

            var folders = LoadFolders(_currentComponent.FolderPath);
            var sharedDataSources = LoadSharedDataSources();
            var sharedDataSets = LoadSharedDataSets();
            //LoadReports();

            //return _serverElement;
        }

        //public void SetIndexes(ServerElement serverElement)
        //{
        //    SetFolderIndex(serverElement.Folders.ToList());
        //    var rootDataSources = serverElement.Children.OfType<SharedDataSourceElement>();
        //    var rootDataSets = serverElement.Children.OfType<SharedDataSetElement>();
        //    var allDataSources = rootDataSources.Union(CollectElements<SharedDataSourceElement>(serverElement.Folders)).ToList();
        //    var allDataSets = rootDataSets.Union(CollectElements<SharedDataSetElement>(serverElement.Folders)).ToList();
        //    SetSharedDataSourceIndex(allDataSources);
        //    SetSharedDataSetIndex(allDataSets);
        //}

        private IEnumerable<T> CollectElements<T>(IEnumerable<FolderElement> folders)
        {
            var init = folders.SelectMany(x => x.Children.OfType<T>());
            return init.Union(folders.SelectMany(x => CollectElements<T>(x.Folders)));
        }

        public void SetFolderIndex(List<FolderElement> folderElements)
        {
            _folderPathMap = GetFolderMap(folderElements);
        }

        private Dictionary<string, FolderElement> GetFolderMap(List<FolderElement> folderElements)
        {
            if (folderElements.Any(x => x.FullPath == "/"))
            {
                while (folderElements.Any(x => string.IsNullOrWhiteSpace(x.FullPath)))
                {
                    folderElements.Remove(folderElements.Where(x => string.IsNullOrWhiteSpace(x.FullPath)).First());
                }
            }

            //foreach (var folder in folderElements)
            //{
            //    ConfigManager.Log.Important("Folder '" + folder.FullPath + "' " + (folder.FullPath == null).ToString());
            //}

            var res = folderElements.ToDictionary(x => (x.FullPath == null ? "/" : x.FullPath), x => x);
            var subItems = folderElements.SelectMany(x => GetFolderMap(x.Folders.ToList()));
            foreach (var subItem in subItems)
            {
                //ConfigManager.Log.Important(subItem.Key + " subfolder");
                if (subItem.Key == "/")
                {
                    continue;
                }
                res.Add(subItem.Key, subItem.Value);
            }
            return res;
        }

        public void SetSharedDataSourceIndex(List<SharedDataSourceElement> dataSourceElements)
        {
            _dataSourceIndex = new DataSourceIndex();
            foreach (var dataSource in dataSourceElements)
            {
                _dataSourceIndex.AddSharedDataSource(dataSource.FullPath, dataSource);
            }
        }

        public void SetSharedDataSetIndex(List<SharedDataSetElement> dataSetElements)
        {
            _dataSetIndex = new DataSetIndex();
            foreach (var dataSet in dataSetElements)
            {
                _dataSetIndex.AddSharedDataSet(dataSet);
            }
        }

        private List<string> GeneratePaths(string path, string commonPrefix)
        {
            var pos = commonPrefix.Length;
            List<string> paths = new List<string>();
            while (pos < path.Length)
            {
                var slashPos = path.IndexOf('/', pos);
                if (slashPos == -1)
                {
                    break;
                }

                var prefix = path.Substring(0, slashPos);
                paths.Add(prefix);
                pos = slashPos + 1;
            }

            paths.Add(path);

            return paths;
        }

        private Dictionary<string, FolderElement> LoadFolders(string rootFolderPath)
        {
            //var folders = items.Where(x => x.ItemType == SsrsItemTypeEnum.Folder).OrderBy(x => x.FullPath).ToList();
            
            var datasets = _stageManager.GetExtractItems(_extractId, _currentComponent.SsrsProjectComponentId, ExtractTypeEnum.SsrsSharedDataSet)
                .Select(x => (SsrsItem)x).ToList();
            var datasources = _stageManager.GetExtractItems(_extractId, _currentComponent.SsrsProjectComponentId, ExtractTypeEnum.SsrsSharedDataSource)
                .Select(x => (SsrsItem)x).ToList();
            var reports = _stageManager.GetExtractItems(_extractId, _currentComponent.SsrsProjectComponentId, ExtractTypeEnum.SsrsReport)
                .Select(x => (SsrsItem)x).ToList();
            
            var paths = datasets.Union(datasources).Union(reports).Select(x => x.FullPath.Substring(0, x.FullPath.LastIndexOf('/')))
                .Distinct().ToList();

            datasets.Clear();
            datasources.Clear();
            reports.Clear();
            datasets = null;
            datasources = null;
            reports = null;
            GC.Collect();


            //if (_currentComponent.SsrsMode == SsrsModeEnum.Native)
            //{
            //    var folders = _stageManager.GetExtractItems(_extractId, _currentComponent.SsrsProjectComponentId,
            //        DLS.DAL.Objects.Extract.ExtractTypeEnum.SsrsFolder).Select(x => (SsrsFolder)x).ToList();

            //    if (!folders.Any(x => x.FullPath == "/"))
            //    {
            //        var nf = new SsrsFolder()
            //        {
            //            FullPath = "/",
            //            FileName = "/",
            //            ID = "/",
            //            Content = "/",
            //            ItemType = SsrsItemTypeEnum.Folder
            //        };
            //        folders.Add(nf);


            //        folders = folders.OrderBy(x => x.FullPath).ToList();
            //    }

            //    paths = folders.Select(x => x.FullPath).Where(x => x != "/").ToList();
            //}
            //// sharepoint mode
            //else
            //{

            //}

            //FolderElement prefixRootElement = null;


            if (!paths.Any())
            {
                return new Dictionary<string, FolderElement>();
            }


                var commonPrefix = new string(
        paths.First().Substring(0, paths.Min(s => s.Length))
        .TakeWhile((c, i) => paths.All(s => s[i] == c)).ToArray());
            if (commonPrefix != "/")
            {
                commonPrefix = commonPrefix.TrimEnd('/');
            }
            
                ConfigManager.Log.Important("Common SSRS prefix: " + commonPrefix);


            paths = paths.SelectMany(p => GeneratePaths(p, commonPrefix)).Select(x => x.TrimEnd('/')).Distinct().OrderBy(x => x).ToList();

            foreach (var path in paths)
            {
                ConfigManager.Log.Important("SSRS folder: " + path);
            }

            //    if (commonPrefix.Length > 1)
            //    {
            //        var searchFrom = 1;
            //        var slIdx = commonPrefix.IndexOf('/', searchFrom);
            //        FolderElement ancestorFolder = null;

            //        while (slIdx > 0)
            //        {
            //            var prefixFolder = commonPrefix.Substring(0, slIdx);
            //            if (!folders.Any(x => x.FullPath.TrimEnd('/') == prefixFolder))
            //            {
            //                ConfigManager.Log.Important("Adding common parent folder \"" + prefixFolder + "\"");
            //                var prfFolder = new SsrsFolder()
            //                {
            //                    FullPath = prefixFolder,
            //                    FileName = prefixFolder,
            //                    ID = prefixFolder,
            //                    Content = prefixFolder,
            //                    ItemType = SsrsItemTypeEnum.Folder
            //                };

            //                folders.Add(prfFolder);
            //                var prfFolderUrn = _urnBuilder.GetFolderUrn(prfFolder.Name, ancestorFolder == null ? _serverElement.RefPath : ancestorFolder.RefPath);
            //                var prfFolderElement = ancestorFolder == null ? new FolderElement(prfFolderUrn, "/", null, _serverElement) :
            //                    new FolderElement(prfFolderUrn, prefixFolder, null, ancestorFolder);
            //                prfFolderElement.FullPath = prefixFolder;

            //                if (ancestorFolder != null)
            //                {
            //                    ancestorFolder.AddChild(prfFolderElement);
            //                }
            //                else
            //                {
            //                    prefixRootElement = prfFolderElement;
            //                    ConfigManager.Log.Important("Prefix root: " + prfFolderElement.RefPath.Path);
            //                }
            //                ancestorFolder = prfFolderElement;

            //                folders.Add(prfFolder);
            //                _folderPathMap.Add(prefixFolder, prfFolderElement);

            //            }

            //            searchFrom = slIdx + 1;
            //            slIdx = commonPrefix.IndexOf('/', searchFrom);
            //        }

            //        if (!folders.Any(x => x.FullPath.TrimEnd('/') == commonPrefix.TrimEnd('/')))
            //        {
            //            ConfigManager.Log.Important("Adding common parent folder \"" + commonPrefix + "\"");
            //            var newFld = new SsrsFolder()
            //            {
            //                FullPath = commonPrefix,
            //                FileName = commonPrefix,
            //                ID = commonPrefix,
            //                Content = commonPrefix,
            //                ItemType = SsrsItemTypeEnum.Folder
            //            };

            //            var newFolderUrn = _urnBuilder.GetFolderUrn(newFld.Name, ancestorFolder.RefPath);

            //            FolderElement newFolderElement = null;
            //            if (ancestorFolder == null)
            //            {
            //                newFolderElement = new FolderElement(newFolderUrn, "/", null, _serverElement);
            //                _serverElement.AddChild(newFolderElement);
            //            }
            //            else
            //            {
            //                newFolderElement = new FolderElement(newFolderUrn, "/", null, ancestorFolder);
            //                ancestorFolder.AddChild(newFolderElement);
            //            }

            //            newFolderElement.FullPath = "/";

            //            ConfigManager.Log.Important("Added " + newFolderElement.RefPath.Path + " to " + newFolderElement.Parent.RefPath.Path + "(" + commonPrefix + ")");
            //            ancestorFolder = newFolderElement;

            //            folders.Add(newFld);
            //            _folderPathMap.Add(commonPrefix, newFolderElement);
            //        }

            //        folders = folders.OrderBy(x => x.FullPath).ToList();
            //    }
            //}

            //if (rootFolderPath != null)
            //{
            //    if (rootFolderPath != "/")
            //    {
            //ConfigManager.Log.Important("Root folder ref path not null and not /");

            var rootFolderName = commonPrefix;
                    var rootFolderUrn = _urnBuilder.GetFolderUrn(rootFolderName, _serverElement.RefPath);
                    var rootFolderElement = new FolderElement(rootFolderUrn, rootFolderName, null, _serverElement);
                    _serverElement.AddChild(rootFolderElement);
                    _folderPathMap.Add(rootFolderName, rootFolderElement);

            //if (prefixRootElement != null)
            //{
            //    ConfigManager.Log.Important("Linking prefix root " + prefixRootElement.Caption + " to " + rootFolderElement.RefPath.Path);
            //    rootFolderElement.AddChild(prefixRootElement);
            //    prefixRootElement = rootFolderElement;
            //}
            //    }
            //}

            paths.Sort();

            foreach (var path in paths)
            {
                if (path == commonPrefix)
                {
                    continue;
                }

                string folderName = path;
                var lastSlash = path.LastIndexOf('/');
                if (lastSlash > 0)
                {
                    folderName = path.Substring(lastSlash + 1);
                }

                //ConfigManager.Log.Info(string.Format("Adding folder {0}", path));

                //if (_folderPathMap.ContainsKey(path))
                //{
                //    continue;
                //}
                
                var parentFolderElement = GetParentFolder(path);

                //ConfigManager.Log.Important("Adding folder " + path + " to " + parentFolderElement.RefPath.Path);

                var folderUrn = _urnBuilder.GetFolderUrn(folderName, parentFolderElement.RefPath);
                var folderElement = new FolderElement(folderUrn, folderName, null, parentFolderElement);
                folderElement.FullPath = path;
                parentFolderElement.AddChild(folderElement);
                _folderPathMap.Add(path, folderElement);
            }


            //var foldersMsg = "Folders in " + _serverElement.RefPath.Path + Environment.NewLine;

            //foldersMsg = ListFolders(foldersMsg, _serverElement.Folders, 1);

            //var foldersMsg = "Folder list complete: " + _serverElement.RefPath.Path + Environment.NewLine;


            //ConfigManager.Log.Important(foldersMsg);


            //if (_folderPathMap.ContainsKey("/"))
            //{
            //    var root = _folderPathMap["/"];
            //    if (root.Parent != _serverElement)
            //    {
            //        var fullPath = ((FolderElement)root.Parent).FullPath;
            //        root.FullPath = fullPath;
            //        root.Parent = _serverElement;
            //        while (_serverElement.Children.Any())
            //        {
            //            _serverElement.RemoveChild(_serverElement.Children.First());
            //        }
            //        _serverElement.AddChild(root);
            //        _folderPathMap[fullPath] = root;
            //    }
            //}

            return _folderPathMap;
        }

        //private string ListFolders(string foldersMsg, IEnumerable<FolderElement> folders, int level)
        //{
        //    var prefix = new string('-', level);
        //    foreach (var folder in folders)
        //    {
        //        foldersMsg += prefix + folder.FullPath + "(" + folder.RefPath.Path + ")" + Environment.NewLine;
        //        ConfigManager.Log.Info(string.Format("Adding folder {0} at level {1}", folder.RefPath.Path, level));
        //        foldersMsg += ListFolders(foldersMsg, folder.Folders, level + 1);
        //    }
        //    return foldersMsg;
        //}


        #region REPORT_DATA
        private DataSourceIndex LoadSharedDataSources()
        {
            //var dataSources = items.Where(x => x.ItemType == SsrsItemTypeEnum.SharedDataSource);

            var dataSources = _stageManager.GetExtractItems(_extractId, _currentComponent.SsrsProjectComponentId,
                DLS.DAL.Objects.Extract.ExtractTypeEnum.SsrsSharedDataSource).Select(x => (SsrsSharedDataSource)x).ToList();


            foreach (var dataSourceItem in dataSources)
            {
                var parentFolder = GetParentFolder(dataSourceItem);
                //var dsDef = _rs.GetDataSourceContents(dataSourceItem.Path);
                var dsUrn = _urnBuilder.GetDataSourceUrn(dataSourceItem.Name, parentFolder.RefPath);
                DataSourceElement.SourceTypeEnum sourceType;
                switch (dataSourceItem.DataSourceExtension)
                {
                    case "SQL":
                        sourceType = DataSourceElement.SourceTypeEnum.Sql;
                        break;
                    case "OLEDB-MD":
                        sourceType = DataSourceElement.SourceTypeEnum.SsasMultidimensional;
                        break;
                    default:
                        var msg = string.Format("Unrecognized datasource provider: {0}", dataSourceItem.DataSourceExtension);
                        ConfigManager.Log.Error(msg);
                        throw new Exception(msg);
                }


                SharedDataSourceElement dsElement = new SharedDataSourceElement(dsUrn, Path.GetFileNameWithoutExtension(dataSourceItem.Name), null, parentFolder);
                parentFolder.AddChild(dsElement);
                dsElement.SourceType = sourceType;
                dsElement.ConnectionString = dataSourceItem.DataSourceConnectionString;
                dsElement.FullPath = dataSourceItem.FullPath;
                _dataSourceIndex.AddSharedDataSource(dataSourceItem.FullPath, dsElement);
            }

            return _dataSourceIndex;
        }

        private DataSetIndex LoadSharedDataSets()
        {
            //var dataSets = items.Where(x => x.ItemType == SsrsItemTypeEnum.SharedDataSet);

            var dataSets = _stageManager.GetExtractItems(_extractId, _currentComponent.SsrsProjectComponentId,
                DLS.DAL.Objects.Extract.ExtractTypeEnum.SsrsSharedDataSet).Select(x => (SsrsSharedDataSet)x).ToList();


            foreach (var dataSetItem in dataSets)
            {
                var parentFolder = GetParentFolder(dataSetItem);
                var dsUrn = _urnBuilder.DataSetUrn(dataSetItem.Name, parentFolder.RefPath);
                var dataSetElement = new SharedDataSetElement(dsUrn, dataSetItem.Name, null, parentFolder);
                parentFolder.AddChild(dataSetElement);

                //var definition = _rs.GetItemDefinition(dataSetItem.Path);
                var stringDefinition = dataSetItem.Content; // BytesToText(definition);
                var clearedDefinition = RemoveAllNamespaces(stringDefinition, "DataSet");

                XmlSerializer serializer = new XmlSerializer(typeof(Rdl2016.DataSet));
                Rdl2016.DataSet dsObj = null;
                using (var stream = GenerateStreamFromString(clearedDefinition))
                {
                    dsObj = (Rdl2016.DataSet)(serializer.Deserialize(stream));
                }
                var query = dsObj.Items.First(x => x is QueryType) as QueryType;
                var fields = dsObj.Items.First(x => x is FieldsType) as FieldsType;

                var sourceReferenceElem = query.Items.First(x => x is XmlElement && ((XmlElement)x).LocalName == "DataSourceReference") as XmlElement;
                var dataSource = _dataSourceIndex.GetSharedDataSource(sourceReferenceElem.InnerText);
                var queryText = query.Items.First(x => x is string) as string;
                var parameters = query.Items.FirstOrDefault(x => x is QueryParametersType) as QueryParametersType;

                LoadDataSet(dataSource, dataSetElement, queryText, fields, parameters);

                _dataSetIndex.AddSharedDataSet(dataSetElement);
            }

            return _dataSetIndex;
        }

        private void LoadDataSet(DataSourceElement dataSource, DataSetElement dataSetElement, string queryText, FieldsType fields, QueryParametersType parameters)
        {
            //bool interestingDataset = dataSetElement.RefPath.Path == "SSRSServer[@Name='FSCZPRCT0041']/Folder[@Name='/']/Folder[@Name='NRWH_Test']/Folder[@Name='MO']/Report[@Name='RC2018_Overview']/DataSet[@Name='dsMain']";

            //ConfigManager.Log.Info("Loading dataset " + dataSetElement.RefPath.Path + " (" + interestingDataset.ToString() + ")");
            //if (interestingDataset)
            //{
            //    ConfigManager.Log.Info(string.Format("IDS: Loading dataset {0}", dataSetElement.RefPath.Path));
            //}

            if (parameters != null)
            {
                foreach (var parameter in parameters.QueryParameter)
                {
                    var paramName = parameter.Name;
                    var paramValue = parameter.Items.First(x => x is StringWithDataTypeAttribute) as StringWithDataTypeAttribute;
                    var paramUrn = _urnBuilder.GetQueryParameterUrn(paramName, dataSetElement.RefPath);
                    var paramElement = new QueryParameterElement(paramUrn, paramName, null, dataSetElement);
                    dataSetElement.AddChild(paramElement);

                    var expressionUrn = _urnBuilder.GetExpressionUrn(paramElement.RefPath);
                    var expressionElement = new SsrsExpressionElement(expressionUrn, "Expression", paramValue.Value, paramElement);
                    paramElement.AddChild(expressionElement);
                    // TODO resolve expression
                }
            }

            // only one of these will be filled
            Dictionary<string, MssqlModelElement> outputColumnsFromNames = null;
            Dictionary<string, Model.Mssql.Ssas.SsasModelElement> daxOutputColumnsFromNames = null;
            MdxStatementIndex statementIndex = new MdxStatementIndex();

            SsasTypeEnum sourceSsasDbType = SsasTypeEnum.Multidimensional;

            if (dataSource != null)
            {
                //if (interestingDataset)
                //{
                //    ConfigManager.Log.Info(string.Format("IDS: Datasource {0} - {1}", dataSource.RefPath.Path, dataSource.SourceType.ToString()));
                //}

                if (dataSource.SourceType == DataSourceElement.SourceTypeEnum.Sql)
                {
                    var dbIdentifier = _sqlExtractor.GetDbNameFromConnectionString(dataSource.ConnectionString);
                    var serverName = _sqlExtractor.GetServerNameFromConnectionString(dataSource.ConnectionString, dataSource.LocalhostServer());
                    var dbIndex = _sqlContext.GetDatabaseIndex(serverName, dataSource.LocalhostServer());
                    //if (interestingDataset)
                    //{
                    //    ConfigManager.Log.Info(string.Format("IDS: Tables indexed in {0}: {1}", dbIndex.ContextServerName, dbIndex.TableSourcesAvailable.Count().ToString()));
                    //    ConfigManager.Log.Info(string.Format("IDS: SPs indexed in {0}: {1}", dbIndex.ContextServerName, dbIndex.StoredProceduresAvailable.Count.ToString()));
                    //}

                    //_sqlContext.ContextServerName = serverName;

                    var variablesIndex = new Db.AddPresetVariablesReferrableIndex(dbIndex);
                    foreach (var param in dataSetElement.Parameters)
                    {
                        variablesIndex.AddPresetVariable("@" + param.Caption, param);
                    }

                    //if (interestingDataset)
                    //{
                    //    ConfigManager.Log.Info(string.Format("IDS: Resolving {0}", queryText));
                    //}
                    var sqlElement = _sqlExtractor.ExtractScriptModel(queryText, dataSetElement, variablesIndex, new Identifier() { Value = dbIdentifier }, out outputColumnsFromNames);
                    //if (interestingDataset)
                    //{
                    //    ConfigManager.Log.Info(string.Format("IDS: Columns: {0}", string.Join(", ", outputColumnsFromNames.Keys)));
                    //}
                    dataSetElement.AddChild(sqlElement);
                }
                else if (dataSource.SourceType == DataSourceElement.SourceTypeEnum.SsasMultidimensional)
                {
                    var queryTextParametrized = queryText;
                    foreach (var param in dataSetElement.Parameters.OrderByDescending(x => x.Caption.Length))
                    {
                        queryTextParametrized = queryTextParametrized.Replace("@[" + param.Caption + "]", string.Format("'Param_{0}'", param.Caption));
                        queryTextParametrized = queryTextParametrized.Replace("@" + param.Caption, string.Format("'Param_{0}'", param.Caption));
                        // TODO map & resolve parameters
                    }
                    // TODO use canse-insensitive param resolution instead
                    queryTextParametrized = queryTextParametrized.Replace("@", "UnknownParam_");

                    var databaseName = _sqlExtractor.GetDbNameFromConnectionString(dataSource.ConnectionString);
                    var serverName = _sqlExtractor.GetServerNameFromConnectionString(dataSource.ConnectionString, dataSource.LocalhostServer());
                    var ssasEnvironment = _ssasContext.GetDatabase(serverName, databaseName);

                    // some SSAS databases may not be covered by the model
                    if (ssasEnvironment != null)
                    {
                        sourceSsasDbType = ssasEnvironment.SsasType;

                        if (ssasEnvironment.SsasType == SsasTypeEnum.Multidimensional)
                        {
                            var mdxElement = _mdxExtractor.ExtractMdxStatement(queryTextParametrized, _ssasContext.GetDatabase(serverName, databaseName), dataSetElement, out statementIndex);
                            if (mdxElement != null) // MDX extraction can fail for special queries (e.g. SQL query to system DB)
                            {
                                dataSetElement.AddChild(mdxElement);
                            }
                        }
                        else if (ssasEnvironment.SsasType == SsasTypeEnum.Tabular)
                        {
                            var daxElement = _daxExtractor.ExtractDaxScript(queryTextParametrized, _ssasContext.GetDatabase(serverName, databaseName), dataSetElement, out daxOutputColumnsFromNames);
                            if (daxElement != null)
                            {
                                dataSetElement.AddChild(daxElement);
                            }
                        }
                        else throw new Exception("Unknown SSAS type: " + ssasEnvironment.SsasType.ToString());
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            foreach (var field in fields.Field)
            {
                var fieldSourceStr = field.Items.FirstOrDefault(x => x is string) as string;
                bool isCalculated = fieldSourceStr == null;

                var fieldName = field.Name;
                var fieldUrn = _urnBuilder.GetDataSetFieldUrn(field.Name, dataSetElement.RefPath);
                var fieldElement = new DataSetFieldElement(fieldUrn, field.Name, fieldSourceStr, dataSetElement);
                dataSetElement.AddChild(fieldElement);

                if (isCalculated)
                {
                    var expression = field.Items.First(x => x is StringWithDataTypeAttribute) as StringWithDataTypeAttribute;
                    var expressionUrn = _urnBuilder.GetExpressionUrn(fieldElement.RefPath);
                    var expressionElement = LoadExpression(expression.Value, fieldElement);
                    
                    continue;
                }

                if (dataSource != null)
                {
                    if (dataSource.SourceType == DataSourceElement.SourceTypeEnum.SsasMultidimensional)
                    {
                        if (sourceSsasDbType == SsasTypeEnum.Multidimensional)
                        {
                            if (!fieldSourceStr.StartsWith("<"))
                            {
                                // system DB columns or other specialties - do not resolve source
                                continue;
                            }
                            XDocument fieldXDoc = null;
                            string fieldType = null;
                            XAttribute fieldUniequeName = null;
                            XAttribute fieldUniequeLevelName = null;
                            XAttribute propertyName = null;
                            int realResolutionLength;

                            try
                            {
                                fieldXDoc = XDocument.Parse(fieldSourceStr);
                                fieldType = fieldXDoc.Root.Attributes().First(x => x.Name.LocalName == "type").Value;
                                fieldUniequeName = fieldXDoc.Root.Attributes().FirstOrDefault(x => x.Name.LocalName == "UniqueName");
                                fieldUniequeLevelName = fieldXDoc.Root.Attributes().FirstOrDefault(x => x.Name.LocalName == "LevelUniqueName");
                                propertyName = fieldXDoc.Root.Attributes().FirstOrDefault(x => x.Name.LocalName == "PropertyName");

                            }
                            catch (Exception ex)
                            {
                                ConfigManager.Log.Error(string.Format("Failed to parse dataset field definition: {0} in {1}", fieldSourceStr, fieldElement.RefPath.Path));
                                continue;
                            }

                            if (fieldType == "Measure")
                            {
                                var measureResolution = statementIndex.TryResolveIdentifier(0, fieldUniequeName.Value, false, out realResolutionLength);
                                fieldElement.Source = measureResolution;
                            }
                            else if (fieldType == "Level")
                            {
                                var levelResolution = statementIndex.TryResolveIdentifier(1, fieldUniequeName.Value, true, out realResolutionLength);
                                fieldElement.Source = levelResolution;
                            }
                            else if (fieldType == "MemberProperty")
                            {
                                var hierarchyId = fieldUniequeLevelName.Value.Substring(0, fieldUniequeLevelName.Value.LastIndexOf(".["));
                                var propertyId = string.Format("{0}.[{1}]", hierarchyId, propertyName.Value);
                                var levelResolution = statementIndex.TryResolveIdentifier(0, propertyId, true, out realResolutionLength);
                                fieldElement.Source = levelResolution;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        else
                        {
                            var availableColumns = daxOutputColumnsFromNames;
                            var columnReference = fieldSourceStr;
                            if (columnReference.EndsWith("]"))
                            {
                                columnReference = columnReference.Substring(columnReference.IndexOf("["));
                                columnReference = columnReference.TrimStart('[').TrimEnd(']');
                            }

                            if (availableColumns.ContainsKey(columnReference))
                            {
                                fieldElement.Source = availableColumns[columnReference];
                            }

                            //fieldElement.Source =
                        }
                    }
                    else if (dataSource.SourceType == DataSourceElement.SourceTypeEnum.Sql)
                    {
                        if (outputColumnsFromNames != null && outputColumnsFromNames.ContainsKey(fieldSourceStr))
                        {
                            fieldElement.Source = outputColumnsFromNames[fieldSourceStr];
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
        }


        private void LoadReportDataSources(Report report, ReportElement parent)
        {
            _dataSourceIndex.ClearReportDataSources();

            var dataSources = report.Items.FirstOrDefault(x => x is DataSourcesType) as DataSourcesType;
            if (dataSources == null)
            {
                return;
            }

            foreach (var dataSource in dataSources.DataSource)
            {
                var dsName = dataSource.Name;
                var dsRef = dataSource.Items.FirstOrDefault(x => x is string) as string;
                var connProp = dataSource.Items.FirstOrDefault(x => x is ConnectionPropertiesType) as ConnectionPropertiesType;

                // shared datasource
                if (dsRef != null)
                {
                    _dataSourceIndex.AddSharedReportDataSource(dsName, dsRef);
                    continue;
                }

                var provider = (string)(connProp.Items[connProp.ItemsElementName.ToList().IndexOf(ItemsChoiceType.DataProvider)]);
                var connString = (string)(connProp.Items[connProp.ItemsElementName.ToList().IndexOf(ItemsChoiceType.ConnectString)]);

                var dsUrn = _urnBuilder.GetDataSourceUrn(dataSource.Name, parent.RefPath);
                DataSourceElement.SourceTypeEnum sourceType;
                switch (provider)
                {
                    case "SQL":
                        sourceType = DataSourceElement.SourceTypeEnum.Sql;
                        break;
                    case "OLEDB-MD":
                        sourceType = DataSourceElement.SourceTypeEnum.SsasMultidimensional;
                        break;
                    default:
                        var msg = string.Format("Unrecognized datasource provider: {0}", provider);
                        ConfigManager.Log.Error(msg);
                        throw new Exception(msg);
                }

                ReportDataSourceElement dsElement = new ReportDataSourceElement(dsUrn, dataSource.Name, null, parent);
                parent.AddChild(dsElement);
                dsElement.SourceType = sourceType;
                dsElement.ConnectionString = connString;
                _dataSourceIndex.AddReportDataSource(dsElement);
            }
        }

        private void LoadReportDataSets(Report report, ReportElement parent)
        {
            _dataSetIndex.ClearReportDataSets();

            var dataSets = report.Items.FirstOrDefault(x => x is DataSetsType) as DataSetsType;
            if (dataSets == null)
            {
                return;
            }

            foreach (var dataSet in dataSets.DataSet)
            {
                var name = dataSet.Name;
                var sharedDataset = dataSet.Items.FirstOrDefault(x => x is SharedDataSetType) as SharedDataSetType;
                var fields = dataSet.Items.FirstOrDefault(x => x is FieldsType) as FieldsType;
                var query = dataSet.Items.FirstOrDefault(x => x is QueryType) as QueryType;
                
                if (sharedDataset != null)
                {
                    _dataSetIndex.AddSharedReportDataSet(name, sharedDataset, fields);
                    continue;
                }

                var commandText = (string)(query.Items[query.ItemsElementName.ToList().IndexOf(ItemsChoiceType1.CommandText)]);
                var queryTypeIndex = query.ItemsElementName.ToList().IndexOf(ItemsChoiceType1.CommandType);
                if (queryTypeIndex > -1)
                {
                    var commandType = (QueryTypeCommandType)(query.Items[queryTypeIndex]);
                    if (commandType == QueryTypeCommandType.StoredProcedure)
                    {
                        commandText = MdxHelper.WrapIdentifier(commandText);
                        commandText = "EXECUTE " + commandText;
                    }
                }
                var dataSourceName = (string)(query.Items[query.ItemsElementName.ToList().IndexOf(ItemsChoiceType1.DataSourceName)]);
                var dataSource = _dataSourceIndex.GetReportDataSource(dataSourceName);

                QueryParametersType parameters = null;
                var paramsIdx = query.ItemsElementName.ToList().IndexOf(ItemsChoiceType1.QueryParameters);
                if (paramsIdx > -1)
                {
                    parameters = (QueryParametersType)(query.Items[paramsIdx]);
                }

                var dsUrn = _urnBuilder.DataSetUrn(name, parent.RefPath);
                var dataSetElement = new ReportDataSetElement(dsUrn, name, null, parent);
                parent.AddChild(dataSetElement);
                if (dataSetElement.RefPath.Path == "SSRSServer[@Name='FSCZPRCT0013']/Folder[@Name='/']/Folder[@Name='Administration']/Report[@Name='PREPAID SERVICES - DEALER VIEW']/DataSet[@Name='ds_ps_product']")
                {
                }
                LoadDataSet(dataSource, dataSetElement, commandText, fields, parameters);

                _dataSetIndex.AddReportDataSet(dataSetElement);
            }
        }

        private void LoadReportParameters(Report report, ReportElement parent)
        {
            _parameterIndex.ClearReportParameters();

            var parameters = report.Items.FirstOrDefault(x => x is ReportParametersType) as ReportParametersType;
            if (parameters == null)
            {
                return;
            }

            foreach (var parameter in parameters.ReportParameter)
            {
                var name = parameter.Name;
                var validValues = parameter.Items.FirstOrDefault(x => x is ValidValuesType) as ValidValuesType;
                var defaultValues = parameter.Items.FirstOrDefault(x => x is DefaultValueType) as DefaultValueType;
                var allowBlankIndex = parameter.ItemsElementName.ToList().IndexOf(ItemsChoiceType3.AllowBlank);
                var multivalueIndex = parameter.ItemsElementName.ToList().IndexOf(ItemsChoiceType3.MultiValue);
                var promptIndex = parameter.ItemsElementName.ToList().IndexOf(ItemsChoiceType3.Prompt);
                
                var parameterUrn = _urnBuilder.GetReportParameterUrn(name, parent.RefPath);
                var parameterElement = new ReportParameterElement(parameterUrn, name, null, parent);
                parent.AddChild(parameterElement);

                // parameter available values are specified
                if (validValues != null)
                {
                    var item = validValues.Items[0]; // can be multiple?

                    // available values from dataset
                    if (item is DataSetReferenceType)
                    {
                        var dsRef = item as DataSetReferenceType;
                        var dataSetName = dsRef.Items[dsRef.ItemsElementName.ToList().IndexOf(ItemsChoiceType2.DataSetName)] as string;
                        var valueField = dsRef.Items[dsRef.ItemsElementName.ToList().IndexOf(ItemsChoiceType2.ValueField)] as string;
                        var labelFieldIndex = dsRef.ItemsElementName.ToList().IndexOf(ItemsChoiceType2.LabelField);
                        string labelField = null;
                        if (labelFieldIndex > -1)
                        {
                            labelField = dsRef.Items[labelFieldIndex] as string;
                        }
                        var dataSet = _dataSetIndex.GetReportDataSet(dataSetName);
                        if (dataSet == null)

                        {
                            continue;
                        }

                        var dataSetElem = dataSet.ModelElement;
                        var valueFieldElem = dataSet[valueField];
                        
                        var dsValidValuesUrn = _urnBuilder.GetReportParameterValidValuesDataSetUrn(parameterElement.RefPath);
                        var dsValidValuesElement = new ReportParameterValidValuesDataSetElement(dsValidValuesUrn, "ValidValues", null, parameterElement);
                        parameterElement.AddChild(dsValidValuesElement);

                        dsValidValuesElement.DataSet = dataSetElem;
                        dsValidValuesElement.ValueField = valueFieldElem;
                        if (labelFieldIndex > -1)
                        {
                            var labelFieldElem = dataSet[labelField];
                            dsValidValuesElement.LabelField = labelFieldElem;
                        }
                    }

                    // available values specified in enumeration
                    else if (item is ParameterValuesType)
                    {
                        var values = item as ParameterValuesType;

                        var staticValidValuesUrn = _urnBuilder.GetReportParameterValidValuesStaticUrn(parameterElement.RefPath);
                        var staticValidValuesElement = new ReportParameterValidValuesStaticElement(staticValidValuesUrn, "ValidValues", null, parameterElement);
                        parameterElement.AddChild(staticValidValuesElement);

                        int valueCounter = 1;
                        foreach (var paramValue in values.ParameterValue)
                        {
                            var value = paramValue.Items.FirstOrDefault(x => x is string) as string;
                            var label = paramValue.Items.FirstOrDefault(x => x is StringLocIDType) as StringLocIDType;
                            string labelValue = null;
                            if (label != null)
                            {
                                labelValue = label.Value;
                                if (value == null)
                                {
                                    value = label.Value;
                                }
                            }
                            if (value == null)
                            {
                                value = string.Empty;
                            }

                            var paramValueUrn = _urnBuilder.GetParameterValueUrn(staticValidValuesElement.RefPath, valueCounter++);
                            var paramValueElement = new ParameterValueElement(paramValueUrn, value, null, staticValidValuesElement);
                            staticValidValuesElement.AddChild(paramValueElement);

                            paramValueElement.Value = value;
                            paramValueElement.Label = labelValue;
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

                // parameter default values are specified
                if (defaultValues != null)
                {
                    var item = defaultValues.Items[0]; // can be multiple?

                    // default values from dataset
                    if (item is DataSetReferenceType)
                    {
                        var dsRef = item as DataSetReferenceType;
                        var dataSetName = dsRef.Items[dsRef.ItemsElementName.ToList().IndexOf(ItemsChoiceType2.DataSetName)] as string;
                        var valueField = dsRef.Items[dsRef.ItemsElementName.ToList().IndexOf(ItemsChoiceType2.ValueField)] as string;

                        var dataSet = _dataSetIndex.GetReportDataSet(dataSetName);
                        if (dataSet == null)
                        {
                            continue;
                        }

                        var dataSetElem = dataSet.ModelElement;
                        var valueFieldElem = dataSet[valueField];

                        var dsDefaultValuesUrn = _urnBuilder.GetReportParameterDefaultValuesDataSetUrn(parameterElement.RefPath);
                        var dsDefaultValuesElement = new ReportParameterDefaultValuesDataSetElement(dsDefaultValuesUrn, "DefaultValues", null, parameterElement);
                        parameterElement.AddChild(dsDefaultValuesElement);

                        dsDefaultValuesElement.DataSet = dataSetElem;
                        dsDefaultValuesElement.ValueField = valueFieldElem;
                    }

                    // default values specified in enumeration
                    else if (item is ValuesType)
                    {
                        var values = item as ValuesType;

                        var staticDefaultValuesUrn = _urnBuilder.GetReportParameterDefaultValuesStaticUrn(parameterElement.RefPath);
                        var staticDefaultValuesElement = new ReportParameterDefaultValuesStaticElement(staticDefaultValuesUrn, "DefaultValues", null, parameterElement);
                        parameterElement.AddChild(staticDefaultValuesElement);

                        int valueCounter = 1;
                        foreach (var value in values.Value)
                        {
                            var valueUrn = _urnBuilder.GetParameterValueUrn(staticDefaultValuesElement.RefPath, valueCounter++);
                            var valueElement = new ParameterValueElement(valueUrn, value, null, staticDefaultValuesElement);
                            staticDefaultValuesElement.AddChild(valueElement);

                            valueElement.Value = value;
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

                if (promptIndex > -1)
                {
                    var prompt = (StringLocIDType)parameter.Items[promptIndex];
                    parameterElement.Prompt = prompt.Value;
                }
                if (allowBlankIndex > -1)
                {
                    var bv = (bool)parameter.Items[allowBlankIndex];
                    parameterElement.AllowBlank = bv;
                }
                if (multivalueIndex > -1)
                {
                    var mv = (bool)parameter.Items[multivalueIndex];
                    parameterElement.Multivalue = mv;
                }

                _parameterIndex.AddReportParameter(parameterElement);
            }
        }
        #endregion

        #region REPORT_BODY
        private void LoadReportBody(Report report, ReportElement parent)
        {
            _independentReportItemLevel = true;
            _independentTextBoxes = new List<TextBoxElement>();

            var sectionsCollection = report.Items.FirstOrDefault(x => x is ReportSectionsType) as ReportSectionsType;
            if (sectionsCollection == null)
            {
                var bodyElement = report.Items.FirstOrDefault(x => x is XmlElement && ((XmlElement)x).LocalName == "Body") as XmlElement;
                if (bodyElement == null)
                {
                    return;
                }

                XmlSerializer serializer = new XmlSerializer(typeof(BodyRootType));
                BodyRootType bodyObj = null;

                //var clearedDefinition = RemoveAllNamespaces(bodyElement.InnerXml);
                using (var stream = GenerateStreamFromString(bodyElement.OuterXml))
                {

                    bodyObj = (BodyRootType)(serializer.Deserialize(stream));
                }

                var sectionBodyUrn = _urnBuilder.GetReportSectionBodyUrn(parent.RefPath);
                var sectionBodyElement = new ReportSectionBodyElement(sectionBodyUrn, "SectoinBody", null, parent);
                parent.AddChild(sectionBodyElement);

                var items = bodyObj.Items.FirstOrDefault(x => x is ReportItemsType) as ReportItemsType;
                if (items == null)
                {
                    return;
                }
                LoadReportItems(items, sectionBodyElement);

                return;
            }
            for (int i = 0; i < sectionsCollection.ReportSection.Length; i++)
            {
                var sectionUrn = _urnBuilder.GetReportSectionUrn(parent.RefPath, i + 1);
                var section = sectionsCollection.ReportSection[i];
                
                var sectionElement = new ReportSectionElement(sectionUrn, string.Format("Section {0}", i + 1), null, parent);
                parent.AddChild(sectionElement);
                
                MeasureEnum sectionSizeMeasure = MeasureEnum.Cm;
                sectionElement.Position.Measure = MeasureEnum.Cm;
                var sectionWidthIdx = section.ItemsElementName.ToList().IndexOf(ItemsChoiceType119.Width);
                if (sectionWidthIdx > -1)
                {
                    var sectionWidthText = (string)section.Items[sectionWidthIdx];
                    var sectionWidth = ParseSize(sectionWidthText, out sectionSizeMeasure);
                    sectionElement.Position.Width = sectionWidth;
                    sectionElement.Position.Measure = sectionSizeMeasure;
                }

                var body = section.Items.First(x => x is BodyType) as BodyType;
                var sectionBodyUrn = _urnBuilder.GetReportSectionBodyUrn(sectionElement.RefPath);
                var sectionBodyElement = new ReportSectionBodyElement(sectionBodyUrn, "SectoinBody", null, sectionElement);
                parent.AddChild(sectionBodyElement);

                // TODO: extract body height

                var items = body.Items.FirstOrDefault(x => x is ReportItemsType) as ReportItemsType;
                if (items == null)
                {
                    continue;
                }
                LoadReportItems(items, sectionBodyElement);
            }
        }
        
        private void LoadReportItems(ReportItemsType items, SsrsModelElement parent)
        {
            // textboxes first
            foreach (var item in items.Items.OrderByDescending(x => (x is TextboxType ? 16 : 0) + 0))
            {
                if (item is TablixType)
                {
                    LoadTablix((TablixType)item, parent);
                }
                else if (item is RectangleType)
                {
                    LoadRectangle((RectangleType)item, parent);
                }
                else if (item is TextboxType)
                {
                    LoadTextBox((TextboxType)item, parent);
                }
                else if (item is ChartType)
                {
                    LoadChart((ChartType)item, parent);
                }
                else if (item is MapType)
                {
                    LoadMap((MapType)item, parent);
                }
                else if (item is GaugePanelType)
                {
                    LoadGaugePanel((GaugePanelType)item, parent);
                }
                else
                {
                    ConfigManager.Log.Info(string.Format("{0}: Skipping unsupported report item: {1}", parent.RefPath.Path, item.GetType().FullName));
                }
            }
        }

        private void LoadRectangle(RectangleType rectangle, SsrsModelElement parent)
        {
            var rectangleLeftIdx = rectangle.ItemsElementName.ToList().IndexOf(ItemsChoiceType17.Left);
            var rectangleTopIdx = rectangle.ItemsElementName.ToList().IndexOf(ItemsChoiceType17.Top);
            var rectangleWidthIdx = rectangle.ItemsElementName.ToList().IndexOf(ItemsChoiceType17.Width);
            var rectangleHeightIdx = rectangle.ItemsElementName.ToList().IndexOf(ItemsChoiceType17.Height);

            ReportDesignArea designArea = new ReportDesignArea() { Measure = MeasureEnum.Cm };
            // highest probability of being defined
            if (rectangleWidthIdx > -1)
            {
                MeasureEnum measureType;
                var rectangleWidthText = rectangle.Items[rectangleWidthIdx] as string;
                designArea.Width = ParseSize(rectangleWidthText, out measureType);
                designArea.Measure = measureType;
            }
            if (rectangleHeightIdx > -1)
            {
                var rectangleHeightText = rectangle.Items[rectangleHeightIdx] as string;
                designArea.Height = ParseSize(rectangleHeightText);
            }
            if (rectangleLeftIdx > -1)
            {
                var rectangleLeftText = rectangle.Items[rectangleLeftIdx] as string;
                designArea.Left = ParseSize(rectangleLeftText);
            }
            if (rectangleTopIdx > -1)
            {
                var rectangleTopText = rectangle.Items[rectangleTopIdx] as string;
                designArea.Top = ParseSize(rectangleTopText);
            }

            var rectangleUrn = _urnBuilder.GetRectangleUrn(parent.RefPath, rectangle.Name);
            RectangleElement rectangleElement = new RectangleElement(rectangleUrn, rectangle.Name, null, parent);
            rectangleElement.Position = designArea;
            parent.AddChild(rectangleElement);

            var reportItems = rectangle.Items.FirstOrDefault(x => x is ReportItemsType) as ReportItemsType;
            if (reportItems != null)
            {
                LoadReportItems(reportItems, rectangleElement);
            }
        }

        private CellContentsType[,] GetTablixAsTable(TablixType tablix, out List<double> columnWidths, 
            out List<double> rowHeights, out DataTable illustrativeTable, out List<bool> hiddenRowsList)
        {
            illustrativeTable = new DataTable();

            List<double> columnHierarchyHeights;
            List<double> rowHierarchyWidths;
            List<double> bodyColumnsWidths;
            List<double> bodyRowHeights;
            
            int columnDepth = GetTablixColumnDepth(tablix, out columnHierarchyHeights);
            int rowDepth = GetTablixRowDepth(tablix, out rowHierarchyWidths);
            var tablixBodyIndex = tablix.ItemsElementName.ToList().IndexOf(ItemsChoiceType84.TablixBody);
            var body = (TablixBodyType)tablix.Items[tablixBodyIndex];

            int bodyRowCount = GetTablixBodyRowCount(body, out bodyRowHeights);
            int bodyColumnCount = GetTablixBodyColumnCount(body, out bodyColumnsWidths);

            var tableColCount = rowDepth + bodyColumnCount;
            var tableRowCount = columnDepth + bodyRowCount;

            rowHeights = new List<double>(columnHierarchyHeights);
            rowHeights.AddRange(bodyRowHeights);
            columnWidths = new List<double>(rowHierarchyWidths);
            columnWidths.AddRange(bodyColumnsWidths);
            

            illustrativeTable = new DataTable();
            for (int i = 0; i < tableColCount; i++)
            {
                illustrativeTable.Columns.Add(new DataColumn(i.ToString(), typeof(string)));
            }
            for (int i = 0; i < tableRowCount; i++)
            {
                var nr = illustrativeTable.NewRow();
                illustrativeTable.Rows.Add(nr);
            }

            CellContentsType[,] cellContents = new CellContentsType[tableRowCount, tableColCount];

            var cornerIndex = tablix.ItemsElementName.ToList().IndexOf(ItemsChoiceType84.TablixCorner);
            if (cornerIndex > -1)
            {
                var tablixCorner = (TablixCornerType)tablix.Items[cornerIndex];
                FillTablixCorner(tablixCorner, illustrativeTable, cellContents, rowDepth, columnDepth);
            }

            hiddenRowsList = new List<bool>();
            var rowHierarchyIndex = tablix.ItemsElementName.ToList().IndexOf(ItemsChoiceType84.TablixRowHierarchy);
            if (rowHierarchyIndex > -1)
            {
                var rowHierarchy = (TablixHierarchyType)tablix.Items[rowHierarchyIndex];
                FillTablixRowHierarchy(rowHierarchy, illustrativeTable, cellContents, rowDepth, columnDepth, out hiddenRowsList);
            }

            var columnHierarchyIndex = tablix.ItemsElementName.ToList().IndexOf(ItemsChoiceType84.TablixColumnHierarchy);
            if (columnHierarchyIndex > -1)
            {
                var columnHierarchy = (TablixHierarchyType)tablix.Items[columnHierarchyIndex];
                FillTablixColumnHierarchy(columnHierarchy, illustrativeTable, cellContents, rowDepth, columnDepth);
            }

            FillTablixBody(body, illustrativeTable, cellContents, rowDepth, columnDepth, bodyRowCount, bodyColumnCount);

            // remove empty columns

            int empties = 0;
            for (int c = 0; c < tableColCount - empties - 1; c++)
            {
                var emptyColumn = true;
                for (int re = 0; re < tableRowCount; re++)
                {
                    if (cellContents[re, c] != null)
                    {
                        emptyColumn = false;
                        break;
                    }
                }

                if (emptyColumn)
                {
                    for (int mc = c; mc < tableColCount - 1; mc++)
                    {
                        for (int mr = 0; mr < tableRowCount; mr++)
                        {
                            cellContents[mr, mc] = cellContents[mr, mc + 1];
                        }
                    }

                    empties++;
                    for (int mr = 0; mr < tableRowCount; mr++)
                    {
                        cellContents[mr, tableColCount - empties] = null;
                    }

                    // the same column can be empty again after the shift
                    c--;
                }
            }

            // remove empty rows

            empties = 0;
            for (int r = 0; r < tableRowCount - empties - 1; r++)
            {
                var emptyRow = true;
                for (int ce = 0; ce < tableColCount; ce++)
                {
                    if (cellContents[r, ce] != null)
                    {
                        emptyRow = false;
                        break;
                    }
                }

                if (emptyRow)
                {
                    for (int mr = r; mr < tableRowCount - 1; mr++)
                    {
                        for (int mc = 0; mc < tableColCount; mc++)
                        {
                            cellContents[mr, mc] = cellContents[mr + 1, mc];
                        }
                    }

                    empties++;
                    for (int mc = 0; mc < tableColCount; mc++)
                    {
                        cellContents[tableRowCount - empties, mc] = null;
                    }

                    r--;
                }
            }

            //for (int r = 0; r < tableRowCount; r++)
            //{
            //    int rowColCount = tableColCount;
            //    while (cellContents[r, 0] == null)
            //    {
            //        rowColCount--;
            //        for (int i = 0; i < rowColCount; i++)
            //        {
            //            cellContents[r, i] = cellContents[r, i + 1];
            //        }
            //    }
            //}

            return cellContents;
        }

        private void FillTablixColumnHierarchy(TablixHierarchyType columnHierarchy, DataTable illustrativeTable, CellContentsType[,] cellContents, int rowHierarchyDepth, int columnHierarchyDepth)
        {
            var rowOffset = 0;
            int columnOffset = rowHierarchyDepth;
            var members = columnHierarchy.Items.OfType<TablixMembersType>().FirstOrDefault();
            if (members == null)
            {
                return;
            }
            foreach (TablixMemberType member in members.TablixMember)
            {
                var colSpan = FillablixColumnHierarchyMember(member, illustrativeTable, cellContents, rowOffset, columnOffset);
                columnOffset += colSpan;
            }
        }

        // returns column span
        private int FillablixColumnHierarchyMember(TablixMemberType member, DataTable illustrativeTable, CellContentsType[,] cellContentsTable, int rowOffset, int columnOffset)
        {
            int columnSpan = 1;
            if (member.Items == null)
            {
                return columnSpan;
            }

            var headerIndex = member.ItemsElementName.ToList().IndexOf(ItemsChoiceType83.TablixHeader);
            if (headerIndex > -1)
            {
                var header = (TablixHeaderType)member.Items[headerIndex];
                var cellContents = header.Items.OfType<CellContentsType>().FirstOrDefault();
                if (cellContents != null)
                {
                    cellContentsTable[rowOffset, columnOffset] = cellContents;
                    IllustrateCellContents(cellContents, illustrativeTable, rowOffset, columnOffset);
                }
            }

            var submembers = member.Items.OfType<TablixMembersType>().FirstOrDefault();
            if (submembers == null)
            {
                return columnSpan;
            }

            // next level of hierarchy
            rowOffset++;
            bool first = true;
            foreach (TablixMemberType submember in submembers.TablixMember)
            {
                var subColumnSpan = FillablixColumnHierarchyMember(submember, illustrativeTable, cellContentsTable, rowOffset, columnOffset);
                columnOffset += subColumnSpan;
                columnSpan += subColumnSpan;
                // this is a submember - the first member is displayed below to parent in the same column
                if (first)
                {
                    //columnOffset--;
                    columnSpan--;
                }
                first = false;
            }

            return columnSpan;
        }



        private void FillTablixRowHierarchy(TablixHierarchyType rowHierarchy, DataTable illustrativeTable, CellContentsType[,] cellContents, int rowHierarchyDepth, int columnHierarchyDepth, out List<bool> hiddenList)
        {
            var rowOffset = columnHierarchyDepth;
            int columnOffset = 0;
            var members = rowHierarchy.Items.OfType<TablixMembersType>().FirstOrDefault();
            hiddenList = new List<bool>();
            if (members == null)
            {
                return;
            }
            foreach (TablixMemberType member in members.TablixMember)
            {
                var rowSpan = FillablixRowHierarchyMember(member, illustrativeTable, cellContents, rowOffset, columnOffset,
                    hiddenList);
                rowOffset += rowSpan;
                
            }
        }

        // returns row span
        private int FillablixRowHierarchyMember(TablixMemberType member, DataTable illustrativeTable, CellContentsType[,] cellContentsTable, int rowOffset, int columnOffset, List<bool> hiddenList)
        {
            int rowSpan = 1;
            if (member.Items == null)
            {
                return rowSpan;
            }
            
            var headerIndex = member.ItemsElementName.ToList().IndexOf(ItemsChoiceType83.TablixHeader);
            if (headerIndex > -1)
            {
                var header = (TablixHeaderType)member.Items[headerIndex];
                var cellContents = header.Items.OfType<CellContentsType>().FirstOrDefault();
                if (cellContents != null)
                {
                    cellContentsTable[rowOffset, columnOffset] = cellContents;
                    IllustrateCellContents(cellContents, illustrativeTable, rowOffset, columnOffset);
                }
            }

            bool hidden = false;
            var visibilityIndex = member.ItemsElementName.ToList().IndexOf(ItemsChoiceType83.Visibility);
            if (visibilityIndex > -1)
            {
                var visibility = (VisibilityType)member.Items[visibilityIndex];
                var hiddenIndex = visibility.ItemsElementName.ToList().IndexOf(ItemsChoiceType14.Hidden);
                if (hiddenIndex > -1)
                {
                    if (!bool.TryParse(visibility.Items[hiddenIndex].ToString(), out hidden))
                    {
                        hidden = false;
                    }
                }
            }
            
            var submembers = member.Items.OfType<TablixMembersType>().FirstOrDefault();
            if (submembers == null)
            {
                hiddenList.Add(hidden);
                return rowSpan;
            }

            if (!(submembers.TablixMember.Any() && headerIndex == -1))
            {
                // next level of hierarchy
                // if there are submembers in this member and no header textbox,
                // keep the column offset - the TB is in the submember
                columnOffset++;
            }
            bool first = true;

            // visibility will be set for descendant members
            if (submembers.TablixMember.Length == 0)
            {
                hiddenList.Add(hidden);
            }

            foreach (TablixMemberType submember in submembers.TablixMember)
            {
                var subRowSpan = FillablixRowHierarchyMember(submember, illustrativeTable, cellContentsTable, rowOffset, columnOffset, hiddenList);
                rowOffset += subRowSpan;
                rowSpan += subRowSpan;
                // this is a submember - the first member is displayed right to the parent on the same row
                if (first)
                {
                    //rowOffset--;
                    rowSpan--;
                }
                first = false;
            }

            return rowSpan;
        }

        private void FillTablixBody(TablixBodyType tablixBody, DataTable illustrativeTable, CellContentsType[,] cellContentsTable, 
            int rowHierarchyDepth, int columnHierarchyDepth, int bodyRowCount, int bodyColumnCount)
        {
            var rowOffsetByColumn = new List<int>();
            for (int i = 0; i < bodyColumnCount; i++)
            {
                rowOffsetByColumn.Add(columnHierarchyDepth);
            }

            var rows = tablixBody.Items.First(x => x is TablixRowsType) as TablixRowsType;
            for (int i = 0; i < rows.Items.Length; i++)
            {
                var columnOffset = rowHierarchyDepth;

                var row = (TablixRowType)(rows.Items[i]);
                var rowCells = row.Items.First(x => x is TablixCellsType) as TablixCellsType;
                for (int j = 0; j < rowCells.Items.Length; j++)
                {
                    var cell = (TablixCellType)(rowCells.Items[j]);
                    if (cell.Items != null)
                    {
                        var cellContents = cell.Items.First(x => x is CellContentsType) as CellContentsType;
                        int rowSpan, columnSpan;
                        GetCellSpans(cellContents, out rowSpan, out columnSpan);

                        var rowOffset = rowOffsetByColumn[columnOffset - rowHierarchyDepth];
                        cellContentsTable[rowOffset, columnOffset] = cellContents;
                        IllustrateCellContents(cellContents, illustrativeTable, rowOffset, columnOffset);

                        for (int k = 0; k < columnSpan; k++)
                        {
                            rowOffsetByColumn[k + columnOffset - rowHierarchyDepth] += rowSpan;
                        }
                        columnOffset += columnSpan;
                    }
                }
            }
        }

        private void FillTablixCorner(TablixCornerType tablixCorner, DataTable illustrativeTable, CellContentsType[,] cellContentsTable, int cornerColumnCount, int cornerRowCount)
        {
            int rowIndex = 0;
            int columnIndex = 0;
            var rowsElem = tablixCorner.Items.FirstOrDefault(x => x is TablixCornerRowsType) as TablixCornerRowsType;
            if (rowsElem == null)
            {
                return;
            }

            // cells can have row spans > 1 => cells below are shifted
            List<int> rowOffsetsByColumn = new List<int>();
            for (int i = 0; i < cornerColumnCount; i++)
            {
                rowOffsetsByColumn.Add(0);
            }
            foreach (TablixCornerRowType row in rowsElem.Items)
            {
                int columnOffset = 0;
                foreach (TablixCornerCellType cell in row.Items.OfType<TablixCornerCellType>())
                {
                    if (cell.Items == null)
                    {
                        // e.g.: first row with rowspan 2 followed by 2 empty cells
                        continue;
                    }
                    var cellContents = cell.Items.OfType<CellContentsType>().FirstOrDefault();
                    
                    int rowSpan;
                    int colSpan;
                    GetCellSpans(cellContents, out rowSpan, out colSpan);

                    var rowOffset = rowOffsetsByColumn[columnOffset];
                    cellContentsTable[rowOffset, columnOffset] = cellContents;
                    IllustrateCellContents(cellContents, illustrativeTable, rowOffset, columnOffset);
                    

                    // shift the next cells
                    for (int i = 0; i < colSpan; i++)
                    {
                        rowOffsetsByColumn[columnOffset + i] += rowSpan;
                    }
                    columnOffset += colSpan;
                }
            }
        }

        private void IllustrateCellContents(CellContentsType cellContents, DataTable canvas, int row, int column)
        {
            var textBoxIndex = cellContents.ItemsElementName.ToList().IndexOf(ItemsChoiceType82.Textbox);
            var chartIndex = cellContents.ItemsElementName.ToList().IndexOf(ItemsChoiceType82.Chart);
            var gpIndex = cellContents.ItemsElementName.ToList().IndexOf(ItemsChoiceType82.GaugePanel);

            var tr = canvas.Rows[row];
            if (textBoxIndex > -1)
            {
                var tb = (TextboxType)cellContents.Items[textBoxIndex];
                tr[column] = tb.Name;
            }
            else if (chartIndex > -1)
            {
                var chart = (ChartType)cellContents.Items[chartIndex];
                tr[column] = chart.Name;
            }
            else if (gpIndex > -1)
            {
                var gp = (GaugePanelType)cellContents.Items[gpIndex];
                tr[column] = gp.Name;
            }
        }

        private void GetCellSpans(CellContentsType cellContents, out int rowSpan, out int columnSpan)
        {
            var rowSpanIndex = cellContents.ItemsElementName.ToList().IndexOf(ItemsChoiceType82.RowSpan);
            var columnSpanIndex = cellContents.ItemsElementName.ToList().IndexOf(ItemsChoiceType82.ColSpan);
            if (rowSpanIndex != -1)
            {
                var rowSpanText = cellContents.Items[rowSpanIndex].ToString();
                rowSpan = int.Parse(rowSpanText);
            }
            else
            {
                rowSpan = 1;
            }

            if (columnSpanIndex != -1)
            {
                var columnSpanText = cellContents.Items[columnSpanIndex].ToString();
                columnSpan = int.Parse(columnSpanText);
            }
            else
            {
                columnSpan = 1;
            }
        }

        private int GetTablixRowDepth(TablixType tablix, out List<double> heights)
        {
            var rowHierarchyIdx = tablix.ItemsElementName.ToList().IndexOf(ItemsChoiceType84.TablixRowHierarchy);
            if (rowHierarchyIdx == -1)
            {
                heights = new List<double>();
                return 0;
            }

            var rowHierarchy =  (TablixHierarchyType)tablix.Items[rowHierarchyIdx];
            return GetTablixHierarchyDepth(rowHierarchy, out heights);
        }

        private int GetTablixColumnDepth(TablixType tablix, out List<double> widths)
        {
            var columnHierarchyIdx = tablix.ItemsElementName.ToList().IndexOf(ItemsChoiceType84.TablixColumnHierarchy);
            if (columnHierarchyIdx == -1)
            {
                widths = new List<double>();
                return 0;
            }

            var columnHierarchy = (TablixHierarchyType)tablix.Items[columnHierarchyIdx];
            return GetTablixHierarchyDepth(columnHierarchy, out widths);
        }

        private int GetTablixHierarchyDepth(TablixHierarchyType hierarchy, out List<double> layoutSizes)
        {
            layoutSizes = new List<double>();
            int depth = 0;
            var members = hierarchy.Items.FirstOrDefault() as TablixMembersType;
            if (members == null)
            {
                return depth;
            }
            
            var membList = members.TablixMember;

            while (true)
            {
                if (membList == null)
                {
                    return depth;
                }
                if (membList.All(x => x.Items == null))
                {
                    return depth;
                }
                
                // some member has content
                depth++;

                var headers = membList.Where(x => x.Items != null).SelectMany(x => x.Items).Where(x => x is TablixHeaderType).Select(x => (TablixHeaderType)x).ToList();
                if (headers.Any())
                {
                    var sizes = headers.SelectMany(x => x.Items).Where(x => x is string).Select(x => ParseSize((string)x));
                    if (sizes.Any())
                    {
                        layoutSizes.Add(sizes.Min());
                    }
                    else
                    {
                        layoutSizes.Add(1);
                    }
                    //var items = headers.
                    // get size
                }
                else
                {
                    // ?!!
                    //layoutSizes.Add(1);
                }

                var nextMembers = membList.Where(x => x.Items != null).Where(x => x.ItemsElementName.ToList().IndexOf(ItemsChoiceType83.TablixMembers) > -1)
                    .Select(x => (TablixMembersType)x.Items[x.ItemsElementName.ToList().IndexOf(ItemsChoiceType83.TablixMembers)])
                    .ToList();

                membList = nextMembers.SelectMany(x => x.TablixMember).ToArray();
            }
        }
        
        private int GetTablixBodyRowCount(TablixBodyType body, out List<double> heights)
        {
            var rows = body.Items.FirstOrDefault(x => x is TablixRowsType) as TablixRowsType;
            heights = new List<double>();
            if (rows == null)
            {
                return 0;
            }
            int rowCount = 0;

            foreach (var row in rows.Items.Where(x => x is TablixRowType).Select(x => x as TablixRowType))
            {
                var heightText = (string)(row.Items[0]);
                var heightParsed = ParseSize(heightText);
                heights.Add(heightParsed);
                rowCount++;
            }
            return rowCount;
        }

        private int GetTablixBodyColumnCount(TablixBodyType body, out List<double> widths)
        {
            var columns = body.Items.FirstOrDefault(x => x is TablixColumnsType) as TablixColumnsType;
            widths = new List<double>();
            if (columns == null)
            {
                return 0;
            }
            int columnCount = 0;

            foreach (var column in columns.Items.Where(x => x is TablixColumnType).Select(x => x as TablixColumnType))
            {
                var widthText = (string)(column.Items[0]);
                var widthParsed = ParseSize(widthText);
                widths.Add(widthParsed);
                columnCount++;
            }
            return columnCount;
        }


        private void LoadTablix(TablixType tablix, SsrsModelElement parent)
        {
            //create element
            var tablixUrn = _urnBuilder.GetTablixUrn(parent.RefPath, tablix.Name);
            var tablixElement = new TablixElement(tablixUrn, tablix.Name, null, parent);
            parent.AddChild(tablixElement);

            MeasureEnum tablixSizeMeasure = MeasureEnum.Cm;
            tablixElement.Position.Measure = MeasureEnum.Cm;
            var tablixLeftIdx = tablix.ItemsElementName.ToList().IndexOf(ItemsChoiceType84.Left);
            var tablixTopIdx = tablix.ItemsElementName.ToList().IndexOf(ItemsChoiceType84.Top);
            var tablixWidthIdx = tablix.ItemsElementName.ToList().IndexOf(ItemsChoiceType84.Width);
            var tablixHeightIdx = tablix.ItemsElementName.ToList().IndexOf(ItemsChoiceType84.Height);
            // highest chance of being defined
            if (tablixWidthIdx > -1)
            {
                var tablixWidthText = (string)tablix.Items[tablixWidthIdx];
                var tablixWidth = ParseSize(tablixWidthText, out tablixSizeMeasure);
                tablixElement.Position.Width = tablixWidth;
                tablixElement.Position.Measure = tablixSizeMeasure;
            }
            if (tablixHeightIdx > -1)
            {
                var tablixHeightText = (string)tablix.Items[tablixHeightIdx];
                var tablixHeight = ParseSize(tablixHeightText);
                tablixElement.Position.Height = tablixHeight;
            }
            if (tablixLeftIdx > -1)
            {
                var tablixLeftText = (string)tablix.Items[tablixLeftIdx];
                var tablixLeft = ParseSize(tablixLeftText, out tablixSizeMeasure);
                tablixElement.Position.Left = tablixLeft;
            }
            if (tablixTopIdx > -1)
            {
                var tablixTopText = (string)tablix.Items[tablixTopIdx];
                var tablixTop = ParseSize(tablixTopText);
                tablixElement.Position.Top = tablixTop;
            }

            // find the dataset
            var originalContextDataset = _currentContextDataSet;
            var dataSetItemIndex = tablix.ItemsElementName.ToList().IndexOf(ItemsChoiceType84.DataSetName);
            if (dataSetItemIndex != -1)
            {
                var datasetName = (string)(tablix.Items[dataSetItemIndex]);
                _currentContextDataSet = _dataSetIndex.GetReportDataSet(datasetName);
                if (_currentContextDataSet != null)
                {
                    tablixElement.DataSet = _currentContextDataSet.ModelElement;
                }
            }

            DataTable illustrativeTable;
            List<double> columnWidths;
            List<double> rowHeights;
            List<bool> hiddenRowsList;
            var cellContentsTable = GetTablixAsTable(tablix, out columnWidths, 
                out rowHeights, out illustrativeTable, out hiddenRowsList);

            if (tablixElement.RefPath.Path ==
                "SSRSServer[@Name='localhost']/Folder[@Name='/']/Folder[@Name='Reports']/Report[@Name='04 - CDCM']/ReportSection[@Name='No_1']/Body/Rectangle[@Name='Rectangle6']/Tablix[@Name='Tablix1']")
            {
            }
            var totalWidth = columnWidths.Sum();
            var totalHeight = rowHeights.Sum();
            double topOffset = 0;

            Dictionary<int, int> columnsFilledUntilRow = new Dictionary<int, int>();
            for (int i = 0; i < columnWidths.Count; i++)
            {
                columnsFilledUntilRow.Add(i, -1);
            }

            for (int i = 0; i < rowHeights.Count; i++)
            {
                var rowUrn = _urnBuilder.GetTablixRowUrn(tablixElement.RefPath, i + 1);
                var rowElement = new TablixRowElement(rowUrn, string.Format("Row {0}", i + 1), null, tablixElement);
                tablixElement.AddChild(rowElement);
                
                rowElement.Position.Measure = MeasureEnum.Cm;
                rowElement.Position.Left = 0;
                rowElement.Position.Top = topOffset;
                rowElement.Position.Width = totalWidth;
                rowElement.Position.Height = rowHeights[i];

                rowElement.Hidden = false;
                if (hiddenRowsList.Count > i)
                {
                    rowElement.Hidden = hiddenRowsList[i];
                }

                double cellLeftOffset = 0;

                // columns that need to be skipped when positioning cells
                for (int j = 0; j < columnWidths.Count; j++)
                {
                    var columnWidth = columnWidths[j];

                    double cellWidth = columnWidth;
                    ReportDesignArea cellArea = new ReportDesignArea()
                    {
                        Measure = MeasureEnum.Cm,
                        Left = cellLeftOffset,
                        Top = 0,
                        Width = cellWidth,
                        Height = rowHeights[i]
                    };

                    if (cellContentsTable[i, j] != null)
                    {
                        int columnPositioningoffset = 0;

                        if (!columnsFilledUntilRow.ContainsKey(j + columnPositioningoffset))
                        {
                            continue;
                        }

                        while (columnsFilledUntilRow[j + columnPositioningoffset] >= i)
                        {
                            columnPositioningoffset++;
                            if (!columnsFilledUntilRow.ContainsKey(j + columnPositioningoffset))
                            {
                                break;
                            }
                        }

                        if (!columnsFilledUntilRow.ContainsKey(j + columnPositioningoffset))
                        {
                            continue;
                        }

                        if (columnPositioningoffset > 0)
                        {
                            columnsFilledUntilRow[j + columnPositioningoffset] = Math.Max(
                                columnsFilledUntilRow[j + columnPositioningoffset], i);

                            cellArea.Left = cellLeftOffset + columnWidths.Skip(j).Take(columnPositioningoffset).Sum();
                        }

                        var cellContents = cellContentsTable[i, j];
                        var cellUrn = _urnBuilder.GetCellUrn(rowElement.RefPath, j + 1);
                        var cellElement = new CellElement(cellUrn, string.Format("Cell {0}", j + 1), null, rowElement);
                        rowElement.AddChild(cellElement);
                        cellElement.Position = cellArea;
                        int colSpan, rowSpan;
                        LoadCellContents(cellContents, cellElement, out colSpan, out rowSpan);
                        if (colSpan > 1)
                        {
                            cellWidth = columnWidths.Skip(j).Take(colSpan).Sum();
                            cellArea.Width = cellWidth;
                            cellElement.Position.Width = cellWidth;

                            j += colSpan - 1;
                        }
                        if (rowSpan > 1)
                        {
                            var cellHeight = rowHeights.Skip(i).Take(rowSpan).Sum();
                            cellArea.Height = cellHeight;
                            cellElement.Position.Height = cellHeight;
                            for (int extCol = j; extCol < j + colSpan; extCol++)
                            {
                                columnsFilledUntilRow[extCol] = Math.Max(columnsFilledUntilRow[extCol], i + rowSpan - 1);
                            }
                            // j += colSpan - 1;
                        }
                    }

                    cellLeftOffset += cellWidth;

                }

                topOffset += rowHeights[i];
            }

            //var tablixBodyItemIndex = tablix.ItemsElementName.ToList().IndexOf(ItemsChoiceType84.TablixBody);
            //var tablixBody = (TablixBodyType)(tablix.Items[tablixBodyItemIndex]);

            //// get column widths - expected measure is CM
            //List<double> tablixColumnWidths = new List<double>();
            //var columns = tablixBody.Items.First(x => x is TablixColumnsType) as TablixColumnsType;
            //for (int i = 0; i < columns.Items.Length; i++)
            //{
            //    var column = columns.Items[i] as TablixColumnType;
            //    var columnWidthText = (string)(column.Items[0]);
            //    var widthParsed = ParseSize(columnWidthText);
            //    tablixColumnWidths.Add(widthParsed);
            //}
            //var totalWidth = tablixColumnWidths.Sum();

            //CellElement[] headerCells = new CellElement[columns.Items.Length];

            //var rows = tablixBody.Items.First(x => x is TablixRowsType) as TablixRowsType;
            //double rowTopOffset = 0;
            //for (int i = 0; i < rows.Items.Length; i++)
            //{
            //    var row = (TablixRowType)(rows.Items[i]);
            //    var rowHeightText = row.Items.First(x => x is string) as string;
            //    MeasureEnum rowSizeMeasure;
            //    var rowHeightParsed = ParseSize(rowHeightText, out rowSizeMeasure);

            //    var rowUrn = _urnBuilder.GetTablixRowUrn(tablixElement.RefPath, i + 1);
            //    var rowElement = new TablixRowElement(rowUrn, string.Format("Row {0}", i + 1), null, tablixElement);
            //    tablixElement.AddChild(rowElement);

            //    rowElement.Position.Measure = rowSizeMeasure;
            //    rowElement.Position.Left = 0;
            //    rowElement.Position.Top = rowTopOffset;
            //    rowElement.Position.Width = totalWidth;
            //    rowElement.Position.Height = rowHeightParsed;

            //    var rowCells = row.Items.First(x => x is TablixCellsType) as TablixCellsType;
            //    double cellLeftOffset = 0;
            //    for (int j = 0; j < rowCells.Items.Length; j++)
            //    {
            //        var columnWidth = tablixColumnWidths[j];

            //        ReportDesignArea cellArea = new ReportDesignArea()
            //        {
            //            Measure = rowSizeMeasure,
            //            Left = cellLeftOffset,
            //            Top = 0,    // relative to parent (row)
            //            Width = columnWidth,
            //            Height = rowHeightParsed
            //        };

            //        var cell = (TablixCellType)(rowCells.Items[j]);
            //        if (cell.Items != null)
            //        {
            //            var cellContents = cell.Items.First(x => x is CellContentsType) as CellContentsType;
            //            var cellUrn = _urnBuilder.GetCellUrn(rowElement.RefPath, j + 1);
            //            var cellElement = new CellElement(cellUrn, string.Format("Cell {0}", j + 1), null, rowElement);
            //            //if (cellElement.RefPath.Path == "SSRSServer[@Name='SSRS']/Folder[@Name='/']/Folder[@Name='Administration']/Report[@Name='PREPAID SERVICES - CONTRACT VIEW']/ReportSection[@Name='No_1']/Body/Tablix[@Name='Tablix1']/TablixRow[@Name='No_2']/Cell[@Name='No_1']")
            //            //{

            //            //}
            //            rowElement.AddChild(cellElement);
            //            cellElement.Position = cellArea;
            //            LoadCellContents(cellContents, cellElement);
            //            if (i == 0)
            //            {
            //                headerCells[j] = cellElement;
            //            }
            //            else
            //            {
            //                cellElement.CaptionCell = headerCells[j];
            //            }
            //        }
            //        cellLeftOffset += columnWidth;
            //    }

            //    rowTopOffset += rowHeightParsed;
            //}

            var tablixPosition = tablixElement.Position;
            var potentialCaptionTextBoxes = _independentTextBoxes.Where(x => IsCaptionLeftTop(x.Position, tablixPosition));
            if (potentialCaptionTextBoxes.Any())
            {
                tablixElement.CaptionTextBox = potentialCaptionTextBoxes.OrderBy(x => Distance(tablixPosition, x.Position)).First();
            }

            var rowHierarchyIdx = tablix.ItemsElementName.ToList().IndexOf(ItemsChoiceType84.TablixRowHierarchy);
            var rowHierarchy = (TablixHierarchyType)tablix.Items[rowHierarchyIdx];
            var rowHierarchyUrn = _urnBuilder.GetRowHierarchyUrn(tablixElement.RefPath);
            var rowHierarchyElement = new TablixRowHierarchyElement(rowHierarchyUrn, "RowHierarchy", null, tablixElement);
            tablixElement.AddChild(rowHierarchyElement);

            var columnHierarchyIdx = tablix.ItemsElementName.ToList().IndexOf(ItemsChoiceType84.TablixColumnHierarchy);
            var columnHierarchy = (TablixHierarchyType)tablix.Items[columnHierarchyIdx];
            var columnHierarchyUrn = _urnBuilder.GetColumnHierarchyUrn(tablixElement.RefPath);
            var columnHierarchyElement = new TablixColumnHierarchyElement(columnHierarchyUrn, "ColumnHierarchy", null, tablixElement);
            tablixElement.AddChild(columnHierarchyElement);

            if (rowHierarchy.Items.Length != 1 || columnHierarchy.Items.Length != 1)
            {
                throw new Exception("Exactly one members collection was expected for row and column hierarchies");
            }

            var rowMembers = (TablixMembersType)rowHierarchy.Items[0];
            var columnMembers = (TablixMembersType)columnHierarchy.Items[0];

            var memberCounter = 0;

            foreach (var member in rowMembers.TablixMember)
            {
                LoadTablixMember(member, rowHierarchyElement, tablixElement, ref memberCounter);
            }
            foreach (var member in columnMembers.TablixMember)
            {
                LoadTablixMember(member, columnHierarchyElement, tablixElement, ref memberCounter);
            }

            _currentContextDataSet = originalContextDataset;
        }

        private void LoadTablixMember(TablixMemberType member, SsrsModelElement parent, TablixElement rootTablix, ref int counter)
        {
            var memberUrn = _urnBuilder.GetHierarchyMemberUrn(parent.RefPath, counter);
            var memberElement = new HierarchyMemberElement(memberUrn, string.Format("Member {0}", counter), null, parent);
            parent.AddChild(memberElement);
            counter++;

            // e.g. a tablix will have a column hierarchy with one member even if there are no column groups
            if (member.Items == null)
            {
                return;
            }

            var headerIndex = member.ItemsElementName.ToList().IndexOf(ItemsChoiceType83.TablixHeader);
            var groupIndex = member.ItemsElementName.ToList().IndexOf(ItemsChoiceType83.Group);
            var membersIndex = member.ItemsElementName.ToList().IndexOf(ItemsChoiceType83.TablixMembers);

            if (headerIndex != -1)
            {
                var header = (TablixHeaderType)member.Items[headerIndex];
                var cellContentCount = header.Items.Where(x => x is CellContentsType).ToList();
                if (cellContentCount.Count != 1)
                {
                    throw new Exception("The member header is expected to contain exactly one cell content element (in a tablix)");
                }

                var cellContent = header.Items.First(x => x is CellContentsType) as CellContentsType;

                var textBoxIndex = cellContent.ItemsElementName.ToList().IndexOf(ItemsChoiceType82.Textbox);
                if (textBoxIndex != -1)
                {
                    var sb = new StringBuilder();
                    var textBox = (TextboxType)cellContent.Items[textBoxIndex];
                    LoadTextBox(textBox, memberElement);
                }
            }

            if (groupIndex != -1)
            {
                var group = (GroupType)member.Items[groupIndex];
                var groupUrn = _urnBuilder.GetHierarchyGroupUrn(memberElement.RefPath, group.Name);
                var groupElement = new HierarchyGroupElement(groupUrn, group.Name, null, memberElement);
                memberElement.AddChild(groupElement);

                if (group.ItemsElementName != null)
                {
                    var dataElementNameIndex = group.ItemsElementName.ToList().IndexOf(ItemsChoiceType24.DataElementName);
                    if (dataElementNameIndex > -1)
                    {
                        string dataElementName = (string)group.Items[dataElementNameIndex];
                        groupElement.DataElementName = dataElementName;
                    }
                }
            }

            if (membersIndex != -1)
            {
                var members = (TablixMembersType)member.Items[membersIndex];
                foreach (var subMember in members.TablixMember)
                {
                    LoadTablixMember(subMember, memberElement, rootTablix, ref counter);
                }
            }

            //if (headerIndex != -1 && groupIndex != -1)
            //{

            //}

        }

        public double Distance(ReportDesignArea a, ReportDesignArea b)
        {
            return Math.Sqrt(Math.Pow(a.Top - b.Top, 2) + Math.Pow(a.Left - b.Left, 2));
        }

        public bool IsCaptionLeftTop(ReportDesignArea caption, ReportDesignArea item)
        {
            var angle = Math.Atan2(item.Top - caption.Top, item.Left - caption.Left);
            return angle > Math.PI / 4 && angle < 3 * Math.PI / 4;
        }

        private void LoadChart(ChartType chart, SsrsModelElement parent)
        {
            //create element
            var chartUrn = _urnBuilder.GetChartUrn(parent.RefPath, chart.Name);
            var chartElement = new ChartElement(chartUrn, chart.Name, null, parent);
            parent.AddChild(chartElement);

            MeasureEnum chartSizeMeasure = MeasureEnum.Cm;
            chartElement.Position.Measure = MeasureEnum.Cm;
            var chartLeftIdx = chart.ItemsElementName.ToList().IndexOf(ItemsChoiceType46.Left);
            var chartTopIdx = chart.ItemsElementName.ToList().IndexOf(ItemsChoiceType46.Top);
            var chartWidthIdx = chart.ItemsElementName.ToList().IndexOf(ItemsChoiceType46.Width);
            var chartHeightIdx = chart.ItemsElementName.ToList().IndexOf(ItemsChoiceType46.Height);
            // highest chance of being defined
            if (chartWidthIdx > -1)
            {
                var chartWidthText = (string)chart.Items[chartWidthIdx];
                var chartWidth = ParseSize(chartWidthText, out chartSizeMeasure);
                chartElement.Position.Width = chartWidth;
                chartElement.Position.Measure = chartSizeMeasure;
            }
            if (chartHeightIdx > -1)
            {
                var chartHeightText = (string)chart.Items[chartHeightIdx];
                var chartHeight = ParseSize(chartHeightText);
                chartElement.Position.Height = chartHeight;
            }
            if (chartLeftIdx > -1)
            {
                var chartLeftText = (string)chart.Items[chartLeftIdx];
                var chartLeft = ParseSize(chartLeftText, out chartSizeMeasure);
                chartElement.Position.Left = chartLeft;
            }
            if (chartTopIdx > -1)
            {
                var chartTopText = (string)chart.Items[chartTopIdx];
                var chartTop = ParseSize(chartTopText);
                chartElement.Position.Top = chartTop;
            }

            // find the dataset
            var originalContextDataset = _currentContextDataSet;
            var dataSetItemIndex = chart.ItemsElementName.ToList().IndexOf(ItemsChoiceType46.DataSetName);
            if (dataSetItemIndex != -1)
            {
                var datasetName = (string)(chart.Items[dataSetItemIndex]);
                _currentContextDataSet = _dataSetIndex.GetReportDataSet(datasetName);
                if (_currentContextDataSet != null)
                {
                    chartElement.DataSet = _currentContextDataSet.ModelElement;
                }
            }

            var dataIndex = chart.ItemsElementName.ToList().IndexOf(ItemsChoiceType46.ChartData);
            var chartData = (ChartDataType)chart.Items[dataIndex];
            var chartDataUrn = _urnBuilder.GetChartDataUrn(chartElement.RefPath);
            var chartDataElement = new ChartDataElement(chartDataUrn, "ChartData", null, chartElement);
            chartElement.AddChild(chartDataElement);

            var seriesCollection = chartData.Items.FirstOrDefault(x => x is ChartSeriesCollectionType) as ChartSeriesCollectionType;
            if (seriesCollection == null)
            {
                _currentContextDataSet = originalContextDataset;
                return;   
            }

            foreach (ChartSeriesType series in seriesCollection.ChartSeries)
            {
                var seriesUrn = _urnBuilder.GetChartSeriesUrn(chartDataElement.RefPath, series.Name);
                var seriesElement = new ChartSeriesElement(seriesUrn, series.Name, null, chartDataElement);
                chartDataElement.AddChild(seriesElement);

                var seriesTypeIndex = series.ItemsElementName.ToList().IndexOf(ItemsChoiceType32.Type);
                var seriesSubtypeIndex = series.ItemsElementName.ToList().IndexOf(ItemsChoiceType32.Subtype);
                if(seriesTypeIndex > -1)
                {
                    var typeString = (string)(series.Items[seriesTypeIndex]);
                    seriesElement.Type = typeString;
                }
                if (seriesSubtypeIndex > -1)
                {
                    var subtypeString = (string)(series.Items[seriesSubtypeIndex]);
                    seriesElement.Subtype = subtypeString;
                }

                var pointsCollection = series.Items.FirstOrDefault(x => x is ChartDataPointsType) as ChartDataPointsType;
                if (pointsCollection == null)
                {
                    continue;
                }
                for(int i = 0; i < pointsCollection.ChartDataPoint.Length; i++)
                {
                    var point = pointsCollection.ChartDataPoint[i];
                    var pointUrn = _urnBuilder.GetChartDataPointUrn(seriesElement.RefPath, i + 1);
                    var pointElement = new ChartDataPointElement(pointUrn, string.Format("Point {0}", i + 1), null, seriesElement);
                    seriesElement.AddChild(pointElement);

                    var pointValues = point.Items.First(x => x is ChartDataPointValuesType) as ChartDataPointValuesType;
                    var highValueIdx = pointValues.ItemsElementName.ToList().IndexOf(ItemsChoiceType25.High);
                    var lowValueIdx = pointValues.ItemsElementName.ToList().IndexOf(ItemsChoiceType25.Low);
                    var xValueIdx = pointValues.ItemsElementName.ToList().IndexOf(ItemsChoiceType25.X);
                    var yValueIdx = pointValues.ItemsElementName.ToList().IndexOf(ItemsChoiceType25.Y);
                    if (highValueIdx > -1)
                    {
                        var expressionText = (string)(pointValues.Items[highValueIdx]);
                        var highValueExpression = LoadExpression(expressionText, pointElement, "HighValue");
                        pointElement.HighValue = highValueExpression;
                    }
                    if (lowValueIdx > -1)
                    {
                        var expressionText = (string)(pointValues.Items[lowValueIdx]);
                        var lowValueExpression = LoadExpression(expressionText, pointElement, "LowValue");
                        pointElement.LowValue = lowValueExpression;
                    }
                    if (xValueIdx > -1)
                    {
                        var expressionText = (string)(pointValues.Items[xValueIdx]);
                        var xValueExpression = LoadExpression(expressionText, pointElement, "X");
                        pointElement.X = xValueExpression;
                    }
                    if (yValueIdx > -1)
                    {
                        var expressionText = (string)(pointValues.Items[yValueIdx]);
                        var yValueExpression = LoadExpression(expressionText, pointElement, "Y");
                        pointElement.Y = yValueExpression;
                    }
                }
            }

            var tablixPosition = chartElement.Position;
            var potentialCaptionTextBoxes = _independentTextBoxes.Where(x => IsCaptionLeftTop(x.Position, tablixPosition));
            if (potentialCaptionTextBoxes.Any())
            {
                chartElement.CaptionTextBox = potentialCaptionTextBoxes.OrderBy(x => Distance(tablixPosition, x.Position)).First();
            }

            _currentContextDataSet = originalContextDataset;
        }

        private void LoadMap(MapType map, SsrsModelElement parent)
        {
            //create element
            var mapUrn = _urnBuilder.GetMapUrn(parent.RefPath, map.Name);
            var mapElement = new MapElement(mapUrn, map.Name, null, parent);
            parent.AddChild(mapElement);

            MeasureEnum mapSizeMeasure = MeasureEnum.Cm;
            mapElement.Position.Measure = MeasureEnum.Cm;
            var mapLeftIdx = map.ItemsElementName.ToList().IndexOf(ItemsChoiceType118.Left);
            var mapTopIdx = map.ItemsElementName.ToList().IndexOf(ItemsChoiceType118.Top);
            var mapWidthIdx = map.ItemsElementName.ToList().IndexOf(ItemsChoiceType118.Width);
            var mapHeightIdx = map.ItemsElementName.ToList().IndexOf(ItemsChoiceType118.Height);
            // highest chance of being defined
            if (mapWidthIdx > -1)
            {
                var mapWidthText = (string)map.Items[mapWidthIdx];
                var mapWidth = ParseSize(mapWidthText, out mapSizeMeasure);
                mapElement.Position.Width = mapWidth;
                mapElement.Position.Measure = mapSizeMeasure;
            }
            if (mapHeightIdx > -1)
            {
                var mapHeightText = (string)map.Items[mapHeightIdx];
                var mapHeight = ParseSize(mapHeightText);
                mapElement.Position.Height = mapHeight;
            }
            if (mapLeftIdx > -1)
            {
                var mapLeftText = (string)map.Items[mapLeftIdx];
                var mapLeft = ParseSize(mapLeftText, out mapSizeMeasure);
                mapElement.Position.Left = mapLeft;
            }
            if (mapTopIdx > -1)
            {
                var mapTopText = (string)map.Items[mapTopIdx];
                var mapTop = ParseSize(mapTopText);
                mapElement.Position.Top = mapTop;
            }

            var originalContextDataset = _currentContextDataSet;
            var dataRegions = map.Items.First(x => x is MapDataRegionsType) as MapDataRegionsType;
            foreach (var dataRegion in dataRegions.MapDataRegion)
            {
                // create element
                var dataRegionUrn = _urnBuilder.GetMapRegionUrn(mapElement.RefPath, dataRegion.Name);
                var dataRegionElement = new MapDataRegionElement(dataRegionUrn, dataRegion.Name, null, mapElement);
                mapElement.AddChild(dataRegionElement);
                // find dataset
                var dataSetName = dataRegion.Items.FirstOrDefault(x => x is string) as string;
                _currentContextDataSet = _dataSetIndex.GetReportDataSet(dataSetName);
                if (_currentContextDataSet != null)
                {
                    dataRegionElement.DataSet = _currentContextDataSet.ModelElement;
                }
                // load groups
                var members = dataRegion.Items.OfType<MapMemberType>();
                var groups = members.SelectMany(m => m.Items.OfType<GroupType>());
                foreach (var group in groups)
                {
                    var groupUrn = _urnBuilder.GetMapGroupUrn(dataRegionElement.RefPath, group.Name);
                    var groupElement = new MapGroupElement(groupUrn, group.Name, null, dataRegionElement);
                    dataRegionElement.AddChild(groupElement);

                    var groupExpressions = group.Items.OfType<GroupExpressionsType>().First();
                    for (int i = 0; i < groupExpressions.GroupExpression.Length; i++)
                    {
                        var expressionText = groupExpressions.GroupExpression[i];
                        var expressionElement = LoadExpression(expressionText, groupElement, string.Format("No_{0}", i + 1));
                    }
                }

                _currentContextDataSet = originalContextDataset;
            }
        }

        private void LoadGaugePanel(GaugePanelType gaugePanel, SsrsModelElement parent)
        {
            //create element
            var gaugePanelUrn = _urnBuilder.GetGaugePanelUrn(parent.RefPath, gaugePanel.Name);
            var gaugePanelElement = new GaugePanelElement(gaugePanelUrn, gaugePanel.Name, null, parent);
            parent.AddChild(gaugePanelElement);

            MeasureEnum gaugePanelSizeMeasure = MeasureEnum.Cm;
            gaugePanelElement.Position.Measure = MeasureEnum.Cm;
            var gaugePanelLeftIdx = gaugePanel.ItemsElementName.ToList().IndexOf(ItemsChoiceType75.Left);
            var gaugePanelTopIdx = gaugePanel.ItemsElementName.ToList().IndexOf(ItemsChoiceType75.Top);
            var gaugePanelWidthIdx = gaugePanel.ItemsElementName.ToList().IndexOf(ItemsChoiceType75.Width);
            var gaugePanelHeightIdx = gaugePanel.ItemsElementName.ToList().IndexOf(ItemsChoiceType75.Height);
            // highest chance of being defined
            if (gaugePanelWidthIdx > -1)
            {
                var gaugePanelWidthText = (string)gaugePanel.Items[gaugePanelWidthIdx];
                var gaugePanelWidth = ParseSize(gaugePanelWidthText, out gaugePanelSizeMeasure);
                gaugePanelElement.Position.Width = gaugePanelWidth;
                gaugePanelElement.Position.Measure = gaugePanelSizeMeasure;
            }
            if (gaugePanelHeightIdx > -1)
            {
                var gaugePanelHeightText = (string)gaugePanel.Items[gaugePanelHeightIdx];
                var gaugePanelHeight = ParseSize(gaugePanelHeightText);
                gaugePanelElement.Position.Height = gaugePanelHeight;
            }
            if (gaugePanelLeftIdx > -1)
            {
                var gaugePanelLeftText = (string)gaugePanel.Items[gaugePanelLeftIdx];
                var gaugePanelLeft = ParseSize(gaugePanelLeftText, out gaugePanelSizeMeasure);
                gaugePanelElement.Position.Left = gaugePanelLeft;
            }
            if (gaugePanelTopIdx > -1)
            {
                var gaugePanelTopText = (string)gaugePanel.Items[gaugePanelTopIdx];
                var gaugePanelTop = ParseSize(gaugePanelTopText);
                gaugePanelElement.Position.Top = gaugePanelTop;
            }

            // find the dataset
            var originalContextDataset = _currentContextDataSet;
            var dataSetItemIndex = gaugePanel.ItemsElementName.ToList().IndexOf(ItemsChoiceType75.DataSetName);
            if (dataSetItemIndex != -1)
            {
                var datasetName = (string)(gaugePanel.Items[dataSetItemIndex]);
                _currentContextDataSet = _dataSetIndex.GetReportDataSet(datasetName);
                if (_currentContextDataSet != null)
                {
                    gaugePanelElement.DataSet = _currentContextDataSet.ModelElement;
                }
            }

            // only one gauge type will be used - linear, radial or state indicator

            foreach (var linearGauge in gaugePanel.Items.OfType<LinearGaugesType>().SelectMany(x => x.LinearGauge))
            {
                var gaugeUrn = _urnBuilder.GetGaugeUrn(gaugePanelElement.RefPath, linearGauge.Name);
                var gaugeElement = new ScalesGaugeElement(gaugeUrn, linearGauge.Name, null, gaugePanelElement);
                gaugeElement.GaugeType = GaugeElement.GaugeTypeEnum.Linear;
                gaugePanelElement.AddChild(gaugeElement);

                foreach (var linearScale in linearGauge.Items.OfType<LinearScalesType>().SelectMany(x => x.LinearScale))
                {
                    var scaleUrn = _urnBuilder.GetGaugeScaleUrn(gaugeElement.RefPath, linearScale.Name);
                    var scaleElement = new GaugeScaleElement(scaleUrn, linearScale.Name, null, gaugeElement);
                    gaugeElement.AddChild(scaleElement);

                    foreach (var linearPointer in linearScale.Items.OfType<LinearPointersType>().SelectMany(x => x.LinearPointer))
                    {
                        var pointerUrn = _urnBuilder.GetGaugePointerUrn(scaleElement.RefPath, linearPointer.Name);
                        var pointerElement = new GaugePointerElement(pointerUrn, linearPointer.Name, null, scaleElement);
                        scaleElement.AddChild(pointerElement);

                        var gaugeInputValue = linearPointer.Items.OfType<GaugeInputValueType>().FirstOrDefault();
                        if (gaugeInputValue != null)
                        {
                            var valueIndex = gaugeInputValue.ItemsElementName.ToList().IndexOf(ItemsChoiceType50.Value);
                            var valueExpressionText = (string)(gaugeInputValue.Items[valueIndex]);
                            var valueExpressionElement = LoadExpression(valueExpressionText, pointerElement);
                        }
                    }
                }
            }

            foreach (var radialGauge in gaugePanel.Items.OfType<RadialGaugesType>().SelectMany(x => x.RadialGauge))
            {
                var gaugeUrn = _urnBuilder.GetGaugeUrn(gaugePanelElement.RefPath, radialGauge.Name);
                var gaugeElement = new ScalesGaugeElement(gaugeUrn, radialGauge.Name, null, gaugePanelElement);
                gaugeElement.GaugeType = GaugeElement.GaugeTypeEnum.Radial;
                gaugePanelElement.AddChild(gaugeElement);

                foreach (var radialScale in radialGauge.Items.OfType<RadialScalesType>().SelectMany(x => x.RadialScale))
                {
                    var scaleUrn = _urnBuilder.GetGaugeScaleUrn(gaugeElement.RefPath, radialScale.Name);
                    var scaleElement = new GaugeScaleElement(scaleUrn, radialScale.Name, null, gaugeElement);
                    gaugeElement.AddChild(scaleElement);

                    foreach (var radialPointer in radialScale.Items.OfType<RadialPointersType>().SelectMany(x => x.RadialPointer))
                    {
                        var pointerUrn = _urnBuilder.GetGaugePointerUrn(scaleElement.RefPath, radialPointer.Name);
                        var pointerElement = new GaugePointerElement(pointerUrn, radialPointer.Name, null, scaleElement);
                        scaleElement.AddChild(pointerElement);

                        var gaugeInputValue = radialPointer.Items.OfType<GaugeInputValueType>().FirstOrDefault();
                        if (gaugeInputValue != null)
                        {
                            var valueIndex = gaugeInputValue.ItemsElementName.ToList().IndexOf(ItemsChoiceType50.Value);
                            var valueExpressionText = (string)(gaugeInputValue.Items[valueIndex]);
                            var valueExpressionElement = LoadExpression(valueExpressionText, pointerElement);
                        }
                    }
                }
            }

            foreach (var stateIndicator in gaugePanel.Items.OfType<StateIndicatorsType>().SelectMany(x => x.StateIndicator))
            {
                var gaugeUrn = _urnBuilder.GetGaugeUrn(gaugePanelElement.RefPath, stateIndicator.Name);
                var gaugeElement = new IndicatorGaugeElement(gaugeUrn, stateIndicator.Name, null, gaugePanelElement);
                gaugeElement.GaugeType = GaugeElement.GaugeTypeEnum.Indicator;
                gaugePanelElement.AddChild(gaugeElement);

                var inputValueIdx = stateIndicator.ItemsElementName.ToList().IndexOf(ItemsChoiceType72.GaugeInputValue);
                var minValueIdx = stateIndicator.ItemsElementName.ToList().IndexOf(ItemsChoiceType72.MinimumValue);
                var maxValueIdx = stateIndicator.ItemsElementName.ToList().IndexOf(ItemsChoiceType72.MaximumValue);

                if (inputValueIdx > -1)
                {
                    var inputValue = (GaugeInputValueType)(stateIndicator.Items[inputValueIdx]);
                    var valueIndex = inputValue.ItemsElementName.ToList().IndexOf(ItemsChoiceType50.Value);
                    var valueExpressionText = (string)(inputValue.Items[valueIndex]);
                    if (valueExpressionText != "NaN")
                    {
                        var valueExpressionElement = LoadExpression(valueExpressionText, gaugeElement, "InputValueExpression");
                        gaugeElement.InputValue = valueExpressionElement;
                    }
                }

                if (minValueIdx > -1)
                {
                    var minValue = (GaugeInputValueType)(stateIndicator.Items[minValueIdx]);
                    var valueIndex = minValue.ItemsElementName.ToList().IndexOf(ItemsChoiceType50.Value);
                    var valueExpressionText = (string)(minValue.Items[valueIndex]);
                    if (valueExpressionText != "NaN")
                    {
                        var valueExpressionElement = LoadExpression(valueExpressionText, gaugeElement, "MinValueExpression");
                        gaugeElement.MinValue = valueExpressionElement;
                    }
                }

                if (maxValueIdx > -1)
                {
                    var maxValue = (GaugeInputValueType)(stateIndicator.Items[maxValueIdx]);
                    var valueIndex = maxValue.ItemsElementName.ToList().IndexOf(ItemsChoiceType50.Value);
                    var valueExpressionText = (string)(maxValue.Items[valueIndex]);
                    if (valueExpressionText != "NaN")
                    {
                        var valueExpressionElement = LoadExpression(valueExpressionText, gaugeElement, "MaxValueExpression");
                        gaugeElement.MaxValue = valueExpressionElement;
                    }
                }
            }

            var tablixPosition = gaugePanelElement.Position;
            var potentialCaptionTextBoxes = _independentTextBoxes.Where(x => IsCaptionLeftTop(x.Position, tablixPosition));
            if (potentialCaptionTextBoxes.Any())
            {
                gaugePanelElement.CaptionTextBox = potentialCaptionTextBoxes.OrderBy(x => Distance(tablixPosition, x.Position)).First();
            }

            _currentContextDataSet = originalContextDataset;
        }

        private void LoadCellContents(CellContentsType cell, CellElement cellElement, 
            out int colSpan, out int rowSpan)
        {
            var originalIndependenceLevel = _independentReportItemLevel;
            _independentReportItemLevel = false;

            if (cell.ItemsElementName.Where(x => x != ItemsChoiceType82.Item && x != ItemsChoiceType82.ColSpan
                && x != ItemsChoiceType82.RowSpan).Count() > 1)
            {
                throw new Exception();
            }
            var itemIndex = cell.ItemsElementName.ToList().FindIndex(x => x != ItemsChoiceType82.Item && x != ItemsChoiceType82.ColSpan
                && x != ItemsChoiceType82.RowSpan);
            var cellItem = cell.Items[itemIndex];
            
            switch (cell.ItemsElementName[itemIndex])
            {
                case ItemsChoiceType82.Tablix:
                    LoadTablix((TablixType)cellItem, cellElement);
                    break;
                case ItemsChoiceType82.Textbox:
                    LoadTextBox((TextboxType)cellItem, cellElement);
                    break;
                case ItemsChoiceType82.GaugePanel:
                    LoadGaugePanel((GaugePanelType)cellItem, cellElement);
                    break;
                case ItemsChoiceType82.Chart:
                    LoadChart((ChartType)cellItem, cellElement);
                    break;
                //case ItemsChoiceType82.ColSpan:
                //    colSpan = (int)cellItem;
                //    break;
                default:
                    ConfigManager.Log.Info(string.Format("{0}: Skipping unsupported report item: {1}", cellElement.RefPath.Path, cellElement.GetType().FullName));
                    break;
            }

            colSpan = 1;
            var colSpanItemIndex = cell.ItemsElementName.ToList().FindIndex(x => x == ItemsChoiceType82.ColSpan);
            if (colSpanItemIndex > -1)
            {
                colSpan = int.Parse((cell.Items[colSpanItemIndex]).ToString());
            }

            rowSpan = 1;
            var rowSpanItemIndex = cell.ItemsElementName.ToList().FindIndex(x => x == ItemsChoiceType82.RowSpan);
            if (rowSpanItemIndex > -1)
            {
                rowSpan = int.Parse((cell.Items[rowSpanItemIndex]).ToString());
            }

            _independentReportItemLevel = originalIndependenceLevel;
        }

        private void LoadTextBox(TextboxType textBox, SsrsModelElement parent)
        {
            // create element
            var textBoxLeftIdx = textBox.ItemsElementName.ToList().IndexOf(ItemsChoiceType21.Left);
            var textBoxTopIdx = textBox.ItemsElementName.ToList().IndexOf(ItemsChoiceType21.Top);
            var textBoxWidthIdx = textBox.ItemsElementName.ToList().IndexOf(ItemsChoiceType21.Width);
            var textBoxHeightIdx = textBox.ItemsElementName.ToList().IndexOf(ItemsChoiceType21.Height);
            //var textBoxHeightIdx = textBox.ItemsElementName.ToList().IndexOf(ItemsChoiceType21.f);
            var styleIdx = textBox.ItemsElementName.ToList().IndexOf(ItemsChoiceType21.Style);

            ReportDesignArea designArea = new ReportDesignArea() { Measure = MeasureEnum.Cm };
            // highest probability of being defined
            if (textBoxWidthIdx > -1)
            {
                MeasureEnum measureType;
                var textBoxWidthText = textBox.Items[textBoxWidthIdx] as string;
                designArea.Width = ParseSize(textBoxWidthText, out measureType);
                designArea.Measure = measureType;
            }
            if (textBoxHeightIdx > -1)
            {
                var textBoxHeightText = textBox.Items[textBoxHeightIdx] as string;
                designArea.Height = ParseSize(textBoxHeightText);
            }
            if (textBoxLeftIdx > -1)
            {
                var textBoxLeftText = textBox.Items[textBoxLeftIdx] as string;
                designArea.Left = ParseSize(textBoxLeftText);
            }
            if (textBoxTopIdx > -1)
            {
                var textBoxTopText = textBox.Items[textBoxTopIdx] as string;
                designArea.Top = ParseSize(textBoxTopText);
            }
            
            var textBoxUrn = _urnBuilder.GetTextBoxUrn(parent.RefPath, textBox.Name);
            var textBoxElement = new TextBoxElement(textBoxUrn, textBox.Name, null, parent);
            textBoxElement.Position = designArea;
            parent.AddChild(textBoxElement);

            if (styleIdx > -1)
            {
                StyleType tbStyle = (StyleType)textBox.Items[styleIdx];
                if (tbStyle != null && tbStyle.Items != null)
                {
                    var formatIdx = tbStyle.ItemsElementName.ToList().IndexOf(ItemsChoiceType11.Format);
                    if (formatIdx > -1)
                    {
                        string format = (string)tbStyle.Items[formatIdx];
                        if (!string.IsNullOrEmpty(format))
                        {
                            textBoxElement.Format = format;
                        }
                    }
                }
            }

            // read content
            var expressionOrdinal = 1;
            var paragraphs = textBox.Items.First(x => x is ParagraphsType) as ParagraphsType;
            foreach (ParagraphType paragraph in paragraphs.Paragraph)
            {
                var textRuns = paragraph.Items.First(x => x is TextRunsType) as TextRunsType;
                foreach (TextRunType textRun in textRuns.TextRun)
                {
                    var valueIdx = textRun.ItemsElementName.ToList().IndexOf(ItemsChoiceType18.Value);
                    var trStyleIdx = textRun.ItemsElementName.ToList().IndexOf(ItemsChoiceType18.Style);

                    var valueLocAttr = (LocIDStringWithDataTypeAttribute)(textRun.Items[valueIdx]);
                    var value = valueLocAttr.Value;
                    
                    LoadExpression(value, textBoxElement, string.Format("Expression_{0}", expressionOrdinal++));

                    if (trStyleIdx > -1)
                    {
                        StyleType trStyle = (StyleType)textRun.Items[trStyleIdx];
                        if (trStyle != null && trStyle.Items != null)
                        {
                            var formatIdx = trStyle.ItemsElementName.ToList().IndexOf(ItemsChoiceType11.Format);
                            if (formatIdx > -1)
                            {
                                string format = (string)trStyle.Items[formatIdx];
                                // take only the first text run - will likely cover the whole textbox
                                if (!string.IsNullOrEmpty(format) && textBoxElement.Format == null)
                                {
                                    //if (textBoxElement.Format != null && textBoxElement.Format != format)
                                    //{
                                    //    throw new Exception("Text box format specified multiple times");
                                    //}
                                    textBoxElement.Format = format;
                                }
                            }
                        }
                    }
                }
            }

            if (_independentReportItemLevel)
            {
                _independentTextBoxes.Add(textBoxElement);
            }
        }
        #endregion

        #region EXPRESSION_PARSING
        private SsrsModelElement TryResolveReference(SsrsExpressionTreeNavigator.PotentialReference reference)
        {
            switch (reference.ReferenceType)
            {
                case SsrsExpressionTreeNavigator.ReferenceTypeEnum.Field:
                    if (_currentContextDataSet == null)
                    {
                        return null;
                    }
                    return _currentContextDataSet.GetField(reference.Identifier);
                case SsrsExpressionTreeNavigator.ReferenceTypeEnum.Parameter:
                    return _parameterIndex.GetReportParameter(reference.Identifier);
                default:
                    throw new Exception();
            }
        }

        private SsrsExpressionElement LoadExpression(string value, SsrsModelElement parent, string name = null)
        {
            //var expressionUrn = _urnBuilder.GetExpressionUrn(parent.RefPath, ordinal);
            RefPath expressionUrn;
            if (name == null)
            {
                expressionUrn = _urnBuilder.GetExpressionUrn(parent.RefPath);
            }
            else
            {
                expressionUrn = _urnBuilder.GetExpressionUrn(parent.RefPath, name);
            }

            // empty cell
            if (value == null)
            {
                value = string.Empty;
            }
            
            // replace ommitted parameters with 0
            var origVal = value;
            var nonEmptyArgsVal = Regex.Replace(origVal, "([(,])(\\s*)([,)])", "$1 0 $2$3");
            while (origVal != nonEmptyArgsVal)
            {
                origVal = nonEmptyArgsVal;
                nonEmptyArgsVal = Regex.Replace(origVal, "([(,])(\\s*)([,)])", "$1 0 $2$3");
            }

            var expressionElement = new SsrsExpressionElement(expressionUrn, "Expression", nonEmptyArgsVal, parent);
            parent.AddChild(expressionElement);
            
            var valueTrimmed = value.Trim();
            // constant
            if (!valueTrimmed.StartsWith("="))
            {
                return expressionElement;
            }
            
            var parsed = _expressionParser.Parse(nonEmptyArgsVal);
            if (parsed.Root == null)
            {
                // TODO log errors
                foreach (var msg in parsed.ParserMessages)
                {
                    ConfigManager.Log.Warning("Failed to parse SSRS expression: " + value + "[" + nonEmptyArgsVal + "]");
                    ConfigManager.Log.Warning(msg.Message + " at " + msg.Location.Position.ToString() + ", " + parent.RefPath.Path);
                }
                return expressionElement;
            }
            expressionElement.OffsetFrom = parsed.Root.Span.EndPosition - parsed.Root.Span.EndPosition;
            expressionElement.Length = parsed.Root.Span.Length;

            var navigator = new SsrsExpressionTreeNavigator(parsed);
            var parts = navigator.GetTermsAndContent(parsed.Root, nonEmptyArgsVal);
            var potentialReferences = navigator.GetPotentialReferences(parsed.Root, nonEmptyArgsVal);

            int fragmentOrdinal = 1;
            foreach (var potentialReference in potentialReferences)
            {
                var resolution = TryResolveReference(potentialReference);
                if (resolution != null)
                {
                    var segmentUrn = _urnBuilder.GetExpressionFragmentUrn(fragmentOrdinal++, expressionElement.RefPath);
                    SsrsExpressionFragmentElement refSegmentElement = new SsrsExpressionFragmentElement(segmentUrn, "Segment_" + fragmentOrdinal, potentialReference.Identifier, expressionElement);
                    expressionElement.AddChild(refSegmentElement);

                    refSegmentElement.OffsetFrom = potentialReference.ParseTreeNode.Span.EndPosition - potentialReference.ParseTreeNode.Span.Length;
                    refSegmentElement.Length = potentialReference.ReferenceLength;
                    refSegmentElement.Reference = resolution;
                }
            }
            return expressionElement;
        }
        #endregion

        private void LoadReports()
        {
            //var reports = items.Where(x => x.ItemType == SsrsItemTypeEnum.Report).OrderBy(x => x.Name).ToList();

            var reports = _stageManager.GetExtractItems(_extractId, _currentComponent.SsrsProjectComponentId,
                DLS.DAL.Objects.Extract.ExtractTypeEnum.SsrsReport).Select(x => (SsrsReport)x).ToList();

            foreach (var reportItem in reports)
            {
                //if (!reportItem.Name.Contains("Overview"))
                //{
                //    continue;
                //}
                LoadReport(reportItem);

            }
        }


        public ReportElement LoadReport(SsrsReport reportItem, SsrsProjectComponent ssrsComponent)
        {
            var origComponent = _currentComponent;
            _currentComponent = ssrsComponent;
            var res = LoadReport(reportItem);
            _currentComponent = origComponent;
            return res;
        }

        public ReportElement LoadReport(SsrsReport reportItem)
        {
            var parentFolder = GetParentFolder(reportItem);
            var dsUrn = _urnBuilder.GetReportUrn(reportItem.Name, parentFolder.RefPath);
            var reportElement = new ReportElement(dsUrn, reportItem.Name, null, parentFolder);
            reportElement.SsrsPath = reportItem.FullPath;
            reportElement.SsrsComponentId = _currentComponent.SsrsProjectComponentId;
            parentFolder.AddChild(reportElement);

            //if (!reportItem.Name.Contains("Overview"))
            //{
            //    return reportElement;
            //}

            //var definition = _rs.GetItemDefinition(reportItem.Path);
            var stringDefinition = reportItem.Content; // BytesToText(definition);
            var clearedDefinition = RemoveAllNamespaces(stringDefinition);

            XmlSerializer serializer = new XmlSerializer(typeof(Report));
            Report reportObj = null;

            try
            {
                using (var stream = GenerateStreamFromString(clearedDefinition))
                {
                    reportObj = (Report)(serializer.Deserialize(stream));
                }
            }
            catch (Exception ex)
            {
                ConfigManager.Log.Error(ex.Message);
                return reportElement;
            }
            LoadReportDataSources(reportObj, reportElement);
            LoadReportDataSets(reportObj, reportElement);
            LoadReportParameters(reportObj, reportElement);
            LoadReportBody(reportObj, reportElement);

            return reportElement;
        }

        private FolderElement GetParentFolder(SsrsItem catalogItem)
        {
            var parentPath = GetParentFolderPath(catalogItem);
            FolderElement parentFolderElement;
            try
            {
                parentFolderElement = _folderPathMap[parentPath];
            }
            catch
            {
                ConfigManager.Log.Error(string.Format("Folder {0} was not found among {1}", parentPath, string.Join(", ", _folderPathMap.Keys)));
                throw;
            }
            return parentFolderElement;
        }

        private FolderElement GetParentFolder(string path)
        {
            var parentPath = GetParentFolderPath(path);
            FolderElement parentFolderElement;
            try
            {
                parentFolderElement = _folderPathMap[parentPath];
            }
            catch
            {
                ConfigManager.Log.Error(string.Format("Folder {0} was not found among {1}", parentPath, string.Join(", ", _folderPathMap.Keys)));
                throw;
            }
            return parentFolderElement;
        }


        private string GetParentFolderPath(SsrsItem catalogItem)
        {
            var path = catalogItem.FullPath;
            var parentPath = path.Substring(0, path.LastIndexOf("/"));
            if (parentPath == "")
            {
                parentPath = "/";
            }
            return parentPath;
        }

        private string GetParentFolderPath(string path)
        {
            var parentPath = path.Substring(0, path.LastIndexOf("/"));
            if (parentPath == "")
            {
                parentPath = "/";
            }
            return parentPath;
        }

        #region HELPERS
        private string BytesToText(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        private string RemoveAllNamespaces(string xmlDocument, string descendToElementName = null)
        {
            if (!char.IsSymbol(xmlDocument[0]))
            {
                xmlDocument = xmlDocument.Substring(1);
            }
            var xElem = XElement.Parse(xmlDocument);
            if (descendToElementName != null)
            {
                xElem = xElem.DescendantNodesAndSelf().First(x => x is XElement && ((XElement)x).Name.LocalName == descendToElementName) as XElement;
            }
            XElement xmlDocumentWithoutNs = RemoveAllNamespaces(xElem);

            return xmlDocumentWithoutNs.ToString();
        }

        private XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;

                foreach (XAttribute attribute in xmlDocument.Attributes())
                {
                    xElement.Add(attribute);
                }

                return xElement;
            }
            var res = new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
            foreach (XAttribute attribute in xmlDocument.Attributes())
            {
                if (attribute.Name.LocalName != "xmlns")
                {
                    res.Add(attribute);
                }
            }
            return res;
        }

        private Regex measureSuffixRegex = new Regex(@"\D+$");
        private Regex sizeValuePrefixRegex = new Regex(@"^\d+(\.\d+)?");

        private double ParseSize(string text, out MeasureEnum measureType)
        {
            var origCulture = Thread.CurrentThread.CurrentCulture;
            if (origCulture.Name != "en-US")
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            }

            var value = sizeValuePrefixRegex.Match(text).Value;
            var measure = measureSuffixRegex.Match(text).Value.ToLower();

            switch (measure)
            {
                case "cm":
                    measureType = MeasureEnum.Cm;
                    break;
                case "in":
                    measureType = MeasureEnum.In;
                    break;
                case "pt":
                    measureType = MeasureEnum.Pt;
                    break;
                case "mm":
                    measureType = MeasureEnum.Mm;
                    break;
                default:
                    throw new Exception();
            }

            var valueParsed = double.Parse(value);

            if (origCulture.Name != "en-US")
            {
                Thread.CurrentThread.CurrentCulture = origCulture;
            }

            return valueParsed;
        }

        private double ParseSize(string text)
        {
            MeasureEnum dummy;
            return ParseSize(text, out dummy);
        }

        #endregion
        private Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
