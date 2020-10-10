using CD.DLS.Interfaces;
using CD.DLS.Interfaces.DependencyGraph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DependencyGraph
{
    
    public class DataFlowDependencyGraphNode : DependencyGraphNode
    {
        
        public int _topologicalOrder;

        public DataFlowDependencyGraphNode(IDependencyGraph graph, IModelElement modelElement)
            :base(graph, modelElement)
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
