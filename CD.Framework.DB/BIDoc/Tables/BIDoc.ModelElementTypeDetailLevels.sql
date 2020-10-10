CREATE TABLE [BIDoc].[ModelElementTypeDetailLevels](
	[ModelElementTypeDetailLevelId] [int] NOT NULL IDENTITY(1,1),
	[ElementType] [nvarchar](255) NULL,
	[DetailLevel] [int] NOT NULL,
 CONSTRAINT [PK_BIDoc.ModelElementTypeDetailLevels] PRIMARY KEY CLUSTERED 
(
	[ModelElementTypeDetailLevelId] ASC
)

)
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_ModelElementTypeDetailLevels_ElementType ON BIDoc.ModelElementTypeDetailLevels(ElementType) INCLUDE([DetailLevel])