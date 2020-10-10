CREATE FUNCTION [BIDoc].[f_GetGraphNodeIdByRefPath]
(
	@projectconfigid UNIQUEIDENTIFIER,
	@graphkind NVARCHAR(50),
	@refpath NVARCHAR(MAX)
)
RETURNS TABLE AS RETURN
(
SELECT n.[BasicGraphNodeId]
FROM BIDoc.BasicGraphNodes n
INNER JOIN BIDoc.ModelElements e ON n.SourceElementId = e.ModelElementId 
WHERE GraphKind = @graphkind AND e.ProjectConfigId = @projectconfigid AND e.RefPathPrefix = CONVERT(NVARCHAR(300), LEFT(@refpath, 300)) AND e.RefPath = @refPath
)
