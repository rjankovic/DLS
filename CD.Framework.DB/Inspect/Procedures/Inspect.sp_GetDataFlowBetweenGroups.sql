CREATE PROCEDURE [Inspect].[sp_GetDataFlowBetweenGroups]
	@projectConfigId UNIQUEIDENTIFIER,
	@sourcePrefix NVARCHAR(MAX),
	@targetPrefix NVARCHAR(MAX),
	@sourceType NVARCHAR(200),
	@targetType NVARCHAR(200)
AS


IF OBJECT_ID('tempdb.dbo.#sourceNodes') IS NOT NULL
DROP TABLE #sourceNodes
IF OBJECT_ID('tempdb.dbo.#targetNodes') IS NOT NULL
DROP TABLE #targetNodes
IF OBJECT_ID('tempdb.dbo.#sourceDescendants') IS NOT NULL
DROP TABLE #sourceDescendants
IF OBJECT_ID('tempdb.dbo.#targetDescendants') IS NOT NULL
DROP TABLE #targetDescendants
--IF OBJECT_ID('tempdb.dbo.#nodes') IS NOT NULL
--DROP TABLE #nodes

--SELECT n.BasicGraphNodeId, e.RefPath, n.Name, n.SourceElementId, NodeType
--INTO #nodes
--FROM BIDoc.BasicGraphNodes n 
--INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
--WHERE 
--GraphKind = N'DataFlow'  --'DataFlowTransitive' 
--AND e.ProjectConfigId = @projectConfigId

DECLARE @id NVARCHAR(100)
SET @id = REPLACE(CAST(@projectConfigId AS NVARCHAR(100)),'-','')


DECLARE @sourceNodeId INT = (SELECT BasicGraphNodeId FROM [BIDoc].[f_GetGraphNodeIdByRefPath](@projectConfigId, N'DataFlow', @sourcePrefix))
DECLARE @targetNodeId INT = (SELECT BasicGraphNodeId FROM [BIDoc].[f_GetGraphNodeIdByRefPath](@projectConfigId, N'DataFlow', @targetPrefix))
DECLARE 
	@sourceIntervalFrom INT,
	@sourceIntervalTo INT,
	@targetIntervalFrom INT,
	@targetIntervalTo INT

SELECT @sourceIntervalFrom = n.RefPathIntervalStart, @sourceIntervalTo = n.RefPathIntervalEnd FROM BIDoc.BasicGraphNodes n WHERE n.BasicGraphNodeId = @sourceNodeId
SELECT @targetIntervalFrom = n.RefPathIntervalStart, @targetIntervalTo = n.RefPathIntervalEnd FROM BIDoc.BasicGraphNodes n WHERE n.BasicGraphNodeId = @targetNodeId


SELECT n.BasicGraphNodeId, n.SourceElementId
INTO #sourceNodes
FROM BIDoc.BasicGraphNodes n
WHERE n.ProjectConfigId = @projectConfigId AND n.GraphKind = N'DataFlow' AND n.NodeType = @sourceType 
	AND n.ProjectConfigId = @projectConfigId 
	AND n.RefPathIntervalStart BETWEEN @sourceIntervalFrom AND @sourceIntervalTo

SELECT n.BasicGraphNodeId, n.SourceElementId
INTO #targetNodes
FROM BIDoc.BasicGraphNodes n
WHERE n.ProjectConfigId = @projectConfigId AND n.GraphKind = N'DataFlow' AND n.NodeType = @targetType
	AND n.ProjectConfigId = @projectConfigId 
	AND n.RefPathIntervalStart BETWEEN @targetIntervalFrom AND @targetIntervalTo

SELECT n.BasicGraphNodeId, snn.BasicGraphNodeId OriginalNodeId, n.SourceElementId
INTO #sourceDescendants
FROM 
#sourceNodes sn
INNER JOIN BIDoc.BasicGraphNodes snn ON snn.BasicGraphNodeId = sn.BasicGraphNodeId
INNER JOIN BIDoc.BasicGraphNodes n ON n.RefPathIntervalStart BETWEEN snn.RefPathIntervalStart AND snn.RefPathIntervalEnd
WHERE n.ProjectConfigId = @projectConfigId AND n.GraphKind = N'DataFlow'
	AND n.ProjectConfigId = @projectConfigId 
	AND n.RefPathIntervalStart BETWEEN @sourceIntervalFrom AND @sourceIntervalTo

SELECT n.BasicGraphNodeId, snn.BasicGraphNodeId OriginalNodeId, n.SourceElementId
INTO #targetDescendants
FROM 
#targetNodes sn
INNER JOIN BIDoc.BasicGraphNodes snn ON snn.BasicGraphNodeId = sn.BasicGraphNodeId
INNER JOIN BIDoc.BasicGraphNodes n ON n.RefPathIntervalStart BETWEEN snn.RefPathIntervalStart AND snn.RefPathIntervalEnd
WHERE n.ProjectConfigId = @projectConfigId AND n.GraphKind = N'DataFlow'
	AND n.ProjectConfigId = @projectConfigId 
	AND n.RefPathIntervalStart BETWEEN @targetIntervalFrom AND @targetIntervalTo

	--SELECT * FROM #sourceNodes
	--SELECT * FROM #targetNodes
	--SELECT * FROM #sourceDescendants
	--SELECT * FROM #targetDescendants




-- create new sequences from those that end in the front nodes (and such sequence does not exist already)
DECLARE @sql NVARCHAR(MAX)
SET @sql = 
N'
SELECT 
DISTINCT s.BasicGraphNodeId SourceNodeId, se.Caption SourceNodeName, /*dps.DescriptivePath*/ se.RefPath SourceNodePath, s.SourceElementId SourceElementId,
dps.DescriptivePath SourceDescriptivePath,
t.BasicGraphNodeId TargetNodeId, te.Caption TargetNodeName, /*dpt.DescriptivePath*/ te.RefPath TargetNodePath, t.SourceElementId TargetElementId,
dpt.DescriptivePath TargetDescriptivePath
FROM [' + @id + N'].DataFlowSequences sq
INNER JOIN #sourceDescendants sd ON sd.BasicGraphNodeId = sq.SourceNode
INNER JOIN #targetDescendants td ON td.BasicGraphNodeId = sq.TargetNode
INNER JOIN #sourceNodes s ON s.BasicGraphNodeId = sd.OriginalNodeId
INNER JOIN #targetNodes t ON t.BasicGraphNodeId = td.OriginalNodeId
INNER JOIN BIDoc.ModelElementDescriptivePaths dps ON dps.ModelElementId = s.SourceElementId
INNER JOIN BIDoc.ModelElementDescriptivePaths dpt ON dpt.ModelElementId = t.SourceElementId
INNER JOIN BIDoc.ModelElements se ON se.ModelElementId = s.SourceElementId
INNER JOIN BIDoc.ModelElements te ON te.ModelElementId = t.SourceElementId
WHERE sq.ProjectConfigid = @projectConfigId AND sq.DetailLevel = 1
'
EXEC sp_executesql  @sql, N'@projectconfigid UNIQUEIDENTIFIER', @projectconfigid = @projectconfigid
