CREATE TABLE [Adm].[RolePermissions](
	[RolePermissionId] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [int] NOT NULL,
	[PermissionId] [int] NOT NULL,
 CONSTRAINT [PK_Adm_RolePermissions] PRIMARY KEY CLUSTERED 
(
	[RolePermissionId] ASC
),
 CONSTRAINT[FK_Adm_RolePermissions_Permission] FOREIGN KEY
 (
 [PermissionId]
 ) REFERENCES [Adm].[Permissions]([PermissionId]),
 CONSTRAINT[FK_Adm_UserPermissions_Role] FOREIGN KEY
 (
 [RoleId]
 ) REFERENCES [Adm].[Roles]([RoleId]),
)