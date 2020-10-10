CREATE TABLE [BIDoc].[ModelElementDescriptivePaths](
	[ModelElementId] [int] NOT NULL,
	[DescriptivePath] [nvarchar](max) NULL,
	[DescriptiveRootPath] [nvarchar](max) NULL,
	
 CONSTRAINT [PK_BIDoc_ModelElementDescriptivePaths] PRIMARY KEY CLUSTERED 
	(
	[ModelElementId] ASC
	)
)
GO