CREATE TABLE [BIDoc].[DataMessages]
(
	[MessageId] INT NOT NULL IDENTITY(1,1),
	[SourceElementId] INT,
	[TargetElementId] INT,
	[Message] NVARCHAR(MAX) NOT NULL,
	[DataMessagesTypeId] INT NOT NULL,
	CONSTRAINT [PK_BIDoc_DataMessages] PRIMARY KEY CLUSTERED ([MessageId] ASC),
	CONSTRAINT [FK_BIDoc_DataMessages_SourceId] FOREIGN KEY ([SourceElementId]) REFERENCES [BIDoc].[ModelElements]([ModelElementId]),
	CONSTRAINT [FK_BIDoc_DataMessages_TargetId] FOREIGN KEY ([TargetElementId]) REFERENCES [BIDoc].[ModelElements]([ModelElementId]),
	CONSTRAINT [FK_BIDoc_DataMessages_DataMessagesTypeId] FOREIGN KEY ([DataMessagesTypeId]) REFERENCES [BIDoc].[DataMessagesType]([DataMessagesTypeId]),
)
GO