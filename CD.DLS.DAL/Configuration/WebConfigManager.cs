using CD.DLS.Common.Interfaces;
using CD.DLS.Common.Tools;
using CD.DLS.DAL.Identity;
using CD.DLS.DAL.Misc;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CD.DLS.DAL.Configuration
{
    public class WebConfigManager : StandardConfigManager
    {
        private UserData _userData;

        public WebConfigManager(UserData userData)
        {
            _userData = userData;
        }

        protected override string GetConfiguredValueFromRegistryOrAppConfig(string key)
        {
            var configValue = System.Configuration.ConfigurationManager.AppSettings[key];
            return configValue;
        }

        public override string CustomerCode
        {
            get
            {
                return _userData.GetCustomerCode();
            }
        }

        public override string AadClientId
        {
            get
            {
                return GetConfiguredValueFromRegistryOrAppConfig(DLS_AAD_CLIENTID); 
            }
        }
    }
}
