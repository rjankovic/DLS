using CD.DLS.API;
using CD.DLS.Clients.Controls.Diagrams;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BIDoc;
using CD.DLS.DAL.Receiver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CD.DLS.Clients.Controls.Dialogs.CentricGraphBrowser
{
    /// <summary>
    /// Interaction logic for CentricBroser.xaml
    /// </summary>
    public partial class CentricBrowser : UserControl
    {
        public event ElementView.BusinessViewLinkClickedHander BusinessViewLinkClicked;
        public event ElementView.ElementViewEventHandler OpenPivotButtonClick;

        private int _centralElementId;
        private ServiceHelper _serviceHelper;
        private Guid _projectId;
        private Diagram _diagram;
        private List<BIDocGraphInfoLink> _displayedLinks = new List<BIDocGraphInfoLink>();

        private List<BIDocGraphInfoLink> _oldLink = new List<BIDocGraphInfoLink>();
        private int _currentDetailLevel = 0;
        private int _detailLevel = 0;
        private BIDocGraphInfoNodeExtended _centralNode;
        private bool _businessLinkClikThrough = false;

        /// <summary>
        /// Links that were displayed instead of original links after changing the view's detail level
        /// </summary>
        private List<BIDocGraphInfoLink> _translatedDisplayedLinks = new List<BIDocGraphInfoLink>();
        private Dictionary<int, BIDocGraphInfoNodeExtended> _nodeDictionary = new Dictionary<int, BIDocGraphInfoNodeExtended>();
        /// <summary>
        /// Nodes that have all inoming links displayed
        /// </summary>
        private HashSet<int> _nodesWithDisplayedIncomingLinks = new HashSet<int>();
        /// <summary>
        /// Nodes that have all outoming links displayed
        /// </summary>
        private HashSet<int> _nodesWithDisplayedOutcomingLinks = new HashSet<int>();
        private DiagramNode _rightClickedNode = null;


        private AnnotationManager _annotationManager;
        private SearchManager _searchManager;
        private GraphManager _graphManager;
        private InspectManager _inspectManager;

        private AnnotationManager AnnotationManager
        {
            get { return _annotationManager; }
        }
        private SearchManager SearchManager
        {
            get { return _searchManager; }
        }
        private GraphManager GraphManager
        {
            get { return _graphManager; }
        }
        private InspectManager InspectManager
        {
            get { return _inspectManager; }
        }

        public bool BusinessLinkClikThrough {
            get { return _businessLinkClikThrough; }
            set { _businessLinkClikThrough = value; }
        }

        public CentricBrowser()
        {
            InitializeComponent();

            _annotationManager = new AnnotationManager();
            _searchManager = new SearchManager();
            _graphManager = new GraphManager();
            _inspectManager = new InspectManager();

        }

        public void Init(ServiceHelper servicehelpser, int elementId, Guid projectId)
        {
            _serviceHelper = servicehelpser;
            _projectId = projectId;
            elementView.SetServiceHelper(_serviceHelper, projectId);
            elementView.OpenPivotButtonClick += (o, e) =>
            {
                if (OpenPivotButtonClick != null)
                {
                    OpenPivotButtonClick(o, e);
                }
            };

            LoadElement(elementId);
            
            elementView.BusinessViewLinkClicked += ElementView_BusinessViewLinkClicked;
        }

        private void LoadElement(int elementId, bool selectElement = false)
        {
            _centralElementId = elementId;

            // get the node
            var nodeId = GraphManager.GetGraphNodeId(_centralElementId, DependencyGraphKind.DataFlow);
            var nodeExtended = InspectManager.GetGraphNodeExtended(nodeId);

            // find out the detail level of the node - low / meium / high ~ 3 / 2 / 1
            _currentDetailLevel = SetViewDetailLevel(nodeExtended);
            _detailLevel = _currentDetailLevel;
            BIDocGraphInfoNodeExtended levelNode = nodeExtended;
            int levelNodeId = nodeId;

            // for higheer level nodes, use the corresponding graph
            if (_currentDetailLevel == 2)
            {
                levelNodeId = GraphManager.GetGraphNodeId(_centralElementId, DependencyGraphKind.DataFlowMediumDetail);
                levelNode = InspectManager.GetGraphNodeExtended(levelNodeId);
            }
            else if (_currentDetailLevel == 3)
            {
                levelNodeId = GraphManager.GetGraphNodeId(_centralElementId, DependencyGraphKind.DataFlowLowDetail);
                levelNode = InspectManager.GetGraphNodeExtended(levelNodeId);
            }
            _centralNode = levelNode;
            RenderDiagram(new List<BIDocGraphInfoNodeExtended>() { levelNode }, new List<BIDocGraphInfoLink>());

            Diagram d = new Diagram();
            DiagramNodeEventArgs diagramNodeEventArgs = new DiagramNodeEventArgs();

            BIDocGraphInfoNodeExtended value = new BIDocGraphInfoNodeExtended();
            Dictionary<int, BIDocGraphInfoNodeExtended>.ValueCollection values = _nodeDictionary.Values;
            foreach (BIDocGraphInfoNodeExtended val in values)
            {
                value = val;
            }
            if (values.Count == 1)
            {
                diagramNodeEventArgs.Node = new DiagramNode(value.Id, d, nodeExtended.Name, (nodeExtended.Description == null ? nodeExtended.TypeDescription : nodeExtended.Description));
                if (selectElement)
                {
                    Diagram_NodeSelected(nodeId, diagramNodeEventArgs);
                }
            }

            elementView.LoadElement(elementId);
        }

        private void ElementView_BusinessViewLinkClicked(object sender, ElementView.BusinessViewLinkClickedArgs e)
        {
            if (_businessLinkClikThrough)
            {
                LoadElement(e.LinkedElementId/*, true*/);
            }

            if (BusinessViewLinkClicked != null)
            {
                BusinessViewLinkClicked(this, e);
            }
        }

        private void Diagram_NodeSelected(object sender, DiagramNodeEventArgs e)
        {
            var node = _nodeDictionary[e.Node.Id];
            elementView.LoadElement(node.SourceElementId);
            ToggleImpactButton.IsEnabled = true;
            ToggleLineageButton.IsEnabled = true;
            if (_nodesWithDisplayedIncomingLinks.Contains(node.Id))
            {
                ToggleLineageButton.Content = "Hide Lineage";
            }
            else
            {
                ToggleLineageButton.Content = "Show Lineage";
            }

            if (_nodesWithDisplayedOutcomingLinks.Contains(node.Id))
            {
                ToggleImpactButton.Content = "Hide Impact";
            }
            else
            {
                ToggleImpactButton.Content = "Show Impact";
            }
        }

        private void Diagram_NodeRightClick(object sender, DiagramNodeEventArgs e)
        {
            ContextMenu cm = this.FindResource("nodeContextMenu") as ContextMenu;
            var border = (Border)sender;
            var shape = (TextBlock)(border.Child);
            cm.PlacementTarget = shape;
            _rightClickedNode = e.Node;
            cm.IsOpen = true;
        }

        private int SetViewDetailLevel(BIDocGraphInfoNodeExtended node)
        {
            var detailLevel = InspectManager.GetElementTypeDetailLevel(node.ElementType);
            switch (detailLevel)
            {
                case 1: HighDetailComboItem.IsSelected = true;
                    break;
                case 2: MediumDetailComboItem.IsSelected = true;
                    break;
                case 3: LowDetailComboItem.IsSelected = true;
                    break;
                default: throw new Exception(string.Format("Unexpected detail level {0}", detailLevel));
            }
            return detailLevel;
        }

        private void CMShowLineage_Click(object sender, RoutedEventArgs e)
        {
            ShowNodeLineage(_rightClickedNode.Id);
        }

        private void CmHideLineage_Click(object sender, RoutedEventArgs e)
        {
            HideNodeLineage(_rightClickedNode.Id);
        }

        private void CmShowImpact_Click(object sender, RoutedEventArgs e)
        {
            ShowNodeImpact(_rightClickedNode.Id);
        }

        private void CmHideImpact_Click(object sender, RoutedEventArgs e)
        {
            HideNodeImpact(_rightClickedNode.Id);
        }

        private void ToggleImpactButton_Click(object sender, RoutedEventArgs e)
        {
            if (_nodesWithDisplayedOutcomingLinks.Contains(_diagram.SelectedNode.Id))
            {
                HideNodeImpact(_diagram.SelectedNode.Id);
            }
            else
            {
                ShowNodeImpact(_diagram.SelectedNode.Id);
            }
        }
        
        private void ToggleLineageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_nodesWithDisplayedIncomingLinks.Contains(_diagram.SelectedNode.Id))
            {
                HideNodeLineage(_diagram.SelectedNode.Id);
            }
            else
            {
                ShowNodeLineage(_diagram.SelectedNode.Id);
            }
        }
        
        private void HideNodeImpact(int nodeId)
        {
            SetDisplayedLinksFromTranslatedLinks();
            ToggleImpactButton.Content = "Show Impact";
            _nodesWithDisplayedOutcomingLinks.Remove(nodeId);

            var linkToRemove = _displayedLinks.FirstOrDefault(l => l.NodeFromId == nodeId);
            while (linkToRemove != null)
            {
                // the target nodes of the outcoming links have no longer their full lineage displayed
                _nodesWithDisplayedIncomingLinks.Remove(linkToRemove.NodeToId);
                _displayedLinks.Remove(linkToRemove);

                _diagram.RemoveLink(linkToRemove.Id);
                // if the removed link was the only link of the impacted node, remove that node as well
                var nodeTo = _diagram.GetNode(linkToRemove.NodeToId);
                if (!nodeTo.Links.Any())
                {
                    _nodeDictionary.Remove(nodeTo.Id);
                    _diagram.RemoveNode(nodeTo);
                }

                linkToRemove = _displayedLinks.FirstOrDefault(l => l.NodeFromId == nodeId);
            }

            UpdateDiagram();
        }

        private void ShowNodeImpact(int nodeId)
        {
            SetDisplayedLinksFromTranslatedLinks();
            ToggleImpactButton.Content = "Hide Impact";
            _nodesWithDisplayedOutcomingLinks.Add(nodeId);

            var linksFrom = InspectManager.GetDataFlowLinksFromNode(_projectId, nodeId);
            var newTargetNodesToDisplay = linksFrom.Where(x => !_nodeDictionary.ContainsKey(x.NodeToId)).Select(x => x.NodeToId).Distinct().ToList();
            var nodesToDisplay = InspectManager.GetNodesExtended(newTargetNodesToDisplay);
            var sourceDiagramNode = _diagram.GetNode(nodeId);
            foreach (var nodeToDisplay in nodesToDisplay)
            {
                _diagram.AddNode(nodeToDisplay.Id, nodeToDisplay.Name, nodeToDisplay.TypeDescription);
                _nodeDictionary.Add(nodeToDisplay.Id, nodeToDisplay);
            }

            foreach (var link in linksFrom)
            {
                if (_displayedLinks.Any(x => x.Id == link.Id))
                {
                    continue;
                }
                _diagram.AddLink(link.Id, _diagram.GetNode(link.NodeFromId), _diagram.GetNode(link.NodeToId), 1);
                _displayedLinks.Add(link);
            }

            UpdateDiagram();
        }

        private void HideNodeLineage(int nodeId)
        {
            bool clickHide = true;
            SetDisplayedLinksFromTranslatedLinks();
            ToggleLineageButton.Content = "Show Lineage";
            _nodesWithDisplayedIncomingLinks.Remove(nodeId);

            var linkToRemove = _displayedLinks.FirstOrDefault(l => l.NodeToId == nodeId);
            while (linkToRemove != null)
            {
                // the source nodes of the incoming links have no longer their full impact displayed
                _nodesWithDisplayedOutcomingLinks.Remove(linkToRemove.NodeFromId);
                _displayedLinks.Remove(linkToRemove);

                _diagram.RemoveLink(linkToRemove.Id);
                // if the removed link was the only link of the source node, remove that node as well
                var nodeFrom = _diagram.GetNode(linkToRemove.NodeFromId);
                if (!nodeFrom.Links.Any())
                {
                    _nodeDictionary.Remove(nodeFrom.Id);
                    _diagram.RemoveNode(nodeFrom);
                }

                linkToRemove = _displayedLinks.FirstOrDefault(l => l.NodeToId == nodeId);
            }
            
            UpdateDiagram(clickHide);
        }

        private void ShowNodeLineage(int nodeId)
        {
            SetDisplayedLinksFromTranslatedLinks();
            ToggleLineageButton.Content = "Hide Lineage";
            _nodesWithDisplayedIncomingLinks.Add(nodeId);

            var linksTo = InspectManager.GetDataFlowLinksToNode(_projectId, nodeId);
            var newTargetNodesToDisplay = linksTo.Where(x => !_nodeDictionary.ContainsKey(x.NodeFromId)).Select(x => x.NodeFromId).Distinct().ToList();
            var nodesToDisplay = InspectManager.GetNodesExtended(newTargetNodesToDisplay);
            var sourceDiagramNode = _diagram.GetNode(nodeId);
            foreach (var nodeToDisplay in nodesToDisplay)
            {
                _diagram.AddNode(nodeToDisplay.Id, nodeToDisplay.Name, nodeToDisplay.TypeDescription);
                _nodeDictionary.Add(nodeToDisplay.Id, nodeToDisplay);
            }

            foreach (var link in linksTo)
            {
                if (_displayedLinks.Any(x => x.Id == link.Id))
                {
                    continue;
                }
                _diagram.AddLink(link.Id, _diagram.GetNode(link.NodeFromId), _diagram.GetNode(link.NodeToId), 1);
                _displayedLinks.Add(link);
            }

            UpdateDiagram();

        }

        private void SetDisplayedLinksFromTranslatedLinks()
        {
            if (_translatedDisplayedLinks != null)
            {
                _displayedLinks = _translatedDisplayedLinks;
                _translatedDisplayedLinks = null;
            }
        }
        
        private void DetailLevelCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboItem = e.AddedItems[0] as ComboBoxItem;
            // component not initialized yet
            if (_currentDetailLevel == 0 || comboItem == null)
            {
                return;
            }
            int newDetailLevel = 0;
            if (comboItem == HighDetailComboItem)
            {
                newDetailLevel = 1;
            }
            else if (comboItem == MediumDetailComboItem)
            {
                newDetailLevel = 2;
            }
            else if (comboItem == LowDetailComboItem)
            {
                newDetailLevel = 3;
            }
            else
            {
                throw new Exception("Unexpected detail level selected: " + comboItem.Content.ToString());
            }

            if (_displayedLinks.Count == 0)
            {
                TranslateNodeDetail(_detailLevel, newDetailLevel);
            }
            else
            {
                TranslateLinkDetail(_detailLevel, newDetailLevel);
            }
        }

        private void TranslateLinkDetail(int currentDetailLevel, int newDetailLevel)
        {
            _oldLink = new List<BIDocGraphInfoLink>();
            foreach (var link in _displayedLinks)
            {
                _oldLink.Add(new BIDocGraphInfoLink() { Id = link.Id, ExtendedProperties = link.ExtendedProperties,
                    LinkType = link.LinkType, NodeFromId = link.NodeFromId, NodeToId = link.NodeToId, ProjectConfigId = link.ProjectConfigId });
            }

            var linkIds = _displayedLinks.Select(x => x.Id).ToList();
            var translatedLinks = InspectManager.TranslateDataFlowLinksDetailLevel(_projectId, linkIds, currentDetailLevel, newDetailLevel);
            var nodeIds = translatedLinks.Select(x => x.NodeFromId).Union(translatedLinks.Select(y => y.NodeToId)).Distinct().ToList();
            var translatedNodes = InspectManager.GetNodesExtended(nodeIds);

            RenderDiagram(translatedNodes, translatedLinks);
            _translatedDisplayedLinks = translatedLinks;
            _currentDetailLevel = newDetailLevel;
            _displayedLinks = _oldLink;
        }

        private void TranslateNodeDetail(int currentDetailLevel, int newDetailLevel)
        {
            List<BIDocGraphInfoNodeExtended> nodes = new List<BIDocGraphInfoNodeExtended>();
            nodes = InspectManager.TranslateDataFlowLNodeDetailLevel(_centralNode.Id, currentDetailLevel, newDetailLevel);
                
            RenderDiagram(nodes, new List<BIDocGraphInfoLink>());
            _currentDetailLevel = newDetailLevel;
        }

        private void RenderDiagram(List<BIDocGraphInfoNodeExtended> nodes, List<BIDocGraphInfoLink> links)
        {
            _nodeDictionary.Clear();
            _displayedLinks.Clear();
            ResetShowHideNodeLinks();

            foreach (var node in nodes)
            {
                _nodeDictionary.Add(node.Id, node);
            }
            foreach (var link in links)
            {
                _displayedLinks.Add(link);
            }

            if (_diagram != null)
            {
                _diagram.NodeSelected -= Diagram_NodeSelected;
                _diagram.NodeRightClick -= Diagram_NodeRightClick;
                _diagram.NodeDoubleClick -= Diagram_NodeDoubleClick;
            }

            var localNodeDictionary = new Dictionary<int, DiagramNode>();

            _diagram = new Diagram();
            _diagram.SelectableNodes = true;
            _diagram.NodeSelected += Diagram_NodeSelected;
            _diagram.NodeRightClick += Diagram_NodeRightClick;
            _diagram.NodeDoubleClick += Diagram_NodeDoubleClick;
            
            foreach (var node in nodes)
            {
                var dgn = _diagram.AddNode(node.Id, node.Name, node.TypeDescription);
                localNodeDictionary[node.Id] = dgn;
            }
            foreach (var link in links)
            {
                _diagram.AddLink(link.Id, localNodeDictionary[link.NodeFromId], localNodeDictionary[link.NodeToId], 1);
            }
            
            _diagram.ArrangeDiagram(Diagram.DiagramArrangementDirection.Vertical);
            _diagram.Render();
            diagramViewer.Content = _diagram.Canvas;
            
            // TODO center to central element
        }

        private void Diagram_NodeDoubleClick(object sender, DiagramNodeEventArgs e)
        {
            ShowNodeLineage(e.Node.Id);
        }

        private void UpdateDiagram(bool clickHide = false)
        {
            _diagram.ArrangeDiagram(Diagram.DiagramArrangementDirection.Vertical, clickHide);
            _diagram.Render();
        }

        private void ResetShowHideNodeLinks()
        {
            _nodesWithDisplayedIncomingLinks = new HashSet<int>();
            _nodesWithDisplayedOutcomingLinks = new HashSet<int>();
            if (ToggleImpactButton != null)
            {
                ToggleImpactButton.IsEnabled = false;
                ToggleLineageButton.IsEnabled = false;
            }
        }
    }
}
