using CD.DLS.Serialization;
using CD.DLS.API;
using CD.DLS.Common.Structures;
using System.Collections.Generic;
using System.Linq;
using CD.DLS.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BIDoc;
using System;

namespace CD.DLS.Operations
{
    internal class LineageDetailRequestProcessor : BIDocRequestProcessor<LineageDetailRequest>
    {

        
        public LineageDetailRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override ProcessingResult ProcessRequest(LineageDetailRequest request, ProjectConfig projectConfig)
        {
            var attachments = new List<Attachment>();
            var requestResult = new LineageDetailRequestResponse();

            var sourceNodeId = GraphManager.GetGraphNodeId(projectConfig.ProjectConfigId, request.SourceRefPath, DependencyGraphKind.DataFlow /*DependencyGraphKind.DataFlowTransitive*/);
            var targetNodeId = GraphManager.GetGraphNodeId(projectConfig.ProjectConfigId, request.TargetRefPath, DependencyGraphKind.DataFlow /*DependencyGraphKind.DataFlowTransitive*/);
            requestResult.SourceRefPath = request.SourceRefPath;
            requestResult.TargetRefPath = request.TargetRefPath;

            _core.Log.Important(string.Format("Listing detailed links from node {0} to node {1}", sourceNodeId, targetNodeId));
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
                
                var nodeDescription = new NodeDescription()
                {
                    Definition = nodeExtended.Description,
                    ModelElementId = nodeExtended.SourceElementId,
                    Name = nodeExtended.Name,
                    NodeId = nodeExtended.Id,
                    NodeType = nodeExtended.NodeType,
                    TypeDescription = nodeExtended.TypeDescription,
                    RefPath = nodeExtended.RefPath,
                    DescriptivePath = nodeExtended.DescriptivePath
                };
                
                requestResult.Nodes.Add(nodeDescription);

            }
            
            var stringResult = requestResult.Serialize();

            return new ProcessingResult()
            {
                Content = stringResult,
                Attachments = attachments
            };

        }
        

    }
}
