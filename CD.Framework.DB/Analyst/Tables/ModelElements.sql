CREATE TABLE [Analyst].[ModelElements](
	[ModelElementId] [int] NOT NULL IDENTITY(1,1),
	[RefPath] [nvarchar](max) NULL,
	[Name] [nvarchar](max) NULL,
	[ModelElementTypeId] INT NOT NULL,
	[ProjectConfigId] [uniqueidentifier] NOT NULL,
	[ObjectId]	INT	NOT NULL
 CONSTRAINT [PK_Analyst_ModelElements] PRIMARY KEY CLUSTERED 
(
	[ModelElementId] ASC
),
CONSTRAINT [FK_Analyst_ModelElements_ProjectConfigId] FOREIGN KEY ([ProjectConfigId]) REFERENCES [Adm].[ProjectConfigs]([ProjectConfigId]),
CONSTRAINT [FK_Analyst_ModelElements_ModelElementTypeId] FOREIGN KEY ([ModelElementTypeId]) REFERENCES [Analyst].[ModelElementTypes]([ModelElementTypeId]),
CONSTRAINT [FK_Analyst_ModelElements_ObjectId] FOREIGN KEY ([ObjectId]) REFERENCES [Analyst].[Objects]([ObjectId]),
)
GO