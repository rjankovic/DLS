using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Model.Interfaces;

namespace CD.DLS.Model.Mssql
{
    /// <summary>
    /// Stores logged messages.
    /// </summary>
    public class Log : ILog
    {
        private readonly List<string> _messages = new List<string>();

        public void Error(string format, params object[] args)
        {
            _messages.Add(string.Format(format, args));
        }

        public void Warning(string format, params object[] args)
        {
            _messages.Add(string.Format(format, args));
        }

        /// <summary>
        /// Throws an exception if there were any messages.
        /// </summary>
        public void ThrowMessages()
        {
            if (_messages.Count != 0)
                throw new Exception(string.Join("\n", _messages));
        }
    }
}
