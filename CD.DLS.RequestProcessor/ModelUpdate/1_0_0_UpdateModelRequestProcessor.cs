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
    public class UpdateModelRequestProcessor : RequestProcessorBase, IRequestProcessor<UpdateModelRequest, DLSApiProgressResponse>
    {
        public DLSApiProgressResponse Process(UpdateModelRequest request, ProjectConfig projectConfig)
        {
            // set model unavailable
            var msgs = RequestManager.GetActiveBroadcastMessages();
            foreach (var msg in msgs.Where(x => x.Type == DAL.Receiver.BroadcastMessageType.ProjectUpdateFinished))
            {
                RequestManager.SetBroadcastMessageInactive(msg);
            }

            // TODO revive model unavailable later
            /*
            var broadcast = new BroadcastMessage() { Active = true, BroadcastMessageId = Guid.NewGuid(), ProjectConfigId = projectConfig.ProjectConfigId, Type = BroadcastMessageType.ProjectUpdateStarted };
            RequestManager.SaveBroadcastMessageSingleton(broadcast);
            ClientSender.PostBroadcastMessageToServiceBus(broadcast, CustomerCode);
            */

            //GraphManager.ClearModel(projectConfig.ProjectConfigId, RequestId);
            RequestManager.CreateProcedureExecution("[BIDoc].[sp_ClearModel]", projectConfig.ProjectConfigId, RequestId);
            
            return new DLSApiProgressResponse()
            {
                ContinueWith = new ParseSqlDatabasesRequest()
                {
                    ExtractId = //Guid.Parse("9fa61938-3792-4aee-9ba2-a8c710ca02e5") // 
                    request.ExtractId
                },
                ContinuationsWaitForDb = true
            };
            
        }
    }
}
