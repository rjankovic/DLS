CREATE TABLE [Learning].[OlapRules]
(
	[OlapRuleId] INT NOT NULL IDENTITY(1,1),
	[ProjectConfigId] UNIQUEIDENTIFIER NOT NULL,
	[RuleCode] NVARCHAR(30) NOT NULL,
	[Confidence] REAL NOT NULL,
	[Support] REAL NOT NULL,
	[ServerName] NVARCHAR(MAX),
	[DbName] NVARCHAR(MAX),
	[CubeName] NVARCHAR(MAX),
	CONSTRAINT PK_Learning_OlapRules PRIMARY KEY([OlapRuleId])
)
