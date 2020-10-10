CREATE PROCEDURE [BIDoc].[sp_BuildHigherDataFlowSequences]
	@projectconfigid UNIQUEIDENTIFIER
AS


DECLARE @id NVARCHAR(100)
SET @id = REPLACE(CAST(@projectconfigid AS NVARCHAR(100)),'-','')	
DECLARE @graphkind NVARCHAR(50) = N'DataFlow'

--DECLARE @SQLTask3 NVARCHAR(MAX)
--	SET @SQLTask3 = 'TRUNCATE TABLE [' + @id + '].[DataFlowSequenceSteps_Heap]
--					 TRUNCATE TABLE [' + @id + '].[DataFlowSequences_Heap]'
--	EXEC sp_executesql  @SQLTask3
	

DECLARE @SQLTask2 NVARCHAR(MAX)

SELECT hla.SouceElementId, hla.DetailLevel, hla.AncestorElementId, n.BasicGraphNodeId, ancn.BasicGraphNodeId AncestorNodeId, ae.[Type] AncestorElementType, ae.Caption AncestorElementName
INTO #upperLevels
FROM BIDoc.HigherLevelElementAncestors hla
INNER JOIN BIDoc.BasicGraphNodes n ON n.SourceElementId = hla.SouceElementId
INNER JOIN BIDoc.ModelElements ae ON ae.ModelElementId = hla.AncestorElementId
INNER JOIN BIDoc.BasicGraphNodes ancn ON ancn.SourceElementId = hla.AncestorElementId
WHERE n.GraphKind = @graphkind AND ancn.GraphKind = @graphkind AND ae.ProjectConfigId = @projectconfigid


;WITH nodes as
(
SELECT n.BasicGraphNodeId OriginalNodeId, n.BasicGraphNodeId, n.ParentId, e.ModelElementId, e.Caption, e.Type, lvl.DetailLevel
FROM BIDoc.BasicGraphNodes n
INNER JOIN BIDoc.ModelElements e ON n.SourceElementId = e.ModelElementId
INNER JOIN BIDoc.ModelElementTypeDetailLevels lvl ON lvl.ElementType = e.Type
WHERE n.GraphKind = @graphkind AND n.ProjectConfigId = @projectconfigid
)
SELECT dl.*, 
cls1.AncestorNodeId Detail_1_NodeId, cls1.AncestorElementType Detail_1_Type, cls1.AncestorElementName Detail_1_Name,
cls2.AncestorNodeId Detail_2_NodeId, cls2.AncestorElementType Detail_2_Type, cls2.AncestorElementName Detail_2_Name,
cls3.AncestorNodeId Detail_3_NodeId, cls3.AncestorElementType Detail_3_Type, cls3.AncestorElementName Detail_3_Name
INTO #upperLevelNodeMapping
FROM nodes dl
LEFT JOIN #upperLevels cls1 ON cls1.BasicGraphNodeId = dl.OriginalNodeId AND cls1.DetailLevel = 1
LEFT JOIN #upperLevels cls2 ON cls2.BasicGraphNodeId = dl.OriginalNodeId AND cls2.DetailLevel = 2
LEFT JOIN #upperLevels cls3 ON cls3.BasicGraphNodeId = dl.OriginalNodeId AND cls3.DetailLevel = 3




CREATE TABLE #higherLevelSequences
(
	SourceSequenceId INT,
	StepFromNodeId INT,
	StepToNodeId INT,
	StepNumber INT,
	DetailLevel INT
)


DECLARE @higherLevelSequencesSQL NVARCHAR(MAX)
		SET @higherLevelSequencesSQL = 'INSERT INTO #higherLevelSequences

SELECT DISTINCT sq.SequenceId SourceSequenceId,
ul2sf.AncestorNodeId StepSourceNodeId,
ul2st.AncestorNodeId StepTargetNodeId,
stp.StepNumber,
2 DetailLevel
FROM [' + @id + '].DataFlowSequences sq
INNER JOIN [' + @id + '].DataFlowSequenceSteps stp ON stp.SequenceId = sq.SequenceId
INNER JOIN #upperLevels ul2sf ON ul2sf.DetailLevel = 2 AND ul2sf.BasicGraphNodeId = stp.SourceNodeId
INNER JOIN #upperLevels ul2st ON ul2st.DetailLevel = 2 AND ul2st.BasicGraphNodeId = stp.TargetNodeId
WHERE sq.ProjectConfigid = @projectconfigid AND sq.DetailLevel = 1
' 
EXEC sp_executesql  @higherLevelSequencesSQL, N'@projectconfigid UNIQUEIDENTIFIER', @projectconfigid = @projectconfigid

/*
INSERT INTO #higherLevelSequences

SELECT DISTINCT sq.SequenceId SourceSequenceId,
ul2sf.AncestorNodeId StepSourceNodeId,
ul2st.AncestorNodeId StepTargetNodeId,
stp.StepNumber,
2 DetailLevel
FROM BIDoc.DataFlowSequences sq
INNER JOIN BIDoc.DataFlowSequenceSteps stp ON stp.SequenceId = sq.SequenceId
INNER JOIN #upperLevels ul2sf ON ul2sf.DetailLevel = 2 AND ul2sf.BasicGraphNodeId = stp.SourceNodeId
INNER JOIN #upperLevels ul2st ON ul2st.DetailLevel = 2 AND ul2st.BasicGraphNodeId = stp.TargetNodeId
WHERE sq.ProjectConfigid = @projectconfigid AND sq.DetailLevel = 1
*/


DECLARE @higherLevelSequences3SQL NVARCHAR(MAX)
		SET @higherLevelSequences3SQL = 'INSERT INTO #higherLevelSequences

SELECT DISTINCT sq.SequenceId SourceSequenceId,
ul2sf.AncestorNodeId StepSourceNodeId,
ul2st.AncestorNodeId StepTargetNodeId,
stp.StepNumber,
3 DetailLevel
FROM [' + @id + '].DataFlowSequences sq
INNER JOIN [' + @id + '].DataFlowSequenceSteps stp ON stp.SequenceId = sq.SequenceId
INNER JOIN #upperLevels ul2sf ON ul2sf.DetailLevel = 3 AND ul2sf.BasicGraphNodeId = stp.SourceNodeId
INNER JOIN #upperLevels ul2st ON ul2st.DetailLevel = 3 AND ul2st.BasicGraphNodeId = stp.TargetNodeId
WHERE sq.ProjectConfigid = @projectconfigid AND sq.DetailLevel = 1
'
EXEC sp_executesql  @higherLevelSequences3SQL, N'@projectconfigid UNIQUEIDENTIFIER', @projectconfigid = @projectconfigid

/*
INSERT INTO #higherLevelSequences

SELECT DISTINCT sq.SequenceId SourceSequenceId,
ul2sf.AncestorNodeId StepSourceNodeId,
ul2st.AncestorNodeId StepTargetNodeId,
stp.StepNumber,
3 DetailLevel
FROM BIDoc.DataFlowSequences sq
INNER JOIN BIDoc.DataFlowSequenceSteps stp ON stp.SequenceId = sq.SequenceId
INNER JOIN #upperLevels ul2sf ON ul2sf.DetailLevel = 3 AND ul2sf.BasicGraphNodeId = stp.SourceNodeId
INNER JOIN #upperLevels ul2st ON ul2st.DetailLevel = 3 AND ul2st.BasicGraphNodeId = stp.TargetNodeId
WHERE sq.ProjectConfigid = @projectconfigid AND sq.DetailLevel = 1
*/

-- delete the steps (and sequences that only span one higher level node)
DELETE FROM #higherLevelSequences WHERE StepFromNodeId = StepToNodeId


--SELECT COUNT(*) FROM #higherLevelSequences

--------

-- insert distinct sequence
DECLARE @InsertSQL NVARCHAR(MAX)
		SET @InsertSQL = 'INSERT INTO [' + @id + '].[DataFlowSequences_Heap] WITH(TABLOCK)
		(SourceNode, TargetNode, DetailLevel, ProjectConfigid)
SELECT DISTINCT 
sq.SourceNode, 
sq.TargetNode,
hls.DetailLevel,
sq.ProjectConfigid
FROM #higherLevelSequences hls
INNER JOIN [' + @id + '].[DataFlowSequences] sq ON sq.SequenceId = hls.SourceSequenceId'
EXEC sp_executesql  @InsertSQL

-- insert higher level steps
DECLARE @InsertSQL1 NVARCHAR(MAX)
		SET @InsertSQL1 = 'INSERT INTO [' + @id + '].[DataFlowSequenceSteps_Heap] WITH(TABLOCK)(
SourceNodeId, TargetNodeId, SequenceId, StepNumber
)
SELECT 
hls.StepFromNodeId, 
hls.StepToNodeId, 
tgts.SequenceId, 
MIN(hls.StepNumber)
FROM #higherLevelSequences hls
INNER JOIN [' + @id + '].[DataFlowSequences] orgs ON orgs.SequenceId = hls.SourceSequenceId
INNER JOIN [' + @id + '].[DataFlowSequences_Heap] tgts ON tgts.DetailLevel = hls.DetailLevel AND tgts.SourceNode = orgs.SourceNode AND tgts.TargetNode = orgs.TargetNode
GROUP BY
hls.StepFromNodeId, 
hls.StepToNodeId, 
tgts.SequenceId'
EXEC sp_executesql  @InsertSQL1


	DECLARE @InsertSQL11 NVARCHAR(MAX)
		SET @InsertSQL11 = N'INSERT INTO [' + @id + '].[DataFlowSequences] WITH(TABLOCK)
		(
		[SequenceId],
		SourceNode,
		TargetNode,
		DetailLevel,
		ProjectConfigid--,
		--LastLinkId
		)
		SELECT
		s.[SequenceId],
		s.SourceNode,
		s.TargetNode,
		s.DetailLevel,
		s.ProjectConfigid

		FROM [' + @id + '].[DataFlowSequences_Heap] s
WHERE s.DetailLevel > 1
'
		EXEC sp_executesql  @InsertSQL11, N'@projectconfigid UNIQUEIDENTIFIER', @projectconfigid = @projectconfigid

			DECLARE @InsertSQL12 NVARCHAR(MAX)
		SET @InsertSQL12 = N'INSERT INTO [' + @id + '].DataFlowSequenceSteps WITH(TABLOCK)
		(
		SourceNodeId,
		TargetNodeId,
		StepNumber,
		SequenceId
		)
		
SELECT 
		st.SourceNodeId,
		st.TargetNodeId,
		st.StepNumber,
		st.SequenceId

FROM [' + @id + '].[DataFlowSequences_Heap] s
INNER JOIN [' + @id + '].DataFlowSequenceSteps_Heap st ON st.SequenceId = s.SequenceId
WHERE s.DetailLevel > 1
'

EXEC sp_executesql  @InsertSQL12, N'@projectconfigid UNIQUEIDENTIFIER', @projectconfigid = @projectconfigid


DROP TABLE #higherLevelSequences
DROP TABLE #upperLevelNodeMapping
DROP TABLE #upperLevels

PRINT N'Removing backward steps'

DECLARE @DeleteSQL NVARCHAR(MAX)
		SET @DeleteSQL = '
DELETE st FROM [' + @id + '].[DataFlowSequenceSteps] st
INNER JOIN [' + @id + '].DataFlowSequenceSteps prvsrc ON st.SequenceId = prvsrc.SequenceId AND prvsrc.StepNumber < st.StepNumber AND st.TargetNodeId = prvsrc.SourceNodeId'
EXEC sp_executesql @DeleteSQL

PRINT N'Rebilding indexes'
	
DECLARE @SQLTask NVARCHAR(MAX)
	SET @SQLTask = 'ALTER INDEX [IX_' + @id + '_DataFlowSequenceSteps_SequenceId] ON [' + @id + '].[DataFlowSequenceSteps]
	REBUILD
ALTER INDEX [IX_' + @id + '_DataFlowSequenceSteps_SequenceId_SourceNodeId] ON [' + @id + '].[DataFlowSequenceSteps]
	REBUILD
ALTER INDEX [IX_' + @id + '_DataFlowSequenceSteps_SequenceId_TargetNodeId] ON [' + @id + '].[DataFlowSequenceSteps]
	REBUILD'
EXEC sp_executesql  @SQLTask
