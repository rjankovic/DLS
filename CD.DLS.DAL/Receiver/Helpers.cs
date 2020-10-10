using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Receiver
{
    public static class Helpers
    {
        public static Guid GetTargetProjectId(RequestMessage message)
        {
            return message.MessageToProjectId;
        }

        public static RequestMessage CreateResponse(RequestMessage message)
        {
            return new RequestMessage()
            {
                //MessageFromId = r.Id,
                //MessageFromName = r.Name,
                MessageId = Guid.NewGuid(),
                MessageOriginId = message.MessageOriginId,
                MessageOriginName = message.MessageOriginName,
                MessageToProjectId = message.MessageToProjectId,
                MessageToObjectId = message.MessageFromId,
                MessageToObjectName = string.Empty,
                MessageType = MessageTypeEnum.RequestProcessed,
                CreatedDateTime = DateTimeOffset.Now,
                RequestForCoreType = message.RequestForCoreType,
                RequestId = message.RequestId,
                CustomerCode = message.CustomerCode,
                RequestFromUserId = message.RequestFromUserId
            };
        }
        
        public static RequestMessage CreateRequest(IReceiver r, Guid projectId, string customerCode = null)
        {
            var messageId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            if (customerCode == null)
            {
                customerCode = ConfigManager.CustomerCode;
            }

            var currentUser = IdentityProvider.GetCurrentUser();

            return new RequestMessage()
            {
                MessageFromId = r.Id,
                MessageFromName = r.Name,
                MessageId = messageId,
                MessageOriginId = r.Id,
                MessageOriginName = r.Name,
                MessageToProjectId = projectId,
                MessageToObjectId = Guid.Empty, // message.MessageToProjectId,
                MessageToObjectName = string.Empty,
                MessageType = MessageTypeEnum.RequestCreated,
                CreatedDateTime = DateTimeOffset.Now,
                RequestForCoreType = Common.Interfaces.CoreTypeEnum.ManagementApi,
                RequestId = requestId,
                CustomerCode = customerCode,
                RequestFromUserId = currentUser == null ? 0 : currentUser.UserId
            };
        }
    }

    public enum ServiceBusMessageTypeEnum { RequestMessage = 1, ResponseMessage = 2, ServiceLog = 3, Broadcast = 4 }

    public class ServiceBusMessage
    {
        public ServiceBusMessageTypeEnum MessageType { get; set; }
        public Guid MessageId { get; set; }
        public Guid TargetId { get; set; }
        public Guid ResponseToRequestId { get; set; }
        public string Content { get; set; }
        public string CustomerCode { get; set; }

        public string Serialize()
        {
            if (Content != null)
            {
                if (Content.Length > 1000)
                {
                    Content = Content.Substring(0, 1000) + "...";
                }
            }
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.SerializeObject(this, settings);
        }

        public static ServiceBusMessage Deserialize(string serialized)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.DeserializeObject<ServiceBusMessage>(serialized, settings);
        }
    }


    public class JsonSerializable<T>
    {
        public string Serialize()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.SerializeObject(this, settings);
        }

        public static T Deserialize(string serialized)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.DeserializeObject<T>(serialized, settings);
        }
    }

    public enum BroadcastMessageType { ProjectUpdateStarted, ProjectUpdateFinished }

    public class BroadcastMessage : JsonSerializable<BroadcastMessage>
    {
        [JsonIgnore]
        public Guid BroadcastMessageId { get; set; }
        [JsonIgnore]
        public BroadcastMessageType Type { get; set; }
        [JsonIgnore]
        public Guid ProjectConfigId { get; set; }
        [JsonIgnore]
        public bool Active { get; set; }
    }
}
