CREATE TYPE [Analyst].[UDTT_ModelElementTypeAttributes] AS TABLE(
	[Name] NVARCHAR(255) NOT NULL,
	[ModelElementAttributeTypeCode] NVARCHAR(30) NOT NULL,
	[ExtendedProperties] NVARCHAR(MAX) NULL
)