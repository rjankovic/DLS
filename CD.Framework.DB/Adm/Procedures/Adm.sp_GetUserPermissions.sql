CREATE PROCEDURE [Adm].[sp_GetUserPermissions]
	@projectConfigId UNIQUEIDENTIFIER,
	@userId INT
AS 

SELECT DISTINCT p.PermissionName, p.PermissionId
--, PermissionScope
FROM [Adm].[RolePermissions] rp
INNER JOIN [Adm].[Roles] r ON rp.RoleId = r.RoleId
INNER JOIN [Adm].[Permissions] p ON rp.PermissionId = p.PermissionId
INNER JOIN adm.UserRoles ur ON ur.RoleId = r.RoleId
WHERE ur.UserId = @userId AND (ProjectConfigId = @projectConfigId OR ProjectConfigId IS NULL)