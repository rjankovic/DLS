
INSERT INTO [Adm].[Roles]
([RoleName])
SELECT roles.RoleName
FROM   (VALUES
	--(N'ProjectCreator'),	-- system
	(N'CustomerUser'),	-- system; the role gained by assignment to AAD group corresponding to the customer
	--(N'ProjectAdmin'),	-- project
	(N'CustomerAdmin')		-- system
	) roles(RoleName)
LEFT JOIN [Adm].[Roles] ex ON ex.RoleName = roles.RoleName
WHERE ex.RoleId IS NULL


INSERT INTO [Adm].[Permissions]
([PermissionName])
SELECT perms.PermissionName
FROM   (VALUES
	--(N'CreateProject'),
	(N'ManageProject'),
	(N'ViewLineage'),
	(N'UpdateLineage'),
	(N'ViewAnnotations'),
	(N'EditAnnotations'),
	(N'EditPermissions')
	) perms(PermissionName)
LEFT JOIN [Adm].[Permissions] ex ON ex.PermissionName = perms.PermissionName
WHERE ex.PermissionId IS NULL

INSERT INTO [Adm].[RolePermissions]([RoleId], [PermissionId])
SELECT r.RoleId, p.PermissionId FROM
(VALUES
	(N'CustomerUser', N'ViewLineage'),
	(N'CustomerUser', N'ViewAnnotations'),
	(N'CustomerUser', N'EditAnnotations'),

	--(N'CustomerAdmin', N'CreateProject'),
	(N'CustomerAdmin', N'ManageProject'),
	(N'CustomerAdmin', N'ViewLineage'),
	(N'CustomerAdmin', N'UpdateLineage'),
	(N'CustomerAdmin', N'ViewAnnotations'),
	(N'CustomerAdmin', N'EditAnnotations'),
	(N'CustomerAdmin', N'EditPermissions')
	--,
	--(N'ProjectAdmin', N'ManageProject'),
	--(N'ProjectAdmin', N'ViewLineage'),
	--(N'ProjectAdmin', N'UpdateLineage'),
	--(N'ProjectAdmin', N'ViewAnnotations'),
	--(N'ProjectAdmin', N'EditAnnotations')
	) perms(RoleName, PermissionName)
INNER JOIN adm.Roles r ON r.RoleName = perms.RoleName
INNER JOIN adm.Permissions p ON p.PermissionName = perms.PermissionName
LEFT JOIN adm.RolePermissions rp ON rp.PermissionId = p.PermissionId AND rp.RoleId = r.RoleId
WHERE r.ProjectConfigId IS NULL AND rp.RolePermissionId IS NULL
