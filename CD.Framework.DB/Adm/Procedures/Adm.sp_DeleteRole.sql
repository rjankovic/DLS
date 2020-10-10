CREATE PROCEDURE [Adm].[sp_DeleteRole]
	@roleId INT
AS

DELETE [Adm].[UserRoles] FROM [Adm].[UserRoles] ur
WHERE ur.RoleId = @roleId

DELETE [Adm].[RolePermissions] FROM [Adm].[RolePermissions] rp
WHERE rp.RoleId = @roleId

DELETE [Adm].[Roles] FROM [Adm].[Roles]
WHERE RoleId = @roleId