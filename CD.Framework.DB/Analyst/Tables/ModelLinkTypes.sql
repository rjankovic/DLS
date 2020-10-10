CREATE TABLE [Analyst].[ModelLinkTypes](
	[ModelLinkTypeId] [int] NOT NULL IDENTITY(1,1),
	[Name] [nvarchar](max) NOT NULL,
	[Code] [nvarchar](30)  NOT NULL,
	[ProjectConfigId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Analyst_ModelLinkTypes] PRIMARY KEY CLUSTERED 
(
	[ModelLinkTypeId] ASC
),
CONSTRAINT [FK_Analyst_ModelLinkTypes_ProjectConfigId] FOREIGN KEY ([ProjectConfigId]) REFERENCES [Adm].[ProjectConfigs]([ProjectConfigId]),

)
