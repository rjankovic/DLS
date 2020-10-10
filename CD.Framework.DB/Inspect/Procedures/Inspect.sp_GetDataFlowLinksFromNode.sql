CREATE PROCEDURE [Inspect].[sp_GetDataFlowLinksFromNode]
	@sourceNodeId INT
AS
	SELECT 
	l.BasicGraphLinkId, 
	l.NodeFromId, 
	l.NodeToId 
	FROM BIDoc.BasicGraphLinks l
	INNER JOIN BIDoc.BasicGraphNodes n ON n.BasicGraphNodeId = l.NodeFromId
	WHERE l.LinkType = N'DataFlow' AND n.BasicGraphNodeId = @sourceNodeId
