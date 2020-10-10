CREATE PROCEDURE [Learning].[sp_GetOlapRuleConclusions]
	@projectconfigid UNIQUEIDENTIFIER
AS

SELECT c.OlapRuleId, c.OlapFieldId
FROM Learning.OlapRules r
INNER JOIN Learning.OlapRuleConclusions c ON r.OlapRuleId = c.OlapRuleId
WHERE r.ProjectConfigId = @projectconfigid
