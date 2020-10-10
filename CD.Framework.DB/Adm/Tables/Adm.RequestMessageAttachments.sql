CREATE TABLE [Adm].[RequestMessageAttachments](
	[AttachmentId] [uniqueidentifier] NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](200) NULL,
	[Uri] [nvarchar](800) NULL,
	[MessageId] [uniqueidentifier] NOT NULL,
	[OriginalRequestMessage_MessageId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_dbo.MessageAttachments] PRIMARY KEY CLUSTERED 
(
	[AttachmentId] ASC
)
/*
,
CONSTRAINT [FK_Adm_MessageAttachments_Adm_RequestMessages_MessageId] FOREIGN KEY([MessageId])
REFERENCES [Adm].[RequestMessages] ([MessageId]),
CONSTRAINT [FK_Adm_MessageAttachments_Adm_RequestMessages_OrignalRequestMessage_MessageId] FOREIGN KEY([OriginalRequestMessage_MessageId])
REFERENCES [Adm].[RequestMessages] ([MessageId])
*/
)

GO
