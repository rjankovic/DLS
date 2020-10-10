CREATE TYPE [Adm].[UDTT_Log] AS TABLE(
	[CreatedDate] [datetimeoffset](7),
	[MessageType] NVARCHAR(100) NOT NULL,
	[Message] NVARCHAR(MAX) NOT NULL,
	[StackTrace] NVARCHAR(MAX) NULL
)
