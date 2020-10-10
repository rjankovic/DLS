CREATE PROCEDURE [BIDoc].[sp_ListSsrsReports]
	@projectconfigid UNIQUEIDENTIFIER
AS

SELECT 
e.ModelElementId, 
e.RefPath, 
JSON_VALUE(e.ExtendedProperties, '$.SsrsPath') SsrsPath, 
CONVERT(INT, JSON_VALUE(e.ExtendedProperties, '$.SsrsComponentId')) SsrsComponentId, 
e.Caption 
FROM BIDoc.ModelElements e 
WHERE e.[Type] = N'CD.DLS.Model.Mssql.Ssrs.ReportElement' 
AND e.ProjectConfigId = @projectconfigid

