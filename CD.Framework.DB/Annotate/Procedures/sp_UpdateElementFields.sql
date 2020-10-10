CREATE PROCEDURE [Annotate].[sp_UpdateElementFields]
	@projectConfigId UNIQUEIDENTIFIER,
	@fieldValues  [Annotate].[UDTT_FieldValues] READONLY,
	@links [Annotate].[UDTT_ElementLinks] READONLY,
	@modifiedModelElementIds [BIDoc].[UDTT_IdList] READONLY,
	@userId INT
AS

SELECT DISTINCT ae.AnnotationElementId, ae.ModelElementId
INTO #copiedElementIds
FROM Annotate.AnnotationElements ae
INNER JOIN @modifiedModelElementIds me ON me.Id = ae.ModelElementId
WHERE ae.IsCurrentVersion = 1
/*
LEFT JOIN @fieldValues v ON ae.ModelElementId = v.ModelElementId
LEFT JOIN @links l ON l.ModelElementFromId = v.ModelElementId
WHERE ae.IsCurrentVersion = 1 AND (l.ModelElementFromId IS NOT NULL OR v.ModelElementId IS NOT NULL)
*/

-- copy existing elements with new versions
CREATE TABLE #insertedElements
(
[ModelElementId] INT,
[AnnotationElementId] INT
)

INSERT INTO Annotate.AnnotationElements(
	ProjectConfigId
	,ModelElementId
	,[RefPath]
	,[Name]
	,[CreatedBy]
	,[UpdatedBy]
	,[VersionNumber]
	,[IsCurrentVersion]
	,[Date]
	)

OUTPUT
inserted.ModelElementId
,inserted.AnnotationElementId
INTO #insertedElements

SELECT
ee.ProjectConfigId
,ee.ModelElementId
,ee.RefPath
,ee.Name
,ee.CreatedBy
,@userId UpdatedBy
,VersionNumber + 1
,1 IsCurrentVersion
,GETDATE()
FROM Annotate.AnnotationElements ee
INNER JOIN #copiedElementIds ce ON ee.AnnotationElementId = ce.AnnotationElementId
WHERE ee.IsCurrentVersion = 1

-- copy field values
INSERT INTO Annotate.FieldValues(
	FieldId
	,AnnotationElementId
	,[Value]
	,UpdatedVersion
	)
SELECT
	fv.FieldId
	,ie.AnnotationElementId
	,fv.Value
	,fv.UpdatedVersion
FROM FieldValues fv
INNER JOIN Annotate.Fields f ON f.FieldId = fv.FieldId
INNER JOIN #copiedElementIds cid ON cid.AnnotationElementId = fv.AnnotationElementId
INNER JOIN #insertedElements ie ON ie.ModelElementId = cid.ModelElementId AND f.Deleted = 0

-- copy links
/*
INSERT INTO Annotate.ElementLinks(
	LinkTypeId,
	AnnotationElementFromId,
	AnnotationElementToId,
	UpdatedVersion
	)
SELECT
	l.LinkTypeId,
	ie.AnnotationElementId,
	l.AnnotationElementToId,
	l.UpdatedVersion
FROM Annotate.ElementLinks l
INNER JOIN #copiedElementIds cid ON cid.AnnotationElementId = l.AnnotationElementFromId
INNER JOIN #insertedElements ie ON ie.ModelElementId = cid.ModelElementId
*/

-- set old elements as not current
UPDATE ae SET IsCurrentVersion = 0 
FROM  Annotate.AnnotationElements ae
INNER JOIN #copiedElementIds ce ON ae.AnnotationElementId = ce.AnnotationElementId

-- add annotation elements if no previous version exists
INSERT INTO Annotate.AnnotationElements(ProjectConfigId, RefPath, ModelElementId, Name, VersionNumber, IsCurrentVersion, CreatedBy, UpdatedBy, Date)
SELECT @projectConfigId, e.RefPath, e.ModelElementId, e.Caption, 1, 1, @userId, @userId, GETDATE()
FROM (
	SELECT DISTINCT ModelElementId FROM @fieldValues
	UNION
	SELECT DISTINCT ModelElementFromId FROM @links
	UNION
	SELECT DISTINCT ModelElementToId FROM @links
	) v
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = v.ModelElementId
LEFT JOIN Annotate.AnnotationElements existent ON existent.ProjectConfigId = @projectConfigId AND existent.RefPath = e.RefPath
WHERE existent.AnnotationElementId IS NULL

-- merge field values
MERGE Annotate.FieldValues AS t 
USING (
	SELECT  e.AnnotationElementId, fv.FieldId, fv.Value, e.VersionNumber
	FROM @fieldValues fv
	INNER JOIN Annotate.AnnotationElements e ON fv.ModelElementId = e.ModelElementId AND e.IsCurrentVersion = 1
	) AS s
ON t.AnnotationElementId = s.AnnotationElementId AND t.FieldId = s.FieldId
WHEN NOT MATCHED THEN INSERT([AnnotationElementId], [FieldId], [Value], [UpdatedVersion])
VALUES(
[AnnotationElementId], [FieldId], [Value], VersionNumber
)
WHEN MATCHED THEN UPDATE SET t.[Value] = s.[Value], t.[UpdatedVersion] = s.[VersionNumber];

-- merge links
MERGE Annotate.ElementLinks AS t 
USING (
	SELECT  ef.AnnotationElementId AnnotationElementFromId, et.AnnotationElementId AnnotationElementToId, l.LinkTypeId, ef.VersionNumber
	FROM @links l
	INNER JOIN Annotate.AnnotationElements ef ON l.ModelElementFromId = ef.ModelElementId AND ef.IsCurrentVersion = 1
	INNER JOIN Annotate.AnnotationElements et ON l.ModelElementToId = et.ModelElementId AND et.IsCurrentVersion = 1
	) AS s
ON t.AnnotationElementFromId = s.AnnotationElementFromId AND t.AnnotationElementToId = s.AnnotationElementToId AND t.LinkTypeId = s.LinkTypeId
WHEN NOT MATCHED THEN INSERT([LinkTypeId], [AnnotationElementFromId], [AnnotationElementToId],  [UpdatedVersion])
VALUES(
[LinkTypeId], [AnnotationElementFromId], [AnnotationElementToId], VersionNumber
);
--WHEN NOT MATCHED BY SOURCE THEN DELETE;

DROP TABLE #insertedElements
DROP TABLE #copiedElementIds