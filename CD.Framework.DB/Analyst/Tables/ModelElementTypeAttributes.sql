CREATE TABLE [Analyst].[ModelElementTypeAttributes](
	[ModelElementTypeAttributeId] INT NOT NULL IDENTITY(1,1),
	[Name] NVARCHAR(255) NOT NULL,
	[ModelElementTypeId] [int] NOT NULL,
	[ModelElementAttributeTypeId] [int] NOT NULL,
	[ExtendedProperties] NVARCHAR(MAX) NULL,	-- enumerations and links (target type & tree root)
 CONSTRAINT [PK_Analyst_ModelElementTypeAttributes] PRIMARY KEY CLUSTERED 
(
	[ModelElementTypeAttributeId] ASC
),
CONSTRAINT [FK_Analyst_ModelElementTypeAttributes_ModelElementTypeId] FOREIGN KEY ([ModelElementTypeId]) REFERENCES [Analyst].[ModelElementTypes]([ModelElementTypeId]),
CONSTRAINT [FK_Analyst_ModelElementTypeAttributes_ModelElementAttributeTypeId] FOREIGN KEY ([ModelElementAttributeTypeId]) REFERENCES [Analyst].[ModelElementAttributeTypes]([ModelElementAttributeTypeId]),
)
