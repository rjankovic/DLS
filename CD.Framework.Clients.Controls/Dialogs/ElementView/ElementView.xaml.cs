using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Managers;
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

namespace CD.DLS.Clients.Controls.Dialogs.ElementView
{
    public class ElementViewEventArgs : EventArgs
    {
        public BIDocModelElement Element { get; set; }
    }

    public delegate void ElementViewEventHandler(object sender, ElementViewEventArgs e);

    /// <summary>
    /// Interaction logic for ElementView.xaml
    /// </summary>
    public partial class ElementView : UserControl
    {
        private DAL.Receiver.ServiceHelper _serviceHelper;
        private int _currentElementId = -1;
        private BIDocModelElement _currentElement = null;
        private Guid _projectId;
        private string _copiedRefPath = null;
        
        public event BusinessViewLinkClickedHander BusinessViewLinkClicked;
        public event ElementViewEventHandler OpenPivotButtonClick;

        private GraphManager _graphManager;
        private List<SecurityManager.SecurityRolePermission> _permissions;

        private GraphManager GraphManager
        {
            get { return _graphManager; }
        }
        
        public ElementView()
        {
            InitializeComponent();
            businessView.BusinessViewLinkClicked += BusinessView_BusinessViewLinkClicked;
        }

        private void BusinessView_BusinessViewLinkClicked(object sender, BusinessViewLinkClickedArgs e)
        {
            if (BusinessViewLinkClicked != null)
            {
                BusinessViewLinkClicked(sender, e);
            }
        }

        public void SetServiceHelper(DAL.Receiver.ServiceHelper serviceHelper, Guid projectConfigId)
        {
            _serviceHelper = serviceHelper;
            _projectId = projectConfigId;
            var securityManager = new SecurityManager();
            _permissions = securityManager.UserPermissions(_projectId);
            techView.SetServiceHelper(serviceHelper);
            businessView.Editable = _permissions.Any(x => x.Type == DAL.Security.PermissionEnum.EditAnnotations);
            
        }

        public void LoadElement(int elementId)
        {
            if (_graphManager == null)
            {
                _graphManager = new GraphManager();
            }

            if (_currentElementId == elementId)
            {
                return;
            }

            _currentElementId = elementId;
            _currentElement = GraphManager.GetModelElementById(elementId);
            businessView.GetViewFields(_projectId, _currentElement.Type);

            techView.LoadElement(elementId);
            dataView.LoadData(elementId);
            businessView.LoadElement(elementId);

            string text = _currentElement.RefPath.ToString();
            text = text.Replace("[@Name='", ": ");
            text = text.Replace("']/", "" + System.Environment.NewLine);
            text = text.Replace("']", "");
            refPathBox.Text = text;

            refPathOrigBox.Text = _currentElement.RefPath;
            refPathCopyButton.Content = "Copy";

            var customView = GetCustomView();
            if (customView != null)
            {
                customViewTab.Content = customView;
                customViewTab.Visibility = Visibility.Visible;
                customViewTab.IsSelected = true;
            }
            else
            {
                if (customViewTab.IsSelected)
                {
                    techViewTab.IsSelected = true;
                }

                customViewTab.Content = null;
                customViewTab.Visibility = Visibility.Collapsed;
            }
        }

        private Control GetCustomView()
        {
            if (_currentElement.Type.EndsWith("PivotTableTemplateElement") && ConfigManager.ClientClass == ClientClassEnum.Excel)
            {
                PivotTableTemplateCustomView pivotView = new PivotTableTemplateCustomView();
                pivotView.OpenPivotButtonClick += (o, e) =>
                {
                    if (OpenPivotButtonClick != null)
                    {
                        OpenPivotButtonClick(o, new ElementViewEventArgs()
                        {
                            Element = _currentElement
                        });
                    }
                };
                return pivotView;
            }
            else
            {
                return null;
            }
        }


        public void Clear()
        {
            techView.Clear();
        }

        private void refPathCopyButton_Click(object sender, RoutedEventArgs e)
        {
            if(_currentElement == null)
            {

            }
            else
            {
                Clipboard.SetText(_currentElement.RefPath);
                refPathCopyButton.Content = "Copied";
                _copiedRefPath = _currentElement.RefPath;
            }
        }

        public void CopyButtonIsCopy()
        {
            refPathCopyButton.Content = "Copy";
            refPathOrigBox.Text = "";
        }
    }
}
