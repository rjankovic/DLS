CREATE PROCEDURE [BIDOc].[sp_SetRefPathIntervals]
	@projectConfigId UNIQUEIDENTIFIER,
	@requestId UNIQUEIDENTIFIER = NULL
AS

IF OBJECT_ID('tempdb.dbo.#intervals') IS NOT NULL
DROP TABLE #intervals
IF OBJECT_ID('tempdb.dbo.#ranking') IS NOT NULL
DROP TABLE #ranking


SELECT ModelElementId, RefPath, l.ElementToId ParentElementId, ROW_NUMBER() OVER(ORDER BY RefPath) RN
INTO #ranking
FROM BIDoc.ModelElements e
LEFT JOIN BIDoc.ModelLinks l ON l.Type = N'parent' AND l.ElementFromId = e.ModelElementId 
WHERE ProjectConfigId = @projectConfigId

--SELECT * FROM #ranking

;WITH descendants AS(
SELECT r.ModelElementId, r.ModelElementId DescendantId, r.RefPath DRP, r.RN DRN FROM #ranking r

UNION ALL

SELECT descendants.ModelElementId, ch.ModelElementId, ch.RefPath DRP, ch.RN DRN  
FROM #ranking ch INNER JOIN descendants 
ON ch.ParentElementId = descendants.DescendantId
)
SELECT r.ModelElementId, r.RefPath, r.RN,
MAX(d.DRN) LastDescendantRN
INTO #intervals
FROM #ranking r
INNER JOIN descendants d ON r.ModelElementId = d.ModelElementId
GROUP BY r.ModelElementId, r.RefPath, r.RN
OPTION (MAXRECURSION 1000)

UPDATE e SET RefPathIntervalStart = i.RN, RefPathIntervalEnd = i.LastDescendantRN FROM #intervals i
INNER JOIN BIDoc.ModelElements e ON i.ModelElementId = e.ModelElementId

UPDATE n SET RefPathIntervalStart = i.RN, RefPathIntervalEnd = i.LastDescendantRN FROM #intervals i
INNER JOIN BIDoc.BasicGraphNodes n ON i.ModelElementId = n.SourceElementId

DROP TABLE #intervals
DROP TABLE #ranking

IF @requestId IS NOT NULL
BEGIN
-- notify the WF followup
EXEC [Adm].[sp_SaveDbOperationFinishedMessage] @requestId
END

RETURN 0