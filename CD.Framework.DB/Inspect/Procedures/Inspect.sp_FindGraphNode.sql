CREATE PROCEDURE [Inspect].[sp_FindGraphNode]
	@search NVARCHAR(1000),
	@under NVARCHAR(1000) = '',
	@projectId UNIQUEIDENTIFIER = NULL
AS
  SELECT TOP 100 n.BasicGraphInfoNodeId, n.Name, n.RefPath FROM BasicGraphInfoNodes n
  WHERE n.RefPath LIKE @under + '%'
  AND n.ProjectConfigId = ISNULL(@projectId, n.ProjectConfigId)
  AND GraphKind = 0
  ORDER BY DIFFERENCE(n.Name, @search) DESC
GO