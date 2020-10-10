CREATE PROCEDURE [Learning].[sp_GetOlapRules]
	@projectconfigid UNIQUEIDENTIFIER
AS

SELECT r.OlapRuleId, r.RuleCode, r.Support, r.Confidence, r.ServerName, r.DbName, r.CubeName
FROM Learning.OlapRules r
WHERE r.ProjectConfigId = @projectconfigid
