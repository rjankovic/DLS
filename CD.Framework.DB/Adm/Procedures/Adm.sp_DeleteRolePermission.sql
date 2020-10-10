CREATE PROCEDURE [Adm].[sp_DeleteRolePermission]
	@roleId INT,
	@permissionId INT
AS
DELETE [Adm].[RolePermissions] FROM [Adm].[RolePermissions] rp
WHERE rp.RoleId = @roleId AND rp.PermissionId = @permissionId