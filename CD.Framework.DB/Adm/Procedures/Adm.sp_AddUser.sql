CREATE PROCEDURE [Adm].[sp_AddUser]
	@userIdentity NVARCHAR(300),
	@displayName NVARCHAR(MAX)
AS
IF NOT EXISTS(SELECT TOP 1 1 FROM adm.Users WHERE [Identity] = @userIdentity)
BEGIN
	INSERT INTO [Adm].[Users] ([Identity]) VALUES(@userIdentity);
END
UPDATE adm.Users SET DisplayName = @displayName WHERE [Identity] = @userIdentity

INSERT INTO adm.UserRoles (UserId, RoleId)
SELECT u.UserId, r.RoleId FROM 
adm.Users u
CROSS JOIN adm.Roles r
LEFT JOIN adm.UserRoles ur ON u.UserId = ur.UserId AND r.RoleId = ur.RoleId
WHERE u.[Identity] = @userIdentity AND r.RoleName = N'CustomerUser' AND ur.UserRoleId IS NULL