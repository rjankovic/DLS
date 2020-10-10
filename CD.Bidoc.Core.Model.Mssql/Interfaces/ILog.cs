using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Interfaces
{
    /// <summary>
    /// Log for internal events in the core
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="format">Format of the message.</param>
        /// <param name="args">Message arguments.</param>
        void Warning(string format, params object[] args);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="format">Format of the message.</param>
        /// <param name="args">Message arguments.</param>
        void Error(string format, params object[] args);
    }
}
