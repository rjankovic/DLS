CREATE PROCEDURE [Learning].[sp_GetOlapRulePremises]
	@projectconfigid UNIQUEIDENTIFIER
AS

SELECT p.OlapRuleId, p.OlapFieldId
FROM Learning.OlapRules r
INNER JOIN Learning.OlapRulePremises p ON r.OlapRuleId = p.OlapRuleId
WHERE r.ProjectConfigId = @projectconfigid
