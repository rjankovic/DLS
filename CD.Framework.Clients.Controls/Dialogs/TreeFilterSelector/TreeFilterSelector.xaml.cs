using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Inspect;
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

namespace CD.DLS.Clients.Controls.Dialogs.TreeFilterSelector
{

    public class TreeFilterEventArgs : EventArgs
    {
        public string SelectedRefPath { get; set; }
        public int SelectedElementId { get; set; }
        public ElementTypeDescription SelectedType {get; set;}
    }

    public delegate void TreeFilterEventHandler(object sender, TreeFilterEventArgs e);

    /// <summary>
    /// Interaction logic for TreeFilterSelector.xaml
    /// </summary>
    public partial class TreeFilterSelector : UserControl
    {
        public TreeFilterSelector()
        {
            InitializeComponent();
        }
        
        public TreeFilterSelector(ProjectConfig config, IReceiver receiver, bool sync = false)
        {
            InitializeComponent();

            _config = config;
            _receiver = receiver;
            rootSelector.LoadData(_config, sync);
            rootSelector.SelectionChanged += RootSelector_SelectionChanged;
            typeSelector.SelectionChanged += TypeSelector_SelectionChanged;

            var receiverIdConfigValue = ConfigManager.ServiceReceiverId;
            _serviceReceiverId = receiverIdConfigValue;
        }

        public event TreeFilterEventHandler OkButtonClicked;
        public event TreeFilterEventHandler CancelButtonClicked;

        private ProjectConfig _config;
        private IReceiver _receiver;
        private Guid _serviceReceiverId = Guid.Empty;
        
        private void RootSelector_SelectionChanged(object sender, EventArgs e)
        {
            typeSelectorTab.IsEnabled = rootSelector.SourceSelected;
            if (rootSelector.SourceSelected)
            {
                typeSelector.LoadData(_config, rootSelector.SourceSelectedElementId.Value);
                NextButton.IsEnabled = true;
                OkButton.IsEnabled = false;
            }
        }

        private void TypeSelector_SelectionChanged(object sender, EventArgs e)
        {
            if (typeSelector.SourceSelected)
            {
                OkButton.IsEnabled = true;

            }
        }
        

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedItem = rootSelectorTab;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedItem = typeSelectorTab;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Window wnd = (Window)this.Parent;
            
            if (OkButtonClicked != null)
            {
                OkButtonClicked(this, new TreeFilterEventArgs() {
                    SelectedRefPath = rootSelector.SourceSelectedElementPath,
                    SelectedElementId = rootSelector.SourceSelectedElementId.Value,
                    SelectedType = typeSelector.SourceType });
            }
            wnd.Close();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (CancelButtonClicked != null)
            {
                CancelButtonClicked(this, new TreeFilterEventArgs());
            }
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl == null)
            {
                return;
            }

            if (rootSelectorTab == null || typeSelectorTab == null)
            {
                return;
            }

            if (BackButton == null || NextButton == null)
            {
                return;
            }

            if (tabControl.SelectedItem == rootSelectorTab)
            {
                BackButton.IsEnabled = false;
                NextButton.IsEnabled = rootSelector.SourceSelected;
            }
            else if (tabControl.SelectedItem == typeSelectorTab)
            {
                BackButton.IsEnabled = true;
                NextButton.IsEnabled = false;
            }
        }
    }
}
