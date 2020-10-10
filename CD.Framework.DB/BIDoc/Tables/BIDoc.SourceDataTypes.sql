CREATE TABLE [BIDoc].[SourceDataTypes]
(
	[SourceDataTypesId] INT NOT NULL IDENTITY(1,1),
	[SourceName] NVARCHAR(30) NOT NULL,
	[SourceDataTypeName] NVARCHAR(30) NOT NULL,
	[DataTypeId] INT NOT NULL,
	CONSTRAINT [PK_BIDoc_BIDocSourceDataTypes] PRIMARY KEY CLUSTERED ([SourceDataTypesId] ASC),
	CONSTRAINT [FK_BIDoc_SourceDataTypes_DataTypeId] FOREIGN KEY ([DataTypeId]) REFERENCES [BIDoc].[DataTypes]([DataTypesId]),
)
GO