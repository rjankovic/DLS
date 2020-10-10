CREATE TABLE [Adm].[BroadcastMessages](
        [BroadcastMessageId] UNIQUEIDENTIFIER NOT NULL,
        [BroadcastMessageType] NVARCHAR(200) NOT NULL,
        [ProjectConfigId] UNIQUEIDENTIFIER NULL,
        [Active] BIT NOT NULL,
		[Content] NVARCHAR(MAX) NOT NULL,
		[CreatedTime] DATETIME DEFAULT(GETDATE())
CONSTRAINT [FK_Adm_BroadcastMessages_Adm_ProjectConfigs_ProjectConfigId] FOREIGN KEY([ProjectConfigId])
REFERENCES [Adm].[ProjectConfigs] ([ProjectConfigId]),
CONSTRAINT [PK_Adm_BroadcastMessages] PRIMARY KEY([BroadcastMessageId])
)
