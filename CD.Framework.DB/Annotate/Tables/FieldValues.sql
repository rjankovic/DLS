CREATE TABLE [Annotate].[FieldValues]
(
	[FieldValueId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[FieldId] INT NOT NULL FOREIGN KEY REFERENCES [Annotate].[Fields]([FieldId]),
	[AnnotationElementId] INT NOT NULL FOREIGN KEY REFERENCES [Annotate].[AnnotationElements]([AnnotationElementId]),
	[Value] NVARCHAR(MAX) NULL,
	[UpdatedVersion] INT NOT NULL
)
