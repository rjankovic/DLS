CREATE FUNCTION [Inspect].[f_ListExternalDfSources]
(
	@projectconfigid UNIQUEIDENTIFIER
)
RETURNS TABLE AS RETURN
(

SELECT 
se.ModelElementId, 
/**/ISNULL(JSON_VALUE(se.ExtendedProperties, '$.Command'), JSON_VALUE(se.ExtendedProperties, '$.OpenRowset'))/**/ /*N''*/ Command,
se.RefPath,
se.Caption,
mngre.Caption ManagerCaption,
/**/JSON_VALUE(mngre.ExtendedProperties, N'$.SourceType')/**/ /*adm.f_SimpleJsonValue(mngre.ExtendedProperties, N'SourceType')*/ SourceType,
/**/JSON_VALUE(mngre.ExtendedProperties, N'$.ConnectionString')/**/ /*adm.f_SimpleJsonValue(mngre.ExtendedProperties, N'ConnectionString')*/ ConnectionString,
/**/JSON_VALUE(mngre.ExtendedProperties, N'$.LocaleID')/**/ /*adm.f_SimpleJsonValue(mngre.ExtendedProperties, N'LocaleID')*/ LocaleID,
/**/JSON_VALUE(mngre.ExtendedProperties, N'$.CodePage')/**/ /*adm.f_SimpleJsonValue(mngre.ExtendedProperties, N'CodePage')*/ CodePage,
/**/JSON_VALUE(mngre.ExtendedProperties, N'$.Format')/**/ /*adm.f_SimpleJsonValue(mngre.ExtendedProperties, N'Format')*/ FileFormat
,pkge.ModelElementId PackageElementId
,pkge.RefPath PackageRefPath
,pkge.Caption PackageCaption
FROM BIDoc.ModelElements se
INNER JOIN BIDoc.ModelLinks mngrl ON se.ModelElementId = mngrl.ElementFromId AND mngrl.Type = N'SourceConnection'
INNER JOIN BIDoc.ModelElements mngre ON mngre.ModelElementId = mngrl.ElementToId
INNER JOIN BIDoc.ModelElements pkge ON LEFT(se.RefPath, LEN(pkge.RefPath)) = pkge.RefPath
WHERE se.Type = N'CD.DLS.Model.Mssql.Ssis.DfSourceElement' 
--AND adm.f_SimpleJsonValue(se.ExtendedProperties, N'IsExternalSource') = N'true' 
AND JSON_VALUE(se.ExtendedProperties, '$.IsExternalSource') = N'true' 
AND se.ProjectConfigId = @projectconfigid
AND pkge.Type = N'CD.DLS.Model.Mssql.Ssis.PackageElement' AND pkge.ProjectConfigId = @projectconfigid

)
