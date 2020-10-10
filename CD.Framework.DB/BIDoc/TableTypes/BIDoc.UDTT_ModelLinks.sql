CREATE TYPE [BIDoc].[UDTT_ModelLinks] AS TABLE(
	[ModelLinkId] [int] NULL,
	[ElementFromId] [int] NOT NULL,
	[ElementToId] [int] NOT NULL,
	[Type] [nvarchar](255) NULL,
	[ExtendedProperties] [nvarchar](max) NULL
)