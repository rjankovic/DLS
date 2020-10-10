using CD.DLS.Common.Interfaces;
using CD.DLS.DAL.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Misc
{
    public delegate void DLSLogEventHandler(object sender, LogEventArgs e);


    public class LogEventArgs
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public LogTypeEnum LogType { get; set; }
    }

    public class EventLogger : ILogger
    {


        public event DLSLogEventHandler LogEvent;

        public EventLogger()
        {
        }
        

        public void Error(string message, params object[] args)
        {
            Write(message, args, LogTypeEnum.Error);
        }

        public void Important(string message, params object[] args)
        {
            Write(message, args, LogTypeEnum.Important);
        }

        public void Info(string message, params object[] args)
        {
            Write(message, args, LogTypeEnum.Info);
        }

        public void FlushMessages()
        {
            //throw new NotImplementedException();
        }

        public void Warning(string message, params object[] args)
        {
            Write(message, args, LogTypeEnum.Warning);
        }
        

        public void Write(string message, object[] args, LogTypeEnum type)
        {
            var messageFormatted = message;
            StackTrace stackTrace = new StackTrace();
            //_logManager.WriteLog(type, messageFormatted, stackTrace.ToString());

            if (LogEvent != null)
            {
                LogEvent(this, new LogEventArgs()
                {
                    Message = message,
                    StackTrace = stackTrace.ToString(),
                    LogType = type
                });
            }

            //if (type == LogTypeEnum.Important || type == LogTypeEnum.Error)
            //{
            //    var consoleMsg = DateTime.Now.ToString("u") + "\t" + messageFormatted;
            //    Console.WriteLine(consoleMsg);
            //    if (type == LogTypeEnum.Error)
            //    {
            //        //using (EventLog eventLog = new EventLog("Application"))
            //        //{
            //        //    eventLog.Source = "CD Framework";
            //        //    eventLog.WriteEntry(consoleMsg);
            //        //}
            //    }
            //}

        }

        public void LogUserAction(UserActionEventType eventType, string frameworkElement, string dataContext, string extendedProperties)
        {
            //throw new NotImplementedException();
        }
    }
}
