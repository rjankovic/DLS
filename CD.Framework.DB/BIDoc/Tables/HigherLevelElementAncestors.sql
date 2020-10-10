CREATE TABLE [BIDoc].[HigherLevelElementAncestors]
(
	[HigherLevelElementAncestorsId] INT NOT NULL IDENTITY(1,1) CONSTRAINT PK_HigherLevelElementAncestors PRIMARY KEY,
	[SouceElementId] INT NOT NULL CONSTRAINT FK_HigherLevelElementAncestors_SourceElement FOREIGN KEY REFERENCES [BIDoc].[ModelElements]([ModelElementId]),
	[AncestorElementId] INT NOT NULL CONSTRAINT FK_HigherLevelElementAncestors_AncestorElement FOREIGN KEY REFERENCES [BIDoc].[ModelElements]([ModelElementId]),

	[SouceDfNodeId] INT NOT NULL CONSTRAINT FK_HigherLevelElementAncestors_SourceNode FOREIGN KEY REFERENCES [BIDoc].[BasicGraphNodes]([BasicGraphNodeId]),
	[AncestorDfNodeId] INT NOT NULL CONSTRAINT FK_HigherLevelElementAncestors_AncestorNode FOREIGN KEY REFERENCES [BIDoc].[BasicGraphNodes]([BasicGraphNodeId]),
	-- 1 = high, 2 = medium, 3 = low
	[DetailLevel] INT NOT NULL
)
GO

CREATE INDEX IX_HigherLevelElementAncestors_SouceModelElementId_DetailLevel ON [BIDoc].[HigherLevelElementAncestors] 
([SouceElementId] ASC, [DetailLevel] ASC) INCLUDE([AncestorElementId]) 

GO

CREATE NONCLUSTERED INDEX [IX_HigherLevelElementAncestors_AncestorElementId]
ON [BIDoc].[HigherLevelElementAncestors] ([AncestorElementId])
GO