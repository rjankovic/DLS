CREATE PROCEDURE [Inspect].[sp_GetDataFlowBetweenGroupsSteps]
	@projectConfigId UNIQUEIDENTIFIER,
	@sourcePrefix NVARCHAR(MAX),
	@targetPrefix NVARCHAR(MAX),
	@sourceType NVARCHAR(200),
	@targetType NVARCHAR(200),
	@stepCount INT = 10
AS
--DECLARE @sourcePrefix NVARCHAR(MAX) = N'IntegrationServices[@Name=''FSCZPRCT0041'']/Catalog[@Name=''SSISDB'']/CatalogFolder[@Name=''NRWH_SSIS'']/ProjectInfo[@Name=''NRWH_SSIS'']'
--DECLARE @targetPrefix NVARCHAR(MAX) = N'Server[@Name=''FSCZPRCT0041'']/Database[@Name=''NRWH_L2'']'
--DECLARE @sourceType NVARCHAR(200) = N'PackageElement'
--DECLARE @targetType NVARCHAR(200) = N'SchemaTableElement'

/*
DECLARE
	@projectConfigId UNIQUEIDENTIFIER = N'18258D08-66CC-4B97-A695-226C7BA64AFE',
	@sourcePrefix NVARCHAR(MAX) = N'Server[@Name=''FSCZPRCT0041'']/Database[@Name=''NDWH_L1'']',
	@targetPrefix NVARCHAR(MAX) = N'Server[@Name=''FSCZPRCT0041'']/Database[@Name=''NRWH_L2'']',
	@sourceType NVARCHAR(200) = N'ColumnElement',
	@targetType NVARCHAR(200) = N'ColumnElement',
	@stepCount INT = 10
*/

IF OBJECT_ID('tempdb.dbo.#sourceNodes') IS NOT NULL
DROP TABLE #sourceNodes
IF OBJECT_ID('tempdb.dbo.#targetNodes') IS NOT NULL
DROP TABLE #targetNodes
IF OBJECT_ID('tempdb.dbo.#sourceDescendants') IS NOT NULL
DROP TABLE #sourceDescendants
IF OBJECT_ID('tempdb.dbo.#targetDescendants') IS NOT NULL
DROP TABLE #targetDescendants
IF OBJECT_ID('tempdb.dbo.#targetReach') IS NOT NULL
DROP TABLE #targetReach


SELECT n.BasicGraphNodeId, e.RefPath, n.Name, n.SourceElementId 
INTO #sourceNodes
FROM BIDoc.BasicGraphNodes n 
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
WHERE GraphKind = 'DataFlowTransitive' 
AND n.ProjectConfigId = @projectConfigId
AND LEFT(e.RefPath, LEN(@sourcePrefix)) = @sourcePrefix 
AND NodeType = @sourceType

SELECT n.BasicGraphNodeId, e.RefPath, n.Name, n.SourceElementId
INTO #targetNodes
FROM BIDoc.BasicGraphNodes n 
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
WHERE GraphKind = 'DataFlowTransitive' 
AND e.ProjectConfigId = @projectConfigId
AND LEFT(e.RefPath, LEN(@targetPrefix)) = @targetPrefix 
AND NodeType = @targetType


;WITH  sourceDescendants AS
(
SELECT BasicGraphNodeId, BasicGraphNodeId OriginalNodeId FROM #sourceNodes
UNION ALL
SELECT n.BasicGraphNodeId, sd.OriginalNodeId FROM BIDoc.BasicGraphNodes n
INNER JOIN sourceDescendants sd ON sd.BasicGraphNodeId = n.ParentId
)
SELECT * INTO #sourceDescendants FROM sourceDescendants

;WITH  targetDescendants AS
(
SELECT BasicGraphNodeId,BasicGraphNodeId OriginalNodeId FROM #targetNodes
UNION ALL
SELECT n.BasicGraphNodeId, sd.OriginalNodeId FROM BIDoc.BasicGraphNodes n
INNER JOIN targetDescendants sd ON sd.BasicGraphNodeId = n.ParentId
)
SELECT * INTO #targetDescendants FROM targetDescendants

--SELECT * FROM #sourceDescendants
--SELECT * FROM #targetDescendants

;WITH newTargets AS
(
SELECT DISTINCT s.OriginalNodeId, n.BasicGraphNodeId 
FROM #sourceDescendants s
INNER JOIN BIDoc.BasicGraphLinks l ON l.NodeFromId = s.BasicGraphNodeId AND l.LinkType = N'DataFlow'
INNER JOIN BIDoc.BasicGraphNodes n ON n.BasicGraphNodeId = l.NodeToId
UNION ALL
SELECT sd.OriginalNodeId, n.BasicGraphNodeId FROM BIDoc.BasicGraphNodes n
INNER JOIN newTargets sd ON sd.BasicGraphNodeId = n.ParentId
)
SELECT * INTO #targetReach FROM newTargets

DECLARE @remainingSteps INT = @stepCount - 1
	
WHILE @remainingSteps > 0
BEGIN
	;WITH newTargets AS
	(
	SELECT DISTINCT s.OriginalNodeId, n.BasicGraphNodeId 
	FROM #targetReach s
	INNER JOIN BIDoc.BasicGraphLinks l ON l.NodeFromId = s.BasicGraphNodeId AND l.LinkType = N'DataFlow'
	INNER JOIN BIDoc.BasicGraphNodes n ON n.BasicGraphNodeId = l.NodeToId
	UNION ALL
	SELECT sd.OriginalNodeId, n.BasicGraphNodeId FROM BIDoc.BasicGraphNodes n
	INNER JOIN newTargets sd ON sd.BasicGraphNodeId = n.ParentId
	) 
	INSERT INTO #targetReach(OriginalNodeId, BasicGraphNodeId)
	SELECT nt.OriginalNodeId, nt.BasicGraphNodeId FROM newTargets nt
	LEFT JOIN #targetReach rs ON rs.BasicGraphNodeId = nt.BasicGraphNodeId
	WHERE rs.BasicGraphNodeId IS NULL

	SET @remainingSteps = @remainingSteps - 1
END

--SELECT * FROM #targetReach
/*
SELECT *
FROM #sourceNodes s
INNER JOIN #targetReach tr ON tr.OriginalNodeId = s.BasicGraphNodeId
INNER JOIN BIDoc.BasicGraphNodes tn ON tr.BasicGraphNodeId = tn.BasicGraphNodeId
INNER JOIN BIDoc.ModelElements te ON te.ModelElementId = tn.SourceElementId
WHERE s.Name = N'VREGNO'
ORDER BY s.BasicGraphNodeId

SELECT * FROM BIDoc.BasicGraphNodes n WHERE n.BasicGraphNodeId = 2801264
SELECT * FROM BIDoc.BasicGraphNodes n WHERE n.BasicGraphNodeId = 2801262
SELECT * FROM BIDoc.BasicGraphNodes n WHERE n.BasicGraphNodeId = 2801261
SELECT * FROM BIDoc.BasicGraphNodes n WHERE n.BasicGraphNodeId = 2801181


SELECT * FROM BIDoc.BasicGraphNodes n WHERE n.NodeType = N'DfColumnElement'

--2801261

SELECT * FROM BIDoc.BasicGraphLinks l WHERE l.NodeFromId = 2801261

SELECT * FROM BIDoc.BasicGraphNodes WHERE BasicGraphNodeId = 2674732
SELECT * FROM BIDoc.BasicGraphNodes WHERE BasicGraphNodeId = 2801265

SELECT * FROM BIDoc.BasicGraphNodes WHERE ParentId = 2801265

SELECT * FROM BIDoc.BasicGraphLinks l 
WHERE l.NodeFromId = 2801265

SELECT * FROM BIDoc.BasicGraphLinks l 
WHERE l.NodeFromId = 2674732


SELECT * FROM #targetReach
*/

SELECT 
DISTINCT s.BasicGraphNodeId SourceNodeId, s.Name SourceNodeName, s.RefPath SourceNodePath, s.SourceElementId SourceElementId, 
t.BasicGraphNodeId TargetNodeId, t.Name TargetNodeName, t.RefPath TargetNodePath, t.SourceElementId TargetElementId
FROM #sourceNodes s
INNER JOIN #targetReach tr ON tr.OriginalNodeId = s.BasicGraphNodeId
INNER JOIN #targetDescendants td ON td.BasicGraphNodeId = tr.BasicGraphNodeId
INNER JOIN #targetNodes t ON t.BasicGraphNodeId = td.OriginalNodeId
