using CD.DLS.Common.Interfaces;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Identity;
using CD.DLS.DAL.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CD.DLS.DAL.Misc
{
    public class DbLogger : ILogger
    {
        private LogManager _logManager;
        private List<LogItem> _buffer = new List<LogItem>();
        private List<UserActionLogItem> _userActionBuffer = new List<UserActionLogItem>();
        
        public DbLogger()
        {
            _logManager = new LogManager();
        }

        public DbLogger(LogManager logmanager)
        {
            _logManager = logmanager;
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

        public void Warning(string message, params object[] args)
        {
            Write(message, args, LogTypeEnum.Warning);
        }

        public void FlushMessages()
        {
            lock (_buffer)
            {
                    _logManager.WriteLogBatch(_buffer);
                    _buffer.Clear();
            }

            lock (_userActionBuffer)
            {
                _logManager.WriteUserActionLogBatch(_userActionBuffer);
                _userActionBuffer.Clear();
            }
        }
        
        public void Write(string message, object[] args, LogTypeEnum type)
        {
            if (message.Contains("WriteLogBatch")){
                return;
            }

            var messageFormatted = message;
            StackTrace stackTrace = new StackTrace();

            _buffer.Add(new LogItem() { CreatedDate = DateTimeOffset.Now, Message = messageFormatted, MessageType = type.ToString(), StackTrace = null });

            //_logManager.WriteLog(type, messageFormatted, stackTrace.ToString());

            if (type == LogTypeEnum.Important || type == LogTypeEnum.Error)
            {
                var consoleMsg = DateTime.Now.ToString("u") + "\t" + messageFormatted;
                Console.WriteLine(consoleMsg);
                if (type == LogTypeEnum.Error)
                {
                    //using (EventLog eventLog = new EventLog("Application"))
                    //{
                    //    eventLog.Source = "CD Framework";
                    //    eventLog.WriteEntry(consoleMsg);
                    //}
                }
            }

            if (DateTimeOffset.Now.ToUnixTimeSeconds() - _buffer.Min(x => x.CreatedDate).ToUnixTimeSeconds() >= 0.1)
            {
                FlushMessages();
            }


            /*
            try
            {
                if (args.Length > 0)
                {
                    messageFormatted = string.Format(message, args);
                }
            }
            catch
            {
            }
            if (type == LogTypeEnum.Important || type == LogTypeEnum.Error)
            {
                var consoleMsg = DateTime.Now.ToString("u") + "\t" + _source + "\t" + messageFormatted;
                Console.WriteLine(consoleMsg);
                if (type == LogTypeEnum.Error)
                {
                    //using (EventLog eventLog = new EventLog("Application"))
                    //{
                    //    eventLog.Source = "CD Framework";
                    //    eventLog.WriteEntry(consoleMsg);
                    //}
                }
            }
            if (type == LogTypeEnum.Info || type == LogTypeEnum.Important)
            {
                LogManager.WriteLog(type, message);
            }
            else
            {
                StackTrace stackTrace = new StackTrace();

                LogManager.WriteLog(type, messageFormatted, stackTrace.ToString());
            }
            */
        }

        public void LogUserAction(UserActionEventType eventType, string frameworkElement, string dataContext, string extendedProperties)
        {
            _userActionBuffer.Add(new UserActionLogItem()
            {
                ApplicationName = System.AppDomain.CurrentDomain.FriendlyName,
                CreatedDate = DateTimeOffset.Now,
                DataContext = dataContext,
                EventType = eventType.ToString(),
                ExtendedProperties = extendedProperties,
                FrameworkElement = frameworkElement,
                UserId = IdentityProvider.GetCurrentUser().UserId
            });

            if (_userActionBuffer.Max(x => x.CreatedDate).ToUnixTimeSeconds() - _userActionBuffer.Min(x => x.CreatedDate).ToUnixTimeSeconds() >= 30)
            {
                FlushMessages();
            }
        }
    }
}
