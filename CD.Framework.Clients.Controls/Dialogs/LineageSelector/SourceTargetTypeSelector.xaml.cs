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
using CD.Framework.Clients.Controls.Dialogs;
using CD.Framework.Common.Structures;
using CD.Framework.ORM.Managers;

namespace CD.Framework.Clients.Controls.Dialogs.LineageSelector
{
    /// <summary>
    /// Interaction logic for AnnotationList.xaml
    /// </summary>
    public partial class SourceTargetRootSelector : UserControl
    {
        ProjectConfig _config;
        int _viewId;
        List<AnnotationViewField> _fields;
        List<AnnotationViewFieldValue> _values;
        List<DataGridField> _gridFields;
        List<DataGridFieldValue> _gridBaseValues;
        Dictionary<int, Dictionary<int, DataGridFieldValue>> _gridBaseValuesByElementAndField;

        public TreeNode SourceElement { get { return sourceRecursiveTree.SelectedItem; } }
        public TreeNode TargetElement { get { return targetRecursiveTree.SelectedItem; } }
        
        public SourceTargetRootSelector(ProjectConfig config, int viewId)
        {
            InitializeComponent();
            _viewId = viewId;
            _config = config;
            var highLevelTree = InspectManager.GetHighLevelSolutionTree(_config.ProjectConfigId);
            var sourceTreeItems = highLevelTree.Select(x => new Clients.Controls.Dialogs.TreeNode() { Id = x.ModelElementId, Name = "[" + x.TypeDescription + "] " + x.Caption, ParentId = x.ParentElementId }).ToList();
            sourceRecursiveTree.SetData(sourceTreeItems);
            var targetTreeItems = highLevelTree.Select(x => new Clients.Controls.Dialogs.TreeNode() { Id = x.ModelElementId, Name = "[" + x.TypeDescription + "] " + x.Caption, ParentId = x.ParentElementId }).ToList();
            targetRecursiveTree.SetData(targetTreeItems);
        }
        
    }
}
