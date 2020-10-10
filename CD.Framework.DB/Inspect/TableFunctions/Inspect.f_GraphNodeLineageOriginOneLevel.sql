CREATE FUNCTION [Inspect].[f_GraphNodeLineageOriginOneLevel]
(
  @nodeId INT
)
RETURNS TABLE
AS RETURN
(
  -- the selected node and all descendants
  WITH descendants AS
  (
	SELECT e.BasicGraphNodeId, e.Name, e.Description 
	FROM BIDoc.BasicGraphNodes e WHERE e.BasicGraphNodeId = @nodeId
	
	UNION ALL

	SELECT e.BasicGraphNodeId, e.Name, e.Description
	FROM descendants d
	INNER JOIN BIDoc.BasicGraphNodes e ON e.ParentId = d.BasicGraphNodeId
  )
  SELECT DISTINCT
		n.[BasicGraphNodeId]
		,n.[Name]
		,n.[NodeType]
		,n.[Description]
		,n.[ParentId]
		,n.[GraphKind]
		,n.[ProjectConfigId]
		,n.[SourceElementId]
		,n.[TopologicalOrder]
		,e.[RefPath]
		FROM BIDoc.BasicGraphLinks lnk
		INNER JOIN descendants l ON l.BasicGraphNodeId = lnk.NodeToId
		INNER JOIN BIDoc.BasicGraphNodes n ON n.BasicGraphNodeId = lnk.NodeFromId
		INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
		LEFT JOIN descendants cycle ON cycle.BasicGraphNodeId = n.BasicGraphNodeId
		WHERE cycle.BasicGraphNodeId IS NULL AND lnk.LinkType IN ('DataFlow')
)