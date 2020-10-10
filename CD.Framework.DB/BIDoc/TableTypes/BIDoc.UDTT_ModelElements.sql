CREATE TYPE [BIDoc].[UDTT_ModelElements] AS TABLE(
	[ModelElementId] [int] NULL,
	[ExtendedProperties] [nvarchar](max) NULL,
	[RefPath] [nvarchar](max) NULL,
	[Definition] [nvarchar](max) NULL,
	[Caption] [nvarchar](max) NULL,
	[Type] [nvarchar](255) NULL,
	[RefPathSuffix] NVARCHAR(300) NULL
)