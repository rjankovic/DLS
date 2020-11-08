using CD.DLS.Common.Interfaces;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Misc;
using CD.DLS.DAL.Objects;
using CD.DLS.DAL.Receiver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Managers
{
    public class LogEntry
    {
        public LogTypeEnum LogType { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public int LogId { get; set; }
    }

    public class RequestManager
    {
        private NetBridge _netBridge;
        public NetBridge NetBridge { get { return _netBridge; } }
        public RequestManager(NetBridge netBridge)
        {
            _netBridge = netBridge;
            _logger = new DbLogger(new LogManager(netBridge));
        }

        public RequestManager()
        {
            _netBridge = new NetBridge();
            _logger = new DbLogger();
        }

        private ILogger _logger;

        public List<RequestMessage> GetPotentialCacheMatches(Guid projectId, CoreTypeEnum coreType)
        {
            var dt = NetBridge.ExecuteTableFunction("Adm.f_GetPotentialCacheMatches", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId },
                { "coretype", coreType }
            });
            var res = ReadRequestMessages(dt);
            return res;
        }

        public RequestMessage GetresponseForCachedMessage(Guid initMessageId)
        {
            var dt = NetBridge.ExecuteTableFunction("Adm.f_GetResponseForCachedMessage", new Dictionary<string, object>()
            {
                { "initMessageId", initMessageId }
            });
            var res = ReadRequestMessages(dt);
            return res.First();
        }
        
        public RequestMessage GetMessageById(Guid messageId)
        {
            var dt = NetBridge.ExecuteTableFunction("Adm.f_GetMessageById", new Dictionary<string, object>()
            {
                { "messageid", messageId }
            });
            var res = ReadRequestMessages(dt);
            return res.First();
        }

        public RequestMessage GetResponseForRequest(Guid requestId)
        {
            var dt = NetBridge.ExecuteTableFunction("Adm.f_GetRespoonseForRequest", new Dictionary<string, object>()
            {
                { "requestId", requestId }
            });
            var res = ReadRequestMessages(dt);
            if (res.Count == 0)
            {
                return null;
            }
            return res.First();
        }

        public RequestMessage GetCreationForRequest(Guid requestId)
        {
            var dt = NetBridge.ExecuteTableFunction("Adm.f_GetCreationForRequest", new Dictionary<string, object>()
            {
                { "requestId", requestId }
            });
            var res = ReadRequestMessages(dt);
            if (res.Count == 0)
            {
                return null;
            }
            return res.First();
        }

        public RequestMessage GetProgressForRequest(Guid requestId)
        {
            var dt = NetBridge.ExecuteTableFunction("Adm.f_GetProgressForRequest", new Dictionary<string, object>()
            {
                { "requestId", requestId }
            });
            var res = ReadRequestMessages(dt);
            if (res.Count == 0)
            {
                return null;
            }
            return res.First();
        }

        public RequestMessage GetMessageByReqestAndType(Guid requestId, MessageTypeEnum messageType)
        {
            var dt = NetBridge.ExecuteTableFunction("Adm.f_GetMessageByRequestAndType", new Dictionary<string, object>()
            {
                { "requestid", requestId },
                { "messagetype", messageType.ToString() }
            });
            var res = ReadRequestMessages(dt);
            return res.First();
        }

        public bool MarkMessageReceived(Guid messageId)
        {
            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
            ConfigManager.Log.Info(string.Format("Message {0} received in {1}", messageId, t.ToString()));

            var res = (int)NetBridge.ExecuteProcedureScalar("Adm.sp_MarkMessageReceived", new Dictionary<string, object>()
            {
                { "messageId", messageId }
            });
            return res == 1;
        }

        public void MarkMessageUnreceived(Guid messageId)
        {
            //System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
            //ConfigManager.Log.Info(string.Format("Message {0} received in {1}", messageId, t.ToString()));

            NetBridge.ExecuteProcedure("Adm.sp_MarkMessageUnreceived", new Dictionary<string, object>()
            {
                { "messageId", messageId }
            });
            
            //return res == 1;
        }

        public List<RequestMessage> GetNewMessagesToObject(Guid id)
        {
            var dt = NetBridge.ExecuteProcedureTable("Adm.sp_GetNewMessagesToObject", new Dictionary<string, object>()
            {
                { "objectId", id }
            });
            var res = ReadRequestMessages(dt);
            return res;
        }

        public void InvalidateRequestCache(Guid projectId)
        {
            NetBridge.ExecuteProcedure("Adm.sp_InvalidateRequestCache", new Dictionary<string, object>
            {
                { "projectconfigid", projectId }
            });
        }

        public void InvalidateRequestCache(Guid projectId, CoreTypeEnum coreType)
        {
            NetBridge.ExecuteProcedure("Adm.sp_InvalidateRequestCache", new Dictionary<string, object>
            {
                { "projectconfigid", projectId },
                { "coretype", coreType }
            });
        }

        public void CreateProcedureExecution(string procedureName, Guid projectConfigId, Guid requestId)
        {
            /*
             CREATE PROCEDURE [Adm].[sp_CreateProcedureExecution]
	@procedureName NVARCHAR(MAX),
	@projectConfigId UNIQUEIDENTIFIER

             */

            NetBridge.ExecuteProcedure("[Adm].[sp_CreateProcedureExecution]", new Dictionary<string, object>
            {
                { "procedureName", procedureName },
                { "projectConfigId", projectConfigId },
                { "requestId", requestId }
            });
        }

        public int GetLastLogId()
        {
            var res = NetBridge.ExecuteSelectStatement(
            "SELECT MAX(LogId) LastLogId FROM adm.Log");
            var cellRes = res.Rows[0][0];
            if (cellRes == DBNull.Value)
            {
                return 0;
            }
            else
            {
                return (int)cellRes;
            }
        }

        public List<LogEntry> GetLogs(int lastSeenLogId)
        {
            var tbl = NetBridge.ExecuteProcedureTable("Adm.sp_GetLogSinceId", new Dictionary<string, object>
            {
                { "lastSeenLogId", lastSeenLogId }
            });
            var res = ReadLogEntries(tbl);
            return res;
        }

        public List<LogEntry> ReadLogEntries(DataTable dt)
        {
            List<LogEntry> entries = new List<LogEntry>();
            foreach (DataRow row in dt.Rows)
            {
                LogTypeEnum lt = LogTypeEnum.Info;
                Enum.TryParse(row["MessageType"].ToString(), out lt);
                entries.Add(new LogEntry()
                {
                    LogId = (int)row["LogId"],
                    StackTrace = (row["StackTrace"] == DBNull.Value ? null : (string)row["Stacktrace"]),
                    Message = (string)row["Message"],
                    CreatedTime = (DateTimeOffset)row["CreatedDate"],
                    LogType = lt
                });
            }
            return entries;
            //LogId, CreatedDate, MessageType, Message, StackTrace
        }

        public void SaveRequestWaitFor(Guid requestId, List<Guid> waitForRequestIds)
        {
            var wfDt = FlattenGuidList(waitForRequestIds);

            /*
             CREATE PROCEDURE [Adm].[sp_SaveRequestWaitFors]
	@requestId UNIQUEIDENTIFIER,
	@waitForRequests [Adm].[UDTT_GuidList] READONLY
AS
             */

            NetBridge.ExecuteProcedure("[Adm].[sp_SaveRequestWaitFors]", new Dictionary<string, object>()
            {
                { "requestId", requestId },
                { "waitForRequests", wfDt}
            });
        }

        public void SaveRequestWaitForInactive(Guid requestId, List<Guid> waitForRequestIds)
        {
            var wfDt = FlattenGuidList(waitForRequestIds);

            /*
             CREATE PROCEDURE [Adm].[sp_SaveRequestWaitFors]
	@requestId UNIQUEIDENTIFIER,
	@waitForRequests [Adm].[UDTT_GuidList] READONLY
AS
             */

            NetBridge.ExecuteProcedure("[Adm].[sp_SaveRequestWaitForsInactive]", new Dictionary<string, object>()
            {
                { "requestId", requestId },
                { "waitForRequests", wfDt}
            });
        }

        public List<Guid> GetRequestsFinishedWaiting()
        {
            var resDt = NetBridge.ExecuteProcedureTable("[Adm].[sp_GetRequestsFinishedWaiting]", new Dictionary<string, object>
            ());

            List<Guid> res = new List<Guid>();
            foreach (DataRow r in resDt.Rows)
            {
                res.Add((Guid)r[0]);
            }
            return res;
        }

        public List<Guid> GetCompletedComplexRequests(Guid processedRequestId)
        {
            var resDt = NetBridge.ExecuteProcedureTable("[Adm].[sp_GetCompletedComplexRequests]", new Dictionary<string, object>
            ());

            List<Guid> res = new List<Guid>();
            foreach (DataRow r in resDt.Rows)
            {
                res.Add((Guid)r[0]);
            }
            return res;
        }

        private DataTable FlattenGuidList(List<Guid> guids)
        {
            DataTable res = new DataTable("Adm.UDTT_GuidList");
            res.Columns.Add("Id");

            foreach (var guid in guids)
            {
                var nr = res.NewRow();
                nr[0] = guid;
                res.Rows.Add(nr);
            }

            return res;
        }

        public bool SaveRequestMessage(RequestMessage message)
        {
            var msgDt = FlattenRequestMessages(new List<RequestMessage>() { message });

            if (message.Attachments == null)
            {
                message.Attachments = new List<Attachment>();
            }
            var atts = message.Attachments;
            foreach (var att in atts)
            {
                att.MessageId = message.MessageId;
            }
            var attachmentsDt = FlattenMessageAttachments(atts);
        
            var rowcount = (int)NetBridge.ExecuteProcedureScalar("Adm.sp_SaveRequestMessages", new Dictionary<string, object>
            {
                { "requestmessages", msgDt },
                { "attachments", attachmentsDt }
            });

            if (rowcount == 0)
            {
                NetBridge.ExecuteProcedure("Adm.sp_WriteLog", new Dictionary<string, object>
            {
                { "messageType", "Warning" },
                { "message", "Failed to save request message " + message.Serialize() },
                { "stackTrace", null}
            });
            }

            return true; // rowcount > 0; // not ideal...
        }

        public void LoadRequestMessageAttachments(RequestMessage message)
        {
            var attDt = NetBridge.ExecuteTableFunction("Adm.f_GetRequestMessageAttachments", new Dictionary<string, object>()
            {
                { "messageid", message.MessageId }
            });

            var deser = ReadAttachments(attDt);
            foreach (var att in deser)
            {
                att.Message = message;
            }
            message.Attachments = deser;
        }

        public void SaveHistory(RequestMessage responseMessage, DateTimeOffset cacheValidUntil)
        {
            var msgDt = FlattenRequestMessages(new List<RequestMessage>() { responseMessage });
            NetBridge.ExecuteProcedure("Adm.sp_SaveRequestMessageHistory", new Dictionary<string, object>
            {
                { "requestmessages", msgDt },
                { "cachevaliduntil", cacheValidUntil }
            });
        }

        private List<RequestMessage> ReadRequestMessages(DataTable table)
        {
            List<RequestMessage> messages = new List<RequestMessage>();
            foreach (DataRow row in table.Rows)
            {
                RequestMessage rm = new RequestMessage();

                rm.MessageId = Guid.Parse(row["MessageId"].ToString());
                rm.RequestId = Guid.Parse(row["RequestId"].ToString());
                rm.Content = (string)(row["Content"] == DBNull.Value ? string.Empty : row["Content"]);
                rm.RequestForCoreType = (CoreTypeEnum)Enum.Parse(typeof(CoreTypeEnum), (string)row["RequestForCoreType"]);
                rm.RequestProcessingMethod = (RequestMessage.RequestProcessingMethodEnum)
                    Enum.Parse(typeof(RequestMessage.RequestProcessingMethodEnum), (string)row["RequestProcessingMethod"]);
                rm.MessageFromId = Guid.Parse(row["MessageFromId"].ToString());
                rm.MessageFromName = row["MessageFromName"] == DBNull.Value ? null : (string)row["MessageFromName"];
                rm.MessageOriginId = Guid.Parse(row["MessageOriginId"].ToString());
                rm.MessageOriginName = (string)row["MessageOriginName"];
                rm.MessageToObjectId = Guid.Parse(row["MessageToObjectId"].ToString());
                rm.MessageToObjectName = (string)row["MessageToObjectName"];
                rm.MessageType = (MessageTypeEnum)Enum.Parse(typeof(MessageTypeEnum), (string)row["MessageType"]);
                rm.CreatedDateTime = DateTimeOffset.Parse(row["CreatedDateTime"].ToString());
                rm.MessageToProjectId = row["Project_ProjectConfigId"] == DBNull.Value ? Guid.Empty : Guid.Parse(row["Project_ProjectConfigId"].ToString());
                rm.CustomerCode = (string)row["CustomerCode"];
                rm.RequestFromUserId = row["RequestFromUserId"] == DBNull.Value ? 0 : (int)row["RequestFromUserId"];
                messages.Add(rm);
            }

            return messages;
            
        }

        public DataTable FlattenRequestMessages(List<RequestMessage> messages)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Adm.UDTT_RequestMessages";

            dt.Columns.Add("MessageId", typeof(Guid));
            dt.Columns.Add("RequestId", typeof(Guid));
            dt.Columns.Add("Content", typeof(string));
            dt.Columns.Add("RequestForCoreType", typeof(string));
            dt.Columns.Add("RequestProcessingType", typeof(string));
            dt.Columns.Add("MessageFromId", typeof(Guid));
            dt.Columns.Add("MessageOriginName", typeof(string));
            dt.Columns.Add("MessageOriginId", typeof(Guid));
            dt.Columns.Add("MessageFromName", typeof(string));
            dt.Columns.Add("MessageToObjectId", typeof(Guid));
            dt.Columns.Add("MessageToProjectId", typeof(Guid));
            dt.Columns.Add("MessageToObjectName", typeof(string));
            dt.Columns.Add("MessageType", typeof(string));
            dt.Columns.Add("CreatedDateTime", typeof(DateTimeOffset));
            dt.Columns.Add("Project_ProjectConfigId", typeof(Guid));
            dt.Columns.Add("CustomerCode", typeof(string));
            dt.Columns.Add("RequestFromUserId", typeof(int));

            foreach (var msg in messages)
            {
                var nr = dt.NewRow();

                nr[0] = msg.MessageId;
                nr[1] = msg.RequestId;
                nr[2] = msg.Content;
                nr[3] = msg.RequestForCoreType;
                nr[4] = msg.RequestProcessingMethod;
                nr[5] = msg.MessageFromId;
                nr[6] = msg.MessageOriginName;
                nr[7] = msg.MessageOriginId;
                nr[8] = msg.MessageFromName;
                nr[9] = msg.MessageToObjectId;
                nr[10] = msg.MessageToProjectId == Guid.Empty ? (object)DBNull.Value : msg.MessageToProjectId;
                nr[11] = msg.MessageToObjectName;
                nr[12] = msg.MessageType;
                nr[13] = msg.CreatedDateTime;
                nr[14] = msg.MessageToProjectId == Guid.Empty ? (object)DBNull.Value : msg.MessageToProjectId;
                nr[15] = msg.CustomerCode;
                nr[16] = msg.RequestFromUserId;

                dt.Rows.Add(nr);
            }
            
            return dt;
        }
        
        private DataTable FlattenMessageAttachments(List<Attachment> attachments)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Adm.UDTT_RequestMessageAttachments";

            dt.Columns.Add("AttachmentId", typeof(Guid));
            dt.Columns.Add("Type", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Uri", typeof(string));
            dt.Columns.Add("MessageId", typeof(Guid));
            dt.Columns.Add("OriginalRequestMessage_MessageId", typeof(Guid));

            foreach (var attachment in attachments)
            {
                var nr = dt.NewRow();
                nr[0] = attachment.AttachmentId;
                nr[1] = attachment.Type.ToString();
                nr[2] = attachment.Name;
                nr[3] = attachment.Uri.AbsolutePath;
                nr[4] = attachment.MessageId;
                nr[5] = DBNull.Value;

                dt.Rows.Add(nr);
            }
            
            return dt;
        }

        internal void SetMessageReceived(Guid messageId)
        {
            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
            ConfigManager.Log.Info(string.Format("Message {0} received in {1}", messageId, t.ToString()));

            NetBridge.ExecuteProcedure("Adm.sp_SetMessageReceived", new Dictionary<string, object>
            {
                { "messageId", messageId }
            });
        }

        private List<Attachment> ReadAttachments(DataTable dt)
        {
            List<Attachment> res = new List<Attachment>();
            foreach (DataRow r in dt.Rows)
            {
                var att = new Attachment();
                att.AttachmentId = Guid.Parse(r["AttachmentId"].ToString());
                att.Type = (AttachmentTypeEnum)(Enum.Parse(typeof(AttachmentTypeEnum), r["Type"].ToString()));
                att.Name = (string)r["Name"];
                att.Uri = new Uri((string)r["Uri"]);
                att.MessageId = Guid.Parse(r["MessageId"].ToString());

                res.Add(att);
            }

            return res;
        }

        public SqlDependency GetMessageHandle(Guid targetObjectId)
        {
            var brokerCommand = new SqlCommand(
            "SELECT [MessageId] FROM adm.RequestMessages WHERE [MessageToObjectId] = @objectId AND Received = 0",
                    NetBridge.BrokerConnection);
            _logger.Important("Waiting for messages: " + brokerCommand.CommandText + " (" + targetObjectId.ToString() + ")");
            brokerCommand.Parameters.AddWithValue("@objectId", targetObjectId);
            var dependency = new SqlDependency(brokerCommand);
            using (SqlDataReader reader = brokerCommand.ExecuteReader())
            {
            }
            return dependency;
        }


        public SqlDependency GetLogHandle(int lastLogId)
        {
            var brokerCommand = new SqlCommand(
            "SELECT LogId FROM adm.Log WHERE LogId > @lastLogId",
                    NetBridge.BrokerConnection);
            brokerCommand.Parameters.AddWithValue("@lastLogId", lastLogId);
            var dependency = new SqlDependency(brokerCommand);
            try
            {
                using (SqlDataReader reader = brokerCommand.ExecuteReader())
                {
                }
            }
            catch
            { }
            return dependency;
        }

        public SqlDependency GetResponseHandle(RequestMessage message)
        {
            //var cmd = string.Format("SELECT [MessageId] FROM adm.RequestMessages WHERE [RequestId] = '{0}' AND MessageType NOT IN ('RequestAcknowledged', 'RequestCreated')", message.RequestId);
            var cmd = string.Format("SELECT [MessageId] FROM adm.RequestMessages WHERE [RequestId] = N'{0}' AND MessageType IN (N'RequestProcessed','Timeout','Error','IsBusy')", message.RequestId.ToString());
            var brokerCommand = new SqlCommand(
            cmd,
                    NetBridge.BrokerConnection);
            //brokerCommand.Parameters.AddWithValue("@requestId", message.RequestId);
            var dependency = new SqlDependency(brokerCommand);
            using (SqlDataReader reader = brokerCommand.ExecuteReader())
            {
            }
            return dependency;
        }

        public void SaveBroadcastMessage(BroadcastMessage broadcastMessage)
        {
            var serialized = broadcastMessage.Serialize();

            NetBridge.ExecuteProcedure("[Adm].[sp_SaveBroadcastMessage]", new Dictionary<string, object>
            {
                { "BroadcastMessageId", broadcastMessage.BroadcastMessageId },
                { "BroadcastMessageType", broadcastMessage.Type.ToString() },
                { "ProjectConfigId", broadcastMessage.ProjectConfigId },
                { "Active", broadcastMessage.Active },
                { "Content", serialized }
            });
        }
        
        public bool SaveBroadcastMessageSingleton(BroadcastMessage broadcastMessage)
        {
            var serialized = broadcastMessage.Serialize();

            var res = NetBridge.ExecuteProcedureScalar("[Adm].[sp_SaveBroadcastMessageSingleton]", new Dictionary<string, object>
            {
                { "BroadcastMessageId", broadcastMessage.BroadcastMessageId },
                { "BroadcastMessageType", broadcastMessage.Type.ToString() },
                { "ProjectConfigId", broadcastMessage.ProjectConfigId },
                { "Active", broadcastMessage.Active },
                { "Content", serialized }
            });

            return (int)res == 1;
        }


        public void SetBroadcastMessageInactive(BroadcastMessage broadcastMessage)
        {
            NetBridge.ExecuteProcedure("[Adm].[sp_SetBroadcastMessageInactive]", new Dictionary<string, object>
            {
                { "BroadcastMessageId", broadcastMessage.BroadcastMessageId }
            });
        }
        
        public BroadcastMessage GetBroadcastMessageById(Guid broadcastMessageId)
        {
            var dt = NetBridge.ExecuteProcedureTable("[Adm].[sp_GetBroadcastMessageById]", new Dictionary<string, object>
            {
                { "BroadcastMessageId", broadcastMessageId }
            });

            var deser = ReadBroadcastMessages(dt);
            return deser.FirstOrDefault();
        }
        
        public List<BroadcastMessage> GetActiveBroadcastMessages()
        {
            var dt = NetBridge.ExecuteProcedureTable("[Adm].[sp_GetActiveBroadcastMessages]", new Dictionary<string, object>
            {
            });

            var deser = ReadBroadcastMessages(dt);
            return deser;
        }

        private List<BroadcastMessage> ReadBroadcastMessages(DataTable dt)
        {
            List<BroadcastMessage> res = new List<BroadcastMessage>();
            
            foreach (DataRow dr in dt.Rows)
            {
                var content = (string)dr["Content"];
                var bm = BroadcastMessage.Deserialize(content);
                bm.Type = (BroadcastMessageType)(Enum.Parse(typeof(BroadcastMessageType), (string)dr["BroadcastMessageType"]));
                bm.BroadcastMessageId = (Guid)dr["BroadcastMessageId"];
                bm.Active = (bool)dr["Active"];
                bm.ProjectConfigId = (Guid)dr["ProjectConfigId"];
                res.Add(bm);
            }

            return res;
        }

        public List<AbandonedRequest> GetAbandonedRequests()
        {
            var dt = NetBridge.ExecuteProcedureTable("[Adm].[sp_GetAbandonedRequests]", new Dictionary<string, object>());

            List<AbandonedRequest> res = new List<AbandonedRequest>();

            foreach (DataRow dr in dt.Rows)
            {
                try
                {
                    res.Add(new AbandonedRequest()
                    {
                        RequestId = new Guid(dr["RequestId"].ToString()),
                        MessageId = new Guid(dr["MessageId"].ToString()),
                        CreatedDateTime = DateTime.Parse(dr["CreatedDateTime"].ToString())
                    });
                }
                catch (Exception ex)
                {
                    ConfigManager.Log.Important(string.Format("{0} {1} {2}",
                        dr["RequestId"].ToString(), dr["MessageId"].ToString(), dr["CreatedDateTime"].ToString()));
                }
            }

            return res;
        }
    }
}
