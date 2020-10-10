using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using CD.DLS.API;
using CD.DLS.API.Query;
using CD.DLS.Clients.Controls.Renderers;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Receiver;

namespace CD.DLS.Clients.Controls.Dialogs.SourceTargetSelector
{
    public class LineageDetailTaskSpec
    {
        public int SourceNodeId { get; set; }
        public int TargetNodeId { get; set; }
        private List<string> _statusLabel = new List<string>();
        
        public bool Equals(LineageDetailTaskSpec other)
        {
            if (other == null)
            {
                return false;
            }

            return
                (
                SourceNodeId == other.SourceNodeId
                && TargetNodeId == other.TargetNodeId
                );
        }
    }

    public partial class SourceTargetFlowDetail : UserControl
    {
        private ProjectConfig _config;
        private IReceiver _receiver = null;
        private Guid _serviceReceiverId = Guid.Empty;
        private string _sourceRefPath;
        private string _targetRefPath;
        private LineageDetailResponse _currentLineage;
        private Diagrams.Diagram _diagram;
        private Dictionary<int, NodeDescription> _nodeDictionary;
        private Dictionary<int, VisualNodeDescription> _visualNodeDictionary;

        private GraphManager _graphManager;
        private InspectManager _inspectManager;

        public event ElementView.BusinessViewLinkClickedHander BusinessViewLinkClicked;
        
        private GraphManager GraphManager
        {
            get { return _graphManager; }
        }
        private InspectManager InspectManager
        {
            get { return _inspectManager; }
        }

        public SourceTargetFlowDetail()
        {
            InitializeComponent();
            
        }

        public void SetServiceInterface(IReceiver receiver, Guid serviceReceiverId, ProjectConfig config)
        {
            _receiver = receiver;
            _serviceReceiverId = serviceReceiverId;
            _config = config;

            elementView.SetServiceHelper(new ServiceHelper(receiver, serviceReceiverId, config), config.ProjectConfigId);

            elementView.BusinessViewLinkClicked += ElementView_BusinessViewLinkClicked;
        }

        private void ElementView_BusinessViewLinkClicked(object sender, ElementView.BusinessViewLinkClickedArgs e)
        {
            if (BusinessViewLinkClicked != null)
            {
                BusinessViewLinkClicked(this, e);
            }
        }

        public void SetSourceAndTargetPaths(string sourceRefPath, string targetRefPath)
        {
            _sourceRefPath = sourceRefPath;
            _targetRefPath = targetRefPath;
            LoadData();
        }

        private void LoadData()
        {
            if (_graphManager == null)
            {
                _graphManager = new GraphManager();
                _inspectManager = new InspectManager();
            }

            if (_sourceRefPath == null || _targetRefPath == null)
            {
                return;
            }
            detailLevelCombo.IsEnabled = false;
            waitingPanel.Visibility = System.Windows.Visibility.Visible;
            var request = CreateEmptyRequest();
            ComboBoxItem cbi = (ComboBoxItem)(detailLevelCombo.SelectedValue);
            var detailLevel = (LineageDetailLevelEnum)Enum.Parse(typeof(LineageDetailLevelEnum), (string)cbi.Content);
            DLSApiMessage content = new LineageDetailRequest { SourceRefPath = _sourceRefPath, TargetRefPath = _targetRefPath, DetailLevel = detailLevel };
            request.Content = content.Serialize();
            var resHandle = _receiver.PostMessage(request);
            resHandle.ContinueWith((t) => { Dispatcher.Invoke(new Action<Task<RequestMessage>>(UpdateView), t); });
            var headerTask = GetStatusLabel();
            headerTask.ContinueWith((t) => { Dispatcher.Invoke(new Action<Task<List<string>>>(UpdateStatusLabel), t); });
        }

        private Task<List<string>> GetStatusLabel()
        {
            var task = Task.Factory.StartNew(() =>
               {
                   var sourceElementId = GraphManager.GetModelElementIdByRefPath(_config.ProjectConfigId, _sourceRefPath);
                   var targetElementId = GraphManager.GetModelElementIdByRefPath(_config.ProjectConfigId, _targetRefPath);
                   var sourceNodeDescriptivePath = GraphManager.GetModelElementDescriptivePath(sourceElementId);
                   var targetNodeDescriptivePath = GraphManager.GetModelElementDescriptivePath(targetElementId);
                   List<string> res = new List<string>();
                   res.Add("Source: " + sourceNodeDescriptivePath);
                   res.Add(">>");
                   res.Add("Target: " + targetNodeDescriptivePath);
                   return res;
               });
            return task;
        }

        private void UpdateStatusLabel(Task<List<string>> processingTask)
        {
            statusLabelLeft.Content = processingTask.Result[0];
            statusLabelCenter.Content = processingTask.Result[1];
            statusLabelRight.Content = processingTask.Result[2];
        }

        private void UpdateView(Task<RequestMessage> processingTask)
        {
            var res = processingTask.Result;
            if (res == null)
            {
                return;
            }
            var msg = DLSApiMessage.Deserialize(res.Content);
            var lineageResponse = (LineageDetailResponse)msg;
            if (lineageResponse.SourceRefPath != _sourceRefPath || lineageResponse.TargetRefPath != _targetRefPath)
            {
                return;
            }
            _currentLineage = lineageResponse;
            UpdateView();
        }

        private void UpdateView()
        {
            if (_sourceRefPath == null || _targetRefPath == null)
            {
                return;
            }
            waitingPanel.Visibility = System.Windows.Visibility.Visible;

            var lineage = _currentLineage;

            _diagram = null;
            _diagram = Diagrams.DiagramConstructor.Construct(_currentLineage.Nodes, _currentLineage.Links);
            _diagram.SelectableNodes = true;
            _diagram.NodeSelected += DiagramNodeSelected;
            _diagram.ArrangeDiagram(Diagrams.Diagram.DiagramArrangementDirection.Vertical);
            _diagram.Render();
            diagramViewer.Content = _diagram.Canvas;

            //definitionGrid.Children.Clear();
            elementView.Clear();

            elementView.CopyButtonIsCopy();

            _nodeDictionary = lineage.Nodes.ToDictionary(x => x.NodeId, x => x);
            //_visualNodeDictionary = lineage.VisualAncestors.ToDictionary(x => x.NodeId, x => x);

            
            waitingPanel.Visibility = System.Windows.Visibility.Hidden;
            detailLevelCombo.IsEnabled = true;
        }

        private void DiagramNodeSelected(object sender, Diagrams.DiagramNodeEventArgs e)
        {
            var node = _nodeDictionary[e.Node.Id];
            elementView.LoadElement(node.ModelElementId);

            //definitionGrid.Children.Clear();
            //refPathBox.Text = node.RefPath;

            //if (node is VisualPartNodeDescription)
            //{
            //    var visualPartNodeDesc = (VisualPartNodeDescription)node;
            //    var visualNode = _visualNodeDictionary[visualPartNodeDesc.VisualNodeId];
            //    var definitionView = VisualPartNodeRenderer.Render(visualPartNodeDesc, visualNode);
            //    if (definitionView != null)
            //    {
            //        definitionGrid.Children.Add(definitionView);
            //    }
            //}
        }

        private void DetailSelectionChanged(object sender, EventArgs args)
        {
            LoadData();
        }

        private void ClassFilterSelectionChanged(object sender, EventArgs args)
        {
            UpdateView();
        }

        private RequestMessage CreateEmptyRequest()
        {
            var msg = Helpers.CreateRequest(_receiver, _config.ProjectConfigId);
            msg.MessageToObjectId = _serviceReceiverId;
            msg.RequestForCoreType = Common.Interfaces.CoreTypeEnum.BIDoc;
            return msg;
        }
    }
}
