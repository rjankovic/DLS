CREATE FUNCTION [Annotate].[f_GetBusinessDescriptionByMEId]
(
	@elementid INT,
	@projectConfigId UNIQUEIDENTIFIER
)
RETURNS TABLE AS RETURN
(
SELECT f.FieldName
	  ,fv.Value	
  FROM [Annotate].[AnnotationElements] ae
  INNER JOIN [Annotate].[AnnotationViews] av ON av.ProjectConfigId = ae.ProjectConfigId
  INNER JOIN [Annotate].[AnnotationViewFields] avf ON avf.AnnotationViewId = av.AnnotationViewId
  INNER JOIN [Annotate].[Fields] f ON f.FieldId = avf.FieldId
  INNER JOIN [Annotate].[FieldValues] fv ON fv.FieldId = f.FieldId
  WHERE ae.ModelElementId = @elementid AND ae.ProjectConfigId = @projectConfigId
)
