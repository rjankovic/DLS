using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Mssql.Db;
using CD.DLS.Model.Serialization;
using CD.DLS.Parse.Mssql.Db;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class ParseSqlDatabaseObjectDeepRequestProcessor : RequestProcessorBase, IRequestProcessor<ParseSqlDatabaseObjectDeepRequest, DLSApiMessage>
    {
        public DLSApiMessage Process(ParseSqlDatabaseObjectDeepRequest request, ProjectConfig projectConfig)
        {
            AvailableDatabaseModelIndex adbIndex = new AvailableDatabaseModelIndex(projectConfig, GraphManager);
            var referrableIndex = adbIndex.GetDatabaseIndex(request.ServerName, request.DbName);
            
            var extractObject = (SmoObject)StageManager.GetExtractItem(request.ExtractItemId);

            SerializationHelper sh = new SerializationHelper(projectConfig, GraphManager);
            var objectRefPath = DbModelParserBase.RefPathFor(extractObject.Urn).Path;

            var dbModel = (DatabaseElement)sh.LoadElementModelToChildrenOfType(request.DatabaseRefPath, typeof(SchemaElement));
            var schemaModel = dbModel.SchemaByName(extractObject.SchemaName);

            var modelElement = sh.LoadElementModel(objectRefPath);
       
            var premappedElements = sh.CreatePremappedModel(modelElement);
            foreach (var indexObject in referrableIndex.PremappedIds)
            {
                if (!premappedElements.ContainsKey(indexObject.Key))
                {
                    premappedElements.Add(indexObject.Key, indexObject.Value);
                }
            }

            modelElement.Parent = schemaModel;
            schemaModel.AddChild(modelElement);
       
            LocalDeepModelParser ldmp = new LocalDeepModelParser(projectConfig, request.ServerName);
            ldmp.ExtractModel(extractObject, request.ServerName, referrableIndex, modelElement);

            modelElement.Parent = null;
            sh.SaveModelPart(modelElement, premappedElements, true);
            
            return new DLSApiMessage();
        }
        
    }
}
