CREATE TABLE [BIDoc].[HighLevelTypeDescendants]
(
	[HighLevelTypeDescendantId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[ParentType] NVARCHAR(300) NOT NULL,
	[DescendantType] NVARCHAR(300) NOT NULL,
	[NodeType] NVARCHAR(300) NOT NULL
)
