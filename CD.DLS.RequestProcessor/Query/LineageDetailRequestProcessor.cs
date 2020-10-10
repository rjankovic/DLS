using CD.DLS.API;
using CD.DLS.API.Query;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Objects.BIDoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.RequestProcessor.Query
{
    public class LineageDetailRequestProcessor : RequestProcessorBase, IRequestProcessor<LineageDetailRequest, LineageDetailResponse>
    {
        public LineageDetailResponse Process(LineageDetailRequest request, ProjectConfig projectConfig)
        {
            var requestResult = new LineageDetailResponse();

            var sourceNodeId = GraphManager.GetGraphNodeId(projectConfig.ProjectConfigId, request.SourceRefPath, DependencyGraphKind.DataFlow);
            var targetNodeId = GraphManager.GetGraphNodeId(projectConfig.ProjectConfigId, request.TargetRefPath, DependencyGraphKind.DataFlow);
            requestResult.SourceRefPath = request.SourceRefPath;
            requestResult.TargetRefPath = request.TargetRefPath;

            Log.Important(string.Format("Listing detailed links from node {0} to node {1}", sourceNodeId, targetNodeId));
            var links = InspectManager.GetDataFlowLinksBetweenNodes(projectConfig.ProjectConfigId, sourceNodeId, targetNodeId, (int)(request.DetailLevel));



            requestResult.Links = links.Select(x => new LinkDeclaration() { LinkType = x.LinkType, NodeFromId = x.NodeFromId, NodeToId = x.NodeToId }).ToList();

            var nodeIds = requestResult.Links.Select(x => x.NodeFromId).Union(requestResult.Links.Select(y => y.NodeToId)).Distinct();

            requestResult.Nodes = new List<NodeDescription>();

            Dictionary<int, BIDocGraphInfoNodeExtended> nodesExtended = new Dictionary<int, BIDocGraphInfoNodeExtended>();

            foreach (var nodeId in nodeIds)
            {
                var nodeExtended = InspectManager.GetGraphNodeExtended(nodeId);
                nodesExtended.Add(nodeId, nodeExtended);

            }

            foreach (var nodeId in nodeIds)
            {
                var nodeExtended = nodesExtended[nodeId];

                bool isSchemaObject = new string[] { "SchemaTableElement", "ViewElement" }.Contains(nodeExtended.NodeType);

                var nodeDescription = new NodeDescription()
                {
                    Definition = nodeExtended.Description,
                    ModelElementId = nodeExtended.SourceElementId,
                    Name = isSchemaObject ? nodeExtended.DescriptivePath.Substring(nodeExtended.DescriptivePath.IndexOf('[')) : nodeExtended.Name,
                    NodeId = nodeExtended.Id,
                    NodeType = nodeExtended.NodeType,
                    TypeDescription = nodeExtended.TypeDescription,
                    RefPath = nodeExtended.RefPath,
                    DescriptivePath = nodeExtended.DescriptivePath
                };

                requestResult.Nodes.Add(nodeDescription);

            }

            return requestResult;
        }
    }
}
