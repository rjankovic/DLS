using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CD.DLS.Model.Interfaces.DependencyGraph
{
    /// <summary>
    /// Kinds of dependencies
    /// </summary>
    public enum DependencyKind
    {
        /// <summary>
        /// The meaning of the dependency is not known.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// The target node is a parent of the source node.
        /// </summary>
        Parent = 1,
        /// <summary>
        /// The target node is a child of the source node.
        /// </summary>
        Child = 2,
        /// <summary>
        /// The definition of the source node depends on (uses) the existence of the target node.
        /// </summary>
        DefinitionDepends = 3,
        /// <summary>
        /// The definition of the target node depends on (uses) the existence of the source node.
        /// </summary>
        DefinitionDependent = 4,

        DataFlow = 5,
        /// <summary>
        /// The target node is a source of a foreign key
        /// </summary>
        ForeignKeySource = 6,
        /// <summary>
        /// The target node is a target of a foreign key
        /// </summary>
        ForeignKeyTarget = 7,

        ControlFlowPredecessor = 8,
        ControlFlowSuccessor = 9,


        /// <summary>
        /// The target's definition contains the source's definition
        /// </summary>
        DefinitionContainer = 10,

    }
    

    /*
     * 
     * TODO: support by usage
    public class HTMLRefPath
    {
        public static string GetHTMLRefPath(IModelElement element)
        {

        }
    }
    */
    /*
    public static class IDependencyGraphNodeExtensions
    {
        /// <summary>
        /// The covering node of this node (e.g. column -> table -> schema -> database)
        /// </summary>
        public static IDependencyGraphNode GetParent(this IDependencyGraphNode node)
        {
            IEnumerable<IDependencyGraphLink> links = node.GetLinksByKind(DependencyKind.Parent);
            foreach(IDependencyGraphLink link in links)
            {
                return link.NodeTo;
            }

            return null;
        }
    }*/
    /// <summary>
    /// An edge in the dependency graph
    /// </summary>

    public interface IDependencyGraphLink
    {
        DependencyKind DependencyKind { get; }
        /// <summary>
        /// The dependant node
        /// </summary>
        IDependencyGraphNode NodeFrom { get; }
        /// <summary>
        /// The node being referenced (the node being depended upon)
        /// </summary>
        IDependencyGraphNode NodeTo { get; }
    }
    
    /// <summary>
    /// A node in the dependency graph
    /// </summary>
    public interface IDependencyGraphNode
    {
        IModelElement ModelElement { get; }
        IDependencyGraph Graph { get; }
        int Id { get; set; }
    }

    public interface ILineageAnalysis
    {
        /// <summary>
        /// Adds derived links into the dependency graph.
        /// </summary>
        /// <param name="dependencyGraph"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        IDependencyGraph AddDerivedLinks(IDependencyGraph dependencyGraph, IDependencyGraphNode node);

        /// <summary>
        /// Direct node dependencies (not all links)
        /// </summary>
        /// <param name="dependencyGraph">The input dependency graph.</param>
        /// <param name="node">A node of the graph.</param>
        /// <returns></returns>
        IDependencyGraph GetNodeDependencies(IDependencyGraph dependencyGraph, IDependencyGraphNode node);
        /// <summary>
        /// The nodes that would be potentially impacted by a change to the definition of the node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        IDependencyGraph GetNodeImpact(IDependencyGraph dependencyGraph, IDependencyGraphNode node);
    }
   
    public interface IDependencyGraph
    {
        DependencyGraphKind GraphKind { get; }
        IEnumerable<IDependencyGraphNode> AllNodes { get; }
        IEnumerable<IDependencyGraphLink> AllLinks { get; }
        IDependencyGraphNode GetNode(string refPath);

        int LinkCount { get; }
        int NodeCount { get;}

        IEnumerable<IDependencyGraphLink> GetOutboundLinks(IDependencyGraphNode nodeFrom);
        IEnumerable<IDependencyGraphLink> GetInboundLinks(IDependencyGraphNode nodeFrom);

        IEnumerable<IDependencyGraphLink> GetOutboundLinks(IDependencyGraphNode nodeFrom, DependencyKind kind);
        IEnumerable<IDependencyGraphLink> GetInboundLinks(IDependencyGraphNode nodeFrom, DependencyKind kind);
        void BuildIndexes();
        void BuildNodeIndex();
        void RemapNodeIds(Dictionary<int, int> newIds);
    }    
}
