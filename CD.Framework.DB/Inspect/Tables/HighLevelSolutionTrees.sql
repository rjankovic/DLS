CREATE TABLE [Inspect].[HighLevelSolutionTrees]
(
	[HighLevelSolutionTreeId] INT NOT NULL IDENTITY(1,1),
	[ProjectConfigId] UNIQUEIDENTIFIER NOT NULL,
	[ModelElementId] INT,
	[Caption] NVARCHAR(MAX),
	[Type] NVARCHAR(MAX), 
	[TypeDescription] NVARCHAR(MAX), 
	[MaxParentLevel] INT, 
	[ParentElementId] INT, 
	[RefPath] NVARCHAR(MAX)

CONSTRAINT [PK_Inspect_HighLevelSolutionTrees] PRIMARY KEY CLUSTERED 
(
	[HighLevelSolutionTreeId] ASC
),
CONSTRAINT [FK_Inspect_HighLevelSolutionTrees_ProjectConfigId] FOREIGN KEY ([ProjectConfigId]) REFERENCES [Adm].[ProjectConfigs]([ProjectConfigId])

)
GO

CREATE NONCLUSTERED INDEX IX_HighLevelSolutionTrees_ProjectConfigId ON [Inspect].[HighLevelSolutionTrees]([ProjectConfigId])
