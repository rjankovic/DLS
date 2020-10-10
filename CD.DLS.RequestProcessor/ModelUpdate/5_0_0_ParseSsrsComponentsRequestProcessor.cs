using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Serialization;
using CD.DLS.Parse.Mssql.Ssas;
using System.Collections.Generic;
using System.Linq;
using CD.DLS.Model.Mssql.Ssrs;
using CD.DLS.Parse.Mssql.Ssrs;
using CD.DLS.Parse.Mssql.Db;
using System;
using CD.DLS.DAL.Configuration;
using CD.DLS.Model;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class ParseParseSsrsComponentsRequest : RequestProcessorBase, IRequestProcessor<ParseSsrsComponentsRequest, DLSApiProgressResponse>
    {
        public DLSApiProgressResponse Process(ParseSsrsComponentsRequest request, ProjectConfig projectConfig)
        {
            try
            {
                List<DLSApiMessage> parseComponentRequests = new List<DLSApiMessage>();

                SerializationHelper serializationHelper = new SerializationHelper(projectConfig, GraphManager);
                var solutionElement = (SolutionModelElement)serializationHelper.LoadElementModelToChildrenOfType("", typeof(SolutionModelElement));
                var premappedIds = serializationHelper.CreatePremappedModel(solutionElement);

                var urnBuilder = new Parse.Mssql.Ssrs.UrnBuilder();

                AvailableDatabaseModelIndex adbix = new AvailableDatabaseModelIndex(projectConfig, GraphManager);
                SsasServerIndex ssasIndex = new SsasServerIndex(projectConfig, GraphManager);
                SsrsModelExtractor extractor = new SsrsModelExtractor(adbix, ssasIndex, new MdxScriptModelExtractor(), projectConfig, request.ExtractId, StageManager);

                var serverNames = projectConfig.SsrsComponents.Select(x => x.ServerName).Distinct();
                foreach (var serverName in serverNames)
                {
                    var serverUrn = urnBuilder.GetServerUrn(serverName);
                    var serverElement = new ServerElement(serverUrn, serverName, null, solutionElement);
                    solutionElement.AddChild(serverElement);

                    foreach (var ssrsComponent in projectConfig.SsrsComponents.Where(x => x.ServerName == serverName))
                    {
                        extractor.ExtractComponentModelShallow(serverElement, ssrsComponent);
                        parseComponentRequests.Add(new ParseSsrsComponentRequest()
                        {
                            SsrsComponentId = ssrsComponent.SsrsProjectComponentId,
                            ExtractId = request.ExtractId,
                            ServerRefPath = serverUrn.Path
                        });
                    }

                    foreach (var dbItem in adbix.GetAllPremappedIds())
                    {
                        if (!premappedIds.ContainsKey(dbItem.Key))
                        {
                            premappedIds.Add(dbItem.Key, dbItem.Value);
                        }
                    }
                    serializationHelper.SaveModelPart(serverElement, premappedIds);
                }

                GraphManager.SetRefPathIntervals(projectConfig.ProjectConfigId, RequestId);

                return new DLSApiProgressResponse()
                {
                    ContinuationsWaitForDb = true,
                    ParallelRequests = parseComponentRequests,
                    ContinueWith = new ParsePbiComponentsRequest() // BuildAggregationsRequest()
                    {
                        ExtractId = request.ExtractId
                    }
                };
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw ex;
            }
        }
    }
}
