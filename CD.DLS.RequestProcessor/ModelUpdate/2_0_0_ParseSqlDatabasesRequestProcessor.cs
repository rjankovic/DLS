using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.Model;
using CD.DLS.Model.DependencyGraph.KnowledgeBase;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Serialization;
using CD.DLS.Parse.Mssql;
using CD.DLS.Parse.Mssql.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class ParseSqlDatabasesRequestProcessor : RequestProcessorBase, IRequestProcessor<ParseSqlDatabasesRequest, DLSApiProgressResponse>
    {
        public DLSApiProgressResponse Process(ParseSqlDatabasesRequest request, ProjectConfig projectConfig)
        {
            try
            {
                GraphManager.SetRefPathIntervals(projectConfig.ProjectConfigId);

                List<DLSApiMessage> parseDatabaseShallowRequests = projectConfig.DatabaseComponents
                    .Select(x => (DLSApiMessage)new ParseSqlDatabaseShallowRequest()
                    { DbComponentId = x.MssqlDbProjectComponentId, ExtractId = request.ExtractId }).ToList();


                SerializationHelper serializationHelper = new SerializationHelper(projectConfig, GraphManager);

                var solutionModel = serializationHelper.LoadElementModelToChildrenOfType(
                    string.Empty,
                    typeof(Model.Mssql.Db.ServerElement)) as SolutionModelElement;
                //var solutionModel = new SolutionModelElement(new RefPath(""), "");

                var premappedModel = serializationHelper.CreatePremappedModel(solutionModel);

                var dbServerNames = projectConfig.DatabaseComponents.Select(x => x.ServerName).Distinct();
                foreach (var serverName in dbServerNames)
                {
                    var serverElement = new Model.Mssql.Db.ServerElement(UrnBuilder.GetServerUrn(serverName), serverName);
                    serverElement.Parent = solutionModel;
                    solutionModel.AddChild(serverElement);
                }


                var elementIdMap = serializationHelper.SaveModelPart(solutionModel, premappedModel, true);


                return new DLSApiProgressResponse()
                {
                    ParallelRequests = parseDatabaseShallowRequests,
                    ContinueWith = new ParseSsasDatabasesRequest()
                    {
                        ExtractId = request.ExtractId
                    }
                };
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw;
            }
        }
    }
}
