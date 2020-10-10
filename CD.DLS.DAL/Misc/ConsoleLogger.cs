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
    public class ConsoleLogger : ILogger
    {
        string _source;

        public ConsoleLogger(string source)
        {
            _source = source;
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
            throw new NotImplementedException();
        }

        public void Write(string message, object[] args, LogTypeEnum type)
        {
            var consoleMsg = DateTime.Now.ToString("u") + "\t" + _source + "\t" + message;
            Console.WriteLine(consoleMsg);
        }

        public void LogUserAction(UserActionEventType eventType, string frameworkElement, string dataContext, string extendedProperties)
        {
            //throw new NotImplementedException();
        }
    }
}
