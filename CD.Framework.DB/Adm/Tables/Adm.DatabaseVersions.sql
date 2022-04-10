CREATE TABLE [Adm].[DatabaseVersions](
	[DatabaseVersionId] [int] IDENTITY(1,1) NOT NULL,
	[VersionNumber] INT NOT NULL
 CONSTRAINT [PK_Adm_DatabaseVersions] PRIMARY KEY CLUSTERED 
(
	[DatabaseVersionId] ASC
)
)