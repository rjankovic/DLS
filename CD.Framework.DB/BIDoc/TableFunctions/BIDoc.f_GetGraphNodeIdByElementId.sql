CREATE FUNCTION [BIDoc].[f_GetGraphNodeIdByElementId]
(
	@elementid INT,
	@graphkind NVARCHAR(50)
)
RETURNS TABLE AS RETURN
(
SELECT n.[BasicGraphNodeId]
FROM BIDoc.BasicGraphNodes n
INNER JOIN BIDoc.ModelElements e ON n.SourceElementId = e.ModelElementId 
WHERE GraphKind = @graphkind AND e.ModelElementId = @elementid
)
