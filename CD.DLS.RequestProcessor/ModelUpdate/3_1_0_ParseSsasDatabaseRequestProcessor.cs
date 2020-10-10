using CD.BIDoc.Core.Parse.Mssql.Tabular;
using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model;
using CD.DLS.Model.Mssql;
using CD.DLS.Parse.Mssql.Ssas;
using System;
using System.Linq;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class ParseSsasDatabaseRequestProcessor : RequestProcessorBase, IRequestProcessor<ParseSsasDatabaseRequest, DLSApiMessage>
    {
        public DLSApiMessage Process(ParseSsasDatabaseRequest request, ProjectConfig projectConfig)
        {

            try
            {
                Parse.Mssql.Db.AvailableDatabaseModelIndex adbix = new Parse.Mssql.Db.AvailableDatabaseModelIndex(projectConfig, GraphManager);
                SsasModelExtractor extractor = new SsasModelExtractor(projectConfig, request.ExtractId, StageManager, GraphManager, adbix);
                TabularParser tparser = new TabularParser(adbix, projectConfig, request.ExtractId, StageManager);
                Model.Serialization.SerializationHelper sh = new Model.Serialization.SerializationHelper(projectConfig, GraphManager);
                var ssasComponent = projectConfig.SsasComponents.First(x => x.SsaslDbProjectComponentId == request.SsasDbComponentId);



                var solutionModel = sh.LoadElementModelToChildrenOfType(
                    string.Empty,
                    typeof(Model.Mssql.Ssas.ServerElement)) as SolutionModelElement;

                var premappedModel = sh.CreatePremappedModel(solutionModel);

                Model.Mssql.Ssas.ServerElement serverElement = solutionModel.SsasServers.First(x => x.Caption == ssasComponent.ServerName);
                Model.Mssql.Ssas.SsasDatabaseElement dbElement = null;
                if (ssasComponent.Type == SsasTypeEnum.Tabular)
                {
                    //tabular
                    ConfigManager.Log.Important(string.Format("Parsing tabular database {0}", ssasComponent.DbName));
                    var dbExtractT = (TabularDB)(StageManager.GetExtractItems(
                    request.ExtractId, ssasComponent.SsaslDbProjectComponentId, ExtractTypeEnum.TabularDB)[0]);
                    dbElement = tparser.Parse(ssasComponent.SsaslDbProjectComponentId, ssasComponent.ServerName, serverElement);


                }
                else
                {
                    //OLAP
                    ConfigManager.Log.Important(string.Format("Parsing multidimensional database {0}", ssasComponent.DbName));
                    var dbExtract = (MultidimensionalDatabase)(StageManager.GetExtractItems(
                        request.ExtractId, ssasComponent.SsaslDbProjectComponentId, ExtractTypeEnum.SsasMultidimensionalDatabase)[0]);

                    dbElement = extractor.ExtractDatabase(request.SsasDbComponentId, dbExtract, serverElement);
                    
                }

                var dbIdMap = adbix.GetAllPremappedIds();
                foreach (var kv in dbIdMap)
                {
                    premappedModel.Add(kv.Key, kv.Value);
                }

                sh.SaveModelPart(dbElement, premappedModel);
            }
            catch (Exception ex)
            {
                ConfigManager.Log.Error(ex.Message);
                ConfigManager.Log.Error(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    ConfigManager.Log.Error(ex.InnerException.Message);
                }

                throw ex;
            }

            return new DLSApiMessage();
        }
    }
}
