CREATE PROCEDURE [BIDoc].[sp_CreateDataFlowMediumDetailGraph]
	@projectConfigId UNIQUEIDENTIFIER
AS
--DECLARE @projectConfigId UNIQUEIDENTIFIER = N'87E5A6F8-7ADD-4292-8537-E422EE66AD61'

DECLARE @sourceGraphKind NVARCHAR(50) = N'DataFlow'
DECLARE @targetGraphKind NVARCHAR(50) = N'DataFlowMediumDetail'

--------- clear graph

EXEC BIDoc.sp_ClearGraph @projectconfigid, @targetgraphkind

---------------------
CREATE TABLE #nodeIdMap
(
OldNodeId INT,
NewNodeId INT,
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
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
INNER JOIN BIDoc.ModelElementTypeDetailLevels dl ON dl.ElementType = e.[Type]
WHERE 
	n.ProjectConfigId = @projectconfigid 
	AND n.GraphKind  = @sourcegraphkind
	AND dl.DetailLevel >= 2

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
SELECT DISTINCT
l.LinkType,
mf.NewNodeId,
mt.NewNodeId
FROM BIDoc.BasicGraphLinks l 
INNER JOIN BIDoc.BasicGraphNodes nf ON l.NodeFromId = nf.BasicGraphNodeId
INNER JOIN BIDoc.HigherLevelElementAncestors ancf ON ancf.SouceDfNodeId = l.NodeFromId
INNER JOIN BIDoc.HigherLevelElementAncestors anct ON anct.SouceDfNodeId = l.NodeToId
INNER JOIN #nodeIdMap mf ON mf.OldNodeId = ancf.AncestorDfNodeId
INNER JOIN #nodeIdMap mt ON mt.OldNodeId = anct.AncestorDfNodeId
WHERE /*l.LinkType <> @linktype AND*/ 
nf.ProjectConfigId = @projectconfigid AND nf.GraphKind = @sourcegraphkind
AND ancf.DetailLevel = 2
AND anct.DetailLevel = 2
AND mf.NewNodeid <> mf.OldNodeId

DROP TABLE #nodeIdMap
