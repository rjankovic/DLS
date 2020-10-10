CREATE FUNCTION [Search].[f_GetRootElements]
(
	@projectConfigId UNIQUEIDENTIFIER
)
RETURNS TABLE AS RETURN
(
	SELECT e.ModelElementId, r.Caption, e.Type ElementType, e.RefPathPrefix
	FROM Search.RootElements r
	INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = r.ModelElementId
	WHERE r.ProjectConfigId = @projectConfigId
)
