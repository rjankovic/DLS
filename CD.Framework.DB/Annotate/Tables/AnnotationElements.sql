CREATE TABLE [Annotate].[AnnotationElements]
(
	[AnnotationElementId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[ProjectConfigId] UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Adm.ProjectConfigs([ProjectConfigId]),
	[ModelElementId] INT NULL FOREIGN KEY REFERENCES [BIDoc].[ModelElements]([ModelElementId]),
	[RefPath] NVARCHAR(MAX) NULL,
	[Name] NVARCHAR(MAX) NULL,
	[CreatedBy] INT NOT NULL FOREIGN KEY REFERENCES Adm.Users([UserId]),
	[UpdatedBy] INT NOT NULL FOREIGN KEY REFERENCES Adm.Users([UserId]),
	[VersionNumber] INT NOT NULL,
	[IsCurrentVersion] BIT NOT NULL,
	[Date] DATETIME NOT NULL
)
GO

CREATE NONCLUSTERED INDEX IX_AnnotationElements_CurrentVersion 
ON [Annotate].[AnnotationElements]([ModelElementId]) 
INCLUDE([AnnotationElementId]) 
WHERE [IsCurrentVersion] = 1
GO
