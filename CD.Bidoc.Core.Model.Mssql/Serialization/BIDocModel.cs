using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BIDoc;
using CD.DLS.Model.Interfaces.DependencyGraph;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private GraphManager _graphManager;

        private void Load()
        {
            _nodes = _graphManager.GetGraph(out _links, _projectConfigId, _graphKind);
            _loaded = true;
        }

        public BIDocGraphStored(Guid projectConfigId, DependencyGraphKind graphKind, GraphManager graphManager = null)
        {
            _graphKind = graphKind;
            _projectConfigId = projectConfigId;
            _graphManager = graphManager;
            if (_graphManager == null)
            {
                _graphManager = new GraphManager();
            }
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

        private GraphManager _graphManager;

        public BIDocGraphBulk(GraphManager graphManager = null)
        {
                _graphManager = graphManager;
                if (_graphManager == null)
                {
                    _graphManager = new GraphManager();
                }
        }

        public void UpdateGraph(Guid projectConfigId, DependencyGraphKind graphKind)
        {
            _graphManager.ClearGraph(projectConfigId, graphKind);
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
            _graphManager.SaveGraphNodes(_nodes, _links, out newNodeIdMap);
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

            var nextNodeId = _graphManager.GetGraphNodesIdSequence(graph.NodeCount);
            var firstNodeId = nextNodeId;
            var nextLinkId = _graphManager.GetGraphLinksIdSequence(graph.LinkCount);

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

        /// <summary>
        /// The ids of elements already saved to database
        /// </summary>
        public HashSet<int> PremappedElementTargetIds { get; set; }
        public bool EnableUpdate { get; set; }

        private GraphManager _graphManager;
        private int _maxPremappedElementId = 0;
        
        public BIDocModelBulk(GraphManager graphManager = null, HashSet<int> premappedTargetIds = null)
        {
            _graphManager = graphManager;
            if (_graphManager == null)
            {
                _graphManager = new GraphManager();
            }
            if (premappedTargetIds == null)
            {
                PremappedElementTargetIds = new HashSet<int>();
            }
            else
            {
                PremappedElementTargetIds = premappedTargetIds;
                if (premappedTargetIds.Count > 0)
                {
                    _maxPremappedElementId = premappedTargetIds.Max();
                }
            }
        }

        /// <summary>
        ///  returns the mapping from originally assigned element IDs to the saved IDs
        /// </summary>
        /// <param name="projectConfigId"></param>
        /// <returns></returns>
        public Dictionary<int, int> UpdateModel(Guid projectConfigId)
        {
            //_graphManager.ClearModel(projectConfigId);

            foreach (var element in _elements)
                element.ProjectConfigId = projectConfigId;
            foreach (var link in _links)
                link.ProjectConfigId = projectConfigId;

            return _graphManager.SaveModelElements(_elements.ToList(), _links.ToList(), PremappedElementTargetIds, EnableUpdate);
        }

        public void AddElement(BIDocModelElement modelElement)
        {
            _elements.Add(modelElement);
            // so that the ID does not conflict with the premapped IDs
            modelElement.Id = -1 * _elements.Count; // + _maxPremappedElementId;
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
        private Type _leafElementType = null;
        private List<Type> _leafElementTypes = null;
        private bool _loadDefinitions = true;

        public bool LoadDefinitions { get { return _loadDefinitions; } set { _loadDefinitions = value; } }

        public int RootId { get { return _rootId; } }

        private GraphManager _graphManager;

        public enum LoadMethodEnum { Standard, SecondLevelAncestor }
        private LoadMethodEnum _loadMethod = LoadMethodEnum.Standard;
        
        public BIDocModelStored(Guid projectConfigId, string refPathPrefix = null, GraphManager graphManager = null)
        {
            _graphManager = graphManager;
            if (_graphManager == null)
            {
                _graphManager = new GraphManager();
            }
            
            //_context = dbContext;
            _projectConfigId = projectConfigId;
            _refPathPrefix = refPathPrefix;
            if (refPathPrefix == null)
            {
                _rootId = _graphManager.GetModelElementIdByRefPath(projectConfigId, "");
            }
            else
            {
                _rootId = _graphManager.GetModelElementIdByRefPath(projectConfigId, refPathPrefix);
            }
        }
        
        public BIDocModelStored(Guid projectConfigId, int elementId, LoadMethodEnum loadMethod, GraphManager graphManager = null)
        {
            _graphManager = graphManager;
            if (_graphManager == null)
            {
                _graphManager = new GraphManager();
            }

            //_context = dbContext;
            _projectConfigId = projectConfigId;

            _rootId = elementId;
            _loadMethod = loadMethod;
        }

        public BIDocModelStored(Guid projectConfigId, string refPathPrefix, Type leafElementType, GraphManager graphManager)
        {
            _graphManager = graphManager;

            //_context = dbContext;
            _projectConfigId = projectConfigId;
            _refPathPrefix = refPathPrefix;
            _rootId = _graphManager.GetModelElementIdByRefPath(projectConfigId, refPathPrefix);
            _leafElementType = leafElementType;
        }

        public BIDocModelStored(Guid projectConfigId, string refPathPrefix, List<Type> leafElementTypes, GraphManager graphManager)
        {
            _graphManager = graphManager;

            //_context = dbContext;
            _projectConfigId = projectConfigId;
            _refPathPrefix = refPathPrefix;
            _rootId = _graphManager.GetModelElementIdByRefPath(projectConfigId, refPathPrefix);
            _leafElementTypes = leafElementTypes;
        }

        public BIDocModelStored(Guid projectConfigId, List<int> elementIds, GraphManager graphManager = null)
        {
            //_context = dbContext;
            _projectConfigId = projectConfigId;
            _elementIds = elementIds;

            _graphManager = graphManager;
            if (_graphManager == null)
            {
                _graphManager = new GraphManager();
            }

        }

        private void Load()
        {

            ConfigManager.Log.Info(string.Format("Reading model elements in {0}", _refPathPrefix));

            if (_loadMethod == LoadMethodEnum.SecondLevelAncestor)
            {
                _elements = _graphManager.GetModelSecondLevelAncestor(out _links, _rootId);
            }
            else if (_leafElementTypes != null)
            {
                _elements = _graphManager.GetModelFromRootToChildrenOfTypes(out _links, _projectConfigId, _refPathPrefix, _leafElementTypes, _loadDefinitions);
            }
            else if (_leafElementType != null)
            {
                _elements = _graphManager.GetModelFromRootToChildrenOfType(out _links, _projectConfigId, _refPathPrefix, _leafElementType, _loadDefinitions);
            }
            else if (_refPathPrefix != null  /*!string.IsNullOrEmpty(_refPathPrefix)*/)
            {
                _elements = _graphManager.GetModel(out _links, _projectConfigId, _refPathPrefix, _loadDefinitions);
            }
            else if (_elementIds != null)
            {
                _elements = _graphManager.GetElementsById(out _links, _elementIds);  
            }
            else
            {
                _elements = _graphManager.GetModel(out _links, _projectConfigId, null, _loadDefinitions);
            }

            ConfigManager.Log.Info(string.Format("Done reading model elements in {0}", _refPathPrefix));
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

        private GraphManager _graphManager;

        public BIDocDocumentBulk(GraphManager graphManager = null)
        {
            _graphManager = graphManager;
            if (_graphManager == null)
            {
                _graphManager = new GraphManager();
            }
        }

        public void UpdateDocuments(Guid projectConfigId, DependencyGraphKind graphKind)
        {
            _graphManager.ClearGraphDocuments(projectConfigId, graphKind);
            //context.Documents.Where(doc => doc.ProjectConfigId == projectConfigId && doc.DocumentType != DocumentTypeEnum.BusinessDictionary).Delete();

            foreach (var document in _documents)
                document.ProjectConfigId = projectConfigId;

            _graphManager.SaveGraphDocuments(_documents);
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
