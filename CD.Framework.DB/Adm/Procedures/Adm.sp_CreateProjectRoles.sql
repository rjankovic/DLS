CREATE PROCEDURE [Adm].[sp_CreateProjectRoles]
@projectconfigid UNIQUEIDENTIFIER
AS

INSERT INTO [Adm].[Roles]([RoleName], [ProjectConfigId])
SELECT N'ProjectAdmin', @projectconfigid


INSERT INTO [Adm].[RolePermissions]([RoleId], [PermissionId])
SELECT r.RoleId, p.PermissionId FROM

(VALUES
	(N'ProjectAdmin', N'ManageProject'),
	(N'ProjectAdmin', N'ViewLineage'),
	(N'ProjectAdmin', N'UpdateLineage'),
	(N'ProjectAdmin', N'ViewAnnotations'),
	(N'ProjectAdmin', N'EditAnnotations')
	) perms(RoleName, PermissionName)
INNER JOIN adm.Roles r ON r.RoleName = perms.RoleName
INNER JOIN adm.Permissions p ON p.PermissionName = perms.PermissionName
WHERE r.ProjectConfigId = @projectconfigid