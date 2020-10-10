CREATE PROCEDURE [Learning].[sp_ListOlapFields]
	@projectconfigid UNIQUEIDENTIFIER
AS

SELECT f.OlapFieldId, f.FieldElementId, f.FieldName, f.FieldReference, f.FieldType,
f.ServerName, f.DbName, f.CubeName
FROM Learning.OlapFields f
WHERE ProjectConfigId = @projectconfigid
