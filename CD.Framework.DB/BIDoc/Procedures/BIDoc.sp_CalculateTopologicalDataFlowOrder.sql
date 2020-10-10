CREATE PROCEDURE [BIDoc].[sp_CalculateTopologicalDataFlowOrder]
	--DECLARE 
	@projectconfigid UNIQUEIDENTIFIER --= N'e99a3b4e-7f04-4b98-9780-10e71e6258cf'
AS
DECLARE @linktype NVARCHAR(50) = N'DataFlow'
DECLARE @graphkind NVARCHAR(50) = N'DataFlow'
DECLARE @topol INT = 2
DECLARE @rc INT

UPDATE BIDoc.BasicGraphNodes SET TopologicalOrder = 0 WHERE GraphKind = @graphkind AND ProjectConfigId = @projectconfigid

-- set the topological order 1 to nodes that contribute the links from another node to itself (e.g. merge statemtnts, updates that read and modify the same column)

UPDATE dfn SET TopologicalOrder = 1
FROM BIDoc.BasicGraphLinks l 
INNER JOIN BIDoc.BasicGraphNodes n ON l.NodeFromId = n.BasicGraphNodeId
INNER JOIN BIDoc.BasicGraphLinks lfr ON lfr.NodeFromId = l.NodeFromId AND lfr.LinkType = @linktype AND lfr.NodeToId <> l.NodeToId
INNER JOIN BIDoc.BasicGraphLinks lto ON lto.NodeToId = l.NodeFromId AND lto.LinkType = @linktype AND lto.NodeFromId <> l.NodeFromId
INNER JOIN BIDoc.BasicGraphNodes midn ON midn.BasicGraphNodeId = lfr.NodeToId AND midn.BasicGraphNodeId = lto.NodeFromId
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = midn.SourceElementId
INNER JOIN BIDoc.BasicGraphNodes dfn ON dfn.SourceElementId = e.ModelElementId AND dfn.GraphKind = @graphkind
WHERE l.NodeFromId = l.NodeToId AND l.LinkType = @linktype
AND n.GraphKind = N'DataFlowTransitive' AND midn.NodeType <> N'ColumnElement' AND n.ProjectConfigId = @projectconfigid


-- while there is a node with incoming link(s) without an ordwer assigned
WHILE EXISTS(
SELECT TOP 1 1 FROM BIDoc.BasicGraphNodes n 
INNER JOIN BIDoc.BasicGraphLinks l ON l.NodeToId = n.BasicGraphNodeId
WHERE n.TopologicalOrder = 0 AND n.ProjectConfigId = @projectconfigid AND n.GraphKind = @graphkind AND l.LinkType = @linktype
)
BEGIN
	
	-- assign new topological order to all nodes for which all incoming links come from nodes that already have their number assigned
	UPDATE n SET n.TopologicalOrder = @topol
	FROM BIDoc.BasicGraphNodes n
	LEFT JOIN BIDoc.BasicGraphLinks l ON l.NodeToId = n.BasicGraphNodeId AND l.LinkType = @linktype
	LEFT JOIN BIDoc.BasicGraphNodes srcN ON srcN.BasicGraphNodeId = l.NodeFromId AND srcN.TopologicalOrder = 0
	WHERE n.GraphKind = @graphkind AND n.ProjectConfigId = @projectconfigid AND srcN.BasicGraphNodeId IS NULL AND n.TopologicalOrder = 0

	SET @rc = @@ROWCOUNT

	PRINT CONVERT(NVARCHAR(255), @rc) + N' nodes assigned topological order of ' + CONVERT(NVARCHAR(255), @topol)

	IF @rc = 0 BEGIN
		IF EXISTS(
			SELECT TOP 1 1 FROM BIDoc.BasicGraphNodes n 
			INNER JOIN BIDoc.BasicGraphLinks l ON l.NodeToId = n.BasicGraphNodeId
			WHERE n.TopologicalOrder = 0 AND n.ProjectConfigId = @projectconfigid AND n.GraphKind = @graphkind AND l.LinkType = @linktype
			)
			BEGIN
			SELECT n.* FROM BIDoc.BasicGraphNodes n 
			INNER JOIN BIDoc.BasicGraphLinks l ON l.NodeToId = n.BasicGraphNodeId
			WHERE n.TopologicalOrder = 0 AND n.ProjectConfigId = @projectconfigid AND n.GraphKind = @graphkind AND l.LinkType = @linktype

			RAISERROR(N'Unreachable nodes detected', 16, 1)
			RETURN
			END
	END

	SET @topol = @topol + 1
END


-- set parent topology as the max of child topological orders
WHILE EXISTS(
	SELECT TOP 1 1 FROM BIDoc.BasicGraphNodes n
	INNER JOIN BIDoc.BasicGraphNodes cn ON cn.ParentId = n.BasicGraphNodeId
	WHERE n.ProjectConfigId = @projectconfigid AND n.GraphKind = @graphkind AND n.TopologicalOrder < cn.TopologicalOrder
)
BEGIN
	UPDATE n SET TopologicalOrder = cn.TopologicalOrder
	FROM BIDoc.BasicGraphNodes n
	INNER JOIN BIDoc.BasicGraphNodes cn ON cn.ParentId = n.BasicGraphNodeId
	WHERE n.ProjectConfigId = @projectconfigid AND n.GraphKind = @graphkind AND n.TopologicalOrder < cn.TopologicalOrder
	
END

-- copy the order from to DataFlowTransitive
UPDATE n SET TopologicalOrder = dfn.TopologicalOrder FROM
BIDoc.BasicGraphNodes n 
INNER JOIN BIDoc.BasicGraphNodes dfn ON dfn.GraphKind = @graphkind AND dfn.SourceElementId = n.SourceElementId
WHERE n.GraphKind = N'DataFlowTransitive' AND n.ProjectConfigId = @projectconfigid

GO


