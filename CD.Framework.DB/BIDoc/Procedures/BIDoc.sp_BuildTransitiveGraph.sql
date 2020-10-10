CREATE PROCEDURE [BIDoc].[sp_BuildTransitiveGraph]
	--DECLARE 
	@projectconfigid UNIQUEIDENTIFIER --= N'e99a3b4e-7f04-4b98-9780-10e71e6258cf'
	--DECLARE 
	,@sourcegraphkind NVARCHAR(50) --= N'DataFlow'
	--DECLARE 
	,@targetgraphkind NVARCHAR(50) --= N'DataFlowTransitive'
	--DECLARE 
	,@linktype NVARCHAR(50) --= N'DataFlow'
AS


---------------------- clear graph

EXEC BIDoc.sp_ClearGraph @projectconfigid, @targetgraphkind

------------------------ replicate graph

CREATE TABLE #nodeIdMap
(
OldNodeId INT,
NewNodeid INT,
)


INSERT INTO BIDoc.BasicGraphNodes
           (--[BasicGraphNodeId]
           --,
		   [Name]
           ,[NodeType]
           ,[Description]
           ,[ParentId]
           ,[GraphKind]
           ,[ProjectConfigId]
           ,[SourceElementId]
           ,[TopologicalOrder])
SELECT 
			--mid.NewNodeid
			--,
			n.Name
			,n.NodeType
			,n.Description
			,NULL --pid.NewNodeid
			,@targetgraphkind
			,@projectconfigid
			,n.SourceElementId
			,0 TopologicalOrder
FROM BIDoc.BasicGraphNodes n
--INNER JOIN #nodeIdMap mid ON mid.OldNodeId = n.BasicGraphNodeId
--LEFT JOIN #nodeIdMap pid ON pid.OldNodeId = n.ParentId
WHERE n.ProjectConfigId = @projectconfigid AND n.GraphKind  = @sourcegraphkind

INSERT INTO #nodeIdMap(OldNodeId, NewNodeid)
SELECT o.BasicGraphNodeId, n.BasicGraphNodeId
FROM BIDoc.BasicGraphNodes o
INNER JOIN BIDoc.BasicGraphNodes n ON o.SourceElementId = n.SourceElementId
WHERE o.ProjectConfigId = @projectconfigid AND n.ProjectConfigId = @projectconfigid
	AND o.GraphKind = @sourcegraphkind AND n.GraphKind = @targetgraphkind

CREATE NONCLUSTERED INDEX TIX_nodeIdMap_OldNodeId ON #nodeIdMap(OldNodeId ASC)
CREATE NONCLUSTERED INDEX TIX_nodeIdMap_NewNodeId ON #nodeIdMap(NewNodeId ASC)

UPDATE n SET ParentId = np.NewNodeid FROM BIDoc.BasicGraphNodes n
INNER JOIN #nodeIdMap m ON n.BasicGraphNodeId = m.NewNodeid
INNER JOIN BIDoc.BasicGraphNodes o ON o.BasicGraphNodeId = m.OldNodeId
INNER JOIN #nodeIdMap np ON np.OldNodeId = o.ParentId

INSERT INTO BIDoc.BasicGraphLinks
(
	--BasicGraphLinkId, 
	LinkType, 
	NodeFromId, 
	NodeToId
)
SELECT --ROW_NUMBER() OVER(ORDER BY l.NodeFromId) + @linkSequenceStart - 1,
l.LinkType,
mf.NewNodeid,
mt.NewNodeid
FROM BIDoc.BasicGraphLinks l 
INNER JOIN BIDoc.BasicGraphNodes n ON l.NodeFromId = n.BasicGraphNodeId
INNER JOIN #nodeIdMap mf ON mf.OldNodeId = l.NodeFromId
INNER JOIN #nodeIdMap mt ON mt.OldNodeId = l.NodeToId
WHERE /*l.LinkType <> @linktype AND*/ n.ProjectConfigId = @projectconfigid AND n.GraphKind = @sourcegraphkind


---------------------------------------------------------------------------------------------
------------------------------------ ensure transitivity

DECLARE @rc INT = 1
WHILE (@rc > 0)
BEGIN
	INSERT INTO BIDoc.BasicGraphLinks
	(NodeFromId, NodeToId, LinkType)
	SELECT  
	DISTINCT --TOP 1000000
	sl.NodeFromId
	,tl.NodeToId
	,@linktype
	FROM BIDoc.BasicGraphNodes sn
	INNER JOIN BIDoc.BasicGraphLinks sl ON sl.NodeFromId = sn.BasicGraphNodeId
	INNER JOIN BIDoc.BasicGraphLinks tl ON tl.NodeFromId = sl.NodeToId
	LEFT JOIN BIDoc.BasicGraphLinks dl ON dl.LinkType = @linktype 
		AND dl.NodeFromId = sl.NodeFromId AND dl.NodeToId = tl.NodeToId
	WHERE sn.GraphKind = @targetgraphkind AND sl.LinkType = @linktype AND tl.LinkType = @linktype
	AND dl.BasicGraphLinkId IS NULL AND sn.ProjectConfigId = @projectconfigid
	
	SET @rc = @@ROWCOUNT
	PRINT CONVERT(NVARCHAR(100), @rc) + ' new transitive links'
END


----------------------------------------------------------------------------------------------
-------------------------------------- cleanup

DROP TABLE #nodeIdMap


GO


