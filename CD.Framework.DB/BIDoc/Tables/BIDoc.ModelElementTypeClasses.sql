CREATE TABLE [BIDoc].[ModelElementTypeClasses](
	[ModelElementTypeClassId] [int] NOT NULL IDENTITY(1,1),
	[ElementType] [nvarchar](255) NULL,
	[ClassCode] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_BIDoc.ModelElementTypeClasses] PRIMARY KEY CLUSTERED 
(
	[ModelElementTypeClassId] ASC
)

)
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_ModelElementTypeClasses_ElementType_ClassCode ON BIDoc.ModelElementTypeClasses(ElementType) INCLUDE ([ClassCode])