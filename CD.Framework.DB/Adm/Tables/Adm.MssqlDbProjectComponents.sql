CREATE TABLE [Adm].[MssqlDbProjectComponents](
	[MssqlDbProjectComponentId] [int] IDENTITY(1,1) NOT NULL,
	[ServerName] [nvarchar](200) NULL,
	[DbName] [nvarchar](200) NULL,
	[ProjectConfig_ProjectConfigId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Adm_MssqlDbProjectComponents] PRIMARY KEY CLUSTERED 
(
	[MssqlDbProjectComponentId] ASC
),
CONSTRAINT [FK_Adm_MssqlDbProjectComponents_Adm_ProjectConfigs_ProjectConfig_ProjectConfigId] FOREIGN KEY([ProjectConfig_ProjectConfigId])
REFERENCES [Adm].[ProjectConfigs] ([ProjectConfigId])
)