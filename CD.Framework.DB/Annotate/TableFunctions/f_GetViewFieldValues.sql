CREATE FUNCTION Annotate.f_GetViewFieldValues
(@viewId INT, @modelElementId INT = NULL)
RETURNS TABLE
AS RETURN
SELECT vals.FieldValueId, vals.AnnotationElementId, vals.FieldId, e.ModelElementId, vals.Value
FROM Annotate.AnnotationViewFields vf
INNER JOIN Annotate.FieldValues vals ON vals.FieldId = vf.FieldId
INNER JOIN Annotate.AnnotationElements e ON e.AnnotationElementId = vals.AnnotationElementId
WHERE vf.AnnotationViewId = @viewId AND ISNULL(@modelElementId, e.ModelElementId) = e.ModelElementId
AND e.IsCurrentVersion = 1
