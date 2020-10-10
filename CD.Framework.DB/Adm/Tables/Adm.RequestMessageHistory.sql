CREATE TABLE [Adm].[RequestMessageHistory](
	[MessageHistoryId] [int] IDENTITY(1,1) NOT NULL,
	[RequestId] [uniqueidentifier] NOT NULL,
	[CacheValid] [bit] NOT NULL,
	[CacheValidUntil] [datetimeoffset](7) NOT NULL,
	[InitMessage_MessageId] [uniqueidentifier] NULL,
	[ResponseMessage_MessageId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Adm_MessageHistory] PRIMARY KEY CLUSTERED 
(
	[MessageHistoryId] ASC
)
/*,
CONSTRAINT [FK_Adm_MessageHistory_Adm_RequestMessages_InitMessage_MessageId] FOREIGN KEY([InitMessage_MessageId])
REFERENCES [Adm].[RequestMessages] ([MessageId]),
CONSTRAINT [FK_Adm_MessageHistory_Adm_RequestMessages_ResponseMessage_MessageId] FOREIGN KEY([ResponseMessage_MessageId])
REFERENCES [Adm].[RequestMessages] ([MessageId])
*/
)
