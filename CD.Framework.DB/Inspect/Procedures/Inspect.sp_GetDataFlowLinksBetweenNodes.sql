CREATE PROCEDURE [Inspect].[sp_GetDataFlowLinksBetweenNodes]
	@sourceNodeId INT,
	@targetNodeId INT,
	@detailLevel INT,
	@projectConfigId UNIQUEIDENTIFIER
AS

DECLARE @projectSchema NVARCHAR(50) = REPLACE(N'[' + CONVERT(NVARCHAR(50), @projectConfigId) + N']', N'-', N'')

;WITH  sourceDescendants AS
	(
	SELECT @sourceNodeId SourceNodeId
	UNION ALL
	SELECT n.BasicGraphNodeId FROM BIDoc.BasicGraphNodes n
	INNER JOIN sourceDescendants sd ON sd.SourceNodeId = n.ParentId
	)
SELECT * INTO #sourceDescendants FROM sourceDescendants
OPTION (MAXRECURSION 1000)

;WITH targetDescendants AS
	(
	SELECT @targetNodeId TargetNodeId
	UNION ALL
	SELECT n.BasicGraphNodeId FROM BIDoc.BasicGraphNodes n
	INNER JOIN targetDescendants sd ON sd.TargetNodeId = n.ParentId
	)
SELECT * INTO #targetDescendants FROM targetDescendants
OPTION (MAXRECURSION 1000)

DECLARE @sql NVARCHAR(MAX) =
N'
-- if direct link exists, use it
IF(EXISTS(SELECT TOP 1 1 FROM ' + @projectSchema +  N'.DataFlowSequences s WHERE s.SourceNode = @sourceNodeId AND s.TargetNode = @targetNodeId AND s.DetailLevel = @detailLevel))
BEGIN
	SELECT --DISTINCT 
	stp.Id SequenceStepId, 
	stp.SourceNodeId, 
	stp.TargetNodeId 
	FROM ' + @projectSchema +  N'.DataFlowSequences sq
	INNER JOIN ' + @projectSchema +  N'.DataFlowSequenceSteps stp ON stp.SequenceId = sq.SequenceId
	INNER JOIN BIDoc.BasicGraphNodes n ON stp.SourceNodeId = n.BasicGraphNodeId

	WHERE sq.DetailLevel = @detailLevel AND sq.SourceNode = @sourceNodeId AND sq.TargetNode = @targetNodeId AND n.ProjectConfigId = sq.ProjectConfigId --AND stp.SourceNodeId <> stp.TargetNodeId
END
-- if direct links to target exists, use those
ELSE IF EXISTS(SELECT TOP 1 1 FROM ' + @projectSchema +  N'.DataFlowSequences s 
INNER JOIN #sourceDescendants sd ON sd.SourceNodeId = s.SourceNode
WHERE s.TargetNode = @targetNodeId AND s.DetailLevel = @detailLevel)
BEGIN
	SELECT --DISTINCT 
	MIN(stp.Id) SequenceStepId, 
	stp.SourceNodeId, 
	stp.TargetNodeId 
	FROM ' + @projectSchema +  N'.DataFlowSequences sq
	INNER JOIN #sourceDescendants sd ON sq.SourceNode = sd.SourceNodeId
	INNER JOIN ' + @projectSchema +  N'.DataFlowSequenceSteps stp ON stp.SequenceId = sq.SequenceId
	INNER JOIN BIDoc.BasicGraphNodes n ON stp.SourceNodeId = n.BasicGraphNodeId

	WHERE sq.DetailLevel = @detailLevel AND sq.TargetNode = @targetNodeId AND n.ProjectConfigId = sq.ProjectConfigId --AND stp.SourceNodeId <> stp.TargetNodeId
	GROUP BY 
	stp.SourceNodeId, 
	stp.TargetNodeId
END
-- if direct links to source exists, use those
ELSE IF EXISTS(SELECT TOP 1 1 FROM ' + @projectSchema +  N'.DataFlowSequences s 
INNER JOIN #targetDescendants td ON td.TargetNodeId = s.TargetNode
WHERE s.SourceNode = @sourceNodeId AND s.DetailLevel = @detailLevel)
BEGIN
	SELECT --DISTINCT 
	MIN(stp.Id) SequenceStepId, 
	stp.SourceNodeId, 
	stp.TargetNodeId 
	FROM ' + @projectSchema +  N'.DataFlowSequences sq
	INNER JOIN #targetDescendants td ON sq.TargetNode = td.TargetNodeId
	INNER JOIN ' + @projectSchema +  N'.DataFlowSequenceSteps stp ON stp.SequenceId = sq.SequenceId
	INNER JOIN BIDoc.BasicGraphNodes n ON stp.SourceNodeId = n.BasicGraphNodeId

	WHERE sq.DetailLevel = @detailLevel AND sq.SourceNode = @sourceNodeId AND n.ProjectConfigId = sq.ProjectConfigId --AND stp.SourceNodeId <> stp.TargetNodeId
	GROUP BY 
	stp.SourceNodeId, 
	stp.TargetNodeId
END
ELSE
BEGIN
	SELECT --DISTINCT 
	MIN(stp.Id) SequenceStepId, 
	stp.SourceNodeId, 
	stp.TargetNodeId 
	FROM ' + @projectSchema +  N'.DataFlowSequences sq
	INNER JOIN #sourceDescendants sd ON sq.SourceNode = sd.SourceNodeId
	INNER JOIN #targetDescendants td ON sq.TargetNode = td.TargetNodeId
	INNER JOIN ' + @projectSchema +  N'.DataFlowSequenceSteps stp ON stp.SequenceId = sq.SequenceId
	INNER JOIN BIDoc.BasicGraphNodes n ON stp.SourceNodeId = n.BasicGraphNodeId
	LEFT JOIN #sourceDescendants stepToSource ON stepToSource.SourceNodeId = stp.TargetNodeId
	LEFT JOIN #targetDescendants stepFromTarget ON stepFromTarget.TargetNodeId = stp.SourceNodeId
	
	WHERE sq.DetailLevel = @detailLevel AND sq.ProjectConfigId = n.ProjectConfigId --AND stp.SourceNodeId <> stp.TargetNodeId
	AND stepFromTarget.TargetNodeId IS NULL AND stepToSource.SourceNodeId IS NULL
	GROUP BY 
	stp.SourceNodeId, 
	stp.TargetNodeId
END
'

DECLARE @paramDef NVARCHAR(MAX) = N'@sourceNodeId INT, @targetNodeId INT, @detailLevel INT'
EXEC sp_executesql @sql, @paramDef, @sourceNodeId = @sourceNodeId, @targetNodeId = @targetNodeId, @detailLevel = @detailLevel


DROP TABLE #sourceDescendants
DROP TABLE #targetDescendants