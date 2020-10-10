using CD.DLS.Interfaces.DependencyGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.DAL.Objects.BIDoc;

namespace CD.DLS.DependencyGraph.Mssql.KnowledgeBase
{
    public class TransitiveDataFlowKnowledgeBase : GeneralKnowledgeBase
    {
        public TransitiveDataFlowKnowledgeBase(IDependencyGraph dataflowGraph)
        :base(
        DependencyGraphKind.DataFlowTransitive,
                  new MssqlDataFlowRule[]
            {
                new TransitiveDataFlowRule()
            })
        {
            _context.AddSourceGraph(dataflowGraph);
        }

        protected override DependencyGraphNode ExtendDependencyGraphHierarchy(MssqlModelElement modelElement)
        {
            var dataflowGraph = _context.GetSourceGraphByKind(DependencyGraphKind.DataFlow);
            var correspondingNode = dataflowGraph.GetNode(modelElement.RefPath.Path);

            DependencyGraphNode node = new DataFlowDependencyGraphNode(_graph, modelElement);
            ((DataFlowDependencyGraphNode)node).TopologicalOrder = ((DataFlowDependencyGraphNode)correspondingNode).TopologicalOrder;
            _context.MapElementToNode(modelElement, node);
            _graph.AddNode(node);
            
            foreach (var child in modelElement.Children)
            {
                DependencyGraphNode childNode = ExtendDependencyGraphHierarchy(child);
                DependencyGraphLink link = new DependencyGraphLink(childNode, node, DependencyKind.Parent);
                _graph.AddLink(link);
            }

            return node;
        }
    }
}
