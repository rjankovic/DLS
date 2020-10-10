CREATE TABLE [Annotate].[LinkTypes]
(
	[LinkTypeId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[ProjectConfigId] UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Adm].[ProjectConfigs]([ProjectConfigId]),
	[LinkTypeName] NVARCHAR(255),
	[Deleted]  BIT DEFAULT 0
)
