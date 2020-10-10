CREATE FUNCTION Annotate.f_GetListLinksTo
(@projectConfigId UNIQUEIDENTIFIER, @modelElementId INT)
RETURNS TABLE
AS RETURN
SELECT l.ElementLinkId ,lt.LinkTypeName, fe.Name AS ElementFromCaption, fed.DescriptivePath AS ElementFromDescriptivePath, lt.LinkTypeId, fe.ModelElementId AS  ModelElementFromId, te.ModelElementId AS ModelElementToId,  UpdatedVersion
FROM Annotate.ElementLinks l
INNER JOIN Annotate.AnnotationElements te ON l.AnnotationElementToId = te.AnnotationElementId
INNER JOIN Annotate.AnnotationElements fe ON l.AnnotationElementFromId = fe.AnnotationElementId
INNER JOIN BIDoc.ModelElementDescriptivePaths fed ON fe.ModelElementId = fed.ModelElementId
INNER JOIN Annotate.LinkTypes lt ON l.LinkTypeId = lt.LinkTypeId
WHERE lt.ProjectConfigId = @projectConfigId AND te.ModelElementId = @modelElementId AND te.IsCurrentVersion = 1 AND fe.IsCurrentVersion = 1