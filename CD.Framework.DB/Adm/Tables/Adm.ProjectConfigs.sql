CREATE TABLE [Adm].[ProjectConfigs](
	[ProjectConfigId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NULL,
 CONSTRAINT [PK_Adm_ProjectConfigs] PRIMARY KEY CLUSTERED 
(
	[ProjectConfigId] ASC
)
)