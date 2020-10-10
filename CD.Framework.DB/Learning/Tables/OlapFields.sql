CREATE TABLE [Learning].[OlapFields]
(
	[OlapFieldId] INT NOT NULL IDENTITY(1,1),
	[ProjectConfigId] UNIQUEIDENTIFIER,
	[FieldElementId] INT, -- FOREIGN KEY REFERENCES [BIDoc].[ModelElements]([ModelElementId]),
	[FieldType] NVARCHAR(30),
	[FieldReference] NVARCHAR(MAX),
	[FieldName] NVARCHAR(MAX),
	[ServerName] NVARCHAR(MAX),
	[DbName] NVARCHAR(MAX),
	[CubeName] NVARCHAR(MAX),
	CONSTRAINT PK_Learning_OlapFields PRIMARY KEY([OlapFieldId])
)
