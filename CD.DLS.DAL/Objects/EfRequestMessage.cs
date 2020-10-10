//namespace CD.Framework.EF.Entities
//{
//    public class EfRequestMessage : RequestMessage
//    {
//        public virtual ProjectConfig Project { get; set; }

//        public EfRequestMessage()
//        { }
//        public EfRequestMessage(RequestMessage requestMessage, ProjectConfig project)
//        {
//            this.Attachments = requestMessage.Attachments;
//            this.Content = requestMessage.Content;
//            this.CreatedDateTime = requestMessage.CreatedDateTime;
//            this.MessageFromId = requestMessage.MessageFromId;
//            this.MessageFromName = requestMessage.MessageFromName;
//            this.MessageId = requestMessage.MessageId;
//            this.MessageOriginId = requestMessage.MessageOriginId;
//            this.MessageOriginName = requestMessage.MessageOriginName;
//            this.MessageToObjectId = requestMessage.MessageToObjectId;
//            this.MessageToObjectName = requestMessage.MessageToObjectName;
//            this.MessageToProjectId = requestMessage.MessageToProjectId;
//            this.MessageType = requestMessage.MessageType;
//            this.RequestForCoreType = requestMessage.RequestForCoreType;
//            this.RequestId = requestMessage.RequestId;
//            Project = project;
//        }
//    }
    
//}
// map request message directly
// mapping will include inserts to other tables (?) -> yes, but using other mappings to save them