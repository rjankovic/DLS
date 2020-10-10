CREATE FUNCTION [Inspect].[f_GetHighLevelLineageDestinations]
(
  @elementId INT
)
RETURNS TABLE
AS RETURN
(
 
 WITH descendants AS
(
SELECT n.BasicGraphNodeId 
FROM BIDoc.BasicGraphNodes n
WHERE n.SourceElementId = @elementId AND n.GraphKind = N'DataFlowTransitive'

UNION ALL

SELECT n.BasicGraphNodeId
FROM BIDoc.BasicGraphNodes n
INNER JOIN descendants d ON d.BasicGraphNodeId = n.ParentId
)
,targetLeaves AS(
SELECT --*
src.BasicGraphNodeId, src.TopologicalOrder, src.SourceElementId, src.ParentId
FROM descendants d
INNER JOIN BIDoc.BasicGraphLinks l ON l.NodeFromId = d.BasicGraphNodeId
INNER JOIN BIDoc.BasicGraphNodes src ON src.BasicGraphNodeId = l.NodeToId
WHERE l.LinkType = N'DataFlow'
--ORDER BY src.TopologicalOrder DESC
)
--SELECT * FROM sourceLeaves WHERE 
,ancestors AS
(
SELECT 
sl.BasicGraphNodeId, 
sl.TopologicalOrder, 
sl.BasicGraphNodeId AncestorNodeId, 
sl.ParentId AncestorParentId
FROM targetLeaves sl
INNER JOIN BIDoc.BasicGraphNodes a ON a.BasicGraphNodeId = sl.ParentId

UNION ALL

SELECT
sl.BasicGraphNodeId, 
sl.TopologicalOrder, 
a.BasicGraphNodeId AncestorNodeId, 
a.ParentId AncestorParentId
FROM ancestors sl
INNER JOIN BIDoc.BasicGraphNodes a ON a.BasicGraphNodeId = sl.AncestorParentId
)
--SELECT * FROM ancestors a WHERE a.AncestorNodeId = 21235622
,baseLevelAncestors AS
(
SELECT 
a.TopologicalOrder, n.Name, n.BasicGraphNodeId, n.NodeType, n.SourceElementId, n.[Description],
ROW_NUMBER() OVER(PARTITION BY n.BasicGraphNodeId ORDER BY a.TopologicalOrder DESC) RN
FROM ancestors a
INNER JOIN BIDoc.BasicGraphNodes n ON n.BasicGraphNodeId = a.AncestorNodeId
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
WHERE n.NodeType IN (
N'ViewElement',
N'PhysicalMeasureElement',
N'ReportElement',
N'SchemaTableElement',
N'ProcedureElement',
N'PackageElement',
N'CalculatedMeasureElement'
)
)
SELECT 
n.BasicGraphNodeId
,n.Name
,n.GraphKind
,n.[Description]
,n.NodeType
,n.ProjectConfigId
,n.SourceElementId
,ROW_NUMBER() OVER(ORDER BY
IIF(a.NodeType IN (N'ReportCalculatedMeasureElement', N'CubeCalculatedMeasureElement'), 1, 0),
a.TopologicalOrder) TopologicalOrder
,n.ParentId
,e.RefPath
,td.TypeDescription
,e.Type ElementType
FROM baseLevelAncestors a 
INNER JOIN BIDoc.BasicGraphNodes n ON n.BasicGraphNodeId = a.BasicGraphNodeId 
INNER JOIN BIDoc.ModelElements e ON n.SourceElementId = e.ModelElementId
INNER JOIN BIDoc.ModelElementTypeDescriptions td ON td.ElementType = e.Type
WHERE a.RN = 1
)
