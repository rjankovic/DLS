CREATE TABLE [Learning].[OlapRuleConclusions]
(
	[OlapRuleConclusionId] INT NOT NULL IDENTITY(1,1),
	[OlapRuleId] INT FOREIGN KEY REFERENCES [Learning].[OlapRules]([OlapRuleId]),
	[OlapFieldId] INT FOREIGN KEY REFERENCES [Learning].[OlapFields]([OlapFieldId]),
	CONSTRAINT PK_Learning_OlapRuleConclusions PRIMARY KEY([OlapRuleConclusionId])
)
