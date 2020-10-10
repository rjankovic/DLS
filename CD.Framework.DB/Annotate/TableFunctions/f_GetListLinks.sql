CREATE FUNCTION Annotate.f_GetListLinks
(@projectConfigId UNIQUEIDENTIFIER, @elementType NVARCHAR(MAX),@refPath NVARCHAR(MAX))
RETURNS TABLE
AS RETURN
SELECT l.ElementLinkId ,lt.LinkTypeName, lt.LinkTypeId, ef.ModelElementId AS ModelElementFromId, et.ModelElementId AS ModelElementToId, et.Name AS ElementToCaption,
met.RefPath AS ElementToDescriptivePath, UpdatedVersion
FROM Annotate.ElementLinks l
INNER JOIN Annotate.AnnotationElements et ON l.AnnotationElementToId = et.AnnotationElementId
INNER JOIN BIDoc.ModelElements met ON met.ModelElementId = et.ModelElementId
INNER JOIN Annotate.AnnotationElements ef ON l.AnnotationElementFromId = ef.AnnotationElementId
INNER JOIN BIDoc.ModelElements mef ON mef.ModelElementId = ef.ModelElementId
INNER JOIN Annotate.LinkTypes lt ON l.LinkTypeId = lt.LinkTypeId
WHERE lt.ProjectConfigId = @projectConfigId AND et.IsCurrentVersion = 1 AND ef.IsCurrentVersion = 1 AND mef.Type = @elementType
AND (mef.RefPathIntervalStart >= (SELECT RefPathIntervalStart FROM BIDoc.ModelElements WHERE RefPath = @refPath AND ProjectConfigId = @projectConfigId) AND mef.RefPathIntervalStart <= (SELECT RefPathIntervalEnd FROM BIDoc.ModelElements WHERE RefPath = @refPath AND ProjectConfigId = @projectConfigId))