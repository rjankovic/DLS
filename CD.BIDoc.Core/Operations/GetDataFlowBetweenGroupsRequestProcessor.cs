using CD.DLS.Serialization;
using CD.DLS.API;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Parse.Mssql.Ssas;
using System.Data.SqlClient;
using CD.DLS.Tools;
using CD.DLS.DAL.Managers;
//using CD.Framework.EF.Entities;

namespace CD.DLS.Operations
{
    internal class GetDataFlowBetweenGroupsRequestProcessor : BIDocRequestProcessor<GetDataFlowBetweenGroupsRequest>
    {
        public GetDataFlowBetweenGroupsRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override ProcessingResult ProcessRequest(GetDataFlowBetweenGroupsRequest request, ProjectConfig projectConfig)
        {

            var links = InspectManager.GetDataFlowBetweenGroups(projectConfig.ProjectConfigId, request.SourceRefPathPrefix, request.TargetRefPathPrefix, request.SourceNodeType, request.TargetNodeType);

            GetDataFlowBetweenGroupsRequestResponse result = new GetDataFlowBetweenGroupsRequestResponse();
            result.Links = links
                .Select(x => new Tuple<NodeDeclaration, NodeDeclaration>(
                    NodeDeclarationConverter.ToNodeDeclaration(x.Item1), 
                    NodeDeclarationConverter.ToNodeDeclaration(x.Item2)))
                    .ToList();
            
            var stringResult = result.Serialize();

            return new ProcessingResult()
            {
                Content = stringResult,
                Attachments = new List<Attachment>()
            };
        }
        
    }
}
