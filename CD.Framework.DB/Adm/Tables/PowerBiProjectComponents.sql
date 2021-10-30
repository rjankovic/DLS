CREATE TABLE [Adm].[PowerBiProjectComponents] (
    [PowerBiProjectComponentId]     INT              IDENTITY (1, 1) NOT NULL,
    [RedirectUri]                   NVARCHAR (200)   NULL,
    [ApplicationID]                 NVARCHAR (200)   NULL,
	[WorkspaceID]					NVARCHAR (200)   NULL,
	[ReportServerURL]				NVARCHAR (200)   NULL,
	[ReportServerFolder]			NVARCHAR (200)   NULL,
    [DiskFolder]                    NVARCHAR(MAX)    NULL,
    [ConfigType]                    NVARCHAR(50)     NULL,
    [ProjectConfig_ProjectConfigId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_dbo.PowerBiProjectComponents] PRIMARY KEY CLUSTERED ([PowerBiProjectComponentId] ASC),
    CONSTRAINT [FK_Adm_PowerBiProjectComponents_Adm_ProjectConfigs_ProjectConfig_ProjectConfigId] FOREIGN KEY ([ProjectConfig_ProjectConfigId]) REFERENCES [Adm].[ProjectConfigs] ([ProjectConfigId])
);

