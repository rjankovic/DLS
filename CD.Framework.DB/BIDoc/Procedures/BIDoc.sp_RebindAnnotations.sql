CREATE PROCEDURE [BIDoc].[sp_RebindAnnotations]
	@projectconfigid UNIQUEIDENTIFIER
AS


UPDATE ae SET ModelElementId = me.ModelElementId
FROM Annotate.AnnotationElements ae
INNER JOIN BIDoc.ModelElements me ON ae.ProjectConfigId = me.ProjectConfigId AND ae.RefPath = me.RefPath
WHERE ae.ProjectConfigId = @projectconfigid


