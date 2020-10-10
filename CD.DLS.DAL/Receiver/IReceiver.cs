using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Receiver
{
    public interface IReceiver
    {
        Guid Id { get; }
        string Name { get; }
        Task<RequestMessage> PostMessage(RequestMessage message);
        Task PostMessageNoResponse(RequestMessage message);
    }
}
