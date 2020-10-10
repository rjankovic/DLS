CREATE PROCEDURE [Adm].[sp_SecurityQuery]
	@userId INT,
	@permission NVARCHAR(100),
	@scope NVARCHAR(MAX)
AS
	IF @scope IS NULL
	BEGIN
		SET @scope = N''
	END

	IF EXISTS(
		SELECT TOP 1 1 FROM adm.UserRoles r
		INNER JOIN adm.RolePermissions rp ON rp.RoleId = r.RoleId
		INNER JOIN adm.Permissions p ON p.PermissionId = rp.PermissionId
		WHERE r.UserId = @userId AND p.PermissionName = @permission 
			-- the granted permission is global or is a superset (prefix) of the required permission
			AND (p.PermissionScope IS NULL OR LEFT(@scope, LEN(p.PermissionScope)) = p.PermissionScope)
	)
	BEGIN
		SELECT 1 Result, NULL [Message]
	END
	ELSE BEGIN
		SELECT 0 Result, N'The permission has not been granted' [Message]
	END
RETURN 0
