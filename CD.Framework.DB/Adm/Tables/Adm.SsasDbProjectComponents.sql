CREATE TABLE [Adm].[SsasDbProjectComponents](
	[SsaslDbProjectComponentId] [int] IDENTITY(1,1) NOT NULL,
	[ServerName] [nvarchar](200) NULL,
	[DbName] [nvarchar](200) NULL,
	[SSASType] [nvarchar] (20) NULL,
	[ProjectConfig_ProjectConfigId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_dbo.SsasDbProjectComponents] PRIMARY KEY CLUSTERED 
(
	[SsaslDbProjectComponentId] ASC
),
CONSTRAINT [FK_Adm_SsasDbProjectComponents_Adm_ProjectConfigs_ProjectConfig_ProjectConfigId] FOREIGN KEY([ProjectConfig_ProjectConfigId])
REFERENCES [Adm].[ProjectConfigs] ([ProjectConfigId])
)
GO
