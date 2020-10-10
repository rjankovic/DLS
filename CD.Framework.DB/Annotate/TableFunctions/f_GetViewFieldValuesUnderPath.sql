CREATE FUNCTION Annotate.f_GetViewFieldValuesUnderPath
(@viewId INT, @path NVARCHAR(MAX), @type NVARCHAR(200))
RETURNS TABLE
AS RETURN
SELECT vals.FieldValueId, vals.AnnotationElementId, vals.FieldId, e.ModelElementId, vals.Value
FROM Annotate.AnnotationViewFields vf
INNER JOIN Annotate.FieldValues vals ON vals.FieldId = vf.FieldId
INNER JOIN Annotate.AnnotationElements e ON e.AnnotationElementId = vals.AnnotationElementId
INNER JOIN BIDoc.ModelElements me ON me.ModelElementId = e.ModelElementId
WHERE vf.AnnotationViewId = @viewId
AND e.IsCurrentVersion = 1

AND
  me.RefPathPrefix >= LEFT(@path, 300) AND me.RefPathPrefix <= LEFT(@path, 300) + N'~' AND 
  me.RefPath >= @path AND me.RefPath <= @path + N'~'
  AND me.[Type] = @type
