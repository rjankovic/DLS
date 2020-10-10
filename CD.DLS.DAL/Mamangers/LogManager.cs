using CD.DLS.Common.Interfaces;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Engine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Managers
{
    public class LogManager
    {
        private NetBridge _netBridge;
        public NetBridge NetBridge { get { return _netBridge; } }
        public LogManager(NetBridge netBridge)
        {
            _netBridge = netBridge;
        }

        public LogManager()
        {
            _netBridge = new NetBridge();
        }

        public void WriteLog(LogTypeEnum logType, string message, string stackTrace = null)
        {
            /*
             * CREATE PROCEDURE [Adm].[sp_WriteLog]
	@messageType NVARCHAR(100),
	@message NVARCHAR(MAX),
	@stackTrace NVARCHAR(MAX) = NULL
AS
             */
            if (string.IsNullOrEmpty(NetBridge.ConnString))
            {
                return;
            }

            NetBridge.ExecuteProcedure("Adm.sp_WriteLog", new Dictionary<string, object>
            {
                { "messageType", logType },
                { "message", message },
                { "stackTrace", stackTrace}
            });
        }


        public void WriteLogBatch(List<LogItem> messages)
        {
            var dt = FlattenLog(messages);

            NetBridge.ExecuteProcedure("Adm.sp_WriteLogBatch", new Dictionary<string, object>
            {
                { "log", dt }
            });
        }

        public void WriteUserActionLogBatch(List<UserActionLogItem> messages)
        {
            var dt = FlattenUserActionLog(messages);

            NetBridge.ExecuteProcedure("Adm.sp_WriteUserActionLogBatch", new Dictionary<string, object>
            {
                { "log", dt }
            });
        }

        public DataTable FlattenLog(List<LogItem> messages)
        {
            /*
             CREATE TYPE [Adm].[UDTT_Log] AS TABLE(
	[CreatedDate] [datetimeoffset](7),
	[MessageType] NVARCHAR(100) NOT NULL,
	[Message] NVARCHAR(MAX) NOT NULL,
	[StackTrace] NVARCHAR(MAX) NULL
             */

            DataTable dt = new DataTable();
            dt.TableName = "Adm.UDTT_Log";

            dt.Columns.Add("CreatedDate", typeof(DateTimeOffset));
            dt.Columns.Add("MessageType", typeof(string));
            dt.Columns.Add("Message", typeof(string));
            dt.Columns.Add("StackTrace", typeof(string));

            foreach (var msg in messages)
            {
                var nr = dt.NewRow();

                nr[0] = msg.CreatedDate;
                nr[1] = msg.MessageType;
                nr[2] = msg.Message;
                nr[3] = msg.StackTrace;

                dt.Rows.Add(nr);
            }

            return dt;
        }

        public DataTable FlattenUserActionLog(List<UserActionLogItem> messages)
        {
            /*
             CREATE TYPE [Adm].[UDTT_UserActionLog] AS TABLE(
	[CreatedDate] [datetimeoffset](7),
	[EventType] NVARCHAR(MAX) NOT NULL,
	[UserId] INT NULL,
	[ApplicationName] NVARCHAR(MAX) NULL,
	[FrameworkElement] NVARCHAR(MAX) NULL,
	[DataContext] NVARCHAR(MAX) NULL,
	[ExtendedProperties] NVARCHAR(MAX) NULL
)
             */

            DataTable dt = new DataTable();
            dt.TableName = "Adm.UDTT_UserActionLog";

            dt.Columns.Add("CreatedDate", typeof(DateTimeOffset));
            dt.Columns.Add("EventType", typeof(string));
            dt.Columns.Add("UserId", typeof(int));
            dt.Columns.Add("ApplicationName", typeof(string));
            dt.Columns.Add("FrameworkElement", typeof(string));
            dt.Columns.Add("DataContext", typeof(string));
            dt.Columns.Add("ExtendedProperties", typeof(string));

            foreach (var msg in messages)
            {
                var nr = dt.NewRow();

                nr[0] = msg.CreatedDate;
                nr[1] = msg.EventType;
                nr[2] = msg.UserId;
                nr[3] = msg.ApplicationName;
                nr[4] = msg.FrameworkElement;
                nr[5] = msg.DataContext;
                nr[6] = msg.ExtendedProperties;

                dt.Rows.Add(nr);
            }

            return dt;
        }

    }
}
