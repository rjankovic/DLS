CREATE FUNCTION Annotate.f_ListFields
(@projectConfigId UNIQUEIDENTIFIER)
RETURNS TABLE
AS RETURN
SELECT f.Deleted, f.FieldId, f.FieldName, IIF((EXISTS (SELECT TOP 1 1 FROM Annotate.AnnotationViewFields vf WHERE vf.FieldId = f.FieldId)), 1, 0) UsedInViews
FROM Annotate.Fields f 
WHERE f.ProjectConfigId = @projectConfigId
AND Deleted = 0
