CREATE PROCEDURE [Annotate].[sp_UpdateModelElememntReferences]
	@projectconfigId UNIQUEIDENTIFIER
AS

UPDATE  Annotate.AnnotationElements 
SET     ModelElementId = m.ModelElementId
FROM    Annotate.AnnotationElements a
INNER JOIN    BIDoc.ModelElements m ON m.RefPath = a.RefPath AND m.ProjectConfigId = @projectconfigId  AND a.ProjectConfigId = @projectconfigId