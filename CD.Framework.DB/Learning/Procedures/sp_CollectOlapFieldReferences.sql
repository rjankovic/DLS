CREATE PROCEDURE [Learning].[sp_CollectOlapFieldReferences]
	@projectconfigid UNIQUEIDENTIFIER
AS

DELETE FROM Learning.OlapFieldReferences WHERE ProjectConfigId = @projectconfigid
/*
;WITH x AS(
---------- pivot tables
SELECT e.ProjectConfigId, SUBSTRING(e.RefPath, 0, CHARINDEX(N'/Field[', e.RefPath)) [GroupPath], 
IIF(
JSON_VALUE(e.ExtendedProperties, '$.Orientation') IN(N'0', N'1'), N'Axis', 
IIF(JSON_VALUE(e.ExtendedProperties, '$.Orientation') = N'2', N'Measure', N'Filter')
) [FieldType],
e.RefPath, N'[' + IIF(JSON_VALUE(e.ExtendedProperties, '$.Orientation') = N'2', N'Measures', he.Caption) + N'].[' + le.Caption + N']' FieldReference, 
le.ModelElementId FieldElementId, IIF(JSON_VALUE(e.ExtendedProperties, '$.Orientation') = N'2', le.Caption, N'[' + he.Caption + N'].[' + le.Caption + N']') FieldName 
FROM BIDoc.ModelElements e 
INNER JOIN BIDoc.ModelLinks l ON l.ElementFromId = e.ModelElementId
INNER JOIN BIDoc.ModelElements le ON le.ModelElementId = l.ElementToId
INNER JOIN BIDoc.ModelLinks hl ON hl.ElementFromId = le.ModelElementId AND hl.Type = N'parent'
INNER JOIN BIDoc.ModelElements he ON he.ModelElementId = hl.ElementToId
WHERE e.Type = N'CD.DLS.Model.Business.Excel.PivotTableFieldElement' 
AND l.Type = N'SourceField'
AND le.RefPath LIKE N'SSAS%' 
AND le.Type IN (N'CD.DLS.Model.Mssql.Ssas.CalculatedMeasureElement', 
N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement',
N'CD.DLS.Model.Mssql.Ssas.DimensionAttributeElement')
--AND e.Definition NOT LIKE N'%(%'
AND e.ProjectConfigId = @projectConfigId

UNION ALL

------ measures
SELECT e.ProjectConfigId, SUBSTRING(e.RefPath, 0, CHARINDEX(N'/MdxStatement', e.RefPath)) [GroupPath], N'Measure' [FieldType],
e.RefPath, e.Definition FieldReference, le.ModelElementId FieldElementId, le.Caption FieldName 
FROM BIDoc.ModelElements e 
INNER JOIN BIDoc.ModelLinks l ON l.ElementFromId = e.ModelElementId
INNER JOIN BIDoc.ModelElements le ON le.ModelElementId = l.ElementToId
WHERE e.Type = N'CD.DLS.Model.Mssql.Ssas.MdxFragmentElement' 
AND l.Type = N'Reference'
AND e.RefPath LIKE N'SSRS%' 
AND le.RefPath LIKE N'SSAS%' 
AND le.Type IN (N'CD.DLS.Model.Mssql.Ssas.CalculatedMeasureElement', N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement')
AND e.Definition NOT LIKE N'%(%'
AND e.ProjectConfigId = @projectConfigId

UNION ALL

--------- direct attributes
SELECT e.ProjectConfigId, SUBSTRING(e.RefPath, 0, CHARINDEX(N'/MdxStatement', e.RefPath)) [GroupPath], N'Axis' [FieldType],
e.RefPath, /*e.Definition*/ N'[' + he.Caption + N'].[' + le.Caption + N']' FieldReference, 
le.ModelElementId FieldElementId, N'[' + he.Caption + N'].[' + le.Caption + N']' FieldName 
FROM BIDoc.ModelElements e 
INNER JOIN BIDoc.ModelLinks l ON l.ElementFromId = e.ModelElementId
INNER JOIN BIDoc.ModelElements le ON le.ModelElementId = l.ElementToId
INNER JOIN BIDoc.ModelLinks hl ON hl.ElementFromId = le.ModelElementId AND hl.Type = N'parent'
INNER JOIN BIDoc.ModelElements he ON he.ModelElementId = hl.ElementToId
--INNER JOIN BIDoc.ModelLinks dl ON dl.ElementFromId = he.ModelElementId AND dl.Type = N'parent'
--INNER JOIN BIDoc.ModelElements de ON de.ModelElementId = dl.ElementToId
WHERE e.Type = N'CD.DLS.Model.Mssql.Ssas.MdxFragmentElement' AND e.Definition NOT LIKE '%.&%'
AND l.Type = N'Reference'
AND e.RefPath LIKE N'SSRS%' 
AND le.RefPath LIKE N'SSAS%' 
AND le.Type IN (N'CD.DLS.Model.Mssql.Ssas.DimensionAttributeElement')
AND e.Definition NOT LIKE N'%(%'
AND e.ProjectConfigId = @projectConfigId

UNION ALL

-------- hierarchy levels
SELECT e.ProjectConfigId, SUBSTRING(e.RefPath, 0, CHARINDEX(N'/MdxStatement', e.RefPath)) [GroupPath], N'Axis' [FieldType],
e.RefPath, /*e.Definition*/ N'[' + he.Caption + N'].[' + ae.Caption + N']' FieldReference, 
ae.ModelElementId FieldElementId, N'[' + he.Caption + N'].[' + ae.Caption + N']' FieldName 
FROM BIDoc.ModelElements e 
INNER JOIN BIDoc.ModelLinks l ON l.ElementFromId = e.ModelElementId
INNER JOIN BIDoc.ModelElements le ON le.ModelElementId = l.ElementToId
INNER JOIN BIDoc.ModelLinks al ON al.ElementFromId = le.ModelElementId AND al.Type = N'Attribute'
INNER JOIN BIDoc.ModelElements ae ON ae.ModelElementId = al.ElementToId
INNER JOIN BIDoc.ModelLinks hl ON hl.ElementFromId = ae.ModelElementId AND hl.Type = N'parent'
INNER JOIN BIDoc.ModelElements he ON he.ModelElementId = hl.ElementToId
--INNER JOIN BIDoc.ModelLinks dl ON dl.ElementFromId = he.ModelElementId AND dl.Type = N'parent'
--INNER JOIN BIDoc.ModelElements de ON de.ModelElementId = dl.ElementToId
WHERE e.Type = N'CD.DLS.Model.Mssql.Ssas.MdxFragmentElement' AND e.Definition NOT LIKE '%.&%'
AND l.Type = N'Reference'
AND e.RefPath LIKE N'SSRS%' 
AND le.RefPath LIKE N'SSAS%' 
AND le.Type IN (N'CD.DLS.Model.Mssql.Ssas.HierarchyLevelElement')
AND e.Definition NOT LIKE N'%(%'
AND e.ProjectConfigId = @projectConfigId

UNION ALL

------- filters
SELECT e.ProjectConfigId, SUBSTRING(e.RefPath, 0, CHARINDEX(N'/MdxStatement', e.RefPath)) [GroupPath], N'Filter' [FieldType],
e.RefPath, e.Definition FieldReference, 
le.ModelElementId FieldElementId, le.Caption + N' ' + SUBSTRING(e.Definition, CHARINDEX(N'.&[', e.Definition) + 2, 1000) FieldName 
FROM BIDoc.ModelElements e 
INNER JOIN BIDoc.ModelLinks l ON l.ElementFromId = e.ModelElementId
INNER JOIN BIDoc.ModelElements le ON le.ModelElementId = l.ElementToId
WHERE e.Type = N'CD.DLS.Model.Mssql.Ssas.MdxFragmentElement' AND e.Definition LIKE '%.&%'
AND l.Type = N'Reference'
AND e.RefPath LIKE N'SSRS%' 
AND le.RefPath LIKE N'SSAS%' 
AND le.Type IN (N'CD.DLS.Model.Mssql.Ssas.DimensionAttributeElement', N'CD.DLS.Model.Mssql.Ssas.HierarchyLevelElement')
AND e.Definition NOT LIKE N'%(%'
AND e.ProjectConfigId = @projectConfigId
)
INSERT INTO Learning.OlapFieldReferences
(
ProjectConfigId, 
QueryElementId,
FieldElementId,
ReferenceElementId,
FieldType,
FieldReference,
FieldName, 
QueryElementRefPath,
ReferenceElementRefPath
)
SELECT 
x.ProjectConfigId, 
queryElement.ModelElementId QueryElementId,
FieldElementId,
referenceElement.ModelElementId ReferenceElementId,
x.FieldType,
x.FieldReference,
x.FieldName,
x.GroupPath QueryElementRefPath,
x.RefPath ReferenceElementRefPath
FROM x
INNER JOIN BIDoc.ModelElements queryElement ON queryElement.RefPath = x.GroupPath
INNER JOIN BIDoc.ModelElements referenceElement ON referenceElement.RefPath = x.RefPath
WHERE queryElement.ProjectConfigId = @projectConfigId AND referenceElement.ProjectConfigId = @projectConfigId
*/

;WITH x AS(
---------- pivot tables
SELECT e.ProjectConfigId, SUBSTRING(e.RefPath, 0, CHARINDEX(N'/Field[', e.RefPath)) [GroupPath], 
IIF(
JSON_VALUE(e.ExtendedProperties, '$.Orientation') IN(N'0', N'1'), N'Axis', 
IIF(JSON_VALUE(e.ExtendedProperties, '$.Orientation') = N'2', N'Measure', N'Filter')
) [FieldType],
e.RefPath, N'[' + IIF(JSON_VALUE(e.ExtendedProperties, '$.Orientation') = N'2', N'Measures', he.Caption) + N'].[' + le.Caption + N']' FieldReference, 
le.ModelElementId FieldElementId, IIF(JSON_VALUE(e.ExtendedProperties, '$.Orientation') = N'2', le.Caption, N'[' + he.Caption + N'].[' + le.Caption + N']') FieldName 
FROM BIDoc.ModelElements e 
INNER JOIN BIDoc.ModelLinks l ON l.ElementFromId = e.ModelElementId
INNER JOIN BIDoc.ModelElements le ON le.ModelElementId = l.ElementToId
INNER JOIN BIDoc.ModelLinks hl ON hl.ElementFromId = le.ModelElementId AND hl.Type = N'parent'
INNER JOIN BIDoc.ModelElements he ON he.ModelElementId = hl.ElementToId
WHERE e.Type = N'CD.DLS.Model.Business.Excel.PivotTableFieldElement' 
AND l.Type = N'SourceField'
AND le.RefPath LIKE N'SSAS%' 
AND le.Type IN (N'CD.DLS.Model.Mssql.Ssas.ReportCalculatedMeasureElement', N'CD.DLS.Model.Mssql.Ssas.CubeCalculatedMeasureElement', 
N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement',
N'CD.DLS.Model.Mssql.Ssas.CubeDimensionAttributeElement')
--AND e.Definition NOT LIKE N'%(%'
AND e.ProjectConfigId = @projectConfigId
-- for now, exclude pivot table template filters
AND (ISNULL(JSON_VALUE(e.ExtendedProperties, '$.Orientation'), N'') <> N'3')
UNION ALL

------ measures
SELECT e.ProjectConfigId, SUBSTRING(e.RefPath, 0, CHARINDEX(N'/MdxStatement', e.RefPath)) [GroupPath], N'Measure' [FieldType],
e.RefPath, e.Definition FieldReference, le.ModelElementId FieldElementId, le.Caption FieldName 
FROM BIDoc.ModelElements e 
INNER JOIN BIDoc.ModelLinks l ON l.ElementFromId = e.ModelElementId
INNER JOIN BIDoc.ModelElements le ON le.ModelElementId = l.ElementToId
WHERE e.Type = N'CD.DLS.Model.Mssql.Ssas.MdxFragmentElement' 
AND l.Type = N'Reference'
AND e.RefPath LIKE N'SSRS%' 
AND le.RefPath LIKE N'SSAS%' 
AND le.Type IN (N'CD.DLS.Model.Mssql.Ssas.ReportCalculatedMeasureElement', N'CD.DLS.Model.Mssql.Ssas.CubeCalculatedMeasureElement', N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement')
AND e.Definition NOT LIKE N'%(%'
AND e.ProjectConfigId = @projectConfigId

UNION ALL

--------- direct attributes
SELECT e.ProjectConfigId, SUBSTRING(e.RefPath, 0, CHARINDEX(N'/MdxStatement', e.RefPath)) [GroupPath], N'Axis' [FieldType],
e.RefPath, /*e.Definition*/ N'[' + he.Caption + N'].[' + le.Caption + N']' FieldReference, 
le.ModelElementId FieldElementId, N'[' + he.Caption + N'].[' + le.Caption + N']' FieldName 
FROM BIDoc.ModelElements e 
INNER JOIN BIDoc.ModelLinks l ON l.ElementFromId = e.ModelElementId
INNER JOIN BIDoc.ModelElements le ON le.ModelElementId = l.ElementToId
INNER JOIN BIDoc.ModelLinks hl ON hl.ElementFromId = le.ModelElementId AND hl.Type = N'parent'
INNER JOIN BIDoc.ModelElements he ON he.ModelElementId = hl.ElementToId
--INNER JOIN BIDoc.ModelLinks dl ON dl.ElementFromId = he.ModelElementId AND dl.Type = N'parent'
--INNER JOIN BIDoc.ModelElements de ON de.ModelElementId = dl.ElementToId
WHERE e.Type = N'CD.DLS.Model.Mssql.Ssas.MdxFragmentElement' AND e.Definition NOT LIKE '%.&%'
AND l.Type = N'Reference'
AND e.RefPath LIKE N'SSRS%' 
AND le.RefPath LIKE N'SSAS%' 
AND le.Type IN (N'CD.DLS.Model.Mssql.Ssas.CubeDimensionAttributeElement')
AND e.Definition NOT LIKE N'%(%'
AND e.ProjectConfigId = @projectConfigId

UNION ALL

-------- hierarchy levels
SELECT e.ProjectConfigId, SUBSTRING(e.RefPath, 0, CHARINDEX(N'/MdxStatement', e.RefPath)) [GroupPath], N'Axis' [FieldType],
e.RefPath, /*e.Definition*/ N'[' + he.Caption + N'].[' + ae.Caption + N']' FieldReference, 
ae.ModelElementId FieldElementId, N'[' + he.Caption + N'].[' + ae.Caption + N']' FieldName 
FROM BIDoc.ModelElements e 
INNER JOIN BIDoc.ModelLinks l ON l.ElementFromId = e.ModelElementId
INNER JOIN BIDoc.ModelElements le ON le.ModelElementId = l.ElementToId
INNER JOIN BIDoc.ModelLinks al ON al.ElementFromId = le.ModelElementId AND al.Type = N'Attribute'
INNER JOIN BIDoc.ModelElements ae ON ae.ModelElementId = al.ElementToId
INNER JOIN BIDoc.ModelLinks hl ON hl.ElementFromId = ae.ModelElementId AND hl.Type = N'parent'
INNER JOIN BIDoc.ModelElements he ON he.ModelElementId = hl.ElementToId
--INNER JOIN BIDoc.ModelLinks dl ON dl.ElementFromId = he.ModelElementId AND dl.Type = N'parent'
--INNER JOIN BIDoc.ModelElements de ON de.ModelElementId = dl.ElementToId
WHERE e.Type = N'CD.DLS.Model.Mssql.Ssas.MdxFragmentElement' AND e.Definition NOT LIKE '%.&%'
AND l.Type = N'Reference'
AND e.RefPath LIKE N'SSRS%' 
AND le.RefPath LIKE N'SSAS%' 
AND le.Type IN (N'CD.DLS.Model.Mssql.Ssas.CubeDimensionHierarchyLevelElement')
AND e.Definition NOT LIKE N'%(%'
AND e.ProjectConfigId = @projectConfigId

UNION ALL

------- filters
SELECT e.ProjectConfigId, SUBSTRING(e.RefPath, 0, CHARINDEX(N'/MdxStatement', e.RefPath)) [GroupPath], N'Filter' [FieldType],
e.RefPath, e.Definition FieldReference, 
le.ModelElementId FieldElementId, le.Caption + N' ' + SUBSTRING(e.Definition, CHARINDEX(N'.&[', e.Definition) + 2, 1000) FieldName 
FROM BIDoc.ModelElements e 
INNER JOIN BIDoc.ModelLinks l ON l.ElementFromId = e.ModelElementId
INNER JOIN BIDoc.ModelElements le ON le.ModelElementId = l.ElementToId
WHERE e.Type = N'CD.DLS.Model.Mssql.Ssas.MdxFragmentElement' AND e.Definition LIKE '%.&%'
AND l.Type = N'Reference'
AND e.RefPath LIKE N'SSRS%' 
AND le.RefPath LIKE N'SSAS%' 
AND le.Type IN (N'CD.DLS.Model.Mssql.Ssas.CubeDimensionAttributeElement', N'CD.DLS.Model.Mssql.Ssas.CubeDimensionHierarchyLevelElement')
AND e.Definition NOT LIKE N'%(%'
AND e.ProjectConfigId = @projectConfigId
AND e.Definition LIKE '%&%'
)
INSERT INTO Learning.OlapFieldReferences
(
ServerName,
DbName,
CubeName,
ProjectConfigId, 
QueryElementId,
FieldElementId,
ReferenceElementId,
FieldType,
FieldReference,
FieldName, 
QueryElementRefPath,
ReferenceElementRefPath
)
SELECT 
SUBSTRING(fe.RefPath, LEN(N'SSASServer[@Name=') + 2, CHARINDEX(N']/Db[@Name=', fe.RefPath) - LEN(N'SSASServer[@Name=') - 3) ServerName
,SUBSTRING(fe.RefPath, CHARINDEX(N']/Db[@Name=', fe.RefPath) + LEN(N']/Db[@Name=') + 1, CHARINDEX(N']/Cube[@Name=', fe.RefPath) - (CHARINDEX(N']/Db[@Name=', fe.RefPath) + LEN(N']/Db[@Name=') + 1) - 1) DbName
,SUBSTRING(fe.RefPath, CHARINDEX(N']/Cube[@Name=', fe.RefPath) + LEN(N']/Cube[@Name=') + 1, 
	CHARINDEX(N''']/', SUBSTRING(fe.RefPath, CHARINDEX(N']/Cube[@Name=', fe.RefPath) + LEN(N']/Cube[@Name=') + 1, 1000)) - 1) CubeName,
x.ProjectConfigId, 
queryElement.ModelElementId QueryElementId,
FieldElementId,
referenceElement.ModelElementId ReferenceElementId,
x.FieldType,
x.FieldReference,
x.FieldName,
x.GroupPath QueryElementRefPath,
x.RefPath ReferenceElementRefPath
FROM x
INNER JOIN BIDoc.ModelElements queryElement ON queryElement.RefPath = x.GroupPath
INNER JOIN BIDoc.ModelElements referenceElement ON referenceElement.RefPath = x.RefPath
INNER JOIN BIDoc.ModelElements fe ON fe.ModelElementId = x.FieldElementId
WHERE queryElement.ProjectConfigId = @projectConfigId AND referenceElement.ProjectConfigId = @projectConfigId



DELETE rp FROM Learning.OlapRulePremises rp
INNER JOIN Learning.OlapRules r ON rp.OlapRuleId = r.OlapRuleId WHERE r.ProjectConfigId = @projectConfigId

DELETE rc FROM Learning.OlapRuleConclusions rc
INNER JOIN Learning.OlapRules r ON rc.OlapRuleId = r.OlapRuleId WHERE r.ProjectConfigId = @projectConfigId

DELETE FROM [Learning].[OlapQueryFields] WHERE ProjectConfigId = @projectConfigId
DELETE FROM Learning.OlapFields WHERE ProjectConfigId = @projectConfigId

INSERT INTO Learning.OlapFields
(
ProjectConfigId,
FieldElementId,
FieldType,
FieldReference,
FieldName,
ServerName,
DbName,
CubeName
)
SELECT DISTINCT @projectConfigId ProjectConfigId, r.FieldElementId, r.FieldType, r.FieldReference, r.FieldName, 
ServerName, DbName, CubeName
FROM Learning.OlapFieldReferences r

INSERT INTO Learning.OlapQueryFields
(
ProjectConfigId,
QueryElementId,
OlapFieldId
)
SELECT DISTINCT
@projectConfigId,
r.QueryElementId,
f.OlapFieldId
FROM Learning.OlapFieldReferences r
INNER JOIN Learning.OlapFields f ON f.FieldElementId = r.FieldElementId AND f.FieldReference = r.FieldReference AND f.FieldType = r.FieldType
	AND f.ServerName = r.ServerName AND f.DbName = r.DbName AND f.CubeName = r.CubeName
WHERE r.ProjectConfigId = @projectConfigId AND f.ProjectConfigId = @projectConfigId
