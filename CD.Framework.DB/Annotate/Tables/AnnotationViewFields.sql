CREATE TABLE [Annotate].[AnnotationViewFields]
(
	[AnnotationViewFieldId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[AnnotationViewId] INT NOT NULL FOREIGN KEY REFERENCES [Annotate].[AnnotationViews]([AnnotationViewId]),
	[FieldId] INT NOT NULL FOREIGN KEY REFERENCES [Annotate].[Fields]([FieldId]),
	[FieldOrder] INT NOT NULL CONSTRAINT DF_AnnotationViewFields_FieldOrder DEFAULT 0
)
