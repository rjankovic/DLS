using CD.DLS.Common.Interfaces;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Misc;
using CD.DLS.DAL.Objects;
using CD.DLS.DAL.Receiver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Managers
{
    public class DbDeploymentManager
    {
        private NetBridge _netBridge;
        private ILogger _logger;

        public NetBridge NetBridge { get { return _netBridge; } }

        public event SqlInfoMessageEventHandler ConnectionInfoMessage;

        public DbDeploymentManager(NetBridge netBridge, ILogger logger)
        {
            //var tempDirPath = Path.GetTempPath();
            //var extractDirPath = Path.Combine(tempDirPath, "DLS_Deployment");
            _netBridge = netBridge;
            _logger = logger;
        }

        public DbDeploymentManager(ILogger logger)
        {
            _netBridge = new NetBridge(true);
            _logger = logger;
            _netBridge.ConnectionInfoMessage += NetBridge_ConnectionInfoMessage;
        }

        private void NetBridge_ConnectionInfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            if (ConnectionInfoMessage != null)
            {
                ConnectionInfoMessage(sender, e);
            }
        }

        
        public void AddAppliedDbVersion(int appliedVersion)
        {
            NetBridge.ExecuteProcedure("Adm.sp_AddAppliedDbVersion", new Dictionary<string, object>()
            {
                { "appliedVersion", appliedVersion }
            });
        }

        public void CheckAppliedDbVersion(int appliedVersion)
        {
            try
            {
                NetBridge.ExecuteProcedure("Adm.sp_CheckAppliedDbVersion", new Dictionary<string, object>()
            {
                { "appliedVersion", appliedVersion }
            });
            }
            catch (SqlException sqlex)
            {
                if (appliedVersion == 0 && sqlex.Message.ToLower().Contains("could not find"))
                {
                    return;
                }
                else
                {
                    throw;
                }
            }

        }

    }
}
