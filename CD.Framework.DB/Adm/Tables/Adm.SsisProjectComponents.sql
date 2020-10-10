CREATE TABLE [Adm].[SsisProjectComponents](
	[SsisProjectComponentId] [int] IDENTITY(1,1) NOT NULL,
	[ServerName] [nvarchar](200) NULL,
	[FolderName] [nvarchar](200) NULL,
	[ProjectName] [nvarchar](200) NULL,
	[ProjectConfig_ProjectConfigId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Adm_SsisProjectComponents] PRIMARY KEY CLUSTERED 
(
	[SsisProjectComponentId] ASC
),
CONSTRAINT [FK_Adm_SsisProjectComponents_Adm_ProjectConfigs_ProjectConfig_ProjectConfigId] FOREIGN KEY([ProjectConfig_ProjectConfigId])
REFERENCES [Adm].[ProjectConfigs] ([ProjectConfigId])
)

