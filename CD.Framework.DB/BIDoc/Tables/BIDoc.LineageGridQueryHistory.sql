CREATE TABLE [BIDoc].[LineageGridHistory](
	[LineageGridHistoryId] [int] NOT NULL IDENTITY(1,1),
	[ProjectConfigId] UNIQUEIDENTIFIER NOT NULL,
	[SourceRootElementPath] NVARCHAR(MAX) NOT NULL,
	[TargetRootElementPath] NVARCHAR(MAX) NOT NULL,
	[SourceElementType] NVARCHAR(MAX) NOT NULL,
	[TargetElementType] NVARCHAR(MAX) NOT NULL,
	[SourceRootElementId] INT NULL,
	[TargetRootElementId] INT NULL,
	[CreatedDateTime] DATETIME NOT NULL,
	[UserId] INT NOT NULL
 CONSTRAINT [PK_BIDoc_LineageGridHistory] PRIMARY KEY CLUSTERED 
(
	[LineageGridHistoryId] ASC
),
CONSTRAINT [FK_BIDoc_LineageGridHistory_ProjectConfigId] 
	FOREIGN KEY ([ProjectConfigId]) REFERENCES [Adm].[ProjectConfigs]([ProjectConfigId]),
CONSTRAINT [FK_BIDoc_LineageGridHistory_UserId] 
	FOREIGN KEY ([UserId]) REFERENCES [Adm].[Users]([UserId]),
CONSTRAINT [FK_BIDoc_LineageGridHistory_SourceElementId] 
	FOREIGN KEY ([SourceRootElementId]) REFERENCES [BIDoc].[ModelElements]([ModelElementId]),
CONSTRAINT [FK_BIDoc_LineageGridHistory_TargetElementId] 
	FOREIGN KEY ([TargetRootElementId]) REFERENCES [BIDoc].[ModelElements]([ModelElementId])
)
GO

CREATE NONCLUSTERED INDEX [IX_BIDoc_LineageGridHistory_UserId] 
ON [BIDoc].[LineageGridHistory] ([UserId])

GO
