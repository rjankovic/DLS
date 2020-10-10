CREATE TYPE [Learning].[UDTT_OlapRules] AS TABLE(
	[RuleCode] NVARCHAR(30),
	[Support] REAL,
	[Confidence] REAL,
	-- ID1;ID2;ID3
	[Premises] NVARCHAR(MAX),
	[Conclusions] NVARCHAR(MAX),
	[ServerName] NVARCHAR(MAX),
	[DbName] NVARCHAR(MAX),
	[CubeName] NVARCHAR(MAX)
)
