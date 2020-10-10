CREATE TABLE [Annotate].[AnnotationElementTypeDescriptions](
	[AnnotationElementTypeDescriptionsId] [int] NOT NULL IDENTITY(1,1),
	[ElementType] [nvarchar](255) NULL,
	[TypeDescription] [nvarchar](255) NULL,
 CONSTRAINT [PK_Annotate.AnnotationElementTypeDescriptions] PRIMARY KEY CLUSTERED 
(
	[AnnotationElementTypeDescriptionsId] ASC
)

)
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_AnnotationElementTypeDescriptions_ElementType ON Annotate.AnnotationElementTypeDescriptions(ElementType) INCLUDE([TypeDescription])