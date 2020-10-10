using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.Model.DependencyGraph.KnowledgeBase;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Parse.Mssql;
using CD.DLS.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class BuildAggregationsRequestProcessor : RequestProcessorBase, IRequestProcessor<BuildAggregationsRequest, DLSApiProgressResponse>
    {
        public DLSApiProgressResponse Process(BuildAggregationsRequest request, ProjectConfig projectConfig)
        {
            //GraphManager.BuildAggregations(projectConfig.ProjectConfigId, RequestId);
            RequestManager.CreateProcedureExecution("[BIDoc].[sp_BuildAggregations]", projectConfig.ProjectConfigId, RequestId);

            return new DLSApiProgressResponse()
            {
                ContinuationsWaitForDb = true,
                ContinueWith = new FindAssociationRulesRequest() //SetModelAvailableRequest()
            };
            
        }
    }
}
