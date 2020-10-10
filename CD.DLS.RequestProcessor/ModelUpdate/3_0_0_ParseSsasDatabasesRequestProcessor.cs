using CD.BIDoc.Core.Parse.Mssql.Tabular;
using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Serialization;
using CD.DLS.Parse.Mssql.Ssas;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class ParseSsasDatabasesRequestProcessor : RequestProcessorBase, IRequestProcessor<ParseSsasDatabasesRequest, DLSApiProgressResponse>
    {
        public DLSApiProgressResponse Process(ParseSsasDatabasesRequest request, ProjectConfig projectConfig)
        {
            try
            {
                List<DLSApiMessage> parseDatabaseRequests = new List<DLSApiMessage>();

                SerializationHelper serializationHelper = new SerializationHelper(projectConfig, GraphManager);
                var solutionElement = (SolutionModelElement)serializationHelper.LoadElementModelToChildrenOfType("", typeof(SolutionModelElement));
                var premappedIds = serializationHelper.CreatePremappedModel(solutionElement);


                var urnBuilder = new UrnBuilder();
                List<Model.Mssql.Ssas.ServerElement> res = new List<Model.Mssql.Ssas.ServerElement>();
                foreach (var ssasComponent in projectConfig.SsasComponents.OrderBy(x => x.ServerName))
                {
                    ConfigManager.Log.Info("SSAS DB type: " + ssasComponent.Type.ToString());

                    if (ssasComponent.Type == SsasTypeEnum.Tabular)
                    {
                        //tabular
                        //  Parse.Mssql.Db.AvailableDatabaseModelIndex idx=null;
                        //  TabularParser tparser = new TabularParser(idx, projectConfig, request.ExtractId, StageManager);
                        //  tparser.Parse(ssasComponent.SsaslDbProjectComponentId, ssasComponent.ServerName);

                        ConfigManager.Log.Info("Loading tabular DB extract");

                        var dbExtract = (TabularDB)(StageManager
                                .GetExtractItems(request.ExtractId, ssasComponent.SsaslDbProjectComponentId,
                                ExtractTypeEnum.TabularDB)[0]);

                        ConfigManager.Log.Info("Tabular DB extract loaded");

                        if (res.Count == 0 || !Common.Tools.ConnectionStringTools.AreServersNamesEqual(res.Last().Caption, ssasComponent.ServerName))
                        {
                            var refPath = urnBuilder.GetServerUrn(ssasComponent.ServerName);
                            var serverElement = new Model.Mssql.Ssas.ServerElement(refPath, ssasComponent.ServerName, null, solutionElement);
                            solutionElement.AddChild(serverElement);
                            res.Add(serverElement);
                            serializationHelper.SaveModelPart(serverElement, premappedIds, true);

                            parseDatabaseRequests.Add(new ParseSsasDatabaseRequest()
                            {
                                SsasDbComponentId = ssasComponent.SsaslDbProjectComponentId,
                                ExtractId = request.ExtractId,
                                DbRefPath = refPath.Path
                            });

                        }
                    }
                    else
                    {
                        ConfigManager.Log.Info("Loading multidimensional DB extract");

                        var dbExtract = (MultidimensionalDatabase)(StageManager
                            .GetExtractItems(request.ExtractId, ssasComponent.SsaslDbProjectComponentId,
                            ExtractTypeEnum.SsasMultidimensionalDatabase)[0]);

                        ConfigManager.Log.Info("Multidimensional DB extract loaded");

                        if (res.Count == 0 || !Common.Tools.ConnectionStringTools.AreServersNamesEqual(res.Last().Caption, ssasComponent.ServerName))
                        {
                            var refPath = urnBuilder.GetServerUrn(dbExtract.ServerID);
                            var serverElement = new Model.Mssql.Ssas.ServerElement(refPath, dbExtract.ServerName, null, solutionElement);
                            solutionElement.AddChild(serverElement);
                            res.Add(serverElement);
                            serializationHelper.SaveModelPart(serverElement, premappedIds, true);

                            parseDatabaseRequests.Add(new ParseSsasDatabaseRequest()
                            {
                                SsasDbComponentId = ssasComponent.SsaslDbProjectComponentId,
                                ExtractId = request.ExtractId,
                                DbRefPath = refPath.Path
                            });

                        }
                    }

                }

                GraphManager.SetRefPathIntervals(projectConfig.ProjectConfigId, RequestId);

                return new DLSApiProgressResponse()
                {
                    ContinuationsWaitForDb = true,
                    ParallelRequests = parseDatabaseRequests,
                    //ContinueWith = new ParseSsrsComponentsRequest()
                    //{
                    //    ExtractId = request.ExtractId
                    //}
                    ContinueWith = new ParseSsisProjectsRequest()
                    {
                        ExtractId = request.ExtractId
                    }
                };
            }
            catch (Exception ex1)
            {
                LogException(ex1);
                throw ex1;
            }
            
        }
    }
}
