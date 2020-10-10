using CD.DLS.Interfaces.DependencyGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DependencyGraph
{
    class LineageAnalysis : ILineageAnalysis
    {
        public IDependencyGraph AddDerivedLinks(IDependencyGraph dependencyGraph, IDependencyGraphNode node)
        {
            //TODO: process graph
            return dependencyGraph;
        }

        public IDependencyGraph GetNodeDependencies(IDependencyGraph inputGraph, IDependencyGraphNode node)
        {
            //TODO: process graph
            return inputGraph;
        }

        public IDependencyGraph GetNodeImpact(IDependencyGraph inputGraph, IDependencyGraphNode node)
        {
            //TODO: process graph
            return inputGraph;
           
        }
    }
}
