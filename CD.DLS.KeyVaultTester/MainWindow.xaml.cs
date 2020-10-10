using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace CD.DLS.KeyVaultTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ManualConfigManager _config;
        public MainWindow()
        {
            InitializeComponent();

            _config = new ManualConfigManager();
            ConfigManager.SetCustomConfigManager(_config);
            _config.DeploymentMode = DeploymentModeEnum.Azure;
            _config.ApplicationClass = ApplicationClassEnum.Client;

            /*
             <add key="ida:AADInstance" value="https://login.microsoftonline.com/{0}" />
    <add key="ida:Tenant" value="lukasmatejovskycleverdecisi.onmicrosoft.com" />
    <add key="ida:ClientId" value="f4ac2e9b-c278-4ab4-9bb0-0664a55e6466" />
    <add key="ida:RedirectUri" value="https://lukasmatejovskycleverdecisi.onmicrosoft.com/DLSClient" />
    <add key="ida:GraphResourceId" value="https://graph.windows.net/" />
    <add key="ida:GraphApiVersion" value="1.5" />
    <add key="ida:GraphEndpoint" value="https://graph.windows.net/" />
             */

            _config.AadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
            _config.AzureTenant = ConfigurationManager.AppSettings["ida:Tenant"];
            _config.AadClientId = ConfigurationManager.AppSettings["ida:ClientId"];
            _config.AadClientId = ConfigurationManager.AppSettings["ida:ClientId"];
            _config.MsGraphResourceId = ConfigurationManager.AppSettings["ida:GraphResourceId"];
            _config.AadRedirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];


            IdentityProvider.Logout();
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            IdentityProvider.Logout();
            identityTb.Clear();
        }

        private async void getButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (clientRadio.IsChecked.Value)
                {
                    _config.ApplicationClass = ApplicationClassEnum.Client;
                    if (IdentityProvider.GetCurrentUser() == null)
                    {
                        IdentityProvider.Login(false);
                        identityTb.Text = IdentityProvider.GetCurrentUser().Identity;
                    }
                }
                else
                {
                    _config.ApplicationClass = ApplicationClassEnum.Service;
                }

                var value = await IdentityProvider.GetKeyVaultSecretVaultUrl(kvUrlTb.Text, secretNameTb.Text);
                valueTb.Text = value;

                //var groups = IdentityProvider.GetGroups();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
