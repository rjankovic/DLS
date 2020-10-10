CREATE PROCEDURE [BIDoc].[sp_ClenseDataFlowSequences]
	--DECLARE
	@projectconfigid UNIQUEIDENTIFIER --= N'FD1312FC-1182-4B9C-82D9-2089F3468BFB'
AS
DECLARE @id NVARCHAR(100)
SET @id = REPLACE(CAST(@projectconfigid AS NVARCHAR(100)),'-','')

DECLARE @message NVARCHAR(MAX)

-- firstTableSource
SELECT st.Id, st.SequenceId, st.SourceNodeId, pst.TargetNodeId AS 'pstTargetNodeId', st.TargetNodeId
INTO #firstStepSource
FROM BIDoc.DataFlowSequenceSteps st
INNER JOIN BIDoc.DataFlowSequenceSteps pst ON st.SequenceId = pst.SequenceId AND st.SourceNodeId = pst.SourceNodeId
INNER JOIN BIDoc.DataFlowSequences s ON s.SequenceId = st.SequenceId
INNER JOIN BIDoc.BasicGraphNodes n ON st.SourceNodeId = n.BasicGraphNodeId
WHERE s.ProjectConfigid = @projectconfigid AND n.ProjectConfigId = @projectconfigid

-- firtstTableTarget
SELECT st.Id, st.SequenceId, st.TargetNodeId, st.SourceNodeId, pst.SourceNodeId AS 'pstSourceNodeId'
INTO #firstStepTarget
FROM BIDoc.DataFlowSequenceSteps st
INNER JOIN BIDoc.DataFlowSequenceSteps pst ON st.SequenceId = pst.SequenceId AND st.TargetNodeId = pst.TargetNodeId
INNER JOIN BIDoc.DataFlowSequences s ON s.SequenceId = st.SequenceId
INNER JOIN BIDoc.BasicGraphNodes n ON st.SourceNodeId = n.BasicGraphNodeId
WHERE s.ProjectConfigid = @projectconfigid AND n.ProjectConfigId = @projectconfigid


-- delete lower level duplicate targets
SELECT st.Id
INTO #viewDuplicateTargets
FROM #firstStepSource st
INNER JOIN BIDoc.BasicGraphNodes n ON n.BasicGraphNodeId = st.TargetNodeId AND n.ParentId = st.pstTargetNodeId
-- do not delete child nodes (st) if there is a grandchild (chn) with the same source
LEFT JOIN BIDoc.BasicGraphNodes chn ON chn.ParentId = n.BasicGraphNodeId
LEFT JOIN BIDoc.DataFlowSequenceSteps chst ON st.SequenceId = chst.SequenceId AND chst.SourceNodeId = st.SourceNodeId AND chst.TargetNodeId = chn.BasicGraphNodeId
WHERE chst.Id IS NULL

DECLARE @rc INT = 1
WHILE @rc > 0
BEGIN
	DECLARE @DeleteSQL NVARCHAR(MAX)
	SET @DeleteSQL = 
	'DELETE TOP (100000) st FROM [' + @id + '].[DataFlowSequenceSteps] st
	INNER JOIN #viewDuplicateTargets vdt ON vdt.Id = st.Id'
	EXEC sp_executesql @DeleteSQL

	SET @rc = @@ROWCOUNT
	SET @message = N'Clensed ' + CONVERT(NVARCHAR(20), @rc) + N' duplicate targets'
	PRINT @message
	EXEC [Adm].[sp_WriteLogInfo] @message

END

DROP TABLE #viewDuplicateTargets


-- delete lower level duplicate sources

SELECT st.Id
INTO #viewDuplicateSources
FROM #firstStepTarget st
INNER JOIN BIDoc.BasicGraphNodes n ON n.BasicGraphNodeId = st.SourceNodeId AND n.ParentId = pstSourceNodeId
-- do not delete child nodes (st) if there is a grandchild (chn) with the same target
LEFT JOIN BIDoc.BasicGraphNodes chn ON chn.ParentId = n.BasicGraphNodeId
LEFT JOIN BIDoc.DataFlowSequenceSteps chst ON st.SequenceId = chst.SequenceId AND chst.TargetNodeId = st.TargetNodeId AND chst.SourceNodeId = chn.BasicGraphNodeId
WHERE chst.Id IS NULL

SET @rc = 1
WHILE @rc > 0
BEGIN
	DECLARE @DeleteSQL1 NVARCHAR(MAX)
	SET @DeleteSQL1 = 
	'
	DELETE TOP (100000) st FROM [' + @id + '].[DataFlowSequenceSteps] st
	INNER JOIN #viewDuplicateSources vds ON vds.Id = st.Id'
	EXEC sp_executesql @DeleteSQL1
	
	SET @rc = @@ROWCOUNT
	SET @message = N'Clensed ' + CONVERT(NVARCHAR(20), @rc) + N' duplicate sources'
	PRINT @message
	EXEC [Adm].[sp_WriteLogInfo] @message

END

DROP TABLE #viewDuplicateSources

---------------------------------------------------

-- delete lower level duplicate targets - 2 step skip

SELECT st.Id
INTO #viewLowerTargets
FROM #firstStepSource st
INNER JOIN BIDoc.BasicGraphNodes n ON n.ParentId = pstTargetNodeId
INNER JOIN BIDoc.BasicGraphNodes n2 ON n2.BasicGraphNodeId = st.TargetNodeId AND n2.ParentId = n.BasicGraphNodeId

SET @rc = 1
WHILE @rc > 0
BEGIN
	DECLARE @DeleteSQL2 NVARCHAR(MAX)
	SET @DeleteSQL2 = 
	'
	DELETE TOP (100000) st FROM [' + @id + '].[DataFlowSequenceSteps] st
	INNER JOIN #viewLowerTargets vlt ON vlt.Id = st.Id'
	EXEC sp_executesql @DeleteSQL2

	SET @rc = @@ROWCOUNT
	SET @message = N'Clensed ' + CONVERT(NVARCHAR(20), @rc) + N' duplicate targets'
	PRINT @message
	EXEC [Adm].[sp_WriteLogInfo] @message
END

DROP TABLE #firstStepSource
DROP TABLE #viewLowerTargets

-- delete lower level duplicate sources - 2 step skip

SELECT st.Id
INTO #viewLowerSources
FROM #firstStepTarget st
INNER JOIN BIDoc.BasicGraphNodes n ON n.ParentId = pstSourceNodeId
INNER JOIN BIDoc.BasicGraphNodes n2 ON n2.BasicGraphNodeId = st.SourceNodeId AND n2.ParentId = n.BasicGraphNodeId

SET @rc = 1
WHILE @rc > 0
BEGIN
	DECLARE @DeleteSQL3 NVARCHAR(MAX)
	SET @DeleteSQL3 = 
	'
	DELETE TOP (100000) st FROM [' + @id + '].[DataFlowSequenceSteps] st
	INNER JOIN #viewLowerSources vls ON vls.Id = st.Id'
	EXEC sp_executesql @DeleteSQL3

	SET @rc = @@ROWCOUNT
	SET @message = N'Clensed ' + CONVERT(NVARCHAR(20), @rc) + N' duplicate sources'
	PRINT @message
	EXEC [Adm].[sp_WriteLogInfo] @message
END

DROP TABLE #firstStepTarget
DROP TABLE #viewLowerSources

----------------------------------------------

-- delete transitive steps
SELECT st.Id
INTO #viewTransitiveSteps
FROM BIDoc.DataFlowSequenceSteps st
INNER JOIN BIDoc.DataFlowSequenceSteps fst ON st.SequenceId = fst.SequenceId AND st.SourceNodeId = fst.SourceNodeId
INNER JOIN BIDoc.DataFlowSequenceSteps tst ON st.SequenceId = tst.SequenceId AND st.TargetNodeId = tst.TargetNodeId
INNER JOIN BIDoc.DataFlowSequences s ON s.SequenceId = st.SequenceId AND s.ProjectConfigId = @projectconfigid
INNER JOIN BIDoc.BasicGraphNodes n ON st.SourceNodeId = n.BasicGraphNodeId

WHERE s.ProjectConfigid = @projectconfigid AND fst.TargetNodeId = tst.SourceNodeId AND n.ProjectConfigId = @projectconfigid

SET @rc = 1
WHILE @rc > 0
BEGIN
	DECLARE @DeleteSQL4 NVARCHAR(MAX)
	SET @DeleteSQL4 = 
	'
	DELETE TOP (100000) st FROM [' + @id + '].[DataFlowSequenceSteps] st
	INNER JOIN #viewTransitiveSteps vst ON vst.Id = st.Id'
	EXEC sp_executesql @DeleteSQL4

	SET @rc = @@ROWCOUNT
	SET @message = N'Deleted '+ CONVERT(NVARCHAR(20), @rc) + N' transitive steps'
	PRINT @message
	EXEC [Adm].[sp_WriteLogInfo] @message
END

DROP TABLE #viewTransitiveSteps

--------------------------------------------

PRINT N'Simplifying view targets'

--FirstStepTargets/Scripts
SELECT st.TargetNodeId, st.Id StepId, st.SequenceId, viewNode.SourceElementId,st.SourceNodeId
INTO #firstStepScript
FROM BIDoc.DataFlowSequenceSteps st
INNER JOIN BIDoc.BasicGraphNodes colNode ON colNode.BasicGraphNodeId = st.TargetNodeId
INNER JOIN BIDoc.BasicGraphNodes viewNode ON viewNode.BasicGraphNodeId = colNode.ParentId
WHERE colNode.NodeType = N'ColumnElement' AND viewNode.NodeType = N'ViewElement'
  AND colNode.ProjectConfigId = @projectConfigId


SELECT pst.SourceNodeId, pst.TargetNodeId MidNodeid, st.TargetNodeId, pst.StepNumber, pst.Id PreStepId, st.StepId, st.SequenceId
INTO #viewTargets
FROM #firstStepScript st
INNER JOIN BIDoc.BasicGraphNodes preColNode ON preColNode.BasicGraphNodeId = st.SourceNodeId
INNER JOIN BIDoc.ModelElements preColElement ON preColNode.SourceElementId = preColElement.ModelElementId
INNER JOIN BIDoc.ModelElements viewElement ON viewElement.ModelElementId = st.SourceElementId
INNER JOIN BIDoc.DataFlowSequenceSteps pst ON pst.SequenceId = st.SequenceId AND pst.TargetNodeId = st.SourceNodeId
WHERE LEFT(preColElement.RefPath, LEN(viewElement.RefPath)) = viewElement.RefPath

DECLARE @DeleteSQL5 NVARCHAR(MAX)
	SET @DeleteSQL5 = 
	'INSERT INTO [' + @id + '].[DataFlowSequenceSteps](SourceNodeId, TargetNodeId, SequenceId, StepNumber)
SELECT SourceNodeId, TargetNodeId, SequenceId, StepNumber FROM #viewTargets

DELETE st FROM [' + @id + '].[DataFlowSequenceSteps] st
INNER JOIN #viewTargets vt ON vt.PreStepId = st.Id

DELETE st FROM [' + @id + '].[DataFlowSequenceSteps] st
INNER JOIN #viewTargets vt ON vt.StepId = st.Id'
EXEC sp_executesql @DeleteSQL5

DROP TABLE #viewTargets

SELECT st.StepId
INTO #viewFirstScriptSteps
FROM #firstStepScript st
LEFT JOIN BIDoc.DataFlowSequenceSteps pst ON pst.SequenceId = st.SequenceId AND pst.TargetNodeId = st.SourceNodeId
INNER JOIN BIDoc.BasicGraphNodes scrNode ON scrNode.BasicGraphNodeId = st.SourceNodeId
WHERE pst.Id IS NULL AND scrNode.NodeType = N'SqlScriptElement'

DECLARE @DeleteSQL6 NVARCHAR(MAX)
	SET @DeleteSQL6 = 
	'DELETE st FROM [' + @id + '].[DataFlowSequenceSteps] st
INNER JOIN #viewFirstScriptSteps vfs ON vfs.StepId = st.Id'
EXEC sp_executesql @DeleteSQL6

DROP TABLE #firstStepScript
DROP TABLE #viewFirstScriptSteps
