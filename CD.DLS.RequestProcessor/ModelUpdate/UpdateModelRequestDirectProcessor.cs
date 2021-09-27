using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.DAL.Receiver;
using CD.DLS.Model;
using CD.DLS.Model.DependencyGraph.KnowledgeBase;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Serialization;
using CD.DLS.Parse.Mssql;
using CD.DLS.Parse.Mssql.Db;
using CD.DLS.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class UpdateModelRequestDirectProcessor : RequestProcessorBase, IRequestProcessor<UpdateModelRequest, DLSApiMessage>
    {
        public DLSApiMessage Process(UpdateModelRequest request, ProjectConfig projectConfig)
        {
            // set model unavailable
            var msgs = RequestManager.GetActiveBroadcastMessages();
            foreach (var msg in msgs.Where(x => x.Type == DAL.Receiver.BroadcastMessageType.ProjectUpdateFinished))
            {
                RequestManager.SetBroadcastMessageInactive(msg);
            }

            // clear model
            GraphManager.ClearModel(projectConfig.ProjectConfigId, RequestId);

            SqlShallowParser sqlShallowParser = new SqlShallowParser();

            //var dbComponent = projectConfig.DatabaseComponents.First(x => x.MssqlDbProjectComponentId == request.DbComponentId);

            GraphManager.SetRefPathIntervals(projectConfig.ProjectConfigId);

            #region SQL_DATABASES
            // parse SQL DBs

            Model.Serialization.SerializationHelper sh = new Model.Serialization.SerializationHelper(projectConfig, GraphManager);

            var solutionModel = sh.LoadElementModelToChildrenOfType(
                string.Empty,
                typeof(Model.Mssql.Db.ServerElement)) as SolutionModelElement;

            //var solutionModel = new SolutionModelElement(new RefPath(""), "");

            var premappedModel = sh.CreatePremappedModel(solutionModel);

            var dbServerNames = projectConfig.DatabaseComponents.Select(x => x.ServerName).Distinct();
            foreach (var serverName in dbServerNames)
            {
                var serverElement = new Model.Mssql.Db.ServerElement(Parse.Mssql.Db.UrnBuilder.GetServerUrn(serverName), serverName);
                serverElement.Parent = solutionModel;
                solutionModel.AddChild(serverElement);
            }

            var elementIdMap = sh.SaveModelPart(solutionModel, premappedModel, true);

            solutionModel = sh.LoadElementModelToChildrenOfType(
                    string.Empty,
                    typeof(Model.Mssql.Db.ServerElement)) as SolutionModelElement;

            var sqlServerPremappedModel = sh.CreatePremappedModel(solutionModel);

            foreach (var dbComponent in projectConfig.DatabaseComponents)
            {
                ConfigManager.Log.Important(string.Format("Parsing MSSQL DB structures {0} from {1}", dbComponent.DbName, dbComponent.ServerName));

                var existentServerElement = solutionModel.DbServers.First(x => x.Caption == dbComponent.ServerName);

                var dbElement = new Model.Mssql.Db.DatabaseElement(Parse.Mssql.Db.UrnBuilder.GetDbUrn(existentServerElement.RefPath, dbComponent.DbName), dbComponent.DbName, existentServerElement);
                existentServerElement.AddChild(dbElement);

                // need to save schemas before the parts due to race conditions
                var schemas = StageManager.GetExtractItems(request.ExtractId, dbComponent.MssqlDbProjectComponentId, DAL.Objects.Extract.ExtractTypeEnum.SqlSchema);
                foreach (DAL.Objects.Extract.SqlSchema schema in schemas)
                {
                    var schemaElement = new Model.Mssql.Db.SchemaElement(Parse.Mssql.Db.DbModelParserBase.RefPathFor(schema.Urn), schema.Name, dbElement);
                    dbElement.AddChild(schemaElement);
                }

                var procedures = StageManager.GetExtractItems(request.ExtractId, dbComponent.MssqlDbProjectComponentId,
                                DAL.Objects.Extract.ExtractTypeEnum.SqlProcedure);
                var scalarUdfs = StageManager.GetExtractItems(request.ExtractId, dbComponent.MssqlDbProjectComponentId,
                                DAL.Objects.Extract.ExtractTypeEnum.SqlScalarUdf);
                var tables = StageManager.GetExtractItems(request.ExtractId, dbComponent.MssqlDbProjectComponentId,
                                DAL.Objects.Extract.ExtractTypeEnum.SqlTable);
                var tableTypes = StageManager.GetExtractItems(request.ExtractId, dbComponent.MssqlDbProjectComponentId,
                                DAL.Objects.Extract.ExtractTypeEnum.SqlTableType);
                var tableUdfs = StageManager.GetExtractItems(request.ExtractId, dbComponent.MssqlDbProjectComponentId,
                                DAL.Objects.Extract.ExtractTypeEnum.SqlTableUdf);
                var views = StageManager.GetExtractItems(request.ExtractId, dbComponent.MssqlDbProjectComponentId,
                                DAL.Objects.Extract.ExtractTypeEnum.SqlView);

                var extractItems = procedures.Union(scalarUdfs).Union(tables).Union(tableTypes)
                    .Union(tableUdfs).Union(views);

                foreach (DAL.Objects.Extract.SmoObject extractItem in extractItems)
                {
                    var schemaElement = dbElement.SchemaByName(extractItem.SchemaName);
                    Model.Mssql.Db.DbModelElement objectElement = sqlShallowParser.ParseSqlObjectShallow(extractItem, schemaElement);
                }

                //Parse.Mssql.Db.ReferrableIndexBuilder rib = new Parse.Mssql.Db.ReferrableIndexBuilder();
                //var ix = rib.BuildIndex(new List<Model.Mssql.Db.ServerElement>() { existentServerElement });
                //ix.SetContextServer(dbComponent.ServerName);
                //ix.PremappedIds = rib.ElementIdMap;

                //foreach (DAL.Objects.Extract.SmoObject extractObject in extractItems)
                //{
                //    ConfigManager.Log.Important(string.Format("Parsing SQL scripts from {0}", extractObject.Urn));

                //    var objectRefPath = Parse.Mssql.Db.DbModelParserBase.RefPathFor(extractObject.Urn).Path;

                //    var schemaModel = dbElement.SchemaByName(extractObject.SchemaName);

                //    var modelElement = schemaModel.Children.First(x => x.RefPath.Path == objectRefPath);

                //    Parse.Mssql.Db.LocalDeepModelParser ldmp = new Parse.Mssql.Db.LocalDeepModelParser(projectConfig, dbComponent.ServerName);
                //    ldmp.ExtractModel(extractObject, dbComponent.ServerName, ix, modelElement);
                //}

                //Parse.Mssql.Db.AvailableDatabaseModelIndex tadbix = new Parse.Mssql.Db.AvailableDatabaseModelIndex(projectConfig, GraphManager);
                
                //var premapped = tadbix.GetAllPremappedIds();
                //foreach (var kv in premapped)
                //{
                //    premappedModel.Add(kv.Key, kv.Value);
                //}

                sh.SaveModelPart(dbElement, sqlServerPremappedModel);
            }

            var schemasModel = sh.LoadElementModelToChildrenOfType(
                    string.Empty,
                    typeof(Model.Mssql.Db.SchemaElement)) as SolutionModelElement;


            AvailableDatabaseModelIndex adbIndex = new AvailableDatabaseModelIndex(projectConfig, GraphManager);
            
            foreach (var dbComponent in projectConfig.DatabaseComponents)
            {
                var referrableIndex = adbIndex.GetDatabaseIndex(dbComponent.ServerName, dbComponent.DbName);

                ConfigManager.Log.Important(string.Format("Parsing MSSQL DB structures {0} from {1}", dbComponent.DbName, dbComponent.ServerName));

                var existentServerElement = schemasModel.DbServers.First(x => x.Caption == dbComponent.ServerName);

                var dbRefPath = Parse.Mssql.Db.UrnBuilder.GetDbUrn(existentServerElement.RefPath, dbComponent.DbName);
                var dbElement = (CD.DLS.Model.Mssql.Db.DatabaseElement)sh.LoadElementModel(dbRefPath.Path);

                //var dbElement = _componentsToDbElements[dbComponent.MssqlDbProjectComponentId]; //new Model.Mssql.Db.DatabaseElement(Parse.Mssql.Db.UrnBuilder.GetDbUrn(existentServerElement.RefPath, dbComponent.DbName), dbComponent.DbName, existentServerElement);
                //existentServerElement.AddChild(dbElement);

                //// need to save schemas before the parts due to race conditions
                //var schemas = StageManager.GetExtractItems(request.ExtractId, dbComponent.MssqlDbProjectComponentId, DAL.Objects.Extract.ExtractTypeEnum.SqlSchema);
                //foreach (DAL.Objects.Extract.SqlSchema schema in schemas)
                //{
                //    var schemaElement = new Model.Mssql.Db.SchemaElement(Parse.Mssql.Db.DbModelParserBase.RefPathFor(schema.Urn), schema.Name, dbElement);
                //    dbElement.AddChild(schemaElement);
                //}

                var extractItems = ListDbExtractObjects(request, dbComponent);

                //Parse.Mssql.Db.ReferrableIndexBuilder rib = new Parse.Mssql.Db.ReferrableIndexBuilder();
                //var ix = rib.BuildIndex(new List<Model.Mssql.Db.ServerElement>() { existentServerElement });
                //ix.SetContextServer(dbComponent.ServerName);
                //ix.PremappedIds = rib.ElementIdMap;

                foreach (DAL.Objects.Extract.SmoObject extractObject in extractItems)
                {
                    ConfigManager.Log.Important(string.Format("Parsing SQL scripts from {0}", extractObject.Urn));

                    var objectRefPath = Parse.Mssql.Db.DbModelParserBase.RefPathFor(extractObject.Urn).Path;

                    var schemaModel = dbElement.SchemaByName(extractObject.SchemaName);

                    var modelElement = schemaModel.Children.First(x => x.RefPath.Path == objectRefPath);

                    Parse.Mssql.Db.LocalDeepModelParser ldmp = new Parse.Mssql.Db.LocalDeepModelParser(projectConfig, dbComponent.ServerName);
                    ldmp.ExtractModel(extractObject, dbComponent.ServerName, referrableIndex, modelElement);
                }

                //Parse.Mssql.Db.AvailableDatabaseModelIndex tadbix = new Parse.Mssql.Db.AvailableDatabaseModelIndex(projectConfig, GraphManager);

                //var premapped = tadbix.GetAllPremappedIds();
                //foreach (var kv in premapped)
                //{
                //    premappedModel.Add(kv.Key, kv.Value);
                //}

                var premapped = adbIndex.GetAllPremappedIds();
                //foreach (var server in schemasModel.DbServers)
                //{ 
                //    if(!premapped.ContainsKey(server))
                //}

                dbElement.Parent = null;
                sh.SaveModelPart(dbElement, premapped, true);
            }

            //sh.SaveModelPart(solutionModel, sqlServerPremappedModel, true);

            GraphManager.SetRefPathIntervals(projectConfig.ProjectConfigId);





            //foreach (var dbComponent in projectConfig.DatabaseComponents)
            //{

            //    AvailableDatabaseModelIndex adbIndex = new AvailableDatabaseModelIndex(projectConfig, GraphManager);
            //    var referrableIndex = adbIndex.GetDatabaseIndex(dbComponent.ServerName, dbComponent.DbName);

            //    //adbIndex.GetAllPremappedIds();
            //    //var extractObject = (SmoObject)StageManager.GetExtractItem(request.ExtractItemId);

            //    //sh = new SerializationHelper(projectConfig, GraphManager);

            //    //var existentServerElement = solutionModel.DbServers.First(x => x.Caption == dbComponent.ServerName);


            //    //Model.Mssql.Db.ServerElement serverModel = (Model.Mssql.Db.ServerElement)sh.LoadElementModel(existentServerElement.RefPath.Path); // LoadElementModelToChildrenOfType(request.DatabaseRefPath, typeof(SchemaElement));

            //    //Parse.Mssql.Db.ReferrableIndexBuilder rib = new Parse.Mssql.Db.ReferrableIndexBuilder();

            //    //var ix = rib.BuildIndex(new List<Model.Mssql.Db.ServerElement>() { serverModel });
            //    //ix.SetContextServer(dbComponent.ServerName);
            //    //ix.PremappedIds = rib.ElementIdMap;


            //    //var schemaModel = dbModel.SchemaByName(extractObject.SchemaName);

            //    //var modelElement = sh.LoadElementModel(objectRefPath);

            //    var premappedElements = sh.CreatePremappedModel(serverModel);
            //    foreach (var indexObject in referrableIndex.PremappedIds)
            //    {
            //        if (!premappedElements.ContainsKey(indexObject.Key))
            //        {
            //            premappedElements.Add(indexObject.Key, indexObject.Value);
            //        }
            //    }

            //    var extractItems = ListDbExtractObjects(request, dbComponent);

            //    var dbModel = serverModel.Databases.First(x => x.DbName == dbComponent.DbName);


            //    foreach (DAL.Objects.Extract.SmoObject extractObject in extractItems)
            //    {
            //        ConfigManager.Log.Important(string.Format("Parsing SQL scripts from {0}", extractObject.Urn));

            //        var objectRefPath = Parse.Mssql.Db.DbModelParserBase.RefPathFor(extractObject.Urn).Path;


            //        var schemaModel = dbModel.SchemaByName(extractObject.SchemaName);

            //        var modelElement = schemaModel.Children.First(x => x.RefPath.Path == objectRefPath);

            //        Parse.Mssql.Db.LocalDeepModelParser ldmp = new Parse.Mssql.Db.LocalDeepModelParser(projectConfig, dbComponent.ServerName);
            //        ldmp.ExtractModel(extractObject, dbComponent.ServerName, ix, modelElement);
            //    }


            //    sh.SaveModelPart(dbModel, premappedElements, true);



            //}



            #endregion

            #region SSAS_DATABASES

            var ssasUrnBuilder = new Parse.Mssql.Ssas.UrnBuilder();
            List<Model.Mssql.Ssas.ServerElement> res = new List<Model.Mssql.Ssas.ServerElement>();
            
            var solutionElement = (SolutionModelElement)sh.LoadElementModelToChildrenOfType("", typeof(SolutionModelElement));
            var premappedIds = sh.CreatePremappedModel(solutionElement);

            Parse.Mssql.Db.AvailableDatabaseModelIndex adbix = new Parse.Mssql.Db.AvailableDatabaseModelIndex(projectConfig, GraphManager);
            Parse.Mssql.Ssas.SsasModelExtractor extractor = new Parse.Mssql.Ssas.SsasModelExtractor(projectConfig, request.ExtractId, StageManager, GraphManager, adbix);
            BIDoc.Core.Parse.Mssql.Tabular.TabularParser tparser = new BIDoc.Core.Parse.Mssql.Tabular.TabularParser(adbix, projectConfig, request.ExtractId, StageManager);


            
            foreach (var ssasComponent in projectConfig.SsasComponents.OrderBy(x => x.ServerName))
            {
                Model.Mssql.Ssas.ServerElement ssasServerElement = solutionModel.SsasServers.FirstOrDefault(x => x.Caption == ssasComponent.ServerName);
                Model.Mssql.Ssas.SsasDatabaseElement ssasDbElement = null;


                ConfigManager.Log.Info("SSAS DB type: " + ssasComponent.Type.ToString());

                if (ssasComponent.Type == SsasTypeEnum.Tabular)
                {
                    ConfigManager.Log.Info("Loading tabular DB extract");

                    var dbExtract = (DAL.Objects.Extract.TabularDB)(StageManager
                            .GetExtractItems(request.ExtractId, ssasComponent.SsaslDbProjectComponentId,
                            DAL.Objects.Extract.ExtractTypeEnum.TabularDB)[0]);

                    ConfigManager.Log.Info("Tabular DB extract loaded");
                    
                    //Model.Mssql.Ssas.SsasDatabaseElement dbElement = null;

                    if (res.Count == 0 || !Common.Tools.ConnectionStringTools.AreServersNamesEqual(res.Last().Caption, ssasComponent.ServerName))
                    {
                        var refPath = ssasUrnBuilder.GetServerUrn(ssasComponent.ServerName);
                        ssasServerElement = new Model.Mssql.Ssas.ServerElement(refPath, ssasComponent.ServerName, null, solutionElement);
                        //solutionElement.AddChild(serverElement);
                        solutionModel.AddChild(ssasServerElement);
                        res.Add(ssasServerElement);

                        sh.SaveModelPart(ssasServerElement, premappedModel, true);

                        //parseDatabaseRequests.Add(new ParseSsasDatabaseRequest()
                        //{
                        //    SsasDbComponentId = ssasComponent.SsaslDbProjectComponentId,
                        //    ExtractId = request.ExtractId,
                        //    DbRefPath = refPath.Path
                        //});
                    }

                    ConfigManager.Log.Important(string.Format("Parsing tabular database {0}", ssasComponent.DbName));
                    var dbExtractT = (DAL.Objects.Extract.TabularDB)(StageManager.GetExtractItems(
                    request.ExtractId, ssasComponent.SsaslDbProjectComponentId, DAL.Objects.Extract.ExtractTypeEnum.TabularDB)[0]);
                    ssasDbElement = tparser.Parse(ssasComponent.SsaslDbProjectComponentId, ssasComponent.ServerName, ssasServerElement);
                }
                else
                {
                    ConfigManager.Log.Info("Loading multidimensional DB extract");

                    var dbExtract = (DAL.Objects.Extract.MultidimensionalDatabase)(StageManager
                        .GetExtractItems(request.ExtractId, ssasComponent.SsaslDbProjectComponentId,
                        DAL.Objects.Extract.ExtractTypeEnum.SsasMultidimensionalDatabase)[0]);

                    ConfigManager.Log.Info("Multidimensional DB extract loaded");

                    ssasServerElement = solutionModel.SsasServers.FirstOrDefault(x => x.Caption == ssasComponent.ServerName);

                    if (res.Count == 0 || !Common.Tools.ConnectionStringTools.AreServersNamesEqual(res.Last().Caption, ssasComponent.ServerName))
                    {
                        var refPath = ssasUrnBuilder.GetServerUrn(dbExtract.ServerID);
                        ssasServerElement = new Model.Mssql.Ssas.ServerElement(refPath, dbExtract.ServerName, null, solutionElement);
                        solutionElement.AddChild(ssasServerElement);
                        res.Add(ssasServerElement);

                        sh.SaveModelPart(ssasServerElement, premappedIds, true);

                        //parseDatabaseRequests.Add(new ParseSsasDatabaseRequest()
                        //{
                        //    SsasDbComponentId = ssasComponent.SsaslDbProjectComponentId,
                        //    ExtractId = request.ExtractId,
                        //    DbRefPath = refPath.Path
                        //});
                    }

                    ConfigManager.Log.Important(string.Format("Parsing multidimensional database {0}", ssasComponent.DbName));
                    dbExtract = (DAL.Objects.Extract.MultidimensionalDatabase)(StageManager.GetExtractItems(
                        request.ExtractId, ssasComponent.SsaslDbProjectComponentId, DAL.Objects.Extract.ExtractTypeEnum.SsasMultidimensionalDatabase)[0]);

                    ssasDbElement = extractor.ExtractDatabase(ssasComponent.SsaslDbProjectComponentId, dbExtract, ssasServerElement);
                }

                var solutionModelUpd = sh.LoadElementModelToChildrenOfType(
                    string.Empty,
                    typeof(Model.Mssql.Ssas.ServerElement)) as SolutionModelElement;

                var premappedModelUpd = sh.CreatePremappedModel(solutionModelUpd);

                var dbIdMap = adbix.GetAllPremappedIds();
                foreach (var kv in dbIdMap)
                {
                    premappedModel.Add(kv.Key, kv.Value);
                }

                sh.SaveModelPart(ssasDbElement, premappedModel);
            }

            GraphManager.SetRefPathIntervals(projectConfig.ProjectConfigId, RequestId);

            #endregion


            #region SSIS

            var ssisSolutionElement = (SolutionModelElement)sh.LoadElementModelToChildrenOfType("", typeof(SolutionModelElement));
            var ssisPremappedIds = sh.CreatePremappedModel(solutionElement);

            var groupByServer = projectConfig.SsisComponents.GroupBy(x => x.ServerName);
            foreach (var serverGrp in groupByServer)
            {
                var serverName = serverGrp.Key;
                var rp = new RefPath(string.Format("IntegrationServices[@Name='{0}']", serverName));
                var ssisServerElement = new CD.DLS.Model.Mssql.Ssis.ServerElement(rp, serverName, rp.Path);
                //res.Add(serverElement);

                ssisServerElement.Parent = solutionElement;
                solutionElement.AddChild(ssisServerElement);

                var catalogRp = rp.Child("Catalog");

                var catalogElement = new Model.Mssql.Ssis.CatalogElement(catalogRp, "SSIS Catalog", catalogRp.Path, ssisServerElement);
                ssisServerElement.AddChild(catalogElement);

                var grpByFolder = serverGrp.GroupBy(x => x.FolderName);
                foreach (var folderGrp in grpByFolder)
                {
                    var folderName = folderGrp.Key;
                    //var folder = catalog.Folders[folderName];
                    var folderRp = catalogRp.NamedChild("Folder", folderName);
                    var folderElement = new Model.Mssql.Ssis.FolderElement(folderRp, folderName, folderRp.Path, catalogElement);
                    catalogElement.AddChild(folderElement);
                    var ssisUrnBuilder = new CD.DLS.Parse.Mssql.Ssis.UrnBuilder();

                    foreach (var projectComponent in folderGrp)
                    {
                        //parseProjectRequests.Add(new ParseSsisProjectShallowRequest()
                        //{
                        //    ExtractId = request.ExtractId,
                        //    SsisComponentId = projectComponent.SsisProjectComponentId,
                        //    FolderRefPath = folderElement.RefPath.Path,
                        //    ServerRefPath = serverElement.RefPath.Path
                        //});

                        //        var xmlProvider = new Parse.Mssql.Ssis.SsisXmlProvider(request.ExtractId, projectComponent.SsisProjectComponentId, // request.SsisComponentId,
                        //StageManager, null, false);
                        var xmlProvider = new Parse.Mssql.Ssis.SsisXmlProvider(request.ExtractId, projectComponent.SsisProjectComponentId, // request.SsisComponentId,
                        StageManager, null, true);
                        var dbIndex = new Parse.Mssql.Db.AvailableDatabaseModelIndex(projectConfig, GraphManager);
                        Parse.Mssql.Ssis.ProjectModelParser modelParser = new Parse.Mssql.Ssis.ProjectModelParser(xmlProvider, dbIndex, projectConfig,
                            request.ExtractId, StageManager);

                        var projectModel = modelParser.ParseProjectShallow(projectComponent.SsisProjectComponentId, folderElement);


                        var packages = StageManager.GetExtractItems(request.ExtractId, projectComponent.SsisProjectComponentId, DAL.Objects.Extract.ExtractTypeEnum.SsisPackage);

                        foreach (DAL.Objects.Extract.SsisPackage package in packages)
                        {
                            ConfigManager.Log.Important("Parsing " + package.Urn + " shallow");

                            var refPath = ssisUrnBuilder.GetPackageUrn(projectModel, package.Name);// packageInfo.Urn;
                            Model.Mssql.Ssis.PackageElement packageElement = new Model.Mssql.Ssis.PackageElement(refPath, package.Name, null /* definition*/, projectModel);
                            projectModel.AddChild(packageElement);

                            //serializationHelper.SaveModelPart(packageElement, premappedIds);
                        }

                        var xmlFiles = StageManager.GetExtractItems(request.ExtractId, projectComponent.SsisProjectComponentId, ExtractTypeEnum.SsisPackageFile);

                        var joins = packages.Join(xmlFiles, x => x.Name, y => HttpUtility.UrlDecode(y.Name), (pkg, xml) => new Tuple<SsisPackage, SsisPackageFile>((SsisPackage)pkg, (SsisPackageFile)xml))
                            .Join(projectModel.Packages, x => x.Item1.Name, me => me.Caption, (j, p) => new Tuple<SsisPackage, SsisPackageFile, Model.Mssql.Ssis.PackageElement>(j.Item1, j.Item2, p));
                        if (joins.Count() != packages.Count)
                        {
                            var missingPacakges = packages.Where(p => !joins.Any(x => x.Item1 == p)).ToList();
                            var missingXmls = xmlFiles.Where(xml => !joins.Any(x => x.Item2 == xml)).ToList();

                            throw new Exception();
                        }

                        //Parse.Mssql.Db.AvailableDatabaseModelIndex adbix = new Parse.Mssql.Db.AvailableDatabaseModelIndex(projectConfig, GraphManager);

                        List<ParseSsisPackageItem> items = new List<ParseSsisPackageItem>();

                        foreach (var jn in joins)
                        {

                            var packageExractItemId = jn.Item1.ExtractItemId;
                            var xmlExtractItemId = jn.Item2.ExtractItemId;
                            var packageRefPath = jn.Item3.RefPath.Path;

                            //var projectElementWithCMs =  (ProjectElement)serializationHelper.LoadElementModelToChildrenOfType(request.ProjectRefPath, typeof(ConnectionManagerElement));
                            //var projectCMs = new List<ConnectionManagerElement>();

                            //if (projectElementWithCMs != null)
                            //{
                            //    projectElementWithCMs.ConnectionManagers.ToList();
                            //    foreach (var projectCM in projectCMs)
                            //    {
                            //        var map = serializationHelper.CreatePremappedModel(projectCM);
                            //        foreach (var mapItem in map)
                            //        {
                            //            premappedIds.Add(mapItem.Key, mapItem.Value);
                            //        }
                            //    }
                            //}


                            //var cms = new List<ConnectionManagerElement>();
                            //if (projectElementWithCMs != null)
                            //{
                            //    cms = projectElementWithCMs.ConnectionManagers.ToList();
                            //}

                            //AvailableDatabaseModelIndex adbix = new AvailableDatabaseModelIndex(projectConfig, GraphManager);

                            //var itemIdx = 0;

                            //do
                            //{
                                
                                //projectElement.Parent = folderElement;

                                var packageExtract = (SsisPackage)StageManager.GetExtractItem(packageExractItemId);
                                var xmlExtract = (SsisPackageFile)StageManager.GetExtractItem(xmlExtractItemId);
                                var packageElement = projectModel.Packages.First(x => x.RefPath.Path == packageRefPath);

                                ConfigManager.Log.Important("Parsing " + packageExtract.Urn + " deep");

                                //Parse.Mssql.Ssis.SsisXmlProvider xmlProvider = new Parse.Mssql.Ssis.SsisXmlProvider(request.ExtractId, projectComponent.SsisProjectComponentId, StageManager, xmlExtract);
                                Parse.Mssql.Ssis.ProjectModelParser projectModelParser = new Parse.Mssql.Ssis.ProjectModelParser(xmlProvider, adbix, projectConfig, request.ExtractId, StageManager);

                                projectModelParser.ParsePackage(projectComponent.SsisProjectComponentId, projectModel, packageElement,
                                    packageExtract, projectModel.ConnectionManagers.ToList());

                                //projectElement.Parent = null;


                                
                            //    itemIdx++;

                            //} while (itemIdx < joins.Count());

                            var dbIdMap = adbix.GetAllPremappedIds();
                            foreach (var kv in dbIdMap)
                            {
                                if (!premappedIds.ContainsKey(kv.Key))
                                {
                                    premappedIds.Add(kv.Key, kv.Value);
                                }
                            }

                            //serializationHelper.SaveModelPart(packageElement, premappedIds, true);

                        }

                        //joins = joins.Take(100);

                        //foreach (var jn in joins)
                        //{

                        //    items.Add(new ParseSsisPackageItem()
                        //    {
                        //        PackageExractItemId = jn.Item1.ExtractItemId,
                        //        XmlExtractItemId = jn.Item2.ExtractItemId,
                        //        PackageRefPath = jn.Item3.RefPath.Path
                        //    });

                        //}
                    }
                    }

                sh.SaveModelPart(ssisServerElement, premappedIds);
            }

            #endregion

            #region AGGREGATIONS

            GraphManager.BuildAggregations(projectConfig.ProjectConfigId, RequestId);
            
            #endregion


            msgs = RequestManager.GetActiveBroadcastMessages();
            foreach (var msg in msgs.Where(x => x.Type == BroadcastMessageType.ProjectUpdateStarted))
            {
                RequestManager.SetBroadcastMessageInactive(msg);
            }

            return new DLSApiMessage();
            
        }

        private List<ExtractObject> ListDbExtractObjects(UpdateModelRequest request, MssqlDbProjectComponent dbComponent)
        {
            var procedures = StageManager.GetExtractItems(request.ExtractId, dbComponent.MssqlDbProjectComponentId,
                                DAL.Objects.Extract.ExtractTypeEnum.SqlProcedure);
            var scalarUdfs = StageManager.GetExtractItems(request.ExtractId, dbComponent.MssqlDbProjectComponentId,
                            DAL.Objects.Extract.ExtractTypeEnum.SqlScalarUdf);
            var tables = StageManager.GetExtractItems(request.ExtractId, dbComponent.MssqlDbProjectComponentId,
                            DAL.Objects.Extract.ExtractTypeEnum.SqlTable);
            var tableTypes = StageManager.GetExtractItems(request.ExtractId, dbComponent.MssqlDbProjectComponentId,
                            DAL.Objects.Extract.ExtractTypeEnum.SqlTableType);
            var tableUdfs = StageManager.GetExtractItems(request.ExtractId, dbComponent.MssqlDbProjectComponentId,
                            DAL.Objects.Extract.ExtractTypeEnum.SqlTableUdf);
            var views = StageManager.GetExtractItems(request.ExtractId, dbComponent.MssqlDbProjectComponentId,
                            DAL.Objects.Extract.ExtractTypeEnum.SqlView);

            var extractItems = procedures.Union(scalarUdfs).Union(tables).Union(tableTypes)
                .Union(tableUdfs).Union(views);

            return extractItems.ToList();
        }

    }
}
