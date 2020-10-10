using CD.DLS.API.Query;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BIDoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.DLS.Clients.Web.Models.Diagram
{
    public class DiagramLoader
    {
        private NetBridge _netBridge;

        public DiagramLoader(NetBridge netBridge)
        {
            _netBridge = netBridge;
        }

        public Diagram LoadDiagram(Guid projectId, DependencyGraphKind graphKind)
        {
            List<BIDocGraphInfoLink> links;
            InspectManager im = new InspectManager(_netBridge);
            var nodes = im.GetGraphExtended(out links, projectId, graphKind);

            int detailLevel = 1;
            if (graphKind == DependencyGraphKind.DFHigh)
            {
                detailLevel = 3;
            }
            if (graphKind == DependencyGraphKind.DataFlowMediumDetail)
            {
                detailLevel = 2;
            }

            var res = new Diagram()
            {
                DetailLevel = detailLevel,
                Nodes = nodes.Select(x => new DiagramNode(x.Id, x.Name, x.TypeDescription)).ToArray(),
                Links = links.Select(x => new DiagramLink() { id = x.Id, source = x.NodeFromId, target = x.NodeToId }).ToArray()
            };

            return res;
        }

        public Diagram LoadDiagram(LineageDetailRequest request, LineageDetailResponse lineageDetail)
        {
            List<DiagramLink> links = new List<DiagramLink>();
            int linkId = 1;
            foreach (var link in lineageDetail.Links)
            {
                links.Add(new DiagramLink() { id = linkId++, source = link.NodeFromId, target = link.NodeToId });
            }

            int detailLevel = 1;
            if (request.DetailLevel == LineageDetailLevelEnum.Medium)
            {
                detailLevel = 2;
            }
            if (request.DetailLevel == LineageDetailLevelEnum.High)
            {
                detailLevel = 3;
            }

            var res = new Diagram()
            {
                DetailLevel = detailLevel,
                Nodes = lineageDetail.Nodes.Select(x => new DiagramNode(x.NodeId, x.Name, x.TypeDescription)).ToArray(),
                Links = links.ToArray()
            };

            return res;
        }
    }
}