CREATE TABLE [Stg].[Extracts]
(
	[ExtractId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[ProjectConfigId] UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Extracts_ProjectConfig FOREIGN KEY REFERENCES [Adm].[ProjectConfigs]([ProjectConfigId]),
	[ExtractedBy] NVARCHAR(MAX) NOT NULL,
	[ExtractStartTime] DATETIME NOT NULL
)
GO

GO