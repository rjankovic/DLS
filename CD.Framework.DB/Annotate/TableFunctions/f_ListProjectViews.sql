CREATE FUNCTION Annotate.f_ListProjectViews
(@projectConfigId UNIQUEIDENTIFIER)
RETURNS TABLE
AS RETURN
SELECT AnnotationViewId, ViewName, dsc.ElementType, dsc.TypeDescription
FROM Annotate.AnnotationViews vw
LEFT JOIN BIDoc.ModelElementTypeDescriptions dsc ON N'Type_' + dsc.ElementType = vw.ViewName
WHERE vw.ProjectConfigId = @projectConfigId