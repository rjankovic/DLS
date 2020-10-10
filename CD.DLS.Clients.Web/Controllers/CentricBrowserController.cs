using CD.DLS.Clients.Web.Models;
using CD.DLS.Clients.Web.Models.Diagram;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BIDoc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CD.DLS.Clients.Web.Controllers
{
    public class CentricBrowserController : BaseController
    {
        /// <summary>
        /// initial page, starts loading the node
        /// </summary>
        /// <param name="argument1"></param>
        /// <returns></returns>
        public ActionResult Default(string argument1)
        {
            var guid = Guid.NewGuid().ToString();
            var elementId = int.Parse(argument1);
            var centricBrowserInit = new CentricBrowserInit()
            {
                ElementId = elementId,
                TileId = guid
            };

            return PartialView("Default", centricBrowserInit);
        }

        /// <summary>
        /// start
        /// </summary>
        /// <param name="argument1"></param>
        /// <returns></returns>
        public ActionResult InitialNode(string argument1)
        {
            var elementId = int.Parse(argument1);
            var graphManager = new GraphManager(NetBridge);
            var inspectManager = new InspectManager(NetBridge);
            var nodeId = graphManager.GetGraphNodeId(elementId, DependencyGraphKind.DataFlow);
            var nodeExtended = inspectManager.GetGraphNodeExtended(nodeId);
            var detaillevel = inspectManager.GetElementTypeDetailLevel(nodeExtended.ElementType);
            CentricBrowserDisplayedNodes displayedNodes = new CentricBrowserDisplayedNodes()
            {
                CentralNodeId = nodeId,
                DisplayedNodeIds = new int[] { nodeId }
            };

            var diagram = new Diagram()
            {
                DetailLevel = detaillevel,
                Nodes = new DiagramNode[] { new DiagramNode(nodeId, nodeExtended.Name, nodeExtended.TypeDescription) },
                Links = new DiagramLink[0]
            };
            
            var json = JsonConvert.SerializeObject(diagram);

            var result = new ContentResult
            {
                Content = json,
                ContentType = "application/json"
            };
            return result;
            
            //return View("Index");
        }

        public ActionResult ShowNodeLineage(string argument1)
        {
            int nodeId = int.Parse(argument1);
            var nodesToDiagram = GetLinksTo(nodeId);
            var json = JsonConvert.SerializeObject(nodesToDiagram);

            var result = new ContentResult
            {
                Content = json,
                ContentType = "application/json"
            };
            return result;
        }

        public ActionResult ShowNodeImpact(string argument1)
        {
            int nodeId = int.Parse(argument1);
            var nodesFromDiagram = GetLinksFrom(nodeId);
            var json = JsonConvert.SerializeObject(nodesFromDiagram);

            var result = new ContentResult
            {
                Content = json,
                ContentType = "application/json"
            };
            return result;
        }

        public ActionResult SwitchDetailLevel(string argument1)
        {
            CentricBrowserDisplaySwitch switchSpec = JsonConvert.DeserializeObject<CentricBrowserDisplaySwitch>(argument1);
            var diagram = SwitchToDetailLevel(switchSpec);
            var json = JsonConvert.SerializeObject(diagram);

            var result = new ContentResult
            {
                Content = json,
                ContentType = "application/json"
            };
            return result;
        }

        private Diagram SwitchToDetailLevel(CentricBrowserDisplaySwitch displaySwitch)
        {
            InspectManager inspectManager = new InspectManager(NetBridge);

            List<BIDocGraphInfoNodeExtended> nodes = new List<BIDocGraphInfoNodeExtended>();
            List<BIDocGraphInfoLink> links = new List<BIDocGraphInfoLink>();
            if (displaySwitch.DisplayedNodeIds.Length == 1)
            {
                nodes = inspectManager.TranslateDataFlowLNodeDetailLevel(displaySwitch.DisplayedNodeIds[0], displaySwitch.SourceDetailLevel, displaySwitch.TargetDetailLevel);
            }
            else
            {
                var linksAmontCurrentNodes = inspectManager.GetDataFlowLinksAmongNodes(ProjectConfig.ProjectConfigId, displaySwitch.DisplayedNodeIds.ToList());
                var translatedLinks = inspectManager.TranslateDataFlowLinksDetailLevel(ProjectConfig.ProjectConfigId, 
                    linksAmontCurrentNodes.Select(x => x.Id).Distinct().ToList(), 
                    displaySwitch.SourceDetailLevel, 
                    displaySwitch.TargetDetailLevel);
                var nodeIds = translatedLinks.Select(x => x.NodeFromId).Union(translatedLinks.Select(y => y.NodeToId)).Distinct().ToList();
                var translatedNodes = inspectManager.GetNodesExtended(nodeIds);
                nodes = translatedNodes;

                // no links among the nodes
                if (nodes.Count == 0)
                {
                    nodes = inspectManager.TranslateDataFlowLNodeDetailLevel(displaySwitch.DisplayedNodeIds[0], displaySwitch.SourceDetailLevel, displaySwitch.TargetDetailLevel);
                }
                links = translatedLinks;
            }

            return new Diagram()
            {
                DetailLevel = displaySwitch.TargetDetailLevel,
                Links = links.Select(x => new DiagramLink() { id = x.Id, source = x.NodeFromId, target = x.NodeToId }).ToArray(),
                Nodes = nodes.Select(x => new DiagramNode(id: x.Id, name: x.Name, description: x.TypeDescription)).ToArray()
            };
        }

        private Diagram GetLinksFrom(int sourceNodeId)
        {
            var inspectManager = new InspectManager(NetBridge);
            var linksFrom = inspectManager.GetDataFlowLinksFromNode(ProjectConfig.ProjectConfigId, sourceNodeId);
            var targetNodesToDisplay = linksFrom.Select(x => x.NodeToId).Distinct().ToList();
            var nodesToDisplay = inspectManager.GetNodesExtended(targetNodesToDisplay);

            return new Diagram()
            {
                Links = linksFrom.Select(x => new DiagramLink() { id = x.Id, source = x.NodeFromId, target = x.NodeToId }).ToArray(),
                Nodes = nodesToDisplay.Select(x => new DiagramNode(id: x.Id, name: x.Name, description: x.TypeDescription)).ToArray()
            };
        }

        private Diagram GetLinksTo(int targetNodeId)
        {
            var inspectManager = new InspectManager(NetBridge);
            var linksTo = inspectManager.GetDataFlowLinksToNode(ProjectConfig.ProjectConfigId, targetNodeId);
            var targetNodesToDisplay = linksTo.Select(x => x.NodeFromId).Distinct().ToList();
            var nodesToDisplay = inspectManager.GetNodesExtended(targetNodesToDisplay);

            return new Diagram()
            {
                Links = linksTo.Select(x => new DiagramLink() { id = x.Id, source = x.NodeFromId, target = x.NodeToId }).ToArray(),
                Nodes = nodesToDisplay.Select(x => new DiagramNode(id: x.Id, name: x.Name, description: x.TypeDescription)).ToArray()
            };
        }
    }
}