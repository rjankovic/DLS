CREATE PROCEDURE [BIDoc].[sp_AddNodesToGraph]
	@projectconfigid UNIQUEIDENTIFIER,
	@graphkind NVARCHAR(50),
	@nodes  [BIDoc].[UDTT_BasicGraphNodes] READONLY
AS
INSERT INTO [BIDoc].[BasicGraphNodes]
(
	--[BasicGraphNodeId]
      --,
	  [Name]
      ,[NodeType]
      ,[Description]
      ,[ParentId]
      ,[GraphKind]
      ,[ProjectConfigId]
      ,[SourceElementId]
      ,[TopologicalOrder]
)
SELECT
--[BasicGraphNodeId]
      --,
	  [Name]
      ,[NodeType]
      ,[Description]
      ,NULL --[ParentId]
      ,@graphkind
      ,@projectconfigid
      ,[SourceElementId]
      ,[TopologicalOrder]
FROM @nodes


SELECT o.BasicGraphNodeId SequentialGraphNodeId, n.BasicGraphNodeId 
INTO #nodeIdMap
FROM BIDoc.BasicGraphNodes n
INNER JOIN @nodes o ON o.SourceElementId = n.SourceElementId AND o.NodeType = n.NodeType AND @graphkind = n.GraphKind


-- map new nodes to their sequential sources, map parents of those to the new IDs and assign new parent IDs
UPDATE n SET n.ParentId = pmap.BasicGraphNodeId
FROM BIDoc.BasicGraphNodes n
INNER JOIN #nodeIdMap map ON map.BasicGraphNodeId = n.BasicGraphNodeId
INNER JOIN @nodes o ON o.BasicGraphNodeId = map.SequentialGraphNodeId
INNER JOIN @nodes po ON po.BasicGraphNodeId = o.ParentId
INNER JOIN #nodeIdMap pmap ON pmap.SequentialGraphNodeId = po.BasicGraphNodeId

SELECT SequentialGraphNodeId, BasicGraphNodeId FROM #nodeIdMap

DROP TABLE #nodeIdMap

