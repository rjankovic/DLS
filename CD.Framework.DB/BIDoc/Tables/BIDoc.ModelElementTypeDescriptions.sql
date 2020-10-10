CREATE TABLE [BIDoc].[ModelElementTypeDescriptions](
	[ModelElementTypeDescriptionsId] [int] NOT NULL IDENTITY(1,1),
	[ElementType] [nvarchar](255) NULL,
	[TypeDescription] [nvarchar](255) NULL,
 CONSTRAINT [PK_BIDoc.ModelElementTypeDescriptions] PRIMARY KEY CLUSTERED 
(
	[ModelElementTypeDescriptionsId] ASC
)

)
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_ModelElementTypeDescriptions_ElementType ON BIDoc.ModelElementTypeDescriptions(ElementType) INCLUDE([TypeDescription])