using CD.DLS.Common.Structures;
using CD.DLS.Common.Tools;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects.Extract;
using Microsoft.SqlServer.ReportingServices2010;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace CD.DLS.Extract.Mssql.Ssrs
{
    class SsrsExtractor
    {
        private SsrsProjectComponent _ssrsProject;
        private string _outputDirPath;
        private string _relativePathBase;
        private Manifest _manifest;



        public SsrsExtractor(SsrsProjectComponent ssrsProjectComponent, string outputDirPath, string relativePathBase, Manifest manifest)
        {
            _ssrsProject = ssrsProjectComponent;
            _outputDirPath = outputDirPath;
            _relativePathBase = relativePathBase;
            _manifest = manifest;
        }

        public void Extract()
        {
            var serverName = _ssrsProject.ServerName;

            ConfigManager.Log.Important(string.Format("Extracting SSRS items from {0}, folder {1}", _ssrsProject.ServerName, _ssrsProject.CombinedFolder));

            
            var projDirName = $"Project_{_ssrsProject.SsrsProjectComponentId}_{serverName}";
            projDirName = FileTools.NormalizeFileName(projDirName);

            _outputDirPath = Path.Combine(_outputDirPath, projDirName);
            _relativePathBase = Path.Combine(_relativePathBase, projDirName);

            Directory.CreateDirectory(_outputDirPath);


            List<SsrsItem> items = null;

            //switch (_ssrsProject.SsrsMode)
            //{
            //    case SsrsModeEnum.Native:
            //        items = ExtractNativeMode();
            //        break;
            //    case SsrsModeEnum.SpIntegrated:
            //        items = ExtractIntegratedMode();
            //        break;
            //    default: throw new Exception();
            //}

            items = ExtractNativeMode();

            int counter = 0;
            foreach (var item in items)
            {
                //StageManager.SaveExtract(item, _requestId, _ssrsProject.SsrsProjectComponentId);

                var fileSerialized = item.Serialize();
                var jsonFileName = FileTools.NormalizeFileName("SsrsItem_" + (counter++).ToString() + "_" + item.FileName.Substring(0, Math.Min(item.FileName.Length, 10)) + ".json");
                var savePath = Path.Combine(_outputDirPath, jsonFileName);
                var relativePath = Path.Combine(_relativePathBase, jsonFileName);
                File.WriteAllText(savePath, fileSerialized);
                _manifest.Items.Add(new ManifestItem()
                {
                    ComponentId = _ssrsProject.SsrsProjectComponentId,
                    Name = jsonFileName,
                    ExtractType = item.ExtractType.ToString(),
                    RelativePath = relativePath
                });

            }

        }
        
        private List<SsrsItem> ExtractNativeMode()
        {
            List<SsrsItem> res = new List<SsrsItem>();
            
            var rs = new ReportingService2010();
            rs.Credentials = System.Net.CredentialCache.DefaultCredentials;
            // Set the base Web service URL of the source server  
            rs.Url = _ssrsProject.SsrsServiceUrl; // "http://localhost/reportserver/reportservice2010.asmx";

            ConfigManager.Log.Important(string.Format("Extracting SSRS items from {0}", rs.Url));

            var items = rs.FindItems(_ssrsProject.FolderPath, BooleanOperatorEnum.Or, new Microsoft.SqlServer.ReportingServices2010.Property[] { }, new SearchCondition[] { });
            
            foreach (var item in items)
            {
                byte[] definition = new byte[0];
                if (item.TypeName != "Folder")
                {
                    definition = rs.GetItemDefinition(item.Path);
                }
                string dsConnstring = null;
                string dsExtension = null;
                if (item.TypeName == "DataSource")
                {
                    var dsDef = rs.GetDataSourceContents(item.Path);
                    dsConnstring = dsDef.ConnectString;
                    dsExtension = dsDef.Extension;
                }

                SsrsItem download = null;

                SsrsItemTypeEnum itemType;
                switch (item.TypeName)
                {
                    case "Report":
                        itemType = SsrsItemTypeEnum.Report;

                        download = new SsrsReport()
                        {
                            ID = item.ID,
                            FullPath = item.Path,
                            FileName = item.Name,
                            ItemType = itemType,
                            Content = item.TypeName == "Folder" ? null : Encoding.UTF8.GetString(definition),
                            DataSourceExtension = dsExtension,
                            DataSourceConnectionString = dsConnstring
                        };
                        break;
                    case "DataSet":
                        itemType = SsrsItemTypeEnum.SharedDataSet;

                        download = new SsrsSharedDataSet()
                        {
                            ID = item.ID,
                            FullPath = item.Path,
                            FileName = item.Name,
                            ItemType = itemType,
                            Content = item.TypeName == "Folder" ? null : Encoding.UTF8.GetString(definition),
                            DataSourceExtension = dsExtension,
                            DataSourceConnectionString = dsConnstring
                        };
                        break;
                    case "DataSource":
                        itemType = SsrsItemTypeEnum.SharedDataSource;

                        download = new SsrsSharedDataSource()
                        {
                            ID = item.ID,
                            FullPath = item.Path,
                            FileName = item.Name,
                            ItemType = itemType,
                            Content = item.TypeName == "Folder" ? null : Encoding.UTF8.GetString(definition),
                            DataSourceExtension = dsExtension,
                            DataSourceConnectionString = dsConnstring
                        }; break;
                    case "Folder":
                        itemType = SsrsItemTypeEnum.Folder;

                        download = new SsrsFolder()
                        {
                            ID = item.ID,
                            FullPath = item.Path,
                            FileName = item.Name,
                            ItemType = itemType,
                            Content = item.TypeName == "Folder" ? null : Encoding.UTF8.GetString(definition),
                            DataSourceExtension = dsExtension,
                            DataSourceConnectionString = dsConnstring
                        };
                        break;
                    default: continue;
                }

                

                ConfigManager.Log.Important(download.FullPath);

                res.Add(download);
            }
            
            return res;
        }

        private List<SsrsItem> ExtractIntegratedMode()
        {
            Sharepoint.SharepointDataProvider dataProvider = new Sharepoint.SharepointDataProvider();
            var extensions = new string[] { ".rdl", ".rsd", ".rds", ".rsds" };
            var items = dataProvider.ExtractFolderContents(_ssrsProject.SharePointBaseUrl, _ssrsProject.SharePointFolder, extensions);

            List<SsrsItem> res = new List<SsrsItem>();


            foreach (var item in items)
            {
                SsrsItemTypeEnum itemType = SsrsItemTypeEnum.Folder;
                var fullPath = "/" + item.RelativePath.TrimStart('/');
                SsrsItem download = null;

                if (item.Type == Sharepoint.SharepointLibraryItemTypeEnum.File)
                {
                    var extension = Path.GetExtension(item.Name);
                    switch (extension)
                    {
                        case ".rdl":
                            itemType = SsrsItemTypeEnum.Report;

                            download = new SsrsReport()
                            {
                                ID = item.Name,
                                FullPath = item.RelativePath,
                                FileName = item.Name,
                                ItemType = itemType,
                                Content = itemType == SsrsItemTypeEnum.Folder ? null : item.Content
                            };
                            break;
                        case ".rsd":
                            itemType = SsrsItemTypeEnum.SharedDataSet;

                            download = new SsrsSharedDataSet()
                            {
                                ID = item.Name,
                                FullPath = item.RelativePath,
                                FileName = item.Name,
                                ItemType = itemType,
                                Content = itemType == SsrsItemTypeEnum.Folder ? null : item.Content
                            };
                            break;
                        case ".rds":
                        case ".rsds":
                            itemType = SsrsItemTypeEnum.SharedDataSource;

                            download = new SsrsSharedDataSource()
                            {
                                ID = item.Name,
                                FullPath = item.RelativePath,
                                FileName = item.Name,
                                ItemType = itemType,
                                Content = itemType == SsrsItemTypeEnum.Folder ? null : item.Content
                            };
                            break;
                        default: throw new Exception("Unexpected SSRS item extension in " + item.Name);
                    }
                }
                else if (item.Type == Sharepoint.SharepointLibraryItemTypeEnum.Folder)
                {
                    itemType = SsrsItemTypeEnum.Folder;

                    download = new SsrsFolder()
                    {
                        ID = item.Name,
                        FullPath = item.RelativePath,
                        FileName = item.Name,
                        ItemType = itemType,
                        Content = itemType == SsrsItemTypeEnum.Folder ? null : item.Content
                    };
                }

                //}

                //SsrsItem download = new SsrsItem()
                //{
                //    ID = item.Name,
                //    FullPath = item.RelativePath,
                //    FileName = item.Name,
                //    ItemType = itemType,
                //    Content = itemType == SsrsItemTypeEnum.Folder ? null : item.Content //,
                //    //DataSourceExtension = dsExtension,
                //    //DataSourceConnectionString = dsConnstring
                //};

                if (itemType == SsrsItemTypeEnum.SharedDataSource)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(download.Content);
                    var root = doc.DocumentElement;
                    var connectStringTag = root.GetElementsByTagName("ConnectString")[0];
                    var extensionTag = root.GetElementsByTagName("Extension")[0];
                    var connectString = connectStringTag.InnerText;
                    var extension = extensionTag.InnerText;
                    ConfigManager.Log.Important(string.Format("DataSource {0} : {1}", download.FileName, connectString));
                    download.DataSourceConnectionString = connectString;
                    download.DataSourceExtension = extension;
                }

                ConfigManager.Log.Important(download.FullPath);

                res.Add(download);
            }

            return res;
        }
    }
}
