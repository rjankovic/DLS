using CD.DLS.API;
using CD.DLS.API.Query;
using CD.DLS.Clients.Controls.Renderers;
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

namespace CD.DLS.Clients.Controls.Dialogs.ElementView
{
    /// <summary>
    /// Interaction logic for ElementTechView.xaml
    /// </summary>
    public partial class ElementTechView : UserControl
    {
        private DAL.Receiver.ServiceHelper _serviceHelper;
        // node that is being loaded
        private int _currentElementId = -1;
        // node that has been loaded
        private int _displayedElementId = -1;
        private ElementTechViewResponse _currentResponse = null;

        public ElementTechView()
        {
            InitializeComponent();
        }

        public void SetServiceHelper(DAL.Receiver.ServiceHelper serviceHelper)
        {
            _serviceHelper = serviceHelper;
        }
        
        public void LoadElement(int elementId)
        {
            infoPanel.Visibility = System.Windows.Visibility.Hidden;
            if (_currentElementId == elementId)
            {
                return;
            }

            _currentElementId = elementId;
            _displayedElementId = -1;

            waitingPanel.Visibility = Visibility.Visible;
            Clear();

            var techViewRequest = new ElementTechViewRequest() { ElementId = elementId };
            var resHandle = _serviceHelper.PostRequest<ElementTechViewResponse>(techViewRequest);
            
            resHandle.ContinueWith((t) => { Dispatcher.Invoke(new Action<Task<ElementTechViewResponse>>(UpdateView), t); });
        }

        private void UpdateView(Task<ElementTechViewResponse> respTask)
        {
            var resp = respTask.Result;

            // the currently displayed node already corresponds to the correct node, leave it
            if (_displayedElementId == _currentElementId)
            {
                return;
            }

            // this is not the element we're waiting for
            if (resp.RequestedElementId != _currentElementId)
            {
                return;
            }

            if (resp.ElementId != _currentElementId)
            {
                // if the node selection has been changed before this loading could finish,
                // this result is irrelevant
                if(resp.ElementId == 0)
                {
                    Clear();
                    waitingPanel.Visibility = Visibility.Hidden;
                    infoPanel.Visibility = Visibility.Visible;
                }
                return;
            }

            Clear();
            _currentResponse = resp;

            if (resp.NodeDescription is VisualPartNodeDescription)
            {
                var definitionView = VisualPartNodeRenderer.Render((VisualPartNodeDescription)(resp.NodeDescription), resp.VisualAncestor);
                if (definitionView != null)
                {
                    definitionGrid.Children.Add(definitionView);
                }
            }

            waitingPanel.Visibility = Visibility.Hidden;
            infoPanel.Visibility = Visibility.Hidden;
            _displayedElementId = resp.ElementId;
        }

        public void Clear()
        {
            definitionGrid.Children.Clear();
        }

    }
}
