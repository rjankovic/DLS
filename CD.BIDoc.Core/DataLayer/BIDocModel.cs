using CD.DLS.Interfaces.DependencyGraph;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BIDoc;

namespace CD.DLS.Serialization
{
    public class BIDocGraphStored
    {
        //private readonly CDFrameworkContext _context;
        private readonly Guid _projectConfigId;
        private readonly DependencyGraphKind _graphKind;
        private List<BIDocGraphInfoNode> _nodes = new List<BIDocGraphInfoNode>();
        private List<BIDocGraphInfoLink> _links = new List<BIDocGraphInfoLink>();
        private bool _loaded = false;

        private void Load()
        {
            _nodes = GraphManager.GetGraph(out _links, _projectConfigId, _graphKind);
            _loaded = true;
        }

        public BIDocGraphStored(Guid projectConfigId, DependencyGraphKind graphKind)
        {
            _graphKind = graphKind;
            _projectConfigId = projectConfigId;
        }


        public IEnumerable<BIDocGraphInfoNode> Nodes
        {
            get
            {
                if (!_loaded)
                {
                    Load();
                }
                return _nodes;
            }

        }
        public IEnumerable<BIDocGraphInfoLink> Links
        {
            get
            {
                if (!_loaded)
                {
                    Load();
                }
                return _links;
            }
        }


    }

    /// <summary>
    /// Bulk inserts dependency graph.
    /// </summary>
    public class BIDocGraphBulk
    {
        private readonly List<BIDocGraphInfoNode> _nodes = new List<BIDocGraphInfoNode>();
        private readonly List<BIDocGraphInfoLink> _links = new List<BIDocGraphInfoLink>();

        public List<BIDocGraphInfoNode> Nodes { get { return _nodes; } }
        public List<BIDocGraphInfoLink> Links { get { return _links; } }
        private Dictionary<DependencyGraphKind, IDependencyGraph> _graphs = new Dictionary<DependencyGraphKind, IDependencyGraph>();


        public void UpdateGraph(Guid projectConfigId, DependencyGraphKind graphKind)
        {
            GraphManager.ClearGraph(projectConfigId, graphKind);
            foreach (var node in _nodes)
            {
                node.GraphKind = graphKind;
                node.ProjectConfigId = projectConfigId;
            }
            foreach (var link in _links)
            {
                link.ProjectConfigId = projectConfigId;
            }
            Dictionary<int, int> newNodeIdMap;
            GraphManager.SaveGraphNodes(_nodes, _links, out newNodeIdMap);
            if (_graphs.ContainsKey(graphKind))
            {
                var gr = _graphs[graphKind];
                gr.RemapNodeIds(newNodeIdMap);
            }
        }

        public void AddGraph(IDependencyGraph graph)
        {
            _graphs[graph.GraphKind] = graph;
            Dictionary<IDependencyGraphNode, int> nodeIds = new Dictionary<IDependencyGraphNode, int>();

            var nextNodeId = GraphManager.GetGraphNodesIdSequence(graph.NodeCount);
            var firstNodeId = nextNodeId;
            var nextLinkId = GraphManager.GetGraphLinksIdSequence(graph.LinkCount);

            foreach (IDependencyGraphNode node in graph.AllNodes)
            {
                //if (node.ModelElement.RefPath.Path == "Server[@Name='FSCZPRCT0013']/Database[@Name='MIS_DB_TEST']/StoredProcedure[@Name='rep_brands_mngmt_brand' and @Schema='rs']/[CREATE_0]/[SELECT_125]/[SELECT_126]/[IIf_211]")
                //{

                //}

                var nodeInfo = new BIDocGraphInfoNode
                {
                    Id = nextNodeId++, //_nodes.Count + 1,
                    //RefPath = node.ModelElement.RefPath.ToString(),
                    Name = node.ModelElement.Caption,
                    Description = node.ModelElement.Definition,
                    NodeType = node.ModelElement.GetType().Name,
                    ParentId = null,
                    GraphKind = node.Graph.GraphKind,
                    SourceElementId = node.ModelElement.Id
                    //TopologicalOrder = node.TopologicalOrder
                };
                node.Id = nodeInfo.Id;

                //node.

                //if (node is DataFlowDependencyGraphNode)
                //{
                //    nodeInfo = new BIDocGraphInfoNode
                //    {
                //        Id = _nodes.Count + 1,
                //        //RefPath = node.ModelElement.RefPath.ToString(),
                //        Name = node.ModelElement.Caption,
                //        Description = node.ModelElement.Definition,
                //        NodeType = node.ModelElement.GetType().Name,
                //        ParentId = null,
                //        GraphKind = node.Graph.GraphKind//,
                //        //TopologicalOrder = node.TopologicalOrder
                //    };
                //}
                //else
                //{
                //    nodeInfo = new BIDocGraphInfoNode
                //    {
                //        Id = _nodes.Count + 1,
                //        RefPath = node.ModelElement.RefPath.ToString(),
                //        Name = node.ModelElement.Caption,
                //        Description = node.ModelElement.Definition,
                //        NodeType = node.ModelElement.GetType().Name,
                //        ParentId = null,
                //        GraphKind = node.Graph.GraphKind
                //    };
                //}

                _nodes.Add(nodeInfo);
                nodeIds[node] = nodeInfo.Id;
            }

            foreach (IDependencyGraphLink link in graph.AllLinks)
            {
                var nodeFromId = nodeIds[link.NodeFrom];
                var nodeToId = nodeIds[link.NodeTo];

                if (link.DependencyKind == DependencyKind.Parent)
                {
                    //if(link.NodeFrom.ModelElement.RefPath.Path == "SSASServer[@Name='RJ-THINK']/Db[@Name='Manpower_SSAS']/Cube[@Name='Manpower']/MdxScript[@Name='MdxScript']/CalculatedMeasure[@Name='BLC']")
                    //{

                    //}
                    _nodes[nodeFromId - firstNodeId].ParentId = nodeToId;
                }

                BIDocGraphInfoLink linkInfo = new BIDocGraphInfoLink
                {
                    Id = nextLinkId++,
                    NodeFromId = nodeFromId,
                    NodeToId = nodeToId,
                    LinkType = (LinkTypeEnum)((int)(link.DependencyKind))
                };

                _links.Add(linkInfo);
            }
        }
    }


    /// <summary>
    /// Bulk inserts entities belonging to the model.
    /// </summary>
    public class BIDocModelBulk
    {
        private readonly IList<BIDocModelElement> _elements = new List<BIDocModelElement>(); // new SegmentedFileList<BIDocModelElement>();
        private readonly IList<BIDocModelLink> _links = new List<BIDocModelLink>(); // new SegmentedFileList<BIDocModelLink>();

        public IList<BIDocModelElement> Elements { get { return _elements; } }
        public IList<BIDocModelLink> Links { get { return _links; } }

        public void UpdateModel(Guid projectConfigId)
        {
            GraphManager.ClearModel(projectConfigId);

            foreach (var element in _elements)
                element.ProjectConfigId = projectConfigId;
            foreach (var link in _links)
                link.ProjectConfigId = projectConfigId;

            GraphManager.SaveModelElements(_elements.ToList(), _links.ToList());
            GraphManager.RebindAnnotations(projectConfigId);
        }

        public void AddElement(BIDocModelElement modelElement)
        {
            _elements.Add(modelElement);
            modelElement.Id = _elements.Count;
        }
        public void AddLink(BIDocModelLink modelLink)
        {
            _links.Add(modelLink);
            modelLink.Id = _links.Count;
        }
    }

    public class BIDocModelStored
    {
        //private readonly CDFrameworkContext _context;
        private readonly Guid _projectConfigId;
        private readonly string _refPathPrefix = null;
        private bool _loaded = false;
        private List<BIDocModelElement> _elements = new List<BIDocModelElement>();
        private List<BIDocModelLink> _links = new List<BIDocModelLink>();
        private int _rootId;
        private List<int> _elementIds = null;

        public int RootId { get { return _rootId; } }

        public BIDocModelStored(Guid projectConfigId, string refPathPrefix = null)
        {
            //_context = dbContext;
            _projectConfigId = projectConfigId;
            _refPathPrefix = refPathPrefix;
            _rootId = GraphManager.GetModelElementIdByRefPath(projectConfigId, "Solution");
        }

        public BIDocModelStored(Guid projectConfigId, List<int> elementIds)
        {
            //_context = dbContext;
            _projectConfigId = projectConfigId;
            _elementIds = elementIds;
        }

        private void Load()
        {
            if (_refPathPrefix != null)
            {
                _elements = GraphManager.GetModel(out _links, _projectConfigId, _refPathPrefix);
            }
            else if (_elementIds != null)
            {
                _elements = GraphManager.GetElementsById(out _links, _elementIds);  
            }
            else
            {
                _elements = GraphManager.GetModel(out _links, _projectConfigId);
            }
            _loaded = true;
        }

        public List<BIDocModelElement> Elements
        {
            get
            {
                if (!_loaded)
                {
                    Load();
                }
                // if (_refPathPrefix == null)
                //{
                return _elements;
                // }
            }

        }
        public List<BIDocModelLink> Links
        {
            get
            {
                if (!_loaded)
                {
                    Load();
                }
                //if (_refPathPrefix == null)
                //{
                return _links;
                //}
                //else
                //{

                //// outbound links are not needed
                //    return from gl in _links
                //           join gnf in Elements on gl.ElementFromId equals gnf.Id
                //           join gnt in Elements on gl.ElementToId equals gnt.Id
                //           select gl;
                //}
            }
        }
        
    }

    /// <summary>
    /// Bulk inserts documents.
    /// </summary>
    public class BIDocDocumentBulk
    {
        private readonly List<BIDocGraphDocument> _documents = new List<BIDocGraphDocument>();
        public List<BIDocGraphDocument> Documents { get { return _documents; } }

        public void UpdateDocuments(Guid projectConfigId, DependencyGraphKind graphKind)
        {
            GraphManager.ClearGraphDocuments(projectConfigId, graphKind);
            //context.Documents.Where(doc => doc.ProjectConfigId == projectConfigId && doc.DocumentType != DocumentTypeEnum.BusinessDictionary).Delete();

            foreach (var document in _documents)
                document.ProjectConfigId = projectConfigId;

            GraphManager.SaveGraphDocuments(_documents);
        }


        public void AddDocuments(IEnumerable<GraphDocument> documents)
        {
            _documents.AddRange(documents.Select(
                doc =>
                    new BIDocGraphDocument()
                    {
                        DocumentType = doc.DocumentType,
                        NodeRefPath = doc.NodeRefPath,
                        Content = doc.Content,
                        GraphNodeId = doc.GraphNodeId
                        //,
                        //GraphNodeId = doc.GraphNodeId
                        //,
                        //GraphNode = doc.
                        //TODO !
                    }
                )
                );
        }
    }

}
