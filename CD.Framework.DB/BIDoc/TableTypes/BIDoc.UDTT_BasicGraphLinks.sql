CREATE TYPE [BIDoc].[UDTT_BasicGraphLinks] AS TABLE(
	[BasicGraphLinkId] [int] NOT NULL,
	[LinkType] NVARCHAR(50) NOT NULL,
	[NodeFromId] [int] NOT NULL,
	[NodeToId] [int] NOT NULL
)