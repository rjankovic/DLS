CREATE PROCEDURE [Learning].[sp_GetOlapQueryFields]
	@projectconfigid UNIQUEIDENTIFIER
AS

SELECT OlapQueryFieldId, QueryElementId, OlapFieldId 
FROM Learning.OlapQueryFields
WHERE ProjectConfigId = @projectconfigid
