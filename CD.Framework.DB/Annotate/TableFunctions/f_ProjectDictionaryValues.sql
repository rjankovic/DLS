CREATE FUNCTION Annotate.f_ProjectDictionaryValues
(@projectconfigid UNIQUEIDENTIFIER)
RETURNS TABLE
AS RETURN
SELECT ae.AnnotationElementId 
      ,ae.ModelElementId
	  ,f.FieldName
	  ,f.FieldId
	  ,fv.Value
	  ,fv.FieldValueId
  FROM [Annotate].[AnnotationElements] ae
  INNER JOIN [Annotate].[FieldValues] fv ON fv.AnnotationElementId = ae.AnnotationElementId
  INNER JOIN [Annotate].[Fields] f ON f.FieldId = fv.FieldId
  WHERE ae.ProjectConfigId = @projectconfigid AND ae.IsCurrentVersion = 1 AND ae.ModelElementId IS NOT NULL

