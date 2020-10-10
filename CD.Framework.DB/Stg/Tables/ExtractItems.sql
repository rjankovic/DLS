CREATE TABLE [Stg].[ExtractItems]
(
	[ExtractItemId] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[ExtractId]	UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_ExtractItems_Extract FOREIGN KEY REFERENCES [Stg].[Extracts]([ExtractId]),
	[ComponentId] INT NOT NULL,
	[ObjectType] NVARCHAR(200) NOT NULL,
	[ObjectName] NVARCHAR(MAX) NOT NULL,
	[Content] NVARCHAR(MAX) NULL
)
GO

CREATE NONCLUSTERED INDEX IX_Stg_Extracts ON [Stg].[ExtractItems] ([ExtractId], [ComponentId], [ObjectType])
GO