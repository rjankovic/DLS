CREATE TYPE [Adm].[UDTT_SsrsProjectComponents] AS TABLE(
	[SsrsMode] NVARCHAR(200) NOT NULL,
	[ServerName] [nvarchar](200) NULL,
	[SsrsServiceUrl] [nvarchar](max) NULL,
	[SsrsExecutionServiceUrl] [nvarchar](max) NULL,
	[FolderPath] [nvarchar](max) NULL,
	[SharepointBaseUrl] [nvarchar](max) NULL,
	[SharepointFolder] [nvarchar](max) NULL
)
