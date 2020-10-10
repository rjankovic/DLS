CREATE PROCEDURE [Search].[sp_FindRootElements]
@projectConfigId UNIQUEIDENTIFIER
AS

DELETE FROM Search.RootElements WHERE ProjectConfigId = @projectConfigId

INSERT INTO Search.RootElements
(
ProjectConfigId,
ModelElementId,
Caption
)

SELECT @projectConfigId, e.ModelElementId, dp.DescriptivePath 
FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelElementDescriptivePaths dp ON e.ModelElementId = dp.ModelElementId
LEFT JOIN BIDoc.ModelLinks pl ON pl.Type = N'parent' AND pl.ElementFromId = e.ModelElementId
LEFT JOIN BIDoc.ModelElements parentFolder ON parentFolder.ModelElementId = pl.ElementToId AND parentFolder.Type = N'CD.DLS.Model.Mssql.Ssrs.FolderElement'
WHERE e.[Type] IN (
 N'CD.DLS.Model.Mssql.Db.DatabaseElement'
 ,N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement'
 ,N'CD.DLS.Model.Mssql.Ssis.ProjectElement'
  ,N'CD.DLS.Model.Mssql.Pbi.TenantElement'
 ,N'CD.DLS.Model.Mssql.Ssrs.FolderElement'
 ,N'CD.DLS.Model.Business.Organization.BusinessRootElement')
AND parentFolder.ModelElementId IS NULL
AND e.ProjectConfigId = @projectConfigId
