CREATE PROCEDURE [Inspect].[sp_TranslateDataFlowLinksDetailLevel]
@linkIds [BIDoc].[UDTT_IdList] READONLY,
@sourceDetailLevel INT,
@targetDetailLevel INT
AS
--DECLARE @linkIds [BIDoc].[UDTT_IdList]
--DECLARE @sourceDetailLevel INT = 1
--DECLARE @targetDetailLevel INT = 3

DECLARE @targetDetailGraphKind NVARCHAR(50) =
 IIF(@targetDetailLevel = 1, N'DataFlow', IIF(@targetDetailLevel = 2, N'DataFlowMediumDetail', N'DataFlowLowDetail'))
DECLARE @sourceDetailGraphKind NVARCHAR(50) = 
 IIF(@sourceDetailLevel = 1, N'DataFlow', IIF(@sourceDetailLevel = 2, N'DataFlowMediumDetail', N'DataFlowLowDetail'))

;WITH linkNodes AS
(
SELECT DISTINCT l.NodeFromId Id FROM BIDoc.BasicGraphLinks l
INNER JOIN @linkIds lid ON l.BasicGraphLinkId = lid.Id
UNION
SELECT DISTINCT l.NodeToId Id FROM BIDoc.BasicGraphLinks l
INNER JOIN @linkIds lid ON l.BasicGraphLinkId = lid.Id
)
SELECT n.Id INTO #nodeIds 
FROM linkNodes n

CREATE TABLE #nodeIdMap
(
OldNodeId INT,
NewNodeId INT
)

IF @sourceDetailLevel = @targetDetailLevel
BEGIN
	INSERT INTO #nodeIdMap(OldNodeId, NewNodeId)
	SELECT nl.Id OriginalNodeId, nl.Id NewNodeId FROM #nodeIds nl
END
ELSE IF @sourceDetailLevel < @targetDetailLevel
BEGIN
	INSERT INTO #nodeIdMap(OldNodeId, NewNodeId)
	SELECT srcN.BasicGraphNodeId OriginalNodeId, tgtN.BasicGraphNodeId NewNodeId 
	FROM #nodeIds nl
	INNER JOIN BIDoc.BasicGraphNodes srcN ON srcN.BasicGraphNodeId = nl.Id
	INNER JOIN BIDoc.HigherLevelElementAncestors anc ON anc.SouceElementId = srcN.SourceElementId
	INNER JOIN BIDoc.BasicGraphNodes tgtN ON tgtN.SourceElementId = anc.AncestorElementId
	WHERE anc.DetailLevel = @targetDetailLevel AND tgtN.GraphKind = @targetDetailGraphKind
END
ELSE
BEGIN
	INSERT INTO #nodeIdMap(OldNodeId, NewNodeId)
	SELECT srcN.BasicGraphNodeId OriginalNodeId, tgtn.BasicGraphNodeId NewNodeId
	FROM #nodeIds nl
	INNER JOIN BIDoc.BasicGraphNodes srcN ON srcN.BasicGraphNodeId = nl.Id
	INNER JOIN BIDoc.HigherLevelElementAncestors anc ON anc.AncestorElementId = srcN.SourceElementId
	INNER JOIN BIDoc.BasicGraphNodes tgtN ON tgtN.SourceElementId = anc.SouceElementId
	WHERE anc.DetailLevel = @sourceDetailLevel AND tgtN.GraphKind = @targetDetailGraphKind
END

SELECT DISTINCT
	nl.BasicGraphLinkId, 
	nl.NodeFromId, 
	nl.NodeToId 
FROM 
@linkIds li
INNER JOIN BIDoc.BasicGraphLinks ol ON ol.BasicGraphLinkId = li.Id
INNER JOIN #nodeIdMap nmFrom ON ol.NodeFromId = nmFrom.OldNodeId
INNER JOIN #nodeIdMap nmTo ON ol.NodeToId = nmTo.OldNodeId
INNER JOIN BIDoc.BasicGraphLinks nl ON nl.NodeFromId = nmFrom.NewNodeId AND nl.NodeToId = nmTo.NewNodeId

DROP TABLE #nodeIdMap
DROP TABLE #nodeIds

