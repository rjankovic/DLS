CREATE TABLE [Adm].[MssqlAgentProjectComponents](
	[MssqlAgentProjectComponentId] [int] IDENTITY(1,1) NOT NULL,
	[ServerName] [nvarchar](200) NULL,
	[JobName] [nvarchar](200) NULL,
	[ProjectConfig_ProjectConfigId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_dbo.MssqlAgentProjectComponents] PRIMARY KEY CLUSTERED 
(
	[MssqlAgentProjectComponentId] ASC
),
CONSTRAINT [FK_Adm_MssqlAgentProjectComponents_Adm_ProjectConfigs_ProjectConfig_ProjectConfigId] FOREIGN KEY([ProjectConfig_ProjectConfigId])
REFERENCES [Adm].[ProjectConfigs] ([ProjectConfigId])
)
