using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API
{
    public class DLSApiProgressResponse : DLSApiMessage
    {
        private List<DLSApiMessage> _parallelRequests = new List<DLSApiMessage>();

        /// <summary>
        /// These take priority over ContinueWith
        /// </summary>
        public List<DLSApiMessage> ParallelRequests { get { return _parallelRequests; } set { _parallelRequests = value; } }
        public DLSApiMessage ContinueWith { get; set; }
        /// <summary>
        /// Both parallel and ContinueWith will wait for
        /// </summary>
        public bool ContinuationsWaitForDb { get; set; }
    }
}
