CREATE TABLE [Analyst].[ModelElementAttributes](
	[ModelElementAttributeId] [int] NOT NULL IDENTITY(1,1),
	[RefPath] [nvarchar](max) NULL,
	[Value] [nvarchar](max) NULL,
	[ModelElementId] INT NOT NULL,
	[ModelElementTypeAttributeId] INT NOT NULL,
	[ObjectId]	INT	NOT NULL
 CONSTRAINT [PK_Analyst_ModelElementAttributes] PRIMARY KEY CLUSTERED 
(
	[ModelElementAttributeId] ASC
),
CONSTRAINT [FK_Analyst_ModelElementAttributess_ModelElementId] FOREIGN KEY ([ModelElementId]) REFERENCES [Analyst].[ModelElements]([ModelElementId]),
CONSTRAINT [FK_Analyst_ModelElementAttributes_ModelElementTypeAttributeId] FOREIGN KEY ([ModelElementTypeAttributeId]) REFERENCES [Analyst].[ModelElementTypeAttributes]([ModelElementTypeAttributeId]),
CONSTRAINT [FK_Analyst_ModelElementAttributes_ObjectId] FOREIGN KEY ([ObjectId]) REFERENCES [Analyst].[Objects]([ObjectId]),
)
