CREATE TYPE [BIDoc].[UDTT_BasicGraphNodes] AS TABLE(
	[BasicGraphNodeId] [int] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[NodeType] [nvarchar](200) NULL,
	[Description] [nvarchar](max) NULL,
	[ParentId] [int] NULL,
	[SourceElementId] INT NOT NULL,
	[TopologicalOrder] [int] NULL
)