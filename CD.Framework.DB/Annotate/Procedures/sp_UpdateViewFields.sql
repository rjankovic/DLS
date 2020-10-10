CREATE PROCEDURE [Annotate].[sp_UpdateViewFields]
	@annotationViewId INT,
	@fields [BIDoc].[UDTT_OrderedIdList] READONLY
AS

DELETE FROM Annotate.AnnotationViewFields WHERE @annotationViewId = AnnotationViewId

INSERT INTO Annotate.AnnotationViewFields(AnnotationViewId, FieldId, FieldOrder)
SELECT @annotationViewId, f.Id, f.[Order]
FROM @fields f
