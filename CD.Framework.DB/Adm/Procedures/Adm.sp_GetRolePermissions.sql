CREATE PROCEDURE [Adm].[sp_GetRolePermissions]
	@roleId INT 
AS 

SELECT p.PermissionName, p.PermissionId
--, PermissionScope
FROM [Adm].[RolePermissions] rp
INNER JOIN [Adm].[Permissions] p ON rp.PermissionId = p.PermissionId
WHERE rp.RoleId = @roleId