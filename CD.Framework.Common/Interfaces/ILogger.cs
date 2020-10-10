using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CD.DLS.Common.Interfaces
{
    public interface ILogger
    {
        void Info(string message, params object[] args);
        void Important(string message, params object[] args);
        void Error(string message, params object[] args);
        void Warning(string message, params object[] args);
        void FlushMessages();
        void LogUserAction(UserActionEventType eventType, string frameworkElement, string dataContext, string extendedProperties);
    }

    public enum LogTypeEnum { Info, Important, Warning, Error }
    public enum UserActionEventType { LeftClick, RightClick, Error }
}
