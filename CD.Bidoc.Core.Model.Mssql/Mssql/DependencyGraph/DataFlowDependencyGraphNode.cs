using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Interfaces.DependencyGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.DependencyGraph
{
    public class DataFlowDependencyGraphNode : DependencyGraphNode
    {

        public int _topologicalOrder;

        public DataFlowDependencyGraphNode(IDependencyGraph graph, IModelElement modelElement)
            : base(graph, modelElement)
        {
        }

        public int TopologicalOrder
        {
            get
            {
                return _topologicalOrder;
            }
            set
            {
                _topologicalOrder = value;
            }
        }

    }
}
