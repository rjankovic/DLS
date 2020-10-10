using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.Model;
using CD.DLS.Model.Mssql.Pbi;
using CD.DLS.Model.Serialization;
using CD.DLS.Parse.Mssql.Pbi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    class ParsePbiComponentsRequestProcessor : RequestProcessorBase, IRequestProcessor<ParsePbiComponentsRequest, DLSApiProgressResponse>
    {
        public DLSApiProgressResponse Process(ParsePbiComponentsRequest request, ProjectConfig projectConfig)
        {
            var urnBuilder = new Parse.Mssql.Pbi.UrnBuilder();

            List<DLSApiMessage> parseComponentRequests = new List<DLSApiMessage>();

            SerializationHelper serializationHelper = new SerializationHelper(projectConfig, GraphManager);
          
            var solutionElement = (SolutionModelElement)serializationHelper.LoadElementModelToChildrenOfType("", typeof(SolutionModelElement));
            var premappedIds = serializationHelper.CreatePremappedModel(solutionElement);

            var tenantNames = projectConfig.PowerBiComponents.Select(x => x.Tenant.ToString()).Distinct();

            foreach (var tenantName in tenantNames)
            {
                var tenantUrn = urnBuilder.GetTenantUrn(tenantName);
                var tenantElement = new TenantElement(tenantUrn, tenantName, null, solutionElement);
                solutionElement.AddChild(tenantElement);

                foreach (var pbiComponent in projectConfig.PowerBiComponents.Where(X => X.Tenant == tenantName))
                {
                    parseComponentRequests.Add(new ParsePbiComponentRequest()
                    {
                        ExtractId = request.ExtractId,
                        PbiComponentId = pbiComponent.PowerBiProjectComponentId,
                        TenantRefPath = tenantUrn.Path
                    });
                }

                serializationHelper.SaveModelPart(tenantElement, premappedIds);
            }

            GraphManager.SetRefPathIntervals(projectConfig.ProjectConfigId, RequestId);

            return new DLSApiProgressResponse()
            {
                ContinuationsWaitForDb = true,
                ParallelRequests = parseComponentRequests,
                ContinueWith = new ParseBusinessObjectsRequest() // BuildAggregationsRequest()
                {
                }
            };
        }
    }
}
