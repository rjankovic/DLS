CREATE PROCEDURE [Adm].[sp_AddRolePermission]
	@permissionId INT,
	@roleId INT
AS

INSERT INTO [Adm].[RolePermissions]
(RoleId, PermissionId)
VALUES
(@roleId, @permissionId)
