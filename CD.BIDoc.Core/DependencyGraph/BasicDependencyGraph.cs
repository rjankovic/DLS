using CD.DLS.Interfaces;
using CD.DLS.Interfaces.DependencyGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.DAL.Objects.BIDoc;

namespace CD.DLS.DependencyGraph
{

    public class DependencyGraph : IDependencyGraph
    {
        private readonly IList<DependencyGraphLink> _links = new List<DependencyGraphLink>(); //new SegmentedFileList<DependencyGraphLink>(100000, ;
        private readonly IList<DependencyGraphNode> _nodes = new /*SegmentedFile*/List<DependencyGraphNode>();
        private readonly DependencyGraphKind _graphKind;
        
        private Dictionary<string, IDependencyGraphNode> _nodeDictionary = null;
        private Dictionary<IDependencyGraphNode, List<IDependencyGraphLink>> _inboundLinksDictionary = null;
        private Dictionary<IDependencyGraphNode, List<IDependencyGraphLink>> _outboundLinksDictionary = null;
        private Dictionary<IDependencyGraphNode, Dictionary<DependencyKind, List<IDependencyGraphLink>>> _inboundLinksKindsDictionary = null;
        private Dictionary<IDependencyGraphNode, Dictionary<DependencyKind, List<IDependencyGraphLink>>> _outboundLinksKindsDictionary = null;

        private bool _indexesUpToDate = false;
        private bool _nodeIndexUpToDate = false;

        public DependencyGraph(DependencyGraphKind graphKind)
        {
            _graphKind = graphKind;
            //_links = new SegmentedFileList<DependencyGraphLink>(100000, new List<JsonConverter>() { new DependencyGraphLinkJsonConverter(this) });
        }

        IEnumerable<IDependencyGraphLink> IDependencyGraph.AllLinks
        {
            get
            {
                return _links;
            }
        }

        IEnumerable<IDependencyGraphNode> IDependencyGraph.AllNodes
        {
            get
            {
                return _nodes;
            }
        }

        public int LinkCount
        {
            get { return _links.Count; }
        }
        public int NodeCount
        {
            get { return _nodes.Count; }
        }

        public IDependencyGraphNode GetNode(string refPath)
        {
            if (_indexesUpToDate || _nodeIndexUpToDate)
            {
                return _nodeDictionary[refPath];
            }

            return _nodes.First(x => x._modelElement.RefPath.Path == refPath);
        }

        IEnumerable<IDependencyGraphLink> IDependencyGraph.GetInboundLinks(IDependencyGraphNode nodeTo)
        {
            if (_indexesUpToDate)
            {
                if (!_inboundLinksDictionary.ContainsKey(nodeTo))
                {
                    return new IDependencyGraphLink[0];
                }
                return _inboundLinksDictionary[nodeTo];
            }

            return _links.Where(l => ((IDependencyGraphLink)l).NodeTo == nodeTo);
        }

        IEnumerable<IDependencyGraphLink> IDependencyGraph.GetInboundLinks(IDependencyGraphNode nodeTo, DependencyKind kind)
        {
            if (_indexesUpToDate)
            {
                if (!_inboundLinksKindsDictionary.ContainsKey(nodeTo))
                {
                    return new IDependencyGraphLink[0];
                }
                if (!_inboundLinksKindsDictionary[nodeTo].ContainsKey(kind))
                {
                    return new IDependencyGraphLink[0];
                }
                return _inboundLinksKindsDictionary[nodeTo][kind];
            }

            return _links.Where(l => ((IDependencyGraphLink)l).NodeTo == nodeTo && l.Kind == kind);
        }

        IEnumerable<IDependencyGraphLink> IDependencyGraph.GetOutboundLinks(IDependencyGraphNode nodeFrom)
        {
            if (_indexesUpToDate)
            {
                if (!_outboundLinksDictionary.ContainsKey(nodeFrom))
                {
                    return new IDependencyGraphLink[0];
                }
                return _outboundLinksDictionary[nodeFrom];
            }

            return _links.Where(l => ((IDependencyGraphLink)l).NodeFrom == nodeFrom);
        }

        IEnumerable<IDependencyGraphLink> IDependencyGraph.GetOutboundLinks(IDependencyGraphNode nodeFrom, DependencyKind kind)
        {
            if (_indexesUpToDate)
            {
                if (!_outboundLinksKindsDictionary.ContainsKey(nodeFrom))
                {
                    return new IDependencyGraphLink[0];
                }
                if (!_outboundLinksKindsDictionary[nodeFrom].ContainsKey(kind))
                {
                    return new IDependencyGraphLink[0];
                }
                return _outboundLinksKindsDictionary[nodeFrom][kind];
            }

            return _links.Where(l => ((IDependencyGraphLink)l).NodeFrom == nodeFrom && l.Kind == kind);
        }

        public void AddLink(DependencyGraphLink link)
        {
            if (link.NodeFrom.ModelElement.RefPath.Path == link.NodeTo.ModelElement.RefPath.Path)
            {
            }
            _links.Add(link);
            _indexesUpToDate = false;
        }
        public void AddNode(DependencyGraphNode node)
        {
            _nodes.Add(node);
            _indexesUpToDate = false;
            _nodeIndexUpToDate = false;
        }

        public void BuildIndexes()
        {
            _nodeDictionary = _nodes.ToDictionary(x => x._modelElement.RefPath.Path, x => (IDependencyGraphNode)x);
            _inboundLinksDictionary = new Dictionary<IDependencyGraphNode, List<IDependencyGraphLink>>();
            _outboundLinksDictionary = new Dictionary<IDependencyGraphNode, List<IDependencyGraphLink>>();
            _inboundLinksKindsDictionary = new Dictionary<IDependencyGraphNode, Dictionary<DependencyKind, List<IDependencyGraphLink>>>();
            _outboundLinksKindsDictionary = new Dictionary<IDependencyGraphNode, Dictionary<DependencyKind, List<IDependencyGraphLink>>>();

            var inboundGroups = _links.GroupBy(x => ((IDependencyGraphLink)x).NodeTo);
            var outboundGroups = _links.GroupBy(x => ((IDependencyGraphLink)x).NodeFrom);

            foreach (var inGroup in inboundGroups)
            {
                _inboundLinksDictionary.Add(inGroup.Key, inGroup.Select(x => (IDependencyGraphLink)x).ToList());
                _inboundLinksKindsDictionary.Add(inGroup.Key, new Dictionary<DependencyKind, List<IDependencyGraphLink>>());
                var typeGroups = _inboundLinksDictionary[inGroup.Key].GroupBy(x => x.DependencyKind);
                foreach (var tGroup in typeGroups)
                {
                    _inboundLinksKindsDictionary[inGroup.Key].Add(tGroup.Key, tGroup.ToList());
                }
            }

            foreach (var outGroup in outboundGroups)
            {
                _outboundLinksDictionary.Add(outGroup.Key, outGroup.Select(x => (IDependencyGraphLink)x).ToList());
                _outboundLinksKindsDictionary.Add(outGroup.Key, new Dictionary<DependencyKind, List<IDependencyGraphLink>>());
                var typeGroups = _outboundLinksDictionary[outGroup.Key].GroupBy(x => x.DependencyKind);
                foreach (var tGroup in typeGroups)
                {
                    _outboundLinksKindsDictionary[outGroup.Key].Add(tGroup.Key, tGroup.ToList());
                }
            }

            _indexesUpToDate = true;
        }

        public void BuildNodeIndex()
        {
            _nodeDictionary = _nodes.ToDictionary(x => x._modelElement.RefPath.Path, x => (IDependencyGraphNode)x);
            _nodeIndexUpToDate = true;
        }

        public void RemapNodeIds(Dictionary<int, int> newIds)
        {
            foreach (var node in _nodes)
            {
                node.Id = newIds[node.Id];
            }

            if (_indexesUpToDate)
            {
                BuildIndexes();
            }
            else if (_nodeIndexUpToDate)
            {
                BuildNodeIndex();
            }
        }

        public DependencyGraphKind GraphKind
        {
            get
            {
                return _graphKind;
            }
        }
    }
    

    public class DependencyGraphNode : IDependencyGraphNode
    {
        internal readonly IModelElement _modelElement;
        internal readonly IDependencyGraph _graph;
        internal readonly Dictionary<DependencyGraphNode, List<DependencyGraphLink>> _links;

        public int Id { get; set; }

        public DependencyGraphNode(IDependencyGraph graph, IModelElement modelElement)
        {
            _modelElement = modelElement;
            _graph = graph;
        }

        IModelElement IDependencyGraphNode.ModelElement
        {
            get
            {
                return _modelElement;
            }
        }

        IDependencyGraph IDependencyGraphNode.Graph
        {
            get
            {
                return _graph;
            }
        }
    }
    public class DependencyGraphLink : IDependencyGraphLink
    {
        //internal readonly DependencyGraphNode _from;
        //internal readonly DependencyGraphNode _to;
        public IDependencyGraphNode NodeFrom { get; set; }
        public IDependencyGraphNode NodeTo { get; set; }
        public DependencyKind Kind { get; set; }


        DependencyKind IDependencyGraphLink.DependencyKind
        {
            get
            {
                return Kind;
            }
        }

        IDependencyGraphNode IDependencyGraphLink.NodeFrom
        {
            get
            {
                return NodeFrom; // _from;
            }
        }

        IDependencyGraphNode IDependencyGraphLink.NodeTo
        {
            get
            {
                return NodeTo; // _to;
            }
        }


        public DependencyGraphLink(DependencyGraphNode from, DependencyGraphNode to, DependencyKind kind)
        {
            //_from = from;
            //_to = to;
            Kind = kind;
            NodeFrom = from;
            NodeTo = to;

        }

        //public DependencyGraphLink(IDependencyGraph graph)
        //{
        //    _graph = graph;
        //}

    }


    //public class DependencyGraphLinkJsonConverter : JsonConverter
    //{
    //    private readonly IDependencyGraph _graph;
    //    public DependencyGraphLinkJsonConverter(IDependencyGraph graph)
    //    {
    //        _graph = graph;
    //    }

    //    public override bool CanConvert(Type objectType)
    //    {
    //        return typeof(DependencyGraphLink).IsAssignableFrom(objectType);
    //    }

        
    //    public override object ReadJson(JsonReader reader,
    //                            Type objectType,
    //                             object existingValue,
    //                             JsonSerializer serializer)
    //    {
    //        // Load JObject from stream
    //        JObject jObject = JObject.Load(reader);

    //        // Create target object based on JObject
    //        DependencyGraphLink lnk = new DependencyGraphLink(_graph);

    //        // Populate the object properties
    //        serializer.Populate(jObject.CreateReader(), lnk);

    //        return lnk;
    //    }


    //    public override bool CanWrite
    //    {
    //        get
    //        {
    //            return false;
    //        }
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}


}
