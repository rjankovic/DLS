using CD.DLS.Common.Structures;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace CD.DLS.Clients.Web.Models
{
    public class SessionManager
    {
        private const string SESSION_KEY_USERDATA = "USER_DATA";
        private const string SESSION_KEY_PROJECTCONFIG = "PROJECT_CONFIG";
        private const string SESSION_KEY_CUSTOMERCONNECTIONSTRING = "CUSTOMER_CONNECTION_STRING";
        private const string SESSION_KEY_NETBRIDGE = "NET_BRIDGE";

        private HttpSessionStateBase _session;

        public SessionManager(HttpSessionStateBase session)
        {
            _session = session;
        }

        public UserData UserData
        {
            get { return GetFromSession<UserData>(SESSION_KEY_USERDATA); }
            set { SaveToSession(SESSION_KEY_USERDATA, value); }
        }
        
        public ProjectConfig ProjectConfig
        {
            get { return GetFromSession<ProjectConfig>(SESSION_KEY_PROJECTCONFIG); }
            set { SaveToSession(SESSION_KEY_PROJECTCONFIG, value); }
        }

        public NetBridge NetBridge
        {
            get { return GetFromSession<NetBridge>(SESSION_KEY_NETBRIDGE); }
            set { SaveToSession(SESSION_KEY_NETBRIDGE, value); }
        }
        
        private T GetFromSession<T>(string key) where T : class
        {
            if (_session[key] == null)
            {
                return null;
            }
            return (T)_session[key];
        }

        private void SaveToSession(string key, object val)
        {
            _session[key] = val;
        }
    }
}