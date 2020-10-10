CREATE TABLE [Adm].[Log](
	[LogId] [int] IDENTITY(1,1) NOT NULL,
	[CreatedDate] [datetimeoffset](7) CONSTRAINT DF_Adm_Log_CreatedDate DEFAULT GETDATE(),
	[MessageType] NVARCHAR(100) NOT NULL,
	[Message] NVARCHAR(MAX) NOT NULL,
	[StackTrace] NVARCHAR(MAX) NULL
 CONSTRAINT [PK_Adm_Log] PRIMARY KEY CLUSTERED 
(
	[LogId] ASC
)
)
