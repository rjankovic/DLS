CREATE TABLE [Adm].[Roles](
	[RoleId] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] NVARCHAR(300) NOT NULL,
	-- NULL if global
	[ProjectConfigId] UNIQUEIDENTIFIER NULL
 CONSTRAINT [PK_Adm_Roles] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC
),
 CONSTRAINT[UI_Adm_Roles_RoleName_ProjectConfigId] UNIQUE
 (
 [RoleName], [ProjectConfigId]
 )
)