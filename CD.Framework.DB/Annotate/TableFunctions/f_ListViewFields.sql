CREATE FUNCTION Annotate.f_ListViewFields
(@viewId INT)
RETURNS TABLE
AS RETURN
SELECT vf.AnnotationViewId, f.FieldId, f.FieldName, vf.FieldOrder 
FROM Annotate.Fields f 
INNER JOIN Annotate.AnnotationViewFields vf ON f.FieldId = vf.FieldId
--LEFT JOIN BIDoc.ModelElementTypeDescriptions dsc ON N'Type_' + dsc.ElementType = 
WHERE vf.AnnotationViewId = @viewId
