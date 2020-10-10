CREATE PROCEDURE [Learning].[sp_SetOlapRules]
	@projectconfigid UNIQUEIDENTIFIER,
	@rules [Learning].[UDTT_OlapRules] READONLY
AS

DELETE p FROM Learning.OlapRulePremises p
INNER JOIN Learning.OlapRules r ON p.OlapRuleId = r.OlapRuleId
WHERE r.ProjectConfigId = @projectconfigid

DELETE c FROM Learning.OlapRuleConclusions c
INNER JOIN Learning.OlapRules r ON c.OlapRuleId = r.OlapRuleId
WHERE r.ProjectConfigId = @projectconfigid

DELETE FROM Learning.OlapRules WHERE ProjectConfigId = @projectconfigid

INSERT INTO Learning.OlapRules
(ProjectConfigId, RuleCode, Support, Confidence, ServerName, DbName, CubeName)
SELECT @projectconfigid, RuleCode, Support, Confidence, ServerName, DbName, CubeName FROM @rules

;WITH premises AS
(
SELECT r.RuleCode, CONVERT(INT, p.item) FieldId FROM @rules r
CROSS APPLY adm.f_splitstring(r.Premises, N';') p
)
INSERT INTO Learning.OlapRulePremises
(OlapRuleId, OlapFieldId)
SELECT r.OlapRuleId, premises.FieldId 
FROM premises
INNER JOIN Learning.OlapRules r ON r.RuleCode = premises.RuleCode
WHERE r.ProjectConfigId = @projectconfigid

;WITH conclusions AS
(
SELECT r.RuleCode, CONVERT(INT, p.item) FieldId FROM @rules r
CROSS APPLY adm.f_splitstring(r.Conclusions, N';') p
)
INSERT INTO Learning.OlapRuleConclusions
(OlapRuleId, OlapFieldId)
SELECT r.OlapRuleId, conclusions.FieldId 
FROM conclusions
INNER JOIN Learning.OlapRules r ON r.RuleCode = conclusions.RuleCode
WHERE r.ProjectConfigId = @projectconfigid

RETURN 0
