CREATE PROCEDURE [Adm].[sp_AddUserRoles]
	@roleId INT,
    @userId INT
AS
	INSERT INTO  [Adm].[UserRoles] (UserId, RoleId)
	VALUES (@userId, @roleId)
RETURN 0
