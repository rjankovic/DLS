using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Identity;
using CD.DLS.DAL.Managers;
using Fluent;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;


namespace CD.DLS.Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private WindowsIdentity _windowsIdentity;

        protected override void OnStartup(StartupEventArgs e)
        {
            //// add custom accent and theme resource dictionaries to the ThemeManager
            //// you should replace FluentRibbonThemesSample with your application name
            //// and correct place where your custom accent lives
            //ThemeManager.AddAccent("RibbonTheme",
            //    //new Uri("pack://application:,,,/CD.DLS.Manager;component/Resources/RibbonTheme.xaml")
            //    new Uri("Resources/RibbonTheme.xaml", UriKind.Relative)
            //    );

            //// get the current app style (theme and accent) from the application
            //Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(Application.Current);

            //var accent = ThemeManager.GetAccent("RibbonTheme");

            //// now change app style to the custom accent and current theme
            //ThemeManager.ChangeAppStyle(Application.Current,
            //                            accent,
            //                            theme.Item1);

            base.OnStartup(e);
            ConfigManager.ApplicationClass = ApplicationClassEnum.Client;

            IdentityProvider.Login();
            if (IdentityProvider.GetCurrentUser() == null)
            {
                this.Shutdown();
            }
        }
    }
}
