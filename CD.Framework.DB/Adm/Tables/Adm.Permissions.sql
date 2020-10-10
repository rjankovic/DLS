CREATE TABLE [Adm].[Permissions](
	[PermissionId] [int] IDENTITY(1,1) NOT NULL,
	[PermissionName] NVARCHAR(300) NOT NULL,
	-- entity ID
	[PermissionScope] NVARCHAR(MAX) NULL,
 CONSTRAINT [PK_Adm_Permissions] PRIMARY KEY CLUSTERED 
(
	[PermissionId] ASC
)
)