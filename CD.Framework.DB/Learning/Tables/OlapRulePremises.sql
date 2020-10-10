CREATE TABLE [Learning].[OlapRulePremises]
(
	[OlapRulePremiseId] INT NOT NULL IDENTITY(1,1),
	[OlapRuleId] INT FOREIGN KEY REFERENCES [Learning].[OlapRules]([OlapRuleId]),
	[OlapFieldId] INT FOREIGN KEY REFERENCES [Learning].[OlapFields]([OlapFieldId]),
	CONSTRAINT PK_Learning_OlapRulePremises PRIMARY KEY([OlapRulePremiseId])
)
