CREATE PROCEDURE [Inspect].[sp_FindElement]
	@search NVARCHAR(1000),
	@under NVARCHAR(1000) = ''
AS
  SELECT TOP 100 e.Id, e.Caption, e.RefPath FROM BIDocModelElements e
  WHERE e.RefPath LIKE @under + '%'
  ORDER BY DIFFERENCE(e.Caption, @search) DESC

GO
