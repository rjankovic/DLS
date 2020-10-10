using CD.DLS.API;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Receiver
{
    public class ServiceHelper
    {
        protected ProjectConfig _config;
        protected IReceiver _receiver = null;
        protected Guid _serviceReceiverId = Guid.Empty;


        public ServiceHelper(IReceiver receiver, Guid serviceReceiverId, ProjectConfig config)
        {
            _receiver = receiver;
            _serviceReceiverId = serviceReceiverId;
            _config = config;
        }

        public async Task<R> PostRequest<R>(DLSApiRequest<R> request) where R : DLSApiMessage
        {
            var requestMessage = CreateEmptyRequest();
            requestMessage.Content = request.Serialize();
            var response = await _receiver.PostMessage(requestMessage);
            var deser = DLSApiMessage.Deserialize(response.Content);
            return (R)deser;
        }

        private RequestMessage CreateEmptyRequest()
        {
            var msg = Helpers.CreateRequest(_receiver, _config.ProjectConfigId);
            msg.MessageToObjectId = _serviceReceiverId;
            msg.RequestForCoreType = Common.Interfaces.CoreTypeEnum.BIDoc;
            
            return msg;
        }

    }
}
