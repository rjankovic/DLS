CREATE FUNCTION [Inspect].[f_ListExternalDfSourceColumns]
(
	@projectconfigid UNIQUEIDENTIFIER
)
RETURNS TABLE AS
RETURN
(

WITH externalDfSources AS(
SELECT 
se.ModelElementId, 
/**/ISNULL(JSON_VALUE(se.ExtendedProperties, '$.Command'), JSON_VALUE(se.ExtendedProperties, '$.OpenRowset')) /**/ /*N''*/ Command,
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
SELECT se.ModelElementId SourceElementId, ce.ModelElementId, ce.Caption ColumnName
,/**/JSON_VALUE(ce.ExtendedProperties, '$.DtsDataType') /**/ /*adm.f_SimpleJsonValue(ce.ExtendedProperties, N'DtsDataType')*/ DataType
,/**/JSON_VALUE(ce.ExtendedProperties, '$.Length') /**/ /*adm.f_SimpleJsonValue(ce.ExtendedProperties, N'Length')*/ Length
,/**/JSON_VALUE(ce.ExtendedProperties, '$.Precision') /**/ /*adm.f_SimpleJsonValue(ce.ExtendedProperties, N'Precision')*/ Precision
,/**/JSON_VALUE(ce.ExtendedProperties, '$.Scale') /**/ /*adm.f_SimpleJsonValue(ce.ExtendedProperties, N'Scale')*/ Scale
,ce.RefPath

FROM externalDfSources se
INNER JOIN BIDoc.BasicGraphNodes n ON n.SourceElementId = se.ModelElementId AND n.GraphKind = N'DataFlow'
INNER JOIN BIDoc.BasicGraphNodes c1 ON c1.ParentId = n.BasicGraphNodeId AND c1.NodeType = N'DfOutputElement'
INNER JOIN BIDoc.ModelElements oe ON oe.ModelElementId = c1.SourceElementId
INNER JOIN BIDoc.BasicGraphNodes c2 ON c2.ParentId = c1.BasicGraphNodeId
INNER JOIN BIDoc.ModelElements ce ON ce.ModelElementId = c2.SourceElementId

WHERE oe.Type = N'CD.DLS.Model.Mssql.Ssis.DfOutputElement' 
AND JSON_VALUE(oe.ExtendedProperties, '$.OutputType') = N'0' 
AND ce.Type = N'CD.DLS.Model.Mssql.Ssis.DfColumnElement'


)