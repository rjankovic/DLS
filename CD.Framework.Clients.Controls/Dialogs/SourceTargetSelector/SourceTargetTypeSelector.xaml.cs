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
using CD.DLS.Clients.Controls.Dialogs;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Inspect;

namespace CD.DLS.Clients.Controls.Dialogs.SourceTargetSelector
{
    /// <summary>
    /// Interaction logic for AnnotationList.xaml
    /// </summary>
    public partial class SourceTargetTypeSelector : UserControl
    {
        private ProjectConfig _config;
        private int _sourceElementId;
        private int _targetElementId;
        private string _preselectedSourceElementType = null;
        private string _preselectedTargetElementType = null;

        public event EventHandler SelectionChanged;

        public ElementTypeDescription SourceType { get { return sourceCombo.SelectedItem as ElementTypeDescription; } }
        public ElementTypeDescription TargetType { get { return targetCombo.SelectedItem as ElementTypeDescription; } }
        public bool SourceAndTargetSelected { get { return SourceType != null && TargetType != null; } }


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

        public SourceTargetTypeSelector()
        {
            InitializeComponent();
            sourceCombo.SelectionChanged += Combo_SelectionChanged;
            targetCombo.SelectionChanged += Combo_SelectionChanged;
            
        }

        private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new EventArgs());
            }
        }

        public void LoadData(ProjectConfig config, int sourceRootElementId, int targetRootElementId)
        {
            if (_graphManager == null)
            {
                _graphManager = new GraphManager();
                _inspectManager = new InspectManager();
            }

            waitingPanel.Visibility = Visibility.Visible;
            _config = config;
            _sourceElementId = sourceRootElementId;
            _targetElementId = targetRootElementId;

            var sourceNodeDescriptivePath = GraphManager.GetModelElementDescriptivePath(_sourceElementId);
            var targetNodeDescriptivePath = GraphManager.GetModelElementDescriptivePath(_targetElementId);

            statusLabelLeft.Content = "Source: " + sourceNodeDescriptivePath;
            statusLabelRight.Content = "Target: " + targetNodeDescriptivePath;


            Task loadingTask = Task.Factory.StartNew(QueryData);
            loadingTask.ContinueWith((t) => { Dispatcher.Invoke(UpdateGui); });

        }

        private void UpdateGui()
        {
            sourceCombo.ItemsSource = _sourceTypes;
            targetCombo.ItemsSource = _targetTypes;
            sourceCombo.DataContext = _sourceTypes;
            targetCombo.DataContext = _targetTypes;

            if (_preselectedSourceElementType != null)
            {
                var selectedSourceItem = _sourceTypes.FirstOrDefault(x => x.ElementType == _preselectedSourceElementType);
                if (selectedSourceItem != null)
                {
                    sourceCombo.SelectedItem = selectedSourceItem;
                }
                //_preselectedSourceElementType = null;
            }

            if (_preselectedTargetElementType != null)
            {
                var selectedTargetItem = _targetTypes.FirstOrDefault(x => x.ElementType == _preselectedTargetElementType);
                if (selectedTargetItem != null)
                {
                    targetCombo.SelectedItem = selectedTargetItem;
                }
                //_preselectedTargetElementType = null;
            }

            waitingPanel.Visibility = Visibility.Hidden;
        }

        private void QueryData()
        {
            _sourceTypes = InspectManager.GetHighLevelTypesUnderElement(_config.ProjectConfigId, _sourceElementId);
            _targetTypes = InspectManager.GetHighLevelTypesUnderElement(_config.ProjectConfigId, _targetElementId);

            var ssasTypes = new List<string>() {
                "CD.DLS.Model.Mssql.Ssas.DimensionAttributeElement",
                "D.DLS.Model.Mssql.Ssas.PhysicalMeasureElement",
                "CD.DLS.Model.Mssql.Ssas.CubeCalculatedMeasureElement",
                "CD.DLS.Model.Mssql.Ssas.ReportCalculatedMeasureElement"
            };
            var ssasTypesConcat = string.Join(";", ssasTypes);
            var ssasNodeTypesConcat = string.Join(";", ssasTypes.Select(x => x.Substring(x.LastIndexOf('.') + 1)));
            var ssasTypesLabel = "SSAS Measure / Dimension";

            if (_sourceTypes.Any(x => ssasTypes.Contains(x.ElementType)))
            {
                _sourceTypes.Insert(0, new ElementTypeDescription()
                {
                    ElementType = ssasTypesConcat,
                    NodeType = ssasNodeTypesConcat,
                    TypeDescription = ssasTypesLabel
                });
            }

            if (_targetTypes.Any(x => ssasTypes.Contains(x.ElementType)))
            {
                _targetTypes.Insert(0, new ElementTypeDescription()
                {
                    ElementType = ssasTypesConcat,
                    NodeType = ssasNodeTypesConcat,
                    TypeDescription = ssasTypesLabel
                });
            }

            /*
(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.DimensionAttributeElement',N'DimensionAttributeElement'),
(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement',N'PhysicalMeasureElement'),
(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.CubeCalculatedMeasureElement',N'CubeCalculatedMeasureElement'),
(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.ReportCalculatedMeasureElement',N'ReportCalculatedMeasureElement')
             */

        }

        public void PreselectSourceElementType(string type)
        {
            _preselectedSourceElementType = type;
        }

        public void PreselectTargetElementType(string type)
        {
            _preselectedTargetElementType = type;
        }

    }
}
