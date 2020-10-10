CREATE PROCEDURE [Adm].[sp_ListPermissions]
AS 

SELECT p.PermissionName, p.PermissionId
--, PermissionScope
FROM [Adm].[Permissions] p