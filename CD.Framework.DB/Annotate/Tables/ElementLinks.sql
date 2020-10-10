CREATE TABLE [Annotate].[ElementLinks]
(
	[ElementLinkId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[LinkTypeId] INT NOT NULL FOREIGN KEY REFERENCES [Annotate].[LinkTypes]([LinkTypeId]),
	[AnnotationElementFromId] INT NOT NULL FOREIGN KEY REFERENCES [Annotate].[AnnotationElements]([AnnotationElementId]),
	[AnnotationElementToId] INT NOT NULL FOREIGN KEY REFERENCES [Annotate].[AnnotationElements]([AnnotationElementId]),
	[UpdatedVersion] INT NOT NULL
)
