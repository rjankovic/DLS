CREATE PROCEDURE [BIDoc].[sp_PropagateDataFlowVertically]
	--DECLARE 
	@projectconfigid UNIQUEIDENTIFIER --= N'e99a3b4e-7f04-4b98-9780-10e71e6258cf'
AS


	DECLARE @sourcegraphkind NVARCHAR(50) = N'DataFlow'
	DECLARE @linktype NVARCHAR(50) = N'DataFlow'
	DECLARE @graphkind NVARCHAR(50) = N'DataFlow'

-- df column sources - propagate down
DECLARE @dfPropSourceCount INT
DECLARE @dfPropTargetCount INT


DELETE FROM BIDoc.BasicGraphLinks WHERE NodeFromId = NodeToId

;WITH dfPropSources AS
(
SELECT l.NodeFromId, l.NodeToId FROM BIDoc.BasicGraphNodes n 
INNER JOIN BIDoc.BasicGraphLinks l ON n.BasicGraphNodeId = l.NodeToId AND l.LinkType = @linktype
WHERE n.ProjectConfigId = @projectconfigid AND n.GraphKind = @graphkind AND n.NodeType IN ( N'DfColumnElement', N'DfUnpivotSourceReferenceElement')
UNION ALL
SELECT n.BasicGraphNodeId NodeFromId, ps.NodeToId FROM dfPropSources ps
INNER JOIN BIDoc.BasicGraphNodes n ON n.ParentId = ps.NodeFromId
)
SELECT @dfPropSourceCount = COUNT(*) FROM dfPropSources


PRINT 'Propagated DF source links - ' + CONVERT(VARCHAR(10), @dfPropSourceCount)


--EXEC sp_sequence_get_range @sequence_name = N'BIDoc.BasicGraphLinksSequence', @range_size = @dfPropSourceCount, @range_first_value = @linkSequenceStartVariant OUTPUT
--	SET @linkSequenceStart = CONVERT(INT, @linkSequenceStartVariant)

;WITH dfPropSources AS
(
SELECT l.NodeFromId, l.NodeToId FROM BIDoc.BasicGraphNodes n 
INNER JOIN BIDoc.BasicGraphLinks l ON n.BasicGraphNodeId = l.NodeToId AND l.LinkType = @linktype
WHERE n.ProjectConfigId = @projectconfigid AND n.GraphKind = @graphkind AND n.NodeType IN ( N'DfColumnElement', N'DfUnpivotSourceReferenceElement')
UNION ALL
SELECT n.BasicGraphNodeId NodeFromId, ps.NodeToId FROM dfPropSources ps
INNER JOIN BIDoc.BasicGraphNodes n ON n.ParentId = ps.NodeFromId
)
INSERT INTO BIDoc.BasicGraphLinks(
			--[BasicGraphLinkId]
           --,
		   [LinkType]
           ,[NodeFromId]
           ,[NodeToId]
		   )
	SELECT 
	--ROW_NUMBER() OVER(ORDER BY s.NodeFromId) + @linkSequenceStart - 1
	--,
	@linktype
	,s.NodeFromId
	,s.NodeToId
	FROM dfPropSources s

;WITH dfPropTargets AS
(
SELECT l.NodeFromId, l.NodeToId FROM BIDoc.BasicGraphNodes n 
INNER JOIN BIDoc.BasicGraphLinks l ON n.BasicGraphNodeId = l.NodeFromId AND l.LinkType = @linktype
WHERE n.ProjectConfigId = @projectconfigid AND n.GraphKind = @graphkind AND n.NodeType IN ( N'DfColumnElement', N'DfUnpivotSourceReferenceElement')
UNION ALL
SELECT pt.NodeFromId NodeFromId, n.BasicGraphNodeId NodeToId FROM dfPropTargets pt
INNER JOIN BIDoc.BasicGraphNodes n ON n.ParentId = pt.NodeToId
)
SELECT @dfPropTargetCount = COUNT(*) FROM dfPropTargets

PRINT 'Propagated DF target links - ' + CONVERT(VARCHAR(10), @dfPropTargetCount)

--EXEC sp_sequence_get_range @sequence_name = N'BIDoc.BasicGraphLinksSequence', @range_size = @dfPropTargetCount, @range_first_value = @linkSequenceStartVariant OUTPUT
--	SET @linkSequenceStart = CONVERT(INT, @linkSequenceStartVariant)

;WITH dfPropTargets AS
(
SELECT l.NodeFromId, l.NodeToId FROM BIDoc.BasicGraphNodes n 
INNER JOIN BIDoc.BasicGraphLinks l ON n.BasicGraphNodeId = l.NodeFromId AND l.LinkType = @linktype
WHERE n.ProjectConfigId = @projectconfigid AND n.GraphKind = @graphkind AND n.NodeType IN ( N'DfColumnElement', N'DfUnpivotSourceReferenceElement')
UNION ALL
SELECT pt.NodeFromId NodeFromId, n.BasicGraphNodeId NodeToId FROM dfPropTargets pt
INNER JOIN BIDoc.BasicGraphNodes n ON n.ParentId = pt.NodeToId
)
INSERT INTO BIDoc.BasicGraphLinks(
			--[BasicGraphLinkId]
           --,
		   [LinkType]
           ,[NodeFromId]
           ,[NodeToId]
		   )
	SELECT DISTINCT
	--ROW_NUMBER() OVER(ORDER BY t.NodeToId) + @linkSequenceStart - 1
	--,
	@linktype
	,t.NodeFromId
	,t.NodeToId
	FROM dfPropTargets t
--N'DimensionAttributeElement'


DECLARE @dfPropSourcesUpCount INT


;WITH dfPropMap AS
(
SELECT n.BasicGraphNodeId OriginalNodeId, n.BasicGraphNodeId, n.Description NodeDef FROM BIDoc.BasicGraphNodes n
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId 
WHERE (n.NodeType IN (N'DimensionAttributeElement'/*, N'SqlScriptElement'*/)
OR
(n.NodeType = N'SqlScriptElement' AND e.RefPath LIKE '%/View%')
) AND n.GraphKind = @graphkind
AND n.ProjectConfigId = @projectconfigid
UNION ALL

SELECT dfPropMap.OriginalNodeId, n.BasicGraphNodeId, dfPropMap.NodeDef FROM BIDoc.BasicGraphNodes n
INNER JOIN dfPropMap ON n.ParentId = dfPropMap.BasicGraphNodeId AND (n.NodeType <> N'SqlScriptElement' OR n.Description = dfPropMap.NodeDef)
)
SELECT DISTINCT l.NodeFromId, pm.OriginalNodeId NodeToId 
INTO #propUpTargets
FROM BIDoc.BasicGraphLinks l
INNER JOIN dfPropMap pm ON pm.BasicGraphNodeId = l.NodeToId AND l.LinkType = @linktype
LEFT JOIN BIDoc.BasicGraphLinks lr ON lr.NodeFromId = l.NodeFromId AND lr.NodeToId = pm.OriginalNodeId AND lr.LinkType = @linktype
WHERE lr.BasicGraphLinkId IS NULL AND l.NodeFromId <> pm.OriginalNodeId


DECLARE @propUpCount INT  = (SELECT COUNT(*) FROM #propUpTargets)
PRINT 'Target propagation links - ' + CONVERT(VARCHAR(10), @propUpCount)

--EXEC sp_sequence_get_range @sequence_name = N'BIDoc.BasicGraphLinksSequence', @range_size = @propUpCount, @range_first_value = @linkSequenceStartVariant OUTPUT
--	SET @linkSequenceStart = CONVERT(INT, @linkSequenceStartVariant)

INSERT INTO BIDoc.BasicGraphLinks(
			--[BasicGraphLinkId]
           --,
		   [LinkType]
           ,[NodeFromId]
           ,[NodeToId]
		   )
	SELECT DISTINCT
	--ROW_NUMBER() OVER(ORDER BY t.NodeFromId) + @linkSequenceStart - 1
	--,
	@linktype
	,t.NodeFromId
	,t.NodeToId
	FROM #propUpTargets t

------------- SqlScripts with future ------------------

;WITH targetNodes AS(
-- nodes with future - have links leading from them (?)
SELECT DISTINCT tn.BasicGraphNodeId FROM BIDoc.BasicGraphNodes tn
INNER JOIN BIDoc.BasicGraphLinks l ON l.NodeFromId = tn.BasicGraphNodeId 
WHERE tn.ProjectConfigId = @projectconfigid AND tn.GraphKind = @graphkind
AND l.LinkType = @linktype AND tn.NodeType IN( 'SqlScriptElement', N'SqlDmlSourceElement', 
	N'SqlNAryOperationOperandColumnElement', N'SqlNAryOperationOutputColumnElement')
)
,targetDescendants AS
(
SELECT BasicGraphNodeId, BasicGraphNodeId AS DescendantNodeId FROM targetNodes
UNION ALL
SELECT td.BasicGraphNodeId, n.BasicGraphNodeId DescendantNodeId FROM BIDoc.BasicGraphNodes n
INNER JOIN targetDescendants td ON td.DescendantNodeId = n.ParentId
)
-- descendants with lineage - have links leading to them -> ancestors with future will counter-inherit their links
INSERT INTO BIDoc.BasicGraphLinks
(NodeFromId, NodeToId, LinkType)
SELECT DISTINCT l.NodeFromId, td.BasicGraphNodeId NodeToId, @linktype
FROM targetDescendants td
INNER JOIN BIDoc.BasicGraphLinks l ON l.LinkType = @linktype AND l.NodeToId = td.DescendantNodeId
LEFT JOIN BIDoc.BasicGraphLinks exl ON exl.NodeFromId = l.NodeFromId AND exl.NodeToId = td.BasicGraphNodeId AND exl.LinkType = @linktype
WHERE exl.BasicGraphLinkId IS NULL
OPTION(MAXRECURSION  1000)


PRINT CONVERT(NVARCHAR(20), @@ROWCOUNT) + N' new links from SqlScript ancestors ''with future'' propagation'



;WITH targetNodes AS(
-- nodes with future - have links leading from them (?)
SELECT DISTINCT tn.BasicGraphNodeId FROM BIDoc.BasicGraphNodes tn
WHERE tn.ProjectConfigId = @projectconfigid AND tn.GraphKind = @graphkind AND tn.NodeType IN( N'ReportCalculatedMeasureElement', N'CubeCalculatedMeasureElement', N'DimensionAttributeElement')
)
,targetDescendants AS
(
SELECT BasicGraphNodeId, BasicGraphNodeId AS DescendantNodeId FROM targetNodes
UNION ALL
SELECT td.BasicGraphNodeId, n.BasicGraphNodeId DescendantNodeId FROM BIDoc.BasicGraphNodes n
INNER JOIN targetDescendants td ON td.DescendantNodeId = n.ParentId
)
-- descendants with lineage - have links leading to them -> ancestors with future will counter-inherit their links
INSERT INTO BIDoc.BasicGraphLinks
(NodeFromId, NodeToId, LinkType)
SELECT DISTINCT l.NodeFromId, td.BasicGraphNodeId NodeToId, @linktype
FROM targetDescendants td
INNER JOIN BIDoc.BasicGraphLinks l ON l.LinkType = @linktype AND l.NodeToId = td.DescendantNodeId
LEFT JOIN BIDoc.BasicGraphLinks exl ON exl.NodeFromId = l.NodeFromId AND exl.NodeToId = td.BasicGraphNodeId AND exl.LinkType = @linktype
WHERE exl.BasicGraphLinkId IS NULL
OPTION(MAXRECURSION  1000)


PRINT CONVERT(NVARCHAR(20), @@ROWCOUNT) + N' new links from SSAS descendants propagation'


DROP TABLE #propUpTargets


---------------------------------------------------



-- textboxes with lineage
;WITH targetNodes AS(
SELECT DISTINCT tn.BasicGraphNodeId FROM BIDoc.BasicGraphNodes tn
WHERE tn.ProjectConfigId = @projectconfigid AND tn.GraphKind = @graphkind
AND tn.NodeType IN( N'TextBoxElement')
)
,targetDescendants AS
(
SELECT BasicGraphNodeId, BasicGraphNodeId AS DescendantNodeId FROM targetNodes
UNION ALL
SELECT td.BasicGraphNodeId, n.BasicGraphNodeId DescendantNodeId FROM BIDoc.BasicGraphNodes n
INNER JOIN targetDescendants td ON td.DescendantNodeId = n.ParentId
)
-- descendants with lineage - have links leading to them -> ancestors with future will counter-inherit their links
INSERT INTO BIDoc.BasicGraphLinks
(NodeFromId, NodeToId, LinkType)

SELECT DISTINCT l.NodeFromId, td.BasicGraphNodeId NodeToId, @linktype
FROM targetDescendants td
INNER JOIN BIDoc.BasicGraphLinks l ON l.LinkType = @linktype AND l.NodeToId = td.DescendantNodeId
LEFT JOIN BIDoc.BasicGraphLinks exl ON exl.NodeFromId = l.NodeFromId AND exl.NodeToId = td.BasicGraphNodeId AND exl.LinkType = @linktype
WHERE exl.BasicGraphLinkId IS NULL
OPTION(MAXRECURSION  1000)

PRINT CONVERT(NVARCHAR(20), @@ROWCOUNT) + N' new links from textboxes propagation'


-- delete duplicate links
-- (links for which there is an equivalent link (same souce, target, type) with a lower ID)
;WITH fstLnks AS
(
SELECT MIN(l.BasicGraphLinkId) BasicGraphLinkId, l.NodeFromId, l.NodeToId, l.LinkType 
FROM BIDoc.BasicGraphLinks l
INNER JOIN BIDoc.BasicGraphNodes n ON l.NodeFromId = n.BasicGraphNodeId
WHERE n.ProjectConfigId = @projectconfigid
GROUP BY l.NodeFromId, l.NodeToId, l.LinkType
)
DELETE dl FROM BIDoc.BasicGraphLinks dl
INNER JOIN fstLnks f ON f.NodeFromId = dl.NodeFromId AND f.NodeToId = dl.NodeToId AND f.LinkType = dl.LinkType AND dl.BasicGraphLinkId <> f.BasicGraphLinkId


------------- SSAS hierarchies - descendants with lineage ------------------

;WITH targetNodes AS(
-- nodes with future - have links leading from them (?)
SELECT DISTINCT tn.BasicGraphNodeId FROM BIDoc.BasicGraphNodes tn
WHERE tn.ProjectConfigId = @projectconfigid AND tn.GraphKind = @graphkind
AND tn.NodeType IN( 'HierarchyElement')
)
,targetDescendants AS
(
SELECT BasicGraphNodeId, BasicGraphNodeId AS DescendantNodeId FROM targetNodes
UNION ALL
SELECT td.BasicGraphNodeId, n.BasicGraphNodeId DescendantNodeId FROM BIDoc.BasicGraphNodes n
INNER JOIN targetDescendants td ON td.DescendantNodeId = n.ParentId
)
-- descendants with lineage - have links leading to them -> ancestors with future will counter-inherit their links
INSERT INTO BIDoc.BasicGraphLinks
(NodeFromId, NodeToId, LinkType)
SELECT DISTINCT l.NodeFromId, td.BasicGraphNodeId NodeToId, @linktype
FROM targetDescendants td
INNER JOIN BIDoc.BasicGraphLinks l ON l.LinkType = @linktype AND l.NodeToId = td.DescendantNodeId
LEFT JOIN BIDoc.BasicGraphLinks exl ON exl.NodeFromId = l.NodeFromId AND exl.NodeToId = td.BasicGraphNodeId AND exl.LinkType = @linktype
WHERE exl.BasicGraphLinkId IS NULL
OPTION(MAXRECURSION  1000)


PRINT CONVERT(NVARCHAR(20), @@ROWCOUNT) + N' new links from hierarchy descendants propagation'



GO


