CREATE PROCEDURE [BIDoc].[sp_SetElementSubtreeContents]
		@projectconfigid UNIQUEIDENTIFIER
AS

UPDATE BIDoc.ModelElements SET SubtreeContent = NULL WHERE ProjectConfigId = @projectconfigid

CREATE TABLE #elementIds
(
ModelElementId INT NOT NULL
)

;WITH bottomElementIds AS(
SELECT DISTINCT e.ModelElementId FROM BIDoc.ModelElements e 
LEFT JOIN BIDoc.ModelLinks l ON l.ElementToId = e.ModelElementId AND l.Type = N'parent'
LEFT JOIN BIDoc.ModelElements sube ON sube.ModelElementId = l.ElementFromId AND sube.SubtreeContent IS NULL
WHERE e.SubtreeContent IS NULL AND sube.ModelElementId IS NULL AND e.ProjectConfigId = @projectconfigid
AND e.RefPath NOT LIKE 'Business%'
)
UPDATE e SET SubtreeContent = N'(' + e.RefPathSuffix + N' ' + e.ExtendedProperties + N')'  FROM bottomElementIds ids 
INNER JOIN BIDoc.ModelElements e ON ids.ModelElementId = e.ModelElementId

DECLARE @repeat INT = 1


WHILE @repeat = 1
BEGIN
	TRUNCATE TABLE #elementIds

	;WITH elementsToFill AS(
		-- dont have a child without filled subtreecontent and themselves have empty subtreecontent
		SELECT e.ModelElementId
		FROM BIDoc.ModelElements e
		INNER JOIN BIDoc.ModelElementTypeDetailLevels dl ON dl.ElementType = e.Type
		WHERE 
		e.SubtreeContent IS NULL
		AND e.ProjectConfigId = @projectconfigid
		AND e.RefPath NOT LIKE 'Business%'
		AND dl.DetailLevel IN (1,2)
		AND NOT EXISTS(
			SELECT TOP 1 1 FROM BIDoc.ModelLinks l
			INNER JOIN BIDoc.ModelElements sube ON l.ElementFromId = sube.ModelElementId
			WHERE l.ElementToId = e.ModelElementId AND l.Type = N'parent' AND sube.SubtreeContent IS NULL
		)
	)
	INSERT INTO #elementIds(ModelElementId)
	SELECT ModelElementId 
	FROM elementsToFill

	--SELECT TOP 10 * FROM #elementIds
--	SELECT COUNT(*) FROM BiDoc.ModelElements WHERE ProjectConfigId = N'4290174E-AA85-4704-A4BA-DD910C1A0850' AND SubTreeContent IS NULL
--AND RefPath NOT LIKE 'Business%'

	UPDATE e SET SubtreeContent = N'(' + e.RefPathSuffix + N' ' + e.ExtendedProperties +
		(
			SELECT STRING_AGG(sube.SubtreeContent, N',') WITHIN GROUP (ORDER BY sube.RefPath)
			FROM BIDoc.ModelElements sube
			INNER JOIN BIDoc.ModelLinks subl ON subl.Type = N'parent' AND subl.ElementFromId = sube.ModelElementId
			WHERE subl.ElementToId = e.ModelElementId AND sube.RefPath NOT LIKE 'Business%'
		) + N')'
	FROM BIDoc.ModelElements e
	INNER JOIN #elementIds ids ON ids.ModelElementId = e.ModelElementId

	SELECT @repeat = IIF(EXISTS(SELECT TOP 1 1 FROM #elementIds), 1, 0)
END

DROP TABLE #elementIds
