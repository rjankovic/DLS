CREATE PROCEDURE [Adm].[sp_ListUsers]
AS
	SELECT UserId, DisplayName, [Identity] 
	FROM adm.Users 
	WHERE DisplayName IS NOT NULL