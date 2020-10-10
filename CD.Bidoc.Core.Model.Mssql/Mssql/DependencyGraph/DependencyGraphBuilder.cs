using CD.DLS.Common.Structures;
using CD.DLS.Model.Interfaces.DependencyGraph;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Db;
using CD.DLS.Model.Mssql.Ssis;
using System.Collections.Generic;
using System.Linq;

namespace CD.DLS.Model.DependencyGraph
{
    public class MssqlDependencyGraphBuilder
    {
        private readonly Dictionary<MssqlModelElement, DependencyGraphNode> _elementsToNodes = new Dictionary<MssqlModelElement, DependencyGraphNode>();
        private readonly List<MssqlModelElement> _pendingElements = new List<MssqlModelElement>();

        public IDependencyGraph BuildDependencyGraph(MssqlModelElement model, DependencyGraphKind graphKind)
        {
            DependencyGraph graph = new DependencyGraph(graphKind);
            ExtendDependencyGraph(model, graph);
            return graph;
        }

        private void ApplyRules(DependencyGraph graph)
        {
            /* Dependecy graph rules:
             * 1) child->parent
             * 2) foreign key: source->target (fk)
             * 3) col, fk -> parent (script definition)
             * 4) precedence constraint: from->to (precedence)
             * 
             * 5) script fragment -> reference
             */


            foreach(var element in _pendingElements)
            {
                if (element is ForeignKeyElement)
                {
                    ForeignKeyElement fkElement = (ForeignKeyElement)element;
                    DependencyGraphLink link = new DependencyGraphLink(_elementsToNodes[fkElement.SourceColumn], _elementsToNodes[fkElement.TargetColumn], DependencyKind.ForeignKeyTarget);
                    graph.AddLink(link);

                    DependencyGraphLink definitionLink = new DependencyGraphLink(_elementsToNodes[fkElement], _elementsToNodes[fkElement.Parent], DependencyKind.DefinitionContainer);
                    graph.AddLink(definitionLink);


                }
                else if(element is ColumnElement)
                {
                    DependencyGraphLink definitionLink = new DependencyGraphLink(_elementsToNodes[element], _elementsToNodes[element.Parent], DependencyKind.DefinitionContainer);
                    graph.AddLink(definitionLink);
                }
                else if (element is PrecedenceConstraintElement)
                {
                    PrecedenceConstraintElement pcElement = (PrecedenceConstraintElement)element;
                    DependencyGraphLink link = new DependencyGraphLink(_elementsToNodes[pcElement.From], _elementsToNodes[pcElement.To], DependencyKind.ControlFlowSuccessor);
                    graph.AddLink(link);
                }
                else if (element is SqlFragmentElement)
                {
                    SqlFragmentElement fragmentElement = (SqlFragmentElement)element;
                    if (fragmentElement.Reference != null)
                    {
                        DependencyGraphLink link = new DependencyGraphLink(_elementsToNodes[fragmentElement], _elementsToNodes[fragmentElement.Reference], DependencyKind.DefinitionDependent);
                        graph.AddLink(link);
                    }
                }
            }
        }

        internal void ExtendDependencyGraph(MssqlModelElement modelElement, DependencyGraph graph)
        {
            ExtendDependencyGraphHierarchy(modelElement, graph);
            ApplyRules(graph);
        }
        private DependencyGraphNode ExtendDependencyGraphHierarchy(MssqlModelElement modelElement, DependencyGraph graph)
        {
            DependencyGraph dg = new DependencyGraph(DependencyGraphKind.DataFlow);
            DependencyGraphNode node = new DependencyGraphNode(dg, modelElement);
            _elementsToNodes.Add(modelElement, node);
            graph.AddNode(node);

            if(modelElement is ForeignKeyElement || modelElement is PrecedenceConstraintElement || modelElement is ColumnElement || modelElement is SqlFragmentElement)
            {
                _pendingElements.Add(modelElement);
            }

            foreach(var child in modelElement.Children)
            {
                DependencyGraphNode childNode = ExtendDependencyGraphHierarchy(child, graph);
                DependencyGraphLink link = new DependencyGraphLink(childNode, node, DependencyKind.Parent);
                graph.AddLink(link);
            }

            return node;
        }
    }

}
