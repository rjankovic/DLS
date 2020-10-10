CREATE PROCEDURE [Adm].[sp_DeleteRoleMember]
	@userId INT,
	@roleId INT
AS
DELETE [Adm].[UserRoles] FROM [Adm].[UserRoles] ur
WHERE  ur.UserId = @userId AND ur.RoleId = @roleId
