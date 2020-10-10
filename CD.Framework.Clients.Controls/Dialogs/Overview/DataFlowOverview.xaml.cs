using CD.DLS.DAL.Objects.BIDoc;
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
using CD.DLS.Clients.Controls.Diagrams;
using CD.DLS.Common.Structures;

namespace CD.DLS.Clients.Controls.Dialogs.Overview
{
    /// <summary>
    /// Interaction logic for DataFlowOverview.xaml
    /// </summary>
    public partial class DataFlowOverview : UserControl
    {
        private Guid _projectConfigId;
        private Diagram _diagram;

        public DataFlowOverview()
        {
            InitializeComponent();
        }

        public void LoadData(Guid projectConfigId)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            _projectConfigId = projectConfigId;
            _diagram = DiagramLoader.LoadDiagram(_projectConfigId, DependencyGraphKind.DFHigh);
            _diagram.ArrangeDiagram(Diagram.DiagramArrangementDirection.Horizontal);
            _diagram.Render();
            scrollViewer.Content = _diagram.Canvas;
            Mouse.OverrideCursor = Cursors.Arrow;
        }
        
        private void levelCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
