using CD.DLS.Common.Interfaces;
using CD.DLS.DAL.Misc;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.AzureFunctionService
{
    public class AzureFunctionLogger : ILogger
    {
        private TraceWriter _traceWriter;
        public DbLogger DbLogger { get; set; }

        public AzureFunctionLogger(TraceWriter traceWriter)
        {
            _traceWriter = traceWriter;
        }

        public void Error(string message, params object[] args)
        {
            _traceWriter.Error(message);
            if (DbLogger != null)
            {
                DbLogger.Error(message);
            }
        }

        public void Important(string message, params object[] args)
        {
            Log(message);
        }

        public void Info(string message, params object[] args)
        {
            Log(message);
        }

        public void FlushMessages()
        {
            throw new NotImplementedException();
        }

        public void Warning(string message, params object[] args)
        {
            Log(message);
        }

        private void Log(string message)
        {
            if (DbLogger != null)
            {
                DbLogger.Info(message);
            }
            _traceWriter.Info(message);
        }

        public void LogUserAction(UserActionEventType eventType, string frameworkElement, string dataContext, string extendedProperties)
        {
            //throw new NotImplementedException();
        }
    }
}
