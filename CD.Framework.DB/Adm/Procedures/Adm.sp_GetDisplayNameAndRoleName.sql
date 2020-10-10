CREATE PROCEDURE [Adm].[sp_GetDisplayNameAndRoleName]
	@roleName NVARCHAR(MAX)
AS
	SELECT u.DisplayName, r.RoleName FROM [Adm].[UserRoles] ur
	INNER JOIN [Adm].[Users] u ON ur.UserId = u.UserId
	INNER JOIN [Adm].[Roles] r ON r.RoleId = r.RoleId
	WHERE DisplayName IS NOT NULL AND r.RoleName = @roleName 
RETURN