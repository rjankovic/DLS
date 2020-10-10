CREATE TABLE [Adm].[UserRoles](
	[UserRoleId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
 CONSTRAINT [PK_Adm_UserRoles] PRIMARY KEY CLUSTERED 
(
	[UserRoleId] ASC
),
 CONSTRAINT[FK_Adm_UserRoles_User] FOREIGN KEY
 (
 [UserId]
 ) REFERENCES [Adm].[Users]([UserId]),
 CONSTRAINT[FK_Adm_UserRoles_Role] FOREIGN KEY
 (
 [RoleId]
 ) REFERENCES [Adm].[Roles]([RoleId]),
)