CREATE PROCEDURE [Inspect].[sp_TranslateDataFlowNodeDetailLevel]
@nodeId INT,
@sourceDetailLevel INT,
@targetDetailLevel INT
AS

DECLARE @targetDetailGraphKind NVARCHAR(50) =
 IIF(@targetDetailLevel = 1, N'DataFlow', IIF(@targetDetailLevel = 2, N'DataFlowMediumDetail', N'DataFlowLowDetail'))
DECLARE @sourceDetailGraphKind NVARCHAR(50) = 
 IIF(@sourceDetailLevel = 1, N'DataFlow', IIF(@sourceDetailLevel = 2, N'DataFlowMediumDetail', N'DataFlowLowDetail'))

CREATE TABLE #nodeIdMap
(
OldNodeId INT,
NewNodeId INT
)

IF @sourceDetailLevel = @targetDetailLevel
BEGIN
	INSERT INTO #nodeIdMap(OldNodeId, NewNodeId)
	SELECT @nodeId OriginalNodeId, @nodeId NewNodeId
END
ELSE IF @sourceDetailLevel < @targetDetailLevel
BEGIN
	INSERT INTO #nodeIdMap(OldNodeId, NewNodeId)
	SELECT srcN.BasicGraphNodeId OriginalNodeId, tgtN.BasicGraphNodeId NewNodeId 
	FROM BIDoc.BasicGraphNodes srcN
	INNER JOIN BIDoc.HigherLevelElementAncestors anc ON anc.SouceElementId = srcN.SourceElementId
	INNER JOIN BIDoc.BasicGraphNodes tgtN ON tgtN.SourceElementId = anc.AncestorElementId
	WHERE anc.DetailLevel = @targetDetailLevel AND tgtN.GraphKind = @targetDetailGraphKind AND srcN.BasicGraphNodeId = @nodeId
END
ELSE
BEGIN
	INSERT INTO #nodeIdMap(OldNodeId, NewNodeId)
	SELECT srcN.BasicGraphNodeId OriginalNodeId, tgtn.BasicGraphNodeId NewNodeId
	FROM BIDoc.BasicGraphNodes srcN
	INNER JOIN BIDoc.HigherLevelElementAncestors anc ON anc.AncestorElementId = srcN.SourceElementId
	INNER JOIN BIDoc.BasicGraphNodes tgtN ON tgtN.SourceElementId = anc.SouceElementId
	WHERE anc.DetailLevel = @sourceDetailLevel AND tgtN.GraphKind = @targetDetailGraphKind AND srcN.BasicGraphNodeId = @nodeId
END

DECLARE @nodeIds [BIDoc].[UDTT_IdList]

INSERT INTO @nodeIds(Id)
SELECT DISTINCT NewNodeId FROM #nodeIdMap

SELECT
	[BasicGraphNodeId]
	,[Name]
	,[NodeType]
	,[Description]
	,[ParentId]
	,[GraphKind]
	,[ProjectConfigId]
	,[SourceElementId]
	,[TopologicalOrder]
	,[RefPath]
	,TypeDescription
	,ElementType
	,DescriptivePath
FROM [Inspect].[f_GetGraphNodesByIdExtended](@nodeIds)

DROP TABLE #nodeIdMap

