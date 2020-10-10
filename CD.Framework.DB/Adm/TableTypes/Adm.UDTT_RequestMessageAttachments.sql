CREATE TYPE [Adm].[UDTT_RequestMessageAttachments] AS TABLE(
	[AttachmentId] [uniqueidentifier] NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](200) NULL,
	[Uri] [nvarchar](800) NULL,
	[MessageId] [uniqueidentifier] NOT NULL,
	[OriginalRequestMessage_MessageId] [uniqueidentifier] NULL
)
