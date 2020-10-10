CREATE PROCEDURE [Adm].[spRemovedUserFromRole]
@userId INT,
@roleName NVARCHAR(300),
@projectconfigid UNIQUEIDENTIFIER
AS

DELETE ur FROM adm.UserRoles ur
INNER JOIN Adm.Roles r ON r.RoleId = ur.RoleId
WHERE ISNULL(r.ProjectConfigId, N'00000000-0000-0000-0000-000000000000') = ISNULL(@projectconfigid, N'00000000-0000-0000-0000-000000000000')
AND ur.UserId = @userId
