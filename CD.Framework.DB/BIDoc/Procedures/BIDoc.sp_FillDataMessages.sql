CREATE PROCEDURE [BIDoc].[sp_FillDataMessages]
		--DECLARE
	@projectconfigid UNIQUEIDENTIFIER --= N'FD1312FC-1182-4B9C-82D9-2089F3468BFB'
AS

DELETE em FROM [BIDoc].[DataMessages] em
INNER JOIN BIDoc.ModelElements me ON em.SourceElementId = me.ModelElementId
WHERE me.ProjectConfigId = @projectconfigid

-- Prepaire firts table without spliting json value

--OLEDB Source
SELECT 
outColElem.ModelElementId AS 'SourceElementId',
tabColElem.ModelElementId AS 'TargetElementId',
outColElem.ExtendedProperties AS 'SourcePath', 
tabColElem.ExtendedProperties AS 'TargetPath'
INTO #DataMessageFirstStep
FROM BIDoc.ModelElements e 
INNER JOIN BIDoc.BasicGraphNodes en ON en.SourceElementId = e.ModelElementId
INNER JOIN BIDoc.BasicGraphNodes outNode ON outNode.ParentId = en.BasicGraphNodeId
INNER JOIN BIDoc.BasicGraphNodes outColNode ON outColNode.ParentId = outNode.BasicGraphNodeId 
INNER JOIN BIDoc.ModelElements outColElem ON outColElem.ModelElementId = outColNode.SourceElementId
INNER JOIN BIDoc.BasicGraphLinks l ON l.NodeToId = outColNode.BasicGraphNodeId
INNER JOIN BIDoc.BasicGraphNodes tabColNode ON tabColNode.BasicGraphNodeId = l.NodeFromId
INNER JOIN BIDoc.ModelElements tabColElem ON tabColElem.ModelElementId = tabColNode.SourceElementId
WHERE e.Type = 'CD.DLS.Model.Mssql.Ssis.DfSourceElement'
AND e.ProjectConfigId =  @projectconfigid
AND en.GraphKind = N'DataFlow'
AND tabColElem.Type = N'CD.DLS.Model.Mssql.Db.ColumnElement'

--OLEDB Destination
INSERT INTO #DataMessageFirstStep 
(SourceElementId,
TargetElementId,
SourcePath, 
TargetPath)
SELECT  
se.ModelElementId AS 'SourceElementId',
te.ModelElementId AS 'TargetElementId',
se.ExtendedProperties AS 'SourcePath',
te.ExtendedProperties  AS 'TargetPath'
FROM BIDoc.ModelLinks l 
INNER JOIN BIDoc.ModelElements se ON l.ElementFromId = se.ModelElementId
INNER JOIN BIDoc.ModelElements te ON l.ElementToId = te.ModelElementId
WHERE l.Type = N'ExternalDestinationColumn'
AND se.ProjectConfigId = @projectconfigid AND te.ProjectConfigId = @projectconfigid

-- Prepaire Second step spliting json value
SELECT 
	[SourceElementId],
	[TargetElementId],
	adm.f_SimpleJsonValue(SourcePath, 'DtsDataType') AS 'SourceDtsDataType',
	adm.f_SimpleJsonValue(SourcePath, 'Length') AS 'SourceLength',
	adm.f_SimpleJsonValue(SourcePath, 'Precision') AS 'SourcePrecision',
	adm.f_SimpleJsonValue(SourcePath, 'Scale') AS 'SourceScale',
	adm.f_SimpleJsonValue(TargetPath, 'SqlDataType') AS 'TargetSqlDataType',
	adm.f_SimpleJsonValue(TargetPath, 'Length') AS 'TargetLength',
	adm.f_SimpleJsonValue(TargetPath, 'Precision') AS 'TargetPrecision',
	adm.f_SimpleJsonValue(TargetPath, 'Scale') AS 'TargetScale'
INTO #DataMessageSecondStep
FROM #DataMessageFirstStep

DROP TABLE #DataMessageFirstStep

--Prepaire Third step
SELECT *
INTO #DataMessageThirdStep
FROM  #DataMessageSecondStep dmss
INNER JOIN BIDoc.SourceDataTypes sdt ON SourceDtsDataType = sdt.SourceDataTypeName
INNER JOIN BIDoc.DataTypes dt ON sdt.DataTypeId = dt.DataTypesId

DROP TABLE #DataMessageSecondStep

--Last Step fill DataMEssages
--INT

INSERT INTO [BIDoc].[DataMessages]
(
	[SourceElementId],
	[TargetElementId],
	[Message],
	[DataMessagesTypeId]
)
SELECT 
SourceElementId,
TargetElementId,
'Source Data Type (Length): '+ SourceDtsDataType + '('+
SourceLength +') Target Data Type (Length): '+
TargetSqlDataType +'('+ TargetLength +')' AS 'Message',
(SELECT DataMessagesTypeId FROM BIDoc.DataMessagesType WHERE DataMessageCode='Truncat') AS 'DataMessagesTypeId'
FROM  #DataMessageThirdStep dmts
WHERE dmts.DataTypeName IN (N'String', N'Binary',N'Text',N'Integer',N'Floating-point number') 
AND CAST(SourceLength AS INT)  > CAST(TargetLength AS INT) 
AND CAST(TargetLength AS INT) >= 0
AND NOT dmts.SourceDataTypeName = N'DT_GUID'
AND ('Source Data Type (Length): '+ SourceDtsDataType + '('+
SourceLength +') Target Data Type (Length): '+
TargetSqlDataType +'('+ TargetLength +')') IS NOT NULL

--BOOL
INSERT INTO [BIDoc].[DataMessages]
(
	[SourceElementId],
	[TargetElementId],
	[Message],
	[DataMessagesTypeId]
)
SELECT 
SourceElementId,
TargetElementId,
'Source Data Type (Length): '+ SourceDtsDataType + '('+
SourceLength +') Target Data Type (Length): '+
TargetSqlDataType +'('+ TargetLength +')' AS 'Message',
(SELECT DataMessagesTypeId FROM BIDoc.DataMessagesType WHERE DataMessageCode='Truncat') AS 'DataMessagesTypeId'
FROM #DataMessageThirdStep dmts
WHERE dmts.DataTypeName IN (N'Boolean')
AND NOT CAST(SourceLength AS INT) = CAST(TargetLength AS INT)
AND ('Source Data Type (Length): '+ SourceDtsDataType + '('+
SourceLength +') Target Data Type (Length): '+
TargetSqlDataType +'('+ TargetLength +')') IS NOT NULL

--Numeric, Decimal
INSERT INTO [BIDoc].[DataMessages]
(
	[SourceElementId],
	[TargetElementId],
	[Message],
	[DataMessagesTypeId]
)
SELECT 
SourceElementId,
TargetElementId,
'Source Data Type (Scale, Precision): '+ SourceDtsDataType + '('+
SourceScale +' ,'+ SourcePrecision 
+') Target Data Type (Scale, Precisionh): '+
TargetSqlDataType +'('+ TargetScale +' ,'+ 
TargetPrecision +')' AS 'Message',
(SELECT DataMessagesTypeId FROM BIDoc.DataMessagesType WHERE DataMessageCode='Truncat') AS 'DataMessagesTypeId'
FROM #DataMessageThirdStep dmts
WHERE dmts.DataTypeName IN (N'Fixed-point number')
AND ((CAST(SourceScale AS INT)  > CAST(TargetScale AS INT) 
AND CAST(SourcePrecision AS INT)  > CAST(TargetPrecision AS INT))
OR (CAST(SourceScale AS INT)  > CAST(TargetScale AS INT) 
OR CAST(SourcePrecision AS INT)  > CAST(TargetPrecision AS INT)))
AND  ('Source Data Type (Scale, Precision): '+ SourceDtsDataType + '('+
SourceScale +' ,'+ SourcePrecision 
+') Target Data Type (Scale, Precisionh): '+
TargetSqlDataType +'('+ TargetScale +' ,'+ 
TargetPrecision +')') IS NOT NULL

DROP TABLE #DataMessageThirdStep

/*INSERT INTO [BIDoc].[DataMessages]
(
	[SourceElementId],
	[TargetElementId],
	[Message],
	[DataMessagesTypeId]
)
SELECT 
SourceElementId,
TargetElementId,
'Source Data Type (Length): '+ SourceDtsDataType + '('+
SourceLength +') Target Data Type (Length): '+
TargetSqlDataType +'('+ TargetLength +')' AS 'Message',
(SELECT DataMessagesTypeId FROM BIDoc.DataMessagesType WHERE DataMessageCode='Truncat') AS 'DataMessagesTypeId'
FROM  #DataMessageSecondStep dmss
INNER JOIN BIDoc.SourceDataTypes sdt ON SourceDtsDataType = sdt.SourceDataTypeName 
INNER JOIN BIDoc.DataTypes dt ON sdt.DataTypeId = dt.DataTypesId
WHERE dt.DataTypeName IN (N'String', N'Binary',N'Text',N'Integer',N'Floating-point number') 
AND CAST(SourceLength AS INT)  > CAST(TargetLength AS INT) 
AND CAST(TargetLength AS INT) >= 0
AND NOT sdt.SourceDataTypeName = N'DT_GUID'

--BOOL
INSERT INTO [BIDoc].[DataMessages]
(
	[SourceElementId],
	[TargetElementId],
	[Message],
	[DataMessagesTypeId]
)
SELECT 
SourceElementId,
TargetElementId,
'Source Data Type (Length): '+ SourceDtsDataType + '('+
SourceLength +') Target Data Type (Length): '+
TargetSqlDataType +'('+ TargetLength +')' AS 'Message',
(SELECT DataMessagesTypeId FROM BIDoc.DataMessagesType WHERE DataMessageCode='Truncat') AS 'DataMessagesTypeId'
FROM #DataMessageSecondStep dmss
INNER JOIN BIDoc.SourceDataTypes sdt ON SourceDtsDataType = sdt.SourceDataTypeName 
INNER JOIN BIDoc.DataTypes dt ON sdt.DataTypeId = dt.DataTypesId
WHERE dt.DataTypeName IN (N'Boolean')
AND NOT CAST(SourceLength AS INT) = CAST(TargetLength AS INT)

--Numeric, Decimal
INSERT INTO [BIDoc].[DataMessages]
(
	[SourceElementId],
	[TargetElementId],
	[Message],
	[DataMessagesTypeId]
)
SELECT 
SourceElementId,
TargetElementId,
'Source Data Type (Scale, Precision): '+ SourceDtsDataType + '('+
SourceScale +' ,'+ SourcePrecision 
+') Target Data Type (Scale, Precisionh): '+
TargetSqlDataType +'('+ TargetScale +' ,'+ 
TargetScale +')' AS 'Message',
(SELECT DataMessagesTypeId FROM BIDoc.DataMessagesType WHERE DataMessageCode='Truncat') AS 'DataMessagesTypeId'
FROM #DataMessageSecondStep dmss
INNER JOIN BIDoc.SourceDataTypes sdt ON SourceDtsDataType = sdt.SourceDataTypeName
INNER JOIN BIDoc.DataTypes dt ON sdt.DataTypeId = dt.DataTypesId
WHERE dt.DataTypeName IN (N'Fixed-point number')
AND ((CAST(SourceScale AS INT)  > CAST(TargetScale AS INT) 
AND CAST(SourcePrecision AS INT)  > CAST(TargetPrecision AS INT))
OR (CAST(SourceScale AS INT)  > CAST(TargetScale AS INT) 
OR CAST(SourcePrecision AS INT)  > CAST(TargetPrecision AS INT)))

DROP TABLE #DataMessageSecondStep


INSERT INTO [BIDoc].[DataMessages]
(
	[SourceElementId],
	[TargetElementId],
	[Message],
	[DataMessagesTypeId]
)
SELECT 
outColElem.ModelElementId AS 'SourceElementId',
tabColElem.ModelElementId AS 'TargetElementId',
'Source Data Type (Length): '+ adm.f_SimpleJsonValue(outColElem.ExtendedProperties, 'DtsDataType') + '('+
CAST(adm.f_SimpleJsonValue(outColElem.ExtendedProperties, 'Length')AS NVARCHAR) +') Target Data Type (Length): '+
adm.f_SimpleJsonValue(tabColElem.ExtendedProperties, 'SqlDataType') +'('+ CAST(adm.f_SimpleJsonValue(tabColElem.ExtendedProperties, 'Length')AS NVARCHAR) +')' AS 'Message',
(SELECT DataMessagesTypeId FROM BIDoc.DataMessagesType WHERE DataMessageCode='Truncat') AS 'DataMessagesTypeId'
FROM BIDoc.ModelElements e 
INNER JOIN BIDoc.BasicGraphNodes en ON en.SourceElementId = e.ModelElementId
INNER JOIN BIDoc.BasicGraphNodes outNode ON outNode.ParentId = en.BasicGraphNodeId
INNER JOIN BIDoc.BasicGraphNodes outColNode ON outColNode.ParentId = outNode.BasicGraphNodeId 
INNER JOIN BIDoc.ModelElements outColElem ON outColElem.ModelElementId = outColNode.SourceElementId
INNER JOIN BIDoc.BasicGraphLinks l ON l.NodeToId = outColNode.BasicGraphNodeId
INNER JOIN BIDoc.BasicGraphNodes tabColNode ON tabColNode.BasicGraphNodeId = l.NodeFromId
INNER JOIN BIDoc.ModelElements tabColElem ON tabColElem.ModelElementId = tabColNode.SourceElementId
INNER JOIN BIDoc.SourceDataTypes sdt ON adm.f_SimpleJsonValue(outColElem.ExtendedProperties, 'DtsDataType') = sdt.SourceDataTypeName 
INNER JOIN BIDoc.DataTypes dt ON sdt.DataTypeId = dt.DataTypesId
WHERE e.Type = 'CD.DLS.Model.Mssql.Ssis.DfSourceElement'
AND e.ProjectConfigId =  @projectconfigid
AND en.GraphKind = N'DataFlow'
AND tabColElem.Type = N'CD.DLS.Model.Mssql.Db.ColumnElement'
AND dt.DataTypeName IN (N'String', N'Binary',N'Text',N'Integer',N'Floating-point number') 
AND CAST(adm.f_SimpleJsonValue(outColElem.ExtendedProperties, 'Length') AS INT)  > CAST(adm.f_SimpleJsonValue(tabColElem.ExtendedProperties, 'Length')AS INT) AND CAST(adm.f_SimpleJsonValue(tabColElem.ExtendedProperties, 'Length')AS INT) >= 0
AND NOT sdt.SourceDataTypeName = N'DT_GUID'

--BOOL
INSERT INTO [BIDoc].[DataMessages]
(
	[SourceElementId],
	[TargetElementId],
	[Message],
	[DataMessagesTypeId]
)
SELECT outColElem.ModelElementId AS 'SourceElementId', tabColElem.ModelElementId AS 'TargetElementId',
'Source Data Type (Length): '+ adm.f_SimpleJsonValue(outColElem.ExtendedProperties, 'DtsDataType') + '('+
CAST(adm.f_SimpleJsonValue(outColElem.ExtendedProperties, 'Length')AS NVARCHAR) +') Target Data Type (Length): '+
adm.f_SimpleJsonValue(tabColElem.ExtendedProperties, 'SqlDataType') +'('+ CAST(adm.f_SimpleJsonValue(tabColElem.ExtendedProperties, 'Length')AS NVARCHAR) +')' AS 'Message',
(SELECT DataMessagesTypeId FROM BIDoc.DataMessagesType WHERE DataMessageCode='Truncat') AS 'DataMessagesTypeId'
FROM BIDoc.ModelElements e 
INNER JOIN BIDoc.BasicGraphNodes en ON en.SourceElementId = e.ModelElementId
INNER JOIN BIDoc.BasicGraphNodes outNode ON outNode.ParentId = en.BasicGraphNodeId
INNER JOIN BIDoc.BasicGraphNodes outColNode ON outColNode.ParentId = outNode.BasicGraphNodeId 
INNER JOIN BIDoc.ModelElements outColElem ON outColElem.ModelElementId = outColNode.SourceElementId
INNER JOIN BIDoc.BasicGraphLinks l ON l.NodeToId = outColNode.BasicGraphNodeId
INNER JOIN BIDoc.BasicGraphNodes tabColNode ON tabColNode.BasicGraphNodeId = l.NodeFromId
INNER JOIN BIDoc.ModelElements tabColElem ON tabColElem.ModelElementId = tabColNode.SourceElementId
INNER JOIN BIDoc.SourceDataTypes sdt ON adm.f_SimpleJsonValue(outColElem.ExtendedProperties, 'DtsDataType') = sdt.SourceDataTypeName
INNER JOIN BIDoc.DataTypes dt ON sdt.DataTypeId = dt.DataTypesId
WHERE e.Type = 'CD.DLS.Model.Mssql.Ssis.DfSourceElement'
AND e.ProjectConfigId =  @projectconfigid
AND en.GraphKind = N'DataFlow'
AND tabColElem.Type = N'CD.DLS.Model.Mssql.Db.ColumnElement'
AND dt.DataTypeName IN (N'Boolean')
AND NOT CAST(adm.f_SimpleJsonValue(outColElem.ExtendedProperties, 'Length') AS INT) = CAST(adm.f_SimpleJsonValue(tabColElem.ExtendedProperties, 'Length')AS INT)

--Numeric, Decimal
INSERT INTO [BIDoc].[DataMessages]
(
	[SourceElementId],
	[TargetElementId],
	[Message],
	[DataMessagesTypeId]
)
SELECT outColElem.ModelElementId AS 'SourceElementId', tabColElem.ModelElementId AS 'TargetElementId',
'Source Data Type (Scale, Precision): '+ adm.f_SimpleJsonValue(outColElem.ExtendedProperties, 'DtsDataType') + '('+
CAST(adm.f_SimpleJsonValue(outColElem.ExtendedProperties, 'Scale')AS NVARCHAR)+' ,'+CAST(adm.f_SimpleJsonValue(outColElem.ExtendedProperties, 'Precision')AS NVARCHAR) 
+') Target Data Type (Scale, Precisionh): '+
adm.f_SimpleJsonValue(tabColElem.ExtendedProperties, 'SqlDataType') +'('+ CAST(adm.f_SimpleJsonValue(tabColElem.ExtendedProperties, 'Scale')AS NVARCHAR) +' ,'+ 
CAST(adm.f_SimpleJsonValue(tabColElem.ExtendedProperties, 'Scale')AS NVARCHAR) +')' AS 'Message',
(SELECT DataMessagesTypeId FROM BIDoc.DataMessagesType WHERE DataMessageCode='Truncat') AS 'DataMessagesTypeId'
FROM BIDoc.ModelElements e 
INNER JOIN BIDoc.BasicGraphNodes en ON en.SourceElementId = e.ModelElementId
INNER JOIN BIDoc.BasicGraphNodes outNode ON outNode.ParentId = en.BasicGraphNodeId
INNER JOIN BIDoc.BasicGraphNodes outColNode ON outColNode.ParentId = outNode.BasicGraphNodeId 
INNER JOIN BIDoc.ModelElements outColElem ON outColElem.ModelElementId = outColNode.SourceElementId
INNER JOIN BIDoc.BasicGraphLinks l ON l.NodeToId = outColNode.BasicGraphNodeId
INNER JOIN BIDoc.BasicGraphNodes tabColNode ON tabColNode.BasicGraphNodeId = l.NodeFromId
INNER JOIN BIDoc.ModelElements tabColElem ON tabColElem.ModelElementId = tabColNode.SourceElementId
INNER JOIN BIDoc.SourceDataTypes sdt ON adm.f_SimpleJsonValue(outColElem.ExtendedProperties, 'DtsDataType') = sdt.SourceDataTypeName
INNER JOIN BIDoc.DataTypes dt ON sdt.DataTypeId = dt.DataTypesId
WHERE e.Type = 'CD.DLS.Model.Mssql.Ssis.DfSourceElement'
AND e.ProjectConfigId =  @projectconfigid
AND en.GraphKind = N'DataFlow'
AND tabColElem.Type = N'CD.DLS.Model.Mssql.Db.ColumnElement'
AND dt.DataTypeName IN (N'Fixed-point number')
AND ((CAST(adm.f_SimpleJsonValue(outColElem.ExtendedProperties, 'Scale') AS INT)  > CAST(adm.f_SimpleJsonValue(tabColElem.ExtendedProperties, 'Scale')AS INT) 
AND CAST(adm.f_SimpleJsonValue(outColElem.ExtendedProperties, 'Precision') AS INT)  > CAST(adm.f_SimpleJsonValue(tabColElem.ExtendedProperties, 'Precision')AS INT))
OR (CAST(adm.f_SimpleJsonValue(outColElem.ExtendedProperties, 'Scale') AS INT)  > CAST(adm.f_SimpleJsonValue(tabColElem.ExtendedProperties, 'Scale')AS INT) 
OR CAST(adm.f_SimpleJsonValue(outColElem.ExtendedProperties, 'Precision') AS INT)  > CAST(adm.f_SimpleJsonValue(tabColElem.ExtendedProperties, 'Precision')AS INT)))
*/