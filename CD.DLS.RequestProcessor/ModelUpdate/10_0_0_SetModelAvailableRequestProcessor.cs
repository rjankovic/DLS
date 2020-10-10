using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Receiver;
using CD.DLS.Model.DependencyGraph.KnowledgeBase;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Parse.Mssql;
using CD.DLS.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class SetModelAvailableRequestProcessor : RequestProcessorBase, IRequestProcessor<SetModelAvailableRequest, DLSApiMessage>
    {
        public DLSApiMessage Process(SetModelAvailableRequest request, ProjectConfig projectConfig)
        {
            // set model available
            var msgs = RequestManager.GetActiveBroadcastMessages();
            foreach (var msg in msgs.Where(x => x.Type == DAL.Receiver.BroadcastMessageType.ProjectUpdateStarted))
            {
                RequestManager.SetBroadcastMessageInactive(msg);
            }

            // TODO revive model available later
            /*
            var broadcast = new BroadcastMessage() { Active = true, BroadcastMessageId = Guid.NewGuid(), ProjectConfigId = projectConfig.ProjectConfigId, Type = BroadcastMessageType.ProjectUpdateFinished };
            RequestManager.SaveBroadcastMessageSingleton(broadcast);
            ClientSender.PostBroadcastMessageToServiceBus(broadcast, CustomerCode);
            */

            return new DLSApiMessage();
        }
    }
}
