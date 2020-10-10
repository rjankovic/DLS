CREATE FUNCTION [Adm].[f_GetUserDataByIdentity]
(
@userIdentity NVARCHAR(300)
)
RETURNS TABLE
AS RETURN
(
SELECT [UserId],
	   [Identity],
	   [DisplayName]
  FROM [Adm].[Users]
  WHERE [Identity] = @userIdentity
)