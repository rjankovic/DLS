using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Common.Interfaces
{
    public interface IQueueProvider
    {
        Task<RequestMessage> GetMessage();
        List<RequestMessage> GetWaitingMessages();
        void AddMessage(RequestMessage message);
        void RemoveMessage(RequestMessage message);
    }
}
