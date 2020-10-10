CREATE TABLE [Adm].[SsrsProjectComponents](
	[SsrsProjectComponentId] [int] IDENTITY(1,1) NOT NULL,
	[SsrsMode] NVARCHAR(200) NOT NULL DEFAULT N'Native',
	[ServerName] [nvarchar](200) NULL,
	[SsrsServiceUrl] [nvarchar](max) NULL,
	[SsrsExecutionServiceUrl] [nvarchar](max) NULL,
	[FolderPath] [nvarchar](max) NULL,
	[SharepointBaseUrl] [nvarchar](max) NULL,
	[SharepointFolder] [nvarchar](max) NULL,
	[ProjectConfig_ProjectConfigId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_dbo.SsrsProjectComponents] PRIMARY KEY CLUSTERED 
(
	[SsrsProjectComponentId] ASC
),
CONSTRAINT [FK_Adm_SsrsProjectComponents_Adm_ProjectConfigs_ProjectConfig_ProjectConfigId] FOREIGN KEY([ProjectConfig_ProjectConfigId])
REFERENCES [Adm].[ProjectConfigs] ([ProjectConfigId])
)
