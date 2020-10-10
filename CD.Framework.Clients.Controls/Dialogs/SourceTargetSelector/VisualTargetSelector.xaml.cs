using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Shapes;
using CD.DLS.API;
using CD.DLS.API.Query;
using CD.DLS.API.Structures;
using CD.DLS.Clients.Controls.Renderers;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Inspect;
using CD.DLS.DAL.Objects.SsisDiagram;

namespace CD.DLS.Clients.Controls.Dialogs.SourceTargetSelector
{
    public partial class VisualTargetSelector : UserControl
    {

        public int? TargetElementId { get; set; }
        public string TargetNodeType { get; set; }
        public string TargetNodePath { get; set; }

        private ProjectConfig _config;
        private List<int> _selectableItems = new List<int>();

        public event EventHandler SelectionChanged;

        public VisualTargetSelector()
        {
            InitializeComponent();
        }

        public void LoadData(ProjectConfig config, Task<RequestMessage> loadingTask, List<int> selectableItems)
        {
            TargetElementId = null;
            TargetNodeType = null;
            TargetNodePath = null;
            _selectableItems = selectableItems;
            waitingPanel.Visibility = System.Windows.Visibility.Visible;
            loadingTask.ContinueWith((t) => { Dispatcher.Invoke(new Action<Task<RequestMessage>>(UpdateView), t); });
        }
        
        private void UpdateView(Task<RequestMessage> processingTask)
        {
            var res = processingTask.Result;
            if (res == null)
            {
                return;
            }
            var msg = DLSApiMessage.Deserialize(res.Content);
            var positions = (ReportItemPositionsResponse)msg;
            ReportLayoutRenderer renderer = new ReportLayoutRenderer(_selectableItems);
            var reportCanvas = renderer.DrawReportCanvas(positions.RootElement);
            var scrollViewer = new ScrollViewer();
            scrollViewer.Content = reportCanvas;

            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            renderer.CanvasItemSelected += Renderer_CanvasItemSelected;

            waitingPanel.Visibility = System.Windows.Visibility.Hidden;
            Grid.SetRow(scrollViewer, 0);
            Grid.SetColumn(scrollViewer, 0);
            //selectorGrid.Children.Clear();
            for (int i = 0; i < selectorGrid.Children.Count; i++)
            {
                var ch = selectorGrid.Children[i];
                if (ch is ScrollViewer)
                {
                    selectorGrid.Children.Remove(ch);
                    break;
                }
            }
            selectorGrid.Children.Add(scrollViewer);
            //selectorGrid.Children.Add(waitingPanel);
        }

        private void Renderer_CanvasItemSelected(object sender, ReportLayoutRenderer.CanvasItemArgs e)
        {
            TargetElementId = e.ElementId;
            TargetNodeType = e.Type;
            TargetNodePath = e.RefPath;
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new EventArgs());
            }
        }
    }
}
