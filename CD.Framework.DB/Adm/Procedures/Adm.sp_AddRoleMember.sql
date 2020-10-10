CREATE PROCEDURE [Adm].[sp_AddRoleMember]
	@roleId INT,
	@userId INT
AS

INSERT INTO [Adm].[UserRoles]
(UserId, RoleId)
VALUES (@userId, @roleId)