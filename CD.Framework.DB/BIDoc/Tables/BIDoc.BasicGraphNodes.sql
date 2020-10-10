CREATE TABLE [BIDoc].[BasicGraphNodes](
	[BasicGraphNodeId] [int] NOT NULL IDENTITY(1,1),
	[Name] [nvarchar](max) NULL,
	[NodeType] [nvarchar](200) NULL,
	[Description] [nvarchar](max) NULL,
	[ParentId] [int] NULL,
	[GraphKind] [nvarchar](50) NOT NULL,
	[ProjectConfigId] [uniqueidentifier] NOT NULL,
	[SourceElementId] INT NOT NULL,
	[TopologicalOrder] [int] NULL,
	[RefPathIntervalStart] INT NOT NULL DEFAULT 0,
	[RefPathIntervalEnd] INT NOT NULL DEFAULT 0,
 CONSTRAINT [PK_Adm_BasicGraphNodes] PRIMARY KEY CLUSTERED 
(
	[BasicGraphNodeId] ASC
),
--CONSTRAINT FK_BIDoc_BasciGraphNodes_Parent FOREIGN KEY(ParentId) REFERENCES [BIDoc].[BasicGraphNodes]([BasicGraphNodeId]),
CONSTRAINT FK_BIDoc_BasciGraphNodes_SourceElement FOREIGN KEY(SourceElementId) REFERENCES [BIDoc].[ModelElements]([ModelElementId]),
CONSTRAINT FK_BIDoc_BasciGraphNodes_ProjectConfig FOREIGN KEY(ProjectConfigId) REFERENCES [Adm].[ProjectConfigs]([ProjectConfigId]),
)
GO

CREATE NONCLUSTERED INDEX IX_BIDoc_BasicGraphNodes_GraphKind_SourceElement ON [BIdoc].[BasicGraphNodes]([GraphKind] ASC, [SourceElementId] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_BIDoc_BasicGraphNodes_GrphKind_NodeType_ProjectConfigId]
ON [BIDoc].[BasicGraphNodes] ([GraphKind],[ProjectConfigId], [NodeType])
INCLUDE ([BasicGraphNodeId], [SourceElementId])
GO

CREATE NONCLUSTERED INDEX [IX_BIDoc_BasicGraphNodes_GrphKind_ProjectConfigId_RefPathIntervalStart]
ON [BIDoc].[BasicGraphNodes] ([GraphKind] ASC, [ProjectConfigId] ASC, [RefPathIntervalStart] ASC)
INCLUDE ([BasicGraphNodeId], [SourceElementId], [Name], [Description], [ParentId], [NodeType])
GO

CREATE NONCLUSTERED INDEX [IX_BIDoc_BasicGraphNodes_GrphKind_ProjectConfigId_NodeType_RefPathIntervalStart]
ON [BIDoc].[BasicGraphNodes] ([GraphKind] ASC, [ProjectConfigId] ASC, [NodeType] ASC, [RefPathIntervalStart] ASC)
INCLUDE ([BasicGraphNodeId], [SourceElementId], [Name], [Description], [ParentId])
GO


CREATE NONCLUSTERED INDEX [IX_BIDoc_BasicGraphNodes_ParentId]
ON [BIDoc].[BasicGraphNodes] ([ParentId])
GO
