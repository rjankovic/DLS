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
using System.Windows.Shapes;
using CD.DLS.API;
using CD.DLS.API.Query;
using CD.DLS.Clients.Controls.Dialogs;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BIDoc;
using CD.DLS.DAL.Objects.Inspect;
using CD.DLS.DAL.Receiver;

namespace CD.DLS.Clients.Controls.Dialogs.SourceTargetSelector
{
    /// <summary>
    /// Interaction logic for AnnotationList.xaml
    /// </summary>
    public partial class SourceTargetSelector : UserControl
    {
        private ProjectConfig _config;
        private IReceiver _receiver;
        private DataFlowBetweenGroupsItem _currentDetailItem = null;
        private Guid _serviceReceiverId = Guid.Empty;
        ElementView.ElementView _elementView = new ElementView.ElementView();
        private bool _jumpBack = false;

        public event ElementView.BusinessViewLinkClickedHander BusinessViewLinkClicked;


        public SourceTargetSelector(ProjectConfig config, IReceiver receiver, bool sync = false)
        {
            InitializeComponent();

            _config = config;
            _receiver = receiver;
            rootSelector.LoadData(_config, sync);
            favorities.LoadData(_config);
            rootSelector.SelectionChanged += RootSelector_SelectionChanged;
            typeSelector.SelectionChanged += TypeSelector_SelectionChanged;
            lineageGrid.SelectionChanged += LineageGrid_SelectionChanged;
            visualTargetSelector.SelectionChanged += VisualTargetSelector_SelectionChanged;
            favorities.SelectionChanged += Favorities_SelectionChanged;
            favoritiesTab.GotFocus += FavoritiesTab_GotFocus;

            var receiverIdConfigValue = ConfigManager.ServiceReceiverId;
            _serviceReceiverId = receiverIdConfigValue;

            flowDetail.SetServiceInterface(receiver, _serviceReceiverId, config);
            flowDetail.BusinessViewLinkClicked += FlowDetail_BusinessViewLinkClicked;
        }

        private void FavoritiesTab_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_jumpBack)
            {
                _jumpBack = false;
                lineageTab.IsSelected = true;
            }

        }

        private void Favorities_SelectionChanged(object sender, EventArgs e)
        {
            var selectedItem = favorities.SelectedItem;
            if (selectedItem == null)
            {
                return;
            }
            typeSelector.PreselectSourceElementType(selectedItem.SourceElementType);
            typeSelector.PreselectTargetElementType(selectedItem.TargetElementType);
            rootSelector.SetSourceRootElementPath(selectedItem.SourceRootElementPath);
            rootSelector.SetTargetRootElementPath(selectedItem.TargetRootElementPath);
            _jumpBack = true;
            lineageTab.IsSelected = true;
            
        }

        private void FlowDetail_BusinessViewLinkClicked(object sender, ElementView.BusinessViewLinkClickedArgs e)
        {
            if (BusinessViewLinkClicked != null)
            {
                BusinessViewLinkClicked(sender, e);
            }
        }

        public void SetSourceRootElementId(string refPath)
        {
            rootSelector.SetSourceRootElementPath(refPath);
        }

        public void SetTargetRootElementId(string refPath)
        {
            rootSelector.SetTargetRootElementPath(refPath);
        }

        private void RootSelector_SelectionChanged(object sender, EventArgs e)
        {
            typeSelectorTab.IsEnabled = rootSelector.SourceAndTargetSelected;
            if (rootSelector.SourceAndTargetSelected)
            {
                typeSelector.LoadData(_config, rootSelector.SourceSelectedElementId.Value, rootSelector.TargetSelectedElementId.Value);
            }
            lineageTab.IsEnabled = false;
            visualTargetTab.IsEnabled = false;
            detailTab.IsEnabled = false;
        }

        private void TypeSelector_SelectionChanged(object sender, EventArgs e)
        {
            visualTargetTab.IsEnabled = false;
            lineageTab.IsEnabled = typeSelector.SourceAndTargetSelected;
            detailTab.IsEnabled = false;
            if (typeSelector.SourceAndTargetSelected)
            {
                LineageBetweenGoupsTaskSpec spec = new LineageBetweenGoupsTaskSpec()
                {
                    SourceElementId = rootSelector.SourceSelectedElementId.Value,
                    TargetElementId = rootSelector.TargetSelectedElementId.Value,
                    SourceElementRefPath = rootSelector.SourceSelectedElementPath,
                    TargetElementRefPath = rootSelector.TargetSelectedElementPath,
                    SourceNodeType = typeSelector.SourceType.NodeType,
                    TargetNodeType = typeSelector.TargetType.NodeType,
                    SourceElementType = typeSelector.SourceType.ElementType,
                    TargetElementType = typeSelector.TargetType.ElementType,
                    SourceNodeTypeDescription = typeSelector.SourceType.TypeDescription,
                    TargetNodeTypeDescription = typeSelector.TargetType.TypeDescription
                };
                var waitingTask = lineageGrid.LoadData(_config, spec);
                if (waitingTask == null)
                {
                    UpdateVisualTargetSelectorTab();
                }
                else
                {
                    waitingTask.ContinueWith(t => Dispatcher.InvokeAsync(UpdateVisualTargetSelectorTab));
                }
                
            }
        }

        private void UpdateVisualTargetSelectorTab()
        {
            visualTargetTab.IsEnabled = false;
            if (rootSelector.TargetSelectedElementType.Contains("ReportElement") && typeSelector.TargetType.NodeType == "TextBoxElement")
            {
                visualTargetTab.IsEnabled = true;
                var request = CreateEmptyRequest();
                DLSApiMessage content = new ReportItemPositionsRequest() { ReportElementId = rootSelector.TargetSelectedElementId.Value };
                request.Content = content.Serialize();
                var resHandle = _receiver.PostMessage(request);

                var mappedTargets = lineageGrid.CurrentDataFlow.Select(x => x.TargetElementId).Distinct().ToList();

                visualTargetSelector.LoadData(_config, resHandle, mappedTargets);
            }
        }

        private void LineageGrid_SelectionChanged(object sender, EventArgs e)
        {
            var selected = lineageGrid.SelectedItem is DataFlowBetweenGroupsItem;
            detailTab.IsEnabled = selected;
            if (!selected)
            {
                return;
            }
            if (FlowItemsEqualForLineageDetailRequest(_currentDetailItem, lineageGrid.SelectedItem))
            {
              return;
            }

            //flowDetail.SelectableTargets = lineageGrid.CurrentDataFlow.Select(x => x.TargetElementId).ToList();

            _currentDetailItem = lineageGrid.SelectedItem;
            var request = CreateEmptyRequest();
            //BIDocApiMessage content = new LineageDetailRequest { SourceRefPath = _currentDetailItem.SourceElementRefPath, TargetRefPath = _currentDetailItem.TargetElementRefPath };
            //request.Content = content.Serialize();
            //var resHandle = _receiver.PostMessage(request);
            flowDetail.SetSourceAndTargetPaths(_currentDetailItem.SourceElementRefPath, _currentDetailItem.TargetElementRefPath); // LoadData(_config, resHandle);
        }

        private void VisualTargetSelector_SelectionChanged(object sender, EventArgs e)
        {
            var selected = visualTargetSelector.TargetElementId.HasValue;
            detailTab.IsEnabled = selected;
            if (!selected)
            {
                return;
            }
            var newDetailsItem = new DataFlowBetweenGroupsItem()
            {
                SourceElementRefPath = rootSelector.SourceSelectedElementPath,
                TargetElementRefPath = visualTargetSelector.TargetNodePath
            };

            if (FlowItemsEqualForLineageDetailRequest(_currentDetailItem, newDetailsItem))
            {
                return;
            }

            //flowDetail.SelectableTargets = lineageGrid.CurrentDataFlow.Select(x => x.TargetElementId).ToList();


            _currentDetailItem = newDetailsItem;
            var request = CreateEmptyRequest();
            //BIDocApiMessage content = new LineageDetailRequest { SourceRefPath = _currentDetailItem.SourceElementRefPath, TargetRefPath = _currentDetailItem.TargetElementRefPath };
            //request.Content = content.Serialize();
            //var resHandle = _receiver.PostMessage(request);
            flowDetail.SetSourceAndTargetPaths(_currentDetailItem.SourceElementRefPath, _currentDetailItem.TargetElementRefPath); // .LoadData(_config, resHandle);
        }

        private RequestMessage CreateEmptyRequest()
        {
            var msg = Helpers.CreateRequest(_receiver, _config.ProjectConfigId);
            msg.MessageToObjectId = _serviceReceiverId;
            msg.RequestForCoreType = Common.Interfaces.CoreTypeEnum.BIDoc;
            return msg;
        }

        private bool FlowItemsEqualForLineageDetailRequest(DataFlowBetweenGroupsItem a, DataFlowBetweenGroupsItem b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            return a.SourceElementRefPath == b.SourceElementRefPath && a.TargetElementRefPath == b.TargetElementRefPath;
        }
    }
}
