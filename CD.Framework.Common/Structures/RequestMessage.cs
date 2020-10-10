using CD.DLS.Common.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Common.Structures
{
    public class RequestMessage
    {
        public enum RequestProcessingMethodEnum { Short, Long }

        public Guid RequestId { get; set; }
        public Guid MessageId { get; set; }
        public string Content { get; set; }
        public CoreTypeEnum RequestForCoreType { get; set; }
        public RequestProcessingMethodEnum RequestProcessingMethod { get; set; }
        public Guid MessageFromId { get; set; }
        public string MessageOriginName { get; set; }
        public Guid MessageOriginId { get; set; }
        public string MessageFromName { get; set; }
        public Guid MessageToObjectId { get; set; }
        public Guid MessageToProjectId { get; set; }
        public string MessageToObjectName { get; set; }
        public MessageTypeEnum MessageType { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public virtual List<Attachment> Attachments { get; set; }
        public string CustomerCode { get; set; }
        public int RequestFromUserId { get; set; }

        public RequestMessage()
        {
            RequestId = Guid.NewGuid();
            MessageId = Guid.NewGuid();
            Attachments = new List<Attachment>();
        }

        public string Serialize()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                //TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.SerializeObject(this, settings);
        }

        public static RequestMessage Deserialize(string serialized)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                //TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.DeserializeObject<RequestMessage>(serialized, settings);
        }

        public bool RequestsSameInfo(RequestMessage other)
        {
            if (MessageType != MessageTypeEnum.RequestCreated)
            {
                return false;
            }
            return RequestForCoreType == other.RequestForCoreType
                && MessageType == other.MessageType
                && MessageToProjectId == other.MessageToProjectId
                && Content == other.Content;
        }
    }

    public class LogItem
    {
        public DateTimeOffset CreatedDate { get; set; }
	    public string MessageType { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }

    public class UserActionLogItem
    {
        public DateTimeOffset CreatedDate { get; set; }
        public string EventType { get; set; }
        public int UserId { get; set; }
        public string ApplicationName { get; set; }
	    public string FrameworkElement { get; set; }
	    public string DataContext { get; set; }
	    public string ExtendedProperties { get; set; }
    }

    public class Attachment
    {
        public Guid AttachmentId { get; set; }
        public AttachmentTypeEnum Type { get; set; }
        public string Name { get; set; }
        public string DbUri { get; set; }
        public Uri Uri
        {
            get
            {
                return new Uri(DbUri);
            }

            set
            {
                DbUri = value.OriginalString;
            }
        }
        public Guid MessageId { get; set; }
        public virtual RequestMessage Message { get; set; }
    }


    public enum AttachmentTypeEnum { WordFile, TextFile, Plaintext, JSON, XML, SVG, Dataset, Excel, Html }
    public enum MessageTypeEnum { RequestCreated = 1, RequestProcessed = 2, RequestAcknowledged = 3,
        Timeout = 4, Error = 5, IsBusy = 6, Progress = 7,
        DbOperationFinished = 8 }
}
