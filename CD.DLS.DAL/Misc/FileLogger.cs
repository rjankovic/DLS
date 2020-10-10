using CD.DLS.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Misc
{
    public class FileLogger : ILogger
    {
        string _source;
        private List<string> _unwrittenMessages = new List<string>();

        public FileLogger(string source = null, string filePath = null)
        {
            try
            {
                if (source == null)
                {
                    source = string.Empty;
                }
                _source = source;
                if (filePath != null)
                {
                    Trace.Listeners.Add(new TextWriterTraceListener(filePath));
                }
                Trace.AutoFlush = true;
            }
            catch (Exception ex)
            {
                //File.WriteAllText("C:\\Log\\Receiver1.log", ex.Message + (ex.InnerException == null ? string.Empty : (Environment.NewLine + ex.InnerException.Message)) + Environment.NewLine + ex.StackTrace);
                throw;
            }
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
            if (_unwrittenMessages.Count != 0)
                throw new Exception(string.Join("\n", _unwrittenMessages));
        }

        public void Write(string message, object[] args, LogTypeEnum type)
        {
            var messageFormatted = message; // string.Format(message, args);
            var msg = DateTime.Now.ToString("u") + "\t" + _source + "\t" + type.ToString() + "\t" + messageFormatted;
            Trace.WriteLine(msg);
            if (Trace.Listeners.Count == 0)
            {
                _unwrittenMessages.Add(msg);
            }
            if (type == LogTypeEnum.Important || type == LogTypeEnum.Error)
            {
                var consoleMsg = DateTime.Now.ToString("u") + "\t" + _source + "\t" + messageFormatted;
                Console.WriteLine(consoleMsg);
            }
        }

        public void LogUserAction(UserActionEventType eventType, string frameworkElement, string dataContext, string extendedProperties)
        {
            //throw new NotImplementedException();
        }
    }
}
