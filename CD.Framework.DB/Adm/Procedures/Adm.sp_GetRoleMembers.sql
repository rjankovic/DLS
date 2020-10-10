CREATE PROCEDURE [Adm].[sp_GetRoleMembers]
	@roleId INT 
AS 

SELECT u.DisplayName, u.UserId
FROM [Adm].[UserRoles] ur
INNER JOIN [Adm].[Users] u ON ur.UserId = u.UserId
WHERE ur.RoleId = @roleId AND u.DisplayName IS NOT NULL