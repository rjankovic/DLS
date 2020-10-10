using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Managers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Receiver
{
    public class LogListener
    {
        private NetBridge _netBridge;
        private RequestManager _requestManager;

        public class LogEventArgs : EventArgs
        {
            public LogEntry LogEntry { get; set; }
        }

        public delegate void LogEventHandler(object sender, LogEventArgs args);
        public event LogEventHandler NewLog;


        private SqlDependency _sqlDependency;
        private int _lastLogId;
        public LogListener(NetBridge netBridge = null)
        {
            if (netBridge == null)
            {
                netBridge = new NetBridge();
            }

            _netBridge = netBridge;
            _requestManager = new RequestManager(_netBridge);

            if (Configuration.ConfigManager.DeploymentMode == Configuration.DeploymentModeEnum.OnPremises)
            {
                _lastLogId = _requestManager.GetLastLogId();
                _sqlDependency = _requestManager.GetLogHandle(_lastLogId);
                _sqlDependency.OnChange += SqlDependency_OnChange;
            }

            else if (Configuration.ConfigManager.DeploymentMode == Configuration.DeploymentModeEnum.Azure)
            {

            }
        }

        private void SqlDependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            var logs = _requestManager.GetLogs(_lastLogId);
            _lastLogId = logs.Max(x => x.LogId);
            _sqlDependency.OnChange -= SqlDependency_OnChange;
            _sqlDependency = _requestManager.GetLogHandle(_lastLogId);
            _sqlDependency.OnChange += SqlDependency_OnChange;

            foreach (var log in logs)
            {
                var args = new LogEventArgs();
                args.LogEntry = log;
                NewLog?.Invoke(this, args);
            }
        }
    }
}
