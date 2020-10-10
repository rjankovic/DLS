CREATE PROCEDURE [Adm].[sp_AddUserToRole]
@userId INT,
@roleName NVARCHAR(300),
@projectconfigid UNIQUEIDENTIFIER
AS

INSERT INTO adm.UserRoles (UserId, RoleId)
SELECT @userId, r.RoleId
FROM Adm.Roles r
LEFT JOIN adm.UserRoles ex ON ex.RoleId = r.RoleId AND ex.UserId = @userId
WHERE ISNULL(r.ProjectConfigId, N'00000000-0000-0000-0000-000000000000') = ISNULL(@projectconfigid, N'00000000-0000-0000-0000-000000000000')
AND ex.UserRoleId IS NULL