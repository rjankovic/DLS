CREATE PROCEDURE [Adm].[sp_AddRole]
	@roleName NVARCHAR(MAX),
	@projectConfigId UNIQUEIDENTIFIER
AS
	INSERT INTO [Adm].[Roles] (RoleName,ProjectConfigId)
	VALUES (@roleName, @projectConfigId) 
