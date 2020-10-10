using CD.DLS.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Clients.Controls.Diagrams
{
    public static class DiagramConstructor
    {
        public static Diagram Construct(List<NodeDescription> nodeDescriptions, List<LinkDeclaration> linkDeclarations)
        {

            Diagram res = new Diagram();
            Dictionary<int, DiagramNode> nodeDictionary = new Dictionary<int, DiagramNode>();

            foreach (var node in nodeDescriptions)
            {
                var header = (node.Name.StartsWith(node.TypeDescription) ? node.Name : node.TypeDescription + " [" + node.Name + "]");
                var description = string.Empty;
                if (node is VisualPartNodeDescription)
                {
                    description = ((VisualPartNodeDescription)node).DescriptivePath;
                }
                var dn = res.AddNode(node.NodeId, header, description);
                nodeDictionary.Add(node.NodeId, dn);
            }

            int lnkId = 0;
            foreach (var link in linkDeclarations)
            {
                var lnk = res.AddLink(lnkId++, nodeDictionary[link.NodeFromId], nodeDictionary[link.NodeToId], 1);
            }

            return res;
        }
    }
}
