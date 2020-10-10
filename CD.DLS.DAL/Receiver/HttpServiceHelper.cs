using CD.DLS.API;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Receiver
{
    public class HttpServiceHelper : ServiceHelper
    {
        public HttpServiceHelper(string customerCode, Guid serviceReceiverId, ProjectConfig config)
            :base(new HttpReceiver(customerCode), serviceReceiverId, config)
        {
            _receiver = new HttpReceiver(customerCode);
            _serviceReceiverId = serviceReceiverId;
            _config = config;
        }
    }
}
