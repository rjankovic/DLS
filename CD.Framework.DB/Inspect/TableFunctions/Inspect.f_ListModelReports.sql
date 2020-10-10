CREATE FUNCTION [Inspect].[f_ListModelReports]
(
	@projectconfigid UNIQUEIDENTIFIER
)
RETURNS TABLE AS RETURN
(

WITH srvs AS
(
SELECT ModelElementId FROM BIDoc.ModelElements e WHERE e.Type = N'CD.DLS.Model.Mssql.Ssrs.ServerElement'
AND e.ProjectConfigId = @projectconfigid
)
,folders AS
(
SELECT f.Caption ItemPath, f.ModelElementId FROM srvs
INNER JOIN BIDoc.ModelLinks l ON l.Type = 'parent' AND l.ElementToId = srvs.ModelElementId
INNER JOIN BIDoc.ModelElements f ON f.ModelElementId = l.ElementFromId
WHERE f.Type = N'CD.DLS.Model.Mssql.Ssrs.FolderElement'

UNION ALL

SELECT IIF(folders.ItemPath = N'/', folders.ItemPath, folders.ItemPath + N'/') + subf.Caption, subf.ModelElementId 
FROM folders
INNER JOIN BIDoc.ModelLinks l ON l.ElementToId = folders.ModelElementId AND l.Type = 'parent'
INNER JOIN BIDoc.ModelElements subf ON subf.ModelElementId = l.ElementFromId
WHERE subf.Type = N'CD.DLS.Model.Mssql.Ssrs.FolderElement'
)
SELECT IIF(folders.ItemPath = N'/', folders.ItemPath, folders.ItemPath + N'/') + re.Caption ItemPath, re.Caption, re.ModelElementId, re.RefPath 
FROM folders
INNER JOIN BIDoc.ModelLinks l ON l.ElementToId = folders.ModelElementId AND l.Type = 'parent'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementFromId
WHERE re.Type = N'CD.DLS.Model.Mssql.Ssrs.ReportElement'

)
