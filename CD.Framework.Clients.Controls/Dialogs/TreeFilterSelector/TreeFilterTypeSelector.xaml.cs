using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Inspect;
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

namespace CD.DLS.Clients.Controls.Dialogs.TreeFilterSelector
{
    /// <summary>
    /// Interaction logic for TreeFilterTypeSelector.xaml
    /// </summary>
    public partial class TreeFilterTypeSelector : UserControl
    {

        private ProjectConfig _config;
        private int _sourceElementId;

        public event EventHandler SelectionChanged;

        public ElementTypeDescription SourceType { get { return sourceCombo.SelectedItem as ElementTypeDescription; } }
        public bool SourceSelected { get { return SourceType != null; } }


        private List<ElementTypeDescription> _sourceTypes = null;
        private List<ElementTypeDescription> _targetTypes = null;

        private GraphManager _graphManager;
        private InspectManager _inspectManager;
        private GraphManager GraphManager
        {
            get { return _graphManager; }
        }
        private InspectManager InspectManager
        {
            get { return _inspectManager; }
        }

        public TreeFilterTypeSelector()
        {
            InitializeComponent();
            sourceCombo.SelectionChanged += Combo_SelectionChanged;

            _graphManager = new GraphManager();
            _inspectManager = new InspectManager();
        }

        private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new EventArgs());
            }
        }

        public void LoadData(ProjectConfig config, int sourceRootElementId)
        {
            waitingPanel.Visibility = Visibility.Visible;
            _config = config;
            _sourceElementId = sourceRootElementId;

            var sourceNodeDescriptivePath = GraphManager.GetModelElementDescriptivePath(_sourceElementId);
            
            Task loadingTask = Task.Factory.StartNew(QueryData);
            loadingTask.ContinueWith((t) => { Dispatcher.Invoke(UpdateGui); });

        }

        private void UpdateGui()
        {
            sourceCombo.ItemsSource = _sourceTypes;
            sourceCombo.DataContext = _sourceTypes;

            waitingPanel.Visibility = Visibility.Hidden;
        }

        private void QueryData()
        {
            _sourceTypes = InspectManager.GetHighLevelTypesUnderElement(_config.ProjectConfigId, _sourceElementId);

        }
    }
}
