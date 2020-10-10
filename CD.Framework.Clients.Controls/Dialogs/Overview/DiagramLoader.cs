using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BIDoc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Clients.Controls.Diagrams;
using CD.DLS.Common.Structures;

namespace CD.DLS.Clients.Controls.Dialogs.Overview
{
    public static class DiagramLoader
    {
        public static Diagram LoadDiagram(Guid projectId, DependencyGraphKind graphKind)
        {
            Diagram res = new Diagram();
            
            List<BIDocGraphInfoLink> links;
            InspectManager im = new InspectManager();
            var nodes = im.GetGraphExtended(out links, projectId, graphKind);

            Dictionary<int, DiagramNode> nodeDictionary = new Dictionary<int, DiagramNode>();
            foreach (var node in nodes)
            {
                var dn = res.AddNode(node.Id, node.Description, node.TypeDescription);
                nodeDictionary.Add(node.Id, dn);
            }
            
            foreach (var link in links)
            {
                var ep = link.ExtendedProperties;
                var jo = JObject.Parse(ep);
                var strength = jo["Strength"].Value<int>();
                var lnk = res.AddLink(link.Id, nodeDictionary[link.NodeFromId], nodeDictionary[link.NodeToId], strength);
            }
            
            return res;


                /*
                             var n3 = _diagram.AddNode(3, "DifferentName2", "Different Description 2");
            var n4 = _diagram.AddNode(4, "DifferentName3", "Different Description 3");
            var l1 = _diagram.AddLink(1, n1, n2, 100);
            var l2 = _diagram.AddLink(1, n1, n3, 100);
            var l3 = _diagram.AddLink(1, n1, n4, 100);

            var l1b = _diagram.AddLink(1, n2, n1, 50);
            var l2b = _diagram.AddLink(2, n3, n1, 5);

            var n33 = _diagram.AddNode(5, "DifferentName2X", "Different Description 2X");
                 */
        }
    }
}
