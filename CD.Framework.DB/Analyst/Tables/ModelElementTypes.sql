CREATE TABLE [Analyst].[ModelElementTypes](
	[ModelElementTypeId] [int] NOT NULL IDENTITY(1,1),
	[Name] [nvarchar](max) NOT NULL,
	[ProjectConfigId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Analyst_ModelElementTypes] PRIMARY KEY CLUSTERED 
(
	[ModelElementTypeId] ASC
),
CONSTRAINT [FK_Analyst_ModelElementTypes_ProjectConfigId] FOREIGN KEY ([ProjectConfigId]) REFERENCES [Adm].[ProjectConfigs]([ProjectConfigId]),

)
