CREATE TABLE [BIDoc].[DataMessagesType]
(
	[DataMessagesTypeId] INT NOT NULL IDENTITY(1,1),
	[DataMessageType] NVARCHAR(100) NOT NULL,
	[DataMessageCode] NVARCHAR(10) NOT NULL,
	CONSTRAINT [PK_BIDoc_DataMessagesType] PRIMARY KEY CLUSTERED ([DataMessagesTypeId] ASC),
)
GO