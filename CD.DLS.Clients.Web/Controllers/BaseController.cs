using CD.DLS.Clients.Web.Models;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Identity;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Receiver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CD.DLS.Clients.Web.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        private ProjectConfig _projectConfig = null;
        private UserData _userData = null;
        private NetBridge _netBridge;
        private HttpServiceHelper _afService = null;


        public ProjectConfig ProjectConfig { get { return _projectConfig; } }
        public UserData UserData { get { return _userData; } }
        public NetBridge NetBridge { get { return _netBridge; } }
        public HttpServiceHelper AfService { get { return _afService; } }

        public BaseController()
        {
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            SessionManager sessionManager = new SessionManager(Session);
            
            if (sessionManager.UserData == null)
            {
                WebIdentityProvider wip = new WebIdentityProvider();
                ConfigManager.SetCustomConfigManager(new WebConfigManager(null));
                var secret = ConfigManager.AadClientSecret;
                var userData = Task.Run(async () => await wip.GetAadUser()).Result;
                if (userData.GetCustomerCode() == "default")
                {
                    _userData = null;
                    return;
                }
                var config = new WebConfigManager(userData);
                ConfigManager.SetCustomConfigManager(new WebConfigManager(userData));
                ConfigManager.ApplicationClass = ApplicationClassEnum.WebClient;
                var nbs = new NetBridge(true, false);
                var connectionString = ConfigManager.CustomerDatabaseConnectionString;
                nbs.SetConnectionString(connectionString);
                SecurityManager sm = new SecurityManager(nbs);
                wip.SaveUserData(userData, sm);

                sessionManager.UserData = userData;
                //sessionManager.CustomerDbConnectionString = connectionString;
            }
            _userData = sessionManager.UserData;

            if (sessionManager.ProjectConfig != null)
            {
                _projectConfig = sessionManager.ProjectConfig;
                _afService = new HttpServiceHelper(UserData.GetCustomerCode(), ConfigManager.ServiceReceiverId, ProjectConfig);
            }

            if (sessionManager.NetBridge == null)
            {
                var nb = new NetBridge();
                sessionManager.NetBridge = nb;
            }

            _netBridge = sessionManager.NetBridge;
            
        }
    }
}