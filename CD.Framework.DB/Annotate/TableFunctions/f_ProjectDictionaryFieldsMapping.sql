CREATE FUNCTION Annotate.f_ProjectDictionaryFieldsMapping
(@projectconfigid UNIQUEIDENTIFIER)
RETURNS TABLE
AS RETURN
SELECT f.FieldId, f.FieldName, vf.FieldOrder, REPLACE(v.ViewName, N'Type_', N'') ElementType, v.AnnotationViewId
FROM Annotate.Fields f 
INNER JOIN Annotate.AnnotationViewFields vf ON f.FieldId = vf.FieldId
INNER JOIN Annotate.AnnotationViews v ON vf.AnnotationViewId = v.AnnotationViewId
WHERE v.ProjectConfigId = @projectconfigid AND f.Deleted = 0