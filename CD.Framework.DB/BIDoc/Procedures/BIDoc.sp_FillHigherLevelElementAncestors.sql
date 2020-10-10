CREATE PROCEDURE [BIDoc].[sp_FillHigherLevelElementAncestors]
	@projectConfigId UNIQUEIDENTIFIER
AS

DECLARE @graphkind NVARCHAR(50) = N'DataFlow'


DELETE ea FROM BIDoc.HigherLevelElementAncestors ea
INNER JOIN BIDoc.ModelElements e ON ea.SouceElementId = e.ModelElementId
WHERE e.ProjectConfigId = @projectConfigId

;WITH ancestors as
(
SELECT n.BasicGraphNodeId OriginalNodeId, n.SourceElementId OriginalElementId, n.BasicGraphNodeId, n.ParentId, e.ModelElementId, e.Caption, e.Type, 0 AncestorLevel 
FROM BIDoc.BasicGraphNodes n
INNER JOIN BIDoc.ModelElements e ON n.SourceElementId = e.ModelElementId
WHERE n.GraphKind = @graphkind AND n.ProjectConfigId = @projectconfigid

UNION ALL

SELECT a.OriginalNodeId, a.OriginalElementId OriginalElementId, n.BasicGraphNodeId, n.ParentId, e.ModelElementId, e.Caption, e.Type, a.AncestorLevel + 1 AncestorLevel 
FROM BIDoc.BasicGraphNodes n
INNER JOIN BIDoc.ModelElements e ON n.SourceElementId = e.ModelElementId
INNER JOIN ancestors a ON a.ParentId = n.BasicGraphNodeId
--WHERE n.GraphKind = @graphkind AND n.ProjectConfigId = @projectconfigid
)
,detailLevels AS
(
SELECT a.*, dl.DetailLevel
FROM ancestors a
INNER JOIN BIDoc.ModelElementTypeDetailLevels dl ON dl.ElementType = a.Type
)
,closestDetailLevelsPrep AS
(
SELECT dl.ModelElementId, dl.OriginalNodeId, dl.DetailLevel, dl.OriginalElementId,
dl.AncestorLevel FirstAncestorLevel, 
FIRST_VALUE(dl.BasicGraphNodeId) OVER(PARTITION BY dl.DetailLevel, dl.OriginalNodeId ORDER BY dl.AncestorLevel) DetailLevelNodeId,
FIRST_VALUE(dl.ModelElementId) OVER(PARTITION BY dl.DetailLevel, dl.OriginalNodeId ORDER BY dl.AncestorLevel) DetailLevelElementId,
FIRST_VALUE(dl.Type) OVER(PARTITION BY dl.DetailLevel, dl.OriginalNodeId ORDER BY dl.AncestorLevel) DetailLevelType,
FIRST_VALUE(dl.Caption) OVER(PARTITION BY dl.DetailLevel, dl.OriginalNodeId ORDER BY dl.AncestorLevel) DetailLevelElementName
,ROW_NUMBER() OVER(PARTITION BY dl.DetailLevel, dl.OriginalNodeId ORDER BY dl.AncestorLevel) RN
FROM detailLevels dl
--GROUP BY dl.OriginalNodeId, dl.DetailLevel
)
,closestDetailLevels AS
(
SELECT * FROM closestDetailLevelsPrep WHERE RN = 1
)
SELECT * 
INTO #upperLevels
FROM closestDetailLevels
OPTION(MAXRECURSION 1000)


INSERT INTO BIDoc.HigherLevelElementAncestors(
[SouceElementId],
[AncestorElementId],
[SouceDfNodeId],
[AncestorDfNodeId],
[DetailLevel]
)
SELECT OriginalElementId, DetailLevelElementId, OriginalNodeId, DetailLevelNodeId, DetailLevel 
FROM #upperLevels



DROP TABLE #upperLevels



RETURN 0
