CREATE TABLE [Learning].[OlapQueryFields]
(
	[OlapQueryFieldId] INT NOT NULL IDENTITY(1,1),
	[ProjectConfigId] UNIQUEIDENTIFIER,
	[QueryElementId] INT, -- FOREIGN KEY REFERENCES [BIDoc].[ModelElements]([ModelElementId]),
	[OlapFieldId] INT FOREIGN KEY REFERENCES [Learning].[OlapFields]([OlapFieldId]),
	--[ReferenceElementId] INT FOREIGN KEY REFERENCES [BIDoc].[ModelElements]([ModelElementId]),
	CONSTRAINT PK_Learning_OlapQueryFields PRIMARY KEY([OlapQueryFieldId])
)