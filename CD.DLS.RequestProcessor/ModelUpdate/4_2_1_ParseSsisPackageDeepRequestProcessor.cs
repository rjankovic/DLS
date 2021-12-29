//using CD.DLS.API;
//using CD.DLS.API.ModelUpdate;
//using CD.DLS.Common.Structures;
//using CD.DLS.DAL.Objects.Extract;
//using CD.DLS.Model.Serialization;
//using CD.DLS.Model.Mssql.Ssis;
//using CD.DLS.Parse.Mssql.Ssis;
//using System.Linq;
//using CD.DLS.Parse.Mssql.Db;
//using CD.DLS.DAL.Configuration;
//using System.Collections.Generic;
//using System;
//using System.Diagnostics;

//namespace CD.DLS.RequestProcessor.ModelUpdate
//{
//    public class ParseSsisPackageDeepRequestProcessor : RequestProcessorBase, IRequestProcessor<ParseSsisPackagesDeepRequest, DLSApiMessage>
//    {
//        public DLSApiMessage Process(ParseSsisPackagesDeepRequest request, ProjectConfig projectConfig)
//        {
//            /**/
//            Stopwatch sw = new Stopwatch();
//            sw.Start();

//            var itemIdx = request.ItemIndex;

//            if (itemIdx >= request.Items.Count)
//            {
//                return new DLSApiMessage();
//            }

//            try
//            {
//                SerializationHelper serializationHelper = new SerializationHelper(projectConfig, GraphManager);
//                var projectElement = (ProjectElement)serializationHelper.LoadElementModelToChildrenOfType(request.ProjectRefPath, typeof(PackageElement));
                
//                var premappedIds = serializationHelper.CreatePremappedModel(projectElement);

//                var foldersModel = (ServerElement)serializationHelper.LoadElementModelToChildrenOfType(request.ServerRefPath, typeof(FolderElement));

//                var folderElement = foldersModel.Children.OfType<CatalogElement>().First().Children.OfType<FolderElement>().First(x => x.RefPath.Path == request.FolderRefPath);
//                projectElement.Parent = folderElement;

//                var projectElementWithCMs = (ProjectElement)serializationHelper.LoadElementModelToChildrenOfType(request.ProjectRefPath, typeof(ConnectionManagerElement));
//                var projectCMs = new List<ConnectionManagerElement>();

//                if (projectElementWithCMs != null)
//                {
//                    projectElementWithCMs.ConnectionManagers.ToList();
//                    foreach (var projectCM in projectCMs)
//                    {
//                        var map = serializationHelper.CreatePremappedModel(projectCM);
//                        foreach (var mapItem in map)
//                        {
//                            premappedIds.Add(mapItem.Key, mapItem.Value);
//                        }
//                    }
//                }

                
//                var cms = new List<ConnectionManagerElement>();
//                if (projectElementWithCMs != null)
//                {
//                    cms = projectElementWithCMs.ConnectionManagers.ToList();
//                }

//                AvailableDatabaseModelIndex adbix = new AvailableDatabaseModelIndex(projectConfig, GraphManager);

//                do
//                {
//                    var item = request.Items[itemIdx];

//                    projectElement.Parent = folderElement;
                    
//                    var packageExtract = (SsisPackage)StageManager.GetExtractItem(item.PackageExractItemId);
//                    var xmlExtract = (SsisPackageFile)StageManager.GetExtractItem(item.XmlExtractItemId);
//                    var packageElement = projectElement.Packages.First(x => x.RefPath.Path == item.PackageRefPath);

//                    ConfigManager.Log.Important("Parsing " + packageExtract.Urn + " deep");

//                    SsisXmlProvider xmlProvider = new SsisXmlProvider(request.ExtractId, request.SsisComponentId, StageManager, xmlExtract);
//                    ProjectModelParser projectModelParser = new ProjectModelParser(xmlProvider, adbix, projectConfig, request.ExtractId, StageManager);
                    
//                    projectModelParser.ParsePackage(request.SsisComponentId, projectElement, packageElement,
//                        packageExtract, cms);

//                    projectElement.Parent = null;


//                    var dbIdMap = adbix.GetAllPremappedIds();
//                    foreach (var kv in dbIdMap)
//                    {
//                        if (!premappedIds.ContainsKey(kv.Key))
//                        {
//                            premappedIds.Add(kv.Key, kv.Value);
//                        }
//                    }

//                    serializationHelper.SaveModelPart(packageElement, premappedIds, true);

//                    itemIdx++;

//                } while (sw.ElapsedMilliseconds / 1000 < ConfigManager.ServiceTimeout / 3 && itemIdx < request.Items.Count);

//            }
//            catch (Exception ex)
//            {
//                ConfigManager.Log.Error(ex.Message);
//                ConfigManager.Log.Error(ex.StackTrace);
//                if (ex.InnerException != null)
//                {
//                    ConfigManager.Log.Error(ex.InnerException.Message);
//                    ConfigManager.Log.Error(ex.InnerException.StackTrace);
//                }
//                if (ex is AggregateException)
//                {
//                    var aggregate = (AggregateException)ex;
//                    ConfigManager.Log.Error(aggregate.InnerExceptions.First().Message);
//                    ConfigManager.Log.Error(aggregate.InnerExceptions.First().StackTrace);
//                }
//                ConfigManager.Log.Warning("Skipping the errornous package");
//                itemIdx++;
//                ConfigManager.Log.FlushMessages();


//            }
//             /**/

//            if (itemIdx >= request.Items.Count)
//            {
//                return new DLSApiMessage();
//            }
//            else
//            {
//                return new DLSApiProgressResponse()
//                {
//                    ContinueWith = new ParseSsisPackagesDeepRequest()
//                    {
//                        ExtractId = request.ExtractId,
//                        FolderRefPath = request.FolderRefPath,
//                        ItemIndex = itemIdx, //request.ItemIndex + 1,
//                        Items = request.Items,
//                        ProjectRefPath = request.ProjectRefPath,
//                        ServerRefPath = request.ServerRefPath,
//                        SsisComponentId = request.SsisComponentId
//                    }
//                };
//            }
//        }
//    }
//}
