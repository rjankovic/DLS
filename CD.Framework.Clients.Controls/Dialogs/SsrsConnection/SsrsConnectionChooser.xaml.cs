using CD.DLS.Common.Structures;
using CD.DLS.Common.Tools;
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

namespace CD.DLS.Clients.Controls.Dialogs.SsrsConnection
{

    public class SsrsProject
    {
        public string RootFolder { get; set; }
        public string ReportServerUrl { get; set; }
        public string ReportServiceUrl { get {
                //if (SsrsMode != SsrsModeEnum.Native)
                //{
                //    return null;
                //}
                return ReportServerUrl.TrimEnd('/') + "/reportservice2010.asmx";
            } }
        public string ExecutionServiceUrl { get
            {
                //if (SsrsMode != SsrsModeEnum.Native)
                //{
                //    return null;
                //}
                switch(SsrsMode)
                {
                    case SsrsModeEnum.SpIntegrated:
                        return ReportServerUrl.TrimEnd('/') + "/ReportExecution2005.asmx";
                    case SsrsModeEnum.Native:
                        return ReportServerUrl.TrimEnd('/') + "/ReportExecution2005.asmx";
                    default:
                        throw new Exception();
                }
                
            }
        }
        public string ServerName { get { return SsrsMode == SsrsModeEnum.Native ? WebTools.GetHost(ReportServerUrl) : WebTools.GetHost(SharePointBaseUrl); } }
        public SsrsModeEnum SsrsMode { get; set; }
        public string SharePointBaseUrl { get; set; }
        public string SharePointFolder { get; set; }
        public string SharePointFullUrl { get {
                if (SsrsMode != SsrsModeEnum.SpIntegrated)
                {
                    return null;
                }
                return SharePointBaseUrl.TrimEnd('/') + "/" + SharePointFolder.TrimStart('/');
            } }

        public SsrsProject()
        {
        }

        public static SsrsProject CreateNativeMode(string reportServiceUrl, string rootFolder)
        {
            var res = new SsrsProject
            {
                RootFolder = rootFolder,
                ReportServerUrl = reportServiceUrl.Replace("/reportservice2010.asmx", ""),
                SsrsMode = SsrsModeEnum.Native
            };
            return res;
        }

        public static SsrsProject CreateIntegratedMode(string siteUrl, string folder)
        {
            var res = new SsrsProject
            {
                SharePointBaseUrl = siteUrl,
                SharePointFolder = folder,
                SsrsMode = SsrsModeEnum.SpIntegrated
            };
            return res;
        }
    }


    /// <summary>
    /// Interaction logic for ListPicker.xaml
    /// </summary>
    public partial class SsrsConnectionChooser : UserControl
    {
        
        public SsrsProject SelectedProject { get
            {
                if (radioNativeMode.IsChecked.Value)
                {
                    return new SsrsProject()
                    {
                        SsrsMode = SsrsModeEnum.Native,
                        RootFolder = folderTextBox.Text,
                        ReportServerUrl = serverTextBox.Text,
                        SharePointBaseUrl = null,
                        SharePointFolder = null
                    };
                }
                else
                {
                    return new SsrsProject()
                    {
                        SsrsMode = SsrsModeEnum.SpIntegrated,
                        RootFolder = sharePointFolderTextBox.Text,
                        ReportServerUrl = sharePointSiteTextBox.Text.TrimEnd('/') + "/_vti_bin/reportserver",
                        SharePointBaseUrl = sharePointSiteTextBox.Text,
                        SharePointFolder = sharePointFolderTextBox.Text
                    };
                }
                
            }
        }

        public SsrsConnectionChooser()
        {
            InitializeComponent();
        }

        public SsrsConnectionChooser(SsrsProject defaultProject)
        {
            InitializeComponent();
            SetDefaultProject(defaultProject);
        }

        public void SetDefaultProject(SsrsProject defaultProject)
        {
            switch (defaultProject.SsrsMode)
            {
                case SsrsModeEnum.Native:
                    radioIntegratedMode.IsChecked = false;
                    radioNativeMode.IsChecked = true;
                    serverTextBox.Text = defaultProject.ReportServerUrl;
                    folderTextBox.Text = defaultProject.RootFolder;
                    break;
                case SsrsModeEnum.SpIntegrated:
                    radioIntegratedMode.IsChecked = true;
                    radioNativeMode.IsChecked = false;
                    sharePointSiteTextBox.Text = defaultProject.SharePointBaseUrl;
                    sharePointFolderTextBox.Text = defaultProject.SharePointFolder;
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        
        public bool Validate()
        {
            string error;
            string accessUrl = null;
            switch (SelectedProject.SsrsMode)
            {
                case SsrsModeEnum.SpIntegrated:
                    accessUrl = SelectedProject.SharePointFullUrl;
                    break;
                case SsrsModeEnum.Native:
                    accessUrl = SelectedProject.ReportServiceUrl;
                    break;
                default: throw new Exception();
            }
            var valid = WebTools.TestWebAccess(accessUrl, out error);
            if (!valid)
            {
                MessageBox.Show(error, "Failed to connect to the service", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return valid;
        }

        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            if (Validate())
            {
                MessageBox.Show("Connection test successful", "Connection test", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
