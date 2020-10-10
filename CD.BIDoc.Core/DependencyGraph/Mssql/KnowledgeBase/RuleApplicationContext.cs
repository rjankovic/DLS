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
    public class RuleApplicationContext : IRuleApplicationContext
    {
        private DependencyGraph _graph;
        private List<IDependencyGraph> _sourceGraphs;
        private Dictionary<MssqlModelElement, DependencyGraphNode> _elementsToNodes;
        private Dictionary<string, DependencyGraphNode> _refPathsToNodes;
        public RuleApplicationContext(DependencyGraph graph, List<IDependencyGraph> sourceGraphs = null)
        {
            _graph = graph;
            _elementsToNodes = new Dictionary<MssqlModelElement, DependencyGraphNode>();
            _refPathsToNodes = new Dictionary<string, DependencyGraphNode>();
            _sourceGraphs = new List<IDependencyGraph>();
            if (sourceGraphs != null)
            {
                _sourceGraphs = sourceGraphs;
            }
        }

        public DependencyGraph Graph
        {
            get { return _graph; }
        }

        public void AddSourceGraph(IDependencyGraph graph)
        {
            _sourceGraphs.Add(graph);
        }

        public IDependencyGraph GetSourceGraphByKind(DependencyGraphKind kind)
        {
            return _sourceGraphs.First(x => x.GraphKind == kind);
        }

        public void MapElementToNode(MssqlModelElement element, DependencyGraphNode node)
        {
            _elementsToNodes.Add(element, node);
            _refPathsToNodes.Add(element.RefPath.Path, node);
        }

        public void AddLink(IDependencyGraphNode fromNode, IDependencyGraphNode toNode, IRule rule)
        {
            _graph.AddLink(new DependencyGraphLink((DependencyGraphNode)fromNode, (DependencyGraphNode)toNode, rule.DependencyKind));
        }

        public IDependencyGraphNode GetNode(IModelElement modelElement)
        {
            if (!_elementsToNodes.ContainsKey(modelElement as MssqlModelElement))
            {
                return null;
            }
            return _elementsToNodes[(MssqlModelElement)modelElement];
        }

        public IDependencyGraphNode GetNode(string refPath)
        {
            if (!_refPathsToNodes.ContainsKey(refPath))
            {
                return null;
            }
            return _refPathsToNodes[refPath];
        }
    }
}
