CREATE FUNCTION Annotate.f_GetListLinksFrom
(@projectConfigId UNIQUEIDENTIFIER, @modelElementId INT)
RETURNS TABLE
AS RETURN
SELECT l.ElementLinkId ,lt.LinkTypeName, te.Name AS ElementToCaption, ted.DescriptivePath AS ElementToDescriptivePath, lt.LinkTypeId, te.ModelElementId AS ModelElementToId, fe.ModelElementId AS ModelElementFromId, UpdatedVersion
FROM Annotate.ElementLinks l
INNER JOIN Annotate.AnnotationElements fe ON l.AnnotationElementFromId = fe.AnnotationElementId
INNER JOIN Annotate.AnnotationElements te ON l.AnnotationElementToId = te.AnnotationElementId
INNER JOIN BIDoc.ModelElementDescriptivePaths ted ON te.ModelElementId = ted.ModelElementId
INNER JOIN Annotate.LinkTypes lt ON l.LinkTypeId = lt.LinkTypeId
WHERE lt.ProjectConfigId = @projectConfigId AND fe.ModelElementId = @modelElementId AND te.IsCurrentVersion = 1 AND fe.IsCurrentVersion = 1