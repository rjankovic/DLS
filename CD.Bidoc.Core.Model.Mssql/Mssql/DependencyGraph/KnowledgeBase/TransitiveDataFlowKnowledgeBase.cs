using CD.DLS.Common.Structures;
using CD.DLS.Model.Interfaces.DependencyGraph;
using CD.DLS.Model.Mssql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CD.DLS.Model.DependencyGraph.KnowledgeBase
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
