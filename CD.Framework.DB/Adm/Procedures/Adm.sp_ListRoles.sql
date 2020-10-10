CREATE PROCEDURE [Adm].[sp_ListRoles]
	@projectConfigId UNIQUEIDENTIFIER
AS
	SELECT RoleId, RoleName, ProjectConfigId FROM Adm.Roles
	WHERE ISNULL(ProjectConfigId, @projectConfigId) = @projectConfigId
