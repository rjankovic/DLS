CREATE TYPE [Inspect].[UDTT_sp_GetDataFlowBetweenGroups] AS TABLE(
	[SourceNodeId] [int] NOT NULL,
	[SourceNodeName] [nvarchar](max) NULL,
	[SourceNodePath] [nvarchar](max) NULL,
	[SourceElementId] [int] NOT NULL,
	[TargetNodeId] [int] NOT NULL,
	[TargetNodeName] [nvarchar](max) NULL,
	[TargetNodePath] [nvarchar](max) NULL,
	[TargetElementId] [int] NOT NULL
)