CREATE PROCEDURE [Adm].[sp_DeleteProjectConfig]
	@projectconfigid UNIQUEIDENTIFIER
AS

DELETE FROM Inspect.HighLevelSolutionTrees WHERE ProjectConfigId = @projectconfigid

DELETE FROM BIDoc.LineageGridHistory WHERE ProjectConfigId = @projectconfigid

DELETE fv FROM Annotate.FieldValues fv
INNER JOIN Annotate.AnnotationElements e ON e.AnnotationElementId = fv.AnnotationElementId
WHERE e.ProjectConfigId = @projectconfigid

DELETE f FROM Annotate.AnnotationViewFields f
INNER JOIN Annotate.AnnotationViews v ON f.AnnotationViewId = v.AnnotationViewId
WHERE v.ProjectConfigId = @projectconfigid

DELETE FROM Annotate.Fields WHERE ProjectConfigId = @projectconfigid

DELETE l FROM Annotate.ElementLinks l
INNER JOIN Annotate.AnnotationElements e ON l.AnnotationElementFromId = e.AnnotationElementId
WHERE e.ProjectConfigId = @projectconfigid

DELETE FROM Annotate.LinkTypes WHERE ProjectConfigId = @projectconfigid

DELETE e FROM Annotate.AnnotationElements e
WHERE e.ProjectConfigId = @projectconfigid



DELETE FROM Annotate.AnnotationViews WHERE ProjectConfigId = @projectconfigid

EXEC BIDoc.sp_ClearModel @projectconfigid

DECLARE @rc INT

SET @rc = 1
WHILE @rc > 0
BEGIN
DELETE TOP (10000) l FROM [BIDoc].[ModelLinks] l
INNER JOIN [BIDoc].[ModelElements] ef ON l.ElementFromId = ef.ModelElementId
INNER JOIN [BIDoc].[ModelElements] et ON l.ElementToId = et.ModelElementId
WHERE ef.ProjectConfigId = @projectconfigid --AND (ef.Type LIKE N'CD.DLS.Model.Mssql.%' OR et.Type LIKE N'CD.DLS.Model.Mssql.%')
SELECT @rc = @@ROWCOUNT
END


SET @rc = 1
WHILE @rc > 0
BEGIN
DELETE TOP (10000) FROM [BIDoc].[ModelElements] 
WHERE ProjectConfigId = @projectconfigid --AND [Type] LIKE N'CD.DLS.Model.Mssql.%'
SELECT @rc = @@ROWCOUNT
END


SET @rc = 1
WHILE @rc > 0
BEGIN
	DELETE TOP (1000) ei FROM stg.ExtractItems ei
	INNER JOIN stg.Extracts e ON ei.ExtractId = e.ExtractId
	WHERE e.ProjectConfigId = @projectconfigid

	SELECT @rc = @@ROWCOUNT
END

DELETE e FROM stg.Extracts e
WHERE e.ProjectConfigId = @projectconfigid


DELETE FROM adm.RequestMessages WHERE Project_ProjectConfigId = @projectconfigid
DELETE FROM Adm.MssqlAgentProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
DELETE FROM Adm.MssqlDbProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
DELETE FROM Adm.SsasDbProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
DELETE FROM Adm.SsisProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
DELETE FROM Adm.SsrsProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
DELETE FROM Adm.PowerBiProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid

DELETE FROM adm.BroadcastMessages WHERE ProjectConfigId = @projectconfigid

DELETE FROM Adm.ProjectConfigs WHERE ProjectConfigId = @projectconfigid
