CREATE TABLE [Adm].[GlobalConfig](
	[GlobalConfigId] [int] IDENTITY(1,1) NOT NULL,
	[Key]	NVARCHAR(200) NOT NULL,
	[Value] NVARCHAR(MAX)
 CONSTRAINT [PK_Adm_GlobalConfig] PRIMARY KEY CLUSTERED 
(
	[GlobalConfigId] ASC
)
)
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_Adm_GlobalConfig_Project_Key ON [Adm].[GlobalConfig]([Key])
