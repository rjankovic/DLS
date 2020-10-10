CREATE TABLE [Learning].[OlapFieldReferences]
(
	[OlapFieldReferenceId] INT NOT NULL IDENTITY(1,1),
	[ProjectConfigId] UNIQUEIDENTIFIER,
	[QueryElementId] INT, --FOREIGN KEY REFERENCES [BIDoc].[ModelElements]([ModelElementId]),
	[FieldElementId] INT, --FOREIGN KEY REFERENCES [BIDoc].[ModelElements]([ModelElementId]),
	[ReferenceElementId] INT, --FOREIGN KEY REFERENCES [BIDoc].[ModelElements]([ModelElementId]),
	[FieldType] NVARCHAR(30),
	[FieldReference] NVARCHAR(MAX),
	[FieldName] NVARCHAR(MAX),
	[QueryElementRefPath] NVARCHAR(MAX),
	[ReferenceElementRefPath] NVARCHAR(MAX),
	[ServerName] NVARCHAR(MAX),
	[DbName] NVARCHAR(MAX),
	[CubeName] NVARCHAR(MAX)
	CONSTRAINT PK_Learning_OlapFieldReferences PRIMARY KEY([OlapFieldReferenceId])
)

/*
x.ProjectConfigId, 
queryElement.ModelElementId QueryElementId,
FieldElementId,
referenceElement.ModelElementId ReferenceElementId,
x.FieldType,
x.FieldReference,
x.FieldName,
x.GroupPath QueryElementRefPath,
x.RefPath ReferenceElementRefPath
*/
