CREATE TABLE [Analyst].[Objects]
(
	[ObjectId] INT NOT NULL IDENTITY(1,1),
	[Created] DATETIMEOFFSET NOT NULL CONSTRAINT DF_Analyst_Objects_Created DEFAULT GETDATE(),
	[ValidTo] DATETIMEOFFSET NULL,
	[IsCurrent] BIT NOT NULL CONSTRAINT DF_Analyst_Objects_IsCurrent DEFAULT 1,
	[IsDeleted] BIT NOT NULL CONSTRAINT DF_Analyst_Objects_IsDeleted DEFAULT 0,
	[Author_UserId] INT NULL,
	[PreviousVersion_ObjectId] INT NULL
	CONSTRAINT [PK_Analyst_Objects] PRIMARY KEY CLUSTERED 
	(
		[ObjectId] ASC
	),
	CONSTRAINT [FK_Analyst_Object_Author] FOREIGN KEY ([Author_UserId]) REFERENCES [Adm].[Users]([UserId]),
	CONSTRAINT [FK_Analyst_Object_PreviousVersion] FOREIGN KEY ([PreviousVersion_ObjectId]) REFERENCES [Analyst].[Objects]([ObjectId])
)
