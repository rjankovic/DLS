CREATE PROCEDURE [Inspect].[sp_GetDataFlowLinksToNode]
	@targetNodeId INT
AS
	SELECT 
	l.BasicGraphLinkId, 
	l.NodeFromId, 
	l.NodeToId 
	FROM BIDoc.BasicGraphLinks l
	INNER JOIN BIDoc.BasicGraphNodes n ON n.BasicGraphNodeId = l.NodeToId
	WHERE l.LinkType = N'DataFlow' AND n.BasicGraphNodeId = @targetNodeId
