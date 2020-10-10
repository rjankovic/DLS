using CD.DLS.Serialization;
using CD.DLS.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.DAL.Objects.BIDoc;

namespace CD.DLS.Tools
{
    public static class NodeDeclarationConverter
    {
        public static NodeDeclaration ToNodeDeclaration(BasicGraphInfoNode graphNode)
        {
            return new NodeDeclaration()
            {
                //RefPath = graphNode.RefPath,
                //ModelElementId = graphNode
                Name = graphNode.Name,
                NodeType = graphNode.NodeType
            };
        }

        public static NodeDeclaration ToNodeDeclaration(BIDocGraphInfoNodeExtended graphNode)
        {
            if (graphNode == null)
            {
                return null;
            }
            return new NodeDeclaration()
            {
                //RefPath = graphNode.RefPath,
                Name = graphNode.Name,
                NodeType = graphNode.NodeType,
                RefPath = graphNode.RefPath,
                NodeId = graphNode.Id,
                ModelElementId = graphNode.SourceElementId
            };
        }
    }
}
