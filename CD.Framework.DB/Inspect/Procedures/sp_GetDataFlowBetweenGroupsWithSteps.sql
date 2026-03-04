CREATE PROCEDURE [Inspect].[sp_GetDataFlowBetweenGroupsWithSteps]
	@projectConfigId UNIQUEIDENTIFIER,
	@sourcePrefix NVARCHAR(MAX),
	@targetPrefix NVARCHAR(MAX),
	@sourceType NVARCHAR(200),
	@targetType NVARCHAR(200),
	@stepNodeTypes NVARCHAR(MAX) = NULL
	-- Comma-separated list of NodeType values to resolve steps to
	-- (e.g. 'SchemaTableElement,ViewElement,ProcedureElement').
	-- Each low-level step node is walked up via ParentId until a node
	-- whose NodeType matches one of these values is found.
	-- Consecutive duplicate resolved nodes are collapsed into single waypoints.
	-- If NULL, the raw low-level step nodes are returned without resolution.
AS

------------------------------------------------------------------------
-- Clean up any prior temp tables
------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#desiredStepTypes') IS NOT NULL DROP TABLE #desiredStepTypes
IF OBJECT_ID('tempdb..#sourceNodes') IS NOT NULL DROP TABLE #sourceNodes
IF OBJECT_ID('tempdb..#targetNodes') IS NOT NULL DROP TABLE #targetNodes
IF OBJECT_ID('tempdb..#sourceDescendants') IS NOT NULL DROP TABLE #sourceDescendants
IF OBJECT_ID('tempdb..#targetDescendants') IS NOT NULL DROP TABLE #targetDescendants
IF OBJECT_ID('tempdb..#matchedPairs') IS NOT NULL DROP TABLE #matchedPairs
IF OBJECT_ID('tempdb..#rawSteps') IS NOT NULL DROP TABLE #rawSteps
IF OBJECT_ID('tempdb..#uniqueStepNodes') IS NOT NULL DROP TABLE #uniqueStepNodes
IF OBJECT_ID('tempdb..#resolvedNodes') IS NOT NULL DROP TABLE #resolvedNodes
IF OBJECT_ID('tempdb..#nodeChain') IS NOT NULL DROP TABLE #nodeChain
IF OBJECT_ID('tempdb..#resolvedChain') IS NOT NULL DROP TABLE #resolvedChain
IF OBJECT_ID('tempdb..#waypoints') IS NOT NULL DROP TABLE #waypoints

------------------------------------------------------------------------
-- Parse comma-separated step node types
------------------------------------------------------------------------
CREATE TABLE #desiredStepTypes (TypeName NVARCHAR(200) NOT NULL PRIMARY KEY)

IF @stepNodeTypes IS NOT NULL AND LEN(@stepNodeTypes) > 0
BEGIN
	INSERT INTO #desiredStepTypes(TypeName)
	SELECT DISTINCT LTRIM(RTRIM(value))
	FROM STRING_SPLIT(@stepNodeTypes, N',')
	WHERE LEN(LTRIM(RTRIM(value))) > 0
END

------------------------------------------------------------------------
-- Derive dynamic schema name from projectConfigId
------------------------------------------------------------------------
DECLARE @id NVARCHAR(100)
SET @id = REPLACE(CAST(@projectConfigId AS NVARCHAR(100)), '-', '')

------------------------------------------------------------------------
-- Find source & target prefix nodes and their RefPath intervals
------------------------------------------------------------------------
DECLARE @sourceNodeId INT = (SELECT BasicGraphNodeId FROM [BIDoc].[f_GetGraphNodeIdByRefPath](@projectConfigId, N'DataFlow', @sourcePrefix))
DECLARE @targetNodeId INT = (SELECT BasicGraphNodeId FROM [BIDoc].[f_GetGraphNodeIdByRefPath](@projectConfigId, N'DataFlow', @targetPrefix))

DECLARE
	@sourceIntervalFrom INT,
	@sourceIntervalTo INT,
	@targetIntervalFrom INT,
	@targetIntervalTo INT

SELECT @sourceIntervalFrom = n.RefPathIntervalStart, @sourceIntervalTo = n.RefPathIntervalEnd
FROM BIDoc.BasicGraphNodes n WHERE n.BasicGraphNodeId = @sourceNodeId

SELECT @targetIntervalFrom = n.RefPathIntervalStart, @targetIntervalTo = n.RefPathIntervalEnd
FROM BIDoc.BasicGraphNodes n WHERE n.BasicGraphNodeId = @targetNodeId

------------------------------------------------------------------------
-- Find source nodes of the requested type within the source prefix scope
------------------------------------------------------------------------
SELECT n.BasicGraphNodeId, n.SourceElementId
INTO #sourceNodes
FROM BIDoc.BasicGraphNodes n
WHERE n.ProjectConfigId = @projectConfigId
	AND n.GraphKind = N'DataFlow'
	AND n.NodeType = @sourceType
	AND n.RefPathIntervalStart BETWEEN @sourceIntervalFrom AND @sourceIntervalTo

------------------------------------------------------------------------
-- Find target nodes of the requested type within the target prefix scope
------------------------------------------------------------------------
SELECT n.BasicGraphNodeId, n.SourceElementId
INTO #targetNodes
FROM BIDoc.BasicGraphNodes n
WHERE n.ProjectConfigId = @projectConfigId
	AND n.GraphKind = N'DataFlow'
	AND n.NodeType = @targetType
	AND n.RefPathIntervalStart BETWEEN @targetIntervalFrom AND @targetIntervalTo

------------------------------------------------------------------------
-- Find all DataFlow-graph descendants of source and target nodes
-- (descendants identified by RefPathInterval containment)
------------------------------------------------------------------------
SELECT n.BasicGraphNodeId, snn.BasicGraphNodeId AS OriginalNodeId, n.SourceElementId
INTO #sourceDescendants
FROM #sourceNodes sn
INNER JOIN BIDoc.BasicGraphNodes snn ON snn.BasicGraphNodeId = sn.BasicGraphNodeId
INNER JOIN BIDoc.BasicGraphNodes n
	ON n.RefPathIntervalStart BETWEEN snn.RefPathIntervalStart AND snn.RefPathIntervalEnd
WHERE n.ProjectConfigId = @projectConfigId
	AND n.GraphKind = N'DataFlow'
	AND n.RefPathIntervalStart BETWEEN @sourceIntervalFrom AND @sourceIntervalTo

SELECT n.BasicGraphNodeId, snn.BasicGraphNodeId AS OriginalNodeId, n.SourceElementId
INTO #targetDescendants
FROM #targetNodes sn
INNER JOIN BIDoc.BasicGraphNodes snn ON snn.BasicGraphNodeId = sn.BasicGraphNodeId
INNER JOIN BIDoc.BasicGraphNodes n
	ON n.RefPathIntervalStart BETWEEN snn.RefPathIntervalStart AND snn.RefPathIntervalEnd
WHERE n.ProjectConfigId = @projectConfigId
	AND n.GraphKind = N'DataFlow'
	AND n.RefPathIntervalStart BETWEEN @targetIntervalFrom AND @targetIntervalTo

------------------------------------------------------------------------
-- Find sequences (DetailLevel = 1) connecting source descendants
-- to target descendants
------------------------------------------------------------------------
CREATE TABLE #matchedPairs
(
	SequenceId INT NOT NULL,
	SourceOriginalNodeId INT NOT NULL,
	TargetOriginalNodeId INT NOT NULL,
	SourceElementId INT NOT NULL,
	TargetElementId INT NOT NULL
)

DECLARE @sqlSeq NVARCHAR(MAX)
SET @sqlSeq = N'
INSERT INTO #matchedPairs(SequenceId, SourceOriginalNodeId, TargetOriginalNodeId, SourceElementId, TargetElementId)
SELECT DISTINCT sq.SequenceId, s.BasicGraphNodeId, t.BasicGraphNodeId, s.SourceElementId, t.SourceElementId
FROM [' + @id + N'].DataFlowSequences sq
INNER JOIN #sourceDescendants sd ON sd.BasicGraphNodeId = sq.SourceNode
INNER JOIN #targetDescendants td ON td.BasicGraphNodeId = sq.TargetNode
INNER JOIN #sourceNodes s ON s.BasicGraphNodeId = sd.OriginalNodeId
INNER JOIN #targetNodes t ON t.BasicGraphNodeId = td.OriginalNodeId
WHERE sq.ProjectConfigid = @projectConfigId AND sq.DetailLevel = 1
'
EXEC sp_executesql @sqlSeq, N'@projectConfigId UNIQUEIDENTIFIER', @projectConfigId = @projectConfigId

------------------------------------------------------------------------
-- Retrieve the sequence steps for all matched sequences
------------------------------------------------------------------------
CREATE TABLE #rawSteps
(
	SequenceId INT NOT NULL,
	StepNumber INT NOT NULL,
	StepSourceNodeId INT NOT NULL,
	StepTargetNodeId INT NOT NULL
)

DECLARE @sqlSteps NVARCHAR(MAX)
SET @sqlSteps = N'
INSERT INTO #rawSteps(SequenceId, StepNumber, StepSourceNodeId, StepTargetNodeId)
SELECT st.SequenceId, st.StepNumber, st.SourceNodeId, st.TargetNodeId
FROM [' + @id + N'].DataFlowSequenceSteps st
WHERE st.SequenceId IN (SELECT DISTINCT SequenceId FROM #matchedPairs)
'
EXEC sp_executesql @sqlSteps

------------------------------------------------------------------------
-- Build the node chain for each sequence
-- The chain is the ordered list of nodes the data flows through:
--   Step0.Source -> Step0.Target -> Step1.Target -> Step2.Target -> ...
-- (Since steps form a chain, Step[i].Target = Step[i+1].Source)
------------------------------------------------------------------------
CREATE TABLE #nodeChain
(
	SequenceId INT NOT NULL,
	ChainPosition INT NOT NULL,
	NodeId INT NOT NULL
)

;WITH OrderedSteps AS
(
	SELECT
		SequenceId,
		StepNumber,
		StepSourceNodeId,
		StepTargetNodeId,
		ROW_NUMBER() OVER (PARTITION BY SequenceId ORDER BY StepNumber) AS StepOrd
	FROM #rawSteps
)
INSERT INTO #nodeChain(SequenceId, ChainPosition, NodeId)
-- Position 0: source of the first step
SELECT SequenceId, 0, StepSourceNodeId
FROM OrderedSteps WHERE StepOrd = 1
UNION ALL
-- Positions 1..N: target of each step in order
SELECT SequenceId, CAST(StepOrd AS INT), StepTargetNodeId
FROM OrderedSteps

------------------------------------------------------------------------
-- Collect unique step nodes for ancestor resolution
------------------------------------------------------------------------
SELECT DISTINCT NodeId
INTO #uniqueStepNodes
FROM #nodeChain

------------------------------------------------------------------------
-- Resolve each step node to its nearest ancestor of a desired type
-- by walking up the ParentId hierarchy in BasicGraphNodes
------------------------------------------------------------------------
CREATE TABLE #resolvedNodes
(
	OriginalNodeId INT NOT NULL,
	ResolvedNodeId INT NOT NULL,
	ResolvedNodeType NVARCHAR(200) NULL,
	ResolvedElementId INT NOT NULL
)

IF EXISTS(SELECT 1 FROM #desiredStepTypes)
BEGIN
	;WITH NodeAncestorChain AS
	(
		-- Anchor: start with each unique step node
		SELECT
			sn.BasicGraphNodeId AS OriginalNodeId,
			sn.BasicGraphNodeId AS CurrentNodeId,
			sn.ParentId,
			sn.NodeType AS CurrentNodeType,
			sn.SourceElementId,
			0 AS Depth
		FROM BIDoc.BasicGraphNodes sn
		INNER JOIN #uniqueStepNodes usn ON usn.NodeId = sn.BasicGraphNodeId

		UNION ALL

		-- Recursive: walk up the parent chain while the current node's
		-- type is NOT one of the desired step types
		SELECT
			nac.OriginalNodeId,
			p.BasicGraphNodeId,
			p.ParentId,
			p.NodeType,
			p.SourceElementId,
			nac.Depth + 1
		FROM NodeAncestorChain nac
		INNER JOIN BIDoc.BasicGraphNodes p ON p.BasicGraphNodeId = nac.ParentId
		WHERE NOT EXISTS (SELECT 1 FROM #desiredStepTypes dt WHERE dt.TypeName = nac.CurrentNodeType)
		  AND nac.ParentId IS NOT NULL     -- has a parent to walk to
	),
	RankedResolutions AS
	(
		-- Among all ancestors that DO match a desired type,
		-- pick the nearest one (lowest depth) per original node
		SELECT
			nac.OriginalNodeId,
			nac.CurrentNodeId AS ResolvedNodeId,
			nac.CurrentNodeType AS ResolvedNodeType,
			nac.SourceElementId AS ResolvedElementId,
			ROW_NUMBER() OVER (PARTITION BY nac.OriginalNodeId ORDER BY nac.Depth ASC) AS rn
		FROM NodeAncestorChain nac
		INNER JOIN #desiredStepTypes dt ON dt.TypeName = nac.CurrentNodeType
	)
	INSERT INTO #resolvedNodes(OriginalNodeId, ResolvedNodeId, ResolvedNodeType, ResolvedElementId)
	SELECT OriginalNodeId, ResolvedNodeId, ResolvedNodeType, ResolvedElementId
	FROM RankedResolutions
	WHERE rn = 1
	OPTION (MAXRECURSION 200)
END
ELSE
BEGIN
	-- No step-type filtering: each node resolves to itself
	INSERT INTO #resolvedNodes(OriginalNodeId, ResolvedNodeId, ResolvedNodeType, ResolvedElementId)
	SELECT
		sn.BasicGraphNodeId,
		sn.BasicGraphNodeId,
		sn.NodeType,
		sn.SourceElementId
	FROM BIDoc.BasicGraphNodes sn
	INNER JOIN #uniqueStepNodes usn ON usn.NodeId = sn.BasicGraphNodeId
END

------------------------------------------------------------------------
-- Build resolved chain: map each chain node to its resolved ancestor
-- (falls back to the original node when no ancestor of a desired type exists)
------------------------------------------------------------------------
SELECT
	nc.SequenceId,
	nc.ChainPosition,
	nc.NodeId AS OriginalNodeId,
	COALESCE(rn.ResolvedNodeId, nc.NodeId) AS ResolvedNodeId
INTO #resolvedChain
FROM #nodeChain nc
LEFT JOIN #resolvedNodes rn ON rn.OriginalNodeId = nc.NodeId

------------------------------------------------------------------------
-- Collapse consecutive duplicate resolved nodes into distinct waypoints
------------------------------------------------------------------------
;WITH WithPrev AS
(
	SELECT
		rc.SequenceId,
		rc.ChainPosition,
		rc.ResolvedNodeId,
		LAG(rc.ResolvedNodeId) OVER (PARTITION BY rc.SequenceId ORDER BY rc.ChainPosition) AS PrevResolvedNodeId
	FROM #resolvedChain rc
)
SELECT
	SequenceId,
	ResolvedNodeId,
	ROW_NUMBER() OVER (PARTITION BY SequenceId ORDER BY ChainPosition) AS WaypointNumber
INTO #waypoints
FROM WithPrev
WHERE PrevResolvedNodeId IS NULL           -- first node in chain
   OR PrevResolvedNodeId <> ResolvedNodeId -- transition to a different resolved node

------------------------------------------------------------------------
-- Final output
------------------------------------------------------------------------
SELECT DISTINCT
	-- Source info
	s.BasicGraphNodeId AS SourceNodeId,
	se.Caption AS SourceNodeName,
	se.RefPath AS SourceNodePath,
	s.SourceElementId AS SourceElementId,
	dps.DescriptivePath AS SourceDescriptivePath,
	-- Target info
	t.BasicGraphNodeId AS TargetNodeId,
	te.Caption AS TargetNodeName,
	te.RefPath AS TargetNodePath,
	t.SourceElementId AS TargetElementId,
	dpt.DescriptivePath AS TargetDescriptivePath,
	-- Step waypoint info
	CAST(w.WaypointNumber AS INT) AS StepNumber,
	wn.BasicGraphNodeId AS StepNodeId,
	we.Caption AS StepObjectName,
	wn.NodeType AS StepObjectType,
	we.RefPath AS StepObjectPath,
	wdp.DescriptivePath AS StepDescriptivePath
FROM #matchedPairs mp
INNER JOIN #waypoints w ON w.SequenceId = mp.SequenceId
-- Source info
INNER JOIN #sourceNodes s ON s.BasicGraphNodeId = mp.SourceOriginalNodeId
INNER JOIN #targetNodes t ON t.BasicGraphNodeId = mp.TargetOriginalNodeId
INNER JOIN BIDoc.ModelElements se ON se.ModelElementId = s.SourceElementId
INNER JOIN BIDoc.ModelElements te ON te.ModelElementId = t.SourceElementId
INNER JOIN BIDoc.ModelElementDescriptivePaths dps ON dps.ModelElementId = s.SourceElementId
INNER JOIN BIDoc.ModelElementDescriptivePaths dpt ON dpt.ModelElementId = t.SourceElementId
-- Waypoint node info
INNER JOIN BIDoc.BasicGraphNodes wn ON wn.BasicGraphNodeId = w.ResolvedNodeId
INNER JOIN BIDoc.ModelElements we ON we.ModelElementId = wn.SourceElementId
LEFT JOIN BIDoc.ModelElementDescriptivePaths wdp ON wdp.ModelElementId = wn.SourceElementId
ORDER BY
	SourceNodeId,
	TargetNodeId,
	StepNumber

GO
