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
using CD.DLS.DAL.Objects.BIDoc;
using CD.DLS.DAL.Managers;
//using CD.Framework.EF.Entities;

namespace CD.DLS.Operations
{
    internal class GetNodeDescendantsRequestProcessor : BIDocRequestProcessor<GetNodeDescendantsRequest>
    {
        public GetNodeDescendantsRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override ProcessingResult ProcessRequest(GetNodeDescendantsRequest request, ProjectConfig projectConfig)
        {
            
            var element = GraphManager.GetModelElementById(request.RootModelElementId);
            var nodes = GraphManager.GetNodesExtended(projectConfig.ProjectConfigId, DependencyGraphKind.DataFlowTransitive, element.RefPath, request.NodeType);

            GetNodeDescendantsRequestResponse result = new GetNodeDescendantsRequestResponse();
            result.Descendants = nodes.Select(x => NodeDeclarationConverter.ToNodeDeclaration(x)).ToList();
            
            var stringResult = result.Serialize();

            return new ProcessingResult()
            {
                Content = stringResult,
                Attachments = new List<Attachment>()
            };
        }
        
    }
}
