CREATE TABLE [Annotate].[AnnotationElementDescriptivePaths](
	[AnnotationElementId] [int] NOT NULL,
	[DescriptivePath] [nvarchar](max) NULL,
	[DescriptiveRootPath] [nvarchar](max) NULL,
	
 CONSTRAINT [PK_Annotate_AnnotationElementDescriptivePaths] PRIMARY KEY CLUSTERED 
	(
	[AnnotationElementId] ASC
	)
)
GO