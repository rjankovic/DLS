//using CD.DLS.API;
//using CD.DLS.API.ModelUpdate;
//using CD.DLS.Common.Structures;
//using CD.DLS.DAL.Objects.Extract;
//using CD.DLS.Model.Interfaces;
//using CD.DLS.Model.Mssql;
//using CD.DLS.Model.Serialization;
//using CD.DLS.Parse.Mssql.Ssas;
//using System.Collections.Generic;
//using System.Linq;
//using CD.DLS.Model.Mssql.Ssis;
//using CD.DLS.Parse.Mssql.Ssis;
//using System;
//using CD.DLS.Parse.Mssql.Db;
//using CD.DLS.DAL.Configuration;
//using System.Web;

//namespace CD.DLS.RequestProcessor.ModelUpdate
//{
//    public class ParseSsisProjectDeepRequestProcessor : RequestProcessorBase, IRequestProcessor<ParseSsisProjectDeepRequest, DLSApiProgressResponse>
//    {
//        public DLSApiProgressResponse Process(ParseSsisProjectDeepRequest request, ProjectConfig projectConfig)
//        {
//            //List<DLSApiMessage> parsePackageRequests = new List<DLSApiMessage>();

//            SerializationHelper serializationHelper = new SerializationHelper(projectConfig, GraphManager);
//            var projectElement = (ProjectElement)serializationHelper.LoadElementModel(request.ProjectRefPath);
//            var premappedIds = serializationHelper.CreatePremappedModel(projectElement);

//            var foldersModel = (ServerElement)serializationHelper.LoadElementModelToChildrenOfType(request.ServerRefPath, typeof(FolderElement));

//            var folderElement = foldersModel.Children.OfType<CatalogElement>().First().Children.OfType<FolderElement>().First(x => x.RefPath.Path == request.FolderRefPath);
//            projectElement.Parent = folderElement;

//            var packages = StageManager.GetExtractItems(request.ExtractId, request.SsisComponentId, ExtractTypeEnum.SsisPackage);
//            var xmlFiles = StageManager.GetExtractItems(request.ExtractId, request.SsisComponentId, ExtractTypeEnum.SsisPackageFile);

//            var joins = packages.Join(xmlFiles, x => x.Name, y => HttpUtility.UrlDecode(y.Name), (pkg, xml) => new Tuple<SsisPackage, SsisPackageFile>((SsisPackage)pkg, (SsisPackageFile)xml))
//                .Join(projectElement.Packages, x => x.Item1.Name, me => me.Caption, (j, p) => new Tuple<SsisPackage, SsisPackageFile, PackageElement>(j.Item1, j.Item2, p));
//            if (joins.Count() != packages.Count)
//            {
//                var missingPacakges = packages.Where(p => !joins.Any(x => x.Item1 == p)).ToList();
//                var missingXmls = xmlFiles.Where(xml => !joins.Any(x => x.Item2 == xml)).ToList();

//                throw new Exception();
//            }

//            AvailableDatabaseModelIndex adbix = new AvailableDatabaseModelIndex(projectConfig, GraphManager);

//            List<ParseSsisPackageItem> items = new List<ParseSsisPackageItem>();

//            //joins = joins.Take(100);

//            foreach (var jn in joins)
//            {
//                //SerializationHelper serializationHelper = new SerializationHelper(projectConfig, GraphManager);
//                //var projectElement = (ProjectElement)serializationHelper.LoadElementModelToChildrenOfType(request.ProjectRefPath, typeof(PackageElement));
//                //var premappedIds = serializationHelper.CreatePremappedModel(projectElement);

//                //var projectElementWithCMs = (ProjectElement)serializationHelper.LoadElementModelToChildrenOfType(request.ProjectRefPath, typeof(ConnectionManagerElement));
//                //var projectCMs = projectElementWithCMs.ConnectionManagers.ToList();
//                //foreach (var projectCM in projectCMs)
//                //{
//                //    var map = serializationHelper.CreatePremappedModel(projectCM);
//                //    foreach (var item in map)
//                //    {
//                //        premappedIds.Add(item.Key, item.Value);
//                //    }
//                //}

//                //var packageExtract = (SsisPackage)jn.  StageManager.GetExtractItem(request.PackageExractItemId);
//                //var xmlExtract = (SsisPackageFile)StageManager.GetExtractItem(request.XmlExtractItemId);
//                //var packageElement = projectElement.Packages.First(x => x.RefPath.Path == request.PackageRefPath);

//                /*
//                var packageExtract = jn.Item1;
//                var xmlExtract = jn.Item2;
//                var packageElement = jn.Item3;

//                ConfigManager.Log.Important("Parsing " + packageExtract.Urn + " deep");
                
//                SsisXmlProvider xmlProvider = new SsisXmlProvider(request.ExtractId, request.SsisComponentId, StageManager, xmlExtract);
//                ProjectModelParser projectModelParser = new ProjectModelParser(xmlProvider, adbix, projectConfig, request.ExtractId, StageManager);
//                projectModelParser.ParsePackage(request.SsisComponentId, projectElement, packageElement,
//                    packageExtract);
//                    */

//                items.Add(new ParseSsisPackageItem()
//                {
//                    PackageExractItemId = jn.Item1.ExtractItemId,
//                    XmlExtractItemId = jn.Item2.ExtractItemId,
//                    PackageRefPath = jn.Item3.RefPath.Path
//                });

//                //parsePackageRequests.Add(new ParseSsisPackageDeepRequest()
//                //{
//                //    ExtractId = request.ExtractId,
//                //    PackageExractItemId = jn.Item2,
//                //    XmlExtractItemId = jn.Item3,
//                //    PackageRefPath = jn.Item4,
//                //    ProjectRefPath = projectElement.RefPath.Path,
//                //    SsisComponentId = request.SsisComponentId
//                //});
//            };

//            //items = items.Take(10).ToList();

//            foreach (var kv in adbix.GetAllPremappedIds())
//            {
//                if (!premappedIds.ContainsKey(kv.Key))
//                {
//                    premappedIds.Add(kv.Key, kv.Value);
//                }
//            }

//            projectElement.Parent = null;
//            serializationHelper.SaveModelPart(projectElement, premappedIds, true);

//            return new DLSApiProgressResponse()
//            {
//                ContinueWith = new ParseSsisPackagesDeepRequest()
//                {
//                    ExtractId = request.ExtractId,
//                    ItemIndex = 0,
//                    ProjectRefPath = projectElement.RefPath.Path,
//                    SsisComponentId = request.SsisComponentId,
//                    Items = items,
//                    ServerRefPath = request.ServerRefPath,
//                    FolderRefPath = request.FolderRefPath
//                }
//            };

//            //return new DLSApiProgressResponse()
//            //{
//            //    ParallelRequests = parsePackageRequests
//            //};
//        }
//    }
//}
