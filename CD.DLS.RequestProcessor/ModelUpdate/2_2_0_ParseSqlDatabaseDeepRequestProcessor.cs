//using CD.DLS.API;
//using CD.DLS.API.ModelUpdate;
//using CD.DLS.Common.Structures;
//using CD.DLS.DAL.Configuration;
//using CD.DLS.DAL.Objects.Extract;
//using CD.DLS.Model.Mssql.Db;
//using CD.DLS.Model.Serialization;
//using CD.DLS.Parse.Mssql.Db;
//using System.Linq;

//namespace CD.DLS.RequestProcessor.ModelUpdate
//{
//    public class ParseSqlDatabaseDeepRequestProcessor : RequestProcessorBase, IRequestProcessor<ParseSqlDatabaseDeepRequest, DLSApiMessage>
//    {
//        public DLSApiMessage Process(ParseSqlDatabaseDeepRequest request, ProjectConfig projectConfig)
//        {

//            var procedures = StageManager.GetExtractItems(request.ExtractId, request.DbComponentId,
//                            DAL.Objects.Extract.ExtractTypeEnum.SqlProcedure);
//            var scalarUdfs = StageManager.GetExtractItems(request.ExtractId, request.DbComponentId,
//                            DAL.Objects.Extract.ExtractTypeEnum.SqlScalarUdf);
//            var tables = StageManager.GetExtractItems(request.ExtractId, request.DbComponentId,
//                            DAL.Objects.Extract.ExtractTypeEnum.SqlTable);
//            var tableTypes = StageManager.GetExtractItems(request.ExtractId, request.DbComponentId,
//                            DAL.Objects.Extract.ExtractTypeEnum.SqlTableType);
//            var tableUdfs = StageManager.GetExtractItems(request.ExtractId, request.DbComponentId,
//                            DAL.Objects.Extract.ExtractTypeEnum.SqlTableUdf);
//            var views = StageManager.GetExtractItems(request.ExtractId, request.DbComponentId,
//                            DAL.Objects.Extract.ExtractTypeEnum.SqlView);

//            var extractItems = procedures.Union(scalarUdfs).Union(tables).Union(tableTypes)
//                .Union(tableUdfs).Union(views);

//            ConfigManager.Log.Important(string.Format("SQL Parser - deserializing {0}", request.DbRefPath));


//            SerializationHelper sh = new SerializationHelper(projectConfig, GraphManager);
//            //var dbModel = (DatabaseElement)sh.LoadElementModelToChildrenOfType(request.DbRefPath, typeof(SchemaElement));
//            var dbModel = (DatabaseElement)sh.LoadElementModel(request.DbRefPath);

//            ConfigManager.Log.Important(string.Format("SQL Parser - creating DB index"));

//            AvailableDatabaseModelIndex adbIndex = new AvailableDatabaseModelIndex(projectConfig, GraphManager);
//            var referrableIndex = adbIndex.GetDatabaseIndex(request.ServerName, request.DbName);

//            //var extractObject = (SmoObject)StageManager.GetExtractItem(request.ExtractItemId);

//            ConfigManager.Log.Important(string.Format("SQL Parser - creating premapped model"));

//            var premappedElements = sh.CreatePremappedModel(dbModel);
//            foreach (var indexObject in referrableIndex.PremappedIds)
//            {
//                if (!premappedElements.ContainsKey(indexObject.Key))
//                {
//                    premappedElements.Add(indexObject.Key, indexObject.Value);
//                }
//            }

//            ConfigManager.Log.Important(string.Format("SQL Parser - parsing objects"));
            
//            foreach (SmoObject extractObject in extractItems)
//            {
//                ConfigManager.Log.Important(string.Format("Parsing SQL scripts from {0}", extractObject.Urn));


//                var objectRefPath = DbModelParserBase.RefPathFor(extractObject.Urn).Path;

//                var schemaModel = dbModel.SchemaByName(extractObject.SchemaName);

//                var modelElement = schemaModel.Children.First(x => x.RefPath.Path == objectRefPath);
//                //var modelElement = sh.LoadElementModel(objectRefPath);

                

//                //modelElement.Parent = schemaModel;
//                //schemaModel.AddChild(modelElement);

//                LocalDeepModelParser ldmp = new LocalDeepModelParser(projectConfig, request.ServerName);
//                ldmp.ExtractModel(extractObject, request.ServerName, referrableIndex, modelElement);

//                //modelElement.Parent = null;
                
//            }

//            sh.SaveModelPart(dbModel, premappedElements, true);
//            //var objectRequests = extractItems.Select(x =>
//            //new ParseSqlDatabaseObjectDeepRequest()
//            //{
//            //    ExtractId = request.ExtractId,
//            //    ExtractItemId = x.ExtractItemId,
//            //    DatabaseRefPath = request.DbRefPath,
//            //    DbName = request.DbName,
//            //    ServerName = request.ServerName
//            //}).ToList();

//            return new DLSApiMessage();

//            //return new DLSApiProgressResponse()
//            //{
//            //    ParallelRequests = objectRequests.Select(x => (DLSApiMessage)x).ToList()
//            //};

//        }
//    }
//}
